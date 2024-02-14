using UnityEngine;

namespace AggroBird.UnityExtend
{
    public struct PositionRotation
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public static class PositionRotationUtility
    {
        public static PositionRotation GetPositionRotation(this Transform transform)
        {
            PositionRotation result = new();
            transform.GetPositionAndRotation(out result.position, out result.rotation);
            return result;
        }
        public static void SetPositionRotation(this Transform transform, PositionRotation value)
        {
            transform.SetPositionAndRotation(value.position, value.rotation);
        }

        public static PositionRotation GetLocalPositionRotation(this Transform transform)
        {
            PositionRotation result = new();
            transform.GetLocalPositionAndRotation(out result.position, out result.rotation);
            return result;
        }
        public static void SetLocalPositionRotation(this Transform transform, PositionRotation value)
        {
            transform.SetLocalPositionAndRotation(value.position, value.rotation);
        }
    }
}