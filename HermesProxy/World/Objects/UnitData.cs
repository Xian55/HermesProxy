using System;

namespace HermesProxy.World.Objects;

public struct UnitChannel
{
    public int SpellID;
    public int SpellXSpellVisualID;

    public UnitChannel(int spellId, int spellXSpellVisualId)
    {
        SpellID = spellId;
        SpellXSpellVisualID = spellXSpellVisualId;
    }
}

/// <summary>
/// The two NPC-flag slots, stored inside <see cref="UnitData"/>. An inline array so a span over it
/// reads and writes the object's own memory, with no array to allocate.
/// </summary>
[System.Runtime.CompilerServices.InlineArray(2)]
public struct NpcFlagsStorage
{
    private uint? _slot0;
}

public struct VisibleItem
{
    public int ItemID;
    public ushort ItemAppearanceModID;
    public ushort ItemVisual;

    public VisibleItem(int itemId, ushort itemAppearanceModId, ushort itemVisual)
    {
        ItemID = itemId;
        ItemAppearanceModID = itemAppearanceModId;
        ItemVisual = itemVisual;
    }
}

public class UnitData
{
    // An array nothing has written yet reads as one of these shared, all-null stand-ins. They are
    // only ever exposed as ReadOnlySpan, so nothing can write through them, and a creature's
    // Values delta no longer allocates fourteen arrays it never touches (~900 B per object).
    // Writers go through the Ensure* accessors, which materialise the real array on first use.
    private static readonly int?[] _emptyInt7 = new int?[7];
    private static readonly int?[] _emptyInt5 = new int?[5];
    private static readonly float?[] _emptyFloat7 = new float?[7];
    private static readonly uint?[] _emptyUInt2 = new uint?[2];
    private static readonly VisibleItem?[] _emptyVisibleItem3 = new VisibleItem?[3];

    // The fields below are the ones a Values block actually carries. In an AV sniff (2026-09-19)
    // health, target, flags, aura state, cast speed, mount, the bytes-1 fields and the power, npc
    // flag and attack-time arrays covered nearly every unit and player Values block, while each of
    // the other ~100 appeared in under 1% of them. Those live in UnitDataCold, allocated on the
    // first write, so a typical delta costs ~200 B here instead of 1.1 KB. The properties read
    // as null until then, which is what every reader already treats as "not sent".
    public WowGuid128? Target;
    public long? Health;
    private int?[]? _power;
    public ReadOnlySpan<int?> Power => _power ?? _emptyInt7;
    public int?[] EnsurePower() => _power ??= new int?[7];
    public long? MaxHealth;
    private int?[]? _maxPower;
    public ReadOnlySpan<int?> MaxPower => _maxPower ?? _emptyInt7;
    public int?[] EnsureMaxPower() => _maxPower ??= new int?[7];
    public uint? Flags;
    public uint? AuraState;
    private uint?[]? _attackRoundBaseTime;
    public ReadOnlySpan<uint?> AttackRoundBaseTime => _attackRoundBaseTime ?? _emptyUInt2;
    public uint?[] EnsureAttackRoundBaseTime() => _attackRoundBaseTime ??= new uint?[2];
    public int? MountDisplayID;
    public byte? StandState;
    public byte? PetLoyaltyIndex;
    public byte? VisFlags;
    public byte? AnimTier;
    public float? ModCastSpeed;
    // Inline rather than a lazily allocated array like its neighbours: AzerothCore sends
    // UNIT_NPC_FLAGS in 100% of unit and player Values blocks, so the array was allocated for
    // nearly every block (~5 MB over an Alterac Valley). Sixteen bytes in the object beat a
    // 40-byte array plus its reference whenever the field is set more often than it is skipped.
    private NpcFlagsStorage _npcFlags;
    public ReadOnlySpan<uint?> NpcFlags => _npcFlags;
    public Span<uint?> EnsureNpcFlags() => _npcFlags;

    private UnitDataCold? _cold;
    private UnitDataCold Cold => _cold ??= new UnitDataCold();

    // Setting null on an object that has no cold half has nothing to clear, so it allocates nothing.
    public WowGuid128? Charm { get => _cold?.Charm; set { if (value.HasValue || _cold != null) Cold.Charm = value; } }
    public WowGuid128? Summon { get => _cold?.Summon; set { if (value.HasValue || _cold != null) Cold.Summon = value; } }
    public WowGuid128? Critter { get => _cold?.Critter; set { if (value.HasValue || _cold != null) Cold.Critter = value; } }
    public WowGuid128? CharmedBy { get => _cold?.CharmedBy; set { if (value.HasValue || _cold != null) Cold.CharmedBy = value; } }
    public WowGuid128? SummonedBy { get => _cold?.SummonedBy; set { if (value.HasValue || _cold != null) Cold.SummonedBy = value; } }
    public WowGuid128? CreatedBy { get => _cold?.CreatedBy; set { if (value.HasValue || _cold != null) Cold.CreatedBy = value; } }
    public WowGuid128? DemonCreator { get => _cold?.DemonCreator; set { if (value.HasValue || _cold != null) Cold.DemonCreator = value; } }
    public WowGuid128? LookAtControllerTarget { get => _cold?.LookAtControllerTarget; set { if (value.HasValue || _cold != null) Cold.LookAtControllerTarget = value; } }
    public WowGuid128? BattlePetCompanionGUID { get => _cold?.BattlePetCompanionGUID; set { if (value.HasValue || _cold != null) Cold.BattlePetCompanionGUID = value; } }
    public ulong? BattlePetDBID { get => _cold?.BattlePetDBID; set { if (value.HasValue || _cold != null) Cold.BattlePetDBID = value; } }
    public UnitChannel? ChannelData { get => _cold?.ChannelData; set { if (value.HasValue || _cold != null) Cold.ChannelData = value; } }
    public uint? SummonedByHomeRealm { get => _cold?.SummonedByHomeRealm; set { if (value.HasValue || _cold != null) Cold.SummonedByHomeRealm = value; } }
    public byte? RaceId { get => _cold?.RaceId; set { if (value.HasValue || _cold != null) Cold.RaceId = value; } }
    public byte? ClassId { get => _cold?.ClassId; set { if (value.HasValue || _cold != null) Cold.ClassId = value; } }
    public byte? PlayerClassId { get => _cold?.PlayerClassId; set { if (value.HasValue || _cold != null) Cold.PlayerClassId = value; } }
    public byte? SexId { get => _cold?.SexId; set { if (value.HasValue || _cold != null) Cold.SexId = value; } }
    public uint? DisplayPower { get => _cold?.DisplayPower; set { if (value.HasValue || _cold != null) Cold.DisplayPower = value; } }
    public uint? OverrideDisplayPowerID { get => _cold?.OverrideDisplayPowerID; set { if (value.HasValue || _cold != null) Cold.OverrideDisplayPowerID = value; } }
    public ReadOnlySpan<float?> ModPowerRegen => _cold?.ModPowerRegen ?? _emptyFloat7;
    public float?[] EnsureModPowerRegen() => Cold.ModPowerRegen ??= new float?[7];
    public int? Level { get => _cold?.Level; set { if (value.HasValue || _cold != null) Cold.Level = value; } }
    public int? EffectiveLevel { get => _cold?.EffectiveLevel; set { if (value.HasValue || _cold != null) Cold.EffectiveLevel = value; } }
    public int? ContentTuningID { get => _cold?.ContentTuningID; set { if (value.HasValue || _cold != null) Cold.ContentTuningID = value; } }
    public int? ScalingLevelMin { get => _cold?.ScalingLevelMin; set { if (value.HasValue || _cold != null) Cold.ScalingLevelMin = value; } }
    public int? ScalingLevelMax { get => _cold?.ScalingLevelMax; set { if (value.HasValue || _cold != null) Cold.ScalingLevelMax = value; } }
    public int? ScalingLevelDelta { get => _cold?.ScalingLevelDelta; set { if (value.HasValue || _cold != null) Cold.ScalingLevelDelta = value; } }
    public int? ScalingFactionGroup { get => _cold?.ScalingFactionGroup; set { if (value.HasValue || _cold != null) Cold.ScalingFactionGroup = value; } }
    public int? ScalingHealthItemLevelCurveID { get => _cold?.ScalingHealthItemLevelCurveID; set { if (value.HasValue || _cold != null) Cold.ScalingHealthItemLevelCurveID = value; } }
    public int? ScalingDamageItemLevelCurveID { get => _cold?.ScalingDamageItemLevelCurveID; set { if (value.HasValue || _cold != null) Cold.ScalingDamageItemLevelCurveID = value; } }
    public int? FactionTemplate { get => _cold?.FactionTemplate; set { if (value.HasValue || _cold != null) Cold.FactionTemplate = value; } }
    public ReadOnlySpan<VisibleItem?> VirtualItems => _cold?.VirtualItems ?? _emptyVisibleItem3;
    public VisibleItem?[] EnsureVirtualItems() => Cold.VirtualItems ??= new VisibleItem?[3];
    public uint? Flags2 { get => _cold?.Flags2; set { if (value.HasValue || _cold != null) Cold.Flags2 = value; } }
    public uint? Flags3 { get => _cold?.Flags3; set { if (value.HasValue || _cold != null) Cold.Flags3 = value; } }
    public uint? RangedAttackRoundBaseTime { get => _cold?.RangedAttackRoundBaseTime; set { if (value.HasValue || _cold != null) Cold.RangedAttackRoundBaseTime = value; } }
    public float? BoundingRadius { get => _cold?.BoundingRadius; set { if (value.HasValue || _cold != null) Cold.BoundingRadius = value; } }
    public float? CombatReach { get => _cold?.CombatReach; set { if (value.HasValue || _cold != null) Cold.CombatReach = value; } }
    public int? DisplayID { get => _cold?.DisplayID; set { if (value.HasValue || _cold != null) Cold.DisplayID = value; } }
    public float? DisplayScale { get => _cold?.DisplayScale; set { if (value.HasValue || _cold != null) Cold.DisplayScale = value; } }
    public int? NativeDisplayID { get => _cold?.NativeDisplayID; set { if (value.HasValue || _cold != null) Cold.NativeDisplayID = value; } }
    public float? NativeXDisplayScale { get => _cold?.NativeXDisplayScale; set { if (value.HasValue || _cold != null) Cold.NativeXDisplayScale = value; } }
    public float? MinDamage { get => _cold?.MinDamage; set { if (value.HasValue || _cold != null) Cold.MinDamage = value; } }
    public float? MaxDamage { get => _cold?.MaxDamage; set { if (value.HasValue || _cold != null) Cold.MaxDamage = value; } }
    public float? MinOffHandDamage { get => _cold?.MinOffHandDamage; set { if (value.HasValue || _cold != null) Cold.MinOffHandDamage = value; } }
    public float? MaxOffHandDamage { get => _cold?.MaxOffHandDamage; set { if (value.HasValue || _cold != null) Cold.MaxOffHandDamage = value; } }
    public uint? PetNumber { get => _cold?.PetNumber; set { if (value.HasValue || _cold != null) Cold.PetNumber = value; } }
    public uint? PetNameTimestamp { get => _cold?.PetNameTimestamp; set { if (value.HasValue || _cold != null) Cold.PetNameTimestamp = value; } }
    public uint? PetExperience { get => _cold?.PetExperience; set { if (value.HasValue || _cold != null) Cold.PetExperience = value; } }
    public uint? PetNextLevelExperience { get => _cold?.PetNextLevelExperience; set { if (value.HasValue || _cold != null) Cold.PetNextLevelExperience = value; } }
    public float? ModCastHaste { get => _cold?.ModCastHaste; set { if (value.HasValue || _cold != null) Cold.ModCastHaste = value; } }
    public float? ModHaste { get => _cold?.ModHaste; set { if (value.HasValue || _cold != null) Cold.ModHaste = value; } }
    public float? ModRangedHaste { get => _cold?.ModRangedHaste; set { if (value.HasValue || _cold != null) Cold.ModRangedHaste = value; } }
    public float? ModHasteRegen { get => _cold?.ModHasteRegen; set { if (value.HasValue || _cold != null) Cold.ModHasteRegen = value; } }
    public float? ModTimeRate { get => _cold?.ModTimeRate; set { if (value.HasValue || _cold != null) Cold.ModTimeRate = value; } }
    public int? CreatedBySpell { get => _cold?.CreatedBySpell; set { if (value.HasValue || _cold != null) Cold.CreatedBySpell = value; } }
    public int? EmoteState { get => _cold?.EmoteState; set { if (value.HasValue || _cold != null) Cold.EmoteState = value; } }
    public ushort? TrainingPointsUsed { get => _cold?.TrainingPointsUsed; set { if (value.HasValue || _cold != null) Cold.TrainingPointsUsed = value; } }
    public ushort? TrainingPointsTotal { get => _cold?.TrainingPointsTotal; set { if (value.HasValue || _cold != null) Cold.TrainingPointsTotal = value; } }
    public ReadOnlySpan<int?> Stats => _cold?.Stats ?? _emptyInt5;
    public int?[] EnsureStats() => Cold.Stats ??= new int?[5];
    public ReadOnlySpan<int?> StatPosBuff => _cold?.StatPosBuff ?? _emptyInt5;
    public int?[] EnsureStatPosBuff() => Cold.StatPosBuff ??= new int?[5];
    public ReadOnlySpan<int?> StatNegBuff => _cold?.StatNegBuff ?? _emptyInt5;
    public int?[] EnsureStatNegBuff() => Cold.StatNegBuff ??= new int?[5];
    public ReadOnlySpan<int?> Resistances => _cold?.Resistances ?? _emptyInt7;
    public int?[] EnsureResistances() => Cold.Resistances ??= new int?[7];
    public ReadOnlySpan<int?> ResistanceBuffModsPositive => _cold?.ResistanceBuffModsPositive ?? _emptyInt7;
    public int?[] EnsureResistanceBuffModsPositive() => Cold.ResistanceBuffModsPositive ??= new int?[7];
    public ReadOnlySpan<int?> ResistanceBuffModsNegative => _cold?.ResistanceBuffModsNegative ?? _emptyInt7;
    public int?[] EnsureResistanceBuffModsNegative() => Cold.ResistanceBuffModsNegative ??= new int?[7];
    public int? BaseMana { get => _cold?.BaseMana; set { if (value.HasValue || _cold != null) Cold.BaseMana = value; } }
    public int? BaseHealth { get => _cold?.BaseHealth; set { if (value.HasValue || _cold != null) Cold.BaseHealth = value; } }
    public byte? SheatheState { get => _cold?.SheatheState; set { if (value.HasValue || _cold != null) Cold.SheatheState = value; } }
    public byte? PvpFlags { get => _cold?.PvpFlags; set { if (value.HasValue || _cold != null) Cold.PvpFlags = value; } }
    public byte? PetFlags { get => _cold?.PetFlags; set { if (value.HasValue || _cold != null) Cold.PetFlags = value; } }
    public byte? ShapeshiftForm { get => _cold?.ShapeshiftForm; set { if (value.HasValue || _cold != null) Cold.ShapeshiftForm = value; } }
    public int? AttackPower { get => _cold?.AttackPower; set { if (value.HasValue || _cold != null) Cold.AttackPower = value; } }
    public int? AttackPowerModPos { get => _cold?.AttackPowerModPos; set { if (value.HasValue || _cold != null) Cold.AttackPowerModPos = value; } }
    public int? AttackPowerModNeg { get => _cold?.AttackPowerModNeg; set { if (value.HasValue || _cold != null) Cold.AttackPowerModNeg = value; } }
    public float? AttackPowerMultiplier { get => _cold?.AttackPowerMultiplier; set { if (value.HasValue || _cold != null) Cold.AttackPowerMultiplier = value; } }
    public int? RangedAttackPower { get => _cold?.RangedAttackPower; set { if (value.HasValue || _cold != null) Cold.RangedAttackPower = value; } }
    public int? RangedAttackPowerModPos { get => _cold?.RangedAttackPowerModPos; set { if (value.HasValue || _cold != null) Cold.RangedAttackPowerModPos = value; } }
    public int? RangedAttackPowerModNeg { get => _cold?.RangedAttackPowerModNeg; set { if (value.HasValue || _cold != null) Cold.RangedAttackPowerModNeg = value; } }
    public float? RangedAttackPowerMultiplier { get => _cold?.RangedAttackPowerMultiplier; set { if (value.HasValue || _cold != null) Cold.RangedAttackPowerMultiplier = value; } }
    public int? AttackSpeedAura { get => _cold?.AttackSpeedAura; set { if (value.HasValue || _cold != null) Cold.AttackSpeedAura = value; } }
    public float? Lifesteal { get => _cold?.Lifesteal; set { if (value.HasValue || _cold != null) Cold.Lifesteal = value; } }
    public float? MinRangedDamage { get => _cold?.MinRangedDamage; set { if (value.HasValue || _cold != null) Cold.MinRangedDamage = value; } }
    public float? MaxRangedDamage { get => _cold?.MaxRangedDamage; set { if (value.HasValue || _cold != null) Cold.MaxRangedDamage = value; } }
    public ReadOnlySpan<int?> PowerCostModifier => _cold?.PowerCostModifier ?? _emptyInt7;
    public int?[] EnsurePowerCostModifier() => Cold.PowerCostModifier ??= new int?[7];
    public ReadOnlySpan<float?> PowerCostMultiplier => _cold?.PowerCostMultiplier ?? _emptyFloat7;
    public float?[] EnsurePowerCostMultiplier() => Cold.PowerCostMultiplier ??= new float?[7];
    public float? MaxHealthModifier { get => _cold?.MaxHealthModifier; set { if (value.HasValue || _cold != null) Cold.MaxHealthModifier = value; } }
    public float? HoverHeight { get => _cold?.HoverHeight; set { if (value.HasValue || _cold != null) Cold.HoverHeight = value; } }
    public int? MinItemLevelCutoff { get => _cold?.MinItemLevelCutoff; set { if (value.HasValue || _cold != null) Cold.MinItemLevelCutoff = value; } }
    public int? MinItemLevel { get => _cold?.MinItemLevel; set { if (value.HasValue || _cold != null) Cold.MinItemLevel = value; } }
    public int? MaxItemLevel { get => _cold?.MaxItemLevel; set { if (value.HasValue || _cold != null) Cold.MaxItemLevel = value; } }
    public int? WildBattlePetLevel { get => _cold?.WildBattlePetLevel; set { if (value.HasValue || _cold != null) Cold.WildBattlePetLevel = value; } }
    public uint? BattlePetCompanionNameTimestamp { get => _cold?.BattlePetCompanionNameTimestamp; set { if (value.HasValue || _cold != null) Cold.BattlePetCompanionNameTimestamp = value; } }
    public int? InteractSpellID { get => _cold?.InteractSpellID; set { if (value.HasValue || _cold != null) Cold.InteractSpellID = value; } }
    public uint? StateSpellVisualID { get => _cold?.StateSpellVisualID; set { if (value.HasValue || _cold != null) Cold.StateSpellVisualID = value; } }
    public uint? StateAnimID { get => _cold?.StateAnimID; set { if (value.HasValue || _cold != null) Cold.StateAnimID = value; } }
    public uint? StateAnimKitID { get => _cold?.StateAnimKitID; set { if (value.HasValue || _cold != null) Cold.StateAnimKitID = value; } }
    public uint? StateWorldEffectsID { get => _cold?.StateWorldEffectsID; set { if (value.HasValue || _cold != null) Cold.StateWorldEffectsID = value; } }
    public int? ScaleDuration { get => _cold?.ScaleDuration; set { if (value.HasValue || _cold != null) Cold.ScaleDuration = value; } }
    public int? LooksLikeMountID { get => _cold?.LooksLikeMountID; set { if (value.HasValue || _cold != null) Cold.LooksLikeMountID = value; } }
    public int? LooksLikeCreatureID { get => _cold?.LooksLikeCreatureID; set { if (value.HasValue || _cold != null) Cold.LooksLikeCreatureID = value; } }
    public int? LookAtControllerID { get => _cold?.LookAtControllerID; set { if (value.HasValue || _cold != null) Cold.LookAtControllerID = value; } }
    public WowGuid128? GuildGUID { get => _cold?.GuildGUID; set { if (value.HasValue || _cold != null) Cold.GuildGUID = value; } }
    public WowGuid128? ComboTarget { get => _cold?.ComboTarget; set { if (value.HasValue || _cold != null) Cold.ComboTarget = value; } }
    public WowGuid128? ChannelObject { get => _cold?.ChannelObject; set { if (value.HasValue || _cold != null) Cold.ChannelObject = value; } }
}

/// <summary>The rarely-sent part of <see cref="UnitData"/>; see the note there.</summary>
internal sealed class UnitDataCold
{
    public WowGuid128? Charm;
    public WowGuid128? Summon;
    public WowGuid128? Critter;
    public WowGuid128? CharmedBy;
    public WowGuid128? SummonedBy;
    public WowGuid128? CreatedBy;
    public WowGuid128? DemonCreator;
    public WowGuid128? LookAtControllerTarget;
    public WowGuid128? BattlePetCompanionGUID;
    public ulong? BattlePetDBID;
    public UnitChannel? ChannelData;
    public uint? SummonedByHomeRealm;
    public byte? RaceId;
    public byte? ClassId;
    public byte? PlayerClassId;
    public byte? SexId;
    public uint? DisplayPower;
    public uint? OverrideDisplayPowerID;
    public float?[]? ModPowerRegen;
    public int? Level;
    public int? EffectiveLevel;
    public int? ContentTuningID;
    public int? ScalingLevelMin;
    public int? ScalingLevelMax;
    public int? ScalingLevelDelta;
    public int? ScalingFactionGroup;
    public int? ScalingHealthItemLevelCurveID;
    public int? ScalingDamageItemLevelCurveID;
    public int? FactionTemplate;
    public VisibleItem?[]? VirtualItems;
    public uint? Flags2;
    public uint? Flags3;
    public uint? RangedAttackRoundBaseTime;
    public float? BoundingRadius;
    public float? CombatReach;
    public int? DisplayID;
    public float? DisplayScale;
    public int? NativeDisplayID;
    public float? NativeXDisplayScale;
    public float? MinDamage;
    public float? MaxDamage;
    public float? MinOffHandDamage;
    public float? MaxOffHandDamage;
    public uint? PetNumber;
    public uint? PetNameTimestamp;
    public uint? PetExperience;
    public uint? PetNextLevelExperience;
    public float? ModCastHaste;
    public float? ModHaste;
    public float? ModRangedHaste;
    public float? ModHasteRegen;
    public float? ModTimeRate;
    public int? CreatedBySpell;
    public int? EmoteState;
    public ushort? TrainingPointsUsed;
    public ushort? TrainingPointsTotal;
    public int?[]? Stats;
    public int?[]? StatPosBuff;
    public int?[]? StatNegBuff;
    public int?[]? Resistances;
    public int?[]? ResistanceBuffModsPositive;
    public int?[]? ResistanceBuffModsNegative;
    public int? BaseMana;
    public int? BaseHealth;
    public byte? SheatheState;
    public byte? PvpFlags;
    public byte? PetFlags;
    public byte? ShapeshiftForm;
    public int? AttackPower;
    public int? AttackPowerModPos;
    public int? AttackPowerModNeg;
    public float? AttackPowerMultiplier;
    public int? RangedAttackPower;
    public int? RangedAttackPowerModPos;
    public int? RangedAttackPowerModNeg;
    public float? RangedAttackPowerMultiplier;
    public int? AttackSpeedAura;
    public float? Lifesteal;
    public float? MinRangedDamage;
    public float? MaxRangedDamage;
    public int?[]? PowerCostModifier;
    public float?[]? PowerCostMultiplier;
    public float? MaxHealthModifier;
    public float? HoverHeight;
    public int? MinItemLevelCutoff;
    public int? MinItemLevel;
    public int? MaxItemLevel;
    public int? WildBattlePetLevel;
    public uint? BattlePetCompanionNameTimestamp;
    public int? InteractSpellID;
    public uint? StateSpellVisualID;
    public uint? StateAnimID;
    public uint? StateAnimKitID;
    public uint? StateWorldEffectsID;
    public int? ScaleDuration;
    public int? LooksLikeMountID;
    public int? LooksLikeCreatureID;
    public int? LookAtControllerID;
    public WowGuid128? GuildGUID;
    public WowGuid128? ComboTarget;
    public WowGuid128? ChannelObject;
}
