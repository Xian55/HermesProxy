using System;
using Framework.IO;

namespace HermesProxy.World.Objects;

/// <summary>
/// Bit flags for SMSG_UPDATE_OBJECT create block movement data.
/// Bit order must match wire format.
/// Reference: https://github.com/TrinityCore/WowPacketParser
/// </summary>
[Flags]
public enum CreateObjectBits : uint
{
    None              = 0,
    NoBirthAnim       = 1 << 0,
    EnablePortals     = 1 << 1,
    PlayHoverAnim     = 1 << 2,
    MovementUpdate    = 1 << 3,
    MovementTransport = 1 << 4,
    Stationary        = 1 << 5,
    CombatVictim      = 1 << 6,
    ServerTime        = 1 << 7,
    Vehicle           = 1 << 8,
    AnimKit           = 1 << 9,
    Rotation          = 1 << 10,
    AreaTrigger       = 1 << 11,
    GameObject        = 1 << 12,
    SmoothPhasing     = 1 << 13,
    ThisIsYou         = 1 << 14,
    SceneObject       = 1 << 15,
    ActivePlayer      = 1 << 16,
    Conversation      = 1 << 17,
}

public static class CreateObjectBitsExtensions
{
    private const int BitCount = 18;

    public static void WriteCreateBits(this CreateObjectBits bits, ByteBuffer data)
    {
        for (int i = 0; i < BitCount; i++)
            data.WriteBit(((uint)bits & (1u << i)) != 0);

        data.FlushBits();
    }
}
