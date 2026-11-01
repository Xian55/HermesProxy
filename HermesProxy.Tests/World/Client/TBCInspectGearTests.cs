using System.Collections.Frozen;
using System.IO;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Inspecting a player on a 2.4.3 server. That server has no inspect gear packet: the only copy
/// of an item's enchantments and random property is the visible-item block it sends in the player
/// update, so the inspect window must be rebuilt from those fields.
/// </summary>
/// <remarks>Runs only under a 2.4.3 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public sealed class TBCInspectGearTests
{
    private const uint Helm = 9_400_000;
    private const uint Cloak = 9_400_001;
    private const uint PermanentEnchant = 3820;
    private const uint SocketOneGemEnchant = 2686;     // Gems2.csv -> item 23233
    private const uint SocketTwoGemEnchant = 2687;     // Gems2.csv -> item 23234
    private const int HelmRandomProperty = 696;
    private const int CloakRandomSuffix = -39;

    private static bool IsTBC => LegacyVersion.ExpansionVersion == 2;

    private static InspectResult Inspect()
    {
        if (!GameData.Gems.ContainsKey(SocketOneGemEnchant))
            GameData.Gems = GameData.ReadGems(Path.Combine("CSV", "Gems2.csv")).ToFrozenDictionary();

        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 1));
        var inspected = new WowGuid64(HighGuidTypeLegacy.Player, 0xFAC);
        var guid = harness.AddKnownObject(inspected, ObjectType.Player);
        harness.Session.GameState.CachedPlayers[guid] = new PlayerCache { Name = "Shammy", ClassId = Class.Shaman };

        int visibleBase = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_0);
        int properties = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_PROPERTIES);
        lock (harness.Session.GameState.ObjectCacheLock)
        {
            var updates = harness.Session.GameState.ObjectCacheLegacy[guid];
            // head: permanent enchantment, two socketed gems and a random property
            updates[visibleBase] = new UpdateField(Helm);
            updates[visibleBase + 1] = new UpdateField(PermanentEnchant);
            // base + 1 + enchantment slot: 0 permanent, 1 temporary, 2 first socket, 3 second.
            updates[visibleBase + 3] = new UpdateField(SocketOneGemEnchant);
            updates[visibleBase + 4] = new UpdateField(SocketTwoGemEnchant);
            updates[properties] = new UpdateField((uint)HelmRandomProperty);
            updates[properties + 1] = new UpdateField(98u);
            // back: a random suffix, with the signed id in the low short
            int cloakBase = visibleBase + 14 * 16;
            updates[cloakBase] = new UpdateField(Cloak);
            updates[cloakBase + 12] = new UpdateField(unchecked((uint)(int)(short)CloakRandomSuffix));
            updates[cloakBase + 13] = new UpdateField(362u);
        }

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_INSPECT_TALENT, packet =>
        {
            packet.WritePackedGuid(inspected);
            packet.WriteUInt32(0);                  // unspent talent points
            for (int i = 0; i < 0x3D; i++)          // the 2.4.3 talent bit array
                packet.WriteUInt8(0);
        });
        harness.Deliver(Opcode.SMSG_INSPECT_TALENT, wire, harness.Client.HandleInspectResult);
        return Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<InspectResult>());
    }

    [Fact]
    public void TheItemsAreTheOnesInTheVisibleItemBlock()
    {
        if (!IsTBC)
            return;

        var items = Inspect().DisplayInfo.Items;

        Assert.Equal([(byte)0, (byte)14], items.Select(i => i.Index));
        Assert.Equal([Helm, Cloak], items.Select(i => i.Item.ItemID));
    }

    [Fact]
    public void TheEnchantments_AreTheVisibleItemFields()
    {
        if (!IsTBC)
            return;

        var helm = Inspect().DisplayInfo.Items[0];

        Assert.Equal([(PermanentEnchant, (byte)0), (SocketOneGemEnchant, (byte)2), (SocketTwoGemEnchant, (byte)3)],
            helm.Enchants.Select(e => (e.Id, e.Index)));
    }

    [Fact]
    public void TheGems_AreTheGemItemsInTheirSockets()
    {
        if (!IsTBC)
            return;

        var helm = Inspect().DisplayInfo.Items[0];

        Assert.Equal([((byte)0, 23233u), ((byte)1, 23234u)], helm.Gems.Select(g => (g.Slot, g.Item.ItemID)));
    }

    [Fact]
    public void ARandomProperty_KeepsItsIdAndSuffixFactor()
    {
        if (!IsTBC)
            return;

        var helm = Inspect().DisplayInfo.Items[0];

        Assert.Equal((uint)HelmRandomProperty, helm.Item.RandomPropertiesID);
        Assert.Equal(98u, helm.Item.RandomPropertiesSeed);
    }

    [Fact]
    public void ARandomSuffix_KeepsItsSign()
    {
        if (!IsTBC)
            return;

        var cloak = Inspect().DisplayInfo.Items[1];

        Assert.Equal(unchecked((uint)CloakRandomSuffix), cloak.Item.RandomPropertiesID);
        Assert.Equal(362u, cloak.Item.RandomPropertiesSeed);
    }
}
