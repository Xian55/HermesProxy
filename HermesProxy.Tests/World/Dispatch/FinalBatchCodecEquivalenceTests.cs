using System;
using System.Collections.Generic;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the last codecs off the reflection table against the frozen <c>Read()</c>
/// bodies they replaced.
/// </summary>
/// <remarks>
/// Mostly flat reads. The two that are not are pinned hardest: the who request reads a 4-bit area
/// count first and its areas last with a whole nested block in between, and both hotfix packets
/// take a wire count that has to be clamped before it is used to pre-size anything.
/// </remarks>
public class FinalBatchCodecEquivalenceTests
{
    static FinalBatchCodecEquivalenceTests()
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

    // ---- talents ----

    /// <summary>Rank is uint16, not uint32 — a width slip here would eat the next field.</summary>
    [Theory]
    [InlineData(0u, (ushort)0)]
    [InlineData(2367u, (ushort)3)]
    [InlineData(uint.MaxValue, ushort.MaxValue)]
    public void LearnPetTalent_Matches(uint talentId, ushort rank)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(talentId);
            w.WriteUInt16(rank);
        });

        var e = new Frozen.LearnPetTalent(); e.Read(o);
        var r = ReaderOver(f); LearnPetTalentCodec.Read(ref r, out var a);

        Assert.Equal(e.PetGUID, a.PetGUID);
        Assert.Equal(e.TalentID, a.TalentID);
        Assert.Equal(e.Rank, a.Rank);
        Assert.Equal(talentId, a.TalentID);
        Assert.Equal(rank, a.Rank);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)5)]
    public void RemoveGlyph_Matches(byte slot)
    {
        var (o, f) = Build(w => w.WriteUInt8(slot));

        var e = new Frozen.RemoveGlyph(); e.Read(o);
        var r = ReaderOver(f); RemoveGlyphCodec.Read(ref r, out var a);

        Assert.Equal(e.GlyphSlot, a.GlyphSlot);
        Assert.Equal(slot, a.GlyphSlot);
    }

    // ---- game objects ----

    [Fact]
    public void GameObjUse_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));

        var e = new Frozen.GameObjUse(); e.Read(o);
        var r = ReaderOver(f); GameObjUseCodec.Read(ref r, out var a);

        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(Guid, a.Guid);
    }

    [Fact]
    public void GameObjReportUse_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));

        var e = new Frozen.GameObjReportUse(); e.Read(o);
        var r = ReaderOver(f); GameObjReportUseCodec.Read(ref r, out var a);

        Assert.Equal(e.Guid, a.Guid);
    }

    // ---- hotfix ----

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(40)]
    public void DBQueryBulk_Matches(int count)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32((uint)DB2Hash.BroadcastText);
            w.WriteBits((uint)count, 13);
            for (int i = 0; i < count; i++)
                w.WriteUInt32((uint)(1000 + i));
        });

        var e = new Frozen.DBQueryBulk(); e.Read(o);
        var r = ReaderOver(f); DBQueryBulkCodec.Read(ref r, out var a);

        Assert.Equal(e.TableHash, a.TableHash);
        Assert.Equal(e.Queries, a.Queries);
        Assert.Equal(count, a.Queries.Count);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(3u)]
    public void HotfixRequest_Matches(uint count)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32(54261);
            w.WriteUInt32(12345);
            w.WriteUInt32(count);
            for (uint i = 0; i < count; i++)
                w.WriteUInt32(2100000 + i);
        });

        var e = new Frozen.HotfixRequest(); e.Read(o);
        var r = ReaderOver(f); HotfixRequestCodec.Read(ref r, out var a);

        Assert.Equal(e.ClientBuild, a.ClientBuild);
        Assert.Equal(e.DataBuild, a.DataBuild);
        Assert.Equal(e.Hotfixes, a.Hotfixes);
        Assert.Equal(54261u, a.ClientBuild);
    }

    /// <summary>
    /// A uint32 count with nothing behind it must not be used to pre-size a list. Without the
    /// clamp this reserves gigabytes before the first read fails.
    /// </summary>
    [Fact]
    public void HotfixRequest_DoesNotPreSizeFromAnAbsurdCount()
    {
        var (_, f) = Build(w =>
        {
            w.WriteUInt32(54261);
            w.WriteUInt32(12345);
            w.WriteUInt32(int.MaxValue);
        });

        Assert.ThrowsAny<Exception>(() =>
        {
            var rr = ReaderOver(f);
            HotfixRequestCodec.Read(ref rr, out _);
        });
    }

    // ---- who ----

    /// <summary>
    /// The area count leads and the areas trail, with the whole WhoRequest block and the request
    /// id between them. Distinct area values, so a misread order cannot pass.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void WhoRequestPkt_KeepsTheAreasAfterTheRequest(int areaCount)
    {
        const string name = "Xian";
        const string guild = "GuildName";

        var (o, f) = Build(w =>
        {
            w.WriteBits((uint)areaCount, 4);
            w.FlushBits();

            w.WriteInt32(10);
            w.WriteInt32(80);
            w.WriteInt64(0x7F);
            w.WriteInt32(-1);
            w.WriteBits((uint)name.GetByteCount(), 6);
            w.WriteBits(0u, 9);
            w.WriteBits((uint)guild.GetByteCount(), 7);
            w.WriteBits(0u, 9);
            w.WriteBits(0u, 3);
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(true);
            w.WriteBit(false);
            w.FlushBits();
            w.WriteString(name);
            w.WriteString("");
            w.WriteString(guild);
            w.WriteString("");

            w.WriteUInt32(77);
            for (int i = 0; i < areaCount; i++)
                w.WriteInt32(1400 + i);
        });

        var e = new Frozen.WhoRequestPkt(); e.Read(o);
        var r = ReaderOver(f); WhoRequestPktCodec.Read(ref r, out var a);

        Assert.Equal(e.RequestID, a.RequestID);
        Assert.Equal(e.Areas, a.Areas);
        Assert.Equal(e.Request.Name, a.Request.Name);
        Assert.Equal(e.Request.Guild, a.Request.Guild);
        Assert.Equal(e.Request.MinLevel, a.Request.MinLevel);
        Assert.Equal(e.Request.MaxLevel, a.Request.MaxLevel);
        Assert.Equal(e.Request.ClassFilter, a.Request.ClassFilter);
        Assert.Equal(e.Request.ExactName, a.Request.ExactName);

        Assert.Equal(77u, a.RequestID);
        Assert.Equal(areaCount, a.Areas.Count);
        Assert.Equal(name, a.Request.Name);
        Assert.Equal(guild, a.Request.Guild);
        // -1 is "any class"; a zero here would mean Warrior.
        Assert.Equal(-1, a.Request.ClassFilter);
    }

    // ---- misc / instance ----

    [Theory]
    [InlineData(0u)]
    [InlineData(2u)]
    public void MountSpecial_Matches(uint count)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32(count);
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                w.WriteInt32(4);
            for (uint i = 0; i < count; i++)
                w.WriteInt32((int)(500 + i));
        });

        var e = new Frozen.MountSpecial(); e.Read(o);
        var r = ReaderOver(f); MountSpecialCodec.Read(ref r, out var a);

        Assert.Equal(e.SpellVisualKitIDs, a.SpellVisualKitIDs);
        Assert.Equal(e.SequenceVariation, a.SequenceVariation);
    }

    /// <summary>A bit, not a byte. Reading it as a byte would consume seven bits too many.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InstanceLockResponse_ReadsABit(bool accept)
    {
        var (o, f) = Build(w => w.WriteBit(accept));

        var e = new Frozen.InstanceLockResponse(); e.Read(o);
        var r = ReaderOver(f); InstanceLockResponseCodec.Read(ref r, out var a);

        Assert.Equal(e.AcceptLock, a.AcceptLock);
        Assert.Equal(accept, a.AcceptLock);
    }

    // ---- session / bnet ----

    /// <summary>The 32-byte secret is copied — it is handed to the RPC and outlives the rental.</summary>
    [Fact]
    public void ChangeRealmTicket_CopiesTheSecret()
    {
        byte[] secret = new byte[32];
        for (int i = 0; i < secret.Length; i++)
            secret[i] = (byte)(i + 1);

        var (o, f) = Build(w =>
        {
            w.WriteUInt32(0xABCDEF01);
            w.WriteBytes(secret);
        });

        var e = new Frozen.ChangeRealmTicket(); e.Read(o);
        var r = ReaderOver(f); ChangeRealmTicketCodec.Read(ref r, out var a);

        Assert.Equal(e.Token, a.Token);
        Assert.Equal(e.Secret, a.Secret);
        Assert.Equal(0xABCDEF01u, a.Token);

        Array.Clear(f);
        Assert.Equal(secret, a.Secret);
    }

    [Fact]
    public void BattlenetRequest_Matches()
    {
        byte[] proto = [10, 20, 30, 40, 50];
        var (o, f) = Build(w =>
        {
            w.WriteUInt64(0x1122334455667788UL);
            w.WriteUInt64(0x99AABBCCDDEEFF00UL);
            w.WriteUInt32(4242);
            w.WriteUInt32((uint)proto.Length);
            w.WriteBytes(proto);
        });

        var e = new Frozen.BattlenetRequest(); e.Read(o);
        var r = ReaderOver(f); BattlenetRequestCodec.Read(ref r, out var a);

        Assert.Equal(e.Method.Type, a.Method.Type);
        Assert.Equal(e.Method.ObjectId, a.Method.ObjectId);
        Assert.Equal(e.Method.Token, a.Method.Token);
        Assert.Equal(e.Data, a.Data);
        Assert.Equal(4242u, a.Method.Token);
        Assert.Equal(proto, a.Data);

        Array.Clear(f);
        Assert.Equal(proto, a.Data);
    }
}
