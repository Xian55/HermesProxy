using System;
using System.Linq;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_NEW_WORLD sends the client to another map. From 4.4.1 on it ends with the player's count of
/// far teleports (TrinityCore cata_classic, WowPacketParser's 4.4 reader; the 4.4.2 client reads 40
/// bytes). No legacy server sends the count, so the proxy keeps it; written without it, every far
/// teleport reached the 4.4.2 client four bytes short.
/// </summary>
public sealed class NewWorldLayoutTests
{
    // map 609, (2361, -5711, 156), facing pi, reason 4, no movement offset
    private const string Destination =
        "61020000" + "00901345" + "0078B2C5" + "00001C43" + "DB0F4940" + "04000000"
        + "00000000" + "00000000" + "00000000";

    [Fact]
    public void NoCounterLayout_IsTheFormerWrite()
    {
        Assert.Equal(Destination, Write(new NewWorld.NoCounterLayout()));
        Assert.Equal(Destination, WriteToSpan(new NewWorld.NoCounterLayout()));
    }

    [Fact]
    public void CounterLayout_EndsWithTheCounter()
    {
        Assert.Equal(Destination + "03000000", Write(new NewWorld.CounterLayout()));
        Assert.Equal(Destination + "03000000", WriteToSpan(new NewWorld.CounterLayout()));
    }

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<NewWorld.NoCounterLayout>(NewWorld.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<NewWorld.NoCounterLayout>(NewWorld.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<NewWorld.NoCounterLayout>(NewWorld.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<NewWorld.CounterLayout>(NewWorld.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void EachFarTeleport_RaisesTheCounter()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);

        // the server announces each transfer first (SMSG_TRANSFER_PENDING)
        harness.Session.GameState.IsWaitingForNewWorld = true;
        harness.Deliver(Opcode.SMSG_NEW_WORLD, LegacyNewWorld(609), harness.Client.HandleNewWorld);
        harness.Session.GameState.IsWaitingForNewWorld = true;
        harness.Deliver(Opcode.SMSG_NEW_WORLD, LegacyNewWorld(0), harness.Client.HandleNewWorld);

        var sent = harness.ClientWire.Sent.Select(s => s.Packet).OfType<NewWorld>().ToList();
        Assert.Equal([1, 2], sent.Select(p => p.Counter));
    }

    private static NewWorld Sample() => new()
    {
        MapID = 609,
        Position = new Vector3(2361f, -5711f, 156f),
        Orientation = MathF.PI,
        Reason = 4,
        Counter = 3,
    };

    private static string Write(ServerPacketLayout<NewWorld> layout)
    {
        using var data = new WorldPacket(1u);
        layout.Write(Sample(), data);
        return Convert.ToHexString(data.GetDataSpan());
    }

    private static string WriteToSpan(ServerPacketLayout<NewWorld> layout)
    {
        Span<byte> buffer = stackalloc byte[layout.MaxSize];
        int written = layout.WriteToSpan(Sample(), buffer);
        Assert.Equal(layout.MaxSize, written);
        return Convert.ToHexString(buffer[..written]);
    }

    private static byte[] LegacyNewWorld(uint mapId)
        => LegacyPacketBuilder.Build(Opcode.SMSG_NEW_WORLD, packet =>
        {
            if (LegacyVersion.ExpansionVersion >= 4)
            {
                packet.WriteFloat(2361f);
                packet.WriteFloat(MathF.PI);
                packet.WriteFloat(156f);
                packet.WriteUInt32(mapId);
                packet.WriteFloat(-5711f);
                return;
            }
            packet.WriteUInt32(mapId);
            packet.WriteFloat(2361f);
            packet.WriteFloat(-5711f);
            packet.WriteFloat(156f);
            packet.WriteFloat(MathF.PI);
        });
}
