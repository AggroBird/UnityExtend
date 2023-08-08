using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    public static class ReadOnlyCollectionsUtility
    {
        public static SerializedProperty ReadOnlyCollectionValue(this SerializedProperty property)
        {
            return property.FindPropertyRelative("value");
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyArray<>))]
    internal sealed class ReadOnlyArrayPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.ReadOnlyCollectionValue(), label);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.ReadOnlyCollectionValue());
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyList<>))]
    internal sealed class ReadOnlyListPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.ReadOnlyCollectionValue(), label);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.ReadOnlyCollectionValue());
        }
    }
}