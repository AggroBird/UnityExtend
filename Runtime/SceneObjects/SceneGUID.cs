using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AggroBird.UnityExtend
{
    [DefaultExecutionOrder(int.MinValue)]
    internal sealed class SceneGUID : MonoBehaviour
    {
        [SerializeField] private GUID sceneGUID;

        private int refCount = 0;

        private static readonly Dictionary<Scene, SceneGUID> activeScenes = new();
        internal static Dictionary<Scene, SceneGUID>.ValueCollection AllScenes => activeScenes.Values;
        private static (Scene scene, SceneGUID obj) sceneCache;

        internal static bool TryGetSceneGUIDObject(Scene scene, out SceneGUID sceneObj)
        {
            if (sceneCache.scene.Equals(scene) && sceneCache.obj)
            {
                sceneObj = sceneCache.obj;
                return true;
            }

            if (activeScenes.TryGetValue(scene, out var value))
            {
                sceneObj = value;
                sceneCache = (scene, sceneObj);
                return true;
            }

            sceneObj = default;
            return false;
        }
        public static bool TryGetSceneGUID(Scene scene, out GUID guid)
        {
            if (TryGetSceneGUIDObject(scene, out SceneGUID obj))
            {
                guid = obj.sceneGUID;
                return true;
            }
            guid = default;
            return false;
        }

        // All pre-placed scene objects (regular scene objects and scene prefab instances)
        private readonly Dictionary<ulong, SceneObject> allLocalSceneObjects = new();
        // All pre-placed scene prefab instances, grouped by GUID
        private readonly Dictionary<GUID, Dictionary<ulong, SceneObject>> allLocalScenePrefabInstances = new();

        // Utility function to get a new ID in case of a collision (Object.Instantiated scene object)
        // Granted instantiation does not run from SceneObject.Awake(), all scene objects should have already
        // had their ID's registered (SceneObject.Awake should not be able to instantiate anyway, because of recursion)
        private ulong lastReassignedObjectId = 0;
        private ulong GetUniqueObjectID(ulong currentId)
        {
            // Prefab instantiation
            if (currentId == 0)
            {
                currentId = ++lastReassignedObjectId;
            }

            // Ensure ID not 0 or in use
            while (currentId == 0 || allLocalSceneObjects.ContainsKey(currentId))
            {
                currentId++;
            }

            return currentId;
        }


        internal bool TryFindSceneObject<T>(SceneObjectReference reference, out T result) where T : SceneObject
        {
            if (reference.guid != GUID.zero)
            {
                // Check if object within this scene
                if (reference.guid == sceneGUID)
                {
                    // Object ID cannot be 0 here
                    if (reference.objectId != 0)
                    {
                        // Local pre-placed regular scene object
                        if (allLocalSceneObjects.TryGetValue(reference.objectId, out SceneObject sceneObject))
                        {
                            if (sceneObject && sceneObject is T casted)
                            {
                                result = casted;
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    // Search the prefab instances
                    if (allLocalScenePrefabInstances.TryGetValue(reference.guid, out var table))
                    {
                        if (reference.objectId != 0)
                        {
                            // Specific prefab instance
                            if (table.TryGetValue(reference.objectId, out SceneObject sceneObject))
                            {
                                if (sceneObject && sceneObject is T casted)
                                {
                                    result = casted;
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            // Any of prefab type
                            foreach (var sceneObject in table.Values)
                            {
                                if (sceneObject && sceneObject is T casted)
                                {
                                    result = casted;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            result = default;
            return false;
        }
        internal void FindSceneObjects<T>(SceneObjectReference reference, List<T> result) where T : SceneObject
        {
            if (reference.guid != GUID.zero)
            {
                // Check if object within this scene
                if (reference.guid == sceneGUID)
                {
                    // Object ID cannot be 0 here
                    if (reference.objectId != 0)
                    {
                        // Local pre-placed regular scene object
                        if (allLocalSceneObjects.TryGetValue(reference.objectId, out SceneObject sceneObject))
                        {
                            if (sceneObject && sceneObject is T casted)
                            {
                                result.Add(casted);
                            }
                        }
                    }
                }
                else
                {
                    // Search the prefab instances
                    if (allLocalScenePrefabInstances.TryGetValue(reference.guid, out var table))
                    {
                        if (reference.objectId != 0)
                        {
                            // Specific prefab instance
                            if (table.TryGetValue(reference.objectId, out SceneObject sceneObject))
                            {
                                if (sceneObject && sceneObject is T casted)
                                {
                                    result.Add(casted);
                                }
                            }
                        }
                        else
                        {
                            // Any of prefab type
                            foreach (var sceneObject in table.Values)
                            {
                                if (sceneObject && sceneObject is T casted)
                                {
                                    result.Add(casted);
                                }
                            }
                        }
                    }
                }
            }
        }


        private GUID RegisterLocalSceneObject(SceneObject sceneObject, ref ulong objectId)
        {
            GUID objectGUID = sceneObject.SceneObjectGUID;
            objectId = sceneObject.SceneObjectID;
            if (objectGUID != GUID.zero)
            {
                bool isPrefabInstance = objectGUID != sceneGUID;
                if (objectId != 0 || isPrefabInstance)
                {
                    objectId = GetUniqueObjectID(objectId);

                    // If the guid is not the scene GUID, its a prefab instance
                    if (isPrefabInstance)
                    {
                        // Add to prefab instance table
                        if (!allLocalScenePrefabInstances.TryGetValue(objectGUID, out var table))
                        {
                            allLocalScenePrefabInstances[objectGUID] = table = new();
                        }
                        table[objectId] = sceneObject;
                    }

                    // Add to all objects table
                    allLocalSceneObjects[objectId] = sceneObject;
                }

                return sceneGUID;
            }
            else
            {
                return GUID.zero;
            }
        }
        internal static GUID RegisterSceneObject(SceneObject sceneObject, ref ulong objectId)
        {
            if (TryGetSceneGUIDObject(sceneObject.gameObject.scene, out SceneGUID sceneGUIDObj))
            {
                // Register this object to the scene that its currently in
                return sceneGUIDObj.RegisterLocalSceneObject(sceneObject, ref objectId);
            }
            else
            {
                return GUID.zero;
            }
        }


        private void Awake()
        {
            if (sceneGUID != GUID.zero)
            {
                var scene = gameObject.scene;
                if (scene.IsValid())
                {
                    if (activeScenes.TryGetValue(scene, out var value))
                    {
                        Debug.LogError($"Scene '{scene}' is being loaded additively multiple times. This may cause problems with scene object identification.", this);
                        value.refCount++;
                    }
                    else
                    {
                        activeScenes[scene] = this;
                        refCount = 1;
                    }
                }
            }
        }
        private void OnDestroy()
        {
            if (sceneGUID != GUID.zero)
            {
                var scene = gameObject.scene;
                if (scene.IsValid())
                {
                    if (activeScenes.TryGetValue(scene, out var value))
                    {
                        if (value.refCount == 1)
                        {
                            activeScenes.Remove(scene);
                        }
                        else
                        {
                            value.refCount--;
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        private bool isPendingValidation = false;
        private void ValidateDelayed()
        {
            UnityEditor.EditorApplication.delayCall -= ValidateDelayed;
            isPendingValidation = false;

            if (!this)
            {
                return;
            }

            var globalObjectId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(this);
            if (globalObjectId.identifierType == 2)
            {
                GUID sceneGUID = new(globalObjectId.assetGUID.ToString());
                if (this.sceneGUID != sceneGUID)
                {
                    this.sceneGUID = sceneGUID;
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
            else
            {
                if (sceneGUID != GUID.zero)
                {
                    sceneGUID = GUID.zero;
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
        private void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!isPendingValidation && gameObject.scene.IsValid())
                {
                    isPendingValidation = true;
                    UnityEditor.EditorApplication.delayCall += ValidateDelayed;
                }
            }
        }
#endif
    }
}