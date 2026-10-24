using System.Linq;
using HermesProxy;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A unit playing an AnimKit once. A 4.3.4 server sends the unit's packed GUID and the kit
/// (TrinityCore 4.3.4 PlayOneShotAnimKit::Write), the 4.4.2 client reads the same two as a
/// PackedGuid128 and a u16 (TrinityCore cata_classic).
/// </summary>
public sealed class PlayOneShotAnimKitTests
{
    // The 3.4.3 client has no such packet, and no 3.3.5a server sends one.
    private static void SkipUnlessTheClientHasIt()
        => Assert.SkipUnless(ModernVersion.GetCurrentOpcode(Opcode.SMSG_PLAY_ONE_SHOT_ANIM_KIT) != 0,
            "the running client has no SMSG_PLAY_ONE_SHOT_ANIM_KIT");

    [Fact]
    public void TheUnitAndItsKit_ReachTheClient()
    {
        SkipUnlessTheClientHasIt();
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(0x2226));
        harness.ClientWire.Sent.Clear();

        // as captured: mask 0x03, GUID bytes 26 22, kit 770
        harness.Deliver(Opcode.SMSG_PLAY_ONE_SHOT_ANIM_KIT, [0, 0, 0x03, 0x26, 0x22, 0x02, 0x03],
            harness.Client.HandlePlayOneShotAnimKit);

        var sent = Assert.Single(harness.ClientWire.Sent);
        var animKit = Assert.IsType<PlayOneShotAnimKit>(sent.Packet);
        Assert.Equal(new WowGuid64(0x2226).To128(harness.Session.GameState), animKit.Unit);
        Assert.Equal(770, animKit.AnimKitID);
        Assert.Equal([0x02, 0x03], sent.Bytes.TakeLast(2));
    }

    [Fact]
    public void BothSerialisers_WriteTheSameBytes()
    {
        SkipUnlessTheClientHasIt();
        var animKit = new PlayOneShotAnimKit { Unit = new WowGuid128(0x2226, 0x0800000000000000), AnimKitID = 770 };
        animKit.WritePacketData();
        byte[] written = animKit.GetDataSpan().ToArray();

        byte[] span = new byte[animKit.MaxSize];
        int length = animKit.WriteToSpan(span);

        Assert.Equal(written, span[..length]);
    }
}
