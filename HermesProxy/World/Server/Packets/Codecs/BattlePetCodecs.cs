using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Battle-pet, mount and critter CMSG codecs. All flat.

public static class BattlePetSummonCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BattlePetSummon packet)
        => packet = new BattlePetSummon(r.ReadPackedGuid128());
}

public static class BattlePetSetFlagsCodec
{
    /// <remarks>
    /// Flags is uint16 and ControlType is two bits, per 3.4.3 / WPP V3_4_4. Retail TrinityCore and
    /// lineagedr both document a uint32 Flags here, which over-reads the seven-byte payload — guid
    /// 4, flags 2, control 1 — and throws.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BattlePetSetFlags packet)
    {
        WowGuid128 petGuid = r.ReadPackedGuid128();
        ushort flags = r.ReadUInt16();
        packet = new BattlePetSetFlags(petGuid, flags, (byte)r.ReadBits<uint>(2));
    }
}

public static class DismissCritterCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DismissCritter packet)
        => packet = new DismissCritter(r.ReadPackedGuid128());
}

public static class MountSetFavoriteCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MountSetFavorite packet)
    {
        uint mountSpellId = r.ReadUInt32();
        packet = new MountSetFavorite(mountSpellId, r.ReadBit());
    }
}
