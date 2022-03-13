using UnityEngine;
using UnityEditor;
using AggroBird.UnityEngineExtend;

namespace AggroBird.UnityEditorExtend
{
    [CustomPropertyDrawer(typeof(ClampedAttribute))]
    public sealed class ClampedAttributeDrawer : PropertyDrawer
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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ClampedAttribute attr = (ClampedAttribute)attribute;
            minInt = (int)attr.min;
            maxInt = (int)attr.max;
            minFloat = (float)attr.min;
            maxFloat = (float)attr.max;

            bool mixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.PropertyField(position, property, label, true);
            if (!property.hasMultipleDifferentValues)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = Clamp(property.intValue);
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = Clamp(property.floatValue);
                        break;
                    case SerializedPropertyType.Vector2:
                        Vector2 v2 = property.vector2Value;
                        v2.x = Clamp(v2.x);
                        v2.y = Clamp(v2.y);
                        property.vector2Value = v2;
                        break;
                    case SerializedPropertyType.Vector3:
                        Vector3 v3 = property.vector3Value;
                        v3.x = Clamp(v3.x);
                        v3.y = Clamp(v3.y);
                        v3.z = Clamp(v3.z);
                        property.vector3Value = v3;
                        break;
                    case SerializedPropertyType.Vector4:
                        Vector4 v4 = property.vector4Value;
                        v4.x = Clamp(v4.x);
                        v4.y = Clamp(v4.y);
                        v4.z = Clamp(v4.z);
                        v4.w = Clamp(v4.w);
                        property.vector4Value = v4;
                        break;
                    default:
                        EditorGUI.LabelField(position, label.text, "Invalid property type used for Clamped attribute");
                        break;
                }
            }
            EditorGUI.showMixedValue = mixed;
        }
    }
}