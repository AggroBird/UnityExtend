using System;
using System.Collections;
using System.Collections.Generic;

namespace AggroBird.UnityExtend
{
    public readonly struct ReadOnlyArray<T> : IReadOnlyList<T>
    {
        public static ReadOnlyArray<T> Empty()
        {
            return new(Array.Empty<T>());
        }


        public ReadOnlyArray(T[] arr)
        {
            this.arr = arr;
        }


        public T this[int index] => arr[index];

        public int Count => arr.Length;

        public IEnumerator<T> GetEnumerator()
        {
            return ((IReadOnlyList<T>)arr).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return arr.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return arr == null ? 0 : arr.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is ReadOnlyArray<T> other && ReferenceEquals(arr, other.arr);
        }


        public static implicit operator ReadOnlyArray<T>(T[] arr) => new(arr);


        private readonly T[] arr;
    }

    public readonly struct ReadOnlyList<T> : IReadOnlyList<T>
    {
        private static readonly List<T> empty = new();

        public static ReadOnlyList<T> Empty()
        {
            return new(empty);
        }


        public ReadOnlyList(List<T> list)
        {
            this.list = list;
        }


        public T this[int index] => list[index];

        public int Count => list.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return list == null ? 0 : list.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is ReadOnlyArray<T> other && ReferenceEquals(list, other.list);
        }


        public static implicit operator ReadOnlyList<T>(List<T> list) => new(list);


        private readonly List<T> list;
    }
}