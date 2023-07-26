using System;
using System.Collections.Generic;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    internal static class BitfieldUtility
    {
        public const int Precision = 32;

        public static void GetIdxFlag(int index, out int idx, out int flag)
        {
            if (index < 0) throw new IndexOutOfRangeException();
            idx = index / Precision;
            flag = 1 << (index % Precision);
        }
    }


    // Implement this interface to allow editor to load label names from asset.
    // Asset must be loadable by AssetDatabase.
    public interface IBitfieldLabelNameProvider
    {
        IBitfieldLabelList GetBitfieldLabelList();
    }

    // Add this attribute to any field that is a BitfieldFlag or a BitfieldMask.
    // Allows the editor to load the string values defined in a bitfield label list.
    // ProviderType must implement IBitfieldLabelNameProvider interface and must be
    // loadable by the AssetDatabase.
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BitfieldLabelGlobalNameProviderAttribute : Attribute
    {
        public BitfieldLabelGlobalNameProviderAttribute(Type providerType)
        {
            this.ProviderType = providerType;
        }

        public Type ProviderType { get; private set; }
    }

    public enum NestedNameProviderSource
    {
        DeclaringType,
        SerializedObject,
    }

    // Add this attribute to any field that is a BitfieldFlag or a BitfieldMask.
    // Allows the editor to load the string values defined in a bitfield label list.
    // If Source is DeclaringType, the declaring type of the current field will be used.
    // If Source is SerializedObject, the serialized object value of the property will be used.
    // Source must implement IBitfieldLabelNameProvider interface.
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BitfieldLabelNestedNameProviderAttribute : Attribute
    {
        public BitfieldLabelNestedNameProviderAttribute(NestedNameProviderSource source)
        {
            Source = source;
        }

        public NestedNameProviderSource Source { get; private set; }
    }

    // Individual bitfield flag
    [Serializable]
    public struct BitfieldFlag : IEquatable<BitfieldFlag>
    {
        public BitfieldFlag(int flag)
        {
            value = flag;
        }

        [SerializeField] private int value;


        public static implicit operator BitfieldFlag(int flag) => new BitfieldFlag(flag);
        public static implicit operator int(BitfieldFlag flag) => flag.value;

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is BitfieldFlag other && Equals(other);
        }

        public bool Equals(BitfieldFlag other)
        {
            return value == other.value;
        }

        public static bool operator ==(BitfieldFlag lhs, BitfieldFlag rhs) => lhs.Equals(rhs);
        public static bool operator !=(BitfieldFlag lhs, BitfieldFlag rhs) => !lhs.Equals(rhs);

        public override string ToString()
        {
            return value.ToString();
        }
    }


    // Bitfield mask
    // Collection of flags that can perform bitwise operations.
    // Before using bitwise-and on other masks, it is recommended to bitwise-and with the corresponding
    // label list, in case there are any ghost fields that have been removed from the label list still
    // present in the mask.
    public interface IBitfieldMask
    {
        // Precision
        int BitCount { get; }
        // Check if flag is set, or set value
        bool this[BitfieldFlag index] { get; set; }
    }

    [Serializable]
    public struct BitfieldMask32 : IEquatable<BitfieldMask32>, IBitfieldMask
    {
        public static readonly BitfieldMask32 none = new BitfieldMask32();

        [SerializeField] private int mask0;

        private int GetMask(int index)
        {
            switch (index)
            {
                case 0: return mask0;
            }
            throw new IndexOutOfRangeException();
        }
        private void SetMask(int index, int value)
        {
            switch (index)
            {
                case 0: mask0 = value; return;
            }
            throw new IndexOutOfRangeException();
        }

        public int BitCount => 32;
        public bool this[BitfieldFlag index]
        {
            get
            {
                BitfieldUtility.GetIdxFlag(index, out int idx, out int flag);
                return (GetMask(idx) & flag) != 0;
            }
            set
            {
                BitfieldUtility.GetIdxFlag(index, out int idx, out int flag);
                SetMask(idx, value ? GetMask(idx) | flag : GetMask(idx) & ~flag);
            }
        }

        public static BitfieldMask32 operator &(BitfieldMask32 lhs, BitfieldMask32 rhs)
        {
            BitfieldMask32 result = lhs;
            result.mask0 &= rhs.mask0;
            return result;
        }
        public static BitfieldMask32 operator |(BitfieldMask32 lhs, BitfieldMask32 rhs)
        {
            BitfieldMask32 result = lhs;
            result.mask0 |= rhs.mask0;
            return result;
        }
        public static BitfieldMask32 operator ^(BitfieldMask32 lhs, BitfieldMask32 rhs)
        {
            BitfieldMask32 result = lhs;
            result.mask0 ^= rhs.mask0;
            return result;
        }
        public static BitfieldMask32 operator ~(BitfieldMask32 rhs)
        {
            BitfieldMask32 result = rhs;
            result.mask0 = ~result.mask0;
            return result;
        }

        public bool Equals(BitfieldMask32 other)
        {
            return mask0 == other.mask0;
        }

        public override int GetHashCode()
        {
            int result = mask0;
            return result;
        }
        public override bool Equals(object obj)
        {
            return obj is BitfieldMask32 other && Equals(other);
        }

        public static bool operator ==(BitfieldMask32 lhs, BitfieldMask32 rhs) => lhs.Equals(rhs);
        public static bool operator !=(BitfieldMask32 lhs, BitfieldMask32 rhs) => !lhs.Equals(rhs);

        public override string ToString()
        {
            return $"{mask0:X8}";
        }
    }

    [Serializable]
    public struct BitfieldMask64 : IEquatable<BitfieldMask64>, IBitfieldMask
    {
        public static readonly BitfieldMask64 none = new BitfieldMask64();

        [SerializeField] private int mask0;
        [SerializeField] private int mask1;

        private int GetMask(int index)
        {
            switch (index)
            {
                case 0: return mask0;
                case 1: return mask1;
            }
            throw new IndexOutOfRangeException();
        }
        private void SetMask(int index, int value)
        {
            switch (index)
            {
                case 0: mask0 = value; return;
                case 1: mask1 = value; return;
            }
            throw new IndexOutOfRangeException();
        }

        public int BitCount => 64;
        public bool this[BitfieldFlag index]
        {
            get
            {
                BitfieldUtility.GetIdxFlag(index, out int idx, out int flag);
                return (GetMask(idx) & flag) != 0;
            }
            set
            {
                BitfieldUtility.GetIdxFlag(index, out int idx, out int flag);
                SetMask(idx, value ? GetMask(idx) | flag : GetMask(idx) & ~flag);
            }
        }

        public static BitfieldMask64 operator &(BitfieldMask64 lhs, BitfieldMask64 rhs)
        {
            BitfieldMask64 result = lhs;
            result.mask0 &= rhs.mask0;
            result.mask1 &= rhs.mask1;
            return result;
        }
        public static BitfieldMask64 operator |(BitfieldMask64 lhs, BitfieldMask64 rhs)
        {
            BitfieldMask64 result = lhs;
            result.mask0 |= rhs.mask0;
            result.mask1 |= rhs.mask1;
            return result;
        }
        public static BitfieldMask64 operator ^(BitfieldMask64 lhs, BitfieldMask64 rhs)
        {
            BitfieldMask64 result = lhs;
            result.mask0 ^= rhs.mask0;
            result.mask1 ^= rhs.mask1;
            return result;
        }
        public static BitfieldMask64 operator ~(BitfieldMask64 rhs)
        {
            BitfieldMask64 result = rhs;
            result.mask0 = ~result.mask0;
            result.mask1 = ~result.mask1;
            return result;
        }

        public bool Equals(BitfieldMask64 other)
        {
            return mask0 == other.mask0 &&
                mask1 == other.mask1;
        }

        public override int GetHashCode()
        {
            int result = mask0;
            result ^= mask1;
            return result;
        }
        public override bool Equals(object obj)
        {
            return obj is BitfieldMask64 other && Equals(other);
        }

        public static bool operator ==(BitfieldMask64 lhs, BitfieldMask64 rhs) => lhs.Equals(rhs);
        public static bool operator !=(BitfieldMask64 lhs, BitfieldMask64 rhs) => !lhs.Equals(rhs);

        public override string ToString()
        {
            return $"{mask1:X8}{mask0:X8}";
        }
    }

    [Serializable]
    public struct BitfieldMask128 : IEquatable<BitfieldMask128>, IBitfieldMask
    {
        public static readonly BitfieldMask128 none = new BitfieldMask128();

        [SerializeField] private int mask0;
        [SerializeField] private int mask1;
        [SerializeField] private int mask2;
        [SerializeField] private int mask3;

        private int GetMask(int index)
        {
            switch (index)
            {
                case 0: return mask0;
                case 1: return mask1;
                case 2: return mask2;
                case 3: return mask3;
            }
            throw new IndexOutOfRangeException();
        }
        private void SetMask(int index, int value)
        {
            switch (index)
            {
                case 0: mask0 = value; return;
                case 1: mask1 = value; return;
                case 2: mask2 = value; return;
                case 3: mask3 = value; return;
            }
            throw new IndexOutOfRangeException();
        }

        public int BitCount => 128;
        public bool this[BitfieldFlag index]
        {
            get
            {
                BitfieldUtility.GetIdxFlag(index, out int idx, out int flag);
                return (GetMask(idx) & flag) != 0;
            }
            set
            {
                BitfieldUtility.GetIdxFlag(index, out int idx, out int flag);
                SetMask(idx, value ? GetMask(idx) | flag : GetMask(idx) & ~flag);
            }
        }

        public static BitfieldMask128 operator &(BitfieldMask128 lhs, BitfieldMask128 rhs)
        {
            BitfieldMask128 result = lhs;
            result.mask0 &= rhs.mask0;
            result.mask1 &= rhs.mask1;
            result.mask2 &= rhs.mask2;
            result.mask3 &= rhs.mask3;
            return result;
        }
        public static BitfieldMask128 operator |(BitfieldMask128 lhs, BitfieldMask128 rhs)
        {
            BitfieldMask128 result = lhs;
            result.mask0 |= rhs.mask0;
            result.mask1 |= rhs.mask1;
            result.mask2 |= rhs.mask2;
            result.mask3 |= rhs.mask3;
            return result;
        }
        public static BitfieldMask128 operator ^(BitfieldMask128 lhs, BitfieldMask128 rhs)
        {
            BitfieldMask128 result = lhs;
            result.mask0 ^= rhs.mask0;
            result.mask1 ^= rhs.mask1;
            result.mask2 ^= rhs.mask2;
            result.mask3 ^= rhs.mask3;
            return result;
        }
        public static BitfieldMask128 operator ~(BitfieldMask128 rhs)
        {
            BitfieldMask128 result = rhs;
            result.mask0 = ~result.mask0;
            result.mask1 = ~result.mask1;
            result.mask2 = ~result.mask2;
            result.mask3 = ~result.mask3;
            return result;
        }

        public bool Equals(BitfieldMask128 other)
        {
            return mask0 == other.mask0 &&
                mask1 == other.mask1 &&
                mask2 == other.mask2 &&
                mask3 == other.mask3;
        }

        public override int GetHashCode()
        {
            int result = mask0;
            result ^= mask1;
            result ^= mask2;
            result ^= mask3;
            return result;
        }
        public override bool Equals(object obj)
        {
            return obj is BitfieldMask128 other && Equals(other);
        }

        public static bool operator ==(BitfieldMask128 lhs, BitfieldMask128 rhs) => lhs.Equals(rhs);
        public static bool operator !=(BitfieldMask128 lhs, BitfieldMask128 rhs) => !lhs.Equals(rhs);

        public override string ToString()
        {
            return $"{mask3:X8}{mask2:X8}{mask1:X8}{mask0:X8}";
        }
    }

    [Serializable]
    public struct BitfieldMask256 : IEquatable<BitfieldMask256>, IBitfieldMask
    {
        public static readonly BitfieldMask256 none = new BitfieldMask256();

        [SerializeField] private int mask0;
        [SerializeField] private int mask1;
        [SerializeField] private int mask2;
        [SerializeField] private int mask3;
        [SerializeField] private int mask4;
        [SerializeField] private int mask5;
        [SerializeField] private int mask6;
        [SerializeField] private int mask7;

        private int GetMask(int index)
        {
            switch (index)
            {
                case 0: return mask0;
                case 1: return mask1;
                case 2: return mask2;
                case 3: return mask3;
                case 4: return mask4;
                case 5: return mask5;
                case 6: return mask6;
                case 7: return mask7;
            }
            throw new IndexOutOfRangeException();
        }
        private void SetMask(int index, int value)
        {
            switch (index)
            {
                case 0: mask0 = value; return;
                case 1: mask1 = value; return;
                case 2: mask2 = value; return;
                case 3: mask3 = value; return;
                case 4: mask4 = value; return;
                case 5: mask5 = value; return;
                case 6: mask6 = value; return;
                case 7: mask7 = value; return;
            }
            throw new IndexOutOfRangeException();
        }

        public int BitCount => 256;
        public bool this[BitfieldFlag index]
        {
            get
            {
                BitfieldUtility.GetIdxFlag(index, out int idx, out int flag);
                return (GetMask(idx) & flag) != 0;
            }
            set
            {
                BitfieldUtility.GetIdxFlag(index, out int idx, out int flag);
                SetMask(idx, value ? GetMask(idx) | flag : GetMask(idx) & ~flag);
            }
        }

        public static BitfieldMask256 operator &(BitfieldMask256 lhs, BitfieldMask256 rhs)
        {
            BitfieldMask256 result = lhs;
            result.mask0 &= rhs.mask0;
            result.mask1 &= rhs.mask1;
            result.mask2 &= rhs.mask2;
            result.mask3 &= rhs.mask3;
            result.mask4 &= rhs.mask4;
            result.mask5 &= rhs.mask5;
            result.mask6 &= rhs.mask6;
            result.mask7 &= rhs.mask7;
            return result;
        }
        public static BitfieldMask256 operator |(BitfieldMask256 lhs, BitfieldMask256 rhs)
        {
            BitfieldMask256 result = lhs;
            result.mask0 |= rhs.mask0;
            result.mask1 |= rhs.mask1;
            result.mask2 |= rhs.mask2;
            result.mask3 |= rhs.mask3;
            result.mask4 |= rhs.mask4;
            result.mask5 |= rhs.mask5;
            result.mask6 |= rhs.mask6;
            result.mask7 |= rhs.mask7;
            return result;
        }
        public static BitfieldMask256 operator ^(BitfieldMask256 lhs, BitfieldMask256 rhs)
        {
            BitfieldMask256 result = lhs;
            result.mask0 ^= rhs.mask0;
            result.mask1 ^= rhs.mask1;
            result.mask2 ^= rhs.mask2;
            result.mask3 ^= rhs.mask3;
            result.mask4 ^= rhs.mask4;
            result.mask5 ^= rhs.mask5;
            result.mask6 ^= rhs.mask6;
            result.mask7 ^= rhs.mask7;
            return result;
        }
        public static BitfieldMask256 operator ~(BitfieldMask256 rhs)
        {
            BitfieldMask256 result = rhs;
            result.mask0 = ~result.mask0;
            result.mask1 = ~result.mask1;
            result.mask2 = ~result.mask2;
            result.mask3 = ~result.mask3;
            result.mask4 = ~result.mask4;
            result.mask5 = ~result.mask5;
            result.mask6 = ~result.mask6;
            result.mask7 = ~result.mask7;
            return result;
        }

        public bool Equals(BitfieldMask256 other)
        {
            return mask0 == other.mask0 &&
                mask1 == other.mask1 &&
                mask2 == other.mask2 &&
                mask3 == other.mask3 &&
                mask4 == other.mask4 &&
                mask5 == other.mask5 &&
                mask6 == other.mask6 &&
                mask7 == other.mask7;
        }

        public override int GetHashCode()
        {
            int result = mask0;
            result ^= mask1;
            result ^= mask2;
            result ^= mask3;
            result ^= mask4;
            result ^= mask5;
            result ^= mask6;
            result ^= mask7;
            return result;
        }
        public override bool Equals(object obj)
        {
            return obj is BitfieldMask256 other && Equals(other);
        }

        public static bool operator ==(BitfieldMask256 lhs, BitfieldMask256 rhs) => lhs.Equals(rhs);
        public static bool operator !=(BitfieldMask256 lhs, BitfieldMask256 rhs) => !lhs.Equals(rhs);

        public override string ToString()
        {
            return $"{mask7:X8}{mask6:X8}{mask5:X8}{mask4:X8}{mask3:X8}{mask2:X8}{mask1:X8}{mask0:X8}";
        }
    }


    [Serializable]
    public struct BitfieldLabel
    {
        internal BitfieldLabel(string name, int index)
        {
            this.index = index;
            this.name = name;
        }

        [SerializeField] private int index;
        [SerializeField] private string name;

        public int Index => index;
        public string Name => name;

        public override string ToString()
        {
            return Name;
        }
    }

    // Bitfield label list
    // Contains labels defined by user.
    public interface IBitfieldLabelList
    {
        // Precision
        int BitCount { get; }
        // Get user defined labels (ordered by user definition, not index)
        IReadOnlyList<BitfieldLabel> Labels { get; }
        // Check if flag is defined in label list
        bool this[BitfieldFlag index] { get; }
    }

    [Serializable]
    public struct BitfieldLabelList32 : IBitfieldLabelList
    {
#if UNITY_EDITOR
        [SerializeField] private BitfieldLabel[] editorValues;
#endif

        [SerializeField] private BitfieldLabel[] values;
        [SerializeField] private BitfieldMask32 mask;

        public int BitCount => 32;
        public IReadOnlyList<BitfieldLabel> Labels => values;
        public bool this[BitfieldFlag flag] => mask[flag];

        public static BitfieldMask32 operator &(BitfieldMask32 lhs, BitfieldLabelList32 rhs)
        {
            return lhs & rhs.mask;
        }
    }

    [Serializable]
    public struct BitfieldLabelList64 : IBitfieldLabelList
    {
#if UNITY_EDITOR
        [SerializeField] private BitfieldLabel[] editorValues;
#endif

        [SerializeField] private BitfieldLabel[] values;
        [SerializeField] private BitfieldMask64 mask;

        public int BitCount => 64;
        public IReadOnlyList<BitfieldLabel> Labels => values;
        public bool this[BitfieldFlag flag] => mask[flag];

        public static BitfieldMask64 operator &(BitfieldMask64 lhs, BitfieldLabelList64 rhs)
        {
            return lhs & rhs.mask;
        }
    }

    [Serializable]
    public struct BitfieldLabelList128 : IBitfieldLabelList
    {
#if UNITY_EDITOR
        [SerializeField] private BitfieldLabel[] editorValues;
#endif

        [SerializeField] private BitfieldLabel[] values;
        [SerializeField] private BitfieldMask128 mask;

        public int BitCount => 128;
        public IReadOnlyList<BitfieldLabel> Labels => values;
        public bool this[BitfieldFlag flag] => mask[flag];

        public static BitfieldMask128 operator &(BitfieldMask128 lhs, BitfieldLabelList128 rhs)
        {
            return lhs & rhs.mask;
        }
    }

    [Serializable]
    public struct BitfieldLabelList256 : IBitfieldLabelList
    {
#if UNITY_EDITOR
        [SerializeField] private BitfieldLabel[] editorValues;
#endif

        [SerializeField] private BitfieldLabel[] values;
        [SerializeField] private BitfieldMask256 mask;

        public int BitCount => 256;
        public IReadOnlyList<BitfieldLabel> Labels => values;
        public bool this[BitfieldFlag flag] => mask[flag];

        public static BitfieldMask256 operator &(BitfieldMask256 lhs, BitfieldLabelList256 rhs)
        {
            return lhs & rhs.mask;
        }
    }
}
