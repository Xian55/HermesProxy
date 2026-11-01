using System;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A 2.4.3 server sends the durations of a unit's timed auras at login in one packet: a packed
/// guid, then for each aura its slot, spell, full and remaining duration. CMaNGOS TBC also sends
/// each of them in SMSG_SET_EXTRA_AURA_INFO, so nothing was missing there, but the packet warned
/// as one nobody had read.
/// </summary>
public sealed class InitExtraAuraInfoTests
{
    private static readonly WowGuid64 Unit = new(HighGuidTypeLegacy.Player, 8);

    private static void Deliver(LegacyHandlerHarness harness, Action<WorldPacket> records)
    {
        harness.Deliver(Opcode.SMSG_INIT_EXTRA_AURA_INFO, LegacyPacketBuilder.Build(Opcode.SMSG_INIT_EXTRA_AURA_INFO, p =>
        {
            p.WritePackedGuid(Unit);
            records(p);
        }), harness.Client.HandleInitExtraAuraInfo);
    }

    private static void Record(WorldPacket p, byte slot, uint spell, int full, int left)
    {
        p.WriteUInt8(slot);
        p.WriteUInt32(spell);
        p.WriteInt32(full);
        p.WriteInt32(left);
    }

    [Fact]
    public void HandleInitExtraAuraInfo_TwoAuras_KeepsBothDurations()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        WowGuid128 guid = harness.SetActivePlayer(Unit);

        Deliver(harness, p =>
        {
            Record(p, 3, 1243, 1800000, 1200000);
            Record(p, 7, 465, -1, -1);
        });

        harness.Session.GameState.GetAuraDuration(guid, 3, out int left, out int full);
        Assert.Equal(1800000, full);
        Assert.InRange(left, 1, 1200000);
        harness.Session.GameState.GetAuraDuration(guid, 7, out left, out full);
        Assert.Equal(-1, full);
        Assert.Equal(-1, left);
    }

    /// <summary>What a character without timed auras gets, from a 2.5.3 session on CMaNGOS.</summary>
    [Fact]
    public void HandleInitExtraAuraInfo_NoAuras_ReadsNothingMore()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(Unit);

        Deliver(harness, _ => { });

        Assert.Empty(harness.ClientWire.Sent);
    }
}
