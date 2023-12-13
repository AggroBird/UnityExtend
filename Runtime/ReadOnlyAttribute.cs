using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ReadOnlyAttribute : PropertyAttribute
    {

    }
}