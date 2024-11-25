using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal sealed class ReadOnlyAttributeDrawer : PropertyDrawer, IDisposable
    {
        private PropertyDrawer cachedPropertyDrawer;

        private bool TryGetCachedPropertyDrawer(out PropertyDrawer propertyDrawer)
        {
            if (cachedPropertyDrawer != null)
            {
                propertyDrawer = cachedPropertyDrawer;
                return true;
            }
            if (PropertyDrawerExtendUtility.TryGetPropertyDrawer(fieldInfo, out cachedPropertyDrawer))
            {
                propertyDrawer = cachedPropertyDrawer;
                return true;
            }
            propertyDrawer = null;
            return false;
        }
        public void Dispose()
        {
            if (cachedPropertyDrawer is IDisposable disposable)
            {
                disposable.Dispose();
            }
            cachedPropertyDrawer = null;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                if (TryGetCachedPropertyDrawer(out var defaultPropertyDrawer))
                {
                    defaultPropertyDrawer.OnGUI(position, property, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (TryGetCachedPropertyDrawer(out var defaultPropertyDrawer))
            {
                return defaultPropertyDrawer.GetPropertyHeight(property, label);
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
        }
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            if (TryGetCachedPropertyDrawer(out var defaultPropertyDrawer))
            {
                return defaultPropertyDrawer.CanCacheInspectorGUI(property);
            }
            else
            {
                return base.CanCacheInspectorGUI(property);
            }
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (TryGetCachedPropertyDrawer(out var defaultPropertyDrawer))
            {
                return defaultPropertyDrawer.CreatePropertyGUI(property);
            }
            else
            {
                return base.CreatePropertyGUI(property);
            }
        }
    }
}