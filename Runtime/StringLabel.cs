using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // Implement this interface to allow editor to load label names from asset.
    // Asset must be loadable by AssetDatabase when referencing scriptable objects.
    public interface IStringLabelNameProvider
    {
        string GetStringLabelName(int index);
        int StringLabelCount { get; }
    }

    // Unique string label.
    [Serializable]
    public struct StringLabel
    {
        [field: SerializeField] public string Value { get; private set; }
    }
}