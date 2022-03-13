using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    // Swizzles for setting values in common structs
    // Since most of unity's fields are properties, assigning individual fields is not allowed
    // (transform.position.x). These swizzles allow for setting fields in one line:
    // (e.g. transform.position = Swizzle.SetX(transform.position, 15))
    public struct Swizzle
    {
        public static Vector2 Vec2(float xy)
        {
            return new Vector2(xy, xy);
        }

        public static Vector3 Vec3(float xyz)
        {
            return new Vector3(xyz, xyz, xyz);
        }
        public static Vector3 Vec3(Vector2 xy, float z)
        {
            return new Vector3(xy.x, xy.y, z);
        }
        public static Vector3 Vec3(float x, Vector2 yz)
        {
            return new Vector3(x, yz.x, yz.y);
        }
        public static Vector3 Vec3(Vector2 vec)
        {
            return new Vector3(vec.x, vec.y);
        }

        public static Vector2 SetX(Vector2 vec, float x)
        {
            return new Vector2(x, vec.y);
        }
        public static Vector2 SetY(Vector2 vec, float y)
        {
            return new Vector2(vec.x, y);
        }

        public static Vector3 SetX(Vector3 vec, float x)
        {
            return new Vector3(x, vec.y, vec.z);
        }
        public static Vector3 SetY(Vector3 vec, float y)
        {
            return new Vector3(vec.x, y, vec.z);
        }
        public static Vector3 SetZ(Vector3 vec, float z)
        {
            return new Vector3(vec.x, vec.y, z);
        }

        public static Vector3 SetXY(Vector3 vec, Vector2 xy)
        {
            vec.x = xy.x;
            vec.y = xy.y;
            return vec;
        }
        public static Vector3 SetYZ(Vector3 vec, Vector2 yz)
        {
            vec.y = yz.x;
            vec.z = yz.y;
            return vec;
        }

        public static Color SetR(Color c, float r)
        {
            return new Color(r, c.g, c.b, c.a);
        }
        public static Color SetG(Color c, float g)
        {
            return new Color(c.r, g, c.b, c.a);
        }
        public static Color SetB(Color c, float b)
        {
            return new Color(c.r, c.g, b, c.a);
        }
        public static Color SetA(Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }
        public static Color SetRGB(Color rgb, float a)
        {
            return new Color(rgb.r, rgb.g, rgb.b, a);
        }
        public static Color SetRGB(float r, float g, float b, float a)
        {
            return new Color(r, g, b, a);
        }
    }

    public static class Vector2Swizzle
    {
        public static Vector3 Horizontal3D(this Vector2 vec)
        {
            return new Vector3(vec.x, 0, vec.y);
        }
    }

    public static class Vector3Swizzle
    {
        public static Vector2 GetXY(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.y);
        }
        public static Vector2 GetXZ(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }
        public static Vector2 GetYZ(this Vector3 vec)
        {
            return new Vector2(vec.y, vec.z);
        }

        public static void SetXY(this Vector3 vec, Vector2 set)
        {
            vec.x = set.x;
            vec.y = set.y;
        }
        public static void SetXZ(this Vector3 vec, Vector2 set)
        {
            vec.x = set.x;
            vec.z = set.y;
        }
        public static void SetYZ(this Vector3 vec, Vector2 set)
        {
            vec.y = set.x;
            vec.z = set.y;
        }
    }

    public static class RectIntSwizzle
    {
        public static Rect ToRect(this RectInt rect)
        {
            return new Rect(rect.x, rect.y, rect.width, rect.height);
        }
    }
}