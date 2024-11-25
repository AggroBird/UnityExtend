using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal sealed class ReadOnlyAttributeDrawer : PropertyDrawer, IDisposable
    {
        static ReadOnlyAttributeDrawer()
        {
            reflectionSucceeded = true;
            customPropertyDrawerTypeFieldInfo = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            reflectionSucceeded &= customPropertyDrawerTypeFieldInfo != null;
            customPropertyDrawerUseForChildrenFieldInfo = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);
            reflectionSucceeded &= customPropertyDrawerUseForChildrenFieldInfo != null;
            propertyDrawerFieldInfoFieldInfo = typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            reflectionSucceeded &= propertyDrawerFieldInfoFieldInfo != null;
        }
        private static readonly FieldInfo customPropertyDrawerTypeFieldInfo;
        private static readonly FieldInfo customPropertyDrawerUseForChildrenFieldInfo;
        private static readonly FieldInfo propertyDrawerFieldInfoFieldInfo;
        private static readonly bool reflectionSucceeded;

        private static Dictionary<Type, (Type drawer, Type type)> propertyDrawerTypeCache = null;
        private PropertyDrawer cachedPropertyDrawer;

        // Referenced from unity assembly
        private bool TryGetCachedDefaultPropertyDrawer(Type propertyType, out PropertyDrawer propertyDrawer)
        {
            static Type GetCachedDrawerTypeForType(Type propertyType)
            {
                if (propertyDrawerTypeCache == null)
                {
                    propertyDrawerTypeCache = new();
                    foreach (Type drawer in TypeCache.GetTypesDerivedFrom<GUIDrawer>())
                    {
                        var customAttributes = drawer.GetCustomAttributes<CustomPropertyDrawer>(inherit: true);
                        foreach (var customPropertyDrawer in customAttributes)
                        {
                            Type type = (Type)customPropertyDrawerTypeFieldInfo.GetValue(customPropertyDrawer);
                            bool useForChildren = (bool)customPropertyDrawerUseForChildrenFieldInfo.GetValue(customPropertyDrawer);
                            propertyDrawerTypeCache[type] = (drawer, type);
                            if (!useForChildren)
                            {
                                continue;
                            }
                            foreach (Type derivedType in TypeCache.GetTypesDerivedFrom(type))
                            {
                                if (!propertyDrawerTypeCache.ContainsKey(derivedType) || !type.IsAssignableFrom(propertyDrawerTypeCache[derivedType].type))
                                {
                                    propertyDrawerTypeCache[derivedType] = (drawer, type);
                                }
                            }
                        }
                    }
                }
                propertyDrawerTypeCache.TryGetValue(propertyType, out var value);
                if (value.drawer != null)
                {
                    return value.drawer;
                }
                if (propertyType.IsGenericType)
                {
                    propertyDrawerTypeCache.TryGetValue(propertyType.GetGenericTypeDefinition(), out value);
                }
                return value.drawer;
            }

            if (cachedPropertyDrawer != null)
            {
                propertyDrawer = cachedPropertyDrawer;
                return true;
            }
            if (reflectionSucceeded)
            {
                Type drawerType = GetCachedDrawerTypeForType(propertyType);
                if (drawerType != null)
                {
                    try
                    {
                        propertyDrawer = cachedPropertyDrawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
                        propertyDrawerFieldInfoFieldInfo.SetValue(propertyDrawer, fieldInfo);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
            propertyDrawer = null;
            return false;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                if (TryGetCachedDefaultPropertyDrawer(fieldInfo.FieldType, out var defaultPropertyDrawer))
                {
                    defaultPropertyDrawer.OnGUI(position, property, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (TryGetCachedDefaultPropertyDrawer(fieldInfo.FieldType, out var defaultPropertyDrawer))
            {
                return defaultPropertyDrawer.GetPropertyHeight(property, label);
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
        }
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            if (TryGetCachedDefaultPropertyDrawer(fieldInfo.FieldType, out var defaultPropertyDrawer))
            {
                return defaultPropertyDrawer.CanCacheInspectorGUI(property);
            }
            else
            {
                return base.CanCacheInspectorGUI(property);
            }
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (TryGetCachedDefaultPropertyDrawer(fieldInfo.FieldType, out var defaultPropertyDrawer))
            {
                return defaultPropertyDrawer.CreatePropertyGUI(property);
            }
            else
            {
                return base.CreatePropertyGUI(property);
            }
        }

        public void Dispose()
        {
            if (cachedPropertyDrawer is IDisposable disposable)
            {
                disposable.Dispose();
            }
            cachedPropertyDrawer = null;
        }
    }
}