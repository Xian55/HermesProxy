using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using static HermesProxy.World.Server.Packets.QueryPageTextResponse;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_QUERY_TIME_RESPONSE)]
    void HandleQueryTimeResponse(WorldPacket packet)
    {
        QueryTimeResponse response = new QueryTimeResponse();
        response.CurrentTime = packet.ReadInt32();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && packet.CanRead())
            packet.ReadInt32(); // Next Daily Quest Reset Time
        SendPacketToClient(response);
    }
    [PacketHandler(Opcode.SMSG_QUERY_QUEST_INFO_RESPONSE)]
    void HandleQueryQuestInfoResponse(WorldPacket packet)
    {
        QueryQuestInfoResponse response = new QueryQuestInfoResponse();
        var id = packet.ReadEntry();
        response.QuestID = (uint)id.Key;
        if (id.Value) // entry is masked
        {
            response.Allow = false;
            SendPacketToClient(response);
            return;
        }

        response.Allow = true;
        response.Info = new QuestTemplate();
        QuestTemplate quest = response.Info;

        quest.QuestID = response.QuestID;
        quest.QuestType = packet.ReadInt32();
        quest.QuestLevel = packet.ReadInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            quest.MinLevel = packet.ReadInt32();
        else
            quest.MinLevel = 1;

        quest.QuestSortID = packet.ReadInt32();
        quest.QuestInfoID = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            quest.SuggestedGroupNum = packet.ReadUInt32();

        sbyte objectiveCounter = 0;
        for (int i = 0; i < 2; i++)
        {
            int factionId = packet.ReadInt32(); // RequiredFactionID
            int factionValue = packet.ReadInt32(); // RequiredFactionValue
            if (factionId != 0 && factionValue != 0)
            {
                QuestObjective objective = new QuestObjective();
                objective.QuestID = response.QuestID;
                objective.Id = QuestObjective.QuestObjectiveCounter++;
                objective.StorageIndex = objectiveCounter++;
                objective.Type = QuestObjectiveType.MinReputation;
                objective.ObjectID = factionId;
                objective.Amount = factionValue;
                quest.Objectives.Add(objective);
            }
        }

        quest.RewardNextQuest = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            quest.RewardXPDifficulty = packet.ReadUInt32();

        int rewOrReqMoney = packet.ReadInt32();
        if (rewOrReqMoney >= 0)
            quest.RewardMoney = rewOrReqMoney;
        else
        {
            QuestObjective objective = new QuestObjective();
            objective.QuestID = response.QuestID;
            objective.Id = QuestObjective.QuestObjectiveCounter++;
            objective.StorageIndex = objectiveCounter++;
            objective.Type = QuestObjectiveType.Money;
            objective.ObjectID = 0;
            objective.Amount = -rewOrReqMoney;
            quest.Objectives.Add(objective);
        }
        quest.RewardBonusMoney = packet.ReadUInt32();
        quest.RewardDisplaySpell[0] = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            quest.RewardSpell = packet.ReadUInt32();
            quest.RewardHonor = packet.ReadUInt32();
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            quest.RewardKillHonor = packet.ReadFloat();

        quest.StartItem = packet.ReadUInt32();
        quest.Flags = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
            quest.RewardTitle = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            int requiredPlayerKills = packet.ReadInt32();
            if (requiredPlayerKills != 0)
            {
                QuestObjective objective = new QuestObjective();
                objective.QuestID = response.QuestID;
                objective.Id = QuestObjective.QuestObjectiveCounter++;
                objective.StorageIndex = objectiveCounter++;
                objective.Type = QuestObjectiveType.PlayerKills;
                objective.ObjectID = 0;
                objective.Amount = requiredPlayerKills;
                quest.Objectives.Add(objective);
            }
            packet.ReadUInt32(); // RewardTalents
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            quest.RewardArenaPoints = packet.ReadInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            packet.ReadInt32(); // Unk

        for (int i = 0; i < 4; i++)
        {
            quest.RewardItems[i] = packet.ReadUInt32();
            quest.RewardAmount[i] = packet.ReadUInt32();
        }

        for (int i = 0; i < 6; i++)
        {
            QuestInfoChoiceItem choiceItem = new QuestInfoChoiceItem();
            choiceItem.ItemID = packet.ReadUInt32();
            choiceItem.Quantity = packet.ReadUInt32();

            uint displayId = GameData.GetItemDisplayId(choiceItem.ItemID);
            if (displayId != 0)
                choiceItem.DisplayID = displayId;

            quest.UnfilteredChoiceItems[i] = choiceItem;
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
        {
            for (int i = 0; i < 5; i++)
                quest.RewardFactionID[i] = packet.ReadUInt32();

            for (int i = 0; i < 5; i++)
                quest.RewardFactionValue[i] = packet.ReadInt32();

            for (int i = 0; i < 5; i++)
                quest.RewardFactionOverride[i] = (int)packet.ReadUInt32();
        }

        quest.POIContinent = packet.ReadUInt32();
        quest.POIx = packet.ReadFloat();
        quest.POIy = packet.ReadFloat();
        quest.POIPriority = packet.ReadUInt32();
        quest.LogTitle = packet.ReadCString();
        quest.LogDescription = packet.ReadCString();
        quest.QuestDescription = packet.ReadCString();
        quest.AreaDescription = packet.ReadCString();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            quest.QuestCompletionLog = packet.ReadCString();

        var reqId = new KeyValuePair<int, bool>[4];
        var reqItemFieldCount = 4;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
            reqItemFieldCount = 5;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            reqItemFieldCount = 6;
        int[] requiredItemID = new int[reqItemFieldCount];
        int[] requiredItemCount = new int[reqItemFieldCount];

        for (int i = 0; i < 4; i++)
        {
            reqId[i] = packet.ReadEntry();
            bool isGo = reqId[i].Value;

            int creatureOrGoId = reqId[i].Key;
            int creatureOrGoAmount = packet.ReadInt32();

            if (creatureOrGoId != 0 && creatureOrGoAmount != 0)
            {
                QuestObjective objective = new QuestObjective();
                objective.QuestID = response.QuestID;
                objective.Id = QuestObjective.QuestObjectiveCounter++;
                objective.StorageIndex = objectiveCounter++;
                objective.Type = isGo ? QuestObjectiveType.GameObject : QuestObjectiveType.Monster;
                objective.ObjectID = creatureOrGoId;
                objective.Amount = creatureOrGoAmount;
                quest.Objectives.Add(objective);
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                requiredItemID[i] = packet.ReadInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                requiredItemCount[i] = packet.ReadInt32();

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_8_9464))
            {
                requiredItemID[i] = packet.ReadInt32();
                requiredItemCount[i] = packet.ReadInt32();
            }
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
        {
            for (int i = 0; i < reqItemFieldCount; i++)
            {
                requiredItemID[i] = packet.ReadInt32();
                requiredItemCount[i] = packet.ReadInt32();
            }
        }

        for (int i = 0; i < reqItemFieldCount; i++)
        {
            if (requiredItemID[i] != 0 && requiredItemCount[i] != 0)
            {
                QuestObjective objective = new QuestObjective();
                objective.QuestID = response.QuestID;
                objective.Id = QuestObjective.QuestObjectiveCounter++;
                objective.StorageIndex = objectiveCounter++;
                objective.Type = QuestObjectiveType.Item;
                objective.ObjectID = requiredItemID[i];
                objective.Amount = requiredItemCount[i];
                quest.Objectives.Add(objective);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            string objectiveText = packet.ReadCString();
            if (quest.Objectives.Count > i)
                quest.Objectives[i].Description = objectiveText;
        }

        // Placeholders
        quest.QuestMaxScalingLevel = 255;
        quest.RewardXPMultiplier = 1;
        quest.RewardMoneyMultiplier = 1;
        quest.RewardArtifactXPMultiplier = 1;
        for (int i = 0; i < QuestConst.QuestRewardReputationsCount; i++)
            quest.RewardFactionCapIn[i] = 7;
        quest.AllowableRaces = 511;
        quest.AcceptedSoundKitID = 890;
        quest.CompleteSoundKitID = 878;

        GameData.StoreQuestTemplate(response.QuestID, quest);
        SendPacketToClient(response);
    }

    [PacketHandler(Opcode.SMSG_QUERY_CREATURE_RESPONSE)]
    void HandleQueryCreatureResponse(WorldPacket packet)
    {
        QueryCreatureResponse response = new QueryCreatureResponse();
        var id = packet.ReadEntry();
        response.CreatureID = (uint)id.Key;
        if (id.Value) // entry is masked
        {
            response.Allow = false;
            SendPacketToClient(response);
            return;
        }

        response.Allow = true;
        response.Stats = new CreatureTemplate();
        CreatureTemplate creature = response.Stats;

        for (int i = 0; i < 4; i++)
            creature.Name[i] = packet.ReadCString();

        creature.Title = packet.ReadCString();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            creature.CursorName = packet.ReadCString();

        creature.Flags[0] = packet.ReadUInt32(); // Type Flags
        creature.Type = packet.ReadInt32();
        creature.Family = packet.ReadInt32();
        creature.Classification = packet.ReadInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            for (int i = 0; i < 2; ++i)
                creature.ProxyCreatureID[i] = packet.ReadUInt32();
        }
        else
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadInt32(); // unk
            creature.PetSpellDataId = packet.ReadUInt32();
        }

        int displayIdCount = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 4 : 1;
        for (int i = 0; i < displayIdCount; i++)
        {
            uint displayId = packet.ReadUInt32();
            if (displayId != 0)
                creature.Display.CreatureDisplay.Add(new CreatureXDisplay(displayId, 1, 0));
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            creature.HpMulti = packet.ReadFloat();
            creature.EnergyMulti = packet.ReadFloat();
        }
        else
        {
            creature.HpMulti = 1;
            creature.EnergyMulti = 1;
        }

        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            creature.Civilian = packet.ReadBool();
        creature.Leader = packet.ReadBool();

        int questItems = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192) ? 6 : 4;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            for (uint i = 0; i < questItems; ++i)
            {
                uint itemId = packet.ReadUInt32();
                if (itemId != 0)
                    creature.QuestItems.Add(itemId);
            }

            packet.ReadUInt32(); // Movement ID
        }

        // Placeholders
        creature.Flags[0] |= 134217728;
        creature.MovementInfoID = 1693;
        creature.Class = 1;

        GameData.StoreCreatureTemplate(response.CreatureID, creature);
        SendPacketToClient(response);
    }
    [PacketHandler(Opcode.SMSG_QUERY_GAME_OBJECT_RESPONSE)]
    void HandleQueryGameObjectResposne(WorldPacket packet)
    {
        QueryGameObjectResponse response = new QueryGameObjectResponse();
        var id = packet.ReadEntry();
        response.GameObjectID = (uint)id.Key;
        response.Guid = WowGuid128.Empty;
        if (id.Value) // entry is masked
        {
            response.Allow = false;
            SendPacketToClient(response);
            return;
        }

        response.Allow = true;
        response.Stats = new GameObjectStats();
        GameObjectStats gameObject = response.Stats;

        gameObject.Type = packet.ReadUInt32();
        gameObject.DisplayID = packet.ReadUInt32();

        for (int i = 0; i < 4; i++)
            gameObject.Name[i] = packet.ReadCString();

        gameObject.IconName = packet.ReadCString();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            gameObject.CastBarCaption = packet.ReadCString();
            gameObject.UnkString = packet.ReadCString();
        }

        for (int i = 0; i < 24; i++)
            gameObject.Data[i] = packet.ReadInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            gameObject.Size = packet.ReadFloat();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            uint count = (uint)(LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192) ? 6 : 4);
            for (uint i = 0; i < count; i++)
            {
                uint itemId = packet.ReadUInt32();
                if (itemId != 0)
                    gameObject.QuestItems.Add(itemId);
            }
        }

        SendPacketToClient(response);
    }
    [PacketHandler(Opcode.SMSG_QUERY_PAGE_TEXT_RESPONSE)]
    void HandleQueryPageTextResponse(WorldPacket packet)
    {
        QueryPageTextResponse response = new QueryPageTextResponse();
        response.PageTextID = packet.ReadUInt32();
        response.Allow = true;
        PageTextInfo page = new PageTextInfo();
        page.Id = response.PageTextID;
        page.Text = packet.ReadCString();
        page.NextPageID = packet.ReadUInt32();
        response.Pages.Add(page);
        SendPacketToClient(response);
    }
    [PacketHandler(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE)]
    void HandleQueryNpcTextResponse(WorldPacket packet)
    {
        QueryNPCTextResponse response = new QueryNPCTextResponse();
        var id = packet.ReadEntry();
        response.TextID = (uint)id.Key;
        if (id.Value) // entry is masked
        {
            response.Allow = false;
            SendPacketToClient(response);
            return;
        }

        response.Allow = true;

        for (int i = 0; i < 8; i++)
        {
            response.Probabilities[i] = packet.ReadFloat();
            string maleText = packet.ReadCString().TrimEnd().Replace("\0", "");
            string femaleText = packet.ReadCString().TrimEnd().Replace("\0", "");
            uint language = packet.ReadUInt32();

            ushort[] emoteDelays = new ushort[3];
            ushort[]  emotes = new ushort[3];
            for (int j = 0; j < 3; j++)
            {
                emoteDelays[j] = (ushort)packet.ReadUInt32();
                emotes[j] = (ushort)packet.ReadUInt32();
            }

            const string placeholderGossip = "Greetings $N";

            if (String.IsNullOrEmpty(maleText) && String.IsNullOrEmpty(femaleText) ||
                maleText.Equals(placeholderGossip) && femaleText.Equals(placeholderGossip) && i != 0)
                response.BroadcastTextID[i] = 0;
            else
                response.BroadcastTextID[i] = GameData.GetBroadcastTextId(maleText, femaleText, language, emoteDelays, emotes);
        }

        SendPacketToClient(response);
    }

    [PacketHandler(Opcode.SMSG_ITEM_QUERY_SINGLE_RESPONSE)]
    void HandleItemQueryResponse(WorldPacket packet)
    {
        var entry = packet.ReadEntry();
        if (entry.Value)
        {
            if (GetSession().GameState.RequestedItemHotfixes.Contains((uint)entry.Key))
            {
                DBReply reply = new();
                reply.RecordID = (uint)entry.Key;
                reply.TableHash = DB2Hash.Item;
                reply.Status = HotfixStatus.Invalid;
                reply.Timestamp = (uint)Time.UnixTime;
                SendPacketToClient(reply);
            }
            if (GetSession().GameState.RequestedItemSparseHotfixes.Contains((uint)entry.Key))
            {
                DBReply reply2 = new();
                reply2.RecordID = (uint)entry.Key;
                reply2.TableHash = DB2Hash.ItemSparse;
                reply2.Status = HotfixStatus.Invalid;
                reply2.Timestamp = (uint)Time.UnixTime;
                SendPacketToClient(reply2);
            }
            // issue #34: even an "invalid" answer is an answer — drop this id
            // from any pending waiting-set so the deferred batch can release.
            FlushDeferredUpdatesFor((uint)entry.Key);
            return;
        }

        ItemTemplate item = new ItemTemplate();
        item.ReadFromLegacyPacket((uint)entry.Key, packet);

        SendItemUpdatesIfNeeded(item);
        GameData.StoreItemTemplate((uint)entry.Key, item);

        // issue #34: any UpdateObject batch that was held back waiting on this
        // item template's hotfix can be released now that the DB2 row is in
        // place on the modern client.
        FlushDeferredUpdatesFor((uint)entry.Key);
    }

    private void FlushDeferredUpdatesFor(uint resolvedItemId)
    {
        var session = GetSession();
        List<PendingObjectUpdate>? toFlush = null;
        lock (session.GameState.DeferredObjectUpdatesLock)
        {
            var pending = session.GameState.DeferredObjectUpdates;
            for (int i = 0; i < pending.Count; i++)
            {
                var entry = pending[i];
                entry.WaitingForItemIds.Remove(resolvedItemId);
                if (entry.WaitingForItemIds.Count == 0)
                {
                    toFlush ??= [];
                    toFlush.Add(entry);
                }
            }
            if (toFlush != null)
            {
                foreach (var entry in toFlush)
                    pending.Remove(entry);
            }
        }

        if (toFlush == null)
            return;

        foreach (var entry in toFlush)
        {
            // Pre-filter Values updates BEFORE the emptiness check (mirrors the
            // UpdateHandler.HandleUpdateObject path) so we don't ship empty
            // SMSG_UPDATE_OBJECT packets to the V3_4_3 client.
            UpdateObject.FilterV3_4_3Values(entry.UpdateObject, session.GameState);

            if (entry.UpdateObject.ObjectUpdates.Count != 0 ||
                entry.UpdateObject.DestroyedGuids.Count != 0 ||
                entry.UpdateObject.OutOfRangeGuids.Count != 0)
                SendPacketToClient(entry.UpdateObject);

            foreach (var auraUpdate in entry.AuraUpdates)
                SendPacketToClient(auraUpdate);

            // V3_4_3-only: when this deferred batch contained any CreateObject for
            // the player, immediately follow it with an empty SMSG_AURA_UPDATE_ALL.
            // Mirrors the in-line trigger in UpdateHandler.HandleUpdateObject — the
            // deferred path bypasses that code, but the V3_4_3 client requires the
            // post-Create AURA_UPDATE handshake regardless of which path delivered
            // the player object. Without this, the player CreateObject2 ships to the
            // client but the client never sends CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE
            // and the loading screen never dismisses.
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                var currentPlayerGuid = session.GameState.CurrentPlayerGuid;
                bool playerCreateInBatch = false;
                foreach (var u in entry.UpdateObject.ObjectUpdates)
                {
                    if (u.CreateData != null && u.Guid == currentPlayerGuid &&
                        (u.Type == UpdateTypeModern.CreateObject1 || u.Type == UpdateTypeModern.CreateObject2))
                    {
                        playerCreateInBatch = true;
                        break;
                    }
                }

                Log.Print(LogType.Trace,
                    $"[PlayerEnterTrace] deferred-flush: objects={entry.UpdateObject.ObjectUpdates.Count} " +
                    $"playerCreateMatched={playerCreateInBatch} playerGuid={currentPlayerGuid} " +
                    $"types=[{string.Join(",", entry.UpdateObject.ObjectUpdates.Select(o => $"{o.Guid.Low}:{o.Type}"))}]");

                if (playerCreateInBatch)
                {
                    // TC reference packet #141 is an EMPTY SMSG_UPDATE_OBJECT (NumObjUpdates=0,
                    // Data size=0, 11 bytes total) sent immediately after the player+items
                    // batch and BEFORE the post-Create handshake (PhaseShiftChange, etc.).
                    // The V3_4_3 client may use this empty marker as a "create burst
                    // complete" signal that transitions its state from "loading-screen"
                    // to "in-world" — at TC #143 the client emits CMSG_REQUEST_PLAYED_TIME
                    // unprompted, which never happens in our flow without this empty
                    // packet. Without this marker, the post-Create handshake arrives but
                    // the client never enters the in-world state machine, and so never
                    // fires CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE.
                    var emptyUpdateMarker = new UpdateObject(session.GameState);
                    SendPacketToClient(emptyUpdateMarker);
                    Log.Print(LogType.Trace,
                        $"[PlayerEnterTrace] deferred-flush empty SMSG_UPDATE_OBJECT marker sent (mirrors TC #141)");

                    // Post-CreateObject world-ready handshake. Order matches TC reference
                    // (`World_login_parsed.txt` packets #142–#151): AURA_UPDATE_ALL →
                    // PHASE_SHIFT_CHANGE → INIT_WORLD_STATES → UPDATE_ACTION_BUTTONS.
                    // UPDATE_ACTION_BUTTONS must be LAST: TC's parse shows it as the final
                    // server packet, with CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE arriving
                    // 1ms later — the client uses that last packet of the world-entry
                    // burst as its "world ready" trigger.
                    //
                    // SMSG_MOVE_SET_ACTIVE_MOVER is intentionally NOT in this block.
                    // TC sends it BEFORE the player CreateObject (#138), so we send it
                    // early in CharacterHandler.HandleLoginVerifyWorld instead.
                    var playerAuraSync = new AuraUpdate(currentPlayerGuid, true);
                    SendPacketToClient(playerAuraSync);
                    Log.Print(LogType.Trace,
                        $"[PlayerEnterTrace] deferred-flush post-CreateObject AURA_UPDATE_ALL sent for player guid={currentPlayerGuid}");

                    var phaseShiftAfter = new PhaseShiftChange
                    {
                        Client = currentPlayerGuid,
                    };
                    SendPacketToClient(phaseShiftAfter);
                    Log.Print(LogType.Trace,
                        $"[PlayerEnterTrace] deferred-flush post-CreateObject SMSG_PHASE_SHIFT_CHANGE resent for player guid={currentPlayerGuid}");

                    var cachedWorldStates = session.GameState.LastInitWorldStates;
                    if (cachedWorldStates != null)
                    {
                        SendPacketToClient(cachedWorldStates);
                        Log.Print(LogType.Trace,
                            $"[PlayerEnterTrace] deferred-flush post-CreateObject SMSG_INIT_WORLD_STATES resent (mapId={cachedWorldStates.MapID} zoneId={cachedWorldStates.ZoneID})");
                    }

                    // LAST packet — TC's canary trigger. cmangos sends action buttons
                    // BEFORE the player CreateObject; the early forward in
                    // CharacterHandler.HandleUpdateActionButtons reaches the client too
                    // soon to bind to a not-yet-existing player. Re-emit here so the
                    // client gets a second copy AFTER the player object — this is the
                    // emission TC's last server packet maps to.
                    var cachedButtons = session.GameState.ActionButtons;
                    if (cachedButtons != null && cachedButtons.Count > 0)
                    {
                        // UpdateActionButtons.Write pads to PlayerConst.MaxActionButtonsModern (180)
                        // internally — no need to pad the list ourselves.
                        var modern = new UpdateActionButtons { Reason = 0 };
                        modern.ActionButtons.AddRange(cachedButtons);
                        SendPacketToClient(modern);
                        Log.Print(LogType.Trace,
                            $"[PlayerEnterTrace] deferred-flush post-CreateObject SMSG_UPDATE_ACTION_BUTTONS resent LAST ({modern.ActionButtons.Count} legacy entries, Reason=0)");
                    }
                }
            }
        }
    }

    void SendItemUpdatesIfNeeded(ItemTemplate item)
    {
        Server.Packets.HotFixMessage? reply;

        reply = GameData.GenerateItemUpdateIfNeeded(item);
        if (reply != null)
            SendPacketToClient(reply);

        reply = GameData.GenerateItemSparseUpdateIfNeeded(item);
        if (reply != null)
        {
            // The V3_4_3 ItemSparse layout has been brought into alignment with
            // WPP's ItemSparseHandler341 expectations (StartQuestID/ItemRange split,
            // MinReputation Int32, three new Int32 fields between FactionRelated and
            // MaxDurability, no trailing MinReputation byte, removed duplicate
            // StartQuestId UInt16). The HotFixMessage path is safe to send again.
            SendPacketToClient(reply);

            Server.Packets.DBReply replyA = new();
            replyA.Status = HotfixStatus.Valid;
            replyA.Timestamp = (uint)Time.UnixTime;
            replyA.RecordID = reply.Hotfixes[0].RecordId;
            replyA.TableHash = reply.Hotfixes[0].TableHash;
            replyA.Data = reply.Hotfixes[0].HotfixContent;
            SendPacketToClient(replyA);
        }

        for (byte i = 0; i < 5; i++)
        {
            reply = GameData.GenerateItemEffectUpdateIfNeeded(item, i);
            if (reply != null)
            {
                SendPacketToClient(reply);

                // Mirror the ItemSparse path: a paired DBReply is needed for the modern client
                // to actually apply the corrected ItemEffect record in the running session.
                // Without this, SoM 1.14.1+ renumbered on-use spells (e.g. Diamond Flask) stay
                // bound to their modern spell id even after the hotfix is sent.
                Server.Packets.DBReply replyA = new();
                replyA.Status = HotfixStatus.Valid;
                replyA.Timestamp = (uint)Time.UnixTime;
                replyA.RecordID = reply.Hotfixes[0].RecordId;
                replyA.TableHash = reply.Hotfixes[0].TableHash;
                replyA.Data = reply.Hotfixes[0].HotfixContent;
                SendPacketToClient(replyA);
            }
        }

        if (!GameData.ItemCanHaveModel(item))
            return;

        reply = GameData.GenerateItemAppearanceUpdateIfNeeded(item);
        if (reply != null)
            SendPacketToClient(reply);

        reply = GameData.GenerateItemModifiedAppearanceUpdateIfNeeded(item);
        if (reply != null)
            SendPacketToClient(reply);
    }

    [PacketHandler(Opcode.SMSG_QUERY_PET_NAME_RESPONSE)]
    void HandleQueryPetNameResponse(WorldPacket packet)
    {
        uint petNumber = packet.ReadUInt32();
        WowGuid128 guid = GetSession().GameState.GetPetGuidByNumber(petNumber);
        if (guid == default)
        {
            Log.Print(LogType.Error, $"Pet name query response for unknown pet {petNumber}!");
            return;
        }

        QueryPetNameResponse response = new QueryPetNameResponse();
        response.UnitGUID = guid;
        response.Name = packet.ReadCString();
        if (response.Name.Length == 0)
        {
            response.Allow = false;
            packet.ReadBytes(7); // 0s
            return;
        }

        response.Allow = true;
        response.Timestamp = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            var declined = packet.ReadBool();

            const int maxDeclinedNameCases = 5;

            if (declined)
            {
                for (var i = 0; i < maxDeclinedNameCases; i++)
                    response.DeclinedNames.name[i] = packet.ReadCString();
            }
        }
        SendPacketToClient(response);
    }
    [PacketHandler(Opcode.SMSG_ITEM_NAME_QUERY_RESPONSE)]
    void HandleItemNameQueryResponse(WorldPacket packet)
    {
        uint entry = packet.ReadUInt32();
        string name = packet.ReadCString();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.ReadUInt32(); // Inventory Type
        GameData.StoreItemName(entry, name);
    }
    [PacketHandler(Opcode.SMSG_WHO)]
    void HandleWhoResponse(WorldPacket packet)
    {
        WhoResponsePkt response = new WhoResponsePkt();
        response.RequestID = GetSession().GameState.LastWhoRequestId;
        var count = packet.ReadUInt32();
        packet.ReadUInt32(); // Online count
        for (var i = 0; i < count; ++i)
        {
            WhoEntry player = new();
            player.PlayerData.Name = packet.ReadCString();
            player.GuildName = packet.ReadCString();
            player.PlayerData.Level = (byte)packet.ReadUInt32();
            player.PlayerData.ClassID = (Class)packet.ReadUInt32();
            player.PlayerData.RaceID = (Race)packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                player.PlayerData.Sex = (Gender)packet.ReadUInt8();
            player.AreaID = packet.ReadInt32();

            player.PlayerData.GuidActual = GetSession().GameState.GetPlayerGuidByName(player.PlayerData.Name);
            if (player.PlayerData.GuidActual == default)
                player.PlayerData.GuidActual = WowGuid128.CreateUnknownPlayerGuid();
            player.PlayerData.AccountID = GetSession().GetGameAccountGuidForPlayer(player.PlayerData.GuidActual);
            player.PlayerData.BnetAccountID = GetSession().GetBnetAccountGuidForPlayer(player.PlayerData.GuidActual);
            player.PlayerData.VirtualRealmAddress = GetSession().RealmId.GetAddress();

            if (!String.IsNullOrEmpty(player.GuildName))
            {
                player.GuildGUID = GetSession().GetGuildGuid(player.GuildName);
                player.GuildVirtualRealmAddress = player.PlayerData.VirtualRealmAddress;
            }
            response.Players.Add(player);
            Session.GameState.UpdatePlayerCache(player.PlayerData.GuidActual, new PlayerCache
            {
                Name = player.PlayerData.Name,
                RaceId = player.PlayerData.RaceID,
                ClassId = player.PlayerData.ClassID,
                SexId = player.PlayerData.Sex,
                Level = player.PlayerData.Level,
            });
        }
        SendPacketToClient(response);
    }
}
