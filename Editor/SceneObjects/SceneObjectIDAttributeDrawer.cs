using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(SceneObjectIDAttribute))]
    internal sealed class SceneObjectIDAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                position = EditorGUI.PrefixLabel(position, label);
                if (property.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                    EditorGUI.TextField(position, string.Empty);
                }
                else
                {
                    EditorGUI.TextField(position, property.ulongValue.ToString());
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}