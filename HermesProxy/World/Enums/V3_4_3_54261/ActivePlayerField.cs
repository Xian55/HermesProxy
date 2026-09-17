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
    // Create — fully declarative. What was one 1800-line hand-written writer is now
    // members in wire order; the writers that remain are named, single-purpose hooks for
    // shapes a per-field descriptor cannot express (computed values, session-sourced state,
    // interleaved parallel arrays, nested structs with non-zero defaults).
    // ActivePlayerSectionEquivalenceTests pins the whole Create wire against a frozen copy
    // of the pre-migration hand-port, so this stays byte-identical to it.
    //
    // Writes below are emitted in declaration order — this enum IS the wire order.
    // ===========================================================================

    // InvSlots[141]: value is computed (GetModernInvSlot fans legacy slot arrays into the
    // modern flat layout), so it stays a custom writer rather than a field read.
    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerInvSlots))]
    ACTIVEPLAYER_CREATE_INVSLOTS_CUSTOM,

    [DescriptorCreateField(nameof(ActivePlayerData.FarsightObject), DescriptorType.PackedGuid128)]
    ACTIVEPLAYER_CREATE_FARSIGHT_OBJECT,

    // Falls back to session state, which DefaultExpression cannot express.
    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerSummonedBattlePet))]
    ACTIVEPLAYER_CREATE_SUMMONED_BATTLE_PET_CUSTOM,

    // bit 3 dynamic field — count here, payload much later in the sequence.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerKnownTitlesCount))]
    ACTIVEPLAYER_CREATE_KNOWN_TITLES_COUNT_CUSTOM,

    [DescriptorCreateField(nameof(ActivePlayerData.Coinage), DescriptorType.UInt64)]
    ACTIVEPLAYER_CREATE_COINAGE,

    [DescriptorCreateField(nameof(ActivePlayerData.XP), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_XP,

    [DescriptorCreateField(nameof(ActivePlayerData.NextLevelXP), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_NEXT_LEVEL_XP,

    [DescriptorCreateField(nameof(ActivePlayerData.TrialXP), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_TRIAL_XP,

    // 256 slots x 7 parallel ushort arrays woven per index.
    [DescriptorCreatePlaceholder(DescriptorType.UInt16,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerSkillInterleaved))]
    ACTIVEPLAYER_CREATE_SKILL_INTERLEAVED_CUSTOM,

    // ---- Create slice 2: bits 33-68 ----

    [DescriptorCreateField(nameof(ActivePlayerData.CharacterPoints), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_CHARACTER_POINTS,

    [DescriptorCreateField(nameof(ActivePlayerData.MaxTalentTiers), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_MAX_TALENT_TIERS,

    [DescriptorCreateField(nameof(ActivePlayerData.TrackCreatureMask), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_TRACK_CREATURE_MASK,

    [DescriptorCreateField(nameof(ActivePlayerData.TrackResourceMask), DescriptorType.UInt32, ArrayCount = 2)]
    ACTIVEPLAYER_CREATE_TRACK_RESOURCE_MASK,

    // bits 36-48: block-38 expertise / percentage floats.
    [DescriptorCreateField(nameof(ActivePlayerData.MainhandExpertise), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_MAINHAND_EXPERTISE,

    [DescriptorCreateField(nameof(ActivePlayerData.OffhandExpertise), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_OFFHAND_EXPERTISE,

    [DescriptorCreateField(nameof(ActivePlayerData.RangedExpertise), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_RANGED_EXPERTISE,

    [DescriptorCreateField(nameof(ActivePlayerData.CombatRatingExpertise), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_COMBAT_RATING_EXPERTISE,

    [DescriptorCreateField(nameof(ActivePlayerData.BlockPercentage), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_BLOCK_PERCENTAGE,

    [DescriptorCreateField(nameof(ActivePlayerData.DodgePercentage), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_DODGE_PERCENTAGE,

    [DescriptorCreateField(nameof(ActivePlayerData.DodgePercentageFromAttribute), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_DODGE_PERCENTAGE_FROM_ATTRIBUTE,

    [DescriptorCreateField(nameof(ActivePlayerData.ParryPercentage), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_PARRY_PERCENTAGE,

    [DescriptorCreateField(nameof(ActivePlayerData.ParryPercentageFromAttribute), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_PARRY_PERCENTAGE_FROM_ATTRIBUTE,

    [DescriptorCreateField(nameof(ActivePlayerData.CritPercentage), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_CRIT_PERCENTAGE,

    [DescriptorCreateField(nameof(ActivePlayerData.RangedCritPercentage), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_RANGED_CRIT_PERCENTAGE,

    [DescriptorCreateField(nameof(ActivePlayerData.OffhandCritPercentage), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_OFFHAND_CRIT_PERCENTAGE,

    // 7 stat groups x 4 parallel float arrays (Min/Max/ModPos/ModNeg) woven per group.
    [DescriptorCreatePlaceholder(DescriptorType.Float,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerDamageDoneInterleaved))]
    ACTIVEPLAYER_CREATE_DAMAGE_DONE_INTERLEAVED_CUSTOM,

    [DescriptorCreateField(nameof(ActivePlayerData.ShieldBlock), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_SHIELD_BLOCK,

    // bit 50: ShieldBlockCritPercentage — no WotLK source property.
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_SHIELD_BLOCK_CRIT_PERCENTAGE_PLACEHOLDER,

    [DescriptorCreateField(nameof(ActivePlayerData.Mastery), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_MASTERY,

    [DescriptorCreateField(nameof(ActivePlayerData.Speed), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_SPEED,

    [DescriptorCreateField(nameof(ActivePlayerData.Avoidance), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_AVOIDANCE,

    [DescriptorCreateField(nameof(ActivePlayerData.Sturdiness), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_STURDINESS,

    [DescriptorCreateField(nameof(ActivePlayerData.Versatility), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_VERSATILITY,

    [DescriptorCreateField(nameof(ActivePlayerData.VersatilityBonus), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_VERSATILITY_BONUS,

    [DescriptorCreateField(nameof(ActivePlayerData.PvpPowerDamage), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_PVP_POWER_DAMAGE,

    [DescriptorCreateField(nameof(ActivePlayerData.PvpPowerHealing), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_PVP_POWER_HEALING,

    // bit 299 (parent 298): ExploredZones[240].
    [DescriptorCreateField(nameof(ActivePlayerData.ExploredZones), DescriptorType.UInt64, ArrayCount = 240)]
    ACTIVEPLAYER_CREATE_EXPLORED_ZONES,

    // bits 540-541 (parent 539): RestInfo[2] nested struct; StateID defaults to 1.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerRestInfo))]
    ACTIVEPLAYER_CREATE_REST_INFO_CUSTOM,

    [DescriptorCreateField(nameof(ActivePlayerData.ModHealingDonePos), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_MOD_HEALING_DONE_POS,

    [DescriptorCreateField(nameof(ActivePlayerData.ModHealingPercent), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_MOD_HEALING_PERCENT,

    [DescriptorCreateField(nameof(ActivePlayerData.ModHealingDonePercent), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_MOD_HEALING_DONE_PERCENT,

    [DescriptorCreateField(nameof(ActivePlayerData.ModPeriodicHealingDonePercent), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_MOD_PERIODIC_HEALING_DONE_PERCENT,

    // bits 543/546 (parent 542): WeaponDmgMultipliers[3] + WeaponAtkSpeedMultipliers[3], default 1f.
    [DescriptorCreatePlaceholder(DescriptorType.Float,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerWeaponMultipliersInterleaved))]
    ACTIVEPLAYER_CREATE_WEAPON_MULTIPLIERS_INTERLEAVED_CUSTOM,

    [DescriptorCreateField(nameof(ActivePlayerData.ModSpellPowerPercent), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_MOD_SPELL_POWER_PERCENT,

    [DescriptorCreateField(nameof(ActivePlayerData.ModResiliencePercent), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_MOD_RESILIENCE_PERCENT,

    [DescriptorCreateField(nameof(ActivePlayerData.OverrideSpellPowerByAPPercent), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_OVERRIDE_SPELL_POWER_BY_AP_PERCENT,

    [DescriptorCreateField(nameof(ActivePlayerData.OverrideAPBySpellPowerPercent), DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_OVERRIDE_AP_BY_SPELL_POWER_PERCENT,

    [DescriptorCreateField(nameof(ActivePlayerData.ModTargetResistance), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_MOD_TARGET_RESISTANCE,

    [DescriptorCreateField(nameof(ActivePlayerData.ModTargetPhysicalResistance), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_MOD_TARGET_PHYSICAL_RESISTANCE,

    // ---- Create slice 3: bits 69-114 ----

    [DescriptorCreateField(nameof(ActivePlayerData.LocalFlags), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_LOCAL_FLAGS,

    // bits 71-74 (parent 70): block-70 byte cluster. Real values, not zeros — see 2026-05-21
    // action-bar persistence fix.
    [DescriptorCreateField(nameof(ActivePlayerData.GrantableLevels), DescriptorType.UInt8)]
    ACTIVEPLAYER_CREATE_GRANTABLE_LEVELS,

    [DescriptorCreateField(nameof(ActivePlayerData.MultiActionBars), DescriptorType.UInt8)]
    ACTIVEPLAYER_CREATE_MULTI_ACTION_BARS,

    [DescriptorCreateField(nameof(ActivePlayerData.LifetimeMaxRank), DescriptorType.UInt8)]
    ACTIVEPLAYER_CREATE_LIFETIME_MAX_RANK,

    [DescriptorCreateField(nameof(ActivePlayerData.NumRespecs), DescriptorType.UInt8)]
    ACTIVEPLAYER_CREATE_NUM_RESPECS,

    [DescriptorCreateField(nameof(ActivePlayerData.AmmoID), DescriptorType.Int32, Cast = "(int)")]
    ACTIVEPLAYER_CREATE_AMMO_I_D,

    [DescriptorCreateField(nameof(ActivePlayerData.PvpMedals), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_PVP_MEDALS,

    // bits 550/562 (parent 549): BuybackPrice[12] + BuybackTimestamp[12], interleaved by slot.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerBuybackInterleaved))]
    ACTIVEPLAYER_CREATE_BUYBACK_INTERLEAVED_CUSTOM,

    // bits 77-84: honorable/dishonorable kill counters.
    [DescriptorCreateField(nameof(ActivePlayerData.TodayHonorableKills), DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_TODAY_HONORABLE_KILLS,

    [DescriptorCreateField(nameof(ActivePlayerData.TodayDishonorableKills), DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_TODAY_DISHONORABLE_KILLS,

    [DescriptorCreateField(nameof(ActivePlayerData.YesterdayHonorableKills), DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_YESTERDAY_HONORABLE_KILLS,

    [DescriptorCreateField(nameof(ActivePlayerData.YesterdayDishonorableKills), DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_YESTERDAY_DISHONORABLE_KILLS,

    [DescriptorCreateField(nameof(ActivePlayerData.LastWeekHonorableKills), DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_LAST_WEEK_HONORABLE_KILLS,

    [DescriptorCreateField(nameof(ActivePlayerData.LastWeekDishonorableKills), DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_LAST_WEEK_DISHONORABLE_KILLS,

    [DescriptorCreateField(nameof(ActivePlayerData.ThisWeekHonorableKills), DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_THIS_WEEK_HONORABLE_KILLS,

    [DescriptorCreateField(nameof(ActivePlayerData.ThisWeekDishonorableKills), DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_THIS_WEEK_DISHONORABLE_KILLS,

    // bits 85-91: contribution / lifetime kills / lastweek rank.
    [DescriptorCreateField(nameof(ActivePlayerData.ThisWeekContribution), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_THIS_WEEK_CONTRIBUTION,

    [DescriptorCreateField(nameof(ActivePlayerData.LifetimeHonorableKills), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_LIFETIME_HONORABLE_KILLS,

    [DescriptorCreateField(nameof(ActivePlayerData.LifetimeDishonorableKills), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_LIFETIME_DISHONORABLE_KILLS,

    // bit 88: Field_F24 — unused, no source property.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_FIELD_F24_PLACEHOLDER,

    [DescriptorCreateField(nameof(ActivePlayerData.YesterdayContribution), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_YESTERDAY_CONTRIBUTION,

    [DescriptorCreateField(nameof(ActivePlayerData.LastWeekContribution), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_LAST_WEEK_CONTRIBUTION,

    [DescriptorCreateField(nameof(ActivePlayerData.LastWeekRank), DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_LAST_WEEK_RANK,

    [DescriptorCreateField(nameof(ActivePlayerData.WatchedFactionIndex), DescriptorType.Int32, DefaultExpression = "-1")]
    ACTIVEPLAYER_CREATE_WATCHED_FACTION_INDEX,

    // bit 575 (parent 574): CombatRatings[32].
    [DescriptorCreateField(nameof(ActivePlayerData.CombatRatings), DescriptorType.Int32, ArrayCount = 32)]
    ACTIVEPLAYER_CREATE_COMBAT_RATINGS,

    [DescriptorCreateField(nameof(ActivePlayerData.MaxLevel), DescriptorType.Int32, DefaultExpression = "LegacyVersion.GetMaxLevel()")]
    ACTIVEPLAYER_CREATE_MAX_LEVEL,

    // bits 94-95: ScalingPlayerLevelDelta / MaxCreatureScalingLevel — live properties
    // exist but legacy 3.3.5 never populates them.
    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_SCALING_PLAYER_LEVEL_DELTA_PLACEHOLDER,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_MAX_CREATURE_SCALING_LEVEL_PLACEHOLDER,

    // bit 616 (parent 615): NoReagentCostMask[4] — live property exists; TODO per-element read.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, Count = 4)]
    ACTIVEPLAYER_CREATE_NO_REAGENT_COST_MASK_PLACEHOLDER,

    [DescriptorCreateField(nameof(ActivePlayerData.PetSpellPower), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_PET_SPELL_POWER,

    // bit 621 (parent 620): ProfessionSkillLine[2].
    [DescriptorCreateField(nameof(ActivePlayerData.ProfessionSkillLine), DescriptorType.Int32, ArrayCount = 2)]
    ACTIVEPLAYER_CREATE_PROFESSION_SKILL_LINE,

    // bits 97-99: UiHitModifier / UiSpellHitModifier / HomeRealmTimeOffset — live
    // properties exist but legacy 3.3.5 never populates them.
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_UI_HIT_MODIFIER_PLACEHOLDER,

    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    ACTIVEPLAYER_CREATE_UI_SPELL_HIT_MODIFIER_PLACEHOLDER,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_HOME_REALM_TIME_OFFSET_PLACEHOLDER,

    [DescriptorCreateField(nameof(ActivePlayerData.ModPetHaste), DescriptorType.Float, DefaultExpression = "1f")]
    ACTIVEPLAYER_CREATE_MOD_PET_HASTE,

    [DescriptorCreateField(nameof(ActivePlayerData.LocalRegenFlags), DescriptorType.UInt8)]
    ACTIVEPLAYER_CREATE_LOCAL_REGEN_FLAGS,

    [DescriptorCreateField(nameof(ActivePlayerData.AuraVision), DescriptorType.UInt8)]
    ACTIVEPLAYER_CREATE_AURA_VISION,

    [DescriptorCreateField(nameof(ActivePlayerData.NumBackpackSlots), DescriptorType.UInt8, DefaultExpression = "16")]
    ACTIVEPLAYER_CREATE_NUM_BACKPACK_SLOTS,

    // bits 105-108: no WotLK source.
    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_OVERRIDE_SPELLS_ID_PLACEHOLDER,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_LFG_BONUS_FACTION_ID_PLACEHOLDER,

    [DescriptorCreatePlaceholder(DescriptorType.UInt16)]
    ACTIVEPLAYER_CREATE_LOOT_SPEC_ID_PLACEHOLDER,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_OVERRIDE_ZONE_PVP_TYPE_PLACEHOLDER,

    // bit 624 (parent 623): BagSlotFlags[4] — live property exists; TODO per-element read.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, Count = 4)]
    ACTIVEPLAYER_CREATE_BAG_SLOT_FLAGS_PLACEHOLDER,

    // bit 629 (parent 628): BankBagSlotFlags[7] — live property exists; TODO per-element read.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, Count = 7)]
    ACTIVEPLAYER_CREATE_BANK_BAG_SLOT_FLAGS_PLACEHOLDER,

    // bit 637 (parent 636): QuestCompleted[875] — live property exists; TODO per-element read.
    [DescriptorCreatePlaceholder(DescriptorType.UInt64, Count = 875)]
    ACTIVEPLAYER_CREATE_QUEST_COMPLETED_PLACEHOLDER,

    [DescriptorCreateField(nameof(ActivePlayerData.Honor), DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_HONOR,

    [DescriptorCreateField(nameof(ActivePlayerData.HonorNextLevel), DescriptorType.Int32, DefaultExpression = "5500")]
    ACTIVEPLAYER_CREATE_HONOR_NEXT_LEVEL,

    // bit 111: Field_F74 — descriptor: unused.
    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_FIELD_F74_PLACEHOLDER,

    // bits 112-113: PvP tier maxima, sentinel -1 when unset.
    [DescriptorCreatePlaceholder(DescriptorType.Int32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerPvPTierMax))]
    ACTIVEPLAYER_CREATE_PVP_TIER_MAX_CUSTOM,

    [DescriptorCreateField(nameof(ActivePlayerData.PvPRankProgress), DescriptorType.UInt8)]
    ACTIVEPLAYER_CREATE_PVP_RANK_PROGRESS,

    // PerksProgramCurrency — no WotLK source.
    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_PERKS_PROGRAM_CURRENCY_PLACEHOLDER,

    // ---- Create slice 4: dynamic-field prefixes, payloads and bit tail ----

    // Resize prefixes: ResearchSites, ResearchSiteProgress, Research,
    // DailyQuestsCompleted, AvailableQuestLineXQuestIDs, Field_1000 — all empty.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, Count = 6)]
    ACTIVEPLAYER_CREATE_DYNAMIC_RESIZE_PREFIXES_A_PLACEHOLDER,

    // Heirlooms.Resize + HeirloomFlags.Resize — the two non-zero prefixes.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerHeirloomCounts))]
    ACTIVEPLAYER_CREATE_HEIRLOOM_COUNTS_CUSTOM,

    // Resize prefixes: Toys (learned count), then Transmog / ConditionalTransmog /
    // SelfResSpells / CharacterRestrictions / SpellPctModByLabel / SpellFlatModByLabel /
    // TaskQuests (all empty).
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerToyResizePrefixes))]
    ACTIVEPLAYER_CREATE_DYNAMIC_RESIZE_PREFIXES_B_PLACEHOLDER,

    // TransportServerTime — no WotLK source.
    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ACTIVEPLAYER_CREATE_TRANSPORT_SERVER_TIME_PLACEHOLDER,

    // TraitConfigs.size (always 0) + ActiveCombatTraitConfigID — no WotLK source.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, Count = 2)]
    ACTIVEPLAYER_CREATE_TRAIT_CONFIG_PLACEHOLDER,

    // bit 1512 GlyphsGroup + bit 120 GlyphsEnabled, both sourced from session state.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerGlyphs))]
    ACTIVEPLAYER_CREATE_GLYPHS_CUSTOM,

    // LfgRoles.
    [DescriptorCreatePlaceholder(DescriptorType.UInt8)]
    ACTIVEPLAYER_CREATE_LFG_ROLES_PLACEHOLDER,

    // CategoryCooldownMods.Resize + WeeklySpellUses.Resize.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, Count = 2)]
    ACTIVEPLAYER_CREATE_SPELL_USES_RESIZE_PLACEHOLDER,

    // NumStableSlots — real value, not a zero filler; see the custom writer.
    [DescriptorCreatePlaceholder(DescriptorType.UInt8,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerNumStableSlots))]
    ACTIVEPLAYER_CREATE_NUM_STABLE_SLOTS_PLACEHOLDER,

    // Dynamic-field payloads: KnownTitles, Heirlooms, HeirloomFlags.
    [DescriptorCreatePlaceholder(DescriptorType.UInt64,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerDynamicPayloads))]
    ACTIVEPLAYER_CREATE_DYNAMIC_PAYLOADS_CUSTOM,

    // bits 608-614 (parent 607): PvpInfo[7], each element ending in its own FlushBits.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateActivePlayerPvpInfo))]
    ACTIVEPLAYER_CREATE_PVP_INFO_CUSTOM,

    // Align after the PvpInfo group (WriteBits with a zero count is a bare flush).
    [DescriptorCreateBitsPlaceholder(0u, 0)]
    ACTIVEPLAYER_CREATE_PVP_INFO_TAIL_FLUSH,

    // Three trailing group-cap bits.
    [DescriptorCreateBitsPlaceholder(0u, 3)]
    ACTIVEPLAYER_CREATE_TAIL_BITS_3,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ACTIVEPLAYER_CREATE_TAIL_UINT32_PLACEHOLDER,

    [DescriptorCreatePlaceholder(DescriptorType.Int32, Count = 8)]
    ACTIVEPLAYER_CREATE_TAIL_INT32_PLACEHOLDER,

    [DescriptorCreatePlaceholder(DescriptorType.Int64)]
    ACTIVEPLAYER_CREATE_TAIL_INT64_PLACEHOLDER,

    // Final trailing bit.
    [DescriptorCreateBitsPlaceholder(0u, 1)]
    ACTIVEPLAYER_CREATE_TAIL_BIT_1,

    // Closing flush the hand-port emitted twice; the first is already above.
    [DescriptorCreateBitsPlaceholder(0u, 0)]
    ACTIVEPLAYER_CREATE_TAIL_FINAL_FLUSH,

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

    // GlyphsDirty: the mutator body captures + clears the session's ActiveGlyphsDirty.
    // When true, sets 1512 + 1513-1518 + 1519-1524 in one shot. Body writes in matching
    // CustomField.
    // HasAnyPredicate: dirty flag must trigger Values update (glyph/spec swap case).
    // It reads `gameState`, not `_gameState`: the predicate is inlined into the generated
    // static HasAnyActivePlayerFieldSet(updateData, gameState), which the Values filter
    // calls without a builder. The mutator body is an instance method and still says
    // _gameState. Name a predicate's source after the static's parameters or the
    // generated file will not compile.
    [DescriptorMaskMutator(
        nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.ApplyActivePlayerGlyphsMaskMutator),
        HasAnyPredicate = "gameState.ActiveGlyphsDirty")]
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

    // Toys: DynamicUpdateField<int32, 0, 9> in Wrathion UpdateFields.h.
    // PlayerHasToy / Already known / journal count read this field, not
    // SMSG_ACCOUNT_TOY_UPDATE. A live Values write is what makes learn
    // show up without a relog.
    [DescriptorMaskPreamble(bit: 9,
        customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerToysPreamble))]
    ACTIVEPLAYER_TOYS_PREAMBLE,

    [DescriptorUpdateField(nameof(ActivePlayerData.Toys), DescriptorType.Int32, bit: 9, ParentBit = 0,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerToysBody),
        CustomPredicate = "src.Toys != null && src.Toys.Count > 0")]
    ACTIVEPLAYER_TOYS,

    // ===========================================================================
    // Update — Block 0 scalars (group bit 0, bits 26-37)
    // ===========================================================================

    [DescriptorUpdateField(nameof(ActivePlayerData.FarsightObject), DescriptorType.PackedGuid128, bit: 26, ParentBit = 0)]
    ACTIVEPLAYER_FARSIGHT_OBJECT,
    [DescriptorUpdateField(nameof(ActivePlayerData.SummonedBattlePetGUID), DescriptorType.PackedGuid128, bit: 27, ParentBit = 0)]
    ACTIVEPLAYER_SUMMONED_BATTLE_PET_GUID,
    [DescriptorUpdateField(nameof(ActivePlayerData.Coinage), DescriptorType.UInt64, bit: 28, ParentBit = 0)]
    ACTIVEPLAYER_COINAGE,
    // Native UpdateFields.h:739 — UpdateField<uint8, 102, 123>. Parent 102 is the same
    // nested group the glyph slots hang off, so both bits have to be set.
    [DescriptorUpdateField(nameof(ActivePlayerData.NumStableSlots), DescriptorType.UInt8, bit: 123, ParentBit = 102)]
    ACTIVEPLAYER_NUM_STABLE_SLOTS,
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
    // bit 120: GlyphsEnabled — nested under parent bit 102 in the V3_4_3 changesMask
    // (WPP V3_4_0 UpdateFieldsHandler343.cs:3389-3454 — bits 103-123 all live inside
    // `if (changesMask[102])`). ParentBit=102 makes the generator emit
    // `if (src.GlyphsEnabled.HasValue) { blocks.SetBit(102); blocks.SetBit(120); }`
    // automatically. Source-driven from ActivePlayerData.GlyphsEnabled which
    // UpdateHandler populates alongside the _gameState cache used by the Create path.
    [DescriptorUpdateField(nameof(ActivePlayerData.GlyphsEnabled), DescriptorType.UInt8, bit: 120, ParentBit = 102)]
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
    // The null check is the whole guard: an element only exists once UpdateHandler has been told
    // something about that bracket. It used to also require a non-zero Rating or SeasonPlayed,
    // which silently dropped a real but unplayed arena team — every stat on a freshly created
    // team is zero, so the bracket was suppressed exactly when the client most needed telling
    // it existed, and the arena panel's tile stayed blank.
    // WriteOrder=999999 forces emit AFTER GlyphsGroup (bit 1512) to match TC's
    // ActivePlayerData::WriteUpdate layout (hand-port file:2018-2072 — PvpInfo
    // serialized last despite low bit position).
    [DescriptorUpdateField(nameof(ActivePlayerData.PvpInfo), DescriptorType.UInt32, bit: 608,
        ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 607,
        WriteOrder = 999999,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateActivePlayerPvpInfo),
        CustomPredicate = "src.PvpInfo != null && {i} < src.PvpInfo.Length && src.PvpInfo[{i}] != null")]
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
