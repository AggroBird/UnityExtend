using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FormattedTagAttribute : PropertyAttribute
    {
        public int MaxLength { get; set; } = 32;
    }
}
