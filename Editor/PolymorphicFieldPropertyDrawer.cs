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
            position.height = EditorGUIUtility.singleLineHeight;

            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                GUI.Label(position, "Field type must be a managed reference");
            }
            else if (typeCache.TryGetCacheData(property, out PolymorphicTypeCacheData cacheData))
            {
                // Ensure there is at least one object
                object obj = property.managedReferenceValue;
                if (obj == null)
                {
                    property.managedReferenceValue = obj = Activator.CreateInstance(cacheData.types[0]);
                }

                EditorGUI.BeginProperty(position, label, property);

                float labelWidth = EditorGUIUtility.labelWidth + 2f;
                Rect propertyFieldPos = position;
                propertyFieldPos.width = labelWidth;
                bool expand = EditorGUI.PropertyField(propertyFieldPos, property, label, false);

                Type objType = obj.GetType();

                // Get current type
                int currentType = -1;
                for (int i = 0; i < cacheData.types.Length; i++)
                {
                    if (objType.Equals(cacheData.types[i]))
                    {
                        currentType = i;
                        break;
                    }
                }

                Rect typeFieldPos = position;

                typeFieldPos.x += labelWidth;
                typeFieldPos.width -= labelWidth;
                int selectedType = EditorGUI.Popup(typeFieldPos, currentType, cacheData.dropdownOptions);
                if (selectedType != currentType)
                {
                    // Fetch current top-level fields
                    List<(string name, Type type, object value)> fields = new();
                    foreach (var iter in new SerializedPropertyEnumerator(property.Copy()))
                    {
                        if (TryGetSerializedField(objType, iter.name, out FieldInfo fieldInfo))
                        {
                            fields.Add((iter.name, fieldInfo.FieldType, fieldInfo.GetValue(obj)));
                        }
                    }

                    // Create new object
                    obj = Activator.CreateInstance(cacheData.types[selectedType]);

                    // Migrate shared fields
                    objType = obj.GetType();
                    foreach (var field in fields)
                    {
                        if (TryGetSerializedField(objType, field.name, out FieldInfo fieldInfo) && fieldInfo.FieldType.IsAssignableFrom(field.type))
                        {
                            fieldInfo.SetValue(obj, field.value);
                        }
                    }

                    // Assign new object
                    property.managedReferenceValue = obj;
                }

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

                EditorGUI.EndProperty();
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
                return EditorGUI.GetPropertyHeight(property);
            }
            else
            {
                return EditorExtendUtility.SinglePropertyHeight;
            }
        }
    }
}