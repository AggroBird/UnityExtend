using System;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    [Serializable]
    public struct EditorAssetRef<T> where T : UnityObject
    {
        [SerializeField]
        private string guid;


        public T Load()
        {
            if (!string.IsNullOrEmpty(guid))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(assetPath);
                }
            }
            return null;
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