using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PolymorphicFieldAttribute : PropertyAttribute
    {
        public PolymorphicFieldAttribute(bool allowNull = false)
        {
            AllowNull = allowNull;
        }

        public bool AllowNull { get; set; }
        public bool ShowFoldout { get; set; }
    }
}