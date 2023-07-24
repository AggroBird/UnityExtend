using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityEngineExtend.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphicFieldAttribute))]
    [InitializeOnLoad]
    internal sealed class PolymorphicFieldPropertyDrawer : PropertyDrawer
    {
        private const string ManagedRefenceDataKey = "MANAGED_REFERENCE_DATA:";

        static PolymorphicFieldPropertyDrawer()
        {
            EditorApplication.contextualPropertyMenu += ShowSerializeReferenceCopyPasteContextMenu;
        }

        private static void ShowSerializeReferenceCopyPasteContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var copyProperty = property.Copy();
                if (copyProperty.managedReferenceValue != null)
                {
                    menu.AddItem(new GUIContent("Copy Managed Reference Data"), false, (_) =>
                    {
                        EditorGUIUtility.systemCopyBuffer = ManagedRefenceDataKey + $"<{copyProperty.managedReferenceFullTypename}>" + JsonUtility.ToJson(copyProperty.managedReferenceValue);
                    }, null);
                }

                string currentClipboard = EditorGUIUtility.systemCopyBuffer;
                if (currentClipboard.StartsWith(ManagedRefenceDataKey))
                {
                    // Extract type from clipboard
                    int split = currentClipboard.IndexOf('>', ManagedRefenceDataKey.Length);
                    string typename = currentClipboard.Substring(ManagedRefenceDataKey.Length + 1, split - ManagedRefenceDataKey.Length - 1);
                    if (TryGetTypeFromManagedReferenceTypename(typename, out Type clipboardType))
                    {
                        // Ensure that the clipboard type is supported by the destination field
                        if (TryGetTypeFromManagedReferenceTypename(copyProperty.managedReferenceFieldTypename, out Type fieldType) && GetSupportedFieldTypes(fieldType).Contains(clipboardType))
                        {
                            menu.AddItem(new GUIContent("Paste Managed Reference Data"), false, (_) =>
                            {
                                object obj = FormatterServices.GetUninitializedObject(clipboardType);
                                JsonUtility.FromJsonOverwrite(currentClipboard.Substring(split + 1), obj);
                                copyProperty.managedReferenceValue = obj;
                                copyProperty.serializedObject.ApplyModifiedProperties();
                                copyProperty.serializedObject.Update();
                            }, null);
                        }
                        else
                        {
                            // Show disabled for incompatible type
                            menu.AddDisabledItem(new GUIContent("Paste Managed Reference Data"));
                        }
                    }
                }
            }
        }

        private sealed class PolymorphicTypeDropdown : AdvancedDropdown
        {
            private readonly IEnumerable<string> typeNames;
            private readonly Dictionary<AdvancedDropdownItem, int> indexLookup = new();
            private readonly Action<int> onSelectedTypeIndex;

            public PolymorphicTypeDropdown(AdvancedDropdownState state, IEnumerable<string> typeNames, Action<int> onSelectedNewType) : base(state)
            {
                this.typeNames = typeNames;
                onSelectedTypeIndex = onSelectedNewType;

                minimumSize = new Vector2(0, 200);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Types");
                indexLookup.Clear();

                var index = 0;
                foreach (var typeName in typeNames)
                {
                    var item = new AdvancedDropdownItem(typeName);
                    indexLookup.Add(item, index);
                    root.AddChild(item);
                    index++;
                }
                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                base.ItemSelected(item);
                if (indexLookup.TryGetValue(item, out var index))
                {
                    onSelectedTypeIndex.Invoke(index);
                }
            }
        }

        private static bool IsEditingMultipleDifferentTypes(SerializedProperty property)
        {
            UnityObject[] objects = property.serializedObject.targetObjects;
            if (objects.Length > 1)
            {
                string propertyPath = property.propertyPath;
                object firstObject = new SerializedObject(objects[0]).FindProperty(propertyPath).managedReferenceValue;
                bool result = false;
                for (int i = 1; i < objects.Length; i++)
                {
                    object nextObject = new SerializedObject(objects[i]).FindProperty(propertyPath).managedReferenceValue;
                    if (nextObject != null && firstObject != null)
                    {
                        if (!firstObject.GetType().Equals(nextObject.GetType()))
                        {
                            return true;
                        }
                    }
                    else if (nextObject != firstObject)
                    {
                        return true;
                    }
                }
                return result;
            }
            return false;
        }
        private static bool IsEditingMultipleDifferentTypes(SerializedProperty property, out SerializedProperty[] serializedProperties)
        {
            UnityObject[] objects = property.serializedObject.targetObjects;
            if (objects.Length > 1)
            {
                string propertyPath = property.propertyPath;
                serializedProperties = new SerializedProperty[objects.Length];
                serializedProperties[0] = new SerializedObject(objects[0]).FindProperty(propertyPath);
                object firstObject = serializedProperties[0].managedReferenceValue;
                bool result = false;
                for (int i = 1; i < objects.Length; i++)
                {
                    serializedProperties[i] = new SerializedObject(objects[i]).FindProperty(propertyPath);
                    if (!result)
                    {
                        object nextObject = serializedProperties[i].managedReferenceValue;
                        if (nextObject != null && firstObject != null)
                        {
                            if (!firstObject.GetType().Equals(nextObject.GetType()))
                            {
                                result = true;
                            }
                        }
                        else if (nextObject != firstObject)
                        {
                            result = true;
                        }
                    }
                }
                return result;
            }
            serializedProperties = new SerializedProperty[1] { property };
            return false;
        }

        private static void ChangeManagedReferenceType(SerializedProperty property, Type newType)
        {
            if (newType == null)
            {
                property.managedReferenceValue = null;
                return;
            }

            object obj = property.managedReferenceValue;
            if (obj == null)
            {
                property.managedReferenceValue = FormatterServices.GetUninitializedObject(newType);
                return;
            }

            Type currentType = obj.GetType();

            // Fetch current top-level fields
            List<(string name, Type type, object value)> fields = new();
            foreach (var iter in new SerializedPropertyEnumerator(property.Copy()))
            {
                if (TryGetSerializedField(currentType, iter.name, out FieldInfo fieldInfo))
                {
                    fields.Add((iter.name, fieldInfo.FieldType, fieldInfo.GetValue(obj)));
                }
            }

            // Create new object
            obj = FormatterServices.GetUninitializedObject(newType);

            // Migrate shared fields
            currentType = obj.GetType();
            foreach (var field in fields)
            {
                if (TryGetSerializedField(currentType, field.name, out FieldInfo fieldInfo) && fieldInfo.FieldType.IsAssignableFrom(field.type))
                {
                    fieldInfo.SetValue(obj, field.value);
                }
            }

            // Assign new object
            property.managedReferenceValue = obj;
        }
        private static void ChangeManagedReferenceType(SerializedProperty[] properties, Type newType)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                properties[i].serializedObject.Update();
                object obj = properties[i].managedReferenceValue;
                if (obj == null || !obj.GetType().Equals(newType))
                {
                    ChangeManagedReferenceType(properties[i], newType);
                }
                properties[i].serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                position.height = EditorGUIUtility.singleLineHeight;
                position = EditorGUI.PrefixLabel(position, label);
                EditorGUI.LabelField(position, "<Field type must be a managed reference>");
            }
            else
            {
                TryGetTypeFromManagedReferenceTypename(property.managedReferenceFieldTypename, out Type fieldType);
                TryGetTypeFromManagedReferenceTypename(property.managedReferenceFullTypename, out Type currentType);

                bool allowNull = property.TryGetFieldInfo(out FieldInfo fieldInfo, out _) && AllowNull(fieldInfo);

                // Get all supported types
                List<Type> supportedTypes = new();
                if (allowNull) supportedTypes.Insert(0, null);
                supportedTypes.AddRange(GetSupportedFieldTypes(fieldType));
                supportedTypes.Sort((lhs, rhs) =>
                {
                    if (lhs == null && rhs == null) return 0;
                    if (lhs == null) return -1;
                    if (rhs == null) return 1;
                    PolymorphicClassTypeAttribute lhsAttribute = lhs.GetCustomAttribute<PolymorphicClassTypeAttribute>();
                    int lhsOrder = lhsAttribute == null ? int.MinValue : lhsAttribute.Order;
                    PolymorphicClassTypeAttribute rhsAttribute = rhs.GetCustomAttribute<PolymorphicClassTypeAttribute>();
                    int rhsOrder = rhsAttribute == null ? int.MinValue : rhsAttribute.Order;
                    return lhsOrder == rhsOrder ? lhs.Name.CompareTo(rhs.Name) : lhsOrder.CompareTo(rhsOrder);
                });

                // Check if we should show mixed values
                bool showMixedValue = IsEditingMultipleDifferentTypes(property, out SerializedProperty[] serializedProperties);
                using (new EditorExtendUtility.MixedValueScope(showMixedValue))
                {
                    Rect dropdownRect = position;
                    float indent = EditorGUIUtility.labelWidth + 2;
                    dropdownRect.x += indent;
                    dropdownRect.width -= indent;
                    dropdownRect.height = EditorGUIUtility.singleLineHeight;
                    GetTypeEditorInfo(currentType, out string displayName, out string tooltip);
                    if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(text: displayName, tooltip: tooltip), FocusType.Keyboard))
                    {
                        var dropdown = new PolymorphicTypeDropdown(new AdvancedDropdownState(), supportedTypes.Select(GetTypeDisplayName), (int selection) =>
                        {
                            ChangeManagedReferenceType(serializedProperties, supportedTypes[selection]);
                        });
                        dropdown.Show(dropdownRect);
                    }
                }

                EditorGUI.PropertyField(position, property, label, !showMixedValue);
            }

            EditorGUI.EndProperty();
        }

        private static bool TryGetTypeFromManagedReferenceTypename(string typename, out Type type)
        {
            if (!string.IsNullOrEmpty(typename))
            {
                var splitFieldTypename = typename.Split(' ');
                var assemblyName = splitFieldTypename[0];
                var subStringTypeName = splitFieldTypename[1];
                var assembly = Assembly.Load(assemblyName);
                type = assembly.GetType(subStringTypeName);
                return type != null;
            }
            type = null;
            return false;
        }

        private static IEnumerable<Type> GetSupportedFieldTypes(Type fieldType)
        {
            return TypeCache.GetTypesDerivedFrom(fieldType).Where(IsAssignableNonUnityType);
        }
        private static bool IsAssignableType(Type type)
        {
            return type.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface;
        }
        private static bool IsAssignableNonUnityType(Type type)
        {
            return IsAssignableType(type) && !type.IsSubclassOf(typeof(UnityObject));
        }

        private static string GetTypeDisplayName(Type type)
        {
            if (type == null)
            {
                return "null";
            }
            else if (TryGetPolymorphicClassTypeAttribute(type, out var attribute))
            {
                return string.IsNullOrEmpty(attribute.DisplayName) ? ObjectNames.NicifyVariableName(type.Name) : attribute.DisplayName;
            }
            else
            {
                return ObjectNames.NicifyVariableName(type.Name);
            }
        }
        private static void GetTypeEditorInfo(Type type, out string displayName, out string tooltip)
        {
            if (type == null)
            {
                displayName = "null";
                tooltip = string.Empty;
            }
            else if (TryGetPolymorphicClassTypeAttribute(type, out var attribute))
            {
                displayName = string.IsNullOrEmpty(attribute.DisplayName) ? ObjectNames.NicifyVariableName(type.Name) : attribute.DisplayName;
                tooltip = attribute.Tooltip;
            }
            else
            {
                displayName = ObjectNames.NicifyVariableName(type.Name);
                tooltip = string.Empty;
            }
        }

        private static bool TryGetPolymorphicClassTypeAttribute(Type type, out PolymorphicClassTypeAttribute attribute)
        {
            attribute = type.GetCustomAttribute<PolymorphicClassTypeAttribute>();
            return attribute != null;
        }

        private static bool IsSerializedField(FieldInfo fieldInfo)
        {
            return fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null;
        }
        private static bool TryGetSerializedField(Type type, string name, out FieldInfo fieldInfo)
        {
            fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                return false;
            }
            if (!IsSerializedField(fieldInfo))
            {
                fieldInfo = null;
                return false;
            }
            return true;
        }

        private static bool AllowNull(FieldInfo fieldInfo)
        {
            PolymorphicFieldAttribute attribute = fieldInfo.GetCustomAttribute<PolymorphicFieldAttribute>();
            return attribute != null && attribute.AllowNull;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference && !IsEditingMultipleDifferentTypes(property))
            {
                return EditorGUI.GetPropertyHeight(property);
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }
    }
}