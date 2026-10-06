using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

// A 3.4.3 client lands after every seat move, and a landing on a boat puts it back on the boat.
// These pin the proxy's answer: gravity off around a seat taken from a deck, and nothing of it
// shown to the legacy server. Under any other client build every test here expects no change.
public class SeatGravityTests
{
    private static readonly WowGuid64 Player = new(HighGuidTypeLegacy.Player, 77);
    private static readonly WowGuid64 Rider = new(HighGuidTypeLegacy.Player, 78);
    private static readonly WowGuid64 Boat = new(HighGuidTypeLegacy.MOTransport, 7);

    private const uint EnterSeat = 0x00800000;
    private const uint ExitSeat = 0x01000000;

    [Fact]
    public void SeatTakenFromADeck_GetsGravityOffAheadOfTheSeatMove()
    {
        var harness = OnADeck(out WowGuid128 player);

        DeliverSeatMove(harness, Player, Rider, seat: 2, EnterSeat);

        if (!SeatGravity.Applies)
        {
            Assert.IsType<MonsterMove>(Assert.Single(harness.ClientWire.Sent).Packet);
            Assert.Equal(SeatGravityState.None, harness.Session.GameState.SeatGravity);
            return;
        }

        Assert.Equal(2, harness.ClientWire.Sent.Count);
        var gravity = Assert.IsType<MoveSetFlag>(harness.ClientWire.Sent[0].Packet);
        Assert.Equal(Opcode.SMSG_MOVE_DISABLE_GRAVITY, harness.ClientWire.Sent[0].Opcode);
        Assert.Equal(player, gravity.MoverGUID);
        Assert.Equal(SeatGravity.SequenceIndex, gravity.MoveCounter);
        Assert.IsType<MonsterMove>(harness.ClientWire.Sent[1].Packet);
        Assert.Equal(SeatGravityState.Held, harness.Session.GameState.SeatGravity);
    }

    [Fact]
    public void SeatTakenOnLand_IsLeftAlone()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(Player);
        harness.AddKnownObject(Rider, ObjectType.Player);

        DeliverSeatMove(harness, Player, Rider, seat: 2, EnterSeat);

        Assert.IsType<MonsterMove>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(SeatGravityState.None, harness.Session.GameState.SeatGravity);
    }

    [Fact]
    public void AnotherPlayersSeat_IsLeftAlone()
    {
        var harness = OnADeck(out _);
        var other = new WowGuid64(HighGuidTypeLegacy.Player, 79);
        harness.AddKnownObject(other, ObjectType.Player);

        DeliverSeatMove(harness, other, Rider, seat: 1, EnterSeat);

        Assert.IsType<MonsterMove>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(SeatGravityState.None, harness.Session.GameState.SeatGravity);
    }

    // The native order on leaving a seat that floats its passenger: gravity, unroot, exit move.
    [Fact]
    public void Unroot_GivesGravityBackFirst_AndTheExitMoveDoesNotRepeatIt()
    {
        var harness = OnADeck(out WowGuid128 player);
        DeliverSeatMove(harness, Player, Rider, seat: 2, EnterSeat);
        harness.ClientWire.Sent.Clear();

        DeliverFlagChange(harness, Opcode.SMSG_MOVE_UNROOT, Player);
        DeliverExitMove(harness, Player);

        if (!SeatGravity.Applies)
        {
            Assert.Equal(2, harness.ClientWire.Sent.Count);
            Assert.Equal(Opcode.SMSG_MOVE_UNROOT, harness.ClientWire.Sent[0].Opcode);
            Assert.IsType<MonsterMove>(harness.ClientWire.Sent[1].Packet);
            return;
        }

        Assert.Equal(3, harness.ClientWire.Sent.Count);
        Assert.Equal(Opcode.SMSG_MOVE_ENABLE_GRAVITY, harness.ClientWire.Sent[0].Opcode);
        var gravity = Assert.IsType<MoveSetFlag>(harness.ClientWire.Sent[0].Packet);
        Assert.Equal(player, gravity.MoverGUID);
        Assert.Equal(SeatGravity.SequenceIndex, gravity.MoveCounter);
        Assert.Equal(Opcode.SMSG_MOVE_UNROOT, harness.ClientWire.Sent[1].Opcode);
        Assert.IsType<MonsterMove>(harness.ClientWire.Sent[2].Packet);
        Assert.Equal(SeatGravityState.Releasing, harness.Session.GameState.SeatGravity);
    }

    [Fact]
    public void SeatSwitchWhileHeld_SendsNoSecondGravityPacket()
    {
        var harness = OnADeck(out _);
        DeliverSeatMove(harness, Player, Rider, seat: 2, EnterSeat);
        harness.ClientWire.Sent.Clear();

        DeliverSeatMove(harness, Player, Rider, seat: 1, EnterSeat);

        Assert.IsType<MonsterMove>(Assert.Single(harness.ClientWire.Sent).Packet);
    }

    // A seat the server floats its passenger in arrives as the server's own disable gravity,
    // ahead of the root. The proxy must not add a second one, nor take the flag away afterwards.
    [Fact]
    public void SeatTheServerFloats_IsLeftToTheServer()
    {
        var harness = OnADeck(out _);
        DeliverFlagChange(harness, Opcode.SMSG_MOVE_DISABLE_GRAVITY, Player);
        harness.ClientWire.Sent.Clear();

        DeliverSeatMove(harness, Player, Rider, seat: 2, EnterSeat);

        Assert.IsType<MonsterMove>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(SeatGravityState.None, harness.Session.GameState.SeatGravity);
    }

    [Fact]
    public void OwnGravityAcks_StopAtTheProxy_AndTheReleaseAckEndsTheHold()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var own = new MovementAckMessage(player, new MovementAck { MoveCounter = SeatGravity.SequenceIndex });

        harness.Session.GameState.SeatGravity = SeatGravityState.Held;
        MovementSystem.HandleMoveForceAck2(Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK, in own, in ctx);
        Assert.Equal(SeatGravityState.Held, harness.Session.GameState.SeatGravity);

        harness.Session.GameState.SeatGravity = SeatGravityState.Releasing;
        MovementSystem.HandleMoveForceAck2(Opcode.CMSG_MOVE_GRAVITY_ENABLE_ACK, in own, in ctx);
        Assert.Equal(SeatGravityState.None, harness.Session.GameState.SeatGravity);

        Assert.Equal(0, harness.ServerWire.Count);
    }

    // The same two opcodes answer a gravity change the server did ask for, and a root ack may
    // carry any index at all.
    [Theory]
    [InlineData(Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK, 5u)]
    [InlineData(Opcode.CMSG_MOVE_GRAVITY_ENABLE_ACK, 6u)]
    [InlineData(Opcode.CMSG_MOVE_FORCE_ROOT_ACK, SeatGravity.SequenceIndex)]
    public void ServerAskedAcks_StillReachTheServer(Opcode opcode, uint moveCounter)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var ack = new MovementAckMessage(player, new MovementAck { MoveCounter = moveCounter });

        MovementSystem.HandleMoveForceAck2(opcode, in ack, in ctx);

        Assert.Equal(1, harness.ServerWire.Count);
    }

    [Theory]
    [InlineData(SeatGravityState.Held)]
    [InlineData(SeatGravityState.Releasing)]
    public void HeldGravityFlag_IsNotShownToTheServer(SeatGravityState state)
    {
        byte[] plain = SentMovement(SeatGravityState.None, MovementFlagModern.Root);
        byte[] floating = SentMovement(SeatGravityState.None, MovementFlagModern.Root | MovementFlagModern.DisableGravity);
        byte[] held = SentMovement(state, MovementFlagModern.Root | MovementFlagModern.DisableGravity);

        // With no hold the flag is the server's own and goes through.
        Assert.NotEqual(plain, floating);
        Assert.Equal(plain, held);
    }

    // A 3.3.5a server blanks the transport in the first report that names a unit while it still
    // has the player down as the boat's passenger, and relays that to everyone. The repeat is the
    // one it takes as written, so the vehicle's rider is told about the seat.
    [Fact]
    public void SeatReportedFromADeck_IsSentToTheServerTwice()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        WowGuid128 rider = harness.AddKnownObject(Rider, ObjectType.Player);
        harness.Session.GameState.LastReportedTransportGuid = Boat.To128(harness.Session.GameState);

        ReportTransport(harness, player, rider, seat: 2);

        Assert.Equal(rider, harness.Session.GameState.LastReportedTransportGuid);
        if (!SeatGravity.Applies)
        {
            Assert.Single(harness.ServerWire.Sent);
            return;
        }

        Assert.Equal(2, harness.ServerWire.Sent.Count);
        Assert.Equal(harness.ServerWire.Sent[0].Opcode, harness.ServerWire.Sent[1].Opcode);
        Assert.Equal(harness.ServerWire.Sent[0].Bytes, harness.ServerWire.Sent[1].Bytes);

        // Once on the vehicle, later reports go out singly.
        ReportTransport(harness, player, rider, seat: 2);
        Assert.Equal(3, harness.ServerWire.Sent.Count);
    }

    [Theory]
    [InlineData(false, true)]   // land to a vehicle seat
    [InlineData(true, false)]   // boat to land
    [InlineData(false, false)]  // land to land
    public void OtherTransportChanges_AreSentOnce(bool fromBoat, bool toRider)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        WowGuid128 rider = harness.AddKnownObject(Rider, ObjectType.Player);
        if (fromBoat)
            harness.Session.GameState.LastReportedTransportGuid = Boat.To128(harness.Session.GameState);

        ReportTransport(harness, player, toRider ? rider : default, seat: 2);

        Assert.Single(harness.ServerWire.Sent);
    }

    [Fact]
    public void BoatToAnotherBoat_IsSentOnce()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        harness.Session.GameState.LastReportedTransportGuid = Boat.To128(harness.Session.GameState);
        WowGuid128 otherBoat = new WowGuid64(HighGuidTypeLegacy.MOTransport, 8).To128(harness.Session.GameState);

        ReportTransport(harness, player, otherBoat, seat: -1);

        Assert.Single(harness.ServerWire.Sent);
    }

    private static void ReportTransport(LegacyHandlerHarness harness, WowGuid128 player, WowGuid128 transport, sbyte seat)
    {
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var movement = new ClientPlayerMovement { Guid = player };
        if (transport != default)
            movement.MoveInfo.Transport = new TransportInfo { Guid = transport, Seat = seat };

        MovementSystem.HandlePlayerMove(Opcode.CMSG_MOVE_CHANGE_TRANSPORT, in movement, in ctx);
    }

    private static byte[] SentMovement(SeatGravityState state, MovementFlagModern flags)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        harness.Session.GameState.SeatGravity = state;
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var movement = new ClientPlayerMovement { Guid = player };
        movement.MoveInfo.Flags = flags;

        MovementSystem.HandlePlayerMove(Opcode.CMSG_MOVE_HEARTBEAT, in movement, in ctx);

        return Assert.Single(harness.ServerWire.Sent).Bytes;
    }

    private static LegacyHandlerHarness OnADeck(out WowGuid128 player)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        player = harness.SetActivePlayer(Player);
        harness.AddKnownObject(Rider, ObjectType.Player);
        // What the client's own movement last named as its transport.
        harness.Session.GameState.LastReportedTransportGuid = Boat.To128(harness.Session.GameState);
        return harness;
    }

    private static void DeliverSeatMove(LegacyHandlerHarness harness, WowGuid64 mover, WowGuid64 vehicle, sbyte seat, uint splineFlags)
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
            p.WriteUInt32(splineFlags);
            p.WriteUInt32(1);           // duration
            p.WriteUInt32(1);           // point count
            p.WriteFloat(0); p.WriteFloat(0); p.WriteFloat(0); // destination
        });
        harness.Deliver(Opcode.SMSG_MONSTER_MOVE_TRANSPORT, wire, harness.Client.HandleMonsterMove);
    }

    private static void DeliverExitMove(LegacyHandlerHarness harness, WowGuid64 mover)
    {
        // 3.3.5a SMSG_MONSTER_MOVE, the step off the vehicle onto the ground beside it.
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_ON_MONSTER_MOVE, p =>
        {
            p.WritePackedGuid(mover);
            p.WriteUInt8(0);            // toggle anim tier in transport
            p.WriteFloat(10); p.WriteFloat(20); p.WriteFloat(30); // start
            p.WriteUInt32(2);           // spline id
            p.WriteUInt8(0);            // spline type: normal
            p.WriteUInt32(ExitSeat);
            p.WriteUInt32(1);           // duration
            p.WriteUInt32(1);           // point count
            p.WriteFloat(11); p.WriteFloat(21); p.WriteFloat(30); // destination
        });
        harness.Deliver(Opcode.SMSG_ON_MONSTER_MOVE, wire, harness.Client.HandleMonsterMove);
    }

    private static void DeliverFlagChange(LegacyHandlerHarness harness, Opcode opcode, WowGuid64 mover)
    {
        byte[] wire = LegacyPacketBuilder.Build(opcode, p =>
        {
            p.WritePackedGuid(mover);
            p.WriteUInt32(3);           // the server's own movement counter
        });
        harness.Deliver(opcode, wire, harness.Client.HandleMoveForceFlagChange);
    }
}
