using System;
using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    [Serializable]
    public struct GUID : IEquatable<GUID>
    {
        public static readonly GUID zero = new GUID();

        public GUID(string value)
        {
            if (value == null || value.Length != 32)
            {
                throw new ArgumentException("Invalid GUID");
            }

            value0 = Convert.ToInt64(value.Substring(0, 16), 16);
            value1 = Convert.ToInt64(value.Substring(16, 16), 16);
        }

        [SerializeField] private long value0;
        [SerializeField] private long value1;

        public override int GetHashCode()
        {
            return (value0 ^ value1).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is GUID other && Equals(other);
        }

        public bool Equals(GUID other)
        {
            return value0 == other.value0 && value1 == other.value1;
        }

        public static bool operator ==(GUID lhs, GUID rhs) => lhs.Equals(rhs);
        public static bool operator !=(GUID lhs, GUID rhs) => !lhs.Equals(rhs);

        public override string ToString()
        {
            return $"{value0:x16}{value1:x16}";
        }
    }
}