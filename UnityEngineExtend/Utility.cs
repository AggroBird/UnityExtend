using System;
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

        // Set transform to identity
        public static void SetIdentity(this Transform transform)
        {
            if (transform)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }
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
        public static bool TryGetComponentInParent<T>(this Component component, out T result)
        {
            result = component.GetComponentInParent<T>();
            return result != null;
        }
        public static bool TryGetComponentInParent(this Component component, Type type, out Component result)
        {
            result = component.GetComponentInParent(type);
            return result != null;
        }

        // Try get for children
        public static bool TryGetComponentInChildren<T>(this Component component, out T result, bool includeInactive = false)
        {
            result = component.GetComponentInChildren<T>(includeInactive);
            return result != null;
        }
        public static bool TryGetComponentInChildren(this Component component, Type type, out Component result, bool includeInactive = false)
        {
            result = component.GetComponentInChildren(type, includeInactive);
            return result != null;
        }
    }
}