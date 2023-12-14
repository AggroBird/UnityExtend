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

            this.upper = upper;
            this.lower = lower;
        }
        public GUID(ulong upper, ulong lower)
        {
            this.upper = upper;
            this.lower = lower;
        }

        [SerializeField] private ulong upper;
        [SerializeField] private ulong lower;

        public readonly ulong this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return upper;
                    case 1: return lower;
                }
                throw new IndexOutOfRangeException();
            }
        }

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
