using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Arena CMSG codecs.
//
// Flat reads throughout — the arena wire is unusually plain. What is not plain is the dispatch:
// three of the ten opcodes here share a body with another and forward the opcode they arrived with,
// so the universal-to-legacy table decides what the server actually does. ShapeBOpcodeForwardingTests
// pins those.

public static class ArenaTeamRosterRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ArenaTeamRosterRequest packet)
        => packet = new ArenaTeamRosterRequest(r.ReadUInt32());
}

public static class ArenaTeamQueryCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ArenaTeamQuery packet)
        => packet = new ArenaTeamQuery(r.ReadUInt32());
}

public static class RequestRatedPvpInfoCodec
{
    /// <remarks>No payload — the reader is not advanced.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RequestRatedPvpInfo packet)
        => packet = default;
}

public static class ArenaTeamLeaveCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ArenaTeamLeave packet)
        => packet = new ArenaTeamLeave(r.ReadUInt32());
}

public static class ArenaTeamRemoveCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ArenaTeamRemove packet)
    {
        uint teamId = r.ReadUInt32();
        packet = new ArenaTeamRemove(teamId, r.ReadPackedGuid128());
    }
}

public static class ArenaTeamAcceptCodec
{
    /// <remarks>Two packed GUIDs back to back — swapping them is silent, so the test uses distinct values.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ArenaTeamAccept packet)
    {
        WowGuid128 playerGuid = r.ReadPackedGuid128();
        packet = new ArenaTeamAccept(playerGuid, r.ReadPackedGuid128());
    }
}

public static class BattlemasterJoinArenaCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BattlemasterJoinArena packet)
    {
        WowGuid128 guid = r.ReadPackedGuid128();
        byte teamIndex = r.ReadUInt8();
        packet = new BattlemasterJoinArena(guid, teamIndex, r.ReadUInt8());
    }
}

public static class BattlemasterJoinSkirmishCodec
{
    /// <remarks>
    /// Roles precedes TeamSize — two adjacent bytes, and then two adjacent bits. Both pairs are
    /// orderings a reader can transpose without failing, so both are pinned.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BattlemasterJoinSkirmish packet)
    {
        WowGuid128 guid = r.ReadPackedGuid128();
        byte roles = r.ReadUInt8();
        byte teamSize = r.ReadUInt8();
        bool asGroup = r.HasBit();
        packet = new BattlemasterJoinSkirmish(guid, roles, teamSize, asGroup, r.HasBit());
    }
}
