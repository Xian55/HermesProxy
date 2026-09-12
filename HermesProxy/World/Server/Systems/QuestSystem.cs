using System;
using System.Collections.Generic;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Quest translation for the modern client: the quest giver frame, the quest log, POI queries and
/// party quest sharing.
/// </summary>
/// <remarks>
/// <para>
/// Bodies were moved from <c>World/Server/PacketHandlers/QuestHandler.cs</c>, not retyped;
/// <c>verify-handler-port.py</c> diffs each one against the original.
/// </para>
/// <para>
/// <see cref="ReturnQuestFrameToGossip"/> and <see cref="ReturnDetailsToGossip"/> arrived a slice
/// early, when <c>NpcSystem</c> converted and needed them; they are ordinary members of this type
/// now that the rest of the file has caught up.
/// </para>
/// <para>
/// Much of the behaviour here is V3_4_3-only bookkeeping around a client that reuses one frame for
/// details, request-items and offer-reward, and signals transitions by closing and reopening it.
/// The <c>JustSent*</c> / <c>Awaiting*</c> flags on <c>GameSessionData</c> are that state machine;
/// the comments on each branch say which client behaviour it answers.
/// </para>
/// </remarks>
public static class QuestSystem
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(WorldSocket).PadRight(15);
    private static readonly string _netDirRecv = Log.FormatDir(LogNetDir.C2P);

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_QUERY_QUEST)]
    public static void HandleQuestGiverQueryQuest(in QuestGiverQueryQuest quest, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_QUERY_QUEST);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V2_0_1_6180))
            packet.WriteBool(quest.RespondToGiver);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST)]
    public static void HandleQuestGiverAcceptQuest(in QuestGiverAcceptQuest quest, in SessionContext ctx)
    {
        ctx.GetSession().GameState.CloseQuestDetails();
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V3_1_2_9901))
            packet.WriteInt32(quest.StartCheat ? 1 : 0);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_LOG_REMOVE_QUEST)]
    public static void HandleQuestLogRemoveQuest(in QuestLogRemoveQuest quest, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_LOG_REMOVE_QUEST);
        packet.WriteUInt8(quest.Slot);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY)]
    public static void HandleQuestGiverStatusQuery(in QuestGiverStatusQuery query, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY);
        packet.WriteGuid(query.QuestGiverGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY)]
    public static void HandleQuestGiverStatusMultipleQuery(in EmptyClientPacket query, in SessionContext ctx)
    {
        if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY);
            ctx.SendPacketToServer(packet);
        }
        else
        {
            int UNIT_NPC_FLAGS = ModernVersion.GetUpdateField(UnitField.UNIT_NPC_FLAGS);
            if (UNIT_NPC_FLAGS < 0)
                return;

            List<WowGuid128> npcGuids = new List<WowGuid128>();
            lock (ctx.GetSession().GameState.ObjectCacheLock)
            {
                foreach (var obj in ctx.GetSession().GameState.ObjectCacheModern)
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
                ctx.SendPacketToServer(packet);
            }
        }
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_HELLO)]
    public static void HandleQuestGiverHello(in QuestGiverHello hello, in SessionContext ctx)
    {
        ctx.GetSession().GameState.CloseQuestDetails();
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_HELLO);
        packet.WriteGuid(hello.QuestGiverGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_CLOSE_QUEST)]
    public static void HandleQuestGiverCloseQuest(in QuestGiverCloseQuest close, in SessionContext ctx)
    {
        if (ModernVersion.Build != HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261)
            return;

        var state = ctx.GetSession().GameState;
        // First CLOSE_QUEST after OfferReward is leaving the item list
        if (state.JustSentOfferReward)
        {
            state.JustSentOfferReward = false;
            WorldSocketLogMessages.QuestClose(_melLog, _sourceFile, _netDirRecv, close.QuestID, "swallow-offer");
            return;
        }

        if (state.AwaitingQuestRewardId == close.QuestID)
        {
            if (state.JustSentRequestItems)
            {
                state.JustSentRequestItems = false;
                WorldSocketLogMessages.QuestClose(_melLog, _sourceFile, _netDirRecv, close.QuestID, "swallow-request-items");
                return;
            }

            ReturnQuestFrameToGossip(in ctx, (uint)close.QuestID, state.AwaitingQuestGiver, "close-request-items");
            return;
        }

        state.CloseQuestDetails();
    }

    [HandlesCmsg(Opcode.CMSG_CLOSE_INTERACTION)]
    public static void HandleCloseInteraction(in CloseInteraction close, in SessionContext ctx)
    {
        var state = ctx.GetSession().GameState;
        if (state.AwaitingQuestRewardId != 0)
        {
            if (ModernVersion.Build != HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261)
                return;

            if (state.JustSentRequestItems)
            {
                state.JustSentRequestItems = false;
                WorldSocketLogMessages.QuestClose(_melLog, _sourceFile, _netDirRecv, (int)state.AwaitingQuestRewardId, "swallow-request-items");
                return;
            }

            ReturnQuestFrameToGossip(in ctx, state.AwaitingQuestRewardId, state.AwaitingQuestGiver, "cancel-request-items");
            return;
        }

        if (!state.QuestDetailsOpen)
            return;

        int questId = (int)(state.LastQuestDetails?.QuestID ?? 0);

        // GossipFrame hid under QuestFrame. Leave details up
        if (state.JustLeftGossipForDetails)
        {
            state.JustLeftGossipForDetails = false;
            WorldSocketLogMessages.QuestClose(_melLog, _sourceFile, _netDirRecv, questId, "swallow-left-gossip");
            return;
        }

        state.CloseQuestDetails();
        WorldSocketLogMessages.QuestClose(_melLog, _sourceFile, _netDirRecv, questId, "release-details");
        _ = close;
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_POI_QUERY)]
    public static void HandleQuestPOIQuery(in QuestPOIQuery query, in SessionContext ctx)
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
            ctx.SendPacketToServer(info);
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_POI_QUERY);
        packet.WriteInt32(query.MissingQuestPOIs.Length);
        foreach (int questId in query.MissingQuestPOIs)
            packet.WriteInt32(questId);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD)]
    public static void HandleQuestGiverRequestReward(in QuestGiverRequestReward quest, in SessionContext ctx)
    {
        var state = ctx.GetSession().GameState;
        if (ModernVersion.Build == HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261)
        {
            var last = state.LastRequestItems;
            if (last != null && last.QuestID == quest.QuestID && last.StatusFlags != QuestGiverRequestItems.StatusComplete)
            {
                ctx.SendPacket(last);
                return;
            }

            if (state.AwaitingQuestRewardId == quest.QuestID)
                state.AwaitingQuestRewardId = 0;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_CHOOSE_REWARD)]
    public static void HandleQuestGiverChooseReward(in QuestGiverChooseReward quest, in SessionContext ctx)
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
                ctx.SendPacketToServer(packet2);
                QuestGiverQuestFailed fail = new QuestGiverQuestFailed();
                fail.QuestID = quest.QuestID;
                fail.Reason = InventoryResult.ItemNotFound;
                ctx.SendPacket(fail);
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
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_GIVER_COMPLETE_QUEST)]
    public static void HandleQuestGiverCompleteQuest(in QuestGiverCompleteQuest quest, in SessionContext ctx)
    {
        Opcode opcode = Opcode.CMSG_QUEST_GIVER_COMPLETE_QUEST;
        if (ModernVersion.Build == HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261
            && ctx.GetSession().GameState.AwaitingQuestRewardId == quest.QuestID)
        {
            opcode = Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD;
            ctx.GetSession().GameState.AwaitingQuestRewardId = 0;
        }

        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteGuid(quest.QuestGiverGUID.To64());
        packet.WriteUInt32(quest.QuestID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_CONFIRM_ACCEPT)]
    public static void HandleQuestConfirmAcceptResponse(in QuestConfirmAcceptResponse quest, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_CONFIRM_ACCEPT);
        packet.WriteUInt32(quest.QuestID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PUSH_QUEST_TO_PARTY)]
    public static void HandlePushQuestToParty(in PushQuestToParty quest, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PUSH_QUEST_TO_PARTY);
        packet.WriteUInt32(quest.QuestID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUEST_PUSH_RESULT)]
    public static void HandleQuestPushResult(in QuestPushResultResponse quest, in SessionContext ctx)
    {
        // MSG_QUEST_PUSH_RESULT is one of the few opcodes where the 3.3.5a cores disagree
        // on the layout, so this is deliberately shaped to satisfy all of them at once
        // rather than branching on the backend:
        //
        //   TrinityCore  Handlers/QuestHandler.cpp     guid >> questId >> msg   (13 bytes)
        //   AzerothCore  Packets/QuestPackets.cpp:98   guid >> QuestId >> msg   (13 bytes)
        //   cMaNGOS      Quests/QuestHandler.cpp:651   guid >> msg              ( 9 bytes)
        //
        // Sending only guid + msg makes TrinityCore and AzerothCore underflow on questId and
        // abandon the handler before ClearQuestSharingInfo() / SetDivider(), so the recipient
        // stays flagged as sharing a quest and every later share comes back "is busy" until
        // they relog. Sending the real questId instead breaks cMaNGOS, which would read msg
        // from that field's low byte.
        //
        // Both TrinityCore and AzerothCore parse questId and then never use it -- their
        // handlers only touch the guid and the message -- so putting the result in the
        // questId slot is harmless there, and little-endian puts it exactly where cMaNGOS
        // looks for msg. One packet, all three cores.
        WorldPacket packet = new WorldPacket(Opcode.MSG_QUEST_PUSH_RESULT);
        packet.WriteGuid(quest.SenderGUID.To64());
        packet.WriteUInt32((byte)quest.Result);
        packet.WriteUInt8((byte)quest.Result);
        ctx.SendPacketToServer(packet);
    }
    // 3.4.3 Decline is TALK_TO_GOSSIP, not a cancel opcode. Dismiss the
    // parchment, then put back this NPC's cached list only.
    internal static void ReturnDetailsToGossip(in SessionContext ctx, string action)
    {
        var state = ctx.GetSession().GameState;
        int questId = (int)(state.LastQuestDetails?.QuestID ?? 0);
        WowGuid128 npc = state.LastQuestDetails?.QuestGiverGUID ?? default;
        ReturnQuestFrameToGossip(in ctx, (uint)questId, npc, action);
    }

    internal static void ReturnQuestFrameToGossip(in SessionContext ctx, uint questId, WowGuid128 npc, string action)
    {
        var state = ctx.GetSession().GameState;
        var gossip = state.LastGossip;
        var list = state.LastQuestList;
        state.CloseQuestDetails();
        state.ClearQuestRewardWait();

        ctx.SendPacket(new QuestGiverInvalidQuest
        {
            Reason = QuestFailedReasons.None,
            SendErrorMessage = false
        });

        if (gossip != null && npc != default && gossip.GossipGUID == npc)
            ctx.SendPacket(gossip);
        else if (list != null && npc != default && list.QuestGiverGUID == npc)
            ctx.SendPacket(list);

        WorldSocketLogMessages.QuestClose(_melLog, _sourceFile, _netDirRecv, (int)questId, action);
    }
}
