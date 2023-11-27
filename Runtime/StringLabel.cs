using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // Implement this interface to allow editor to load label names from asset.
    // Asset must be loadable by AssetDatabase when referencing scriptable objects.
    public interface IStringLabelNameProvider
    {
        IStringLabelList GetStringLabelList(int index);
    }

    public interface IStringLabelList
    {
        string GetStringLabelName(int index);
        int StringLabelCount { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StringLabelAttribute : PropertyAttribute
    {

    }
}