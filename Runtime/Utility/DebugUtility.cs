using System;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    public static class DebugUtility
    {
        private const int CircleVertexCount = 64;
        private static Vector3[] circleVertices = null;
        internal static ReadOnlySpan<Vector3> CircleVertices
        {
            get
            {
                if (circleVertices == null)
                {
                    const float step = Mathf.PI * 2 * (1.0f / CircleVertexCount);
                    circleVertices = new Vector3[CircleVertexCount];
                    float f = 0;
                    for (int i = 0; i < CircleVertexCount; i++, f += step)
                    {
                        circleVertices[i] = new Vector3(Mathf.Sin(f), 0, Mathf.Cos(f));
                    }
                }
                return circleVertices;
            }
        }

        private static void DrawCircleVertices(Vector3 position, Quaternion rotation, float radius, Color color, float duration, bool depthTest, int vertCount = CircleVertexCount)
        {
            ReadOnlySpan<Vector3> coords = CircleVertices;
            Vector3 p0 = position + rotation * (coords[0] * radius);
            for (int i = 1; i <= vertCount; i++)
            {
                Vector3 p1 = position + rotation * (coords[i % CircleVertexCount] * radius);
                Debug.DrawLine(p0, p1, color, duration, depthTest);
                p0 = p1;
            }
        }

        public static void DrawWireCircle(Vector3 position, Quaternion rotation, float radius, Color color, float duration = 0, bool depthTest = true)
        {
            if (radius > 0)
            {
                DrawCircleVertices(position, rotation, radius, color, duration, depthTest);
            }
        }
        public static void DrawWireCircle(Vector3 position, Quaternion rotation, float radius, float duration = 0, bool depthTest = true)
        {
            if (radius > 0)
            {
                DrawCircleVertices(position, rotation, radius, Color.white, duration, depthTest);
            }
        }

        public static void DrawWireSphere(Vector3 position, Quaternion rotation, float radius, Color color, float duration = 0, bool depthTest = true)
        {
            if (radius > 0)
            {
                DrawCircleVertices(position, rotation, radius, color, duration, depthTest);
                rotation *= Quaternion.Euler(90, 0, 0);
                DrawCircleVertices(position, rotation, radius, color, duration, depthTest);
                rotation *= Quaternion.Euler(0, 0, 90);
                DrawCircleVertices(position, rotation, radius, color, duration, depthTest);
            }
        }
        public static void DrawWireSphere(Vector3 position, Quaternion rotation, float radius, float duration = 0, bool depthTest = true)
        {
            DrawWireSphere(position, rotation, radius, Color.white, duration, depthTest);
        }

        public static void DrawWireCapsule(Vector3 position, Quaternion rotation, float height, float radius, Color color, float duration = 0, bool depthTest = true)
        {
            if (radius > 0)
            {
                float doubleRadius = radius * 2;
                bool drawEdges = height > doubleRadius;
                if (!drawEdges)
                {
                    height = doubleRadius;
                }
                Vector3 dir = rotation * new Vector3(0, height * 0.5f - radius, 0);

                Vector3 p0 = position + dir;
                Quaternion r0 = rotation;
                DrawCircleVertices(p0, r0, radius, color, duration, depthTest);
                r0 *= Quaternion.Euler(0, 0, 90);
                DrawCircleVertices(p0, r0, radius, color, duration, depthTest, CircleVertexCount >> 1);
                r0 *= Quaternion.Euler(90, 0, 0);
                DrawCircleVertices(p0, r0, radius, color, duration, depthTest, CircleVertexCount >> 1);

                Vector3 p1 = position - dir;
                Quaternion r1 = rotation;
                if (drawEdges)
                {
                    DrawCircleVertices(p1, r1, radius, color, duration, depthTest);
                }
                r1 *= Quaternion.Euler(0, 0, 270);
                DrawCircleVertices(p1, r1, radius, color, duration, depthTest, CircleVertexCount >> 1);
                r1 *= Quaternion.Euler(90, 0, 0);
                DrawCircleVertices(p1, r1, radius, color, duration, depthTest, CircleVertexCount >> 1);

                if (drawEdges)
                {
                    Vector3 offset = rotation * new Vector3(0, 0, radius);
                    Debug.DrawLine(p0 + offset, p1 + offset, color, duration, depthTest);
                    offset = rotation * new Vector3(0, 0, -radius);
                    Debug.DrawLine(p0 + offset, p1 + offset, color, duration, depthTest);
                    offset = rotation * new Vector3(radius, 0, 0);
                    Debug.DrawLine(p0 + offset, p1 + offset, color, duration, depthTest);
                    offset = rotation * new Vector3(-radius, 0, 0);
                    Debug.DrawLine(p0 + offset, p1 + offset, color, duration, depthTest);
                }
            }
            else if (height > 0)
            {
                Vector3 dir = rotation * new Vector3(0, height * 0.5f, 0);
                Debug.DrawLine(position + dir, position - dir, color, duration, depthTest);
            }
        }
        public static void DrawWireCapsule(Vector3 position, Quaternion rotation, float height, float radius, float duration = 0, bool depthTest = true)
        {
            DrawWireCapsule(position, rotation, height, radius, Color.white, duration, depthTest);
        }

        public static void DrawRectangle(Vector3 position, Quaternion rotation, Vector2 size, Color color, float duration = 0, bool depthTest = true)
        {
            Vector3 fwd = rotation * new Vector3(0, 0, size.y);
            Vector3 right = rotation * new Vector3(size.x, 0, 0);
            Vector3 p0 = position - (fwd + right) * 0.5f;
            Vector3 p1 = p0 + fwd;
            Vector3 p2 = p1 + right;
            Vector3 p3 = p2 - fwd;
            Debug.DrawLine(p0, p1, color, duration, depthTest);
            Debug.DrawLine(p1, p2, color, duration, depthTest);
            Debug.DrawLine(p2, p3, color, duration, depthTest);
            Debug.DrawLine(p3, p0, color, duration, depthTest);
        }
        public static void DrawRectangle(Vector3 position, Quaternion rotation, Vector2 size, float duration = 0, bool depthTest = true)
        {
            DrawRectangle(position, rotation, size, Color.white, duration, depthTest);
        }
    }

    public static class GizmoUtility
    {
        public static void DrawCircle(Vector3 position, Quaternion rotation, float radius)
        {
            ReadOnlySpan<Vector3> coords = DebugUtility.CircleVertices;
            Span<Vector3> transformed = stackalloc Vector3[coords.Length * 2];
            Vector3 p0 = position + rotation * (coords[^1] * radius);
            for (int i = 0, j = 0; i < coords.Length; i++)
            {
                Vector3 p1 = position + rotation * (coords[i] * radius);
                transformed[j++] = p0;
                transformed[j++] = p1;
                p0 = p1;
            }
            Gizmos.DrawLineList(transformed);
        }

        public static void DrawRectangle(Vector3 position, Quaternion rotation, Vector2 size)
        {
            Vector3 fwd = rotation * new Vector3(0, 0, size.y);
            Vector3 right = rotation * new Vector3(size.x, 0, 0);
            Vector3 p0 = position - (fwd + right) * 0.5f;
            Vector3 p1 = p0 + fwd;
            Vector3 p2 = p1 + right;
            Vector3 p3 = p2 - fwd;
            ReadOnlySpan<Vector3> points = stackalloc Vector3[8]
            {
                p0, p1, p1, p2, p2, p3, p3, p0,
            };
            Gizmos.DrawLineList(points);
        }

        public static void DrawBounds(Bounds bounds, Quaternion rotation)
        {
            Vector3 right = rotation * new Vector3(bounds.extents.x * 2, 0, 0);
            Vector3 up = rotation * new Vector3(0, bounds.extents.y * 2, 0);
            Vector3 fwd = rotation * new Vector3(0, 0, bounds.extents.z * 2);
            Vector3 p0 = bounds.center - (fwd + right + up) * 0.5f;
            Vector3 p1 = p0 + fwd;
            Vector3 p2 = p1 + right;
            Vector3 p3 = p2 - fwd;
            Vector3 p4 = p0 + up;
            Vector3 p5 = p1 + up;
            Vector3 p6 = p2 + up;
            Vector3 p7 = p3 + up;
            ReadOnlySpan<Vector3> points = stackalloc Vector3[24]
            {
                p0, p1, p1, p2, p2, p3, p3, p0,
                p4, p5, p5, p6, p6, p7, p7, p4,
                p0, p4, p1, p5, p2, p6, p3, p7,
            };
            Gizmos.DrawLineList(points);
        }
    }
}