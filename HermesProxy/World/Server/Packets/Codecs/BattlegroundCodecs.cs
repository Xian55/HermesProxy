using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Battleground CMSG codecs.
//
// Three of the six carry no payload — status, PvP log and leave are all bare opcodes, and the
// system works out what to send from session state rather than from the packet.

public static class RequestBattlefieldStatusCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RequestBattlefieldStatus packet)
        => packet = default;
}

public static class PVPLogDataRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out PVPLogDataRequest packet)
        => packet = default;
}

public static class BattlefieldLeaveCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BattlefieldLeave packet)
        => packet = default;
}

public static class BattlefieldListRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BattlefieldListRequest packet)
        => packet = new BattlefieldListRequest(r.ReadInt32());
}

public static class BattlemasterJoinCodec
{
    /// <remarks>
    /// The queue id is an int64 carrying a 0x1F10_0000_0000_0000 tag; masking it off leaves the
    /// battlefield list id. The two blacklist entries sit between the roles byte and the
    /// battlemaster GUID, so skipping them would shift the GUID rather than fail.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out BattlemasterJoin packet)
    {
        long queueId = r.ReadInt64();
        uint battlefieldListId = (uint)(queueId & ~0x1F10000000000000);
        byte roles = r.ReadUInt8();

        BlacklistMaps blacklistMap = default;
        blacklistMap[0] = r.ReadInt32();
        blacklistMap[1] = r.ReadInt32();

        WowGuid128 battlemasterGuid = r.ReadPackedGuid128();
        int verification = r.ReadInt32();
        int battlefieldInstanceId = r.ReadInt32();

        packet = new BattlemasterJoin(battlefieldListId, roles, blacklistMap, battlemasterGuid,
            verification, battlefieldInstanceId, r.HasBit());
    }
}

public static class BattlefieldPortCodec
{
    /// <remarks>
    /// RideTicket.Read owns the V3_4_3 trailing Unknown925 bit and the byte-align after it. Without
    /// that, the AcceptedInvite bit below lands on the Unknown925 bit instead of the real accept bit
    /// in the following byte, so "Enter Battle" read as a decline and the player never entered the
    /// battleground that had popped — issue #102.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out BattlefieldPort packet)
    {
        var ticket = new RideTicket();
        ticket.Read(ref r);
        packet = new BattlefieldPort(ticket, r.HasBit());
    }
}
