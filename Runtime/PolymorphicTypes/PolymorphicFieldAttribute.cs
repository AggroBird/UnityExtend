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

        // Can this field be nulled
        public bool AllowNull { get; set; }
        // Should this property be expandable in the inspector (overrides the class setting)
        public bool ShowFoldout { get; set; }
    }
}