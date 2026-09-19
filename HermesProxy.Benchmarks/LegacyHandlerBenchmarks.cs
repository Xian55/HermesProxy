using System;
using BenchmarkDotNet.Attributes;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;

namespace HermesProxy.Benchmarks;

/// <summary>
/// Real legacy handlers on a real session, fed 3.3.5a packets shaped like Alterac Valley traffic
/// (see <see cref="AlteracValleyScenario"/>). Each operation is one received packet: parse,
/// translate, serialize every client packet it produces, then the per-packet outbox tick. The
/// sockets are sinks, so encryption and the send syscall are the only production steps missing.
/// </summary>
[MemoryDiagnoser]
public class LegacyHandlerBenchmarks
{
    private LegacyHandlerHarness _harness = null!;
    private byte[] _valuesBatch = null!;
    private byte[] _spellStart = null!;
    private byte[] _spellGo = null!;
    private Action<WorldPacket> _handleUpdateObject = null!;
    private Action<WorldPacket> _handleSpellStart = null!;
    private Action<WorldPacket> _handleSpellGo = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;

        _harness = new LegacyHandlerHarness(recordClientPackets: false);
        var scenario = new AlteracValleyScenario(_harness);
        _valuesBatch = scenario.BuildValuesBatch();
        _spellStart = scenario.BuildSpellStart();
        _spellGo = scenario.BuildSpellGo();

        _handleUpdateObject = _harness.Client.HandleUpdateObject;
        _handleSpellStart = _harness.Client.HandleSpellStart;
        _handleSpellGo = _harness.Client.HandleSpellGo;

        // The first Values batch builds the field caches every later one merges into.
        UpdateObject_ValuesBatch();
        if (_harness.ClientWire.Count == 0)
            throw new InvalidOperationException("The Values batch sent nothing; the scenario is not reaching the writer.");
    }

    /// <summary>Eight Values blocks: four creatures, three other players, the player's own mana.</summary>
    [Benchmark(Baseline = true)]
    public void UpdateObject_ValuesBatch()
        => _harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, _valuesBatch, _handleUpdateObject);

    [Benchmark]
    public void SpellStart()
        => _harness.Deliver(Opcode.SMSG_SPELL_START, _spellStart, _handleSpellStart);

    [Benchmark]
    public void SpellGo()
        => _harness.Deliver(Opcode.SMSG_SPELL_GO, _spellGo, _handleSpellGo);
}
