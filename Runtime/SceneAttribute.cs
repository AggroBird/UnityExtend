using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // Draw strings as scene asset reference
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SceneAttribute : PropertyAttribute
    {

    }
}