using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    public static class EditorExtendUtility
    {
        // Commonly used editor positioning values
        // Unity's default indent width
        public const float IndentWidth = 15f;
        // Actual property height
        public static float SingleLineHeight = EditorGUIUtility.singleLineHeight;
        // Distance margin between two consecutive properties
        public static float StandardVerticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        // Property height + bottom spacing
        public static float SinglePropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        // Mixed value content character
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
        public static string GetPropertyName(Expression<Func<object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? name : string.Empty;
        }
        public static SerializedProperty FindProperty(this SerializedObject serializedObject, Expression<Func<object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? serializedObject.FindProperty(name) : null;
        }
        public static SerializedProperty FindPropertyRelative(this SerializedProperty serializedProperty, Expression<Func<object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? serializedProperty.FindPropertyRelative(name) : null;
        }
        public static string GetPropertyName<T>(Expression<Func<T, object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? name : string.Empty;
        }
        public static SerializedProperty FindProperty<T>(this SerializedObject serializedObject, Expression<Func<T, object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? serializedObject.FindProperty(name) : null;
        }
        public static SerializedProperty FindPropertyRelative<T>(this SerializedProperty serializedProperty, Expression<Func<T, object>> exp)
        {
            return TryGetPropertyNameFromLambdaExpression(exp, out string name) ? serializedProperty.FindPropertyRelative(name) : null;
        }



        internal class SearchableStringListWindow : EditorWindow
        {
            public static int SelectedValue { get; private set; }

            private const string ControlName = "SearchableStringListWindow.SearchName";

            private Vector2 scrollPosition = Vector2.zero;
            private string filter = string.Empty;
            private string[] filterSplit = Array.Empty<string>();
            private readonly List<string> values = new();
            private bool firstFrame = true;
            private GUIStyle buttonStyle;


            public void SetList(IReadOnlyList<string> list, int selectedValue)
            {
                SelectedValue = selectedValue;

                foreach (var item in list)
                {
                    if (item != null)
                    {
                        string trimmed = item.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            values.Add(trimmed);
                        }
                    }
                }
            }

            private void OnEnable()
            {
                minSize = new Vector2(200, 300);
            }


            private void OnGUI()
            {
                buttonStyle ??= new(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft
                };

                if (firstFrame) GUI.SetNextControlName(ControlName);
                string filterValue = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField);
                if (filterValue != filter)
                {
                    if (string.IsNullOrEmpty(filterValue))
                    {
                        filterSplit = Array.Empty<string>();
                    }
                    else
                    {
                        filterSplit = filterValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    filter = filterValue;
                }

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width));
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        string str = values[i];
                        if (filterSplit.Length > 0)
                        {
                            bool containsFilter = true;
                            foreach (var filter in filterSplit)
                            {
                                if (!str.Contains(filter, StringComparison.OrdinalIgnoreCase))
                                {
                                    containsFilter = false;
                                    break;
                                }
                            }
                            if (!containsFilter)
                            {
                                continue;
                            }
                        }
                        if (GUILayout.Button(str, buttonStyle))
                        {
                            SelectedValue = i;
                            Close();
                            break;
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                if (firstFrame)
                {
                    GUI.FocusControl(ControlName);
                    firstFrame = false;
                }
            }
        }

        public static int SearchableStringList(GUIContent label, int currentSelection, IReadOnlyList<string> list)
        {
            return SearchableStringList(EditorGUILayout.GetControlRect(), label, currentSelection, list);
        }
        public static int SearchableStringList(string label, int currentSelection, IReadOnlyList<string> list)
        {
            return SearchableStringList(EditorGUILayout.GetControlRect(), new GUIContent(label), currentSelection, list);
        }
        public static int SearchableStringList(Rect position, GUIContent label, int currentSelection, IReadOnlyList<string> list)
        {
            position = EditorGUI.PrefixLabel(position, label);
            string currentValue = list == null || (uint)currentSelection >= (uint)list.Count ? string.Empty : list[currentSelection];
            bool pressed = GUI.Button(position, currentValue) && list != null;

            if (pressed)
            {
                SearchableStringListWindow window = ScriptableObject.CreateInstance<SearchableStringListWindow>();
                window.SetList(list, currentSelection);
                window.ShowModal();
                currentSelection = SearchableStringListWindow.SelectedValue;
            }

            return currentSelection;
        }
        public static int SearchableStringList(Rect position, string label, int currentSelection, IReadOnlyList<string> list)
        {
            return SearchableStringList(position, new GUIContent(label), currentSelection, list);
        }
    }
}