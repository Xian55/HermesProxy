using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ObjectUpdateConstructionBenchmarks
{
    private GlobalSessionData _session = null!;
    private WowGuid128 _itemGuid;
    private WowGuid128 _creatureGuid;
    private WowGuid128 _playerGuid;
    private WowGuid128 _gameObjectGuid;
    private WowGuid128 _dynamicObjectGuid;
    private WowGuid128 _corpseGuid;

    [GlobalSetup]
    public void Setup()
    {
        _session = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        _itemGuid = WowGuid128.Create(HighGuidType703.Item, 1);
        _creatureGuid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        _playerGuid = WowGuid128.Create(HighGuidType703.Player, 1);
        _gameObjectGuid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        _dynamicObjectGuid = WowGuid128.Create(HighGuidType703.DynamicObject, 0, 100, 1);
        _corpseGuid = WowGuid128.Create(HighGuidType703.Corpse, 0, 200, 1);
    }

    [Benchmark]
    public ObjectUpdate CreateItem()
    {
        return new ObjectUpdate(_itemGuid, UpdateTypeModern.Values, _session);
    }

    [Benchmark]
    public ObjectUpdate CreateUnit()
    {
        return new ObjectUpdate(_creatureGuid, UpdateTypeModern.Values, _session);
    }

    [Benchmark(Baseline = true)]
    public ObjectUpdate CreatePlayer()
    {
        return new ObjectUpdate(_playerGuid, UpdateTypeModern.Values, _session);
    }

    [Benchmark]
    public ObjectUpdate CreateGameObject()
    {
        return new ObjectUpdate(_gameObjectGuid, UpdateTypeModern.Values, _session);
    }

    [Benchmark]
    public ObjectUpdate CreateDynamicObject()
    {
        return new ObjectUpdate(_dynamicObjectGuid, UpdateTypeModern.Values, _session);
    }

    [Benchmark]
    public ObjectUpdate CreateCorpse()
    {
        return new ObjectUpdate(_corpseGuid, UpdateTypeModern.Values, _session);
    }

    [Benchmark]
    public ObjectUpdate CreatePlayerWithCreateData()
    {
        return new ObjectUpdate(_playerGuid, UpdateTypeModern.CreateObject1, _session);
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ObjectUpdateBatchBenchmarks
{
    private GlobalSessionData _session = null!;
    private WowGuid128[] _mixedGuids = null!;

    [GlobalSetup]
    public void Setup()
    {
        _session = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

        // Simulate a zone with mixed object types (100 objects)
        _mixedGuids = new WowGuid128[100];
        for (int i = 0; i < 100; i++)
        {
            _mixedGuids[i] = (i % 5) switch
            {
                0 => WowGuid128.Create(HighGuidType703.Creature, 0, (uint)(1000 + i), (ulong)i),
                1 => WowGuid128.Create(HighGuidType703.Player, (ulong)i),
                2 => WowGuid128.Create(HighGuidType703.GameObject, 0, (uint)(2000 + i), (ulong)i),
                3 => WowGuid128.Create(HighGuidType703.Item, (ulong)i),
                _ => WowGuid128.Create(HighGuidType703.DynamicObject, 0, (uint)(3000 + i), (ulong)i),
            };
        }
    }

    /// <summary>
    /// Simulates zone load: create 100 ObjectUpdate instances of mixed types.
    /// Measures total allocation pressure from the nullable field pattern.
    /// </summary>
    [Benchmark]
    public int BatchCreate_100Mixed()
    {
        int count = 0;
        for (int i = 0; i < _mixedGuids.Length; i++)
        {
            var update = new ObjectUpdate(_mixedGuids[i], UpdateTypeModern.CreateObject1, _session);
            count += update.ObjectData != null ? 1 : 0; // Prevent dead code elimination
        }
        return count;
    }
}
