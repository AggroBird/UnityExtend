using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AggroBird.UnityExtend
{
    public static class Utility
    {
        // Reset transform to identity
        public static void SetIdentity(this Transform transform)
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }

        // Null or empty check for arrays
        public static bool IsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }
        public static bool IsNullOrEmpty<T>(List<T> list)
        {
            return list == null || list.Count == 0;
        }

        // Returns 0 if array is null
        public static int GetLengthSafe<T>(T[] array)
        {
            return array == null ? 0 : array.Length;
        }
        public static int GetLengthSafe<T>(List<T> list)
        {
            return list == null ? 0 : list.Count;
        }

        // Returns Array.Empty if array is null
        public static IReadOnlyList<T> GetReadOnlyListSafe<T>(T[] array)
        {
            return array == null ? Array.Empty<T>() : array;
        }
        public static IReadOnlyList<T> GetReadOnlyListSafe<T>(List<T> list)
        {
            return list == null ? Array.Empty<T>() : list;
        }

        // Check if index is within range
        public static bool IsValidIndex<T>(this T[] array, int idx)
        {
            if (array == null) return false;
            return (uint)idx < (uint)array.Length;
        }
        public static bool IsValidIndex<T>(this List<T> list, int idx)
        {
            if (list == null) return false;
            return (uint)idx < (uint)list.Count;
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

        // Copy from transform
        public static void CopyTransform(this Transform transform, Transform copyFrom, bool copyScale = false)
        {
            transform.SetPositionAndRotation(copyFrom.position, copyFrom.rotation);
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

        public static bool EnsureComponentReference<T>(Component owner, ref T field) where T : Component
        {
            bool result = true;
            if (!field || !ReferenceEquals(field.gameObject, owner.gameObject))
            {
                result = field = owner.GetComponent<T>();
#if UNITY_EDITOR
                if (result)
                {
                    UnityEditor.EditorUtility.SetDirty(owner);
                }
#endif
            }
            return result;
        }

        // Check if scene is active
        public static bool IsSceneActive(string scene)
        {
            if (!string.IsNullOrEmpty(scene))
            {
                Scene activeScene = SceneManager.GetActiveScene();
                return activeScene.IsValid() && (activeScene.path == scene || activeScene.name == scene);
            }
            return false;
        }
    }
}