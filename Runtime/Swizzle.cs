using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    // Swizzles for setting values in common structs
    public static class Swizzle
    {
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
    }
}