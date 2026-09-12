using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Guild CMSG codecs — roster, ranks, and the guild bank.
//
// Nothing here branches on client build: the guild CMSG layouts are the same across every modern
// build the proxy accepts, so these are plain codecs rather than ranged ones. The bank-item family
// is the largest group of near-identical layouts in the inbound set, and each still gets its own
// codec — sharing one would mean a codec that reads conditionally, which is the shape ranged codecs
// exist to replace.
//
// The reads below mirror the ClientPacket.Read() bodies they replace field for field, including bit
// order. GuildCodecEquivalenceTests proves that against the frozen originals, final reader position
// included.

public static class QueryGuildInfoCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryGuildInfo packet)
    {
        WowGuid128 guildGuid = r.ReadPackedGuid128();
        WowGuid128 playerGuid = r.ReadPackedGuid128();
        packet = new QueryGuildInfo(guildGuid, playerGuid);
    }
}

// ---- guild text ----

public static class GuildUpdateMotdTextCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildUpdateMotdText packet)
        => packet = new GuildUpdateMotdText(r.ReadString(r.ReadBits<uint>(11)));
}

public static class GuildUpdateInfoTextCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildUpdateInfoText packet)
        => packet = new GuildUpdateInfoText(r.ReadString(r.ReadBits<uint>(11)));
}

// ---- membership ----

public static class GuildSetMemberNoteCodec
{
    public static void Read(ref SpanPacketReader r, out GuildSetMemberNote packet)
    {
        WowGuid128 noteeGuid = r.ReadPackedGuid128();
        uint noteLen = r.ReadBits<uint>(8);
        bool isPublic = r.HasBit();
        string note = r.ReadString(noteLen);
        packet = new GuildSetMemberNote(noteeGuid, isPublic, note);
    }
}

public static class GuildPromoteMemberCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildPromoteMember packet)
        => packet = new GuildPromoteMember(r.ReadPackedGuid128());
}

public static class GuildDemoteMemberCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildDemoteMember packet)
        => packet = new GuildDemoteMember(r.ReadPackedGuid128());
}

public static class GuildOfficerRemoveMemberCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildOfficerRemoveMember packet)
        => packet = new GuildOfficerRemoveMember(r.ReadPackedGuid128());
}

public static class GuildInviteByNameCodec
{
    public static void Read(ref SpanPacketReader r, out GuildInviteByName packet)
    {
        uint nameLen = r.ReadBits<uint>(9);
        bool isArena = r.HasBit();

        string name = r.ReadString(nameLen);

        // Absent for a guild invite, so ArenaTeamId stays 0 — which is what the handler tests to
        // tell the two apart.
        uint arenaTeamId = 0;
        if (isArena)
            arenaTeamId = r.ReadUInt32();

        packet = new GuildInviteByName(name, arenaTeamId);
    }
}

public static class GuildSetGuildMasterCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildSetGuildMaster packet)
        => packet = new GuildSetGuildMaster(r.ReadString(r.ReadBits<uint>(9)));
}

// ---- ranks ----

public static class GuildSetRankPermissionsCodec
{
    public static void Read(ref SpanPacketReader r, out GuildSetRankPermissions packet)
    {
        uint rankId = r.ReadUInt32();
        uint rankOrder = r.ReadUInt32();
        uint flags = r.ReadUInt32();
        int withdrawGoldLimit = r.ReadInt32();

        GuildBankTabLimits tabFlags = default;
        GuildBankTabLimits tabWithdrawItemLimit = default;
        for (byte i = 0; i < GuildConst.MaxBankTabs; i++)
        {
            tabFlags[i] = r.ReadUInt32();
            tabWithdrawItemLimit[i] = r.ReadUInt32();
        }

        uint oldFlags = r.ReadUInt32();

        r.ResetBitPos();
        uint rankNameLen = r.ReadBits<uint>(7);
        string rankName = r.ReadString(rankNameLen);

        packet = new GuildSetRankPermissions(
            rankId, rankOrder, flags, withdrawGoldLimit, tabFlags, tabWithdrawItemLimit, oldFlags, rankName);
    }
}

public static class GuildAddRankCodec
{
    public static void Read(ref SpanPacketReader r, out GuildAddRank packet)
    {
        // The length is read before the bit position is reset, so the int32 that follows starts on
        // the next byte. Reading them the other way round shifts everything after it.
        uint nameLen = r.ReadBits<uint>(7);
        r.ResetBitPos();

        int rankOrder = r.ReadInt32();
        string name = r.ReadString(nameLen);
        packet = new GuildAddRank(name, rankOrder);
    }
}

public static class GuildDeleteRankCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildDeleteRank packet)
        => packet = new GuildDeleteRank(r.ReadInt32());
}

// ---- emblem and invite settings ----

public static class SaveGuildEmblemCodec
{
    public static void Read(ref SpanPacketReader r, out SaveGuildEmblem packet)
    {
        WowGuid128 designerGuid = r.ReadPackedGuid128();
        uint emblemStyle = r.ReadUInt32();
        uint emblemColor = r.ReadUInt32();
        uint borderStyle = r.ReadUInt32();
        uint borderColor = r.ReadUInt32();
        uint backgroundColor = r.ReadUInt32();
        packet = new SaveGuildEmblem(
            designerGuid, emblemStyle, emblemColor, borderStyle, borderColor, backgroundColor);
    }
}

public static class SetAutoDeclineGuildInvitesCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetAutoDeclineGuildInvites packet)
        => packet = new SetAutoDeclineGuildInvites(r.ReadBool());
}

// ---- guild bank ----

public static class GuildBankAtivateCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildBankAtivate packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        bool fullUpdate = r.HasBit();
        packet = new GuildBankAtivate(bankGuid, fullUpdate);
    }
}

public static class GuildBankQueryTabCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildBankQueryTab packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        byte tab = r.ReadUInt8();
        bool fullUpdate = r.HasBit();
        packet = new GuildBankQueryTab(bankGuid, tab, fullUpdate);
    }
}

public static class GuildBankDepositMoneyCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildBankDepositMoney packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        ulong money = r.ReadUInt64();
        packet = new GuildBankDepositMoney(bankGuid, money);
    }
}

public static class GuildBankWithdrawMoneyCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildBankWithdrawMoney packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        ulong money = r.ReadUInt64();
        packet = new GuildBankWithdrawMoney(bankGuid, money);
    }
}

public static class GuildBankTextQueryCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildBankTextQuery packet)
        => packet = new GuildBankTextQuery(r.ReadInt32());
}

public static class GuildBankLogQueryCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildBankLogQuery packet)
        => packet = new GuildBankLogQuery(r.ReadInt32());
}

public static class GuildBankSetTabTextCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildBankSetTabText packet)
    {
        int tab = r.ReadInt32();
        string tabText = r.ReadString(r.ReadBits<uint>(14));
        packet = new GuildBankSetTabText(tab, tabText);
    }
}

public static class GuildBankUpdateTabCodec
{
    public static void Read(ref SpanPacketReader r, out GuildBankUpdateTab packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        byte bankTab = r.ReadUInt8();

        r.ResetBitPos();
        uint nameLen = r.ReadBits<uint>(7);
        uint iconLen = r.ReadBits<uint>(9);

        string name = r.ReadString(nameLen);
        string icon = r.ReadString(iconLen);
        packet = new GuildBankUpdateTab(bankGuid, bankTab, name, icon);
    }
}

public static class GuildBankBuyTabCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GuildBankBuyTab packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        byte bankTab = r.ReadUInt8();
        packet = new GuildBankBuyTab(bankGuid, bankTab);
    }
}

// ---- guild bank item movement ----

public static class AutoGuildBankItemCodec
{
    public static void Read(ref SpanPacketReader r, out AutoGuildBankItem packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        byte bankTab = r.ReadUInt8();
        byte bankSlot = r.ReadUInt8();
        byte containerItemSlot = r.ReadUInt8();

        // Null means the backpack, which WritePlayerBagAndSlot translates differently from a bag.
        byte? containerSlot = null;
        if (r.HasBit())
            containerSlot = r.ReadUInt8();

        packet = new AutoGuildBankItem(bankGuid, bankTab, bankSlot, containerSlot, containerItemSlot);
    }
}

public static class SplitItemToGuildBankCodec
{
    public static void Read(ref SpanPacketReader r, out SplitItemToGuildBank packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        byte bankTab = r.ReadUInt8();
        byte bankSlot = r.ReadUInt8();
        byte containerItemSlot = r.ReadUInt8();
        uint stackCount = r.ReadUInt32();

        byte? containerSlot = null;
        if (r.HasBit())
            containerSlot = r.ReadUInt8();

        packet = new SplitItemToGuildBank(
            bankGuid, bankTab, bankSlot, containerSlot, containerItemSlot, stackCount);
    }
}

public static class AutoStoreGuildBankItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out AutoStoreGuildBankItem packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        byte bankTab = r.ReadUInt8();
        byte bankSlot = r.ReadUInt8();
        packet = new AutoStoreGuildBankItem(bankGuid, bankTab, bankSlot);
    }
}

public static class MoveGuildBankItemCodec
{
    public static void Read(ref SpanPacketReader r, out MoveGuildBankItem packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        byte bankTab1 = r.ReadUInt8();
        byte bankSlot1 = r.ReadUInt8();
        byte bankTab2 = r.ReadUInt8();
        byte bankSlot2 = r.ReadUInt8();
        packet = new MoveGuildBankItem(bankGuid, bankTab1, bankSlot1, bankTab2, bankSlot2);
    }
}

public static class SplitGuildBankItemCodec
{
    public static void Read(ref SpanPacketReader r, out SplitGuildBankItem packet)
    {
        WowGuid128 bankGuid = r.ReadPackedGuid128();
        byte bankTab1 = r.ReadUInt8();
        byte bankSlot1 = r.ReadUInt8();
        byte bankTab2 = r.ReadUInt8();
        byte bankSlot2 = r.ReadUInt8();
        uint stackCount = r.ReadUInt32();
        packet = new SplitGuildBankItem(bankGuid, bankTab1, bankSlot1, bankTab2, bankSlot2, stackCount);
    }
}
