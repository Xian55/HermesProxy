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
/// Equivalence for the arena codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// These reads are flat, so the risk is transposition rather than misalignment: two adjacent bytes
/// in the skirmish join, two adjacent bits after them, and two packed GUIDs back to back in the
/// invite response. Each of those pairs is asserted with distinct values, because a swap produces a
/// perfectly well-formed packet that simply means something else.
/// </remarks>
public class ArenaCodecEquivalenceTests
{
    static ArenaCodecEquivalenceTests()
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

    private static readonly WowGuid128 Guid = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);
    private static readonly WowGuid128 Guid2 = new(0x1122334455667788UL, 0x99AABBCCDDEEFF00UL);

    // ---- single uint32 ----

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(2u)]
    public void ArenaTeamRosterRequest_Matches(uint teamIndex)
    {
        var (o, f) = Build(w => w.WriteUInt32(teamIndex));
        var e = new Frozen.ArenaTeamRosterRequest(); e.Read(o);
        var r = ReaderOver(f); ArenaTeamRosterRequestCodec.Read(ref r, out var a);
        Assert.Equal(e.TeamIndex, a.TeamIndex);
        Assert.Equal(teamIndex, a.TeamIndex);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(4242u)]
    [InlineData(uint.MaxValue)]
    public void ArenaTeamQuery_Matches(uint teamId)
    {
        var (o, f) = Build(w => w.WriteUInt32(teamId));
        var e = new Frozen.ArenaTeamQuery(); e.Read(o);
        var r = ReaderOver(f); ArenaTeamQueryCodec.Read(ref r, out var a);
        Assert.Equal(e.TeamId, a.TeamId);
        Assert.Equal(teamId, a.TeamId);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(77u)]
    public void ArenaTeamLeave_Matches(uint teamId)
    {
        var (o, f) = Build(w => w.WriteUInt32(teamId));
        var e = new Frozen.ArenaTeamLeave(); e.Read(o);
        var r = ReaderOver(f); ArenaTeamLeaveCodec.Read(ref r, out var a);
        Assert.Equal(e.TeamId, a.TeamId);
        Assert.Equal(teamId, a.TeamId);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- id + guid ----

    [Fact]
    public void ArenaTeamRemove_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(9u); w.WritePackedGuid128(Guid); });
        var e = new Frozen.ArenaTeamRemove(); e.Read(o);
        var r = ReaderOver(f); ArenaTeamRemoveCodec.Read(ref r, out var a);
        Assert.Equal(e.TeamId, a.TeamId);
        Assert.Equal(e.PlayerGuid, a.PlayerGuid);
        Assert.Equal(9u, a.TeamId);
        Assert.Equal(Guid, a.PlayerGuid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>Two packed GUIDs in a row — distinct values, so a swap cannot pass.</summary>
    [Fact]
    public void ArenaTeamAccept_KeepsTheTwoGuidsInOrder()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WritePackedGuid128(Guid2); });
        var e = new Frozen.ArenaTeamAccept(); e.Read(o);
        var r = ReaderOver(f); ArenaTeamAcceptCodec.Read(ref r, out var a);
        Assert.Equal(e.PlayerGuid, a.PlayerGuid);
        Assert.Equal(e.TeamGuid, a.TeamGuid);
        Assert.Equal(Guid, a.PlayerGuid);
        Assert.Equal(Guid2, a.TeamGuid);
        Assert.NotEqual(a.PlayerGuid, a.TeamGuid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- the joins ----

    [Theory]
    [InlineData((byte)0, (byte)0)]
    [InlineData((byte)1, (byte)2)]
    [InlineData((byte)2, (byte)8)]
    public void BattlemasterJoinArena_Matches(byte teamIndex, byte roles)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(teamIndex);
            w.WriteUInt8(roles);
        });

        var e = new Frozen.BattlemasterJoinArena(); e.Read(o);
        var r = ReaderOver(f); BattlemasterJoinArenaCodec.Read(ref r, out var a);

        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(e.TeamIndex, a.TeamIndex);
        Assert.Equal(e.Roles, a.Roles);
        Assert.Equal(teamIndex, a.TeamIndex);
        Assert.Equal(roles, a.Roles);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// Roles then TeamSize, then AsGroup then Requeue. Both pairs are transposable without
    /// failing, and the system forwards TeamSize as the arena bracket — so a swap there queues the
    /// wrong bracket silently.
    /// </summary>
    [Theory]
    [InlineData((byte)0, (byte)2, false, false)]
    [InlineData((byte)8, (byte)3, true, false)]
    [InlineData((byte)2, (byte)5, false, true)]
    [InlineData((byte)1, (byte)2, true, true)]
    public void BattlemasterJoinSkirmish_Matches(byte roles, byte teamSize, bool asGroup, bool requeue)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(roles);
            w.WriteUInt8(teamSize);
            w.WriteBit(asGroup);
            w.WriteBit(requeue);
        });

        var e = new Frozen.BattlemasterJoinSkirmish(); e.Read(o);
        var r = ReaderOver(f); BattlemasterJoinSkirmishCodec.Read(ref r, out var a);

        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(e.Roles, a.Roles);
        Assert.Equal(e.TeamSize, a.TeamSize);
        Assert.Equal(e.AsGroup, a.AsGroup);
        Assert.Equal(e.Requeue, a.Requeue);
        Assert.Equal(roles, a.Roles);
        Assert.Equal(teamSize, a.TeamSize);
        Assert.Equal(asGroup, a.AsGroup);
        Assert.Equal(requeue, a.Requeue);
    }
}
