using System;
using UnityEngine;

namespace AggroBird.UnityEngineExtend
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

        public float Min => min;
        public float Max => max;

        public float Range => max - min;

        public bool IsWithin(float value)
        {
            return value >= min && value <= max;
        }

        public override string ToString()
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

        public double Min => min;
        public double Max => max;

        public double Range => max - min;

        public bool IsWithin(double value)
        {
            return value >= min && value <= max;
        }

        public override string ToString()
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

        public int Min => min;
        public int Max => max;

        public int Range => max - min;

        public bool IsWithin(int value)
        {
            return value >= min && value <= max;
        }

        public override string ToString()
        {
            return $"({min} - {max})";
        }
    }
}