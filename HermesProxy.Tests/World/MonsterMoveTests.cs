using System.Collections.Generic;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

public class MonsterMoveConstructorTests
{
    static MonsterMoveConstructorTests()
    {
        if (global::Framework.Settings.ClientBuild == ClientVersionBuild.Zero)
            global::Framework.Settings.ClientBuild = ClientVersionBuild.V1_14_2_42597;
    }

    private static WowGuid128 TestGuid => WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);

    private static ServerSideMovement CreateBaseSpline(
        SplineTypeModern splineType = SplineTypeModern.None,
        SplineFlagModern flags = SplineFlagModern.None)
    {
        return new ServerSideMovement
        {
            SplineType = splineType,
            SplineFlags = flags,
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
        };
    }

    [Fact]
    public void Constructor_UncompressedPath_AddsPointsAndEndPosition()
    {
        var spline = CreateBaseSpline(flags: SplineFlagModern.UncompressedPath);
        spline.SplinePoints = new List<Vector3>
        {
            new(101f, 201f, 301f),
            new(102f, 202f, 302f),
        };

        var packet = new MonsterMove(TestGuid, spline);

        // SplinePoints + EndPosition
        Assert.Equal(3, packet.Points.Count);
        Assert.Empty(packet.PackedDeltas);
    }

    [Fact]
    public void Constructor_CompressedPath_CalculatesDeltas()
    {
        var spline = CreateBaseSpline(); // No UncompressedPath flag
        spline.SplinePoints = new List<Vector3>
        {
            new(104f, 204f, 304f),
            new(106f, 206f, 306f),
        };

        var packet = new MonsterMove(TestGuid, spline);

        // EndPosition added as point
        Assert.Single(packet.Points);
        Assert.Equal(spline.EndPosition, packet.Points[0]);
        // Deltas calculated from midpoint
        Assert.Equal(2, packet.PackedDeltas.Count);
    }

    [Fact]
    public void Constructor_NoEndPosition_NoPointsAdded()
    {
        var spline = CreateBaseSpline();
        spline.EndPosition = Vector3.Zero;
        spline.SplinePoints = new List<Vector3>();

        var packet = new MonsterMove(TestGuid, spline);

        Assert.Empty(packet.Points);
        Assert.Empty(packet.PackedDeltas);
    }
}

public class MonsterMoveWriteTests
{
    static MonsterMoveWriteTests()
    {
        if (global::Framework.Settings.ClientBuild == ClientVersionBuild.Zero)
            global::Framework.Settings.ClientBuild = ClientVersionBuild.V1_14_2_42597;
    }

    private static WowGuid128 TestGuid => WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);

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

    [Theory]
    [InlineData(SplineTypeModern.FacingSpot)]
    [InlineData(SplineTypeModern.FacingTarget)]
    [InlineData(SplineTypeModern.FacingAngle)]
    [InlineData(SplineTypeModern.None)]
    public void WriteToSpan_MatchesWrite(SplineTypeModern splineType)
    {
        var spline = CreateSpline(splineType);
        var packet1 = new MonsterMove(TestGuid, spline);
        var packet2 = new MonsterMove(TestGuid, spline);

        // Write via ByteBuffer path
        packet1.Write();
        packet1.WritePacketData();
        byte[] byteBufferData = packet1.GetData()!;

        // Write via Span path
        byte[] spanBuffer = new byte[packet2.MaxSize];
        int written = packet2.WriteToSpan(spanBuffer);

        Assert.True(written > 0, $"WriteToSpan should succeed for {splineType}");
        Assert.Equal(byteBufferData.Length, written);
        Assert.Equal(byteBufferData, spanBuffer[..written]);
    }

    [Fact]
    public void WriteToSpan_WithPoints_MatchesWrite()
    {
        var spline = CreateSpline(SplineTypeModern.None);
        spline.SplineFlags = SplineFlagModern.UncompressedPath;
        spline.SplinePoints = new List<Vector3>
        {
            new(101f, 201f, 301f),
            new(102f, 202f, 302f),
            new(103f, 203f, 303f),
        };
        var packet1 = new MonsterMove(TestGuid, spline);
        var packet2 = new MonsterMove(TestGuid, spline);

        packet1.Write();
        packet1.WritePacketData();
        byte[] byteBufferData = packet1.GetData()!;

        byte[] spanBuffer = new byte[packet2.MaxSize];
        int written = packet2.WriteToSpan(spanBuffer);

        Assert.True(written > 0);
        Assert.Equal(byteBufferData.Length, written);
        Assert.Equal(byteBufferData, spanBuffer[..written]);
    }

    [Fact]
    public void WriteToSpan_ExceedsMaxPoints_ReturnsMinus1()
    {
        var spline = CreateSpline(SplineTypeModern.None);
        spline.SplineFlags = SplineFlagModern.UncompressedPath;
        // Create more than MaxSplinePoints (16) points
        spline.SplinePoints = new List<Vector3>();
        for (int i = 0; i < 20; i++)
            spline.SplinePoints.Add(new Vector3(i, i, i));

        var packet = new MonsterMove(TestGuid, spline);

        byte[] spanBuffer = new byte[2048];
        int written = packet.WriteToSpan(spanBuffer);

        Assert.Equal(-1, written);
    }

    [Fact]
    public void WriteToSpan_ReturnsPositiveBytesWritten()
    {
        var spline = CreateSpline(SplineTypeModern.FacingSpot);
        var packet = new MonsterMove(TestGuid, spline);

        byte[] spanBuffer = new byte[packet.MaxSize];
        int written = packet.WriteToSpan(spanBuffer);

        Assert.True(written > 0);
        Assert.True(written <= packet.MaxSize);
    }
}
