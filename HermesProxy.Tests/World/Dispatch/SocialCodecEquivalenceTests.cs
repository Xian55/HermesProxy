using System;
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
/// Equivalence for the social, reputation, duel and toy codecs against the frozen <c>Read()</c>
/// bodies they replaced.
/// </summary>
/// <remarks>
/// The string cases carry the risk here. <c>AddFriend</c> reads two lengths from one bit block
/// before either string, and <c>AddIgnore</c> reads a length, then an optional GUID, then the
/// string that length describes — so in both a misordered read corrupts the text rather than
/// throwing. Every case asserts the reader's final position as well as the fields, because a
/// length read at the wrong width still produces a plausible-looking short string.
/// </remarks>
public class SocialCodecEquivalenceTests
{
    static SocialCodecEquivalenceTests()
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

    // ---- social ----

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(uint.MaxValue)]
    public void ContactListRequest_Matches(uint flags)
    {
        var (o, f) = Build(w => w.WriteUInt32(flags));
        var e = new Frozen.ContactListRequest(); e.Read(o);
        var r = ReaderOver(f); ContactListRequestCodec.Read(ref r, out var a);
        Assert.Equal(e.Flags, a.Flags);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>Two lengths in one bit block, then both strings.</summary>
    [Theory]
    [InlineData("Pally", "healer, owes me gold")]
    [InlineData("A", "")]
    [InlineData("", "")]
    [InlineData("Xian11", "x")]
    public void AddFriend_Matches(string name, string note)
    {
        var (o, f) = Build(w =>
        {
            w.WriteBits(name.Length, 9);
            w.WriteBits(note.Length, 10);
            w.WriteString(name);
            w.WriteString(note);
        });

        var e = new Frozen.AddFriend(); e.Read(o);
        var r = ReaderOver(f); AddFriendCodec.Read(ref r, out var a);

        Assert.Equal(e.Name, a.Name);
        Assert.Equal(e.Note, a.Note);
        Assert.Equal(name, a.Name);
        Assert.Equal(note, a.Note);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// The account GUID sits between the name length and the name on builds that send it. The
    /// oracle keeps that field private, so the position assertion is what proves the GUID was
    /// consumed rather than read as string bytes.
    /// </summary>
    [Theory]
    [InlineData("Ninja")]
    [InlineData("")]
    public void AddIgnore_Matches(string name)
    {
        bool sendsAccountGuid = ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3);

        var (o, f) = Build(w =>
        {
            w.WriteBits(name.Length, 9);
            if (sendsAccountGuid)
                w.WritePackedGuid128(Guid);
            w.WriteString(name);
        });

        var e = new Frozen.AddIgnore(); e.Read(o);
        var r = ReaderOver(f); AddIgnoreCodec.Read(ref r, out var a);

        Assert.Equal(e.Name, a.Name);
        Assert.Equal(name, a.Name);
        Assert.Equal(sendsAccountGuid ? Guid : default, a.AccountGuid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void DelFriend_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(0x1234u); w.WritePackedGuid128(Guid); });
        var e = new Frozen.DelFriend(); e.Read(o);
        var r = ReaderOver(f); DelFriendCodec.Read(ref r, out var a);
        Assert.Equal(e.VirtualRealmAddress, a.VirtualRealmAddress);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>The note length is read after the GUID, not with a leading bit block.</summary>
    [Theory]
    [InlineData("tank, invite for raids")]
    [InlineData("")]
    public void SetContactNotes_Matches(string notes)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32(0x5678u);
            w.WritePackedGuid128(Guid2);
            w.WriteBits(notes.Length, 10);
            w.WriteString(notes);
        });

        var e = new Frozen.SetContactNotes(); e.Read(o);
        var r = ReaderOver(f); SetContactNotesCodec.Read(ref r, out var a);

        Assert.Equal(e.VirtualRealmAddress, a.VirtualRealmAddress);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(e.Notes, a.Notes);
        Assert.Equal(notes, a.Notes);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- reputation ----

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)7)]
    [InlineData((byte)255)]
    public void SetFactionAtWar_Matches(byte factionIndex)
    {
        var (o, f) = Build(w => w.WriteUInt8(factionIndex));
        var e = new Frozen.SetFactionAtWar(); e.Read(o);
        var r = ReaderOver(f); SetFactionAtWarCodec.Read(ref r, out var a);
        Assert.Equal(e.FactionIndex, a.FactionIndex);
        Assert.Equal(factionIndex, a.FactionIndex);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)255)]
    public void SetFactionNotAtWar_Matches(byte factionIndex)
    {
        var (o, f) = Build(w => w.WriteUInt8(factionIndex));
        var e = new Frozen.SetFactionNotAtWar(); e.Read(o);
        var r = ReaderOver(f); SetFactionNotAtWarCodec.Read(ref r, out var a);
        Assert.Equal(e.FactionIndex, a.FactionIndex);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>A uint32 followed by a single bit — the bit reader starts after a byte boundary.</summary>
    [Theory]
    [InlineData(0u, true)]
    [InlineData(0u, false)]
    [InlineData(uint.MaxValue, true)]
    public void SetFactionInactive_Matches(uint factionIndex, bool state)
    {
        var (o, f) = Build(w => { w.WriteUInt32(factionIndex); w.WriteBit(state); });
        var e = new Frozen.SetFactionInactive(); e.Read(o);
        var r = ReaderOver(f); SetFactionInactiveCodec.Read(ref r, out var a);
        Assert.Equal(e.FactionIndex, a.FactionIndex);
        Assert.Equal(e.State, a.State);
        Assert.Equal(state, a.State);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1037u)]
    public void SetWatchedFaction_Matches(uint factionIndex)
    {
        var (o, f) = Build(w => w.WriteUInt32(factionIndex));
        var e = new Frozen.SetWatchedFaction(); e.Read(o);
        var r = ReaderOver(f); SetWatchedFactionCodec.Read(ref r, out var a);
        Assert.Equal(e.FactionIndex, a.FactionIndex);
        Assert.Equal(factionIndex, a.FactionIndex);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- duel ----

    [Fact]
    public void CanDuel_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.CanDuel(); e.Read(o);
        var r = ReaderOver(f); CanDuelCodec.Read(ref r, out var a);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>Two consecutive bits — swapping them is silent, so both combinations are checked.</summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void DuelResponse_Matches(bool accepted, bool forfeited)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteBit(accepted);
            w.WriteBit(forfeited);
        });

        var e = new Frozen.DuelResponse(); e.Read(o);
        var r = ReaderOver(f); DuelResponseCodec.Read(ref r, out var a);

        Assert.Equal(e.ArbiterGUID, a.ArbiterGUID);
        Assert.Equal(e.Accepted, a.Accepted);
        Assert.Equal(e.Forfeited, a.Forfeited);
        Assert.Equal(accepted, a.Accepted);
        Assert.Equal(forfeited, a.Forfeited);
    }

    // ---- toys ----

    [Theory]
    [InlineData(0u)]
    [InlineData(1973u)]
    public void ToyClearFanfare_Matches(uint itemId)
    {
        var (o, f) = Build(w => w.WriteUInt32(itemId));
        var e = new Frozen.ToyClearFanfare(); e.Read(o);
        var r = ReaderOver(f); ToyClearFanfareCodec.Read(ref r, out var a);
        Assert.Equal(e.ItemID, a.ItemID);
        Assert.Equal(itemId, a.ItemID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void AddToy_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.AddToy(); e.Read(o);
        var r = ReaderOver(f); AddToyCodec.Read(ref r, out var a);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// The old class defaulted <c>Guid</c> to <c>WowGuid128.Empty</c>, which is what a positional
    /// record struct defaults to anyway — but an all-zero GUID is also a legitimate wire value, so
    /// this pins that the two agree rather than assuming it.
    /// </summary>
    [Fact]
    public void AddToy_ZeroGuid_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(default));
        var e = new Frozen.AddToy(); e.Read(o);
        var r = ReaderOver(f); AddToyCodec.Read(ref r, out var a);
        Assert.Equal(e.Guid, a.Guid);
        Assert.True(a.Guid.IsEmpty());
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>The collection type is int32 on the wire, not the byte its enum suggests.</summary>
    [Theory]
    [InlineData(ItemCollectionType.Toy, 1973u, true)]
    [InlineData(ItemCollectionType.Toy, 1973u, false)]
    [InlineData(ItemCollectionType.Heirloom, 0u, false)]
    [InlineData(ItemCollectionType.Transmog, uint.MaxValue, true)]
    public void CollectionItemSetFavorite_Matches(ItemCollectionType type, uint id, bool isFavorite)
    {
        var (o, f) = Build(w =>
        {
            w.WriteInt32((int)type);
            w.WriteUInt32(id);
            w.WriteBit(isFavorite);
        });

        var e = new Frozen.CollectionItemSetFavorite(); e.Read(o);
        var r = ReaderOver(f); CollectionItemSetFavoriteCodec.Read(ref r, out var a);

        Assert.Equal(e.Type, a.Type);
        Assert.Equal(e.ID, a.ID);
        Assert.Equal(e.IsFavorite, a.IsFavorite);
        Assert.Equal(type, a.Type);
        Assert.Equal(id, a.ID);
        Assert.Equal(isFavorite, a.IsFavorite);
    }
}
