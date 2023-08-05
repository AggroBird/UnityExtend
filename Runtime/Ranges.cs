using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    [Serializable]
    public struct FloatRange
    {
        public FloatRange(float min, float max)
        {
            if (min > max)
            {
                this.min = max;
                this.max = min;
            }
            else
            {
                this.min = min;
                this.max = max;
            }
        }

        [SerializeField] private float min;
        [SerializeField] private float max;

        public readonly float Min => min;
        public readonly float Max => max;

        public readonly float Range => max - min;

        public readonly bool Contains(float value)
        {
            return value >= min && value <= max;
        }

        public override readonly string ToString()
        {
            return $"({min} - {max})";
        }
    }

    [Serializable]
    public struct DoubleRange
    {
        public DoubleRange(double min, double max)
        {
            if (min > max)
            {
                this.min = max;
                this.max = min;
            }
            else
            {
                this.min = min;
                this.max = max;
            }
        }

        [SerializeField] private double min;
        [SerializeField] private double max;

        public readonly double Min => min;
        public readonly double Max => max;

        public readonly double Range => max - min;

        public readonly bool Contains(double value)
        {
            return value >= min && value <= max;
        }

        public override readonly string ToString()
        {
            return $"({min} - {max})";
        }
    }

    [Serializable]
    public struct IntRange
    {
        public IntRange(int min, int max)
        {
            if (min > max)
            {
                this.min = max;
                this.max = min;
            }
            else
            {
                this.min = min;
                this.max = max;
            }
        }

        [SerializeField] private int min;
        [SerializeField] private int max;

        public readonly int Min => min;
        public readonly int Max => max;

        public readonly int Range => max - min;

        public readonly bool Contains(int value)
        {
            return value >= min && value <= max;
        }

        public override readonly string ToString()
        {
            return $"({min} - {max})";
        }
    }
}