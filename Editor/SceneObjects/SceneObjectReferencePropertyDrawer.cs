using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(SceneObjectReference<>))]
    internal sealed class SceneObjectReferencePropertyDrawer : PropertyDrawer
    {
        internal static class SceneObjectReferenceCache
        {
            private static readonly Dictionary<EditorSceneObjectReference, UnityObject> cache = new();

            public static bool TryGetSceneObject(EditorSceneObjectReference key, out UnityObject obj)
            {
                return cache.TryGetValue(key, out obj) && obj;
            }
            public static void StoreSceneObject(EditorSceneObjectReference key, UnityObject obj)
            {
                cache[key] = obj;
            }
        }

        private static void GetSceneObjectReferenceValues(SerializedProperty property, out GUID guid, out ulong prefabId, out ulong objectId)
        {
            var guidProperty = property.FindPropertyRelative("guid");
            ulong upper = guidProperty.FindPropertyRelative((GUID def) => def.Upper).ulongValue;
            ulong lower = guidProperty.FindPropertyRelative((GUID def) => def.Lower).ulongValue;
            guid = new(upper, lower);
            prefabId = property.FindPropertyRelative("prefabId").ulongValue;
            objectId = property.FindPropertyRelative("objectId").ulongValue;
        }
        private static void SetSceneObjectReferenceValues(SerializedProperty property, GUID guid, ulong prefabId, ulong objectId)
        {
            var guidProperty = property.FindPropertyRelative("guid");
            guidProperty.FindPropertyRelative((GUID def) => def.Upper).ulongValue = guid.Upper;
            guidProperty.FindPropertyRelative((GUID def) => def.Lower).ulongValue = guid.Lower;
            property.FindPropertyRelative("prefabId").ulongValue = prefabId;
            property.FindPropertyRelative("objectId").ulongValue = objectId;
        }

        private static bool TryLoadPrefabAsset(GUID guid, Type type, out UnityObject prefab)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid.ToString());
            if (!string.IsNullOrEmpty(path))
            {
                prefab = AssetDatabase.LoadAssetAtPath(path, type);
                return prefab;
            }
            prefab = null;
            return false;
        }

        private static GUIStyle buttonStyle;

        private static Texture sceneIconTexture;
        private static Texture SceneIconTexture
        {
            get
            {
                if (!sceneIconTexture)
                {
                    sceneIconTexture = EditorGUIUtility.IconContent("d_SceneAsset Icon").image;
                }
                return sceneIconTexture;
            }
        }
        private static Texture prefabIconTexture;
        private static Texture PrefabIconTexture
        {
            get
            {
                if (!prefabIconTexture)
                {
                    prefabIconTexture = EditorGUIUtility.IconContent("d_Prefab Icon").image;
                }
                return prefabIconTexture;
            }
        }

        private readonly ref struct CustomObjectFieldContentScope
        {
            private static readonly FieldInfo mixedValueContentFieldInfo = typeof(EditorGUI).GetField("s_MixedValueContent", BindingFlags.Static | BindingFlags.NonPublic);

            private readonly GUIContent contentReference;
            private readonly string originalText;
            private readonly string originalTooltip;
            private readonly bool currentMixedValueState;

            public CustomObjectFieldContentScope(string text, string tooltip)
            {
                contentReference = mixedValueContentFieldInfo.GetValue(null) as GUIContent;
                if (contentReference != null)
                {
                    originalText = contentReference.text;
                    originalTooltip = contentReference.tooltip;
                    contentReference.text = text;
                    contentReference.tooltip = tooltip;
                }
                else
                {
                    originalText = originalTooltip = default;
                }

                currentMixedValueState = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = true;
            }

            public void Dispose()
            {
                if (contentReference != null)
                {
                    contentReference.text = originalText;
                    contentReference.tooltip = originalTooltip;
                }
                EditorGUI.showMixedValue = currentMixedValueState;
            }
        }



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            if (!property.hasMultipleDifferentValues)
            {
                GetSceneObjectReferenceValues(property, out GUID guid, out ulong prefabId, out ulong objectId);

                Type referenceType = fieldInfo.FieldType.GetGenericArguments()[0];

                void DrawPropertyField()
                {
                    if (guid == GUID.zero && prefabId == 0 && objectId == 0)
                    {
                        // No object
                        PrefixButton(position, property, null, false, null, referenceType);
                        return;
                    }
                    else if (guid != GUID.zero && prefabId != 0 && objectId == 0)
                    {
                        // Any of prefab
                        EditorSceneObjectReference key = new(guid, prefabId, objectId);
                        if (!SceneObjectReferenceCache.TryGetSceneObject(key, out UnityObject prefabObject))
                        {
                            if (TryLoadPrefabAsset(guid, referenceType, out prefabObject))
                            {
                                SceneObjectReferenceCache.StoreSceneObject(key, prefabObject);
                            }
                        }
                        if (prefabObject)
                        {
                            if (PrefixButton(position, property, PrefabIconTexture, true, prefabObject, referenceType))
                            {
                                AssetDatabase.OpenAsset(prefabObject);
                            }
                        }
                        else
                        {
                            PrefixButton(position, property, PrefabIconTexture, false, EditorExtendUtility.MissingObject, referenceType);
                        }
                        return;
                    }
                    else if (guid != GUID.zero && objectId != 0)
                    {
                        // Scene object
                        string scenePath = AssetDatabase.GUIDToAssetPath(guid.ToString());
                        if (!string.IsNullOrEmpty(scenePath))
                        {
                            if (scenePath == SceneManager.GetActiveScene().path)
                            {
                                EditorSceneObjectReference key = new(guid, prefabId, objectId);
                                if (!SceneObjectReferenceCache.TryGetSceneObject(key, out UnityObject sceneObject))
                                {
                                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                                    {
                                        if (SceneObject.TryFindSceneObject(key, out SceneObject playingSceneObject))
                                        {
                                            sceneObject = playingSceneObject;
                                            SceneObjectReferenceCache.StoreSceneObject(key, sceneObject);
                                        }
                                    }
                                    else if (TryParseGlobalObjectId(guid, prefabId, objectId, out GlobalObjectId globalObjectId))
                                    {
                                        // TODO: This can cause the editor to slow down when referencing a broken object
                                        // If we can find a faster way to find a scene object through an object id, that would be great
                                        // But at time of writing, Unity does not give us any solution for this.
                                        sceneObject = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                                        if (sceneObject)
                                        {
                                            SceneObjectReferenceCache.StoreSceneObject(key, sceneObject);
                                        }
                                    }
                                }
                                if (sceneObject)
                                {
                                    PrefixButton(position, property, SceneIconTexture, false, sceneObject, referenceType);
                                }
                                else
                                {
                                    // Missing object
                                    PrefixButton(position, property, SceneIconTexture, false, EditorExtendUtility.MissingObject, referenceType);
                                }
                            }
                            else
                            {
                                // Different scene
                                using (new CustomObjectFieldContentScope("Scene Reference", null))
                                {
                                    if (PrefixButton(position, property, SceneIconTexture, true, null, referenceType))
                                    {
                                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Missing scene
                            using (new CustomObjectFieldContentScope("Missing Scene", null))
                            {
                                PrefixButton(position, property, SceneIconTexture, false, null, referenceType);
                            }
                        }
                    }
                    else
                    {
                        // Invalid object reference
                        PrefixButton(position, property, null, false, EditorExtendUtility.MissingObject, referenceType);
                    }
                }

                DrawPropertyField();
            }
            else
            {
                EditorGUI.showMixedValue = true;
                EditorGUI.ObjectField(position, null, typeof(UnityObject), true);
            }

            EditorGUI.EndProperty();
        }

        static void ObjectField(Rect position, SerializedProperty property, UnityObject showValue, Type referenceType)
        {
            EditorGUI.BeginChangeCheck();
            UnityObject newObj = EditorGUI.ObjectField(position, showValue, referenceType, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (newObj)
                {
                    GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(newObj);
                    if (globalObjectId.assetGUID == default)
                    {
                        Debug.LogError($"Object '{newObj}' has no GUID, possibly because it is within a scene that has not been saved yet.");
                        return;
                    }

                    if (globalObjectId.identifierType == 1)
                    {
                        // Assigned prefab
                        SetSceneObjectReferenceValues(property, new GUID(globalObjectId.assetGUID.ToString()), globalObjectId.targetObjectId, 0);
                    }
                    else if (globalObjectId.identifierType == 2)
                    {
                        if (globalObjectId.targetPrefabId != 0)
                        {
                            // Assigned prefab instance
                            SetSceneObjectReferenceValues(property, new GUID(globalObjectId.assetGUID.ToString()), globalObjectId.targetObjectId, globalObjectId.targetPrefabId);
                        }
                        else
                        {
                            // Assigned regular object
                            SetSceneObjectReferenceValues(property, new GUID(globalObjectId.assetGUID.ToString()), 0, globalObjectId.targetObjectId);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Object '{newObj}' is not a valid scene object reference.");
                    }
                }
                else
                {
                    // Assigned null
                    SetSceneObjectReferenceValues(property, GUID.zero, 0, 0);
                }
            }
        }

        static bool PrefixButton(Rect position, SerializedProperty property, Texture content, bool clickable, UnityObject showValue, Type referenceType)
        {
            buttonStyle ??= new GUIStyle(GUI.skin.button) { padding = new RectOffset(1, 1, 1, 1) };
            Rect buttonRect = position;
            buttonRect.width = 18;
            position.x += 20;
            position.width -= 20;
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            ObjectField(position, property, showValue, referenceType);
            EditorGUI.indentLevel = indent;
            bool guiEnabled = GUI.enabled;
            GUI.enabled = clickable;
            bool result = GUI.Button(buttonRect, content, buttonStyle);
            GUI.enabled = guiEnabled;
            return result;
        }

        static bool TryParseGlobalObjectId(GUID guid, ulong prefabId, ulong objectId, out GlobalObjectId globalObjectId)
        {
            return prefabId != 0 ?
                GlobalObjectId.TryParse($"GlobalObjectId_V1-2-{guid}-{prefabId}-{objectId}", out globalObjectId) :
                GlobalObjectId.TryParse($"GlobalObjectId_V1-2-{guid}-{objectId}-0", out globalObjectId);
        }
    }
}