using System;
using System.Linq;
using System.Reflection;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// A player's collision height after a mount or scale change (#338). The player's own goes out as
/// SMSG_MOVE_SET_COLLISION_HEIGHT, which the client takes on and answers. For a 3.4.3 client another
/// player's goes out as SMSG_MOVE_UPDATE_COLLISION_HEIGHT, with the status of the next movement the
/// server reports for that player: the client applies that status as a movement of the unit, and a
/// legacy server gives none with the height. The older clients keep getting the SET for everyone.
/// </summary>
public class RemoteCollisionHeightTests
{
    private const uint HumanMale = 49;
    private const uint Horse = 2404;

    private static LegacyHandlerHarness NewHarness()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        harness.AddKnownObject(MovementScenarios.OtherPlayer, ObjectType.Player);
        return harness;
    }

    private static void DeliverMount(LegacyHandlerHarness harness, WowGuid64 player, float scale = 1.0f)
    {
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
        {
            packet.WriteUInt32(1);
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteBool(false);    // has transport
            LegacyPacketBuilder.WriteValuesBlock(packet, player, LegacyVersion.GetUpdateField(PlayerField.PLAYER_END),
                (LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_SCALE_X), BitConverter.SingleToUInt32Bits(scale)),
                (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_DISPLAYID), HumanMale),
                (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_NATIVEDISPLAYID), HumanMale),
                (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MOUNTDISPLAYID), Horse));
        });
        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, wire, harness.Client.HandleUpdateObject);
    }

    private static void DeliverHeartbeat(LegacyHandlerHarness harness)
        => harness.Deliver(Opcode.MSG_MOVE_HEARTBEAT, MovementScenarios.Heartbeat(MovementScenarios.Forward), harness.Client.HandleMovementMessages);

    private static SentClientPacket[] Sent<T>(LegacyHandlerHarness harness) where T : ServerPacket
        => harness.ClientWire.Sent.Where(s => s.Packet is T).ToArray();

    [Fact]
    public void OwnMount_IsSetOnThePlayer()
    {
        var harness = NewHarness();

        DeliverMount(harness, MovementScenarios.ActivePlayer);

        var set = Assert.Single(Sent<MoveSetCollisionHeight>(harness));
        Assert.Equal(harness.Session.GameState.CurrentPlayerGuid, ((MoveSetCollisionHeight)set.Packet).MoverGUID);
        Assert.Empty(Sent<MoveUpdateCollisionHeight>(harness));
    }

    // A 3.3.5a server also orders the player's height on a mount (AzerothCore Unit::Mount), and that
    // one needs its ack, so the client gets both. Only the order is marked for its ack to go back.
    [Fact]
    public void OwnMount_TheServersOrderGoesOutBesideTheHeightFromTheFields()
    {
        if (LegacyVersion.ExpansionVersion != 3) return;
        var harness = NewHarness();

        DeliverMount(harness, MovementScenarios.ActivePlayer);
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_MOVE_SET_COLLISION_HGT, packet =>
        {
            packet.WritePackedGuid(MovementScenarios.ActivePlayer);
            packet.WriteUInt32(53);         // the server's move counter
            packet.WriteFloat(2.8f);
        });
        harness.Deliver(Opcode.SMSG_MOVE_SET_COLLISION_HGT, wire, harness.Client.HandleMoveSetCollisionHgt);

        var heights = Sent<MoveSetCollisionHeight>(harness).Select(s => (MoveSetCollisionHeight)s.Packet).ToArray();
        Assert.Equal(2, heights.Length);
        Assert.Equal(new MoveSetCollisionHeight().SequenceIndex, heights[0].SequenceIndex);
        Assert.Equal(53u | MoveSetCollisionHeight.ServerOrderBit, heights[1].SequenceIndex);
    }

    [Fact]
    public void AnotherPlayersMount_GoesOutWithItsNextMovement()
    {
        var harness = NewHarness();

        DeliverMount(harness, MovementScenarios.OtherPlayer);

        if (!ModernVersion.IsWotLKClassicOrLater)
        {
            Assert.Single(Sent<MoveSetCollisionHeight>(harness));
            return;
        }
        Assert.Empty(Sent<MoveSetCollisionHeight>(harness));
        Assert.Empty(Sent<MoveUpdateCollisionHeight>(harness));

        DeliverHeartbeat(harness);

        var move = Assert.Single(Sent<MoveUpdate>(harness));
        var update = Assert.Single(Sent<MoveUpdateCollisionHeight>(harness));
        var height = (MoveUpdateCollisionHeight)update.Packet;
        Assert.Equal(MovementScenarios.OtherPlayer.To128(harness.Session.GameState), height.MoverGUID);
        Assert.True(height.Height > 0f);
        Assert.Equal(1.0f, height.Scale);
        // the movement's own status, then the height and the scale
        Assert.Equal(move.Bytes, update.Bytes[..^8]);
        Assert.Equal(height.Height, BitConverter.ToSingle(update.Bytes, update.Bytes.Length - 8));
    }

    [Fact]
    public void AnotherPlayersHeight_GoesOutOnce()
    {
        if (!ModernVersion.IsWotLKClassicOrLater) return;
        var harness = NewHarness();

        DeliverMount(harness, MovementScenarios.OtherPlayer);
        DeliverHeartbeat(harness);
        DeliverHeartbeat(harness);

        Assert.Equal(2, Sent<MoveUpdate>(harness).Length);
        Assert.Single(Sent<MoveUpdateCollisionHeight>(harness));
    }

    [Fact]
    public void AnotherPlayersNewerHeight_ReplacesTheHeldOne()
    {
        if (!ModernVersion.IsWotLKClassicOrLater) return;
        var harness = NewHarness();

        DeliverMount(harness, MovementScenarios.OtherPlayer);
        DeliverMount(harness, MovementScenarios.OtherPlayer, scale: 2.0f);
        DeliverHeartbeat(harness);

        var update = Assert.Single(Sent<MoveUpdateCollisionHeight>(harness));
        Assert.Equal(2.0f, ((MoveUpdateCollisionHeight)update.Packet).Scale);
    }

    [Fact]
    public void MoveUpdateCollisionHeight_BothWritersAgree()
    {
        var packet = new MoveUpdateCollisionHeight
        {
            MoverGUID = MovementScenarios.OtherPlayer.To128(new LegacyHandlerHarness(recordClientPackets: false).Session.GameState),
            MoveInfo = new HermesProxy.World.Objects.MovementInfo { Position = new Vector3(1f, 2f, 3f), Orientation = 0.5f, MoveTime = 77 },
            Height = 3.25f,
            Scale = 1.5f,
        };
        packet.Write();
        var worldPacket = typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;
        byte[] written = ((WorldPacket)worldPacket.GetValue(packet)!).GetData();

        byte[] buffer = new byte[packet.MaxSize];
        int length = packet.WriteToSpan(buffer);

        Assert.Equal(written, buffer[..length]);
        Assert.Equal(3.25f, BitConverter.ToSingle(written, written.Length - 8));
        Assert.Equal(1.5f, BitConverter.ToSingle(written, written.Length - 4));
    }
}
