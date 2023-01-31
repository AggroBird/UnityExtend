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
            IBitfieldLabelNameProvider provider = null;

            if (EditorExtendUtility.TryGetFieldInfo(property, out FieldInfo fieldInfo))
            {
                BitfieldLabelNameProviderAttribute providerAttribute = fieldInfo.GetCustomAttribute<BitfieldLabelNameProviderAttribute>();
                if (providerAttribute != null)
                {
                    // Load from asset
                    if (providerAttribute.ProviderType != null)
                    {
                        if (!providerCache.TryGetValue(providerAttribute.ProviderType, out provider) || !IsValid(provider))
                        {
                            provider = null;

                            string[] guids = AssetDatabase.FindAssets($"t:{providerAttribute.ProviderType.Name}");
                            if (guids != null && guids.Length > 0)
                            {
                                for (int i = 0; i < guids.Length; i++)
                                {
                                    UnityObject obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), providerAttribute.ProviderType);
                                    if (obj && obj is IBitfieldLabelNameProvider validProvider)
                                    {
                                        provider = validProvider;
                                        providerCache.Add(providerAttribute.ProviderType, provider);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Load from scriptable object
                    UnityObject obj = property.serializedObject.targetObject;
                    if (obj && obj is IBitfieldLabelNameProvider validProvider)
                    {
                        provider = validProvider;
                    }
                }
            }

            if (provider != null)
            {
                labelList = provider.GetBitfieldLabelList();
                return labelList != null;
            }
            else
            {
                labelList = null;
                return false;
            }
        }

        private static bool IsValid(IBitfieldLabelNameProvider provider)
        {
            return provider is UnityObject unityObject && unityObject;
        }

        public static void GetBitfieldLabel(this SerializedProperty value, out string name, out int index)
        {
            name = value.FindPropertyRelative("name").stringValue;
            index = value.FindPropertyRelative("index").intValue;
        }
        public static void SetBitfieldLabel(this SerializedProperty value, string name, int index)
        {
            value.FindPropertyRelative("name").stringValue = name;
            value.FindPropertyRelative("index").intValue = index;
        }

        public static void SetBitfieldLabelArray(this SerializedProperty property, IReadOnlyCollection<BitfieldLabel> values)
        {
            property.arraySize = values.Count;
            int idx = 0;
            foreach (var value in values)
            {
                property.GetArrayElementAtIndex(idx).SetBitfieldLabel(value.Name, value.Index);
                idx++;
            }
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
                    int newSelection = EditorGUI.Popup(position, label.text, currentSelection, names.ToArray());
                    if (newSelection != currentSelection && newSelection != 0)
                    {
                        value.intValue = values[newSelection - 1];
                    }
                }
                else
                {
                    int newSelection = EditorGUI.Popup(position, label.text, currentSelection, names.ToArray());
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
                if (GUI.Button(position, labelBuilder.Length == 0 ? "<none>" : labelBuilder.ToString(), EditorStyles.popup))
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
            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            SerializedProperty indexProperty = property.FindPropertyRelative("index");
            string name = nameProperty.stringValue;

            label = new GUIContent($"Flag {indexProperty.intValue}");
            EditorGUI.BeginProperty(position, label, property);

            GUI.color = (string.IsNullOrEmpty(name) || !BitfieldLabelListPropertyDrawer.UniqueLabels.ContainsKey(name)) ? Color.white : new Color(1, 0.5f, 0.5f, 1);
            {
                position.height -= EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, nameProperty, label);

                // Validate after modification
                EditorExtendUtility.FormatTag(nameProperty);
                property.GetBitfieldLabel(out name, out int index);

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
        private static readonly Dictionary<string, int> editorHistory = new();
        private static readonly Dictionary<int, BitfieldLabel> rebuildEditorHistory = new();
        private static readonly List<BitfieldLabel> writeValues = new();


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty editorData = property.FindPropertyRelative("editorData");
            SerializedProperty editorHistoryProperty = editorData.FindPropertyRelative("history");
            SerializedProperty editorValuesProperty = editorData.FindPropertyRelative("values");
            SerializedProperty valuesProperty = property.FindPropertyRelative("values");

            // Get editor history
            editorHistory.Clear();
            for (int i = 0; i < editorHistoryProperty.arraySize; i++)
            {
                editorHistoryProperty.GetArrayElementAtIndex(i).GetBitfieldLabel(out string name, out int index);
                editorHistory[name] = index;
            }

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
                editorValuesProperty.GetArrayElementAtIndex(i).GetBitfieldLabel(out _, out int index);
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
                    editorValuesProperty.GetArrayElementAtIndex(i).SetBitfieldLabel(string.Empty, index);
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
                SerializedProperty value = editorValuesProperty.GetArrayElementAtIndex(i);
                value.GetBitfieldLabel(out string name, out int index);
                if (!string.IsNullOrEmpty(name) && !UniqueLabels.ContainsKey(name))
                {
                    // Check against history
                    if (editorHistory.TryGetValue(name, out int historyIndex) && index != historyIndex)
                    {
                        Debug.LogError($"Flag {index} name '{name}' collides with flag {historyIndex} which previously held that name.\n" +
                            $"Reordering flags through name change will cause problems with bitfield masks in other assets that have these flags set.\n" +
                            $"To safely reorder flags please use the drag handle next to the flag field, which preserves the internal flag index.\n");
                        value.SetBitfieldLabel(string.Empty, index);
                    }
                    else
                    {
                        SetMaskFlag(index, true);
                        UniqueLabels.Add(name, index);
                        writeValues.Add(new BitfieldLabel(name, index));
                    }
                }
            }

            // Rebuild history
            rebuildEditorHistory.Clear();
            foreach (var history in editorHistory)
            {
                rebuildEditorHistory.Add(history.Value, new BitfieldLabel(history.Key, history.Value));
            }
            foreach (var value in writeValues)
            {
                rebuildEditorHistory[value.Index] = value;
            }
            editorHistoryProperty.SetBitfieldLabelArray(rebuildEditorHistory.Values);

            // Write values
            valuesProperty.SetBitfieldLabelArray(writeValues);

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
            SerializedProperty editorData = property.FindPropertyRelative("editorData");
            SerializedProperty editorValuesProperty = editorData.FindPropertyRelative("values");
            return EditorGUI.GetPropertyHeight(editorValuesProperty);
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