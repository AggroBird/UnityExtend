using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace AggroBird.UnityExtend.Editor
{
    public static class DrivenPropertyManager
    {
        private const string DelegatePostfix = "Delegate";

        private delegate void RegisterPropertyDelegate(Object driver, Object target, string propertyPath);
        private delegate void TryRegisterPropertyDelegate(Object driver, Object target, string propertyPath);
        private delegate void UnregisterPropertyDelegate(Object driver, Object target, string propertyPath);
        private delegate void UnregisterPropertiesDelegate(Object driver);

        private static readonly RegisterPropertyDelegate registerProperty;
        private static readonly TryRegisterPropertyDelegate tryRegisterProperty;
        private static readonly UnregisterPropertyDelegate unregisterProperty;
        private static readonly UnregisterPropertiesDelegate unregisterProperties;

        static DrivenPropertyManager()
        {
            static T BindDelegate<T>(Type type) where T : Delegate
            {
                string name = typeof(T).Name;
                name = name.Substring(0, name.Length - DelegatePostfix.Length);
                return type.GetMethod(name).CreateDelegate(typeof(T)) as T ?? throw new MissingMethodException($"Failed find method '{name}' in type '{type}'");
            }

            const string TypeName = "UnityEngine.DrivenPropertyManager";
            var assembly = typeof(Object).Assembly;
            Type drivenPropertyManagerType = assembly.GetType(TypeName) ?? throw new TypeLoadException($"Failed to find type '{TypeName}' in assembly '{assembly}'");
            registerProperty = BindDelegate<RegisterPropertyDelegate>(drivenPropertyManagerType);
            tryRegisterProperty = BindDelegate<TryRegisterPropertyDelegate>(drivenPropertyManagerType);
            unregisterProperty = BindDelegate<UnregisterPropertyDelegate>(drivenPropertyManagerType);
            unregisterProperties = BindDelegate<UnregisterPropertiesDelegate>(drivenPropertyManagerType);
        }

        public static void RegisterProperty(AnimationModeDriver driver, Object target, string propertyPath) => registerProperty(driver, target, propertyPath);
        public static void TryRegisterProperty(AnimationModeDriver driver, Object target, string propertyPath) => tryRegisterProperty(driver, target, propertyPath);
        public static void UnregisterProperty(AnimationModeDriver driver, Object target, string propertyPath) => unregisterProperty(driver, target, propertyPath);
        public static void UnregisterProperties(AnimationModeDriver driver) => unregisterProperties(driver);
    }
}