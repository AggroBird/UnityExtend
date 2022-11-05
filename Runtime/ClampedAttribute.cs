using System;
using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    // Works the same as RangeAttribute, but doesn't display the property with a slider
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ClampedAttribute : PropertyAttribute
    {
        public enum Precision
        {
            Integer,
            Single,
        }

        public readonly Precision precision;
        public readonly float minFloat;
        public readonly float maxFloat;
        public readonly int minInt;
        public readonly int maxInt;

        public ClampedAttribute(float min = 0, float max = float.MaxValue)
        {
            precision = Precision.Single;
            minFloat = min;
            maxFloat = max;
            minInt = (int)min;
            maxInt = (int)max;
        }
        public ClampedAttribute(int min = 0, int max = int.MaxValue)
        {
            precision = Precision.Integer;
            minFloat = min;
            maxFloat = max;
            minInt = min;
            maxInt = max;
        }
    }
}