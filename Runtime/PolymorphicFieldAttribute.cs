using System;
using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PolymorphicClassTypeAttribute : Attribute
    {
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