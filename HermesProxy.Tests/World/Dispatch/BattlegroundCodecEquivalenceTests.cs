using System;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the battleground codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// <c>BattlefieldPort</c> is the one with history. Its <c>AcceptedInvite</c> bit follows a nested
/// <c>RideTicket</c>, and on V3_4_3 that ticket ends with an <c>Unknown925</c> bit plus a
/// byte-align. Miss the align and the accept bit reads the wrong bit entirely — "Enter Battle"
/// arrived as a decline and the player never entered the battleground that had popped (#102). The
/// oracle reads the ticket through the <c>WorldPacket</c> overload while the codec uses the span
/// twin, so the two genuinely cross-check rather than sharing an implementation.
/// </remarks>
public class BattlegroundCodecEquivalenceTests
{
    static BattlegroundCodecEquivalenceTests()
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

    private static bool SendsUnknown925 => ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;

    private static readonly WowGuid128 Battlemaster = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);

    // ---- the empty ones ----

    [Fact]
    public void RequestBattlefieldStatus_ConsumesNothing()
    {
        var (o, f) = Build(_ => { });
        var e = new Frozen.RequestBattlefieldStatus(); e.Read(o);
        var r = ReaderOver(f); RequestBattlefieldStatusCodec.Read(ref r, out var a);
        Assert.Equal(default, a);
        Assert.Equal(0, r.Remaining);
        Assert.NotNull(e);
    }

    [Fact]
    public void PVPLogDataRequest_ConsumesNothing()
    {
        var (o, f) = Build(_ => { });
        var e = new Frozen.PVPLogDataRequest(); e.Read(o);
        var r = ReaderOver(f); PVPLogDataRequestCodec.Read(ref r, out var a);
        Assert.Equal(default, a);
        Assert.Equal(0, r.Remaining);
        Assert.NotNull(e);
    }

    [Fact]
    public void BattlefieldLeave_ConsumesNothing()
    {
        var (o, f) = Build(_ => { });
        var e = new Frozen.BattlefieldLeave(); e.Read(o);
        var r = ReaderOver(f); BattlefieldLeaveCodec.Read(ref r, out var a);
        Assert.Equal(default, a);
        Assert.Equal(0, r.Remaining);
        Assert.NotNull(e);
    }

    // ---- single field ----

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(32)]
    [InlineData(int.MaxValue)]
    public void BattlefieldListRequest_Matches(int listId)
    {
        var (o, f) = Build(w => w.WriteInt32(listId));
        var e = new Frozen.BattlefieldListRequest(); e.Read(o);
        var r = ReaderOver(f); BattlefieldListRequestCodec.Read(ref r, out var a);
        Assert.Equal(e.ListID, a.ListID);
        Assert.Equal(listId, a.ListID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- the composed join ----

    /// <summary>
    /// The queue id carries a 0x1F10... tag in its high bits; only the low half is the list id, so
    /// a tagged value and a bare one must yield the same result.
    /// </summary>
    [Theory]
    [InlineData(0x1F10000000000002L, 2u)]
    [InlineData(0x1F1000000000000BL, 11u)]
    [InlineData(0x0000000000000007L, 7u)]
    public void BattlemasterJoin_MasksTheQueueTag(long queueId, uint expectedListId)
    {
        var (o, f) = Build(w =>
        {
            w.WriteInt64(queueId);
            w.WriteUInt8(3);              // Roles
            w.WriteInt32(-1);             // BlacklistMap[0]
            w.WriteInt32(-1);             // BlacklistMap[1]
            w.WritePackedGuid128(Battlemaster);
            w.WriteInt32(99);             // Verification
            w.WriteInt32(7);              // BattlefieldInstanceID
            w.WriteBit(true);             // JoinAsGroup
        });

        var e = new Frozen.BattlemasterJoin(); e.Read(o);
        var r = ReaderOver(f); BattlemasterJoinCodec.Read(ref r, out var a);

        Assert.Equal(e.BattlefieldListId, a.BattlefieldListId);
        Assert.Equal(expectedListId, a.BattlefieldListId);
        Assert.Equal(e.Roles, a.Roles);
        Assert.Equal(e.BattlemasterGuid, a.BattlemasterGuid);
        Assert.Equal(e.Verification, a.Verification);
        Assert.Equal(e.BattlefieldInstanceID, a.BattlefieldInstanceID);
        Assert.Equal(e.JoinAsGroup, a.JoinAsGroup);
        Assert.Equal(Battlemaster, a.BattlemasterGuid);
    }

    /// <summary>
    /// The two blacklist entries sit between the roles byte and the battlemaster GUID, so reading
    /// them wrongly shifts the GUID rather than failing. Distinct values catch a swap.
    /// </summary>
    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(-1, -1, true)]
    [InlineData(529, 30, false)]
    public void BattlemasterJoin_ReadsBothBlacklistEntriesInOrder(int map0, int map1, bool joinAsGroup)
    {
        var (o, f) = Build(w =>
        {
            w.WriteInt64(0x1F10000000000002L);
            w.WriteUInt8(3);
            w.WriteInt32(map0);
            w.WriteInt32(map1);
            w.WritePackedGuid128(Battlemaster);
            w.WriteInt32(99);
            w.WriteInt32(7);
            w.WriteBit(joinAsGroup);
        });

        var e = new Frozen.BattlemasterJoin(); e.Read(o);
        var r = ReaderOver(f); BattlemasterJoinCodec.Read(ref r, out var a);

        Assert.Equal(e.BlacklistMap[0], a.BlacklistMap[0]);
        Assert.Equal(e.BlacklistMap[1], a.BlacklistMap[1]);
        Assert.Equal(map0, a.BlacklistMap[0]);
        Assert.Equal(map1, a.BlacklistMap[1]);

        // The GUID follows the blacklist pair, so it is the field a misread there corrupts.
        Assert.Equal(Battlemaster, a.BattlemasterGuid);
        Assert.Equal(joinAsGroup, a.JoinAsGroup);
    }

    // ---- the #102 packet ----

    /// <summary>
    /// Both values of the accept bit, because reading it off the wrong bit is exactly what #102
    /// was: the decline path looked identical to a correct parse from the outside.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BattlefieldPort_Matches(bool acceptedInvite)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(WowGuid128.Empty);   // RideTicket.RequesterGuid
            w.WriteUInt32(11);                        // RideTicket.Id
            w.WriteUInt32(1);                         // RideTicket.Type = Battlegrounds
            w.WriteInt64(1700000000L);                // RideTicket.Time
            if (SendsUnknown925)
            {
                w.WriteBit(false);                    // RideTicket.Unknown925
                w.FlushBits();                        // and the byte-align after it
            }
            w.WriteBit(acceptedInvite);
            w.FlushBits();
        });

        var e = new Frozen.BattlefieldPort(); e.Read(o);
        var r = ReaderOver(f); BattlefieldPortCodec.Read(ref r, out var a);

        Assert.Equal(e.Ticket.Id, a.Ticket.Id);
        Assert.Equal(e.Ticket.Type, a.Ticket.Type);
        Assert.Equal(e.Ticket.Time, a.Ticket.Time);
        Assert.Equal(e.AcceptedInvite, a.AcceptedInvite);
        Assert.Equal(acceptedInvite, a.AcceptedInvite);
        Assert.Equal(11u, a.Ticket.Id);
        Assert.Equal(RideType.Battlegrounds, a.Ticket.Type);
    }

    /// <summary>
    /// The reader must finish exactly at the end. #102 was an alignment fault, and field values
    /// stayed plausible right up until the bit that mattered — the end position is what catches it.
    /// </summary>
    [Fact]
    public void BattlefieldPort_ConsumesExactlyTheWholePacket()
    {
        var (_, f) = Build(w =>
        {
            w.WritePackedGuid128(WowGuid128.Empty);
            w.WriteUInt32(11);
            w.WriteUInt32(1);
            w.WriteInt64(1700000000L);
            if (SendsUnknown925)
            {
                w.WriteBit(false);
                w.FlushBits();
            }
            w.WriteBit(true);
            w.FlushBits();
        });

        var r = ReaderOver(f);
        BattlefieldPortCodec.Read(ref r, out _);

        Assert.Equal(0, r.Remaining);
    }
}
