using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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
        public static float TotalPropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        // Mixed value content character
        public const string MixedValueContent = "\u2014";

        private static readonly GUIContent tmpText = new();
        // Temporary gui content
        public static GUIContent TempContent(string t)
        {
            tmpText.image = null;
            tmpText.text = t;
            tmpText.tooltip = null;
            return tmpText;
        }

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


        // Clear the console
        private static readonly MethodInfo clearConsole = Assembly.GetAssembly(typeof(SceneView)).GetType("UnityEditor.LogEntries").GetMethod("Clear");
        public static void ClearConsole()
        {
            clearConsole?.Invoke(null, null);
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
        // Get property that contains provided object through reflection (may not respond to modifications)
        public static bool TryGetPropertyContainer(this SerializedProperty property, out object container)
        {
            if (stacktrace == null)
            {
                stacktrace = new();
            }
            else
            {
                stacktrace.Clear();
            }

            if (TryGetFieldInfo(property, out _, out _, stacktrace) && stacktrace.Count > 0)
            {
                container = stacktrace[^1];
                return true;
            }

            container = null;
            return false;
        }
        private static List<object> stacktrace = null;

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


        // Calculate total bounds for a gameobject from attached child renderers (local space of the parent)
        public static Bounds CalculateTotalBounds(GameObject gameObject)
        {
            GameObject copy = new();
            try
            {
                static void CopyComponents(GameObject src, GameObject dst)
                {
                    if (src.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh && src.TryGetComponent(out Renderer renderer))
                    {
                        dst.AddComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
                        dst.AddComponent(renderer.GetType());
                    }
                }

                static void CopyChildren(GameObject copyParent, GameObject child)
                {
                    GameObject copy = new();
                    copy.transform.SetParent(copyParent.transform);
                    copy.transform.SetLocalPositionAndRotation(child.transform.localPosition, child.transform.localRotation);
                    copy.transform.localScale = child.transform.localScale;
                    CopyComponents(child, copy);
                    var transform = child.transform;
                    var childCount = transform.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        CopyChildren(copy, transform.GetChild(i).gameObject);
                    }
                }

                Bounds? bounds = null;
                void GatherBounds(GameObject go)
                {
                    if (go.TryGetComponent(out Renderer renderer))
                    {
                        if (bounds == null)
                        {
                            bounds = renderer.bounds;
                        }
                        else
                        {
                            Bounds b = bounds.Value;
                            b.Encapsulate(renderer.bounds);
                            bounds = b;
                        }
                    }
                    var transform = go.transform;
                    int childCount = transform.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        GatherBounds(transform.GetChild(i).gameObject);
                    }
                }

                CopyComponents(gameObject, copy);
                var transform = gameObject.transform;
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    CopyChildren(copy, transform.GetChild(i).gameObject);
                }

                GatherBounds(copy);
                if (bounds.HasValue)
                {
                    return bounds.Value;
                }
                return new Bounds();
            }
            finally
            {
                UnityObject.DestroyImmediate(copy);
            }
        }


        public static bool TryGetTypeFromManagedReferenceTypename(string typename, out Type type)
        {
            if (!string.IsNullOrEmpty(typename))
            {
                var splitFieldTypename = typename.Split(' ');
                if (splitFieldTypename.Length >= 2)
                {
                    var assemblyName = splitFieldTypename[0];
                    var subStringTypeName = splitFieldTypename[1];
                    var assembly = Assembly.Load(assemblyName);
                    type = assembly.GetType(subStringTypeName);
                    return type != null;
                }
            }
            type = null;
            return false;
        }


        // Convert scene view window point to ray (regardless of projection mode)
        public static Ray GUIPointToRay(this SceneView sceneView, Vector2 guiPoint, float startZ = float.NegativeInfinity)
        {
            if (sceneView == null) throw new NullReferenceException(nameof(sceneView));
            Camera camera = sceneView.camera;
            if (!camera) throw new UnityException($"Scene view has no camera");
            if (float.IsNegativeInfinity(startZ))
            {
                startZ = camera.nearClipPlane;
            }
            Rect pixelRect = camera.pixelRect;
            Matrix4x4 cameraToWorldMatrix = camera.cameraToWorldMatrix;
            Vector2 pixelCoordinate = HandleUtility.GUIPointToScreenPixelCoordinate(guiPoint);
            Vector3 converted = camera.projectionMatrix.inverse.MultiplyPoint(new((pixelCoordinate.x - pixelRect.x) * 2f / pixelRect.width - 1f, (pixelCoordinate.y - pixelRect.y) * 2f / pixelRect.height - 1f, 0.95f));
            if (camera.orthographic)
            {
                Vector3 origin = cameraToWorldMatrix.MultiplyPoint(new Vector3(converted.x, converted.y, startZ));
                Vector3 direction = cameraToWorldMatrix.MultiplyVector(new Vector3(0f, 0f, -1f)).normalized;
                return new Ray(origin, direction);
            }
            else
            {
                Vector3 normal = converted.normalized;
                Vector3 direction = cameraToWorldMatrix.MultiplyVector(normal);
                Vector3 root = cameraToWorldMatrix.MultiplyPoint(Vector3.zero);
                Vector3 projected = direction * Mathf.Abs(startZ / normal.z);
                return new Ray(root + projected, direction);
            }
        }


        private static bool IsChildOf(Transform parent, Transform child)
        {
            Transform current = child;
            while (current)
            {
                if (current == parent) return true;
                current = current.parent;
            }
            return false;
        }
        [MenuItem("Tools/Snap to Ground %g", priority = 500)]
        public static void SnapSelectionToGround()
        {
            foreach (var transform in Selection.transforms)
            {
                RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 50);
                float shortestDistance = float.MaxValue;
                Vector3 position = Vector3.zero;
                foreach (var hit in hits)
                {
                    if (!IsChildOf(transform, hit.collider.transform) && !hit.collider.isTrigger)
                    {
                        if (hit.distance < shortestDistance)
                        {
                            shortestDistance = hit.distance;
                            position = hit.point;
                        }
                    }
                }
                if (shortestDistance != float.MaxValue)
                {
                    Undo.RecordObject(transform, "Snap to Ground");
                    transform.position = position;
                    EditorUtility.SetDirty(transform);
                }
            }
        }
        [MenuItem("Tools/Snap to Ground At Cursor %#&g", priority = 501)]
        public static void SnapSelectionToGroundAtCursor()
        {
            //Physics.Raycast
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Vector2 mousePos = Event.current.mousePosition;
                mousePos -= sceneView.rootVisualElement.Q("overlay-window-root").worldBound.position;
                Ray ray = sceneView.GUIPointToRay(mousePos);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                float shortestDistance = float.PositiveInfinity;
                Vector3 position = Vector3.zero;
                foreach (var hit in hits)
                {
                    if (hit.collider.isTrigger)
                    {
                        goto Skip;
                    }

                    foreach (var transform in Selection.transforms)
                    {
                        if (IsChildOf(transform, hit.collider.transform))
                        {
                            goto Skip;
                        }
                    }

                    if (hit.distance < shortestDistance)
                    {
                        shortestDistance = hit.distance;
                        position = hit.point;
                    }

                Skip:
                    continue;
                }
                if (shortestDistance != float.PositiveInfinity)
                {
                    foreach (var transform in Selection.transforms)
                    {
                        Undo.RecordObject(transform, "Snap to Ground At Cursor");
                        transform.position = position;
                        EditorUtility.SetDirty(transform);
                    }
                }
            }
        }
    }

    public static class EditorGUIExtend
    {
        private static readonly int searchableStringListPropertyHash = "SearchableStringList_Property".GetHashCode();

        internal class SearchableStringListWindow : EditorWindow
        {
            public static int? lastSelectedValue = 0;
            private static int lastControlId = -1;

            public static bool GetSelectedValue(int controlId, out int selected)
            {
                if (lastSelectedValue.HasValue && lastControlId == controlId)
                {
                    selected = lastSelectedValue.Value;
                    lastSelectedValue = null;
                    return true;
                }
                selected = 0;
                return false;
            }

            private const string ControlName = "SearchableStringListWindow.SearchName";

            private Vector2 scrollPosition = Vector2.zero;
            private string filter = string.Empty;
            private string[] filterSplit = Array.Empty<string>();
            private readonly List<string> values = new();
            private bool firstFrame = true;


            public void SetList(IReadOnlyList<string> list, int selectedValue, int controlId)
            {
                lastSelectedValue = selectedValue;
                lastControlId = controlId;

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
            private void OnLostFocus()
            {
                Close();
            }


            private void OnGUI()
            {
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
                        var rect = EditorGUILayout.GetControlRect();
                        if (GUI.Button(rect, str, EditorStyles.label))
                        {
                            lastSelectedValue = i;
                            Close();
                            break;
                        }
                        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
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

        public static int SearchableStringList(Rect position, int currentSelection, IReadOnlyList<string> list)
        {
            int controlID = GUIUtility.GetControlID(searchableStringListPropertyHash, FocusType.Keyboard, position);

            string currentValue = list == null || (uint)currentSelection >= (uint)list.Count ? string.Empty : list[currentSelection];
            bool pressed = GUI.Button(position, currentValue, EditorStyles.miniPullDown) && list != null;

            if (pressed)
            {
                GUI.changed = false;
                SearchableStringListWindow window = ScriptableObject.CreateInstance<SearchableStringListWindow>();
                window.SetList(list, currentSelection, controlID);
                window.Show();
            }

            if (SearchableStringListWindow.GetSelectedValue(controlID, out int selected))
            {
                GUI.changed |= selected != currentSelection;
                return selected;
            }

            return currentSelection;
        }
        public static int SearchableStringList(Rect position, GUIContent label, int currentSelection, IReadOnlyList<string> list)
        {
            position = EditorGUI.PrefixLabel(position, label);
            return SearchableStringList(position, currentSelection, list);
        }
        public static int SearchableStringList(Rect position, string label, int currentSelection, IReadOnlyList<string> list)
        {
            position = EditorGUI.PrefixLabel(position, EditorExtendUtility.TempContent(label));
            return SearchableStringList(position, currentSelection, list);
        }
    }

    public static class EditorGUILayoutExtend
    {
        public static int SearchableStringList(int currentSelection, IReadOnlyList<string> list)
        {
            return EditorGUIExtend.SearchableStringList(EditorGUILayout.GetControlRect(), currentSelection, list);
        }
        public static int SearchableStringList(GUIContent label, int currentSelection, IReadOnlyList<string> list)
        {
            return EditorGUIExtend.SearchableStringList(EditorGUILayout.GetControlRect(), label, currentSelection, list);
        }
        public static int SearchableStringList(string label, int currentSelection, IReadOnlyList<string> list)
        {
            return EditorGUIExtend.SearchableStringList(EditorGUILayout.GetControlRect(), label, currentSelection, list);
        }
    }
}