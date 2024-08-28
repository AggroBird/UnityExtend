using UnityEngine;

namespace AggroBird.UnityExtend
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

        public static Vector3 OnlyX(this Vector3 vec)
        {
            return new(vec.x, 0, 0);
        }
        public static Vector3 OnlyY(this Vector3 vec)
        {
            return new(0, vec.y, 0);
        }
        public static Vector3 OnlyZ(this Vector3 vec)
        {
            return new(0, 0, vec.z);
        }
        public static Vector3 OnlyXY(this Vector3 vec)
        {
            return new(vec.x, vec.y, 0);
        }
        public static Vector3 OnlyXZ(this Vector3 vec)
        {
            return new(vec.x, 0, vec.z);
        }
        public static Vector3 OnlyYZ(this Vector3 vec)
        {
            return new(0, vec.y, vec.z);
        }

        public static Vector3 OverrideX(this Vector3 vec, float x)
        {
            return new(x, vec.y, vec.z);
        }
        public static Vector3 OverrideY(this Vector3 vec, float y)
        {
            return new(vec.x, y, vec.z);
        }
        public static Vector3 OverrideZ(this Vector3 vec, float z)
        {
            return new(vec.x, vec.y, z);
        }
        public static Vector3 OverrideXY(this Vector3 vec, Vector2 xy)
        {
            return new(xy.x, xy.y, vec.z);
        }
        public static Vector3 OverrideXZ(this Vector3 vec, Vector2 xz)
        {
            return new(xz.x, vec.y, xz.y);
        }
        public static Vector3 OverrideYZ(this Vector3 vec, Vector2 yz)
        {
            return new(vec.x, yz.x, yz.y);
        }

        public static Vector3 Horizontal3D(this Vector2 vec)
        {
            return new Vector3(vec.x, 0, vec.y);
        }
        public static Vector3 Vertical3D(this Vector2 vec)
        {
            return new Vector3(vec.x, vec.y, 0);
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

        public static Color OverrideR(this Color c, float r)
        {
            return new Color(r, c.g, c.b, c.a);
        }
        public static Color OverrideG(this Color c, float g)
        {
            return new Color(c.r, g, c.b, c.a);
        }
        public static Color OverrideB(this Color c, float b)
        {
            return new Color(c.r, c.g, b, c.a);
        }
        public static Color OverrideA(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }

        public static Rect ToRect(this RectInt rect)
        {
            return new Rect(rect.x, rect.y, rect.width, rect.height);
        }

        // Transform swizzles
        public static Vector2 GetPositionXZ(this Transform transform)
        {
            return transform.position.GetXZ();
        }
        public static void SetPositionXZ(this Transform transform, Vector2 xz)
        {
            transform.position = new Vector3(xz.x, transform.position.y, xz.y);
        }
        public static Vector2 GetPositionXY(this Transform transform)
        {
            return transform.position.GetXY();
        }
        public static void SetPositionXY(this Transform transform, Vector2 xy)
        {
            transform.position = new Vector3(xy.x, xy.y, transform.position.z);
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