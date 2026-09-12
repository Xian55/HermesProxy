using System;
using System.Linq;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the quest codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// Every case asserts field values <b>and</b> the reader's final position, over readers built
/// through <c>GetRemainingSpan()</c>. The two packets that keep a reference field —
/// <c>QuestPOIQuery</c>'s array and <c>QuestGiverChooseReward</c>'s <c>QuestChoiceItem</c> — get
/// the most attention, because a count-driven loop and a three-level nested read are where a
/// hand-written span reader diverges from the oracle without the field values ever disagreeing.
/// </remarks>
public class QuestCodecEquivalenceTests
{
    private static (WorldPacket Oracle, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    private static readonly WowGuid128 Guid = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);

    // ---- GUID + quest id + a trailing bit ----

    [Theory]
    [InlineData(0u, true)]
    [InlineData(1234u, false)]
    [InlineData(uint.MaxValue, true)]
    public void QuestGiverQueryQuest_Matches(uint questId, bool respondToGiver)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(questId); w.WriteBit(respondToGiver); });
        var e = new Frozen.QuestGiverQueryQuest(); e.Read(o);
        var r = ReaderOver(f); QuestGiverQueryQuestCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestGiverGUID, a.QuestGiverGUID);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(e.RespondToGiver, a.RespondToGiver);
        Assert.Equal(respondToGiver, a.RespondToGiver);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(77u, true)]
    [InlineData(77u, false)]
    public void QuestGiverAcceptQuest_Matches(uint questId, bool startCheat)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(questId); w.WriteBit(startCheat); });
        var e = new Frozen.QuestGiverAcceptQuest(); e.Read(o);
        var r = ReaderOver(f); QuestGiverAcceptQuestCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestGiverGUID, a.QuestGiverGUID);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(e.StartCheat, a.StartCheat);
        Assert.Equal(startCheat, a.StartCheat);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(4242u, true)]
    [InlineData(4242u, false)]
    public void QuestGiverCompleteQuest_Matches(uint questId, bool fromScript)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(questId); w.WriteBit(fromScript); });
        var e = new Frozen.QuestGiverCompleteQuest(); e.Read(o);
        var r = ReaderOver(f); QuestGiverCompleteQuestCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestGiverGUID, a.QuestGiverGUID);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(e.FromScript, a.FromScript);
        Assert.Equal(fromScript, a.FromScript);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- single-field and GUID-only ----

    [Fact]
    public void QuestGiverStatusQuery_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.QuestGiverStatusQuery(); e.Read(o);
        var r = ReaderOver(f); QuestGiverStatusQueryCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestGiverGUID, a.QuestGiverGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void QuestGiverHello_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.QuestGiverHello(); e.Read(o);
        var r = ReaderOver(f); QuestGiverHelloCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestGiverGUID, a.QuestGiverGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void CloseInteraction_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.CloseInteraction(); e.Read(o);
        var r = ReaderOver(f); CloseInteractionCodec.Read(ref r, out var a);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)24)]
    [InlineData(byte.MaxValue)]
    public void QuestLogRemoveQuest_Matches(byte slot)
    {
        var (o, f) = Build(w => w.WriteUInt8(slot));
        var e = new Frozen.QuestLogRemoveQuest(); e.Read(o);
        var r = ReaderOver(f); QuestLogRemoveQuestCodec.Read(ref r, out var a);
        Assert.Equal(e.Slot, a.Slot);
        Assert.Equal(slot, a.Slot);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// The quest id here is signed where every other quest packet has it unsigned; the handler
    /// casts it back. A codec that "tidied" it to uint would change the log line and the cast.
    [Theory]
    [InlineData(0)]
    [InlineData(9999)]
    [InlineData(int.MinValue)]
    public void QuestGiverCloseQuest_Matches(int questId)
    {
        var (o, f) = Build(w => w.WriteInt32(questId));
        var e = new Frozen.QuestGiverCloseQuest(); e.Read(o);
        var r = ReaderOver(f); QuestGiverCloseQuestCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(questId, a.QuestID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1234u)]
    public void QuestConfirmAcceptResponse_Matches(uint questId)
    {
        var (o, f) = Build(w => w.WriteUInt32(questId));
        var e = new Frozen.QuestConfirmAcceptResponse(); e.Read(o);
        var r = ReaderOver(f); QuestConfirmAcceptResponseCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1234u)]
    public void PushQuestToParty_Matches(uint questId)
    {
        var (o, f) = Build(w => w.WriteUInt32(questId));
        var e = new Frozen.PushQuestToParty(); e.Read(o);
        var r = ReaderOver(f); PushQuestToPartyCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void QuestGiverRequestReward_Matches()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(555); });
        var e = new Frozen.QuestGiverRequestReward(); e.Read(o);
        var r = ReaderOver(f); QuestGiverRequestRewardCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestGiverGUID, a.QuestGiverGUID);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(QuestPushReason.Success)]
    [InlineData(QuestPushReason.OnQuest)]
    public void QuestPushResultResponse_Matches(QuestPushReason result)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(321); w.WriteUInt8((byte)result); });
        var e = new Frozen.QuestPushResultResponse(); e.Read(o);
        var r = ReaderOver(f); QuestPushResultResponseCodec.Read(ref r, out var a);
        Assert.Equal(e.SenderGUID, a.SenderGUID);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(e.Result, a.Result);
        Assert.Equal(result, a.Result);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- the count-driven array ----

    /// <summary>
    /// Only the populated prefix is on the wire. Zero is the interesting case: the handler writes
    /// <c>MissingQuestPOIs.Length</c> straight back out, so a codec that left the array null would
    /// throw rather than forward an empty query.
    /// </summary>
    [Theory]
    [InlineData(new int[0])]
    [InlineData(new[] { 42 })]
    [InlineData(new[] { 1, 2, 3, int.MaxValue, int.MinValue })]
    public void QuestPOIQuery_Matches(int[] questIds)
    {
        var (o, f) = Build(w =>
        {
            w.WriteInt32(questIds.Length);
            foreach (int id in questIds)
                w.WriteInt32(id);
        });

        var e = new Frozen.QuestPOIQuery(); e.Read(o);
        var r = ReaderOver(f); QuestPOIQueryCodec.Read(ref r, out var a);
        Assert.NotNull(a.MissingQuestPOIs);
        Assert.Equal(e.MissingQuestPOIs, a.MissingQuestPOIs);
        Assert.Equal(questIds, a.MissingQuestPOIs);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- the nested reward choice ----

    private static void WriteChoice(WorldPacket w, uint itemId, uint seed, uint propertiesId,
                                    (uint Value, ItemModifier Type)[] mods, uint[]? bonusIds, uint quantity)
    {
        w.WriteBits(1u, 2);                       // LootItemType
        w.WriteUInt32(itemId);                    // ItemInstance
        w.WriteUInt32(seed);
        w.WriteUInt32(propertiesId);
        w.WriteBit(bonusIds != null);
        w.FlushBits();
        w.WriteBits((uint)mods.Length, 6);        // ItemModList
        w.FlushBits();
        foreach (var (value, type) in mods)
        {
            w.WriteUInt32(value);
            w.WriteUInt8((byte)type);
        }
        if (bonusIds != null)                     // ItemBonuses
        {
            w.WriteUInt8((byte)ItemContext.None);
            w.WriteUInt32((uint)bonusIds.Length);
            foreach (uint id in bonusIds)
                w.WriteUInt32(id);
        }
        w.WriteUInt32(quantity);
    }

    /// <summary>
    /// The whole <c>QuestChoiceItem</c> → <c>ItemInstance</c> → <c>ItemModList</c>/<c>ItemBonuses</c>
    /// chain, which is where the new span overloads earn their keep. The optional bonus block and a
    /// non-empty modifier list are both exercised, because each changes how many bytes precede
    /// <c>Quantity</c> — and Quantity landing in the wrong place is invisible to a field-only
    /// assertion.
    /// </summary>
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 3)]
    [InlineData(true, 0)]
    [InlineData(true, 2)]
    public void QuestGiverChooseReward_Matches(bool withBonuses, int modCount)
    {
        var mods = Enumerable.Range(0, modCount)
            .Select(i => ((uint)(100 + i), (ItemModifier)(byte)i))
            .ToArray();
        uint[]? bonusIds = withBonuses ? [11u, 22u] : null;

        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(880);
            WriteChoice(w, itemId: 12345, seed: 777, propertiesId: 88, mods, bonusIds, quantity: 7);
        });

        var e = new Frozen.QuestGiverChooseReward(); e.Read(o);
        var r = ReaderOver(f); QuestGiverChooseRewardCodec.Read(ref r, out var a);

        Assert.Equal(e.QuestGiverGUID, a.QuestGiverGUID);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(e.Choice.LootItemType, a.Choice.LootItemType);
        Assert.Equal(e.Choice.Quantity, a.Choice.Quantity);
        Assert.Equal(7u, a.Choice.Quantity);
        Assert.Equal(e.Choice.Item.ItemID, a.Choice.Item.ItemID);
        Assert.Equal(12345u, a.Choice.Item.ItemID);
        Assert.Equal(e.Choice.Item.RandomPropertiesSeed, a.Choice.Item.RandomPropertiesSeed);
        Assert.Equal(e.Choice.Item.RandomPropertiesID, a.Choice.Item.RandomPropertiesID);

        Assert.Equal(e.Choice.Item.Modifications.Values.Count, a.Choice.Item.Modifications.Values.Count);
        for (int i = 0; i < modCount; i++)
        {
            Assert.Equal(e.Choice.Item.Modifications.Values[i].Value, a.Choice.Item.Modifications.Values[i].Value);
            Assert.Equal(e.Choice.Item.Modifications.Values[i].Type, a.Choice.Item.Modifications.Values[i].Type);
        }

        Assert.Equal(e.Choice.Item.ItemBonus == null, a.Choice.Item.ItemBonus == null);
        if (withBonuses)
            Assert.Equal(e.Choice.Item.ItemBonus!.BonusListIDs, a.Choice.Item.ItemBonus!.BonusListIDs);

        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
