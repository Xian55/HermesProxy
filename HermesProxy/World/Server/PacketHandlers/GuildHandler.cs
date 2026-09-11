using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client
    [PacketHandler(Opcode.CMSG_QUERY_GUILD_INFO)]
    void HandleQueryGuildInfo(QueryGuildInfo query)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_GUILD_INFO);
        packet.WriteUInt32((uint)query.GuildGuid.GetCounter());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_PERMISSIONS_QUERY)]
    void HandleGuildPermissionsQuery(GuildPermissionsQuery query)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;

        WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_PERMISSIONS);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY)]
    void HandleGuildBankRemainingWithdrawnMoneyQuery(GuildBankRemainingWithdrawMoneyQuery query)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;

        WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_GET_ROSTER)]
    void HandleGuildGetRoster(GuildGetRoster query)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_INFO);
        SendPacketToServer(packet);

        WorldPacket packet2 = new WorldPacket(Opcode.CMSG_GUILD_GET_ROSTER);
        SendPacketToServer(packet2);
    }

    [PacketHandler(Opcode.CMSG_GUILD_UPDATE_MOTD_TEXT)]
    void HandleGuildUpdateMotdText(GuildUpdateMotdText text)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_UPDATE_MOTD_TEXT);
        packet.WriteCString(text.MotdText);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_UPDATE_INFO_TEXT)]
    void HandleGuildUpdateInfoText(GuildUpdateInfoText text)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_UPDATE_INFO_TEXT);
        packet.WriteCString(text.InfoText);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_SET_MEMBER_NOTE)]
    void HandleGuildSetMemberNote(GuildSetMemberNote note)
    {
        WorldPacket packet = new WorldPacket(note.IsPublic ? Opcode.CMSG_GUILD_SET_PUBLIC_NOTE : Opcode.CMSG_GUILD_SET_OFFICER_NOTE);
        packet.WriteCString(GetSession().GameState.GetPlayerName(note.NoteeGUID));
        packet.WriteCString(note.Note);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_PROMOTE_MEMBER)]
    void HandleGuildPromoteMember(GuildPromoteMember promote)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_PROMOTE_MEMBER);
        packet.WriteCString(GetSession().GameState.GetPlayerName(promote.Promotee));
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_DEMOTE_MEMBER)]
    void HandleGuildDemoteMember(GuildDemoteMember demote)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DEMOTE_MEMBER);
        packet.WriteCString(GetSession().GameState.GetPlayerName(demote.Demotee));
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_OFFICER_REMOVE_MEMBER)]
    void HandleGuildOfficerRemoveMember(GuildOfficerRemoveMember remove)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_OFFICER_REMOVE_MEMBER);
        packet.WriteCString(GetSession().GameState.GetPlayerName(remove.Removee));
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_INVITE_BY_NAME)]
    void HandleGuildInviteByName(GuildInviteByName invite)
    {
        if (invite.ArenaTeamId == 0)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_INVITE_BY_NAME);
            packet.WriteCString(invite.Name);
            SendPacketToServer(packet);
        }
        else
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ARENA_TEAM_INVITE);
            packet.WriteUInt32(invite.ArenaTeamId);
            packet.WriteCString(invite.Name);
            SendPacketToServer(packet);
        }
    }

    // One Apply in the 3.4.3 guild control panel sends a CMSG_GUILD_SET_RANK_PERMISSIONS per
    // changed setting, all in the same millisecond and each carrying the rank's complete state:
    // a native Wrathion capture shows five for one Apply. A 3.3.5a client sends one
    // CMSG_GUILD_RANK, and AzerothCore kicks after three in one second (antidos_opcode_policies,
    // opcode 561). Only the last of a burst matters, so hold them briefly and forward the newest
    // per rank. Issue #283.
    private const int RankPermissionsCoalesceMs = 100;
    private readonly LatestPerKeyCoalescer<uint, GuildSetRankPermissions> _pendingRankPermissions = new();
    // Armed on the packet thread and disposed from it, the timer callback or OnClose, so every
    // swap goes through Interlocked.
    private System.Threading.Timer? _rankPermissionsTimer;

    [PacketHandler(Opcode.CMSG_GUILD_SET_RANK_PERMISSIONS)]
    void HandleGuildSetRankPermissions(GuildSetRankPermissions rank)
    {
        // The burst was only observed, and the fix only tested, on the 3.4.3 client.
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
        {
            SendPacketToServer(BuildLegacyGuildRank(rank));
            return;
        }

        if (_pendingRankPermissions.Offer(rank.RankID, rank))
        {
            var timer = new System.Threading.Timer(OnRankPermissionsDue, null, RankPermissionsCoalesceMs, System.Threading.Timeout.Infinite);
            System.Threading.Interlocked.Exchange(ref _rankPermissionsTimer, timer)?.Dispose();
        }
    }

    private void OnRankPermissionsDue(object? state)
    {
        System.Threading.Interlocked.Exchange(ref _rankPermissionsTimer, null)?.Dispose();
        FlushRankPermissions();
    }

    private void FlushRankPermissions()
    {
        // Runs on a timer thread or from OnClose. An exception escaping a timer callback has no
        // handler above it and would take the whole proxy down.
        try
        {
            var ranks = _pendingRankPermissions.Drain(out int received);
            if (ranks.Count == 0)
                return;

            WorldSocketLogMessages.GuildRankPermissionsCoalesced(_melLog, _sourceFile, _netDirRecv, received, ranks.Count);
            foreach (var rank in ranks)
                SendPacketToServer(BuildLegacyGuildRank(rank));
        }
        catch (Exception ex)
        {
            WorldSocketLogMessages.GuildRankPermissionsFlushFailed(_melLog, _sourceFile, _netDirRecv, ex);
        }
    }

    internal static WorldPacket BuildLegacyGuildRank(GuildSetRankPermissions rank)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_SET_RANK_PERMISSIONS);
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

    [PacketHandler(Opcode.CMSG_GUILD_ADD_RANK)]
    void HandleGuildAddRank(GuildAddRank rank)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_ADD_RANK);
        packet.WriteCString(rank.Name);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_DELETE_RANK)]
    void HandleGuildDeleteRank(GuildDeleteRank rank)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DELETE_RANK);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_SET_GUILD_MASTER)]
    void HandleGuildSetGuildMaster(GuildSetGuildMaster master)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_SET_GUILD_MASTER);
        packet.WriteCString(master.NewMasterName);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_LEAVE)]
    void HandleGuildLeave(GuildLeave leave)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_LEAVE);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_ACCEPT_GUILD_INVITE)]
    void HandleGuildAccept(AcceptGuildInvite accept)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ACCEPT_GUILD_INVITE);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_DECLINE_INVITATION)]
    void HandleGuildDecline(DeclineGuildInvite decline)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DECLINE_INVITATION);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_DELETE)]
    void HandleGuildDelete(GuildDelete delete)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DELETE);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_TABARD_VENDOR_ACTIVATE)]
    void HandleTabardVendorActivate(InteractWithNPC interact)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_TABARDVENDOR_ACTIVATE);
        packet.WriteGuid(interact.CreatureGUID.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SAVE_GUILD_EMBLEM)]
    void HandleSaveGuildEmblem(SaveGuildEmblem emblem)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_SAVE_GUILD_EMBLEM);
        packet.WriteGuid(emblem.DesignerGUID.To64());
        packet.WriteUInt32(emblem.EmblemStyle);
        packet.WriteUInt32(emblem.EmblemColor);
        packet.WriteUInt32(emblem.BorderStyle);
        packet.WriteUInt32(emblem.BorderColor);
        packet.WriteUInt32(emblem.BackgroundColor);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_DECLINE_GUILD_INVITES)]
    void HandleDeclineGuildInvites(SetAutoDeclineGuildInvites packet)
    {
        var settings = GetSession().GameState.CurrentPlayerStorage.Settings;
        if (settings == null)
        {
            Log.Print(LogType.Error, "CMSG_DECLINE_GUILD_INVITES received before the player was loaded, ignoring.");
            return;
        }

        settings.SetAutoBlockGuildInvites(packet.GuildInvitesShouldGetBlocked);

        // Send update to client
        ObjectUpdate updateData = new ObjectUpdate(GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
        PlayerFlags flags = settings.CreateNewFlags();
        updateData.PlayerData.PlayerFlags = (uint) flags;
        UpdateObject updatePacket = new UpdateObject(GetSession().GameState);
        updatePacket.ObjectUpdates.Add(updateData);
        GetSession().WorldClient!.SendPacketToClient(updatePacket);
    }

    [PacketHandler(Opcode.CMSG_GUILD_AUTO_DECLINE_INVITATION)]
    void HandleGuildAutoDeclineInvitation(AutoDeclineGuildInvite autoDecline)
    { // This is called when the client still receives a guild invite after enabling AutoDecline
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DECLINE_INVITATION);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_ACTIVATE)]
    void HandleGuildBankActivate(GuildBankAtivate activate)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_ACTIVATE);
        packet.WriteGuid(activate.BankGuid.To64());
        packet.WriteBool(activate.FullUpdate);
        SendPacketToServer(packet);

        // A client-sent activate subscribes us just the same. V3_4_3 never sends one,
        // but older modern builds do, and it saves a redundant injected activate.
        GetSession().GameState.GuildBankSubscribed = true;
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_QUERY_TAB)]
    void HandleGuildBankQueryTab(GuildBankQueryTab query)
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
        var state = GetSession().GameState;
        bool isPurchaseSlot = state.GuildBankPurchasedTabs > 0
            && query.Tab >= state.GuildBankPurchasedTabs;

        if (!state.GuildBankSubscribed && !isPurchaseSlot)
            SendGuildBankActivate(query.BankGuid);

        SendGuildBankQueryTab(query.BankGuid, query.Tab);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_DEPOSIT_MONEY)]
    void HandleGuildBankDepositMoney(GuildBankDepositMoney deposit)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_DEPOSIT_MONEY);
        packet.WriteGuid(deposit.BankGuid.To64());
        packet.WriteUInt32((uint)deposit.Money);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_TEXT_QUERY)]
    void HandleGuildBankTextQuery(GuildBankTextQuery query)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_QUERY_GUILD_BANK_TEXT);
        packet.WriteUInt8((byte)query.Tab);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_UPDATE_TAB)]
    void HandleGuildBankUpdateTab(GuildBankUpdateTab update)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_UPDATE_TAB);
        packet.WriteGuid(update.BankGuid.To64());
        packet.WriteUInt8(update.BankTab);
        packet.WriteCString(update.Name);
        packet.WriteCString(update.Icon);
        SendPacketToServer(packet);
        // Tab list lives on tab 0 FullUpdate. Refresh it so the strip
        // does not wait for a relog after GE_BANK_TAB_UPDATED.
        SendGuildBankQueryTab(update.BankGuid, 0);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_LOG_QUERY)]
    void HandleGuildBankLogQuery(GuildBankLogQuery query)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_BANK_LOG_QUERY);
        packet.WriteUInt8((byte)query.Tab);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_SET_TAB_TEXT)]
    void HandleGuildBankSetTabText(GuildBankSetTabText query)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SET_TAB_TEXT);
        packet.WriteUInt8((byte)query.Tab);
        packet.WriteCString(query.TabText);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_BUY_TAB)]
    void HandleGuildBankBuyTab(GuildBankBuyTab buy)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_BUY_TAB);
        packet.WriteGuid(buy.BankGuid.To64());
        packet.WriteUInt8(buy.BankTab);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_GUILD_BANK_WITHDRAW_MONEY)]
    void HandleGuildBankBuyTab(GuildBankWithdrawMoney withdraw)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_WITHDRAW_MONEY);
        packet.WriteGuid(withdraw.BankGuid.To64());
        packet.WriteUInt32((uint)withdraw.Money);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_AUTO_GUILD_BANK_ITEM)]
    [PacketHandler(Opcode.CMSG_SWAP_ITEM_WITH_GUILD_BANK_ITEM)]
    void HandleGuildBankItem(AutoGuildBankItem item)
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
        SendPacketToServer(packet);
        SendGuildBankQueryTab(item.BankGuid, item.BankTab);
    }

    [PacketHandler(Opcode.CMSG_SPLIT_ITEM_TO_GUILD_BANK)]
    [PacketHandler(Opcode.CMSG_MERGE_ITEM_WITH_GUILD_BANK_ITEM)]
    void HandleSplitItemToGuildBank(SplitItemToGuildBank item)
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
        SendPacketToServer(packet);
        SendGuildBankQueryTab(item.BankGuid, item.BankTab);
    }

    [PacketHandler(Opcode.CMSG_AUTO_STORE_GUILD_BANK_ITEM)]
    void HandleAutoStoreGuildBankItem(AutoStoreGuildBankItem item)
    {
        // moves an item from the bank to the player
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
        packet.WriteGuid(item.BankGuid.To64());
        packet.WriteBool(false); // bank to bank
        packet.WriteUInt8(item.BankTab);
        packet.WriteUInt8(item.BankSlot);
        packet.WriteUInt32(0); // item id
        packet.WriteBool(true); // auto store
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(0); // auto store count
        else
            packet.WriteUInt8(0); // auto store count
        packet.WriteBool(true); // to char
        packet.WriteUInt8(0); // unknown
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_STORE_GUILD_BANK_ITEM)]
    void HandleStoreGuildBankItem(AutoGuildBankItem item)
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
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_MERGE_GUILD_BANK_ITEM_WITH_ITEM)]
    [PacketHandler(Opcode.CMSG_SPLIT_GUILD_BANK_ITEM_TO_INVENTORY)]
    void HandleMergeGuildBankItemWithItem(SplitItemToGuildBank item)
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
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_MOVE_GUILD_BANK_ITEM)]
    void HandleMoveGuildBankItem(MoveGuildBankItem item)
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
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SPLIT_GUILD_BANK_ITEM)]
    [PacketHandler(Opcode.CMSG_MERGE_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM)]
    void HandleMoveGuildBankItem(SplitGuildBankItem item)
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
        SendPacketToServer(packet);
    }

    void SendGuildBankActivate(WowGuid128 bankGuid)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_ACTIVATE);
        packet.WriteGuid(bankGuid.To64());
        packet.WriteBool(true);
        SendPacketToServer(packet);

        // Guild::SendBankTabsInfo subscribes us to bank deltas on the legacy side.
        GetSession().GameState.GuildBankSubscribed = true;
    }

    void SendGuildBankQueryTab(WowGuid128 bankGuid, byte tab)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_QUERY_TAB);
        packet.WriteGuid(bankGuid.To64());
        packet.WriteUInt8(tab);
        packet.WriteBool(true);
        SendPacketToServer(packet);
    }

    // V3_4_3 CMSG slots are InvSlots descriptor indexes (backpack 35-58),
    // not WotLK 23-38. AdjustInventorySlot only remaps bank/buyback/keyring
    // between Classic/TBC/WotLK and leaves 35 as 35. AC then looks in an
    // empty backpack cell and SwapItemsWithInventory is a silent no-op.
    void WritePlayerBagAndSlot(WorldPacket packet, byte? containerSlot, byte containerItemSlot, byte bankTab, byte bankSlot)
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
}
