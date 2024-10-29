using System;
using System.Globalization;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    // 2D euler rotation
    [Serializable]
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

        public static implicit operator Rotator3(Rotator2 rotator)
        {
            return new Rotator3(rotator.pitch, rotator.yaw, 0);
        }
        public static Rotator2 FromEuler(Vector2 euler)
        {
            return new Rotator2(euler.x, euler.y);
        }

        public static Rotator2 operator +(Rotator2 lhs, Rotator2 rhs)
        {
            return new(lhs.pitch + rhs.pitch, lhs.yaw + rhs.yaw);
        }
        public static Rotator2 operator -(Rotator2 lhs, Rotator2 rhs)
        {
            return new(lhs.pitch - rhs.pitch, lhs.yaw - rhs.yaw);
        }

        public static Rotator2 operator *(Rotator2 lhs, float f)
        {
            return new(lhs.pitch * f, lhs.yaw * f);
        }
        public static Rotator2 operator /(Rotator2 lhs, float f)
        {
            return new(lhs.pitch / f, lhs.yaw / f);
        }

        public static bool operator ==(Rotator2 lhs, Rotator2 rhs)
        {
            float pitch = lhs.pitch - rhs.pitch;
            float yaw = lhs.yaw - rhs.yaw;
            return pitch * pitch + yaw * yaw < 9.9999994E-11f;
        }
        public static bool operator !=(Rotator2 lhs, Rotator2 rhs)
        {
            return !(lhs == rhs);
        }

        public static Rotator2 operator -(Rotator2 rhs)
        {
            return new Rotator2(-rhs.pitch, -rhs.yaw);
        }

        public static readonly Rotator2 zero = new(0, 0);


        public override readonly int GetHashCode()
        {
            return pitch.GetHashCode() ^ (yaw.GetHashCode() << 2);
        }
        public override readonly bool Equals(object obj)
        {
            return obj is Rotator2 other && Equals(other);
        }
        public readonly bool Equals(Rotator2 other)
        {
            return pitch.Equals(other.pitch) && yaw.Equals(other.yaw);
        }

        public readonly override string ToString()
        {
            return ToString(null, null);
        }
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(format)) format = "F2";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            return $"({pitch.ToString(format, formatProvider)}, {yaw.ToString(format, formatProvider)})";
        }
    }

    // 3D euler rotation
    [Serializable]
    public struct Rotator3
    {
        public Rotator3(float pitch, float yaw, float roll)
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
            return new(lhs.pitch + rhs.pitch, lhs.yaw + rhs.yaw, lhs.roll - rhs.roll);
        }
        public static Rotator3 operator -(Rotator3 lhs, Rotator3 rhs)
        {
            return new(lhs.pitch - rhs.pitch, lhs.yaw - rhs.yaw, lhs.roll - rhs.roll);
        }

        public static Rotator3 operator *(Rotator3 lhs, float f)
        {
            return new(lhs.pitch * f, lhs.yaw * f, lhs.roll * f);
        }
        public static Rotator3 operator /(Rotator3 lhs, float f)
        {
            return new(lhs.pitch / f, lhs.yaw / f, lhs.roll / f);
        }

        public static bool operator ==(Rotator3 lhs, Rotator3 rhs)
        {
            float pitch = lhs.pitch - rhs.pitch;
            float yaw = lhs.yaw - rhs.yaw;
            float roll = lhs.roll - rhs.roll;
            return pitch * pitch + yaw * yaw + roll * roll < 9.9999994E-11f;
        }
        public static bool operator !=(Rotator3 lhs, Rotator3 rhs)
        {
            return !(lhs == rhs);
        }

        public static Rotator3 operator -(Rotator3 rhs)
        {
            return new Rotator3(-rhs.pitch, -rhs.yaw, -rhs.roll);
        }

        public static readonly Rotator3 zero = new(0, 0, 0);


        public override readonly int GetHashCode()
        {
            return pitch.GetHashCode() ^ (yaw.GetHashCode() << 2) ^ (roll.GetHashCode() >> 2);
        }
        public override readonly bool Equals(object obj)
        {
            return obj is Rotator3 other && Equals(other);
        }
        public readonly bool Equals(Rotator3 other)
        {
            return pitch.Equals(other.pitch) && yaw.Equals(other.yaw) && roll.Equals(other.roll);
        }

        public readonly override string ToString()
        {
            return ToString(null, null);
        }
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(format)) format = "F2";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            return $"({pitch.ToString(format, formatProvider)}, {yaw.ToString(format, formatProvider)}, {roll.ToString(format, formatProvider)})";
        }
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
