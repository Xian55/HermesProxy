using System;
using System.Buffers;
using BenchmarkDotNet.Attributes;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Benchmarks;

/// <summary>
/// The <see cref="WorldPacket"/> object every packet is carried in, in both directions. After
/// PR #321 the receive-side object was the largest allocation left in the Alterac Valley trace
/// (900k party-state packets in one match). Each arm lets the packet escape the way production
/// does — into <c>SessionExecutor.Post</c> as an <c>object</c>, or into an outbox — so the JIT
/// cannot stack-allocate it and the numbers are the heap cost production pays.
/// </summary>
[MemoryDiagnoser]
public class PacketObjectBenchmarks
{
    private static readonly WowGuid128 AuraTarget = WowGuid128.Create(HighGuidType703.Player, 42);

    private byte[] _partyStateWire = null!;
    private object? _escape;

    [GlobalSetup]
    public void Setup()
    {
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;

        // A party-member partial state as AzerothCore sends it mid-battle: opcode, packed guid,
        // update mask, health.
        _partyStateWire = [0x7C, 0x00, 0x01, 0x2A, 0x02, 0x00, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00];
    }

    /// <summary>
    /// <c>WorldClient.ReceiveLoop</c> → <c>Executor.Post</c> → <c>HandlePacketOnOwner</c>: a
    /// pooled read-mode packet, handed on as an object, read, then disposed.
    /// </summary>
    [Benchmark(Baseline = true)]
    public uint ReceiveLegacyPacket()
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(_partyStateWire.Length);
        _partyStateWire.CopyTo(buffer, 0);
        var packet = new WorldPacket(buffer, _partyStateWire.Length, isPooled: true);
        packet.SetReceiveTime(Environment.TickCount);
        _escape = packet;
        using (packet)
            return packet.ReadUInt8();
    }

    /// <summary>A CMSG built by a handler and disposed by <c>WorldClient.SendPacket</c>.</summary>
    [Benchmark]
    public uint SendLegacyPacket()
    {
        var packet = new WorldPacket(0x1CEu);
        packet.WriteUInt64(0x0000000000000042);
        packet.WriteUInt32(133);
        _escape = packet;
        using (packet)
            return packet.GetSize();
    }

    /// <summary>
    /// A modern packet on the ByteBuffer path (not <c>ISpanWritable</c>): its <c>_worldPacket</c>
    /// is created on first write, serialized, detached and released as <c>WorldSocket</c> does.
    /// </summary>
    [Benchmark]
    public int SendModernPacket()
    {
        var packet = new AuraUpdate(AuraTarget, false);
        packet.Auras.Add(new AuraInfo { Slot = 3 });
        _escape = packet;
        packet.WritePacketData();
        int length = packet.GetDataSpan().Length;
        packet.ReleaseData();
        return length;
    }
}
