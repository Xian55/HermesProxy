using System;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Archaeology's completed projects from a 4.3.4 server, which the 4.4.2 client keeps in its player's
/// ActivePlayerData. The server sends them at login ahead of the player's create: a Values update
/// sent then was for an object the client did not have yet, and the 4.4.2 client answered it with
/// CMSG_OBJECT_UPDATE_FAILED for its own player. The create writes the history instead.
/// </summary>
public sealed class CataResearchHistoryTests
{
    // 22 bits of count (1), then the project: id 201, completed twice, first on 2026-10-15
    private const string OneProject = "00 00 04 c9 00 00 00 02 00 00 00 45 42 d0 6a";

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);

    // a received packet starts with its opcode, which the handlers never look at
    private static byte[] Wire(string hex) => [0, 0, .. Convert.FromHexString(hex.Replace(" ", ""))];

    [Fact]
    public void SetupResearchHistory_BeforeThePlayersCreate_IsLeftToTheCreate()
    {
        _harness.Session.GameState.CurrentPlayerGuid = new WowGuid64(2).To128(_harness.Session.GameState);

        _harness.Deliver(Opcode.SMSG_SETUP_RESEARCH_HISTORY, Wire(OneProject), _harness.Client.HandleSetupResearchHistory);

        Assert.Empty(_harness.ClientWire.Sent);
        Assert.Equal([(201u, 1792033349L, 2u)], _harness.Session.GameState.ResearchHistory);
        Assert.True(_harness.Session.GameState.ResearchHistoryDirty);
    }

    [Fact]
    public void SetupResearchHistory_AfterThePlayersCreate_GoesOutAsAValuesUpdate()
    {
        var player = _harness.SetActivePlayer(new WowGuid64(2));

        _harness.Deliver(Opcode.SMSG_SETUP_RESEARCH_HISTORY, Wire(OneProject), _harness.Client.HandleSetupResearchHistory);

        var packet = Assert.IsType<UpdateObject>(Assert.Single(_harness.ClientWire.Sent).Packet);
        var update = Assert.Single(packet.ObjectUpdates);
        Assert.Equal(player, update.Guid);
        Assert.Equal(UpdateTypeModern.Values, update.Type);
    }
}
