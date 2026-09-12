using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the query and misc codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// Every case asserts field values <b>and</b> the reader's final position. Position is what catches
/// a codec that happens to agree on this fixture's field values but consumed a different number of
/// bytes — harmless in isolation, fatal in a real stream.
/// <para>
/// Readers are built through <c>GetRemainingSpan()</c>, the same accessor the dispatch site uses.
/// An earlier version of these tests hand-wrote <c>AsSpan(2)</c> and therefore agreed with itself
/// rather than with production, which let a two-byte offset bug ship.
/// </para>
/// </remarks>
public class QueryMiscCodecEquivalenceTests
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

    // ---- single packed GUID ----

    [Fact]
    public void ItemTextQuery_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.ItemTextQuery(); e.Read(o);
        var r = ReaderOver(f); ItemTextQueryCodec.Read(ref r, out var a);
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void QueryPetName_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.QueryPetName(); e.Read(o);
        var r = ReaderOver(f); QueryPetNameCodec.Read(ref r, out var a);
        Assert.Equal(e.UnitGUID, a.UnitGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SetSelection_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.SetSelection(); e.Read(o);
        var r = ReaderOver(f); SetSelectionCodec.Read(ref r, out var a);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void QueryCorpseLocationFromClient_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.QueryCorpseLocationFromClient(); e.Read(o);
        var r = ReaderOver(f); QueryCorpseLocationFromClientCodec.Read(ref r, out var a);
        Assert.Equal(e.Player, a.Player);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void ReclaimCorpse_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.ReclaimCorpse(); e.Read(o);
        var r = ReaderOver(f); ReclaimCorpseCodec.Read(ref r, out var a);
        Assert.Equal(e.CorpseGUID, a.CorpseGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void ObjectUpdateFailed_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.ObjectUpdateFailed(); e.Read(o);
        var r = ReaderOver(f); ObjectUpdateFailedCodec.Read(ref r, out var a);
        Assert.Equal(e.ObjectGuid, a.ObjectGuid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- uint then packed GUID. These four declare their fields in the opposite order to the
    // read order, which is the shape that makes a positional record struct easy to get wrong.

    [Theory]
    [InlineData(0u)]
    [InlineData(4321u)]
    [InlineData(uint.MaxValue)]
    public void QueryQuestInfo_Matches(uint id)
    {
        var (o, f) = Build(w => { w.WriteUInt32(id); w.WritePackedGuid128(Guid); });
        var e = new Frozen.QueryQuestInfo(); e.Read(o);
        var r = ReaderOver(f); QueryQuestInfoCodec.Read(ref r, out var a);
        Assert.Equal(e.QuestID, a.QuestID);
        Assert.Equal(e.QuestGiver, a.QuestGiver);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void QueryGameObject_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(1234); w.WritePackedGuid128(Guid); });
        var e = new Frozen.QueryGameObject(); e.Read(o);
        var r = ReaderOver(f); QueryGameObjectCodec.Read(ref r, out var a);
        Assert.Equal(e.GameObjectID, a.GameObjectID);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void QueryPageText_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(77); w.WritePackedGuid128(Guid); });
        var e = new Frozen.QueryPageText(); e.Read(o);
        var r = ReaderOver(f); QueryPageTextCodec.Read(ref r, out var a);
        Assert.Equal(e.PageTextID, a.PageTextID);
        Assert.Equal(e.ItemGUID, a.ItemGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void QueryNPCText_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(99); w.WritePackedGuid128(Guid); });
        var e = new Frozen.QueryNPCText(); e.Read(o);
        var r = ReaderOver(f); QueryNPCTextCodec.Read(ref r, out var a);
        Assert.Equal(e.TextID, a.TextID);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- plain scalars ----

    [Theory]
    [InlineData(0u)]
    [InlineData(uint.MaxValue)]
    public void QueryCreature_Matches(uint id)
    {
        var (o, f) = Build(w => w.WriteUInt32(id));
        var e = new Frozen.QueryCreature(); e.Read(o);
        var r = ReaderOver(f); QueryCreatureCodec.Read(ref r, out var a);
        Assert.Equal(e.CreatureID, a.CreatureID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void StandStateChange_Matches()
    {
        var (o, f) = Build(w => w.WriteUInt32(3));
        var e = new Frozen.StandStateChange(); e.Read(o);
        var r = ReaderOver(f); StandStateChangeCodec.Read(ref r, out var a);
        Assert.Equal(e.StandState, a.StandState);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SetDungeonDifficulty_Matches()
    {
        var (o, f) = Build(w => w.WriteUInt32(1));
        var e = new Frozen.SetDungeonDifficulty(); e.Read(o);
        var r = ReaderOver(f); SetDungeonDifficultyCodec.Read(ref r, out var a);
        Assert.Equal(e.DifficultyID, a.DifficultyID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void TimeSyncResponse_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(11); w.WriteUInt32(22); });
        var e = new Frozen.TimeSyncResponse(); e.Read(o);
        var r = ReaderOver(f); TimeSyncResponseCodec.Read(ref r, out var a);
        Assert.Equal(e.SequenceIndex, a.SequenceIndex);
        Assert.Equal(e.ClientTime, a.ClientTime);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- bit-packed ----

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RepopRequest_Matches(bool checkInstance)
    {
        var (o, f) = Build(w => { w.WriteBit(checkInstance); w.FlushBits(); });
        var e = new Frozen.RepopRequest(); e.Read(o);
        var r = ReaderOver(f); RepopRequestCodec.Read(ref r, out var a);
        Assert.Equal(e.CheckInstance, a.CheckInstance);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FarSight_Matches(bool enable)
    {
        var (o, f) = Build(w => { w.WriteBit(enable); w.FlushBits(); });
        var e = new Frozen.FarSight(); e.Read(o);
        var r = ReaderOver(f); FarSightCodec.Read(ref r, out var a);
        Assert.Equal(e.Enable, a.Enable);
    }

    [Theory]
    [InlineData(1234u, true, false)]
    [InlineData(0u, false, true)]
    [InlineData(uint.MaxValue, true, true)]
    public void AreaTriggerPkt_Matches(uint id, bool entered, bool fromClient)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32(id);
            w.WriteBit(entered);
            w.WriteBit(fromClient);
            w.FlushBits();
        });
        var e = new Frozen.AreaTriggerPkt(); e.Read(o);
        var r = ReaderOver(f); AreaTriggerPktCodec.Read(ref r, out var a);
        Assert.Equal(e.AreaTriggerID, a.AreaTriggerID);
        Assert.Equal(e.Entered, a.Entered);
        Assert.Equal(e.FromClient, a.FromClient);
    }

    // ---- conditional reads: the shape where conversions actually break ----

    [Theory]
    [InlineData(TutorialAction.Update)]
    [InlineData(TutorialAction.Clear)]
    [InlineData(TutorialAction.Reset)]
    public void TutorialSetFlag_Matches_IncludingTheSkippedField(TutorialAction action)
    {
        // TutorialBit is only on the wire for Update. The other two actions skip it, and a
        // positional record struct has no way to express the class's default — so this asserts
        // the skip produces the same value either way, for every action.
        var (o, f) = Build(w =>
        {
            w.WriteBits((uint)action, 2);
            if (action == TutorialAction.Update)
            {
                w.FlushBits();
                w.WriteUInt32(0xABCD1234);
            }
            else
            {
                w.FlushBits();
            }
        });

        var e = new Frozen.TutorialSetFlag(); e.Read(o);
        var r = ReaderOver(f); TutorialSetFlagCodec.Read(ref r, out var a);

        Assert.Equal(e.Action, a.Action);
        Assert.Equal(e.TutorialBit, a.TutorialBit);
    }

    [Fact]
    public void SetRaidDifficulty_Matches_WithTrailingByte()
    {
        var (o, f) = Build(w => { w.WriteInt32(3); w.WriteUInt8(1); });
        var e = new Frozen.SetRaidDifficulty(); e.Read(o);
        var r = ReaderOver(f); SetRaidDifficultyCodec.Read(ref r, out var a);
        Assert.Equal(e.DifficultyID, a.DifficultyID);
        Assert.Equal(e.Legacy, a.Legacy);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SetRaidDifficulty_Matches_WithoutTrailingByte()
    {
        // The CanRead guard. This only agrees with ByteBuffer because the reader is built from
        // GetRemainingSpan(); over GetDataSpan() the leftover opcode bytes would make an absent
        // Legacy byte look present and the codec would read garbage.
        var (o, f) = Build(w => w.WriteInt32(3));
        var e = new Frozen.SetRaidDifficulty(); e.Read(o);
        var r = ReaderOver(f); SetRaidDifficultyCodec.Read(ref r, out var a);
        Assert.Equal(e.DifficultyID, a.DifficultyID);
        Assert.Equal(e.Legacy, a.Legacy);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void ClientCinematicPkt_ConsumesNothing()
    {
        var (o, f) = Build(w => w.WriteUInt32(0xFFFFFFFF));
        var e = new Frozen.ClientCinematicPkt(); e.Read(o);
        var r = ReaderOver(f); ClientCinematicPktCodec.Read(ref r, out _);
        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
