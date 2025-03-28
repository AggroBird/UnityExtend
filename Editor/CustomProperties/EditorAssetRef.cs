using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    public static class EditorAssetRefUtility
    {
        public static EditorAssetRef<T>[] MakeArray<T>(IList<T> assets) where T : UnityObject
        {
            if (assets == null || assets.Count == 0)
            {
                return Array.Empty<EditorAssetRef<T>>();
            }
            EditorAssetRef<T>[] result = new EditorAssetRef<T>[assets.Count];
            for (int i = 0; i < assets.Count; i++)
            {
                result[i] = assets[i];
            }
            return result;
        }
        public static T[] Load<T>(IList<EditorAssetRef<T>> assetRefs) where T : UnityObject
        {
            if (assetRefs == null || assetRefs.Count == 0)
            {
                return Array.Empty<T>();
            }
            T[] result = new T[assetRefs.Count];
            for (int i = 0; i < assetRefs.Count; i++)
            {
                result[i] = assetRefs[i];
            }
            return result;
        }
    }

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

        public override readonly int GetHashCode() => string.IsNullOrEmpty(guid) ? 0 : guid.GetHashCode();
        public override readonly bool Equals(object obj) => (obj is EditorAssetRef<T> other) && Equals(other);
        public readonly bool Equals(EditorAssetRef<T> other)
        {
            bool a = string.IsNullOrEmpty(guid);
            bool b = string.IsNullOrEmpty(other.guid);
            return (a || b) ? (a == b) : (guid == other.guid);
        }

        public static bool operator ==(EditorAssetRef<T> lhs, EditorAssetRef<T> rhs) => lhs.Equals(rhs);
        public static bool operator !=(EditorAssetRef<T> lhs, EditorAssetRef<T> rhs) => !lhs.Equals(rhs);

        public static implicit operator T(EditorAssetRef<T> assetRef)
        {
            return assetRef.TryLoad(out T asset) ? asset : default;
        }
        public static implicit operator EditorAssetRef<T>(T asset)
        {
            return asset && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long _) ? new(guid) : default;
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