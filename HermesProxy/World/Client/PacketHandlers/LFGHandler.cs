using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Legacy server -> Modern client (LFG / Dungeon Finder).
    // Legacy 3.3.5a is the only target backend (LFG added 3.3.0).

    // One-shot diagnostic flags — log the first PLAYER_INFO + PARTY_INFO field
    // dump per session, then stay quiet. Used while wire layout was being
    // verified against the 3.4.3_54261 reference sniff.
    private bool _lfgPlayerInfoLogged;
    private bool _lfgPartyInfoLogged;

    private RideTicket MakeLfgTicket()
    {
        // Must stay stable for the lifetime of the queue — see GetOrCreateLfgTicket. Minting a
        // fresh Time per packet gave every LFG message a different ticket.
        return GetSession().GameState.GetOrCreateLfgTicket(
            GetSession().GameState.CurrentPlayerGuid,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    [HandlesSmsg(Opcode.SMSG_LFG_DISABLED)]
    internal void HandleLFGDisabled(WorldPacket packet)
    {
        SendPacketToClient(new LFGDisabled());
    }

    [HandlesSmsg(Opcode.SMSG_LFG_OFFER_CONTINUE)]
    internal void HandleLFGOfferContinue(WorldPacket packet)
    {
        LFGOfferContinue offer = new LFGOfferContinue();
        offer.Slot = packet.ReadUInt32();
        SendPacketToClient(offer);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_JOIN_RESULT)]
    internal void HandleLFGJoinResult(WorldPacket packet)
    {
        DFJoinResult result = new DFJoinResult();
        result.Ticket = MakeLfgTicket();
        // The legacy and V3_4_3 LfgJoinResult enums do not share numbering, so forwarding the
        // raw byte made every rejection invisible in the modern UI.
        byte legacyResult = (byte)packet.ReadUInt32();    // joinData.result
        result.Result = ModernVersion.IsWotLKClassicOrLater
            ? LfgJoinResults.ToModern(legacyResult)
            : legacyResult;
        result.ResultDetail = (byte)packet.ReadUInt32();  // joinData.state
        if (packet.CanRead())
        {
            byte partySize = packet.ReadUInt8();
            for (int p = 0; p < partySize; p++)
            {
                DFJoinBlackList bl = new DFJoinBlackList();
                bl.PlayerGuid = packet.ReadGuid().To128(GetSession().GameState);
                uint dungeonCount = packet.ReadUInt32();
                for (uint d = 0; d < dungeonCount; d++)
                {
                    DFJoinBlackListSlot slot = new DFJoinBlackListSlot();
                    slot.Slot = packet.ReadUInt32();
                    slot.Reason = LfgSlots.ToDisplayLockStatus(packet.ReadUInt32());
                    bl.Slots.Add(slot);
                }
                result.BlackList.Add(bl);
            }
        }
        SendPacketToClient(result);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_UPDATE_PLAYER)]
    internal void HandleLFGUpdatePlayer(WorldPacket packet)
    {
        WriteUpdateStatus(packet, isParty: false);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_UPDATE_PARTY)]
    internal void HandleLFGUpdateParty(WorldPacket packet)
    {
        WriteUpdateStatus(packet, isParty: true);
    }

    private void WriteUpdateStatus(WorldPacket packet, bool isParty)
    {
        DFUpdateStatus status = new DFUpdateStatus();
        status.Ticket = MakeLfgTicket();
        status.IsParty = isParty;
        status.RequestedRoles = GetSession().GameState.LfgRequestedRoles;

        byte legacyUpdateType = packet.ReadUInt8();
        bool isV343 = ModernVersion.IsWotLKClassicOrLater;

        // TC 3.4.3 puts a constant queue type in SubType and the update type in Reason
        // (Handlers/LFGHandler.cpp SendLfgUpdateStatus). Forwarding the legacy update type as
        // SubType left Reason at 0, so the client saw every update as LFG_UPDATETYPE_DEFAULT.
        byte modernUpdateType = isV343 ? LfgUpdateTypes.ToModern(legacyUpdateType) : legacyUpdateType;
        status.SubType = isV343 ? LfgUpdateTypes.ModernQueueDungeon : legacyUpdateType;
        status.Reason = isV343 ? modernUpdateType : (byte)0;

        // TC sets NotifyUI unconditionally; gating it on the legacy extra-info block meant a
        // bare REMOVED_FROM_QUEUE (which carries no dungeons) never refreshed the panel.
        status.NotifyUI = true;
        status.LfgJoined = !isV343 || LfgUpdateTypes.IsStillLfgJoined(modernUpdateType);

        bool hasExtraInfo = packet.ReadUInt8() != 0;
        if (hasExtraInfo)
        {
            // SMSG_LFG_UPDATE_PARTY is NOT the same shape as SMSG_LFG_UPDATE_PLAYER: it carries
            // a leading "join" byte and three extra bytes before the dungeon count. Reading the
            // player layout for both landed dungeonCount on an always-zero filler byte, so party
            // updates never carried a single slot. Confirmed identical in azerothcore-wotlk
            // (Handlers/LFGHandler.cpp SendLfgUpdateParty) and cMaNGOS (BuildLfgUpdate).
            if (isParty)
                status.Joined = packet.ReadUInt8() != 0; // LFG Join

            status.Queued = packet.ReadUInt8() != 0;
            packet.ReadUInt8(); // NoPartialClear
            packet.ReadUInt8(); // Achievements

            if (isParty)
            {
                packet.ReadUInt8(); // Needs[0]
                packet.ReadUInt8(); // Needs[1]
                packet.ReadUInt8(); // Needs[2]
            }

            byte dungeonCount = packet.ReadUInt8();
            for (int i = 0; i < dungeonCount; i++)
                status.Slots.Add(GetSession().GameState.GetLfgSlotForDungeon(packet.ReadUInt32()));
            packet.ReadCString(); // comment — unused in modern

            // The player variant has no "join" byte; extra info at all means the player is
            // attached to a queue, which is what legacy signals by sending the block.
            if (!isParty)
                status.Joined = true;
        }

        if (status.Slots.Count > 0)
            GetSession().GameState.LfgQueuedSlots = new List<uint>(status.Slots);
        else if (!status.LfgJoined)
            GetSession().GameState.LfgQueuedSlots = [];

        if (Log.IsDebugEnabled)
            Log.Print(LogType.Debug,
            $"LFG[diag]: DFUpdateStatus(player/party) subType={status.SubType} reason={status.Reason} " +
            $"requestedRoles=0x{status.RequestedRoles:X2} slots=[{string.Join(", ", status.Slots)}] " +
            $"isParty={status.IsParty} joined={status.Joined} lfgJoined={status.LfgJoined} queued={status.Queued}");

        SendPacketToClient(status);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_TELEPORT_DENIED)]
    internal void HandleLFGTeleportDenied(WorldPacket packet)
    {
        // Legacy sends the reason as a uint32, V3_4_3 wants it in 4 bits; the enum values line
        // up, so this is a straight forward. Previously unhandled, which meant a refused
        // teleport produced no message whatsoever on the client.
        LFGTeleportDenied denied = new LFGTeleportDenied();
        denied.Reason = (byte)packet.ReadUInt32();
        Log.Print(LogType.Debug, $"LFG[diag]: SMSG_LFG_TELEPORT_DENIED reason={denied.Reason}");
        SendPacketToClient(denied);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_QUEUE_STATUS)]
    internal void HandleLFGQueueStatus(WorldPacket packet)
    {
        DFQueueStatus status = new DFQueueStatus();
        status.Ticket = MakeLfgTicket();
        status.Slot = ToQueueStatusSlot(packet.ReadUInt32());
        status.AvgWaitTime = (uint)packet.ReadInt32();
        status.AvgWaitTimeMe = (uint)packet.ReadInt32();
        status.AvgWaitTimeByRole[0] = (uint)packet.ReadInt32(); // Tank
        status.AvgWaitTimeByRole[1] = (uint)packet.ReadInt32(); // Healer
        status.AvgWaitTimeByRole[2] = (uint)packet.ReadInt32(); // DPS
        status.LastNeeded[0] = packet.ReadUInt8();
        status.LastNeeded[1] = packet.ReadUInt8();
        status.LastNeeded[2] = packet.ReadUInt8();
        status.QueuedTime = packet.ReadUInt32();
        SendPacketToClient(status);
    }

    /// <summary>
    /// The slot a queue status reports, which has to be one the client queued for.
    /// </summary>
    /// <remarks>
    /// The client's queue tooltip (QueueStatusFrame.lua QueueStatusEntry_SetUpLFG) takes its
    /// active queue from the queue status and then reads the roles (GetLFGInfoServer) and the
    /// role counts (GetLFGQueueStats) under that dungeon. A 3.3.5a server reports the first
    /// dungeon a random queue expanded to (dungeon 205 while queued for Random Lich King Heroic,
    /// 262), so both lookups missed and the tooltip showed nothing but the time in queue.
    /// </remarks>
    private uint ToQueueStatusSlot(uint dungeonId)
    {
        uint slot = GetSession().GameState.GetLfgSlotForDungeon(dungeonId);
        var queued = GetSession().GameState.LfgQueuedSlots;
        if (queued.Count > 0 && !queued.Contains(slot))
            return queued[0];
        // Not in the server's lists: a dungeon the random queue expanded to, so a plain dungeon.
        return (slot & 0xFF000000) != 0 ? slot : LfgSlots.PackSlot(LfgSlots.LfgTypeDungeon, slot);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_PROPOSAL_UPDATE)]
    internal void HandleLFGProposalUpdate(WorldPacket packet)
    {
        DFProposalUpdate prop = new DFProposalUpdate();
        prop.Ticket = MakeLfgTicket();
        uint dungeonEntry = packet.ReadUInt32();
        prop.Slot = dungeonEntry;
        prop.State = (sbyte)packet.ReadUInt8();
        prop.ProposalID = packet.ReadUInt32();
        prop.CompletedMask = packet.ReadUInt32();
        bool silent = packet.ReadUInt8() != 0;
        prop.ProposalSilent = silent;
        byte playerCount = packet.ReadUInt8();
        for (int i = 0; i < playerCount; i++)
        {
            DFProposalPlayer player = new DFProposalPlayer();
            player.Roles = (byte)packet.ReadUInt32();
            player.Me = packet.ReadUInt8() != 0;
            bool inDungeon = packet.ReadUInt8() != 0;
            bool sameGroup = packet.ReadUInt8() != 0;
            player.SameParty = sameGroup;
            player.MyParty = inDungeon;
            player.Responded = packet.ReadUInt8() != 0;
            player.Accepted = packet.ReadUInt8() != 0;
            prop.Players.Add(player);
        }
        SendPacketToClient(prop);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_ROLE_CHECK_UPDATE)]
    internal void HandleLFGRoleCheckUpdate(WorldPacket packet)
    {
        LFGRoleCheckUpdate roleCheck = new LFGRoleCheckUpdate();
        roleCheck.PartyIndex = 0;
        roleCheck.RoleCheckStatus = (byte)packet.ReadUInt32(); // state
        roleCheck.IsBeginning = packet.ReadBool();
        roleCheck.IsRequeue = false;
        roleCheck.GroupFinderActivityID = 0;
        byte dungeonCount = packet.ReadUInt8();
        for (byte i = 0; i < dungeonCount; i++)
            roleCheck.JoinSlots.Add(packet.ReadUInt32());
        byte memberCount = packet.ReadUInt8();
        for (byte i = 0; i < memberCount; i++)
        {
            LFGRoleCheckMember m = new LFGRoleCheckMember();
            m.Guid = packet.ReadGuid().To128(GetSession().GameState);
            bool ready = packet.ReadBool();
            m.RolesDesired = packet.ReadUInt32();
            m.Level = packet.ReadUInt8();
            m.RoleCheckComplete = ready;
            roleCheck.Members.Add(m);
        }
        SendPacketToClient(roleCheck);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_ROLE_CHOSEN)]
    internal void HandleLFGRoleChosen(WorldPacket packet)
    {
        var chosen = new RoleChosen();
        chosen.Player = packet.ReadGuid().To128(GetSession().GameState);
        chosen.Accepted = packet.ReadUInt8() != 0;
        chosen.RoleMask = (byte)packet.ReadUInt32();
        SendPacketToClient(chosen);
    }

    [HandlesSmsg(Opcode.SMSG_LFG_PARTY_INFO)]
    internal void HandleLFGPartyInfo(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleLFGPartyInfoCata(packet);
            return;
        }

        LFGPartyInfo info = new LFGPartyInfo();
        byte playerCount = packet.ReadUInt8();
        for (byte i = 0; i < playerCount; i++)
        {
            LFGBlackListEntry entry = new LFGBlackListEntry();
            entry.PlayerGuid = packet.ReadGuid().To128(GetSession().GameState);
            uint lockCount = packet.ReadUInt32();
            var alreadyListed = new HashSet<uint>((int)lockCount);
            for (uint j = 0; j < lockCount; j++)
            {
                LFGLockInfoData li = new LFGLockInfoData();
                li.Slot = packet.ReadUInt32();
                li.LockStatus = LfgSlots.ToDisplayLockStatus(packet.ReadUInt32());
                li.SoftLock = LfgSlots.ToPartySoftLock(li.LockStatus);
                entry.Locks.Add(li);
                alreadyListed.Add(LfgSlots.GetDungeonId(li.Slot));
                GetSession().GameState.RememberLfgSlot(li.Slot);
            }
            // Party Type dropdown reads this list, not PLAYER_INFO. Same
            // hide-rows as solo or Titan Rune / out-of-range randoms stay greyed.
            foreach (uint packed in LfgSlots.GetTitanRuneHideSlots(alreadyListed))
            {
                entry.Locks.Add(new LFGLockInfoData
                {
                    Slot = packed,
                    LockStatus = (uint)LFGLockStatus.NotInSeason,
                    SoftLock = (uint)LFGSoftLock.Unk2,
                });
            }
            info.Players.Add(entry);
        }
        SendPacketToClient(info);

        if (!_lfgPartyInfoLogged)
        {
            _lfgPartyInfoLogged = true;
            int totalLocks = 0;
            foreach (var p in info.Players)
                totalLocks += p.Locks.Count;
            Log.Print(LogType.Debug,
                $"LFG[diag]: SMSG_LFG_PARTY_INFO sent — Players={info.Players.Count} TotalLocks={totalLocks} (one-shot)");
        }
    }

    // Counts past these are a misread, not a dungeon list.
    private const int MaxLfgEntries = 1024;

    /// <summary>
    /// TrinityCore 4.3.4 <c>LFGPartyInfo::Write</c>: the locks of each other member, by a plain
    /// GUID, four numbers a lock (the dungeon, the reason, the item level needed and had).
    /// </summary>
    internal void HandleLFGPartyInfoCata(WorldPacket packet)
    {
        LFGPartyInfo info = new LFGPartyInfo();
        byte playerCount = packet.ReadUInt8();
        for (int i = 0; i < playerCount; i++)
        {
            LFGBlackListEntry entry = new LFGBlackListEntry();
            entry.PlayerGuid = packet.ReadGuid().To128(GetSession().GameState);
            uint lockCount = packet.ReadUInt32();
            if (lockCount > MaxLfgEntries)
                return;
            var alreadyListed = new HashSet<uint>((int)lockCount);
            for (uint j = 0; j < lockCount; j++)
            {
                LFGLockInfoData lockInfo = new LFGLockInfoData();
                lockInfo.Slot = packet.ReadUInt32();
                lockInfo.LockStatus = LfgSlots.ToDisplayLockStatus(packet.ReadUInt32());
                lockInfo.SubReason1 = packet.ReadUInt32();
                lockInfo.SubReason2 = packet.ReadUInt32();
                lockInfo.SoftLock = LfgSlots.ToPartySoftLock(lockInfo.LockStatus);
                entry.Locks.Add(lockInfo);
                alreadyListed.Add(LfgSlots.GetDungeonId(lockInfo.Slot));
                GetSession().GameState.RememberLfgSlot(lockInfo.Slot);
            }
            // the party's dungeon list reads these, not the player's (as on 3.4.3)
            foreach (uint packed in LfgSlots.GetCataHideSlots(alreadyListed))
                entry.Locks.Add(new LFGLockInfoData { Slot = packed, LockStatus = (uint)LFGLockStatus.NotInSeason, SoftLock = (uint)LFGSoftLock.Unk2 });
            info.Players.Add(entry);
        }
        SendPacketToClient(info);
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>LFGPlayerInfo::Write</c>: the dungeons the player may queue for, each
    /// with what it pays and how often it may be had, a shortage reward per role and the rewards
    /// themselves; then the locked dungeons, four numbers each.
    /// </summary>
    /// <remarks>
    /// 3.3.5a sends a dungeon's slot, a done flag and the rewards only; 4.3.4 sends the numbers a
    /// modern client wants, so they go on as they came.
    /// </remarks>
    internal void HandleLFGPlayerInfoCata(WorldPacket packet)
    {
        LFGPlayerInfoPkt info = new LFGPlayerInfoPkt();
        var session = GetSession().GameState;

        byte dungeonCount = packet.ReadUInt8();
        for (int i = 0; i < dungeonCount; i++)
        {
            LFGPlayerDungeonInfo dungeon = new LFGPlayerDungeonInfo();
            dungeon.Slot = packet.ReadUInt32();
            dungeon.FirstReward = packet.ReadUInt8() != 0;
            dungeon.CompletionQuantity = packet.ReadInt32();
            dungeon.CompletionLimit = packet.ReadInt32();
            dungeon.CompletionCurrencyID = packet.ReadInt32();
            dungeon.SpecificQuantity = packet.ReadInt32();
            dungeon.SpecificLimit = packet.ReadInt32();
            dungeon.OverallQuantity = packet.ReadInt32();
            dungeon.OverallLimit = packet.ReadInt32();
            dungeon.PurseWeeklyQuantity = packet.ReadInt32();
            dungeon.PurseWeeklyLimit = packet.ReadInt32();
            dungeon.PurseQuantity = packet.ReadInt32();
            dungeon.PurseLimit = packet.ReadInt32();
            dungeon.Quantity = packet.ReadInt32();
            dungeon.CompletedMask = packet.ReadUInt32();

            // A role mask and a reward for each of three roles when a role is short, else the
            // masks alone. The modern packet's shortage rewards are not filled.
            bool shortageEligible = packet.ReadUInt8() != 0;
            for (int role = 0; role < 3; role++)
            {
                packet.ReadUInt32();
                if (shortageEligible)
                    ReadLfgRewardCata(packet);
            }

            dungeon.Rewards = ReadLfgRewardCata(packet);
            info.Dungeons.Add(dungeon);
        }

        LFGBlackList blackList = new LFGBlackList();
        blackList.Slots = new List<LFGBlackListSlot>();
        uint lockCount = packet.ReadUInt32();
        if (lockCount > MaxLfgEntries)
            return;
        for (uint i = 0; i < lockCount; i++)
        {
            LFGBlackListSlot slot = new LFGBlackListSlot();
            slot.Slot = packet.ReadUInt32();
            slot.Reason = LfgSlots.ToDisplayLockStatus(packet.ReadUInt32());
            slot.SubReason1 = packet.ReadInt32();
            slot.SubReason2 = packet.ReadInt32();
            slot.SoftLock = LfgSlots.ToSoftLock(slot.Reason);
            blackList.Slots.Add(slot);
        }

        // The client's own dungeons the server does not have would show as open and queue
        // into nothing; listed as out of season with the soft lock that hides a row.
        var alreadyListed = new HashSet<uint>(blackList.Slots.Count + info.Dungeons.Count);
        foreach (var existing in blackList.Slots)
            alreadyListed.Add(LfgSlots.GetDungeonId(existing.Slot));
        foreach (var dungeon in info.Dungeons)
            alreadyListed.Add(LfgSlots.GetDungeonId(dungeon.Slot));
        foreach (uint packed in LfgSlots.GetCataHideSlots(alreadyListed))
            blackList.Slots.Add(new LFGBlackListSlot { Slot = packed, Reason = (uint)LFGLockStatus.NotInSeason, SoftLock = (uint)LFGSoftLock.Unk2 });
        info.BlackList = blackList;

        var known = session.LfgKnownDungeonIds;
        foreach (var dungeon in info.Dungeons)
        {
            known.Add(dungeon.Slot & 0xFFFFFF);
            session.RememberLfgSlot(dungeon.Slot);
        }
        foreach (var locked in blackList.Slots)
        {
            known.Add(locked.Slot & 0xFFFFFF);
            session.RememberLfgSlot(locked.Slot);
        }
        SendPacketToClient(info);
    }

    /// <summary>
    /// A 4.3.4 dungeon reward: money, experience, then items and currencies in one list, each
    /// marked by its last byte (1 a currency, 0 an item).
    /// </summary>
    private static LFGPlayerQuestReward ReadLfgRewardCata(WorldPacket packet)
    {
        LFGPlayerQuestReward rewards = new LFGPlayerQuestReward();
        rewards.Items = new List<LFGPlayerQuestRewardItem>();
        rewards.Currency = new List<LFGPlayerQuestRewardCurrency>();
        rewards.BonusCurrency = new List<LFGPlayerQuestRewardCurrency>();
        rewards.RewardMoney = packet.ReadInt32();
        rewards.RewardXP = packet.ReadInt32();
        byte count = packet.ReadUInt8();
        for (int i = 0; i < count; i++)
        {
            int id = packet.ReadInt32();
            packet.ReadInt32();     // an item's display, 0 for a currency
            int quantity = packet.ReadInt32();
            bool isCurrency = packet.ReadUInt8() != 0;
            if (isCurrency)
                rewards.Currency.Add(new LFGPlayerQuestRewardCurrency { CurrencyID = id, Quantity = quantity });
            else
                rewards.Items.Add(new LFGPlayerQuestRewardItem { ItemID = id, Quantity = quantity });
        }
        return rewards;
    }

    [HandlesSmsg(Opcode.SMSG_LFG_PLAYER_INFO)]
    internal void HandleLFGPlayerInfo(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleLFGPlayerInfoCata(packet);
            return;
        }

        LFGPlayerInfoPkt info = new LFGPlayerInfoPkt();

        // Random / available dungeons
        byte randomCount = packet.ReadUInt8();
        for (int i = 0; i < randomCount; i++)
        {
            LFGPlayerDungeonInfo d = new LFGPlayerDungeonInfo();
            d.Slot = packet.ReadUInt32();
            bool isDone = packet.ReadUInt8() != 0;
            d.FirstReward = !isDone;
            // Reward-eligibility "Limit" + "Quantity" fields. Wrathion 3.4.3
            // reference sniff (World_solo_dungeon_finder_queue_parsed.txt:
            // 62404-62416) populates these as 1; modern V3_4_3 client treats
            // 0 as "no daily allowance left" and silently disables the Queue
            // button (no CMSG_DF_JOIN emitted on click). Mirror Wrathion.
            d.CompletionQuantity = isDone ? 1 : 0;
            d.CompletionLimit = 1;
            d.SpecificLimit = 1;
            d.OverallLimit = 1;
            d.Quantity = 1;

            LFGPlayerQuestReward rewards = new LFGPlayerQuestReward();
            rewards.Items = new List<LFGPlayerQuestRewardItem>();
            rewards.Currency = new List<LFGPlayerQuestRewardCurrency>();
            rewards.BonusCurrency = new List<LFGPlayerQuestRewardCurrency>();
            rewards.RewardMoney = (int)packet.ReadUInt32();
            rewards.RewardXP = (int)packet.ReadUInt32();
            packet.ReadUInt32(); // unknown
            packet.ReadUInt32(); // unknown
            byte itemCount = packet.ReadUInt8();
            for (int j = 0; j < itemCount; j++)
            {
                LFGPlayerQuestRewardItem item = new LFGPlayerQuestRewardItem();
                item.ItemID = (int)packet.ReadUInt32();
                packet.ReadUInt32(); // displayInfo
                item.Quantity = (int)packet.ReadUInt32();
                rewards.Items.Add(item);
            }
            d.Rewards = rewards;
            info.Dungeons.Add(d);
        }

        // Locked dungeons (blacklist)
        LFGBlackList blackList = new LFGBlackList();
        blackList.Slots = new List<LFGBlackListSlot>();
        uint lockCount = packet.ReadUInt32();
        for (uint i = 0; i < lockCount; i++)
        {
            LFGBlackListSlot slot = new LFGBlackListSlot();
            slot.Slot = packet.ReadUInt32();
            slot.Reason = LfgSlots.ToDisplayLockStatus(packet.ReadUInt32());
            // Map legacy LFGLockStatus → modern LFGSoftLock per TC
            // wotlk_classic LFGMgr.cpp:1807-1823.
            slot.SoftLock = LfgSlots.ToSoftLock(slot.Reason);
            blackList.Slots.Add(slot);
        }

        // Titan Rune Alpha/Beta/Gamma live only in the 3.4.3 LFGDungeons.db2.
        // SoftLock=Unk2 hides a row; without these the three categories stay
        // visible and every click on them is InvalidSlot.
        var alreadyListed = new HashSet<uint>(blackList.Slots.Count);
        foreach (var existing in blackList.Slots)
            alreadyListed.Add(LfgSlots.GetDungeonId(existing.Slot));
        foreach (uint packed in LfgSlots.GetTitanRuneHideSlots(alreadyListed))
        {
            blackList.Slots.Add(new LFGBlackListSlot
            {
                Slot = packed,
                Reason = (uint)LFGLockStatus.NotInSeason,
                SoftLock = (uint)LFGSoftLock.Unk2,
            });
        }

        info.BlackList = blackList;

        // Both lists together are the full set of dungeons this backend understands.
        var known = GetSession().GameState.LfgKnownDungeonIds;
        // These are the only packets that spell out a dungeon's type byte, so remember it:
        // the UPDATE_PLAYER / UPDATE_PARTY packets carry bare dungeon IDs and the modern
        // client wants the full slot back.
        var session = GetSession().GameState;
        foreach (var dungeon in info.Dungeons)
        {
            known.Add(dungeon.Slot & 0xFFFFFF);
            session.RememberLfgSlot(dungeon.Slot);
        }
        foreach (var locked in blackList.Slots)
        {
            known.Add(locked.Slot & 0xFFFFFF);
            session.RememberLfgSlot(locked.Slot);
        }

        SendPacketToClient(info);

        if (!_lfgPlayerInfoLogged)
        {
            _lfgPlayerInfoLogged = true;
            if (Log.IsDebugEnabled)
                Log.Print(LogType.Debug,
                $"LFG[diag]: SMSG_LFG_PLAYER_INFO sent, Dungeons={info.Dungeons.Count} BlackListSlots={info.BlackList.Slots?.Count ?? 0} " +
                $"offeredSlots=[{string.Join(", ", info.Dungeons.ConvertAll(x => x.Slot.ToString()))}] (one-shot)");
        }
    }

    [HandlesSmsg(Opcode.SMSG_LFG_PLAYER_REWARD)]
    internal void HandleLFGPlayerReward(WorldPacket packet)
    {
        LFGPlayerReward reward = new LFGPlayerReward();
        reward.QueuedSlot = packet.ReadUInt32();        // rdungeonEntry
        reward.ActualSlot = packet.ReadUInt32();        // sdungeonEntry
        packet.ReadUInt8();                              // done
        packet.ReadUInt32();                             // always 1
        reward.RewardMoney = (int)packet.ReadUInt32();
        reward.AddedXP = (int)packet.ReadUInt32();
        packet.ReadUInt32();                             // unknown
        packet.ReadUInt32();                             // unknown
        byte itemNum = packet.ReadUInt8();
        for (byte i = 0; i < itemNum; i++)
        {
            LFGPlayerRewardItem item = new LFGPlayerRewardItem();
            item.ItemID = packet.ReadUInt32();
            packet.ReadUInt32(); // displayId
            item.Quantity = packet.ReadUInt32();
            item.IsCurrency = false;
            item.BonusCurrency = 0;
            reward.Rewards.Add(item);
        }
        SendPacketToClient(reward);
    }
}
