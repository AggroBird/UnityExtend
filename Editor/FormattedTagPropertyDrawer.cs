using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(FormattedTagAttribute))]
    internal sealed class FormattedTagPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            FormattedTagAttribute attribute = fieldInfo.GetCustomAttribute<FormattedTagAttribute>();

            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label, true);
                EditorExtendUtility.FormatTag(property, attribute.MaxLength);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "FormattedTag attribute must be used with string");
            }

            EditorGUI.EndProperty();
        }
    }
}
