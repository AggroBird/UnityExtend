using System;

namespace AggroBird.UnityExtend
{
    // Additional per-class customizations for polymorphic properties
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PolymorphicClassTypeAttribute : Attribute
    {
        // Custom display name in the class type search field
        public string DisplayName { get; set; }
        // Tooltip on hover class type dropdown
        public string Tooltip { get; set; }
        // Should this class type be expandable in the inspector
        public bool ShowFoldout { get; set; }
    }
}