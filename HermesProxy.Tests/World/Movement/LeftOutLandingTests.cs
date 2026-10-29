using System.Linq;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// When a charm on the player ends, the client reports the spline done falling and mostly lands
/// right after, but not always. VMaNGOS kicks a player whose next report stops falling with
/// anything but a landing (Anticheat.BadFallStop, #528), so towards a vanilla server the proxy
/// sends the landing the client left out. Runs under the 1.12 pairs of
/// <c>run-version-matrix.sh</c>; under any other legacy build every report goes out alone.
/// </summary>
public sealed class LeftOutLandingTests
{
    private static readonly WowGuid64 Player = new(HighGuidTypeLegacy.Player, 77);

    private static bool Vanilla => LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180);

    [Fact]
    public void AFallTheClientNeverLanded_IsLandedBeforeTheNextReport()
    {
        var (harness, ctx, player) = InTheWorld();

        SplineDoneFalling(player, in ctx);
        Move(Opcode.CMSG_MOVE_START_FORWARD, player, MovementFlagModern.Forward, in ctx);

        if (!Vanilla)
        {
            Assert.Equal([Legacy(Opcode.CMSG_MOVE_SPLINE_DONE), Legacy(Opcode.MSG_MOVE_START_FORWARD)], Opcodes(harness));
            return;
        }
        Assert.Equal(
            [Legacy(Opcode.CMSG_MOVE_SPLINE_DONE), Legacy(Opcode.MSG_MOVE_FALL_LAND), Legacy(Opcode.MSG_MOVE_START_FORWARD)],
            Opcodes(harness));
        // The landing is the report it comes before, under the landing's opcode.
        Assert.Equal(harness.ServerWire.Sent[2].Bytes, harness.ServerWire.Sent[1].Bytes);
    }

    [Fact]
    public void AFallTheClientLanded_GetsNoSecondLanding()
    {
        var (harness, ctx, player) = InTheWorld();

        SplineDoneFalling(player, in ctx);
        Move(Opcode.CMSG_MOVE_FALL_LAND, player, MovementFlagModern.None, in ctx);
        Move(Opcode.CMSG_MOVE_START_FORWARD, player, MovementFlagModern.Forward, in ctx);

        Assert.Equal(
            [Legacy(Opcode.CMSG_MOVE_SPLINE_DONE), Legacy(Opcode.MSG_MOVE_FALL_LAND), Legacy(Opcode.MSG_MOVE_START_FORWARD)],
            Opcodes(harness));
    }

    // VMaNGOS runs the same check on a speed change ack, which can be the next report.
    [Fact]
    public void ASpeedAckAfterAFallTheClientNeverLanded_IsLandedFirst()
    {
        var (harness, ctx, player) = InTheWorld();

        SplineDoneFalling(player, in ctx);
        var ack = new MovementSpeedAck(player, new MovementAck { MoveCounter = 51 }, 7f);
        MovementSystem.HandleMoveForceSpeedChangeAck(Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK, in ack, in ctx);

        uint[] expected = Vanilla
            ? [Legacy(Opcode.CMSG_MOVE_SPLINE_DONE), Legacy(Opcode.MSG_MOVE_FALL_LAND), Legacy(Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK)]
            : [Legacy(Opcode.CMSG_MOVE_SPLINE_DONE), Legacy(Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK)];
        Assert.Equal(expected, Opcodes(harness));
    }

    private static (LegacyHandlerHarness Harness, SessionContext Ctx, WowGuid128 Player) InTheWorld()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        return (harness, new SessionContext(harness.Session, socket: null, harness.Client), player);
    }

    // As the reporter's client sent it: falling, with no fall time and no speed.
    private static void SplineDoneFalling(WowGuid128 player, in SessionContext ctx)
    {
        var done = new MoveSplineDone(player, new MovementInfo { Flags = MovementFlagModern.Falling, MoveTime = 1000 }, 5271815);
        MovementSystem.HandleMoveSplineDone(in done, in ctx);
    }

    private static void Move(Opcode opcode, WowGuid128 player, MovementFlagModern flags, in SessionContext ctx)
    {
        var movement = new ClientPlayerMovement { Guid = player, MoveInfo = new MovementInfo { Flags = flags, MoveTime = 2000 } };
        MovementSystem.HandlePlayerMove(opcode, in movement, in ctx);
    }

    private static uint Legacy(Opcode opcode) => LegacyVersion.GetCurrentOpcode(opcode);

    private static uint[] Opcodes(LegacyHandlerHarness harness) => [.. harness.ServerWire.Sent.Select(p => p.Opcode)];
}
