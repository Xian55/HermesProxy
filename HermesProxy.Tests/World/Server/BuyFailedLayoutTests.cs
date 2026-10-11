using System;
using System.Buffers;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// 4.4.2 widened SMSG_BUY_FAILED's reason from a byte to an int32 (the client's reader:
/// guid, u32, u32; 3.4.3's is guid, u32, u8). The old layout keeps the old bytes; the new one
/// writes the reason the 4.4.2 client reads.
/// </summary>
public sealed class BuyFailedLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<BuyFailed.ByteReasonLayout>(BuyFailed.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<BuyFailed.ByteReasonLayout>(BuyFailed.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<BuyFailed.IntReasonLayout>(BuyFailed.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void ByteReasonLayout_MatchesTheOldWriter()
    {
        var packet = NotEnoughMoney();

        using var old = new WorldPacket(1u);
        old.WritePackedGuid128(packet.VendorGUID);
        old.WriteUInt32(packet.Slot);
        old.WriteUInt8((byte)packet.Reason);

        AssertBothSerialisers(new BuyFailed.ByteReasonLayout(), packet, old.GetDataSpan().ToArray());
    }

    [Fact]
    public void IntReasonLayout_WritesTheReasonAsAnInt32()
    {
        var packet = NotEnoughMoney();

        using var expected = new WorldPacket(1u);
        expected.WritePackedGuid128(packet.VendorGUID);
        expected.WriteUInt32(packet.Slot);
        expected.WriteInt32((int)BuyResult.NotEnoughtMoney);

        AssertBothSerialisers(new BuyFailed.IntReasonLayout(), packet, expected.GetDataSpan().ToArray());
    }

    private static BuyFailed NotEnoughMoney() => new()
    {
        VendorGUID = new WowGuid128(3039, 0x2000000000000000),
        Slot = 49258,
        Reason = BuyResult.NotEnoughtMoney,
    };

    private static void AssertBothSerialisers(ServerPacketLayout<BuyFailed> layout, BuyFailed packet, byte[] expected)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        byte[] buffer = ArrayPool<byte>.Shared.Rent(packet.MaxSize);
        try
        {
            int written = layout.WriteToSpan(packet, buffer);
            Assert.Equal(expected, buffer.AsSpan(0, written).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
