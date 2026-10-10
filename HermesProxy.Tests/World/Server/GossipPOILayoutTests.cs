using System;
using System.Buffers;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// <see cref="GossipPOI"/> moved from one class with a 3.4.3 branch in each serialiser to one
/// layout type per shape. Both serialisers of both layouts must give the bytes the old branches did.
/// </summary>
public class GossipPOILayoutTests
{
    public static TheoryData<string> Names() => ["", "Stormwind Bank", "Ткацкая мастерская", new string('x', 63)];

    [Theory]
    [MemberData(nameof(Names))]
    public void RetailLayout_MatchesTheOldRetailBranch(string name)
    {
        var packet = Sample(name);
        AssertBothSerialisers(new GossipPOI.RetailLayout(), packet, OldRetail(packet));
    }

    [Theory]
    [MemberData(nameof(Names))]
    public void FlatLayout_MatchesTheOld3_4_3Branch(string name)
    {
        var packet = Sample(name);
        AssertBothSerialisers(new GossipPOI.FlatLayout(), packet, OldFlat(packet));
    }

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<GossipPOI.RetailLayout>(GossipPOI.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<GossipPOI.RetailLayout>(GossipPOI.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<GossipPOI.FlatLayout>(GossipPOI.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<GossipPOI.FlatLayout>(GossipPOI.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    private static GossipPOI Sample(string name) => new()
    {
        Id = 7,
        Flags = 0x2A5,
        Pos = new(-8823.5f, 631.25f, 94.0f),
        Icon = 7,
        Importance = 6,
        Unknown905 = 3,
        Name = name,
    };

    private static void AssertBothSerialisers(ServerPacketLayout<GossipPOI> layout, GossipPOI packet, byte[] expected)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        byte[] buffer = ArrayPool<byte>.Shared.Rent(layout.MaxSize);
        try
        {
            int written = layout.WriteToSpan(packet, buffer);
            Assert.Equal(expected, buffer.AsSpan(0, written).ToArray());
            Assert.True(written <= layout.MaxSize);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    // GossipPOI.Write as it stood before the layouts, frozen: the branch for builds other than 3.4.3.
    private static byte[] OldRetail(GossipPOI p)
    {
        using var w = new WorldPacket(1u);
        w.WriteUInt32(p.Id);
        w.WriteFloat(p.Pos.X);
        w.WriteFloat(p.Pos.Y);
        w.WriteFloat(p.Pos.Z);
        w.WriteUInt32(p.Icon);
        w.WriteUInt32(p.Importance);
        w.WriteUInt32(p.Unknown905);
        w.WriteBits(p.Flags, 14);
        w.WriteBits(p.Name.GetByteCount(), 6);
        w.FlushBits();
        w.WriteString(p.Name);
        return w.GetDataSpan().ToArray();
    }

    // ... and its V3_4_3_54261 branch.
    private static byte[] OldFlat(GossipPOI p)
    {
        using var w = new WorldPacket(1u);
        w.WriteUInt32(p.Id);
        w.WriteUInt32(p.Flags);
        w.WriteFloat(p.Pos.X);
        w.WriteFloat(p.Pos.Y);
        w.WriteFloat(p.Pos.Z);
        w.WriteUInt32(p.Icon);
        w.WriteUInt32(p.Importance);
        w.WriteUInt32(p.Unknown905);
        w.WriteBits(p.Name.GetByteCount(), 6);
        w.FlushBits();
        w.WriteString(p.Name);
        return w.GetDataSpan().ToArray();
    }
}
