using System;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Dungeon Finder CMSG codecs.
//
// Three of these carry no payload at all — the client polls join status and the LFG list, and
// leaves the queue, with a bare opcode. The interesting ones are DFJoin, whose bit block and slot
// array must be read in a precise order, and DFProposalResponse, which nests a RideTicket.

public static class DFGetJoinStatusPktCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DFGetJoinStatusPkt packet)
        => packet = default;
}

public static class DFLeavePktCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DFLeavePkt packet)
        => packet = default;
}

public static class LFGListGetStatusPktCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LFGListGetStatusPkt packet)
        => packet = default;
}

public static class DFGetSystemInfoPktCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DFGetSystemInfoPkt packet)
        => packet = new DFGetSystemInfoPkt(r.HasBit());
}

public static class DFTeleportPktCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DFTeleportPkt packet)
        => packet = new DFTeleportPkt(r.HasBit());
}

public static class DFSetRolesPktCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DFSetRolesPkt packet)
        => packet = new DFSetRolesPkt(r.ReadUInt8());
}

public static class DFJoinPktCodec
{
    /// <remarks>
    /// Three bits, then a byte, then the slot count, then the optional party index, and only then
    /// the slots — the party-index byte sits between the count and the array it sizes, so reading
    /// it late shifts every slot.
    /// <para>
    /// The count is wire data and each slot is four bytes, so a corrupt count would reserve
    /// gigabytes here before the element loop could fail. The array is the packet field and cannot
    /// be clamped without silently dropping dungeons, so it guards instead — the same treatment
    /// QuestPOIQueryCodec gets, and for the same reason.
    /// </para>
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out DFJoinPkt packet)
    {
        bool queueAsGroup = r.HasBit();
        bool hasPartyIndex = r.HasBit();
        r.HasBit(); // Mercenary
        byte roles = r.ReadUInt8();
        uint slotCount = r.ReadUInt32();
        if (hasPartyIndex)
            r.ReadUInt8();

        if (slotCount > r.Remaining / sizeof(uint))
            throw new ArgumentOutOfRangeException(nameof(slotCount), slotCount,
                $"CMSG_DF_JOIN claims {slotCount} dungeon slots but only {r.Remaining} bytes remain.");

        uint[] slots = new uint[slotCount];
        for (int i = 0; i < slotCount; i++)
            slots[i] = r.ReadUInt32();

        packet = new DFJoinPkt(queueAsGroup, roles, slots);
    }
}

public static class DFProposalResponsePktCodec
{
    /// <remarks>
    /// RideTicket.Read owns the V3_4_3 trailing Unknown925 bit and the byte-align that follows it.
    /// Consuming a second one here over-read the buffer by a byte and made the final Accepted bit
    /// throw, crashing the proxy on every proposal reply — issue #103.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out DFProposalResponsePkt packet)
    {
        var ticket = new RideTicket();
        ticket.Read(ref r);
        ulong instanceId = r.ReadUInt64();
        uint proposalId = r.ReadUInt32();
        packet = new DFProposalResponsePkt(ticket, instanceId, proposalId, r.HasBit());
    }
}
