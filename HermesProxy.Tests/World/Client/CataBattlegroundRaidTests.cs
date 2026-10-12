using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The raid of a battle the player has left. TrinityCore 4.3.4 ends it with a group list of its
/// own before the teleport out; a 4.3.4 server that sends only the teleport (seen: transfer
/// pending, new world, nothing else) left the client with ten raid frames in Elwynn Forest.
/// </summary>
public sealed class CataBattlegroundRaidTests
{
    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly WowGuid128 _raidGuid;

    public CataBattlegroundRaidTests()
    {
        _harness.SetActivePlayer(new WowGuid64(2));
        _raidGuid = new WowGuid64(0x1F50000000000002UL).To128(_harness.Session.GameState);
    }

    private void GivenInstanceGroup(GroupType type)
    {
        var state = _harness.Session.GameState;
        state.CurrentGroups[1] = new PartyUpdate { PartyIndex = 1, PartyType = type, PartyGUID = _raidGuid };
        state.LastAnnouncedPartyIndex = 1;
    }

    [Fact]
    public void EndBattlegroundRaidLeftBehind_WithTheBattlesRaid_TakesItAway()
    {
        GivenInstanceGroup(GroupType.PvP);

        _harness.Client.EndBattlegroundRaidLeftBehind();

        var gone = Assert.IsType<PartyUpdate>(Assert.Single(_harness.ClientWire.Sent).Packet);
        Assert.Equal(1, gone.PartyIndex);
        Assert.True(gone.PartyFlags.HasFlag(GroupFlags.Destroyed));
        Assert.Equal(_raidGuid, gone.PartyGUID);
        Assert.Equal(WowGuid128.Empty, gone.LeaderGUID);
        Assert.Equal(-1, gone.MyIndex);

        var state = _harness.Session.GameState;
        Assert.Null(state.CurrentGroups[1]);
        Assert.Equal(0, state.LastAnnouncedPartyIndex);
    }

    /// <summary>The server's own removal list came first: there is nothing left to end.</summary>
    [Fact]
    public void EndBattlegroundRaidLeftBehind_WithNoInstanceGroup_SendsNothing()
    {
        _harness.Client.EndBattlegroundRaidLeftBehind();

        Assert.Empty(_harness.ClientWire.Sent);
    }

    /// <summary>A dungeon finder group is in the same category and outlives every teleport.</summary>
    [Fact]
    public void EndBattlegroundRaidLeftBehind_WithADungeonGroup_KeepsIt()
    {
        GivenInstanceGroup(GroupType.Normal);

        _harness.Client.EndBattlegroundRaidLeftBehind();

        Assert.Empty(_harness.ClientWire.Sent);
        Assert.NotNull(_harness.Session.GameState.CurrentGroups[1]);
    }
}
