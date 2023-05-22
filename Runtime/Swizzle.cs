﻿using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    // Swizzles for setting values in common structs
    public static class Swizzle
    {
        // Vector swizzles
        public static Vector2 GetXY(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.y);
        }
        public static void SetXY(this ref Vector3 vec, Vector2 xy)
        {
            vec.x = xy.x;
            vec.y = xy.y;
        }
        public static void SetXY(this ref Vector3 vec, float x, float y)
        {
            vec.x = x;
            vec.y = y;
        }

        public static Vector2 GetXZ(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }
        public static void SetXZ(this ref Vector3 vec, Vector2 xz)
        {
            vec.x = xz.x;
            vec.z = xz.y;
        }
        public static void SetXZ(this ref Vector3 vec, float x, float z)
        {
            vec.x = x;
            vec.z = z;
        }

        public static Vector2 GetYZ(this Vector3 vec)
        {
            return new Vector2(vec.y, vec.z);
        }
        public static void SetYZ(this ref Vector3 vec, Vector2 yz)
        {
            vec.y = yz.x;
            vec.z = yz.y;
        }
        public static void SetYZ(this ref Vector3 vec, float y, float z)
        {
            vec.y = y;
            vec.z = z;
        }

        public static Vector3 Horizontal3D(this Vector2 vec)
        {
            return new Vector3(vec.x, 0, vec.y);
        }

        public static void SetRGB(this ref Color c, float rgb)
        {
            c.r = rgb;
            c.g = rgb;
            c.b = rgb;
        }
        public static void SetRGB(this ref Color c, float r, float g, float b)
        {
            c.r = r;
            c.g = g;
            c.b = b;
        }

        public static Rect ToRect(this RectInt rect)
        {
            return new Rect(rect.x, rect.y, rect.width, rect.height);
        }


        // Transform swizzles
        public static void SetIdentity(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        public static Vector2 PositionXZ(this Transform transform)
        {
            return transform.position.GetXZ();
        }
        public static Vector2 PositionXY(this Transform transform)
        {
            return transform.position.GetXY();
        }

        public static void SetPitch(this Transform transform, float pitch)
        {
            Vector3 euler = transform.eulerAngles;
            euler.x = pitch;
            transform.eulerAngles = euler;
        }
        public static float GetPitch(this Transform transform)
        {
            return transform.eulerAngles.x;
        }
        public static void SetYaw(this Transform transform, float yaw)
        {
            Vector3 euler = transform.eulerAngles;
            euler.y = yaw;
            transform.eulerAngles = euler;
        }
        public static float GetYaw(this Transform transform)
        {
            return transform.eulerAngles.y;
        }
        public static void SetRoll(this Transform transform, float roll)
        {
            Vector3 euler = transform.eulerAngles;
            euler.z = roll;
            transform.eulerAngles = euler;
        }
        public static float GetRoll(this Transform transform)
        {
            return transform.eulerAngles.z;
        }
    }
}