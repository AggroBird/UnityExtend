using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityObject = System.Object;

namespace AggroBird.UnityEngineExtend.Editor
{
    internal static class BitfieldEditorUtility
    {
        private static Dictionary<Type, IBitfieldLabelNameProvider> providerCache = new();

        public static bool TryGetLabelNameProvider(SerializedProperty property, out IBitfieldLabelList labelList)
        {
            if (EditorExtendUtility.TryGetFieldInfo(property, out FieldInfo fieldInfo))
            {
                BitfieldLabelNameProviderAttribute providerAttribute = fieldInfo.GetCustomAttribute<BitfieldLabelNameProviderAttribute>();
                if (providerAttribute != null && providerAttribute.providerType != null)
                {
                    if (!providerCache.TryGetValue(providerAttribute.providerType, out IBitfieldLabelNameProvider provider))
                    {
                        UnityObject[] resources = Resources.LoadAll(string.Empty, providerAttribute.providerType);
                        if (resources != null && resources.Length > 0)
                        {
                            for (int i = 0; i < resources.Length; i++)
                            {
                                if (resources[i] is IBitfieldLabelNameProvider resource)
                                {
                                    provider = resource;
                                    providerCache.Add(providerAttribute.providerType, provider);
                                    break;
                                }
                            }
                        }
                    }
                    if (provider != null)
                    {
                        labelList = provider.GetBitfieldLabelList();
                        return labelList != null;
                    }
                }
            }

            labelList = null;
            return false;
        }
    }


    [CustomPropertyDrawer(typeof(BitfieldLabel))]
    internal sealed class BitfieldLabelPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            string labelName = nameProperty.stringValue;
            bool isLabelSet = !string.IsNullOrEmpty(labelName);
            bool isUnique = !isLabelSet || !BitfieldLabelListPropertyDrawer.UniqueLabels.Contains(labelName);
            GUI.color = isUnique ? Color.white : Color.red;
            {
                position.height -= EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, nameProperty, label);
                EditorExtendUtility.FormatTag(nameProperty);

                if (isLabelSet && isUnique)
                {
                    BitfieldLabelListPropertyDrawer.UniqueLabels.Add(labelName);
                }
            }
            GUI.color = Color.white;

            EditorGUI.EndProperty();
        }
    }

    internal abstract class BitfieldLabelListPropertyDrawer : PropertyDrawer
    {
        public abstract int BitCount { get; }

        private static int[] maskValue = default;
        private static int valueCount = 0;
        private static bool IsMaskFlagSet(int idx)
        {
            return (maskValue[idx / BitfieldUtility.Precision] & (1 << (idx % BitfieldUtility.Precision))) != 0;
        }
        private static void SetMaskFlag(int idx, bool value)
        {
            if (value)
            {
                maskValue[idx / BitfieldUtility.Precision] |= 1 << (idx % BitfieldUtility.Precision);
            }
            else
            {
                maskValue[idx / BitfieldUtility.Precision] &= ~(1 << (idx % BitfieldUtility.Precision));
            }
        }
        private static int FindNextAvailableIndex()
        {
            for (int i = 0; i < valueCount; i++)
            {
                for (int j = 0; j < BitfieldUtility.Precision; j++)
                {
                    if ((maskValue[i] & (1 << j)) == 0)
                    {
                        return i * BitfieldUtility.Precision + j;
                    }
                }
            }
            throw new UnityException("Failed to find a slot");
        }

        public static readonly HashSet<string> UniqueLabels = new HashSet<string>();

        private SerializedProperty valuesProperty;

        private void GetNameIndex(int idx, out string name, out int index)
        {
            SerializedProperty value = valuesProperty.GetArrayElementAtIndex(idx);
            name = value.FindPropertyRelative("name").stringValue;
            index = value.FindPropertyRelative("index").intValue;
        }
        private void SetNameIndex(int idx, string name, int index)
        {
            SerializedProperty value = valuesProperty.GetArrayElementAtIndex(idx);
            value.FindPropertyRelative("name").stringValue = name;
            value.FindPropertyRelative("index").intValue = index;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            valuesProperty = property.FindPropertyRelative("values");

            // Create mask array
            valueCount = BitCount / BitfieldUtility.Precision;
            if (maskValue == null || maskValue.Length < valueCount)
                maskValue = new int[valueCount];
            else
                Array.Clear(maskValue, 0, valueCount);

            // Check if current indices are still unique
            int currentCount = valuesProperty.arraySize;
            for (int i = 0; i < currentCount; i++)
            {
                GetNameIndex(i, out _, out int index);
                if (IsMaskFlagSet(index))
                {
                    index = FindNextAvailableIndex();
                }
                SetMaskFlag(index, true);
            }

            // Display property field
            UniqueLabels.Clear();
            EditorGUI.PropertyField(position, valuesProperty, label);
            if (valuesProperty.arraySize > currentCount)
            {
                if (valuesProperty.arraySize > BitCount) valuesProperty.arraySize = BitCount;

                // Initialize new fields to empty
                for (int i = currentCount; i < valuesProperty.arraySize; i++)
                {
                    int index = FindNextAvailableIndex();
                    SetNameIndex(i, string.Empty, index);
                    SetMaskFlag(index, true);
                }
            }
            currentCount = valuesProperty.arraySize;

            // Rebuild mask
            UniqueLabels.Clear();
            Array.Clear(maskValue, 0, valueCount);
            for (int i = 0; i < currentCount; i++)
            {
                GetNameIndex(i, out string name, out int index);
                if (!string.IsNullOrEmpty(name) && !UniqueLabels.Contains(name))
                {
                    SetMaskFlag(index, true);
                    UniqueLabels.Add(name);
                }
            }

            // Write values
            for (int i = 0; i < valueCount; i++)
            {
                SerializedProperty maskProperty = property.FindPropertyRelative($"mask{i}");
                if (maskProperty.intValue != maskValue[i])
                {
                    maskProperty.intValue = maskValue[i];
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("values"));
        }
    }

    [CustomPropertyDrawer(typeof(BitfieldLabelList32))]
    internal sealed class BitfieldLabelList32PropertyDrawer : BitfieldLabelListPropertyDrawer
    {
        public override int BitCount => 32;
    }

    [CustomPropertyDrawer(typeof(BitfieldLabelList64))]
    internal sealed class BitfieldLabelList64PropertyDrawer : BitfieldLabelListPropertyDrawer
    {
        public override int BitCount => 64;
    }

    [CustomPropertyDrawer(typeof(BitfieldLabelList128))]
    internal sealed class BitfieldLabelList128PropertyDrawer : BitfieldLabelListPropertyDrawer
    {
        public override int BitCount => 128;
    }

    [CustomPropertyDrawer(typeof(BitfieldLabelList256))]
    internal sealed class BitfieldLabelList256PropertyDrawer : BitfieldLabelListPropertyDrawer
    {
        public override int BitCount => 256;
    }


    internal class BitfieldMaskSelectWindow : EditorWindow
    {
        public static SerializedProperty CurrentProperty { get; set; }
        public static IBitfieldLabelList CurrentLabelList { get; set; }

        public static BitfieldMaskSelectWindow CurrentWindow { get; private set; }
        private Vector2 scrollPosition = Vector2.zero;
        private string filter = string.Empty;

        private void OnEnable()
        {
            CurrentWindow = this;

            minSize = new Vector2(200, 300);

            Undo.undoRedoPerformed += Close;

            if (CurrentProperty != null)
            {
                titleContent = new GUIContent(CurrentProperty.displayName);
            }
        }
        private void OnDisable()
        {
            CurrentWindow = null;

            Undo.undoRedoPerformed -= Close;
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnGUI()
        {
            if (CurrentProperty == null)
            {
                Close();
                return;
            }

            var labelList = CurrentLabelList;
            var property = CurrentProperty;

            CurrentProperty.serializedObject.Update();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width));
            {
                filter = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField);
                string[] filterSplit = filter.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var bitfieldLabel in labelList.Labels)
                {
                    // Apply filter
                    foreach (var filterStr in filterSplit)
                    {
                        if (!bitfieldLabel.name.Contains(filterStr, StringComparison.OrdinalIgnoreCase))
                        {
                            goto Skip;
                        }
                    }

                    // Show tags
                    BitfieldUtility.GetIdxFlag(bitfieldLabel.index, out int idx, out int flag);
                    if ((flag & labelList.GetMask(idx)) != 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(bitfieldLabel.name);
                        GUILayout.FlexibleSpace();
                        SerializedProperty value = property.FindPropertyRelative($"mask{idx}");
                        bool isSet = (value.intValue & flag) != 0;
                        bool setValue = EditorGUILayout.Toggle(isSet, GUILayout.Width(20));
                        if (setValue != isSet)
                        {
                            value.intValue = setValue ? (value.intValue | flag) : (value.intValue & ~flag);
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                Skip:
                    continue;
                }
            }
            EditorGUILayout.EndScrollView();
            CurrentProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    internal abstract class BitfieldMask : PropertyDrawer
    {
        private static StringBuilder labelBuilder = new StringBuilder();

        public abstract int BitCount { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            if (BitfieldEditorUtility.TryGetLabelNameProvider(property, out IBitfieldLabelList labelList))
            {
                labelBuilder.Clear();

                // Get currently selected labels
                foreach (var bitfieldLabel in labelList.Labels)
                {
                    BitfieldUtility.GetIdxFlag(bitfieldLabel.index, out int idx, out int flag);
                    if ((flag & labelList.GetMask(idx)) != 0)
                    {
                        SerializedProperty value = property.FindPropertyRelative($"mask{idx}");
                        if ((value.intValue & flag) != 0)
                        {
                            if (labelBuilder.Length > 0) labelBuilder.Append(", ");
                            labelBuilder.Append(bitfieldLabel.name);
                        }
                    }
                }

                // Open edit window
                if (GUI.Button(position, labelBuilder.ToString()))
                {
                    if (BitfieldMaskSelectWindow.CurrentWindow)
                    {
                        BitfieldMaskSelectWindow.CurrentWindow.Close();
                    }

                    BitfieldMaskSelectWindow.CurrentProperty = property;
                    BitfieldMaskSelectWindow.CurrentLabelList = labelList;
                    BitfieldMaskSelectWindow window = ScriptableObject.CreateInstance<BitfieldMaskSelectWindow>();
                    window.ShowUtility();
                }
            }
            else
            {
                EditorGUI.LabelField(position, "Failed to load label name provider asset");
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(BitfieldMask32))]
    internal sealed class BitfieldMask32PropertyDrawer : BitfieldMask
    {
        public override int BitCount => 32;
    }

    [CustomPropertyDrawer(typeof(BitfieldMask64))]
    internal sealed class BitfieldMask642PropertyDrawer : BitfieldMask
    {
        public override int BitCount => 64;
    }

    [CustomPropertyDrawer(typeof(BitfieldMask128))]
    internal sealed class BitfieldMask128PropertyDrawer : BitfieldMask
    {
        public override int BitCount => 128;
    }

    [CustomPropertyDrawer(typeof(BitfieldMask256))]
    internal sealed class BitfieldMask256PropertyDrawer : BitfieldMask
    {
        public override int BitCount => 256;
    }


    [CustomPropertyDrawer(typeof(BitfieldValue))]
    internal sealed class BitfieldValuePropertyDrawer : PropertyDrawer
    {
        private static readonly SortedDictionary<string, int> uniqueLabels = new SortedDictionary<string, int>();
        private static readonly List<string> names = new();
        private static readonly List<int> values = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            if (BitfieldEditorUtility.TryGetLabelNameProvider(property, out IBitfieldLabelList labelList))
            {
                SerializedProperty value = property.FindPropertyRelative("value");

                uniqueLabels.Clear();

                names.Clear();
                values.Clear();

                foreach (var bitfieldLabel in labelList.Labels)
                {
                    BitfieldUtility.GetIdxFlag(bitfieldLabel.index, out int idx, out int flag);
                    if ((flag & labelList.GetMask(idx)) != 0)
                    {
                        names.Add(bitfieldLabel.name);
                        values.Add(bitfieldLabel.index);
                    }
                }

                int currentSelection = -1;
                int currentValue = value.intValue;
                for (int i = 0; i < values.Count; i++)
                {
                    if (currentValue == values[i])
                    {
                        currentSelection = i;
                        break;
                    }
                }

                if (currentSelection == -1)
                {
                    names.Insert(0, "<missing>");
                    currentSelection = 0;
                    int newSelection = EditorGUI.Popup(position, currentSelection, names.ToArray());
                    if (newSelection != currentSelection && newSelection != 0)
                    {
                        value.intValue = values[newSelection - 1];
                    }
                }
                else
                {
                    int newSelection = EditorGUI.Popup(position, currentSelection, names.ToArray());
                    if (newSelection != currentSelection)
                    {
                        value.intValue = values[newSelection];
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, "Failed to load label name provider asset");
            }

            EditorGUI.EndProperty();
        }
    }
}