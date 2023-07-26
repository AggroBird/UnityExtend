using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(GUID))]
    internal class GUIDPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                using (new EditorExtendUtility.MixedValueScope(property.hasMultipleDifferentValues))
                {
                    long value0 = property.FindPropertyRelative("value0").longValue;
                    long value1 = property.FindPropertyRelative("value1").longValue;
                    EditorGUI.TextField(position, label, $"{value0:x16}{value1:x16}");
                }
            }
        }
    }
}