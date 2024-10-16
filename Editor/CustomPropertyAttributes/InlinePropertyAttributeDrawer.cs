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

                return EditorGUI.GetPropertyHeight(property, label, true);
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
                property.isExpanded = true;

                EditorGUI.BeginProperty(position, label, property);

                position.height = EditorExtendUtility.SingleLineHeight;
                EditorGUI.LabelField(position, label);
                position.y += position.height + EditorExtendUtility.StandardVerticalSpacing;
                using (new EditorGUI.IndentLevelScope())
                {
                    foreach (var iter in new SerializedPropertyEnumerator(property))
                    {
                        position.height = EditorGUI.GetPropertyHeight(iter, iter.hasVisibleChildren);
                        EditorGUI.PropertyField(position, iter, iter.hasVisibleChildren);
                        position.y += position.height + EditorExtendUtility.StandardVerticalSpacing;
                    }
                }

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}