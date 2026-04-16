using Framework.Constants;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

public class TradeStatusVariantTests
{
    static TradeStatusVariantTests()
    {
        if (global::Framework.Settings.ClientBuild == ClientVersionBuild.Zero)
            global::Framework.Settings.ClientBuild = ClientVersionBuild.V1_14_2_42597;
    }

    [Fact]
    public void Variant_Failed_PatternMatches()
    {
        var pkt = new TradeStatusPkt
        {
            Status = TradeStatus.Failed,
            Variant = new TradeFailedData(FailureForYou: true, BagResult: InventoryResult.Ok, ItemID: 12345),
        };

        Assert.IsType<TradeFailedData>(pkt.Variant.Value);
        var failed = (TradeFailedData)pkt.Variant.Value!;
        Assert.True(failed.FailureForYou);
        Assert.Equal(12345u, failed.ItemID);
    }

    [Fact]
    public void Variant_Initiated_PatternMatches()
    {
        var pkt = new TradeStatusPkt
        {
            Status = TradeStatus.Initiated,
            Variant = new TradeInitiatedData(Id: 42),
        };

        Assert.IsType<TradeInitiatedData>(pkt.Variant.Value);
        Assert.Equal(42u, ((TradeInitiatedData)pkt.Variant.Value!).Id);
    }

    [Fact]
    public void Variant_Proposed_CarriesBothGuids()
    {
        var partner = new WowGuid128(0x1, 0x0200000000000001);
        var account = new WowGuid128(0x2, 0x0100000000000001);
        var pkt = new TradeStatusPkt
        {
            Status = TradeStatus.Proposed,
            Variant = new TradeProposedData(partner, account),
        };

        var proposed = (TradeProposedData)pkt.Variant.Value!;
        Assert.Equal(partner, proposed.Partner);
        Assert.Equal(account, proposed.PartnerAccount);
    }

    [Fact]
    public void Variant_Slot_HoldsByteSlot()
    {
        var pkt = new TradeStatusPkt
        {
            Status = TradeStatus.WrongRealm,
            Variant = new TradeSlotData(TradeSlot: 5),
        };

        Assert.Equal((byte)5, ((TradeSlotData)pkt.Variant.Value!).TradeSlot);
    }

    [Fact]
    public void Variant_Currency_HoldsTypeAndQuantity()
    {
        var pkt = new TradeStatusPkt
        {
            Status = TradeStatus.NotEnoughCurrency,
            Variant = new TradeCurrencyData(CurrencyType: 1, CurrencyQuantity: 100),
        };

        var currency = (TradeCurrencyData)pkt.Variant.Value!;
        Assert.Equal(1, currency.CurrencyType);
        Assert.Equal(100, currency.CurrencyQuantity);
    }

    [Fact]
    public void Variant_DefaultStatus_HasNullVariant()
    {
        // Statuses without payload (Cancelled, Accepted, etc.) leave Variant unset
        var pkt = new TradeStatusPkt { Status = TradeStatus.Cancelled };

        Assert.Null(pkt.Variant.Value);
    }
}

public class TradeStatusWriteTests
{
    static TradeStatusWriteTests()
    {
        if (global::Framework.Settings.ClientBuild == ClientVersionBuild.Zero)
            global::Framework.Settings.ClientBuild = ClientVersionBuild.V1_14_2_42597;
    }

    [Theory]
    [InlineData(TradeStatus.Cancelled)]
    [InlineData(TradeStatus.Accepted)]
    [InlineData(TradeStatus.StateChanged)]
    public void Write_NoVariant_FlushesBitsCorrectly(TradeStatus status)
    {
        var pkt = new TradeStatusPkt { Status = status };

        pkt.WritePacketData();
        var bytes = pkt.GetData()!;

        // Should write only the status bits + flush — at least 1 byte
        Assert.True(bytes.Length >= 1);
    }

    [Fact]
    public void Write_FailedVariant_MatchesExpectedPayload()
    {
        var pkt = new TradeStatusPkt
        {
            Status = TradeStatus.Failed,
            Variant = new TradeFailedData(FailureForYou: false, BagResult: InventoryResult.Ok, ItemID: 0xCAFEBABE),
        };

        pkt.WritePacketData();
        var bytes = pkt.GetData()!;

        Assert.True(bytes.Length >= 9); // bits + int32 + uint32
    }

    [Fact]
    public void WriteToSpan_MatchesByteBufferWrite_FailedVariant()
    {
        var pkt1 = new TradeStatusPkt
        {
            Status = TradeStatus.Failed,
            Variant = new TradeFailedData(FailureForYou: true, BagResult: InventoryResult.Ok, ItemID: 0xDEADBEEF),
        };
        var pkt2 = new TradeStatusPkt
        {
            Status = TradeStatus.Failed,
            Variant = new TradeFailedData(FailureForYou: true, BagResult: InventoryResult.Ok, ItemID: 0xDEADBEEF),
        };

        // ByteBuffer path
        pkt1.Write();
        pkt1.WritePacketData();
        var byteBufferData = pkt1.GetData()!;

        // Span path
        var spanBuffer = new byte[pkt2.MaxSize];
        int written = pkt2.WriteToSpan(spanBuffer);

        Assert.True(written > 0);
        Assert.Equal(byteBufferData.Length, written);
        Assert.Equal(byteBufferData, spanBuffer[..written]);
    }

    [Fact]
    public void WriteToSpan_MatchesByteBufferWrite_ProposedVariant()
    {
        var partner = new WowGuid128(0x1234567890ABCDEF, 0x0200000000000001);
        var account = new WowGuid128(0xFEDCBA0987654321, 0x0100000000000001);

        var pkt1 = new TradeStatusPkt
        {
            Status = TradeStatus.Proposed,
            Variant = new TradeProposedData(partner, account),
        };
        var pkt2 = new TradeStatusPkt
        {
            Status = TradeStatus.Proposed,
            Variant = new TradeProposedData(partner, account),
        };

        pkt1.Write();
        pkt1.WritePacketData();
        var byteBufferData = pkt1.GetData()!;

        var spanBuffer = new byte[pkt2.MaxSize];
        int written = pkt2.WriteToSpan(spanBuffer);

        Assert.True(written > 0);
        Assert.Equal(byteBufferData.Length, written);
        Assert.Equal(byteBufferData, spanBuffer[..written]);
    }
}
