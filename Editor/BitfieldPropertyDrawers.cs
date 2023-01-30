using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityObject = System.Object;

namespace AggroBird.UnityEngineExtend.Editor
{
    internal static class BitfieldUtility
    {
        public const int Precision = 32;
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
            bool isUnique = !isLabelSet || !BitfieldLabelListPropertyDrawer.UniqueTags.Contains(labelName);
            GUI.color = isUnique ? Color.white : Color.red;
            {
                position.height -= 2;
                EditorGUI.PropertyField(position, nameProperty, label);
                EditorExtendUtility.FormatTag(nameProperty);

                if (isLabelSet && isUnique)
                {
                    BitfieldLabelListPropertyDrawer.UniqueTags.Add(labelName);
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

        public static readonly HashSet<string> UniqueTags = new HashSet<string>();

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
            UniqueTags.Clear();
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
            UniqueTags.Clear();
            Array.Clear(maskValue, 0, valueCount);
            for (int i = 0; i < currentCount; i++)
            {
                GetNameIndex(i, out string name, out int index);
                if (!string.IsNullOrEmpty(name) && !UniqueTags.Contains(name))
                {
                    SetMaskFlag(index, true);
                    UniqueTags.Add(name);
                }
            }
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


    internal class BitfieldLabelMaskSelectWindow : EditorWindow
    {
        public static SerializedProperty CurrentProperty { get; set; }
        public static IBitfieldLabelList CurrentLabelList { get; set; }

        public static BitfieldLabelMaskSelectWindow CurrentWindow { get; private set; }
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
                foreach (var maskValue in labelList.Labels)
                {
                    // Apply filter
                    foreach (var filterStr in filterSplit)
                    {
                        if (!maskValue.name.Contains(filterStr, StringComparison.OrdinalIgnoreCase))
                        {
                            goto Skip;
                        }
                    }

                    // Show tags
                    int index = maskValue.index / BitfieldUtility.Precision;
                    int flag = 1 << (maskValue.index % BitfieldUtility.Precision);
                    if ((flag & labelList.GetMask(index)) != 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(maskValue.name);
                        GUILayout.FlexibleSpace();
                        SerializedProperty value = property.FindPropertyRelative($"value{index}");
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

    internal abstract class BitfieldLabelMask : PropertyDrawer
    {
        private static Dictionary<Type, IBitfieldLabelNameProvider> providerCache = new();
        private static StringBuilder labelBuilder = new StringBuilder();

        public abstract int BitCount { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            if (TryGetLabelNameProvider(property, out IBitfieldLabelList labelList))
            {
                labelBuilder.Clear();

                // Get currently selected labels
                foreach (var maskValue in labelList.Labels)
                {
                    int index = maskValue.index / BitfieldUtility.Precision;
                    int flag = 1 << (maskValue.index % BitfieldUtility.Precision);
                    if ((flag & labelList.GetMask(index)) != 0)
                    {
                        SerializedProperty value = property.FindPropertyRelative($"value{index}");
                        if ((value.intValue & flag) != 0)
                        {
                            if (labelBuilder.Length > 0) labelBuilder.Append(", ");
                            labelBuilder.Append(maskValue.name);
                        }
                    }
                }

                // Open edit window
                if (GUI.Button(position, labelBuilder.ToString()))
                {
                    if (BitfieldLabelMaskSelectWindow.CurrentWindow)
                    {
                        BitfieldLabelMaskSelectWindow.CurrentWindow.Close();
                    }

                    BitfieldLabelMaskSelectWindow.CurrentProperty = property;
                    BitfieldLabelMaskSelectWindow.CurrentLabelList = labelList;
                    BitfieldLabelMaskSelectWindow window = ScriptableObject.CreateInstance<BitfieldLabelMaskSelectWindow>();
                    window.ShowUtility();
                }
            }
            else
            {
                EditorGUI.LabelField(position, "Failed to load label name provider asset");
            }



            EditorGUI.EndProperty();
        }

        private static bool TryGetLabelNameProvider(SerializedProperty property, out IBitfieldLabelList labelList)
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


    [CustomPropertyDrawer(typeof(BitfieldLabelMask32))]
    internal sealed class BitfieldLabelMask32PropertyDrawer : BitfieldLabelMask
    {
        public override int BitCount => 32;
    }

    [CustomPropertyDrawer(typeof(BitfieldLabelMask64))]
    internal sealed class BitfieldLabelMask642PropertyDrawer : BitfieldLabelMask
    {
        public override int BitCount => 64;
    }

    [CustomPropertyDrawer(typeof(BitfieldLabelMask128))]
    internal sealed class BitfieldLabelMask128PropertyDrawer : BitfieldLabelMask
    {
        public override int BitCount => 128;
    }

    [CustomPropertyDrawer(typeof(BitfieldLabelMask256))]
    internal sealed class BitfieldLabelMask256PropertyDrawer : BitfieldLabelMask
    {
        public override int BitCount => 256;
    }
}