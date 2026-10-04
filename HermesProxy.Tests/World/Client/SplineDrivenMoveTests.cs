using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Issue #339: AzerothCore playerbots heartbeat while the server runs their spline, and the
/// V3_4_3 client drops the spline when that heartbeat reaches it as a MoveUpdate.
/// </summary>
public class SplineDrivenMoveTests
{
    // Flags from the reporter's capture: Forward | SplineEnabled mid-spline, 0 once it has ended.
    private const uint ForwardOnSpline = 0x08000001;
    private const uint Forward = 0x00000001;

    [Theory]
    [InlineData(ForwardOnSpline, ClientVersionBuild.V3_4_3_54261, true)]
    [InlineData((uint)MovementFlagWotLK.SplineEnabled, ClientVersionBuild.V3_4_3_54261, true)]
    [InlineData(Forward, ClientVersionBuild.V3_4_3_54261, false)]
    [InlineData(0u, ClientVersionBuild.V3_4_3_54261, false)]
    // No vanilla or TBC backend has been seen sending the flag, so those clients keep the old path.
    [InlineData(ForwardOnSpline, ClientVersionBuild.V1_14_2_42597, false)]
    [InlineData(ForwardOnSpline, ClientVersionBuild.V2_5_3_41750, false)]
    public void IsSplineDrivenMove_OnlyForV343WithSplineEnabled(uint legacyFlags, ClientVersionBuild modernBuild, bool expected)
        => Assert.Equal(expected, WorldClient.IsSplineDrivenMove(legacyFlags, modernBuild));

    // The suite runs as a V1_14 client, so this is the path every build but V3_4_3 takes.
    [Theory]
    [InlineData(Forward)]
    [InlineData(ForwardOnSpline)]
    public void Heartbeat_OutsideV343_IsForwardedWithoutTheSplineBit(uint legacyFlags)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var legacyGuid = new WowGuid64(HighGuidTypeLegacy.Player, 13053);
        var guid = legacyGuid.To128(harness.Session.GameState);

        harness.Deliver(Opcode.MSG_MOVE_HEARTBEAT,
            LegacyPacketBuilder.Build(Opcode.MSG_MOVE_HEARTBEAT, packet =>
            {
                packet.WritePackedGuid(legacyGuid);
                packet.WriteUInt32(legacyFlags);
                packet.WriteUInt16(0);          // extra flags
                packet.WriteUInt32(3080);       // move time
                packet.WriteFloat(1199.4f);
                packet.WriteFloat(1472.1f);
                packet.WriteFloat(307.5f);
                packet.WriteFloat(1.5f);
                packet.WriteUInt32(0);          // fall time
            }),
            harness.Client.HandleMovementMessages);

        var sent = Assert.Single(harness.ClientWire.Sent);
        Assert.Equal(Opcode.SMSG_MOVE_UPDATE, sent.Opcode);
        var update = Assert.IsType<MoveUpdate>(sent.Packet);
        Assert.Equal(guid, update.MoverGUID);
        Assert.Equal(MovementFlagModern.Forward, update.MoveInfo.Flags);
    }
}
