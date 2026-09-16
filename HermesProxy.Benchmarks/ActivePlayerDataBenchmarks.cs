using BenchmarkDotNet.Attributes;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Benchmarks;

/// <summary>
/// What one <see cref="ActivePlayerData"/> costs to bring into existence, and what an update
/// carrying a single owner field costs to write.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ObjectUpdate"/> already declines to allocate this block for foreign players, so a
/// battleground full of bots no longer pays for it. What is left is the local player, where every
/// owner-field delta — a combo point, a farsight target, a stable-slot count — materialises all
/// thirty nullable arrays. <c>QuestCompleted[875]</c> is 14,024 bytes of that on its own, and the
/// four V1_14/V2_5 builders then scan all 875 slots whether or not a quest bit was ever set.
/// </para>
/// <para>
/// <see cref="Allocate"/> is the honest headline: it has no session, no builder and no I/O, so its
/// allocated-bytes column is exactly the constructor's array footprint and nothing else.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[ShortRunJob]
public class ActivePlayerDataBenchmarks
{
    private WowGuid128 _playerGuid;
    private GlobalSessionData _session = null!;

    [GlobalSetup]
    public void Setup()
    {
        _playerGuid = WowGuid128.Create(HighGuidType703.Player, 1);
        _session = (GlobalSessionData)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(GlobalSessionData));
    }

    /// <summary>Constructor only. Allocated bytes here is the array footprint, full stop.</summary>
    [Benchmark(Baseline = true)]
    public ActivePlayerData Allocate() => new ActivePlayerData();

    /// <summary>
    /// The shape that dominates a live session: an owner-field delta that touches exactly one
    /// scalar and never looks at a quest bit, an explored zone or a skill line.
    /// </summary>
    [Benchmark]
    public ObjectUpdate SingleOwnerField()
    {
        var update = new ObjectUpdate(_playerGuid, UpdateTypeModern.Values, _session);
        update.EnsureActivePlayerData().ComboTarget = _playerGuid;
        return update;
    }

    /// <summary>
    /// A quest completion, as <c>CompletedQuestTracker.SendSingleUpdateToClient</c> builds it:
    /// one 64-bit word out of 875 set, everything else untouched.
    /// </summary>
    [Benchmark]
    public ObjectUpdate SingleQuestBit()
    {
        var update = new ObjectUpdate(_playerGuid, UpdateTypeModern.Values, _session);
        update.EnsureActivePlayerData().EnsureQuestCompleted()[3] = 1UL << 17;
        return update;
    }
}
