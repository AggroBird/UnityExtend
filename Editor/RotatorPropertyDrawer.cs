using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(Rotator2))]
    internal sealed class Rotator2PropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty pitch = property.FindPropertyRelative((Rotator2 def) => def.pitch);
            SerializedProperty yaw = property.FindPropertyRelative((Rotator2 def) => def.yaw);

            EditorGUIUtility.labelWidth = 13;
            position.width -= 3;
            position.width /= 2;
            EditorGUI.PropertyField(position, pitch, new GUIContent(char.ToUpper(pitch.displayName[0]).ToString()));
            position.x += position.width + 3;
            EditorGUI.PropertyField(position, yaw, new GUIContent(char.ToUpper(yaw.displayName[0]).ToString()));
            EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }

    [CustomPropertyDrawer(typeof(Rotator3))]
    internal sealed class Rotator3PropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty pitch = property.FindPropertyRelative((Rotator3 def) => def.pitch);
            SerializedProperty yaw = property.FindPropertyRelative((Rotator3 def) => def.yaw);
            SerializedProperty roll = property.FindPropertyRelative((Rotator3 def) => def.roll);

            EditorGUIUtility.labelWidth = 13;
            position.width -= 6;
            position.width /= 3;
            EditorGUI.PropertyField(position, pitch, new GUIContent(char.ToUpper(pitch.displayName[0]).ToString()));
            position.x += position.width + 3;
            EditorGUI.PropertyField(position, yaw, new GUIContent(char.ToUpper(yaw.displayName[0]).ToString()));
            position.x += position.width + 3;
            EditorGUI.PropertyField(position, roll, new GUIContent(char.ToUpper(roll.displayName[0]).ToString()));
            EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}
