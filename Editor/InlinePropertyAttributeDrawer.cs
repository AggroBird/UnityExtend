using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityEngineExtend.Editor
{
    [CustomPropertyDrawer(typeof(InlinePropertyAttribute))]
    public class InlinePropertyAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.hasChildren)
            {
                property.isExpanded = true;
                return EditorGUI.GetPropertyHeight(property, label, true) - EditorGUI.GetPropertyHeight(property, label, false);
            }
            else
            {
                return base.GetPropertyHeight(property, label);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.hasChildren)
            {
                SerializedProperty iter = property.Copy();
                property.Next(true);
                do
                {
                    float height = EditorGUI.GetPropertyHeight(property, property.hasVisibleChildren);
                    position.height = height;
                    EditorGUI.PropertyField(position, property, property.hasVisibleChildren);
                    position.y = position.y + height;
                }
                while (property.NextVisible(false));
            }
            else
            {
                base.OnGUI(position, property, label);
            }
        }
    }
}