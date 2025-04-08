using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExcludeFromPropertyDrawerTypeCacheAttribute : Attribute
    {

    }

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
            static bool TryGetCachedDrawerTypeForType(FieldInfo fieldInfo, out Type drawerType)
            {
                if (reflectionSucceeded)
                {
                    Type propertyType = fieldInfo.FieldType;
                    if (propertyDrawerTypeCache == null)
                    {
                        propertyDrawerTypeCache = new();
                        foreach (Type drawer in TypeCache.GetTypesDerivedFrom<GUIDrawer>())
                        {
                            if (drawer.GetCustomAttribute<ExcludeFromPropertyDrawerTypeCacheAttribute>() != null)
                            {
                                continue;
                            }

                            var customAttributes = drawer.GetCustomAttributes<CustomPropertyDrawer>(inherit: true);
                            foreach (var customPropertyDrawer in customAttributes)
                            {
                                Type type = (Type)customPropertyDrawerTypeFieldInfo.GetValue(customPropertyDrawer);
                                void AssignChildTypes()
                                {
                                    foreach (Type derivedType in TypeCache.GetTypesDerivedFrom(type))
                                    {
                                        if (!propertyDrawerTypeCache.ContainsKey(derivedType) || !type.IsAssignableFrom(propertyDrawerTypeCache[derivedType].type))
                                        {
                                            propertyDrawerTypeCache[derivedType] = (drawer, type);
                                        }
                                    }
                                }
                                bool useForChildren = (bool)customPropertyDrawerUseForChildrenFieldInfo.GetValue(customPropertyDrawer);
                                if (type.IsSubclassOf(typeof(PropertyAttribute)))
                                {
                                    foreach (var fieldAttribute in fieldInfo.CustomAttributes)
                                    {
                                        var attributeType = fieldAttribute.AttributeType;
                                        if (attributeType.IsSubclassOf(type) || attributeType.Equals(type))
                                        {
                                            propertyDrawerTypeCache[type] = (drawer, type);
                                            if (useForChildren)
                                            {
                                                AssignChildTypes();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    propertyDrawerTypeCache[type] = (drawer, type);
                                    if (useForChildren)
                                    {
                                        AssignChildTypes();
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
            if (TryGetCachedDrawerTypeForType(fieldInfo, out var drawerType))
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
            else if (true)
            {

            }
            propertyDrawer = null;
            return false;
        }
    }
}