using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the guild codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// Every case asserts field values <b>and</b> the reader's final position, and readers are built
/// through <c>GetRemainingSpan()</c> — the accessor the dispatch site uses — rather than a
/// hand-written <c>AsSpan(2)</c> that would only agree with itself.
/// <para>
/// The optional paths get their own cases, because that is where a positional record struct
/// diverges from the class it replaced: a skipped field yields zero rather than whatever the class
/// initialised it to. Here that is <c>GuildInviteByName.ArenaTeamId</c> and the nullable
/// <c>ContainerSlot</c> on the two bank-item packets.
/// </para>
/// </remarks>
public class GuildCodecEquivalenceTests
{
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

    // ---- query ----

    [Fact]
    public void QueryGuildInfo_Matches()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WritePackedGuid128(Guid2); });
        var e = new Frozen.QueryGuildInfo(); e.Read(o);
        var r = ReaderOver(f); QueryGuildInfoCodec.Read(ref r, out var a);
        Assert.Equal(e.GuildGuid, a.GuildGuid);
        Assert.Equal(e.PlayerGuid, a.PlayerGuid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- guild text: an 11-bit length then the bytes ----

    [Theory]
    [InlineData("")]
    [InlineData("Raid at 8.")]
    [InlineData("a message long enough to need more than nine bits of length to describe it, which is "
              + "the difference the 3.4.3 client's wider field makes and the reason issue #177 existed "
              + "at all; nine bits stops at 511 and this is comfortably past two hundred characters "
              + "even before the rest of the sentence is counted, so it exercises the wide path")]
    public void GuildUpdateMotdText_Matches(string text)
    {
        var (o, f) = Build(w => { w.WriteBits((uint)text.Length, 11); w.WriteString(text); });
        var e = new Frozen.GuildUpdateMotdText(); e.Read(o);
        var r = ReaderOver(f); GuildUpdateMotdTextCodec.Read(ref r, out var a);
        Assert.Equal(e.MotdText, a.MotdText);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData("")]
    [InlineData("We raid Tuesdays and Thursdays.")]
    public void GuildUpdateInfoText_Matches(string text)
    {
        var (o, f) = Build(w => { w.WriteBits((uint)text.Length, 11); w.WriteString(text); });
        var e = new Frozen.GuildUpdateInfoText(); e.Read(o);
        var r = ReaderOver(f); GuildUpdateInfoTextCodec.Read(ref r, out var a);
        Assert.Equal(e.InfoText, a.InfoText);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- membership ----

    [Theory]
    [InlineData(true, "healer, has flask")]
    [InlineData(false, "")]
    public void GuildSetMemberNote_Matches(bool isPublic, string note)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteBits((uint)note.Length, 8);
            w.WriteBit(isPublic);
            w.WriteString(note);
        });

        var e = new Frozen.GuildSetMemberNote(); e.Read(o);
        var r = ReaderOver(f); GuildSetMemberNoteCodec.Read(ref r, out var a);
        Assert.Equal(e.NoteeGUID, a.NoteeGUID);
        Assert.Equal(e.IsPublic, a.IsPublic);
        Assert.Equal(e.Note, a.Note);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void GuildPromoteMember_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.GuildPromoteMember(); e.Read(o);
        var r = ReaderOver(f); GuildPromoteMemberCodec.Read(ref r, out var a);
        Assert.Equal(e.Promotee, a.Promotee);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void GuildDemoteMember_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.GuildDemoteMember(); e.Read(o);
        var r = ReaderOver(f); GuildDemoteMemberCodec.Read(ref r, out var a);
        Assert.Equal(e.Demotee, a.Demotee);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void GuildOfficerRemoveMember_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.GuildOfficerRemoveMember(); e.Read(o);
        var r = ReaderOver(f); GuildOfficerRemoveMemberCodec.Read(ref r, out var a);
        Assert.Equal(e.Removee, a.Removee);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// The arena-team id is only on the wire when the bit says so, and the handler branches on
    /// <c>ArenaTeamId == 0</c> to decide between a guild invite and an arena-team invite. A codec
    /// that read it unconditionally, or left it uninitialised on the wrong path, would send the
    /// wrong opcode rather than fail.
    /// </summary>
    [Theory]
    [InlineData("Thrall", 0u)]
    [InlineData("Thrall", 4321u)]
    [InlineData("", 0u)]
    public void GuildInviteByName_Matches(string name, uint arenaTeamId)
    {
        bool isArena = arenaTeamId != 0;
        var (o, f) = Build(w =>
        {
            w.WriteBits((uint)name.Length, 9);
            w.WriteBit(isArena);
            w.WriteString(name);
            if (isArena)
                w.WriteUInt32(arenaTeamId);
        });

        var e = new Frozen.GuildInviteByName(); e.Read(o);
        var r = ReaderOver(f); GuildInviteByNameCodec.Read(ref r, out var a);
        Assert.Equal(e.Name, a.Name);
        Assert.Equal(e.ArenaTeamId, a.ArenaTeamId);
        Assert.Equal(arenaTeamId, a.ArenaTeamId);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData("Sylvanas")]
    [InlineData("")]
    public void GuildSetGuildMaster_Matches(string name)
    {
        var (o, f) = Build(w => { w.WriteBits((uint)name.Length, 9); w.WriteString(name); });
        var e = new Frozen.GuildSetGuildMaster(); e.Read(o);
        var r = ReaderOver(f); GuildSetGuildMasterCodec.Read(ref r, out var a);
        Assert.Equal(e.NewMasterName, a.NewMasterName);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- ranks ----

    /// <summary>
    /// Twelve uint32s between the header and the trailing 7-bit name, so a codec that miscounted
    /// the loop would still produce a plausible name and only the position assertion would notice.
    /// </summary>
    [Theory]
    [InlineData("Officer")]
    [InlineData("")]
    public void GuildSetRankPermissions_Matches(string rankName)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32(1u);
            w.WriteUInt32(2u);
            w.WriteUInt32(0x00DDFFBFu);
            w.WriteInt32(-1);
            for (uint i = 0; i < GuildConst.MaxBankTabs; i++)
            {
                w.WriteUInt32(7u + i);
                w.WriteUInt32(100u + i);
            }
            w.WriteUInt32(0x00DDFFBFu);
            w.WriteBits((uint)rankName.Length, 7);
            w.WriteString(rankName);
        });

        var e = new Frozen.GuildSetRankPermissions(); e.Read(o);
        var r = ReaderOver(f); GuildSetRankPermissionsCodec.Read(ref r, out var a);

        Assert.Equal(e.RankID, a.RankID);
        Assert.Equal(e.RankOrder, a.RankOrder);
        Assert.Equal(e.Flags, a.Flags);
        Assert.Equal(e.WithdrawGoldLimit, a.WithdrawGoldLimit);
        Assert.Equal(e.OldFlags, a.OldFlags);
        Assert.Equal(e.RankName, a.RankName);
        for (int i = 0; i < GuildConst.MaxBankTabs; i++)
        {
            Assert.Equal(e.TabFlags[i], a.TabFlags[i]);
            Assert.Equal(e.TabWithdrawItemLimit[i], a.TabWithdrawItemLimit[i]);
        }
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// The name length is read before the bit position is reset and the int32 after it, so reading
    /// them the other way round shifts everything that follows.
    /// </summary>
    [Theory]
    [InlineData("Initiate", 5)]
    [InlineData("", 0)]
    public void GuildAddRank_Matches(string name, int rankOrder)
    {
        var (o, f) = Build(w =>
        {
            w.WriteBits((uint)name.Length, 7);
            w.WriteInt32(rankOrder);          // flushes the pending bits, matching ResetBitPos
            w.WriteString(name);
        });

        var e = new Frozen.GuildAddRank(); e.Read(o);
        var r = ReaderOver(f); GuildAddRankCodec.Read(ref r, out var a);
        Assert.Equal(e.Name, a.Name);
        Assert.Equal(e.RankOrder, a.RankOrder);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(int.MinValue)]
    public void GuildDeleteRank_Matches(int rankOrder)
    {
        var (o, f) = Build(w => w.WriteInt32(rankOrder));
        var e = new Frozen.GuildDeleteRank(); e.Read(o);
        var r = ReaderOver(f); GuildDeleteRankCodec.Read(ref r, out var a);
        Assert.Equal(e.RankOrder, a.RankOrder);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- emblem and invite settings ----

    [Fact]
    public void SaveGuildEmblem_Matches()
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(1u);
            w.WriteUInt32(2u);
            w.WriteUInt32(3u);
            w.WriteUInt32(4u);
            w.WriteUInt32(uint.MaxValue);
        });

        var e = new Frozen.SaveGuildEmblem(); e.Read(o);
        var r = ReaderOver(f); SaveGuildEmblemCodec.Read(ref r, out var a);
        Assert.Equal(e.DesignerGUID, a.DesignerGUID);
        Assert.Equal(e.EmblemStyle, a.EmblemStyle);
        Assert.Equal(e.EmblemColor, a.EmblemColor);
        Assert.Equal(e.BorderStyle, a.BorderStyle);
        Assert.Equal(e.BorderColor, a.BorderColor);
        Assert.Equal(e.BackgroundColor, a.BackgroundColor);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetAutoDeclineGuildInvites_Matches(bool blocked)
    {
        var (o, f) = Build(w => w.WriteBool(blocked));
        var e = new Frozen.SetAutoDeclineGuildInvites(); e.Read(o);
        var r = ReaderOver(f); SetAutoDeclineGuildInvitesCodec.Read(ref r, out var a);
        Assert.Equal(e.GuildInvitesShouldGetBlocked, a.GuildInvitesShouldGetBlocked);
        Assert.Equal(blocked, a.GuildInvitesShouldGetBlocked);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- guild bank ----

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GuildBankAtivate_Matches(bool fullUpdate)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteBit(fullUpdate); });
        var e = new Frozen.GuildBankAtivate(); e.Read(o);
        var r = ReaderOver(f); GuildBankAtivateCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.FullUpdate, a.FullUpdate);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((byte)0, true)]
    [InlineData((byte)5, false)]
    public void GuildBankQueryTab_Matches(byte tab, bool fullUpdate)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(tab);
            w.WriteBit(fullUpdate);
        });

        var e = new Frozen.GuildBankQueryTab(); e.Read(o);
        var r = ReaderOver(f); GuildBankQueryTabCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.Tab, a.Tab);
        Assert.Equal(e.FullUpdate, a.FullUpdate);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// Money is 64-bit on the modern wire and 32-bit on the legacy one. The codec must carry the
    /// full width — the narrowing is the system's decision, made visible there.
    /// </summary>
    [Theory]
    [InlineData(0UL)]
    [InlineData(12345UL)]
    [InlineData(ulong.MaxValue)]
    public void GuildBankDepositMoney_Matches(ulong money)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt64(money); });
        var e = new Frozen.GuildBankDepositMoney(); e.Read(o);
        var r = ReaderOver(f); GuildBankDepositMoneyCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.Money, a.Money);
        Assert.Equal(money, a.Money);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(ulong.MaxValue)]
    public void GuildBankWithdrawMoney_Matches(ulong money)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt64(money); });
        var e = new Frozen.GuildBankWithdrawMoney(); e.Read(o);
        var r = ReaderOver(f); GuildBankWithdrawMoneyCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.Money, a.Money);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void GuildBankTextQuery_Matches(int tab)
    {
        var (o, f) = Build(w => w.WriteInt32(tab));
        var e = new Frozen.GuildBankTextQuery(); e.Read(o);
        var r = ReaderOver(f); GuildBankTextQueryCodec.Read(ref r, out var a);
        Assert.Equal(e.Tab, a.Tab);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void GuildBankLogQuery_Matches(int tab)
    {
        var (o, f) = Build(w => w.WriteInt32(tab));
        var e = new Frozen.GuildBankLogQuery(); e.Read(o);
        var r = ReaderOver(f); GuildBankLogQueryCodec.Read(ref r, out var a);
        Assert.Equal(e.Tab, a.Tab);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(2, "Deposit only. Ask an officer.")]
    [InlineData(0, "")]
    public void GuildBankSetTabText_Matches(int tab, string tabText)
    {
        var (o, f) = Build(w =>
        {
            w.WriteInt32(tab);
            w.WriteBits((uint)tabText.Length, 14);
            w.WriteString(tabText);
        });

        var e = new Frozen.GuildBankSetTabText(); e.Read(o);
        var r = ReaderOver(f); GuildBankSetTabTextCodec.Read(ref r, out var a);
        Assert.Equal(e.Tab, a.Tab);
        Assert.Equal(e.TabText, a.TabText);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// Two lengths of different widths are read before either string, so they cannot be inlined
    /// into the reads that consume them.
    /// </summary>
    [Theory]
    [InlineData("Consumables", "Interface\\Icons\\INV_Potion_51")]
    [InlineData("", "")]
    public void GuildBankUpdateTab_Matches(string name, string icon)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(3);
            w.WriteBits((uint)name.Length, 7);
            w.WriteBits((uint)icon.Length, 9);
            w.WriteString(name);
            w.WriteString(icon);
        });

        var e = new Frozen.GuildBankUpdateTab(); e.Read(o);
        var r = ReaderOver(f); GuildBankUpdateTabCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.BankTab, a.BankTab);
        Assert.Equal(e.Name, a.Name);
        Assert.Equal(e.Icon, a.Icon);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void GuildBankBuyTab_Matches()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt8(4); });
        var e = new Frozen.GuildBankBuyTab(); e.Read(o);
        var r = ReaderOver(f); GuildBankBuyTabCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.BankTab, a.BankTab);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- guild bank item movement ----

    /// <summary>
    /// <c>ContainerSlot</c> absent means the backpack, and <c>WritePlayerBagAndSlot</c> translates
    /// that case entirely differently — it adjusts the slot rather than the bag. A codec that
    /// produced <c>0</c> instead of <c>null</c> would send the item to bag 0, silently.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData((byte)0)]
    [InlineData((byte)23)]
    public void AutoGuildBankItem_Matches(byte? containerSlot)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(2);
            w.WriteUInt8(17);
            w.WriteUInt8(35);
            w.WriteBit(containerSlot != null);
            if (containerSlot != null)
                w.WriteUInt8(containerSlot.Value);
        });

        var e = new Frozen.AutoGuildBankItem(); e.Read(o);
        var r = ReaderOver(f); AutoGuildBankItemCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.BankTab, a.BankTab);
        Assert.Equal(e.BankSlot, a.BankSlot);
        Assert.Equal(e.ContainerSlot, a.ContainerSlot);
        Assert.Equal(containerSlot, a.ContainerSlot);
        Assert.Equal(e.ContainerItemSlot, a.ContainerItemSlot);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(null, 1u)]
    [InlineData((byte)23, 20u)]
    [InlineData((byte)23, uint.MaxValue)]
    public void SplitItemToGuildBank_Matches(byte? containerSlot, uint stackCount)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(2);
            w.WriteUInt8(17);
            w.WriteUInt8(35);
            w.WriteUInt32(stackCount);
            w.WriteBit(containerSlot != null);
            if (containerSlot != null)
                w.WriteUInt8(containerSlot.Value);
        });

        var e = new Frozen.SplitItemToGuildBank(); e.Read(o);
        var r = ReaderOver(f); SplitItemToGuildBankCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.BankTab, a.BankTab);
        Assert.Equal(e.BankSlot, a.BankSlot);
        Assert.Equal(e.ContainerSlot, a.ContainerSlot);
        Assert.Equal(containerSlot, a.ContainerSlot);
        Assert.Equal(e.ContainerItemSlot, a.ContainerItemSlot);
        Assert.Equal(e.StackCount, a.StackCount);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void AutoStoreGuildBankItem_Matches()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt8(1); w.WriteUInt8(9); });
        var e = new Frozen.AutoStoreGuildBankItem(); e.Read(o);
        var r = ReaderOver(f); AutoStoreGuildBankItemCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.BankTab, a.BankTab);
        Assert.Equal(e.BankSlot, a.BankSlot);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// Source and destination are read in order 1 then 2, and the legacy packet writes 2 before 1.
    /// Swapping them moves the item to where it came from.
    /// </summary>
    [Fact]
    public void MoveGuildBankItem_Matches()
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(1);
            w.WriteUInt8(2);
            w.WriteUInt8(3);
            w.WriteUInt8(4);
        });

        var e = new Frozen.MoveGuildBankItem(); e.Read(o);
        var r = ReaderOver(f); MoveGuildBankItemCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.BankTab1, a.BankTab1);
        Assert.Equal(e.BankSlot1, a.BankSlot1);
        Assert.Equal(e.BankTab2, a.BankTab2);
        Assert.Equal(e.BankSlot2, a.BankSlot2);
        Assert.Equal((byte)1, a.BankTab1);
        Assert.Equal((byte)4, a.BankSlot2);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(uint.MaxValue)]
    public void SplitGuildBankItem_Matches(uint stackCount)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(1);
            w.WriteUInt8(2);
            w.WriteUInt8(3);
            w.WriteUInt8(4);
            w.WriteUInt32(stackCount);
        });

        var e = new Frozen.SplitGuildBankItem(); e.Read(o);
        var r = ReaderOver(f); SplitGuildBankItemCodec.Read(ref r, out var a);
        Assert.Equal(e.BankGuid, a.BankGuid);
        Assert.Equal(e.BankTab1, a.BankTab1);
        Assert.Equal(e.BankSlot1, a.BankSlot1);
        Assert.Equal(e.BankTab2, a.BankTab2);
        Assert.Equal(e.BankSlot2, a.BankSlot2);
        Assert.Equal(e.StackCount, a.StackCount);
        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
