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
            return new(Array.Empty<T>());
        }


        public ReadOnlyArray(T[] arr)
        {
            this.value = arr;
        }


        public readonly T this[int index] => value[index];

        public readonly int Count => value.Length;

        public readonly IEnumerator<T> GetEnumerator()
        {
            return ((IReadOnlyList<T>)value).GetEnumerator();
        }
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return value.GetEnumerator();
        }

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

        public readonly Span<T> AsSpan() => value.AsSpan();
        public readonly Span<T> AsSpan(int start) => value.AsSpan(start);
        public readonly Span<T> AsSpan(int start, int length) => value.AsSpan(start, length);


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
    }

    [Serializable]
    public struct ReadOnlyList<T> : IReadOnlyList<T>
    {
        private static readonly List<T> empty = new();

        public static ReadOnlyList<T> Empty()
        {
            return new(empty);
        }


        public ReadOnlyList(List<T> list)
        {
            this.value = list;
        }


        public readonly T this[int index] => value[index];

        public readonly int Count => value.Count;

        public readonly IEnumerator<T> GetEnumerator()
        {
            return value.GetEnumerator();
        }
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return value.GetEnumerator();
        }

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
    }
}