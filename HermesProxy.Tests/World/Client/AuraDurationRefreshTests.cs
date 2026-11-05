using System;
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
        Assert.Equal(13050, aura.Remaining);
        Assert.True(update.Auras[0].ExpiresAt > 0);
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
        Assert.Equal(0, update.Auras[0].ExpiresAt);
    }
}
