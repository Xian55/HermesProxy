using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Misc CMSG codecs. See QueryCodecs.cs for why every field goes through a local first.

public static class TimeSyncResponseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out TimeSyncResponse packet)
    {
        uint sequenceIndex = r.ReadUInt32();
        uint clientTime = r.ReadUInt32();
        packet = new TimeSyncResponse(sequenceIndex, clientTime);
    }
}

public static class AreaTriggerPktCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out AreaTriggerPkt packet)
    {
        uint areaTriggerId = r.ReadUInt32();
        bool entered = r.HasBit();
        bool fromClient = r.HasBit();
        packet = new AreaTriggerPkt(areaTriggerId, entered, fromClient);
    }
}

public static class SetSelectionCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetSelection packet)
        => packet = new SetSelection(r.ReadPackedGuid128());
}

public static class RepopRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RepopRequest packet)
        => packet = new RepopRequest(r.HasBit());
}

public static class QueryCorpseLocationFromClientCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryCorpseLocationFromClient packet)
        => packet = new QueryCorpseLocationFromClient(r.ReadPackedGuid128());
}

public static class ReclaimCorpseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ReclaimCorpse packet)
        => packet = new ReclaimCorpse(r.ReadPackedGuid128());
}

public static class StandStateChangeCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out StandStateChange packet)
        => packet = new StandStateChange(r.ReadUInt32());
}

public static class ClientCinematicPktCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ClientCinematicPkt packet)
        => packet = default;
}

public static class FarSightCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out FarSight packet)
        => packet = new FarSight(r.HasBit());
}

public static class TutorialSetFlagCodec
{
    public static void Read(ref SpanPacketReader r, out TutorialSetFlag packet)
    {
        // TutorialBit is only on the wire for Update. On the other actions the field stays at its
        // default, which was 0 on the class this replaced and is 0 in the struct — so the skip is
        // behaviour-preserving, but it is a skip, and skips are where conversions go wrong.
        var action = (TutorialAction)r.ReadBits<byte>(2);
        uint tutorialBit = action == TutorialAction.Update ? r.ReadUInt32() : 0u;
        packet = new TutorialSetFlag(action, tutorialBit);
    }
}

public static class ObjectUpdateFailedCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ObjectUpdateFailed packet)
        => packet = new ObjectUpdateFailed(r.ReadPackedGuid128());
}

public static class SetDungeonDifficultyCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetDungeonDifficulty packet)
        => packet = new SetDungeonDifficulty(r.ReadUInt32());
}

public static class SetRaidDifficultyCodec
{
    public static void Read(ref SpanPacketReader r, out SetRaidDifficulty packet)
    {
        // Legacy is an optional trailing byte — older clients simply stop after DifficultyID.
        // CanRead here means the same thing ByteBuffer.CanRead() did: position < end of payload.
        // That equivalence only holds because the reader is built from GetRemainingSpan(); over
        // GetDataSpan() the extra opcode bytes would make an absent Legacy look present.
        int difficultyId = r.ReadInt32();
        byte legacy = r.CanRead ? r.ReadUInt8() : (byte)0;
        packet = new SetRaidDifficulty(difficultyId, legacy);
    }
}
