using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    public static class PolymorphicFieldUtility
    {
        public static string GetTypeDisplayName(Type type)
        {
            if (type == null)
            {
                return "null";
            }
            else if (TryGetPolymorphicClassTypeAttribute(type, out var attribute))
            {
                string displayName = attribute.DisplayName;
                return string.IsNullOrEmpty(displayName) ? ObjectNames.NicifyVariableName(type.Name) : displayName;
            }
            else
            {
                return ObjectNames.NicifyVariableName(type.Name);
            }
        }
        internal static bool TryGetPolymorphicClassTypeAttribute(Type type, out PolymorphicClassTypeAttribute attribute)
        {
            attribute = type.GetCustomAttribute<PolymorphicClassTypeAttribute>();
            return attribute != null;
        }

        public static IPolymorphicTypeFilter InstantiateFilterObject(Type type)
        {
            try
            {
                if (type != null)
                {
                    if (!typeof(IPolymorphicTypeFilter).IsAssignableFrom(type))
                    {
                        Debug.LogError($"Failed to instantiate polymorphic field type filter: Type {type.Name} does not implement {nameof(IPolymorphicTypeFilter)} interface.");
                        return null;
                    }
                    return (IPolymorphicTypeFilter)Activator.CreateInstance(type);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to instantiate polymorphic field type filter: {e.Message}");
            }
            return null;
        }
    }

    [CustomPropertyDrawer(typeof(PolymorphicFieldAttribute))]
    [InitializeOnLoad]
    internal sealed class PolymorphicFieldPropertyDrawer : PropertyDrawer
    {
        private const string ManagedRefenceDataKey = "MANAGED_REFERENCE_DATA";

        static PolymorphicFieldPropertyDrawer()
        {
            EditorApplication.contextualPropertyMenu += ShowSerializeReferenceCopyPasteContextMenu;
        }

        private static void ShowSerializeReferenceCopyPasteContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference || !property.TryGetFieldInfo(out var fieldInfo, out _))
            {
                return;
            }

            var fieldAttribute = fieldInfo.GetCustomAttribute<PolymorphicFieldAttribute>();
            if (fieldAttribute == null)
            {
                return;
            }

            var copyProperty = property.Copy();
            if (copyProperty.managedReferenceValue != null)
            {
                menu.AddItem(new GUIContent("Copy Managed Reference Data"), false, (_) =>
                {
                    EditorGUIUtility.systemCopyBuffer = $"{ManagedRefenceDataKey}<{copyProperty.managedReferenceFullTypename}>{JsonUtility.ToJson(copyProperty.managedReferenceValue)}";
                }, null);
            }

            string currentClipboard = EditorGUIUtility.systemCopyBuffer;
            if (currentClipboard.StartsWith(ManagedRefenceDataKey))
            {
                // Extract type from clipboard
                int split = currentClipboard.IndexOf('>', ManagedRefenceDataKey.Length);
                string typename = currentClipboard.Substring(ManagedRefenceDataKey.Length + 1, split - ManagedRefenceDataKey.Length - 1);
                if (EditorExtendUtility.TryGetTypeFromManagedReferenceTypename(typename, out Type clipboardType))
                {
                    // Ensure that the clipboard type is supported by the destination field
                    if (EditorExtendUtility.TryGetTypeFromManagedReferenceTypename(copyProperty.managedReferenceFieldTypename, out Type fieldType) && GetSupportedFieldTypes(fieldType, PolymorphicFieldUtility.InstantiateFilterObject(fieldAttribute.FilterType)).Contains(clipboardType))
                    {
                        menu.AddItem(new GUIContent("Paste Managed Reference Data"), false, (_) =>
                        {
                            copyProperty.serializedObject.Update();
                            object obj = FormatterServices.GetUninitializedObject(clipboardType);
                            JsonUtility.FromJsonOverwrite(currentClipboard.Substring(split + 1), obj);
                            copyProperty.managedReferenceValue = obj;
                            copyProperty.serializedObject.ApplyModifiedProperties();
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
            Object[] objects = property.serializedObject.targetObjects;
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
            Object[] objects = property.serializedObject.targetObjects;
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
            object newObj = null;

            if (newType != null)
            {
                // Create new object (call default constructor if available)
                newObj = newType.GetConstructor(Type.EmptyTypes) != null ? Activator.CreateInstance(newType) : FormatterServices.GetUninitializedObject(newType);

                // Migrate compatible property values if possible
                object currentObj = property.managedReferenceValue;
                if (currentObj != null)
                {
                    // Fetch current property data
                    string json = JsonUtility.ToJson(currentObj);

                    // Migrate shared properties
                    JsonUtility.FromJsonOverwrite(json, newObj);
                }
            }

            // Assign new object
            property.managedReferenceValue = newObj;
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
                EditorExtendUtility.TryGetTypeFromManagedReferenceTypename(property.managedReferenceFieldTypename, out Type fieldType);
                EditorExtendUtility.TryGetTypeFromManagedReferenceTypename(property.managedReferenceFullTypename, out Type currentType);

                var fieldAttribute = fieldInfo.GetCustomAttribute<PolymorphicFieldAttribute>();

                // Check if we should show mixed values
                bool showMixedValue = IsEditingMultipleDifferentTypes(property, out SerializedProperty[] serializedProperties);
                GetTypeEditorInfo(currentType, out string displayName, out string tooltip, out bool showFoldout);
                showFoldout |= fieldAttribute.ShowFoldout;
                if (!showFoldout || showMixedValue)
                {
                    Rect labelPos = position;
                    labelPos.height = EditorExtendUtility.SingleLineHeight;
                    EditorGUI.PrefixLabel(position, label);
                }
                using (new EditorExtendUtility.MixedValueScope(showMixedValue))
                {
                    Rect dropdownRect = position;
                    float indent = EditorGUIUtility.labelWidth + 2;
                    dropdownRect.x += indent;
                    dropdownRect.width -= indent;
                    dropdownRect.height = EditorGUIUtility.singleLineHeight;
                    if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(text: displayName, tooltip: tooltip), FocusType.Keyboard))
                    {
                        // Get all supported types
                        List<Type> supportedTypes = new();
                        supportedTypes.AddRange(GetSupportedFieldTypes(fieldType, PolymorphicFieldUtility.InstantiateFilterObject(fieldAttribute.FilterType)));
                        if (fieldAttribute.AllowNull) supportedTypes.Insert(0, null);
                        var dropdown = new PolymorphicTypeDropdown(new AdvancedDropdownState(), supportedTypes.Select(PolymorphicFieldUtility.GetTypeDisplayName), (int selection) =>
                        {
                            ChangeManagedReferenceType(serializedProperties, supportedTypes[selection]);
                        });
                        dropdown.Show(dropdownRect);
                    }
                }

                if (!showMixedValue)
                {
                    if (showFoldout)
                    {
                        EditorGUI.PropertyField(position, property, label, !showMixedValue);
                    }
                    else
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            position.y += EditorExtendUtility.TotalPropertyHeight;
                            foreach (var iter in new SerializedPropertyEnumerator(property))
                            {
                                float height = EditorGUI.GetPropertyHeight(iter, iter.hasVisibleChildren);
                                position.height = height;
                                EditorGUI.PropertyField(position, iter, iter.hasVisibleChildren);
                                position.y += height;
                                position.y += EditorExtendUtility.StandardVerticalSpacing;
                            }
                        }
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        private static readonly List<Type> supportedTypeListBuilder = new();
        private static readonly Dictionary<Type, Type[]> supportedFieldTypeCache = new();
        private static IEnumerable<Type> GetSupportedFieldTypes(Type fieldType, IPolymorphicTypeFilter filter = null)
        {
            // A filter can modify supported types at runtime, so it invalidates the cache
            if (filter != null || !supportedFieldTypeCache.TryGetValue(fieldType, out var supportedTypes))
            {
                bool CheckFilter(Type type) => filter == null || filter.IncludeType(type);
                supportedTypeListBuilder.Clear();
                foreach (var type in TypeCache.GetTypesDerivedFrom(fieldType).Where(IsAssignableType))
                {
                    if (type.GetCustomAttribute<ObsoleteAttribute>() == null && CheckFilter(type))
                    {
                        supportedTypeListBuilder.Add(type);
                    }
                }
                if (IsAssignableType(fieldType) && CheckFilter(fieldType))
                {
                    supportedTypeListBuilder.Add(fieldType);
                }
                supportedTypeListBuilder.Sort((lhs, rhs) =>
                {
                    return PolymorphicFieldUtility.GetTypeDisplayName(lhs).CompareTo(PolymorphicFieldUtility.GetTypeDisplayName(rhs));
                });
                supportedTypes = supportedTypeListBuilder.ToArray();
                if (filter == null)
                {
                    supportedFieldTypeCache[fieldType] = supportedTypes;
                }
            }
            return supportedTypes;
        }
        private static bool IsAssignableType(Type type)
        {
            return type.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && !type.IsSubclassOf(typeof(Object));
        }

        private static void GetTypeEditorInfo(Type type, out string displayName, out string tooltip, out bool showFoldout)
        {
            if (type == null)
            {
                displayName = "null";
                tooltip = string.Empty;
                showFoldout = false;
            }
            else if (PolymorphicFieldUtility.TryGetPolymorphicClassTypeAttribute(type, out var attribute))
            {
                displayName = attribute.DisplayName;
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = ObjectNames.NicifyVariableName(type.Name);
                }
                tooltip = attribute.Tooltip;
                showFoldout = attribute.ShowFoldout;
            }
            else
            {
                displayName = ObjectNames.NicifyVariableName(type.Name);
                tooltip = string.Empty;
                showFoldout = false;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference || IsEditingMultipleDifferentTypes(property))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var fieldAttribute = fieldInfo.GetCustomAttribute<PolymorphicFieldAttribute>();
            bool showFoldout = fieldAttribute.ShowFoldout;
            if (EditorExtendUtility.TryGetTypeFromManagedReferenceTypename(property.managedReferenceFullTypename, out Type currentType))
            {
                if (PolymorphicFieldUtility.TryGetPolymorphicClassTypeAttribute(currentType, out var attribute))
                {
                    showFoldout |= attribute.ShowFoldout;
                }
            }

            if (!showFoldout)
            {
                property.isExpanded = true;
            }

            return EditorGUI.GetPropertyHeight(property);
        }
    }
}