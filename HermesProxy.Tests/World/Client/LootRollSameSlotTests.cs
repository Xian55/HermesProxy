using System.Linq;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A server numbers a roll's slot within its corpse, so two corpses looted together each have a
/// slot 0. Its votes and its winner name no loot object, and the proxy found the object by the slot
/// alone: everything then named the newest roll, and the end of the first item's roll closed the
/// second item's dialog while the server still waited for a vote on it. From a 3.4.3 session on
/// AzerothCore, with items 37116 and 37115.
/// </summary>
public sealed class LootRollSameSlotTests
{
    private const byte Slot = 0;
    private const uint FirstItem = 37116;
    private const uint SecondItem = 37115;

    private static readonly WowGuid64 Player = new(HighGuidTypeLegacy.Player, 703);
    private static readonly WowGuid64 First = new(HighGuidTypeLegacy.Item, 0x41F5E1);
    private static readonly WowGuid64 Second = new(HighGuidTypeLegacy.Item, 0x41F5E2);
    private static readonly WowGuid64 NoObject = new(HighGuidTypeLegacy.Item, 0);

    private static LegacyHandlerHarness NewHarness()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(Player);
        return harness;
    }

    private static void StartRoll(LegacyHandlerHarness harness, WowGuid64 lootObj, uint itemId)
    {
        harness.Deliver(Opcode.SMSG_LOOT_START_ROLL, LegacyPacketBuilder.Build(Opcode.SMSG_LOOT_START_ROLL, p =>
        {
            p.WriteGuid(lootObj);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                p.WriteUInt32(595);
            p.WriteUInt32(Slot);
            p.WriteUInt32(itemId);
            p.WriteUInt32(0);
            p.WriteUInt32(0);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                p.WriteUInt32(1);
            p.WriteUInt32(60000);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                p.WriteUInt8(7);
        }), harness.Client.HandleLootStartRoll);
    }

    private static void Vote(LegacyHandlerHarness harness, uint itemId)
    {
        harness.Deliver(Opcode.SMSG_LOOT_ROLL, LegacyPacketBuilder.Build(Opcode.SMSG_LOOT_ROLL, p =>
        {
            p.WriteGuid(NoObject);
            p.WriteUInt32(Slot);
            p.WriteGuid(Player);
            p.WriteUInt32(itemId);
            p.WriteUInt32(0);
            p.WriteUInt32(0);
            if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
                p.WriteInt32(0);
            else
                p.WriteUInt8(0);
            p.WriteUInt8(1);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                p.WriteUInt8(0);
        }), harness.Client.HandleLootRoll);
    }

    private static void Win(LegacyHandlerHarness harness, uint itemId)
    {
        harness.Deliver(Opcode.SMSG_LOOT_ROLL_WON, LegacyPacketBuilder.Build(Opcode.SMSG_LOOT_ROLL_WON, p =>
        {
            p.WriteGuid(NoObject);
            p.WriteUInt32(Slot);
            p.WriteUInt32(itemId);
            p.WriteUInt32(0);
            p.WriteUInt32(0);
            p.WriteGuid(Player);
            if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
                p.WriteInt32(19);
            else
                p.WriteUInt8(19);
            p.WriteUInt8(1);
        }), harness.Client.HandleLootRollWon);
    }

    private static void AllPassed(LegacyHandlerHarness harness, WowGuid64 lootObj, uint itemId)
    {
        harness.Deliver(Opcode.SMSG_LOOT_ALL_PASSED, LegacyPacketBuilder.Build(Opcode.SMSG_LOOT_ALL_PASSED, p =>
        {
            p.WriteGuid(lootObj);
            p.WriteUInt32(Slot);
            p.WriteUInt32(itemId);
            p.WriteUInt32(0);
            p.WriteUInt32(0);
        }), harness.Client.HandleLootAllPassed);
    }

    private static T[] Sent<T>(LegacyHandlerHarness harness) where T : ServerPacket
        => [.. harness.ClientWire.Sent.Select(s => s.Packet).OfType<T>()];

    [Fact]
    public void HandleLootRoll_TwoCorpsesInOneSlot_NamesTheRollOfItsItem()
    {
        var harness = NewHarness();
        StartRoll(harness, First, FirstItem);
        StartRoll(harness, Second, SecondItem);

        Vote(harness, FirstItem);
        Vote(harness, SecondItem);

        var votes = Sent<LootRollBroadcast>(harness);
        Assert.Equal(First.ToLootGuid(), votes[0].LootObj);
        Assert.Equal(Second.ToLootGuid(), votes[1].LootObj);
    }

    [Fact]
    public void HandleLootRollWon_TwoCorpsesInOneSlot_CompletesOnlyTheRollThatEnded()
    {
        var harness = NewHarness();
        StartRoll(harness, First, FirstItem);
        StartRoll(harness, Second, SecondItem);

        Win(harness, FirstItem);

        Assert.Equal(First.ToLootGuid(), Assert.Single(Sent<LootRollWon>(harness)).LootObj);
        Assert.Equal(First.ToLootGuid(), Assert.Single(Sent<LootRollsComplete>(harness)).LootObj);
    }

    [Fact]
    public void HandleLootRoll_AfterTheOtherRollEnded_StillNamesItsRoll()
    {
        var harness = NewHarness();
        StartRoll(harness, First, FirstItem);
        StartRoll(harness, Second, SecondItem);
        Win(harness, FirstItem);

        Vote(harness, SecondItem);

        Assert.Equal(Second.ToLootGuid(), Assert.Single(Sent<LootRollBroadcast>(harness)).LootObj);
    }

    [Fact]
    public void HandleLootRollWon_SameItemInOneSlot_EndsTheRollsInTheirOrder()
    {
        var harness = NewHarness();
        StartRoll(harness, First, FirstItem);
        StartRoll(harness, Second, FirstItem);

        Win(harness, FirstItem);
        Win(harness, FirstItem);

        var complete = Sent<LootRollsComplete>(harness);
        Assert.Equal(First.ToLootGuid(), complete[0].LootObj);
        Assert.Equal(Second.ToLootGuid(), complete[1].LootObj);
    }

    /// <summary>The server drops a roll with its corpse and says nothing.</summary>
    [Fact]
    public void HandleLootRoll_RollTheServerDropped_DoesNotTakeTheVotesOfTheNext()
    {
        var harness = NewHarness();
        harness.Session.GameState.LootRolls.Add(new(Slot, FirstItem, First.ToLootGuid(), ExpiresAt: 0));
        StartRoll(harness, Second, FirstItem);

        Vote(harness, FirstItem);

        Assert.Equal(Second.ToLootGuid(), Assert.Single(Sent<LootRollBroadcast>(harness)).LootObj);
    }

    [Fact]
    public void HandleLootAllPassed_LeavesTheOtherRollOpen()
    {
        var harness = NewHarness();
        StartRoll(harness, First, FirstItem);
        StartRoll(harness, Second, SecondItem);

        AllPassed(harness, Second, SecondItem);
        Vote(harness, FirstItem);

        Assert.Equal(First.ToLootGuid(), Assert.Single(Sent<LootRollBroadcast>(harness)).LootObj);
    }
}
