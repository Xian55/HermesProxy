using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using HermesProxy.Enums;
using HermesProxy.World.Enums;

namespace HermesProxy.Benchmarks;

/// <summary>
/// Measures the per-call cost of the four hot-path opcode translations used on every packet:
///   LegacyVersion.GetUniversalOpcode(uint)  — wire opcode (legacy server) → universal enum
///   LegacyVersion.GetCurrentOpcode(Opcode)  — universal enum → wire opcode (legacy server)
///   ModernVersion.GetUniversalOpcode(uint)  — wire opcode (modern client) → universal enum
///   ModernVersion.GetCurrentOpcode(Opcode)  — universal enum → wire opcode (modern client)
///
/// Each benchmark loops a pre-shuffled key array so branch predictor / hash-bucket access
/// patterns are representative of real traffic (not a single hot key repeatedly). 1024 iters
/// per invocation keeps the per-op time above BenchmarkDotNet's noise floor without
/// swamping the translation cost itself.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class OpcodeLookupBenchmarks
{
    private const int Iterations = 1024;

    private uint[] _legacyWireKeys = null!;
    private Opcode[] _legacyUniversalKeys = null!;
    private uint[] _modernWireKeys = null!;
    private Opcode[] _modernUniversalKeys = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Seed the static classes. Mirror the tests' conventional build (1.14.2) — auto-pairs
        // to the 1.12.1 legacy opcodes, giving us a realistic ~1k-entry legacy table and
        // a ~1.7k-entry modern table.
        if (global::HermesProxy.VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            global::HermesProxy.VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (global::HermesProxy.VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            global::HermesProxy.VersionBootstrap.LegacyBuild = ClientVersionBuild.V1_12_1_5875;

        // Touch the tables once so LoadOpcodeTables runs outside the benchmark window.
        _ = global::HermesProxy.LegacyVersion.GetUniversalOpcode(0);
        _ = global::HermesProxy.ModernVersion.GetUniversalOpcode(0);

        // Source keys: enumerate each version's native opcode enum to get the exact value
        // set present in its dictionary. That gives us only "hit" keys — the realistic case
        // for translating a packet we actually handle.
        _legacyWireKeys = BuildWireKeys(typeof(HermesProxy.World.Enums.V1_12_1_5875.Opcode));
        _modernWireKeys = BuildWireKeys(typeof(HermesProxy.World.Enums.V1_14_1_40688.Opcode));

        // Reverse direction: use the universal Opcode values that map through each version.
        _legacyUniversalKeys = BuildUniversalKeys(_legacyWireKeys, isLegacy: true);
        _modernUniversalKeys = BuildUniversalKeys(_modernWireKeys, isLegacy: false);

        Shuffle(_legacyWireKeys, 1);
        Shuffle(_modernWireKeys, 2);
        Shuffle(_legacyUniversalKeys, 3);
        Shuffle(_modernUniversalKeys, 4);
    }

    [Benchmark]
    public uint LegacyGetUniversalOpcode_Array()
    {
        var keys = _legacyWireKeys;
        uint acc = 0;
        int len = keys.Length;
        for (int i = 0; i < Iterations; i++)
            acc += (uint)global::HermesProxy.LegacyVersion.GetUniversalOpcode(keys[i % len]);
        return acc;
    }

    [Benchmark]
    public uint LegacyGetCurrentOpcode_Array()
    {
        var keys = _legacyUniversalKeys;
        uint acc = 0;
        int len = keys.Length;
        for (int i = 0; i < Iterations; i++)
            acc += global::HermesProxy.LegacyVersion.GetCurrentOpcode(keys[i % len]);
        return acc;
    }

    [Benchmark]
    public uint ModernGetUniversalOpcode_Array()
    {
        var keys = _modernWireKeys;
        uint acc = 0;
        int len = keys.Length;
        for (int i = 0; i < Iterations; i++)
            acc += (uint)global::HermesProxy.ModernVersion.GetUniversalOpcode(keys[i % len]);
        return acc;
    }

    [Benchmark]
    public uint ModernGetCurrentOpcode_Array()
    {
        var keys = _modernUniversalKeys;
        uint acc = 0;
        int len = keys.Length;
        for (int i = 0; i < Iterations; i++)
            acc += global::HermesProxy.ModernVersion.GetCurrentOpcode(keys[i % len]);
        return acc;
    }

    private static uint[] BuildWireKeys(Type enumType)
    {
        var values = Enum.GetValues(enumType);
        var list = new List<uint>(values.Length);
        foreach (var v in values)
        {
            uint wire = Convert.ToUInt32(v);
            if (wire != 0) // drop MSG_NULL_ACTION
                list.Add(wire);
        }
        return list.ToArray();
    }

    private static Opcode[] BuildUniversalKeys(uint[] wireKeys, bool isLegacy)
    {
        var list = new List<Opcode>(wireKeys.Length);
        foreach (var wire in wireKeys)
        {
            var universal = isLegacy
                ? global::HermesProxy.LegacyVersion.GetUniversalOpcode(wire)
                : global::HermesProxy.ModernVersion.GetUniversalOpcode(wire);
            if (universal != Opcode.MSG_NULL_ACTION)
                list.Add(universal);
        }
        return list.ToArray();
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
