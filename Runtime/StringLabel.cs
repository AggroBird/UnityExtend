using System;

namespace AggroBird.UnityExtend
{
    // Implement this interface to allow editor to load label names from asset.
    // Asset must be loadable by AssetDatabase when referencing scriptable objects.
    public interface IStringLabelNameProvider
    {
        string GetStringLabelName(int index);
        int StringLabelCount { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StringLabelAttribute : Attribute
    {

    }
}