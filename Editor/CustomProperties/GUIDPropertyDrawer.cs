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
                var upperProperty = property.FindPropertyRelative((GUID def) => def.Upper);
                var lowerProperty = property.FindPropertyRelative((GUID def) => def.Lower);
                string currentValue = $"{upperProperty.ulongValue:x16}{lowerProperty.ulongValue:x16}";
                string newValue = EditorGUI.DelayedTextField(position, label, currentValue);
                if (newValue != currentValue)
                {
                    try
                    {
                        GUID newGUID = new(newValue);
                        upperProperty.ulongValue = newGUID.Upper;
                        lowerProperty.ulongValue = newGUID.Lower;
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