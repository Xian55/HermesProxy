using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>The time a temporary weapon enchant has left, sent when one is applied and at login.</summary>
public sealed class ItemEnchantTimeUpdateTests
{
    [Fact]
    public unsafe void EnchantTimeUpdate_HasALegacyHandler()
        => Assert.True(GeneratedSmsgDispatch.Get(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE) != null);

    [Fact]
    public void EnchantTimeUpdate_WaitsForThePlayerWhoseItemsComeWithIt()
        => Assert.True(new ItemEnchantTimeUpdate().NeedsObject(new WowGuid128()));

    [Fact]
    public void EnchantTimeUpdate_NamesTheItemTheSlotTheTimeAndTheOwner()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var playerGuid = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        WowGuid128 player = harness.SetActivePlayer(playerGuid);
        var itemGuid = new WowGuid64(HighGuidTypeLegacy.Item, 78);
        WowGuid128 item = harness.AddKnownObject(itemGuid, ObjectType.Item);

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE, packet =>
        {
            packet.WriteGuid(itemGuid);
            packet.WriteUInt32(1);
            packet.WriteUInt32(1800);
            packet.WriteGuid(playerGuid);
        });
        harness.Deliver(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE, wire, harness.Client.HandleItemEnchantTimeUpdate);

        var update = Assert.IsType<ItemEnchantTimeUpdate>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(item, update.ItemGuid);
        Assert.Equal(1u, update.Slot);
        Assert.Equal(1800u, update.DurationLeft);
        Assert.Equal(player, update.OwnerGuid);
    }

    [Fact]
    public void EnchantTimeUpdate_SentAtLogin_WaitsForThePlayerCreate()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var playerGuid = MovementScenarios.ActivePlayer;
        WowGuid128 player = harness.SetActivePlayer(playerGuid);
        harness.Session.GameState.ClientHasPlayerObject = false;
        var itemGuid = new WowGuid64(HighGuidTypeLegacy.Item, 78);
        harness.AddKnownObject(itemGuid, ObjectType.Item);

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE, packet =>
        {
            packet.WriteGuid(itemGuid);
            packet.WriteUInt32(1);
            packet.WriteUInt32(1800);
            packet.WriteGuid(playerGuid);
        });
        harness.Deliver(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE, wire, harness.Client.HandleItemEnchantTimeUpdate);

        Assert.Empty(harness.ClientWire.Sent);

        harness.Session.GameState.ClientHasPlayerObject = true;
        harness.Session.ToClient.Notify(OutboxEvent.GuidKnown(player));

        Assert.IsType<ItemEnchantTimeUpdate>(Assert.Single(harness.ClientWire.Sent).Packet);
    }
}
