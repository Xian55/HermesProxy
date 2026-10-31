using System;
using System.Numerics;

namespace Framework.GameMath;

public struct EulerAngles
{
    // All values as radians
    public float Roll;     // x
    public float Pitch;    // y
    public float Yaw;      // z

    public Quaternion AsQuaternion()
    {
        (float sy, float cy) = MathF.SinCos(Yaw * 0.5f);
        (float sp, float cp) = MathF.SinCos(Pitch * 0.5f);
        (float sr, float cr) = MathF.SinCos(Roll * 0.5f);

        return new Quaternion(
            sr * cp * cy - cr * sp * sy,  // X
            cr * sp * cy + sr * cp * sy,  // Y
            cr * cp * sy - sr * sp * cy,  // Z
            cr * cp * cy + sr * sp * sy   // W
        );
    }
}
