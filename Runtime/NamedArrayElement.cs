using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // Implement this interface to allow editor to load label names from asset.
    // Asset must be loadable by AssetDatabase when referencing scriptable objects.
    public interface INamedArrayElementNameProvider
    {
        INamedArray GetNamedArray(int index);
    }

    public interface INamedArray
    {
        string GetElementName(int index);
        int ElementCount { get; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NamedArrayElementAttribute : PropertyAttribute
    {

    }
}