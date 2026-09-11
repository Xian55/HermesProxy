using System;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Issue #278: cMaNGOS WotLK uses 0x4700 for item guids, every other supported core uses 0x4000.
/// The proxy accepted both inbound but rebuilt every item as 0x4000, so item-guid packets from
/// the client (quest-starting items, vendor sells, repairs) named a guid that backend could not
/// find, and it silently did nothing.
///
/// <para>
/// <see cref="LegacyItemGuidHigh"/> is process-global state and xunit.runner.json runs test
/// classes in parallel, so every mutating test lives in this one class — xUnit serialises tests
/// within a class — and each one resets first rather than assuming a pristine default.
/// </para>
/// </summary>
[CollectionDefinition("LegacyItemGuidHigh", DisableParallelization = true)]
public class LegacyItemGuidHighCollection;

[Collection("LegacyItemGuidHigh")]
public class LegacyItemGuidHighTests : IDisposable
{
    public LegacyItemGuidHighTests() => LegacyItemGuidHigh.Reset();

    public void Dispose() => LegacyItemGuidHigh.Reset();

    [Fact]
    public void Default_IsStandardItemHigh()
    {
        Assert.Equal(HighGuidTypeLegacy.Item, LegacyItemGuidHigh.Current);
    }

    // The regression the issue is about: an item that arrives as 0x4700 must go back as 0x4700.
    [Fact]
    public void ItemArrivingAs4700_GoesBackAs4700()
    {
        // Arrange — the Riding Training Pamphlet from the issue: counter 579.
        var legacy = new WowGuid64(HighGuidTypeLegacy.ItemContainer, 579);

        // Act — ingest (server → client), then rebuild (client → server).
        var modern = legacy.To128(null!);
        var backToLegacy = modern.To64();

        // Assert
        Assert.Equal(HighGuidTypeLegacy.ItemContainer, backToLegacy.GetHighGuidTypeLegacy());
        Assert.Equal(579ul, backToLegacy.GetCounter());
        Assert.Equal(legacy, backToLegacy);
    }

    // TrinityCore / AzerothCore / cMaNGOS classic / cMaNGOS TBC must be untouched by the fix.
    [Fact]
    public void ItemArrivingAs4000_GoesBackAs4000()
    {
        // Arrange
        var legacy = new WowGuid64(HighGuidTypeLegacy.Item, 579);

        // Act
        var backToLegacy = legacy.To128(null!).To64();

        // Assert
        Assert.Equal(HighGuidTypeLegacy.Item, backToLegacy.GetHighGuidTypeLegacy());
        Assert.Equal(579ul, backToLegacy.GetCounter());
        Assert.Equal(legacy, backToLegacy);
    }

    // With no 0x4700 ever seen, rebuilding must still produce the standard high — a modern item
    // guid that the proxy minted itself (no legacy original) is the common case here.
    [Fact]
    public void ModernItemGuid_WithNothingObserved_RebuildsAsStandardHigh()
    {
        // Arrange
        var modern = WowGuid128.Create(HighGuidType703.Item, 42);

        // Act
        var legacy = modern.To64();

        // Assert
        Assert.Equal(HighGuidTypeLegacy.Item, legacy.GetHighGuidTypeLegacy());
        Assert.Equal(42ul, legacy.GetCounter());
    }

    // Latching: proxy-built item guids carry the 0x4000 default, and converting one must not
    // undo what the backend told us, or the rebuilt high would depend on conversion order.
    [Fact]
    public void ObservingStandardHighAfter4700_DoesNotRevert()
    {
        // Arrange
        LegacyItemGuidHigh.Observe(HighGuidTypeLegacy.ItemContainer);

        // Act
        LegacyItemGuidHigh.Observe(HighGuidTypeLegacy.Item);

        // Assert
        Assert.Equal(HighGuidTypeLegacy.ItemContainer, LegacyItemGuidHigh.Current);
        Assert.Equal(
            HighGuidTypeLegacy.ItemContainer,
            WowGuid128.Create(HighGuidType703.Item, 7).To64().GetHighGuidTypeLegacy());
    }

    [Theory]
    [InlineData(HighGuidTypeLegacy.Creature)]
    [InlineData(HighGuidTypeLegacy.Player)]
    [InlineData(HighGuidTypeLegacy.GameObject)]
    [InlineData(HighGuidTypeLegacy.Corpse)]
    public void ObservingNonItemHigh_IsIgnored(HighGuidTypeLegacy high)
    {
        // Act
        LegacyItemGuidHigh.Observe(high);

        // Assert
        Assert.Equal(HighGuidTypeLegacy.Item, LegacyItemGuidHigh.Current);
    }

    // Converting a non-item guid must not disturb the learned high either.
    [Fact]
    public void ConvertingCreatureGuid_DoesNotChangeItemHigh()
    {
        // Arrange
        LegacyItemGuidHigh.Observe(HighGuidTypeLegacy.ItemContainer);
        var creature = new WowGuid64(HighGuidTypeLegacy.Creature, 25000, 42);

        // Act
        _ = WowGuid64.Create(WowGuid128.Create(HighGuidType703.Creature, 0, 25000, 42));
        _ = creature.GetHighType();

        // Assert
        Assert.Equal(HighGuidTypeLegacy.ItemContainer, LegacyItemGuidHigh.Current);
    }

    // Both highs must still read as items on the modern side, which is what makes the ingest
    // side work at all.
    [Theory]
    [InlineData(HighGuidTypeLegacy.Item)]
    [InlineData(HighGuidTypeLegacy.ItemContainer)]
    public void BothHighs_ConvertToModernItemGuid(HighGuidTypeLegacy high)
    {
        // Arrange
        var legacy = new WowGuid64(high, 461);

        // Act
        var modern = legacy.To128(null!);

        // Assert
        Assert.Equal(HighGuidType.Item, modern.GetHighType());
        Assert.True(modern.IsItem());
        Assert.Equal(461ul, modern.GetCounter());
    }
}
