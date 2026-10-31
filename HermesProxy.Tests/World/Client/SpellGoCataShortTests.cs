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
/// A pet summon's fake cast from a second 4.3.4 core (#407): caster, spell 688 (Summon Imp), flags
/// 0x100, a zero time, then two zero target counts and nothing more. The packet as it was logged
/// (<c>hermes-20261012_234322.log</c>, 23:45:26), opcode first, in a buffer longer than itself as a
/// received packet is (the rest 0xFF here): read on, the target flags came from past its end.
/// </summary>
/// <remarks>Runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public class SpellGoCataShortTests
{
    private static readonly byte[] Logged = Convert.FromHexString(
        "16-6E-03-F4-21-03-F4-21-00-B0-02-00-00-00-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00".Replace("-", ""));

    [Fact]
    public void FakeSummonCast_IsForwardedWithNoTargets()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 1));

        byte[] buffer = [.. Logged, .. Enumerable.Repeat((byte)0xFF, 16)];
        using (var packet = new WorldPacket(buffer, Logged.Length, isPooled: false))
            harness.Client.HandleSpellGo(packet);

        var go = Assert.IsType<SpellGo>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellGo).Packet);
        Assert.Equal(688, go.Cast.SpellID);
        Assert.Empty(go.Cast.HitTargets);
        Assert.Equal((SpellCastTargetFlags)0, go.Cast.Target.Flags);
    }
}
