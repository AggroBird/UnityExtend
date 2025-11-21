using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityExtend
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExcludeFromPropertyDrawerTypeCacheAttribute : Attribute
    {

    }

    public static class Utility
    {
        // Get component or add it if it doesnt exist
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
        }

        // Null or empty check for arrays
        public static bool IsNullOrEmpty<T>(IReadOnlyList<T> list)
        {
            return list == null || list.Count == 0;
        }

        // Returns 0 if array is null
        public static int GetLengthSafe<T>(IReadOnlyList<T> list)
        {
            return list == null ? 0 : list.Count;
        }

        // Returns Array.Empty if array is null
        public static IReadOnlyList<T> GetReadOnlyListSafe<T>(IReadOnlyList<T> list)
        {
            return list == null ? Array.Empty<T>() : list;
        }

        // Check if index is within range
        public static bool IsValidIndex<T>(this IReadOnlyList<T> list, int idx)
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

        // Destroy objects
        public static void DestroyObjects<T>(IReadOnlyList<T> list) where T : UnityObject
        {
            foreach (var item in list)
            {
                if (item)
                {
                    UnityObject.Destroy(item);
                }
            }
        }
        public static void DestroyGameObjects<T>(IReadOnlyList<T> list) where T : Component
        {
            foreach (var item in list)
            {
                if (item)
                {
                    UnityObject.Destroy(item.gameObject);
                }
            }
        }
        // Destroy objects and clear the list
        public static void DestroyObjectsAndClear<T>(List<T> list) where T : UnityObject
        {
            DestroyObjects(list);
            list.Clear();
        }
        public static void DestroyGameObjectsAndClear<T>(List<T> list) where T : Component
        {
            DestroyGameObjects(list);
            list.Clear();
        }

        // Compare object lists by content
        public static bool CompareObjectLists<T>(IReadOnlyList<T> lhs, IReadOnlyList<T> rhs) where T : UnityObject
        {
            // Check equal reference
            if (ReferenceEquals(lhs, rhs)) return true;

            // Check null
            if (lhs == null || rhs == null) return lhs == rhs;

            // Check invalid objects (assume false if so)
            foreach (var go in lhs) if (!go) return false;
            foreach (var go in rhs) if (!go) return false;

            // Compare count
            int count = lhs.Count;
            if (count != rhs.Count)
            {
                return false;
            }

            // Sort and compare
            List<UnityObject> sortedLhs = new(lhs);
            List<UnityObject> sortedRhs = new(rhs);
            sortedLhs.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
            sortedRhs.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));

            for (int i = 0; i < count; ++i)
            {
                if (sortedLhs[i] != sortedRhs[i])
                {
                    return false;
                }
            }
            return true;
        }

        // Reset transform to identity
        public static void SetIdentity(this Transform transform)
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
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

        // Get average normal from a collision with multiple contact points
        public static Vector3 GetAverageNormal(this Collision collision)
        {
            Vector3 result = Vector3.zero;
            int contactCount = collision.contactCount;
            if (contactCount > 0)
            {
                for (int i = 0; i < contactCount; i++)
                {
                    result += collision.GetContact(i).normal;
                }
                return result.normalized;
            }
            return result;
        }

        // Reset all animator triggers
        public static void ResetAllTriggers(this Animator animator)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(parameter.nameHash);
                }
            }
        }

        // Try get for parent
        public static bool TryGetComponentInParent<T>(this Component component, out T result) where T : class
        {
            result = component.GetComponentInParent<T>();
            return !EqualityComparer<T>.Default.Equals(result, default);
        }
        public static bool TryGetComponentInParent(this Component component, Type type, out Component result)
        {
            result = component.GetComponentInParent(type);
            return result != null;
        }
        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T result) where T : class
        {
            result = gameObject.GetComponentInParent<T>();
            return !EqualityComparer<T>.Default.Equals(result, default);
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
            return !EqualityComparer<T>.Default.Equals(result, default);
        }
        public static bool TryGetComponentInChildren(this Component component, Type type, out Component result, bool includeInactive = false)
        {
            result = component.GetComponentInChildren(type, includeInactive);
            return result != null;
        }
        public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T result, bool includeInactive = false) where T : class
        {
            result = gameObject.GetComponentInChildren<T>(includeInactive);
            return !EqualityComparer<T>.Default.Equals(result, default);
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

        // Destroy gameobject this component is attached to
        public static void DestroyGameObject(this Component component)
        {
            UnityObject.Destroy(component.gameObject);
        }
        public static void DestroyGameObjectImmediate(this Component component)
        {
            UnityObject.DestroyImmediate(component.gameObject);
        }

        // Destroy object
        public static void Destroy(this UnityObject obj)
        {
            UnityObject.Destroy(obj);
        }
        public static void DestroyImmediate(this UnityObject obj)
        {
            UnityObject.DestroyImmediate(obj);
        }

        // Destroy all children
        public static void DestroyChildren(this Transform transform)
        {
            int i = 0;
            while (i < transform.childCount)
            {
                var child = transform.GetChild(i++);
                if (child)
                {
                    child.DestroyGameObject();
                }
            }
        }

        // Get property compiler-generated backing field name
        public static string GetPropertyBackingFieldName(string propertyName)
        {
            return $"<{propertyName}>k__BackingField";
        }

        // Load assets of type (editor only)
        public static bool TryLoadFirstAssetOfType<T>(out T asset) where T : UnityObject
        {
#if UNITY_EDITOR
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path) is T casted)
                {
                    asset = casted;
                    return true;
                }
            }
#endif
            asset = default;
            return false;
        }
        public static T[] LoadAllAssetsOfType<T>() where T : UnityObject
        {
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length > 0)
            {
                List<T> result = new();
                foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    if (UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path) is T casted)
                    {
                        result.Add(casted);
                    }
                }
                return result.ToArray();
            }
#endif
            return Array.Empty<T>();
        }
        public static bool TryLoadFirstAssetOfType(Type type, out UnityObject asset)
        {
#if UNITY_EDITOR
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:{type.Name}"))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var loadedAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
                if (loadedAsset)
                {
                    if (type.IsAssignableFrom(loadedAsset.GetType()))
                    {
                        asset = loadedAsset;
                        return true;
                    }
                }
            }
#endif
            asset = default;
            return false;
        }
        public static UnityObject[] LoadAllAssetsOfType(Type type)
        {
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{type.Name}");
            if (guids.Length > 0)
            {
                List<UnityObject> result = new();
                foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:{type.Name}"))
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var loadedAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
                    if (loadedAsset)
                    {
                        if (type.IsAssignableFrom(loadedAsset.GetType()))
                        {
                            result.Add(loadedAsset);
                        }
                    }
                }
                return result.ToArray();
            }
#endif
            return Array.Empty<UnityObject>();
        }
    }
}