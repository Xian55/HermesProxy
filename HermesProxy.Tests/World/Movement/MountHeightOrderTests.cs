using System;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// A 3.3.5a server orders the collision height of a player who mounts, dismounts or takes a flight,
/// and CMaNGOS does the mounting and starts the flight only when the ack comes back. Dropped, the
/// player had the mount's aura and stayed on foot, and no flight left. The proxy also sends heights
/// of its own, so only the ack of an order may reach the server.
/// </summary>
public sealed class MountHeightOrderTests
{
    private const uint Counter = 7;
    private const float Height = 2.0313f;

    private static readonly WowGuid64 Player = new(HighGuidTypeLegacy.Player, 77);

    private static void SkipUnlessLegacyOrdersTheHeight()
        => Assert.SkipUnless(LegacyVersion.ExpansionVersion == 3, "the order is a 3.3.5a packet");

    private static (LegacyHandlerHarness Harness, SessionContext Ctx, WowGuid128 Player) InTheWorld()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        return (harness, new SessionContext(harness.Session, socket: null, harness.Client), player);
    }

    [Fact]
    public void Order_ReachesTheClient_MarkedForItsAck()
    {
        SkipUnlessLegacyOrdersTheHeight();
        var (harness, _, player) = InTheWorld();

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_MOVE_SET_COLLISION_HGT, packet =>
        {
            packet.WritePackedGuid(Player);
            packet.WriteUInt32(Counter);
            packet.WriteFloat(Height);
        });
        harness.Deliver(Opcode.SMSG_MOVE_SET_COLLISION_HGT, wire, harness.Client.HandleMoveSetCollisionHgt);

        var order = Assert.IsType<MoveSetCollisionHeight>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(player, order.MoverGUID);
        Assert.Equal(Counter | MoveSetCollisionHeight.ServerOrderBit, order.SequenceIndex);
        Assert.Equal(Height, order.Height);
    }

    [Fact]
    public void AckOfAnOrder_ReachesTheServer_WithItsOwnCounter()
    {
        SkipUnlessLegacyOrdersTheHeight();
        var (harness, ctx, player) = InTheWorld();

        var ack = new MoveSetCollisionHeightAck(player,
            new MovementAck { MoveCounter = Counter | MoveSetCollisionHeight.ServerOrderBit }, Height, 0, 2);
        MovementSystem.HandleMoveSetCollisionHeightAck(in ack, in ctx);

        var sent = Assert.Single(harness.ServerWire.Sent);
        Assert.Equal(LegacyVersion.GetCurrentOpcode(Opcode.CMSG_MOVE_SET_COLLISION_HGT_ACK), sent.Opcode);
        // The packed guid of a low player guid takes two bytes.
        Assert.Equal(Counter, BitConverter.ToUInt32(sent.Bytes, 2));
        Assert.Equal(Height, BitConverter.ToSingle(sent.Bytes, sent.Bytes.Length - 4));
    }

    // A far teleport out of a flight: CMaNGOS does not act on an ack ahead of the world port ack (#561).
    [Fact]
    public void AckOfAnOrder_BetweenMaps_WaitsForThePlayersCreate()
    {
        SkipUnlessLegacyOrdersTheHeight();
        var (harness, ctx, player) = InTheWorld();
        harness.Session.GameState.ClientHasPlayerObject = false;

        var ack = new MoveSetCollisionHeightAck(player,
            new MovementAck { MoveCounter = Counter | MoveSetCollisionHeight.ServerOrderBit }, Height, 0, 2);
        MovementSystem.HandleMoveSetCollisionHeightAck(in ack, in ctx);
        Assert.Empty(harness.ServerWire.Sent);

        harness.Session.GameState.ClientHasPlayerObject = true;
        harness.Session.ToClient.Notify(OutboxEvent.GuidKnown(player));

        var sent = Assert.Single(harness.ServerWire.Sent);
        Assert.Equal(LegacyVersion.GetCurrentOpcode(Opcode.CMSG_MOVE_SET_COLLISION_HGT_ACK), sent.Opcode);
    }

    [Fact]
    public void AckOfTheProxysOwnHeight_StaysHere()
    {
        SkipUnlessLegacyOrdersTheHeight();
        var (harness, ctx, player) = InTheWorld();

        var ownHeight = new MoveSetCollisionHeight();
        var ack = new MoveSetCollisionHeightAck(player, new MovementAck { MoveCounter = ownHeight.SequenceIndex }, Height, 0, 1);
        MovementSystem.HandleMoveSetCollisionHeightAck(in ack, in ctx);

        Assert.Empty(harness.ServerWire.Sent);
    }
}
