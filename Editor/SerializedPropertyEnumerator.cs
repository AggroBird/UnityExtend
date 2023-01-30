using UnityEditor;

namespace AggroBird.UnityEngineExtend.Editor
{
    public struct SerializedPropertyEnumerator
    {
        public struct Enumerator
        {
            private readonly SerializedProperty iter;
            private readonly SerializedProperty end;
            private bool enterChildren;

            public Enumerator(SerializedProperty serializedProperty)
            {
                iter = serializedProperty;
                end = serializedProperty.GetEndProperty();
                enterChildren = true;
            }

            public SerializedProperty Current => iter;

            public bool MoveNext()
            {
                bool result = iter.NextVisible(enterChildren);
                enterChildren = false;
                return result && !SerializedProperty.EqualContents(iter, end);
            }
        }

        private readonly SerializedProperty iter;

        public SerializedPropertyEnumerator(SerializedProperty serializedProperty)
        {
            iter = serializedProperty;
        }
        public SerializedPropertyEnumerator(SerializedObject serializedObject)
        {
            iter = serializedObject.GetIterator();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(iter);
        }
    }
}
