using UnityEditor;

namespace AggroBird.UnityExtend.Editor
{
    public readonly struct SerializedPropertyEnumerator
    {
        public struct Enumerator
        {
            private readonly SerializedProperty iter;
            private readonly SerializedProperty end;
            private readonly bool recursive;
            private readonly bool onlyVisible;
            private bool enterChildren;

            public Enumerator(SerializedProperty iter, SerializedProperty end, bool recursive, bool onlyVisible)
            {
                this.iter = iter;
                this.end = end;
                this.recursive = recursive;
                this.onlyVisible = onlyVisible;
                enterChildren = true;
            }

            public readonly SerializedProperty Current => iter;

            public bool MoveNext()
            {
                bool result = onlyVisible ? iter.NextVisible(enterChildren) : iter.Next(enterChildren);
                enterChildren = recursive;
                return result && (end == null || !SerializedProperty.EqualContents(iter, end));
            }
        }

        private readonly Enumerator enumerator;

        public SerializedPropertyEnumerator(SerializedProperty serializedProperty, bool recursive = false, bool onlyVisible = true)
        {
            serializedProperty = serializedProperty.Copy();
            enumerator = new Enumerator(serializedProperty, serializedProperty.GetEndProperty(), recursive, onlyVisible);
        }
        public SerializedPropertyEnumerator(SerializedObject serializedObject, bool recursive = false, bool onlyVisible = true)
        {
            enumerator = new Enumerator(serializedObject.GetIterator(), null, recursive, onlyVisible);
        }

        public readonly Enumerator GetEnumerator()
        {
            return enumerator;
        }
    }
}
