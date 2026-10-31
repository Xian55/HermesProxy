using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Stopping a channel, a 3.3.5a server sends SMSG_SPELL_FAILURE and SMSG_SPELL_FAILED_OTHER
/// (<c>Spell::SendInterrupted</c>), the two a native 3.4.3 server sends. Each legacy packet became
/// both modern ones, and as SMSG_SPELL_GO had already taken the cast off the pending queue, under a
/// cast id the proxy made up: the 3.4.3 client raised UNIT_SPELLCAST_INTERRUPTED four times for a
/// cast it never started. From an AzerothCore capture of a level 10 hunter (player 2009) stopping
/// Eagle Eye (6197) in Teldrassil.
/// </summary>
/// <remarks>Runs under a 3.3.5a legacy build, the default.</remarks>
public sealed class ChannelCancelWotLKTests
{
    private const uint EagleEye = 6197;

    private const string ChannelStart =
        "310103d90703d9070035180000020800000000000040000000004037184665267a44c1dda1442d010000";

    private const string ChannelGo =
        "320103d90703d907003518000000090000c536ed0c01d907000000000000004200000000004037184665267a44c1dda1442b01000000";

    private const string Failure = "330103d907003518000028";
    private const string FailedOther = "a60203d907003518000028";

    private static readonly WowGuid128 CastId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, 1, EagleEye, 0x271B);

    private static LegacyHandlerHarness CastEagleEye()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 0x7D9));
        harness.Session.GameState.PendingNormalCasts.Enqueue(new ClientCastRequest { SpellId = EagleEye, ServerGUID = CastId, ClientGUID = CastId });
        harness.Deliver(Opcode.SMSG_SPELL_START, Convert.FromHexString(ChannelStart), harness.Client.HandleSpellStart);
        return harness;
    }

    private static void StopChannel(LegacyHandlerHarness harness)
    {
        harness.Deliver(Opcode.SMSG_SPELL_FAILURE, Convert.FromHexString(Failure), harness.Client.HandleSpellFailedOther);
        harness.Deliver(Opcode.SMSG_SPELL_FAILED_OTHER, Convert.FromHexString(FailedOther), harness.Client.HandleSpellFailedOther);
    }

    [Fact]
    public void StoppedChannel_FailsOnceUnderTheCastIdTheClientStarted()
    {
        Assert.SkipUnless(LegacyVersion.ExpansionVersion == 3, "3.3.5a packet layouts");

        var harness = CastEagleEye();
        harness.Deliver(Opcode.SMSG_SPELL_GO, Convert.FromHexString(ChannelGo), harness.Client.HandleSpellGo);
        Assert.Empty(harness.Session.GameState.PendingNormalCasts);

        StopChannel(harness);

        var failure = Assert.IsType<SpellFailure>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailure).Packet);
        var failedOther = Assert.IsType<SpellFailedOther>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailedOther).Packet);
        Assert.Equal(CastId, failure.CastID);
        Assert.Equal(CastId, failedOther.CastID);
        Assert.Equal(EagleEye, failure.SpellID);
    }

    [Fact]
    public void CastStoppedBeforeItsGo_FailsOncePerPacket()
    {
        Assert.SkipUnless(LegacyVersion.ExpansionVersion == 3, "3.3.5a packet layouts");

        var harness = CastEagleEye();

        StopChannel(harness);

        Assert.Equal(CastId, Assert.IsType<SpellFailure>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailure).Packet).CastID);
        Assert.Equal(CastId, Assert.IsType<SpellFailedOther>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailedOther).Packet).CastID);
    }
}
