using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    internal sealed class LabelNameProviderCache<T>
    {
        private static readonly List<object> propertyValueBuffer = new();

        private readonly Dictionary<Type, T> cache = new();

        public bool TryGetProvider(SerializedProperty property, out T provider, out int index)
        {
            if (EditorExtendUtility.TryGetFieldInfo(property, out FieldInfo fieldInfo, out _, stackTrace: propertyValueBuffer))
            {
                GlobalLabelNameProviderAttribute globalProviderAttribute = fieldInfo.GetCustomAttribute<GlobalLabelNameProviderAttribute>();
                if (globalProviderAttribute != null)
                {
                    // Load from asset
                    if (globalProviderAttribute.ProviderType != null)
                    {
                        if (!cache.TryGetValue(globalProviderAttribute.ProviderType, out T cachedProvider) || cachedProvider is not UnityObject unityObj || !unityObj)
                        {
                            string[] guids = AssetDatabase.FindAssets($"t:{globalProviderAttribute.ProviderType.Name}");
                            if (guids != null && guids.Length > 0)
                            {
                                for (int i = 0; i < guids.Length; i++)
                                {
                                    UnityObject obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), globalProviderAttribute.ProviderType);
                                    if (obj && obj is T validProvider)
                                    {
                                        cache[globalProviderAttribute.ProviderType] = validProvider;
                                        provider = validProvider;
                                        index = globalProviderAttribute.Index;
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            provider = cachedProvider;
                            index = globalProviderAttribute.Index;
                            return true;
                        }
                    }
                }
                else
                {
                    NestedLabelNameProviderAttribute nestedProviderAttribute = fieldInfo.GetCustomAttribute<NestedLabelNameProviderAttribute>();
                    if (nestedProviderAttribute != null)
                    {
                        switch (nestedProviderAttribute.Source)
                        {
                            case NestedNameProviderSource.DeclaringType:
                            {
                                // Try to get from parent property
                                if (propertyValueBuffer.Count > 0 && propertyValueBuffer[^1] is T validProvider)
                                {
                                    provider = validProvider;
                                    index = nestedProviderAttribute.Index;
                                    return true;
                                }
                            }
                            break;
                            case NestedNameProviderSource.SerializedObject:
                            {
                                // Try to get from serialized object
                                UnityObject obj = property.serializedObject.targetObject;
                                if (obj && obj is T validProvider)
                                {
                                    provider = validProvider;
                                    index = nestedProviderAttribute.Index;
                                    return true;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            provider = default;
            index = default;
            return false;
        }
    }
}