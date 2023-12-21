using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AggroBird.UnityExtend
{
    [DefaultExecutionOrder(int.MinValue)]
    internal sealed class SceneGUID : MonoBehaviour
    {
        [SerializeField] private GUID guid;

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
                guid = obj.guid;
                return true;
            }
            guid = default;
            return false;
        }

        // All pre-placed scene objects (regular scene objects and scene prefab instances)
        private readonly Dictionary<ulong, SceneObject> allLocalSceneObjects = new();
        // All pre-placed scene prefab instances, grouped by GUID
        private readonly Dictionary<GUID, Dictionary<ulong, SceneObject>> allLocalScenePrefabInstances = new();
        // All later instantiated objects
        //private readonly Dictionary<GUID, List<SceneObject>> instantiatedPrefabs = new();


        internal bool TryFindSceneObject<T>(SceneObjectReference reference, out T result) where T : SceneObject
        {
            if (reference.guid != GUID.zero)
            {
                // Check if object within this scene
                if (reference.guid == guid)
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
                if (reference.guid == guid)
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


        internal static void RegisterSceneObject(SceneObject sceneObject, out GUID sceneGUID)
        {
            GUID objectGUID = sceneObject.SceneObjectGUID;
            if (objectGUID != GUID.zero)
            {
                // Register this object to the scene that its currently in
                if (TryGetSceneGUIDObject(sceneObject.gameObject.scene, out SceneGUID sceneGUIDObj))
                {
                    ulong objectId = sceneObject.SceneObjectID;
                    if (objectId != 0)
                    {
                        // TODO: Check for duplicates

                        // If the guid is not the scene GUID, its a prefab instance
                        if (objectGUID != sceneGUIDObj.guid)
                        {
                            // Add to prefab instance table
                            if (!sceneGUIDObj.allLocalScenePrefabInstances.TryGetValue(objectGUID, out var table))
                            {
                                sceneGUIDObj.allLocalScenePrefabInstances[objectGUID] = table = new();
                            }
                            table[objectId] = sceneObject;
                        }

                        // Add to all objects table
                        sceneGUIDObj.allLocalSceneObjects[objectId] = sceneObject;
                    }
                    else
                    {
                        // TODO: Instantiated scene object or prefab
                    }

                    sceneGUID = sceneGUIDObj.guid;
                    return;
                }
            }

            sceneGUID = GUID.zero;
        }


        private void Awake()
        {
            if (guid != GUID.zero)
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
            if (guid != GUID.zero)
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
                if (guid != sceneGUID)
                {
                    guid = sceneGUID;
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
            else
            {
                if (guid != GUID.zero)
                {
                    guid = GUID.zero;
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