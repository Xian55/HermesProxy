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
/// A triggered cast's SMSG_SPELL_GO from TrinityCore 4.3.4, which marks it with CAST_FLAG_PENDING
/// and leaves its CastFlagsEx 0. The 4.4.2 client starts the cooldown of every spell the player
/// casts unless CAST_FLAG_EX_IGNORE_COOLDOWN says not to, as TrinityCore cata_classic does for
/// triggered casts. From a capture of a level 55 death knight (player 5) casting Raise Dead, and
/// of a level 1 rogue (player 7) casting Vanish. Another 4.3.4 core leaves triggered casts
/// unmarked; there a cast of the player's that the client did not ask for counts as triggered.
/// </summary>
/// <remarks>Runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public sealed class TriggeredCastCataTests
{
    // Raise Dead's guardian summon (46585), cast triggered: flags 0x40901.
    private const string RaiseDeadSummon =
        "166E0105010500F9B500000109040000000000D9C1861000004000000000D2761B4555CAAEC53201D2430000000000";

    // Raise Dead (46584) itself, as the player cast it: flags 0x900.
    private const string RaiseDead =
        "166E0105010500F8B500000009000000000000D9C1861001050000000000000000020000000000000000";

    // Stealth (1784) as Vanish casts it, triggered: flags 0x40901.
    private const string VanishStealth =
        "166E0107010700F80600000109040000000000B8DD461201070000000000000000020000000000000000";

    // Raise Dead's guardian summon from another 4.3.4 core (player 0x2226), which leaves the
    // triggered cast unmarked: flags 0x900, the same as Raise Dead's own.
    private const string UnmarkedRaiseDeadSummon =
        "166E03262203262200F9B500000009000000000000D5F775010000400000000026301345B7F9B0C598201A430000000000";

    private static SpellCastData Go(string logged, uint player = 5, uint requested = 0)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, player));
        if (requested != 0)
            harness.Session.GameState.PendingNormalCasts.Enqueue(new ClientCastRequest { SpellId = requested });
        var shipped = GameData.CooldownOnEventSpells;
        GameData.LoadCooldownOnEventSpells();
        try
        {
            harness.Deliver(Opcode.SMSG_SPELL_GO, Convert.FromHexString(logged), harness.Client.HandleSpellGo);
        }
        finally
        {
            GameData.CooldownOnEventSpells = shipped;
        }
        return Assert.IsType<SpellGo>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellGo).Packet).Cast;
    }

    [Fact]
    public void TriggeredSummon_StartsNoCooldownOnTheClient()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var cast = Go(RaiseDeadSummon);

        Assert.Equal(46585, cast.SpellID);
        Assert.Equal((uint)CastFlagEx.IgnoreCooldown, cast.CastFlagsEx);
    }

    [Fact]
    public void TheSpellThePlayerCast_StartsItsCooldown()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var cast = Go(RaiseDead, requested: 46584);

        Assert.Equal(46584, cast.SpellID);
        Assert.Equal(0u, cast.CastFlagsEx);
    }

    // The client asked for Raise Dead, not for its summon: a cast of the player's that no request
    // of the client's is waiting for is the server's own.
    [Fact]
    public void UnmarkedTriggeredSummon_StartsNoCooldownOnTheClient()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var cast = Go(UnmarkedRaiseDeadSummon, player: 0x2226, requested: 46584);

        Assert.Equal(46585, cast.SpellID);
        Assert.Equal((uint)CastFlagEx.IgnoreCooldown, cast.CastFlagsEx);
    }

    // The client holds Stealth while it lasts and starts its six seconds on the server's
    // SMSG_COOLDOWN_EVENT, which IGNORE_COOLDOWN would have it drop.
    [Fact]
    public void TriggeredStealth_KeepsTheCooldownThatStartsWhenItEnds()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var cast = Go(VanishStealth, player: 7);

        Assert.Equal(1784, cast.SpellID);
        Assert.Equal(0u, cast.CastFlagsEx);
    }
}
