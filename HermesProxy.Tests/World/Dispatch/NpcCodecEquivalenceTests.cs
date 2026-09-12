using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the NPC-interaction codecs against the frozen <c>Read()</c> bodies they
/// replaced — gossip, trainers, flight masters and the auctioneer.
/// </summary>
/// <remarks>
/// <c>InteractWithNPC</c> is the widest-shared inbound packet in the proxy: sixteen opcodes across
/// four systems reduce to the one packed GUID this covers. Every case asserts field values
/// <b>and</b> the reader's final position, over readers built through <c>GetRemainingSpan()</c>.
/// </remarks>
public class NpcCodecEquivalenceTests
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

    [Fact]
    public void InteractWithNPC_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.InteractWithNPC(); e.Read(o);
        var r = ReaderOver(f); InteractWithNPCCodec.Read(ref r, out var a);
        Assert.Equal(e.CreatureGUID, a.CreatureGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void BuyBankSlot_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.BuyBankSlot(); e.Read(o);
        var r = ReaderOver(f); BuyBankSlotCodec.Read(ref r, out var a);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// GossipID and GossipIndex are declared on the packet in the opposite order to the read order,
    /// which is exactly the shape a positional record struct makes easy to transpose — and both are
    /// uint, so the compiler would not object.
    /// </summary>
    [Theory]
    [InlineData(1234u, 5u, "")]
    [InlineData(0u, 0u, "PROMO-CODE")]
    [InlineData(uint.MaxValue, uint.MaxValue, "x")]
    public void GossipSelectOption_Matches(uint gossipId, uint gossipIndex, string promotionCode)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(gossipId);
            w.WriteUInt32(gossipIndex);
            w.WriteBits((uint)promotionCode.Length, 8);
            w.WriteString(promotionCode);
        });

        var e = new Frozen.GossipSelectOption(); e.Read(o);
        var r = ReaderOver(f); GossipSelectOptionCodec.Read(ref r, out var a);
        Assert.Equal(e.GossipUnit, a.GossipUnit);
        Assert.Equal(e.GossipID, a.GossipID);
        Assert.Equal(e.GossipIndex, a.GossipIndex);
        Assert.Equal(gossipId, a.GossipID);
        Assert.Equal(gossipIndex, a.GossipIndex);
        Assert.Equal(e.PromotionCode, a.PromotionCode);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u, 0u)]
    [InlineData(12u, 34567u)]
    public void TrainerBuySpell_Matches(uint trainerId, uint spellId)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(trainerId);
            w.WriteUInt32(spellId);
        });

        var e = new Frozen.TrainerBuySpell(); e.Read(o);
        var r = ReaderOver(f); TrainerBuySpellCodec.Read(ref r, out var a);
        Assert.Equal(e.TrainerGUID, a.TrainerGUID);
        Assert.Equal(e.TrainerID, a.TrainerID);
        Assert.Equal(e.SpellID, a.SpellID);
        Assert.Equal(trainerId, a.TrainerID);
        Assert.Equal(spellId, a.SpellID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(SpecResetType.Talents)]
    [InlineData(SpecResetType.PetTalents)]
    public void ConfirmRespecWipe_Matches(SpecResetType respecType)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt8((byte)respecType); });
        var e = new Frozen.ConfirmRespecWipe(); e.Read(o);
        var r = ReaderOver(f); ConfirmRespecWipeCodec.Read(ref r, out var a);
        Assert.Equal(e.TrainerGUID, a.TrainerGUID);
        Assert.Equal(e.RespecType, a.RespecType);
        Assert.Equal(respecType, a.RespecType);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// Three uints follow the GUID, and the handler forwards only <c>Node</c>. A codec that
    /// transposed the two mount ids would agree with the server on every flight and still be wrong
    /// the day either is used.
    /// </summary>
    [Fact]
    public void ActivateTaxi_Matches()
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(310);
            w.WriteUInt32(1001);
            w.WriteUInt32(2002);
        });

        var e = new Frozen.ActivateTaxi(); e.Read(o);
        var r = ReaderOver(f); ActivateTaxiCodec.Read(ref r, out var a);
        Assert.Equal(e.FlightMaster, a.FlightMaster);
        Assert.Equal(e.Node, a.Node);
        Assert.Equal(e.GroundMountID, a.GroundMountID);
        Assert.Equal(e.FlyingMountID, a.FlyingMountID);
        Assert.Equal(310u, a.Node);
        Assert.Equal(1001u, a.GroundMountID);
        Assert.Equal(2002u, a.FlyingMountID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(50u)]
    public void AuctionListOwnerItems_Matches(uint offset)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(offset); });
        var e = new Frozen.AuctionListOwnerItems(); e.Read(o);
        var r = ReaderOver(f); AuctionListOwnerItemsCodec.Read(ref r, out var a);
        Assert.Equal(e.Auctioneer, a.Auctioneer);
        Assert.Equal(e.Offset, a.Offset);
        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
