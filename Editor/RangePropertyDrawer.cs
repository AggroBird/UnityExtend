using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityEngineExtend.Editor
{
    [CustomPropertyDrawer(typeof(FloatRange))]
    internal sealed class FloatRangePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty min = property.FindPropertyRelative("min");
            SerializedProperty max = property.FindPropertyRelative("max");

            EditorGUIUtility.labelWidth = 26;
            position.width /= 2;
            position.width -= 1;
            float w = position.width;
            EditorGUI.PropertyField(position, min);
            position.x += w + 2;
            EditorGUI.PropertyField(position, max);
            EditorGUIUtility.labelWidth = 0;

            if (min.floatValue > max.floatValue)
            {
                max.floatValue = min.floatValue;
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(DoubleRange))]
    internal sealed class DoubleRangePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty min = property.FindPropertyRelative("min");
            SerializedProperty max = property.FindPropertyRelative("max");

            EditorGUIUtility.labelWidth = 26;
            position.width /= 2;
            position.width -= 1;
            float w = position.width;
            EditorGUI.PropertyField(position, min);
            position.x += w + 2;
            EditorGUI.PropertyField(position, max);
            EditorGUIUtility.labelWidth = 0;

            if (min.doubleValue > max.doubleValue)
            {
                max.doubleValue = min.doubleValue;
            }

            EditorGUI.EndProperty();
        }

        [CustomPropertyDrawer(typeof(IntRange))]
        internal sealed class IntRangePropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                position = EditorGUI.PrefixLabel(position, label);

                SerializedProperty min = property.FindPropertyRelative("min");
                SerializedProperty max = property.FindPropertyRelative("max");

                EditorGUIUtility.labelWidth = 26;
                position.width /= 2;
                position.width -= 1;
                float w = position.width;
                EditorGUI.PropertyField(position, min);
                position.x += w + 2;
                EditorGUI.PropertyField(position, max);
                EditorGUIUtility.labelWidth = 0;

                if (min.intValue > max.intValue)
                {
                    max.intValue = min.intValue;
                }

                EditorGUI.EndProperty();
            }
        }
    }
}