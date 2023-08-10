using UnityEditor;

namespace AggroBird.UnityExtend.Editor
{
    [InitializeOnLoad]
    internal static class EditorGizmoInitializer
    {
        static EditorGizmoInitializer()
        {
            foreach (var type in TypeCache.GetTypesWithAttribute<HideGizmoInSceneAttribute>())
            {
                string key = $"AggroBird.UnityExtend.HideGizmo<{type.FullName}>";
                if (!EditorPrefs.GetBool(key))
                {
                    if (GizmoUtility.TryGetGizmoInfo(type, out GizmoInfo info))
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
}