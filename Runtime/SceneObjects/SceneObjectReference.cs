using System;
using System.Globalization;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    public readonly struct SceneObjectReference
    {
        public SceneObjectReference(GUID guid, ulong objectId)
        {
            this.guid = guid;
            this.objectId = objectId;
        }

        // GUID part (prefab GUID or scene GUID)
        public readonly GUID guid;
        // Specific object ID in scene (0 in case of prefab)
        public readonly ulong objectId;

        public static implicit operator bool(SceneObjectReference reference) => reference.guid != GUID.zero;

        public bool Equals(SceneObjectReference other)
        {
            return guid.Equals(other.guid) && objectId.Equals(other.objectId);
        }

        public override bool Equals(object obj)
        {
            return obj is SceneObjectReference other && Equals(other);
        }
        public override int GetHashCode()
        {
            return (guid.GetHashCode() << 2) ^ objectId.GetHashCode();
        }

        public override string ToString()
        {
            return $"{guid.Upper:x16}{guid.Lower:x16}{objectId:x16}";
        }
        public static bool TryParse(string str, out SceneObjectReference reference)
        {
            if (str != null && str.Length == 48)
            {
                if (ulong.TryParse(str.AsSpan(0, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong upper))
                {
                    if (ulong.TryParse(str.AsSpan(16, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong lower))
                    {
                        if (ulong.TryParse(str.AsSpan(32, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong objectId))
                        {
                            reference = new SceneObjectReference(new GUID(upper, lower), objectId);
                            return true;
                        }
                    }
                }
            }
            reference = default;
            return false;
        }
    }

    [Serializable]
    public struct SceneObjectReference<T> where T : SceneObject
    {
        [SerializeField] private GUID guid;
#if UNITY_EDITOR
        [SerializeField] private ulong prefabId;
#endif
        [SerializeField] private ulong objectId;

        public static implicit operator SceneObjectReference(SceneObjectReference<T> reference) => new(reference.guid, reference.objectId);
        public static implicit operator bool(SceneObjectReference<T> reference) => reference.guid != GUID.zero;
    }
}