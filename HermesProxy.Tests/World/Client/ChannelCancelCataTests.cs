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
/// Stopping a channel, TrinityCore 4.3.4 sends SMSG_SPELL_FAILURE and SMSG_SPELL_FAILED_OTHER
/// (<c>Spell::SendInterrupted</c>), then SMSG_CAST_FAILED. Each legacy packet became both modern
/// ones, and as SMSG_SPELL_GO had already taken the cast off the pending queue, under a cast id the
/// proxy made up: the 4.4.2 client raised UNIT_SPELLCAST_FAILED four times for a cast it never
/// started. From a capture of a level 20 hunter (player 4) stopping Eagle Eye (6197) in Goldshire.
/// </summary>
/// <remarks>Runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public sealed class ChannelCancelCataTests
{
    private const uint EagleEye = 6197;

    private const string ChannelStart =
        "1564010401040035180000020800000000000000000000400000000067da17c69a995f422825294264000000";

    private const string ChannelGo =
        "166e01040104003518000000090000000000006631e6110104000000000000000042000000000067da17c69a995f42282529426400000000";

    private const string Failure = "35450104003518000000";
    private const string FailedOther = "340c0104003518000000";
    private const string Interrupted = "164d00351800002a";

    private static readonly WowGuid128 CastId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, 0, EagleEye, 0x2714);

    private static LegacyHandlerHarness CastEagleEye()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 4));
        harness.Session.GameState.PendingNormalCasts.Enqueue(new ClientCastRequest { SpellId = EagleEye, ServerGUID = CastId, ClientGUID = CastId });
        harness.Deliver(Opcode.SMSG_SPELL_START, Convert.FromHexString(ChannelStart), harness.Client.HandleSpellStart);
        return harness;
    }

    private static void StopChannel(LegacyHandlerHarness harness)
    {
        harness.Deliver(Opcode.SMSG_SPELL_FAILURE, Convert.FromHexString(Failure), harness.Client.HandleSpellFailedOther);
        harness.Deliver(Opcode.SMSG_SPELL_FAILED_OTHER, Convert.FromHexString(FailedOther), harness.Client.HandleSpellFailedOther);
        harness.Deliver(Opcode.SMSG_CAST_FAILED, Convert.FromHexString(Interrupted), harness.Client.HandleCastFailed);
    }

    [Fact]
    public void StoppedChannel_FailsOnceUnderTheCastIdTheClientStarted()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = CastEagleEye();
        harness.Deliver(Opcode.SMSG_SPELL_GO, Convert.FromHexString(ChannelGo), harness.Client.HandleSpellGo);
        Assert.Empty(harness.Session.GameState.PendingNormalCasts);

        StopChannel(harness);

        var failure = Assert.IsType<SpellFailure>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailure).Packet);
        var failedOther = Assert.IsType<SpellFailedOther>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailedOther).Packet);
        var castFailed = Assert.IsType<CastFailed>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is CastFailed).Packet);
        Assert.Equal(CastId, failure.CastID);
        Assert.Equal(CastId, failedOther.CastID);
        Assert.Equal(CastId, castFailed.CastID);
        Assert.Equal(EagleEye, failure.SpellID);
    }

    [Fact]
    public void CastStoppedBeforeItsGo_FailsOncePerPacket()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = CastEagleEye();

        harness.Deliver(Opcode.SMSG_SPELL_FAILURE, Convert.FromHexString(Failure), harness.Client.HandleSpellFailedOther);
        harness.Deliver(Opcode.SMSG_SPELL_FAILED_OTHER, Convert.FromHexString(FailedOther), harness.Client.HandleSpellFailedOther);

        Assert.Equal(CastId, Assert.IsType<SpellFailure>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailure).Packet).CastID);
        Assert.Equal(CastId, Assert.IsType<SpellFailedOther>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailedOther).Packet).CastID);
    }
}
