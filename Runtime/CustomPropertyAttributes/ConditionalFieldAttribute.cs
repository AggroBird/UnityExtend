using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    public enum ConditionalFieldOperator
    {
        Equal,
        NotEqual,
        LessThan,
        LessEqual,
        GreaterThan,
        GreaterEqual,
    }

    public enum ConditionalFieldStyle
    {
        Hide,
        Disable,
    }

    // Enable/disable properties based on a certain condition.
    // For Equal and NotEqual, object.Equals is used.
    // For the relative comparisons, if the types of property and operand are equal, it will first check IComparable.
    // Otherwise, for certain arithmetic base types (int, uint, long, ulong, float, double) it will perform default C# implicit cast.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false), ExcludeFromPropertyDrawerTypeCache]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public ConditionalFieldAttribute(string fieldName, ConditionalFieldOperator op, object operand, ConditionalFieldStyle style = ConditionalFieldStyle.Hide)
        {
            this.fieldName = fieldName;
            this.op = op;
            this.operand = operand;
            this.style = style;
        }

        public readonly string fieldName;
        public readonly ConditionalFieldOperator op;
        public readonly object operand;
        public readonly ConditionalFieldStyle style;
    }
}