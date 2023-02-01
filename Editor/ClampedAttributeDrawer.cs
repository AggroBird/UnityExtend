using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityEngineExtend.Editor
{
    [CustomPropertyDrawer(typeof(ClampedAttribute))]
    internal sealed class ClampedAttributeDrawer : PropertyDrawer
    {
        private int minInt;
        private int maxInt;
        private float minFloat;
        private float maxFloat;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }


        private float Clamp(float n) => Mathf.Clamp(n, minFloat, maxFloat);
        private int Clamp(int n) => Mathf.Clamp(n, minInt, maxInt);

        private void Clamp(SerializedProperty property)
        {
            if (!property.hasMultipleDifferentValues)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Float:
                        property.floatValue = Clamp(property.floatValue);
                        break;
                    case SerializedPropertyType.Integer:
                        property.intValue = Clamp(property.intValue);
                        break;
                }
            }
        }
        private void Clamp(SerializedProperty property, string name)
        {
            Clamp(property.FindPropertyRelative(name));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            ClampedAttribute attr = (ClampedAttribute)attribute;
            minInt = attr.minInt;
            maxInt = attr.maxInt;
            minFloat = attr.minFloat;
            maxFloat = attr.maxFloat;

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
                    Clamp(property, "x");
                    Clamp(property, "y");
                }
                break;
                case SerializedPropertyType.Vector3Int:
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Clamp(property, "x");
                    Clamp(property, "y");
                    Clamp(property, "z");
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
                    Clamp(property, "x");
                    Clamp(property, "y");
                }
                break;
                case SerializedPropertyType.Vector3:
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Clamp(property, "x");
                    Clamp(property, "y");
                    Clamp(property, "z");
                }
                break;
                case SerializedPropertyType.Vector4:
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Clamp(property, "x");
                    Clamp(property, "y");
                    Clamp(property, "z");
                    Clamp(property, "w");
                }
                break;

                default:
                    EditorGUI.LabelField(position, label.text, "Invalid property type used for Clamped attribute");
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}