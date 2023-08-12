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

    // Enable/disable properties based on a certain condition.
    // For Equal and NotEqual, object.Equals is used.
    // For the relative comparisons, if the types of property and operand are equal, it will first check IComparable.
    // Otherwise, for certain arithmetic base types (int, uint, long, ulong, float, double) it will perform default C# implicit cast.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public ConditionalFieldAttribute(string fieldName, ConditionalFieldOperator op, object operand)
        {
            this.fieldName = fieldName;
            this.op = op;
            this.operand = operand;
        }

        public readonly string fieldName;
        public readonly ConditionalFieldOperator op;
        public readonly object operand;
    }
}