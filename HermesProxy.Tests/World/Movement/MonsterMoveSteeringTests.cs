using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// A vanilla or TBC server's run: a Runmode spline, which the proxy decorates with the modern
/// Classic flags. Steering is not one of them: the client turns a steering unit gradually, and a
/// creature's first chase reaches the proxy before the values update that flags it InCombat, so a
/// creature pulled from range swung round instead of facing its target (#74). Runs under the 1.12
/// and 2.4.3 pairs of <c>run-version-matrix.sh</c>.
/// </summary>
public sealed class MonsterMoveSteeringTests
{
    private const uint Runmode = 0x00000100;

    [Fact]
    public void ACreaturesChase_GoesOutWithoutSteering()
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 77));
        // Known, with no UNIT_FIELD_FLAGS yet: out of combat as far as the proxy can tell.
        harness.AddKnownObject(MovementScenarios.Creature, ObjectType.Unit);

        // A chase as VMaNGOS sends it: a normal spline with no facing.
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_ON_MONSTER_MOVE, p =>
        {
            p.WritePackedGuid(MovementScenarios.Creature);
            p.WriteFloat(10); p.WriteFloat(20); p.WriteFloat(30); // start
            p.WriteUInt32(5);           // spline id
            p.WriteUInt8(0);            // spline type: normal
            p.WriteUInt32(Runmode);
            p.WriteUInt32(2961);        // duration
            p.WriteUInt32(1);           // point count
            p.WriteFloat(25); p.WriteFloat(20); p.WriteFloat(30); // destination
        });
        harness.Deliver(Opcode.SMSG_ON_MONSTER_MOVE, wire, harness.Client.HandleMonsterMove);

        var move = Assert.IsType<MonsterMove>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(SplineFlagModern.Unknown5, move.MoveSpline.SplineFlags);
    }
}
