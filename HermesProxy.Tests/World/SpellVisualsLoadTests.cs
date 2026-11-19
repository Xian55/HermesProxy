using System.Collections.Frozen;
using System.IO;
using System.Linq;

using HermesProxy.World;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// <c>GameData.SpellVisuals</c> comes from two files: the SpellVisuals CSV, and the
/// SpellXSpellVisual hotfix CSV laid over it. They used to load as two parallel jobs that both
/// assigned the field, so the result was whichever finished last. One reader now takes both, in
/// order.
/// </summary>
public sealed class SpellVisualsLoadTests
{
    private static string SpellVisualsCsv(int expansion)
        => Path.Combine("CSV", $"SpellVisuals{expansion}.csv");

    private static string HotfixCsv(int expansion)
        => Path.Combine("CSV", "Hotfix", $"SpellXSpellVisual{expansion}.csv");

    private static readonly string MissingCsv = Path.Combine("CSV", "NoSuchFile.csv");

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public void ReadSpellVisuals_LaysTheHotfixRowsOverTheBaseFile(int expansion)
    {
        var baseOnly = GameData.ParseSpellVisuals(SpellVisualsCsv(expansion));
        var hotfixOnly = GameData.ParseSpellXSpellVisual(
            HotfixCsv(expansion), FrozenDictionary<uint, uint>.Empty, buildHotfixRecords: false).SpellVisuals;

        var table = GameData.ReadSpellVisuals(SpellVisualsCsv(expansion), HotfixCsv(expansion), buildHotfixRecords: false);

        Assert.NotEmpty(baseOnly);
        Assert.NotEmpty(hotfixOnly);
        Assert.Equal(baseOnly.Keys.Union(hotfixOnly.Keys).Count(), table.SpellVisuals.Count);
        Assert.All(hotfixOnly, row => Assert.Equal(row.Value, table.SpellVisuals[row.Key]));
        Assert.All(baseOnly.Where(row => !hotfixOnly.ContainsKey(row.Key)),
            row => Assert.Equal(row.Value, table.SpellVisuals[row.Key]));
    }

    /// <summary>
    /// The build where the race showed: the hotfix file names spells the base file does not have,
    /// so either file alone is a different, smaller map.
    /// </summary>
    [Fact]
    public void ReadSpellVisuals_On1_14_IsNeitherFileAlone()
    {
        var baseOnly = GameData.ParseSpellVisuals(SpellVisualsCsv(1));
        var hotfixOnly = GameData.ParseSpellXSpellVisual(
            HotfixCsv(1), FrozenDictionary<uint, uint>.Empty, buildHotfixRecords: false).SpellVisuals;

        var table = GameData.ReadSpellVisuals(SpellVisualsCsv(1), HotfixCsv(1), buildHotfixRecords: true);

        Assert.True(table.SpellVisuals.Count > baseOnly.Count);
        Assert.True(table.SpellVisuals.Count > hotfixOnly.Count);
        Assert.Equal(hotfixOnly.Count, table.HotfixRecords.Count);
    }

    [Fact]
    public void ReadSpellVisuals_WithoutAHotfixFile_IsTheBaseFile()
    {
        var baseOnly = GameData.ParseSpellVisuals(SpellVisualsCsv(2));

        var table = GameData.ReadSpellVisuals(SpellVisualsCsv(2), MissingCsv, buildHotfixRecords: true);

        Assert.NotEmpty(baseOnly);
        Assert.Equal(baseOnly.OrderBy(row => row.Key), table.SpellVisuals.OrderBy(row => row.Key));
        Assert.Empty(table.SpellXSpellVisualToSpellVisual);
        Assert.Empty(table.HotfixRecords);
    }

    [Fact]
    public void ReadSpellVisuals_WithoutABaseFile_IsTheHotfixFile()
    {
        var hotfixOnly = GameData.ParseSpellXSpellVisual(
            HotfixCsv(3), FrozenDictionary<uint, uint>.Empty, buildHotfixRecords: false);

        var table = GameData.ReadSpellVisuals(MissingCsv, HotfixCsv(3), buildHotfixRecords: false);

        Assert.Equal(hotfixOnly.SpellVisuals.OrderBy(row => row.Key), table.SpellVisuals.OrderBy(row => row.Key));
        Assert.Equal(hotfixOnly.SpellXSpellVisualToSpellVisual.Count, table.SpellXSpellVisualToSpellVisual.Count);
    }

    [Fact]
    public void ParseSpellVisuals_WithoutAFile_IsEmpty()
        => Assert.Empty(GameData.ParseSpellVisuals(MissingCsv));
}
