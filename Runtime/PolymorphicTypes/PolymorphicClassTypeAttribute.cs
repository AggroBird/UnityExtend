using System;

namespace AggroBird.UnityExtend
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PolymorphicClassTypeAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
        public int Order { get; set; }
    }
}