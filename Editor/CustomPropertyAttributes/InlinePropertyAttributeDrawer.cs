using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(InlinePropertyAttribute))]
    internal class InlinePropertyAttributeDrawer : PropertyDrawer
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
                foreach (var iter in new SerializedPropertyEnumerator(property))
                {
                    float height = EditorGUI.GetPropertyHeight(iter, iter.hasVisibleChildren);
                    position.height = height;
                    EditorGUI.PropertyField(position, iter, iter.hasVisibleChildren);
                    position.y += height;
                    position.y += EditorExtendUtility.StandardVerticalSpacing;
                }
            }
            else
            {
                base.OnGUI(position, property, label);
            }
        }
    }
}