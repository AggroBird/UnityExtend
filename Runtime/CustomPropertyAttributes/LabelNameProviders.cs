using System;

namespace AggroBird.UnityExtend
{
    // Allows the editor to load the string values defined in a label list.
    // ProviderType must implement the proper interface and must be loadable by the AssetDatabase.
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GlobalLabelNameProviderAttribute : Attribute
    {
        public GlobalLabelNameProviderAttribute(Type providerType, int index = 0)
        {
            ProviderType = providerType;
            Index = index;
        }

        public Type ProviderType { get; private set; }
        public int Index { get; private set; }
    }

    public enum NestedNameProviderSource
    {
        DeclaringType,
        SerializedObject,
    }

    // Allows the editor to load the string values defined in a label list.
    // If Source is DeclaringType, the declaring type of the current field will be used.
    // If Source is SerializedObject, the serialized object value of the property will be used.
    // Source must implement the proper interface.
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NestedLabelNameProviderAttribute : Attribute
    {
        public NestedLabelNameProviderAttribute(NestedNameProviderSource source, int index = 0)
        {
            Source = source;
            Index = index;
        }

        public NestedNameProviderSource Source { get; private set; }
        public int Index { get; private set; }
    }
}