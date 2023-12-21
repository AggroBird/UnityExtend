using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal sealed class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}