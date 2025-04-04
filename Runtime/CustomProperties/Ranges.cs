using System;
using System.Globalization;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // Add this attribute to a range property in combination with Clamped to draw it as a slider
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RangeSliderAttribute : Attribute
    {

    }

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

        public readonly float Clamp(float value) => (value <= Min) ? Min : (value >= Max) ? Max : value;
        public readonly double Clamp(double value) => (value <= Min) ? Min : (value >= Max) ? Max : value;
        public readonly int Clamp(int value) => (value <= Min) ? Min : (value >= Max) ? Max : value;


        public override readonly int GetHashCode()
        {
            return Min.GetHashCode() ^ (Max.GetHashCode() << 2);
        }
        public override readonly bool Equals(object obj)
        {
            return obj is IntRange other && Equals(other);
        }
        public readonly bool Equals(IntRange other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        public readonly override string ToString()
        {
            return ToString(null, null);
        }
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(format)) format = "F2";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            return $"({Min.ToString(format, formatProvider)} - {Max.ToString(format, formatProvider)})";
        }

        public static bool operator ==(IntRange lhs, IntRange rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(IntRange lhs, IntRange rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static implicit operator Vector2Int(IntRange value)
        {
            return new(value.Min, value.Max);
        }
        public static implicit operator FloatRange(IntRange value)
        {
            return new(value.Min, value.Max);
        }
        public static implicit operator DoubleRange(IntRange value)
        {
            return new(value.Min, value.Max);
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

        public readonly float Clamp(float value) => (value <= Min) ? Min : (value >= Max) ? Max : value;
        public readonly float Lerp(float t) => (t <= 0) ? Min : (t >= 1) ? Max : (Min + Range * t);
        public readonly float InverseLerp(float v) => (Min != Max) ? ((Clamp(v) - Min) / Range) : 0;


        public override readonly int GetHashCode()
        {
            return Min.GetHashCode() ^ (Max.GetHashCode() << 2);
        }
        public override readonly bool Equals(object obj)
        {
            return obj is FloatRange other && Equals(other);
        }
        public readonly bool Equals(FloatRange other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        public readonly override string ToString()
        {
            return ToString(null, null);
        }
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(format)) format = "F2";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            return $"({Min.ToString(format, formatProvider)} - {Max.ToString(format, formatProvider)})";
        }

        public static bool operator ==(FloatRange lhs, FloatRange rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(FloatRange lhs, FloatRange rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static implicit operator Vector2(FloatRange value)
        {
            return new(value.Min, value.Max);
        }
        public static implicit operator DoubleRange(FloatRange value)
        {
            return new(value.Min, value.Max);
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

        public readonly double Clamp(float value) => (value <= Min) ? Min : (value >= Max) ? Max : value;
        public readonly double Clamp(double value) => (value <= Min) ? Min : (value >= Max) ? Max : value;
        public readonly double Lerp(float t) => (t <= 0) ? Min : (t >= 1) ? Max : (Min + Range * (double)t);
        public readonly double Lerp(double t) => (t <= 0) ? Min : (t >= 1) ? Max : (Min + Range * t);
        public readonly double InverseLerp(float v) => (Min != Max) ? ((Clamp(v) - Min) / Range) : 0;
        public readonly double InverseLerp(double v) => (Min != Max) ? ((Clamp(v) - Min) / Range) : 0;


        public override readonly int GetHashCode()
        {
            return Min.GetHashCode() ^ (Max.GetHashCode() << 2);
        }
        public override readonly bool Equals(object obj)
        {
            return obj is DoubleRange other && Equals(other);
        }
        public readonly bool Equals(DoubleRange other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        public readonly override string ToString()
        {
            return ToString(null, null);
        }
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(format)) format = "F2";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            return $"({Min.ToString(format, formatProvider)} - {Max.ToString(format, formatProvider)})";
        }

        public static bool operator ==(DoubleRange lhs, DoubleRange rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(DoubleRange lhs, DoubleRange rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}