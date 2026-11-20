using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Issue #187 — the proxy built tens of thousands of hotfix records at startup for tables the
/// running client is never told about. The advertised set now lives in one place, and the
/// SpellXSpellVisual loader skips record construction when its table is not in it.
/// </summary>
public sealed class AdvertisedHotfixTablesTests
{
    [Theory]
    [InlineData(ClientVersionBuild.V3_4_3_54261)]
    [InlineData(ClientVersionBuild.V4_4_2_60895)]
    public void For_WotLKClassicOrLater_AdvertisesOnlyTheOverriddenTables(ClientVersionBuild build)
    {
        var tables = AdvertisedHotfixTables.For(build);

        Assert.NotNull(tables);
        DB2Hash[] expected = [DB2Hash.AreaTrigger, DB2Hash.CurrencyTypes, DB2Hash.GlobalStrings, DB2Hash.ItemExtendedCost];
        Assert.Equal(expected.Order(), tables.Order());
    }

    [Theory]
    [InlineData(ClientVersionBuild.V1_14_0_40618)]
    [InlineData(ClientVersionBuild.V1_14_2_42597)]
    [InlineData(ClientVersionBuild.V2_5_2_40892)]
    [InlineData(ClientVersionBuild.V2_5_3_42328)]
    public void For_EarlierClassicBuilds_AdvertisesEveryLoadedRecord(ClientVersionBuild build)
    {
        Assert.Null(AdvertisedHotfixTables.For(build));
        Assert.True(AdvertisedHotfixTables.IsAdvertised(DB2Hash.SpellXSpellVisual, build));
    }

    [Theory]
    [InlineData(ClientVersionBuild.V3_4_3_54261)]
    [InlineData(ClientVersionBuild.V4_4_2_60895)]
    public void IsAdvertised_SpellXSpellVisual_IsFalseFromWotLKClassicOn(ClientVersionBuild build)
    {
        Assert.False(AdvertisedHotfixTables.IsAdvertised(DB2Hash.SpellXSpellVisual, build));
        Assert.True(AdvertisedHotfixTables.IsAdvertised(DB2Hash.AreaTrigger, build));
    }

    [Theory]
    [InlineData(ClientVersionBuild.V3_4_3_54261)]
    [InlineData(ClientVersionBuild.V4_4_2_60895)]
    public void IsAdvertised_ItemEffect_IsFalseFromWotLKClassicOn(ClientVersionBuild build)
    {
        Assert.False(AdvertisedHotfixTables.IsAdvertised(DB2Hash.ItemEffect, build));
    }

    private static string SpellXSpellVisualCsv(int expansion)
        => Path.Combine("CSV", "Hotfix", $"SpellXSpellVisual{expansion}.csv");

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public void ParseSpellXSpellVisual_WithoutRecords_BuildsTheSameLookups(int expansion)
    {
        string path = SpellXSpellVisualCsv(expansion);

        var full = GameData.ParseSpellXSpellVisual(path, FrozenDictionary<uint, uint>.Empty, buildHotfixRecords: true);
        var lookupsOnly = GameData.ParseSpellXSpellVisual(path, FrozenDictionary<uint, uint>.Empty, buildHotfixRecords: false);

        Assert.Empty(lookupsOnly.HotfixRecords);
        Assert.NotEmpty(lookupsOnly.SpellVisuals);
        Assert.Equal(full.SpellVisuals.OrderBy(kv => kv.Key), lookupsOnly.SpellVisuals.OrderBy(kv => kv.Key));
        Assert.Equal(
            full.SpellXSpellVisualToSpellVisual.OrderBy(kv => kv.Key),
            lookupsOnly.SpellXSpellVisualToSpellVisual.OrderBy(kv => kv.Key));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public void ParseSpellXSpellVisual_WithRecords_BuildsOneRecordPerRowNumberedByRow(int expansion)
    {
        string path = SpellXSpellVisualCsv(expansion);
        int rows = File.ReadLines(path).Count() - 1; // header

        var table = GameData.ParseSpellXSpellVisual(path, FrozenDictionary<uint, uint>.Empty, buildHotfixRecords: true);

        Assert.Equal(rows, table.HotfixRecords.Count);
        Assert.All(table.HotfixRecords, r => Assert.Equal(DB2Hash.SpellXSpellVisual, r.TableHash));
        Assert.Equal(
            Enumerable.Range(1, rows).Select(i => GameData.HotfixSpellXSpellVisualBegin + (uint)i),
            table.HotfixRecords.Select(r => r.HotfixId));
    }

    [Fact]
    public void ParseSpellXSpellVisual_KeepsEarlierSpellVisualsTheCsvDoesNotOverride()
    {
        const uint unlistedSpellId = uint.MaxValue;
        var earlier = new[] { new KeyValuePair<uint, uint>(unlistedSpellId, 42u) }
            .ToFrozenDictionary();

        var table = GameData.ParseSpellXSpellVisual(SpellXSpellVisualCsv(3), earlier, buildHotfixRecords: false);

        Assert.Equal(42u, table.SpellVisuals[unlistedSpellId]);
    }
}
