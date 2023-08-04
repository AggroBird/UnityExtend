using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace AggroBird.UnityExtend.Editor
{
    public static class EditorExtendUtility
    {
        public static float IndentWidth => 15f;
        public static float SingleLineHeight = EditorGUIUtility.singleLineHeight;
        public static float StandardVerticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        public static float SinglePropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        public const string MixedValueContent = "\u2014";


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


        private static FieldInfo GetFieldRecursive(this Type type, string fieldName)
        {
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                FieldInfo fieldInfo = currentType.GetField(fieldName, FieldBindingFlags);
                if (fieldInfo != null)
                {
                    return fieldInfo;
                }
                currentType = currentType.BaseType;
            }
            return null;
        }

        public static bool TryGetFieldInfo(this SerializedProperty property, out FieldInfo fieldInfo, out Type fieldType, List<object> values = null, bool useInheritedTypes = true)
        {
            object obj = property.serializedObject.targetObject;
            if (values != null)
            {
                values.Clear();
                values.Add(obj);
            }

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
                        IList list = obj as IList;
                        int idx = int.Parse(indexStr);
                        if (idx < 0 || idx >= list.Count) goto OnFailure;
                        obj = list[idx];
                        fieldType = obj == null ? GetElementType(fieldType) : obj.GetType();

                        if (values != null)
                        {
                            values.Add(obj);
                        }
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
                        fieldInfo = fieldType.GetFieldRecursive(fieldName);
                        if (fieldInfo == null) goto OnFailure;
                        fieldType = fieldInfo.FieldType;
                    }
                    else
                    {
                        fieldInfo = obj.GetType().GetFieldRecursive(fieldName);
                        if (fieldInfo == null) goto OnFailure;
                        obj = fieldInfo.GetValue(obj);
                        fieldType = obj == null ? fieldInfo.FieldType : obj.GetType();

                        if (values != null)
                        {
                            values.Add(obj);
                        }
                    }
                }
            }
            return true;

        OnFailure:
            fieldType = null;
            fieldInfo = null;
            return false;
        }

        private static readonly StringBuilder tagBuilder = new();
        public static void FormatTag(SerializedProperty tag, int maxLength = 32)
        {
            if (!tag.hasMultipleDifferentValues)
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


        private static bool TryGetPropertyNameFromExpression(Expression<Func<object>> exp, out string result)
        {
            if (exp.Body is MemberExpression member)
            {
                result = member.Member.Name;
                return true;
            }
            else if (exp.Body is UnaryExpression unary)
            {
                if (unary.Operand is MemberExpression unaryMember)
                {
                    result = unaryMember.Member.Name;
                    return true;
                }
            }
            result = null;
            return false;
        }
        public static SerializedProperty FindProperty(this SerializedObject serializedObject, Expression<Func<object>> exp)
        {
            return TryGetPropertyNameFromExpression(exp, out string name) ? serializedObject.FindProperty(name) : null;
        }
        public static SerializedProperty FindPropertyRelative(this SerializedProperty serializedProperty, Expression<Func<object>> exp)
        {
            return TryGetPropertyNameFromExpression(exp, out string name) ? serializedProperty.FindPropertyRelative(name) : null;
        }
    }
}