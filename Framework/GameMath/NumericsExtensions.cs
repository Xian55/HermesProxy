using System;
using System.Numerics;

namespace Framework.GameMath;

/// <summary>
/// Extension methods for System.Numerics types to provide game-specific functionality.
/// </summary>
public static class NumericsExtensions
{
    #region Quaternion Extensions

    /// <summary>
    /// Creates a quaternion from a WoW packed rotation format (long).
    /// </summary>
    public static Quaternion FromPackedLong(long packed)
    {
        float x = (packed >> 42) * (1.0f / 2097152.0f);
        float y = (((packed << 22) >> 32) >> 11) * (1.0f / 1048576.0f);
        float z = (packed << 43 >> 43) * (1.0f / 1048576.0f);

        float w = x * x + y * y + z * z;
        if (MathF.Abs(w - 1.0f) >= (1 / 1048576.0f))
            w = MathF.Sqrt(1.0f - w);
        else
            w = 0.0f;

        return new Quaternion(x, y, z, w);
    }

    /// <summary>
    /// Packs a quaternion into WoW's packed rotation format (long).
    /// </summary>
    public static long GetPackedRotation(this Quaternion q)
    {
        const int PACK_YZ = 1 << 20;
        const int PACK_X = PACK_YZ << 1;

        const int PACK_YZ_MASK = (PACK_YZ << 1) - 1;
        const int PACK_X_MASK = (PACK_X << 1) - 1;

        sbyte w_sign = (sbyte)(q.W >= 0.0f ? 1 : -1);
        long x = (int)(q.X * PACK_X) * w_sign & PACK_X_MASK;
        long y = (int)(q.Y * PACK_YZ) * w_sign & PACK_YZ_MASK;
        long z = (int)(q.Z * PACK_YZ) * w_sign & PACK_YZ_MASK;
        return (z | (y << 21) | (x << 42));
    }

    /// <summary>
    /// Converts a quaternion to Euler angles (roll, pitch, yaw).
    /// </summary>
    public static EulerAngles AsEulerAngles(this Quaternion q)
    {
        var angles = new EulerAngles();

        // roll / x
        float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.Roll = MathF.Atan2(sinr_cosp, cosr_cosp);

        // pitch / y
        float sinp = 2 * (q.W * q.Y - q.Z * q.X);
        if (MathF.Abs(sinp) >= 1)
        {
            angles.Pitch = MathF.CopySign(MathF.PI / 2, sinp);
        }
        else
        {
            angles.Pitch = MathF.Asin(sinp);
        }

        // yaw / z
        float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Yaw = MathF.Atan2(siny_cosp, cosy_cosp);

        return angles;
    }

    #endregion

    #region Vector3 Extensions

    #endregion
}

public enum Axis { X = 0, Y = 1, Z = 2, Detect = -1 }
