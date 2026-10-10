using System;
using System.Buffers;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Framework.Cryptography;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Benchmarks;

// Outbound path from `new ServerPacket()` to the encrypted wire frame, reproducing
// WorldSocket.SendPacket minus the socket write and minus the >1KB compression branch
// (all three packets here are far below it).
//
// Three stages per packet so the cost can be attributed:
//   *_Construct       ctor only — nothing is rented until the first write (it used to be 256 B up front)
//   *_WritePacketData ctor + serialise into a pooled buffer, released as the send path releases it
//   *_Wire            ctor + serialise + opcode framing + AES-GCM + 16-byte header, as sent
// *_SpanOnly is the floor: WriteToSpan straight into a caller-owned buffer, nothing else.
[MemoryDiagnoser]
[ShortRunJob]
public class SendPipelineBenchmarks
{
    private static readonly byte[] Key16 = "0123456789ABCDEF"u8.ToArray();

    private WorldCrypt _crypt = null!;
    private WowGuid128 _guid;
    private ServerSideMovement _spline = null!;
    private byte[] _spanBuffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;

        WorldCrypt.ForceBouncyCastleForTests = false;
        _crypt = new WorldCrypt();
        _crypt.Initialize(Key16);

        _guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        _spline = new ServerSideMovement
        {
            SplineType = SplineTypeModern.None,
            SplineFlags = SplineFlagModern.None,
            SplineId = 1,
            SplineTimeFull = 1000,
            SplineMode = 0,
            StartPosition = new Vector3(100f, 200f, 300f),
            EndPosition = new Vector3(110f, 210f, 310f),
            TransportGuid = WowGuid128.Empty,
            TransportSeat = 0,
            FinalOrientation = 1.5f,
            FinalFacingSpot = Vector3.Zero,
            FinalFacingGuid = WowGuid128.Empty,
            SplinePoints = new List<Vector3>(),
        };
        _spanBuffer = new byte[4096];
    }

    [GlobalCleanup]
    public void Cleanup() => _crypt?.Dispose();

    private PowerUpdate NewPowerUpdate()
    {
        var packet = new PowerUpdate(_guid);
        packet.Powers.Add(new PowerUpdatePower(1234, 0));
        packet.Powers.Add(new PowerUpdatePower(56, 1));
        return packet;
    }

    private MonsterMove NewMonsterMove()
    {
        var packet = new MonsterMove(_guid, _spline);
        return packet;
    }

    private static CriteriaDeletedPkt NewCriteriaDeleted() => new() { CriteriaID = 42 };

    // Mirrors WorldSocket.SendPacket after WritePacketData: opcode + payload laid out behind
    // the header in one pooled frame, AES-GCM in place, header written in front, then the
    // serialized bytes released.
    private int Wire(ServerPacket packet)
    {
        packet.WritePacketData();
        ReadOnlySpan<byte> data = packet.GetDataSpan();
        uint opcode = packet.GetOpcode();
        int opcodeSize = ModernVersion.OpcodeSize;

        int bodySize = ModernPacketBody.PlainSize(opcodeSize, data.Length);
        int framedSize = PacketHeader.StructSize + bodySize;
        byte[] framed = ArrayPool<byte>.Shared.Rent(framedSize);
        Span<byte> body = framed.AsSpan(PacketHeader.StructSize, bodySize);
        ModernPacketBody.WritePlain(body, opcode, opcodeSize, data);

        PacketHeader header = new();
        header.Size = bodySize;
        _crypt.Encrypt(body, header.Tag);
        header.Write(framed);

        ArrayPool<byte>.Shared.Return(framed);
        packet.ReleaseData();
        return framedSize;
    }

    private static int Serialize(ServerPacket packet)
    {
        packet.WritePacketData();
        int length = packet.GetDataSpan().Length;
        packet.ReleaseData();
        return length;
    }

    // ---- PowerUpdate: ISpanWritable, ~30 bytes, fires on every power change ----

    [Benchmark(Baseline = true)]
    public int PowerUpdate_Construct() => NewPowerUpdate().Powers.Count;

    [Benchmark]
    public int PowerUpdate_WritePacketData()
    {
        return Serialize(NewPowerUpdate());
    }

    [Benchmark]
    public int PowerUpdate_Wire() => Wire(NewPowerUpdate());

    [Benchmark]
    public int PowerUpdate_SpanOnly()
    {
        var packet = NewPowerUpdate();
        return packet.WriteToSpan(_spanBuffer);
    }

    // ---- MonsterMove: ISpanWritable, spline packet, highest-volume SMSG in busy zones ----

    [Benchmark]
    public int MonsterMove_Construct() => NewMonsterMove().MaxSize;

    [Benchmark]
    public int MonsterMove_WritePacketData()
    {
        return Serialize(NewMonsterMove());
    }

    [Benchmark]
    public int MonsterMove_Wire() => Wire(NewMonsterMove());

    [Benchmark]
    public int MonsterMove_SpanOnly()
    {
        var packet = NewMonsterMove();
        return packet.WriteToSpan(_spanBuffer);
    }

    // ---- CriteriaDeleted: 4-byte body, NOT ISpanWritable — the ByteBuffer fallback path ----

    [Benchmark]
    public uint CriteriaDeleted_Construct() => NewCriteriaDeleted().CriteriaID;

    [Benchmark]
    public int CriteriaDeleted_WritePacketData()
    {
        return Serialize(NewCriteriaDeleted());
    }

    [Benchmark]
    public int CriteriaDeleted_Wire() => Wire(NewCriteriaDeleted());
}
