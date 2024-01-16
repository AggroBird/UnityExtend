using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // Readonly wrappers for list/array properties that are not meant to be modified.
    // For when Span is not applicable, and prevents casting back to collection type like interfaces such as IReadOnlyList.

    [Serializable]
    public struct ReadOnlyArray<T> : IReadOnlyList<T>
    {
        public static ReadOnlyArray<T> Empty()
        {
            return new();
        }


        public ReadOnlyArray(T[] arr)
        {
            value = arr;
        }


        public readonly T this[int index] => value[index];

        public readonly int Count => Utility.GetLengthSafe(value);

        public readonly IEnumerator<T> GetEnumerator() => ((IReadOnlyList<T>)ValueSafe).GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => ValueSafe.GetEnumerator();

        public override readonly int GetHashCode()
        {
            return value == null ? 0 : value.GetHashCode();
        }
        public override readonly bool Equals(object obj)
        {
            return obj is ReadOnlyArray<T> other && ReferenceEquals(value, other.value);
        }

        public readonly T[] ToArray()
        {
            if (value == null || value.Length == 0)
            {
                return Array.Empty<T>();
            }

            T[] result = new T[value.Length];
            Array.Copy(value, result, value.Length);
            return result;
        }

        public readonly Span<T> AsSpan() => ValueSafe.AsSpan();
        public readonly Span<T> AsSpan(int start) => ValueSafe.AsSpan(start);
        public readonly Span<T> AsSpan(int start, int length) => ValueSafe.AsSpan(start, length);


        public static implicit operator ReadOnlyArray<T>(T[] arr) => new(arr);
        public static implicit operator Span<T>(ReadOnlyArray<T> arr) => arr.AsSpan();

        public static bool operator ==(ReadOnlyArray<T> lhs, ReadOnlyArray<T> rhs)
        {
            return ReferenceEquals(lhs.value, rhs.value);
        }
        public static bool operator !=(ReadOnlyArray<T> lhs, ReadOnlyArray<T> rhs)
        {
            return !ReferenceEquals(lhs.value, rhs.value);
        }


        [SerializeField] private T[] value;
        private readonly T[] ValueSafe => value ?? Array.Empty<T>();
    }

    [Serializable]
    public struct ReadOnlyList<T> : IReadOnlyList<T>
    {
        private static readonly List<T> empty = new();

        public static ReadOnlyList<T> Empty()
        {
            return new(null);
        }


        public ReadOnlyList(List<T> list)
        {
            value = list;
        }


        public readonly T this[int index] => value[index];

        public readonly int Count => Utility.GetLengthSafe(value);

        public readonly IEnumerator<T> GetEnumerator() => ValueSafe.GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => ValueSafe.GetEnumerator();

        public override readonly int GetHashCode()
        {
            return value == null ? 0 : value.GetHashCode();
        }
        public override readonly bool Equals(object obj)
        {
            return obj is ReadOnlyList<T> other && ReferenceEquals(value, other.value);
        }

        public readonly T[] ToArray()
        {
            if (value == null)
            {
                return Array.Empty<T>();
            }

            return value.ToArray();
        }


        public static implicit operator ReadOnlyList<T>(List<T> list) => new(list);

        public static bool operator ==(ReadOnlyList<T> lhs, ReadOnlyList<T> rhs)
        {
            return ReferenceEquals(lhs.value, rhs.value);
        }
        public static bool operator !=(ReadOnlyList<T> lhs, ReadOnlyList<T> rhs)
        {
            return !ReferenceEquals(lhs.value, rhs.value);
        }


        [SerializeField] private List<T> value;
        private readonly List<T> ValueSafe => value ?? empty;
    }
}
