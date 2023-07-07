using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityEngineExtend.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphicFieldAttribute))]
    internal sealed class PolymorphicFieldPropertyDrawer : PropertyDrawer
    {
        private sealed class PolymorphicTypeCacheData
        {
            public PolymorphicTypeCacheData(Type[] types)
            {
                this.types = types;

                if (types.Length > 0)
                {
                    // Create friendly type names
                    dropdownOptions = new string[types.Length];
                    for (int i = 0; i < types.Length; i++)
                    {
                        dropdownOptions[i] = ObjectNames.NicifyVariableName(types[i].Name);
                    }
                }
                else
                {
                    dropdownOptions = Array.Empty<string>();
                }
            }

            public readonly Type[] types;
            public readonly string[] dropdownOptions;
        }

        private sealed class PolymorphicTypeCache
        {
            private static readonly Dictionary<Type, PolymorphicTypeCacheData> cacheDataLookup = new();
            private static readonly List<Type> typeListBuilder = new();

            public bool TryGetCacheData(SerializedProperty property, out PolymorphicTypeCacheData result)
            {
                if (property.TryGetFieldInfo(out _, out Type fieldType))
                {
                    if (!cacheDataLookup.TryGetValue(fieldType, out result))
                    {
                        typeListBuilder.Clear();
                        if (!fieldType.IsAbstract)
                        {
                            typeListBuilder.Add(fieldType);
                        }
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            foreach (var subType in assembly.GetTypes())
                            {
                                if (!subType.IsAbstract && subType.IsSubclassOf(fieldType))
                                {
                                    typeListBuilder.Add(subType);
                                }
                            }
                        }
                        typeListBuilder.Sort((lhs, rhs) =>
                        {
                            PolymorphicClassTypeAttribute lhsAttribute = lhs.GetCustomAttribute<PolymorphicClassTypeAttribute>();
                            int lhsOrder = lhsAttribute == null ? int.MinValue : lhsAttribute.Order;
                            PolymorphicClassTypeAttribute rhsAttribute = rhs.GetCustomAttribute<PolymorphicClassTypeAttribute>();
                            int rhsOrder = rhsAttribute == null ? int.MinValue : rhsAttribute.Order;
                            return lhsOrder == rhsOrder ? lhs.Name.CompareTo(rhs.Name) : lhsOrder.CompareTo(rhsOrder);
                        });

                        result = new PolymorphicTypeCacheData(typeListBuilder.ToArray());
                        cacheDataLookup.Add(fieldType, result);
                    }
                    return result.types.Length > 0;
                }

                result = null;
                return false;
            }
        }

        private static readonly PolymorphicTypeCache typeCache = new PolymorphicTypeCache();

        private const BindingFlags SerializedFieldBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static bool IsSerializedField(FieldInfo fieldInfo)
        {
            return fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null;
        }
        private static bool TryGetSerializedField(Type type, string name, out FieldInfo fieldInfo)
        {
            fieldInfo = type.GetField(name, SerializedFieldBindingFlags);
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
                return result;
            }
            serializedProperties = new SerializedProperty[1] { property };
            return false;
        }

        private static void ChangeManagedReferenceType(SerializedProperty property, Type newType)
        {
            object obj = property.managedReferenceValue;
            if (obj == null)
            {
                property.managedReferenceValue = Activator.CreateInstance(newType);
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
            obj = Activator.CreateInstance(newType);

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

            position.height = EditorGUIUtility.singleLineHeight;

            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                position = EditorGUI.PrefixLabel(position, label);
                EditorGUI.LabelField(position, "<Field type must be a managed reference>");
            }
            else if (!typeCache.TryGetCacheData(property, out PolymorphicTypeCacheData cacheData))
            {
                position = EditorGUI.PrefixLabel(position, label);
                EditorGUI.LabelField(position, "<Failed to find compatible classes>");
            }
            else
            {
                if (IsEditingMultipleDifferentTypes(property, out SerializedProperty[] serializedProperties))
                {
                    GUI.contentColor = new Color(1, 1, 1, 0.5f);
                    position = EditorGUI.PrefixLabel(position, label);
                    using (new EditorExtendUtility.MixedValueScope(true))
                    {
                        int indentLevel = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;
                        EditorGUI.BeginChangeCheck();
                        int selectedType = EditorGUI.Popup(position, -1, cacheData.dropdownOptions);
                        if (EditorGUI.EndChangeCheck())
                        {
                            ChangeManagedReferenceType(serializedProperties, cacheData.types[selectedType]);
                        }
                        EditorGUI.indentLevel = indentLevel;
                    }
                    GUI.contentColor = Color.white;
                }
                else
                {
                    object obj = property.managedReferenceValue;

                    float labelWidth = EditorGUIUtility.labelWidth + 2f;
                    Rect propertyFieldPos = position;
                    propertyFieldPos.width = labelWidth;
                    bool expand = EditorGUI.PropertyField(propertyFieldPos, property, label, false);

                    // Get current type
                    int currentSelection = -1;
                    List<string> dropdownOptions = new(cacheData.dropdownOptions);
                    if (obj != null)
                    {
                        Type objType = obj.GetType();
                        for (int i = 0; i < cacheData.types.Length; i++)
                        {
                            if (objType.Equals(cacheData.types[i]))
                            {
                                currentSelection = i;
                                break;
                            }
                        }
                    }
                    bool allowNull = currentSelection == -1 || (property.TryGetFieldInfo(out FieldInfo fieldInfo, out _) && AllowNull(fieldInfo));
                    if (allowNull)
                    {
                        dropdownOptions.Insert(0, "<null>");
                        currentSelection++;
                    }

                    int indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    Rect typeFieldPos = position;
                    typeFieldPos.x += labelWidth;
                    typeFieldPos.width -= labelWidth;
                    EditorGUI.BeginChangeCheck();
                    int selectedType = EditorGUI.Popup(typeFieldPos, currentSelection, dropdownOptions.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (allowNull)
                        {
                            if (selectedType == 0)
                            {
                                foreach (var p in serializedProperties)
                                {
                                    p.managedReferenceValue = null;
                                }
                            }
                            else
                            {
                                ChangeManagedReferenceType(serializedProperties, cacheData.types[selectedType - 1]);
                            }
                        }
                        else
                        {
                            ChangeManagedReferenceType(serializedProperties, cacheData.types[selectedType]);
                        }
                    }
                    EditorGUI.indentLevel = indentLevel;

                    if (expand)
                    {
                        position.y += EditorExtendUtility.SinglePropertyHeight;

                        using (new EditorGUI.IndentLevelScope(1))
                        {
                            foreach (var iter in new SerializedPropertyEnumerator(property))
                            {
                                position.height = EditorGUI.GetPropertyHeight(iter);
                                EditorGUI.PropertyField(position, iter, new GUIContent(iter.displayName), true);
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference && !IsEditingMultipleDifferentTypes(property))
            {
                return EditorGUI.GetPropertyHeight(property);
            }
            else
            {
                return EditorExtendUtility.SinglePropertyHeight;
            }
        }
    }
}