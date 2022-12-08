using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    // Math extensions
    // All vector functions assume clockwise rotation and start from up (0,1) at angle 0
    public struct Mathfx
    {
        public const float PI2 = Mathf.PI * 2;

        // Rotate a vector clockwise
        public static Vector2 Rotate(Vector2 vec, float angleRad)
        {
            float x1 = Mathf.Sin(-angleRad);
            float y1 = Mathf.Cos(-angleRad);
            float x = vec.x * y1 - vec.y * x1;
            float y = vec.x * x1 + vec.y * y1;
            return new Vector2(x, y);
        }
        public static Vector2 RotateDeg(Vector2 vec, float angleDeg)
        {
            return Rotate(vec, angleDeg * Mathf.Deg2Rad);
        }

        // Create a vector from a rotation (inverse of AngleFromVector)
        public static Vector2 VectorFromAngle(float angleRad)
        {
            return new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
        }
        public static Vector2 VectorFromAngleDeg(float angleDeg)
        {
            return VectorFromAngle(angleDeg * Mathf.Deg2Rad);
        }

        // Returns the signed angle of a vector from -180 to 180 (inverse of VectorFromAngle)
        public static float AngleFromVector(Vector2 vec)
        {
            return AngleFromNormalizedVector(vec.normalized);
        }
        public static float AngleFromVectorDeg(Vector2 vec)
        {
            return AngleFromNormalizedVectorDeg(vec.normalized);
        }
        // Same as VectorAngle, but assumes vec is already normalized. Results may be skewed if not.
        public static float AngleFromNormalizedVector(Vector2 vec)
        {
            return Mathf.Acos(Mathf.Clamp(vec.y, -1, 1)) * (vec.x >= 0.0f ? 1 : -1);
        }
        public static float AngleFromNormalizedVectorDeg(Vector2 vec)
        {
            return Mathf.Acos(Mathf.Clamp(vec.y, -1, 1)) * (vec.x >= 0.0f ? Mathf.Rad2Deg : -Mathf.Rad2Deg);
        }

        // Get the signed angle in radians between two vectors, taking 0,0 as center
        public static float AngleBetween(Vector2 lhs, Vector2 rhs)
        {
            return AngleBetweenNormalized(lhs.normalized, rhs.normalized);
        }
        public static float AngleBetweenDeg(Vector2 lhs, Vector2 rhs)
        {
            return AngleBetweenNormalized(lhs.normalized, rhs.normalized) * Mathf.Rad2Deg;
        }
        // Same as AngleBetween, but assumes vec is already normalized. Results may be skewed if not.
        public static float AngleBetweenNormalized(Vector2 lhs, Vector2 rhs)
        {
            return Mathf.Acos(Mathf.Clamp(Vector2.Dot(lhs, rhs), -1, 1)) * (Vector2.Dot(rhs, new Vector2(lhs.y, -lhs.x)) >= 0 ? 1 : -1);
        }
        public static float AngleBetweenNormalizedDeg(Vector2 lhs, Vector2 rhs)
        {
            return AngleBetweenNormalized(lhs, rhs) * Mathf.Rad2Deg;
        }

        // Project a vector2 along a surface
        public static Vector3 ProjectAlongSurface(Vector2 dir, Vector3 normal)
        {
            float len = dir.magnitude;
            if (len > 0)
            {
                Vector3 perp = Vector3.Cross(dir.Horizontal3D(), Vector3.up);
                return Vector3.Cross(normal, perp).normalized * dir.magnitude;
            }
            return Vector3.zero;
        }

        // Modulo that ensures the result is always positive (e.g. ModAbs(-45, 360) = 315)
        public static float ModAbs(float f, float m) => ((f % m) + m) % m;

        // Add a value and clamp the result between +range and -range
        // If value is already outside of range, it will discard the addition
        // if the result does not end up being closer to range.
        // Range must be positive
        public static float AddClamped(float value, float add, float range)
        {
            if (add < 0)
            {
                if (value > -range)
                {
                    value += add;

                    if (value < -range)
                    {
                        value = -range;
                    }
                }
            }
            else if (add > 0)
            {
                if (value < range)
                {
                    value += add;

                    if (value > range)
                    {
                        value = range;
                    }
                }
            }

            return value;
        }
        public static Vector2 AddClamped(Vector2 value, Vector2 add, Vector2 range)
        {
            value.x = AddClamped(value.x, add.x, range.x);
            value.y = AddClamped(value.y, add.y, range.y);
            return value;
        }
        public static Vector3 AddClamped(Vector3 value, Vector3 add, Vector3 range)
        {
            value.x = AddClamped(value.x, add.x, range.x);
            value.y = AddClamped(value.y, add.y, range.y);
            value.z = AddClamped(value.z, add.z, range.z);
            return value;
        }
        public static Vector4 AddClamped(Vector4 value, Vector4 add, Vector4 range)
        {
            value.x = AddClamped(value.x, add.x, range.x);
            value.y = AddClamped(value.y, add.y, range.y);
            value.z = AddClamped(value.z, add.z, range.z);
            value.w = AddClamped(value.w, add.w, range.w);
            return value;
        }

        // Sample a spline curve with two control points at time t
        public static Vector2 CatmullRomSpline(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float tinv = (1 - t);
            float tinv2 = tinv * tinv;
            float tinv3 = tinv2 * tinv;
            float t2 = t * t;
            float t3 = t2 * t;

            Vector2 c0 = p0 * tinv3;
            Vector2 c1 = p1 * t * tinv2 * 3;
            Vector2 c2 = p2 * t2 * tinv * 3;
            Vector2 c3 = p3 * t3;

            return c0 + c1 + c2 + c3;
        }
        public static Vector2 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float tinv = (1 - t);
            float tinv2 = tinv * tinv;
            float tinv3 = tinv2 * tinv;
            float t2 = t * t;
            float t3 = t2 * t;

            Vector3 c0 = p0 * tinv3;
            Vector3 c1 = p1 * t * tinv2 * 3;
            Vector3 c2 = p2 * t2 * tinv * 3;
            Vector3 c3 = p3 * t3;

            return c0 + c1 + c2 + c3;
        }

        // Clamp between zero and infinity
        public static float Clamp0(float f)
        {
            return f < 0.0f ? 0.0f : f;
        }
        public static int Clamp0(int i)
        {
            return i < 0 ? 0 : i;
        }

        // Circle overlap check
        public static bool IsWithinRadius(Vector2 a, Vector2 b, float radius)
        {
            return (a - b).sqrMagnitude <= radius * radius;
        }

        // Sphere overlap check
        public static bool IsWithinRadius(Vector3 a, Vector3 b, float radius)
        {
            return (a - b).sqrMagnitude <= radius * radius;
        }
        public static bool IsWithinRadius(Transform a, Vector3 b, float radius)
        {
            return (a.position - b).sqrMagnitude <= radius * radius;
        }
        public static bool IsWithinRadius(Vector3 a, Transform b, float radius)
        {
            return (a - b.position).sqrMagnitude <= radius * radius;
        }
        public static bool IsWithinRadius(Transform a, Transform b, float radius)
        {
            return (a.position - b.position).sqrMagnitude <= radius * radius;
        }
    }
}
