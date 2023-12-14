using System;
using System.Globalization;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    [Serializable]
    public struct GUID : IEquatable<GUID>
    {
        public static readonly GUID zero = new GUID();

        public GUID(string value)
        {
            if (value == null) throw new NullReferenceException(nameof(value));
            if (value.Length != 32 ||
                !ulong.TryParse(value.Substring(0, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong upper) ||
                !ulong.TryParse(value.Substring(16, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong lower))
            {
                throw new ArgumentException("Invalid GUID string");
            }

            Upper = upper;
            Lower = lower;
        }
        public GUID(ulong upper, ulong lower)
        {
            Upper = upper;
            Lower = lower;
        }

        [field: SerializeField, UnityEngine.Serialization.FormerlySerializedAs("value0")] public ulong Upper { get; private set; }
        [field: SerializeField, UnityEngine.Serialization.FormerlySerializedAs("value1")] public ulong Lower { get; private set; }

        public override readonly int GetHashCode()
        {
            return (Upper ^ (Lower << 2)).GetHashCode();
        }
        public override readonly bool Equals(object obj)
        {
            return obj is GUID other && Equals(other);
        }

        public readonly bool Equals(GUID other)
        {
            return Upper == other.Upper && Lower == other.Lower;
        }

        public static bool operator ==(GUID lhs, GUID rhs) => lhs.Equals(rhs);
        public static bool operator !=(GUID lhs, GUID rhs) => !lhs.Equals(rhs);

        public override readonly string ToString()
        {
            return $"{Upper:x16}{Lower:x16}";
        }
    }
}