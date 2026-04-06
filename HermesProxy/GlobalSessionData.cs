using HermesProxy.Auth;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Framework.Realm;
using HermesProxy.World.Server.Packets;
using ArenaTeamInspectData = HermesProxy.World.Server.Packets.ArenaTeamInspectData;
using System;

namespace HermesProxy
{
    public class PlayerCache
    {
        public string? Name;
        public Race RaceId = Race.None;
        public Class ClassId = Class.None;
        public Gender SexId = Gender.None;
        public byte Level = 0;
    }

    public sealed class OwnCharacterInfo : PlayerCache
    {
        public WowGuid128 AccountId;
        public WowGuid128 CharacterGuid;
        public Realm Realm = null!;
        public ulong LastLoginUnixSec;
    }

    public sealed class TradeSession
    {
        public static uint GlobalTradeIdCounter; // Fallback for pre 2.0.0 servers
        public uint TradeId;

        public WowGuid128 Partner;
        public WowGuid128 PartnerAccount;

        public uint ClientStateIndex = 1; // incremented for every update on our side
        public uint ServerStateIndex = 1; // incremented by any trade action
    }

    public sealed class GameSessionData
    {
        public bool HasWsgHordeFlagCarrier;
        public bool HasWsgAllyFlagCarrier;
        public bool ChannelDisplayList;
        public bool ShowPlayedTime;
        public bool IsInFarSight;
        public bool IsInTaxiFlight;
        public bool IsWaitingForTaxiStart;
        public bool IsWaitingForNewWorld;
        public bool IsWaitingForWorldPortAck;
        public bool IsFirstEnterWorld;
        public bool IsConnectedToInstance;
        public Queue<ServerPacket> PendingUninstancedPackets = new(); // Here packets are queued while IsConnectedToInstance = false;
        public readonly Lock PendingUninstancedPacketsLock = new();
        public bool IsInWorld;
        public uint? CurrentMapId;
        public uint CurrentZoneId;
        public uint CurrentTaxiNode;
        public List<byte> UsableTaxiNodes = new();
        public uint PendingTransferMapId;
        public uint LastEnteredAreaTrigger;
        public uint LastDispellSpellId;
        public string LeftChannelName = "";
        public bool IsPassingOnLoot;
        public int GroupUpdateCounter;
        public uint GroupReadyCheckResponses;
        public World.Server.Packets.PartyUpdate?[] CurrentGroups = new World.Server.Packets.PartyUpdate?[2];
        public bool WeWantToLeaveGroup; // Only send kick message when we dont initiated the group-leave
        public List<OwnCharacterInfo> OwnCharacters = new();
        public WowGuid128 CurrentPlayerGuid;
        public long CurrentPlayerCreateTime;
        public OwnCharacterInfo? CurrentPlayerInfo;
        public CurrentPlayerStorage CurrentPlayerStorage = null!;
        public uint CurrentGuildCreateTime;
        public uint CurrentGuildNumAccounts;
        public WowGuid128 CurrentInteractedWithNPC;
        public WowGuid128 CurrentInteractedWithGO;
        public uint LastWhoRequestId;
        public WowGuid128 CurrentPetGuid;
        public WowGuid64 CurrentAttackTarget;        // active CMSG_ATTACK_SWING victim, cleared on ATTACK_STOP/CANCEL_COMBAT
        public bool WaitingForAttackStart;           // true between CMSG_ATTACK_SWING and SMSG_ATTACK_START
        public bool DeferredAttackStop;              // CMSG_ATTACK_STOP received while waiting for SMSG_ATTACK_START
        public uint[] CurrentArenaTeamIds = new uint[3];
        public ConcurrentQueue<ClientCastRequest> PendingNormalCasts = new();  // regular spell casts (queue for proper FIFO handling)
        public ClientCastRequest? CurrentClientNextMeleeCast; // next melee spells (Raptor Strike, Heroic Strike, etc.)
        public ClientCastRequest? CurrentClientAutoRepeatCast; // auto repeat spells (Auto Shot, Shoot, etc.)
        public ConcurrentQueue<ClientCastRequest> PendingPetCasts = new();  // pet spell casts (queue for proper FIFO handling)
        public WowGuid64 LastLootTargetGuid;
        public List<WowGuid128>? MasterLootCandidates;
        public WowGuid64 LastMasterLootSentTarget;
        public List<int> ActionButtons = new();
        public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationUpdateTime = new();
        public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationLeft = new();
        public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationFull = new();
        public Dictionary<WowGuid128, Dictionary<byte, WowGuid128>> UnitAuraCaster = new();
        public Dictionary<WowGuid128, PlayerCache> CachedPlayers = new();
        public HashSet<WowGuid128> IgnoredPlayers = new();
        public Dictionary<WowGuid128, uint> PlayerGuildIds = new();
        public readonly Lock ObjectCacheLock = new();
        public Dictionary<WowGuid128, Dictionary<int, UpdateField>> ObjectCacheLegacy = new();
        public Dictionary<WowGuid128, UpdateFieldsArray> ObjectCacheModern = new();
        public Dictionary<WowGuid128, ObjectType> OriginalObjectTypes = new();
        public Dictionary<WowGuid128, uint[]> ItemGems = new();
        public Dictionary<uint, Class> CreatureClasses = new();
        public Dictionary<string, int> ChannelIds = new();
        public Dictionary<uint, uint> ItemBuyCount = new();
        public Dictionary<uint, uint> RealSpellToLearnSpell = new();
        public Dictionary<uint, ArenaTeamData> ArenaTeams = new();
        public World.Server.Packets.MailListResult? PendingMailListPacket;
        public HashSet<uint> RequestedItemTextIds = new HashSet<uint>();
        public Dictionary<uint, string> ItemTexts = new Dictionary<uint, string>();
        public Dictionary<uint, uint> BattleFieldQueueTypes = new Dictionary<uint, uint>();
        public Dictionary<uint, long> BattleFieldQueueTimes = new Dictionary<uint, long>();
        public Dictionary<uint, uint> DailyQuestsDone = new Dictionary<uint, uint>();
        public HashSet<WowGuid128> FlagCarrierGuids = new HashSet<WowGuid128>();
        public Dictionary<WowGuid64, ushort> ObjectSpawnCount = new Dictionary<WowGuid64, ushort>();
        public HashSet<WowGuid64> DespawnedGameObjects = new();
        public HashSet<WowGuid128> HunterPetGuids = new HashSet<WowGuid128>();
        public Dictionary<WowGuid128, ArenaTeamInspectData[]> PlayerArenaTeams = new Dictionary<WowGuid128, ArenaTeamInspectData[]>();
        public HashSet<string> AddonPrefixes = new HashSet<string>();
        public Dictionary<byte, Dictionary<byte, int>> FlatSpellMods = new Dictionary<byte, Dictionary<byte, int>>();
        public Dictionary<byte, Dictionary<byte, int>> PctSpellMods = new Dictionary<byte, Dictionary<byte, int>>();
        public Dictionary<WowGuid128, Dictionary<uint, WowGuid128>> LastAuraCasterOnTarget = new Dictionary<WowGuid128, Dictionary<uint, WowGuid128>>();
        public TradeSession? CurrentTrade = null;
        public HashSet<uint> RequestedItemHotfixes = new HashSet<uint>();
        public HashSet<uint> RequestedItemSparseHotfixes = new HashSet<uint>();

        private GameSessionData()
        {
            
        }

        public static GameSessionData CreateNewGameSessionData(GlobalSessionData globalSession)
        {
            var self = new GameSessionData();
            self.CurrentPlayerStorage = new CurrentPlayerStorage(globalSession);
            return self;
        }
        
        public uint GetCurrentGroupSize()
        {
            var group = GetCurrentGroup();
            if (group == null)
                return 0;

            // Don't count self.
            return (uint)(group.PlayerList.Count > 1 ? group.PlayerList.Count - 1 : 0);
        }
        public WowGuid128 GetCurrentGroupLeader()
        {
            var group = GetCurrentGroup();
            if (group == null)
                return WowGuid128.Empty;

            return group.LeaderGUID;
        }
        public LootMethod GetCurrentLootMethod()
        {
            var group = GetCurrentGroup();
            if (group == null)
                return LootMethod.FreeForAll;

            return group.LootSettings.Method;
        }
        public WowGuid128 GetCurrentGroupGuid()
        {
            var group = GetCurrentGroup();
            if (group == null)
                return WowGuid128.Empty;

            return group.PartyGUID;
        }
        public World.Server.Packets.PartyUpdate? GetCurrentGroup()
        {
            return CurrentGroups[GetCurrentPartyIndex()];
        }
        public sbyte GetCurrentPartyIndex()
        {
            return (sbyte)(IsInBattleground() ? 1 : 0);
        }
        public byte GetItemSpellSlot(WowGuid128 guid, uint spellId)
        {
            int OBJECT_FIELD_ENTRY = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
            if (OBJECT_FIELD_ENTRY < 0)
                return 0;

            var updates = GetCachedObjectFieldsLegacy(guid);
            if (updates == null)
                return 0;

            uint itemId = updates[OBJECT_FIELD_ENTRY].UInt32Value;
            return GameData.GetItemEffectSlot(itemId, spellId);
        }
        public uint GetItemId(WowGuid128 guid)
        {
            int OBJECT_FIELD_ENTRY = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
            if (OBJECT_FIELD_ENTRY < 0)
                return 0;

            var updates = GetCachedObjectFieldsLegacy(guid);
            if (updates == null)
                return 0;

            uint itemId = updates[OBJECT_FIELD_ENTRY].UInt32Value;
            return itemId;
        }
        public void SetFlatSpellMod(byte spellMod, byte spellMask, int amount)
        {
            if (FlatSpellMods.ContainsKey(spellMod))
            {
                if (FlatSpellMods[spellMod].ContainsKey(spellMask))
                {
                    FlatSpellMods[spellMod][spellMask] = amount;

                }
                else
                {
                    FlatSpellMods[spellMod].Add(spellMask, amount);
                }
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(spellMask, amount);
                FlatSpellMods.Add(spellMod, dict);
            }
        }
        public void SetPctSpellMod(byte spellMod, byte spellMask, int amount)
        {
            if (PctSpellMods.ContainsKey(spellMod))
            {
                if (PctSpellMods[spellMod].ContainsKey(spellMask))
                {
                    PctSpellMods[spellMod][spellMask] = amount;

                }
                else
                {
                    PctSpellMods[spellMod].Add(spellMask, amount);
                }
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(spellMask, amount);
                PctSpellMods.Add(spellMod, dict);
            }
        }
        public ArenaTeamInspectData GetArenaTeamDataForPlayer(WowGuid128 guid, byte slot)
        {
            if (PlayerArenaTeams.TryGetValue(guid, out var teams) && teams[slot] != null)
                return teams[slot];

            return new ArenaTeamInspectData();
        }
        public void StoreArenaTeamDataForPlayer(WowGuid128 guid, byte slot, ArenaTeamInspectData team)
        {
            if (!PlayerArenaTeams.ContainsKey(guid))
                PlayerArenaTeams.Add(guid, new ArenaTeamInspectData[3]);

            PlayerArenaTeams[guid][slot] = team;
        }
        public WowGuid64 GetInventorySlotItem(int slot)
        {
            int PLAYER_FIELD_INV_SLOT_HEAD = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_INV_SLOT_HEAD);
            if (PLAYER_FIELD_INV_SLOT_HEAD >= 0)
            {
                var updates = GetCachedObjectFieldsLegacy(CurrentPlayerGuid);
                if (updates != null)
                    return updates.GetGuidValue(PLAYER_FIELD_INV_SLOT_HEAD + slot * 2).To64();
            }
            return WowGuid64.Empty;
        }
        public WowGuid64 GetInventorySlotItem(byte containerSlot, byte slot)
        {
            // Main backpack (Bag0 = 255): read directly from player inventory fields
            if (containerSlot == 255)
                return GetInventorySlotItem(slot);

            // Extra bag: read from the bag container's slot fields
            var bagGuid64 = GetInventorySlotItem(containerSlot);
            if (bagGuid64 == WowGuid64.Empty)
                return WowGuid64.Empty;

            int containerSlotField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_SLOT_1);
            if (containerSlotField < 0)
                return WowGuid64.Empty;

            var bagGuid128 = bagGuid64.To128(this);
            var bagFields = GetCachedObjectFieldsLegacy(bagGuid128);
            if (bagFields == null)
                return WowGuid64.Empty;

            return bagFields.GetGuidValue(containerSlotField + slot * 2);
        }
        public uint GetItemStackCount(WowGuid128 itemGuid)
        {
            uint count = GetLegacyFieldValueUInt32(itemGuid, ItemField.ITEM_FIELD_STACK_COUNT);
            return count > 0 ? count : 1;
        }
        public (byte containerSlot, byte slot)? FindItemInInventory(WowGuid64 itemGuid64)
        {
            // Search main backpack (slots ItemStart=23 to ItemEnd=39)
            const int itemStart = 23;
            const int itemEnd = 39;
            for (int i = itemStart; i < itemEnd; i++)
            {
                if (GetInventorySlotItem(i) == itemGuid64)
                    return (255, (byte)i);
            }

            // Search extra bags (bag container slots 19-22)
            const int bagStart = 19;
            const int bagEnd = 23;
            int containerSlotField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_SLOT_1);
            int numSlotsField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_NUM_SLOTS);
            if (containerSlotField < 0 || numSlotsField < 0)
                return null;

            for (int bagIdx = bagStart; bagIdx < bagEnd; bagIdx++)
            {
                var bagGuid64 = GetInventorySlotItem(bagIdx);
                if (bagGuid64 == WowGuid64.Empty)
                    continue;

                var bagGuid128 = bagGuid64.To128(this);
                var bagFields = GetCachedObjectFieldsLegacy(bagGuid128);
                if (bagFields == null)
                    continue;

                if (!bagFields.ContainsKey(numSlotsField))
                    continue;
                int numSlots = (int)bagFields[numSlotsField].UInt32Value;

                for (int slot = 0; slot < numSlots; slot++)
                {
                    var slotGuid = bagFields.GetGuidValue(containerSlotField + slot * 2);
                    if (slotGuid == itemGuid64)
                        return ((byte)bagIdx, (byte)slot);
                }
            }

            return null;
        }
        public (byte containerSlot, byte slot)? FindEmptyInventorySlot()
        {
            // Search main backpack first
            const int itemStart = 23;
            const int itemEnd = 39;
            for (int i = itemStart; i < itemEnd; i++)
            {
                if (GetInventorySlotItem(i) == WowGuid64.Empty)
                    return (255, (byte)i);
            }

            // Search extra bags
            const int bagStart = 19;
            const int bagEnd = 23;
            int containerSlotField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_SLOT_1);
            int numSlotsField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_NUM_SLOTS);
            if (containerSlotField < 0 || numSlotsField < 0)
                return null;

            for (int bagIdx = bagStart; bagIdx < bagEnd; bagIdx++)
            {
                var bagGuid64 = GetInventorySlotItem(bagIdx);
                if (bagGuid64 == WowGuid64.Empty)
                    continue;

                var bagGuid128 = bagGuid64.To128(this);
                var bagFields = GetCachedObjectFieldsLegacy(bagGuid128);
                if (bagFields == null)
                    continue;

                if (!bagFields.ContainsKey(numSlotsField))
                    continue;
                int numSlots = (int)bagFields[numSlotsField].UInt32Value;

                for (int slot = 0; slot < numSlots; slot++)
                {
                    var slotGuid = bagFields.GetGuidValue(containerSlotField + slot * 2);
                    if (slotGuid == WowGuid64.Empty)
                        return ((byte)bagIdx, (byte)slot);
                }
            }

            return null;
        }
        public ushort GetObjectSpawnCounter(WowGuid64 guid)
        {
            if (ObjectSpawnCount.TryGetValue(guid, out ushort count))
                return count;
            return 0;
        }
        public void IncrementObjectSpawnCounter(WowGuid64 guid)
        {
            if (ObjectSpawnCount.ContainsKey(guid))
                ObjectSpawnCount[guid]++;
            else
                ObjectSpawnCount.Add(guid, 0);
        }
        public void SetDailyQuestSlot(uint slot, uint questId)
        {
            if (DailyQuestsDone.ContainsKey(slot))
            {
                if (questId != 0)
                    DailyQuestsDone[slot] = questId;
                else
                    DailyQuestsDone.Remove(slot);
            }
            else if (questId != 0)
                DailyQuestsDone.Add(slot, questId);
        }
        public bool IsAlliancePlayer(WowGuid128 guid)
        {
            PlayerCache? cache;
            if (CachedPlayers.TryGetValue(guid, out cache))
                return GameData.IsAllianceRace(cache.RaceId);
            return false;
        }
        public bool IsInBattleground()
        {
            if (CurrentMapId == null)
                return false;

            uint bgId = GameData.GetBattlegroundIdFromMapId((uint)CurrentMapId);
            if (bgId != 0)
            {
                // Only if we are properly queued for the BG.
                foreach (var queue in BattleFieldQueueTypes)
                {
                    if (LegacyVersion.RemovedInVersion(Enums.ClientVersionBuild.V2_0_1_6180))
                    {
                        if (queue.Value == CurrentMapId)
                            return true;
                    }
                    else
                    {
                        if (queue.Value == bgId)
                            return true;
                    }
                }
            }

            return false;
        }
        public long GetBattleFieldQueueTime(uint queueSlot)
        {
            if (BattleFieldQueueTimes.ContainsKey(queueSlot))
                return BattleFieldQueueTimes[queueSlot];
            else
            {
                long time = Time.UnixTime;
                BattleFieldQueueTimes.Add(queueSlot, time);
                return time;
            }
        }
        public void StoreBattleFieldQueueType(uint queueSlot, uint mapOrBgId)
        {
            if (BattleFieldQueueTypes.ContainsKey(queueSlot))
                BattleFieldQueueTypes[queueSlot] = mapOrBgId;
            else
                BattleFieldQueueTypes.Add(queueSlot, mapOrBgId);
        }
        public uint GetBattleFieldQueueType(uint queueSlot)
        {
            if (BattleFieldQueueTypes.ContainsKey(queueSlot))
                return BattleFieldQueueTypes[queueSlot];
            return 0;
        }
        public void StoreAuraDurationLeft(WowGuid128 guid, byte slot, int duration, int currentTime)
        {
            if (UnitAuraDurationLeft.ContainsKey(guid))
            {
                if (UnitAuraDurationLeft[guid].ContainsKey(slot))
                    UnitAuraDurationLeft[guid][slot] = duration;
                else
                    UnitAuraDurationLeft[guid].Add(slot, duration);
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(slot, duration);
                UnitAuraDurationLeft.Add(guid, dict);
            }

            if (UnitAuraDurationUpdateTime.ContainsKey(guid))
            {
                if (UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
                    UnitAuraDurationUpdateTime[guid][slot] = currentTime;
                else
                    UnitAuraDurationUpdateTime[guid].Add(slot, currentTime);
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(slot, currentTime);
                UnitAuraDurationUpdateTime.Add(guid, dict);
            }
        }
        public void StoreAuraDurationFull(WowGuid128 guid, byte slot, int duration)
        {
            if (UnitAuraDurationFull.ContainsKey(guid))
            {
                if (UnitAuraDurationFull[guid].ContainsKey(slot))
                    UnitAuraDurationFull[guid][slot] = duration;
                else
                    UnitAuraDurationFull[guid].Add(slot, duration);
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(slot, duration);
                UnitAuraDurationFull.Add(guid, dict);
            }
        }
        public void ClearAuraDuration(WowGuid128 guid, byte slot)
        {
            if (UnitAuraDurationUpdateTime.ContainsKey(guid) &&
                UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
                UnitAuraDurationUpdateTime[guid].Remove(slot);

            if (UnitAuraDurationLeft.ContainsKey(guid) &&
                UnitAuraDurationLeft[guid].ContainsKey(slot))
                UnitAuraDurationLeft[guid].Remove(slot);

            if (UnitAuraDurationFull.ContainsKey(guid) &&
                UnitAuraDurationFull[guid].ContainsKey(slot))
                UnitAuraDurationFull[guid].Remove(slot);
        }
        public void GetAuraDuration(WowGuid128 guid, byte slot, out int left, out int full)
        {
            if (UnitAuraDurationLeft.ContainsKey(guid) &&
                UnitAuraDurationLeft[guid].ContainsKey(slot))
                left = UnitAuraDurationLeft[guid][slot];
            else
                left = -1;

            if (UnitAuraDurationFull.ContainsKey(guid) &&
                UnitAuraDurationFull[guid].ContainsKey(slot))
                full = UnitAuraDurationFull[guid][slot];
            else
                full = left;

            if (left > 0 &&
                UnitAuraDurationUpdateTime.ContainsKey(guid) &&
                UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
                left = left - (System.Environment.TickCount - UnitAuraDurationUpdateTime[guid][slot]);
        }
        public void StoreAuraCaster(WowGuid128 target, byte slot, WowGuid128 caster)
        {
            if (UnitAuraCaster.ContainsKey(target))
            {
                if (UnitAuraCaster[target].ContainsKey(slot))
                    UnitAuraCaster[target][slot] = caster;
                else
                    UnitAuraCaster[target].Add(slot, caster);
            }
            else
            {
                Dictionary<byte, WowGuid128> dict = new Dictionary<byte, WowGuid128>();
                dict.Add(slot, caster);
                UnitAuraCaster.Add(target, dict);
            }
        }
        public void ClearAuraCaster(WowGuid128 guid, byte slot)
        {
            if (UnitAuraCaster.ContainsKey(guid) &&
                UnitAuraCaster[guid].ContainsKey(slot))
                UnitAuraCaster[guid].Remove(slot);
        }
        public WowGuid128 GetAuraCaster(WowGuid128 target, byte slot)
        {
            if (UnitAuraCaster.ContainsKey(target) &&
                UnitAuraCaster[target].ContainsKey(slot))
                return UnitAuraCaster[target][slot];

            return default;
        }
        public WowGuid128 GetAuraCaster(WowGuid128 target, byte slot, uint spellId)
        {
            WowGuid128 caster = GetAuraCaster(target, slot);
            if (caster == default)
            {
                caster = GetLastAuraCasterOnTarget(target, spellId);
                if (caster != default)
                    StoreAuraCaster(target, slot, caster);
            }

            return caster;
        }
        public void StoreLastAuraCasterOnTarget(WowGuid128 target, uint spellId, WowGuid128 caster)
        {
            if (LastAuraCasterOnTarget.ContainsKey(target))
            {
                if (LastAuraCasterOnTarget[target].ContainsKey(spellId))
                    LastAuraCasterOnTarget[target][spellId] = caster;
                else
                    LastAuraCasterOnTarget[target].Add(spellId, caster);
            }
            else
            {
                Dictionary<uint, WowGuid128> casterDict = new Dictionary<uint, WowGuid128>();
                casterDict.Add(spellId, caster);
                LastAuraCasterOnTarget.Add(target, casterDict);
            }
        }
        public WowGuid128 GetLastAuraCasterOnTarget(WowGuid128 target, uint spellId)
        {
            if (LastAuraCasterOnTarget.ContainsKey(target))
            {
                WowGuid128 caster;
                if (LastAuraCasterOnTarget[target].TryGetValue(spellId, out caster))
                {
                    LastAuraCasterOnTarget[target].Remove(spellId);
                    return caster;
                }
            }

            return default;
        }

        // Spell Cast Queue Helper Methods

        /// <summary>
        /// Try to find and dequeue a pending cast by SpellId.
        /// Uses FIFO order since TCP guarantees packet ordering.
        /// </summary>
        public bool TryDequeuePendingNormalCast(uint spellId, out ClientCastRequest? cast)
        {
            // Since TCP preserves order, the first matching SpellId is the correct one
            var pending = new List<ClientCastRequest>();
            cast = null;

            while (PendingNormalCasts.TryDequeue(out var current))
            {
                if (cast == null && current.SpellId == spellId)
                {
                    cast = current;
                }
                else
                {
                    pending.Add(current);
                }
            }

            // Re-enqueue non-matching casts
            foreach (var item in pending)
            {
                PendingNormalCasts.Enqueue(item);
            }

            return cast != null;
        }

        /// <summary>
        /// Try to find a pending cast by SpellId and mark it as started (for SPELL_START).
        /// </summary>
        public bool TryMarkPendingNormalCastStarted(uint spellId, out ClientCastRequest? cast)
        {
            cast = null;

            foreach (var item in PendingNormalCasts)
            {
                if (item.SpellId == spellId && !item.HasStarted)
                {
                    item.HasStarted = true;
                    cast = item;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clear all pending normal casts (used on timeout or disconnect).
        /// </summary>
        public void ClearPendingNormalCasts()
        {
            while (PendingNormalCasts.TryDequeue(out _)) { }
        }

        /// <summary>
        /// Check if there's a normal cast that has already started (is in progress).
        /// Used to reject new casts without forwarding to server.
        /// </summary>
        public bool HasStartedNormalCast()
        {
            foreach (var item in PendingNormalCasts)
            {
                if (item.HasStarted)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clear only pending normal casts that haven't started yet.
        /// Keeps started casts so SPELL_GO can dequeue them later.
        /// Returns the cleared casts so they can be failed.
        /// </summary>
        public List<ClientCastRequest> ClearNonStartedNormalCasts()
        {
            var cleared = new List<ClientCastRequest>();
            var keep = new List<ClientCastRequest>();

            while (PendingNormalCasts.TryDequeue(out var current))
            {
                if (current.HasStarted)
                    keep.Add(current);
                else
                    cleared.Add(current);
            }

            // Re-enqueue started casts
            foreach (var item in keep)
            {
                PendingNormalCasts.Enqueue(item);
            }

            return cleared;
        }

        /// <summary>
        /// Try to find and dequeue a pending pet cast by SpellId.
        /// </summary>
        public bool TryDequeuePendingPetCast(uint spellId, out ClientCastRequest? cast)
        {
            var pending = new List<ClientCastRequest>();
            cast = null;

            while (PendingPetCasts.TryDequeue(out var current))
            {
                if (cast == null && current.SpellId == spellId)
                {
                    cast = current;
                }
                else
                {
                    pending.Add(current);
                }
            }

            foreach (var item in pending)
            {
                PendingPetCasts.Enqueue(item);
            }

            return cast != null;
        }

        /// <summary>
        /// Try to find a pending pet cast by SpellId and mark it as started.
        /// </summary>
        public bool TryMarkPendingPetCastStarted(uint spellId, out ClientCastRequest? cast)
        {
            cast = null;

            foreach (var item in PendingPetCasts)
            {
                if (item.SpellId == spellId && !item.HasStarted)
                {
                    item.HasStarted = true;
                    cast = item;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clear all pending pet casts.
        /// </summary>
        public void ClearPendingPetCasts()
        {
            while (PendingPetCasts.TryDequeue(out _)) { }
        }

        /// <summary>
        /// Check if there's a pet cast that has already started (is in progress).
        /// Used to reject new casts without forwarding to server.
        /// </summary>
        public bool HasStartedPetCast()
        {
            foreach (var item in PendingPetCasts)
            {
                if (item.HasStarted)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clear only pending pet casts that haven't started yet.
        /// Keeps started casts so SPELL_GO can dequeue them later.
        /// Returns the cleared casts so they can be failed.
        /// </summary>
        public List<ClientCastRequest> ClearNonStartedPetCasts()
        {
            var cleared = new List<ClientCastRequest>();
            var keep = new List<ClientCastRequest>();

            while (PendingPetCasts.TryDequeue(out var current))
            {
                if (current.HasStarted)
                    keep.Add(current);
                else
                    cleared.Add(current);
            }

            // Re-enqueue started casts
            foreach (var item in keep)
            {
                PendingPetCasts.Enqueue(item);
            }

            return cleared;
        }

        /// <summary>
        /// Try to find and dequeue a pending cast by ItemGUID (for item use failures).
        /// Only matches casts that haven't started yet.
        /// </summary>
        public bool TryDequeueItemCast(WowGuid128 itemGuid, out ClientCastRequest? cast)
        {
            var pending = new List<ClientCastRequest>();
            cast = null;

            while (PendingNormalCasts.TryDequeue(out var current))
            {
                if (cast == null && !current.HasStarted && current.ItemGUID == itemGuid)
                {
                    cast = current;
                }
                else
                {
                    pending.Add(current);
                }
            }

            // Re-enqueue non-matching casts
            foreach (var item in pending)
            {
                PendingNormalCasts.Enqueue(item);
            }

            return cast != null;
        }

        public void StorePlayerGuildId(WowGuid128 guid, uint guildId)
        {
            if (PlayerGuildIds.ContainsKey(guid))
                PlayerGuildIds[guid] = guildId;
            else
                PlayerGuildIds.Add(guid, guildId);
        }
        public uint GetPlayerGuildId(WowGuid128 guid)
        {
            if (PlayerGuildIds.ContainsKey(guid))
                return PlayerGuildIds[guid];
            return 0;
        }
        public uint[]? GetGemsForItem(WowGuid128 guid)
        {
            if (ItemGems.ContainsKey(guid))
                return ItemGems[guid];
            return null;
        }
        public void SaveGemsForItem(WowGuid128 guid, uint?[] gems)
        {
            uint[] existing;
            if (ItemGems.ContainsKey(guid))
                existing = ItemGems[guid];
            else
            {
                existing = new uint[ItemConst.MaxGemSockets];
                ItemGems.Add(guid, existing);
            }

            for (int i = 0; i < ItemConst.MaxGemSockets; i++)
            {
                if (gems[i] != null)
                    existing[i] = (uint)gems[i]!;
            }
        }
        public WowGuid128 GetPetGuidByNumber(uint petNumber)
        {
            lock (ObjectCacheLock)
            {
                foreach (var itr in ObjectCacheModern)
                {
                    if (itr.Key.GetHighType() == HighGuidType.Pet &&
                        itr.Key.GetEntry() == petNumber)
                    {
                        return itr.Key;
                    }
                }
                return default;
            }
        }
        public void StoreOriginalObjectType(WowGuid128 guid, ObjectType type)
        {
            if (OriginalObjectTypes.ContainsKey(guid))
                OriginalObjectTypes[guid] = type;
            else
                OriginalObjectTypes.Add(guid, type);
        }
        public ObjectType GetOriginalObjectType(WowGuid128 guid)
        {
            if (OriginalObjectTypes.ContainsKey(guid))
                return OriginalObjectTypes[guid];

            return guid.GetObjectType();
        }
        public void StoreRealSpell(uint realSpellId, uint learnSpellId)
        {
            if (RealSpellToLearnSpell.ContainsKey(realSpellId))
                RealSpellToLearnSpell[realSpellId] = learnSpellId;
            else
                RealSpellToLearnSpell.Add(realSpellId, learnSpellId);
        }
        public uint GetLearnSpellFromRealSpell(uint spellId)
        {
            if (RealSpellToLearnSpell.ContainsKey(spellId))
                return RealSpellToLearnSpell[spellId];

            return spellId;
        }
        public void StoreCreatureClass(uint entry, Class classId)
        {
            if (CreatureClasses.ContainsKey(entry))
                CreatureClasses[entry] = classId;
            else
                CreatureClasses.Add(entry, classId);
        }
        public void SetItemBuyCount(uint itemId, uint buyCount)
        {
            if (ItemBuyCount.ContainsKey(itemId))
                ItemBuyCount[itemId] = buyCount;
            else
                ItemBuyCount.Add(itemId, buyCount);
        }
        public uint GetItemBuyCount(uint itemId)
        {
            if (ItemBuyCount.ContainsKey(itemId))
                return ItemBuyCount[itemId];

            return 1;
        }
        public void SetChannelId(string name, int id)
        {
            if (ChannelIds.ContainsKey(name))
                ChannelIds[name] = id;
            else
                ChannelIds.Add(name, id);
        }
        public string GetChannelName(int id)
        {
            foreach (var itr in ChannelIds)
            {
                if (itr.Value == id)
                    return itr.Key;
            }
            return "";
        }

        public string GetPlayerName(WowGuid128 guid)
        {
            if (CachedPlayers.ContainsKey(guid))
            {
                if (CachedPlayers[guid].Name != null)
                    return CachedPlayers[guid].Name!;
            }
            return "";
        }

        public WowGuid128 GetPlayerGuidByName(string name)
        {
            name = name.Trim().Replace("\0", "");
            foreach (var player in CachedPlayers)
            {
                if (player.Value.Name == name && !WowGuid128.IsUnknownPlayerGuid(player.Key))
                    return player.Key;
            }
            return default;
        }

        public void UpdatePlayerCache(WowGuid128 guid, PlayerCache data)
        {
            if (data.Name != null)
                data.Name = data.Name.Trim().Replace("\0", "");
            if (CachedPlayers.ContainsKey(guid))
            {
                if (!string.IsNullOrEmpty(data.Name))
                    CachedPlayers[guid].Name = data.Name;
                if (data.RaceId != Race.None)
                    CachedPlayers[guid].RaceId = data.RaceId;
                if (data.ClassId != Class.None)
                    CachedPlayers[guid].ClassId = data.ClassId;
                if (data.SexId != Gender.None)
                    CachedPlayers[guid].SexId = data.SexId;
                if (data.Level != 0)
                    CachedPlayers[guid].Level = data.Level;
            }
            else
                CachedPlayers.Add(guid, data);
        }

        public Class GetUnitClass(WowGuid128 guid)
        {
            if (CachedPlayers.ContainsKey(guid))
                return CachedPlayers[guid].ClassId;

            if (CreatureClasses.ContainsKey(guid.GetEntry()))
                return CreatureClasses[guid.GetEntry()];

            return Class.Warrior;
        }

        public int GetLegacyFieldValueInt32<T>(WowGuid128 guid, T field) where T : Enum
        {
            int fieldIndex = LegacyVersion.GetUpdateField(field);
            if (fieldIndex < 0)
                return 0;

            var updates = GetCachedObjectFieldsLegacy(guid);
            if (updates == null)
                return 0;

            if (!updates.ContainsKey(fieldIndex))
                return 0;

            return updates[fieldIndex].Int32Value;
        }

        public uint GetLegacyFieldValueUInt32<T>(WowGuid128 guid, T field) where T : Enum
        {
            int fieldIndex = LegacyVersion.GetUpdateField(field);
            if (fieldIndex < 0)
                return 0;

            var updates = GetCachedObjectFieldsLegacy(guid);
            if (updates == null)
                return 0;

            if (!updates.ContainsKey(fieldIndex))
                return 0;

            return updates[fieldIndex].UInt32Value;
        }

        public float GetLegacyFieldValueFloat<T>(WowGuid128 guid, T field) where T : Enum
        {
            int fieldIndex = LegacyVersion.GetUpdateField(field);
            if (fieldIndex < 0)
                return 0;

            var updates = GetCachedObjectFieldsLegacy(guid);
            if (updates == null)
                return 0;

            if (!updates.ContainsKey(fieldIndex))
                return 0;

            return updates[fieldIndex].FloatValue;
        }

        public Dictionary<int, UpdateField>? GetCachedObjectFieldsLegacy(WowGuid128 guid)
        {
            lock (ObjectCacheLock)
            {
                ObjectCacheLegacy.TryGetValue(guid, out var dict);
                return dict;
            }
        }

        public UpdateFieldsArray? GetCachedObjectFieldsModern(WowGuid128 guid)
        {
            lock (ObjectCacheLock)
            {
                ObjectCacheModern.TryGetValue(guid, out var array);
                return array;
            }
        }
    }

    public class ClientCastRequest
    {
        public bool HasStarted;
        public uint SpellId;
        public uint SpellXSpellVisualId;
        public long Timestamp;
        public WowGuid128 ClientGUID;
        public WowGuid128 ServerGUID;
        public WowGuid128 ItemGUID;
    }
    public class ArenaTeamData
    {
        public string Name = null!;
        public uint TeamSize;
        public uint WeekPlayed;
        public uint WeekWins;
        public uint SeasonPlayed;
        public uint SeasonWins;
        public uint Rating;
        public uint Rank;
        public uint BackgroundColor;
        public uint EmblemStyle;
        public uint EmblemColor;
        public uint BorderStyle;
        public uint BorderColor;
    }
    public class GlobalSessionData
    {
        public BNetServer.Networking.AccountInfo AccountInfo = null!;
        public BNetServer.Networking.GameAccountInfo GameAccountInfo = null!;
        public string Username = null!;
        public string LoginTicket = null!;
        public byte[] SessionKey = null!;
        public string Locale = null!;
        public string OS = null!;
        public uint Build;
        public GameSessionData GameState;
        
        public RealmId RealmId;
        public RealmManager RealmManager = new();
        public Realm? Realm => RealmManager.GetRealm(RealmId);

        public AccountMetaDataManager AccountMetaDataMgr = null!;
        public AccountDataManager AccountDataMgr = null!;

        public WorldSocket RealmSocket = null!;
        public WorldSocket InstanceSocket = null!;
        public AuthClient AuthClient = null!;
        public WorldClient? WorldClient;
        public SniffFile ModernSniff = null!;

        public Dictionary<string, WowGuid128> GuildsByName = new();
        public Dictionary<uint, List<string>> GuildRanks = new();

        public GlobalSessionData()
        {
            GameState = GameSessionData.CreateNewGameSessionData(this);
        }
        
        public void StoreGuildRankNames(uint guildId, List<string> ranks)
        {
            if (GuildRanks.ContainsKey(guildId))
                GuildRanks[guildId] = ranks;
            else
                GuildRanks.Add(guildId, ranks);
        }
        public uint GetGuildRankIdByName(uint guildId, string name)
        {
            if (GuildRanks.ContainsKey(guildId))
            {
                for (int i = 0; i < GuildRanks[guildId].Count; i++)
                {
                    if (GuildRanks[guildId][i] == name)
                        return (uint)i;
                }
            }
            return 0;
        }
        public string GetGuildRankNameById(uint guildId, byte rankId)
        {
            if (GuildRanks.ContainsKey(guildId))
                return GuildRanks[guildId][rankId];

            return $"Rank {rankId}";
        }
        public void StoreGuildGuidAndName(WowGuid128 guid, string name)
        {
            if (GuildsByName.ContainsKey(name))
                GuildsByName[name] = guid;
            else
                GuildsByName.Add(name, guid);
        }
        public WowGuid128 GetGuildGuid(string name)
        {
            if (GuildsByName.ContainsKey(name))
                return GuildsByName[name];

            WowGuid128 guid = WowGuid128.Create(HighGuidType703.Guild, (ulong)(GuildsByName.Count + 1));
            GuildsByName.Add(name, guid);
            return guid;
        }

        public WowGuid128 GetGameAccountGuidForPlayer(WowGuid128 playerGuid)
        {
            if (GameState.OwnCharacters.Any(own => own.CharacterGuid == playerGuid))
                return WowGuid128.Create(HighGuidType703.WowAccount, GameAccountInfo.Id);
            else
                return WowGuid128.Create(HighGuidType703.WowAccount, playerGuid.GetCounter());
        }

        public WowGuid128 GetBnetAccountGuidForPlayer(WowGuid128 playerGuid)
        {
            if (GameState.OwnCharacters.Any(own => own.CharacterGuid == playerGuid))
                return WowGuid128.Create(HighGuidType703.BNetAccount, AccountInfo.Id);
            else
                return WowGuid128.Create(HighGuidType703.BNetAccount, playerGuid.GetCounter());
        }

        public void OnDisconnect()
        {
            if (ModernSniff != null)
            {
                ModernSniff.CloseFile();
                ModernSniff = null!;
            }
            if (AuthClient != null)
            {
                AuthClient.Disconnect();
                AuthClient = null!;
            }
            if (WorldClient != null)
            {
                WorldClient.Disconnect();
                WorldClient = null;
            }
            if (RealmSocket != null)
            {
                RealmSocket.CloseSocket();
                RealmSocket = null!;
            }
            if (InstanceSocket != null)
            {
                InstanceSocket.CloseSocket();
                InstanceSocket = null!;
            }

            GameState = GameSessionData.CreateNewGameSessionData(this);
        }

        public void SendHermesTextMessage(string message, bool isError = false)
        {
            var socket = InstanceSocket;
            if (socket != null)
            {
                var wholeMessage = new StringBuilder();
                wholeMessage.Append("|cFF111111[|r|cFF33DD22HermesProxy|r|cFF111111]|r ");
                if (isError)
                    wholeMessage.Append("|cFFFF0000");
                wholeMessage.Append(message);
                
                var chatPkt = new ChatPkt(this, ChatMessageTypeModern.System, wholeMessage.ToString());
                socket.SendPacket(chatPkt);
            }
        }
    }
}
