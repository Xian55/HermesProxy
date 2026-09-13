using System;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the Dungeon Finder codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// <c>DFJoin</c> carries the risk: three bits, a byte, a slot count, an <i>optional</i> party-index
/// byte, and only then the slot array the count sized. The party-index byte sits between the count
/// and its array, so a reader that takes it late shifts every dungeon id — and dungeon ids are
/// opaque numbers, so the result is a queue for the wrong instance rather than a parse failure.
/// Both the present and absent forms are exercised.
/// </remarks>
public class LfgCodecEquivalenceTests
{
    static LfgCodecEquivalenceTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static (WorldPacket Oracle, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    // ---- the empty ones ----

    [Fact]
    public void DFGetJoinStatus_ConsumesNothing()
    {
        var (o, f) = Build(_ => { });
        var e = new Frozen.DFGetJoinStatusPkt(); e.Read(o);
        var r = ReaderOver(f); DFGetJoinStatusPktCodec.Read(ref r, out var a);
        Assert.Equal(default, a);
        Assert.Equal(0, r.Remaining);
        Assert.NotNull(e);
    }

    [Fact]
    public void DFLeave_ConsumesNothing()
    {
        var (o, f) = Build(_ => { });
        var e = new Frozen.DFLeavePkt(); e.Read(o);
        var r = ReaderOver(f); DFLeavePktCodec.Read(ref r, out var a);
        Assert.Equal(default, a);
        Assert.Equal(0, r.Remaining);
        Assert.NotNull(e);
    }

    [Fact]
    public void LFGListGetStatus_ConsumesNothing()
    {
        var (o, f) = Build(_ => { });
        var e = new Frozen.LFGListGetStatusPkt(); e.Read(o);
        var r = ReaderOver(f); LFGListGetStatusPktCodec.Read(ref r, out var a);
        Assert.Equal(default, a);
        Assert.Equal(0, r.Remaining);
        Assert.NotNull(e);
    }

    // ---- single bit / single byte ----

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DFGetSystemInfo_Matches(bool player)
    {
        var (o, f) = Build(w => w.WriteBit(player));
        var e = new Frozen.DFGetSystemInfoPkt(); e.Read(o);
        var r = ReaderOver(f); DFGetSystemInfoPktCodec.Read(ref r, out var a);
        Assert.Equal(e.Player, a.Player);
        Assert.Equal(player, a.Player);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DFTeleport_Matches(bool teleportOut)
    {
        var (o, f) = Build(w => w.WriteBit(teleportOut));
        var e = new Frozen.DFTeleportPkt(); e.Read(o);
        var r = ReaderOver(f); DFTeleportPktCodec.Read(ref r, out var a);
        Assert.Equal(e.TeleportOut, a.TeleportOut);
        Assert.Equal(teleportOut, a.TeleportOut);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)0x02)]
    [InlineData((byte)0x0E)]
    [InlineData((byte)255)]
    public void DFSetRoles_Matches(byte roles)
    {
        var (o, f) = Build(w => w.WriteUInt8(roles));
        var e = new Frozen.DFSetRolesPkt(); e.Read(o);
        var r = ReaderOver(f); DFSetRolesPktCodec.Read(ref r, out var a);
        Assert.Equal(e.Roles, a.Roles);
        Assert.Equal(roles, a.Roles);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- the composed one ----

    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(false, false, 1)]
    [InlineData(false, false, 4)]
    [InlineData(true, false, 2)]
    [InlineData(false, true, 2)]
    [InlineData(true, true, 3)]
    public void DFJoin_Matches(bool queueAsGroup, bool hasPartyIndex, int slotCount)
    {
        uint[] wanted = new uint[slotCount];
        for (int i = 0; i < slotCount; i++)
            wanted[i] = 250u + (uint)i * 7u;

        var (o, f) = Build(w =>
        {
            w.WriteBit(queueAsGroup);
            w.WriteBit(hasPartyIndex);
            w.WriteBit(false);              // Mercenary
            w.WriteUInt8(0x0E);             // Roles
            w.WriteUInt32((uint)slotCount);
            if (hasPartyIndex)
                w.WriteUInt8(3);
            foreach (uint slot in wanted)
                w.WriteUInt32(slot);
        });

        var e = new Frozen.DFJoinPkt(); e.Read(o);
        var r = ReaderOver(f); DFJoinPktCodec.Read(ref r, out var a);

        Assert.Equal(e.QueueAsGroup, a.QueueAsGroup);
        Assert.Equal(e.Roles, a.Roles);
        Assert.Equal(e.Slots, a.Slots);
        Assert.Equal(queueAsGroup, a.QueueAsGroup);
        Assert.Equal((byte)0x0E, a.Roles);
        Assert.Equal(wanted, a.Slots);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// The slot count is wire data. Sizing the array from it directly would reserve gigabytes
    /// before the element loop could fail, so the codec guards — the same treatment QuestPOIQuery
    /// gets, and for the same reason: the array is the packet field, so clamping it would silently
    /// drop dungeons instead of reporting a bad packet.
    /// </summary>
    [Theory]
    [InlineData(0x7FFFFFFFu)]
    [InlineData(0xFFFFFFFFu)]
    [InlineData(0x01000000u)]
    public void DFJoin_AbsurdSlotCount_ThrowsArgumentOutOfRange(uint slotCount)
    {
        var (_, f) = Build(w =>
        {
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteUInt8(0);
            w.WriteUInt32(slotCount);
        });

        ArgumentOutOfRangeException? caught = null;
        try
        {
            var r = ReaderOver(f);
            DFJoinPktCodec.Read(ref r, out _);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            caught = ex;
        }

        Assert.NotNull(caught);
        Assert.Contains("dungeon slots", caught.Message);
    }

    /// <summary>A count one past what the buffer holds is the boundary the guard defends.</summary>
    [Fact]
    public void DFJoin_SlotCountOneTooLarge_Throws()
    {
        var (_, f) = Build(w =>
        {
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteUInt8(0);
            w.WriteUInt32(3);
            w.WriteUInt32(250);
            w.WriteUInt32(251);   // only two slots behind a count of three
        });

        ArgumentOutOfRangeException? caught = null;
        try
        {
            var r = ReaderOver(f);
            DFJoinPktCodec.Read(ref r, out _);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            caught = ex;
        }

        Assert.NotNull(caught);
    }
}
