using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Quest CMSG codecs.
//
// Two of these hold a reference rather than a value, and both for the same reason MovementInfo
// does: the type they name is a mutable builder on the *outbound* path, so converting it is
// outbound work this round defers. QuestPOIQuery keeps an int[] because the count is client-chosen,
// and QuestGiverChooseReward keeps QuestChoiceItem, which nests ItemInstance. The structs
// themselves cost nothing; the one allocation each survives until outbound lands.

public static class QuestGiverQueryQuestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestGiverQueryQuest packet)
    {
        WowGuid128 questGiverGuid = r.ReadPackedGuid128();
        uint questId = r.ReadUInt32();
        bool respondToGiver = r.HasBit();
        packet = new QuestGiverQueryQuest(questGiverGuid, questId, respondToGiver);
    }
}

public static class QuestGiverAcceptQuestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestGiverAcceptQuest packet)
    {
        WowGuid128 questGiverGuid = r.ReadPackedGuid128();
        uint questId = r.ReadUInt32();
        bool startCheat = r.HasBit();
        packet = new QuestGiverAcceptQuest(questGiverGuid, questId, startCheat);
    }
}

public static class QuestLogRemoveQuestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestLogRemoveQuest packet)
        => packet = new QuestLogRemoveQuest(r.ReadUInt8());
}

public static class QuestGiverStatusQueryCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestGiverStatusQuery packet)
        => packet = new QuestGiverStatusQuery(r.ReadPackedGuid128());
}

public static class QuestGiverHelloCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestGiverHello packet)
        => packet = new QuestGiverHello(r.ReadPackedGuid128());
}

public static class QuestGiverCloseQuestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestGiverCloseQuest packet)
        => packet = new QuestGiverCloseQuest(r.ReadInt32());
}

public static class CloseInteractionCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CloseInteraction packet)
        => packet = new CloseInteraction(r.ReadPackedGuid128());
}

public static class QuestPOIQueryCodec
{
    public static void Read(ref SpanPacketReader r, out QuestPOIQuery packet)
    {
        // Wire: int32 count, int32[count] questIds. CypherCore over-allocates a
        // 175-slot array but only reads `count` ints from the stream — only the
        // populated prefix is on the wire.
        int count = r.ReadInt32();
        int[] missingQuestPOIs = new int[count];
        for (int i = 0; i < count; i++)
            missingQuestPOIs[i] = r.ReadInt32();
        packet = new QuestPOIQuery(missingQuestPOIs);
    }
}

public static class QuestGiverRequestRewardCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestGiverRequestReward packet)
    {
        WowGuid128 questGiverGuid = r.ReadPackedGuid128();
        uint questId = r.ReadUInt32();
        packet = new QuestGiverRequestReward(questGiverGuid, questId);
    }
}

public static class QuestGiverChooseRewardCodec
{
    public static void Read(ref SpanPacketReader r, out QuestGiverChooseReward packet)
    {
        WowGuid128 questGiverGuid = r.ReadPackedGuid128();
        uint questId = r.ReadUInt32();
        var choice = new QuestChoiceItem();
        choice.Read(ref r);
        packet = new QuestGiverChooseReward(questGiverGuid, questId, choice);
    }
}

public static class QuestGiverCompleteQuestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestGiverCompleteQuest packet)
    {
        WowGuid128 questGiverGuid = r.ReadPackedGuid128();
        uint questId = r.ReadUInt32();
        bool fromScript = r.HasBit();
        packet = new QuestGiverCompleteQuest(questGiverGuid, questId, fromScript);
    }
}

public static class QuestConfirmAcceptResponseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestConfirmAcceptResponse packet)
        => packet = new QuestConfirmAcceptResponse(r.ReadUInt32());
}

public static class PushQuestToPartyCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out PushQuestToParty packet)
        => packet = new PushQuestToParty(r.ReadUInt32());
}

public static class QuestPushResultResponseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QuestPushResultResponse packet)
    {
        WowGuid128 senderGuid = r.ReadPackedGuid128();
        uint questId = r.ReadUInt32();
        var result = (QuestPushReason)r.ReadUInt8();
        packet = new QuestPushResultResponse(senderGuid, questId, result);
    }
}
