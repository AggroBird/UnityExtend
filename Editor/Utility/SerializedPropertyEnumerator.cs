using UnityEditor;

namespace AggroBird.UnityExtend.Editor
{
    public struct SerializedPropertyEnumerator
    {
        public struct Enumerator
        {
            private readonly SerializedProperty iter;
            private readonly SerializedProperty end;
            private bool enterChildren;

            public Enumerator(SerializedProperty iter, SerializedProperty end)
            {
                this.iter = iter;
                this.end = end;
                enterChildren = true;
            }

            public SerializedProperty Current => iter;

            public bool MoveNext()
            {
                bool result = iter.NextVisible(enterChildren);
                enterChildren = false;
                return result && (end == null || !SerializedProperty.EqualContents(iter, end));
            }
        }

        private readonly Enumerator enumerator;

        public SerializedPropertyEnumerator(SerializedProperty serializedProperty)
        {
            serializedProperty = serializedProperty.Copy();
            enumerator = new Enumerator(serializedProperty, serializedProperty.GetEndProperty());
        }
        public SerializedPropertyEnumerator(SerializedObject serializedObject)
        {
            enumerator = new Enumerator(serializedObject.GetIterator(), null);
        }

        public Enumerator GetEnumerator()
        {
            return enumerator;
        }
    }
}
