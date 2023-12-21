namespace AggroBird.UnityExtend.Editor
{
    internal readonly struct EditorSceneObjectReference
    {
        public EditorSceneObjectReference(GUID guid, ulong prefabId, ulong objectId)
        {
            this.guid = guid;
            this.prefabId = prefabId;
            this.objectId = objectId;
        }

        public readonly GUID guid;
        public readonly ulong prefabId;
        public readonly ulong objectId;

        public bool Equals(EditorSceneObjectReference other)
        {
            return guid.Equals(other.guid) && prefabId.Equals(other.prefabId) && objectId.Equals(other.objectId);
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode() ^ (prefabId.GetHashCode() << 2) ^ (objectId.GetHashCode() >> 2);
        }
        public override bool Equals(object obj)
        {
            return obj is EditorSceneObjectReference other && Equals(other);
        }

        public static implicit operator SceneObjectReference(EditorSceneObjectReference key)
        {
            return new SceneObjectReference(key.guid, key.objectId);
        }
    }
}