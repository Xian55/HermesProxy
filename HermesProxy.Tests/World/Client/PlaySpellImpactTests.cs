using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The light on a player who has just bought a spell from a trainer. A 3.3.5a server sends it as
/// SMSG_PLAY_SPELL_IMPACT, with the bytes of SMSG_PLAY_SPELL_VISUAL: a unit and a visual kit.
/// </summary>
public sealed class PlaySpellImpactTests
{
    [Fact]
    public unsafe void Impact_HasALegacyHandler()
        => Assert.True(GeneratedSmsgDispatch.Get(Opcode.SMSG_PLAY_SPELL_IMPACT) != null);

    [Fact]
    public void Impact_BecomesAVisualKitOnTheUnit()
    {
        if (WorldClient.IsCataLegacy)
            return;

        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var playerGuid = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        WowGuid128 player = harness.SetActivePlayer(playerGuid);

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_PLAY_SPELL_IMPACT, packet =>
        {
            packet.WriteGuid(playerGuid);
            packet.WriteUInt32(362);
        });
        harness.Deliver(Opcode.SMSG_PLAY_SPELL_IMPACT, wire, harness.Client.HandlePlaySpellVisualKit);

        var kit = Assert.IsType<PlaySpellVisualKit>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(player, kit.Unit);
        Assert.Equal(362u, kit.KitRecID);
    }
}
