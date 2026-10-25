using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A leap of a 3.3.5a server (a jump spell, Death Grip): the spline's Trajectory flag with its
/// gravity and the time the arc starts. The modern client draws the arc from the Parabolic flag
/// and the spline's jump data, which have to come together.
/// </summary>
public sealed class LeapMonsterMoveTests
{
    private static readonly WowGuid64 Leaper = new(HighGuidTypeLegacy.Player, 78);

    private const uint Trajectory = 0x00000800;
    private const uint Knockback = 0x00004000;

    private static MonsterMove Deliver(uint splineFlags, float gravity, uint startTime)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 77));
        harness.AddKnownObject(Leaper, ObjectType.Player);

        // 3.3.5a SMSG_MONSTER_MOVE as MoveSplineInit::Launch writes a MoveJump: one point
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_ON_MONSTER_MOVE, p =>
        {
            p.WritePackedGuid(Leaper);
            p.WriteUInt8(0);            // toggle anim tier in transport
            p.WriteFloat(10); p.WriteFloat(20); p.WriteFloat(30); // start
            p.WriteUInt32(5);           // spline id
            p.WriteUInt8(0);            // spline type: normal
            p.WriteUInt32(splineFlags);
            p.WriteUInt32(900);         // duration
            if ((splineFlags & Trajectory) != 0)
            {
                p.WriteFloat(gravity);
                p.WriteUInt32(startTime);
            }
            p.WriteUInt32(1);           // point count
            p.WriteFloat(25); p.WriteFloat(20); p.WriteFloat(30); // destination
        });
        harness.Deliver(Opcode.SMSG_ON_MONSTER_MOVE, wire, harness.Client.HandleMonsterMove);
        return Assert.IsType<MonsterMove>(Assert.Single(harness.ClientWire.Sent).Packet);
    }

    [Fact]
    public void ALeap_GoesOnAsAnArcWithItsJumpData()
    {
        if (WorldClient.IsCataLegacy)
            return;

        MonsterMove move = Deliver(Trajectory | Knockback, 19.29f, 0);

        Assert.True(move.MoveSpline.SplineFlags.HasFlag(SplineFlagModern.Parabolic));
        Assert.Equal(19.29f, move.MoveSpline.JumpGravity);
        Assert.Equal(0u, move.MoveSpline.JumpStartTime);
        // 3.3.5a's Trajectory bit is modern CatmullRom's: it must not come through as that
        Assert.False(move.MoveSpline.SplineFlags.HasFlag(SplineFlagModern.CatmullRom));
        Assert.Equal(900u, move.MoveSpline.SplineTimeFull);
    }

    [Fact]
    public void AGroundMove_HasNeitherTheFlagNorJumpData()
    {
        if (WorldClient.IsCataLegacy)
            return;

        MonsterMove move = Deliver(0, 0, 0);

        Assert.False(move.MoveSpline.SplineFlags.HasFlag(SplineFlagModern.Parabolic));
        Assert.Null(move.MoveSpline.JumpGravity);
    }
}
