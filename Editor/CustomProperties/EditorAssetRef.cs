using System;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    [Serializable]
    public struct EditorAssetRef<T> where T : UnityObject
    {
        internal EditorAssetRef(string guid)
        {
            this.guid = guid;
        }

        [SerializeField]
        private string guid;


        public readonly bool TryLoad(out T asset)
        {
            if (!string.IsNullOrEmpty(guid))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    return asset;
                }
            }
            asset = default;
            return false;
        }

        public static implicit operator T(EditorAssetRef<T> assetRef)
        {
            return assetRef.TryLoad(out T asset) ? asset : default;
        }
        public static implicit operator EditorAssetRef<T>(T asset)
        {
            return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long _) ? new(guid) : default;
        }

        public static EditorAssetRef<T>[] MakeArray(T[] assets)
        {
            if (assets == null || assets.Length == 0)
            {
                return Array.Empty<EditorAssetRef<T>>();
            }
            EditorAssetRef<T>[] result = new EditorAssetRef<T>[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                result[i] = assets[i];
            }
            return result;
        }
        public static T[] LoadArray(EditorAssetRef<T>[] assetRefs)
        {
            if (assetRefs == null || assetRefs.Length == 0)
            {
                return Array.Empty<T>();
            }
            T[] result = new T[assetRefs.Length];
            for (int i = 0; i < assetRefs.Length; i++)
            {
                result[i] = assetRefs[i];
            }
            return result;
        }
    }

    [CustomPropertyDrawer(typeof(EditorAssetRef<>))]
    internal class EditorAssetRefPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guidProperty = property.FindPropertyRelative("guid");

            EditorGUI.BeginProperty(position, label, property);

            bool multipleDifferentValues = property.hasMultipleDifferentValues;

            UnityObject currentValue = null;
            if (!multipleDifferentValues)
            {
                string guid = guidProperty.stringValue;
                if (!string.IsNullOrEmpty(guid))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        currentValue = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityObject));
                        if (!currentValue)
                        {
                            currentValue = EditorExtendUtility.MissingObject;
                        }
                    }
                    else
                    {
                        currentValue = EditorExtendUtility.MissingObject;
                    }
                }
            }

            using (new EditorExtendUtility.MixedValueScope(multipleDifferentValues))
            {
                Type referenceType = (fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType() : fieldInfo.FieldType).GetGenericArguments()[0];

                EditorGUI.BeginChangeCheck();
                var value = EditorGUI.ObjectField(position, label, currentValue, referenceType, false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!value || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out string guid, out long _))
                    {
                        guidProperty.stringValue = string.Empty;
                    }
                    else
                    {
                        guidProperty.stringValue = guid;
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }
}