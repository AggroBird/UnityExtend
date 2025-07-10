using System;
using System.Globalization;

namespace AggroBird.UnityExtend
{
    [Serializable]
    public struct GUID : IEquatable<GUID>
    {
        public static readonly GUID zero = new();

        public GUID(string value)
        {
            if (!TryParse(value, out ulong upper, out ulong lower))
            {
                throw new ArgumentException("Invalid GUID string");
            }

            this.upper = upper;
            this.lower = lower;
        }
        public GUID(ulong upper, ulong lower)
        {
            this.upper = upper;
            this.lower = lower;
        }

        private static bool TryParse(string str, out ulong upper, out ulong lower)
        {
            if (str != null && str.Length == 32)
            {
                if (ulong.TryParse(str.AsSpan(0, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out upper))
                {
                    if (ulong.TryParse(str.AsSpan(16, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out lower))
                    {
                        return true;
                    }
                }
            }
            upper = lower = default;
            return false;
        }
        public static bool TryParse(string str, out GUID guid)
        {
            if (TryParse(str, out ulong upper, out ulong lower))
            {
                guid = new(upper, lower);
                return true;
            }
            guid = default;
            return false;
        }

        [UnityEngine.Serialization.FormerlySerializedAs("<Upper>k__BackingField")]
        public ulong upper;
        [UnityEngine.Serialization.FormerlySerializedAs("<Lower>k__BackingField")]
        public ulong lower;

        public override readonly int GetHashCode()
        {
            return (upper ^ (lower << 2)).GetHashCode();
        }
        public override readonly bool Equals(object obj)
        {
            return obj is GUID other && Equals(other);
        }

        public readonly bool Equals(GUID other)
        {
            return upper == other.upper && lower == other.lower;
        }

        public static bool operator ==(GUID lhs, GUID rhs) => lhs.Equals(rhs);
        public static bool operator !=(GUID lhs, GUID rhs) => !lhs.Equals(rhs);

        public override readonly string ToString()
        {
            return $"{upper:x16}{lower:x16}";
        }
    }
}