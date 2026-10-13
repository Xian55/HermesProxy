using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's guild CMSGs — roster, ranks, and the guild bank.
/// </summary>
/// <remarks>
/// <para>
/// Bodies were moved from <c>World/Server/PacketHandlers/GuildHandler.cs</c>, not retyped;
/// <c>verify-handler-port.py</c> diffs each one against the original.
/// </para>
/// <para>
/// <see cref="HandleGuildSetRankPermissions"/> coalesces rank edits through the session's server
/// outbox rather than a coalescer and timer on <c>WorldSocket</c>. The outbox belongs to the
/// session, so a pending edit outlives the modern socket and needs no flush when the socket closes.
/// </para>
/// </remarks>
public static class GuildSystem
{
    // Same logger and source label as WorldSocket's, so the guild-bank slot translation keeps
    // logging under the same category and prefix it did as an instance method.
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(WorldSocket).PadRight(15);

    [HandlesCmsg(Opcode.CMSG_QUERY_GUILD_INFO)]
    public static void HandleQueryGuildInfo(in QueryGuildInfo query, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_GUILD_INFO);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleGuildQueryOpcode: the guild's GUID and a member's, both
            // whole. It answers only when that player is in that guild, and the 4.4.2 client
            // leaves the player empty (seen in a capture), so name a member the session knows.
            WowGuid128 member = query.PlayerGuid;
            if (member.IsEmpty())
                member = ctx.GetSession().GameState.GetKnownGuildMember((uint)query.GuildGuid.GetCounter());
            packet.WriteUInt64(CataGuildGuid(query.GuildGuid.GetCounter()));
            packet.WriteGuid(member.To64());
        }
        else
            packet.WriteUInt32((uint)query.GuildGuid.GetCounter());
        ctx.SendPacketToServer(packet);
    }

    /// <summary>
    /// A 4.3.4 guild GUID: the guild id under the guild high type (TrinityCore 4.3.4
    /// <c>ObjectGuid(HighGuid::Guild, id)</c>, 0x1FF shifted by 52).
    /// </summary>
    internal static ulong CataGuildGuid(ulong guildId) => guildId == 0 ? 0 : (0x1FFul << 52) | guildId;

    /// <summary>
    /// The modern client asks for the ranks by themselves. A 4.3.4 server answers that with
    /// SMSG_GUILD_RANKS; the older ones have no such request and send the ranks inside the roster,
    /// which is where the proxy takes them from.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_GUILD_GET_RANKS)]
    public static void HandleGuildGetRanks(in GuildGetRanks query, in SessionContext ctx)
    {
        if (!HermesProxy.World.Client.WorldClient.IsCataLegacy)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_GET_RANKS);
        MaskedGuid.Write(packet, CataGuildGuid(query.GuildGuid.GetCounter()), [2, 3, 0, 6, 4, 7, 5, 1], [3, 4, 5, 7, 1, 0, 6, 2]);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_PERMISSIONS_QUERY)]
    public static void HandleGuildPermissionsQuery(in EmptyClientPacket query, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;

        // 4.3.4 asks and answers with a pair of its own (TrinityCore 4.3.4 HandleGuildPermissionsQuery).
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_GUILD_PERMISSIONS_QUERY));
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_PERMISSIONS);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY)]
    public static void HandleGuildBankRemainingWithdrawnMoneyQuery(in EmptyClientPacket query, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;

        // 4.3.4 HandleGuildBankMoneyWithdrawn: its own request, answered with a 64-bit amount.
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY));
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_GET_ROSTER)]
    public static void HandleGuildGetRoster(in EmptyClientPacket query, in SessionContext ctx)
    {
        // 4.3.4 has no CMSG_GUILD_INFO: its roster carries the creation date and the account count.
        if (!HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_INFO);
            ctx.SendPacketToServer(packet);
        }

        WorldPacket packet2 = new WorldPacket(Opcode.CMSG_GUILD_GET_ROSTER);
        ctx.SendPacketToServer(packet2);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_UPDATE_MOTD_TEXT)]
    public static void HandleGuildUpdateMotdText(in GuildUpdateMotdText text, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_UPDATE_MOTD_TEXT);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            WriteCataText(packet, text.MotdText, 11);   // 4.3.4 HandleGuildMOTDOpcode
        else
            packet.WriteCString(text.MotdText);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_UPDATE_INFO_TEXT)]
    public static void HandleGuildUpdateInfoText(in GuildUpdateInfoText text, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_UPDATE_INFO_TEXT);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            WriteCataText(packet, text.InfoText, 12);   // 4.3.4 HandleGuildChangeInfoTextOpcode
        else
            packet.WriteCString(text.InfoText);
        ctx.SendPacketToServer(packet);
    }

    /// <summary>4.3.4 sends a guild text as a bit-counted length and the bytes, no terminator.</summary>
    private static void WriteCataText(WorldPacket packet, ReadOnlySpan<char> text, int lengthBits)
    {
        string value = text.ToString();
        packet.WriteBits(System.Text.Encoding.UTF8.GetByteCount(value), lengthBits);
        packet.FlushBits();
        packet.WriteString(value);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_SET_MEMBER_NOTE)]
    public static void HandleGuildSetMemberNote(in GuildSetMemberNote note, in SessionContext ctx)
    {
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // 4.3.4 has one opcode for both notes and names the member by GUID
            // (TrinityCore 4.3.4 HandleGuildSetNoteOpcode).
            ulong member = note.NoteeGUID.To64().Low;
            WorldPacket cata = new WorldPacket(Opcode.CMSG_GUILD_SET_NOTE);
            MaskedGuid.WriteMaskBits(cata, member, [1, 4, 5, 3, 0, 7]);
            cata.WriteBit(note.IsPublic);
            MaskedGuid.WriteMaskBits(cata, member, [6]);
            cata.WriteBits(System.Text.Encoding.UTF8.GetByteCount(note.Note), 8);
            MaskedGuid.WriteMaskBits(cata, member, [2]);
            cata.FlushBits();
            MaskedGuid.WriteBytes(cata, member, [4, 5, 0, 3, 1, 6, 7]);
            cata.WriteString(note.Note);
            MaskedGuid.WriteBytes(cata, member, [2]);
            ctx.SendPacketToServer(cata);
            return;
        }

        WorldPacket packet = new WorldPacket(note.IsPublic ? Opcode.CMSG_GUILD_SET_PUBLIC_NOTE : Opcode.CMSG_GUILD_SET_OFFICER_NOTE);
        packet.WriteCString(ctx.GetSession().GameState.GetPlayerName(note.NoteeGUID));
        packet.WriteCString(note.Note);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_PROMOTE_MEMBER)]
    public static void HandleGuildPromoteMember(in GuildPromoteMember promote, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_PROMOTE_MEMBER);
        // 4.3.4 names the member by GUID in all three (TrinityCore 4.3.4 HandleGuildPromoteOpcode,
        // HandleGuildDemoteOpcode, HandleGuildRemoveOpcode), each with its own bit and byte order.
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            MaskedGuid.Write(packet, promote.Promotee.To64().Low, [7, 2, 5, 6, 1, 0, 3, 4], [0, 5, 2, 3, 6, 4, 1, 7]);
        else
            packet.WriteCString(ctx.GetSession().GameState.GetPlayerName(promote.Promotee));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_DEMOTE_MEMBER)]
    public static void HandleGuildDemoteMember(in GuildDemoteMember demote, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DEMOTE_MEMBER);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            MaskedGuid.Write(packet, demote.Demotee.To64().Low, [7, 1, 5, 6, 2, 3, 0, 4], [1, 2, 7, 5, 6, 0, 4, 3]);
        else
            packet.WriteCString(ctx.GetSession().GameState.GetPlayerName(demote.Demotee));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_OFFICER_REMOVE_MEMBER)]
    public static void HandleGuildOfficerRemoveMember(in GuildOfficerRemoveMember remove, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_OFFICER_REMOVE_MEMBER);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            MaskedGuid.Write(packet, remove.Removee.To64().Low, [6, 5, 4, 0, 1, 3, 7, 2], [2, 6, 5, 7, 1, 4, 3, 0]);
        else
            packet.WriteCString(ctx.GetSession().GameState.GetPlayerName(remove.Removee));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_INVITE_BY_NAME)]
    public static void HandleGuildInviteByName(in GuildInviteByName invite, in SessionContext ctx)
    {
        ReadOnlySpan<char> name = LegacyPlayerName.StripRealmSuffix(invite.Name);
        if (invite.ArenaTeamId == 0)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_INVITE_BY_NAME);
            if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
                WriteCataText(packet, name, 7);         // 4.3.4 HandleGuildInviteOpcode
            else
                packet.WriteCString(name);
            ctx.SendPacketToServer(packet);
        }
        else
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ARENA_TEAM_INVITE);
            packet.WriteUInt32(invite.ArenaTeamId);
            packet.WriteCString(name);
            ctx.SendPacketToServer(packet);
        }
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_SET_RANK_PERMISSIONS)]
    public static void HandleGuildSetRankPermissions(in GuildSetRankPermissions rank, in SessionContext ctx)
    {
        // The burst was only observed, and the fix only tested, on the 3.4.3 client.
        if (!ModernVersion.IsWotLKClassicOrLater)
        {
            ctx.SendPacketToServer(BuildLegacyGuildRank(rank));
            return;
        }

        // One Apply in the 3.4.3 guild control panel sends a CMSG_GUILD_SET_RANK_PERMISSIONS per
        // changed setting, all in the same millisecond and each carrying the rank's complete state:
        // a native Wrathion capture shows five for one Apply. A 3.3.5a client sends one
        // CMSG_GUILD_RANK, and AzerothCore kicks after three in one second (antidos_opcode_policies,
        // opcode 561). Only the last of a burst matters, so forward the newest per rank once the
        // burst is over. The copy is what the release captures; the release builds a legacy
        // packet from it and reads no session state, so the timer thread may run it. Issue #283.
        var newest = rank;
        var session = ctx.GetSession();
        ctx.ToServer.Coalesce(
            new HoldKey(HoldKeyKind.GuildRankPermissions, rank.RankID),
            RankPermissionsCoalesceWindow,
            () => session.ToServer.Send(BuildLegacyGuildRank(newest)),
            new HoldOptions(RunOnTimer: true, Scope: OutboxScope.LegacyConnection));
    }

    private static readonly TimeSpan RankPermissionsCoalesceWindow = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Rewrites one rank's complete state as the single 3.3.5a-era CMSG_GUILD_RANK.
    /// </summary>
    /// <remarks>
    /// Also runs from the outbox timer, once per rank at the end of a coalesced burst.
    /// </remarks>
    internal static WorldPacket BuildLegacyGuildRank(GuildSetRankPermissions rank)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_SET_RANK_PERMISSIONS);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleGuildSetRankPermissionsOpcode: old and new rights, eight
            // bank tabs, then the gold limit, the rank's id again and the bit-counted name.
            packet.WriteUInt32(rank.RankID);
            packet.WriteUInt32(rank.OldFlags);
            packet.WriteUInt32(rank.Flags);
            for (var i = 0; i < GuildConst.MaxBankTabsCata; i++)
            {
                packet.WriteUInt32(rank.TabFlags[i]);
                packet.WriteUInt32(rank.TabWithdrawItemLimit[i]);
            }
            packet.WriteInt32(rank.WithdrawGoldLimit);
            packet.WriteUInt32(rank.RankID);
            packet.WriteBits(System.Text.Encoding.UTF8.GetByteCount(rank.RankName), 7);
            packet.FlushBits();
            packet.WriteString(rank.RankName);
            return packet;
        }
        packet.WriteUInt32(rank.RankID);
        packet.WriteUInt32(rank.Flags);
        packet.WriteCString(rank.RankName);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            packet.WriteInt32(rank.WithdrawGoldLimit);
            for (var i = 0; i < 6; i++)
            {
                packet.WriteUInt32(rank.TabFlags[i]);
                packet.WriteUInt32(rank.TabWithdrawItemLimit[i]);
            }
        }
        return packet;
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_ADD_RANK)]
    public static void HandleGuildAddRank(in GuildAddRank rank, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_ADD_RANK);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleGuildAddRankOpcode: the rank's place, then the name.
            packet.WriteUInt32((uint)rank.RankOrder);
            packet.WriteBits(System.Text.Encoding.UTF8.GetByteCount(rank.Name), 7);
            packet.FlushBits();
            packet.WriteString(rank.Name);
        }
        else
            packet.WriteCString(rank.Name);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_DELETE_RANK)]
    public static void HandleGuildDeleteRank(in GuildDeleteRank rank, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DELETE_RANK);
        // 4.3.4 says which rank (HandleGuildDelRankOpcode); the older servers drop the last one.
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            packet.WriteUInt32((uint)rank.RankOrder);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_SET_GUILD_MASTER)]
    public static void HandleGuildSetGuildMaster(in GuildSetGuildMaster master, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_SET_GUILD_MASTER);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 SetGuildMaster::Read: name length, the dethrone flag, the name.
            string newMaster = LegacyPlayerName.StripRealmSuffix(master.NewMasterName).ToString();
            packet.WriteBits(System.Text.Encoding.UTF8.GetByteCount(newMaster), 7);
            packet.WriteBit(false);
            packet.FlushBits();
            packet.WriteString(newMaster);
        }
        else
            packet.WriteCString(LegacyPlayerName.StripRealmSuffix(master.NewMasterName));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_LEAVE)]
    public static void HandleGuildLeave(in EmptyClientPacket leave, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_LEAVE);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ACCEPT_GUILD_INVITE)]
    public static void HandleGuildAccept(in EmptyClientPacket accept, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ACCEPT_GUILD_INVITE);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_DECLINE_INVITATION)]
    public static void HandleGuildDecline(in EmptyClientPacket decline, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DECLINE_INVITATION);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_DELETE)]
    public static void HandleGuildDelete(in EmptyClientPacket delete, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DELETE);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_TABARD_VENDOR_ACTIVATE)]
    public static void HandleTabardVendorActivate(in InteractWithNPC interact, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_TABARDVENDOR_ACTIVATE);
        packet.WriteGuid(interact.CreatureGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SAVE_GUILD_EMBLEM)]
    public static void HandleSaveGuildEmblem(in SaveGuildEmblem emblem, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_SAVE_GUILD_EMBLEM);
        packet.WriteGuid(emblem.DesignerGUID.To64());
        packet.WriteUInt32(emblem.EmblemStyle);
        packet.WriteUInt32(emblem.EmblemColor);
        packet.WriteUInt32(emblem.BorderStyle);
        packet.WriteUInt32(emblem.BorderColor);
        packet.WriteUInt32(emblem.BackgroundColor);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_DECLINE_GUILD_INVITES)]
    public static void HandleDeclineGuildInvites(in SetAutoDeclineGuildInvites packet, in SessionContext ctx)
    {
        var settings = ctx.GetSession().GameState.CurrentPlayerStorage.Settings;
        if (settings == null)
        {
            Log.Print(LogType.Error, "CMSG_DECLINE_GUILD_INVITES received before the player was loaded, ignoring.");
            return;
        }

        settings.SetAutoBlockGuildInvites(packet.GuildInvitesShouldGetBlocked);

        // Send update to client
        ObjectUpdate updateData = new ObjectUpdate(ctx.GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, ctx.GetSession());
        PlayerFlags flags = settings.CreateNewFlags();
        updateData.EnsurePlayerData().PlayerFlags = (uint) flags;
        UpdateObject updatePacket = new UpdateObject(ctx.GetSession().GameState);
        updatePacket.ObjectUpdates.Add(updateData);
        ctx.GetSession().WorldClient!.SendPacketToClient(updatePacket);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_AUTO_DECLINE_INVITATION)]
    public static void HandleGuildAutoDeclineInvitation(in EmptyClientPacket autoDecline, in SessionContext ctx)
    { // This is called when the client still receives a guild invite after enabling AutoDecline
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DECLINE_INVITATION);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_ACTIVATE)]
    public static void HandleGuildBankActivate(in GuildBankAtivate activate, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_ACTIVATE);
        packet.WriteGuid(activate.BankGuid.To64());
        packet.WriteBool(activate.FullUpdate);
        ctx.SendPacketToServer(packet);

        // A client-sent activate subscribes us just the same. V3_4_3 never sends one,
        // but older modern builds do, and it saves a redundant injected activate.
        ctx.GetSession().GameState.GuildBankSubscribed = true;
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_QUERY_TAB)]
    public static void HandleGuildBankQueryTab(in GuildBankQueryTab query, in SessionContext ctx)
    {
        // 3.4.3 often sends FullUpdate=0 when switching tabs. AC _SendBankList
        // only fills ItemInfo when sendAllSlots is true (or a slot set is
        // passed). A false query therefore comes back with items=0 and the
        // vault looks empty even after a successful deposit.
        //
        // The activate is only re-sent when the server has actually unsubscribed us
        // (see GameSessionData.GuildBankSubscribed). Sending it on every query made the
        // server answer with a full tab-0 list that arrived before the reply for the tab
        // the client asked for, dragging the UI back to tab 0 — which made the
        // "Buy new guild bank tab" slot impossible to open. Issue #157.
        // Never spend the activate on the purchase slot: the server answers an activate
        // with a full tab-0 list, which arrives before the reply for the tab the client
        // asked for and drags the UI back to tab 0. That made the "Buy new guild bank tab"
        // window close the moment it opened. Buying a tab makes AzerothCore call
        // SendPermissions (its "hack to force client to update permissions"), which
        // unsubscribes us, so without this the first click after every purchase was eaten.
        var state = ctx.GetSession().GameState;
        bool isPurchaseSlot = state.GuildBankPurchasedTabs > 0
            && query.Tab >= state.GuildBankPurchasedTabs;

        if (!state.GuildBankSubscribed && !isPurchaseSlot)
            SendGuildBankActivate(in ctx, query.BankGuid);

        SendGuildBankQueryTab(in ctx, query.BankGuid, query.Tab);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_DEPOSIT_MONEY)]
    public static void HandleGuildBankDepositMoney(in GuildBankDepositMoney deposit, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_DEPOSIT_MONEY);
        packet.WriteGuid(deposit.BankGuid.To64());
        // 4.3.4 counts bank money in 64 bits (GuildBankDepositMoney::Read, and the withdrawal below).
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            packet.WriteUInt64(deposit.Money);
        else
            packet.WriteUInt32((uint)deposit.Money);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_TEXT_QUERY)]
    public static void HandleGuildBankTextQuery(in GuildBankTextQuery query, in SessionContext ctx)
    {
        // 4.3.4 split the two-way opcode into a request and an answer (GuildBankTextQuery::Read).
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            WorldPacket cata = new WorldPacket(Opcode.CMSG_GUILD_BANK_TEXT_QUERY);
            cata.WriteInt32(query.Tab);
            ctx.SendPacketToServer(cata);
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.MSG_QUERY_GUILD_BANK_TEXT);
        packet.WriteUInt8((byte)query.Tab);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_UPDATE_TAB)]
    public static void HandleGuildBankUpdateTab(in GuildBankUpdateTab update, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_UPDATE_TAB);
        packet.WriteGuid(update.BankGuid.To64());
        packet.WriteUInt8(update.BankTab);
        packet.WriteCString(update.Name);
        packet.WriteCString(update.Icon);
        ctx.SendPacketToServer(packet);
        // Tab list lives on tab 0 FullUpdate. Refresh it so the strip
        // does not wait for a relog after GE_BANK_TAB_UPDATED.
        SendGuildBankQueryTab(in ctx, update.BankGuid, 0);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_LOG_QUERY)]
    public static void HandleGuildBankLogQuery(in GuildBankLogQuery query, in SessionContext ctx)
    {
        // 4.3.4 GuildBankLogQuery::Read: its own request, the tab as an int32.
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            WorldPacket cata = new WorldPacket(Opcode.CMSG_GUILD_BANK_LOG_QUERY);
            cata.WriteInt32(query.Tab);
            ctx.SendPacketToServer(cata);
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_BANK_LOG_QUERY);
        packet.WriteUInt8((byte)query.Tab);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_SET_TAB_TEXT)]
    public static void HandleGuildBankSetTabText(in GuildBankSetTabText query, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SET_TAB_TEXT);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // 4.3.4 GuildBankSetTabText::Read: the tab as an int32, the text counted in 14 bits.
            packet.WriteInt32(query.Tab);
            WriteCataText(packet, query.TabText, 14);
            ctx.SendPacketToServer(packet);
            return;
        }
        packet.WriteUInt8((byte)query.Tab);
        packet.WriteCString(query.TabText);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_BUY_TAB)]
    public static void HandleGuildBankBuyTab(in GuildBankBuyTab buy, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_BUY_TAB);
        packet.WriteGuid(buy.BankGuid.To64());
        packet.WriteUInt8(buy.BankTab);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GUILD_BANK_WITHDRAW_MONEY)]
    public static void HandleGuildBankBuyTab(in GuildBankWithdrawMoney withdraw, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_WITHDRAW_MONEY);
        packet.WriteGuid(withdraw.BankGuid.To64());
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            packet.WriteUInt64(withdraw.Money);
        else
            packet.WriteUInt32((uint)withdraw.Money);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_AUTO_GUILD_BANK_ITEM)]
    [HandlesCmsg(Opcode.CMSG_SWAP_ITEM_WITH_GUILD_BANK_ITEM)]
    public static void HandleGuildBankItem(in AutoGuildBankItem item, in SessionContext ctx)
    {
        // moves an item from the player to the bank
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
        packet.WriteGuid(item.BankGuid.To64());
        packet.WriteBool(false); // bank to bank
        packet.WriteUInt8(item.BankTab);
        packet.WriteUInt8(item.BankSlot);
        packet.WriteUInt32(0); // item id
        packet.WriteBool(false); // auto store
        WritePlayerBagAndSlot(packet, item.ContainerSlot, item.ContainerItemSlot, item.BankTab, item.BankSlot);
        packet.WriteBool(false); // to char
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(0); // splitted amount
        else
            packet.WriteUInt8(0); // splitted amount
        ctx.SendPacketToServer(packet);
        SendGuildBankQueryTab(in ctx, item.BankGuid, item.BankTab);
    }

    [HandlesCmsg(Opcode.CMSG_SPLIT_ITEM_TO_GUILD_BANK)]
    [HandlesCmsg(Opcode.CMSG_MERGE_ITEM_WITH_GUILD_BANK_ITEM)]
    public static void HandleSplitItemToGuildBank(in SplitItemToGuildBank item, in SessionContext ctx)
    {
        // moves a specific amount of stacks from the player to the bank
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
        packet.WriteGuid(item.BankGuid.To64());
        packet.WriteBool(false); // bank to bank
        packet.WriteUInt8(item.BankTab);
        packet.WriteUInt8(item.BankSlot);
        packet.WriteUInt32(0); // item id
        packet.WriteBool(false); // auto store
        WritePlayerBagAndSlot(packet, item.ContainerSlot, item.ContainerItemSlot, item.BankTab, item.BankSlot);
        packet.WriteBool(false); // to char
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(item.StackCount);
        else
            packet.WriteUInt8((byte)item.StackCount);
        ctx.SendPacketToServer(packet);
        SendGuildBankQueryTab(in ctx, item.BankGuid, item.BankTab);
    }

    [HandlesCmsg(Opcode.CMSG_AUTO_STORE_GUILD_BANK_ITEM)]
    public static void HandleAutoStoreGuildBankItem(in AutoStoreGuildBankItem item, in SessionContext ctx)
    {
        // moves an item from the bank to the player
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
        packet.WriteGuid(item.BankGuid.To64());
        packet.WriteBool(false); // bank to bank
        packet.WriteUInt8(item.BankTab);
        packet.WriteUInt8(item.BankSlot);
        packet.WriteUInt32(0); // item id
        packet.WriteBool(true); // auto store
        // A count, the direction, and a second count. From 3.0.2 both counts are 32 bits wide:
        // AzerothCore, TrinityCore and cMaNGOS all read u32, u8, u32. A one-byte second count
        // leaves the packet three bytes short, and the server drops it without a word.
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            packet.WriteUInt32(0); // auto store count
            packet.WriteBool(true); // to char
            packet.WriteUInt32(0); // always 0
        }
        else
        {
            packet.WriteUInt8(0); // auto store count
            packet.WriteBool(true); // to char
            packet.WriteUInt8(0); // always 0
        }
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_STORE_GUILD_BANK_ITEM)]
    public static void HandleStoreGuildBankItem(in AutoGuildBankItem item, in SessionContext ctx)
    {
        // moves an item from the bank to a specific slot in the player inventory
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
        packet.WriteGuid(item.BankGuid.To64());
        packet.WriteBool(false); // bank to bank
        packet.WriteUInt8(item.BankTab);
        packet.WriteUInt8(item.BankSlot);
        packet.WriteUInt32(0); // item id
        packet.WriteBool(false); // auto store
        WritePlayerBagAndSlot(packet, item.ContainerSlot, item.ContainerItemSlot, item.BankTab, item.BankSlot);
        packet.WriteBool(true); // to char
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(0); // splitted amount
        else
            packet.WriteUInt8(0); // splitted amount
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MERGE_GUILD_BANK_ITEM_WITH_ITEM)]
    [HandlesCmsg(Opcode.CMSG_SPLIT_GUILD_BANK_ITEM_TO_INVENTORY)]
    public static void HandleMergeGuildBankItemWithItem(in SplitItemToGuildBank item, in SessionContext ctx)
    {
        // moves a specific amount of stacks from the bank to the player
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
        packet.WriteGuid(item.BankGuid.To64());
        packet.WriteBool(false); // bank to bank
        packet.WriteUInt8(item.BankTab);
        packet.WriteUInt8(item.BankSlot);
        packet.WriteUInt32(0); // item id
        packet.WriteBool(false); // auto store
        WritePlayerBagAndSlot(packet, item.ContainerSlot, item.ContainerItemSlot, item.BankTab, item.BankSlot);
        packet.WriteBool(true); // to char
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(item.StackCount);
        else
            packet.WriteUInt8((byte)item.StackCount);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_GUILD_BANK_ITEM)]
    public static void HandleMoveGuildBankItem(in MoveGuildBankItem item, in SessionContext ctx)
    {
        // moves an item from the bank to the bank
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
        packet.WriteGuid(item.BankGuid.To64());
        packet.WriteBool(true); // bank to bank
        packet.WriteUInt8(item.BankTab2);
        packet.WriteUInt8(item.BankSlot2);
        packet.WriteUInt32(0); // item id
        packet.WriteUInt8(item.BankTab1);
        packet.WriteUInt8(item.BankSlot1);
        packet.WriteUInt32(0); // item id
        packet.WriteBool(false); // auto store
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(0); // splitted amount
        else
            packet.WriteUInt8(0); // splitted amount
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SPLIT_GUILD_BANK_ITEM)]
    [HandlesCmsg(Opcode.CMSG_MERGE_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM)]
    public static void HandleMoveGuildBankItem(in SplitGuildBankItem item, in SessionContext ctx)
    {
        // moves a specific amount of stacks from the bank to the bank
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
        packet.WriteGuid(item.BankGuid.To64());
        packet.WriteBool(true); // bank to bank
        packet.WriteUInt8(item.BankTab2);
        packet.WriteUInt8(item.BankSlot2);
        packet.WriteUInt32(0); // item id
        packet.WriteUInt8(item.BankTab1);
        packet.WriteUInt8(item.BankSlot1);
        packet.WriteUInt32(0); // item id
        packet.WriteBool(false); // auto store
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(item.StackCount);
        else
            packet.WriteUInt8((byte)item.StackCount);
        ctx.SendPacketToServer(packet);
    }

    static void SendGuildBankActivate(in SessionContext ctx, WowGuid128 bankGuid)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_ACTIVATE);
        packet.WriteGuid(bankGuid.To64());
        packet.WriteBool(true);
        ctx.SendPacketToServer(packet);

        // Guild::SendBankTabsInfo subscribes us to bank deltas on the legacy side.
        ctx.GetSession().GameState.GuildBankSubscribed = true;
    }

    static void SendGuildBankQueryTab(in SessionContext ctx, WowGuid128 bankGuid, byte tab)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_QUERY_TAB);
        packet.WriteGuid(bankGuid.To64());
        packet.WriteUInt8(tab);
        packet.WriteBool(true);
        ctx.SendPacketToServer(packet);
    }

    // V3_4_3 CMSG slots are InvSlots descriptor indexes (backpack 35-58),
    // not WotLK 23-38. AdjustInventorySlot only remaps bank/buyback/keyring
    // between Classic/TBC/WotLK and leaves 35 as 35. AC then looks in an
    // empty backpack cell and SwapItemsWithInventory is a silent no-op.
    static void WritePlayerBagAndSlot(WorldPacket packet, byte? containerSlot, byte containerItemSlot, byte bankTab, byte bankSlot)
    {
        byte srcBag;
        byte srcSlot;
        byte legacyBag;
        byte legacySlot;
        if (containerSlot != null)
        {
            srcBag = containerSlot.Value;
            srcSlot = containerItemSlot;
            legacyBag = ModernVersion.AdjustModernInventorySlotToLegacy(srcBag);
            legacySlot = srcSlot;
        }
        else
        {
            srcBag = Enums.Classic.InventorySlots.Bag0;
            srcSlot = containerItemSlot;
            legacyBag = srcBag;
            legacySlot = ModernVersion.AdjustModernInventorySlotToLegacy(srcSlot);
        }

        WorldSocketLogMessages.GuildBankPlayerToBank(
            _melLog, _sourceFile, "C P>S", bankTab, bankSlot, srcBag, legacyBag, srcSlot, legacySlot);

        packet.WriteUInt8(legacyBag);
        packet.WriteUInt8(legacySlot);
    }
    /// <summary>
    /// Asked whenever the guild or the group changes: is this group a guild group? Only a 4.x
    /// server knows the request (TrinityCore 4.3.4: CMSG_GUILD_REQUEST_PARTY_STATE).
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_REQUEST_GUILD_PARTY_STATE)]
    public static void HandleRequestGuildPartyState(in RequestGuildPartyState request, in SessionContext ctx)
    {
        if (!HermesProxy.World.Client.WorldClient.IsCataLegacy)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_GUILD_PARTY_STATE);
        MaskedGuid.Write(packet, CataGuildGuid(request.GuildGuid.GetCounter()), [0, 6, 7, 3, 5, 1, 2, 4], [6, 3, 2, 1, 5, 0, 7, 4]);
        ctx.SendPacketToServer(packet);
    }
    /// <summary>
    /// The guild window asks for the week's challenges. A 4.3.4 server has the request under a
    /// name of its own (CMSG_GUILD_REQUEST_CHALLENGE_UPDATE); both are empty.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_GUILD_CHALLENGE_UPDATE_REQUEST)]
    public static void HandleGuildChallengeUpdateRequest(in EmptyClientPacket request, in SessionContext ctx)
    {
        if (!HermesProxy.World.Client.WorldClient.IsCataLegacy)
            return;

        ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_GUILD_REQUEST_CHALLENGE_UPDATE));
    }
    /// <summary>
    /// The guild achievements the player tracks, sent at login and when the list changes. Only a
    /// 4.x server knows the request (TrinityCore 4.3.4 <c>HandleGuildSetAchievementTracking</c>: a
    /// 24-bit count, then the ids).
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_GUILD_SET_ACHIEVEMENT_TRACKING)]
    public static void HandleGuildSetAchievementTracking(in GuildSetAchievementTracking tracking, in SessionContext ctx)
    {
        if (!HermesProxy.World.Client.WorldClient.IsCataLegacy)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_SET_ACHIEVEMENT_TRACKING);
        packet.WriteBits(tracking.AchievementIds.Length, 24);
        packet.FlushBits();
        foreach (uint achievementId in tracking.AchievementIds)
            packet.WriteUInt32(achievementId);
        ctx.SendPacketToServer(packet);
    }
}
