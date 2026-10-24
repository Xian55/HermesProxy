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
/// SMSG_ARENA_TEAM_EVENT tells the team's members that someone joined, left, was removed or became
/// captain, or that the team is gone. TrinityCore 4.3.4 numbers these as the modern clients do
/// (ERR_ARENA_TEAM_JOIN_SS = 4), 3.3.5a one lower (3). From a capture of Xii joining a 2v2 team on
/// a 4.3.4 server, with the team's name replaced; read as a 3.3.5a event, the 4.4.2 client said
/// that Xii had left it.
/// </summary>
public sealed class ArenaTeamEventCataTests
{
    // 4, two strings (Xii, Arena Team), Xii's guid.
    private const string JoinedCata =
        "1706" + "04" + "02" + "58696900" + "4172656E61205465616D00" + "2422000000000000";

    // The same join from a 3.3.5a server, which numbers it 3.
    private const string JoinedWotlk =
        "1706" + "03" + "02" + "58696900" + "4172656E61205465616D00" + "2422000000000000";

    private static ArenaTeamEvent Deliver(string logged)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 0x2225));
        harness.Deliver(Opcode.SMSG_ARENA_TEAM_EVENT, Convert.FromHexString(logged), harness.Client.HandleArenaTeamEvent);
        return Assert.IsType<ArenaTeamEvent>(Assert.Single(harness.ClientWire.Sent.Where(s => s.Packet is ArenaTeamEvent)).Packet);
    }

    [Fact]
    public void Join_FromA434Server_IsAJoin()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var arena = Deliver(JoinedCata);

        Assert.Equal(ArenaTeamEventModern.PlayerJoined, arena.Event);
        Assert.Equal("Xii", arena.Param1);
        Assert.Equal("Arena Team", arena.Param2);
    }

    [Fact]
    public void Join_FromA335Server_IsAJoin()
    {
        if (WorldClient.IsCataLegacy)
            return;

        var arena = Deliver(JoinedWotlk);

        Assert.Equal(ArenaTeamEventModern.PlayerJoined, arena.Event);
        Assert.Equal("Xii", arena.Param1);
        Assert.Equal("Arena Team", arena.Param2);
    }
}
