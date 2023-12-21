using System;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(ClampedAttribute))]
    internal sealed class ClampedAttributeDrawer : PropertyDrawer
    {
        public readonly struct Context
        {
            public Context(ClampedAttribute attribute)
            {
                // Get values
                minInt = attribute.minInt;
                maxInt = attribute.maxInt;
                minUInt = attribute.minUInt;
                maxUInt = attribute.maxUInt;
                minDouble = attribute.minDouble;
                maxDouble = attribute.maxDouble;

                // Ensure range direction
                if (minInt > maxInt) (maxInt, minInt) = (minInt, maxInt);
                if (minUInt > maxUInt) (maxUInt, minUInt) = (minUInt, maxUInt);
                if (minDouble > maxDouble) (maxDouble, minDouble) = (minDouble, maxDouble);
            }

            private readonly long minInt;
            private readonly long maxInt;
            private readonly ulong minUInt;
            private readonly ulong maxUInt;
            private readonly double minDouble;
            private readonly double maxDouble;

            private int Clamp(int n) => Math.Clamp(n, (int)Math.Clamp(minInt, int.MinValue, int.MaxValue), (int)Math.Clamp(maxInt, int.MinValue, int.MaxValue));
            private uint Clamp(uint n) => Math.Clamp(n, (uint)Math.Clamp(minUInt, uint.MinValue, uint.MaxValue), (uint)Math.Clamp(maxUInt, uint.MinValue, uint.MaxValue));
            private long Clamp(long n) => Math.Clamp(n, minInt, maxInt);
            private ulong Clamp(ulong n) => Math.Clamp(n, minUInt, maxUInt);
            private float Clamp(float n) => (float)Math.Clamp((double)n, minDouble, maxDouble);
            private double Clamp(double n) => Math.Clamp(n, minDouble, maxDouble);
            private void Clamp(SerializedProperty property)
            {
                if (!property.hasMultipleDifferentValues)
                {
                    switch (property.numericType)
                    {
                        case SerializedPropertyNumericType.Int8:
                        case SerializedPropertyNumericType.UInt8:
                        case SerializedPropertyNumericType.Int16:
                        case SerializedPropertyNumericType.UInt16:
                        case SerializedPropertyNumericType.Int32:
                            property.intValue = Clamp(property.intValue);
                            break;
                        case SerializedPropertyNumericType.UInt32:
                            property.uintValue = Clamp(property.uintValue);
                            break;
                        case SerializedPropertyNumericType.Int64:
                            property.longValue = Clamp(property.longValue);
                            break;
                        case SerializedPropertyNumericType.UInt64:
                            property.ulongValue = Clamp(property.ulongValue);
                            break;
                        case SerializedPropertyNumericType.Float:
                            property.floatValue = Clamp(property.floatValue);
                            break;
                        case SerializedPropertyNumericType.Double:
                            property.doubleValue = Clamp(property.doubleValue);
                            break;
                    }
                }
            }
            private void Clamp(SerializedProperty property, string fieldName)
            {
                Clamp(property.FindPropertyRelative(fieldName));
            }

            private const string X = "x";
            private const string Y = "y";
            private const string Z = "z";
            private const string W = "w";

            public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        Clamp(property);
                    }
                    break;
                    case SerializedPropertyType.Vector2Int:
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        Clamp(property, X);
                        Clamp(property, Y);
                    }
                    break;
                    case SerializedPropertyType.Vector3Int:
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        Clamp(property, X);
                        Clamp(property, Y);
                        Clamp(property, Z);
                    }
                    break;

                    case SerializedPropertyType.Float:
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        Clamp(property);
                    }
                    break;
                    case SerializedPropertyType.Vector2:
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        Clamp(property, X);
                        Clamp(property, Y);
                    }
                    break;
                    case SerializedPropertyType.Vector3:
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        Clamp(property, X);
                        Clamp(property, Y);
                        Clamp(property, Z);
                    }
                    break;
                    case SerializedPropertyType.Vector4:
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        Clamp(property, X);
                        Clamp(property, Y);
                        Clamp(property, Z);
                        Clamp(property, W);
                    }
                    break;

                    default:
                        // Special cases
                        if (EditorExtendUtility.TryGetFieldInfo(property, out _, out Type type))
                        {
                            position = EditorGUI.PrefixLabel(position, label);
                            if (type.Equals(typeof(IntRange)) || type.Equals(typeof(FloatRange)) || type.Equals(typeof(DoubleRange)))
                            {
                                RangePropertyDrawer.DrawProperties(position, property);
                                Clamp(property.FindPropertyRelative((IntRange def) => def.Min));
                                Clamp(property.FindPropertyRelative((IntRange def) => def.Max));
                                break;
                            }
                            else if (type.Equals(typeof(Rotator2)))
                            {
                                Rotator2PropertyDrawer.DrawProperties(position, property);
                                Clamp(property.FindPropertyRelative((Rotator2 def) => def.pitch));
                                Clamp(property.FindPropertyRelative((Rotator2 def) => def.yaw));
                                break;
                            }
                            else if (type.Equals(typeof(Rotator3)))
                            {
                                Rotator3PropertyDrawer.DrawProperties(position, property);
                                Clamp(property.FindPropertyRelative((Rotator3 def) => def.pitch));
                                Clamp(property.FindPropertyRelative((Rotator3 def) => def.yaw));
                                Clamp(property.FindPropertyRelative((Rotator3 def) => def.roll));
                                break;
                            }
                        }
                        EditorGUI.LabelField(position, label.text, "Invalid property type used for Clamped attribute");
                        break;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            new Context((ClampedAttribute)attribute).OnGUI(position, property, label);

            EditorGUI.EndProperty();
        }
    }
}