using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    internal class RangePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (HasClampedAttribute(out ClampedAttribute clampedAttribute))
            {
                new ClampedAttributeDrawer.Context(clampedAttribute).OnGUI(position, property, fieldInfo, label);
            }
            else
            {
                position = EditorGUI.PrefixLabel(position, label);
                DrawProperties(position, property);
            }

            EditorGUI.EndProperty();
        }

        private bool HasClampedAttribute(out ClampedAttribute clampedAttribute)
        {
            clampedAttribute = fieldInfo.GetCustomAttribute<ClampedAttribute>();
            return clampedAttribute != null;
        }

        public static void DrawProperties(Rect position, SerializedProperty property)
        {
            float currentLabelWidth = EditorGUIUtility.labelWidth;
            int currentIndent = EditorGUI.indentLevel;
            try
            {
                SerializedProperty min = property.FindPropertyRelative((IntRange def) => def.Min);
                SerializedProperty max = property.FindPropertyRelative((IntRange def) => def.Max);

                // Show property fields
                EditorGUIUtility.labelWidth = 26;
                EditorGUI.indentLevel = 0;

                position.width -= 3;
                position.width /= 2;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, min);
                if (EditorGUI.EndChangeCheck())
                {
                    switch (min.numericType)
                    {
                        case SerializedPropertyNumericType.Int32:
                            if (min.intValue > max.intValue)
                            {
                                max.intValue = min.intValue;
                            }
                            break;
                        case SerializedPropertyNumericType.Float:
                            if (min.floatValue > max.floatValue)
                            {
                                max.floatValue = min.floatValue;
                            }
                            break;
                        case SerializedPropertyNumericType.Double:
                            if (min.doubleValue > max.doubleValue)
                            {
                                max.doubleValue = min.doubleValue;
                            }
                            break;
                        default:
                            Debug.LogWarning($"Unsupported numeric type for range property drawer: {min.numericType}");
                            break;
                    }
                }

                position.x += position.width + 3;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, max);
                if (EditorGUI.EndChangeCheck())
                {
                    switch (min.numericType)
                    {
                        case SerializedPropertyNumericType.Int32:
                            if (min.intValue > max.intValue)
                            {
                                min.intValue = max.intValue;
                            }
                            break;
                        case SerializedPropertyNumericType.Float:
                            if (min.floatValue > max.floatValue)
                            {
                                min.floatValue = max.floatValue;
                            }
                            break;
                        case SerializedPropertyNumericType.Double:
                            if (min.doubleValue > max.doubleValue)
                            {
                                min.doubleValue = max.doubleValue;
                            }
                            break;
                        default:
                            Debug.LogWarning($"Unsupported numeric type for range property drawer: {min.numericType}");
                            break;
                    }
                }
            }
            finally
            {
                EditorGUIUtility.labelWidth = currentLabelWidth;
                EditorGUI.indentLevel = currentIndent;
            }
        }
    }

    [CustomPropertyDrawer(typeof(IntRange))]
    internal sealed class IntRangePropertyDrawer : RangePropertyDrawer
    {

    }

    [CustomPropertyDrawer(typeof(FloatRange))]
    internal sealed class FloatRangePropertyDrawer : RangePropertyDrawer
    {

    }

    [CustomPropertyDrawer(typeof(DoubleRange))]
    internal sealed class DoubleRangePropertyDrawer : RangePropertyDrawer
    {

    }
}