using Framework.Logging;
using HermesProxy.Enums;
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

    [PacketHandler(Opcode.SMSG_LFG_DISABLED)]
    void HandleLFGDisabled(WorldPacket packet)
    {
        SendPacketToClient(new LFGDisabled());
    }

    [PacketHandler(Opcode.SMSG_LFG_OFFER_CONTINUE)]
    void HandleLFGOfferContinue(WorldPacket packet)
    {
        LFGOfferContinue offer = new LFGOfferContinue();
        offer.Slot = packet.ReadUInt32();
        SendPacketToClient(offer);
    }

    [PacketHandler(Opcode.SMSG_LFG_JOIN_RESULT)]
    void HandleLFGJoinResult(WorldPacket packet)
    {
        DFJoinResult result = new DFJoinResult();
        result.Ticket = MakeLfgTicket();
        // The legacy and V3_4_3 LfgJoinResult enums do not share numbering, so forwarding the
        // raw byte made every rejection invisible in the modern UI.
        byte legacyResult = (byte)packet.ReadUInt32();    // joinData.result
        result.Result = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
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
                    slot.Reason = packet.ReadUInt32();
                    bl.Slots.Add(slot);
                }
                result.BlackList.Add(bl);
            }
        }
        SendPacketToClient(result);
    }

    [PacketHandler(Opcode.SMSG_LFG_UPDATE_PLAYER)]
    void HandleLFGUpdatePlayer(WorldPacket packet)
    {
        WriteUpdateStatus(packet, isParty: false);
    }

    [PacketHandler(Opcode.SMSG_LFG_UPDATE_PARTY)]
    void HandleLFGUpdateParty(WorldPacket packet)
    {
        WriteUpdateStatus(packet, isParty: true);
    }

    private void WriteUpdateStatus(WorldPacket packet, bool isParty)
    {
        DFUpdateStatus status = new DFUpdateStatus();
        status.Ticket = MakeLfgTicket();
        status.IsParty = isParty;

        byte legacyUpdateType = packet.ReadUInt8();
        bool isV343 = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;

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
        SendPacketToClient(status);
    }

    [PacketHandler(Opcode.SMSG_LFG_TELEPORT_DENIED)]
    void HandleLFGTeleportDenied(WorldPacket packet)
    {
        // Legacy sends the reason as a uint32, V3_4_3 wants it in 4 bits; the enum values line
        // up, so this is a straight forward. Previously unhandled, which meant a refused
        // teleport produced no message whatsoever on the client.
        LFGTeleportDenied denied = new LFGTeleportDenied();
        denied.Reason = (byte)packet.ReadUInt32();
        Log.Print(LogType.Debug, $"LFG[diag]: SMSG_LFG_TELEPORT_DENIED reason={denied.Reason}");
        SendPacketToClient(denied);
    }

    [PacketHandler(Opcode.SMSG_LFG_QUEUE_STATUS)]
    void HandleLFGQueueStatus(WorldPacket packet)
    {
        DFQueueStatus status = new DFQueueStatus();
        status.Ticket = MakeLfgTicket();
        status.Slot = packet.ReadUInt32();
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

    [PacketHandler(Opcode.SMSG_LFG_PROPOSAL_UPDATE)]
    void HandleLFGProposalUpdate(WorldPacket packet)
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

    [PacketHandler(Opcode.SMSG_LFG_ROLE_CHECK_UPDATE)]
    void HandleLFGRoleCheckUpdate(WorldPacket packet)
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

    [PacketHandler(Opcode.SMSG_LFG_PARTY_INFO)]
    void HandleLFGPartyInfo(WorldPacket packet)
    {
        LFGPartyInfo info = new LFGPartyInfo();
        byte playerCount = packet.ReadUInt8();
        for (byte i = 0; i < playerCount; i++)
        {
            LFGBlackListEntry entry = new LFGBlackListEntry();
            entry.PlayerGuid = packet.ReadGuid().To128(GetSession().GameState);
            uint lockCount = packet.ReadUInt32();
            for (uint j = 0; j < lockCount; j++)
            {
                LFGLockInfoData li = new LFGLockInfoData();
                li.Slot = packet.ReadUInt32();
                li.LockStatus = packet.ReadUInt32();
                entry.Locks.Add(li);
                GetSession().GameState.RememberLfgSlot(li.Slot);
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

    [PacketHandler(Opcode.SMSG_LFG_PLAYER_INFO)]
    void HandleLFGPlayerInfo(WorldPacket packet)
    {
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
            slot.Reason = packet.ReadUInt32();
            // Map legacy LFGLockStatus → modern LFGSoftLock per TC
            // wotlk_classic LFGMgr.cpp:1807-1823.
            slot.SoftLock = (uint)((LFGLockStatus)slot.Reason switch
            {
                LFGLockStatus.InsufficientExpansion
                or LFGLockStatus.TooLowLevel
                or LFGLockStatus.TooHighLevel
                or LFGLockStatus.NotInSeason => LFGSoftLock.Unk2,
                _ => LFGSoftLock.None,
            });
            blackList.Slots.Add(slot);
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
            Log.Print(LogType.Debug,
                $"LFG[diag]: SMSG_LFG_PLAYER_INFO sent, Dungeons={info.Dungeons.Count} BlackListSlots={info.BlackList.Slots?.Count ?? 0} " +
                $"offeredSlots=[{string.Join(", ", info.Dungeons.ConvertAll(x => x.Slot.ToString()))}] (one-shot)");
        }
    }

    [PacketHandler(Opcode.SMSG_LFG_PLAYER_REWARD)]
    void HandleLFGPlayerReward(WorldPacket packet)
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
