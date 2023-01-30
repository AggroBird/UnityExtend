using System;
using System.Collections.Generic;
using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
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
        public IBitfieldLabelList GetBitfieldLabelList();
    }


    [Serializable]
    public struct BitfieldLabel
    {
        public int index;
        public string name;
    }

    public interface IBitfieldLabelList
    {
        public int BitCount { get; }
        public IReadOnlyList<BitfieldLabel> Labels { get; }
        public int GetMask(int index);
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


    public interface IBitfieldLabelMask
    {
        public int BitCount { get; }
        public int GetValue(int index);
    }

    [Serializable]
    public struct BitfieldLabelMask32 : IBitfieldLabelMask
    {
        [SerializeField] private int value0;

        public int BitCount => 32;
        public int GetValue(int index)
        {
            switch (index)
            {
                case 0: return value0;
            }
            throw new IndexOutOfRangeException();
        }
    }

    [Serializable]
    public struct BitfieldLabelMask64 : IBitfieldLabelMask
    {
        [SerializeField] private int value0;
        [SerializeField] private int value1;

        public int BitCount => 64;
        public int GetValue(int index)
        {
            switch (index)
            {
                case 0: return value0;
                case 1: return value1;
            }
            throw new IndexOutOfRangeException();
        }
    }

    [Serializable]
    public struct BitfieldLabelMask128 : IBitfieldLabelMask
    {
        [SerializeField] private int value0;
        [SerializeField] private int value1;
        [SerializeField] private int value2;
        [SerializeField] private int value3;

        public int BitCount => 128;
        public int GetValue(int index)
        {
            switch (index)
            {
                case 0: return value0;
                case 1: return value1;
                case 2: return value2;
                case 3: return value3;
            }
            throw new IndexOutOfRangeException();
        }
    }

    [Serializable]
    public struct BitfieldLabelMask256 : IBitfieldLabelMask
    {
        [SerializeField] private int value0;
        [SerializeField] private int value1;
        [SerializeField] private int value2;
        [SerializeField] private int value3;
        [SerializeField] private int value4;
        [SerializeField] private int value5;
        [SerializeField] private int value6;
        [SerializeField] private int value7;

        public int BitCount => 256;
        public int GetValue(int index)
        {
            switch (index)
            {
                case 0: return value0;
                case 1: return value1;
                case 2: return value2;
                case 3: return value3;
                case 4: return value4;
                case 5: return value5;
                case 6: return value6;
                case 7: return value7;
            }
            throw new IndexOutOfRangeException();
        }
    }
}