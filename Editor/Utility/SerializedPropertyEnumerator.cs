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
            private bool enterChildren;

            public Enumerator(SerializedProperty iter, SerializedProperty end, bool recursive)
            {
                this.iter = iter;
                this.end = end;
                this.recursive = recursive;
                enterChildren = true;
            }

            public readonly SerializedProperty Current => iter;

            public bool MoveNext()
            {
                bool result = iter.NextVisible(enterChildren);
                enterChildren = recursive;
                return result && (end == null || !SerializedProperty.EqualContents(iter, end));
            }
        }

        private readonly Enumerator enumerator;

        public SerializedPropertyEnumerator(SerializedProperty serializedProperty, bool recursive = false)
        {
            serializedProperty = serializedProperty.Copy();
            enumerator = new Enumerator(serializedProperty, serializedProperty.GetEndProperty(), recursive);
        }
        public SerializedPropertyEnumerator(SerializedObject serializedObject, bool recursive = false)
        {
            enumerator = new Enumerator(serializedObject.GetIterator(), null, recursive);
        }

        public readonly Enumerator GetEnumerator()
        {
            return enumerator;
        }
    }
}
