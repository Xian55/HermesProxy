using Framework.IO;
using Framework.Logging;

using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

using nietras.SeparatedValues;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HermesProxy.World
{
    public static partial class GameData
    {
        // From CSV
        public static Dictionary<uint/*Build*/, Dictionary<string /*Platform*/, byte[] /*seed*/>> BuildAuthSeeds = [];
        public static SortedDictionary<uint, BroadcastText> BroadcastTextStore = [];
        public static FrozenDictionary<uint, uint> ItemDisplayIdStore = FrozenDictionary<uint, uint>.Empty;
        public static FrozenDictionary<uint, uint> ItemDisplayIdToFileDataIdStore = FrozenDictionary<uint, uint>.Empty;
        public static FrozenDictionary<uint, ItemSpellsData> ItemSpellsDataStore = FrozenDictionary<uint, ItemSpellsData>.Empty;
        public static Dictionary<uint, ItemRecord> ItemRecordsStore = [];
        public static Dictionary<uint, ItemSparseRecord> ItemSparseRecordsStore = [];
        public static Dictionary<uint, ItemAppearance> ItemAppearanceStore = [];
        public static Dictionary<uint, ItemModifiedAppearance> ItemModifiedAppearanceStore = [];
        public static Dictionary<uint, ItemEffect> ItemEffectStore = [];
        public static FrozenDictionary<uint, Battleground> Battlegrounds = FrozenDictionary<uint, Battleground>.Empty;
        public static FrozenDictionary<uint, ChatChannel> ChatChannels = FrozenDictionary<uint, ChatChannel>.Empty;
        public static Dictionary<uint, Dictionary<uint, byte>> ItemEffects = [];
        public static FrozenDictionary<uint, uint> ItemEnchantVisuals = FrozenDictionary<uint, uint>.Empty;
        public static FrozenDictionary<uint, uint> SpellVisuals = FrozenDictionary<uint, uint>.Empty;
        public static FrozenDictionary<uint, uint> LearnSpells = FrozenDictionary<uint, uint>.Empty;
        public static FrozenDictionary<uint, uint> TotemSpells = FrozenDictionary<uint, uint>.Empty;
        public static FrozenDictionary<uint, uint> Gems = FrozenDictionary<uint, uint>.Empty;
        public static FrozenDictionary<uint, CreatureDisplayInfo> CreatureDisplayInfos = FrozenDictionary<uint, CreatureDisplayInfo>.Empty;
        public static FrozenDictionary<uint, CreatureModelCollisionHeight> CreatureModelCollisionHeights = FrozenDictionary<uint, CreatureModelCollisionHeight>.Empty;
        public static FrozenDictionary<uint, uint> TransportPeriods = FrozenDictionary<uint, uint>.Empty;
        public static FrozenDictionary<uint, string> AreaNames = FrozenDictionary<uint, string>.Empty;
        public static FrozenDictionary<uint, uint> RaceFaction = FrozenDictionary<uint, uint>.Empty;
        public static HashSet<uint> DispellSpells = [];
        public static Dictionary<uint, List<float>> SpellEffectPoints = [];
        public static HashSet<uint> StackableAuras = [];
        public static HashSet<uint> MountAuras = [];
        public static HashSet<uint> NextMeleeSpells = [];
        public static HashSet<uint> AutoRepeatSpells = [];
        public static HashSet<uint> AuraSpells = [];
        public static FrozenDictionary<uint, int> AuraDurations = FrozenDictionary<uint, int>.Empty;
        public static FrozenDictionary<uint, TaxiPath> TaxiPaths = FrozenDictionary<uint, TaxiPath>.Empty;
        public static int[,] TaxiNodesGraph = new int[250, 250];
        public static FrozenDictionary<uint /*questId*/, uint /*questBit*/> QuestBits = FrozenDictionary<uint, uint>.Empty;

        // From Server
        public static Dictionary<uint, ItemTemplate> ItemTemplates = [];
        public static Dictionary<uint, CreatureTemplate> CreatureTemplates = [];
        public static Dictionary<uint, QuestTemplate> QuestTemplates = [];
        public static Dictionary<uint, string> ItemNames = [];

        #region GettersAndSetters
        public static void StoreItemName(uint entry, string name)
        {
            CollectionsMarshal.GetValueRefOrAddDefault(ItemNames, entry, out _) = name;
        }

        public static string GetItemName(uint entry)
        {
            if (ItemNames.TryGetValue(entry, out var data))
                return data;

            ItemTemplate template = GetItemTemplate(entry);
            if (template != null)
                return template.Name[0];

            return "";
        }

        public static void StoreItemTemplate(uint entry, ItemTemplate template)
        {
            CollectionsMarshal.GetValueRefOrAddDefault(ItemTemplates, entry, out _) = template;
        }

        public static ItemTemplate GetItemTemplate(uint entry)
        {
            if (ItemTemplates.TryGetValue(entry, out var data))
                return data;

            return null;
        }

        public static void StoreQuestTemplate(uint entry, QuestTemplate template)
        {
            CollectionsMarshal.GetValueRefOrAddDefault(QuestTemplates, entry, out _) = template;
        }

        public static QuestTemplate GetQuestTemplate(uint entry)
        {
            QuestTemplate data;
            if (QuestTemplates.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static QuestObjective GetQuestObjectiveForItem(uint entry)
        {
            foreach (var quest in QuestTemplates)
            {
                foreach (var objective in quest.Value.Objectives)
                {
                    if (objective.ObjectID == entry &&
                        objective.Type == QuestObjectiveType.Item)
                        return objective;
                }
            }
            return null;
        }

        public static uint? GetUniqueQuestBit(uint questId)
        {
            if (!QuestBits.TryGetValue(questId, out var result))
                return null;

            return result;
        }
        public static void StoreCreatureTemplate(uint entry, CreatureTemplate template)
        {
            CollectionsMarshal.GetValueRefOrAddDefault(CreatureTemplates, entry, out _) = template;
        }

        public static CreatureTemplate GetCreatureTemplate(uint entry)
        {
            if (CreatureTemplates.TryGetValue(entry, out var data))
                return data;

            return null;
        }

        public static uint GetItemDisplayId(uint entry)
        {
            if (ItemDisplayIdStore.TryGetValue(entry, out var displayId))
                return displayId;
            return 0;
        }

        public static uint GetItemIdWithDisplayId(uint displayId)
        {
            foreach (var item in ItemDisplayIdStore)
            {
                if (item.Value == displayId)
                    return item.Key;
            }
            return 0;
        }

        public static ItemAppearance GetItemAppearanceByDisplayId(uint displayId)
        {
            foreach (var item in ItemAppearanceStore)
            {
                if (item.Value.ItemDisplayInfoID == (int)displayId)
                    return item.Value;
            }
            return null;
        }

        public static ItemAppearance GetItemAppearanceByItemId(uint itemId)
        {
            ItemModifiedAppearance modAppearance = GetItemModifiedAppearanceByItemId(itemId);
            if (modAppearance == null)
                return null;

            ItemAppearance data;
            if (ItemAppearanceStore.TryGetValue((uint)modAppearance.ItemAppearanceID, out data))
                return data;
            return null;
        }

        public static uint GetItemIconFileDataIdByDisplayId(uint displayId)
        {
            uint fileDataId;
            if (ItemDisplayIdToFileDataIdStore.TryGetValue(displayId, out fileDataId))
                return fileDataId;
            return 0;
        }

        public static ItemModifiedAppearance GetItemModifiedAppearanceByDisplayId(uint displayId)
        {
            ItemAppearance appearance = GetItemAppearanceByDisplayId(displayId);
            if (appearance != null)
            {
                foreach (var item in ItemModifiedAppearanceStore)
                {
                    if (item.Value.ItemAppearanceID == appearance.Id)
                        return item.Value;
                }
            }
            return null;
        }

        public static ItemModifiedAppearance GetItemModifiedAppearanceByItemId(uint itemId)
        {
            foreach (var item in ItemModifiedAppearanceStore)
            {
                if (item.Value.ItemID == (int)itemId)
                    return item.Value;
            }
            return null;
        }

        public static ItemEffect GetItemEffectByItemId(uint itemId, byte slot)
        {
            foreach (var item in ItemEffectStore)
            {
                if (item.Value.ParentItemID == itemId && item.Value.LegacySlotIndex == slot)
                    return item.Value;
            }
            return null;
        }

        public static uint GetFirstFreeId<T>(Dictionary<uint, T> dict, uint after = 0)
        {
            uint candidate = after + 1;
            while (dict.ContainsKey(candidate))
                candidate++;
            return candidate;
        }

        public static void SaveItemEffectSlot(uint itemId, uint spellId, byte slot)
        {
            ref var innerDict = ref CollectionsMarshal.GetValueRefOrAddDefault(ItemEffects, itemId, out bool exists);
            if (!exists)
                innerDict = [];

            CollectionsMarshal.GetValueRefOrAddDefault(innerDict, spellId, out _) = slot;
        }

        public static byte GetItemEffectSlot(uint itemId, uint spellId)
        {
            ref var innerDict = ref CollectionsMarshal.GetValueRefOrNullRef(ItemEffects, itemId);
            if (!Unsafe.IsNullRef(ref innerDict))
            {
                ref var slot = ref CollectionsMarshal.GetValueRefOrNullRef(innerDict, spellId);
                if (!Unsafe.IsNullRef(ref slot))
                    return slot;
            }
            return 0;
        }

        public static uint GetItemEnchantVisual(uint enchantId)
        {
            uint visualId;
            if (ItemEnchantVisuals.TryGetValue(enchantId, out visualId))
                return visualId;
            return 0;
        }

        public static uint GetSpellVisual(uint spellId)
        {
            uint visual;
            if (SpellVisuals.TryGetValue(spellId, out visual))
                return visual;
            return 0;
        }

        /// <summary>
        /// Gets the base duration for an aura spell in milliseconds.
        /// Used as a fallback when the server doesn't provide duration info (e.g., enemy debuffs in Vanilla).
        /// Durations are loaded from AuraDurations{expansion}.csv, sourced from LibClassicDurations.
        /// </summary>
        public static int GetAuraSpellDuration(uint spellId)
        {
            if (AuraDurations.TryGetValue(spellId, out int duration))
                return duration;

            return 0;
        }

        public static int GetTotemSlotForSpell(uint spellId)
        {
            uint slot;
            if (TotemSpells.TryGetValue(spellId, out slot))
                return (int)slot;
            return -1;
        }

        public static uint GetRealSpell(uint learnSpellId)
        {
            uint realSpellId;
            if (LearnSpells.TryGetValue(learnSpellId, out realSpellId))
                return realSpellId;
            return learnSpellId;
        }

        public static uint GetGemFromEnchantId(uint enchantId)
        {
            uint itemId;
            if (Gems.TryGetValue(enchantId, out itemId))
                return itemId;
            return 0;
        }

        public static uint GetEnchantIdFromGem(uint itemId)
        {
            foreach (var itr in Gems)
            {
                if (itr.Value == itemId)
                    return itr.Key;
            }
            return 0;
        }

        public static float GetUnitCompleteDisplayScale(uint displayId)
        {
            var displayData = GetDisplayInfo(displayId);
            if (displayData.ModelId == 0)
                return 1.0f;

            var modelData = GetModelData(displayId);
            return displayData.DisplayScale * modelData.ModelScale;
        }


        public static CreatureDisplayInfo GetDisplayInfo(uint displayId)
        {
            if (CreatureDisplayInfos.TryGetValue(displayId, out var info))
                return info;
            return new CreatureDisplayInfo(0, 1.0f);
        }

        public static CreatureModelCollisionHeight GetModelData(uint modelId)
        {
            if (CreatureModelCollisionHeights.TryGetValue(modelId, out var info))
                return info;
            return new CreatureModelCollisionHeight(1.0f, 0, 0);
        }

        public static uint GetTransportPeriod(uint entry)
        {
            uint period;
            if (TransportPeriods.TryGetValue(entry, out period))
                return period;
            return 0;
        }

        public static string GetAreaName(uint id)
        {
            string name;
            if (AreaNames.TryGetValue(id, out name))
                return name;
            return "";
        }

        public static uint GetFactionForRace(uint race)
        {
            uint faction;
            if (RaceFaction.TryGetValue(race, out faction))
                return faction;
            return 1;
        }

        public static uint GetBattlegroundIdFromMapId(uint mapId)
        {
            foreach (var bg in Battlegrounds)
            {
                if (bg.Value.MapIds.Contains(mapId))
                    return bg.Key;
            }
            return 0;
        }

        public static uint GetMapIdFromBattlegroundId(uint bgId)
        {
            Battleground bg;
            if (Battlegrounds.TryGetValue(bgId, out bg))
                return bg.MapIds[0];
            return 0;
        }

        public static uint GetChatChannelIdFromName(string name)
        {
            foreach (var channel in ChatChannels)
            {
                if (name.Contains(channel.Value.Name))
                    return channel.Key;
            }
            return 0;
        }

        public static List<ChatChannel> GetChatChannelsWithFlags(ChannelFlags flags)
        {
            List<ChatChannel> channels = new List<ChatChannel>();
            foreach (var channel in ChatChannels)
            {
                if ((channel.Value.Flags & flags) == flags)
                    channels.Add(channel.Value);
            }
            return channels;
        }

        public static bool IsAllianceRace(Race raceId)
        {
            switch (raceId)
            {
                case Race.Human:
                case Race.Dwarf:
                case Race.NightElf:
                case Race.Gnome:
                case Race.Draenei:
                case Race.Worgen:
                    return true;
            }
            return false;
        }

        public static bool IsHordeRace(Race raceId)
        {
            switch (raceId)
            {
                case Race.Orc:
                case Race.Undead:
                case Race.Tauren:
                case Race.Troll:
                case Race.BloodElf:
                case Race.Goblin:
                    return true;
            }
            return false;
        }

        /// returns 0 when unknown
        public static int GetFactionByRace(Race race)
        {
            if (IsAllianceRace(race))
                return 1;
            if (IsHordeRace(race))
                return 2;
            return 0;
        }

        public static BroadcastText GetBroadcastText(uint entry)
        {
            BroadcastText data;
            if (BroadcastTextStore.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static uint GetBroadcastTextId(string maleText, string femaleText, uint language, ushort[] emoteDelays, ushort[] emotes)
        {
            foreach (var itr in BroadcastTextStore)
            {
                if (((!String.IsNullOrEmpty(maleText) && itr.Value.MaleText == maleText) ||
                     (!String.IsNullOrEmpty(femaleText) && itr.Value.FemaleText == femaleText)) &&
                    itr.Value.Language == language &&
                    Enumerable.SequenceEqual(itr.Value.EmoteDelays, emoteDelays) &&
                    Enumerable.SequenceEqual(itr.Value.Emotes, emotes))
                {
                    return itr.Key;
                }
            }

            BroadcastText broadcastText = new();
            broadcastText.Entry = BroadcastTextStore.Keys.Last() + 1;
            broadcastText.MaleText = maleText;
            broadcastText.FemaleText = femaleText;
            broadcastText.Language = language;
            broadcastText.EmoteDelays = emoteDelays;
            broadcastText.Emotes = emotes;
            BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
            return broadcastText.Entry;
        }
        #endregion
        #region Loading
        // Loading code
        public static void LoadEverything()
        {
            Log.Print(LogType.Storage, "Loading data files...");
            LoadBuildAuthSeeds();
            LoadBroadcastTexts();
            LoadItemDisplayIds();
            LoadItemRecords();
            LoadItemSparseRecords();
            LoadItemAppearance();
            LoadItemModifiedAppearance();
            LoadItemEffect();
            LoadItemSpellsData();
            LoadItemDisplayIdToFileDataId();
            LoadBattlegrounds();
            LoadChatChannels();
            LoadItemEnchantVisuals();
            LoadSpellVisuals();
            LoadLearnSpells();
            LoadTotemSpells();
            LoadGems();
            LoadCreatureDisplayInfo();
            LoadCreatureModelCollisionHeights();
            LoadTransports();
            LoadAreaNames();
            LoadRaceFaction();
            LoadDispellSpells();
            LoadSpellEffectPoints();
            LoadStackableAuras();
            LoadMountAuras();
            LoadMeleeSpells();
            LoadAutoRepeatSpells();
            LoadAuraSpells();
            LoadAuraDurations();
            LoadTaxiPaths();
            LoadTaxiPathNodesGraph();
            LoadQuestBits();
            LoadHotfixes();
            Log.Print(LogType.Storage, "Finished loading data.");
        }

        public static void LoadBuildAuthSeeds()
        {
            var path = Path.Combine("CSV", $"BuildAuthSeeds.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint build = uint.Parse(row[0].Span);
                string platform = row[1].ToString(); // Need string for dictionary key
                byte[] seed = row[2].ToString().ParseAsByteArray(); // Need string for this extension method

                if (!BuildAuthSeeds.TryGetValue(build, out var seeds))
                {
                    seeds = new Dictionary<string, byte[]>();
                    BuildAuthSeeds.Add(build, seeds);
                }
                seeds.Add(platform, seed);
            }
        }

        public static void LoadBroadcastTexts()
        {
            var path = Path.Combine("CSV", $"BroadcastTexts{LegacyVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                BroadcastText broadcastText = new BroadcastText();
                broadcastText.Entry = UInt32.Parse(row[0].Span);
                broadcastText.MaleText = row[1].Span.ToString(); // Need string for text processing
                broadcastText.FemaleText = row[2].Span.ToString(); // Need string for text processing
                broadcastText.Language = UInt32.Parse(row[3].Span);
                broadcastText.Emotes[0] = UInt16.Parse(row[4].Span);
                broadcastText.Emotes[1] = UInt16.Parse(row[5].Span);
                broadcastText.Emotes[2] = UInt16.Parse(row[6].Span);
                broadcastText.EmoteDelays[0] = UInt16.Parse(row[7].Span);
                broadcastText.EmoteDelays[1] = UInt16.Parse(row[8].Span);
                broadcastText.EmoteDelays[2] = UInt16.Parse(row[9].Span);
                BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
            }
        }

        public static void LoadItemDisplayIds()
        {
            var path = Path.Combine("CSV", $"ItemIdToDisplayId{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            var dict = new Dictionary<uint, uint>();
            foreach (var row in reader)
            {
                uint entry = uint.Parse(row[0].Span);
                uint displayId = uint.Parse(row[1].Span);
                dict.Add(entry, displayId);
            }

            ItemDisplayIdStore = dict.ToFrozenDictionary();
        }

        public static void LoadItemRecords()
        {
            var path = Path.Combine("CSV", $"Item{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            var dict = new Dictionary<uint, ItemRecord>();

            foreach (var row in reader)
            {
                ItemRecord item = new();
                item.Id = int.Parse(row[0].Span);
                item.ClassId = byte.Parse(row[1].Span);
                item.SubclassId = byte.Parse(row[2].Span);
                item.Material = byte.Parse(row[3].Span);
                item.InventoryType = sbyte.Parse(row[4].Span);
                item.RequiredLevel = int.Parse(row[5].Span);
                item.SheatheType = byte.Parse(row[6].Span);
                item.RandomProperty = ushort.Parse(row[7].Span);
                item.ItemRandomSuffixGroupId = ushort.Parse(row[8].Span);
                item.SoundOverrideSubclassId = sbyte.Parse(row[9].Span);
                item.ScalingStatDistributionId = ushort.Parse(row[10].Span);
                item.IconFileDataId = int.Parse(row[11].Span);
                item.ItemGroupSoundsId = byte.Parse(row[12].Span);
                item.ContentTuningId = int.Parse(row[13].Span);
                item.MaxDurability = uint.Parse(row[14].Span);
                item.AmmoType = byte.Parse(row[15].Span);
                item.DamageType[0] = byte.Parse(row[16].Span);
                item.DamageType[1] = byte.Parse(row[17].Span);
                item.DamageType[2] = byte.Parse(row[18].Span);
                item.DamageType[3] = byte.Parse(row[19].Span);
                item.DamageType[4] = byte.Parse(row[20].Span);
                item.Resistances[0] = short.Parse(row[21].Span);
                item.Resistances[1] = short.Parse(row[22].Span);
                item.Resistances[2] = short.Parse(row[23].Span);
                item.Resistances[3] = short.Parse(row[24].Span);
                item.Resistances[4] = short.Parse(row[25].Span);
                item.Resistances[5] = short.Parse(row[26].Span);
                item.Resistances[6] = short.Parse(row[27].Span);
                item.MinDamage[0] = ushort.Parse(row[28].Span);
                item.MinDamage[1] = ushort.Parse(row[29].Span);
                item.MinDamage[2] = ushort.Parse(row[30].Span);
                item.MinDamage[3] = ushort.Parse(row[31].Span);
                item.MinDamage[4] = ushort.Parse(row[32].Span);
                item.MaxDamage[0] = ushort.Parse(row[33].Span);
                item.MaxDamage[1] = ushort.Parse(row[34].Span);
                item.MaxDamage[2] = ushort.Parse(row[35].Span);
                item.MaxDamage[3] = ushort.Parse(row[36].Span);
                item.MaxDamage[4] = ushort.Parse(row[37].Span);

                ItemRecordsStore.Add((uint)item.Id, item);
            }
        }

        public static void LoadItemSparseRecords()
        {
            var path = Path.Combine("CSV", $"ItemSparse{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));
            foreach (var row in reader)
            {

                ItemSparseRecord item = new();
                item.Id = int.Parse(row[0].Span);
                item.AllowableRace = long.Parse(row[1].Span);
                item.Description = row[2].ToString();
                item.Name4 = row[3].ToString();
                item.Name3 = row[4].ToString();
                item.Name2 = row[5].ToString();
                item.Name1 = row[6].ToString();
                item.DmgVariance = float.Parse(row[7].Span);
                item.DurationInInventory = uint.Parse(row[8].Span);
                item.QualityModifier = float.Parse(row[9].Span);
                item.BagFamily = uint.Parse(row[10].Span);
                item.RangeMod = float.Parse(row[11].Span);
                item.StatPercentageOfSocket[0] = float.Parse(row[12].Span);
                item.StatPercentageOfSocket[1] = float.Parse(row[13].Span);
                item.StatPercentageOfSocket[2] = float.Parse(row[14].Span);
                item.StatPercentageOfSocket[3] = float.Parse(row[15].Span);
                item.StatPercentageOfSocket[4] = float.Parse(row[16].Span);
                item.StatPercentageOfSocket[5] = float.Parse(row[17].Span);
                item.StatPercentageOfSocket[6] = float.Parse(row[18].Span);
                item.StatPercentageOfSocket[7] = float.Parse(row[19].Span);
                item.StatPercentageOfSocket[8] = float.Parse(row[20].Span);
                item.StatPercentageOfSocket[9] = float.Parse(row[21].Span);
                item.StatPercentEditor[0] = int.Parse(row[22].Span);
                item.StatPercentEditor[1] = int.Parse(row[23].Span);
                item.StatPercentEditor[2] = int.Parse(row[24].Span);
                item.StatPercentEditor[3] = int.Parse(row[25].Span);
                item.StatPercentEditor[4] = int.Parse(row[26].Span);
                item.StatPercentEditor[5] = int.Parse(row[27].Span);
                item.StatPercentEditor[6] = int.Parse(row[28].Span);
                item.StatPercentEditor[7] = int.Parse(row[29].Span);
                item.StatPercentEditor[8] = int.Parse(row[30].Span);
                item.StatPercentEditor[9] = int.Parse(row[31].Span);
                item.Stackable = int.Parse(row[32].Span);
                item.MaxCount = int.Parse(row[33].Span);
                item.RequiredAbility = uint.Parse(row[34].Span);
                item.SellPrice = uint.Parse(row[35].Span);
                item.BuyPrice = uint.Parse(row[36].Span);
                item.VendorStackCount = uint.Parse(row[37].Span);
                item.PriceVariance = float.Parse(row[38].Span);
                item.PriceRandomValue = float.Parse(row[39].Span);
                item.Flags[0] = uint.Parse(row[40].Span);
                item.Flags[1] = uint.Parse(row[41].Span);
                item.Flags[2] = uint.Parse(row[42].Span);
                item.Flags[3] = uint.Parse(row[43].Span);
                item.OppositeFactionItemId = int.Parse(row[44].Span);
                item.MaxDurability = uint.Parse(row[45].Span);
                item.ItemNameDescriptionId = ushort.Parse(row[46].Span);
                item.RequiredTransmogHoliday = ushort.Parse(row[47].Span);
                item.RequiredHoliday = ushort.Parse(row[48].Span);
                item.LimitCategory = ushort.Parse(row[49].Span);
                item.GemProperties = ushort.Parse(row[50].Span);
                item.SocketMatchEnchantmentId = ushort.Parse(row[51].Span);
                item.TotemCategoryId = ushort.Parse(row[52].Span);
                item.InstanceBound = ushort.Parse(row[53].Span);
                item.ZoneBound[0] = ushort.Parse(row[54].Span);
                item.ZoneBound[1] = ushort.Parse(row[55].Span);
                item.ItemSet = ushort.Parse(row[56].Span);
                item.LockId = ushort.Parse(row[57].Span);
                item.StartQuestId = ushort.Parse(row[58].Span);
                item.PageText = ushort.Parse(row[59].Span);
                item.Delay = ushort.Parse(row[60].Span);
                item.RequiredReputationId = ushort.Parse(row[61].Span);
                item.RequiredSkillRank = ushort.Parse(row[62].Span);
                item.RequiredSkill = ushort.Parse(row[63].Span);
                item.ItemLevel = ushort.Parse(row[64].Span);
                item.AllowableClass = short.Parse(row[65].Span);
                item.ItemRandomSuffixGroupId = ushort.Parse(row[66].Span);
                item.RandomProperty = ushort.Parse(row[67].Span);
                item.MinDamage[0] = ushort.Parse(row[68].Span);
                item.MinDamage[1] = ushort.Parse(row[69].Span);
                item.MinDamage[2] = ushort.Parse(row[70].Span);
                item.MinDamage[3] = ushort.Parse(row[71].Span);
                item.MinDamage[4] = ushort.Parse(row[72].Span);
                item.MaxDamage[0] = ushort.Parse(row[73].Span);
                item.MaxDamage[1] = ushort.Parse(row[74].Span);
                item.MaxDamage[2] = ushort.Parse(row[75].Span);
                item.MaxDamage[3] = ushort.Parse(row[76].Span);
                item.MaxDamage[4] = ushort.Parse(row[77].Span);
                item.Resistances[0] = short.Parse(row[78].Span);
                item.Resistances[1] = short.Parse(row[79].Span);
                item.Resistances[2] = short.Parse(row[80].Span);
                item.Resistances[3] = short.Parse(row[81].Span);
                item.Resistances[4] = short.Parse(row[82].Span);
                item.Resistances[5] = short.Parse(row[83].Span);
                item.Resistances[6] = short.Parse(row[84].Span);
                item.ScalingStatDistributionId = ushort.Parse(row[85].Span);
                item.ExpansionId = byte.Parse(row[86].Span);
                item.ArtifactId = byte.Parse(row[87].Span);
                item.SpellWeight = byte.Parse(row[88].Span);
                item.SpellWeightCategory = byte.Parse(row[89].Span);
                item.SocketType[0] = byte.Parse(row[90].Span);
                item.SocketType[1] = byte.Parse(row[91].Span);
                item.SocketType[2] = byte.Parse(row[92].Span);
                item.SheatheType = byte.Parse(row[93].Span);
                item.Material = byte.Parse(row[94].Span);
                item.PageMaterial = byte.Parse(row[95].Span);
                item.PageLanguage = byte.Parse(row[96].Span);
                item.Bonding = byte.Parse(row[97].Span);
                item.DamageType = byte.Parse(row[98].Span);
                item.StatType[0] = sbyte.Parse(row[99].Span);
                item.StatType[1] = sbyte.Parse(row[100].Span);
                item.StatType[2] = sbyte.Parse(row[101].Span);
                item.StatType[3] = sbyte.Parse(row[102].Span);
                item.StatType[4] = sbyte.Parse(row[103].Span);
                item.StatType[5] = sbyte.Parse(row[104].Span);
                item.StatType[6] = sbyte.Parse(row[105].Span);
                item.StatType[7] = sbyte.Parse(row[106].Span);
                item.StatType[8] = sbyte.Parse(row[107].Span);
                item.StatType[9] = sbyte.Parse(row[108].Span);
                item.ContainerSlots = byte.Parse(row[109].Span);
                item.RequiredReputationRank = byte.Parse(row[110].Span);
                item.RequiredCityRank = byte.Parse(row[111].Span);
                item.RequiredHonorRank = byte.Parse(row[112].Span);
                item.InventoryType = byte.Parse(row[113].Span);
                item.OverallQualityId = byte.Parse(row[114].Span);
                item.AmmoType = byte.Parse(row[115].Span);
                item.StatValue[0] = sbyte.Parse(row[116].Span);
                item.StatValue[1] = sbyte.Parse(row[117].Span);
                item.StatValue[2] = sbyte.Parse(row[118].Span);
                item.StatValue[3] = sbyte.Parse(row[119].Span);
                item.StatValue[4] = sbyte.Parse(row[120].Span);
                item.StatValue[5] = sbyte.Parse(row[121].Span);
                item.StatValue[6] = sbyte.Parse(row[122].Span);
                item.StatValue[7] = sbyte.Parse(row[123].Span);
                item.StatValue[8] = sbyte.Parse(row[124].Span);
                item.StatValue[9] = sbyte.Parse(row[125].Span);
                item.RequiredLevel = sbyte.Parse(row[126].Span);
                ItemSparseRecordsStore.Add((uint)item.Id, item);
            }
        }

        public static void LoadItemAppearance()
        {
            var path = Path.Combine("CSV", $"ItemAppearance{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            var dict = new Dictionary<uint, ItemAppearance>();
            foreach (var row in reader)
            {
                ItemAppearance appearance = new ItemAppearance();
                appearance.Id = int.Parse(row[0].Span);
                appearance.DisplayType = byte.Parse(row[1].Span);
                appearance.ItemDisplayInfoID = int.Parse(row[2].Span);
                appearance.DefaultIconFileDataID = int.Parse(row[3].Span);
                appearance.UiOrder = int.Parse(row[4].Span);
                ItemAppearanceStore.Add((uint)appearance.Id, appearance);
            }
        }

        public static void LoadItemModifiedAppearance()
        {
            var path = Path.Combine("CSV", $"ItemModifiedAppearance{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            var dict = new Dictionary<uint, ItemModifiedAppearance>();
            foreach (var row in reader)
            {
                ItemModifiedAppearance modifiedAppearance = new ItemModifiedAppearance();
                modifiedAppearance.Id = int.Parse(row[0].Span);
                modifiedAppearance.ItemID = int.Parse(row[1].Span);
                modifiedAppearance.ItemAppearanceModifierID = int.Parse(row[2].Span);
                modifiedAppearance.ItemAppearanceID = int.Parse(row[3].Span);
                modifiedAppearance.OrderIndex = int.Parse(row[4].Span);
                modifiedAppearance.TransmogSourceTypeEnum = int.Parse(row[5].Span);
                ItemModifiedAppearanceStore.Add((uint)modifiedAppearance.Id, modifiedAppearance);
            }
        }

        public static void LoadItemEffect()
        {
            var path = Path.Combine("CSV", $"ItemEffect{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            foreach (var row in reader)
            {
                ItemEffect effect = new ItemEffect();
                effect.Id = int.Parse(row[0].Span);
                effect.LegacySlotIndex = byte.Parse(row[1].Span);
                effect.TriggerType = sbyte.Parse(row[2].Span);
                effect.Charges = short.Parse(row[3].Span);
                effect.CoolDownMSec = int.Parse(row[4].Span);
                effect.CategoryCoolDownMSec = int.Parse(row[5].Span);
                effect.SpellCategoryID = ushort.Parse(row[6].Span);
                effect.SpellID = int.Parse(row[7].Span);
                effect.ChrSpecializationID = ushort.Parse(row[8].Span);
                effect.ParentItemID = int.Parse(row[9].Span);
                ItemEffectStore.Add((uint)effect.Id, effect);
            }
        }

        public static void LoadItemSpellsData()
        {
            var path = Path.Combine("CSV", $"ItemSpellsData{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            var dict = new Dictionary<uint, ItemSpellsData>();
            foreach (var row in reader)
            {
                ItemSpellsData data = new ItemSpellsData();
                data.Id = int.Parse(row[0].Span);
                data.Category = int.Parse(row[1].Span);
                data.RecoveryTime = int.Parse(row[2].Span);
                data.CategoryRecoveryTime = int.Parse(row[3].Span);
                dict.Add((uint)data.Id, data);
            }
            ItemSpellsDataStore = dict.ToFrozenDictionary();
        }

        public static void LoadItemDisplayIdToFileDataId()
        {
            var path = Path.Combine("CSV", $"ItemDisplayIdToFileDataId{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            var dict = new Dictionary<uint, uint>();

            foreach (var row in reader)
            {
                uint displayId = uint.Parse(row[0].Span);
                uint fileDataId = uint.Parse(row[1].Span);
                dict.Add(displayId, fileDataId);
            }

            ItemDisplayIdToFileDataIdStore = dict.ToFrozenDictionary();
        }

        public static void LoadBattlegrounds()
        {
            var path = Path.Combine("CSV", "Battlegrounds.csv");
            Dictionary<uint, Battleground> dict = new Dictionary<uint, Battleground>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                Battleground bg = new Battleground();
                uint bgId = uint.Parse(row[0].Span);
                bg.IsArena = byte.Parse(row[1].Span) != 0;
                for (int i = 0; i < 6; i++)
                {
                    uint mapId = uint.Parse(row[2 + i].Span);
                    if (mapId != 0)
                        bg.MapIds.Add(mapId);
                }
                System.Diagnostics.Trace.Assert(bg.MapIds.Count != 0);
                dict.Add(bgId, bg);
            }
            Battlegrounds = dict.ToFrozenDictionary();
        }

        public static void LoadChatChannels()
        {
            var path = Path.Combine("CSV", "ChatChannels.csv");
            Dictionary<uint, ChatChannel> dict = new Dictionary<uint, ChatChannel>();
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                ChatChannel channel = new ChatChannel();
                channel.Id = uint.Parse(row[0].Span);
                channel.Flags = (ChannelFlags)uint.Parse(row[1].Span);
                channel.Name = row[2].ToString();
                dict.Add(channel.Id, channel);
            }
            ChatChannels = dict.ToFrozenDictionary();
        }

        public static void LoadItemEnchantVisuals()
        {
            var path = Path.Combine("CSV", $"ItemEnchantVisuals{ModernVersion.ExpansionVersion}.csv");
            Dictionary<uint, uint> dict = new Dictionary<uint, uint>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint enchantId = uint.Parse(row[0].Span);
                uint visualId = uint.Parse(row[1].Span);
                dict.Add(enchantId, visualId);
            }
            ItemEnchantVisuals = dict.ToFrozenDictionary();
        }

        public static void LoadSpellVisuals()
        {
            var path = Path.Combine("CSV", $"SpellVisuals{ModernVersion.ExpansionVersion}.csv");
            Dictionary<uint, uint> dict = new Dictionary<uint, uint>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                uint visualId = uint.Parse(row[1].Span);
                dict.Add(spellId, visualId);
            }
            SpellVisuals = dict.ToFrozenDictionary();
        }

        public static void LoadLearnSpells()
        {
            var path = Path.Combine("CSV", "LearnSpells.csv");
            Dictionary<uint, uint> dict = new Dictionary<uint, uint>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint learnSpellId = uint.Parse(row[0].Span);
                uint realSpellId = uint.Parse(row[1].Span);
                if (!dict.ContainsKey(learnSpellId))
                    dict.Add(learnSpellId, realSpellId);
            }
            LearnSpells = dict.ToFrozenDictionary();
        }

        public static void LoadTotemSpells()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", $"TotemSpells.csv");
            Dictionary<uint, uint> dict = new Dictionary<uint, uint>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                uint totemSlot = uint.Parse(row[1].Span);
                dict.Add(spellId, totemSlot);
            }
            TotemSpells = dict.ToFrozenDictionary();
        }

        public static void LoadGems()
        {
            if (ModernVersion.ExpansionVersion <= 1)
                return;

            var path = Path.Combine("CSV", $"Gems{ModernVersion.ExpansionVersion}.csv");

            Dictionary<uint, uint> dict = new Dictionary<uint, uint>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint enchantId = uint.Parse(row[0].Span);
                uint itemId = uint.Parse(row[1].Span);
                dict.Add(enchantId, itemId);
            }
            Gems = dict.ToFrozenDictionary();
        }

        public static void LoadCreatureDisplayInfo()
        {
            var path = Path.Combine("CSV", "CreatureDisplayInfo.csv");

            Dictionary<uint, CreatureDisplayInfo> dict = new Dictionary<uint, CreatureDisplayInfo>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint displayId = uint.Parse(row[0].Span);
                uint modelId = uint.Parse(row[1].Span);
                float scale = float.Parse(row[2].Span);
                dict.Add(displayId, new CreatureDisplayInfo(modelId, scale));
            }
            CreatureDisplayInfos = dict.ToFrozenDictionary();
        }

        public static void LoadCreatureModelCollisionHeights()
        {
            var path = Path.Combine("CSV", $"CreatureModelCollisionHeightsModern{LegacyVersion.ExpansionVersion}.csv");

            Dictionary<uint, CreatureModelCollisionHeight> dict = new Dictionary<uint, CreatureModelCollisionHeight>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint modelId = uint.Parse(row[0].Span);
                float modelScale = float.Parse(row[1].Span);
                float collisionHeight = float.Parse(row[2].Span);
                float collisionHeightMounted = float.Parse(row[3].Span);
                dict.Add(modelId, new CreatureModelCollisionHeight(modelScale, collisionHeight, collisionHeightMounted));
            }
            CreatureModelCollisionHeights = dict.ToFrozenDictionary();
        }

        public static void LoadTransports()
        {
            var path = Path.Combine("CSV", $"Transports{LegacyVersion.ExpansionVersion}.csv");

            Dictionary<uint, uint> dict = new Dictionary<uint, uint>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint entry = uint.Parse(row[0].Span);
                uint period = uint.Parse(row[1].Span);
                dict.Add(entry, period);
            }
            TransportPeriods = dict.ToFrozenDictionary();
        }

        public static void LoadAreaNames()
        {
            var path = Path.Combine("CSV", $"AreaNames.csv");

            Dictionary<uint, string> dict = new Dictionary<uint, string>();
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint id = uint.Parse(row[0].Span);
                string name = row[1].ToString();
                dict.Add(id, name);
            }
            AreaNames = dict.ToFrozenDictionary();
        }

        public static void LoadRaceFaction()
        {
            var path = Path.Combine("CSV", $"RaceFaction.csv");

            Dictionary<uint, uint> dict = new Dictionary<uint, uint>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint id = uint.Parse(row[0].Span);
                uint faction = uint.Parse(row[1].Span);
                dict.Add(id, faction);
            }
            RaceFaction = dict.ToFrozenDictionary();
        }

        public static void LoadDispellSpells()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", "DispellSpells.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                DispellSpells.Add(spellId);
            }
        }

        public static void LoadSpellEffectPoints()
        {
            var path = Path.Combine("CSV", $"SpellEffectPoints{LegacyVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);

                // Those basePoints are usually incremented by 1, only few test spell have another value there (baseDice)
                int basePointsEff1 = int.Parse(row[2].Span);
                if (basePointsEff1 != 0)
                    basePointsEff1 += 1;

                int basePointsEff2 = int.Parse(row[3].Span);
                if (basePointsEff2 != 0)
                    basePointsEff2 += 1;

                int basePointsEff3 = int.Parse(row[4].Span);
                if (basePointsEff3 != 0)
                    basePointsEff3 += 1;

                SpellEffectPoints.Add(spellId, new List<float> { basePointsEff1, basePointsEff2, basePointsEff3 });
            }
        }

        public static void LoadStackableAuras()
        {
            if (LegacyVersion.ExpansionVersion > 2)
                return;

            var path = Path.Combine("CSV", $"StackableAuras{LegacyVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                StackableAuras.Add(spellId);
            }
        }

        public static void LoadMountAuras()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", $"MountAuras.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                MountAuras.Add(spellId);
            }
        }

        public static void LoadMeleeSpells()
        {
            var path = Path.Combine("CSV", $"MeleeSpells{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                NextMeleeSpells.Add(spellId);
            }
        }

        public static void LoadAutoRepeatSpells()
        {
            var path = Path.Combine("CSV", $"AutoRepeatSpells{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                AutoRepeatSpells.Add(spellId);
            }
        }
        public static void LoadAuraSpells()
        {
            var path = Path.Combine("CSV", $"AuraSpells{LegacyVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                AuraSpells.Add(spellId);
            }
        }

        public static void LoadAuraDurations()
        {
            var path = Path.Combine("CSV", $"AuraDurations{LegacyVersion.ExpansionVersion}.csv");
            if (!File.Exists(path))
                return;

            var dict = new Dictionary<uint, int>();
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            foreach (var row in reader)
            {
                uint spellId = uint.Parse(row[0].Span);
                int duration = int.Parse(row[1].Span);
                dict[spellId] = duration;
            }

            AuraDurations = dict.ToFrozenDictionary();
        }

        public static void LoadTaxiPaths()
        {
            var path = Path.Combine("CSV", $"TaxiPath{ModernVersion.ExpansionVersion}.csv");

            Dictionary<uint, TaxiPath> dict = new Dictionary<uint, TaxiPath>();
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            uint counter = 0;

            foreach (var row in reader)
            {
                TaxiPath taxiPath = new()
                {
                    Id = uint.Parse(row[0].Span),
                    From = uint.Parse(row[1].Span),
                    To = uint.Parse(row[2].Span),
                    Cost = int.Parse(row[3].Span)
                };
                dict.Add(counter, taxiPath);
                counter++;
            }
            TaxiPaths = dict.ToFrozenDictionary();
        }
        public static void LoadTaxiPathNodesGraph()
        {
            // Load TaxiNodes (used in calculating first and last parts of path)
            Dictionary<uint, TaxiNode> TaxiNodes = new Dictionary<uint, TaxiNode>();
            var pathNodes = Path.Combine("CSV", $"TaxiNodes{ModernVersion.ExpansionVersion}.csv");
            if (File.Exists(pathNodes))
            {
                using var taxiNodesReader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(pathNodes));

                foreach (var row in taxiNodesReader)
                {
                    TaxiNode taxiNode = new TaxiNode
                    {
                        Id = uint.Parse(row[0].Span),
                        mapId = uint.Parse(row[1].Span),
                        x = float.Parse(row[2].Span),
                        y = float.Parse(row[3].Span),
                        z = float.Parse(row[4].Span)
                    };
                    TaxiNodes.Add(taxiNode.Id, taxiNode);
                }
            }
            // Load TaxiPathNode (used in calculating rest of path)
            Dictionary<uint, TaxiPathNode> TaxiPathNodes = new Dictionary<uint, TaxiPathNode>();
            var pathPathNodes = Path.Combine("CSV", $"TaxiPathNode{ModernVersion.ExpansionVersion}.csv");
            if (File.Exists(pathPathNodes))
            {
                using var taxiPathNodesReader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(pathPathNodes));

                foreach (var row in taxiPathNodesReader)
                {
                    TaxiPathNode taxiPathNode = new()
                    {
                        Id = uint.Parse(row[0].Span),
                        pathId = uint.Parse(row[1].Span),
                        nodeIndex = uint.Parse(row[2].Span),
                        mapId = uint.Parse(row[3].Span),
                        x = float.Parse(row[4].Span),
                        y = float.Parse(row[5].Span),
                        z = float.Parse(row[6].Span),
                        flags = uint.Parse(row[7].Span),
                        delay = uint.Parse(row[8].Span)
                    };
                    TaxiPathNodes.Add(taxiPathNode.Id, taxiPathNode);
                }
            }
            // calculate distances between nodes
            for (uint i = 0; i < TaxiPaths.Count; i++)
            {
                if (!TaxiPaths.TryGetValue(i, out TaxiPath taxiPath))
                {
                    continue;
                }

                float dist = 0.0f;
                TaxiNode nodeFrom = TaxiNodes[taxiPath.From];
                TaxiNode nodeTo = TaxiNodes[taxiPath.To];

                if (nodeFrom.x == 0 && nodeFrom.x == 0 && nodeFrom.z == 0)
                    continue;
                if (nodeTo.x == 0 && nodeTo.x == 0 && nodeTo.z == 0)
                    continue;

                // save all node ids of this path
                HashSet<uint> pathNodeList = [];
                foreach (var itr in TaxiPathNodes)
                {
                    TaxiPathNode pNode = itr.Value;
                    if (pNode.pathId != taxiPath.Id)
                        continue;
                    pathNodeList.Add(pNode.Id);
                }
                // sort ids by node index
                IEnumerable<uint> query = pathNodeList.OrderBy(node => TaxiPathNodes[node].nodeIndex);
                uint curNode = 0;
                foreach (var itr in query)
                {
                    TaxiPathNode pNode = TaxiPathNodes[itr];
                    // calculate distance from start node
                    if (pNode.nodeIndex == 0)
                    {
                        float dx = nodeFrom.x - pNode.x;
                        float dy = nodeFrom.y - pNode.y;
                        dist += MathF.Sqrt(dx * dx + dy * dy);
                        continue;
                    }
                    // set previous node
                    if (curNode == 0)
                    {
                        curNode = pNode.Id;
                        continue;
                    }
                    // calculate distance to previous node
                    if (curNode != 0)
                    {
                        TaxiPathNode prevNode = TaxiPathNodes[curNode];
                        curNode = pNode.Id;
                        if (prevNode.mapId != pNode.mapId)
                            continue;

                        float dx = prevNode.x - pNode.x;
                        float dy = prevNode.y - pNode.y;
                        dist += MathF.Sqrt(dx * dx + dy * dy);
                    }
                }
                // calculate distance to last node
                if (curNode != 0) // should not happen
                {
                    TaxiPathNode lastNode = TaxiPathNodes[curNode];
                    float dx = nodeTo.x - lastNode.x;
                    float dy = nodeTo.y - lastNode.y;
                    dist += MathF.Sqrt(dx * dx + dy * dy);
                }
                TaxiNodesGraph[taxiPath.From, taxiPath.To] = dist > 0 ? (int)dist : 0;
            }
        }

        public static void LoadQuestBits()
        {
            var path = Path.Combine("CSV", $"QuestV2_{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));

            Dictionary<uint, uint> dict = [];
            foreach (var row in reader)
            {
                uint questId = uint.Parse(row[0].Span);
                if (row[1].Span[0] == '-')
                    continue; // Some bits have a negative index, is this an error from WDBX?
                uint uniqueBitFlag = uint.Parse(row[1].Span);
                dict.Add(questId, uniqueBitFlag);
            }
            QuestBits = dict.ToFrozenDictionary();
        }

        #endregion
        #region HotFixes
        // Stores
        public const uint HotfixAreaTriggerBegin = 100000;
        public const uint HotfixSkillLineBegin = 110000;
        public const uint HotfixSkillRaceClassInfoBegin = 120000;
        public const uint HotfixSkillLineAbilityBegin = 130000;
        public const uint HotfixSpellBegin = 140000;
        public const uint HotfixSpellNameBegin = 150000;
        public const uint HotfixSpellLevelsBegin = 160000;
        public const uint HotfixSpellAuraOptionsBegin = 170000;
        public const uint HotfixSpellMiscBegin = 180000;
        public const uint HotfixSpellEffectBegin = 190000;
        public const uint HotfixSpellXSpellVisualBegin = 200000;
        public const uint HotfixItemBegin = 210000;
        public const uint HotfixItemSparseBegin = 220000;
        public const uint HotfixItemAppearanceBegin = 230000;
        public const uint HotfixItemModifiedAppearanceBegin = 240000;
        public const uint HotfixItemEffectBegin = 250000;
        public const uint HotfixItemDisplayInfoBegin = 260000;
        public const uint HotfixCreatureDisplayInfoBegin = 270000;
        public const uint HotfixCreatureDisplayInfoExtraBegin = 280000;
        public const uint HotfixCreatureDisplayInfoOptionBegin = 290000;
        public static Dictionary<uint, HotfixRecord> Hotfixes = [];
        public static void LoadHotfixes()
        {
            LoadAreaTriggerHotfixes();
            LoadSkillLineHotfixes();
            LoadSkillRaceClassInfoHotfixes();
            LoadSkillLineAbilityHotfixes();
            LoadSpellHotfixes();
            LoadSpellNameHotfixes();
            LoadSpellLevelsHotfixes();
            LoadSpellAuraOptionsHotfixes();
            LoadSpellMiscHotfixes();
            LoadSpellEffectHotfixes();
            LoadSpellXSpellVisualHotfixes();
            LoadItemSparseHotfixes();
            LoadItemHotfixes();
            LoadItemEffectHotfixes();
            LoadItemDisplayInfoHotfixes();
            LoadCreatureDisplayInfoHotfixes();
            LoadCreatureDisplayInfoExtraHotfixes();
            LoadCreatureDisplayInfoOptionHotfixes();
        }

        public static void LoadAreaTriggerHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"AreaTrigger{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                AreaTrigger at = new()
                {
                    Message = row[0].ToString(),
                    PositionX = float.Parse(row[1].Span),
                    PositionY = float.Parse(row[2].Span),
                    PositionZ = float.Parse(row[3].Span),
                    Id = uint.Parse(row[4].Span),
                    MapId = ushort.Parse(row[5].Span),
                    PhaseUseFlags = byte.Parse(row[6].Span),
                    PhaseId = ushort.Parse(row[7].Span),
                    PhaseGroupId = ushort.Parse(row[8].Span),
                    Radius = float.Parse(row[9].Span),
                    BoxLength = float.Parse(row[10].Span),
                    BoxWidth = float.Parse(row[11].Span),
                    BoxHeight = float.Parse(row[12].Span),
                    BoxYaw = float.Parse(row[13].Span),
                    ShapeType = byte.Parse(row[14].Span),
                    ShapeId = ushort.Parse(row[15].Span),
                    ActionSetId = ushort.Parse(row[16].Span),
                    Flags = byte.Parse(row[17].Span)
                };

                HotfixRecord record = new()
                {
                    TableHash = DB2Hash.AreaTrigger,
                    HotfixId = HotfixAreaTriggerBegin + counter
                };
                record.UniqueId = record.HotfixId;
                record.RecordId = at.Id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteCString(at.Message);
                record.HotfixContent.WriteFloat(at.PositionX);
                record.HotfixContent.WriteFloat(at.PositionY);
                record.HotfixContent.WriteFloat(at.PositionZ);
                record.HotfixContent.WriteUInt32(at.Id);
                record.HotfixContent.WriteUInt16(at.MapId);
                record.HotfixContent.WriteUInt8(at.PhaseUseFlags);
                record.HotfixContent.WriteUInt16(at.PhaseId);
                record.HotfixContent.WriteUInt16(at.PhaseGroupId);
                record.HotfixContent.WriteFloat(at.Radius);
                record.HotfixContent.WriteFloat(at.BoxLength);
                record.HotfixContent.WriteFloat(at.BoxWidth);
                record.HotfixContent.WriteFloat(at.BoxHeight);
                record.HotfixContent.WriteFloat(at.BoxYaw);
                record.HotfixContent.WriteUInt8(at.ShapeType);
                record.HotfixContent.WriteUInt16(at.ShapeId);
                record.HotfixContent.WriteUInt16(at.ActionSetId);
                record.HotfixContent.WriteUInt8(at.Flags);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSkillLineHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillLine{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                string displayName = row[0].ToString();
                string alternateVerb = row[1].ToString();
                string description = row[2].ToString();
                string hordeDisplayName = row[3].ToString();
                string neutralDisplayName = row[4].ToString();
                uint id = uint.Parse(row[5].Span);
                byte categoryID = byte.Parse(row[6].Span);
                uint spellIconFileID = uint.Parse(row[7].Span);
                byte canLink = byte.Parse(row[8].Span);
                uint parentSkillLineID = uint.Parse(row[9].Span);
                uint parentTierIndex = uint.Parse(row[10].Span);
                ushort flags = ushort.Parse(row[11].Span);
                uint spellBookSpellID = uint.Parse(row[12].Span);

                HotfixRecord record = new()
                {
                    TableHash = DB2Hash.SkillLine,
                    HotfixId = HotfixSkillLineBegin + counter
                };
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteCString(displayName);
                record.HotfixContent.WriteCString(alternateVerb);
                record.HotfixContent.WriteCString(description);
                record.HotfixContent.WriteCString(hordeDisplayName);
                record.HotfixContent.WriteCString(neutralDisplayName);
                record.HotfixContent.WriteUInt32(id);
                record.HotfixContent.WriteUInt8(categoryID);
                record.HotfixContent.WriteUInt32(spellIconFileID);
                record.HotfixContent.WriteUInt8(canLink);
                record.HotfixContent.WriteUInt32(parentSkillLineID);
                record.HotfixContent.WriteUInt32(parentTierIndex);
                record.HotfixContent.WriteUInt16(flags);
                record.HotfixContent.WriteUInt32(spellBookSpellID);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSkillRaceClassInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillRaceClassInfo{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = false }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                ulong raceMask = ulong.Parse(row[1].Span);
                ushort skillId = ushort.Parse(row[2].Span);
                uint classMask = uint.Parse(row[3].Span);
                ushort flags = ushort.Parse(row[4].Span);
                byte availability = byte.Parse(row[5].Span);
                byte minLevel = byte.Parse(row[6].Span);
                ushort skillTierId = ushort.Parse(row[7].Span);

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.SkillRaceClassInfo;
                record.HotfixId = HotfixSkillRaceClassInfoBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt64(raceMask);
                record.HotfixContent.WriteUInt16(skillId);
                record.HotfixContent.WriteUInt32(classMask);
                record.HotfixContent.WriteUInt16(flags);
                record.HotfixContent.WriteUInt8(availability);
                record.HotfixContent.WriteUInt8(minLevel);
                record.HotfixContent.WriteUInt16(skillTierId);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSkillLineAbilityHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillLineAbility{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = false }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                ulong raceMask = ulong.Parse(row[0].Span);
                uint id = uint.Parse(row[1].Span);
                ushort skillId = ushort.Parse(row[2].Span);
                uint spellId = uint.Parse(row[3].Span);
                ushort minSkillLineRank = ushort.Parse(row[4].Span);
                uint classMask = uint.Parse(row[5].Span);
                uint supercedesSpellId = uint.Parse(row[6].Span);
                byte acquireMethod = byte.Parse(row[7].Span);
                ushort trivialSkillLineRankHigh = ushort.Parse(row[8].Span);
                ushort trivialSkillLineRankLow = ushort.Parse(row[9].Span);
                byte flags = byte.Parse(row[10].Span);
                byte numSkillUps = byte.Parse(row[11].Span);
                ushort uniqueBit = ushort.Parse(row[12].Span);
                ushort tradeSkillCategoryId = ushort.Parse(row[13].Span);
                ushort skillUpSkillLineId = ushort.Parse(row[14].Span);
                uint characterPoints1 = uint.Parse(row[15].Span);
                uint characterPoints2 = uint.Parse(row[16].Span);


                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.SkillLineAbility;
                record.HotfixId = HotfixSkillLineAbilityBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt64(raceMask);
                record.HotfixContent.WriteUInt32(id);
                record.HotfixContent.WriteUInt16(skillId);
                record.HotfixContent.WriteUInt32(spellId);
                record.HotfixContent.WriteUInt16(minSkillLineRank);
                record.HotfixContent.WriteUInt32(classMask);
                record.HotfixContent.WriteUInt32(supercedesSpellId);
                record.HotfixContent.WriteUInt8(acquireMethod);
                record.HotfixContent.WriteUInt16(trivialSkillLineRankHigh);
                record.HotfixContent.WriteUInt16(trivialSkillLineRankLow);
                record.HotfixContent.WriteUInt8(flags);
                record.HotfixContent.WriteUInt8(numSkillUps);
                record.HotfixContent.WriteUInt16(uniqueBit);
                record.HotfixContent.WriteUInt16(tradeSkillCategoryId);
                record.HotfixContent.WriteUInt16(skillUpSkillLineId);
                record.HotfixContent.WriteUInt32(characterPoints1);
                record.HotfixContent.WriteUInt32(characterPoints2);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSpellHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"Spell{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                string nameSubText = row[1].ToString();
                string description = row[2].ToString();
                string auraDescription = row[3].ToString();

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.Spell;
                record.HotfixId = HotfixSpellBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteCString(nameSubText);
                record.HotfixContent.WriteCString(description);
                record.HotfixContent.WriteCString(auraDescription);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSpellNameHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellName{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                string name = row[1].ToString();

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.SpellName;
                record.HotfixId = HotfixSpellNameBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteCString(name);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSpellLevelsHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellLevels{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = false }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                byte difficultyId = byte.Parse(row[1].Span);
                ushort baseLevel = ushort.Parse(row[2].Span);
                ushort maxLevel = ushort.Parse(row[3].Span);
                ushort spellLevel = ushort.Parse(row[4].Span);
                byte maxPassiveAuraLevel = byte.Parse(row[5].Span);
                uint spellId = uint.Parse(row[6].Span);

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.SpellLevels;
                record.HotfixId = HotfixSpellLevelsBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt8(difficultyId);
                record.HotfixContent.WriteUInt16(baseLevel);
                record.HotfixContent.WriteUInt16(maxLevel);
                record.HotfixContent.WriteUInt16(spellLevel);
                record.HotfixContent.WriteUInt8(maxPassiveAuraLevel);
                record.HotfixContent.WriteUInt32(spellId);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSpellAuraOptionsHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellAuraOptions{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = false }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                byte difficultyId = byte.Parse(row[1].Span);
                uint cumulatievAura = uint.Parse(row[2].Span);
                uint procCategoryRecovery = uint.Parse(row[3].Span);
                byte procChance = byte.Parse(row[4].Span);
                uint procCharges = uint.Parse(row[5].Span);
                ushort spellProcsPerMinuteId = ushort.Parse(row[6].Span);
                uint procTypeMask0 = uint.Parse(row[7].Span);
                uint procTypeMask1 = uint.Parse(row[8].Span);
                uint spellId = uint.Parse(row[9].Span);

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.SpellAuraOptions;
                record.HotfixId = HotfixSpellAuraOptionsBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt8(difficultyId);
                record.HotfixContent.WriteUInt32(cumulatievAura);
                record.HotfixContent.WriteUInt32(procCategoryRecovery);
                record.HotfixContent.WriteUInt8(procChance);
                record.HotfixContent.WriteUInt32(procCharges);
                record.HotfixContent.WriteUInt16(spellProcsPerMinuteId);
                record.HotfixContent.WriteUInt32(procTypeMask0);
                record.HotfixContent.WriteUInt32(procTypeMask1);
                record.HotfixContent.WriteUInt32(spellId);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSpellMiscHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellMisc{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = false }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                byte difficultyId = byte.Parse(row[1].Span);
                ushort castingTimeIndex = ushort.Parse(row[2].Span);
                ushort durationIndex = ushort.Parse(row[3].Span);
                ushort rangeIndex = ushort.Parse(row[4].Span);
                byte schoolMask = byte.Parse(row[5].Span);
                float speed = float.Parse(row[6].Span);
                float launchDelay = float.Parse(row[7].Span);
                float minDuration = float.Parse(row[8].Span);
                uint spellIconFileDataId = uint.Parse(row[9].Span);
                uint activeIconFileDataId = uint.Parse(row[10].Span);
                uint attributes1 = uint.Parse(row[11].Span);
                uint attributes2 = uint.Parse(row[12].Span);
                uint attributes3 = uint.Parse(row[13].Span);
                uint attributes4 = uint.Parse(row[14].Span);
                uint attributes5 = uint.Parse(row[15].Span);
                uint attributes6 = uint.Parse(row[16].Span);
                uint attributes7 = uint.Parse(row[17].Span);
                uint attributes8 = uint.Parse(row[18].Span);
                uint attributes9 = uint.Parse(row[19].Span);
                uint attributes10 = uint.Parse(row[20].Span);
                uint attributes11 = uint.Parse(row[21].Span);
                uint attributes12 = uint.Parse(row[22].Span);
                uint attributes13 = uint.Parse(row[23].Span);
                uint attributes14 = uint.Parse(row[24].Span);
                uint spellId = uint.Parse(row[25].Span);

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.SpellMisc;
                record.HotfixId = HotfixSpellMiscBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt8(difficultyId);
                record.HotfixContent.WriteUInt16(castingTimeIndex);
                record.HotfixContent.WriteUInt16(durationIndex);
                record.HotfixContent.WriteUInt16(rangeIndex);
                record.HotfixContent.WriteUInt8(schoolMask);
                record.HotfixContent.WriteFloat(speed);
                record.HotfixContent.WriteFloat(launchDelay);
                record.HotfixContent.WriteFloat(minDuration);
                record.HotfixContent.WriteUInt32(spellIconFileDataId);
                record.HotfixContent.WriteUInt32(activeIconFileDataId);
                record.HotfixContent.WriteUInt32(attributes1);
                record.HotfixContent.WriteUInt32(attributes2);
                record.HotfixContent.WriteUInt32(attributes3);
                record.HotfixContent.WriteUInt32(attributes4);
                record.HotfixContent.WriteUInt32(attributes5);
                record.HotfixContent.WriteUInt32(attributes6);
                record.HotfixContent.WriteUInt32(attributes7);
                record.HotfixContent.WriteUInt32(attributes8);
                record.HotfixContent.WriteUInt32(attributes9);
                record.HotfixContent.WriteUInt32(attributes10);
                record.HotfixContent.WriteUInt32(attributes11);
                record.HotfixContent.WriteUInt32(attributes12);
                record.HotfixContent.WriteUInt32(attributes13);
                record.HotfixContent.WriteUInt32(attributes14);
                record.HotfixContent.WriteUInt32(spellId);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSpellEffectHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellEffect{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = false }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                uint difficultyId = uint.Parse(row[1].Span);
                uint effectIndex = uint.Parse(row[2].Span);
                uint effect = uint.Parse(row[3].Span);
                float effectAmplitude = float.Parse(row[4].Span);
                uint effectAttributes = uint.Parse(row[5].Span);
                short effectAura = short.Parse(row[6].Span);
                int effectAuraPeriod = int.Parse(row[7].Span);
                int effectBasePoints = int.Parse(row[8].Span);
                float effectBonusCoefficient = float.Parse(row[9].Span);
                float effectChainAmplitude = float.Parse(row[10].Span);
                int effectChainTargets = int.Parse(row[11].Span);
                int effectDieSides = int.Parse(row[12].Span);
                int effectItemType = int.Parse(row[13].Span);
                int effectMechanic = int.Parse(row[14].Span);
                float effectPointsPerResource = float.Parse(row[15].Span);
                float effectPosFacing = float.Parse(row[16].Span);
                float effectRealPointsPerLevel = float.Parse(row[17].Span);
                int EffectTriggerSpell = int.Parse(row[18].Span);
                float bonusCoefficientFromAP = float.Parse(row[19].Span);
                float pvpMultiplier = float.Parse(row[20].Span);
                float coefficient = float.Parse(row[21].Span);
                float variance = float.Parse(row[22].Span);
                float resourceCoefficient = float.Parse(row[23].Span);
                float groupSizeBasePointsCoefficient = float.Parse(row[24].Span);
                int effectMiscValue1 = int.Parse(row[25].Span);
                int effectMiscValue2 = int.Parse(row[26].Span);
                uint effectRadiusIndex1 = uint.Parse(row[27].Span);
                uint effectRadiusIndex2 = uint.Parse(row[28].Span);
                int effectSpellClassMask1 = int.Parse(row[29].Span);
                int effectSpellClassMask2 = int.Parse(row[30].Span);
                int effectSpellClassMask3 = int.Parse(row[31].Span);
                int effectSpellClassMask4 = int.Parse(row[32].Span);
                short implicitTarget1 = short.Parse(row[33].Span);
                short implicitTarget2 = short.Parse(row[34].Span);
                uint spellId = uint.Parse(row[35].Span);

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.SpellEffect;
                record.HotfixId = HotfixSpellEffectBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt32(difficultyId);
                record.HotfixContent.WriteUInt32(effectIndex);
                record.HotfixContent.WriteUInt32(effect);
                record.HotfixContent.WriteFloat(effectAmplitude);
                record.HotfixContent.WriteUInt32(effectAttributes);
                record.HotfixContent.WriteInt16(effectAura);
                record.HotfixContent.WriteInt32(effectAuraPeriod);
                record.HotfixContent.WriteInt32(effectBasePoints);
                record.HotfixContent.WriteFloat(effectBonusCoefficient);
                record.HotfixContent.WriteFloat(effectChainAmplitude);
                record.HotfixContent.WriteInt32(effectChainTargets);
                record.HotfixContent.WriteInt32(effectDieSides);
                record.HotfixContent.WriteInt32(effectItemType);
                record.HotfixContent.WriteInt32(effectMechanic);
                record.HotfixContent.WriteFloat(effectPointsPerResource);
                record.HotfixContent.WriteFloat(effectPosFacing);
                record.HotfixContent.WriteFloat(effectRealPointsPerLevel);
                record.HotfixContent.WriteInt32(EffectTriggerSpell);
                record.HotfixContent.WriteFloat(bonusCoefficientFromAP);
                record.HotfixContent.WriteFloat(pvpMultiplier);
                record.HotfixContent.WriteFloat(coefficient);
                record.HotfixContent.WriteFloat(variance);
                record.HotfixContent.WriteFloat(resourceCoefficient);
                record.HotfixContent.WriteFloat(groupSizeBasePointsCoefficient);
                record.HotfixContent.WriteInt32(effectMiscValue1);
                record.HotfixContent.WriteInt32(effectMiscValue2);
                record.HotfixContent.WriteUInt32(effectRadiusIndex1);
                record.HotfixContent.WriteUInt32(effectRadiusIndex2);
                record.HotfixContent.WriteInt32(effectSpellClassMask1);
                record.HotfixContent.WriteInt32(effectSpellClassMask2);
                record.HotfixContent.WriteInt32(effectSpellClassMask3);
                record.HotfixContent.WriteInt32(effectSpellClassMask4);
                record.HotfixContent.WriteInt16(implicitTarget1);
                record.HotfixContent.WriteInt16(implicitTarget2);
                record.HotfixContent.WriteUInt32(spellId);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadSpellXSpellVisualHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellXSpellVisual{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = false }).FromText(File.ReadAllText(path));
            var dict = new Dictionary<uint, uint>(SpellVisuals);

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                byte difficultyId = byte.Parse(row[1].Span);
                uint spellVisualId = uint.Parse(row[2].Span);
                float probability = float.Parse(row[3].Span);
                byte flags = byte.Parse(row[4].Span);
                byte priority = byte.Parse(row[5].Span);
                int spellIconFileId = int.Parse(row[6].Span);
                int activeIconFileId = int.Parse(row[7].Span);
                ushort viewerUnitConditionId = ushort.Parse(row[8].Span);
                uint viewerPlayerConditionId = uint.Parse(row[9].Span);
                ushort casterUnitConditionId = ushort.Parse(row[10].Span);
                uint casterPlayerConditionId = uint.Parse(row[11].Span);
                uint spellId = uint.Parse(row[12].Span);

                if (dict.ContainsKey(spellId))
                    dict[spellId] = id;
                else
                    dict.Add(spellId, id);

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.SpellXSpellVisual;
                record.HotfixId = HotfixSpellXSpellVisualBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt32(id);
                record.HotfixContent.WriteUInt8(difficultyId);
                record.HotfixContent.WriteUInt32(spellVisualId);
                record.HotfixContent.WriteFloat(probability);
                record.HotfixContent.WriteUInt8(flags);
                record.HotfixContent.WriteUInt8(priority);
                record.HotfixContent.WriteInt32(spellIconFileId);
                record.HotfixContent.WriteInt32(activeIconFileId);
                record.HotfixContent.WriteUInt16(viewerUnitConditionId);
                record.HotfixContent.WriteUInt32(viewerPlayerConditionId);
                record.HotfixContent.WriteUInt16(casterUnitConditionId);
                record.HotfixContent.WriteUInt32(casterPlayerConditionId);
                record.HotfixContent.WriteUInt32(spellId);
                Hotfixes.Add(record.HotfixId, record);
            }

            SpellVisuals = dict.ToFrozenDictionary();
        }
        public static void LoadItemSparseHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemSparse{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromText(File.ReadAllText(path));

            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = uint.Parse(row[0].Span);
                long allowableRace = long.Parse(row[1].Span);
                string description = row[2].ToString();
                string name4 = row[3].ToString();
                string name3 = row[4].ToString();
                string name2 = row[5].ToString();
                string name1 = row[6].ToString();
                float dmgVariance = float.Parse(row[7].Span);
                uint durationInInventory = uint.Parse(row[8].Span);
                float qualityModifier = float.Parse(row[9].Span);
                uint bagFamily = uint.Parse(row[10].Span);
                float rangeMod = float.Parse(row[11].Span);
                float statPercentageOfSocket1 = float.Parse(row[12].Span);
                float statPercentageOfSocket2 = float.Parse(row[13].Span);
                float statPercentageOfSocket3 = float.Parse(row[14].Span);
                float statPercentageOfSocket4 = float.Parse(row[15].Span);
                float statPercentageOfSocket5 = float.Parse(row[16].Span);
                float statPercentageOfSocket6 = float.Parse(row[17].Span);
                float statPercentageOfSocket7 = float.Parse(row[18].Span);
                float statPercentageOfSocket8 = float.Parse(row[19].Span);
                float statPercentageOfSocket9 = float.Parse(row[20].Span);
                float statPercentageOfSocket10 = float.Parse(row[21].Span);
                int statPercentEditor1 = int.Parse(row[22].Span);
                int statPercentEditor2 = int.Parse(row[23].Span);
                int statPercentEditor3 = int.Parse(row[24].Span);
                int statPercentEditor4 = int.Parse(row[25].Span);
                int statPercentEditor5 = int.Parse(row[26].Span);
                int statPercentEditor6 = int.Parse(row[27].Span);
                int statPercentEditor7 = int.Parse(row[28].Span);
                int statPercentEditor8 = int.Parse(row[29].Span);
                int statPercentEditor9 = int.Parse(row[30].Span);
                int statPercentEditor10 = int.Parse(row[31].Span);
                int stackable = int.Parse(row[32].Span);
                int maxCount = int.Parse(row[33].Span);
                uint requiredAbility = uint.Parse(row[34].Span);
                uint sellPrice = uint.Parse(row[35].Span);
                uint buyPrice = uint.Parse(row[36].Span);
                uint vendorStackCount = uint.Parse(row[37].Span);
                float priceVariance = float.Parse(row[38].Span);
                float priceRandomValue = float.Parse(row[39].Span);
                int flags1 = int.Parse(row[40].Span);
                int flags2 = int.Parse(row[41].Span);
                int flags3 = int.Parse(row[42].Span);
                int flags4 = int.Parse(row[43].Span);
                int oppositeFactionItemId = int.Parse(row[44].Span);
                uint maxDurability = uint.Parse(row[45].Span);
                ushort itemNameDescriptionId = ushort.Parse(row[46].Span);
                ushort requiredTransmogHoliday = ushort.Parse(row[47].Span);
                ushort requiredHoliday = ushort.Parse(row[48].Span);
                ushort limitCategory = ushort.Parse(row[49].Span);
                ushort gemProperties = ushort.Parse(row[50].Span);
                ushort socketMatchEnchantmentId = ushort.Parse(row[51].Span);
                ushort totemCategoryId = ushort.Parse(row[52].Span);
                ushort instanceBound = ushort.Parse(row[53].Span);
                ushort zoneBound1 = ushort.Parse(row[54].Span);
                ushort zoneBound2 = ushort.Parse(row[55].Span);
                ushort itemSet = ushort.Parse(row[56].Span);
                ushort lockId = ushort.Parse(row[57].Span);
                ushort startQuestId = ushort.Parse(row[58].Span);
                ushort pageText = ushort.Parse(row[59].Span);
                ushort delay = ushort.Parse(row[60].Span);
                ushort requiredReputationId = ushort.Parse(row[61].Span);
                ushort requiredSkillRank = ushort.Parse(row[62].Span);
                ushort requiredSkill = ushort.Parse(row[63].Span);
                ushort itemLevel = ushort.Parse(row[64].Span);
                short allowableClass = short.Parse(row[65].Span);
                ushort itemRandomSuffixGroupId = ushort.Parse(row[66].Span);
                ushort randomProperty = ushort.Parse(row[67].Span);
                ushort damageMin1 = ushort.Parse(row[68].Span);
                ushort damageMin2 = ushort.Parse(row[69].Span);
                ushort damageMin3 = ushort.Parse(row[70].Span);
                ushort damageMin4 = ushort.Parse(row[71].Span);
                ushort damageMin5 = ushort.Parse(row[72].Span);
                ushort damageMax1 = ushort.Parse(row[73].Span);
                ushort damageMax2 = ushort.Parse(row[74].Span);
                ushort damageMax3 = ushort.Parse(row[75].Span);
                ushort damageMax4 = ushort.Parse(row[76].Span);
                ushort damageMax5 = ushort.Parse(row[77].Span);
                short armor = short.Parse(row[78].Span);
                short holyResistance = short.Parse(row[79].Span);
                short fireResistance = short.Parse(row[80].Span);
                short natureResistance = short.Parse(row[81].Span);
                short frostResistance = short.Parse(row[82].Span);
                short shadowResistance = short.Parse(row[83].Span);
                short arcaneResistance = short.Parse(row[84].Span);
                ushort scalingStatDistributionId = ushort.Parse(row[85].Span);
                byte expansionId = byte.Parse(row[86].Span);
                byte artifactId = byte.Parse(row[87].Span);
                byte spellWeight = byte.Parse(row[88].Span);
                byte spellWeightCategory = byte.Parse(row[89].Span);
                byte socketType1 = byte.Parse(row[90].Span);
                byte socketType2 = byte.Parse(row[91].Span);
                byte socketType3 = byte.Parse(row[92].Span);
                byte sheatheType = byte.Parse(row[93].Span);
                byte material = byte.Parse(row[94].Span);
                byte pageMaterial = byte.Parse(row[95].Span);
                byte pageLanguage = byte.Parse(row[96].Span);
                byte bonding = byte.Parse(row[97].Span);
                byte damageType = byte.Parse(row[98].Span);
                sbyte statType1 = sbyte.Parse(row[99].Span);
                sbyte statType2 = sbyte.Parse(row[100].Span);
                sbyte statType3 = sbyte.Parse(row[101].Span);
                sbyte statType4 = sbyte.Parse(row[102].Span);
                sbyte statType5 = sbyte.Parse(row[103].Span);
                sbyte statType6 = sbyte.Parse(row[104].Span);
                sbyte statType7 = sbyte.Parse(row[105].Span);
                sbyte statType8 = sbyte.Parse(row[106].Span);
                sbyte statType9 = sbyte.Parse(row[107].Span);
                sbyte statType10 = sbyte.Parse(row[108].Span);
                byte containerSlots = byte.Parse(row[109].Span);
                byte requiredReputationRank = byte.Parse(row[110].Span);
                byte requiredCityRank = byte.Parse(row[111].Span);
                byte requiredHonorRank = byte.Parse(row[112].Span);
                byte inventoryType = byte.Parse(row[113].Span);
                byte overallQualityId = byte.Parse(row[114].Span);
                byte ammoType = byte.Parse(row[115].Span);
                sbyte statValue1 = sbyte.Parse(row[116].Span);
                sbyte statValue2 = sbyte.Parse(row[117].Span);
                sbyte statValue3 = sbyte.Parse(row[118].Span);
                sbyte statValue4 = sbyte.Parse(row[119].Span);
                sbyte statValue5 = sbyte.Parse(row[120].Span);
                sbyte statValue6 = sbyte.Parse(row[121].Span);
                sbyte statValue7 = sbyte.Parse(row[122].Span);
                sbyte statValue8 = sbyte.Parse(row[123].Span);
                sbyte statValue9 = sbyte.Parse(row[124].Span);
                sbyte statValue10 = sbyte.Parse(row[125].Span);
                sbyte requiredLevel = sbyte.Parse(row[126].Span);

                HotfixRecord record = new HotfixRecord();
                record.Status = HotfixStatus.Valid;
                record.TableHash = DB2Hash.ItemSparse;
                record.HotfixId = HotfixItemSparseBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.HotfixContent.WriteInt64(allowableRace);
                record.HotfixContent.WriteCString(description);
                record.HotfixContent.WriteCString(name4);
                record.HotfixContent.WriteCString(name3);
                record.HotfixContent.WriteCString(name2);
                record.HotfixContent.WriteCString(name1);
                record.HotfixContent.WriteFloat(dmgVariance);
                record.HotfixContent.WriteUInt32(durationInInventory);
                record.HotfixContent.WriteFloat(qualityModifier);
                record.HotfixContent.WriteUInt32(bagFamily);
                record.HotfixContent.WriteFloat(rangeMod);
                record.HotfixContent.WriteFloat(statPercentageOfSocket1);
                record.HotfixContent.WriteFloat(statPercentageOfSocket2);
                record.HotfixContent.WriteFloat(statPercentageOfSocket3);
                record.HotfixContent.WriteFloat(statPercentageOfSocket4);
                record.HotfixContent.WriteFloat(statPercentageOfSocket5);
                record.HotfixContent.WriteFloat(statPercentageOfSocket6);
                record.HotfixContent.WriteFloat(statPercentageOfSocket7);
                record.HotfixContent.WriteFloat(statPercentageOfSocket8);
                record.HotfixContent.WriteFloat(statPercentageOfSocket9);
                record.HotfixContent.WriteFloat(statPercentageOfSocket10);
                record.HotfixContent.WriteInt32(statPercentEditor1);
                record.HotfixContent.WriteInt32(statPercentEditor2);
                record.HotfixContent.WriteInt32(statPercentEditor3);
                record.HotfixContent.WriteInt32(statPercentEditor4);
                record.HotfixContent.WriteInt32(statPercentEditor5);
                record.HotfixContent.WriteInt32(statPercentEditor6);
                record.HotfixContent.WriteInt32(statPercentEditor7);
                record.HotfixContent.WriteInt32(statPercentEditor8);
                record.HotfixContent.WriteInt32(statPercentEditor9);
                record.HotfixContent.WriteInt32(statPercentEditor10);
                record.HotfixContent.WriteInt32(stackable);
                record.HotfixContent.WriteInt32(maxCount);
                record.HotfixContent.WriteUInt32(requiredAbility);
                record.HotfixContent.WriteUInt32(sellPrice);
                record.HotfixContent.WriteUInt32(buyPrice);
                record.HotfixContent.WriteUInt32(vendorStackCount);
                record.HotfixContent.WriteFloat(priceVariance);
                record.HotfixContent.WriteFloat(priceRandomValue);
                record.HotfixContent.WriteInt32(flags1);
                record.HotfixContent.WriteInt32(flags2);
                record.HotfixContent.WriteInt32(flags3);
                record.HotfixContent.WriteInt32(flags4);
                record.HotfixContent.WriteInt32(oppositeFactionItemId);
                record.HotfixContent.WriteUInt32(maxDurability);
                record.HotfixContent.WriteUInt16(itemNameDescriptionId);
                record.HotfixContent.WriteUInt16(requiredTransmogHoliday);
                record.HotfixContent.WriteUInt16(requiredHoliday);
                record.HotfixContent.WriteUInt16(limitCategory);
                record.HotfixContent.WriteUInt16(gemProperties);
                record.HotfixContent.WriteUInt16(socketMatchEnchantmentId);
                record.HotfixContent.WriteUInt16(totemCategoryId);
                record.HotfixContent.WriteUInt16(instanceBound);
                record.HotfixContent.WriteUInt16(zoneBound1);
                record.HotfixContent.WriteUInt16(zoneBound2);
                record.HotfixContent.WriteUInt16(itemSet);
                record.HotfixContent.WriteUInt16(lockId);
                record.HotfixContent.WriteUInt16(startQuestId);
                record.HotfixContent.WriteUInt16(pageText);
                record.HotfixContent.WriteUInt16(delay);
                record.HotfixContent.WriteUInt16(requiredReputationId);
                record.HotfixContent.WriteUInt16(requiredSkillRank);
                record.HotfixContent.WriteUInt16(requiredSkill);
                record.HotfixContent.WriteUInt16(itemLevel);
                record.HotfixContent.WriteInt16(allowableClass);
                record.HotfixContent.WriteUInt16(itemRandomSuffixGroupId);
                record.HotfixContent.WriteUInt16(randomProperty);
                record.HotfixContent.WriteUInt16(damageMin1);
                record.HotfixContent.WriteUInt16(damageMin2);
                record.HotfixContent.WriteUInt16(damageMin3);
                record.HotfixContent.WriteUInt16(damageMin4);
                record.HotfixContent.WriteUInt16(damageMin5);
                record.HotfixContent.WriteUInt16(damageMax1);
                record.HotfixContent.WriteUInt16(damageMax2);
                record.HotfixContent.WriteUInt16(damageMax3);
                record.HotfixContent.WriteUInt16(damageMax4);
                record.HotfixContent.WriteUInt16(damageMax5);
                record.HotfixContent.WriteInt16(armor);
                record.HotfixContent.WriteInt16(holyResistance);
                record.HotfixContent.WriteInt16(fireResistance);
                record.HotfixContent.WriteInt16(natureResistance);
                record.HotfixContent.WriteInt16(frostResistance);
                record.HotfixContent.WriteInt16(shadowResistance);
                record.HotfixContent.WriteInt16(arcaneResistance);
                record.HotfixContent.WriteUInt16(scalingStatDistributionId);
                record.HotfixContent.WriteUInt8(expansionId);
                record.HotfixContent.WriteUInt8(artifactId);
                record.HotfixContent.WriteUInt8(spellWeight);
                record.HotfixContent.WriteUInt8(spellWeightCategory);
                record.HotfixContent.WriteUInt8(socketType1);
                record.HotfixContent.WriteUInt8(socketType2);
                record.HotfixContent.WriteUInt8(socketType3);
                record.HotfixContent.WriteUInt8(sheatheType);
                record.HotfixContent.WriteUInt8(material);
                record.HotfixContent.WriteUInt8(pageMaterial);
                record.HotfixContent.WriteUInt8(pageLanguage);
                record.HotfixContent.WriteUInt8(bonding);
                record.HotfixContent.WriteUInt8(damageType);
                record.HotfixContent.WriteInt8(statType1);
                record.HotfixContent.WriteInt8(statType2);
                record.HotfixContent.WriteInt8(statType3);
                record.HotfixContent.WriteInt8(statType4);
                record.HotfixContent.WriteInt8(statType5);
                record.HotfixContent.WriteInt8(statType6);
                record.HotfixContent.WriteInt8(statType7);
                record.HotfixContent.WriteInt8(statType8);
                record.HotfixContent.WriteInt8(statType9);
                record.HotfixContent.WriteInt8(statType10);
                record.HotfixContent.WriteUInt8(containerSlots);
                record.HotfixContent.WriteUInt8(requiredReputationRank);
                record.HotfixContent.WriteUInt8(requiredCityRank);
                record.HotfixContent.WriteUInt8(requiredHonorRank);
                record.HotfixContent.WriteUInt8(inventoryType);
                record.HotfixContent.WriteUInt8(overallQualityId);
                record.HotfixContent.WriteUInt8(ammoType);
                record.HotfixContent.WriteInt8(statValue1);
                record.HotfixContent.WriteInt8(statValue2);
                record.HotfixContent.WriteInt8(statValue3);
                record.HotfixContent.WriteInt8(statValue4);
                record.HotfixContent.WriteInt8(statValue5);
                record.HotfixContent.WriteInt8(statValue6);
                record.HotfixContent.WriteInt8(statValue7);
                record.HotfixContent.WriteInt8(statValue8);
                record.HotfixContent.WriteInt8(statValue9);
                record.HotfixContent.WriteInt8(statValue10);
                record.HotfixContent.WriteInt8(requiredLevel);
                Hotfixes.Add(record.HotfixId, record);
            }
        }

        public static void WriteItemSparseHotfix(ItemTemplate item, Framework.IO.ByteBuffer buffer)
        {
            int[] StatValues = new int[10];
            for (int i = 0; i < item.StatsCount; i++)
            {
                StatValues[i] = item.StatValues[i];
                if (StatValues[i] > 127)
                    StatValues[i] = 127;
                if (StatValues[i] < -127)
                    StatValues[i] = -127;
            }

            buffer.WriteInt64(item.AllowedRaces);
            buffer.WriteCString(item.Description);
            buffer.WriteCString(item.Name[3]);
            buffer.WriteCString(item.Name[2]);
            buffer.WriteCString(item.Name[1]);
            buffer.WriteCString(item.Name[0]);
            buffer.WriteFloat(1);
            buffer.WriteUInt32(item.Duration);
            buffer.WriteFloat(0);
            buffer.WriteUInt32(item.BagFamily);
            buffer.WriteFloat(item.RangedMod);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(item.MaxStackSize);
            buffer.WriteInt32(item.MaxCount);
            buffer.WriteUInt32(item.RequiredSpell);
            buffer.WriteUInt32(item.SellPrice);
            buffer.WriteUInt32(item.BuyPrice);
            buffer.WriteUInt32(item.BuyCount);
            buffer.WriteFloat(1);
            buffer.WriteFloat(1);
            buffer.WriteUInt32(item.Flags);
            buffer.WriteUInt32(item.FlagsExtra);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteUInt32(item.MaxDurability);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16((ushort)item.HolidayID);
            buffer.WriteUInt16((ushort)item.ItemLimitCategory);
            buffer.WriteUInt16((ushort)item.GemProperties);
            buffer.WriteUInt16((ushort)item.SocketBonus);
            buffer.WriteUInt16((ushort)item.TotemCategory);
            buffer.WriteUInt16((ushort)item.MapID);
            buffer.WriteUInt16((ushort)item.AreaID);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16((ushort)item.ItemSet);
            buffer.WriteUInt16((ushort)item.LockId);
            buffer.WriteUInt16((ushort)item.StartQuestId);
            buffer.WriteUInt16((ushort)item.PageText);
            buffer.WriteUInt16((ushort)item.Delay);
            buffer.WriteUInt16((ushort)item.RequiredRepFaction);
            buffer.WriteUInt16((ushort)item.RequiredSkillLevel);
            buffer.WriteUInt16((ushort)item.RequiredSkillId);
            buffer.WriteUInt16((ushort)item.ItemLevel);
            buffer.WriteInt16((short)item.AllowedClasses);
            buffer.WriteUInt16((ushort)item.RandomSuffix);
            buffer.WriteUInt16((ushort)item.RandomProperty);
            buffer.WriteUInt16((ushort)item.DamageMins[0]);
            buffer.WriteUInt16((ushort)item.DamageMins[1]);
            buffer.WriteUInt16((ushort)item.DamageMins[2]);
            buffer.WriteUInt16((ushort)item.DamageMins[3]);
            buffer.WriteUInt16((ushort)item.DamageMins[4]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[0]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[1]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[2]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[3]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[4]);
            buffer.WriteInt16((short)item.Armor);
            buffer.WriteInt16((short)item.HolyResistance);
            buffer.WriteInt16((short)item.FireResistance);
            buffer.WriteInt16((short)item.NatureResistance);
            buffer.WriteInt16((short)item.FrostResistance);
            buffer.WriteInt16((short)item.ShadowResistance);
            buffer.WriteInt16((short)item.ArcaneResistance);
            buffer.WriteUInt16((ushort)item.ScalingStatDistribution);
            buffer.WriteUInt8(254);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8((byte)item.ItemSocketColors[0]);
            buffer.WriteUInt8((byte)item.ItemSocketColors[1]);
            buffer.WriteUInt8((byte)item.ItemSocketColors[2]);
            buffer.WriteUInt8((byte)item.SheathType);
            buffer.WriteUInt8((byte)item.Material);
            buffer.WriteUInt8((byte)item.PageMaterial);
            buffer.WriteUInt8((byte)item.Language);
            buffer.WriteUInt8((byte)item.Bonding);
            buffer.WriteUInt8((byte)item.DamageTypes[0]);
            buffer.WriteInt8((sbyte)item.StatTypes[0]);
            buffer.WriteInt8((sbyte)item.StatTypes[1]);
            buffer.WriteInt8((sbyte)item.StatTypes[2]);
            buffer.WriteInt8((sbyte)item.StatTypes[3]);
            buffer.WriteInt8((sbyte)item.StatTypes[4]);
            buffer.WriteInt8((sbyte)item.StatTypes[5]);
            buffer.WriteInt8((sbyte)item.StatTypes[6]);
            buffer.WriteInt8((sbyte)item.StatTypes[7]);
            buffer.WriteInt8((sbyte)item.StatTypes[8]);
            buffer.WriteInt8((sbyte)item.StatTypes[9]);
            buffer.WriteUInt8((byte)item.ContainerSlots);
            buffer.WriteUInt8((byte)item.RequiredRepValue);
            buffer.WriteUInt8((byte)item.RequiredCityRank);
            buffer.WriteUInt8((byte)item.RequiredHonorRank);
            buffer.WriteUInt8((byte)item.InventoryType);
            buffer.WriteUInt8((byte)item.Quality);
            buffer.WriteUInt8((byte)item.AmmoType);
            buffer.WriteInt8((sbyte)StatValues[0]);
            buffer.WriteInt8((sbyte)StatValues[1]);
            buffer.WriteInt8((sbyte)StatValues[2]);
            buffer.WriteInt8((sbyte)StatValues[3]);
            buffer.WriteInt8((sbyte)StatValues[4]);
            buffer.WriteInt8((sbyte)StatValues[5]);
            buffer.WriteInt8((sbyte)StatValues[6]);
            buffer.WriteInt8((sbyte)StatValues[7]);
            buffer.WriteInt8((sbyte)StatValues[8]);
            buffer.WriteInt8((sbyte)StatValues[9]);
            buffer.WriteInt8((sbyte)item.RequiredLevel);
        }

        public static void WriteItemSparseHotfix(ItemSparseRecord row, Framework.IO.ByteBuffer buffer)
        {
            Span<int> StatValues = stackalloc int[10];
            for (int i = 0; i < 10; i++)
            {
                StatValues[i] = row.StatValue[i];
                if (StatValues[i] > 127)
                    StatValues[i] = 127;
                if (StatValues[i] < -127)
                    StatValues[i] = -127;
            }

            buffer.WriteInt64(row.AllowableRace);
            buffer.WriteCString(row.Description);
            buffer.WriteCString(row.Name4);
            buffer.WriteCString(row.Name3);
            buffer.WriteCString(row.Name2);
            buffer.WriteCString(row.Name1);
            buffer.WriteFloat(row.DmgVariance);
            buffer.WriteUInt32(row.DurationInInventory);
            buffer.WriteFloat(row.QualityModifier);
            buffer.WriteUInt32(row.BagFamily);
            buffer.WriteFloat(row.RangeMod);
            buffer.WriteFloat(row.StatPercentageOfSocket[0]);
            buffer.WriteFloat(row.StatPercentageOfSocket[1]);
            buffer.WriteFloat(row.StatPercentageOfSocket[2]);
            buffer.WriteFloat(row.StatPercentageOfSocket[3]);
            buffer.WriteFloat(row.StatPercentageOfSocket[4]);
            buffer.WriteFloat(row.StatPercentageOfSocket[5]);
            buffer.WriteFloat(row.StatPercentageOfSocket[6]);
            buffer.WriteFloat(row.StatPercentageOfSocket[7]);
            buffer.WriteFloat(row.StatPercentageOfSocket[8]);
            buffer.WriteFloat(row.StatPercentageOfSocket[9]);
            buffer.WriteInt32(row.StatPercentEditor[0]);
            buffer.WriteInt32(row.StatPercentEditor[1]);
            buffer.WriteInt32(row.StatPercentEditor[2]);
            buffer.WriteInt32(row.StatPercentEditor[3]);
            buffer.WriteInt32(row.StatPercentEditor[4]);
            buffer.WriteInt32(row.StatPercentEditor[5]);
            buffer.WriteInt32(row.StatPercentEditor[6]);
            buffer.WriteInt32(row.StatPercentEditor[7]);
            buffer.WriteInt32(row.StatPercentEditor[8]);
            buffer.WriteInt32(row.StatPercentEditor[9]);
            buffer.WriteInt32(row.Stackable);
            buffer.WriteInt32(row.MaxCount);
            buffer.WriteUInt32(row.RequiredAbility);
            buffer.WriteUInt32(row.SellPrice);
            buffer.WriteUInt32(row.BuyPrice);
            buffer.WriteUInt32(row.VendorStackCount);
            buffer.WriteFloat(row.PriceVariance);
            buffer.WriteFloat(row.PriceRandomValue);
            buffer.WriteUInt32(row.Flags[0]);
            buffer.WriteUInt32(row.Flags[1]);
            buffer.WriteUInt32(row.Flags[2]);
            buffer.WriteUInt32(row.Flags[3]);
            buffer.WriteInt32(row.OppositeFactionItemId);
            buffer.WriteUInt32(row.MaxDurability);
            buffer.WriteUInt16(row.ItemNameDescriptionId);
            buffer.WriteUInt16(row.RequiredTransmogHoliday);
            buffer.WriteUInt16(row.RequiredHoliday);
            buffer.WriteUInt16(row.LimitCategory);
            buffer.WriteUInt16(row.GemProperties);
            buffer.WriteUInt16(row.SocketMatchEnchantmentId);
            buffer.WriteUInt16(row.TotemCategoryId);
            buffer.WriteUInt16(row.InstanceBound);
            buffer.WriteUInt16(row.ZoneBound[0]);
            buffer.WriteUInt16(row.ZoneBound[1]);
            buffer.WriteUInt16(row.ItemSet);
            buffer.WriteUInt16(row.LockId);
            buffer.WriteUInt16(row.StartQuestId);
            buffer.WriteUInt16(row.PageText);
            buffer.WriteUInt16(row.Delay);
            buffer.WriteUInt16(row.RequiredReputationId);
            buffer.WriteUInt16(row.RequiredSkillRank);
            buffer.WriteUInt16(row.RequiredSkill);
            buffer.WriteUInt16(row.ItemLevel);
            buffer.WriteInt16(row.AllowableClass);
            buffer.WriteUInt16(row.ItemRandomSuffixGroupId);
            buffer.WriteUInt16(row.RandomProperty);
            buffer.WriteUInt16(row.MinDamage[0]);
            buffer.WriteUInt16(row.MinDamage[1]);
            buffer.WriteUInt16(row.MinDamage[2]);
            buffer.WriteUInt16(row.MinDamage[3]);
            buffer.WriteUInt16(row.MinDamage[4]);
            buffer.WriteUInt16(row.MaxDamage[0]);
            buffer.WriteUInt16(row.MaxDamage[1]);
            buffer.WriteUInt16(row.MaxDamage[2]);
            buffer.WriteUInt16(row.MaxDamage[3]);
            buffer.WriteUInt16(row.MaxDamage[4]);
            buffer.WriteInt16(row.Resistances[0]);
            buffer.WriteInt16(row.Resistances[1]);
            buffer.WriteInt16(row.Resistances[2]);
            buffer.WriteInt16(row.Resistances[3]);
            buffer.WriteInt16(row.Resistances[4]);
            buffer.WriteInt16(row.Resistances[5]);
            buffer.WriteInt16(row.Resistances[6]);
            buffer.WriteUInt16(row.ScalingStatDistributionId);
            buffer.WriteUInt8(row.ExpansionId);
            buffer.WriteUInt8(row.ArtifactId);
            buffer.WriteUInt8(row.SpellWeight);
            buffer.WriteUInt8(row.SpellWeightCategory);
            buffer.WriteUInt8(row.SocketType[0]);
            buffer.WriteUInt8(row.SocketType[1]);
            buffer.WriteUInt8(row.SocketType[2]);
            buffer.WriteUInt8(row.SheatheType);
            buffer.WriteUInt8(row.Material);
            buffer.WriteUInt8(row.PageMaterial);
            buffer.WriteUInt8(row.PageLanguage);
            buffer.WriteUInt8(row.Bonding);
            buffer.WriteUInt8(row.DamageType);
            buffer.WriteInt8(row.StatType[0]);
            buffer.WriteInt8(row.StatType[1]);
            buffer.WriteInt8(row.StatType[2]);
            buffer.WriteInt8(row.StatType[3]);
            buffer.WriteInt8(row.StatType[4]);
            buffer.WriteInt8(row.StatType[5]);
            buffer.WriteInt8(row.StatType[6]);
            buffer.WriteInt8(row.StatType[7]);
            buffer.WriteInt8(row.StatType[8]);
            buffer.WriteInt8(row.StatType[9]);
            buffer.WriteUInt8(row.ContainerSlots);
            buffer.WriteUInt8(row.RequiredReputationRank);
            buffer.WriteUInt8(row.RequiredCityRank);
            buffer.WriteUInt8(row.RequiredHonorRank);
            buffer.WriteUInt8(row.InventoryType);
            buffer.WriteUInt8(row.OverallQualityId);
            buffer.WriteUInt8(row.AmmoType);
            buffer.WriteInt8((sbyte)StatValues[0]);
            buffer.WriteInt8((sbyte)StatValues[1]);
            buffer.WriteInt8((sbyte)StatValues[2]);
            buffer.WriteInt8((sbyte)StatValues[3]);
            buffer.WriteInt8((sbyte)StatValues[4]);
            buffer.WriteInt8((sbyte)StatValues[5]);
            buffer.WriteInt8((sbyte)StatValues[6]);
            buffer.WriteInt8((sbyte)StatValues[7]);
            buffer.WriteInt8((sbyte)StatValues[8]);
            buffer.WriteInt8((sbyte)StatValues[9]);
            buffer.WriteInt8(row.RequiredLevel);
        }
        public static void LoadItemHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"Item{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = row[0].Parse<uint>();
                byte ClassID = row[1].Parse<byte>();
                byte SubclassID = row[2].Parse<byte>();
                byte Material = row[3].Parse<byte>();
                sbyte InventoryType = row[4].Parse<sbyte>();
                uint RequiredLevel = row[5].Parse<uint>();
                byte SheatheType = row[6].Parse<byte>();
                ushort RandomSelect = row[7].Parse<ushort>();
                ushort ItemRandomSuffixGroupID = row[8].Parse<ushort>();
                sbyte Sound_override_subclassID = row[9].Parse<sbyte>();
                ushort ScalingStatDistributionID = row[10].Parse<ushort>();
                int IconFileDataID = row[11].Parse<int>();
                byte ItemGroupSoundsID = row[12].Parse<byte>();
                int ContentTuningID = row[13].Parse<int>();
                uint MaxDurability = row[14].Parse<uint>();
                byte AmmunitionType = row[15].Parse<byte>();
                byte DamageType1 = row[16].Parse<byte>();
                byte DamageType2 = row[17].Parse<byte>();
                byte DamageType3 = row[18].Parse<byte>();
                byte DamageType4 = row[19].Parse<byte>();
                byte DamageType5 = row[20].Parse<byte>();
                short Resistances1 = row[21].Parse<short>();
                short Resistances2 = row[22].Parse<short>();
                short Resistances3 = row[23].Parse<short>();
                short Resistances4 = row[24].Parse<short>();
                short Resistances5 = row[25].Parse<short>();
                short Resistances6 = row[26].Parse<short>();
                short Resistances7 = row[27].Parse<short>();
                ushort MinDamage1 = row[28].Parse<ushort>();
                ushort MinDamage2 = row[29].Parse<ushort>();
                ushort MinDamage3 = row[30].Parse<ushort>();
                ushort MinDamage4 = row[31].Parse<ushort>();
                ushort MinDamage5 = row[32].Parse<ushort>();
                ushort MaxDamage1 = row[33].Parse<ushort>();
                ushort MaxDamage2 = row[34].Parse<ushort>();
                ushort MaxDamage3 = row[35].Parse<ushort>();
                ushort MaxDamage4 = row[36].Parse<ushort>();
                ushort MaxDamage5 = row[37].Parse<ushort>();

                HotfixRecord record = new HotfixRecord();
                record.Status = HotfixStatus.Valid;
                record.TableHash = DB2Hash.Item;
                record.HotfixId = HotfixItemBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.HotfixContent.WriteUInt8(ClassID);
                record.HotfixContent.WriteUInt8(SubclassID);
                record.HotfixContent.WriteUInt8(Material);
                record.HotfixContent.WriteInt8(InventoryType);
                record.HotfixContent.WriteUInt32(RequiredLevel);
                record.HotfixContent.WriteUInt8(SheatheType);
                record.HotfixContent.WriteUInt16(RandomSelect);
                record.HotfixContent.WriteUInt16(ItemRandomSuffixGroupID);
                record.HotfixContent.WriteInt8(Sound_override_subclassID);
                record.HotfixContent.WriteUInt16(ScalingStatDistributionID);
                record.HotfixContent.WriteInt32(IconFileDataID);
                record.HotfixContent.WriteUInt8(ItemGroupSoundsID);
                record.HotfixContent.WriteInt32(ContentTuningID);
                record.HotfixContent.WriteUInt32(MaxDurability);
                record.HotfixContent.WriteUInt8(AmmunitionType);
                record.HotfixContent.WriteUInt8(DamageType1);
                record.HotfixContent.WriteUInt8(DamageType2);
                record.HotfixContent.WriteUInt8(DamageType3);
                record.HotfixContent.WriteUInt8(DamageType4);
                record.HotfixContent.WriteUInt8(DamageType5);
                record.HotfixContent.WriteInt16(Resistances1);
                record.HotfixContent.WriteInt16(Resistances2);
                record.HotfixContent.WriteInt16(Resistances3);
                record.HotfixContent.WriteInt16(Resistances4);
                record.HotfixContent.WriteInt16(Resistances5);
                record.HotfixContent.WriteInt16(Resistances6);
                record.HotfixContent.WriteInt16(Resistances7);
                record.HotfixContent.WriteUInt16(MinDamage1);
                record.HotfixContent.WriteUInt16(MinDamage2);
                record.HotfixContent.WriteUInt16(MinDamage3);
                record.HotfixContent.WriteUInt16(MinDamage4);
                record.HotfixContent.WriteUInt16(MinDamage5);
                record.HotfixContent.WriteUInt16(MaxDamage1);
                record.HotfixContent.WriteUInt16(MaxDamage2);
                record.HotfixContent.WriteUInt16(MaxDamage3);
                record.HotfixContent.WriteUInt16(MaxDamage4);
                record.HotfixContent.WriteUInt16(MaxDamage5);
                Hotfixes.Add(record.HotfixId, record);
            }
        }

        public static void WriteItemHotfix(ItemTemplate item, Framework.IO.ByteBuffer buffer)
        {
            int fileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);

            buffer.WriteUInt8((byte)item.Class);
            buffer.WriteUInt8((byte)item.SubClass);
            buffer.WriteUInt8((byte)item.Material);
            buffer.WriteInt8((sbyte)item.InventoryType);
            buffer.WriteInt32((int)item.RequiredLevel);
            buffer.WriteUInt8((byte)item.SheathType);
            buffer.WriteUInt16((ushort)item.RandomProperty);
            buffer.WriteUInt16((ushort)item.RandomSuffix);
            buffer.WriteInt8(-1);
            buffer.WriteUInt16(0);
            buffer.WriteInt32(fileDataId);
            buffer.WriteUInt8(0);
            buffer.WriteInt32(0);
            buffer.WriteUInt32(item.MaxDurability);
            buffer.WriteUInt8((byte)item.AmmoType);
            buffer.WriteUInt8((byte)item.DamageTypes[0]);
            buffer.WriteUInt8((byte)item.DamageTypes[1]);
            buffer.WriteUInt8((byte)item.DamageTypes[2]);
            buffer.WriteUInt8((byte)item.DamageTypes[3]);
            buffer.WriteUInt8((byte)item.DamageTypes[4]);
            buffer.WriteInt16((short)item.Armor);
            buffer.WriteInt16((short)item.HolyResistance);
            buffer.WriteInt16((short)item.FireResistance);
            buffer.WriteInt16((short)item.NatureResistance);
            buffer.WriteInt16((short)item.FrostResistance);
            buffer.WriteInt16((short)item.ShadowResistance);
            buffer.WriteInt16((short)item.ArcaneResistance);
            buffer.WriteUInt16((ushort)item.DamageMins[0]);
            buffer.WriteUInt16((ushort)item.DamageMins[1]);
            buffer.WriteUInt16((ushort)item.DamageMins[2]);
            buffer.WriteUInt16((ushort)item.DamageMins[3]);
            buffer.WriteUInt16((ushort)item.DamageMins[4]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[0]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[1]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[2]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[3]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[4]);
        }

        public static void WriteItemHotfix(ItemRecord row, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteUInt8(row.ClassId);
            buffer.WriteUInt8(row.SubclassId);
            buffer.WriteUInt8(row.Material);
            buffer.WriteInt8(row.InventoryType);
            buffer.WriteInt32(row.RequiredLevel);
            buffer.WriteUInt8(row.SheatheType);
            buffer.WriteUInt16(row.RandomProperty);
            buffer.WriteUInt16(row.ItemRandomSuffixGroupId);
            buffer.WriteInt8(row.SoundOverrideSubclassId);
            buffer.WriteUInt16(row.ScalingStatDistributionId);
            buffer.WriteInt32(row.IconFileDataId);
            buffer.WriteUInt8(row.ItemGroupSoundsId);
            buffer.WriteInt32(row.ContentTuningId);
            buffer.WriteUInt32(row.MaxDurability);
            buffer.WriteUInt8(row.AmmoType);
            buffer.WriteUInt8(row.DamageType[0]);
            buffer.WriteUInt8(row.DamageType[1]);
            buffer.WriteUInt8(row.DamageType[2]);
            buffer.WriteUInt8(row.DamageType[3]);
            buffer.WriteUInt8(row.DamageType[4]);
            buffer.WriteInt16(row.Resistances[0]);
            buffer.WriteInt16(row.Resistances[1]);
            buffer.WriteInt16(row.Resistances[2]);
            buffer.WriteInt16(row.Resistances[3]);
            buffer.WriteInt16(row.Resistances[4]);
            buffer.WriteInt16(row.Resistances[5]);
            buffer.WriteInt16(row.Resistances[6]);
            buffer.WriteUInt16(row.MinDamage[0]);
            buffer.WriteUInt16(row.MinDamage[1]);
            buffer.WriteUInt16(row.MinDamage[2]);
            buffer.WriteUInt16(row.MinDamage[3]);
            buffer.WriteUInt16(row.MinDamage[4]);
            buffer.WriteUInt16(row.MaxDamage[0]);
            buffer.WriteUInt16(row.MaxDamage[1]);
            buffer.WriteUInt16(row.MaxDamage[2]);
            buffer.WriteUInt16(row.MaxDamage[3]);
            buffer.WriteUInt16(row.MaxDamage[4]);
        }

        public static void WriteItemAppearanceHotfix(ItemAppearance appearance, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteUInt8(appearance.DisplayType);
            buffer.WriteInt32(appearance.ItemDisplayInfoID);
            buffer.WriteInt32(appearance.DefaultIconFileDataID);
            buffer.WriteInt32(appearance.UiOrder);
        }

        public static void WriteItemModifiedAppearanceHotfix(ItemModifiedAppearance modAppearance, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteInt32(modAppearance.Id);
            buffer.WriteInt32(modAppearance.ItemID);
            buffer.WriteInt32(modAppearance.ItemAppearanceModifierID);
            buffer.WriteInt32(modAppearance.ItemAppearanceID);
            buffer.WriteInt32(modAppearance.OrderIndex);
            buffer.WriteInt32(modAppearance.TransmogSourceTypeEnum);
        }

        public static void WriteItemEffectHotfix(ItemEffect effect, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteUInt8(effect.LegacySlotIndex);
            buffer.WriteInt8(effect.TriggerType);
            buffer.WriteInt16(effect.Charges);
            buffer.WriteInt32(effect.CoolDownMSec);
            buffer.WriteInt32(effect.CategoryCoolDownMSec);
            buffer.WriteUInt16(effect.SpellCategoryID);
            buffer.WriteInt32(effect.SpellID);
            buffer.WriteUInt16(effect.ChrSpecializationID);
            buffer.WriteInt32(effect.ParentItemID);
        }

        public static List<HotfixRecord> FindHotfixesByRecordIdAndTable(uint id, DB2Hash table, uint startId = 0)
        {
            return Hotfixes.Values.Where(hotfix => hotfix.HotfixId >= startId && hotfix.TableHash == table && hotfix.RecordId == id).ToList();
        }

        public static void UpdateHotfix(object obj, bool remove = false)
        {
            void DoStuff(uint recordId, DB2Hash table, Action<ByteBuffer>? writer)
            {
                List<HotfixRecord> oldRecords = FindHotfixesByRecordIdAndTable(recordId, table, HotfixItemBegin);
                if (oldRecords.Count == 0)
                {
                    // We have a new entry
                    HotfixRecord record = new HotfixRecord();
                    record.Status = remove ? HotfixStatus.RecordRemoved : HotfixStatus.Valid;
                    record.TableHash = table;
                    record.HotfixId = GetFirstFreeId(Hotfixes, HotfixItemBegin);
                    record.UniqueId = record.HotfixId;
                    record.RecordId = recordId;
                    writer?.Invoke(record.HotfixContent);
                    Hotfixes.Add(record.HotfixId, record);
                }
                else
                {
                    IEnumerable<HotfixRecord> oldRecordsToBeInvalided = oldRecords.SkipLast(1);
                    foreach (var record in oldRecordsToBeInvalided) // TODO maybe just delete these?
                    {
                        record.Status = HotfixStatus.Invalid;
                        record.HotfixContent = new();
                        Log.Print(LogType.Storage, $"Got duplicate record for record {record.RecordId} in {record.TableHash}");
                    }

                    HotfixRecord recordToOverwrite = oldRecords.Last();
                    recordToOverwrite.Status = remove ? HotfixStatus.RecordRemoved : HotfixStatus.Valid;
                    recordToOverwrite.HotfixContent = new();
                    writer?.Invoke(recordToOverwrite.HotfixContent);
                    Hotfixes[recordToOverwrite.HotfixId] = recordToOverwrite;
                }
            }

            if (obj is ItemRecord item)
            {
                DoStuff((uint)item.Id, DB2Hash.Item, remove ? null : (hotfixContentTargetBuffer) => WriteItemHotfix(item, hotfixContentTargetBuffer));
            }
            if (obj is ItemSparseRecord itemSparse)
            {
                DoStuff((uint)itemSparse.Id, DB2Hash.ItemSparse, remove ? null : (hotfixContentTargetBuffer) => WriteItemSparseHotfix(itemSparse, hotfixContentTargetBuffer));
            }
            if (obj is ItemEffect effect)
            {
                DoStuff((uint)effect.Id, DB2Hash.ItemEffect, remove ? null : (hotfixContentTargetBuffer) => WriteItemEffectHotfix(effect, hotfixContentTargetBuffer));
            }
            if (obj is ItemAppearance appearance)
            {
                DoStuff((uint)appearance.Id, DB2Hash.ItemAppearance, remove ? null : (hotfixContentTargetBuffer) => WriteItemAppearanceHotfix(appearance, hotfixContentTargetBuffer));
            }
            if (obj is ItemModifiedAppearance modAppearance)
            {
                DoStuff((uint)modAppearance.Id, DB2Hash.ItemModifiedAppearance, remove ? null : (hotfixContentTargetBuffer) => WriteItemModifiedAppearanceHotfix(modAppearance, hotfixContentTargetBuffer));
            }
        }

        public static Server.Packets.HotFixMessage? GenerateItemUpdateIfNeeded(ItemTemplate item)
        {
            if (ItemRecordsStore.TryGetValue(item.Entry, out var row))
            {
                int iconFileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);
                if (row.ClassId != (byte)item.Class ||
                    row.SubclassId != (byte)item.SubClass ||
                    row.Material != (byte)item.Material ||
                    row.InventoryType != (sbyte)item.InventoryType ||
                    row.RequiredLevel != (int)item.RequiredLevel ||
                    row.SheatheType != (byte)item.SheathType ||
                    row.RandomProperty != (ushort)item.RandomProperty ||
                    row.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix ||
                    row.IconFileDataId != iconFileDataId && iconFileDataId != 0 ||
                    row.MaxDurability != item.MaxDurability ||
                    row.AmmoType != (byte)item.AmmoType ||
                    row.DamageType[0] != (byte)item.DamageTypes[0] ||
                    row.DamageType[1] != (byte)item.DamageTypes[1] ||
                    row.DamageType[2] != (byte)item.DamageTypes[2] ||
                    row.DamageType[3] != (byte)item.DamageTypes[3] ||
                    row.DamageType[4] != (byte)item.DamageTypes[4] ||
                    //row.MinDamage[0] != (ushort)item.DamageMins[0] ||
                    //row.MinDamage[1] != (ushort)item.DamageMins[1] ||
                    //row.MinDamage[2] != (ushort)item.DamageMins[2] ||
                    //row.MinDamage[3] != (ushort)item.DamageMins[3] ||
                    //row.MinDamage[4] != (ushort)item.DamageMins[4] ||
                    //row.MaxDamage[0] != (ushort)item.DamageMaxs[0] ||
                    //row.MaxDamage[1] != (ushort)item.DamageMaxs[1] ||
                    //row.MaxDamage[2] != (ushort)item.DamageMaxs[2] ||
                    //row.MaxDamage[3] != (ushort)item.DamageMaxs[3] ||
                    //row.MaxDamage[4] != (ushort)item.DamageMaxs[4] ||
                    //row.Resistances[0] != (short)item.Armor ||
                    row.Resistances[1] != (short)item.HolyResistance ||
                    row.Resistances[2] != (short)item.FireResistance ||
                    row.Resistances[3] != (short)item.NatureResistance ||
                    row.Resistances[4] != (short)item.FrostResistance ||
                    row.Resistances[5] != (short)item.ShadowResistance ||
                    row.Resistances[6] != (short)item.ArcaneResistance)
                {
                    Log.Print(LogType.Storage, $"Item #{item.Entry} needs to be updated.");

                    string msg;
                    if (row.ClassId != (byte)item.Class)
                        Log.Print(LogType.Storage, $"ClassId {row.ClassId} vs {item.Class}");
                    if (row.SubclassId != (byte)item.SubClass)
                        Log.Print(LogType.Storage, $"SubclassId {row.SubclassId} vs {item.SubClass}");
                    if (row.Material != (byte)item.Material)
                        Log.Print(LogType.Storage, $"Material {row.Material} vs {item.Material}");
                    if (row.InventoryType != (sbyte)item.InventoryType)
                        Log.Print(LogType.Storage, $"InventoryType {row.InventoryType} vs {item.InventoryType}");
                    if (row.RequiredLevel != (int)item.RequiredLevel)
                        Log.Print(LogType.Storage, $"RequiredLevel {row.RequiredLevel} vs {item.RequiredLevel}");
                    if (row.SheatheType != (byte)item.SheathType)
                        Log.Print(LogType.Storage, $"SheatheType {row.SheatheType} vs {item.SheathType}");
                    if (row.RandomProperty != (ushort)item.RandomProperty)
                        Log.Print(LogType.Storage, $"RandomProperty {row.RandomProperty} vs {item.RandomProperty}");
                    if (row.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix)
                        Log.Print(LogType.Storage, $"ItemRandomSuffixGroupId {row.ItemRandomSuffixGroupId} vs {item.RandomSuffix}");
                    if (row.IconFileDataId != iconFileDataId)
                        Log.Print(LogType.Storage, $"IconFileDataId {row.IconFileDataId} vs {iconFileDataId}");
                    if (row.MaxDurability != item.MaxDurability)
                        Log.Print(LogType.Storage, $"MaxDurability {row.MaxDurability} vs {item.MaxDurability}");
                    if (row.AmmoType != (byte)item.AmmoType)
                        Log.Print(LogType.Storage, $"AmmoType {row.AmmoType} vs {item.AmmoType}");
                    for (int i = 0; i < 5; i++)
                    {
                        if (row.DamageType[i] != (byte)item.DamageTypes[i])
                            Log.Print(LogType.Storage, $"DamageType[{i}] {row.DamageType[i]} vs {item.DamageTypes[i]}");
                    }
                    if (row.Resistances[1] != (short)item.HolyResistance)
                        Log.Print(LogType.Storage, $"Resistances[1] {row.Resistances[1]} vs {item.HolyResistance}");
                    if (row.Resistances[2] != (short)item.FireResistance)
                        Log.Print(LogType.Storage, $"Resistances[2] {row.Resistances[2]} vs {item.FireResistance}");
                    if (row.Resistances[3] != (short)item.NatureResistance)
                        Log.Print(LogType.Storage, $"Resistances[3] {row.Resistances[3]} vs {item.NatureResistance}");
                    if (row.Resistances[4] != (short)item.FrostResistance)
                        Log.Print(LogType.Storage, $"Resistances[4] {row.Resistances[4]} vs {item.FrostResistance}");
                    if (row.Resistances[5] != (short)item.ShadowResistance)
                        Log.Print(LogType.Storage, $"Resistances[5] {row.Resistances[5]} vs {item.ShadowResistance}");
                    if (row.Resistances[6] != (short)item.ArcaneResistance)
                        Log.Print(LogType.Storage, $"Resistances[6] {row.Resistances[6]} vs {item.ArcaneResistance}");

                    // something is different so update current data
                    UpdateItemRecord(row, item);
                    UpdateHotfix(row);
                    return GenerateHotFixMessage(row);
                }
            }
            else
            {
                // item is missing so add new record
                //Log.Print(LogType.Storage, $"Item #{item.Entry} needs to be created.");
                row = AddItemRecord(item);
                if (row == null)
                    return null;

                UpdateHotfix(row);
                return GenerateHotFixMessage(row);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateItemSparseUpdateIfNeeded(ItemTemplate item)
        {
            ItemSparseRecordsStore.TryGetValue(item.Entry, out var row);
            if (row != null)
            {
                if (//row.AllowableRace != item.AllowedRaces ||
                    !row.Description.Equals(item.Description) ||
                    !row.Name4.Equals(item.Name[3]) ||
                    !row.Name3.Equals(item.Name[2]) ||
                    !row.Name2.Equals(item.Name[1]) ||
                    !row.Name1.Equals(item.Name[0]) ||
                    row.DurationInInventory != item.Duration ||
                    row.BagFamily != item.BagFamily ||
                    row.RangeMod != item.RangedMod ||
                    //row.Stackable != item.MaxStackSize ||
                    //row.MaxCount != item.MaxCount ||
                    row.RequiredAbility != item.RequiredSpell ||
                    row.SellPrice != item.SellPrice ||
                    row.BuyPrice != item.BuyPrice ||
                    //row.Flags[0] != item.Flags ||
                    //row.Flags[1] != item.FlagsExtra ||
                    row.MaxDurability != item.MaxDurability ||
                    row.RequiredHoliday != (ushort)item.HolidayID ||
                    row.LimitCategory != (ushort)item.ItemLimitCategory ||
                    row.GemProperties != (ushort)item.GemProperties ||
                    row.SocketMatchEnchantmentId != (ushort)item.SocketBonus ||
                    row.TotemCategoryId != (ushort)item.TotemCategory ||
                    row.InstanceBound != (ushort)item.MapID ||
                    row.ZoneBound[0] != (ushort)item.AreaID ||
                    row.ItemSet != (ushort)item.ItemSet ||
                    row.LockId != (ushort)item.LockId ||
                    row.StartQuestId != (ushort)item.StartQuestId ||
                    row.PageText != (ushort)item.PageText ||
                    row.Delay != (ushort)item.Delay ||
                    row.RequiredReputationId != (ushort)item.RequiredRepFaction ||
                    row.RequiredSkillRank != (ushort)item.RequiredSkillLevel ||
                    row.RequiredSkill != (ushort)item.RequiredSkillId ||
                    row.ItemLevel != (ushort)item.ItemLevel ||
                    //row.AllowableClass != (short)item.AllowedClasses ||
                    row.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix ||
                    row.RandomProperty != (ushort)item.RandomProperty ||
                    //row.MinDamage[0] != (ushort)item.DamageMins[0] ||
                    //row.MinDamage[1] != (ushort)item.DamageMins[1] ||
                    //row.MinDamage[2] != (ushort)item.DamageMins[2] ||
                    //row.MinDamage[3] != (ushort)item.DamageMins[3] ||
                    //row.MinDamage[4] != (ushort)item.DamageMins[4] ||
                    //row.MaxDamage[0] != (ushort)item.DamageMaxs[0] ||
                    //row.MaxDamage[1] != (ushort)item.DamageMaxs[1] ||
                    //row.MaxDamage[2] != (ushort)item.DamageMaxs[2] ||
                    //row.MaxDamage[3] != (ushort)item.DamageMaxs[3] ||
                    //row.MaxDamage[4] != (ushort)item.DamageMaxs[4] ||
                    //row.Resistances[0] != (short)item.Armor ||
                    row.Resistances[1] != (short)item.HolyResistance ||
                    row.Resistances[2] != (short)item.FireResistance ||
                    row.Resistances[3] != (short)item.NatureResistance ||
                    row.Resistances[4] != (short)item.FrostResistance ||
                    row.Resistances[5] != (short)item.ShadowResistance ||
                    row.Resistances[6] != (short)item.ArcaneResistance ||
                    row.ScalingStatDistributionId != (ushort)item.ScalingStatDistribution ||
                    row.SocketType[0] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[0]) ||
                    row.SocketType[1] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[1]) ||
                    row.SocketType[2] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[2]) ||
                    row.SheatheType != (byte)item.SheathType ||
                    row.Material != (byte)item.Material ||
                    row.PageMaterial != (byte)item.PageMaterial ||
                    row.PageLanguage != (byte)item.Language ||
                    row.Bonding != (byte)item.Bonding ||
                    row.DamageType != (byte)item.DamageTypes[0] ||
                    row.StatType[0] != (sbyte)item.StatTypes[0] && (row.StatValue[0] != 0 || item.StatValues[0] != 0) ||
                    row.StatType[1] != (sbyte)item.StatTypes[1] && (row.StatValue[1] != 0 || item.StatValues[1] != 0) ||
                    row.StatType[2] != (sbyte)item.StatTypes[2] && (row.StatValue[2] != 0 || item.StatValues[2] != 0) ||
                    row.StatType[3] != (sbyte)item.StatTypes[3] && (row.StatValue[3] != 0 || item.StatValues[3] != 0) ||
                    row.StatType[4] != (sbyte)item.StatTypes[4] && (row.StatValue[4] != 0 || item.StatValues[4] != 0) ||
                    row.StatType[5] != (sbyte)item.StatTypes[5] && (row.StatValue[5] != 0 || item.StatValues[5] != 0) ||
                    row.StatType[6] != (sbyte)item.StatTypes[6] && (row.StatValue[6] != 0 || item.StatValues[6] != 0) ||
                    row.StatType[7] != (sbyte)item.StatTypes[7] && (row.StatValue[7] != 0 || item.StatValues[7] != 0) ||
                    row.StatType[8] != (sbyte)item.StatTypes[8] && (row.StatValue[8] != 0 || item.StatValues[8] != 0) ||
                    row.StatType[9] != (sbyte)item.StatTypes[9] && (row.StatValue[9] != 0 || item.StatValues[9] != 0) ||
                    row.ContainerSlots != (byte)item.ContainerSlots ||
                    row.RequiredReputationRank != (byte)item.RequiredRepValue ||
                    row.RequiredCityRank != (byte)item.RequiredCityRank ||
                    row.RequiredHonorRank != (byte)item.RequiredHonorRank ||
                    row.InventoryType != (byte)item.InventoryType ||
                    row.OverallQualityId != (byte)item.Quality ||
                    row.AmmoType != (byte)item.AmmoType ||
                    row.StatValue[0] != (sbyte)item.StatValues[0] ||
                    row.StatValue[1] != (sbyte)item.StatValues[1] ||
                    row.StatValue[2] != (sbyte)item.StatValues[2] ||
                    row.StatValue[3] != (sbyte)item.StatValues[3] ||
                    row.StatValue[4] != (sbyte)item.StatValues[4] ||
                    row.StatValue[5] != (sbyte)item.StatValues[5] ||
                    row.StatValue[6] != (sbyte)item.StatValues[6] ||
                    row.StatValue[7] != (sbyte)item.StatValues[7] ||
                    row.StatValue[8] != (sbyte)item.StatValues[8] ||
                    row.StatValue[9] != (sbyte)item.StatValues[9] ||
                    row.RequiredLevel != (sbyte)item.RequiredLevel)
                {
                    Log.Print(LogType.Storage, $"ItemSparse #{item.Entry} needs to be updated.");

                    if (!row.Description.Equals(item.Description))
                        Log.Print(LogType.Storage, $"Description \"{row.Description}\" vs \"{item.Description}\"");
                    if (!row.Name4.Equals(item.Name[3]))
                        Log.Print(LogType.Storage, $"Name4 \"{row.Name4}\" vs \"{item.Name[3]}\"");
                    if (!row.Name3.Equals(item.Name[2]))
                        Log.Print(LogType.Storage, $"Name3 \"{row.Name3}\" vs \"{item.Name[2]}\"");
                    if (!row.Name2.Equals(item.Name[1]))
                        Log.Print(LogType.Storage, $"Name2 \"{row.Name2}\" vs \"{item.Name[1]}\"");
                    if (!row.Name1.Equals(item.Name[0]))
                        Log.Print(LogType.Storage, $"Name1 \"{row.Name1}\" vs \"{item.Name[0]}\"");
                    if (row.DurationInInventory != item.Duration)
                        Log.Print(LogType.Storage, $"DurationInInventory {row.DurationInInventory} vs {item.Duration}");
                    if (row.BagFamily != item.BagFamily)
                        Log.Print(LogType.Storage, $"BagFamily {row.BagFamily} vs {item.BagFamily}");
                    if (row.RangeMod != item.RangedMod)
                        Log.Print(LogType.Storage, $"RangeMod {row.RangeMod} vs {item.RangedMod}");
                    if (row.RequiredAbility != item.RequiredSpell)
                        Log.Print(LogType.Storage, $"RequiredAbility {row.RequiredAbility} vs {item.RequiredSpell}");
                    if (row.SellPrice != item.SellPrice)
                        Log.Print(LogType.Storage, $"SellPrice {row.SellPrice} vs {item.SellPrice}");
                    if (row.BuyPrice != item.BuyPrice)
                        Log.Print(LogType.Storage, $"BuyPrice {row.BuyPrice} vs {item.BuyPrice}");
                    if (row.MaxDurability != item.MaxDurability)
                        Log.Print(LogType.Storage, $"MaxDurability {row.MaxDurability} vs {item.MaxDurability}");
                    if (row.RequiredHoliday != (ushort)item.HolidayID)
                        Log.Print(LogType.Storage, $"RequiredHoliday {row.RequiredHoliday} vs {item.HolidayID}");
                    if (row.LimitCategory != (ushort)item.ItemLimitCategory)
                        Log.Print(LogType.Storage, $"LimitCategory {row.LimitCategory} vs {item.ItemLimitCategory}");
                    if (row.GemProperties != (ushort)item.GemProperties)
                        Log.Print(LogType.Storage, $"GemProperties {row.GemProperties} vs {item.GemProperties}");
                    if (row.SocketMatchEnchantmentId != (ushort)item.SocketBonus)
                        Log.Print(LogType.Storage, $"SocketMatchEnchantmentId {row.SocketMatchEnchantmentId} vs {item.SocketBonus}");
                    if (row.TotemCategoryId != (ushort)item.TotemCategory)
                        Log.Print(LogType.Storage, $"TotemCategoryId {row.TotemCategoryId} vs {item.TotemCategory}");
                    if (row.InstanceBound != (ushort)item.MapID)
                        Log.Print(LogType.Storage, $"InstanceBound {row.InstanceBound} vs {item.MapID}");
                    if (row.ZoneBound[0] != (ushort)item.AreaID)
                        Log.Print(LogType.Storage, $"ZoneBound[0] {row.ZoneBound[0]} vs {item.AreaID}");
                    if (row.ItemSet != (ushort)item.ItemSet)
                        Log.Print(LogType.Storage, $"ItemSet {row.ItemSet} vs {item.ItemSet}");
                    if (row.LockId != (ushort)item.LockId)
                        Log.Print(LogType.Storage, $"LockId {row.LockId} vs {item.LockId}");
                    if (row.StartQuestId != (ushort)item.StartQuestId)
                        Log.Print(LogType.Storage, $"StartQuestId {row.StartQuestId} vs {item.StartQuestId}");
                    if (row.PageText != (ushort)item.PageText)
                        Log.Print(LogType.Storage, $"PageText {row.PageText} vs {item.PageText}");
                    if (row.Delay != (ushort)item.Delay)
                        Log.Print(LogType.Storage, $"Delay {row.Delay} vs {item.Delay}");
                    if (row.RequiredReputationId != (ushort)item.RequiredRepFaction)
                        Log.Print(LogType.Storage, $"RequiredReputationId {row.RequiredReputationId} vs {item.RequiredRepFaction}");
                    if (row.RequiredSkillRank != (ushort)item.RequiredSkillLevel)
                        Log.Print(LogType.Storage, $"RequiredSkillRank {row.RequiredSkillRank} vs {item.RequiredSkillLevel}");
                    if (row.RequiredSkill != (ushort)item.RequiredSkillId)
                        Log.Print(LogType.Storage, $"RequiredSkill {row.RequiredSkill} vs {item.RequiredSkillId}");
                    if (row.ItemLevel != (ushort)item.ItemLevel)
                        Log.Print(LogType.Storage, $"ItemLevel {row.ItemLevel} vs {item.ItemLevel}");
                    if (row.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix)
                        Log.Print(LogType.Storage, $"ItemRandomSuffixGroupId {row.ItemRandomSuffixGroupId} vs {item.RandomSuffix}");
                    if (row.RandomProperty != (ushort)item.RandomProperty)
                        Log.Print(LogType.Storage, $"RandomProperty {row.RandomProperty} vs {item.RandomProperty}");
                    if (row.Resistances[1] != (short)item.HolyResistance)
                        Log.Print(LogType.Storage, $"Resistances[1] {row.Resistances[1]} vs {item.HolyResistance}");
                    if (row.Resistances[2] != (short)item.FireResistance)
                        Log.Print(LogType.Storage, $"Resistances[2] {row.Resistances[2]} vs {item.FireResistance}");
                    if (row.Resistances[3] != (short)item.NatureResistance)
                        Log.Print(LogType.Storage, $"Resistances[3]  {row.Resistances[3]} vs {item.NatureResistance}");
                    if (row.Resistances[4] != (short)item.FrostResistance)
                        Log.Print(LogType.Storage, $"Resistances[4] {row.Resistances[4]} vs {item.FrostResistance}");
                    if (row.Resistances[5] != (short)item.ShadowResistance)
                        Log.Print(LogType.Storage, $"Resistances[5] {row.Resistances[5]} vs {item.ShadowResistance}");
                    if (row.Resistances[6] != (short)item.ArcaneResistance)
                        Log.Print(LogType.Storage, $"Resistances[6] {row.Resistances[6]} vs {item.ArcaneResistance}");
                    if (row.ScalingStatDistributionId != (ushort)item.ScalingStatDistribution)
                        Log.Print(LogType.Storage, $"ScalingStatDistributionId {row.ScalingStatDistributionId} vs {item.ScalingStatDistribution}");
                    for (int i = 0; i < 3; i++)
                    {
                        if (row.SocketType[i] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[i]))
                            Log.Print(LogType.Storage, $"SocketType[{i}] {row.SocketType[i]} vs {ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[i])}");
                    }
                    if (row.SheatheType != (byte)item.SheathType)
                        Log.Print(LogType.Storage, $"SheatheType {row.SheatheType} vs {item.SheathType}");
                    if (row.Material != (byte)item.Material)
                        Log.Print(LogType.Storage, $"Material {row.Material} vs {item.Material}");
                    if (row.PageMaterial != (byte)item.PageMaterial)
                        Log.Print(LogType.Storage, $"PageMaterial {row.PageMaterial} vs {item.PageMaterial}");
                    if (row.PageLanguage != (byte)item.Language)
                        Log.Print(LogType.Storage, $"PageLanguage {row.PageLanguage} vs {item.Language}");
                    if (row.Bonding != (byte)item.Bonding)
                        Log.Print(LogType.Storage, $"Bonding {row.Bonding} vs {item.Bonding}");
                    if (row.DamageType != (byte)item.DamageTypes[0])
                        Log.Print(LogType.Storage, $"DamageType {row.DamageType} vs {item.DamageTypes[0]}");
                    for (int i = 0; i < 10; i++)
                    {
                        if (row.StatType[i] != (sbyte)item.StatTypes[i] && (row.StatValue[i] != 0 || item.StatValues[i] != 0))
                            Log.Print(LogType.Storage, $"StatType[{i}] {row.StatType[i]} vs {item.StatTypes[i]}");
                    }
                    if (row.ContainerSlots != (byte)item.ContainerSlots)
                        Log.Print(LogType.Storage, $"ContainerSlots {row.ContainerSlots} vs {item.ContainerSlots}");
                    if (row.RequiredReputationRank != (byte)item.RequiredRepValue)
                        Log.Print(LogType.Storage, $"RequiredReputationRank {row.RequiredReputationRank} vs {item.RequiredRepValue}");
                    if (row.RequiredCityRank != (byte)item.RequiredCityRank)
                        Log.Print(LogType.Storage, $"RequiredCityRank {row.RequiredCityRank} vs {item.RequiredCityRank}");
                    if (row.RequiredHonorRank != (byte)item.RequiredHonorRank)
                        Log.Print(LogType.Storage, $"RequiredHonorRank {row.RequiredHonorRank} vs {item.RequiredHonorRank}");
                    if (row.InventoryType != (byte)item.InventoryType)
                        Log.Print(LogType.Storage, $"InventoryType {row.InventoryType} vs {item.InventoryType}");
                    if (row.OverallQualityId != (byte)item.Quality)
                        Log.Print(LogType.Storage, $"OverallQualityId {row.OverallQualityId} vs {item.Quality}");
                    if (row.AmmoType != (byte)item.AmmoType)
                        Log.Print(LogType.Storage, $"AmmoType {row.AmmoType} vs {item.AmmoType}");
                    for (int i = 0; i < 10; i++)
                    {
                        if (row.StatValue[0] != (sbyte)item.StatValues[0])
                            Log.Print(LogType.Storage, $"StatValue[{i}] {row.StatValue[i]} vs {item.StatValues[i]}");
                    }
                    if (row.RequiredLevel != (sbyte)item.RequiredLevel)
                        Log.Print(LogType.Storage, $"RequiredLevel {row.RequiredLevel} vs {item.RequiredLevel}");

                    // something is different so update current data
                    UpdateItemSparseRecord(row, item);

                    // sending db reply for existing itemsparse entry crashes game, so prepare hotfix for next login
                    UpdateHotfix(row);
                    return null;
                }
            }
            else
            {
                // item is missing so add new record
                //Log.Print(LogType.Storage, $"ItemSparse #{item.Entry} needs to be created.");
                row = AddItemSparseRecord(item);
                if (row == null)
                    return null;

                UpdateHotfix(row);
                return GenerateHotFixMessage(row);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateItemEffectUpdateIfNeeded(ItemTemplate item, byte slot)
        {
            ItemEffect effect = GetItemEffectByItemId(item.Entry, slot);
            if (effect != null)
            {
                // compare to spell data
                bool wrongCategory = false;
                bool wrongCooldown = false;
                bool wrongCatCooldown = false;
                if (item.TriggeredSpellIds[slot] > 0)
                {
                    ItemSpellsData data;
                    ItemSpellsDataStore.TryGetValue((uint)item.TriggeredSpellIds[slot], out data);
                    if (data != null)
                    {
                        // category
                        if (effect.SpellCategoryID != item.TriggeredSpellCategories[slot])
                            wrongCategory = data.Category != item.TriggeredSpellCategories[slot];
                        // cooldown
                        if (Math.Abs(effect.CoolDownMSec - item.TriggeredSpellCooldowns[slot]) > 1)
                            wrongCooldown = data.RecoveryTime != item.TriggeredSpellCooldowns[slot];
                        // category cooldown
                        if (Math.Abs(effect.CategoryCoolDownMSec - item.TriggeredSpellCategoryCooldowns[slot]) > 1)
                            wrongCatCooldown = data.CategoryRecoveryTime != item.TriggeredSpellCategoryCooldowns[slot];
                    }
                }
                if (effect.TriggerType != item.TriggeredSpellTypes[slot] ||
                    effect.Charges != item.TriggeredSpellCharges[slot] ||
                    wrongCooldown ||
                    wrongCatCooldown ||
                    wrongCategory ||
                    effect.SpellID != item.TriggeredSpellIds[slot])
                {
                    if (item.TriggeredSpellIds[slot] > 0)
                    {
                        Log.Print(LogType.Storage, $"ItemEffect for item #{item.Entry} slot #{slot} needs to be updated.");

                        if (effect.TriggerType != item.TriggeredSpellTypes[slot])
                            Log.Print(LogType.Storage, $"TriggerType {effect.TriggerType} vs {item.TriggeredSpellTypes[slot]}");
                        if (effect.Charges != item.TriggeredSpellCharges[slot])
                            Log.Print(LogType.Storage, $"Charges {effect.Charges} vs {item.TriggeredSpellCharges[slot]}");
                        if (wrongCooldown)
                            Log.Print(LogType.Storage, $"CoolDownMSec {effect.CoolDownMSec} vs {item.TriggeredSpellCooldowns[slot]}");
                        if (wrongCatCooldown)
                            Log.Print(LogType.Storage, $"CategoryCoolDownMSec {effect.CategoryCoolDownMSec} vs {item.TriggeredSpellCategoryCooldowns[slot]}");
                        if (wrongCategory)
                            Log.Print(LogType.Storage, $"SpellCategoryId {effect.SpellCategoryID} vs {item.TriggeredSpellCategories[slot]}");
                        if (effect.SpellID != item.TriggeredSpellIds[slot])
                            Log.Print(LogType.Storage, $"SpellId {effect.SpellID} vs {item.TriggeredSpellIds[slot]}");

                        effect.TriggerType = (sbyte)item.TriggeredSpellTypes[slot];
                        effect.Charges = (short)item.TriggeredSpellCharges[slot];
                        effect.CoolDownMSec = wrongCooldown ? item.TriggeredSpellCooldowns[slot] : -1;
                        effect.CategoryCoolDownMSec = wrongCatCooldown ? item.TriggeredSpellCategoryCooldowns[slot] : -1;
                        effect.SpellCategoryID = wrongCategory ? (ushort)item.TriggeredSpellCategories[slot] : (ushort)0;
                        effect.SpellID = item.TriggeredSpellIds[slot];

                        // there is a spell so update current data
                        UpdateItemEffectRecord(effect, item);
                        UpdateHotfix(effect);
                        return GenerateHotFixMessage(effect);
                    }
                    else
                    {
                        // there is no spell so remove the record
                        //Log.Print(LogType.Storage, $"ItemEffect for item #{item.Entry} slot #{slot} needs to be deleted.");
                        RemoveItemEffectRecord(effect);
                        UpdateHotfix(effect, true);
                        return GenerateHotFixMessage(effect, true);
                    }
                }
            }
            else if (item.TriggeredSpellIds[slot] > 0)
            {
                // there is a spell so add new record
                //Log.Print(LogType.Storage, $"ItemEffect for item #{item.Entry} slot #{slot} needs to be created.");
                effect = AddItemEffectRecord(item, slot);
                if (effect == null)
                    return null;

                UpdateHotfix(effect);
                return GenerateHotFixMessage(effect);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateItemAppearanceUpdateIfNeeded(ItemTemplate item)
        {
            ItemAppearance appearance = GetItemAppearanceByDisplayId(item.DisplayID);
            if (appearance != null)
            {
                // never can happen, should not edit existing ItemAppearance as can affect other items

                /*if (appearance.ItemDisplayInfoID != item.DisplayID)
                {
                    Log.Print(LogType.Storage, $"ItemAppearance for item #{item.Entry}, DisplayID #{item.DisplayID} needs to be updated.");
                    Log.Print(LogType.Storage, $"ItemDisplayID {appearance.ItemDisplayInfoID} vs {item.DisplayID}");

                    // something is different so update current data
                    UpdateItemAppearanceRecord(appearance, item);
                    UpdateItemAppearanceHotfix(appearance);
                    return GenerateDbReply(appearance);
                }*/
            }
            else
            {
                // item appearance is missing so add new record
                //Log.Print(LogType.Storage, $"ItemAppearance for item #{item.Entry}, DisplayID #{item.DisplayID} needs to be created.");
                appearance = AddItemAppearanceRecord(item);
                if (appearance == null)
                    return null;

                UpdateHotfix(appearance);
                return GenerateHotFixMessage(appearance);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateItemModifiedAppearanceUpdateIfNeeded(ItemTemplate item)
        {
            ItemModifiedAppearance modAppearance = GetItemModifiedAppearanceByItemId(item.Entry);
            if (modAppearance != null)
            {
                ItemAppearance appearance;
                ItemAppearanceStore.TryGetValue((uint)modAppearance.ItemAppearanceID, out appearance);
                if (appearance == null || appearance.ItemDisplayInfoID != item.DisplayID)
                {
                    Log.Print(LogType.Storage, $"ItemModifiedAppearance #{modAppearance.Id} for item #{item.Entry} needs to be updated.");

                    if (appearance == null)
                        Log.Print(LogType.Storage, $"ItemAppearance #{modAppearance.ItemAppearanceID} missing.");
                    else if (appearance.ItemDisplayInfoID != item.DisplayID)
                        Log.Print(LogType.Storage, $"DisplayID {appearance.ItemDisplayInfoID} vs {item.DisplayID}");

                    // something is different so update current data
                    UpdateItemModifiedAppearanceRecord(modAppearance, item);
                    UpdateHotfix(modAppearance);
                    return GenerateHotFixMessage(modAppearance);
                }
            }
            else
            {
                // item modified appearance is missing so add new record
                //Log.Print(LogType.Storage, $"ItemModifiedAppearance for item #{item.Entry} needs to be created.");
                modAppearance = AddItemModifiedAppearanceRecord(item);
                if (modAppearance == null)
                    return null;

                UpdateHotfix(modAppearance);
                return GenerateHotFixMessage(modAppearance);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateHotFixMessage(object obj, bool remove = false)
        {
            Server.Packets.HotFixMessage reply = new();

            if (obj == null)
            {
                Log.Print(LogType.Error, $"DBReply for NULL object requested!");
                return null;
            }
            System.Type type = obj.GetType();
            //Log.Print(LogType.Storage, $"DBReply generating for {type}");
            if (obj is ItemRecord itemRecord)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)itemRecord.Id, DB2Hash.Item);
                reply.Hotfixes.AddRange(records);
            }
            else if (obj is ItemSparseRecord itemSparseRecord)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)itemSparseRecord.Id, DB2Hash.ItemSparse);
                reply.Hotfixes.AddRange(records);
            }
            else if (obj is ItemEffect itemEffect)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)itemEffect.Id, DB2Hash.ItemEffect);
                reply.Hotfixes.AddRange(records);
            }
            else if (obj is ItemAppearance itemAppearance)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)itemAppearance.Id, DB2Hash.ItemAppearance);
                reply.Hotfixes.AddRange(records);
            }
            else if (obj is ItemModifiedAppearance itemModifiedAppearance)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)itemModifiedAppearance.Id, DB2Hash.ItemModifiedAppearance);
                reply.Hotfixes.AddRange(records);
            }
            else
            {
                Log.Print(LogType.Error, $"Unsupported DBReply requested! ({type})");
                return null;
            }
            return reply;
        }

        public static ItemRecord AddItemRecord(ItemTemplate item)
        {
            ItemRecord record = new()
            {
                Id = (int)item.Entry
            };
            UpdateItemRecord(record, item);
            ItemRecordsStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"Item #{record.Id} created.");
            return record;
        }

        public static void UpdateItemRecord(ItemRecord row, ItemTemplate item)
        {
            row.ClassId = (byte)item.Class;
            row.SubclassId = (byte)item.SubClass;
            row.Material = (byte)item.Material;
            row.InventoryType = (sbyte)item.InventoryType;
            row.RequiredLevel = (int)item.RequiredLevel;
            row.SheatheType = (byte)item.SheathType;
            row.RandomProperty = (ushort)item.RandomProperty;
            row.ItemRandomSuffixGroupId = (ushort)item.RandomSuffix;
            row.SoundOverrideSubclassId = -1;
            row.ScalingStatDistributionId = 0;
            row.IconFileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);
            row.ItemGroupSoundsId = 0;
            row.ContentTuningId = 0;
            row.MaxDurability = item.MaxDurability;
            row.AmmoType = (byte)item.AmmoType;
            row.DamageType[0] = (byte)item.DamageTypes[0];
            row.DamageType[1] = (byte)item.DamageTypes[1];
            row.DamageType[2] = (byte)item.DamageTypes[2];
            row.DamageType[3] = (byte)item.DamageTypes[3];
            row.DamageType[4] = (byte)item.DamageTypes[4];
            row.Resistances[0] = (short)item.Armor;
            row.Resistances[1] = (short)item.HolyResistance;
            row.Resistances[2] = (short)item.FireResistance;
            row.Resistances[3] = (short)item.NatureResistance;
            row.Resistances[4] = (short)item.FrostResistance;
            row.Resistances[5] = (short)item.ShadowResistance;
            row.Resistances[6] = (short)item.ArcaneResistance;
            row.MinDamage[0] = (ushort)item.DamageMins[0];
            row.MinDamage[1] = (ushort)item.DamageMins[1];
            row.MinDamage[2] = (ushort)item.DamageMins[2];
            row.MinDamage[3] = (ushort)item.DamageMins[3];
            row.MinDamage[4] = (ushort)item.DamageMins[4];
            row.MaxDamage[0] = (ushort)item.DamageMaxs[0];
            row.MaxDamage[1] = (ushort)item.DamageMaxs[1];
            row.MaxDamage[2] = (ushort)item.DamageMaxs[2];
            row.MaxDamage[3] = (ushort)item.DamageMaxs[3];
            row.MaxDamage[4] = (ushort)item.DamageMaxs[4];

            if (ItemRecordsStore.ContainsKey(item.Entry))
            {
                ItemRecordsStore[item.Entry] = row;
                //Log.Print(LogType.Storage, $"Item #{row.Id} updated.");
            }
        }

        public static ItemSparseRecord AddItemSparseRecord(ItemTemplate item)
        {
            ItemSparseRecord record = new()
            {
                Id = (int)item.Entry
            };
            UpdateItemSparseRecord(record, item);
            ItemSparseRecordsStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"ItemSparse #{record.Id} created.");
            return record;
        }

        public static void UpdateItemSparseRecord(ItemSparseRecord row, ItemTemplate item)
        {
            Span<int> StatValues = stackalloc int[10];
            for (int i = 0; i < item.StatsCount; i++)
            {
                StatValues[i] = item.StatValues[i];
                if (StatValues[i] > 127)
                    StatValues[i] = 127;
                if (StatValues[i] < -127)
                    StatValues[i] = -127;
            }

            row.AllowableRace = item.AllowedRaces;
            row.Description = item.Description;
            row.Name4 = item.Name[3];
            row.Name3 = item.Name[2];
            row.Name2 = item.Name[1];
            row.Name1 = item.Name[0];
            row.DurationInInventory = item.Duration;
            row.BagFamily = item.BagFamily;
            row.RangeMod = item.RangedMod;
            row.Stackable = item.MaxStackSize;
            row.MaxCount = item.MaxCount;
            row.RequiredAbility = item.RequiredSpell;
            row.SellPrice = item.SellPrice;
            row.BuyPrice = item.BuyPrice;
            row.Flags[0] = item.Flags;
            row.Flags[1] = item.FlagsExtra;
            row.MaxDurability = item.MaxDurability;
            row.RequiredHoliday = (ushort)item.HolidayID;
            row.LimitCategory = (ushort)item.ItemLimitCategory;
            row.GemProperties = (ushort)item.GemProperties;
            row.SocketMatchEnchantmentId = (ushort)item.SocketBonus;
            row.TotemCategoryId = (ushort)item.TotemCategory;
            row.InstanceBound = (ushort)item.MapID;
            row.ZoneBound[0] = (ushort)item.AreaID;
            row.ItemSet = (ushort)item.ItemSet;
            row.LockId = (ushort)item.LockId;
            row.StartQuestId = (ushort)item.StartQuestId;
            row.PageText = (ushort)item.PageText;
            row.Delay = (ushort)item.Delay;
            row.RequiredReputationId = (ushort)item.RequiredRepFaction;
            row.RequiredSkillRank = (ushort)item.RequiredSkillLevel;
            row.RequiredSkill = (ushort)item.RequiredSkillId;
            row.ItemLevel = (ushort)item.ItemLevel;
            row.AllowableClass = (short)item.AllowedClasses;
            row.ItemRandomSuffixGroupId = (ushort)item.RandomSuffix;
            row.RandomProperty = (ushort)item.RandomProperty;
            row.MinDamage[0] = (ushort)item.DamageMins[0];
            row.MinDamage[1] = (ushort)item.DamageMins[1];
            row.MinDamage[2] = (ushort)item.DamageMins[2];
            row.MinDamage[3] = (ushort)item.DamageMins[3];
            row.MinDamage[4] = (ushort)item.DamageMins[4];
            row.MaxDamage[0] = (ushort)item.DamageMaxs[0];
            row.MaxDamage[1] = (ushort)item.DamageMaxs[1];
            row.MaxDamage[2] = (ushort)item.DamageMaxs[2];
            row.MaxDamage[3] = (ushort)item.DamageMaxs[3];
            row.MaxDamage[4] = (ushort)item.DamageMaxs[4];
            row.Resistances[0] = (short)item.Armor;
            row.Resistances[1] = (short)item.HolyResistance;
            row.Resistances[2] = (short)item.FireResistance;
            row.Resistances[3] = (short)item.NatureResistance;
            row.Resistances[4] = (short)item.FrostResistance;
            row.Resistances[5] = (short)item.ShadowResistance;
            row.Resistances[6] = (short)item.ArcaneResistance;
            row.ScalingStatDistributionId = (ushort)item.ScalingStatDistribution;
            row.SocketType[0] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[0]);
            row.SocketType[1] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[1]);
            row.SocketType[2] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[2]);
            row.SheatheType = (byte)item.SheathType;
            row.Material = (byte)item.Material;
            row.PageMaterial = (byte)item.PageMaterial;
            row.PageLanguage = (byte)item.Language;
            row.Bonding = (byte)item.Bonding;
            row.DamageType = (byte)item.DamageTypes[0];
            row.StatType[0] = (sbyte)item.StatTypes[0];
            row.StatType[1] = (sbyte)item.StatTypes[1];
            row.StatType[2] = (sbyte)item.StatTypes[2];
            row.StatType[3] = (sbyte)item.StatTypes[3];
            row.StatType[4] = (sbyte)item.StatTypes[4];
            row.StatType[5] = (sbyte)item.StatTypes[5];
            row.StatType[6] = (sbyte)item.StatTypes[6];
            row.StatType[7] = (sbyte)item.StatTypes[7];
            row.StatType[8] = (sbyte)item.StatTypes[8];
            row.StatType[9] = (sbyte)item.StatTypes[9];
            row.ContainerSlots = (byte)item.ContainerSlots;
            row.RequiredReputationRank = (byte)item.RequiredRepValue;
            row.RequiredCityRank = (byte)item.RequiredCityRank;
            row.RequiredHonorRank = (byte)item.RequiredHonorRank;
            row.InventoryType = (byte)item.InventoryType;
            row.OverallQualityId = (byte)item.Quality;
            row.AmmoType = (byte)item.AmmoType;
            row.StatValue[0] = (sbyte)StatValues[0];
            row.StatValue[1] = (sbyte)StatValues[1];
            row.StatValue[2] = (sbyte)StatValues[2];
            row.StatValue[3] = (sbyte)StatValues[3];
            row.StatValue[4] = (sbyte)StatValues[4];
            row.StatValue[5] = (sbyte)StatValues[5];
            row.StatValue[6] = (sbyte)StatValues[6];
            row.StatValue[7] = (sbyte)StatValues[7];
            row.StatValue[8] = (sbyte)StatValues[8];
            row.StatValue[9] = (sbyte)StatValues[9];
            row.RequiredLevel = (sbyte)item.RequiredLevel;

            if (ItemSparseRecordsStore.ContainsKey(item.Entry))
            {
                ItemSparseRecordsStore[item.Entry] = row;
                //Log.Print(LogType.Storage, $"ItemSparse #{row.Id} updated.");
            }
        }

        public static ItemEffect AddItemEffectRecord(ItemTemplate item, byte slot)
        {
            ItemEffect record = new()
            {
                Id = (int)GetFirstFreeId(ItemEffectStore),
                LegacySlotIndex = slot
            };
            UpdateItemEffectRecord(record, item);
            Log.Print(LogType.Storage, $"ItemEffect #{record.Id} created for item #{item.Entry} slot #{slot}.");
            return record;
        }

        public static void UpdateItemEffectRecord(ItemEffect effect, ItemTemplate item)
        {
            byte i = effect.LegacySlotIndex;
            effect.TriggerType = (sbyte)item.TriggeredSpellTypes[i];
            effect.Charges = (short)item.TriggeredSpellCharges[i];
            effect.CoolDownMSec = item.TriggeredSpellCooldowns[i];
            effect.CategoryCoolDownMSec = item.TriggeredSpellCategoryCooldowns[i];
            effect.SpellCategoryID = (ushort)item.TriggeredSpellCategories[i];
            effect.SpellID = item.TriggeredSpellIds[i];
            effect.ChrSpecializationID = 0;
            effect.ParentItemID = (int)item.Entry;

            CollectionsMarshal.GetValueRefOrAddDefault(ItemEffectStore, (uint)effect.Id, out _) = effect;
            //Log.Print(LogType.Storage, $"ItemEffect #{effect.Id} updated for item #{item.Entry} slot #{i}.");
        }

        public static void RemoveItemEffectRecord(ItemEffect effect)
        {
            ItemEffectStore.Remove((uint)effect.Id);
            Log.Print(LogType.Storage, $"ItemEffect #{effect.Id} removed for item #{effect.ParentItemID} slot #{effect.LegacySlotIndex}.");
        }

        public static ItemAppearance AddItemAppearanceRecord(ItemTemplate item)
        {
            ItemAppearance record = new();
            record.Id = (int)GetFirstFreeId(ItemAppearanceStore);
            UpdateItemAppearanceRecord(record, item);
            ItemAppearanceStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"ItemAppearance #{record.Id} created for DisplayID #{item.DisplayID}.");
            return record;
        }

        public static void UpdateItemAppearanceRecord(ItemAppearance appearance, ItemTemplate item)
        {
            int fileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);

            appearance.DisplayType = 11; // todo find out
            appearance.ItemDisplayInfoID = (int)item.DisplayID;
            appearance.DefaultIconFileDataID = fileDataId;
            appearance.UiOrder = 0;

            if (ItemAppearanceStore.ContainsKey((uint)appearance.Id))
            {
                ItemAppearanceStore[(uint)appearance.Id] = appearance;
                //Log.Print(LogType.Storage, $"ItemAppearance #{appearance.Id} updated for DisplayID #{item.DisplayID}.");
            }
        }

        public static ItemModifiedAppearance AddItemModifiedAppearanceRecord(ItemTemplate item)
        {
            ItemModifiedAppearance record = new();
            record.Id = (int)GetFirstFreeId(ItemModifiedAppearanceStore);
            UpdateItemModifiedAppearanceRecord(record, item);
            if (record.ItemID != item.Entry)
            {
                Log.Print(LogType.Error, $"ItemModifiedAppearance #{record.Id} create failed for item #{record.ItemID}.");
                return null;
            }
            ItemModifiedAppearanceStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"ItemModifiedAppearance #{record.Id} created for item #{record.ItemID}.");
            return record;
        }

        public static void UpdateItemModifiedAppearanceRecord(ItemModifiedAppearance modAppearance, ItemTemplate item)
        {
            ItemAppearance appearance = GetItemAppearanceByDisplayId(item.DisplayID);
            if (appearance == null) // should not happen
            {
                Log.Print(LogType.Error, $"ItemModifiedAppearance #{modAppearance.Id} update failed: no ItemAppearance for DisplayID #{item.DisplayID}");
                return;
            }

            modAppearance.ItemID = (int)item.Entry;
            modAppearance.ItemAppearanceModifierID = 0;
            modAppearance.ItemAppearanceID = appearance.Id;
            modAppearance.OrderIndex = 0;
            modAppearance.TransmogSourceTypeEnum = 0;

            if (ItemModifiedAppearanceStore.ContainsKey((uint)modAppearance.Id))
            {
                ItemModifiedAppearanceStore[(uint)modAppearance.Id] = modAppearance;
                //Log.Print(LogType.Storage, $"ItemModifiedAppearance #{modAppearance.Id} updated for item #{item.Entry}.");
            }
        }

        public static bool ItemCanHaveModel(ItemTemplate item)
        {
            // weapons
            if (item.Class == 2)
                return true;

            // armor (except necklaces, rings, trinkets, relics)
            if (item.Class == 4)
            {
                if (item.SubClass != 7 &&
                    item.SubClass != 8 &&
                    item.SubClass != 9 &&
                    item.InventoryType != 0 &&
                    item.InventoryType != 2 &&
                    item.InventoryType != 11 &&
                    item.InventoryType != 12 &&
                    item.InventoryType != 18 &&
                    item.InventoryType != 28)
                    return true;
            }

            // quivers
            if (item.Class == 11 && item.SubClass == 2)
                return true;

            return false;
        }

        public static void LoadCreatureDisplayInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfo{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = row[0].Parse<uint>();
                ushort modelId = row[1].Parse<ushort>();
                ushort soundId = row[2].Parse<ushort>();
                sbyte sizeClass = row[3].Parse<sbyte>();
                float creatureModelScale = row[4].Parse<float>();
                byte creatureModelAlpha = row[5].Parse<byte>();
                byte bloodId = row[6].Parse<byte>();
                int extendedDisplayInfoId = row[7].Parse<int>();
                ushort nPCSoundId = row[8].Parse<ushort>();
                ushort particleColorId = row[9].Parse<ushort>();
                int portraitCreatureDisplayInfoId = row[10].Parse<int>();
                int portraitTextureFileDataId = row[11].Parse<int>();
                ushort objectEffectPackageId = row[12].Parse<ushort>();
                ushort animReplacementSetId = row[13].Parse<ushort>();
                byte flags = row[14].Parse<byte>();
                int stateSpellVisualKitId = row[15].Parse<int>();
                float playerOverrideScale = row[16].Parse<float>();
                float petInstanceScale = row[17].Parse<float>();
                sbyte unarmedWeaponType = row[18].Parse<sbyte>();
                int mountPoofSpellVisualKitId = row[19].Parse<int>();
                int dissolveEffectId = row[20].Parse<int>();
                sbyte gender = row[21].Parse<sbyte>();
                int dissolveOutEffectId = row[22].Parse<int>();
                sbyte creatureModelMinLod = row[23].Parse<sbyte>();
                int textureVariationFileDataId1 = row[24].Parse<int>();
                int textureVariationFileDataId2 = row[25].Parse<int>();
                int textureVariationFileDataId3 = row[26].Parse<int>();

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.CreatureDisplayInfo;
                record.HotfixId = HotfixCreatureDisplayInfoBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt32(id);
                record.HotfixContent.WriteUInt16(modelId);
                record.HotfixContent.WriteUInt16(soundId);
                record.HotfixContent.WriteInt8(sizeClass);
                record.HotfixContent.WriteFloat(creatureModelScale);
                record.HotfixContent.WriteUInt8(creatureModelAlpha);
                record.HotfixContent.WriteUInt8(bloodId);
                record.HotfixContent.WriteInt32(extendedDisplayInfoId);
                record.HotfixContent.WriteUInt16(nPCSoundId);
                record.HotfixContent.WriteUInt16(particleColorId);
                record.HotfixContent.WriteInt32(portraitCreatureDisplayInfoId);
                record.HotfixContent.WriteInt32(portraitTextureFileDataId);
                record.HotfixContent.WriteUInt16(objectEffectPackageId);
                record.HotfixContent.WriteUInt16(animReplacementSetId);
                record.HotfixContent.WriteUInt8(flags);
                record.HotfixContent.WriteInt32(stateSpellVisualKitId);
                record.HotfixContent.WriteFloat(playerOverrideScale);
                record.HotfixContent.WriteFloat(petInstanceScale);
                record.HotfixContent.WriteInt8(unarmedWeaponType);
                record.HotfixContent.WriteInt32(mountPoofSpellVisualKitId);
                record.HotfixContent.WriteInt32(dissolveEffectId);
                record.HotfixContent.WriteInt8(gender);
                record.HotfixContent.WriteInt32(dissolveOutEffectId);
                record.HotfixContent.WriteInt8(creatureModelMinLod);
                record.HotfixContent.WriteInt32(textureVariationFileDataId1);
                record.HotfixContent.WriteInt32(textureVariationFileDataId2);
                record.HotfixContent.WriteInt32(textureVariationFileDataId3);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadCreatureDisplayInfoExtraHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoExtra{ModernVersion.ExpansionVersion}.csv");

            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = row[0].Parse<uint>();
                sbyte displayRaceId = row[1].Parse<sbyte>();
                sbyte displaySexId = row[2].Parse<sbyte>();
                sbyte displayClassId = row[3].Parse<sbyte>();
                sbyte skinId = row[4].Parse<sbyte>();
                sbyte faceId = row[5].Parse<sbyte>();
                sbyte hairStyleId = row[6].Parse<sbyte>();
                sbyte hairColorId = row[7].Parse<sbyte>();
                sbyte facialHairId = row[8].Parse<sbyte>();
                sbyte flags = row[9].Parse<sbyte>();
                int bakeMaterialResourcesId = row[10].Parse<int>();
                int hDBakeMaterialResourcesId = row[11].Parse<int>();
                byte customDisplayOption1 = row[12].Parse<byte>();
                byte customDisplayOption2 = row[13].Parse<byte>();
                byte customDisplayOption3 = row[14].Parse<byte>();

                HotfixRecord record = new HotfixRecord();
                record.TableHash = DB2Hash.CreatureDisplayInfoExtra;
                record.HotfixId = HotfixCreatureDisplayInfoExtraBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.Status = HotfixStatus.Valid;
                record.HotfixContent.WriteUInt32(id);
                record.HotfixContent.WriteInt8(displayRaceId);
                record.HotfixContent.WriteInt8(displaySexId);
                record.HotfixContent.WriteInt8(displayClassId);
                record.HotfixContent.WriteInt8(skinId);
                record.HotfixContent.WriteInt8(faceId);
                record.HotfixContent.WriteInt8(hairStyleId);
                record.HotfixContent.WriteInt8(hairColorId);
                record.HotfixContent.WriteInt8(facialHairId);
                record.HotfixContent.WriteInt8(flags);
                record.HotfixContent.WriteInt32(bakeMaterialResourcesId);
                record.HotfixContent.WriteInt32(hDBakeMaterialResourcesId);
                record.HotfixContent.WriteUInt8(customDisplayOption1);
                record.HotfixContent.WriteUInt8(customDisplayOption2);
                record.HotfixContent.WriteUInt8(customDisplayOption3);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadCreatureDisplayInfoOptionHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoOption{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = row[0].Parse<uint>();
                int chrCustomizationOptionId = row[1].Parse<int>();
                int chrCustomizationChoiceId = row[2].Parse<int>();
                int creatureDisplayInfoExtraId = row[3].Parse<int>();

                HotfixRecord record = new HotfixRecord();
                record.Status = HotfixStatus.Valid;
                record.TableHash = DB2Hash.CreatureDisplayInfoOption;
                record.HotfixId = HotfixCreatureDisplayInfoOptionBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.HotfixContent.WriteInt32(chrCustomizationOptionId);
                record.HotfixContent.WriteInt32(chrCustomizationChoiceId);
                record.HotfixContent.WriteInt32(creatureDisplayInfoExtraId);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        public static void LoadItemEffectHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemEffect{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = row[0].Parse<uint>();
                byte legacySlotIndex = row[1].Parse<byte>();
                byte triggerType = row[2].Parse<byte>();
                short charges = row[3].Parse<short>();
                int coolDownMSec = row[4].Parse<int>();
                int categoryCoolDownMSec = row[5].Parse<int>();
                short spellCategoryId = row[6].Parse<short>();
                int spellId = row[7].Parse<int>();
                short chrSpecializationId = row[8].Parse<short>();
                int parentItemId = row[9].Parse<int>();

                HotfixRecord record = new HotfixRecord();
                record.Status = HotfixStatus.Valid;
                record.TableHash = DB2Hash.ItemEffect;
                record.HotfixId = HotfixItemEffectBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.HotfixContent.WriteUInt8(legacySlotIndex);
                record.HotfixContent.WriteUInt8(triggerType);
                record.HotfixContent.WriteInt16(charges);
                record.HotfixContent.WriteInt32(coolDownMSec);
                record.HotfixContent.WriteInt32(categoryCoolDownMSec);
                record.HotfixContent.WriteInt16(spellCategoryId);
                record.HotfixContent.WriteInt32(spellId);
                record.HotfixContent.WriteInt16(chrSpecializationId);
                record.HotfixContent.WriteInt32(parentItemId);
                Hotfixes.Add(record.HotfixId, record);
            }
        }

        public static void LoadItemDisplayInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemDisplayInfo{ModernVersion.ExpansionVersion}.csv");
            using var reader = Sep.Reader(o => o with { HasHeader = true }).FromText(File.ReadAllText(path));
            uint counter = 0;
            foreach (var row in reader)
            {
                counter++;

                uint id = row[0].Parse<uint>();
                int itemVisual = row[1].Parse<int>();
                int particleColorID = row[2].Parse<int>();
                uint itemRangedDisplayInfoID = row[3].Parse<uint>();
                uint overrideSwooshSoundKitID = row[4].Parse<uint>();
                int sheatheTransformMatrixID = row[5].Parse<int>();
                int stateSpellVisualKitID = row[6].Parse<int>();
                int sheathedSpellVisualKitID = row[7].Parse<int>();
                uint unsheathedSpellVisualKitID = row[8].Parse<uint>();
                int flags = row[9].Parse<int>();
                uint modelResourcesID1 = row[10].Parse<uint>();
                uint modelResourcesID2 = row[11].Parse<uint>();
                int modelMaterialResourcesID1 = row[12].Parse<int>();
                int modelMaterialResourcesID2 = row[13].Parse<int>();
                int modelType1 = row[14].Parse<int>();
                int modelType2 = row[15].Parse<int>();
                int geosetGroup1 = row[16].Parse<int>();
                int geosetGroup2 = row[17].Parse<int>();
                int geosetGroup3 = row[18].Parse<int>();
                int geosetGroup4 = row[19].Parse<int>();
                int geosetGroup5 = row[20].Parse<int>();
                int geosetGroup6 = row[21].Parse<int>();
                int attachmentGeosetGroup1 = row[22].Parse<int>();
                int attachmentGeosetGroup2 = row[23].Parse<int>();
                int attachmentGeosetGroup3 = row[24].Parse<int>();
                int attachmentGeosetGroup4 = row[25].Parse<int>();
                int attachmentGeosetGroup5 = row[26].Parse<int>();
                int attachmentGeosetGroup6 = row[27].Parse<int>();
                int helmetGeosetVis1 = row[28].Parse<int>();
                int helmetGeosetVis2 = row[29].Parse<int>();

                HotfixRecord record = new HotfixRecord();
                record.Status = HotfixStatus.Valid;
                record.TableHash = DB2Hash.ItemDisplayInfo;
                record.HotfixId = HotfixItemDisplayInfoBegin + counter;
                record.UniqueId = record.HotfixId;
                record.RecordId = id;
                record.HotfixContent.WriteInt32(itemVisual);
                record.HotfixContent.WriteInt32(particleColorID);
                record.HotfixContent.WriteUInt32(itemRangedDisplayInfoID);
                record.HotfixContent.WriteUInt32(overrideSwooshSoundKitID);
                record.HotfixContent.WriteInt32(sheatheTransformMatrixID);
                record.HotfixContent.WriteInt32(stateSpellVisualKitID);
                record.HotfixContent.WriteInt32(sheathedSpellVisualKitID);
                record.HotfixContent.WriteUInt32(unsheathedSpellVisualKitID);
                record.HotfixContent.WriteInt32(flags);
                record.HotfixContent.WriteUInt32(modelResourcesID1);
                record.HotfixContent.WriteUInt32(modelResourcesID2);
                record.HotfixContent.WriteInt32(modelMaterialResourcesID1);
                record.HotfixContent.WriteInt32(modelMaterialResourcesID2);
                record.HotfixContent.WriteInt32(modelType1);
                record.HotfixContent.WriteInt32(modelType2);
                record.HotfixContent.WriteInt32(geosetGroup1);
                record.HotfixContent.WriteInt32(geosetGroup2);
                record.HotfixContent.WriteInt32(geosetGroup3);
                record.HotfixContent.WriteInt32(geosetGroup4);
                record.HotfixContent.WriteInt32(geosetGroup5);
                record.HotfixContent.WriteInt32(geosetGroup6);
                record.HotfixContent.WriteInt32(attachmentGeosetGroup1);
                record.HotfixContent.WriteInt32(attachmentGeosetGroup2);
                record.HotfixContent.WriteInt32(attachmentGeosetGroup3);
                record.HotfixContent.WriteInt32(attachmentGeosetGroup4);
                record.HotfixContent.WriteInt32(attachmentGeosetGroup5);
                record.HotfixContent.WriteInt32(attachmentGeosetGroup6);
                record.HotfixContent.WriteInt32(helmetGeosetVis1);
                record.HotfixContent.WriteInt32(helmetGeosetVis2);
                Hotfixes.Add(record.HotfixId, record);
            }
        }
        #endregion


        // Data structures
        public sealed class BroadcastText
        {
            public uint Entry;
            public string MaleText;
            public string FemaleText;
            public uint Language;
            public ushort[] Emotes = new ushort[3];
            public ushort[] EmoteDelays = new ushort[3];
        }

        public sealed class ItemRecord
        {
            public int Id;
            public byte ClassId;
            public byte SubclassId;
            public byte Material;
            public sbyte InventoryType;
            public int RequiredLevel;
            public byte SheatheType;
            public ushort RandomProperty;
            public ushort ItemRandomSuffixGroupId;
            public sbyte SoundOverrideSubclassId;
            public ushort ScalingStatDistributionId;
            public int IconFileDataId;
            public byte ItemGroupSoundsId;
            public int ContentTuningId;
            public uint MaxDurability;
            public byte AmmoType;
            public byte[] DamageType = new byte[5];
            public short[] Resistances = new short[7];
            public ushort[] MinDamage = new ushort[5];
            public ushort[] MaxDamage = new ushort[5];
        }

        public sealed class ItemSparseRecord
        {
            public int Id;
            public long AllowableRace;
            public string Description;
            public string Name4;
            public string Name3;
            public string Name2;
            public string Name1;
            public float DmgVariance = 1;
            public uint DurationInInventory;
            public float QualityModifier;
            public uint BagFamily;
            public float RangeMod;
            public float[] StatPercentageOfSocket = new float[10];
            public int[] StatPercentEditor = new int[10];
            public int Stackable;
            public int MaxCount;
            public uint RequiredAbility;
            public uint SellPrice;
            public uint BuyPrice;
            public uint VendorStackCount = 1;
            public float PriceVariance = 1;
            public float PriceRandomValue = 1;
            public uint[] Flags = new uint[4];
            public int OppositeFactionItemId;
            public uint MaxDurability;
            public ushort ItemNameDescriptionId;
            public ushort RequiredTransmogHoliday;
            public ushort RequiredHoliday;
            public ushort LimitCategory;
            public ushort GemProperties;
            public ushort SocketMatchEnchantmentId;
            public ushort TotemCategoryId;
            public ushort InstanceBound;
            public ushort[] ZoneBound = new ushort[2];
            public ushort ItemSet;
            public ushort LockId;
            public ushort StartQuestId;
            public ushort PageText;
            public ushort Delay;
            public ushort RequiredReputationId;
            public ushort RequiredSkillRank;
            public ushort RequiredSkill;
            public ushort ItemLevel;
            public short AllowableClass;
            public ushort ItemRandomSuffixGroupId;
            public ushort RandomProperty;
            public ushort[] MinDamage = new ushort[5];
            public ushort[] MaxDamage = new ushort[5];
            public short[] Resistances = new short[7];
            public ushort ScalingStatDistributionId;
            public byte ExpansionId = 254;
            public byte ArtifactId;
            public byte SpellWeight;
            public byte SpellWeightCategory;
            public byte[] SocketType = new byte[3];
            public byte SheatheType;
            public byte Material;
            public byte PageMaterial;
            public byte PageLanguage;
            public byte Bonding;
            public byte DamageType;
            public sbyte[] StatType = new sbyte[10];
            public byte ContainerSlots;
            public byte RequiredReputationRank;
            public byte RequiredCityRank;
            public byte RequiredHonorRank;
            public byte InventoryType;
            public byte OverallQualityId;
            public byte AmmoType;
            public sbyte[] StatValue = new sbyte[10];
            public sbyte RequiredLevel;
        }

        public sealed class ItemAppearance
        {
            public int Id;
            public byte DisplayType;
            public int ItemDisplayInfoID;
            public int DefaultIconFileDataID;
            public int UiOrder;
        }
        public sealed class ItemModifiedAppearance
        {
            public int Id;
            public int ItemID;
            public int ItemAppearanceModifierID;
            public int ItemAppearanceID;
            public int OrderIndex;
            public int TransmogSourceTypeEnum;
        }
        public sealed class ItemEffect
        {
            public int Id;
            public byte LegacySlotIndex;
            public sbyte TriggerType;
            public short Charges;
            public int CoolDownMSec;
            public int CategoryCoolDownMSec;
            public ushort SpellCategoryID;
            public int SpellID;
            public ushort ChrSpecializationID;
            public int ParentItemID;
        }
        public sealed class ItemSpellsData
        {
            public int Id;
            public int Category;
            public int RecoveryTime;
            public int CategoryRecoveryTime;
        }
        public sealed class ItemDisplayData
        {
            public int Id;
            public int IconFileDataId;
            public int ModelResourcesId_1;
            public int ModelResourcesId_2;
            public int ModelMaterialResourcesId_1;
            public int ModelMaterialResourcesId_2;
        }
        public sealed class Battleground
        {
            public bool IsArena;
            public List<uint> MapIds = new List<uint>();
        }
        public sealed class TaxiPath
        {
            public uint Id;
            public uint From;
            public uint To;
            public int Cost;
        }
        public sealed class TaxiNode
        {
            public uint Id;
            public uint mapId;
            public float x, y, z;
        }
        public sealed class TaxiPathNode
        {
            public uint Id;
            public uint pathId;
            public uint nodeIndex;
            public uint mapId;
            public float x, y, z;
            public uint flags;
            public uint delay;
        }
        public sealed class ChatChannel
        {
            public uint Id;
            public ChannelFlags Flags;
            public string Name;
        }

        public record CreatureDisplayInfo(uint ModelId, float DisplayScale);
        public record CreatureModelCollisionHeight(float ModelScale, float Height, float MountHeight);

        // Hotfix structures
        public sealed class AreaTrigger
        {
            public string Message;
            public float PositionX;
            public float PositionY;
            public float PositionZ;
            public uint Id;
            public ushort MapId;
            public byte PhaseUseFlags;
            public ushort PhaseId;
            public ushort PhaseGroupId;
            public float Radius;
            public float BoxLength;
            public float BoxWidth;
            public float BoxHeight;
            public float BoxYaw;
            public byte ShapeType;
            public ushort ShapeId;
            public ushort ActionSetId;
            public byte Flags;
        }
    }
}