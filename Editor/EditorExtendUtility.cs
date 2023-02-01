using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace AggroBird.UnityEngineExtend.Editor
{
    public static class EditorExtendUtility
    {
        public static float SinglePropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


        public class MixedValueScope : IDisposable
        {
            public MixedValueScope(bool showMixedValue)
            {
                currentValue = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = showMixedValue;
            }

            private bool currentValue;

            public void Dispose()
            {
                EditorGUI.showMixedValue = currentValue;
            }
        }


        private const string ArrayDataStr = "Array.data[";
        private const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static Type GetElementType(Type fieldType)
        {
            if (fieldType.IsArray)
            {
                return fieldType.GetElementType();
            }
            else if (fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldType.GetGenericArguments()[0];
            }
            else
            {
                throw new InvalidCastException($"Failed to extract element type from collection type '{fieldType}'");
            }
        }


        private static void TryGetField(SerializedProperty property, out Type fieldType, out FieldInfo fieldInfo, bool useInheritedTypes)
        {
            object obj = property.serializedObject.targetObject;
            fieldType = obj.GetType();
            fieldInfo = null;
            string path = property.propertyPath;
            bool endReached = false;
            while (!endReached)
            {
                if (path.StartsWith(ArrayDataStr))
                {
                    if (!typeof(IList).IsAssignableFrom(fieldType)) goto OnFailure;
                    int findNext = path.IndexOf(']', ArrayDataStr.Length);
                    if (findNext == -1) goto OnFailure;

                    string indexStr = path.Substring(ArrayDataStr.Length, findNext - ArrayDataStr.Length);

                    findNext = path.IndexOf('.', findNext + 1);
                    if (findNext == -1)
                        endReached = true;
                    else
                        path = path.Substring(findNext + 1);

                    if (obj == null || !useInheritedTypes || endReached)
                    {
                        fieldType = GetElementType(fieldType);
                    }
                    else
                    {
                        obj = (obj as IList)[int.Parse(indexStr)];
                        fieldType = obj == null ? GetElementType(fieldType) : obj.GetType();
                    }
                }
                else
                {
                    int findNext = path.IndexOf('.');

                    string fieldName;
                    if (findNext == -1)
                    {
                        fieldName = path;
                        endReached = true;
                    }
                    else
                    {
                        fieldName = path.Substring(0, findNext);
                        path = path.Substring(findNext + 1);
                    }

                    if (obj == null || !useInheritedTypes || endReached)
                    {
                        fieldInfo = fieldType.GetField(fieldName, FieldBindingFlags);
                        if (fieldInfo == null) goto OnFailure;
                        fieldType = fieldInfo.FieldType;
                    }
                    else
                    {
                        fieldInfo = obj.GetType().GetField(fieldName, FieldBindingFlags);
                        if (fieldInfo == null) goto OnFailure;
                        obj = fieldInfo.GetValue(obj);
                        fieldType = obj == null ? fieldInfo.FieldType : obj.GetType();
                    }
                }
            }
            return;

        OnFailure:
            fieldType = null;
            fieldInfo = null;
            return;
        }

        public static bool TryGetFieldType(this SerializedProperty property, out Type fieldType, bool useInheritedTypes = true)
        {
            TryGetField(property, out fieldType, out _, useInheritedTypes);
            return fieldType != null;
        }

        public static bool TryGetFieldInfo(this SerializedProperty property, out FieldInfo fieldInfo, bool useInheritedTypes = true)
        {
            TryGetField(property, out _, out fieldInfo, useInheritedTypes);
            return fieldInfo != null;
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