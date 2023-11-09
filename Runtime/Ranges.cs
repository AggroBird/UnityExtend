using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    [Serializable]
    public struct IntRange
    {
        public IntRange(int min, int max)
        {
            if (min > max)
            {
                Min = max;
                Max = min;
            }
            else
            {
                Min = min;
                Max = max;
            }
        }

        [field: SerializeField, Delayed] public int Min { get; private set; }
        [field: SerializeField, Delayed] public int Max { get; private set; }

        public readonly int Range => Max - Min;

        public readonly bool Contains(int value)
        {
            return value >= Min && value <= Max;
        }

        public readonly double Clamp(float value) => Math.Clamp(value, Min, Max);
        public readonly double Clamp(double value) => Math.Clamp(value, Min, Max);
        public readonly double Clamp(int value) => Math.Clamp(value, Min, Max);

        public override readonly string ToString()
        {
            return $"({Min} - {Max})";
        }
    }

    [Serializable]
    public struct FloatRange
    {
        public FloatRange(float min, float max)
        {
            if (min > max)
            {
                Min = max;
                Max = min;
            }
            else
            {
                Min = min;
                Max = max;
            }
        }

        [field: SerializeField, Delayed] public float Min { get; private set; }
        [field: SerializeField, Delayed] public float Max { get; private set; }

        public readonly float Range => Max - Min;

        public readonly bool Contains(float value)
        {
            return value >= Min && value <= Max;
        }

        public readonly float Clamp(float value) => Math.Clamp(value, Min, Max);

        public override readonly string ToString()
        {
            return $"({Min} - {Max})";
        }
    }

    [Serializable]
    public struct DoubleRange
    {
        public DoubleRange(double min, double max)
        {
            if (min > max)
            {
                Min = max;
                Max = min;
            }
            else
            {
                Min = min;
                Max = max;
            }
        }

        [field: SerializeField, Delayed] public double Min { get; private set; }
        [field: SerializeField, Delayed] public double Max { get; private set; }

        public readonly double Range => Max - Min;

        public readonly bool Contains(double value)
        {
            return value >= Min && value <= Max;
        }

        public readonly double Clamp(float value) => Math.Clamp(value, Min, Max);
        public readonly double Clamp(double value) => Math.Clamp(value, Min, Max);

        public override readonly string ToString()
        {
            return $"({Min} - {Max})";
        }
    }
}