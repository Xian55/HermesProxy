using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// The last of the reflective CMSGs: talents, game objects, hotfix queries, who, mount special,
// instance locks and the two session/bnet opcodes.

public static class LearnPetTalentCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LearnPetTalent packet)
    {
        WowGuid128 petGuid = r.ReadPackedGuid128();
        uint talentId = r.ReadUInt32();
        packet = new LearnPetTalent(petGuid, talentId, r.ReadUInt16());
    }
}

public static class RemoveGlyphCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RemoveGlyph packet)
        => packet = new RemoveGlyph(r.ReadUInt8());
}

public static class GameObjUseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GameObjUse packet)
        => packet = new GameObjUse(r.ReadPackedGuid128());
}

public static class GameObjReportUseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GameObjReportUse packet)
        => packet = new GameObjReportUse(r.ReadPackedGuid128());
}

public static class DBQueryBulkCodec
{
    /// <remarks>
    /// The count is 13 bits, so it tops out at 8191 — small enough to believe, but the ids are
    /// fixed-width so it is still clamped against what is actually left in the packet.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DBQueryBulk packet)
    {
        var tableHash = (DB2Hash)r.ReadUInt32();
        uint count = r.ReadBits<uint>(13);

        int capacity = CodecHelpers.WireCountCapacity(count, in r, sizeof(uint));
        var queries = new List<uint>(capacity);
        for (uint i = 0; i < count; ++i)
            queries.Add(r.ReadUInt32());

        packet = new DBQueryBulk(tableHash, queries);
    }
}

public static class HotfixRequestCodec
{
    /// <remarks>A full uint32 count here, so the pre-size must be clamped or a bogus one reserves
    /// gigabytes before the first read fails.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out HotfixRequest packet)
    {
        uint clientBuild = r.ReadUInt32();
        uint dataBuild = r.ReadUInt32();
        uint count = r.ReadUInt32();

        int capacity = CodecHelpers.WireCountCapacity(count, in r, sizeof(uint));
        var hotfixes = new List<uint>(capacity);
        for (uint i = 0; i < count; ++i)
            hotfixes.Add(r.ReadUInt32());

        packet = new HotfixRequest(clientBuild, dataBuild, hotfixes);
    }
}

public static class WhoRequestPktCodec
{
    /// <remarks>
    /// The 4-bit area count is read first and the areas last, with the whole WhoRequest block and
    /// the request id in between.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out WhoRequestPkt packet)
    {
        uint areasCount = r.ReadBits<uint>(4);

        var request = new WhoRequest();
        request.Read(ref r);
        uint requestId = r.ReadUInt32();

        var areas = new List<int>((int)areasCount);
        for (int i = 0; i < areasCount; ++i)
            areas.Add(r.ReadInt32());

        packet = new WhoRequestPkt(request, requestId, areas);
    }
}

public static class MountSpecialCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MountSpecial packet)
    {
        uint count = r.ReadUInt32();
        int sequenceVariation = 0;
        if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
            sequenceVariation = r.ReadInt32();

        int capacity = CodecHelpers.WireCountCapacity(count, in r, sizeof(int));
        int[] kits = capacity != 0 ? new int[capacity] : Array.Empty<int>();
        for (int i = 0; i < kits.Length; ++i)
            kits[i] = r.ReadInt32();

        packet = new MountSpecial(kits, sequenceVariation);
    }
}

public static class InstanceLockResponseCodec
{
    /// <remarks>A bit, not a byte — the original body read it with HasBit.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out InstanceLockResponse packet)
        => packet = new InstanceLockResponse(r.HasBit());
}

public static class ChangeRealmTicketCodec
{
    /// <remarks>The secret is copied, not viewed — it is handed to the BNet RPC and outlives the packet.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ChangeRealmTicket packet)
    {
        uint token = r.ReadUInt32();
        packet = new ChangeRealmTicket(token, r.ReadBytes(32u));
    }
}

public static class BattlenetRequestCodec
{
    /// <remarks>The protobuf payload is copied for the same reason — it is dispatched onward.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BattlenetRequest packet)
    {
        MethodCall method = default;
        method.Read(ref r);

        uint protoSize = r.ReadUInt32();
        packet = new BattlenetRequest(method, r.ReadBytes(protoSize));
    }
}
