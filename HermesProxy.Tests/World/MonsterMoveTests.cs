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
        Assert.Equal(3, packet.PointCount);
        Assert.Equal(0, packet.PackedDeltaCount);
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
        Assert.Equal(1, packet.PointCount);
        Assert.Equal(spline.EndPosition, packet.Point(0));
        // Deltas calculated from midpoint
        Assert.Equal(2, packet.PackedDeltaCount);
    }

    [Fact]
    public void Constructor_NoEndPosition_NoPointsAdded()
    {
        var spline = CreateBaseSpline();
        spline.EndPosition = Vector3.Zero;
        spline.SplinePoints = new List<Vector3>();

        var packet = new MonsterMove(TestGuid, spline);

        Assert.Equal(0, packet.PointCount);
        Assert.Equal(0, packet.PackedDeltaCount);
    }

    private static readonly WowGuid128 Vehicle = WowGuid128.Create(HighGuidType703.Player, 77);

    // A seat is the origin of the vehicle. A native 3.4.3 server sends that destination as one
    // point at (0,0,0), with no stop tolerance.
    [Fact]
    public void Constructor_SeatAtTheOrigin_IsSentAsAPoint()
    {
        var spline = CreateBaseSpline(SplineTypeModern.FacingAngle, SplineFlagModern.TransportEnter);
        spline.StartPosition = Vector3.Zero;
        spline.EndPosition = Vector3.Zero;
        spline.SplineCount = 1;
        spline.TransportGuid = Vehicle;
        spline.TransportSeat = 2;

        var packet = new MonsterMove(TestGuid, spline);

        Assert.Equal(1, packet.PointCount);
        Assert.Equal(Vector3.Zero, packet.Point(0));
        Assert.Equal(0, packet.PackedDeltaCount);
        Assert.Equal(0, StopDistanceTolerance(packet));
    }

    // A stop carries no destination at all, on a transport or off it.
    [Fact]
    public void Constructor_StopOnATransport_HasNoPoint()
    {
        var spline = CreateBaseSpline();
        spline.EndPosition = Vector3.Zero;
        spline.SplineCount = 0;
        spline.TransportGuid = Vehicle;

        var packet = new MonsterMove(TestGuid, spline);

        Assert.Equal(0, packet.PointCount);
        Assert.Equal(2, StopDistanceTolerance(packet));
    }

    // A smooth path keeps all of its positions in the point list; there is no separate end to add.
    [Fact]
    public void Constructor_SmoothPathOnATransport_GainsNoPoint()
    {
        var spline = CreateBaseSpline(flags: SplineFlagModern.UncompressedPath | SplineFlagModern.CatmullRom);
        spline.EndPosition = Vector3.Zero;
        spline.SplineCount = 2;
        spline.TransportGuid = Vehicle;
        spline.SplinePoints = new List<Vector3> { new(1f, 2f, 3f), new(4f, 5f, 6f) };

        var packet = new MonsterMove(TestGuid, spline);

        Assert.Equal(2, packet.PointCount);
    }

    // mover guid, start, spline id, destination, then one byte: CrzTeleport and the tolerance.
    private static int StopDistanceTolerance(MonsterMove packet)
    {
        packet.Write();
        packet.WritePacketData();
        using var reader = new WorldPacket(1, packet.GetData()!);
        reader.ReadPackedGuid128();
        reader.ReadVector3();
        reader.ReadUInt32();
        reader.ReadVector3();
        return (reader.ReadUInt8() >> 4) & 0x7;
    }
}

public class MonsterMoveWriteTests
{
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

    /// <summary>
    /// A 20-point spline used to exceed the fixed cap of 16 and fall back to the ByteBuffer
    /// path. MaxSize is now sized from the spline the packet actually carries, so this takes
    /// the span path -- which makes byte-for-byte equivalence with Write() load-bearing for
    /// long splines, not just short ones. Bot pathing in a battleground produces these
    /// constantly (370 fallbacks in ten minutes before the change).
    /// </summary>
    [Fact]
    public void WriteToSpan_LongSpline_TakesSpanPathAndMatchesWrite()
    {
        var spline = CreateSpline(SplineTypeModern.None);
        spline.SplineFlags = SplineFlagModern.UncompressedPath;
        spline.SplinePoints = new List<Vector3>();
        for (int i = 0; i < 20; i++)
            spline.SplinePoints.Add(new Vector3(i, i * 2f, i * 3f));

        var packet1 = new MonsterMove(TestGuid, spline);
        var packet2 = new MonsterMove(TestGuid, spline);

        packet1.Write();
        packet1.WritePacketData();
        byte[] byteBufferData = packet1.GetData()!;

        byte[] spanBuffer = new byte[packet2.MaxSize];
        int written = packet2.WriteToSpan(spanBuffer);

        Assert.True(written > 0, "a 20-point spline must no longer fall back");
        Assert.Equal(byteBufferData.Length, written);
        Assert.Equal(byteBufferData, spanBuffer[..written]);
    }

    /// <summary>
    /// The -1 bail survives as a corruption guard: SplineCount is read unbounded off the
    /// legacy wire, and a garbage count must fall back rather than drive a huge pooled rent.
    /// </summary>
    [Fact]
    public void WriteToSpan_AbsurdSplineCount_FallsBack()
    {
        var spline = CreateSpline(SplineTypeModern.None);
        spline.SplineFlags = SplineFlagModern.UncompressedPath;
        spline.SplinePoints = new List<Vector3>();
        for (int i = 0; i < 5000; i++)
            spline.SplinePoints.Add(new Vector3(i, i, i));

        var packet = new MonsterMove(TestGuid, spline);

        byte[] spanBuffer = new byte[packet.MaxSize];
        Assert.Equal(-1, packet.WriteToSpan(spanBuffer));
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

/// <summary>
/// MonsterMove used to copy the spline into Points and PackedDeltas lists in its constructor;
/// it now reads the spline as it writes. The sequences must be exactly what those lists held.
/// </summary>
public class MonsterMoveLayoutTests
{
    // The constructor body that built the two lists, verbatim.
    private static (List<Vector3> Points, List<Vector3> Deltas) Reference(ServerSideMovement moveSpline)
    {
        var points = new List<Vector3>();
        var deltas = new List<Vector3>();
        if (moveSpline.SplineFlags.HasFlag(SplineFlagModern.UncompressedPath))
        {
            if (!moveSpline.SplineFlags.HasFlag(SplineFlagModern.Cyclic))
            {
                foreach (var point in moveSpline.SplinePoints)
                    points.Add(point);
                if (moveSpline.EndPosition != Vector3.Zero)
                    points.Add(moveSpline.EndPosition);
            }
            else
            {
                if (moveSpline.EndPosition != Vector3.Zero)
                    points.Add(moveSpline.EndPosition);
                foreach (var point in moveSpline.SplinePoints)
                    points.Add(point);
            }
        }
        else if (moveSpline.EndPosition != Vector3.Zero)
        {
            points.Add(moveSpline.EndPosition);
            if (moveSpline.SplinePoints.Count > 0)
            {
                Vector3 middle = (moveSpline.StartPosition + moveSpline.EndPosition) / 2.0f;
                for (int i = 0; i < moveSpline.SplinePoints.Count; ++i)
                    deltas.Add(middle - moveSpline.SplinePoints[i]);
            }
        }
        return (points, deltas);
    }

    public static TheoryData<SplineFlagModern, bool, int> Shapes()
    {
        SplineFlagModern[] layouts = [SplineFlagModern.None, SplineFlagModern.UncompressedPath,
                                      SplineFlagModern.UncompressedPath | SplineFlagModern.Cyclic, SplineFlagModern.Cyclic];
        bool[] ends = [false, true];
        int[] counts = [0, 1, 5];

        var data = new TheoryData<SplineFlagModern, bool, int>();
        foreach (var flags in layouts)
            foreach (bool hasEnd in ends)
                foreach (int count in counts)
                    data.Add(flags, hasEnd, count);
        return data;
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void PointsAndDeltas_MatchTheListsTheConstructorBuilt(SplineFlagModern flags, bool hasEnd, int count)
    {
        var spline = new ServerSideMovement
        {
            SplineFlags = flags,
            StartPosition = new Vector3(100f, 200f, 300f),
            EndPosition = hasEnd ? new Vector3(113.5f, 207.25f, 299f) : Vector3.Zero,
        };
        for (int i = 0; i < count; i++)
            spline.SplinePoints.Add(new Vector3(101f + i * 1.7f, 202f - i * 0.3f, 303f + i));

        var packet = new MonsterMove(WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1), spline);
        var (points, deltas) = Reference(spline);

        Assert.Equal(points.Count, packet.PointCount);
        for (int i = 0; i < points.Count; i++)
            Assert.Equal(points[i], packet.Point(i));
        Assert.Equal(deltas.Count, packet.PackedDeltaCount);
        for (int i = 0; i < deltas.Count; i++)
            Assert.Equal(deltas[i], packet.PackedDelta(i));
    }
}
