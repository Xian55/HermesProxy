using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A 3.3.5a server lists every group of one: a party as it is formed (Group::Create adds the
/// leader and lists the group before the one who accepted joins), the group of a dungeon finder
/// queue of one (cMaNGOS in LFG testing mode, #104), a battleground's raid on entry. The list names
/// nobody else and has no loot block; it is the group, not the end of it.
/// </summary>
public sealed class GroupOfOneTests
{
    private const ulong Group = 0x1F50000000000005ul;
    private const ulong Player = 0x2BF;

    private static LegacyHandlerHarness Harness()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(Player));
        harness.ClientWire.Sent.Clear();
        return harness;
    }

    // Group::SendUpdateTo: the group type, own subgroup, flags and roles, the group, its counter,
    // nobody else, the leader, and no loot block
    private static byte[] GroupOfOne(byte groupType) => [0, 0, groupType, 0, 0, 0,
        .. BitConverter.GetBytes(Group), .. BitConverter.GetBytes(3u), .. BitConverter.GetBytes(0u),
        .. BitConverter.GetBytes(Player)];

    // the same for a dungeon finder group: LFG | LFG_RESTRICTED, and the finished status and
    // dungeon after the roles
    private static readonly byte[] LfgGroupOfOne = [0, 0, 0x0C, 0, 0, 8, 0, .. BitConverter.GetBytes(218u),
        .. BitConverter.GetBytes(Group), .. BitConverter.GetBytes(3u), .. BitConverter.GetBytes(0u),
        .. BitConverter.GetBytes(Player)];

    // Group::Disband: the removal mark, the group, its counter, nobody, no leader
    private static readonly byte[] Disbanded = [0, 0, 0x10, 0, 0, 0, .. BitConverter.GetBytes(Group),
        .. BitConverter.GetBytes(4u), .. BitConverter.GetBytes(0u), .. BitConverter.GetBytes(0ul)];

    private static PartyUpdate DeliverAlone(LegacyHandlerHarness harness, byte[] wire)
    {
        harness.Deliver(Opcode.SMSG_GROUP_LIST, wire, harness.Client.HandleGroupListTBC);
        var sent = harness.ClientWire.Sent.Select(s => s.Packet).ToArray();
        Assert.Empty(sent.OfType<GroupUninvite>());
        var party = Assert.Single(sent.OfType<PartyUpdate>());
        Assert.False(party.PartyFlags.HasFlag(GroupFlags.Destroyed));
        Assert.Equal(party.LeaderGUID, Assert.Single(party.PlayerList).GUID);
        return party;
    }

    [Fact]
    public void APartyBeingFormed_IsAPartyOfTheLeader()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var harness = Harness();

        var party = DeliverAlone(harness, GroupOfOne(0));

        Assert.Equal(0, party.PartyIndex);
        Assert.Equal(GroupType.Normal, party.PartyType);
        Assert.Equal(party.PartyGUID, harness.Session.GameState.CurrentGroups[0]!.PartyGUID);
    }

    [Fact]
    public void AGroupOfOne_HasNoLootOrDifficulty()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var harness = Harness();

        var party = DeliverAlone(harness, GroupOfOne(0));

        Assert.Null(party.LootSettings);
        Assert.Null(party.DifficultySettings);
        Assert.Equal(LootMethod.FreeForAll, harness.Session.GameState.GetCurrentLootMethod());
    }

    [Fact]
    public void AGroupOfOne_ThenItsSecondMember_HasLootAndDifficulty()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var harness = Harness();
        harness.Deliver(Opcode.SMSG_GROUP_LIST, GroupOfOne(0), harness.Client.HandleGroupListTBC);
        harness.ClientWire.Sent.Clear();

        // the same group with the one who accepted: name, guid, online, subgroup, flags, roles,
        // then the leader, group loot, no master looter, uncommon, normal dungeon, 10-player raid,
        // not dynamic heroic
        byte[] withMember = [0, 0, 0, 0, 0, 0, .. BitConverter.GetBytes(Group), .. BitConverter.GetBytes(4u),
            .. BitConverter.GetBytes(1u), (byte)'T', (byte)'w', (byte)'o', 0, .. BitConverter.GetBytes(0x2C0ul), 1, 0, 0, 0,
            .. BitConverter.GetBytes(Player), 3, .. BitConverter.GetBytes(0ul), 2, 0, 0, 0];
        harness.Deliver(Opcode.SMSG_GROUP_LIST, withMember, harness.Client.HandleGroupListTBC);

        var party = Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<PartyUpdate>());
        Assert.Equal(2, party.PlayerList.Count);
        Assert.Equal(LootMethod.GroupLoot, party.LootSettings.Method);
        Assert.NotNull(party.DifficultySettings);
        Assert.Equal(LootMethod.GroupLoot, harness.Session.GameState.GetCurrentLootMethod());
    }

    [Fact]
    public void ADungeonFinderGroupOfOne_IsTheInstanceGroup()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var harness = Harness();

        var party = DeliverAlone(harness, LfgGroupOfOne);

        Assert.True(party.PartyFlags.HasFlag(GroupFlags.Lfg));
        Assert.Equal(1, party.PartyIndex);
        Assert.NotNull(party.LfgInfos);
        Assert.NotNull(harness.Session.GameState.CurrentGroups[1]);
    }

    [Fact]
    public void ABattlegroundRaidOfOne_IsTheBattlesRaid()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var harness = Harness();

        var party = DeliverAlone(harness, GroupOfOne(0x01 | 0x02));

        Assert.True(party.PartyFlags.HasFlag(GroupFlags.Raid));
        Assert.Equal(1, party.PartyIndex);
        Assert.Equal(GroupType.PvP, party.PartyType);
    }

    [Fact]
    public void AGroupOfOne_Disbanded_IsGone()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var harness = Harness();
        harness.Deliver(Opcode.SMSG_GROUP_LIST, LfgGroupOfOne, harness.Client.HandleGroupListTBC);
        harness.ClientWire.Sent.Clear();

        harness.Deliver(Opcode.SMSG_GROUP_LIST, Disbanded, harness.Client.HandleGroupListTBC);

        var party = Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<PartyUpdate>());
        Assert.True(party.PartyFlags.HasFlag(GroupFlags.Destroyed));
        Assert.Equal(1, party.PartyIndex);
        Assert.Null(harness.Session.GameState.CurrentGroups[1]);
    }
}
