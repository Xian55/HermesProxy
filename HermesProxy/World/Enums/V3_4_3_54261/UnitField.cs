using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V3_4_3_54261;

// Phase 5b Unit section — descriptor-driven WriteCreateUnitData + WriteUpdateUnitData +
// HasAnyUnitFieldSet emit. Update path uses Cascade = true (V3_4_3 block-bit-0 force
// rule for blocks 0/1/2/3) and 5 hand-rolled custom writers for interleaved-array
// groups + ChannelData composite + ChannelObjects dynamic field.
//
// MaskOnly arrays (Power/MaxPower/ModPowerRegen, Stats/PosBuff/NegBuff,
// Resistances/PowerCostModifier/PowerCostMultiplier, ResistanceBuffModsPositive/Negative)
// register their per-element bits in Pass-1 but skip Pass-2 wire writes; the
// sibling WriteOnly = true DescriptorCustomField at the shared parent bit owns the
// interleaved-by-index write.
[DescriptorSection(DataType = typeof(UnitData), MaskMode = MaskMode.Blocks, MaskWidth = 8, Cascade = true)]
public enum UnitField
{
    // ============================================================
    // Create-path emit order — enum declaration order (lines mirror
    // hand-port WriteCreateUnitData line numbers in comments).
    // Update-path emit order — bit-ascending (generator sorts).
    // ============================================================

    // ---- Health/MaxHealth/DisplayID + NpcFlags[2] + 4 placeholders ----

    [DescriptorCreateField(nameof(UnitData.Health), DescriptorType.Int64)]
    [DescriptorUpdateField(nameof(UnitData.Health), DescriptorType.Int64, bit: 5)]
    UNIT_HEALTH,

    [DescriptorCreateField(nameof(UnitData.MaxHealth), DescriptorType.Int64)]
    [DescriptorUpdateField(nameof(UnitData.MaxHealth), DescriptorType.Int64, bit: 6)]
    UNIT_MAX_HEALTH,

    [DescriptorCreateField(nameof(UnitData.DisplayID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.DisplayID), DescriptorType.Int32, bit: 7)]
    UNIT_DISPLAY_ID,

    // Create: 2-element Grouped (write all w/ default 0). Update: PerElement with
    // `HasValue && != 0` predicate, bits 114-115, parent 113.
    [DescriptorCreateField(nameof(UnitData.NpcFlags), DescriptorType.UInt32, ArrayCount = 2, ArrayMode = ArrayMode.Grouped)]
    [DescriptorUpdateField(nameof(UnitData.NpcFlags), DescriptorType.UInt32, bit: 114,
                           ArrayCount = 2, ArrayMode = ArrayMode.PerElement, ParentBit = 113,
                           CustomPredicate = "src.NpcFlags[{i}].HasValue && src.NpcFlags[{i}] != 0")]
    UNIT_NPC_FLAGS,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_1,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_2,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_3,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_4,

    // ---- GUID block ----

    [DescriptorCreateField(nameof(UnitData.Charm), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(UnitData.Charm), DescriptorType.PackedGuid128, bit: 11)]
    UNIT_CHARM,
    [DescriptorCreateField(nameof(UnitData.Summon), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(UnitData.Summon), DescriptorType.PackedGuid128, bit: 12)]
    UNIT_SUMMON,
    // Critter — Create owner-only, no Update bit (hand-port doesn't track in Update).
    [DescriptorCreateField(nameof(UnitData.Critter), DescriptorType.PackedGuid128, OwnerOnly = true)]
    UNIT_CRITTER_OWNER,
    [DescriptorCreateField(nameof(UnitData.CharmedBy), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(UnitData.CharmedBy), DescriptorType.PackedGuid128, bit: 14)]
    UNIT_CHARMED_BY,
    [DescriptorCreateField(nameof(UnitData.SummonedBy), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(UnitData.SummonedBy), DescriptorType.PackedGuid128, bit: 15)]
    UNIT_SUMMONED_BY,
    [DescriptorCreateField(nameof(UnitData.CreatedBy), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(UnitData.CreatedBy), DescriptorType.PackedGuid128, bit: 16)]
    UNIT_CREATED_BY,
    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128)]
    UNIT_PAD_GUID_1,
    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128)]
    UNIT_PAD_GUID_2,
    [DescriptorCreateField(nameof(UnitData.Target), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(UnitData.Target), DescriptorType.PackedGuid128, bit: 19)]
    UNIT_TARGET,
    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128)]
    UNIT_PAD_GUID_3,

    // ---- UInt64 placeholder + ChannelData composite + UInt32 placeholder ----

    [DescriptorCreatePlaceholder(DescriptorType.UInt64)]
    UNIT_PAD_5,

    // ChannelData: scalar CustomWriter on both Create (existing) and Update.
    // Update predicate is the standard "!= null"; CustomWriter inlines 2× Int32.
    [DescriptorCreatePlaceholder(DescriptorType.Int32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitChannelDataInline))]
    [DescriptorUpdateField(nameof(UnitData.ChannelData), DescriptorType.Int32, bit: 22,
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitChannelDataInline))]
    UNIT_CHANNEL_DATA_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_6,

    // ---- Race/Class/PlayerClass/Sex ----
    // Create uses CustomWriter for IsImpersonatingCreatureBake override.
    // Update writes raw (hand-port has no zero override on the Update path).

    [DescriptorCreatePlaceholder(DescriptorType.UInt8, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitRaceId))]
    [DescriptorUpdateField(nameof(UnitData.RaceId), DescriptorType.UInt8, bit: 24)]
    UNIT_RACE_ID_CUSTOM,
    [DescriptorCreatePlaceholder(DescriptorType.UInt8, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitClassId))]
    [DescriptorUpdateField(nameof(UnitData.ClassId), DescriptorType.UInt8, bit: 25)]
    UNIT_CLASS_ID_CUSTOM,

    [DescriptorCreateField(nameof(UnitData.PlayerClassId), DescriptorType.UInt8)]
    UNIT_PLAYER_CLASS_ID,

    [DescriptorCreatePlaceholder(DescriptorType.UInt8, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitSexId))]
    [DescriptorUpdateField(nameof(UnitData.SexId), DescriptorType.UInt8, bit: 27)]
    UNIT_SEX_ID_CUSTOM,

    // ---- DisplayPower (UInt8 width — bug-history regression vector) ----

    [DescriptorCreateField(nameof(UnitData.DisplayPower), DescriptorType.UInt8, Cast = "(byte)")]
    [DescriptorUpdateField(nameof(UnitData.DisplayPower), DescriptorType.UInt8, bit: 28, Cast = "(byte)")]
    UNIT_DISPLAY_POWER,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_7,

    // ---- Owner float pairs + Power interleaved (Create-only via custom writer) ----

    [DescriptorCreatePlaceholder(DescriptorType.Float, OwnerOnly = true, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitOwnerFloatPairs))]
    UNIT_OWNER_FLOAT_PAIRS_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.Int32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitPowerInterleaved))]
    UNIT_POWER_INTERLEAVED_CUSTOM,

    // ---- Level + EffectiveLevel + Content/Scaling ----

    [DescriptorCreateField(nameof(UnitData.Level), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.Level), DescriptorType.Int32, bit: 30)]
    UNIT_LEVEL,

    [DescriptorCreatePlaceholder(DescriptorType.Int32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitEffectiveLevel))]
    [DescriptorUpdateField(nameof(UnitData.EffectiveLevel), DescriptorType.Int32, bit: 31)]
    UNIT_EFFECTIVE_LEVEL_CUSTOM,

    [DescriptorCreateField(nameof(UnitData.ContentTuningID), DescriptorType.Int32)]
    UNIT_CONTENT_TUNING_ID,
    [DescriptorCreateField(nameof(UnitData.ScalingLevelMin), DescriptorType.Int32)]
    UNIT_SCALING_LEVEL_MIN,
    [DescriptorCreateField(nameof(UnitData.ScalingLevelMax), DescriptorType.Int32)]
    UNIT_SCALING_LEVEL_MAX,
    [DescriptorCreateField(nameof(UnitData.ScalingLevelDelta), DescriptorType.Int32)]
    UNIT_SCALING_LEVEL_DELTA,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    UNIT_PAD_8,
    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    UNIT_PAD_9,
    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    UNIT_PAD_10,

    [DescriptorCreateField(nameof(UnitData.FactionTemplate), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.FactionTemplate), DescriptorType.Int32, bit: 40)]
    UNIT_FACTION_TEMPLATE,

    // ---- VirtualItems[3] — Create custom (PlayerData fallback), Update PerElement with inner mask ----

    [DescriptorCreatePlaceholder(DescriptorType.Int32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitVirtualItems))]
    [DescriptorUpdateField(nameof(UnitData.VirtualItems), DescriptorType.Int32, bit: 168,
                           ArrayCount = 3, ArrayMode = ArrayMode.PerElement, ParentBit = 167,
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitVirtualItem),
                           CustomPredicate = "src.VirtualItems[{i}].HasValue && src.VirtualItems[{i}]!.Value.ItemID != 0")]
    UNIT_VIRTUAL_ITEMS_CUSTOM,

    // ---- Flags / Flags2[sanitize] / AuraState + AttackRoundBaseTime[2] ----

    [DescriptorCreateField(nameof(UnitData.Flags), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(UnitData.Flags), DescriptorType.UInt32, bit: 41)]
    UNIT_FLAGS,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitFlags2Sanitized))]
    [DescriptorUpdateField(nameof(UnitData.Flags2), DescriptorType.UInt32, bit: 42,
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitFlags2))]
    UNIT_FLAGS2_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_11,

    [DescriptorCreateField(nameof(UnitData.AuraState), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(UnitData.AuraState), DescriptorType.UInt32, bit: 44)]
    UNIT_AURA_STATE,

    // Create: Grouped (write 2 elements with default 0). Update: PerElement bits 172-173, parent 171.
    [DescriptorCreateField(nameof(UnitData.AttackRoundBaseTime), DescriptorType.UInt32, ArrayCount = 2, ArrayMode = ArrayMode.Grouped)]
    [DescriptorUpdateField(nameof(UnitData.AttackRoundBaseTime), DescriptorType.UInt32, bit: 172,
                           ArrayCount = 2, ArrayMode = ArrayMode.PerElement, ParentBit = 171)]
    UNIT_ATTACK_ROUND_BASE_TIME,

    // ---- RangedAttackRoundBaseTime owner-only Create (bow-fallback) ----

    [DescriptorCreatePlaceholder(DescriptorType.UInt32, OwnerOnly = true, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitRangedAttackTime))]
    UNIT_RANGED_ATTACK_ROUND_TIME_CUSTOM,

    // ---- BoundingRadius / CombatReach / NativeDisplayID / MountDisplayID + padding ----

    [DescriptorCreateField(nameof(UnitData.BoundingRadius), DescriptorType.Float, DefaultExpression = "0.389f")]
    [DescriptorUpdateField(nameof(UnitData.BoundingRadius), DescriptorType.Float, bit: 46)]
    UNIT_BOUNDING_RADIUS,
    [DescriptorCreateField(nameof(UnitData.CombatReach), DescriptorType.Float, DefaultExpression = "1.5f")]
    [DescriptorUpdateField(nameof(UnitData.CombatReach), DescriptorType.Float, bit: 47)]
    UNIT_COMBAT_REACH,

    [DescriptorCreatePlaceholder(DescriptorType.Float, "1f")]
    UNIT_PAD_FLOAT_ONE_1,

    [DescriptorCreateField(nameof(UnitData.NativeDisplayID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.NativeDisplayID), DescriptorType.Int32, bit: 49)]
    UNIT_NATIVE_DISPLAY_ID,

    [DescriptorCreatePlaceholder(DescriptorType.Float, "1f")]
    UNIT_PAD_FLOAT_ONE_2,

    [DescriptorCreateField(nameof(UnitData.MountDisplayID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.MountDisplayID), DescriptorType.Int32, bit: 51)]
    UNIT_MOUNT_DISPLAY_ID,

    // ---- Damage fields (owner-only Create, unconditional Update) ----

    [DescriptorCreateField(nameof(UnitData.MinDamage), DescriptorType.Float, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.MinDamage), DescriptorType.Float, bit: 52)]
    UNIT_MIN_DAMAGE,
    [DescriptorCreateField(nameof(UnitData.MaxDamage), DescriptorType.Float, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.MaxDamage), DescriptorType.Float, bit: 53)]
    UNIT_MAX_DAMAGE,
    [DescriptorCreateField(nameof(UnitData.MinOffHandDamage), DescriptorType.Float, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.MinOffHandDamage), DescriptorType.Float, bit: 54)]
    UNIT_MIN_OFF_HAND_DAMAGE,
    [DescriptorCreateField(nameof(UnitData.MaxOffHandDamage), DescriptorType.Float, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.MaxOffHandDamage), DescriptorType.Float, bit: 55)]
    UNIT_MAX_OFF_HAND_DAMAGE,

    // ---- StandState / PetLoyalty / VisFlags / AnimTier + Pet UInt32 × 4 ----

    [DescriptorCreateField(nameof(UnitData.StandState), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(UnitData.StandState), DescriptorType.UInt8, bit: 56)]
    UNIT_STAND_STATE,
    [DescriptorCreateField(nameof(UnitData.PetLoyaltyIndex), DescriptorType.UInt8)]
    UNIT_PET_LOYALTY_INDEX,
    [DescriptorCreateField(nameof(UnitData.VisFlags), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(UnitData.VisFlags), DescriptorType.UInt8, bit: 58)]
    UNIT_VIS_FLAGS,
    [DescriptorCreateField(nameof(UnitData.AnimTier), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(UnitData.AnimTier), DescriptorType.UInt8, bit: 59)]
    UNIT_ANIM_TIER,
    [DescriptorCreateField(nameof(UnitData.PetNumber), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(UnitData.PetNumber), DescriptorType.UInt32, bit: 60)]
    UNIT_PET_NUMBER,
    [DescriptorCreateField(nameof(UnitData.PetNameTimestamp), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(UnitData.PetNameTimestamp), DescriptorType.UInt32, bit: 61)]
    UNIT_PET_NAME_TIMESTAMP,
    [DescriptorCreateField(nameof(UnitData.PetExperience), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(UnitData.PetExperience), DescriptorType.UInt32, bit: 62)]
    UNIT_PET_EXPERIENCE,
    [DescriptorCreateField(nameof(UnitData.PetNextLevelExperience), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(UnitData.PetNextLevelExperience), DescriptorType.UInt32, bit: 63)]
    UNIT_PET_NEXT_LEVEL_EXPERIENCE,

    // ---- ModCastSpeed / ModCastHaste (default 1f Create) + float-1 placeholders × 4 + CreatedBySpell + EmoteState + 2× Int16 0 ----

    [DescriptorCreateField(nameof(UnitData.ModCastSpeed), DescriptorType.Float, DefaultExpression = "1f")]
    [DescriptorUpdateField(nameof(UnitData.ModCastSpeed), DescriptorType.Float, bit: 65)]
    UNIT_MOD_CAST_SPEED,
    [DescriptorCreateField(nameof(UnitData.ModCastHaste), DescriptorType.Float, DefaultExpression = "1f")]
    [DescriptorUpdateField(nameof(UnitData.ModCastHaste), DescriptorType.Float, bit: 66)]
    UNIT_MOD_CAST_HASTE,

    [DescriptorCreatePlaceholder(DescriptorType.Float, "1f")]
    UNIT_PAD_FLOAT_ONE_3,
    [DescriptorCreatePlaceholder(DescriptorType.Float, "1f")]
    UNIT_PAD_FLOAT_ONE_4,
    [DescriptorCreatePlaceholder(DescriptorType.Float, "1f")]
    UNIT_PAD_FLOAT_ONE_5,
    [DescriptorCreatePlaceholder(DescriptorType.Float, "1f")]
    UNIT_PAD_FLOAT_ONE_6,

    [DescriptorCreateField(nameof(UnitData.CreatedBySpell), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.CreatedBySpell), DescriptorType.Int32, bit: 71)]
    UNIT_CREATED_BY_SPELL,
    [DescriptorCreateField(nameof(UnitData.EmoteState), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.EmoteState), DescriptorType.Int32, bit: 72)]
    UNIT_EMOTE_STATE,

    [DescriptorCreatePlaceholder(DescriptorType.Int16)]
    UNIT_PAD_INT16_1,
    [DescriptorCreatePlaceholder(DescriptorType.Int16)]
    UNIT_PAD_INT16_2,

    // ---- Stats/Resistances/PowerCost/ResBuffMods interleaved groups (Create custom-writer placeholders) ----

    [DescriptorCreatePlaceholder(DescriptorType.Int32, OwnerOnly = true, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitStatsInterleaved))]
    UNIT_STATS_INTERLEAVED_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.Int32, OwnerOnly = true, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitResistances))]
    UNIT_RESISTANCES_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.Int32, OwnerOnly = true, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitPowerCostInterleaved))]
    UNIT_POWER_COST_INTERLEAVED_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.Int32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitResistanceBuffModsInterleaved))]
    UNIT_RES_BUFF_MODS_INTERLEAVED_CUSTOM,

    // ---- BaseMana / BaseHealth (owner-only Create, unconditional Update) ----

    [DescriptorCreateField(nameof(UnitData.BaseMana), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.BaseMana), DescriptorType.Int32, bit: 75)]
    UNIT_BASE_MANA,

    [DescriptorCreateField(nameof(UnitData.BaseHealth), DescriptorType.Int32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.BaseHealth), DescriptorType.Int32, bit: 76)]
    UNIT_BASE_HEALTH_OWNER,

    // ---- SheatheState / PvpFlags / PetFlags / ShapeshiftForm ----

    [DescriptorCreateField(nameof(UnitData.SheatheState), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(UnitData.SheatheState), DescriptorType.UInt8, bit: 77)]
    UNIT_SHEATHE_STATE,
    [DescriptorCreateField(nameof(UnitData.PvpFlags), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(UnitData.PvpFlags), DescriptorType.UInt8, bit: 78)]
    UNIT_PVP_FLAGS,
    [DescriptorCreateField(nameof(UnitData.PetFlags), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(UnitData.PetFlags), DescriptorType.UInt8, bit: 79)]
    UNIT_PET_FLAGS,
    [DescriptorCreateField(nameof(UnitData.ShapeshiftForm), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(UnitData.ShapeshiftForm), DescriptorType.UInt8, bit: 80)]
    UNIT_SHAPESHIFT_FORM,

    // ---- AttackPower block (owner-only Create, unconditional Update) ----

    [DescriptorCreateField(nameof(UnitData.AttackPower), DescriptorType.Int32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.AttackPower), DescriptorType.Int32, bit: 81)]
    UNIT_ATTACK_POWER,
    [DescriptorCreateField(nameof(UnitData.AttackPowerModPos), DescriptorType.Int32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.AttackPowerModPos), DescriptorType.Int32, bit: 82)]
    UNIT_ATTACK_POWER_MOD_POS,
    [DescriptorCreateField(nameof(UnitData.AttackPowerModNeg), DescriptorType.Int32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.AttackPowerModNeg), DescriptorType.Int32, bit: 83)]
    UNIT_ATTACK_POWER_MOD_NEG,
    [DescriptorCreateField(nameof(UnitData.AttackPowerMultiplier), DescriptorType.Float, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.AttackPowerMultiplier), DescriptorType.Float, bit: 84)]
    UNIT_ATTACK_POWER_MULTIPLIER,
    [DescriptorCreateField(nameof(UnitData.RangedAttackPower), DescriptorType.Int32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.RangedAttackPower), DescriptorType.Int32, bit: 85)]
    UNIT_RANGED_ATTACK_POWER,
    [DescriptorCreateField(nameof(UnitData.RangedAttackPowerModPos), DescriptorType.Int32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.RangedAttackPowerModPos), DescriptorType.Int32, bit: 86)]
    UNIT_RANGED_ATTACK_POWER_MOD_POS,
    [DescriptorCreateField(nameof(UnitData.RangedAttackPowerModNeg), DescriptorType.Int32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.RangedAttackPowerModNeg), DescriptorType.Int32, bit: 87)]
    UNIT_RANGED_ATTACK_POWER_MOD_NEG,
    [DescriptorCreateField(nameof(UnitData.RangedAttackPowerMultiplier), DescriptorType.Float, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.RangedAttackPowerMultiplier), DescriptorType.Float, bit: 88)]
    UNIT_RANGED_ATTACK_POWER_MULTIPLIER,

    [DescriptorCreatePlaceholder(DescriptorType.Int32, OwnerOnly = true)]
    UNIT_PAD_INT32_OWNER,
    [DescriptorCreatePlaceholder(DescriptorType.Float, OwnerOnly = true)]
    UNIT_PAD_FLOAT_OWNER,

    [DescriptorCreateField(nameof(UnitData.MinRangedDamage), DescriptorType.Float, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.MinRangedDamage), DescriptorType.Float, bit: 91)]
    UNIT_MIN_RANGED_DAMAGE,
    [DescriptorCreateField(nameof(UnitData.MaxRangedDamage), DescriptorType.Float, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(UnitData.MaxRangedDamage), DescriptorType.Float, bit: 92)]
    UNIT_MAX_RANGED_DAMAGE,
    [DescriptorCreateField(nameof(UnitData.MaxHealthModifier), DescriptorType.Float, OwnerOnly = true, DefaultExpression = "1f")]
    [DescriptorUpdateField(nameof(UnitData.MaxHealthModifier), DescriptorType.Float, bit: 93)]
    UNIT_MAX_HEALTH_MODIFIER,

    // ---- HoverHeight / MinItemLevel etc. ----

    [DescriptorCreateField(nameof(UnitData.HoverHeight), DescriptorType.Float)]
    [DescriptorUpdateField(nameof(UnitData.HoverHeight), DescriptorType.Float, bit: 94)]
    UNIT_HOVER_HEIGHT,
    [DescriptorCreateField(nameof(UnitData.MinItemLevelCutoff), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.MinItemLevelCutoff), DescriptorType.Int32, bit: 95)]
    UNIT_MIN_ITEM_LEVEL_CUTOFF,
    [DescriptorCreateField(nameof(UnitData.MinItemLevel), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.MinItemLevel), DescriptorType.Int32, bit: 97)]
    UNIT_MIN_ITEM_LEVEL,
    [DescriptorCreateField(nameof(UnitData.MaxItemLevel), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.MaxItemLevel), DescriptorType.Int32, bit: 98)]
    UNIT_MAX_ITEM_LEVEL,
    [DescriptorCreateField(nameof(UnitData.WildBattlePetLevel), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.WildBattlePetLevel), DescriptorType.Int32, bit: 99)]
    UNIT_WILD_BATTLE_PET_LEVEL,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_12,

    [DescriptorCreateField(nameof(UnitData.InteractSpellID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.InteractSpellID), DescriptorType.Int32, bit: 101)]
    UNIT_INTERACT_SPELL_ID,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    UNIT_PAD_13,

    [DescriptorCreateField(nameof(UnitData.LooksLikeMountID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.LooksLikeMountID), DescriptorType.Int32, bit: 103)]
    UNIT_LOOKS_LIKE_MOUNT_ID,
    [DescriptorCreateField(nameof(UnitData.LooksLikeCreatureID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.LooksLikeCreatureID), DescriptorType.Int32, bit: 104)]
    UNIT_LOOKS_LIKE_CREATURE_ID,
    [DescriptorCreateField(nameof(UnitData.LookAtControllerID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(UnitData.LookAtControllerID), DescriptorType.Int32, bit: 105)]
    UNIT_LOOK_AT_CONTROLLER_ID,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    UNIT_PAD_14,

    [DescriptorCreateField(nameof(UnitData.GuildGUID), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(UnitData.GuildGUID), DescriptorType.PackedGuid128, bit: 107)]
    UNIT_GUILD_GUID,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_PASSIVE_SPELLS_COUNT,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_WORLD_EFFECTS_COUNT,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitChannelObjectsCount))]
    UNIT_CHANNEL_OBJECTS_COUNT_CUSTOM,

    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128)]
    UNIT_PAD_GUID_4,

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    UNIT_PAD_15,
    [DescriptorCreatePlaceholder(DescriptorType.Float)]
    UNIT_PAD_FLOAT_ZERO,
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    UNIT_PAD_16,

    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128, OwnerOnly = true)]
    UNIT_PAD_GUID_OWNER,

    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128, CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteCreateUnitChannelObjectsBody))]
    UNIT_CHANNEL_OBJECTS_BODY_CUSTOM,

    // ============================================================
    // Update-only members — no Create-path attribute. Position in enum is arbitrary
    // (generator sorts Update writes by Bit ascending). Includes scalar fields that
    // don't surface in Create + synthetic group writers + the ChannelObjects dynamic
    // preamble + MaskOnly arrays whose write happens inside a group writer.
    // ============================================================

    // ChannelObjects: bit 4. Dynamic field with TWO emit phases:
    //   1. MaskPreamble between blocks-mask prefix + FlushBits (size + per-element bitmask).
    //   2. Body write at bit 4 position in block-0 payload (the actual PackedGuid128).
    // MaskPreamble does not set bits; the scalar UpdateField on ChannelObject (below)
    // sets bit 4 in Pass-1 via its CustomPredicate.
    [DescriptorMaskPreamble(bit: 4, customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitChannelObjectsMaskPreamble))]
    UNIT_CHANNEL_OBJECTS_PREAMBLE_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.ChannelObject), DescriptorType.PackedGuid128, bit: 4,
                           CustomPredicate = "src.ChannelObject.HasValue && !src.ChannelObject.Value.IsEmpty()",
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitChannelObjectsBody))]
    UNIT_CHANNEL_OBJECTS_BODY_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.Flags3), DescriptorType.UInt32, bit: 43)]
    UNIT_FLAGS3_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.OverrideDisplayPowerID), DescriptorType.UInt32, bit: 45)]
    UNIT_OVERRIDE_DISPLAY_POWER_ID_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.DisplayScale), DescriptorType.Float, bit: 48)]
    UNIT_DISPLAY_SCALE_UPDATE,
    [DescriptorUpdateField(nameof(UnitData.NativeXDisplayScale), DescriptorType.Float, bit: 50)]
    UNIT_NATIVE_X_DISPLAY_SCALE_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.ModHaste), DescriptorType.Float, bit: 67)]
    UNIT_MOD_HASTE_UPDATE,
    [DescriptorUpdateField(nameof(UnitData.ModRangedHaste), DescriptorType.Float, bit: 68)]
    UNIT_MOD_RANGED_HASTE_UPDATE,
    [DescriptorUpdateField(nameof(UnitData.ModHasteRegen), DescriptorType.Float, bit: 69)]
    UNIT_MOD_HASTE_REGEN_UPDATE,
    [DescriptorUpdateField(nameof(UnitData.ModTimeRate), DescriptorType.Float, bit: 70)]
    UNIT_MOD_TIME_RATE_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.TrainingPointsUsed), DescriptorType.UInt16, bit: 73)]
    UNIT_TRAINING_POINTS_USED_UPDATE,
    [DescriptorUpdateField(nameof(UnitData.TrainingPointsTotal), DescriptorType.UInt16, bit: 74)]
    UNIT_TRAINING_POINTS_TOTAL_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.AttackSpeedAura), DescriptorType.Int32, bit: 89)]
    UNIT_ATTACK_SPEED_AURA_UPDATE,
    [DescriptorUpdateField(nameof(UnitData.Lifesteal), DescriptorType.Float, bit: 90)]
    UNIT_LIFESTEAL_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.ScaleDuration), DescriptorType.Int32, bit: 102)]
    UNIT_SCALE_DURATION_UPDATE,

    [DescriptorUpdateField(nameof(UnitData.ComboTarget), DescriptorType.PackedGuid128, bit: 112)]
    UNIT_COMBO_TARGET_UPDATE,

    // ============================================================
    // Interleaved Update group writers — pair of (MaskOnly arrays + WriteOnly CustomField).
    // ============================================================

    // -------- Power group at bit 116 --------
    [DescriptorUpdateField(nameof(UnitData.Power), DescriptorType.Int32, bit: 137,
                           ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 116, MaskOnly = true)]
    UNIT_POWER_UPDATE_MASKONLY,
    [DescriptorUpdateField(nameof(UnitData.MaxPower), DescriptorType.Int32, bit: 147,
                           ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 116, MaskOnly = true)]
    UNIT_MAX_POWER_UPDATE_MASKONLY,
    [DescriptorUpdateField(nameof(UnitData.ModPowerRegen), DescriptorType.Float, bit: 157,
                           ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 116, MaskOnly = true)]
    UNIT_MOD_POWER_REGEN_UPDATE_MASKONLY,
    [DescriptorCustomField("UnitPowerGroup", bit: 116, customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitPowerGroup), WriteOnly = true)]
    UNIT_POWER_GROUP_UPDATE,

    // -------- Stats group at bit 174 --------
    [DescriptorUpdateField(nameof(UnitData.Stats), DescriptorType.Int32, bit: 175,
                           ArrayCount = 5, ArrayMode = ArrayMode.PerElement, ParentBit = 174, MaskOnly = true)]
    UNIT_STATS_UPDATE_MASKONLY,
    [DescriptorUpdateField(nameof(UnitData.StatPosBuff), DescriptorType.Int32, bit: 180,
                           ArrayCount = 5, ArrayMode = ArrayMode.PerElement, ParentBit = 174, MaskOnly = true)]
    UNIT_STAT_POS_BUFF_UPDATE_MASKONLY,
    [DescriptorUpdateField(nameof(UnitData.StatNegBuff), DescriptorType.Int32, bit: 185,
                           ArrayCount = 5, ArrayMode = ArrayMode.PerElement, ParentBit = 174, MaskOnly = true)]
    UNIT_STAT_NEG_BUFF_UPDATE_MASKONLY,
    [DescriptorCustomField("UnitStatsGroup", bit: 174, customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitStatsGroup), WriteOnly = true)]
    UNIT_STATS_GROUP_UPDATE,

    // -------- Resistances group at bit 190 --------
    [DescriptorUpdateField(nameof(UnitData.Resistances), DescriptorType.Int32, bit: 191,
                           ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 190, MaskOnly = true)]
    UNIT_RESISTANCES_UPDATE_MASKONLY,
    [DescriptorUpdateField(nameof(UnitData.PowerCostModifier), DescriptorType.Int32, bit: 198,
                           ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 190, MaskOnly = true)]
    UNIT_POWER_COST_MODIFIER_UPDATE_MASKONLY,
    [DescriptorUpdateField(nameof(UnitData.PowerCostMultiplier), DescriptorType.Float, bit: 205,
                           ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 190, MaskOnly = true)]
    UNIT_POWER_COST_MULTIPLIER_UPDATE_MASKONLY,
    [DescriptorCustomField("UnitResistancesGroup", bit: 190, customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitResistancesGroup), WriteOnly = true)]
    UNIT_RESISTANCES_GROUP_UPDATE,

    // -------- ResistanceBuffMods group at bit 212 --------
    [DescriptorUpdateField(nameof(UnitData.ResistanceBuffModsPositive), DescriptorType.Int32, bit: 213,
                           ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 212, MaskOnly = true)]
    UNIT_RES_BUFF_MODS_POS_UPDATE_MASKONLY,
    [DescriptorUpdateField(nameof(UnitData.ResistanceBuffModsNegative), DescriptorType.Int32, bit: 220,
                           ArrayCount = 7, ArrayMode = ArrayMode.PerElement, ParentBit = 212, MaskOnly = true)]
    UNIT_RES_BUFF_MODS_NEG_UPDATE_MASKONLY,
    [DescriptorCustomField("UnitResistanceBuffModsGroup", bit: 212, customWriter: nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteUpdateUnitResistanceBuffModsGroup), WriteOnly = true)]
    UNIT_RES_BUFF_MODS_GROUP_UPDATE,
}
