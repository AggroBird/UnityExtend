using System;
using System.Reflection;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityEngineExtend.Editor
{
    public static class EditorExtendUtility
    {
        public static float SinglePropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


        public static bool TryGetFieldInfo(this SerializedProperty property, out FieldInfo fieldInfo)
        {
            fieldInfo = null;
            UnityObject obj = property.serializedObject.targetObject;
            if (obj)
            {
                Type parentType = obj.GetType();
                foreach (string fieldName in property.propertyPath.Split('.'))
                {
                    fieldInfo = parentType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fieldInfo == null) break;
                    parentType = fieldInfo.FieldType;
                }
                return fieldInfo != null;
            }
            return false;
        }
    }
}