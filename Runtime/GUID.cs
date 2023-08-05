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
                !long.TryParse(value.Substring(0, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long result0) ||
                !long.TryParse(value.Substring(16, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long result1))
            {
                throw new ArgumentException("Invalid GUID string");
            }

            value0 = result0;
            value1 = result1;
        }
        public GUID(params long[] value)
        {
            if (value == null) throw new NullReferenceException(nameof(value));
            if (value.Length != 2) throw new ArgumentException("Invalid GUID length");
            value0 = value[0];
            value1 = value[1];
        }

        [SerializeField] private long value0;
        [SerializeField] private long value1;

        public readonly long this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return value0;
                    case 1: return value1;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public override readonly int GetHashCode()
        {
            return (value0 ^ (value1 << 2)).GetHashCode();
        }
        public override readonly bool Equals(object obj)
        {
            return obj is GUID other && Equals(other);
        }

        public readonly bool Equals(GUID other)
        {
            return value0 == other.value0 && value1 == other.value1;
        }

        public static bool operator ==(GUID lhs, GUID rhs) => lhs.Equals(rhs);
        public static bool operator !=(GUID lhs, GUID rhs) => !lhs.Equals(rhs);

        public override readonly string ToString()
        {
            return $"{value0:x16}{value1:x16}";
        }
    }
}
