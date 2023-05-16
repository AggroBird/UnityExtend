using System;
using System.Collections.Generic;
using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    public static class Utility
    {
        // Null or empty check for arrays
        public static bool IsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }

        // Check if index is within range
        public static bool IsValidIndex<T>(this T[] arr, int idx)
        {
            if (arr == null) return false;
            return idx >= 0 && idx < arr.Length;
        }

        // Remove element and insert the last element at the location, when list order is not important
        public static void RemoveAndSwap<T>(this List<T> list, int idx)
        {
            int last = list.Count - 1;
            if (idx != last)
            {
                list[idx] = list[last];
            }
            list.RemoveAt(last);
        }

        // Set transform to identity
        public static void SetIdentity(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        public static Vector2 PositionXZ(this Transform transform)
        {
            return transform.position.GetXZ();
        }
        public static Vector2 PositionXY(this Transform transform)
        {
            return transform.position.GetXY();
        }

        // Copy from transform
        public static void CopyTransform(this Transform transform, Transform copyFrom, bool copyScale = false)
        {
            transform.position = copyFrom.position;
            transform.rotation = copyFrom.rotation;
            if (copyScale)
            {
                transform.localScale = copyFrom.localScale;
            }
        }

        // Try get for parent
        public static bool TryGetComponentInParent<T>(this Component component, out T result) where T : class
        {
            result = component.GetComponentInParent<T>();
            return !EqualityComparer<T>.Default.Equals(result, default(T));
        }
        public static bool TryGetComponentInParent(this Component component, Type type, out Component result)
        {
            result = component.GetComponentInParent(type);
            return result != null;
        }
        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T result) where T : class
        {
            result = gameObject.GetComponentInParent<T>();
            return !EqualityComparer<T>.Default.Equals(result, default(T));
        }
        public static bool TryGetComponentInParent(this GameObject gameObject, Type type, out Component result)
        {
            result = gameObject.GetComponentInParent(type);
            return result != null;
        }

        // Try get for children
        public static bool TryGetComponentInChildren<T>(this Component component, out T result, bool includeInactive = false) where T : class
        {
            result = component.GetComponentInChildren<T>(includeInactive);
            return !EqualityComparer<T>.Default.Equals(result, default(T));
        }
        public static bool TryGetComponentInChildren(this Component component, Type type, out Component result, bool includeInactive = false)
        {
            result = component.GetComponentInChildren(type, includeInactive);
            return result != null;
        }
        public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T result, bool includeInactive = false) where T : class
        {
            result = gameObject.GetComponentInChildren<T>(includeInactive);
            return !EqualityComparer<T>.Default.Equals(result, default(T));
        }
        public static bool TryGetComponentInChildren(this GameObject gameObject, Type type, out Component result, bool includeInactive = false)
        {
            result = gameObject.GetComponentInChildren(type, includeInactive);
            return result != null;
        }
    }
}