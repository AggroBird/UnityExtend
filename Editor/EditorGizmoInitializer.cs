using UnityEditor;

namespace AggroBird.UnityExtend.Editor
{
    [InitializeOnLoad]
    internal static class EditorGizmoInitializer
    {
        static EditorGizmoInitializer()
        {
            SceneView.beforeSceneGui += BeforeSceneGui;
        }

        private static void BeforeSceneGui(SceneView obj)
        {
            SceneView.beforeSceneGui -= BeforeSceneGui;

            foreach (var type in TypeCache.GetTypesWithAttribute<HideGizmoInSceneAttribute>())
            {
                string key = $"AggroBird.UnityExtend.HideGizmo<{type.FullName}>";
                if (GizmoUtility.TryGetGizmoInfo(type, out GizmoInfo info) && !EditorPrefs.GetBool(key))
                {
                    if (info.hasIcon && info.iconEnabled)
                    {
                        info.iconEnabled = false;
                        GizmoUtility.ApplyGizmoInfo(info, false);
                        EditorPrefs.SetBool(key, true);
                    }
                }
            }
        }
    }
}