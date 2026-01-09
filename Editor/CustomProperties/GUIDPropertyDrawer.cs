using System;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    public static class GUIDPropertyUtility
    {
        public static GUID GetGUIDValue(this SerializedProperty property)
        {
            if (property == null)
            {
                throw new NullReferenceException(nameof(property));
            }
            if (property.type != typeof(GUID).FullName)
            {
                Debug.LogError($"Property is not a {typeof(GUID).Name}");
                return default;
            }
            ulong upper = property.FindPropertyRelative(nameof(GUID.upper)).ulongValue;
            ulong lower = property.FindPropertyRelative(nameof(GUID.lower)).ulongValue;
            return new(upper, lower);
        }
        public static void SetGUIDValues(this SerializedProperty property, GUID guid)
        {
            if (property == null)
            {
                throw new NullReferenceException(nameof(property));
            }
            if (property.type != typeof(GUID).FullName)
            {
                Debug.LogError($"Property is not a {typeof(GUID).Name}");
                return;
            }
            property.FindPropertyRelative(nameof(GUID.upper)).ulongValue = guid.upper;
            property.FindPropertyRelative(nameof(GUID.lower)).ulongValue = guid.lower;
        }
    }

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