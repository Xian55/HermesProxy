using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Framework.GameMath;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class MonsterMoveWriteBenchmarks
{
    private WowGuid128 _guid;
    private ServerSideMovement _splineFacingSpot = null!;
    private ServerSideMovement _splineFacingTarget = null!;
    private ServerSideMovement _splineFacingAngle = null!;
    private ServerSideMovement _splineNone = null!;
    private ServerSideMovement _splineWithPoints = null!;
    private byte[] _spanBuffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (global::HermesProxy.VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            global::HermesProxy.VersionBootstrap.ModernBuild = ClientVersionBuild.V9_0_1_36216;

        _guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);

        _splineFacingSpot = CreateSpline(SplineTypeModern.FacingSpot);
        _splineFacingTarget = CreateSpline(SplineTypeModern.FacingTarget);
        _splineFacingAngle = CreateSpline(SplineTypeModern.FacingAngle);
        _splineNone = CreateSpline(SplineTypeModern.None);
        _splineWithPoints = CreateSplineWithPoints(8);

        // Large enough for any packet
        _spanBuffer = new byte[1024];
    }

    private static ServerSideMovement CreateSpline(SplineTypeModern splineType)
    {
        return new ServerSideMovement
        {
            SplineType = splineType,
            SplineFlags = SplineFlagModern.None,
            SplineId = 1,
            SplineTimeFull = 1000,
            SplineMode = 0,
            StartPosition = new Vector3(100f, 200f, 300f),
            EndPosition = new Vector3(110f, 210f, 310f),
            TransportGuid = WowGuid128.Empty,
            TransportSeat = 0,
            FinalOrientation = 1.5f,
            FinalFacingSpot = new Vector3(120f, 220f, 320f),
            FinalFacingGuid = WowGuid128.Create(HighGuidType703.Player, 99),
            SplinePoints = new List<Vector3>(),
        };
    }

    private static ServerSideMovement CreateSplineWithPoints(int pointCount)
    {
        var spline = new ServerSideMovement
        {
            SplineType = SplineTypeModern.None,
            SplineFlags = SplineFlagModern.UncompressedPath,
            SplineId = 1,
            SplineTimeFull = 2000,
            SplineMode = 0,
            StartPosition = new Vector3(100f, 200f, 300f),
            EndPosition = new Vector3(110f, 210f, 310f),
            TransportGuid = WowGuid128.Empty,
            TransportSeat = 0,
            FinalOrientation = 0f,
            FinalFacingSpot = Vector3.Zero,
            FinalFacingGuid = WowGuid128.Empty,
            SplinePoints = new List<Vector3>(),
        };

        for (int i = 0; i < pointCount; i++)
            spline.SplinePoints.Add(new Vector3(100 + i, 200 + i, 300 + i));

        return spline;
    }

    // ========== ByteBuffer Write (baseline) ==========

    [Benchmark(Baseline = true)]
    public byte[] Write_FacingSpot_ByteBuffer()
    {
        var packet = new MonsterMove(_guid, _splineFacingSpot);
        packet.Write();
        packet.WritePacketData();
        return packet.GetData()!;
    }

    [Benchmark]
    public byte[] Write_FacingTarget_ByteBuffer()
    {
        var packet = new MonsterMove(_guid, _splineFacingTarget);
        packet.Write();
        packet.WritePacketData();
        return packet.GetData()!;
    }

    [Benchmark]
    public byte[] Write_FacingAngle_ByteBuffer()
    {
        var packet = new MonsterMove(_guid, _splineFacingAngle);
        packet.Write();
        packet.WritePacketData();
        return packet.GetData()!;
    }

    [Benchmark]
    public byte[] Write_None_ByteBuffer()
    {
        var packet = new MonsterMove(_guid, _splineNone);
        packet.Write();
        packet.WritePacketData();
        return packet.GetData()!;
    }

    // ========== Span Write ==========

    [Benchmark]
    public int Write_FacingSpot_Span()
    {
        var packet = new MonsterMove(_guid, _splineFacingSpot);
        return packet.WriteToSpan(_spanBuffer);
    }

    [Benchmark]
    public int Write_FacingTarget_Span()
    {
        var packet = new MonsterMove(_guid, _splineFacingTarget);
        return packet.WriteToSpan(_spanBuffer);
    }

    [Benchmark]
    public int Write_FacingAngle_Span()
    {
        var packet = new MonsterMove(_guid, _splineFacingAngle);
        return packet.WriteToSpan(_spanBuffer);
    }

    [Benchmark]
    public int Write_None_Span()
    {
        var packet = new MonsterMove(_guid, _splineNone);
        return packet.WriteToSpan(_spanBuffer);
    }

    // ========== With Points ==========

    [Benchmark]
    public byte[] Write_WithPoints_ByteBuffer()
    {
        var packet = new MonsterMove(_guid, _splineWithPoints);
        packet.Write();
        packet.WritePacketData();
        return packet.GetData()!;
    }

    [Benchmark]
    public int Write_WithPoints_Span()
    {
        var packet = new MonsterMove(_guid, _splineWithPoints);
        return packet.WriteToSpan(_spanBuffer);
    }
}
