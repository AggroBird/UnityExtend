using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(TextureAttribute))]
    internal sealed class TextureAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var fieldType = fieldInfo.FieldType;
            if (fieldType == typeof(Texture))
            {
                property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Texture), false);
            }
            else if (fieldType == typeof(Texture2D))
            {
                property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Texture2D), false);
            }
            else if (fieldType == typeof(Texture3D))
            {
                property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Texture3D), false);
            }
            else if (fieldType == typeof(Sprite))
            {
                property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Sprite), false);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Invalid property type used for Texture attribute");
            }

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 64;
        }
    }
}