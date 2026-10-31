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
/// Under Trap Launcher the 4.4.2 client asks for the trap (Ice Trap 13809) and TrinityCore 4.3.4
/// casts the thrown one in its place (82941, <c>Unit::GetCastSpellInfo</c>), so no pending cast has
/// the id SMSG_SPELL_START and SMSG_SPELL_GO carry. Left pending, the client's cast was failed by
/// the next spell's start: "Another action is in progress" after every throw. From a capture of a
/// level 20 hunter (player 4) throwing Ice Trap in Goldshire.
/// </summary>
/// <remarks>Runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public sealed class TrapLauncherCastCataTests
{
    private const uint IceTrap = 13809;
    private const int IceTrapThrown = 82941;

    // Ice Trap, Trap Launcher (82941): flags 0x802, destination -9464 62 55.9.
    private const string ThrownStart =
        "15640104010400FD430100020800000000000000000000400000000000E013C600007842D1915F4251000000";

    private const string ThrownGo =
        "166E0104010400FD4301000009000000000000B95FB1110000400000000000E013C600007842D1915F425100000000";

    private static readonly WowGuid128 CastId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, 0, IceTrap, 0x28B9);

    private static (LegacyHandlerHarness Harness, ClientCastRequest Request) AskForIceTrap()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 4));
        var request = new ClientCastRequest { SpellId = IceTrap, ServerGUID = CastId, ClientGUID = CastId };
        harness.Session.GameState.PendingNormalCasts.Enqueue(request);
        return (harness, request);
    }

    [Fact]
    public void ThrownTrap_StartsAndEndsTheCastTheClientAskedFor()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var (harness, _) = AskForIceTrap();

        harness.Deliver(Opcode.SMSG_SPELL_START, Convert.FromHexString(ThrownStart), harness.Client.HandleSpellStart);
        harness.Deliver(Opcode.SMSG_SPELL_GO, Convert.FromHexString(ThrownGo), harness.Client.HandleSpellGo);

        var start = Assert.IsType<SpellStart>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellStart).Packet).Cast;
        var go = Assert.IsType<SpellGo>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellGo).Packet).Cast;
        Assert.Equal(IceTrapThrown, start.SpellID);
        Assert.Equal(CastId, start.CastID);
        Assert.Equal(CastId, go.CastID);
        Assert.Empty(harness.Session.GameState.PendingNormalCasts);
    }

    [Fact]
    public void TriggeredCastOfAnotherSpell_LeavesTheClientsCastPending()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var (harness, request) = AskForIceTrap();
        // The same start marked CAST_FLAG_PENDING, as TrinityCore 4.3.4 marks a triggered cast.
        byte[] triggered = Convert.FromHexString(ThrownStart);
        triggered[11] |= (byte)CastFlag.PendingCast;

        harness.Deliver(Opcode.SMSG_SPELL_START, triggered, harness.Client.HandleSpellStart);

        Assert.False(request.HasStarted);
        Assert.Equal(0u, request.OverrideSpellId);
        Assert.Same(request, Assert.Single(harness.Session.GameState.PendingNormalCasts));
    }
}
