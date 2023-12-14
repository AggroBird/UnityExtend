using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    public static class EditorExtendUtility
    {
        // Commonly used editor positioning values
        public const float IndentWidth = 15f;
        public static float SingleLineHeight = EditorGUIUtility.singleLineHeight;
        public static float StandardVerticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        public static float SinglePropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        public const string MixedValueContent = "\u2014";


        // Missing object to show in object fields
        // If SerializedProperty.objectReferenceInstanceIDValue is not 0, but objectReferenceValue is null,
        // it means the object link is broken or the object has been destroyed.
        // Use this object instead of the original value to make the ObjectField show "Missing" in inspector.
        private static ScriptableObject missingObject;
        public static UnityObject MissingObject
        {
            get
            {
                if (missingObject is null)
                {
                    missingObject = ScriptableObject.CreateInstance<ScriptableObject>();
                    UnityObject.DestroyImmediate(missingObject);
                }
                return missingObject;
            }
        }
        public static UnityObject GetObjectReferenceValueOrMissing(this SerializedProperty property)
        {
            return property.objectReferenceValue ? property.objectReferenceValue : property.objectReferenceInstanceIDValue != 0 ? MissingObject : null;
        }


        // Mixed value disposable
        public sealed class MixedValueScope : IDisposable
        {
            public MixedValueScope(bool showMixedValue)
            {
                currentValue = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = showMixedValue;
            }

            private readonly bool currentValue;

            public void Dispose()
            {
                EditorGUI.showMixedValue = currentValue;
            }
        }


        // Utility for getting the field info of a given property (based on property path)
        // Additionally outputs the actual type (in case of serialized references) and a stack of all values if provided
        public static bool TryGetFieldInfo(this SerializedProperty property, out FieldInfo fieldInfo, out Type fieldType, List<object> stackTrace = null, bool useInheritedTypes = true)
        {
            const string ArrayDataStr = "Array.data[";
            const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            static FieldInfo GetFieldRecursive(Type type, string fieldName)
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

            static Type GetElementType(Type fieldType)
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

            object obj = property.serializedObject.targetObject;
            if (stackTrace != null)
            {
                stackTrace.Clear();
                stackTrace.Add(obj);
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

                        if (stackTrace != null)
                        {
                            stackTrace.Add(obj);
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
                        fieldInfo = GetFieldRecursive(fieldType, fieldName);
                        if (fieldInfo == null) goto OnFailure;
                        fieldType = fieldInfo.FieldType;
                    }
                    else
                    {
                        fieldInfo = GetFieldRecursive(obj.GetType(), fieldName);
                        if (fieldInfo == null) goto OnFailure;
                        obj = fieldInfo.GetValue(obj);
                        fieldType = obj == null ? fieldInfo.FieldType : obj.GetType();

                        if (stackTrace != null)
                        {
                            stackTrace.Add(obj);
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


        // Format a tag according to tag formatting rules (e.g. 'Test Tag' => 'TEST_TAG')
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


        // Extension methods that help getting serialized field names through lambda analyzation.
        // example: serializedObject.FindProperty((Foo foo) => foo.bar)
        private static bool TryGetPropertyNameFromLambdaExpression(LambdaExpression exp, out string result)
        {
            static bool ExtractMemberName(MemberInfo memberInfo, out string result)
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        result = memberInfo.Name;
                        return true;

                    case MemberTypes.Property:
                        result = Utility.GetPropertyBackingFieldName(memberInfo.Name);
                        return true;

                    default:
                        result = null;
                        return false;
                }
            }

            if (exp.Body is MemberExpression member)
            {
                return ExtractMemberName(member.Member, out result);
            }
            else if (exp.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            {
                return ExtractMemberName(unaryMember.Member, out result);
            }

            result = null;
            return false;
        }
        public static SerializedProperty FindProperty(this SerializedObject serializedObject, Expression<Func<object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? serializedObject.FindProperty(name) : null;
        }
        public static SerializedProperty FindPropertyRelative(this SerializedProperty serializedProperty, Expression<Func<object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? serializedProperty.FindPropertyRelative(name) : null;
        }
        public static SerializedProperty FindProperty<T>(this SerializedObject serializedObject, Expression<Func<T, object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? serializedObject.FindProperty(name) : null;
        }
        public static SerializedProperty FindPropertyRelative<T>(this SerializedProperty serializedProperty, Expression<Func<T, object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? serializedProperty.FindPropertyRelative(name) : null;
        }
    }
}