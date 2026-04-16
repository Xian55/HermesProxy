using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

public class InventoryFailureVariantTests
{
    static InventoryFailureVariantTests()
    {
        if (global::Framework.Settings.ClientBuild == ClientVersionBuild.Zero)
            global::Framework.Settings.ClientBuild = ClientVersionBuild.V1_14_2_42597;
    }

    [Fact]
    public void Variant_Level_PatternMatches()
    {
        var pkt = new InventoryChangeFailure
        {
            BagResult = InventoryResult.CantEquipLevel,
            Variant = new InventoryFailureLevelData(60),
        };

        Assert.IsType<InventoryFailureLevelData>(pkt.Variant.Value);
        Assert.Equal(60, ((InventoryFailureLevelData)pkt.Variant.Value!).Level);
    }

    [Fact]
    public void Variant_AutoEquip_HoldsAllThreeFields()
    {
        var src = new WowGuid128(0x1, 0x0200000000000001);
        var dst = new WowGuid128(0x2, 0x0200000000000001);
        var pkt = new InventoryChangeFailure
        {
            BagResult = InventoryResult.EventAutoEquipBindConfirm,
            Variant = new InventoryFailureAutoEquipData(src, 5, dst),
        };

        var autoEquip = (InventoryFailureAutoEquipData)pkt.Variant.Value!;
        Assert.Equal(src, autoEquip.SrcContainer);
        Assert.Equal(5, autoEquip.SrcSlot);
        Assert.Equal(dst, autoEquip.DstContainer);
    }

    [Fact]
    public void Variant_LimitCategory_HoldsCategoryId()
    {
        var pkt = new InventoryChangeFailure
        {
            BagResult = InventoryResult.ItemMaxLimitCategoryCountExceeded,
            Variant = new InventoryFailureLimitData(42),
        };

        Assert.Equal(42, ((InventoryFailureLimitData)pkt.Variant.Value!).LimitCategory);
    }

    [Fact]
    public void Variant_NoPayload_IsNull()
    {
        // Most BagResult values don't carry payload
        var pkt = new InventoryChangeFailure { BagResult = InventoryResult.ItemNotFound };

        Assert.Null(pkt.Variant.Value);
    }
}

public class InventoryFailureWriteTests
{
    static InventoryFailureWriteTests()
    {
        if (global::Framework.Settings.ClientBuild == ClientVersionBuild.Zero)
            global::Framework.Settings.ClientBuild = ClientVersionBuild.V1_14_2_42597;
    }

    [Fact]
    public void WriteToSpan_MatchesByteBuffer_LevelVariant()
    {
        var pkt1 = new InventoryChangeFailure
        {
            BagResult = InventoryResult.CantEquipLevel,
            Variant = new InventoryFailureLevelData(80),
        };
        var pkt2 = new InventoryChangeFailure
        {
            BagResult = InventoryResult.CantEquipLevel,
            Variant = new InventoryFailureLevelData(80),
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

    [Fact]
    public void WriteToSpan_MatchesByteBuffer_AutoEquipVariant()
    {
        var src = new WowGuid128(0xDEADBEEF, 0x0200000000000001);
        var dst = new WowGuid128(0xCAFEBABE, 0x0200000000000001);

        var pkt1 = new InventoryChangeFailure
        {
            BagResult = InventoryResult.EventAutoEquipBindConfirm,
            Variant = new InventoryFailureAutoEquipData(src, 7, dst),
        };
        var pkt2 = new InventoryChangeFailure
        {
            BagResult = InventoryResult.EventAutoEquipBindConfirm,
            Variant = new InventoryFailureAutoEquipData(src, 7, dst),
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

    [Fact]
    public void WriteToSpan_MatchesByteBuffer_NoVariant()
    {
        var pkt1 = new InventoryChangeFailure { BagResult = InventoryResult.ItemNotFound };
        var pkt2 = new InventoryChangeFailure { BagResult = InventoryResult.ItemNotFound };

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
