using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace HermesProxy.Benchmarks;

/// <summary>
/// Issue #355: the cost of making a GameData store safe for several sessions, per lookup and per
/// store, against the plain <see cref="Dictionary{TKey,TValue}"/> it is today. The same uint keys
/// and reference values the stores hold, looked up in a shuffled order so the walk is not a
/// stride the prefetcher can follow.
/// </summary>
/// <remarks>
/// Size 2,000 is a template store after a while in the world (creatures, quests, items the server
/// described); 45,000 is the 3.4.3 ItemSparse table loaded from CSV. Hit is the lookup the hot
/// paths make; a miss is the "do we know this item yet" check before an item query.
/// </remarks>
[MemoryDiagnoser]
public class GameDataStoreBenchmarks
{
    private const int Lookups = 1024;

    [Params(2_000, 45_000)]
    public int Size;

    [Params(true, false)]
    public bool Hit;

    private Dictionary<uint, object> _dictionary = null!;
    private ConcurrentDictionary<uint, object> _concurrent = null!;
    private readonly Lock _lock = new();
    private uint[] _keys = null!;
    private object _value = null!;

    [GlobalSetup]
    public void Setup()
    {
        _value = new object();
        _dictionary = new Dictionary<uint, object>(Size);
        _concurrent = new ConcurrentDictionary<uint, object>();
        for (uint i = 1; i <= Size; i++)
        {
            var value = new object();
            _dictionary[i] = value;
            _concurrent[i] = value;
        }

        var random = new Random(355);
        _keys = new uint[Lookups];
        for (int i = 0; i < Lookups; i++)
        {
            uint key = (uint)random.Next(1, Size + 1);
            _keys[i] = Hit ? key : key + (uint)Size;
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = Lookups)]
    public int Lookup_Dictionary()
    {
        int found = 0;
        var store = _dictionary;
        foreach (uint key in _keys)
            if (store.TryGetValue(key, out _))
                found++;
        return found;
    }

    [Benchmark(OperationsPerInvoke = Lookups)]
    public int Lookup_ConcurrentDictionary()
    {
        int found = 0;
        var store = _concurrent;
        foreach (uint key in _keys)
            if (store.TryGetValue(key, out _))
                found++;
        return found;
    }

    [Benchmark(OperationsPerInvoke = Lookups)]
    public int Lookup_DictionaryUnderLock()
    {
        int found = 0;
        var store = _dictionary;
        foreach (uint key in _keys)
        {
            lock (_lock)
            {
                if (store.TryGetValue(key, out _))
                    found++;
            }
        }
        return found;
    }
}

/// <summary>
/// What a store costs to hold: filling one with <see cref="Size"/> rows, as a CSV loader does at
/// start-up. Allocated is the store's own footprint (the values are made once in the setup).
/// </summary>
[MemoryDiagnoser]
public class GameDataStoreBuildBenchmarks
{
    [Params(2_000, 45_000)]
    public int Size;

    private object[] _values = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = new object[Size];
        for (int i = 0; i < Size; i++)
            _values[i] = new object();
    }

    [Benchmark(Baseline = true)]
    public Dictionary<uint, object> Build_Dictionary()
    {
        var store = new Dictionary<uint, object>(Size);
        for (int i = 0; i < Size; i++)
            store.Add((uint)i + 1, _values[i]);
        return store;
    }

    [Benchmark]
    public ConcurrentDictionary<uint, object> Build_ConcurrentDictionary()
    {
        var store = new ConcurrentDictionary<uint, object>(Environment.ProcessorCount, Size);
        for (int i = 0; i < Size; i++)
            store.TryAdd((uint)i + 1, _values[i]);
        return store;
    }

    // As the template stores are made: no capacity, grown as the server answers.
    [Benchmark]
    public ConcurrentDictionary<uint, object> Build_ConcurrentDictionary_Grown()
    {
        var store = new ConcurrentDictionary<uint, object>();
        for (int i = 0; i < Size; i++)
            store.TryAdd((uint)i + 1, _values[i]);
        return store;
    }
}

/// <summary>
/// The write half of <see cref="GameDataStoreBenchmarks"/>: storing a template the server has
/// described again, so the key is already there, as it is for every session after the first.
/// </summary>
[MemoryDiagnoser]
public class GameDataStoreWriteBenchmarks
{
    private const int Stores = 1024;

    [Params(2_000, 45_000)]
    public int Size;

    private Dictionary<uint, object> _dictionary = null!;
    private ConcurrentDictionary<uint, object> _concurrent = null!;
    private readonly Lock _lock = new();
    private uint[] _keys = null!;
    private object _value = null!;

    [GlobalSetup]
    public void Setup()
    {
        _value = new object();
        _dictionary = new Dictionary<uint, object>(Size);
        _concurrent = new ConcurrentDictionary<uint, object>();
        for (uint i = 1; i <= Size; i++)
        {
            _dictionary[i] = _value;
            _concurrent[i] = _value;
        }

        var random = new Random(355);
        _keys = new uint[Stores];
        for (int i = 0; i < Stores; i++)
            _keys[i] = (uint)random.Next(1, Size + 1);
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = Stores)]
    public void Store_Dictionary()
    {
        var store = _dictionary;
        foreach (uint key in _keys)
            store[key] = _value;
    }

    [Benchmark(OperationsPerInvoke = Stores)]
    public void Store_ConcurrentDictionary()
    {
        var store = _concurrent;
        foreach (uint key in _keys)
            store[key] = _value;
    }

    [Benchmark(OperationsPerInvoke = Stores)]
    public void Store_DictionaryUnderLock()
    {
        var store = _dictionary;
        foreach (uint key in _keys)
        {
            lock (_lock)
                store[key] = _value;
        }
    }
}
