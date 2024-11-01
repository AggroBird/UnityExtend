using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // Implement this to apply a filter on supported types for polymorphic fields
    public interface IPolymorphicTypeFilter
    {
        bool IncludeType(Type type);
    }

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
        // Optional type of an object that implements IPolymorphicTypeFilter for filtering types
        // Will get instantiated by the property drawer so needs to have a default constructor
        public Type FilterType { get; set; }
    }
}