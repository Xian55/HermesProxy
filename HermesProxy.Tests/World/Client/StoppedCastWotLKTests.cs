using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A cast stopped during its cast bar: a 3.3.5a server answers SMSG_CAST_FAILED (interrupted), then
/// SMSG_SPELL_FAILURE and SMSG_SPELL_FAILED_OTHER (<c>Spell::cancel</c>, <c>SendInterrupted</c>), as
/// a native 3.4.3 server sends them. SMSG_CAST_FAILED took the cast off the pending queue, so the
/// two after it went out under the last cast of that spell that had gone off: the 3.4.3 client heard
/// that the earlier cast was interrupted. From an AzerothCore capture of a level 10 hunter (player
/// 2009) casting Steady Shot (56641) once and stopping the second one halfway.
/// </summary>
/// <remarks>Runs under a 3.3.5a legacy build, the default.</remarks>
public sealed class StoppedCastWotLKTests
{
    private const uint SteadyShot = 56641;

    private const string FirstStart =
        "310103d90703d9070041dd000022080000cb06000002000000dfe4ee01581330f12d0100006c17000018000000";

    private const string FirstGo =
        "320103d90703d9070041dd000020090000c4d6fa0c01e4ee0158130030f10002000000dfe4ee01581330f1230100006c17000018000000";

    private const string SecondStart =
        "310103d90703d9070041dd000022080000cb06000002000000dfe4ee01581330f1230100006c17000018000000";

    private const string Interrupted = "30010041dd000028";
    private const string Failure = "330103d9070041dd000028";
    private const string FailedOther = "a60203d9070041dd000028";

    private static readonly WowGuid128 FirstCastId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, 1, SteadyShot, 0x2730);
    private static readonly WowGuid128 SecondCastId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, 1, SteadyShot, 0x2731);

    private static void Cast(LegacyHandlerHarness harness, WowGuid128 castId, string start)
    {
        harness.Session.GameState.PendingNormalCasts.Enqueue(new ClientCastRequest { SpellId = SteadyShot, ServerGUID = castId, ClientGUID = castId });
        harness.Deliver(Opcode.SMSG_SPELL_START, Convert.FromHexString(start), harness.Client.HandleSpellStart);
    }

    [Fact]
    public void CastStoppedHalfway_FailsUnderItsOwnCastId_NotTheOneBefore()
    {
        Assert.SkipUnless(LegacyVersion.ExpansionVersion == 3, "3.3.5a packet layouts");

        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 0x7D9));
        Cast(harness, FirstCastId, FirstStart);
        harness.Deliver(Opcode.SMSG_SPELL_GO, Convert.FromHexString(FirstGo), harness.Client.HandleSpellGo);
        Cast(harness, SecondCastId, SecondStart);

        harness.Deliver(Opcode.SMSG_CAST_FAILED, Convert.FromHexString(Interrupted), harness.Client.HandleCastFailed);
        harness.Deliver(Opcode.SMSG_SPELL_FAILURE, Convert.FromHexString(Failure), harness.Client.HandleSpellFailedOther);
        harness.Deliver(Opcode.SMSG_SPELL_FAILED_OTHER, Convert.FromHexString(FailedOther), harness.Client.HandleSpellFailedOther);

        Assert.Equal(SecondCastId, Assert.IsType<CastFailed>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is CastFailed).Packet).CastID);
        Assert.Equal(SecondCastId, Assert.IsType<SpellFailure>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailure).Packet).CastID);
        Assert.Equal(SecondCastId, Assert.IsType<SpellFailedOther>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellFailedOther).Packet).CastID);
    }
}
