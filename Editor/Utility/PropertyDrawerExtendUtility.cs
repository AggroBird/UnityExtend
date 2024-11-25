using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    public static class PropertyDrawerExtendUtility
    {
        static PropertyDrawerExtendUtility()
        {
            reflectionSucceeded = true;
            Type propertyDrawerType = typeof(PropertyDrawer);
            Type customPropertyDrawerType = typeof(CustomPropertyDrawer);
            customPropertyDrawerTypeFieldInfo = customPropertyDrawerType.GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            reflectionSucceeded &= customPropertyDrawerTypeFieldInfo != null;
            customPropertyDrawerUseForChildrenFieldInfo = customPropertyDrawerType.GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);
            reflectionSucceeded &= customPropertyDrawerUseForChildrenFieldInfo != null;
            propertyDrawerFieldInfoFieldInfo = propertyDrawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            reflectionSucceeded &= propertyDrawerFieldInfoFieldInfo != null;
            if (!reflectionSucceeded)
            {
                Debug.LogWarning("Failed to initialize property drawer reflection info; maybe the API has changed.");
            }
        }
        private static readonly FieldInfo customPropertyDrawerTypeFieldInfo;
        private static readonly FieldInfo customPropertyDrawerUseForChildrenFieldInfo;
        private static readonly FieldInfo propertyDrawerFieldInfoFieldInfo;
        private static readonly bool reflectionSucceeded;

        private static Dictionary<Type, (Type drawer, Type type)> propertyDrawerTypeCache = null;

        // Referenced from unity assembly
        public static bool TryGetPropertyDrawer(FieldInfo fieldInfo, out PropertyDrawer propertyDrawer)
        {
            static bool TryGetCachedDrawerTypeForType(Type propertyType, out Type drawerType)
            {
                if (reflectionSucceeded)
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
                                if (useForChildren)
                                {
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
                    }
                    if (propertyDrawerTypeCache.TryGetValue(propertyType, out var value))
                    {
                        drawerType = value.drawer;
                        return true;
                    }
                    if (propertyType.IsGenericType && propertyDrawerTypeCache.TryGetValue(propertyType.GetGenericTypeDefinition(), out value))
                    {
                        drawerType = value.drawer;
                        return true;
                    }
                }
                drawerType = null;
                return false;
            }
            if (TryGetCachedDrawerTypeForType(fieldInfo.FieldType, out var drawerType))
            {
                try
                {
                    propertyDrawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
                    propertyDrawerFieldInfoFieldInfo.SetValue(propertyDrawer, fieldInfo);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            propertyDrawer = null;
            return false;
        }
    }
}