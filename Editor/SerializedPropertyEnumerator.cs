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
                iter.NextVisible(enterChildren);
                enterChildren = false;
                return !SerializedProperty.EqualContents(iter, end);
            }
        }

        private readonly SerializedProperty serializedProperty;

        public SerializedPropertyEnumerator(SerializedProperty serializedProperty)
        {
            this.serializedProperty = serializedProperty;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(serializedProperty);
        }
    }
}
