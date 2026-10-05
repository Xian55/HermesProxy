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
