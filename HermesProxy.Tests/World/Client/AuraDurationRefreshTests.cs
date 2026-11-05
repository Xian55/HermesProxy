using System;
using System.Collections.Generic;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The create block can be held while item templates are resolved, after the aura duration
/// packets were already read. The held aura list has to pick the tracked durations up again.
/// </summary>
public sealed class AuraDurationRefreshTests
{
    private static readonly WowGuid128 Player = WowGuid128.Create(HighGuidType703.Player, 531);

    private static AuraUpdate UpdateWith(uint spellId, byte slot = 0)
    {
        var update = new AuraUpdate(Player, true);
        update.Auras.Add(new AuraInfo
        {
            Slot = slot,
            AuraData = new AuraDataInfo { SpellID = spellId },
        });
        return update;
    }

    [Fact]
    public void TimedAura_GetsTheTrackedDuration()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false);
        var state = harness.Session.GameState;
        state.StoreAuraDurationFull(Player, 0, 13050);
        state.StoreAuraDurationLeft(Player, 0, 13050, Environment.TickCount);

        var update = UpdateWith(5171);
        AuraDurationRefresh.Apply(update, state);

        AuraDataInfo aura = update.Auras[0].AuraData!;
        Assert.True(aura.Flags.HasFlag(AuraFlagsModern.Duration));
        Assert.Equal(13050, aura.Duration);
        Assert.InRange(aura.Remaining!.Value, 12000, 13050);
    }

    [Fact]
    public void PermanentAura_KeepsItsMissingDuration()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false);
        var state = harness.Session.GameState;
        state.StoreAuraDurationFull(Player, 0, -1);
        state.StoreAuraDurationLeft(Player, 0, 0, Environment.TickCount);

        var update = UpdateWith(546);
        AuraDurationRefresh.Apply(update, state);

        AuraDataInfo aura = update.Auras[0].AuraData!;
        Assert.False(aura.Flags.HasFlag(AuraFlagsModern.Duration));
        Assert.Null(aura.Duration);
        Assert.Null(aura.Remaining);
    }

    [Fact]
    public void ExpiredAura_GetsNoFreshTimer()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false);
        var state = harness.Session.GameState;
        // A 13 s aura whose duration came 20 s ago has run out on the proxy's clock.
        state.StoreAuraDurationFull(Player, 0, 13050);
        state.StoreAuraDurationLeft(Player, 0, 13050, Environment.TickCount - 20000);

        var update = UpdateWith(5171);
        AuraDurationRefresh.Apply(update, state);

        AuraDataInfo aura = update.Auras[0].AuraData!;
        Assert.False(aura.Flags.HasFlag(AuraFlagsModern.Duration));
        Assert.Null(aura.Duration);
        Assert.Null(aura.Remaining);
    }

    [Fact]
    public void SendingAnAuraList_RefreshesItsDurations()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var state = harness.Session.GameState;
        state.StoreAuraDurationFull(Player, 0, 13050);
        state.StoreAuraDurationLeft(Player, 0, 13050, Environment.TickCount);

        harness.Client.SendAuraUpdates([UpdateWith(5171)]);

        var sent = Assert.IsType<AuraUpdate>(Assert.Single(harness.ClientWire.Sent).Packet);
        AuraDataInfo aura = sent.Auras[0].AuraData!;
        Assert.True(aura.Flags.HasFlag(AuraFlagsModern.Duration));
        Assert.Equal(13050, aura.Duration);
        Assert.InRange(aura.Remaining!.Value, 12000, 13050);
    }

    [Fact]
    public void SendingAnAuraList_AllocatesNothingBeyondItsPackets()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false);
        AuraUpdate update = UpdateWith(5171);
        List<AuraUpdate> list = [update];

        // Every update batch with an aura change comes through here. A first call allocates for
        // the JIT and lazy statics, so both paths are warmed before they are measured.
        harness.Client.SendPacketToClient(update);
        harness.Client.SendAuraUpdates(list);

        long before = GC.GetAllocatedBytesForCurrentThread();
        harness.Client.SendPacketToClient(update);
        long packetAlone = GC.GetAllocatedBytesForCurrentThread() - before;

        before = GC.GetAllocatedBytesForCurrentThread();
        harness.Client.SendAuraUpdates(list);
        long throughTheList = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(packetAlone, throughTheList);
    }
}
