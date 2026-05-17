using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V3_4_3_54261;

// Phase 5b Player section — descriptor-driven WriteCreatePlayerData /
// WriteUpdatePlayerData / HasAnyPlayerFieldSet emit. 4-block changesMask (~81 bits,
// MaskWidth=4). Update path emits the `IsQuestLogChangesMaskSkipped = true` literal
// bit unconditionally between blocks-mask + FlushBits — modeled via
// DescriptorUpdateBitsPreamble.
//
// Previous-life note: file held legacy descriptor-tree slot-index enum
// (PLAYER_DUEL_ARBITER = 218, etc.). Not referenced from V3_4_3_54261 source.
[DescriptorSection(DataType = typeof(PlayerData), MaskMode = MaskMode.Blocks, MaskWidth = 4)]
public enum PlayerField
{
    // ============================================================
    // Create-path emit order — enum declaration order.
    // Update-path emit order — bit-ascending (generator sorts).
    // ============================================================

    [DescriptorCreateField(nameof(PlayerData.DuelArbiter), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(PlayerData.DuelArbiter), DescriptorType.PackedGuid128, bit: 4, ParentBit = 0)]
    PLAYER_DUEL_ARBITER,

    [DescriptorCreateField(nameof(PlayerData.WowAccount), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(PlayerData.WowAccount), DescriptorType.PackedGuid128, bit: 5, ParentBit = 0)]
    PLAYER_WOW_ACCOUNT,

    [DescriptorCreateField(nameof(PlayerData.LootTargetGUID), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(PlayerData.LootTargetGUID), DescriptorType.PackedGuid128, bit: 6, ParentBit = 0)]
    PLAYER_LOOT_TARGET_GUID,

    [DescriptorCreateField(nameof(PlayerData.PlayerFlags), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(PlayerData.PlayerFlags), DescriptorType.UInt32, bit: 7, ParentBit = 0)]
    PLAYER_FLAGS,

    [DescriptorCreateField(nameof(PlayerData.PlayerFlagsEx), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(PlayerData.PlayerFlagsEx), DescriptorType.UInt32, bit: 8, ParentBit = 0)]
    PLAYER_FLAGS_EX,

    [DescriptorCreateField(nameof(PlayerData.GuildRankID), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(PlayerData.GuildRankID), DescriptorType.UInt32, bit: 9, ParentBit = 0)]
    PLAYER_GUILD_RANK_ID,

    [DescriptorCreateField(nameof(PlayerData.GuildDeleteDate), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(PlayerData.GuildDeleteDate), DescriptorType.UInt32, bit: 10, ParentBit = 0)]
    PLAYER_GUILD_DELETE_DATE,

    [DescriptorCreateField(nameof(PlayerData.GuildLevel), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(PlayerData.GuildLevel), DescriptorType.Int32, bit: 11, ParentBit = 0)]
    PLAYER_GUILD_LEVEL,

    // Customizations count emit (Create-only) — counts non-null entries then writes UInt32.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreatePlayerCustomizationsCount))]
    PLAYER_CUSTOMIZATION_COUNT_CUSTOM,

    [DescriptorCreateField(nameof(PlayerData.PartyType), DescriptorType.UInt8)]
    PLAYER_PARTY_TYPE,

    [DescriptorCreatePlaceholder(DescriptorType.UInt8)]
    PLAYER_PAD_BYTE_1,

    [DescriptorCreateField(nameof(PlayerData.NumBankSlots), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(PlayerData.NumBankSlots), DescriptorType.UInt8, bit: 12, ParentBit = 0)]
    PLAYER_NUM_BANK_SLOTS,

    [DescriptorCreateField(nameof(PlayerData.NativeSex), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(PlayerData.NativeSex), DescriptorType.UInt8, bit: 13, ParentBit = 0)]
    PLAYER_NATIVE_SEX,

    [DescriptorCreateField(nameof(PlayerData.Inebriation), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(PlayerData.Inebriation), DescriptorType.UInt8, bit: 14, ParentBit = 0)]
    PLAYER_INEBRIATION,

    [DescriptorCreateField(nameof(PlayerData.PvpTitle), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(PlayerData.PvpTitle), DescriptorType.UInt8, bit: 15, ParentBit = 0)]
    PLAYER_PVP_TITLE,

    [DescriptorCreateField(nameof(PlayerData.ArenaFaction), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(PlayerData.ArenaFaction), DescriptorType.UInt8, bit: 16, ParentBit = 0)]
    PLAYER_ARENA_FACTION,

    [DescriptorCreateField(nameof(PlayerData.PvPRank), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(PlayerData.PvPRank), DescriptorType.UInt8, bit: 17, ParentBit = 0)]
    PLAYER_PVP_RANK,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    PLAYER_PAD_INT32_1,

    [DescriptorCreateField(nameof(PlayerData.DuelTeam), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(PlayerData.DuelTeam), DescriptorType.UInt32, bit: 19, ParentBit = 0)]
    PLAYER_DUEL_TEAM,

    [DescriptorCreateField(nameof(PlayerData.GuildTimeStamp), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(PlayerData.GuildTimeStamp), DescriptorType.Int32, bit: 20, ParentBit = 0)]
    PLAYER_GUILD_TIMESTAMP,

    // QuestLog Create — owner-gated 25× quest entries (custom writer drops the hand-port trace log).
    [DescriptorCreatePlaceholder(DescriptorType.Int64, OwnerOnly = true, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreatePlayerQuestLog))]
    PLAYER_QUEST_LOG_CUSTOM,

    // VisibleItems Create — 19× always-write (zero fallback for null entries).
    [DescriptorCreatePlaceholder(DescriptorType.Int32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreatePlayerVisibleItems))]
    PLAYER_VISIBLE_ITEMS_CREATE_CUSTOM,

    [DescriptorCreateField(nameof(PlayerData.ChosenTitle), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(PlayerData.ChosenTitle), DescriptorType.Int32, bit: 21, ParentBit = 0)]
    PLAYER_CHOSEN_TITLE,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    PLAYER_PAD_INT32_2,

    [DescriptorCreateField(nameof(PlayerData.VirtualPlayerRealm), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(PlayerData.VirtualPlayerRealm), DescriptorType.UInt32, bit: 23, ParentBit = 0)]
    PLAYER_VIRTUAL_PLAYER_REALM,

    [DescriptorCreateField(nameof(PlayerData.CurrentSpecID), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(PlayerData.CurrentSpecID), DescriptorType.UInt32, bit: 24, ParentBit = 0)]
    PLAYER_CURRENT_SPEC_ID,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    PLAYER_PAD_INT32_3,

    // AvgItemLevel[6] zero placeholders (Create-only — 6× Float 0).
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    PLAYER_PAD_AVG_ITEM_LEVEL_0,
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    PLAYER_PAD_AVG_ITEM_LEVEL_1,
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    PLAYER_PAD_AVG_ITEM_LEVEL_2,
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    PLAYER_PAD_AVG_ITEM_LEVEL_3,
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    PLAYER_PAD_AVG_ITEM_LEVEL_4,
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    PLAYER_PAD_AVG_ITEM_LEVEL_5,

    [DescriptorCreatePlaceholder(DescriptorType.UInt8)]
    PLAYER_PAD_BYTE_2,

    [DescriptorCreateField(nameof(PlayerData.HonorLevel), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(PlayerData.HonorLevel), DescriptorType.Int32, bit: 27, ParentBit = 0)]
    PLAYER_HONOR_LEVEL,

    // LogoutTime — owner-only Int64 UnixTime, else 0L.
    [DescriptorCreatePlaceholder(DescriptorType.Int64, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreatePlayerLogoutTime))]
    PLAYER_LOGOUT_TIME_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    PLAYER_PAD_UINT32_1,
    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    PLAYER_PAD_INT32_4,

    // BnetAccount — owner-only PackedGuid128 via _gameState.GlobalSession lookup.
    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreatePlayerBnetAccount))]
    PLAYER_BNET_ACCOUNT_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    PLAYER_PAD_UINT32_2,

    // 19× UInt32 0 placeholders.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_0,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_1,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_2,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_3,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_4,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_5,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_6,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_7,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_8,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_9,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_10,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_11,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_12,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_13,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_14,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_15,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_16,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_17,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)] PLAYER_PAD_UINT32_PAD_18,

    // Customizations data — variable-length 2× UInt32 per non-null entry.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreatePlayerCustomizationsData))]
    PLAYER_CUSTOMIZATIONS_DATA_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    PLAYER_PAD_FLOAT_1,
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    PLAYER_PAD_FLOAT_2,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    PLAYER_PAD_UINT32_TRAILING,

    // ============================================================
    // Update-only fields not in Create / Update-side bits + arrays.
    // ============================================================

    [DescriptorUpdateField(nameof(PlayerData.FakeInebriation), DescriptorType.Int32, bit: 22, ParentBit = 0)]
    PLAYER_FAKE_INEBRIATION_UPDATE,

    // QuestLog Update — PerElement bit 36-60, parent 35. Per-element CustomWriter writes
    // Int64 EndTime + Int32 QuestID + UInt32 StateFlags + 24× UInt16 ObjectiveProgress
    // (matches hand-port's WriteCreate-format-for-quest-entries documented at file:1576-1583).
    [DescriptorUpdateField(nameof(PlayerData.QuestLog), DescriptorType.Int32, bit: 36,
                           ArrayCount = 25, ArrayMode = ArrayMode.PerElement, ParentBit = 35,
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdatePlayerQuestLogEntry),
                           CustomPredicate = "src.QuestLog[{i}] != null && src.QuestLog[{i}]!.QuestID.HasValue")]
    PLAYER_QUEST_LOG_UPDATE,

    // VisibleItems Update — PerElement bit 62-80, parent 61. Per-element CustomWriter writes
    // inner 4-bit mask (0x0F) + FlushBits + 3 fields.
    [DescriptorUpdateField(nameof(PlayerData.VisibleItems), DescriptorType.Int32, bit: 62,
                           ArrayCount = 19, ArrayMode = ArrayMode.PerElement, ParentBit = 61,
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdatePlayerVisibleItem),
                           CustomPredicate = "src.VisibleItems != null && src.VisibleItems[{i}].HasValue")]
    PLAYER_VISIBLE_ITEMS_UPDATE,

    // IsQuestLogChangesMaskSkipped = true: 1-bit literal between blocks-mask + FlushBits.
    [DescriptorUpdateBitsPreamble(1u, 1)]
    PLAYER_IS_QUEST_LOG_CHANGES_MASK_SKIPPED,
}
