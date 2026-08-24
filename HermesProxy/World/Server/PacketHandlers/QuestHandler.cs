using Framework.Constants;
using Framework.Logging;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_QUERY_QUEST)]
    void HandleQuestGiverQueryQuest(QuestGiverQueryQuest quest)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_QUERY_QUEST);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V2_0_1_6180))
            packet.WriteBool(quest.RespondToGiver);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST)]
    void HandleQuestGiverAcceptQuest(QuestGiverAcceptQuest quest)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V3_1_2_9901))
            packet.WriteInt32(quest.StartCheat ? 1 : 0);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_LOG_REMOVE_QUEST)]
    void HandleQuestLogRemoveQuest(QuestLogRemoveQuest quest)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_LOG_REMOVE_QUEST);
        packet.WriteUInt8(quest.Slot);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY)]
    void HandleQuestGiverStatusQuery(QuestGiverStatusQuery query)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY);
        packet.WriteGuid(query.QuestGiverGUID.To64());
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY)]
    void HandleQuestGiverStatusMultipleQuery(QuestGiverStatusMultipleQuery query)
    {
        if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY);
            SendPacketToServer(packet);
        }
        else
        {
            int UNIT_NPC_FLAGS = ModernVersion.GetUpdateField(UnitField.UNIT_NPC_FLAGS);
            if (UNIT_NPC_FLAGS < 0)
                return;

            List<WowGuid128> npcGuids = new List<WowGuid128>();
            lock (GetSession().GameState.ObjectCacheLock)
            {
                foreach (var obj in GetSession().GameState.ObjectCacheModern)
                {
                    if (obj.Key.GetObjectType() == ObjectType.Unit &&
                        obj.Value.GetUpdateField<uint>(UNIT_NPC_FLAGS).HasAnyFlag(NPCFlags.QuestGiver))
                        npcGuids.Add(obj.Key);
                }
            }

            foreach (var guid in npcGuids)
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY);
                packet.WriteGuid(guid.To64());
                SendPacketToServer(packet);
            }
        }
    }
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_HELLO)]
    void HandleQuestGiverHello(QuestGiverHello hello)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_HELLO);
        packet.WriteGuid(hello.QuestGiverGUID.To64());
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_CLOSE_QUEST)]
    void HandleQuestGiverCloseQuest(QuestGiverCloseQuest close)
    {
        _ = close;
        // 3.4.3 only. On every other build this opcode stayed unhandled, and legacy
        // servers do not expect an unsolicited SMSG_GOSSIP_COMPLETE here.
        if (ModernVersion.Build != HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261)
            return;

        // The first CLOSE_QUEST after OfferReward is the client leaving the item
        // list, not Cancel. The next one is the real Cancel.
        if (GetSession().GameState.JustSentOfferReward)
        {
            GetSession().GameState.JustSentOfferReward = false;
            return;
        }

        GetSession().GameState.ClearQuestRewardWait();
        SendPacket(new GossipComplete());
    }

    [PacketHandler(Opcode.CMSG_CLOSE_INTERACTION)]
    void HandleCloseInteraction(CloseInteraction close)
    {
        // Leaving gossip after RequestItems. Continue is REQUEST_REWARD, not this.
        // The reward wait is deliberately kept: the client re-talks to the same NPC
        // straight after, and NPCHandler replays the item list to rebind the frame.
        // Any interaction with a different NPC clears it there.
        _ = close;
    }



    [PacketHandler(Opcode.CMSG_QUEST_POI_QUERY)]
    void HandleQuestPOIQuery(QuestPOIQuery query)
    {
        // Both legacy 3.3.5a and modern V3_4_3 use the same wire shape:
        // int32 count, int32[] questIds. Forward only the populated prefix.
        // Note: SMSG_QUEST_COMPLETION_NPC_RESPONSE is synthesized by the legacy
        // SMSG_QUEST_POI_QUERY_RESPONSE handler — there it's emitted right after
        // the POI translation, matching CypherCore's order and using
        // SendPacketToClient (auto-routes by ConnectionType.Instance).
        foreach (int questId in query.MissingQuestPOIs)
        {
            if (GameData.GetQuestTemplate((uint)questId) != null)
                continue;
            WorldPacket info = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
            info.WriteUInt32((uint)questId);
            SendPacketToServer(info);
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_POI_QUERY);
        packet.WriteInt32(query.MissingQuestPOIs.Length);
        foreach (int questId in query.MissingQuestPOIs)
            packet.WriteInt32(questId);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD)]
    void HandleQuestGiverRequestReward(QuestGiverRequestReward quest)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_CHOOSE_REWARD)]
    void HandleQuestGiverChooseReward(QuestGiverChooseReward quest)
    {
        int choiceIndex = 0;

        if (quest.Choice.Item.ItemID != 0)
        {
            QuestTemplate? questTemplate = GameData.GetQuestTemplate(quest.QuestID);
            if (questTemplate == null)
            {
                Log.Print(LogType.Error, "Unable to select quest reward because quest template is missing. Try again.");
                WorldPacket packet2 = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
                packet2.WriteUInt32(quest.QuestID);
                SendPacketToServer(packet2);
                QuestGiverQuestFailed fail = new QuestGiverQuestFailed();
                fail.QuestID = quest.QuestID;
                fail.Reason = InventoryResult.ItemNotFound;
                SendPacket(fail);
                return;
            }

            for (int i = 0; i < questTemplate.UnfilteredChoiceItems.Length; i++)
            {
                if (questTemplate.UnfilteredChoiceItems[i].ItemID == quest.Choice.Item.ItemID)
                {
                    choiceIndex = i;
                    break;
                }
            }
        }
        
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_CHOOSE_REWARD);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        packet.WriteInt32(choiceIndex);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_GIVER_COMPLETE_QUEST)]
    void HandleQuestGiverCompleteQuest(QuestGiverCompleteQuest quest)
    {
        Opcode opcode = Opcode.CMSG_QUEST_GIVER_COMPLETE_QUEST;
        if (ModernVersion.Build == HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261
            && GetSession().GameState.AwaitingQuestRewardId == quest.QuestID)
        {
            opcode = Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD;
            GetSession().GameState.AwaitingQuestRewardId = 0;
        }

        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_CONFIRM_ACCEPT)]
    void HandleQuestConfirmAcceptResponse(QuestConfirmAcceptResponse quest)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_CONFIRM_ACCEPT);
        packet.WriteUInt32(quest.QuestID);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_PUSH_QUEST_TO_PARTY)]
    void HandlePushQuestToParty(PushQuestToParty quest)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PUSH_QUEST_TO_PARTY);
        packet.WriteUInt32(quest.QuestID);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUEST_PUSH_RESULT)]
    void HandleQuestPushResult(QuestPushResultResponse quest)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_QUEST_PUSH_RESULT);
        packet.WriteGuid(quest.SenderGUID.To64());
        packet.WriteUInt8((byte)quest.Result);
        SendPacketToServer(packet);
    }
}
