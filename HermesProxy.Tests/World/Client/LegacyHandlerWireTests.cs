using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// What the Values, SpellStart and SpellGo handlers send for the packets
/// <c>LegacyHandlerBenchmarks</c> measures. The hex was captured from the handlers before
/// their allocation work (perf/receive-packet-alloc), so any change to what reaches the client
/// fails here, not in a playtest.
/// </summary>
public class LegacyHandlerWireTests
{
    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly AlteracValleyScenario _scenario;

    public LegacyHandlerWireTests() => _scenario = new AlteracValleyScenario(_harness);

    private void DeliverUpdate(byte[] wire)
        => _harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, wire, _harness.Client.HandleUpdateObject);

    private string[] SentHex() => [.. _harness.ClientWire.Sent.Select(s => $"{s.Opcode}:{Convert.ToHexString(s.Bytes)}")];

    [Fact]
    public void ValuesBatch_SendsTheUpdateAndOnlyTheOwnPowerUpdate()
    {
        DeliverUpdate(_scenario.BuildValuesBatch());

        Opcode[] expected = [Opcode.SMSG_POWER_UPDATE, Opcode.SMSG_UPDATE_OBJECT];
        Assert.Equal(expected, _harness.ClientWire.Sent.Select(s => s.Opcode));

        var power = Assert.IsType<PowerUpdate>(_harness.ClientWire.Sent[0].Packet);
        Assert.Equal(_scenario.Player, power.Guid);
        var only = Assert.Single(power.Powers);
        Assert.Equal((int)AlteracValleyScenario.OwnMana, only.Power);
        Assert.Equal(0, only.PowerType);
    }

    [Fact]
    public void ValuesBatch_WireBytesAreUnchanged()
    {
        DeliverUpdate(_scenario.BuildValuesBatch());

        Assert.Equal(ValuesBatchGolden, SentHex());
    }

    /// <summary>
    /// A power update the outbox has taken may still be waiting to be written, so the next block
    /// must never refill it.
    /// </summary>
    [Fact]
    public void OwnPowerUpdates_InConsecutivePackets_AreSeparatePackets()
    {
        DeliverUpdate(_scenario.BuildOwnPowerUpdate(100));
        DeliverUpdate(_scenario.BuildOwnPowerUpdate(200));

        var powers = _harness.ClientWire.Sent.Select(s => s.Packet).OfType<PowerUpdate>().ToArray();
        Assert.Equal(2, powers.Length);
        Assert.NotSame(powers[0], powers[1]);
        Assert.Equal(100, Assert.Single(powers[0].Powers).Power);
        Assert.Equal(200, Assert.Single(powers[1].Powers).Power);
    }

    /// <summary>
    /// Bit 31 is negative as an int, and PlayerFlagsLegacy was int-backed, so a player flags value
    /// with it set threw OverflowException and took every block of the packet down with it.
    /// </summary>
    [Fact]
    public void PlayerFlags_WithBit31Set_StillTranslates()
    {
        DeliverUpdate(_scenario.BuildPlayerFlags(0x80000002));

        Assert.Contains(_harness.ClientWire.Sent, s => s.Opcode == Opcode.SMSG_UPDATE_OBJECT);
    }

    [Fact]
    public void SpellStart_WireBytesAreUnchanged()
    {
        _harness.Deliver(Opcode.SMSG_SPELL_START, _scenario.BuildSpellStart(), _harness.Client.HandleSpellStart);

        Assert.Equal(SpellStartGolden, SentHex());
    }

    [Fact]
    public void SpellGo_WireBytesAreUnchanged()
    {
        _harness.Deliver(Opcode.SMSG_SPELL_GO, _scenario.BuildSpellGo(), _harness.Client.HandleSpellGo);

        Assert.Equal(SpellGoGolden, SentHex());
    }

    [Fact]
    public void SpellGo_WithoutTargetData_IsForwardedWithNoTargets()
    {
        _harness.Deliver(Opcode.SMSG_SPELL_GO, _scenario.BuildPetSummonSpellGo(), _harness.Client.HandleSpellGo);

        var sent = Assert.Single(_harness.ClientWire.Sent);
        Assert.Equal(Opcode.SMSG_SPELL_GO, sent.Opcode);
        var go = Assert.IsType<SpellGo>(sent.Packet);
        Assert.Equal(52150, go.Cast.SpellID);
        Assert.Empty(go.Cast.HitTargets);
    }

    // The fake summon cast as a second 4.3.4 core writes it: the two target counts, both zero,
    // and no target flags (#407). Read on, the flags came from past the end of the packet: a
    // received packet sits in a pooled buffer longer than itself, here filled with 0xFF.
    [Fact]
    public void SpellGo_WithEmptyCountsAndNoTargetFlags_IsForwardedWithNoTargets()
    {
        byte[] wire = [.. _scenario.BuildPetSummonSpellGo(), 0, 0];
        byte[] buffer = [.. wire, .. Enumerable.Repeat((byte)0xFF, 16)];

        using (var packet = new WorldPacket(buffer, wire.Length, isPooled: false))
            _harness.Client.HandleSpellGo(packet);

        var go = Assert.IsType<SpellGo>(Assert.Single(_harness.ClientWire.Sent).Packet);
        Assert.Equal(52150, go.Cast.SpellID);
        Assert.Empty(go.Cast.HitTargets);
        Assert.Empty(go.Cast.MissTargets);
        Assert.Equal((SpellCastTargetFlags)0, go.Cast.Target.Flags);
    }

    private static readonly string[] ValuesBatchGolden =
    [
        "SMSG_POWER_UPDATE:01A0010408010000007210000000",
        "SMSG_UPDATE_OBJECT:080000001E0000840400000001A764800B0D0420070000000000008001000000000000004000000000000000000000000088130000000000000100000001000000000001A765800B0D04200700000000000080010000000000000000000000000000000000000000891300000000000001000000000001A766C00B0D042007000000000000800100000000000000000000000000000000000000008A1300000000000001000000000001A76780C40B042007000000000000800100000000000000000000000000000000000000008B1300000000000001000000000001A002040818000000000000800100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000581B00000000000001000000000001A003040818000000000000800100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000591B00000000000001000000000001A0040408180000000000008001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005A1B00000000000001000000000001A0010408930000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000",
    ];

    private static readonly string[] SpellStartGolden =
    [
        "SMSG_SPELL_START:01A764800B0D042001A764800B0D042001BBE94321C00304BC000085000000000000000000000000000000AC0D000000000000000000000000000000000000000000000000000000000000000000000000000000800001A00204080000",
    ];

    private static readonly string[] SpellGoGolden =
    [
        "SMSG_SPELL_GO:01A764800B0D042001A764800B0D042001BBE94321C00304BC00008500000000000000000000000000000040E2010000000000000000000000000000000000000000000000000000010000000000000000000000800001A0020408000001A002040800",
    ];
}
