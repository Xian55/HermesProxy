using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V3_4_3_54261;

// Phase 5b ActivePlayer section — descriptor-driven WriteCreateActivePlayerData /
// WriteUpdateActivePlayerData / HasAnyActivePlayerFieldSet emit.
//
// Shape: 48-block changesMask (1536 bits) with split-mask prefix (UInt32 + WriteBits16).
// 4 group-bit headers (0/38/70/102) cover scalar blocks; shared-parent arrays at
// bits 269 (4 sub-arrays), 542 (2), 549 (2), 1512 (2). KnownTitles is a
// DynamicUpdateField (bit 3) with custom preamble + body writers. Skill (bit 32) is
// a nested SkillInfo write via existing WriteUpdateSkillInfo helper. PvpInfo &
// RestInfo are PerElement arrays with their own inner masks. InvSlots[141] is
// driven by a MaskMutator (uses GetModernInvSlot 141→legacy-slots mapping).
// GlyphSlots+Glyphs share parent bit 1512 with a MaskMutator that consumes
// _gameState.ActiveGlyphsDirty exactly once per Values update.
//
// Create path is consolidated into one custom writer (WriteCreateActivePlayerAll)
// because the byte-stream is mostly zero placeholders interleaved with a few real
// fields — declarative per-write enum members would balloon to ~200 entries with
// no readability win.
//
// Previous-life note: file previously held legacy descriptor-tree slot-index enum
// (ACTIVE_PLAYER_FIELD_INV_SLOT_HEAD = 760, etc.). Not referenced from V3_4_3_54261
// source.
[DescriptorSection(
    DataType = typeof(ActivePlayerData),
    MaskMode = MaskMode.Blocks,
    MaskWidth = 0,                                  // Ignored under UInt32PlusBits16
    BlockMaskShape = BlockMaskShape.UInt32PlusBits16)]
public enum ActivePlayerField
{
    // ===========================================================================
    // Create — one big custom writer. Hand-port body lives there to retain the
    // ~180 LOC of zero-placeholder interleave readable in one place.
    // ===========================================================================
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerAll))]
    ACTIVEPLAYER_CREATE_ALL_CUSTOM,

    // ===========================================================================
    // Update — mask mutators (run before all scalar bit-setting)
    // ===========================================================================

    // InvSlots: 141 modern slots fanned across legacy InvSlots/PackSlots/BankSlots/
    // BankBagSlots/BuyBackSlots/KeyringSlots via GetModernInvSlot. Sets bit 124 + 125+i
    // for each non-null mapped slot. Writes happen in matching CustomField below.
    // HasAnyPredicate: any non-null mapped slot → Values update must fire (loot/bag
    // pickup case: without this, generator skipped the update and items only appeared
    // after relog).
    [DescriptorMaskMutator(
        nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.ApplyActivePlayerInvSlotsMaskMutator),
        HasAnyPredicate = "HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.HasAnyInvSlotMapped(src)")]
    ACTIVEPLAYER_INVSLOTS_MASK_MUTATOR,

    // GlyphsDirty: captures + clears _gameState.ActiveGlyphsDirty. When true, sets
    // 1512 + 1513-1518 + 1519-1524 in one shot. Body writes in matching CustomField.
    // HasAnyPredicate: dirty flag must trigger Values update (glyph/spec swap case).
    [DescriptorMaskMutator(
        nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.ApplyActivePlayerGlyphsMaskMutator),
        HasAnyPredicate = "_gameState.ActiveGlyphsDirty")]
    ACTIVEPLAYER_GLYPHS_MASK_MUTATOR,

    // ===========================================================================
    // Update — KnownTitles dynamic field (bit 3)
    // ===========================================================================

    // Preamble: count (uint32) + count× WriteBit(true). Emitted between blocks-mask
    // and FlushBits. Gated on bit 3.
    [DescriptorMaskPreamble(bit: 3,
        customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerKnownTitlesPreamble))]
    ACTIVEPLAYER_KNOWN_TITLES_PREAMBLE,

    // Body scalar field at bit 3 (parent 0). Predicate = "any KnownTitles[i] non-null".
    // CustomWriter writes the folded ulong[6] array.
    [DescriptorUpdateField(nameof(ActivePlayerData.KnownTitles), DescriptorType.UInt64, bit: 3, ParentBit = 0,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerKnownTitlesBody),
        CustomPredicate = "HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.HasAnyKnownTitle(src.KnownTitles)")]
    ACTIVEPLAYER_KNOWN_TITLES,

    // ===========================================================================
    // Update — Block 0 scalars (group bit 0, bits 26-37)
    // ===========================================================================

    [DescriptorUpdateField(nameof(ActivePlayerData.FarsightObject), DescriptorType.PackedGuid128, bit: 26, ParentBit = 0)]
    ACTIVEPLAYER_FARSIGHT_OBJECT,
    // bit 27: SummonedBattlePetGUID — not used
    [DescriptorUpdateField(nameof(ActivePlayerData.Coinage), DescriptorType.UInt64, bit: 28, ParentBit = 0)]
    ACTIVEPLAYER_COINAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.XP), DescriptorType.Int32, bit: 29, ParentBit = 0)]
    ACTIVEPLAYER_XP,
    [DescriptorUpdateField(nameof(ActivePlayerData.NextLevelXP), DescriptorType.Int32, bit: 30, ParentBit = 0)]
    ACTIVEPLAYER_NEXT_LEVEL_XP,
    [DescriptorUpdateField(nameof(ActivePlayerData.TrialXP), DescriptorType.Int32, bit: 31, ParentBit = 0)]
    ACTIVEPLAYER_TRIAL_XP,
    // Skill (bit 32) — nested SkillInfo write via WriteUpdateSkillInfo helper.
    [DescriptorUpdateField(nameof(ActivePlayerData.Skill), DescriptorType.Int32, bit: 32, ParentBit = 0,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerSkill),
        CustomPredicate = "src.Skill != null && HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.HasAnySkillChangedStatic(src.Skill)")]
    ACTIVEPLAYER_SKILL,
    [DescriptorUpdateField(nameof(ActivePlayerData.CharacterPoints), DescriptorType.Int32, bit: 33, ParentBit = 0)]
    ACTIVEPLAYER_CHARACTER_POINTS,
    [DescriptorUpdateField(nameof(ActivePlayerData.MaxTalentTiers), DescriptorType.Int32, bit: 34, ParentBit = 0)]
    ACTIVEPLAYER_MAX_TALENT_TIERS,
    [DescriptorUpdateField(nameof(ActivePlayerData.TrackCreatureMask), DescriptorType.UInt32, bit: 35, ParentBit = 0)]
    ACTIVEPLAYER_TRACK_CREATURE_MASK,
    [DescriptorUpdateField(nameof(ActivePlayerData.MainhandExpertise), DescriptorType.Float, bit: 36, ParentBit = 0)]
    ACTIVEPLAYER_MAINHAND_EXPERTISE,
    [DescriptorUpdateField(nameof(ActivePlayerData.OffhandExpertise), DescriptorType.Float, bit: 37, ParentBit = 0)]
    ACTIVEPLAYER_OFFHAND_EXPERTISE,

    // ===========================================================================
    // Update — Block 38 scalars (group bit 38, bits 39-69)
    // ===========================================================================

    [DescriptorUpdateField(nameof(ActivePlayerData.RangedExpertise), DescriptorType.Float, bit: 39, ParentBit = 38)]
    ACTIVEPLAYER_RANGED_EXPERTISE,
    [DescriptorUpdateField(nameof(ActivePlayerData.CombatRatingExpertise), DescriptorType.Float, bit: 40, ParentBit = 38)]
    ACTIVEPLAYER_COMBAT_RATING_EXPERTISE,
    [DescriptorUpdateField(nameof(ActivePlayerData.BlockPercentage), DescriptorType.Float, bit: 41, ParentBit = 38)]
    ACTIVEPLAYER_BLOCK_PERCENTAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.DodgePercentage), DescriptorType.Float, bit: 42, ParentBit = 38)]
    ACTIVEPLAYER_DODGE_PERCENTAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.DodgePercentageFromAttribute), DescriptorType.Float, bit: 43, ParentBit = 38)]
    ACTIVEPLAYER_DODGE_PERCENTAGE_FROM_ATTRIBUTE,
    [DescriptorUpdateField(nameof(ActivePlayerData.ParryPercentage), DescriptorType.Float, bit: 44, ParentBit = 38)]
    ACTIVEPLAYER_PARRY_PERCENTAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.ParryPercentageFromAttribute), DescriptorType.Float, bit: 45, ParentBit = 38)]
    ACTIVEPLAYER_PARRY_PERCENTAGE_FROM_ATTRIBUTE,
    [DescriptorUpdateField(nameof(ActivePlayerData.CritPercentage), DescriptorType.Float, bit: 46, ParentBit = 38)]
    ACTIVEPLAYER_CRIT_PERCENTAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.RangedCritPercentage), DescriptorType.Float, bit: 47, ParentBit = 38)]
    ACTIVEPLAYER_RANGED_CRIT_PERCENTAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.OffhandCritPercentage), DescriptorType.Float, bit: 48, ParentBit = 38)]
    ACTIVEPLAYER_OFFHAND_CRIT_PERCENTAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.ShieldBlock), DescriptorType.Int32, bit: 49, ParentBit = 38)]
    ACTIVEPLAYER_SHIELD_BLOCK,
    // bit 50: ShieldBlockCritPercentage — no property
    [DescriptorUpdateField(nameof(ActivePlayerData.Mastery), DescriptorType.Float, bit: 51, ParentBit = 38)]
    ACTIVEPLAYER_MASTERY,
    [DescriptorUpdateField(nameof(ActivePlayerData.Speed), DescriptorType.Float, bit: 52, ParentBit = 38)]
    ACTIVEPLAYER_SPEED,
    [DescriptorUpdateField(nameof(ActivePlayerData.Avoidance), DescriptorType.Float, bit: 53, ParentBit = 38)]
    ACTIVEPLAYER_AVOIDANCE,
    [DescriptorUpdateField(nameof(ActivePlayerData.Sturdiness), DescriptorType.Float, bit: 54, ParentBit = 38)]
    ACTIVEPLAYER_STURDINESS,
    [DescriptorUpdateField(nameof(ActivePlayerData.Versatility), DescriptorType.Int32, bit: 55, ParentBit = 38)]
    ACTIVEPLAYER_VERSATILITY,
    [DescriptorUpdateField(nameof(ActivePlayerData.VersatilityBonus), DescriptorType.Float, bit: 56, ParentBit = 38)]
    ACTIVEPLAYER_VERSATILITY_BONUS,
    [DescriptorUpdateField(nameof(ActivePlayerData.PvpPowerDamage), DescriptorType.Float, bit: 57, ParentBit = 38)]
    ACTIVEPLAYER_PVP_POWER_DAMAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.PvpPowerHealing), DescriptorType.Float, bit: 58, ParentBit = 38)]
    ACTIVEPLAYER_PVP_POWER_HEALING,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModHealingDonePos), DescriptorType.Int32, bit: 59, ParentBit = 38)]
    ACTIVEPLAYER_MOD_HEALING_DONE_POS,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModHealingPercent), DescriptorType.Float, bit: 60, ParentBit = 38)]
    ACTIVEPLAYER_MOD_HEALING_PERCENT,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModHealingDonePercent), DescriptorType.Float, bit: 61, ParentBit = 38)]
    ACTIVEPLAYER_MOD_HEALING_DONE_PERCENT,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModPeriodicHealingDonePercent), DescriptorType.Float, bit: 62, ParentBit = 38)]
    ACTIVEPLAYER_MOD_PERIODIC_HEALING_DONE_PERCENT,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModSpellPowerPercent), DescriptorType.Float, bit: 63, ParentBit = 38)]
    ACTIVEPLAYER_MOD_SPELL_POWER_PERCENT,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModResiliencePercent), DescriptorType.Float, bit: 64, ParentBit = 38)]
    ACTIVEPLAYER_MOD_RESILIENCE_PERCENT,
    [DescriptorUpdateField(nameof(ActivePlayerData.OverrideSpellPowerByAPPercent), DescriptorType.Float, bit: 65, ParentBit = 38)]
    ACTIVEPLAYER_OVERRIDE_SPELL_POWER_BY_AP_PERCENT,
    [DescriptorUpdateField(nameof(ActivePlayerData.OverrideAPBySpellPowerPercent), DescriptorType.Float, bit: 66, ParentBit = 38)]
    ACTIVEPLAYER_OVERRIDE_AP_BY_SPELL_POWER_PERCENT,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModTargetResistance), DescriptorType.Int32, bit: 67, ParentBit = 38)]
    ACTIVEPLAYER_MOD_TARGET_RESISTANCE,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModTargetPhysicalResistance), DescriptorType.Int32, bit: 68, ParentBit = 38)]
    ACTIVEPLAYER_MOD_TARGET_PHYSICAL_RESISTANCE,
    [DescriptorUpdateField(nameof(ActivePlayerData.LocalFlags), DescriptorType.UInt32, bit: 69, ParentBit = 38)]
    ACTIVEPLAYER_LOCAL_FLAGS,

    // ===========================================================================
    // Update — Block 70 scalars (group bit 70, bits 71-101)
    // ===========================================================================

    [DescriptorUpdateField(nameof(ActivePlayerData.GrantableLevels), DescriptorType.UInt8, bit: 71, ParentBit = 70)]
    ACTIVEPLAYER_GRANTABLE_LEVELS,
    [DescriptorUpdateField(nameof(ActivePlayerData.MultiActionBars), DescriptorType.UInt8, bit: 72, ParentBit = 70)]
    ACTIVEPLAYER_MULTI_ACTION_BARS,
    [DescriptorUpdateField(nameof(ActivePlayerData.LifetimeMaxRank), DescriptorType.UInt8, bit: 73, ParentBit = 70)]
    ACTIVEPLAYER_LIFETIME_MAX_RANK,
    [DescriptorUpdateField(nameof(ActivePlayerData.NumRespecs), DescriptorType.UInt8, bit: 74, ParentBit = 70)]
    ACTIVEPLAYER_NUM_RESPECS,
    // AmmoID is UInt32? on data, written as Int32 ((int) cast)
    [DescriptorUpdateField(nameof(ActivePlayerData.AmmoID), DescriptorType.Int32, bit: 75, ParentBit = 70, Cast = "(int)")]
    ACTIVEPLAYER_AMMO_ID,
    [DescriptorUpdateField(nameof(ActivePlayerData.PvpMedals), DescriptorType.UInt32, bit: 76, ParentBit = 70)]
    ACTIVEPLAYER_PVP_MEDALS,
    [DescriptorUpdateField(nameof(ActivePlayerData.TodayHonorableKills), DescriptorType.UInt16, bit: 77, ParentBit = 70)]
    ACTIVEPLAYER_TODAY_HONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.TodayDishonorableKills), DescriptorType.UInt16, bit: 78, ParentBit = 70)]
    ACTIVEPLAYER_TODAY_DISHONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.YesterdayHonorableKills), DescriptorType.UInt16, bit: 79, ParentBit = 70)]
    ACTIVEPLAYER_YESTERDAY_HONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.YesterdayDishonorableKills), DescriptorType.UInt16, bit: 80, ParentBit = 70)]
    ACTIVEPLAYER_YESTERDAY_DISHONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.LastWeekHonorableKills), DescriptorType.UInt16, bit: 81, ParentBit = 70)]
    ACTIVEPLAYER_LAST_WEEK_HONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.LastWeekDishonorableKills), DescriptorType.UInt16, bit: 82, ParentBit = 70)]
    ACTIVEPLAYER_LAST_WEEK_DISHONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.ThisWeekHonorableKills), DescriptorType.UInt16, bit: 83, ParentBit = 70)]
    ACTIVEPLAYER_THIS_WEEK_HONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.ThisWeekDishonorableKills), DescriptorType.UInt16, bit: 84, ParentBit = 70)]
    ACTIVEPLAYER_THIS_WEEK_DISHONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.ThisWeekContribution), DescriptorType.UInt32, bit: 85, ParentBit = 70)]
    ACTIVEPLAYER_THIS_WEEK_CONTRIBUTION,
    [DescriptorUpdateField(nameof(ActivePlayerData.LifetimeHonorableKills), DescriptorType.UInt32, bit: 86, ParentBit = 70)]
    ACTIVEPLAYER_LIFETIME_HONORABLE_KILLS,
    [DescriptorUpdateField(nameof(ActivePlayerData.LifetimeDishonorableKills), DescriptorType.UInt32, bit: 87, ParentBit = 70)]
    ACTIVEPLAYER_LIFETIME_DISHONORABLE_KILLS,
    // bit 88: Field_F24 — unused
    [DescriptorUpdateField(nameof(ActivePlayerData.YesterdayContribution), DescriptorType.UInt32, bit: 89, ParentBit = 70)]
    ACTIVEPLAYER_YESTERDAY_CONTRIBUTION,
    [DescriptorUpdateField(nameof(ActivePlayerData.LastWeekContribution), DescriptorType.UInt32, bit: 90, ParentBit = 70)]
    ACTIVEPLAYER_LAST_WEEK_CONTRIBUTION,
    [DescriptorUpdateField(nameof(ActivePlayerData.LastWeekRank), DescriptorType.UInt32, bit: 91, ParentBit = 70)]
    ACTIVEPLAYER_LAST_WEEK_RANK,
    [DescriptorUpdateField(nameof(ActivePlayerData.WatchedFactionIndex), DescriptorType.Int32, bit: 92, ParentBit = 70)]
    ACTIVEPLAYER_WATCHED_FACTION_INDEX,
    [DescriptorUpdateField(nameof(ActivePlayerData.MaxLevel), DescriptorType.Int32, bit: 93, ParentBit = 70)]
    ACTIVEPLAYER_MAX_LEVEL,
    [DescriptorUpdateField(nameof(ActivePlayerData.ScalingPlayerLevelDelta), DescriptorType.Int32, bit: 94, ParentBit = 70)]
    ACTIVEPLAYER_SCALING_PLAYER_LEVEL_DELTA,
    [DescriptorUpdateField(nameof(ActivePlayerData.MaxCreatureScalingLevel), DescriptorType.Int32, bit: 95, ParentBit = 70)]
    ACTIVEPLAYER_MAX_CREATURE_SCALING_LEVEL,
    [DescriptorUpdateField(nameof(ActivePlayerData.PetSpellPower), DescriptorType.Int32, bit: 96, ParentBit = 70)]
    ACTIVEPLAYER_PET_SPELL_POWER,
    [DescriptorUpdateField(nameof(ActivePlayerData.UiHitModifier), DescriptorType.Float, bit: 97, ParentBit = 70)]
    ACTIVEPLAYER_UI_HIT_MODIFIER,
    [DescriptorUpdateField(nameof(ActivePlayerData.UiSpellHitModifier), DescriptorType.Float, bit: 98, ParentBit = 70)]
    ACTIVEPLAYER_UI_SPELL_HIT_MODIFIER,
    [DescriptorUpdateField(nameof(ActivePlayerData.HomeRealmTimeOffset), DescriptorType.Int32, bit: 99, ParentBit = 70)]
    ACTIVEPLAYER_HOME_REALM_TIME_OFFSET,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModPetHaste), DescriptorType.Float, bit: 100, ParentBit = 70)]
    ACTIVEPLAYER_MOD_PET_HASTE,
    [DescriptorUpdateField(nameof(ActivePlayerData.LocalRegenFlags), DescriptorType.UInt8, bit: 101, ParentBit = 70)]
    ACTIVEPLAYER_LOCAL_REGEN_FLAGS,

    // ===========================================================================
    // Update — Block 102 scalars (group bit 102, bits 103-123)
    // ===========================================================================

    [DescriptorUpdateField(nameof(ActivePlayerData.AuraVision), DescriptorType.UInt8, bit: 103, ParentBit = 102)]
    ACTIVEPLAYER_AURA_VISION,
    [DescriptorUpdateField(nameof(ActivePlayerData.NumBackpackSlots), DescriptorType.UInt8, bit: 104, ParentBit = 102)]
    ACTIVEPLAYER_NUM_BACKPACK_SLOTS,
    [DescriptorUpdateField(nameof(ActivePlayerData.OverrideSpellsID), DescriptorType.Int32, bit: 105, ParentBit = 102)]
    ACTIVEPLAYER_OVERRIDE_SPELLS_ID,
    [DescriptorUpdateField(nameof(ActivePlayerData.LfgBonusFactionID), DescriptorType.Int32, bit: 106, ParentBit = 102)]
    ACTIVEPLAYER_LFG_BONUS_FACTION_ID,
    // LootSpecID is uint? on data, written as UInt16 ((ushort) cast)
    [DescriptorUpdateField(nameof(ActivePlayerData.LootSpecID), DescriptorType.UInt16, bit: 107, ParentBit = 102, Cast = "(ushort)")]
    ACTIVEPLAYER_LOOT_SPEC_ID,
    [DescriptorUpdateField(nameof(ActivePlayerData.OverrideZonePVPType), DescriptorType.UInt32, bit: 108, ParentBit = 102)]
    ACTIVEPLAYER_OVERRIDE_ZONE_PVP_TYPE,
    [DescriptorUpdateField(nameof(ActivePlayerData.Honor), DescriptorType.Int32, bit: 109, ParentBit = 102)]
    ACTIVEPLAYER_HONOR,
    [DescriptorUpdateField(nameof(ActivePlayerData.HonorNextLevel), DescriptorType.Int32, bit: 110, ParentBit = 102)]
    ACTIVEPLAYER_HONOR_NEXT_LEVEL,
    // bit 111: Field_F74 — unused
    // PvPTier*FromWins are uint? written as Int32 ((int) cast)
    [DescriptorUpdateField(nameof(ActivePlayerData.PvPTierMaxFromWins), DescriptorType.Int32, bit: 112, ParentBit = 102, Cast = "(int)")]
    ACTIVEPLAYER_PVP_TIER_MAX_FROM_WINS,
    [DescriptorUpdateField(nameof(ActivePlayerData.PvPLastWeeksTierMaxFromWins), DescriptorType.Int32, bit: 113, ParentBit = 102, Cast = "(int)")]
    ACTIVEPLAYER_PVP_LAST_WEEKS_TIER_MAX_FROM_WINS,
    [DescriptorUpdateField(nameof(ActivePlayerData.PvPRankProgress), DescriptorType.UInt8, bit: 114, ParentBit = 102)]
    ACTIVEPLAYER_PVP_RANK_PROGRESS,
    // bits 115-119: skipped
    // bit 120: GlyphsEnabled — Custom (read from _gameState, not ActivePlayerData).
    [DescriptorCustomField("GlyphsEnabled", bit: 120,
        customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerGlyphsEnabled),
        WriteOnly = true)]
    ACTIVEPLAYER_GLYPHS_ENABLED,
    // bits 121-123: skipped
    // PostFlush after bit 120 — hand-port has explicit data.FlushBits() before
    // array writes begin (file:1857).
    [DescriptorUpdatePostFlush(afterBit: 120)]
    ACTIVEPLAYER_BLOCK_102_POST_FLUSH,

    // ===========================================================================
    // Update — Arrays
    // ===========================================================================

    // InvSlots: bits 125-265 set by MaskMutator above. Body writes via CustomField.
    [DescriptorCustomField("InvSlotsGroup", bit: 124,
        customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerInvSlotsGroup),
        WriteOnly = true)]
    ACTIVEPLAYER_INV_SLOTS_GROUP,

    // TrackResourceMask[2] at bits 267-268 under parent 266.
    [DescriptorUpdateField(nameof(ActivePlayerData.TrackResourceMask), DescriptorType.UInt32, bit: 267,
        ArrayCount = 2, ArrayMode = ArrayMode.PerElement, ParentBit = 266)]
    ACTIVEPLAYER_TRACK_RESOURCE_MASK,

    // Shared parent 269 — 4 sub-arrays.
    [DescriptorUpdateField(nameof(ActivePlayerData.SpellCritPercentage), DescriptorType.Float, bit: 270,
        ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 269)]
    ACTIVEPLAYER_SPELL_CRIT_PERCENTAGE,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModDamageDonePos), DescriptorType.Int32, bit: 277,
        ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 269)]
    ACTIVEPLAYER_MOD_DAMAGE_DONE_POS,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModDamageDoneNeg), DescriptorType.Int32, bit: 284,
        ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 269)]
    ACTIVEPLAYER_MOD_DAMAGE_DONE_NEG,
    [DescriptorUpdateField(nameof(ActivePlayerData.ModDamageDonePercent), DescriptorType.Float, bit: 291,
        ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 269)]
    ACTIVEPLAYER_MOD_DAMAGE_DONE_PERCENT,

    // ExploredZones[240] at bits 299-538 under parent 298.
    [DescriptorUpdateField(nameof(ActivePlayerData.ExploredZones), DescriptorType.UInt64, bit: 299,
        ArrayCount = 240, ArrayMode = ArrayMode.PerElement, ParentBit = 298)]
    ACTIVEPLAYER_EXPLORED_ZONES,

    // RestInfo[2] at bits 540-541 under parent 539 — nested struct PerElement+CustomWriter.
    [DescriptorUpdateField(nameof(ActivePlayerData.RestInfo), DescriptorType.UInt32, bit: 540,
        ArrayCount = 2, ArrayMode = ArrayMode.PerElement, ParentBit = 539,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerRestInfo),
        CustomPredicate = "src.RestInfo != null && src.RestInfo[{i}] != null && (src.RestInfo[{i}].Threshold.HasValue || src.RestInfo[{i}].StateID.HasValue)")]
    ACTIVEPLAYER_REST_INFO,

    // Shared parent 542 — 2 sub-arrays.
    [DescriptorUpdateField(nameof(ActivePlayerData.WeaponDmgMultipliers), DescriptorType.Float, bit: 543,
        ArrayCount = 3, ArrayMode = ArrayMode.PerElement, ParentBit = 542)]
    ACTIVEPLAYER_WEAPON_DMG_MULTIPLIERS,
    [DescriptorUpdateField(nameof(ActivePlayerData.WeaponAtkSpeedMultipliers), DescriptorType.Float, bit: 546,
        ArrayCount = 3, ArrayMode = ArrayMode.PerElement, ParentBit = 542)]
    ACTIVEPLAYER_WEAPON_ATK_SPEED_MULTIPLIERS,

    // Shared parent 549 — 2 sub-arrays.
    [DescriptorUpdateField(nameof(ActivePlayerData.BuybackPrice), DescriptorType.UInt32, bit: 550,
        ArrayCount = 12, ArrayMode = ArrayMode.PerElement, ParentBit = 549)]
    ACTIVEPLAYER_BUYBACK_PRICE,
    // BuybackTimestamp is uint? written as Int64 ((long) cast)
    [DescriptorUpdateField(nameof(ActivePlayerData.BuybackTimestamp), DescriptorType.Int64, bit: 562,
        ArrayCount = 12, ArrayMode = ArrayMode.PerElement, ParentBit = 549, Cast = "(long)")]
    ACTIVEPLAYER_BUYBACK_TIMESTAMP,

    // CombatRatings[32] at bits 575-606 under parent 574.
    [DescriptorUpdateField(nameof(ActivePlayerData.CombatRatings), DescriptorType.Int32, bit: 575,
        ArrayCount = 32, ArrayMode = ArrayMode.PerElement, ParentBit = 574)]
    ACTIVEPLAYER_COMBAT_RATINGS,

    // PvpInfo[7] at bits 608-614 under parent 607 — nested struct PerElement+CustomWriter.
    // Source is PVPInfo[6] on data but bits cover 7 slots; predicate guards with length check.
    // WriteOrder=999999 forces emit AFTER GlyphsGroup (bit 1512) to match TC's
    // ActivePlayerData::WriteUpdate layout (hand-port file:2018-2072 — PvpInfo
    // serialized last despite low bit position).
    [DescriptorUpdateField(nameof(ActivePlayerData.PvpInfo), DescriptorType.UInt32, bit: 608,
        ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 607,
        WriteOrder = 999999,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerPvpInfo),
        CustomPredicate = "src.PvpInfo != null && {i} < src.PvpInfo.Length && src.PvpInfo[{i}] != null && (src.PvpInfo[{i}].Rating != 0 || src.PvpInfo[{i}].SeasonPlayed != 0 || src.PvpInfo[{i}].Disqualified)")]
    ACTIVEPLAYER_PVP_INFO,

    // NoReagentCostMask[4] at bits 616-619 under parent 615.
    [DescriptorUpdateField(nameof(ActivePlayerData.NoReagentCostMask), DescriptorType.UInt32, bit: 616,
        ArrayCount = 4, ArrayMode = ArrayMode.PerElement, ParentBit = 615)]
    ACTIVEPLAYER_NO_REAGENT_COST_MASK,

    // ProfessionSkillLine[2] at bits 621-622 under parent 620.
    [DescriptorUpdateField(nameof(ActivePlayerData.ProfessionSkillLine), DescriptorType.Int32, bit: 621,
        ArrayCount = 2, ArrayMode = ArrayMode.PerElement, ParentBit = 620)]
    ACTIVEPLAYER_PROFESSION_SKILL_LINE,

    // BagSlotFlags[4] at bits 624-627 under parent 623.
    [DescriptorUpdateField(nameof(ActivePlayerData.BagSlotFlags), DescriptorType.UInt32, bit: 624,
        ArrayCount = 4, ArrayMode = ArrayMode.PerElement, ParentBit = 623)]
    ACTIVEPLAYER_BAG_SLOT_FLAGS,

    // BankBagSlotFlags[7] at bits 629-635 under parent 628.
    [DescriptorUpdateField(nameof(ActivePlayerData.BankBagSlotFlags), DescriptorType.UInt32, bit: 629,
        ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 628)]
    ACTIVEPLAYER_BANK_BAG_SLOT_FLAGS,

    // QuestCompleted[875] at bits 637-1511 under parent 636.
    [DescriptorUpdateField(nameof(ActivePlayerData.QuestCompleted), DescriptorType.UInt64, bit: 637,
        ArrayCount = 875, ArrayMode = ArrayMode.PerElement, ParentBit = 636)]
    ACTIVEPLAYER_QUEST_COMPLETED,

    // Glyphs group at bit 1512 — bits 1513-1518 (GlyphSlots) and 1519-1524 (Glyphs)
    // are set by ApplyActivePlayerGlyphsMaskMutator above. Body writes interleaved.
    [DescriptorCustomField("GlyphsGroup", bit: 1512,
        customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerGlyphsGroup),
        WriteOnly = true)]
    ACTIVEPLAYER_GLYPHS_GROUP,
}
