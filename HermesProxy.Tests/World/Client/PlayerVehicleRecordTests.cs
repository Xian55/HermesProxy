using System.Linq;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

public class PlayerVehicleRecordTests
{
    [Fact]
    public unsafe void PlayerVehicleData_HasARegisteredLegacyHandler()
        => Assert.True(GeneratedSmsgDispatch.Get(Opcode.SMSG_PLAYER_VEHICLE_DATA) != null);

    [Theory]
    [InlineData("SMSG_SET_VEHICLE_REC_ID", 0x26F7u)]
    [InlineData("SMSG_MOVE_SET_VEHICLE_REC_ID", 0x2E14u)]
    [InlineData("CMSG_MOVE_SET_VEHICLE_REC_ID_ACK", 0x3A14u)]
    [InlineData("CMSG_RIDE_VEHICLE_INTERACT", 0x323Bu)]
    [InlineData("CMSG_EJECT_PASSENGER", 0x323Cu)]
    public void WrathClassicVehicleOpcodes_MatchThe343Protocol(string name, uint expected)
        => Assert.Equal(expected, Opcodes.GetOpcodeValueForVersion(name,
            global::HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261));

    [Theory]
    [InlineData(true, 313u)] // Traveler's Tundra Mammoth (Horde)
    [InlineData(false, 313u)]
    [InlineData(true, 0u)] // dismount removes the kit
    [InlineData(false, 0u)]
    public void PlayerVehicleData_ForwardsTheVehicleKitOnWire(bool ownPlayer, uint vehicleId)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        var other = new WowGuid64(HighGuidTypeLegacy.Player, 78);
        harness.SetActivePlayer(player);
        harness.Session.GameState.ClientHasPlayerObject = true;
        harness.AddKnownObject(other, ObjectType.Player);
        var subject = ownPlayer ? player : other;
        Deliver(harness, subject, vehicleId);

        Assert.Equal(ownPlayer ? 2 : 1, harness.ClientWire.Sent.Count);
        var last = harness.ClientWire.Sent.Last();
        Assert.Equal(Opcode.SMSG_SET_VEHICLE_REC_ID, last.Opcode);
        AssertWire(last.Bytes, subject.To128(harness.Session.GameState), vehicleId);
        if (ownPlayer)
        {
            var move = harness.ClientWire.Sent.First();
            Assert.Equal(Opcode.SMSG_MOVE_SET_VEHICLE_REC_ID, move.Opcode);
            AssertWire(move.Bytes, subject.To128(harness.Session.GameState), vehicleId, sequence: 0);
        }
        Assert.Equal(0, harness.ServerWire.Count);
    }

    [Fact]
    public void RepeatedMountChanges_UseDistinctLocalSequences()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        harness.SetActivePlayer(player);
        harness.Session.GameState.ClientHasPlayerObject = true;
        Deliver(harness, player, 313);
        Deliver(harness, player, 0);
        var guid = player.To128(harness.Session.GameState);
        AssertWire(harness.ClientWire.Sent[0].Bytes, guid, 313, sequence: 0);
        AssertWire(harness.ClientWire.Sent[2].Bytes, guid, 0, sequence: 1);
    }

    [Fact]
    public void VehicleChanges_BeforePlayerCreate_FollowTheMovementHold()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        var guid = harness.SetActivePlayer(player);
        harness.Session.GameState.ClientHasPlayerObject = false;
        harness.Session.GameState.ClientKnownGuids.Remove(guid);
        Deliver(harness, player, 313);
        if (ModernVersion.Build != global::HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261)
        {
            Assert.Equal(2, harness.ClientWire.Sent.Count);
            return;
        }

        Assert.Empty(harness.ClientWire.Sent);
        Assert.Equal(2, harness.Session.ToClient.PendingCount);
        harness.Session.GameState.ClientHasPlayerObject = true;
        harness.Session.ToClient.Notify(OutboxEvent.GuidKnown(guid));
        Assert.Equal([Opcode.SMSG_MOVE_SET_VEHICLE_REC_ID, Opcode.SMSG_SET_VEHICLE_REC_ID],
            harness.ClientWire.Sent.Select(p => p.Opcode));
    }

    [Fact]
    public unsafe void LocalVehicleAck_IsHandledWithoutSendingALegacyAck()
    {
        Assert.True(GeneratedCmsgDispatch.Get(Opcode.CMSG_MOVE_SET_VEHICLE_REC_ID_ACK) != null);
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var ack = new MoveSetVehicleRecIDAck(default, default, 313);
        MovementSystem.HandleMoveSetVehicleRecIDAck(in ack, in ctx);
        Assert.Equal(0, harness.ServerWire.Count);
        Assert.Empty(harness.ClientWire.Sent);
    }

    [Fact]
    public unsafe void VehicleAck_DispatchConsumesTheCompleteWirePayload()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var guid = harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 77));
        using var payload = new WorldPacket();
        payload.WritePackedGuid128(guid);
        bool bitFlags = ModernVersion.Build is ClientVersionBuild.V1_14_0_40237
            or ClientVersionBuild.V2_5_2_39570;
        // Minimal stationary movement acknowledgement, authored independently
        // of the movement writer: flags, time, XYZO, pitch, elevation, counts.
        if (!bitFlags)
            for (int i = 0; i < 3; i++) payload.WriteUInt32(0);
        for (int i = 0; i < 9; i++) payload.WriteUInt32(0);
        if (bitFlags)
            for (int i = 0; i < 6; i++) payload.WriteUInt8(0); // 30 + 18 flag bits
        payload.WriteUInt8(0); // no transport, fall, spline or optional data
        payload.WriteUInt32(7); // sequence index
        payload.WriteUInt32(313); // vehicle ID
        var reader = new SpanPacketReader(payload.GetDataSpan());
        MoveSetVehicleRecIDAckCodec.Read(ref reader, out var decoded);
        Assert.Equal(guid, decoded.MoverGUID);
        Assert.Equal(7u, decoded.Ack.MoveCounter);
        Assert.Equal(313u, decoded.VehicleRecID);
        Assert.Equal(0, reader.Remaining);
        reader = new SpanPacketReader(payload.GetDataSpan());
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var handler = GeneratedCmsgDispatch.Get(Opcode.CMSG_MOVE_SET_VEHICLE_REC_ID_ACK);
        Assert.True(handler != null);
        handler(ref reader, in ctx);
        Assert.Equal(0, reader.Remaining);
        Assert.Equal(0, harness.ServerWire.Count);
    }

    // Login while mounted: the server seats the passengers before the proxy lets the player create
    // out. A seat on an object the client does not have yet is lost for good, so the move has to
    // come out behind the create, after the vehicle record that was registered ahead of it.
    [Fact]
    public void PassengerSeatMove_BeforePlayerCreate_FollowsTheVehicleRecord()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        var passenger = new WowGuid64(HighGuidTypeLegacy.Player, 78);
        var guid = harness.SetActivePlayer(player);
        harness.AddKnownObject(passenger, ObjectType.Player);
        harness.Session.GameState.ClientHasPlayerObject = false;
        harness.Session.GameState.ClientKnownGuids.Remove(guid);

        Deliver(harness, player, 313);
        DeliverCancelExpectedRide(harness);
        DeliverSeatMove(harness, passenger, vehicle: player, seat: 1);
        if (ModernVersion.Build != global::HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261)
        {
            Assert.IsType<MonsterMove>(harness.ClientWire.Sent.Last().Packet);
            return;
        }

        Assert.Empty(harness.ClientWire.Sent);
        harness.Session.GameState.ClientHasPlayerObject = true;
        harness.Session.GameState.ClientKnownGuids.Add(guid);
        harness.Session.ToClient.Notify(OutboxEvent.GuidKnown(guid));
        // By packet type: on 3.4.3 two opcode names share the monster-move value.
        Assert.Equal(
            [typeof(MoveSetVehicleRecID), typeof(SetVehicleRecID), typeof(OnCancelExpectedRideVehicleAura),
                typeof(MonsterMove)],
            harness.ClientWire.Sent.Select(p => p.Packet.GetType()));
    }

    [Fact]
    public void PassengerSeatMove_OnAnotherPlayersVehicle_IsNotHeld()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        var rider = new WowGuid64(HighGuidTypeLegacy.Player, 78);
        var passenger = new WowGuid64(HighGuidTypeLegacy.Player, 79);
        var guid = harness.SetActivePlayer(player);
        harness.AddKnownObject(rider, ObjectType.Player);
        harness.AddKnownObject(passenger, ObjectType.Player);
        harness.Session.GameState.ClientHasPlayerObject = false;
        harness.Session.GameState.ClientKnownGuids.Remove(guid);

        DeliverSeatMove(harness, passenger, vehicle: rider, seat: 2);

        Assert.IsType<MonsterMove>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(0, harness.Session.ToClient.PendingCount);
    }

    // The server sends it to a player whose mount just became a vehicle, and to one who asked to
    // board. Dropped, it left a "No handler" line behind every mount and every boarding.
    [Fact]
    public void CancelExpectedRide_IsForwardedAsAnEmptyPacket()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 77));
        harness.Session.GameState.ClientHasPlayerObject = true;

        DeliverCancelExpectedRide(harness);

        var sent = Assert.Single(harness.ClientWire.Sent);
        Assert.IsType<OnCancelExpectedRideVehicleAura>(sent.Packet);
        Assert.Equal(Opcode.SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA, sent.Opcode);
        Assert.Empty(sent.Bytes);
        Assert.Equal(0, harness.ServerWire.Count);
    }

    private static unsafe void DeliverCancelExpectedRide(LegacyHandlerHarness harness)
    {
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA, _ => { });
        harness.Deliver(Opcode.SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA, wire, p =>
        {
            var handler = GeneratedSmsgDispatch.Get(Opcode.SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA);
            Assert.True(handler != null);
            handler(harness.Client, p);
        });
    }

    private static void DeliverSeatMove(LegacyHandlerHarness harness, WowGuid64 mover, WowGuid64 vehicle, sbyte seat)
    {
        // 3.3.5a SMSG_MONSTER_MOVE_TRANSPORT, a one-point move onto the seat.
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_MONSTER_MOVE_TRANSPORT, p =>
        {
            p.WritePackedGuid(mover);
            p.WritePackedGuid(vehicle);
            p.WriteInt8(seat);
            p.WriteUInt8(0);            // toggle anim tier in transport
            p.WriteFloat(0); p.WriteFloat(0); p.WriteFloat(0); // start, seat-relative
            p.WriteUInt32(1);           // spline id
            p.WriteUInt8(0);            // spline type: normal
            p.WriteUInt32(0);           // spline flags
            p.WriteUInt32(1);           // duration
            p.WriteUInt32(1);           // point count
            p.WriteFloat(0); p.WriteFloat(0); p.WriteFloat(0); // destination
        });
        harness.Deliver(Opcode.SMSG_MONSTER_MOVE_TRANSPORT, wire, harness.Client.HandleMonsterMove);
    }

    // The vehicle record is what makes the client offer these two; without a translation it
    // sends them into a "No handler" line and the seat never changes.
    [Theory]
    [InlineData(Opcode.CMSG_RIDE_VEHICLE_INTERACT, "CMSG_PLAYER_VEHICLE_ENTER", 0x4A8u)]
    [InlineData(Opcode.CMSG_EJECT_PASSENGER, "CMSG_EJECT_PASSENGER", 0x4A9u)]
    public unsafe void SeatRequests_ReachTheLegacyServerAsAFullGuid(Opcode modern, string legacyName, uint legacyValue)
    {
        Assert.Equal(legacyValue, Opcodes.GetOpcodeValueForVersion(legacyName,
            global::HermesProxy.Enums.ClientVersionBuild.V3_3_5a_12340));

        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 77));
        var other = new WowGuid64(HighGuidTypeLegacy.Player, 78);
        WowGuid128 target = other.To128(harness.Session.GameState);

        // Framed like the wire, and read through the accessor the dispatch site uses.
        using var body = new WorldPacket(1u);
        body.WritePackedGuid128(target);
        byte[] payload = body.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        var reader = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());

        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var handler = GeneratedCmsgDispatch.Get(modern);
        Assert.True(handler != null);
        handler(ref reader, in ctx);

        Assert.Equal(0, reader.Remaining);
        var sent = Assert.Single(harness.ServerWire.Sent);
        Assert.Equal(legacyValue, sent.Opcode);
        using var legacy = new WorldPacket(1u);
        legacy.WriteGuid(other);
        Assert.Equal(legacy.GetData(), sent.Bytes);
        Assert.Empty(harness.ClientWire.Sent);
    }

    private static unsafe void Deliver(LegacyHandlerHarness harness, WowGuid64 subject, uint vehicleId)
    {
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_PLAYER_VEHICLE_DATA, p =>
        {
            p.WritePackedGuid(subject);
            p.WriteUInt32(vehicleId);
        });
        harness.Deliver(Opcode.SMSG_PLAYER_VEHICLE_DATA, wire, p =>
        {
            var handler = GeneratedSmsgDispatch.Get(Opcode.SMSG_PLAYER_VEHICLE_DATA);
            Assert.True(handler != null);
            handler(harness.Client, p);
        });
    }

    private static void AssertWire(byte[] bytes, WowGuid128 guid, uint vehicleId, uint? sequence = null)
    {
        using var reader = new WorldPacket(1, bytes);
        Assert.Equal(guid, reader.ReadPackedGuid128());
        if (sequence.HasValue)
            Assert.Equal(sequence.Value, reader.ReadUInt32());
        Assert.Equal(vehicleId, reader.ReadUInt32());
        Assert.False(reader.CanRead(1));
    }
}
