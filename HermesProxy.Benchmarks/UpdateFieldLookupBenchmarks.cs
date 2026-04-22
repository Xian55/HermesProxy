using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using HermesProxy.Enums;
using HermesProxy.World.Enums;

namespace HermesProxy.Benchmarks;

/// <summary>
/// Measures GetUpdateFieldInfo&lt;T&gt; — the hot path UpdateHandler.ReadValuesUpdateBlock hits
/// ~223 times per SMSG_UPDATE_OBJECT packet, across 14 distinct enum types. Currently costs:
///   1. Dictionary&lt;Type, SortedList&lt;int, UpdateFieldInfo&gt;&gt;.TryGetValue(typeof(T), …)
///   2. SortedList.BinarySearch(field) — O(log n) with indirect key/value access
///   3. Return the UpdateFieldInfo at the nearest preceding index
///
/// Each benchmark loops a pre-shuffled int-key array (realistic per-packet access: random
/// ordering across the fields present in one update block).
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class UpdateFieldLookupBenchmarks
{
    private const int Iterations = 1024;

    // Two of the most heavily-accessed enum types on the hot path.
    private int[] _playerKeys = null!;
    private int[] _unitKeys = null!;
    private int[] _objectKeys = null!;
    private int[] _itemKeys = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Seed version state (tests mirror V1_14_2 modern + V1_12_1 legacy).
        if (global::HermesProxy.VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            global::HermesProxy.VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (global::HermesProxy.VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            global::HermesProxy.VersionBootstrap.LegacyBuild = ClientVersionBuild.V1_12_1_5875;

        // Touch the statics so LegacyVersion / ModernVersion cctors fire — gives us a
        // populated table to sample valid keys from.
        _ = global::HermesProxy.LegacyVersion.GetUpdateFieldsDefiningBuild();
        _ = global::HermesProxy.ModernVersion.GetUpdateFieldsDefiningBuild();

        // Sample keys by probing the table itself — GetUpdateFieldInfo only accepts values
        // that are ≥ the lowest start offset (UpdateHandler in production only queries keys
        // it received in the packet bitmap, so this mirrors real traffic).
        _playerKeys = SampleValidKeys<PlayerField>(forLegacy: true);
        _unitKeys = SampleValidKeys<UnitField>(forLegacy: true);
        _objectKeys = SampleValidKeys<ObjectField>(forLegacy: true);
        _itemKeys = SampleValidKeys<ItemField>(forLegacy: true);

        Shuffle(_playerKeys, 11);
        Shuffle(_unitKeys, 12);
        Shuffle(_objectKeys, 13);
        Shuffle(_itemKeys, 14);
    }

    [Benchmark]
    public int LegacyGetUpdateFieldInfo_Player_Array()
    {
        var keys = _playerKeys;
        int len = keys.Length;
        int acc = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var info = global::HermesProxy.LegacyVersion.GetUpdateFieldInfo<PlayerField>(keys[i % len]);
            if (info != null) acc += info.Value;
        }
        return acc;
    }

    [Benchmark]
    public int LegacyGetUpdateFieldInfo_Unit_Array()
    {
        var keys = _unitKeys;
        int len = keys.Length;
        int acc = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var info = global::HermesProxy.LegacyVersion.GetUpdateFieldInfo<UnitField>(keys[i % len]);
            if (info != null) acc += info.Value;
        }
        return acc;
    }

    [Benchmark]
    public int LegacyGetUpdateFieldInfo_Object_Array()
    {
        var keys = _objectKeys;
        int len = keys.Length;
        int acc = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var info = global::HermesProxy.LegacyVersion.GetUpdateFieldInfo<ObjectField>(keys[i % len]);
            if (info != null) acc += info.Value;
        }
        return acc;
    }

    [Benchmark]
    public int LegacyGetUpdateFieldInfo_Item_Array()
    {
        var keys = _itemKeys;
        int len = keys.Length;
        int acc = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var info = global::HermesProxy.LegacyVersion.GetUpdateFieldInfo<ItemField>(keys[i % len]);
            if (info != null) acc += info.Value;
        }
        return acc;
    }

    [Benchmark]
    public int ModernGetUpdateFieldInfo_Player_Array()
    {
        var keys = _playerKeys;
        int len = keys.Length;
        int acc = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var info = global::HermesProxy.ModernVersion.GetUpdateFieldInfo<PlayerField>(keys[i % len]);
            if (info != null) acc += info.Value;
        }
        return acc;
    }

    private static int[] SampleValidKeys<T>(bool forLegacy) where T : System.Enum
    {
        // Walk a dense int range probing GetUpdateFieldInfo — any field the method returns
        // non-null for is in the table. Upper bound 20 000 is well above any known max offset.
        var found = new List<int>();
        for (int i = 0; i < 20_000; i++)
        {
            var info = forLegacy
                ? global::HermesProxy.LegacyVersion.GetUpdateFieldInfo<T>(i)
                : global::HermesProxy.ModernVersion.GetUpdateFieldInfo<T>(i);
            if (info is not null && info.Value == i)
                found.Add(i);
        }
        return found.ToArray();
    }

    private static void Shuffle<T>(T[] array, int seed)
    {
        var rng = new Random(seed);
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
