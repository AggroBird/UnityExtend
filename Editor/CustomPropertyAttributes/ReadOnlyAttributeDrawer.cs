using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal sealed class ReadOnlyAttributeDrawer : OverridePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnGUI(position, property, label);
            }
        }
    }
}