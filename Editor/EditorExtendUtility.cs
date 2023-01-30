using System;
using System.Reflection;
using System.Text;
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

        private static readonly StringBuilder tagBuilder = new StringBuilder();
        public static void FormatTag(SerializedProperty tag, int maxLength = 32)
        {
            if (maxLength < 1) maxLength = 1;
            string str = tag.stringValue.Trim().ToUpper();
            if (str.Length > maxLength) str = str.Substring(0, maxLength);

            tagBuilder.Clear();
            bool allowUnderscore = false;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    tagBuilder.Append(c);
                    allowUnderscore = true;
                }
                else if (c == ' ' || c == '_')
                {
                    if (allowUnderscore)
                    {
                        tagBuilder.Append('_');
                        allowUnderscore = false;
                    }
                }
            }

            if (tagBuilder.Length > 0 && tagBuilder[tagBuilder.Length - 1] == '_')
            {
                tagBuilder.Remove(tagBuilder.Length - 1, 1);
            }

            str = tagBuilder.ToString();
            if (str != tag.stringValue)
            {
                tag.stringValue = str;
            }
        }
    }
}