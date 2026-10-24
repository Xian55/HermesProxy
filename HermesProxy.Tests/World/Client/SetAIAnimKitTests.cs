using System.Linq;
using HermesProxy;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The AnimKit a unit idles in. A 4.3.4 server sends the unit's packed GUID and the kit
/// (TrinityCore 4.3.4 SetAIAnimKit::Write), the 4.4.2 client reads the same two as a
/// PackedGuid128 and a u16 (TrinityCore cata_classic).
/// </summary>
public sealed class SetAIAnimKitTests
{
    // The 3.4.3 client has no such packet, and no 3.3.5a server sends one.
    private static void SkipUnlessTheClientHasIt()
        => Assert.SkipUnless(ModernVersion.GetCurrentOpcode(Opcode.SMSG_SET_AI_ANIM_KIT) != 0,
            "the running client has no SMSG_SET_AI_ANIM_KIT");

    [Fact]
    public void TheCreatureAndItsKit_ReachTheClient()
    {
        SkipUnlessTheClientHasIt();
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(0x2226));
        harness.ClientWire.Sent.Clear();

        // as captured in Zul'Gurub: a Zanzili Zombie (mask 0xF3, GUID F130CB5700000240), kit 1288
        harness.Deliver(Opcode.SMSG_SET_AI_ANIM_KIT, [0, 0, 0xF3, 0x40, 0x02, 0x57, 0xCB, 0x30, 0xF1, 0x08, 0x05],
            harness.Client.HandleSetAIAnimKit);

        var sent = Assert.Single(harness.ClientWire.Sent);
        var animKit = Assert.IsType<SetAIAnimKit>(sent.Packet);
        Assert.Equal(new WowGuid64(0xF130CB5700000240).To128(harness.Session.GameState), animKit.Unit);
        Assert.Equal(1288, animKit.AnimKitID);
        Assert.Equal([0x08, 0x05], sent.Bytes.TakeLast(2));
    }

    [Fact]
    public void BothSerialisers_WriteTheSameBytes()
    {
        SkipUnlessTheClientHasIt();
        var animKit = new SetAIAnimKit { Unit = new WowGuid128(0x240, 0x2030000032D5C000), AnimKitID = 1288 };
        animKit.WritePacketData();
        byte[] written = animKit.GetDataSpan().ToArray();

        byte[] span = new byte[animKit.MaxSize];
        int length = animKit.WriteToSpan(span);

        Assert.Equal(written, span[..length]);
    }
}
