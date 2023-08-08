using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    internal class RangePropertyDrawer : PropertyDrawer
    {
        protected void DrawProperties(Rect position, SerializedProperty min, SerializedProperty max)
        {
            EditorGUIUtility.labelWidth = 26;
            position.width -= 3;
            position.width /= 2;
            EditorGUI.PropertyField(position, min);
            position.x += position.width + 3;
            EditorGUI.PropertyField(position, max);
            EditorGUIUtility.labelWidth = 0;
        }
    }

    [CustomPropertyDrawer(typeof(FloatRange))]
    internal sealed class FloatRangePropertyDrawer : RangePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty min = property.FindPropertyRelative((FloatRange def) => def.Min);
            SerializedProperty max = property.FindPropertyRelative((FloatRange def) => def.Max);

            DrawProperties(position, min, max);

            if (min.floatValue > max.floatValue)
            {
                max.floatValue = min.floatValue;
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(DoubleRange))]
    internal sealed class DoubleRangePropertyDrawer : RangePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty min = property.FindPropertyRelative((DoubleRange def) => def.Min);
            SerializedProperty max = property.FindPropertyRelative((DoubleRange def) => def.Max);

            DrawProperties(position, min, max);

            if (min.doubleValue > max.doubleValue)
            {
                max.doubleValue = min.doubleValue;
            }

            EditorGUI.EndProperty();
        }

        [CustomPropertyDrawer(typeof(IntRange))]
        internal sealed class IntRangePropertyDrawer : RangePropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                position = EditorGUI.PrefixLabel(position, label);

                SerializedProperty min = property.FindPropertyRelative((IntRange def) => def.Min);
                SerializedProperty max = property.FindPropertyRelative((IntRange def) => def.Max);

                DrawProperties(position, min, max);

                if (min.intValue > max.intValue)
                {
                    max.intValue = min.intValue;
                }

                EditorGUI.EndProperty();
            }
        }
    }
}