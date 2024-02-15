using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    internal class SceneAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                SceneAsset scene;
                EditorGUI.BeginChangeCheck();
                using (new EditorExtendUtility.MixedValueScope(property.hasMultipleDifferentValues))
                {
                    if (string.IsNullOrWhiteSpace(property.stringValue))
                    {
                        scene = (SceneAsset)EditorGUI.ObjectField(position, label, null, typeof(SceneAsset), false);
                    }
                    else
                    {
                        scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
                        if (!scene)
                        {
                            scene = (SceneAsset)EditorGUI.ObjectField(position, label, EditorExtendUtility.MissingObject, typeof(SceneAsset), false);
                        }
                        else
                        {
                            scene = (SceneAsset)EditorGUI.ObjectField(position, label, scene, typeof(SceneAsset), false);
                        }
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (scene)
                    {
                        property.stringValue = AssetDatabase.GetAssetPath(scene);
                    }
                    else
                    {
                        property.stringValue = string.Empty;
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Scene attribute can only be used on string properties");
            }

            EditorGUI.EndProperty();
        }
    }
}