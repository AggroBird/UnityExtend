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
        private static List<object> propertyValues = new();

        public static bool TryGetLabelNameProvider(SerializedProperty property, out IBitfieldLabelList labelList)
        {
            IBitfieldLabelNameProvider provider = null;

            if (EditorExtendUtility.TryGetFieldInfo(property, out FieldInfo fieldInfo, out _, values: propertyValues))
            {
                BitfieldLabelGlobalNameProviderAttribute globalProviderAttribute = fieldInfo.GetCustomAttribute<BitfieldLabelGlobalNameProviderAttribute>();
                if (globalProviderAttribute != null)
                {
                    // Load from asset
                    if (globalProviderAttribute.ProviderType != null)
                    {
                        if (!providerCache.TryGetValue(globalProviderAttribute.ProviderType, out provider) || !IsValid(provider))
                        {
                            provider = null;

                            string[] guids = AssetDatabase.FindAssets($"t:{globalProviderAttribute.ProviderType.Name}");
                            if (guids != null && guids.Length > 0)
                            {
                                for (int i = 0; i < guids.Length; i++)
                                {
                                    UnityObject obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), globalProviderAttribute.ProviderType);
                                    if (obj && obj is IBitfieldLabelNameProvider validProvider)
                                    {
                                        provider = validProvider;
                                        providerCache[globalProviderAttribute.ProviderType] = provider;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    BitfieldLabelNestedNameProviderAttribute nestedProviderAttribute = fieldInfo.GetCustomAttribute<BitfieldLabelNestedNameProviderAttribute>();
                    if (nestedProviderAttribute != null)
                    {
                        switch (nestedProviderAttribute.Source)
                        {
                            case NestedNameProviderSource.DeclaringType:
                            {
                                // Try to get from parent property
                                object parent = propertyValues[propertyValues.Count - 1];
                                if (parent != null && parent is IBitfieldLabelNameProvider validProvider)
                                {
                                    provider = validProvider;
                                }
                            }
                            break;
                            case NestedNameProviderSource.SerializedObject:
                            {
                                // Try to get from serialized object
                                UnityObject obj = property.serializedObject.targetObject;
                                if (obj && obj is IBitfieldLabelNameProvider validProvider)
                                {
                                    provider = validProvider;
                                }
                            }
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

        public static void GetBitfieldLabel(this SerializedProperty property, out string name, out int index)
        {
            name = property.FindPropertyRelative("name").stringValue;
            index = property.FindPropertyRelative("index").intValue;
        }
        public static void SetBitfieldLabel(this SerializedProperty property, string name, int index)
        {
            property.FindPropertyRelative("name").stringValue = name;
            property.FindPropertyRelative("index").intValue = index;
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

    public static class BitfieldPropertyUtility
    {
        private const BindingFlags MaskBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private static T GetBitfieldMask<T>(SerializedProperty property) where T : IBitfieldMask, new()
        {
            object value = new T();
            int count = (value as IBitfieldMask).BitCount / BitfieldUtility.Precision;
            for (int i = 0; i < count; i++)
            {
                string fieldName = $"mask{i}";
                typeof(T).GetField(fieldName, MaskBindingFlags).SetValue(value, property.FindPropertyRelative(fieldName).intValue);
            }
            return (T)value;
        }
        private static void SetBitfieldMask<T>(SerializedProperty property, T mask) where T : IBitfieldMask, new()
        {
            object value = mask;
            int count = (value as IBitfieldMask).BitCount / BitfieldUtility.Precision;
            for (int i = 0; i < count; i++)
            {
                string fieldName = $"mask{i}";
                property.FindPropertyRelative(fieldName).intValue = (int)typeof(T).GetField(fieldName, MaskBindingFlags).GetValue(value);
            }
        }

        public static BitfieldMask32 GetBitfieldMask32Value(this SerializedProperty property)
        {
            return GetBitfieldMask<BitfieldMask32>(property);
        }
        public static void SetBitfieldMask32Value(this SerializedProperty property, BitfieldMask32 mask)
        {
            SetBitfieldMask(property, mask);
        }
        public static BitfieldMask64 GetBitfieldMask64Value(this SerializedProperty property)
        {
            return GetBitfieldMask<BitfieldMask64>(property);
        }
        public static void SetBitfieldMask64Value(this SerializedProperty property, BitfieldMask64 mask)
        {
            SetBitfieldMask(property, mask);
        }
        public static BitfieldMask128 GetBitfieldMask128Value(this SerializedProperty property)
        {
            return GetBitfieldMask<BitfieldMask128>(property);
        }
        public static void SetBitfieldMask128Value(this SerializedProperty property, BitfieldMask128 mask)
        {
            SetBitfieldMask(property, mask);
        }
        public static BitfieldMask256 GetBitfieldMask256Value(this SerializedProperty property)
        {
            return GetBitfieldMask<BitfieldMask256>(property);
        }
        public static void SetBitfieldMask256Value(this SerializedProperty property, BitfieldMask256 mask)
        {
            SetBitfieldMask(property, mask);
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
                EditorGUI.LabelField(position, "Failed to find label name provider asset");
            }

            EditorGUI.EndProperty();
        }
    }


    internal class BitfieldMaskSelectWindow : EditorWindow
    {
        public static BitfieldMaskSelectWindow CurrentWindow { get; private set; }

        private Vector2 scrollPosition = Vector2.zero;
        private string filter = string.Empty;

        private SerializedProperty property;
        private IBitfieldLabelList labelList;

        private SerializedObject[] serializedObjects = Array.Empty<SerializedObject>();
        private SerializedProperty[] serializedProperties = Array.Empty<SerializedProperty>();


        public void SetProperty(SerializedProperty property, IBitfieldLabelList labelList)
        {
            this.property = property;
            titleContent = new GUIContent(property.displayName);
            this.labelList = labelList;

            UnityObject[] multipleObjects = property.serializedObject.targetObjects;
            serializedObjects = new SerializedObject[multipleObjects.Length];
            for (int i = 0; i < multipleObjects.Length; i++)
            {
                serializedObjects[i] = new SerializedObject(multipleObjects[i]);
            }
            serializedProperties = new SerializedProperty[multipleObjects.Length];
        }

        private void OnEnable()
        {
            CurrentWindow = this;

            minSize = new Vector2(200, 300);

            Undo.undoRedoPerformed += Close;

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
            foreach (var obj in serializedObjects) obj.Update();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width));
            {
                filter = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField);
                string[] filterSplit = filter.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool hasMultipleDifferentValues = property.hasMultipleDifferentValues;
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
                    SerializedProperty maskValueProperty = property.FindPropertyRelative($"mask{idx}");
                    if (maskValueProperty != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(bitfieldLabel.Name);
                        GUILayout.FlexibleSpace();
                        for (int i = 0; i < serializedObjects.Length; i++)
                        {
                            serializedProperties[i] = serializedObjects[i].FindProperty(maskValueProperty.propertyPath);
                        }
                        bool showMixedValue = false;
                        bool isSet = (serializedProperties[0].intValue & flag) != 0;
                        for (int i = 1; i < serializedObjects.Length; i++)
                        {
                            int nestedValue = serializedProperties[i].intValue;
                            showMixedValue |= ((nestedValue & flag) != 0) != isSet;
                            if (showMixedValue) break;
                        }
                        using (new EditorExtendUtility.MixedValueScope(showMixedValue))
                        {
                            EditorGUI.BeginChangeCheck();
                            bool setValue = EditorGUILayout.Toggle(isSet, GUILayout.Width(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                setValue |= showMixedValue;
                                for (int i = 0; i < serializedObjects.Length; i++)
                                {
                                    serializedProperties[i].intValue = setValue ? (serializedProperties[i].intValue | flag) : (serializedProperties[i].intValue & ~flag);
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                Skip:
                    continue;
                }
            }
            EditorGUILayout.EndScrollView();
            foreach (var obj in serializedObjects) obj.ApplyModifiedProperties();
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

                bool showMixedValue = property.hasMultipleDifferentValues;

                // Get currently selected labels
                if (!showMixedValue)
                {
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
                }

                // Open edit window
                using (new EditorExtendUtility.MixedValueScope(showMixedValue))
                {
                    GUI.contentColor = showMixedValue ? new Color(1, 1, 1, 0.5f) : Color.white;
                    if (GUI.Button(position, showMixedValue ? EditorExtendUtility.MixedValueContent : labelBuilder.Length == 0 ? "<none>" : labelBuilder.ToString(), EditorStyles.popup))
                    {
                        if (BitfieldMaskSelectWindow.CurrentWindow)
                        {
                            BitfieldMaskSelectWindow.CurrentWindow.Close();
                        }

                        BitfieldMaskSelectWindow window = ScriptableObject.CreateInstance<BitfieldMaskSelectWindow>();
                        window.SetProperty(property, labelList);
                        window.ShowUtility();
                    }
                    GUI.contentColor = Color.white;
                }
            }
            else
            {
                EditorGUI.LabelField(position, "Failed to find label name provider asset");
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

            GUI.contentColor = (string.IsNullOrEmpty(name) || !BitfieldLabelListPropertyDrawer.UniqueLabels.ContainsKey(name)) ? Color.white : new Color(1, 0.5f, 0.5f, 1);
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
            GUI.contentColor = Color.white;

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
        private static readonly List<BitfieldLabel> writeValues = new();


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (!property.serializedObject.isEditingMultipleObjects)
            {
                SerializedProperty editorValuesProperty = property.FindPropertyRelative("editorValues");
                SerializedProperty valuesProperty = property.FindPropertyRelative("values");

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
                        SetMaskFlag(index, true);
                        UniqueLabels.Add(name, index);
                        writeValues.Add(new BitfieldLabel(name, index));
                    }
                }

                // Write values
                valuesProperty.SetBitfieldLabelArray(writeValues);

                // Write mask
                SerializedProperty maskProperty = property.FindPropertyRelative("mask");
                for (int i = 0; i < valueCount; i++)
                {
                    maskProperty.FindPropertyRelative($"mask{i}").intValue = maskValue[i];
                }
            }
            else
            {
                position = EditorGUI.PrefixLabel(position, label);
                EditorGUI.LabelField(position, "Multi-object editing not supported.");
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                return EditorExtendUtility.SinglePropertyHeight;
            }

            SerializedProperty editorValuesProperty = property.FindPropertyRelative("editorValues");
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