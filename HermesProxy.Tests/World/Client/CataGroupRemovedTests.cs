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
/// The end of a party on a 4.3.4 server. TrinityCore 4.3.4 says what happened itself:
/// SMSG_GROUP_UNINVITE to a member it removes, SMSG_GROUP_DESTROYED to everyone in a group it
/// disbands, and to each of them the group list with the removal mark and nobody in it.
/// </summary>
/// <remarks>Runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public sealed class CataGroupRemovedTests
{
    private const ulong Group = 0x1F50000000000005ul;

    private static LegacyHandlerHarness InAParty()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(0x2BF));
        harness.Session.GameState.CurrentGroups[0] = new PartyUpdate
        {
            PartyGUID = new WowGuid64(Group).To128(harness.Session.GameState),
        };
        harness.ClientWire.Sent.Clear();
        return harness;
    }

    // TrinityCore 4.3.4 Group::Disband / RemoveMember: the removal mark, own subgroup, flags and
    // roles, the group, its counter, no member, no leader
    private static void DeliverEmptyList(LegacyHandlerHarness harness)
    {
        byte[] wire = [0, 0, 0x10, 0, 0, 0, .. BitConverter.GetBytes(Group), .. BitConverter.GetBytes(7u),
            .. BitConverter.GetBytes(0u), .. BitConverter.GetBytes(0ul)];
        harness.Deliver(Opcode.SMSG_GROUP_LIST, wire, harness.Client.HandleGroupListTBC);
    }

    [Fact]
    public void ADisbandedParty_SaysOnlyThatItWasDisbanded()
    {
        if (!WorldClient.IsCataLegacy)
            return;
        var harness = InAParty();

        harness.Deliver(Opcode.SMSG_GROUP_DESTROYED, [0, 0], harness.Client.HandleGroupDestroyed);
        DeliverEmptyList(harness);

        var sent = harness.ClientWire.Sent.Select(s => s.Packet).ToArray();
        Assert.Collection(sent,
            p => Assert.IsType<GroupDestroyed>(p),
            p => Assert.True(Assert.IsType<PartyUpdate>(p).PartyFlags.HasFlag(GroupFlags.Destroyed)));
    }

    [Fact]
    public void AMemberRemoved_IsToldOnce()
    {
        if (!WorldClient.IsCataLegacy)
            return;
        var harness = InAParty();

        harness.Deliver(Opcode.SMSG_GROUP_UNINVITE, [0, 0], harness.Client.HandleGroupUninvite);
        DeliverEmptyList(harness);

        var sent = harness.ClientWire.Sent.Select(s => s.Packet).ToArray();
        Assert.Single(sent.OfType<GroupUninvite>());
        Assert.IsType<GroupUninvite>(sent[0]);
        Assert.IsType<PartyUpdate>(sent[^1]);
    }

    // TrinityCore 4.3.4 SendPartyResult(PARTY_OP_LEAVE, name, ERR_PARTY_RESULT_OK): the operation,
    // the player's name, the result, its value and a GUID of nothing
    private static void DeliverLeaveAnswer(LegacyHandlerHarness harness)
    {
        byte[] wire = [0, 0, 2, 0, 0, 0, (byte)'D', (byte)'k', 0, 0, 0, 0, 0, 0, 0, 0, 0, .. BitConverter.GetBytes(0ul)];
        harness.Deliver(Opcode.SMSG_PARTY_COMMAND_RESULT, wire, harness.Client.HandlePartyCommandResult);
    }

    [Fact]
    public void LeavingADungeonFinderGroup_WithNoListAfter_TakesThePlayerOut()
    {
        if (!WorldClient.IsCataLegacy)
            return;
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(0x2BF));
        var state = harness.Session.GameState;
        state.CurrentGroups[1] = new PartyUpdate
        {
            PartyIndex = 1,
            PartyFlags = GroupFlags.Lfg | GroupFlags.LfgRestricted,
            PartyGUID = new WowGuid64(Group).To128(state),
        };
        state.LastAnnouncedPartyIndex = 1;
        state.LastGroupWasLfg = true;
        harness.ClientWire.Sent.Clear();

        DeliverLeaveAnswer(harness);

        var sent = harness.ClientWire.Sent.Select(s => s.Packet).ToArray();
        Assert.IsType<PartyCommandResult>(sent[0]);
        var removed = Assert.IsType<PartyUpdate>(sent[1]);
        Assert.True(removed.PartyFlags.HasFlag(GroupFlags.Destroyed));
        Assert.Equal(1, removed.PartyIndex);
        Assert.Equal(2, sent.OfType<DFUpdateStatus>().Count());
        Assert.Null(state.CurrentGroups[1]);
        Assert.False(state.LastGroupWasLfg);
    }

    [Fact]
    public void LeavingAParty_ThenTheList_IsToldOnce()
    {
        if (!WorldClient.IsCataLegacy)
            return;
        var harness = InAParty();

        DeliverLeaveAnswer(harness);
        DeliverEmptyList(harness);

        var sent = harness.ClientWire.Sent.Select(s => s.Packet).ToArray();
        Assert.IsType<PartyCommandResult>(sent[0]);
        Assert.True(Assert.Single(sent.OfType<PartyUpdate>()).PartyFlags.HasFlag(GroupFlags.Destroyed));
        Assert.Null(harness.Session.GameState.CurrentGroups[0]);
    }
}
