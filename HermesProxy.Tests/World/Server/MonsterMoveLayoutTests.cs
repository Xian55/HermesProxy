using System;
using System.Buffers;
using System.Collections.Generic;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// 4.4.2 dropped the Destination vector of SMSG_ON_MONSTER_MOVE. The older layout keeps its 12 zero
/// bytes after the spline id; the 4.4.2 one gives the bytes of a native TrinityCore cata_classic
/// packet, from both serialisers.
/// </summary>
public sealed class MonsterMoveLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<MonsterMove.SplineLayout>(MonsterMove.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.NotSame(MonsterMove.Layouts.For(ClientVersionBuild.V3_4_3_54261), MonsterMove.Layouts.For(ClientVersionBuild.V4_4_2_60895));
        Assert.Same(MonsterMove.Layouts.For(ClientVersionBuild.V2_5_3_41750), MonsterMove.Layouts.For(ClientVersionBuild.V3_4_3_54261));
    }

    [Fact]
    public void CataClassicLayout_MatchesANativePacket()
    {
        var spline = new ServerSideMovement
        {
            SplineId = 4,
            SplineFlags = (SplineFlagModern)0x11000000,     // FastSteering | Steering
            SplineTimeFull = 3389,
            StartPosition = new Vector3(-9000.26f, -133.851f, 84.0311f),
            EndPosition = new Vector3(-8996.252f, -141.25f, 83.0605f),
            TransportGuid = WowGuid128.Empty,
            TransportSeat = -1,
            SplineCount = 1,
        };
        var packet = new MonsterMove(new WowGuid128(230, 0x200004000030B3C0), spline);

        AssertBothSerialisers(MonsterMove.Layouts.For(ClientVersionBuild.V4_4_2_60895), packet, Convert.FromHexString(NativeMove));
    }

    [Fact]
    public void DestinationLayout_IsTheCataBytesWithAZeroDestination()
    {
        var spline = new ServerSideMovement
        {
            SplineId = 9,
            SplineType = SplineTypeModern.FacingTarget,
            SplineFlags = SplineFlagModern.UncompressedPath,
            SplineTimeFull = 1200,
            StartPosition = new Vector3(100f, 200f, 300f),
            EndPosition = new Vector3(110f, 210f, 310f),
            FinalOrientation = 1.5f,
            FinalFacingGuid = new WowGuid128(4023, 0x0800040000000000),
            TransportGuid = WowGuid128.Empty,
            SplinePoints = new List<Vector3> { new(101f, 201f, 301f), new(102f, 202f, 302f) },
        };
        var guid = new WowGuid128(230, 0x200004000030B3C0);
        var packet = new MonsterMove(guid, spline);

        byte[] cata = Write(MonsterMove.Layouts.For(ClientVersionBuild.V4_4_2_60895), packet);
        int afterId = 2 + 1 + 5 + 12 + 4;                // GUID masks and bytes, StartPosition, SplineId
        byte[] expected = [.. cata.AsSpan(0, afterId), .. new byte[12], .. cata.AsSpan(afterId)];

        AssertBothSerialisers(MonsterMove.Layouts.For(ClientVersionBuild.V3_4_3_54261), packet, expected);
    }

    // SMSG_ON_MONSTER_MOVE from TrinityCore cata_classic to a 4.4.2.60895 client
    // (refs/native-captures/tc_cata_442_20261010-141101.pkt, packet 266).
    private const string NativeMove =
        "01A7E6C0B33004200AA10CC6DBD905C3EC0FA842040000000000000011000000003D0D000000000000000000FF000040000002910CC600400DC3FA1EA642";

    private static byte[] Write(ServerPacketLayout<MonsterMove> layout, MonsterMove packet)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        return data.GetDataSpan().ToArray();
    }

    private static void AssertBothSerialisers(ServerPacketLayout<MonsterMove> layout, MonsterMove packet, byte[] expected)
    {
        Assert.Equal(expected, Write(layout, packet));

        byte[] buffer = ArrayPool<byte>.Shared.Rent(packet.MaxSize);
        try
        {
            int written = layout.WriteToSpan(packet, buffer);
            Assert.Equal(expected, buffer.AsSpan(0, written).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
