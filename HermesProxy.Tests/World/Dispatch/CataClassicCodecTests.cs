using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// 4.4.2 CMSG codecs whose shape adds a field to the 3.4.3 one (docs/protocol, the client's own
/// writer, and TrinityCore cata_classic agree on each). Each body is written the way the 4.4.2
/// client writes it, and the codec must land on every field and consume all of it.
/// </summary>
public sealed class CataClassicCodecTests
{
    private static readonly WowGuid128 Npc = new(0x155A, 0x2000040000004AC0);

    // Frame like the wire: WorldPacket's read-mode ctor consumes a 2-byte opcode prefix.
    private static byte[] Framed(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return framed;
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    [Fact]
    public void AutoBankItem_CataClassic_SkipsTheBankType()
    {
        var f = Framed(w =>
        {
            w.WriteBits(0u, 2);                     // InvUpdate: no items
            w.FlushBits();
            w.WriteUInt8(0);                        // BankType
            w.WriteUInt8(255);                      // Bag
            w.WriteUInt8(23);                       // Slot
        });

        var r = ReaderOver(f);
        AutoBankItemCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal(255, packet.PackSlot);
        Assert.Equal(23, packet.Slot);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionRemoveItem_CataClassic_SkipsTheItemId()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt32(4711);                    // AuctionID
            w.WriteInt32(2589);                     // ItemID
            w.WriteBit(false);                      // TaintedBy
            w.FlushBits();
        });

        var r = ReaderOver(f);
        AuctionRemoveItemCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal(Npc, packet.Auctioneer);
        Assert.Equal(4711u, packet.AuctionID);
        Assert.Null(packet.TaintedBy);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AlterAppearance_CataClassic_SkipsTheUnalteredVisualRace()
    {
        var f = Framed(w =>
        {
            w.WriteUInt32(1);                       // customization count
            w.WriteUInt8(1);                        // NewSex
            w.WriteUInt32(1);                       // CustomizedRace
            w.WriteUInt32(49);                      // CustomizedChrModelID
            w.WriteInt32(1);                        // UnalteredVisualRaceID
            w.WriteUInt32(17);                      // ChrCustomizationOptionID
            w.WriteUInt32(150);                     // ChrCustomizationChoiceID
        });

        var r = ReaderOver(f);
        AlterAppearanceCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal((Gender)1, packet.NewSexId);
        Assert.Equal((Race)1, packet.CustomizedRace);
        Assert.Equal(49u, packet.CustomizedChrModelId);
        var choice = Assert.Single(packet.Customizations);
        Assert.Equal(new ChrCustomizationChoice(17, 150), choice);
        Assert.Equal(0, r.Remaining);
    }
}
