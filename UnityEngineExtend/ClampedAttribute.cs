using System;
using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    // Works the same as RangeAttribute, but doesn't display the property with a slider
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ClampedAttribute : PropertyAttribute
    {
        public readonly decimal min;
        public readonly decimal max;

        public ClampedAttribute(float min = 0, float max = float.MaxValue)
        {
            this.min = (decimal)min;
            this.max = (decimal)max;
        }
        public ClampedAttribute(int min = 0, int max = int.MaxValue)
        {
            this.min = min;
            this.max = max;
        }
    }
}