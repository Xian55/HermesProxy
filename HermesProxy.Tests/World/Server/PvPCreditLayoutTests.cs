using System;
using System.Buffers;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_PVP_CREDIT ends with the rank for the 1.14 and 2.5 clients; the 3.4.3 and 4.4.2 ones read a
/// ForceHonorable bit in a byte after it (docs/protocol: one bit in byte 14), which the proxy left
/// out, so they read it past the end of the packet.
/// </summary>
public sealed class PvPCreditLayoutTests
{
    private static PvPCredit Kill() => new()
    {
        OriginalHonor = 21,
        Honor = 18,
        Target = new WowGuid128(0x2BF, 0x0800040000000000),
        Rank = 5,
    };

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<PvPCredit.RankLastLayout>(PvPCredit.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<PvPCredit.RankLastLayout>(PvPCredit.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<PvPCredit.ForceHonorableLayout>(PvPCredit.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<PvPCredit.ForceHonorableLayout>(PvPCredit.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void RankLast_MatchesTheOldWriter()
    {
        var p = Kill();
        using var old = new WorldPacket(1u);
        old.WriteInt32(p.OriginalHonor);
        old.WriteInt32(p.Honor);
        old.WritePackedGuid128(p.Target);
        old.WriteUInt32(p.Rank);

        AssertBothSerialisers(new PvPCredit.RankLastLayout(), p, old);
    }

    [Fact]
    public void ForceHonorable_IsAByteAfterTheRank()
    {
        var p = Kill();
        using var expected = new WorldPacket(1u);
        expected.WriteInt32(p.OriginalHonor);
        expected.WriteInt32(p.Honor);
        expected.WritePackedGuid128(p.Target);
        expected.WriteUInt32(p.Rank);
        expected.WriteUInt8(0);

        AssertBothSerialisers(new PvPCredit.ForceHonorableLayout(), p, expected);

        p.ForceHonorable = true;
        using var forced = new WorldPacket(1u);
        new PvPCredit.ForceHonorableLayout().Write(p, forced);
        Assert.Equal(0x80, forced.GetDataSpan()[^1]);
    }

    private static void AssertBothSerialisers(ServerPacketLayout<PvPCredit> layout, PvPCredit packet, WorldPacket expectedPacket)
    {
        byte[] expected = expectedPacket.GetDataSpan().ToArray();

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
