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
                    ulong upper = property.FindPropertyRelative((GUID def) => def.Upper).ulongValue;
                    ulong lower = property.FindPropertyRelative((GUID def) => def.Lower).ulongValue;
                    EditorGUI.TextField(position, label, $"{upper:x16}{lower:x16}");
                }
            }
        }
    }
}