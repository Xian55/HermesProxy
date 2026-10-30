using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Objects;

namespace HermesProxy.Benchmarks;

/// <summary>
/// Issue #355: the GameData calls a session makes on its hot paths and on an item query, through
/// the real API, so the same file runs against master and against the change. The stores are the
/// 3.4.3 CSVs as the proxy loads them, plus templates as the server would have described them.
/// </summary>
/// <remarks>
/// Reads, per call: an item and a quest template hit (per update batch, per quest packet), the
/// aura spell remap miss (per aura slot), an item level from ItemSparse for an item the server has
/// not described (per update batch), an item's appearance (4.4.2, per item in an update batch) and
/// an item spell's slot (per item cast). Writes: a template stored again, an item spell slot saved
/// again, and an already up-to-date item through the five hotfix generators, which is what every
/// item query after the first costs.
/// </remarks>
[MemoryDiagnoser]
public class GameDataAccessBenchmarks
{
    private const int Calls = 1024;

    private uint[] _templateEntries = null!;
    private uint[] _sparseOnlyEntries = null!;
    private uint[] _appearanceEntries = null!;
    private ItemTemplate _upToDate = null!;
    private ItemTemplate _template = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;

        // The loaders read CSV/ relative to the working directory, as the proxy does.
        string dir = AppContext.BaseDirectory;
        string item = Path.Combine("CSV", $"Item{ModernVersion.ExpansionVersion}.csv");
        while (!File.Exists(Path.Combine(dir, item)))
            dir = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar))
                ?? throw new InvalidOperationException($"{item} not found above {AppContext.BaseDirectory}");
        Environment.CurrentDirectory = dir;
        GameData.LoadEverything();

        var random = new Random(355);
        _templateEntries = new uint[Calls];
        _sparseOnlyEntries = new uint[Calls];
        _appearanceEntries = new uint[Calls];
        uint[] sparse = [.. GameData.ItemSparseRecordsStore.Keys];
        uint[] appearances = [.. GameData.ItemModifiedAppearanceByItemStore.Keys];

        // 2,000 templates, as after a while in the world; looked up in a shuffled order.
        for (uint i = 0; i < 2_000; i++)
        {
            uint entry = sparse[i];
            GameData.StoreItemTemplate(entry, new ItemTemplate { Entry = entry });
            GameData.StoreQuestTemplate(entry, new QuestTemplate());
        }
        for (int i = 0; i < Calls; i++)
        {
            _templateEntries[i] = sparse[random.Next(0, 2_000)];
            _sparseOnlyEntries[i] = sparse[random.Next(2_000, sparse.Length)];
            _appearanceEntries[i] = appearances[random.Next(0, appearances.Length)];
            GameData.SaveItemEffectSlot(_templateEntries[i], 100 + (uint)i, 0);
        }

        _template = new ItemTemplate { Entry = _templateEntries[0] };

        // An item the generators have already brought up to date: the second run of each finds
        // nothing to change, as it does for every session after the first.
        // One with a display the client can draw: the generators skip an item without one.
        uint upToDate = 0, display = 0;
        for (int i = sparse.Length - 1; i >= 2_000 && upToDate == 0; i--)
        {
            if (GameData.ItemDisplayIdStore.TryGetValue(sparse[i], out display) &&
                GameData.GetItemIconFileDataIdByDisplayId(display) != 0)
                upToDate = sparse[i];
        }
        _upToDate = new ItemTemplate { Entry = upToDate, DisplayID = display, Name = ["Bench", "", "", ""] };
        RunGenerators(_upToDate);
        if (RunGenerators(_upToDate) != 0)
            throw new InvalidOperationException("The generators still change the item on a second run.");
    }

    private static int RunGenerators(ItemTemplate item)
    {
        int replies = 0;
        if (GameData.GenerateItemUpdateIfNeeded(item) != null) replies++;
        if (GameData.GenerateItemSparseUpdateIfNeeded(item) != null) replies++;
        for (byte slot = 0; slot < 5; slot++)
            if (GameData.GenerateItemEffectUpdateIfNeeded(item, slot) != null) replies++;
        if (GameData.GenerateItemAppearanceUpdateIfNeeded(item) != null) replies++;
        if (GameData.GenerateItemModifiedAppearanceUpdateIfNeeded(item) != null) replies++;
        return replies;
    }

    [Benchmark(OperationsPerInvoke = Calls)]
    public int ItemTemplate_Hit()
    {
        int found = 0;
        foreach (uint entry in _templateEntries)
            if (GameData.GetItemTemplate(entry) != null)
                found++;
        return found;
    }

    [Benchmark(OperationsPerInvoke = Calls)]
    public int QuestTemplate_Hit()
    {
        int found = 0;
        foreach (uint entry in _templateEntries)
            if (GameData.GetQuestTemplate(entry) != null)
                found++;
        return found;
    }

    [Benchmark(OperationsPerInvoke = Calls)]
    public uint AuraSpellRemap_Miss()
    {
        uint sum = 0;
        foreach (uint entry in _templateEntries)
            sum += GameData.GetModernSpellId(entry);
        return sum;
    }

    [Benchmark(OperationsPerInvoke = Calls)]
    public uint ItemLevel_FromItemSparse()
    {
        uint sum = 0;
        foreach (uint entry in _sparseOnlyEntries)
            if (GameData.TryGetItemLevel(entry, out _, out uint level))
                sum += level;
        return sum;
    }

    [Benchmark(OperationsPerInvoke = Calls)]
    public int ItemAppearance_ByItem()
    {
        int found = 0;
        foreach (uint entry in _appearanceEntries)
            if (GameData.GetItemModifiedAppearanceByItemId(entry) != null)
                found++;
        return found;
    }

    [Benchmark(OperationsPerInvoke = Calls)]
    public int ItemSpellSlot()
    {
        int sum = 0;
        for (int i = 0; i < Calls; i++)
            sum += GameData.GetItemEffectSlot(_templateEntries[i], 100 + (uint)i);
        return sum;
    }

    [Benchmark(OperationsPerInvoke = Calls)]
    public void ItemTemplate_StoreAgain()
    {
        foreach (uint entry in _templateEntries)
            GameData.StoreItemTemplate(entry, _template);
    }

    [Benchmark(OperationsPerInvoke = Calls)]
    public void ItemSpellSlot_SaveAgain()
    {
        for (int i = 0; i < Calls; i++)
            GameData.SaveItemEffectSlot(_templateEntries[i], 100 + (uint)i, 0);
    }

    /// <summary>One item query's hotfix check for an item already up to date: nine generator calls.</summary>
    [Benchmark]
    public int ItemQuery_Generators_UpToDate() => RunGenerators(_upToDate);
}
