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
/// A cast TrinityCore 4.3.4 refuses in <c>Spell::cast</c> after its start goes out as SMSG_SPELL_START,
/// SMSG_CAST_FAILED, then SMSG_SPELL_FAILURE and SMSG_SPELL_FAILED_OTHER (<c>Spell::SendInterrupted</c>).
/// SMSG_CAST_FAILED took the cast off the pending queue, so the two after it went to the 4.4.2 client
/// under a cast id the proxy made up: two more UNIT_SPELLCAST_FAILED for a cast it never started.
/// From a capture of a level 20 hunter (player 4) casting Bestial Wrath (19574) on a pet below level
/// 40 ("Target is too low level").
/// </summary>
/// <remarks>Runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public sealed class RefusedCastCataTests
{
    private const uint BestialWrath = 19574;

    private const string Start = "15640104010400764c00000208000000000000000000000000000009000000";
    private const string Refused = "164d00764c000032";
    private const string Failure = "3545010400764c000000";
    private const string FailedOther = "340c010400764c000000";

    private static readonly WowGuid128 CastId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, 0, BestialWrath, 0x297D);

    [Fact]
    public void CastRefusedAfterItsStart_EveryFailureNamesTheCastTheClientStarted()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 4));
        harness.Session.GameState.PendingNormalCasts.Enqueue(new ClientCastRequest { SpellId = BestialWrath, ServerGUID = CastId, ClientGUID = CastId });
        harness.Deliver(Opcode.SMSG_SPELL_START, Convert.FromHexString(Start), harness.Client.HandleSpellStart);

        harness.Deliver(Opcode.SMSG_CAST_FAILED, Convert.FromHexString(Refused), harness.Client.HandleCastFailed);
        Assert.Empty(harness.Session.GameState.PendingNormalCasts);
        harness.Deliver(Opcode.SMSG_SPELL_FAILURE, Convert.FromHexString(Failure), harness.Client.HandleSpellFailedOther);
        harness.Deliver(Opcode.SMSG_SPELL_FAILED_OTHER, Convert.FromHexString(FailedOther), harness.Client.HandleSpellFailedOther);

        var castFailed = Assert.IsType<CastFailed>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is CastFailed).Packet);
        var failure = Assert.IsType<SpellFailure>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailure).Packet);
        var failedOther = Assert.IsType<SpellFailedOther>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailedOther).Packet);
        Assert.Equal(CastId, castFailed.CastID);
        Assert.Equal(CastId, failure.CastID);
        Assert.Equal(CastId, failedOther.CastID);
        Assert.Equal(BestialWrath, failure.SpellID);
    }
}
