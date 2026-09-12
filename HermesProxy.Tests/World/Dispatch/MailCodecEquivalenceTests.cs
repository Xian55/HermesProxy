using System;
using System.Linq;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the mail and petition codecs against the frozen <c>Read()</c> bodies they
/// replaced.
/// </summary>
/// <remarks>
/// <para>
/// Six mail packets became ranged codec pairs, because their mail id widened from uint32 to int64
/// in V3_4_3. The frozen oracle still carries the original
/// <c>ModernVersion.Build == V3_4_3_54261 ? … : …</c> branch, so each pair is checked against the
/// oracle arm it replaces: the WotLK codec against the oracle under a V3_4_3 bootstrap, the
/// pre-WotLK codec against it under a V1_14 one. Asserting only one arm would leave the other
/// completely unproven, and the wrong-width read is a silent misparse of everything after it.
/// </para>
/// <para>
/// <c>ModernVersion.Build</c> is <see langword="static readonly"/> and cannot be reassigned per
/// test, so the oracle arm is selected by writing the wire in the width under test and calling the
/// matching codec — the oracle is read only in whichever width the ambient bootstrap gives, and the
/// tests that need the other width compare the codec against hand-specified expected values plus
/// the reader's end position.
/// </para>
/// </remarks>
public class MailCodecEquivalenceTests
{
    static MailCodecEquivalenceTests()
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

    /// <summary>An id that does not survive a 32-bit read, so a width mix-up cannot pass.</summary>
    private const long WideMailId = 0x0000_0007_ABCD_1234L;

    // ---- mail: flat ----

    [Fact]
    public void MailGetList_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.MailGetList(); e.Read(o);
        var r = ReaderOver(f); MailGetListCodec.Read(ref r, out var a);
        Assert.Equal(e.Mailbox, a.Mailbox);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- mail: the ranged pairs, 64-bit arm ----

    [Fact]
    public void MailCreateTextItem_WotLKClassic_ReadsSixtyFourBitId()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteInt64(WideMailId); });
        var r = ReaderOver(f); MailCreateTextItemCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.Mailbox);
        Assert.Equal(WideMailId, a.MailID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailCreateTextItem_PreWotLKClassic_ReadsThirtyTwoBitId()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(0xABCD1234u); });
        var r = ReaderOver(f); MailCreateTextItemCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.Mailbox);
        Assert.Equal(0xABCD1234L, a.MailID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailMarkAsRead_WotLKClassic_ReadsSixtyFourBitId()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteInt64(WideMailId); });
        var r = ReaderOver(f); MailMarkAsReadCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.Mailbox);
        Assert.Equal(WideMailId, a.MailID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailMarkAsRead_PreWotLKClassic_ReadsThirtyTwoBitId()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(7u); });
        var r = ReaderOver(f); MailMarkAsReadCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.Mailbox);
        Assert.Equal(7L, a.MailID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailDelete_WotLKClassic_ReadsSixtyFourBitId()
    {
        var (_, f) = Build(w => { w.WriteInt64(WideMailId); w.WriteInt32(-3); });
        var r = ReaderOver(f); MailDeleteCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(WideMailId, a.MailID);
        Assert.Equal(-3, a.DeleteReason);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailDelete_PreWotLKClassic_ReadsThirtyTwoBitId()
    {
        var (_, f) = Build(w => { w.WriteUInt32(42u); w.WriteInt32(-3); });
        var r = ReaderOver(f); MailDeleteCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(42L, a.MailID);
        Assert.Equal(-3, a.DeleteReason);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailReturnToSender_WotLKClassic_ReadsSixtyFourBitId()
    {
        var (_, f) = Build(w => { w.WriteInt64(WideMailId); w.WritePackedGuid128(Guid2); });
        var r = ReaderOver(f); MailReturnToSenderCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(WideMailId, a.MailID);
        Assert.Equal(Guid2, a.SenderGUID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailReturnToSender_PreWotLKClassic_ReadsThirtyTwoBitId()
    {
        var (_, f) = Build(w => { w.WriteUInt32(9u); w.WritePackedGuid128(Guid2); });
        var r = ReaderOver(f); MailReturnToSenderCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(9L, a.MailID);
        Assert.Equal(Guid2, a.SenderGUID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailTakeMoney_WotLKClassic_ReadsSixtyFourBitId()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteInt64(WideMailId); w.WriteInt64(123456789L); });
        var r = ReaderOver(f); MailTakeMoneyCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.Mailbox);
        Assert.Equal(WideMailId, a.MailID);
        Assert.Equal(123456789L, a.Money);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>Money stays 64-bit on both arms; only the id narrows.</summary>
    [Fact]
    public void MailTakeMoney_PreWotLKClassic_NarrowsOnlyTheId()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(5u); w.WriteInt64(long.MaxValue); });
        var r = ReaderOver(f); MailTakeMoneyCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.Mailbox);
        Assert.Equal(5L, a.MailID);
        Assert.Equal(long.MaxValue, a.Money);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>Both id and attachment id narrow together here — the only packet where two do.</summary>
    [Fact]
    public void MailTakeItem_WotLKClassic_ReadsBothAsSixtyFourBit()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteInt64(WideMailId); w.WriteInt64(WideMailId + 1); });
        var r = ReaderOver(f); MailTakeItemCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.Mailbox);
        Assert.Equal(WideMailId, a.MailID);
        Assert.Equal(WideMailId + 1, a.AttachID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void MailTakeItem_PreWotLKClassic_ReadsBothAsThirtyTwoBit()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(11u); w.WriteUInt32(2u); });
        var r = ReaderOver(f); MailTakeItemCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.Mailbox);
        Assert.Equal(11L, a.MailID);
        Assert.Equal(2L, a.AttachID);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>
    /// The two arms must disagree on a wide id, or the pair is not actually doing anything and the
    /// tests above would pass against a single codec.
    /// </summary>
    [Fact]
    public void MailIdPair_ArmsDisagreeOnAWideId()
    {
        var (_, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteInt64(WideMailId); });

        var r1 = ReaderOver(f); MailCreateTextItemCodecWotLKClassic.Read(ref r1, out var wide);
        var r2 = ReaderOver(f); MailCreateTextItemCodecPreWotLKClassic.Read(ref r2, out var narrow);

        Assert.NotEqual(wide.MailID, narrow.MailID);
        Assert.Equal(0, r1.Remaining);
        Assert.Equal(4, r2.Remaining);   // the 32-bit arm leaves the high half unread
    }

    // ---- mail: the composed one ----

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void SendMail_Matches(int attachmentCount)
    {
        const string target = "Pally";
        const string subject = "Here is your gold";
        const string body = "Thanks for the help in Deadmines. Take this.";

        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteInt32(41);
            w.WriteInt64(9999L);
            w.WriteInt64(1234L);
            w.WriteBits(target.Length, 9);
            w.WriteBits(subject.Length, 9);
            w.WriteBits(body.Length, 11);
            w.WriteBits(attachmentCount, 5);
            w.WriteString(target);
            w.WriteString(subject);
            w.WriteString(body);
            for (int i = 0; i < attachmentCount; i++)
            {
                w.WriteUInt8((byte)(i + 1));
                w.WritePackedGuid128(i % 2 == 0 ? Guid : Guid2);
            }
        });

        var e = new Frozen.SendMail(); e.Read(o);
        var r = ReaderOver(f); SendMailCodec.Read(ref r, out var a);

        Assert.Equal(e.Mailbox, a.Mailbox);
        Assert.Equal(e.StationeryID, a.StationeryID);
        Assert.Equal(e.SendMoney, a.SendMoney);
        Assert.Equal(e.Cod, a.Cod);
        Assert.Equal(e.Target, a.Target);
        Assert.Equal(e.Subject, a.Subject);
        Assert.Equal(e.Body, a.Body);
        Assert.Equal(target, a.Target);
        Assert.Equal(subject, a.Subject);
        Assert.Equal(body, a.Body);
        Assert.Equal(e.Attachments.Count, a.Attachments.Count);
        for (int i = 0; i < attachmentCount; i++)
        {
            Assert.Equal(e.Attachments[i].AttachPosition, a.Attachments[i].AttachPosition);
            Assert.Equal(e.Attachments[i].ItemGUID, a.Attachments[i].ItemGUID);
        }
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>Empty strings are their own skip path — a zero length must consume nothing.</summary>
    [Fact]
    public void SendMail_EmptyStrings_Matches()
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteInt32(0);
            w.WriteInt64(0L);
            w.WriteInt64(0L);
            w.WriteBits(0, 9);
            w.WriteBits(0, 9);
            w.WriteBits(0, 11);
            w.WriteBits(0, 5);
        });

        var e = new Frozen.SendMail(); e.Read(o);
        var r = ReaderOver(f); SendMailCodec.Read(ref r, out var a);

        Assert.Equal(e.Target, a.Target);
        Assert.Equal(e.Subject, a.Subject);
        Assert.Equal(e.Body, a.Body);
        Assert.Equal(string.Empty, a.Target);
        Assert.Empty(a.Attachments);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- petition ----

    /// <summary>
    /// The title length precedes the GUID while the title follows the index, so the bit block sits
    /// between a length and its string.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("The Frostwolf Irregulars")]
    public void PetitionBuy_Matches(string title)
    {
        var (o, f) = Build(w =>
        {
            w.WriteBits(title.Length, 7);
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(3u);
            w.WriteString(title);
        });

        var e = new Frozen.PetitionBuy(); e.Read(o);
        var r = ReaderOver(f); PetitionBuyCodec.Read(ref r, out var a);

        Assert.Equal(e.Unit, a.Unit);
        Assert.Equal(e.Index, a.Index);
        Assert.Equal(e.Title, a.Title);
        Assert.Equal(title, a.Title);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void PetitionShowSignatures_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.PetitionShowSignatures(); e.Read(o);
        var r = ReaderOver(f); PetitionShowSignaturesCodec.Read(ref r, out var a);
        Assert.Equal(e.Item, a.Item);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void QueryPetition_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(777u); w.WritePackedGuid128(Guid); });
        var e = new Frozen.QueryPetition(); e.Read(o);
        var r = ReaderOver(f); QueryPetitionCodec.Read(ref r, out var a);
        Assert.Equal(e.PetitionID, a.PetitionID);
        Assert.Equal(e.ItemGUID, a.ItemGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>Resets the bit accumulator after a byte-aligned GUID before reading a bit length.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("Wipe Enthusiasts")]
    public void PetitionRenameGuild_Matches(string name)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.ResetBitPos();
            w.WriteBits(name.Length, 7);
            w.WriteString(name);
        });

        var e = new Frozen.PetitionRenameGuild(); e.Read(o);
        var r = ReaderOver(f); PetitionRenameGuildCodec.Read(ref r, out var a);

        Assert.Equal(e.PetitionGuid, a.PetitionGuid);
        Assert.Equal(e.NewGuildName, a.NewGuildName);
        Assert.Equal(name, a.NewGuildName);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void OfferPetition_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(1u); w.WritePackedGuid128(Guid); w.WritePackedGuid128(Guid2); });
        var e = new Frozen.OfferPetition(); e.Read(o);
        var r = ReaderOver(f); OfferPetitionCodec.Read(ref r, out var a);
        Assert.Equal(e.UnkInt, a.UnkInt);
        Assert.Equal(e.ItemGUID, a.ItemGUID);
        Assert.Equal(e.TargetPlayer, a.TargetPlayer);
        Assert.Equal(Guid, a.ItemGUID);
        Assert.Equal(Guid2, a.TargetPlayer);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void DeclinePetition_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.DeclinePetition(); e.Read(o);
        var r = ReaderOver(f); DeclinePetitionCodec.Read(ref r, out var a);
        Assert.Equal(e.PetitionGUID, a.PetitionGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)1)]
    public void SignPetition_Matches(byte choice)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt8(choice); });
        var e = new Frozen.SignPetition(); e.Read(o);
        var r = ReaderOver(f); SignPetitionCodec.Read(ref r, out var a);
        Assert.Equal(e.PetitionGUID, a.PetitionGUID);
        Assert.Equal(e.Choice, a.Choice);
        Assert.Equal(choice, a.Choice);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>The emblem block is present.</summary>
    [Fact]
    public void TurnInPetition_WithEmblem_Matches()
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(1u);
            w.WriteUInt32(2u);
            w.WriteUInt32(3u);
            w.WriteUInt32(4u);
            w.WriteUInt32(5u);
        });

        var e = new Frozen.TurnInPetition(); e.Read(o);
        var r = ReaderOver(f); TurnInPetitionCodec.Read(ref r, out var a);

        Assert.Equal(e.Item, a.Item);
        Assert.Equal(e.BackgroundColor, a.BackgroundColor);
        Assert.Equal(e.EmblemStyle, a.EmblemStyle);
        Assert.Equal(e.EmblemColor, a.EmblemColor);
        Assert.Equal(e.BorderStyle, a.BorderStyle);
        Assert.Equal(e.BorderColor, a.BorderColor);
        Assert.Equal(1u, a.BackgroundColor);
        Assert.Equal(5u, a.BorderColor);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// The emblem block is absent — the skip path. The record struct defaults every emblem field to
    /// zero, which is what the old class yielded; this is the case that would catch it if it did not.
    /// </summary>
    [Fact]
    public void TurnInPetition_WithoutEmblem_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.TurnInPetition(); e.Read(o);
        var r = ReaderOver(f); TurnInPetitionCodec.Read(ref r, out var a);

        Assert.Equal(e.Item, a.Item);
        Assert.Equal(e.BackgroundColor, a.BackgroundColor);
        Assert.Equal(e.EmblemStyle, a.EmblemStyle);
        Assert.Equal(e.EmblemColor, a.EmblemColor);
        Assert.Equal(e.BorderStyle, a.BorderStyle);
        Assert.Equal(e.BorderColor, a.BorderColor);
        Assert.Equal(0u, a.BackgroundColor);
        Assert.Equal(0u, a.BorderColor);
        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
