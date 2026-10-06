using System.Linq;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// The transport part's VehicleRecID names the vehicle being ridden. A 3.3.5a server never sends
/// it, so the proxy has to supply it from what it was told about the ridden unit (issue #344).
/// What a native 3.4.3 server sends, from a capture: a passenger carries the id of the vehicle it
/// sits on, and a vehicle standing on a boat carries none.
/// </summary>
public class RiddenVehicleRecIdTests
{
    private const uint MammothKit = 312;

    private static readonly WowGuid64 Rider = new(HighGuidTypeLegacy.Player, 77);

    // Vehicles arrived with 3.0.2; the older legacy eras have neither the create flag nor the opcode.
    private static bool LegacyHasVehicles => LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056);

    private static LegacyHandlerHarness NewHarness()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        return harness;
    }

    private static LegacyTransport SeatOn(WowGuid64 ridden, sbyte seat)
        => new(ridden, new Vector3(0.5f, -1f, 2f), 0.25f, 999, seat, 0);

    private static void DeliverCreate(LegacyHandlerHarness harness, LegacyCreate create)
        => harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyCreateWire.Build(create), harness.Client.HandleUpdateObject);

    private static MovementInfo LastCreateMove(LegacyHandlerHarness harness)
        => harness.ClientWire.Sent.Select(s => s.Packet).OfType<UpdateObject>().Last()
            .ObjectUpdates.Single().CreateData!.MoveInfo!.Value;

    private static void DeliverVehicleRecord(LegacyHandlerHarness harness, WowGuid64 player, uint vehicleId)
    {
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_PLAYER_VEHICLE_DATA, p =>
        {
            p.WritePackedGuid(player);
            p.WriteUInt32(vehicleId);
        });
        harness.Deliver(Opcode.SMSG_PLAYER_VEHICLE_DATA, wire, harness.Client.HandlePlayerVehicleData);
    }

    private static uint SeatedHeartbeatRecId(LegacyHandlerHarness harness, WowGuid64 ridden)
    {
        byte[] wire = MovementScenarios.Heartbeat(new LegacyMove { Transport = SeatOn(ridden, 1) });
        harness.Deliver(Opcode.MSG_MOVE_HEARTBEAT, wire, harness.Client.HandleMovementMessages);
        var update = harness.ClientWire.Sent.Select(s => s.Packet).OfType<MoveUpdate>().Last();
        return update.MoveInfo.Transport!.Value.VehicleId;
    }

    [Fact]
    public void PassengerCreate_NamesTheVehicleItSitsOn()
    {
        if (!LegacyHasVehicles) return;
        var harness = NewHarness();
        DeliverCreate(harness, MovementScenarios.CreatureCreate with
        {
            Guid = MovementScenarios.Vehicle, Entry = 28670,
            Vehicle = new LegacyVehicle(MammothKit, 1.25f),
        });

        DeliverCreate(harness, MovementScenarios.CreatureCreate with
        {
            Living = new LegacyMove { Transport = SeatOn(MovementScenarios.Vehicle, 1) },
        });

        var seat = LastCreateMove(harness).Transport!.Value;
        Assert.Equal(MovementScenarios.Vehicle.To128(harness.Session.GameState), seat.Guid);
        Assert.Equal(MammothKit, seat.VehicleId);
    }

    [Fact]
    public void PassengerCreate_BeforeTheVehicleIsKnown_NamesNothing()
    {
        if (!LegacyHasVehicles) return;
        var harness = NewHarness();

        DeliverCreate(harness, MovementScenarios.CreatureCreate with
        {
            Living = new LegacyMove { Transport = SeatOn(MovementScenarios.Vehicle, 1) },
        });

        Assert.Equal(0u, LastCreateMove(harness).Transport!.Value.VehicleId);
    }

    [Fact]
    public void VehicleOnABoat_KeepsItsOwnIdOutOfItsTransportPart()
    {
        if (!LegacyHasVehicles) return;
        var harness = NewHarness();

        DeliverCreate(harness, MovementScenarios.CreatureCreate with
        {
            Guid = MovementScenarios.Vehicle, Entry = 28670,
            Living = MovementScenarios.FallingOnBoat,
            Vehicle = new LegacyVehicle(MammothKit, 1.25f),
        });

        var update = harness.ClientWire.Sent.Select(s => s.Packet).OfType<UpdateObject>().Last().ObjectUpdates.Single();
        Assert.Equal(MammothKit, update.CreateData!.VehicleId);
        Assert.Equal(0u, update.CreateData.MoveInfo!.Value.Transport!.Value.VehicleId);
    }

    [Fact]
    public void VehicleRidingAVehicle_NamesTheOneBeneathIt()
    {
        if (!LegacyHasVehicles) return;
        var harness = NewHarness();
        DeliverCreate(harness, MovementScenarios.CreatureCreate with
        {
            Guid = MovementScenarios.Vehicle, Entry = 28670,
            Vehicle = new LegacyVehicle(MammothKit, 1.25f),
        });

        var turret = new WowGuid64(HighGuidTypeLegacy.Vehicle, 28366, 56);
        DeliverCreate(harness, MovementScenarios.CreatureCreate with
        {
            Guid = turret, Entry = 28366,
            Living = new LegacyMove { Transport = SeatOn(MovementScenarios.Vehicle, 2) },
            Vehicle = new LegacyVehicle(244, 0f),
        });

        var update = harness.ClientWire.Sent.Select(s => s.Packet).OfType<UpdateObject>().Last().ObjectUpdates.Single();
        Assert.Equal(244u, update.CreateData!.VehicleId);
        Assert.Equal(MammothKit, update.CreateData.MoveInfo!.Value.Transport!.Value.VehicleId);
    }

    // A player who mounts after being loaded has no create to learn the id from.
    [Fact]
    public void SeatedPlayerMove_FollowsThePlayerVehicleRecord()
    {
        if (!LegacyHasVehicles) return;
        var harness = NewHarness();
        harness.AddKnownObject(Rider, ObjectType.Player);
        harness.AddKnownObject(MovementScenarios.OtherPlayer, ObjectType.Player);

        Assert.Equal(0u, SeatedHeartbeatRecId(harness, Rider));

        DeliverVehicleRecord(harness, Rider, MammothKit);
        Assert.Equal(MammothKit, SeatedHeartbeatRecId(harness, Rider));

        DeliverVehicleRecord(harness, Rider, 0);
        Assert.Equal(0u, SeatedHeartbeatRecId(harness, Rider));
    }

    [Fact]
    public void DestroyedVehicle_IsForgotten()
    {
        if (!LegacyHasVehicles) return;
        var harness = NewHarness();
        DeliverCreate(harness, MovementScenarios.CreatureCreate with
        {
            Guid = MovementScenarios.Vehicle, Entry = 28670,
            Vehicle = new LegacyVehicle(MammothKit, 1.25f),
        });

        byte[] destroy = LegacyPacketBuilder.Build(Opcode.SMSG_DESTROY_OBJECT, p =>
        {
            p.WriteGuid(MovementScenarios.Vehicle);
            p.WriteUInt8(0);
        });
        harness.Deliver(Opcode.SMSG_DESTROY_OBJECT, destroy, harness.Client.HandleDestroyObject);

        Assert.Equal(0u, harness.Session.GameState.GetVehicleRecId(
            MovementScenarios.Vehicle.To128(harness.Session.GameState)));
    }

    [Fact]
    public void LegacyErasWithoutSeats_NeverLookAVehicleUp()
    {
        if (LegacyHasVehicles) return;
        var harness = NewHarness();
        harness.Session.GameState.SetVehicleRecId(
            MovementScenarios.Boat.To128(harness.Session.GameState), MammothKit);

        byte[] wire = MovementScenarios.Heartbeat(MovementScenarios.FallingOnBoat);
        harness.Deliver(Opcode.MSG_MOVE_HEARTBEAT, wire, harness.Client.HandleMovementMessages);

        var update = harness.ClientWire.Sent.Select(s => s.Packet).OfType<MoveUpdate>().Last();
        Assert.Equal(0u, update.MoveInfo.Transport!.Value.VehicleId);
    }
}
