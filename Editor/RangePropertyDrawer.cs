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
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, min);
            if (EditorGUI.EndChangeCheck())
            {
                if (min.floatValue > max.floatValue)
                {
                    max.floatValue = min.floatValue;
                }
            }
            position.x += w + 2;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, max);
            if (EditorGUI.EndChangeCheck())
            {
                if (max.floatValue < min.floatValue)
                {
                    min.floatValue = max.floatValue;
                }
            }
            EditorGUIUtility.labelWidth = 0;

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
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, min);
            if (EditorGUI.EndChangeCheck())
            {
                if (min.doubleValue > max.doubleValue)
                {
                    max.doubleValue = min.doubleValue;
                }
            }
            position.x += w + 2;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, max);
            if (EditorGUI.EndChangeCheck())
            {
                if (max.doubleValue < min.doubleValue)
                {
                    min.doubleValue = max.doubleValue;
                }
            }
            EditorGUIUtility.labelWidth = 0;

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
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, min);
                if (EditorGUI.EndChangeCheck())
                {
                    if (min.intValue > max.intValue)
                    {
                        max.intValue = min.intValue;
                    }
                }
                position.x += w + 2;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, max);
                if (EditorGUI.EndChangeCheck())
                {
                    if (max.intValue < min.intValue)
                    {
                        min.intValue = max.intValue;
                    }
                }
                EditorGUIUtility.labelWidth = 0;

                EditorGUI.EndProperty();
            }
        }
    }
}