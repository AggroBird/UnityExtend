using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // Works the same as RangeAttribute, but doesn't display the property with a slider
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ClampedAttribute : PropertyAttribute
    {
        public readonly long minInt;
        public readonly long maxInt;
        public readonly ulong minUInt;
        public readonly ulong maxUInt;
        public readonly double minDouble;
        public readonly double maxDouble;

        public ClampedAttribute(int min = 0, int max = int.MaxValue)
        {
            minInt = min;
            maxInt = max;
            minUInt = (uint)Math.Max(min, 0);
            maxUInt = (uint)Math.Max(max, 0);
            minDouble = min;
            maxDouble = max;
        }
        public ClampedAttribute(uint min = 0, uint max = uint.MaxValue)
        {
            minInt = min;
            maxInt = max;
            minUInt = min;
            maxUInt = max;
            minDouble = min;
            maxDouble = max;
        }
        public ClampedAttribute(long min = 0, long max = int.MaxValue)
        {
            minInt = min;
            maxInt = max;
            minUInt = (uint)Math.Max(min, 0);
            maxUInt = (uint)Math.Max(max, 0);
            minDouble = min;
            maxDouble = max;
        }
        public ClampedAttribute(ulong min = 0, ulong max = ulong.MaxValue)
        {
            minInt = (long)min;
            maxInt = (long)max;
            minUInt = min;
            maxUInt = max;
            minDouble = min;
            maxDouble = max;
        }
        public ClampedAttribute(float min = 0, float max = float.MaxValue)
        {
            minInt = (int)min;
            maxInt = (int)max;
            minUInt = (uint)Math.Max(min, 0);
            maxUInt = (uint)Math.Max(max, 0);
            minDouble = min;
            maxDouble = max;
        }
        public ClampedAttribute(double min = 0, double max = double.MaxValue)
        {
            minInt = (int)min;
            maxInt = (int)max;
            minUInt = (uint)Math.Max(min, 0);
            maxUInt = (uint)Math.Max(max, 0);
            minDouble = min;
            maxDouble = max;
        }
    }
}