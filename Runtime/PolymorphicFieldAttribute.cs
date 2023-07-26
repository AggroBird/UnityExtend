using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PolymorphicClassTypeAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
        public int Order { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PolymorphicFieldAttribute : PropertyAttribute
    {
        public PolymorphicFieldAttribute(bool allowNull = false)
        {
            AllowNull = allowNull;
        }

        public bool AllowNull { get; private set; }
    }
}