using System;
using System.Buffers;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// 4.4.2 swapped the two fields of each SMSG_POWER_UPDATE entry. Both serialisers of the old layout
/// keep the old bytes; both serialisers of the new one give the bytes of a native TrinityCore
/// cata_classic packet.
/// </summary>
public sealed class PowerUpdateLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<PowerUpdate.PowerFirstLayout>(PowerUpdate.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<PowerUpdate.PowerFirstLayout>(PowerUpdate.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<PowerUpdate.TypeFirstLayout>(PowerUpdate.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void PowerFirstLayout_MatchesTheOldWriter()
    {
        var packet = new PowerUpdate(new WowGuid128(4023, 0x0800040000000000));
        packet.Powers.Add(new PowerUpdatePower(400, 0));
        packet.Powers.Add(new PowerUpdatePower(-5, 3));

        using var old = new WorldPacket(1u);
        old.WritePackedGuid128(packet.Guid);
        old.WriteInt32(packet.Powers.Count);
        foreach (var power in packet.Powers)
        {
            old.WriteInt32(power.Power);
            old.WriteUInt8(power.PowerType);
        }

        AssertBothSerialisers(new PowerUpdate.PowerFirstLayout(), packet, old.GetDataSpan().ToArray());
    }

    [Fact]
    public void TypeFirstLayout_MatchesANativePacket()
    {
        var packet = new PowerUpdate(new WowGuid128(1, 0x0800040000000000));
        packet.Powers.Add(new PowerUpdatePower(400, 1));
        AssertBothSerialisers(new PowerUpdate.TypeFirstLayout(), packet, Convert.FromHexString(NativeRage));
    }

    // SMSG_POWER_UPDATE (rage 40.0) from TrinityCore cata_classic to a 4.4.2.60895 client
    // (refs/native-captures/tc_cata_442_20261010-150803.pkt, packet 766).
    private const string NativeRage = "01A0010408010000000190010000";

    private static void AssertBothSerialisers(ServerPacketLayout<PowerUpdate> layout, PowerUpdate packet, byte[] expected)
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
