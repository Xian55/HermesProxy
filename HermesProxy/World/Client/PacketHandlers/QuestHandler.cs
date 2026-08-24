using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS)]
    void HandleQuestGiverQuestDetails(WorldPacket packet)
    {
        QuestGiverQuestDetails quest = new();
        quest.QuestGiverGUID = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = quest.QuestGiverGUID;
        quest.QuestGiverCreatureID = quest.QuestGiverGUID.GetEntry();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            quest.InformUnit = packet.ReadGuid().To128(GetSession().GameState);
        else
            quest.InformUnit = quest.QuestGiverGUID;

        quest.QuestID = packet.ReadUInt32();
        quest.QuestTitle = packet.ReadCString();
        quest.DescriptionText = packet.ReadCString();
        quest.LogDescription = packet.ReadCString();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            quest.AutoLaunched = packet.ReadBool();
        else
            quest.AutoLaunched = packet.ReadUInt32() != 0;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
            quest.QuestFlags[0] = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            quest.SuggestedPartyMembers = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.ReadUInt8(); // Unknown

        if (LegacyVersion.InVersion(ClientVersionBuild.V3_1_0_9767, ClientVersionBuild.V3_3_3a_11723))
        {
            quest.StartCheat = packet.ReadBool();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_2_11403))
                quest.DisplayPopup = packet.ReadBool();
        }

        if (quest.QuestFlags[0].HasAnyFlag(QuestFlags.HiddenRewards) && LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_3_5a_12340))
        {
            packet.ReadUInt32(); // Hidden Chosen Items
            packet.ReadUInt32(); // Hidden Items
            quest.Rewards.Money = packet.ReadUInt32(); // Hidden Money

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
                quest.Rewards.XP = packet.ReadUInt32(); // Hidden XP
        }

        ReadExtraQuestInfo(packet, quest.Rewards, false);

        var emoteCount = packet.ReadUInt32();
        for (var i = 0; i < emoteCount; i++)
        {
            quest.DescEmotes[i].Type = packet.ReadUInt32();
            quest.DescEmotes[i].Delay = packet.ReadUInt32();
        }
        SendPacketToClient(quest);
    }

    void ReadExtraQuestInfo(WorldPacket packet, QuestRewards rewards, bool readFlags)
    {
        rewards.ChoiceItemCount = packet.ReadUInt32();
        for (var i = 0; i < rewards.ChoiceItemCount; i++)
        {
            rewards.ChoiceItems[i].Item.ItemID = packet.ReadUInt32();
            rewards.ChoiceItems[i].Quantity = packet.ReadUInt32();
            packet.ReadUInt32(); // Choice Item Display Id
        }

        var rewardCount = packet.ReadUInt32();
        for (var i = 0; i < rewardCount; i++)
        {
            rewards.ItemID[i] = packet.ReadUInt32();
            rewards.ItemQty[i] = packet.ReadUInt32();
            packet.ReadUInt32(); // Reward Item Display Id
        }

        rewards.Money = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
           rewards.XP = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
            rewards.Honor = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            packet.ReadFloat(); // Honor Multiplier

        if (readFlags)
            packet.ReadUInt32(); // Quest Flags

        rewards.SpellCompletionID = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.ReadUInt32(); // Spell Cast Id

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
            rewards.Title = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            rewards.NumSkillUps = packet.ReadUInt32(); // Bonus Talents

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
        {
            packet.ReadUInt32(); // Arena Points
            packet.ReadUInt32(); // Unk
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
        {
            for (var i = 0; i < 5; i++)
                rewards.FactionID[i] = packet.ReadUInt32(); // Reputation Faction

            for (var i = 0; i < 5; i++)
                rewards.FactionValue[i] = packet.ReadInt32(); // Reputation Value Id

            for (var i = 0; i < 5; i++)
                packet.ReadInt32(); // Reputation Value
        }
    }

    [PacketHandler(Opcode.SMSG_QUEST_GIVER_STATUS)]
    void HandleQuestGiverStatus(WorldPacket packet)
    {
        QuestGiverStatusPkt response = new QuestGiverStatusPkt();
        response.QuestGiver.Guid = packet.ReadGuid().To128(GetSession().GameState);
        byte legacyStatus = packet.ReadUInt8();
        response.QuestGiver.Status = LegacyVersion.ConvertQuestGiverStatus(legacyStatus);
        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            $"[QuestStatusTrace] SMSG_QUEST_GIVER_STATUS recv: GUID={response.QuestGiver.Guid} entry={response.QuestGiver.Guid.GetEntry()} " +
            $"legacyByte={legacyStatus} → modern={response.QuestGiver.Status}");
        SendPacketToClient(response);
    }

    [PacketHandler(Opcode.SMSG_QUEST_GIVER_STATUS_MULTIPLE)]
    void HandleQuestGiverStatusMultple(WorldPacket packet)
    {
        QuestGiverStatusMultiple response = new QuestGiverStatusMultiple();
        int count = packet.ReadInt32();
        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            $"[QuestStatusTrace] SMSG_QUEST_GIVER_STATUS_MULTIPLE recv: count={count}");
        for (int i = 0; i < count; i++)
        {
            QuestGiverInfo info = new();
            info.Guid = packet.ReadGuid().To128(GetSession().GameState);
            byte legacyStatus = packet.ReadUInt8();
            info.Status = LegacyVersion.ConvertQuestGiverStatus(legacyStatus);
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[QuestStatusTrace]   recv[{i}] GUID={info.Guid} entry={info.Guid.GetEntry()} " +
                $"legacyByte={legacyStatus} → modern={info.Status}");
            response.QuestGivers.Add(info);
        }
        SendPacketToClient(response);
    }

    [PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE)]
    void HandleQuestGiverQuestListMessage(WorldPacket packet)
    {
        QuestGiverQuestListMessage quests = new QuestGiverQuestListMessage();
        quests.QuestGiverGUID = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = quests.QuestGiverGUID;
        quests.Greeting = packet.ReadCString();
        quests.GreetEmoteDelay = packet.ReadUInt32();
        quests.GreetEmoteType = packet.ReadUInt32();

        byte count = packet.ReadUInt8();
        for (int i = 0; i < count; i++)
        {
            ClientGossipQuest quest = ReadGossipQuestOption(packet);
            quests.QuestOptions.Add(quest);
        }
        SendPacketToClient(quests);
    }

    ClientGossipQuest ReadGossipQuestOption(WorldPacket packet)
    {
        ClientGossipQuest quest = new();
        quest.QuestID = packet.ReadUInt32();
        // The per-quest gossip icon byte's encoding is backend-version specific:
        //   WotLK+ (V3_0_2+): backend already emits modern GossipQuestIcon values
        //                     directly. TC wotlk_classic `Player.cpp:13572-13598`
        //                     calls AddMenuItem(quest_id, 0|2|4) using exactly the
        //                     {AvailableRepeatable=0, Available=2, Complete=4} set.
        //                     Pass through.
        //   TBC   (V2_0_1..V3_0_2): backend emits QuestGiverStatusTBC ordinal
        //                     (Incomplete=3, AvailableRep=5, Available=6, etc).
        //                     Translate to GossipQuestIcon — verified against
        //                     `hermes-20260505_015835.log:283` where V2_4_3 emitted
        //                     byte 3 for the in-progress quest 179.
        //   Vanilla (pre-V2_0_1): backend emits QuestGiverStatusVanilla ordinal.
        // Note: do NOT route through ConvertQuestGiverStatus here; that converter
        // is for the byte in SMSG_QUEST_GIVER_STATUS (the world-space `?`/`!`
        // over the NPC's head), not the per-quest gossip icon byte.
        int rawIcon = packet.ReadInt32();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            quest.QuestType = rawIcon;
        }
        else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            quest.QuestType = (int)((QuestGiverStatusTBC)rawIcon switch
            {
                QuestGiverStatusTBC.LowLevelAvailable => GossipQuestIcon.Available,
                QuestGiverStatusTBC.Incomplete        => GossipQuestIcon.Complete,
                QuestGiverStatusTBC.RewardRep         => GossipQuestIcon.Complete,
                QuestGiverStatusTBC.AvailableRep      => GossipQuestIcon.Available,
                QuestGiverStatusTBC.Available         => GossipQuestIcon.Available,
                QuestGiverStatusTBC.Reward2           => GossipQuestIcon.Complete,
                QuestGiverStatusTBC.Reward            => GossipQuestIcon.Complete,
                _                                     => GossipQuestIcon.AvailableRepeatable,
            });
        }
        else
        {
            quest.QuestType = (int)((QuestGiverStatusVanilla)rawIcon switch
            {
                QuestGiverStatusVanilla.LowLevelAvailable => GossipQuestIcon.Available,
                QuestGiverStatusVanilla.Available         => GossipQuestIcon.Available,
                QuestGiverStatusVanilla.Incomplete        => GossipQuestIcon.Complete,
                QuestGiverStatusVanilla.RewardRep         => GossipQuestIcon.Complete,
                QuestGiverStatusVanilla.Reward2           => GossipQuestIcon.Complete,
                QuestGiverStatusVanilla.Reward            => GossipQuestIcon.Complete,
                _                                         => GossipQuestIcon.AvailableRepeatable,
            });
        }
        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            $"[QuestStatusTrace] ReadGossipQuestOption: questId={quest.QuestID} legacyIcon={rawIcon} → modernQuestType={quest.QuestType}");

        quest.QuestLevel = packet.ReadInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
            quest.QuestFlags = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
            quest.Repeatable = packet.ReadBool();

        GetSession().GameState.GossipQuestTypesById[quest.QuestID] = quest.QuestType;

        quest.QuestTitle = packet.ReadCString();
        return quest;
    }

    [PacketHandler(Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS)]
    void HandleQuestGiverRequestItems(WorldPacket packet)
    {
        QuestGiverRequestItems quest = new QuestGiverRequestItems();
        quest.QuestGiverGUID = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = quest.QuestGiverGUID;
        quest.QuestGiverCreatureID = quest.QuestGiverGUID.GetEntry();
        quest.QuestID = packet.ReadUInt32();
        quest.QuestTitle = packet.ReadCString();
        quest.CompletionText = packet.ReadCString();
        quest.CompEmoteDelay = packet.ReadUInt32();
        quest.CompEmoteType = packet.ReadUInt32();
        quest.AutoLaunched = packet.ReadUInt32() != 0;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
            quest.QuestFlags[0] = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            quest.SuggestPartyMembers = packet.ReadUInt32();

        quest.MoneyToGet = packet.ReadInt32();

        uint itemsCount = packet.ReadUInt32();
        for (int i = 0; i < itemsCount; i++)
        {
            QuestObjectiveCollect item = new();
            item.ObjectID = packet.ReadUInt32();
            item.Amount = packet.ReadUInt32();
            packet.ReadUInt32(); // Item Display Id
            quest.Collect.Add(item);
        }

        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.ReadUInt32(); // unknown meaning, mangos sends always 2

        // flags
        uint statusFlags = packet.ReadUInt32();
        packet.ReadUInt32(); // Unk flags 2
        packet.ReadUInt32(); // Unk flags 3
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.ReadUInt32(); // Unk flags 4

        // 3.4.3.54261: 223 makes Continue send CMSG (0xFF leaves the button dead).
        bool canComplete = (statusFlags & 3) != 0
            || (GetSession().GameState.GossipQuestTypesById.TryGetValue(quest.QuestID, out int gtype) && gtype == 4);
        quest.StatusFlags = canComplete ? 223u : 219u;

        GetSession().GameState.AwaitingQuestRewardId = quest.QuestID;
        GetSession().GameState.AwaitingQuestGiver = quest.QuestGiverGUID;
        GetSession().GameState.LastRequestItems = quest;
        Framework.Logging.Log.Print(Framework.Logging.LogType.Server,
            $"[RequestItems] quest={quest.QuestID} status=0x{quest.StatusFlags:X} flags=0x{quest.QuestFlags[0]:X} collect={quest.Collect.Count} auto={quest.AutoLaunched}");
        SendPacketToClient(quest);
    }

    [PacketHandler(Opcode.SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE)]
    void HandleQuestGiverOfferRewardMessage(WorldPacket packet)
    {
        QuestGiverOfferRewardMessage quest = new QuestGiverOfferRewardMessage();
        quest.QuestData.QuestGiverGUID = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = quest.QuestData.QuestGiverGUID;
        quest.QuestData.QuestGiverCreatureID = quest.QuestData.QuestGiverGUID.GetEntry();
        quest.QuestData.QuestID = packet.ReadUInt32();
        quest.QuestTitle = packet.ReadCString();
        quest.RewardText = packet.ReadCString();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            quest.QuestData.AutoLaunched = packet.ReadBool();
        else
            quest.QuestData.AutoLaunched = packet.ReadUInt32() != 0;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
            quest.QuestData.QuestFlags[0] = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            quest.QuestData.SuggestedPartyMembers = packet.ReadUInt32();

        uint emotesCount = packet.ReadUInt32();
        for (int i = 0; i < emotesCount; i++)
        {
            QuestDescEmote emote = new();
            emote.Delay = packet.ReadUInt32();
            emote.Type = packet.ReadUInt32();
        }

        ReadExtraQuestInfo(packet, quest.QuestData.Rewards, true);

        // Proactively query the quest template if it isn't cached. The proxy-side
        // CHOOSE_REWARD handler needs `QuestTemplate.UnfilteredChoiceItems` to map
        // the modern client's ItemID back to the legacy choice index. Without
        // this query the user's first reward click fails with QUEST_FAILED until
        // they retry (after the cache populates from a manual query).
        if (GameData.GetQuestTemplate(quest.QuestData.QuestID) == null)
        {
            WorldPacket queryQuest = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
            queryQuest.WriteUInt32(quest.QuestData.QuestID);
            SendPacketToServer(queryQuest);
        }

        GetSession().GameState.JustSentOfferReward = true;
        GetSession().GameState.LastRequestItems = null;
        GetSession().GameState.AwaitingQuestRewardId = 0;
        SendPacketToClient(quest);
    }

    [PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_COMPLETE)]
    void HandleQuestGiverQuestComplete(WorldPacket packet)
    {
        QuestGiverQuestComplete quest = new QuestGiverQuestComplete();
        quest.QuestID = packet.ReadUInt32();

        GetSession().GameState.CurrentPlayerStorage.CompletedQuests.MarkQuestAsCompleted(quest.QuestID);
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.ReadUInt32(); // mangos sends always 3

        quest.XPReward = packet.ReadUInt32();
        quest.MoneyReward = packet.ReadInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
            packet.ReadInt32(); // Honor

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            packet.ReadInt32(); // Talents
            packet.ReadInt32(); // Arena Points
        }

        uint itemId = 0;
        uint itemCount = 0;
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            uint itemsCount = packet.ReadUInt32();
            for (uint i = 0; i < itemsCount; ++i)
            {
                uint itemId2 = packet.ReadUInt32();
                uint itemCount2 = packet.ReadUInt32();

                if (itemId2 != 0 && itemCount2 != 0)
                {
                    itemId = itemId2;
                    itemCount = itemCount2;
                }
            }
        }

        quest.ItemReward.ItemID = itemId;
        QuestTemplate? questTemplate = GameData.GetQuestTemplate((uint)quest.QuestID);
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            // 3.4.3 keeps the completion window up when LaunchQuest is set with no
            // follow-up quest. Other builds relied on the default-true field.
            quest.LaunchQuest = questTemplate != null && questTemplate.RewardNextQuest != 0;
        }
        else if (questTemplate != null && questTemplate.RewardNextQuest == 0)
        {
            quest.LaunchQuest = false;
        }

        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261
            && !quest.LaunchQuest
            && GetSession().GameState.CurrentInteractedWithNPC != default)
        {
            uint npcFlags = GetSession().GameState.GetLegacyFieldValueUInt32(GetSession().GameState.CurrentInteractedWithNPC, UnitField.UNIT_NPC_FLAGS);
            if (npcFlags.HasAnyFlag(NPCFlags.Gossip))
                quest.LaunchGossip = true;
        }

        SendPacketToClient(quest);
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            SendPacketToClient(new GossipComplete());

        DisplayToast toast = new();
        toast.QuestID = quest.QuestID;
        if (itemId != 0 && itemCount != 0)
        {
            toast.Quantity = 1;
            toast.Type = 0;
            toast.ItemReward.ItemID = itemId;
        }
        else
        {
            toast.Quantity = 60;
            toast.Type = 2;
        }
        SendPacketToClient(toast);
    }

    [PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_FAILED)]
    void HandleQuestGiverQuestFailed(WorldPacket packet)
    {
        QuestGiverQuestFailed quest = new QuestGiverQuestFailed();
        quest.QuestID = packet.ReadUInt32();
        quest.Reason = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
        SendPacketToClient(quest);
    }

    [PacketHandler(Opcode.SMSG_QUEST_GIVER_INVALID_QUEST)]
    void HandleQuestGiverInvalidQuest(WorldPacket packet)
    {
        QuestGiverInvalidQuest quest = new QuestGiverInvalidQuest();
        quest.Reason = (QuestFailedReasons)packet.ReadUInt32();
        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            $"[QuestInvalidTrace] Reason={quest.Reason} ({(uint)quest.Reason})");
        SendPacketToClient(quest);
    }

    [PacketHandler(Opcode.SMSG_QUEST_UPDATE_COMPLETE)]
    [PacketHandler(Opcode.SMSG_QUEST_UPDATE_FAILED)]
    [PacketHandler(Opcode.SMSG_QUEST_UPDATE_FAILED_TIMER)]
    void HandleQuestUpdateStatus(WorldPacket packet)
    {
        QuestUpdateStatus quest = new QuestUpdateStatus(packet.GetUniversalOpcode(false));
        quest.QuestID = packet.ReadUInt32();
        SendPacketToClient(quest);
    }

    [PacketHandler(Opcode.SMSG_QUEST_UPDATE_ADD_ITEM)]
    void HandleQuestUpdateAddItem(WorldPacket packet)
    {
        // 3.3.5a client counts quest items from bags. AzerothCore even sends this
        // packet empty. 3.4.3 needs SMSG_QUEST_UPDATE_ADD_CREDIT (type Item).
        if (packet.CanRead())
        {
            uint itemId = packet.ReadUInt32();
            if (packet.CanRead())
                packet.ReadUInt32(); // count; inventory total is authoritative
            SendItemQuestCredit(itemId);
            return;
        }

        ResyncItemQuestCredits();
    }

    // Credits a single looted item across every in-log quest that wants it. The
    // count always comes from the bags, so this is also correct when the same item
    // id satisfies two quests at once.
    void SendItemQuestCredit(uint itemId)
    {
        if (itemId == 0)
            return;

        uint have = GetSession().GameState.GetItemCountInInventory(itemId);
        bool sent = false;

        foreach (int questId in GetSession().GameState.QuestLogQuestIDs)
        {
            if (questId == 0)
                continue;
            QuestTemplate? template = GameData.GetQuestTemplate((uint)questId);
            if (template == null)
                continue;
            foreach (QuestObjective obj in template.Objectives)
            {
                if (obj.Type != QuestObjectiveType.Item || obj.ObjectID != (int)itemId)
                    continue;
                SendItemCredit((uint)questId, itemId, have, (ushort)Math.Max(obj.Amount, 1));
                sent = true;
            }
        }

        if (!sent)
            RequestMissingQuestTemplates();
    }

    void SendItemCredit(uint questId, uint itemId, uint have, ushort required)
    {
        ushort count = (ushort)Math.Min(have, required);
        var key = (questId, itemId);
        var sentCredits = GetSession().GameState.SentItemQuestCredits;
        if (sentCredits.TryGetValue(key, out ushort last) && last == count)
            return;
        sentCredits[key] = count;

        Framework.Logging.Log.Print(Framework.Logging.LogType.Server,
            $"[QuestItemCredit] quest={questId} item={itemId} have={have}/{required}");
        SendPacketToClient(new QuestUpdateAddCredit
        {
            QuestID = questId,
            ObjectID = (int)itemId,
            Count = count,
            Required = required,
            ObjectiveType = QuestObjectiveType.Item,
            VictimGUID = WowGuid128.Empty,
        });
    }

    // Drives every item objective of every in-log quest off one bag snapshot, so a
    // count that dropped (sold, destroyed, traded) is pushed down as well as up.
    // SendItemCredit swallows unchanged values, so calling this often is cheap.
    void ResyncItemQuestCredits()
    {
        Dictionary<uint, uint>? counts = null;
        foreach (int questId in GetSession().GameState.QuestLogQuestIDs)
        {
            if (questId == 0)
                continue;
            QuestTemplate? template = GameData.GetQuestTemplate((uint)questId);
            if (template == null)
                continue;
            foreach (QuestObjective obj in template.Objectives)
            {
                if (obj.Type != QuestObjectiveType.Item)
                    continue;
                counts ??= GetSession().GameState.GetInventoryItemCounts();
                counts.TryGetValue((uint)obj.ObjectID, out uint have);
                SendItemCredit((uint)questId, (uint)obj.ObjectID, have, (ushort)Math.Max(obj.Amount, 1));
            }
        }
    }

    void RequestMissingQuestTemplates()
    {
        var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(GetSession().GameState.CurrentPlayerGuid);
        if (updateFields == null)
            return;
        int questsCount = LegacyVersion.GetQuestLogSize();
        for (int i = 0; i < questsCount; i++)
        {
            QuestLog? logEntry = ReadQuestLogEntry(i, null, updateFields);
            if (logEntry?.QuestID == null)
                continue;
            if (GameData.GetQuestTemplate((uint)logEntry.QuestID) != null)
                continue;
            // One query per quest id. Without this, every looted trash item replays
            // the whole log's missing-template queries.
            if (!GetSession().GameState.RequestedQuestTemplateIds.Add((uint)logEntry.QuestID))
                continue;
            WorldPacket query = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
            query.WriteUInt32((uint)logEntry.QuestID);
            SendPacketToServer(query);
        }
    }

    [PacketHandler(Opcode.SMSG_QUEST_UPDATE_ADD_KILL)]
    void HandleQuestUpdateAddKill(WorldPacket packet)
    {
        QuestUpdateAddCredit credit = new QuestUpdateAddCredit();
        credit.QuestID = packet.ReadUInt32();
        var entry = packet.ReadEntry();
        credit.ObjectID = entry.Key;
        credit.ObjectiveType = entry.Value ? QuestObjectiveType.GameObject : QuestObjectiveType.Monster;
        credit.Count = (ushort)packet.ReadUInt32();
        credit.Required = (ushort)packet.ReadUInt32();
        credit.VictimGUID = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(credit);
    }

    [PacketHandler(Opcode.SMSG_QUEST_CONFIRM_ACCEPT)]
    void HandleQuestConfirmAccept(WorldPacket packet)
    {
        QuestConfirmAccept quest = new QuestConfirmAccept();
        quest.QuestID = packet.ReadUInt32();
        quest.QuestTitle = packet.ReadCString();
        quest.InitiatedBy = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(quest);
    }

    [PacketHandler(Opcode.MSG_QUEST_PUSH_RESULT)]
    void HandleQuestPushResult(WorldPacket packet)
    {
        QuestPushResult quest = new QuestPushResult();
        quest.SenderGUID = packet.ReadGuid().To128(GetSession().GameState);
        quest.Result = (QuestPushReason)packet.ReadUInt8();
        SendPacketToClient(quest);
    }

    [PacketHandler(Opcode.SMSG_QUEST_POI_QUERY_RESPONSE)]
    void HandleQuestPOIQueryResponse(WorldPacket packet)
    {
        // Legacy 3.3.5a wire layout (mangos-wotlk QueryHandler.cpp:526):
        //   uint32 count
        //   per quest:
        //     uint32 questID, uint32 blobs.Count
        //     per blob:
        //       uint32 BlobIndex, int32 ObjectiveIndex, uint32 MapID,
        //       uint32 WorldMapAreaID, uint32 FloorID, uint32 Unk3, uint32 Unk4,
        //       uint32 points.Count
        //       per point: int32 X, int32 Y
        //
        // Field mapping (verified against HermesProxy-WOTLK fork — the community version known
        // to render the V3_4_3.54261 "Default Quest Helper" blue polygons):
        //   legacy WorldMapAreaID → modern UiMapID (NO retail-style lookup translation —
        //     the V3_4_3 client preserves legacy WMA IDs; transforming via UiMap.db2 lookup
        //     breaks rendering)
        //   legacy FloorID        → modern Flags (NOT Priority — fork comment "floorId in legacy")
        //   legacy Unk3/Unk4      → discarded
        // QuestObjectiveID synthesized from the cached QuestTemplate's objective IDs so the
        // client can associate the blob with a specific objective row in its tracker.
        QuestPOIQueryResponse response = new();
        uint count = packet.ReadUInt32();
        for (uint i = 0; i < count; i++)
        {
            QuestPOIData data = new();
            data.QuestID = (int)packet.ReadUInt32();
            uint blobCount = packet.ReadUInt32();
            QuestTemplate? template = GameData.GetQuestTemplate((uint)data.QuestID);
            for (uint b = 0; b < blobCount; b++)
            {
                QuestPOIBlobData blob = new();
                blob.BlobIndex = (int)packet.ReadUInt32();
                blob.ObjectiveIndex = packet.ReadInt32();
                blob.MapID = (int)packet.ReadUInt32();
                int legacyWMA = (int)packet.ReadUInt32();
                blob.UiMapID = legacyWMA;
                packet.ReadUInt32(); // FloorID
                packet.ReadUInt32(); // Unk3 / Priority
                packet.ReadUInt32(); // Unk4 / Flags

                bool isV343 = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;

                // V3_4_3 Classic keeps WorldMapArea IDs in this slot (Teldrassil 41
                // renders; Cata UiMap 57 / Classic 1438 do not). Do not GetUiMapId.

                if (template != null && blob.ObjectiveIndex >= 0)
                    BindLegacyPoiObjective(blob, template);

                // Legacy 3.3.5a stored FloorID in this field (typically 0) which the
                // V3_4_3 client rejects as a no-render flag. Rewrite with the native
                // V3_4_3 convention: objective markers get ShowOnMap + ObjectiveNumber,
                // pure location markers (ObjectiveIndex=-1) get ShowOnMap only.
                if (isV343)
                {
                    QuestPOIFlags flags = QuestPOIFlags.ShowOnMap;
                    if (blob.ObjectiveIndex >= 0 && blob.QuestObjectiveID != 0)
                        flags |= QuestPOIFlags.ObjectiveNumber;
                    blob.Flags = (int)flags;
                }
                uint pointsCount = packet.ReadUInt32();
                for (uint p = 0; p < pointsCount; p++)
                {
                    QuestPOIBlobPoint point = new()
                    {
                        X = (short)packet.ReadInt32(),
                        Y = (short)packet.ReadInt32(),
                        Z = 0
                    };
                    blob.Points.Add(point);
                }
                Framework.Logging.Log.Print(Framework.Logging.LogType.Debug,
                    $"[QuestPOI] quest={data.QuestID} blob={blob.BlobIndex} objIdx={blob.ObjectiveIndex} " +
                    $"map={blob.MapID} wma={legacyWMA} uiMap={blob.UiMapID} flags={blob.Flags} " +
                    $"objId={blob.QuestObjectiveID} points={pointsCount}");
                data.Blobs.Add(blob);
            }
            response.QuestPOIDataStats.Add(data);
        }
        SendPacketToClient(response);
        ResyncItemQuestCredits();
    }

    static void BindLegacyPoiObjective(QuestPOIBlobData blob, QuestTemplate template)
    {
        QuestObjective? match = null;

        // Legacy blobs address the raw template column (0-3 creature/GO, 4+N items).
        // Quest 179 stores its meat at column 4 while StorageIndex is 0.
        foreach (QuestObjective objective in template.Objectives)
        {
            if (objective.LegacyPoiIndex >= 0 && objective.LegacyPoiIndex == blob.ObjectiveIndex)
            {
                match = objective;
                break;
            }
        }

        if (match == null)
        {
            foreach (QuestObjective objective in template.Objectives)
            {
                if (objective.StorageIndex == blob.ObjectiveIndex)
                {
                    match = objective;
                    break;
                }
            }
        }

        if (match == null && template.Objectives.Count == 1)
            match = template.Objectives[0];

        if (match == null)
            return;

        blob.ObjectiveIndex = match.StorageIndex;
        blob.QuestObjectiveID = (int)match.Id;
        blob.QuestObjectID = match.ObjectID;
    }
}
