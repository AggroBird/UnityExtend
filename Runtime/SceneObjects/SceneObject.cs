using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AggroBird.UnityExtend
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class SceneObjectIDAttribute : PropertyAttribute
    {

    }

    [DisallowMultipleComponent]
    public class SceneObject : MonoBehaviour
    {
        // In the case of a regular scene object, this contains the scene GUID
        // In the case of a scene prefab instance, this contains the prefab GUID
        [SerializeField] private GUID guid;
        // In case of an actual prefab, this will be 0
        [SerializeField, SceneObjectID] private ulong objectId;

        internal GUID SceneObjectGUID => guid;
        internal ulong SceneObjectID => objectId;

        // The GUID of the scene this object is in at play time (zero if registration failed)
        private GUID sceneGUID;

        public bool IsReferenced(SceneObjectReference reference)
        {
            if (reference.guid != GUID.zero && guid != GUID.zero)
            {
                if (reference.objectId == 0)
                {
                    // Prefab type comparison
                    return reference.guid == guid;
                }
                else
                {
                    // Specific scene object comparison, ensure its a object within this scene
                    // (Don't use the object's GUID, it might be a prefab)
                    if (reference.guid == sceneGUID)
                    {
                        return reference.objectId == objectId;
                    }
                }
            }

            return false;
        }

        private Vector3 initialPosition;
        private Quaternion initialRotation;


        private static class ListBuffer<T>
        {
            private static readonly List<T> list = new();
            public static List<T> Get()
            {
                list.Clear();
                return list;
            }
        }


        // Find first scene object that matches reference within all current scenes
        public static bool TryFindSceneObject<T>(SceneObjectReference reference, out T result) where T : SceneObject
        {
            if (reference)
            {
                foreach (var sceneGUIDObj in SceneGUID.AllScenes)
                {
                    if (sceneGUIDObj.TryFindSceneObject(reference, out result))
                    {
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }
        public static bool TryFindSceneObject<T>(Scene scene, SceneObjectReference reference, out T result) where T : SceneObject
        {
            if (SceneGUID.TryGetSceneGUIDObject(scene, out var sceneGUIDObj))
            {
                return sceneGUIDObj.TryFindSceneObject(reference, out result);
            }
            result = default;
            return false;
        }

        // Find first scene object that matches reference within a specific scene
        public static bool TryFindSceneObject<T>(SceneObjectReference<T> reference, out T result) where T : SceneObject
        {
            return TryFindSceneObject((SceneObjectReference)reference, out result);
        }
        public static bool TryFindSceneObject<T>(Scene scene, SceneObjectReference<T> reference, out T result) where T : SceneObject
        {
            return TryFindSceneObject(scene, (SceneObjectReference)reference, out result);
        }

        // Find all scene objects that match reference within all current scenes
        public static T[] FindSceneObjects<T>(SceneObjectReference reference) where T : SceneObject
        {
            if (reference)
            {
                List<T> list = ListBuffer<T>.Get();
                foreach (var sceneGUIDObj in SceneGUID.AllScenes)
                {
                    sceneGUIDObj.FindSceneObjects(reference, list);
                }
                return list.ToArray();
            }
            return Array.Empty<T>();
        }
        public static T[] FindSceneObjects<T>(SceneObjectReference<T> reference) where T : SceneObject
        {
            return FindSceneObjects<T>((SceneObjectReference)reference);
        }

        // Find all scene objects that match reference within a specific scene
        public static T[] FindSceneObjects<T>(Scene scene, SceneObjectReference reference) where T : SceneObject
        {
            if (SceneGUID.TryGetSceneGUIDObject(scene, out var sceneGUIDObj))
            {
                List<T> list = ListBuffer<T>.Get();
                sceneGUIDObj.FindSceneObjects(reference, list);
                return list.ToArray();
            }
            return Array.Empty<T>();
        }
        public static T[] FindSceneObjects<T>(Scene scene, SceneObjectReference<T> reference) where T : SceneObject
        {
            return FindSceneObjects<T>(scene, (SceneObjectReference)reference);
        }


        protected virtual void Awake()
        {
            SceneGUID.RegisterSceneObject(this, out sceneGUID);

            transform.GetPositionAndRotation(out initialPosition, out initialRotation);
        }

        public virtual void SetActive(bool active)
        {

        }

        public void ReturnToInitialLocation()
        {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
        }

#if UNITY_EDITOR
        private static readonly List<SceneGUID> sceneGUIDCache = new();
        private static readonly string GUIDUpperPropertyPath = $"{nameof(guid)}.{Utility.GetPropertyBackingFieldName("Upper")}";
        private static readonly string GUIDLowerPropertyPath = $"{nameof(guid)}.{Utility.GetPropertyBackingFieldName("Lower")}";
        private static bool IsGUIDModified(SceneObject obj)
        {
            foreach (var modification in UnityEditor.PrefabUtility.GetPropertyModifications(obj))
            {
                if (modification.propertyPath == GUIDUpperPropertyPath || modification.propertyPath == GUIDLowerPropertyPath)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isPendingValidation = false;
        private void ValidateDelayed()
        {
            UnityEditor.EditorApplication.delayCall -= ValidateDelayed;
            isPendingValidation = false;

            if (!this)
            {
                return;
            }

            GameObject go = gameObject;

            // Skip prefab stage assets for now
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(go) != null)
            {
                return;
            }

            if (UnityEditor.EditorUtility.IsPersistent(go))
            {
                var globalObjectId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(this);
                if (globalObjectId.identifierType == 1)
                {
                    // Reset GUID and clear object ID on original prefabs
                    GUID assetGUID = new(globalObjectId.assetGUID.ToString());
                    if (assetGUID != guid || objectId != 0)
                    {
                        guid = assetGUID;
                        objectId = 0;
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
                else if (guid != GUID.zero || objectId != 0)
                {
                    // Clear everything on invalid objects
                    guid = GUID.zero;
                    objectId = 0;
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
            else if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                var globalObjectId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(this);
                if (globalObjectId.identifierType == 2)
                {
                    if (globalObjectId.targetPrefabId == 0)
                    {
                        // Reset GUID and object ID on regular objects
                        GUID sceneGUID = new(globalObjectId.assetGUID.ToString());
                        if (guid != sceneGUID || objectId != globalObjectId.targetObjectId)
                        {
                            guid = sceneGUID;
                            objectId = globalObjectId.targetObjectId;
                            UnityEditor.EditorUtility.SetDirty(this);
                        }
                    }
                    else
                    {
                        // Reset object ID on prefab instances
                        if (objectId != globalObjectId.targetPrefabId)
                        {
                            objectId = globalObjectId.targetPrefabId;
                            UnityEditor.EditorUtility.SetDirty(this);
                        }

                        // Clear GUID modifications on prefab instances
                        if (IsGUIDModified(this))
                        {
                            UnityEditor.SerializedObject obj = new(this);
                            obj.Update();
                            UnityEditor.PrefabUtility.RevertPropertyOverride(obj.FindProperty(nameof(guid)), UnityEditor.InteractionMode.AutomatedAction);
                            obj.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                }
                else if (guid != GUID.zero || objectId != 0)
                {
                    // Clear everything on invalid objects
                    guid = GUID.zero;
                    objectId = 0;
                    UnityEditor.EditorUtility.SetDirty(this);
                }

                // Ensure scene GUID
                var scene = gameObject.scene;
                if (scene.IsValid())
                {
                    SceneGUID sceneObject = null;
                    for (int i = 0; i < sceneGUIDCache.Count;)
                    {
                        if (!sceneGUIDCache[i])
                        {
                            sceneGUIDCache.RemoveAndSwap(i);
                            continue;
                        }
                        if (sceneGUIDCache[i].gameObject.scene == scene)
                        {
                            sceneObject = sceneGUIDCache[i];
                        }
                        i++;
                    }
                    if (!sceneObject)
                    {
                        foreach (var existingSceneObject in FindObjectsOfType<SceneGUID>())
                        {
                            if (existingSceneObject.gameObject.scene == scene)
                            {
                                if (!sceneObject)
                                {
                                    sceneObject = existingSceneObject;
                                }
                                else
                                {
                                    Debug.LogWarning($"Destroying duplicate scene GUID object '{existingSceneObject}'");
                                }
                            }
                        }
                        if (!sceneObject)
                        {
                            var sceneGUIDType = typeof(SceneGUID);
                            sceneObject = UnityEditor.ObjectFactory.CreateGameObject(scene, HideFlags.HideInHierarchy, sceneGUIDType.FullName, sceneGUIDType).GetComponent<SceneGUID>();
                            UnityEditor.Undo.ClearUndo(sceneObject.gameObject);
                        }
                        sceneGUIDCache.Add(sceneObject);
                    }
                }
            }
        }
        protected virtual void OnValidate()
        {
            if (!isPendingValidation)
            {
                isPendingValidation = true;
                UnityEditor.EditorApplication.delayCall += ValidateDelayed;
            }
        }
#endif
    }
}