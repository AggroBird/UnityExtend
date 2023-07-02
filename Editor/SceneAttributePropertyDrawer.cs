using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityEngineExtend.Editor
{
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class SceneAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                SceneAsset sceneObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);

                if (sceneObject == null && !string.IsNullOrWhiteSpace(property.stringValue))
                {
                    sceneObject = GetBuildSettingsSceneObject(property.stringValue);
                }

                if (sceneObject == null && !string.IsNullOrWhiteSpace(property.stringValue))
                {
                    Debug.LogError($"Failed to find scene {property.stringValue} in {property.serializedObject.targetObject} property {property.propertyPath}");
                }

                using (new EditorExtendUtility.MixedValueScope(property.hasMultipleDifferentValues))
                {
                    SceneAsset scene = (SceneAsset)EditorGUI.ObjectField(position, label, sceneObject, typeof(SceneAsset), true);

                    if (!property.hasMultipleDifferentValues)
                    {
                        property.stringValue = AssetDatabase.GetAssetPath(scene);
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Scene attribute can only be used on string properties");
            }

            EditorGUI.EndProperty();
        }

        protected SceneAsset GetBuildSettingsSceneObject(string sceneName)
        {
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                if (sceneAsset != null && sceneAsset.name == sceneName)
                {
                    return sceneAsset;
                }
            }
            return null;
        }
    }
}