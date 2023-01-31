using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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
                if (property.TryGetFieldType(out Type fieldType))
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
                        typeListBuilder.Sort((a, b) => a.Name.CompareTo(b.Name));

                        result = new PolymorphicTypeCacheData(typeListBuilder.ToArray());
                        cacheDataLookup.Add(fieldType, result);
                    }
                    return result.types.Length > 0;
                }

                result = null;
                return false;
            }
        }

        private static PolymorphicTypeCache typeCache = new PolymorphicTypeCache();

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


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                GUI.Label(position, "Field type must be a managed reference");
            }
            else if (typeCache.TryGetCacheData(property, out PolymorphicTypeCacheData cacheData))
            {
                // Ensure there is at least one object
                object obj = property.managedReferenceValue;
                if (obj == null && cacheData.types.Length > 0)
                {
                    property.managedReferenceValue = obj = Activator.CreateInstance(cacheData.types[0]);
                }

                position.height = EditorGUIUtility.singleLineHeight;
                if (EditorGUI.PropertyField(position, property, label, false))
                {
                    if (obj != null)
                    {
                        EditorGUI.indentLevel++;
                        position.y += EditorExtendUtility.SinglePropertyHeight;

                        // Get current type
                        int currentType = -1;
                        for (int i = 0; i < cacheData.types.Length; i++)
                        {
                            if (property.managedReferenceValue.GetType().Equals(cacheData.types[i]))
                            {
                                currentType = i;
                                break;
                            }
                        }

                        int selectedType = EditorGUI.Popup(position, "Type", currentType, cacheData.dropdownOptions);
                        if (selectedType != currentType)
                        {
                            // Fetch current top-level fields
                            Type type = obj.GetType();
                            List<(string name, Type type, object value)> fields = new();
                            foreach (var iter in new SerializedPropertyEnumerator(property.Copy()))
                            {
                                if (TryGetSerializedField(type, iter.name, out FieldInfo fieldInfo))
                                {
                                    fields.Add((iter.name, fieldInfo.FieldType, fieldInfo.GetValue(obj)));
                                }
                            }

                            // Create new object
                            obj = Activator.CreateInstance(cacheData.types[selectedType]);

                            // Migrate shared fields
                            type = obj.GetType();
                            foreach (var field in fields)
                            {
                                if (TryGetSerializedField(type, field.name, out FieldInfo fieldInfo) && fieldInfo.FieldType.IsAssignableFrom(field.type))
                                {
                                    fieldInfo.SetValue(obj, field.value);
                                }
                            }

                            // Assign new object
                            property.managedReferenceValue = obj;
                        }
                        position.y += EditorExtendUtility.SinglePropertyHeight;

                        // Draw object
                        Rect subPos = position;
                        foreach (var iter in new SerializedPropertyEnumerator(property))
                        {
                            subPos.height = EditorGUI.GetPropertyHeight(iter);
                            EditorGUI.PropertyField(subPos, iter, label, true);
                            subPos.y += EditorExtendUtility.SinglePropertyHeight;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            else
            {
                GUI.Label(position, "Failed to find compatible classes");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                float height = EditorGUI.GetPropertyHeight(property);
                if (property.isExpanded)
                {
                    // Make space for the type field
                    height += EditorExtendUtility.SinglePropertyHeight;
                }
                return height;
            }
            else
            {
                return EditorExtendUtility.SinglePropertyHeight;
            }
        }
    }
}