using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

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
                if (providerAttribute != null && providerAttribute.ProviderType != null)
                {
                    if (!providerCache.TryGetValue(providerAttribute.ProviderType, out IBitfieldLabelNameProvider provider) || !IsValid(provider))
                    {
                        provider = null;
                        UnityObject[] resources = Resources.LoadAll(string.Empty, providerAttribute.ProviderType);
                        if (resources != null && resources.Length > 0)
                        {
                            for (int i = 0; i < resources.Length; i++)
                            {
                                if (resources[i] is IBitfieldLabelNameProvider resource)
                                {
                                    provider = resource;
                                    providerCache.Add(providerAttribute.ProviderType, provider);
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

        private static bool IsValid(IBitfieldLabelNameProvider provider)
        {
            return provider is UnityObject unityObject && unityObject;
        }

        public static void GetLabelNameIndex(SerializedProperty value, out string name, out int index)
        {
            name = value.FindPropertyRelative("name").stringValue;
            index = value.FindPropertyRelative("index").intValue;
        }
        public static void SetLabelNameIndex(SerializedProperty value, string name, int index)
        {
            value.FindPropertyRelative("name").stringValue = name;
            value.FindPropertyRelative("index").intValue = index;
        }
    }


    [CustomPropertyDrawer(typeof(BitfieldFlag))]
    internal sealed class BitfieldFlagPropertyDrawer : PropertyDrawer
    {
        private static readonly SortedDictionary<string, int> uniqueLabels = new();
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
                    names.Add(bitfieldLabel.Name);
                    values.Add(bitfieldLabel.Index);
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
                        if (!bitfieldLabel.Name.Contains(filterStr, StringComparison.OrdinalIgnoreCase))
                        {
                            goto Skip;
                        }
                    }

                    // Show tags
                    BitfieldUtility.GetIdxFlag(bitfieldLabel.Index, out int idx, out int flag);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(bitfieldLabel.Name);
                    GUILayout.FlexibleSpace();
                    SerializedProperty maskValueProperty = property.FindPropertyRelative($"mask{idx}");
                    bool isSet = (maskValueProperty.intValue & flag) != 0;
                    bool setValue = EditorGUILayout.Toggle(isSet, GUILayout.Width(20));
                    if (setValue != isSet)
                    {
                        maskValueProperty.intValue = setValue ? (maskValueProperty.intValue | flag) : (maskValueProperty.intValue & ~flag);
                    }
                    EditorGUILayout.EndHorizontal();

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
        private static StringBuilder labelBuilder = new();

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
                    BitfieldUtility.GetIdxFlag(bitfieldLabel.Index, out int idx, out int flag);
                    SerializedProperty value = property.FindPropertyRelative($"mask{idx}");
                    if ((value.intValue & flag) != 0)
                    {
                        if (labelBuilder.Length > 0) labelBuilder.Append(", ");
                        labelBuilder.Append(bitfieldLabel.Name);
                    }
                }

                // Open edit window
                if (GUI.Button(position, labelBuilder.Length == 0 ? "<none>" : labelBuilder.ToString()))
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


    [CustomPropertyDrawer(typeof(BitfieldLabel))]
    internal sealed class BitfieldLabelPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            BitfieldEditorUtility.GetLabelNameIndex(property, out string name, out _);
            GUI.color = (string.IsNullOrEmpty(name) || !BitfieldLabelListPropertyDrawer.UniqueLabels.ContainsKey(name)) ? Color.white : Color.red;
            {
                position.height -= EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, nameProperty, label);

                // Validate after modification
                EditorExtendUtility.FormatTag(nameProperty);
                BitfieldEditorUtility.GetLabelNameIndex(property, out name, out int index);

                if (!string.IsNullOrEmpty(name) && !BitfieldLabelListPropertyDrawer.UniqueLabels.ContainsKey(name))
                {
                    BitfieldLabelListPropertyDrawer.UniqueLabels.Add(name, index);
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

        public static readonly Dictionary<string, int> UniqueLabels = new();
        private static readonly List<(string name, int index)> writeValues = new();


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty editorValuesProperty = property.FindPropertyRelative("editorValues");

            // Create mask array
            valueCount = BitCount / BitfieldUtility.Precision;
            if (maskValue == null || maskValue.Length < valueCount)
                maskValue = new int[valueCount];
            else
                Array.Clear(maskValue, 0, valueCount);

            // Check if current indices are still unique
            int currentCount = editorValuesProperty.arraySize;
            for (int i = 0; i < currentCount; i++)
            {
                BitfieldEditorUtility.GetLabelNameIndex(editorValuesProperty.GetArrayElementAtIndex(i), out _, out int index);
                if (IsMaskFlagSet(index))
                {
                    index = FindNextAvailableIndex();
                }
                SetMaskFlag(index, true);
            }

            // Display property field
            UniqueLabels.Clear();
            EditorGUI.PropertyField(position, editorValuesProperty, label);
            if (editorValuesProperty.arraySize > BitCount)
            {
                editorValuesProperty.arraySize = BitCount;
            }
            if (editorValuesProperty.arraySize > currentCount)
            {
                // Initialize new fields to empty
                for (int i = currentCount; i < editorValuesProperty.arraySize; i++)
                {
                    int index = FindNextAvailableIndex();
                    BitfieldEditorUtility.SetLabelNameIndex(editorValuesProperty.GetArrayElementAtIndex(i), string.Empty, index);
                    SetMaskFlag(index, true);
                }
            }
            currentCount = editorValuesProperty.arraySize;

            // Rebuild mask
            UniqueLabels.Clear();
            Array.Clear(maskValue, 0, valueCount);
            writeValues.Clear();
            for (int i = 0; i < currentCount; i++)
            {
                BitfieldEditorUtility.GetLabelNameIndex(editorValuesProperty.GetArrayElementAtIndex(i), out string name, out int index);
                if (!string.IsNullOrEmpty(name) && !UniqueLabels.ContainsKey(name))
                {
                    SetMaskFlag(index, true);
                    UniqueLabels.Add(name, index);
                    writeValues.Add((name, index));
                }
            }

            // Write values
            SerializedProperty valuesProperty = property.FindPropertyRelative("values");
            valuesProperty.arraySize = writeValues.Count;
            for (int i = 0; i < writeValues.Count; i++)
            {
                BitfieldEditorUtility.SetLabelNameIndex(valuesProperty.GetArrayElementAtIndex(i), writeValues[i].name, writeValues[i].index);
            }

            // Write mask
            SerializedProperty maskProperty = property.FindPropertyRelative("mask");
            for (int i = 0; i < valueCount; i++)
            {
                SerializedProperty maskValueProperty = maskProperty.FindPropertyRelative($"mask{i}");
                maskValueProperty.intValue = maskValue[i];
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("editorValues"));
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
}