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
/// A direction pressed in the air is only pending on the modern client until it lands. A vanilla
/// client reports the direction itself, and VMaNGOS kicks a move start without it
/// (Anticheat.BadMoveStart), so towards a vanilla server the pending direction goes out as the
/// direction. Runs under the 1.12 pairs of <c>run-version-matrix.sh</c>; the other eras keep their
/// own handling (3.3.5a has pending bits of its own), which the golden snapshots hold.
/// </summary>
public sealed class PendingDirectionTests
{
    private static readonly WowGuid64 Player = new(HighGuidTypeLegacy.Player, 77);

    [Theory]
    [InlineData(Opcode.CMSG_MOVE_START_FORWARD, MovementFlagModern.PendingForward, MovementFlagModern.Forward)]
    [InlineData(Opcode.CMSG_MOVE_START_BACKWARD, MovementFlagModern.PendingBackward, MovementFlagModern.Backward)]
    [InlineData(Opcode.CMSG_MOVE_START_STRAFE_LEFT, MovementFlagModern.PendingStrafeLeft, MovementFlagModern.StrafeLeft)]
    [InlineData(Opcode.CMSG_MOVE_START_STRAFE_RIGHT, MovementFlagModern.PendingStrafeRight, MovementFlagModern.StrafeRight)]
    public void ADirectionStartedInTheAir_GoesOutAsTheDirection(Opcode opcode, MovementFlagModern pending, MovementFlagModern direction)
    {
        if (!LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;

        // As the client sends it: running forward, falling, a direction pressed on the way.
        const MovementFlagModern airborne = MovementFlagModern.Forward | MovementFlagModern.Falling;

        Assert.Equal(Sent(opcode, airborne | direction), Sent(opcode, airborne | pending));
    }

    private static byte[] Sent(Opcode opcode, MovementFlagModern flags)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false, recordServerPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var movement = new ClientPlayerMovement
        {
            Guid = player,
            MoveInfo = new MovementInfo { Flags = flags, MoveTime = 2000, FallTime = 120, JumpVerticalSpeed = -2f },
        };

        MovementSystem.HandlePlayerMove(opcode, in movement, in ctx);

        return Assert.Single(harness.ServerWire.Sent).Bytes;
    }
}
