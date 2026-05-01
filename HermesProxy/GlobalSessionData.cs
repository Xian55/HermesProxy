using HermesProxy.Auth;
using HermesProxy.Configuration.Options;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading;
using Framework.Realm;
using HermesProxy.World.Server.Packets;
using ArenaTeamInspectData = HermesProxy.World.Server.Packets.ArenaTeamInspectData;
using System;

namespace HermesProxy;

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

public sealed class PendingObjectUpdate
{
    public required UpdateObject UpdateObject;
    public required List<AuraUpdate> AuraUpdates;
    public required HashSet<uint> WaitingForItemIds;
}

public sealed class GameSessionData
{
    // Back-reference to the owning session. Set by CreateNewGameSessionData. Used by writers
    // (e.g. ObjectUpdateBuilder) that need helpers on GlobalSessionData like
    // GetBnetAccountGuidForPlayer without threading the session through every call.
    public GlobalSessionData GlobalSession = null!;
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
    // Realm-destined packets queued while RealmSocket is null (modern client's BNet→Realm
    // handoff hasn't completed yet but the legacy server is already sending early-session
    // packets like SMSG_TUTORIAL_FLAGS). Flushed in WorldSocket.HandleEnterEncryptedModeAck
    // when RealmSocket is assigned.
    public Queue<ServerPacket> PendingRealmPackets = new();
    public readonly Lock PendingRealmPacketsLock = new();
    public bool IsInWorld;
    public uint? CurrentMapId;
    public uint CurrentZoneId;
    public uint CurrentTaxiNode;
    public List<byte> UsableTaxiNodes = [];
    public uint PendingTransferMapId;
    public uint LastEnteredAreaTrigger;
    public uint LastDispellSpellId;
    public string LeftChannelName = "";
    public bool IsPassingOnLoot;
    public int GroupUpdateCounter;
    public uint GroupReadyCheckResponses;
    public World.Server.Packets.PartyUpdate?[] CurrentGroups = new World.Server.Packets.PartyUpdate?[2];
    public bool WeWantToLeaveGroup; // Only send kick message when we dont initiated the group-leave
    public List<OwnCharacterInfo> OwnCharacters = [];
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
    public List<int> ActionButtons = [];
    public ushort[] ActiveGlyphs = new ushort[6];
    public byte GlyphsEnabled;
    public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationUpdateTime = [];
    public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationLeft = [];
    public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationFull = [];
    public Dictionary<WowGuid128, Dictionary<byte, WowGuid128>> UnitAuraCaster = [];
    public Dictionary<WowGuid128, PlayerCache> CachedPlayers = [];
    public HashSet<WowGuid128> IgnoredPlayers = [];
    public Dictionary<WowGuid128, uint> PlayerGuildIds = [];
    public readonly Lock ObjectCacheLock = new();
    public Dictionary<WowGuid128, Dictionary<int, UpdateField>> ObjectCacheLegacy = [];
    public Dictionary<WowGuid128, UpdateFieldsArray> ObjectCacheModern = [];
    public Dictionary<WowGuid128, ObjectType> OriginalObjectTypes = [];
    public Dictionary<WowGuid128, uint[]> ItemGems = [];
    public Dictionary<uint, Class> CreatureClasses = [];

    // Per-GUID monster-move throttle. The V3_4_3 client OOMs when it receives
    // hundreds of monster-moves per minute (observed: legacy server emits ~20/sec
    // for nearby patrols, client allocates per-move and never frees fast enough).
    // We drop moves for the same GUID arriving within MonsterMoveMinIntervalMs
    // of the previous one for that GUID.
    public Dictionary<WowGuid128, long> LastMonsterMoveTickMs = [];
    public const int MonsterMoveMinIntervalMs = 250;
    public Dictionary<string, int> ChannelIds = [];
    public Dictionary<int, string> ChannelNamesById = [];
    public Dictionary<uint, uint> ItemBuyCount = [];
    public Dictionary<uint, uint> RealSpellToLearnSpell = [];
    public Dictionary<uint, ArenaTeamData> ArenaTeams = [];
    public World.Server.Packets.MailListResult? PendingMailListPacket;
    public HashSet<uint> RequestedItemTextIds = [];
    public Dictionary<uint, string> ItemTexts = [];
    public Dictionary<uint, uint> BattleFieldQueueTypes = [];
    public Dictionary<uint, long> BattleFieldQueueTimes = [];
    public Dictionary<uint, uint> DailyQuestsDone = [];
    public HashSet<WowGuid128> FlagCarrierGuids = [];
    public Dictionary<WowGuid64, ushort> ObjectSpawnCount = [];
    public HashSet<WowGuid64> DespawnedGameObjects = [];
    public HashSet<WowGuid128> HunterPetGuids = [];
    // GUIDs for which we've successfully forwarded a modern CreateObject to the V3_4_3
    // client. Used by the Values-update filter so we don't ship deltas for objects the
    // client doesn't have in its world model — those would round-trip as
    // CMSG_OBJECT_UPDATE_FAILED rejections (e.g. Transports we filter at create time).
    public HashSet<WowGuid128> ClientKnownGuids = [];
    public Dictionary<WowGuid128, ArenaTeamInspectData[]> PlayerArenaTeams = [];
    public HashSet<string> AddonPrefixes = [];
    public Dictionary<byte, Dictionary<byte, int>> FlatSpellMods = [];
    public Dictionary<byte, Dictionary<byte, int>> PctSpellMods = [];
    public Dictionary<WowGuid128, Dictionary<uint, WowGuid128>> LastAuraCasterOnTarget = [];
    public TradeSession? CurrentTrade = null;
    public HashSet<uint> RequestedItemHotfixes = [];
    public HashSet<uint> RequestedItemSparseHotfixes = [];

    // SMSG_UPDATE_OBJECT batches that contain a player CreateObject whose
    // VisibleItems reference item templates not yet cached. Held back until
    // the matching ItemModifiedAppearance hotfixes are emitted, so the modern
    // client renders the player dressed instead of naked. See issue #34.
    public List<PendingObjectUpdate> DeferredObjectUpdates = [];
    public Lock DeferredObjectUpdatesLock = new();

    // Cache of the last SMSG_INIT_WORLD_STATES we sent to the modern client. TC reference
    // re-emits INIT_WORLD_STATES AFTER the player CreateObject (#146 in World_login_parsed.txt),
    // but cmangos sends it earlier and we forward immediately. The deferred-flush re-emits this
    // cached copy AFTER the player Create so the V3_4_3 client sees TC's ordering.
    public InitWorldStates? LastInitWorldStates;

    private GameSessionData()
    {
        
    }

    public static GameSessionData CreateNewGameSessionData(GlobalSessionData globalSession)
    {
        var self = new GameSessionData();
        self.GlobalSession = globalSession;
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
    /// <summary>
    /// If the modern client sent a spell id that the legacy server doesn't know for this item
    /// (e.g. SoM 1.14.1+ renumbered Diamond Flask 17626 → 363880), resolve the legacy spell id
    /// from the item's cached ItemEffects (slot 0 = on-use trinket/potion entry).
    /// Returns 0 when no remap is needed (modern id == legacy id) or when item data isn't cached yet.
    /// </summary>
    public uint GetLegacyItemSpellId(WowGuid128 itemGuid, uint modernSpellId)
    {
        uint itemId = GetItemId(itemGuid);
        if (itemId == 0)
            return 0;

        var slotMap = GameData.GetItemEffectSlotMap(itemId);
        if (slotMap == null)
            return 0;

        // Modern spell id is already known to the legacy server — no remap needed.
        if (slotMap.ContainsKey(modernSpellId))
            return 0;

        // On-use items keep their effect at slot 0; return that legacy spell id.
        foreach (var kvp in slotMap)
        {
            if (kvp.Value == 0)
            {
                // Also remember the legacy → modern direction so subsequent aura updates
                // (which carry the legacy spell id) can be translated back to the modern id
                // the client recognizes — otherwise the buff icon never appears next to the minimap.
                // We learn it here from the client's actual CMSG_USE_ITEM rather than relying on
                // ItemEffect CSV data, which can be stale for SoM-renumbered items.
                GameData.LegacyToModernSpellId[kvp.Key] = modernSpellId;
                return kvp.Key;
            }
        }
        return 0;
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
        ref var dict = ref CollectionsMarshal.GetValueRefOrAddDefault(FlatSpellMods, spellMod, out _);
        dict ??= [];
        dict[spellMask] = amount;
    }
    public void SetPctSpellMod(byte spellMod, byte spellMask, int amount)
    {
        ref var dict = ref CollectionsMarshal.GetValueRefOrAddDefault(PctSpellMods, spellMod, out _);
        dict ??= [];
        dict[spellMask] = amount;
    }
    public ArenaTeamInspectData GetArenaTeamDataForPlayer(WowGuid128 guid, byte slot)
    {
        if (PlayerArenaTeams.TryGetValue(guid, out var teams) && teams[slot] != null)
            return teams[slot];

        return new ArenaTeamInspectData();
    }
    public void StoreArenaTeamDataForPlayer(WowGuid128 guid, byte slot, ArenaTeamInspectData team)
    {
        ref var teams = ref CollectionsMarshal.GetValueRefOrAddDefault(PlayerArenaTeams, guid, out _);
        teams ??= new ArenaTeamInspectData[ArenaTeamConst.MaxArenaSlot];
        teams[slot] = team;
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
        // Main backpack: read directly from player inventory fields
        if (containerSlot == ItemConst.NullSlot)
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
        // Search main backpack
        for (int i = World.Enums.Vanilla.InventorySlots.ItemStart; i < World.Enums.Vanilla.InventorySlots.ItemEnd; i++)
        {
            if (GetInventorySlotItem(i) == itemGuid64)
                return (ItemConst.NullSlot, (byte)i);
        }

        // Search extra bag containers
        int containerSlotField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_SLOT_1);
        int numSlotsField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_NUM_SLOTS);
        if (containerSlotField < 0 || numSlotsField < 0)
            return null;

        for (int bagIdx = World.Enums.Vanilla.InventorySlots.BagStart; bagIdx < World.Enums.Vanilla.InventorySlots.BagEnd; bagIdx++)
        {
            var bagGuid64 = GetInventorySlotItem(bagIdx);
            if (bagGuid64 == WowGuid64.Empty)
                continue;

            var bagGuid128 = bagGuid64.To128(this);
            var bagFields = GetCachedObjectFieldsLegacy(bagGuid128);
            if (bagFields == null)
                continue;

            if (!bagFields.TryGetValue(numSlotsField, out var numSlotsValue))
                continue;
            int numSlots = (int)numSlotsValue.UInt32Value;

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
        for (int i = World.Enums.Vanilla.InventorySlots.ItemStart; i < World.Enums.Vanilla.InventorySlots.ItemEnd; i++)
        {
            if (GetInventorySlotItem(i) == WowGuid64.Empty)
                return (ItemConst.NullSlot, (byte)i);
        }

        // Search extra bag containers
        int containerSlotField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_SLOT_1);
        int numSlotsField = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_NUM_SLOTS);
        if (containerSlotField < 0 || numSlotsField < 0)
            return null;

        for (int bagIdx = World.Enums.Vanilla.InventorySlots.BagStart; bagIdx < World.Enums.Vanilla.InventorySlots.BagEnd; bagIdx++)
        {
            var bagGuid64 = GetInventorySlotItem(bagIdx);
            if (bagGuid64 == WowGuid64.Empty)
                continue;

            var bagGuid128 = bagGuid64.To128(this);
            var bagFields = GetCachedObjectFieldsLegacy(bagGuid128);
            if (bagFields == null)
                continue;

            if (!bagFields.TryGetValue(numSlotsField, out var numSlotsValue))
                continue;
            int numSlots = (int)numSlotsValue.UInt32Value;

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
        ref ushort count = ref CollectionsMarshal.GetValueRefOrAddDefault(ObjectSpawnCount, guid, out bool existed);
        if (existed)
            count++;
        // else: default(ushort) = 0, matching the original "Add(guid, 0)" behavior.
    }
    public void SetDailyQuestSlot(uint slot, uint questId)
    {
        if (questId != 0)
            DailyQuestsDone[slot] = questId;
        else
            DailyQuestsDone.Remove(slot);
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
        if (bgId == 0)
        {
            return false;
        }

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

        return false;
    }
    public long GetBattleFieldQueueTime(uint queueSlot)
    {
        if (BattleFieldQueueTimes.TryGetValue(queueSlot, out var time))
            return time;

        time = Time.UnixTime;
        BattleFieldQueueTimes.Add(queueSlot, time);
        return time;
    }
    public void StoreBattleFieldQueueType(uint queueSlot, uint mapOrBgId)
    {
        BattleFieldQueueTypes[queueSlot] = mapOrBgId;
    }
    public uint GetBattleFieldQueueType(uint queueSlot)
    {
        return BattleFieldQueueTypes.TryGetValue(queueSlot, out var value) ? value : 0u;
    }
    public void StoreAuraDurationLeft(WowGuid128 guid, byte slot, int duration, int currentTime)
    {
        ref var leftDict = ref CollectionsMarshal.GetValueRefOrAddDefault(UnitAuraDurationLeft, guid, out _);
        leftDict ??= [];
        leftDict[slot] = duration;

        ref var timeDict = ref CollectionsMarshal.GetValueRefOrAddDefault(UnitAuraDurationUpdateTime, guid, out _);
        timeDict ??= [];
        timeDict[slot] = currentTime;
    }
    public void StoreAuraDurationFull(WowGuid128 guid, byte slot, int duration)
    {
        ref var dict = ref CollectionsMarshal.GetValueRefOrAddDefault(UnitAuraDurationFull, guid, out _);
        dict ??= [];
        dict[slot] = duration;
    }
    public void ClearAuraDuration(WowGuid128 guid, byte slot)
    {
        if (UnitAuraDurationUpdateTime.TryGetValue(guid, out var timeDict))
            timeDict.Remove(slot);

        if (UnitAuraDurationLeft.TryGetValue(guid, out var leftDict))
            leftDict.Remove(slot);

        if (UnitAuraDurationFull.TryGetValue(guid, out var fullDict))
            fullDict.Remove(slot);
    }
    public void GetAuraDuration(WowGuid128 guid, byte slot, out int left, out int full)
    {
        left = -1;
        if (UnitAuraDurationLeft.TryGetValue(guid, out var leftDict) &&
            leftDict.TryGetValue(slot, out var leftVal))
            left = leftVal;

        full = left;
        if (UnitAuraDurationFull.TryGetValue(guid, out var fullDict) &&
            fullDict.TryGetValue(slot, out var fullVal))
            full = fullVal;

        if (left > 0 &&
            UnitAuraDurationUpdateTime.TryGetValue(guid, out var timeDict) &&
            timeDict.TryGetValue(slot, out var time))
            left -= Environment.TickCount - time;
    }
    public void StoreAuraCaster(WowGuid128 target, byte slot, WowGuid128 caster)
    {
        ref var dict = ref CollectionsMarshal.GetValueRefOrAddDefault(UnitAuraCaster, target, out _);
        dict ??= [];
        dict[slot] = caster;
    }
    public void ClearAuraCaster(WowGuid128 guid, byte slot)
    {
        if (UnitAuraCaster.TryGetValue(guid, out var dict))
            dict.Remove(slot);
    }
    public WowGuid128 GetAuraCaster(WowGuid128 target, byte slot)
    {
        if (UnitAuraCaster.TryGetValue(target, out var dict) &&
            dict.TryGetValue(slot, out var caster))
            return caster;

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
        ref var dict = ref CollectionsMarshal.GetValueRefOrAddDefault(LastAuraCasterOnTarget, target, out _);
        dict ??= [];
        dict[spellId] = caster;
    }
    public WowGuid128 GetLastAuraCasterOnTarget(WowGuid128 target, uint spellId)
    {
        if (LastAuraCasterOnTarget.TryGetValue(target, out var dict) &&
            dict.TryGetValue(spellId, out var caster))
        {
            dict.Remove(spellId);
            return caster;
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
            if (cast == null && CastMatchesSpellId(current, spellId))
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
    /// Match a pending cast against an incoming server spellId, accepting either
    /// the modern (client-sent) SpellId or the LegacySpellId we resolved at item-use time.
    /// Needed for SoM 1.14.1+ items where Blizzard renumbered the on-use spell id
    /// (e.g. Diamond Flask 17626 → 363880); the legacy emulator still replies with the old id.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CastMatchesSpellId(ClientCastRequest cast, uint spellId)
    {
        return cast.SpellId == spellId || (cast.LegacySpellId != 0 && cast.LegacySpellId == spellId);
    }

    /// <summary>
    /// Try to find a pending cast by SpellId and mark it as started (for SPELL_START).
    /// </summary>
    public bool TryMarkPendingNormalCastStarted(uint spellId, out ClientCastRequest? cast)
    {
        cast = null;

        foreach (var item in PendingNormalCasts)
        {
            if (CastMatchesSpellId(item, spellId) && !item.HasStarted)
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
            if (cast == null && CastMatchesSpellId(current, spellId))
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
            if (CastMatchesSpellId(item, spellId) && !item.HasStarted)
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
        PlayerGuildIds[guid] = guildId;
    }
    public uint GetPlayerGuildId(WowGuid128 guid)
    {
        return PlayerGuildIds.TryGetValue(guid, out var value) ? value : 0u;
    }
    public uint[]? GetGemsForItem(WowGuid128 guid)
    {
        return ItemGems.TryGetValue(guid, out var gems) ? gems : null;
    }
    public void SaveGemsForItem(WowGuid128 guid, uint?[] gems)
    {
        ref var existing = ref CollectionsMarshal.GetValueRefOrAddDefault(ItemGems, guid, out _);
        existing ??= new uint[ItemConst.MaxGemSockets];

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
        OriginalObjectTypes[guid] = type;
    }
    public ObjectType GetOriginalObjectType(WowGuid128 guid)
    {
        return OriginalObjectTypes.TryGetValue(guid, out var type) ? type : guid.GetObjectType();
    }
    public void StoreRealSpell(uint realSpellId, uint learnSpellId)
    {
        RealSpellToLearnSpell[realSpellId] = learnSpellId;
    }
    public uint GetLearnSpellFromRealSpell(uint spellId)
    {
        return RealSpellToLearnSpell.TryGetValue(spellId, out var learnSpell) ? learnSpell : spellId;
    }
    public void StoreCreatureClass(uint entry, Class classId)
    {
        CreatureClasses[entry] = classId;
    }
    public void SetItemBuyCount(uint itemId, uint buyCount)
    {
        ItemBuyCount[itemId] = buyCount;
    }
    public uint GetItemBuyCount(uint itemId)
    {
        return ItemBuyCount.TryGetValue(itemId, out var count) ? count : 1u;
    }
    public void SetChannelId(string name, int id)
    {
        // If the name was previously mapped to a different id, evict the stale
        // reverse entry so ChannelNamesById can't accumulate dead ids.
        if (ChannelIds.TryGetValue(name, out var oldId) && oldId != id)
            ChannelNamesById.Remove(oldId);

        ChannelIds[name] = id;
        ChannelNamesById[id] = name;
    }
    public string GetChannelName(int id)
    {
        return ChannelNamesById.TryGetValue(id, out var name) ? name : "";
    }

    public string GetPlayerName(WowGuid128 guid)
    {
        if (CachedPlayers.TryGetValue(guid, out var cache) && cache.Name != null)
            return cache.Name;
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

        if (CachedPlayers.TryGetValue(guid, out var existing))
        {
            if (!string.IsNullOrEmpty(data.Name))
                existing.Name = data.Name;
            if (data.RaceId != Race.None)
                existing.RaceId = data.RaceId;
            if (data.ClassId != Class.None)
                existing.ClassId = data.ClassId;
            if (data.SexId != Gender.None)
                existing.SexId = data.SexId;
            if (data.Level != 0)
                existing.Level = data.Level;
        }
        else
            CachedPlayers.Add(guid, data);
    }

    public Class GetUnitClass(WowGuid128 guid)
    {
        if (CachedPlayers.TryGetValue(guid, out var cache))
            return cache.ClassId;

        if (CreatureClasses.TryGetValue(guid.GetEntry(), out var classId))
            return classId;

        return Class.Warrior;
    }

    public int GetLegacyFieldValueInt32<T>(WowGuid128 guid, T field) where T : Enum
    {
        int fieldIndex = LegacyVersion.GetUpdateField(field);
        if (fieldIndex < 0)
            return 0;

        var updates = GetCachedObjectFieldsLegacy(guid);
        if (updates != null && updates.TryGetValue(fieldIndex, out var value))
            return value.Int32Value;

        return 0;
    }

    public uint GetLegacyFieldValueUInt32<T>(WowGuid128 guid, T field) where T : Enum
    {
        int fieldIndex = LegacyVersion.GetUpdateField(field);
        if (fieldIndex < 0)
            return 0;

        var updates = GetCachedObjectFieldsLegacy(guid);
        if (updates != null && updates.TryGetValue(fieldIndex, out var value))
            return value.UInt32Value;

        return 0;
    }

    public float GetLegacyFieldValueFloat<T>(WowGuid128 guid, T field) where T : Enum
    {
        int fieldIndex = LegacyVersion.GetUpdateField(field);
        if (fieldIndex < 0)
            return 0;

        var updates = GetCachedObjectFieldsLegacy(guid);
        if (updates != null && updates.TryGetValue(fieldIndex, out var value))
            return value.FloatValue;

        return 0;
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
    public uint LegacySpellId; // 0 = same as SpellId; non-zero when modern client used a renumbered spell (e.g. SoM 1.14.1+ items)
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
    public RealmManager RealmManager;
    public Realm? Realm => RealmManager.GetRealm(RealmId);

    public AccountMetaDataManager AccountMetaDataMgr = null!;
    public AccountDataManager AccountDataMgr = null!;

    public WorldSocket RealmSocket = null!;
    public WorldSocket InstanceSocket = null!;
    public AuthClient AuthClient = null!;
    public WorldClient? WorldClient;
    public SniffFile ModernSniff = null!;
    // Sniff of the cMangos↔HermesProxy legacy stream. Created lazily by WorldClient
    // on the first incoming SMSG once decryption is established. Used to capture an
    // (almost-)unaltered view of what the legacy server sends, so we can diff cMangos's
    // CreateObject content against TC's reference parse for the V3_4_3 canary investigation.
    public SniffFile LegacySniff = null!;

    public Dictionary<string, WowGuid128> GuildsByName = [];
    public Dictionary<uint, List<string>> GuildRanks = [];

    // Snapshots of the proxy-wide options captured at session creation.
    // Used by per-session components (AuthClient, WorldClient, packet log) to read
    // configuration without static Framework.Settings reads.
    public ClientOptions ClientOptions { get; }
    public LegacyServerOptions LegacyServerOptions { get; }
    public ProxyNetworkOptions NetworkOptions { get; }
    public DiagnosticsOptions DiagnosticsOptions { get; }

    // Pre-computed read-only struct used on the per-packet LogPacket hot path to avoid
    // an IOptions<T> getter chain on every send/recv.
    public PacketLogContext PacketLogContext { get; }

    public GlobalSessionData(
        ClientOptions clientOptions,
        LegacyServerOptions legacyServerOptions,
        ProxyNetworkOptions networkOptions,
        DiagnosticsOptions diagnosticsOptions)
    {
        ClientOptions = clientOptions;
        LegacyServerOptions = legacyServerOptions;
        NetworkOptions = networkOptions;
        DiagnosticsOptions = diagnosticsOptions;
        PacketLogContext = new PacketLogContext(diagnosticsOptions.PacketsLog, clientOptions.ClientBuild);

        RealmManager = new RealmManager(clientOptions, networkOptions);
        GameState = GameSessionData.CreateNewGameSessionData(this);
    }
    
    public void StoreGuildRankNames(uint guildId, List<string> ranks)
    {
        GuildRanks[guildId] = ranks;
    }
    public uint GetGuildRankIdByName(uint guildId, string name)
    {
        if (GuildRanks.TryGetValue(guildId, out var ranks))
        {
            for (int i = 0; i < ranks.Count; i++)
            {
                if (ranks[i] == name)
                    return (uint)i;
            }
        }
        return 0;
    }
    public string GetGuildRankNameById(uint guildId, byte rankId)
    {
        if (GuildRanks.TryGetValue(guildId, out var ranks))
            return ranks[rankId];

        return $"Rank {rankId}";
    }
    public void StoreGuildGuidAndName(WowGuid128 guid, string name)
    {
        GuildsByName[name] = guid;
    }
    public WowGuid128 GetGuildGuid(string name)
    {
        if (GuildsByName.TryGetValue(name, out var guid))
            return guid;

        guid = WowGuid128.Create(HighGuidType703.Guild, (ulong)(GuildsByName.Count + 1));
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
        if (LegacySniff != null)
        {
            LegacySniff.CloseFile();
            LegacySniff = null!;
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
        if (socket == null)
        {
            return;
        }

        var wholeMessage = new StringBuilder();
        wholeMessage.Append("|cFF111111[|r|cFF33DD22HermesProxy|r|cFF111111]|r ");
        if (isError)
            wholeMessage.Append("|cFFFF0000");
        wholeMessage.Append(message);

        var chatPkt = new ChatPkt(this, ChatMessageTypeModern.System, wholeMessage.ToString());
        socket.SendPacket(chatPkt);
    }
}
