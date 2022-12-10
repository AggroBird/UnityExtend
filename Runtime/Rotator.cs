using UnityEngine;

namespace AggroBird.UnityEngineExtend
{
    // 2D euler rotation
    [System.Serializable]
    public struct Rotator2
    {
        public Rotator2(float pitch, float yaw)
        {
            this.pitch = pitch;
            this.yaw = yaw;
        }

        public float pitch;
        public float yaw;

        public static implicit operator Quaternion(Rotator2 rotator)
        {
            return Quaternion.Euler(rotator.pitch, rotator.yaw, 0);
        }
        public static implicit operator Vector3(Rotator2 rotator)
        {
            return new Vector3(rotator.pitch, rotator.yaw, 0);
        }

        public static implicit operator Rotator3(Rotator2 rotator)
        {
            return new Rotator3(rotator.pitch, rotator.yaw, 0);
        }
        public static Rotator3 FromEuler(Vector3 euler)
        {
            return new Rotator3(euler.x, euler.y, euler.z);
        }

        public static Rotator2 operator +(Rotator2 lhs, Rotator2 rhs)
        {
            return new Rotator2(lhs.pitch + rhs.pitch, lhs.yaw + rhs.yaw);
        }
        public static Rotator2 operator -(Rotator2 lhs, Rotator2 rhs)
        {
            return new Rotator2(lhs.pitch - rhs.pitch, lhs.yaw - rhs.yaw);
        }

        public static readonly Rotator2 zero = new Rotator2(0, 0);
    }

    // 3D euler rotation
    [System.Serializable]
    public struct Rotator3
    {
        public Rotator3(float pitch, float yaw, float roll = 0)
        {
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
        }

        public float pitch;
        public float yaw;
        public float roll;

        public static implicit operator Quaternion(Rotator3 rotator)
        {
            return Quaternion.Euler(rotator.pitch, rotator.yaw, rotator.roll);
        }
        public static implicit operator Vector3(Rotator3 rotator)
        {
            return new Vector3(rotator.pitch, rotator.yaw, rotator.roll);
        }

        public static implicit operator Rotator3(Quaternion quaternion)
        {
            return FromEuler(quaternion.eulerAngles);
        }
        public static Rotator3 FromEuler(Vector3 euler)
        {
            return new Rotator3(euler.x, euler.y, euler.z);
        }

        public static Rotator3 operator +(Rotator3 lhs, Rotator3 rhs)
        {
            return new Rotator3(lhs.pitch + rhs.pitch, lhs.yaw + rhs.yaw, lhs.roll - rhs.roll);
        }
        public static Rotator3 operator -(Rotator3 lhs, Rotator3 rhs)
        {
            return new Rotator3(lhs.pitch - rhs.pitch, lhs.yaw - rhs.yaw, lhs.roll - rhs.roll);
        }

        public static readonly Rotator3 zero = new Rotator3(0, 0);
    }

    public static class RotatorQuaternion
    {
        public static Rotator2 Rotator2D(this Quaternion quaternion)
        {
            Vector3 euler = quaternion.eulerAngles;
            return new Rotator2(euler.x, euler.y);
        }
        public static Rotator3 Rotator3D(this Quaternion quaternion)
        {
            Vector3 euler = quaternion.eulerAngles;
            return new Rotator3(euler.x, euler.y, euler.z);
        }
    }
}