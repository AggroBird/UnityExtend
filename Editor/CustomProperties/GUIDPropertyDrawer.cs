using System;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(GUID))]
    internal class GUIDPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            using (new EditorExtendUtility.MixedValueScope(property.hasMultipleDifferentValues))
            {
                var upperProperty = property.FindPropertyRelative(nameof(GUID.upper));
                var lowerProperty = property.FindPropertyRelative(nameof(GUID.lower));
                string currentValue = $"{upperProperty.ulongValue:x16}{lowerProperty.ulongValue:x16}";
                string newValue = EditorGUI.DelayedTextField(position, label, currentValue);
                if (newValue != currentValue)
                {
                    try
                    {
                        GUID newGUID = new(newValue);
                        upperProperty.ulongValue = newGUID.upper;
                        lowerProperty.ulongValue = newGUID.lower;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }
}