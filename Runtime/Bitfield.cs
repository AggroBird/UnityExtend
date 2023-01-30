using System;
using System.Collections.Generic;
using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    internal static class BitfieldUtility
    {
        public const int Precision = 32;

        internal static void GetIdxFlag(int index, out int idx, out int flag)
        {
            idx = index / Precision;
            flag = 1 << (index % Precision);
        }
    }

    public sealed class BitfieldLabelNameProviderAttribute : Attribute
    {
        public BitfieldLabelNameProviderAttribute(Type providerType)
        {
            this.providerType = providerType;
        }

        public readonly Type providerType;
    }

    public interface IBitfieldLabelNameProvider
    {
        IBitfieldLabelList GetBitfieldLabelList();
    }


    [Serializable]
    public struct BitfieldLabel
    {
        public int index;
        public string name;
    }

    public interface IBitfieldLabelList
    {
        int BitCount { get; }
        IReadOnlyList<BitfieldLabel> Labels { get; }
        int GetMask(int index);
    }

    [Serializable]
    public struct BitfieldLabelList32 : IBitfieldLabelList
    {
        [SerializeField] private BitfieldLabel[] values;
        [SerializeField] private int mask0;

        public int BitCount => 32;
        public IReadOnlyList<BitfieldLabel> Labels => values;
        public int GetMask(int index)
        {
            switch (index)
            {
                case 0: return mask0;
            }
            throw new IndexOutOfRangeException();
        }
    }

    [Serializable]
    public struct BitfieldLabelList64 : IBitfieldLabelList
    {
        [SerializeField] private BitfieldLabel[] values;
        [SerializeField] private int mask0;
        [SerializeField] private int mask1;

        public int BitCount => 64;
        public IReadOnlyList<BitfieldLabel> Labels => values;
        public int GetMask(int index)
        {
            switch (index)
            {
                case 0: return mask0;
                case 1: return mask1;
            }
            throw new IndexOutOfRangeException();
        }
    }

    [Serializable]
    public struct BitfieldLabelList128 : IBitfieldLabelList
    {
        [SerializeField] private BitfieldLabel[] values;
        [SerializeField] private int mask0;
        [SerializeField] private int mask1;
        [SerializeField] private int mask2;
        [SerializeField] private int mask3;

        public int BitCount => 128;
        public IReadOnlyList<BitfieldLabel> Labels => values;
        public int GetMask(int index)
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
    }

    [Serializable]
    public struct BitfieldLabelList256 : IBitfieldLabelList
    {
        [SerializeField] private BitfieldLabel[] values;
        [SerializeField] private int mask0;
        [SerializeField] private int mask1;
        [SerializeField] private int mask2;
        [SerializeField] private int mask3;
        [SerializeField] private int mask4;
        [SerializeField] private int mask5;
        [SerializeField] private int mask6;
        [SerializeField] private int mask7;

        public int BitCount => 256;
        public IReadOnlyList<BitfieldLabel> Labels => values;
        public int GetMask(int index)
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
    }


    public interface IBitfieldMask
    {
        int BitCount { get; }
        int GetMask(int index);
        void SetMask(int index, int value);
        public bool this[int index] { get; set; }
    }

    [Serializable]
    public struct BitfieldMask32 : IBitfieldMask
    {
        [SerializeField] private int mask0;

        public int BitCount => 32;
        public int GetMask(int index)
        {
            switch (index)
            {
                case 0: return mask0;
            }
            throw new IndexOutOfRangeException();
        }
        public void SetMask(int index, int value)
        {
            switch (index)
            {
                case 0: mask0 = value; return;
            }
            throw new IndexOutOfRangeException();
        }
        public bool this[int index]
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
    }

    [Serializable]
    public struct BitfieldMask64 : IBitfieldMask
    {
        [SerializeField] private int mask0;
        [SerializeField] private int mask1;

        public int BitCount => 64;
        public int GetMask(int index)
        {
            switch (index)
            {
                case 0: return mask0;
                case 1: return mask1;
            }
            throw new IndexOutOfRangeException();
        }
        public void SetMask(int index, int value)
        {
            switch (index)
            {
                case 0: mask0 = value; return;
                case 1: mask1 = value; return;
            }
            throw new IndexOutOfRangeException();
        }
        public bool this[int index]
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
    }

    [Serializable]
    public struct BitfieldMask128 : IBitfieldMask
    {
        [SerializeField] private int mask0;
        [SerializeField] private int mask1;
        [SerializeField] private int mask2;
        [SerializeField] private int mask3;

        public int BitCount => 128;
        public int GetMask(int index)
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
        public void SetMask(int index, int value)
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
        public bool this[int index]
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
    }

    [Serializable]
    public struct BitfieldMask256 : IBitfieldMask
    {
        [SerializeField] private int mask0;
        [SerializeField] private int mask1;
        [SerializeField] private int mask2;
        [SerializeField] private int mask3;
        [SerializeField] private int mask4;
        [SerializeField] private int mask5;
        [SerializeField] private int mask6;
        [SerializeField] private int mask7;

        public int BitCount => 256;
        public int GetMask(int index)
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
        public void SetMask(int index, int value)
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
        public bool this[int index]
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
    }


    [Serializable]
    public struct BitfieldValue
    {
        [SerializeField] private int value;
    }
}