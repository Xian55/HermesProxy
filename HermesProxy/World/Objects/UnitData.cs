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

    public WowGuid128? Charm;
    public WowGuid128? Summon;
    public WowGuid128? Critter;
    public WowGuid128? CharmedBy;
    public WowGuid128? SummonedBy;
    public WowGuid128? CreatedBy;
    public WowGuid128? DemonCreator;
    public WowGuid128? LookAtControllerTarget;
    public WowGuid128? Target;
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
    public long? Health;
    private int?[]? _power;
    public ReadOnlySpan<int?> Power => _power ?? _emptyInt7;
    public int?[] EnsurePower() => _power ??= new int?[7];
    public long? MaxHealth;
    private int?[]? _maxPower;
    public ReadOnlySpan<int?> MaxPower => _maxPower ?? _emptyInt7;
    public int?[] EnsureMaxPower() => _maxPower ??= new int?[7];
    private float?[]? _modPowerRegen;
    public ReadOnlySpan<float?> ModPowerRegen => _modPowerRegen ?? _emptyFloat7;
    public float?[] EnsureModPowerRegen() => _modPowerRegen ??= new float?[7];
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
    private VisibleItem?[]? _virtualItems;
    public ReadOnlySpan<VisibleItem?> VirtualItems => _virtualItems ?? _emptyVisibleItem3;
    public VisibleItem?[] EnsureVirtualItems() => _virtualItems ??= new VisibleItem?[3];
    public uint? Flags;
    public uint? Flags2;
    public uint? Flags3;
    public uint? AuraState;
    private uint?[]? _attackRoundBaseTime;
    public ReadOnlySpan<uint?> AttackRoundBaseTime => _attackRoundBaseTime ?? _emptyUInt2;
    public uint?[] EnsureAttackRoundBaseTime() => _attackRoundBaseTime ??= new uint?[2];
    public uint? RangedAttackRoundBaseTime;
    public float? BoundingRadius;
    public float? CombatReach;
    public int? DisplayID;
    public float? DisplayScale;
    public int? NativeDisplayID;
    public float? NativeXDisplayScale;
    public int? MountDisplayID;
    public float? MinDamage;
    public float? MaxDamage;
    public float? MinOffHandDamage;
    public float? MaxOffHandDamage;
    public byte? StandState;
    public byte? PetLoyaltyIndex;
    public byte? VisFlags;
    public byte? AnimTier;
    public uint? PetNumber;
    public uint? PetNameTimestamp;
    public uint? PetExperience;
    public uint? PetNextLevelExperience;
    public float? ModCastSpeed;
    public float? ModCastHaste;
    public float? ModHaste;
    public float? ModRangedHaste;
    public float? ModHasteRegen;
    public float? ModTimeRate;
    public int? CreatedBySpell;
    private uint?[]? _npcFlags;
    public ReadOnlySpan<uint?> NpcFlags => _npcFlags ?? _emptyUInt2;
    public uint?[] EnsureNpcFlags() => _npcFlags ??= new uint?[2];
    public int? EmoteState;
    public ushort? TrainingPointsUsed;
    public ushort? TrainingPointsTotal;
    private int?[]? _stats;
    public ReadOnlySpan<int?> Stats => _stats ?? _emptyInt5;
    public int?[] EnsureStats() => _stats ??= new int?[5];
    private int?[]? _statPosBuff;
    public ReadOnlySpan<int?> StatPosBuff => _statPosBuff ?? _emptyInt5;
    public int?[] EnsureStatPosBuff() => _statPosBuff ??= new int?[5];
    private int?[]? _statNegBuff;
    public ReadOnlySpan<int?> StatNegBuff => _statNegBuff ?? _emptyInt5;
    public int?[] EnsureStatNegBuff() => _statNegBuff ??= new int?[5];
    private int?[]? _resistances;
    public ReadOnlySpan<int?> Resistances => _resistances ?? _emptyInt7;
    public int?[] EnsureResistances() => _resistances ??= new int?[7];
    private int?[]? _resistanceBuffModsPositive;
    public ReadOnlySpan<int?> ResistanceBuffModsPositive => _resistanceBuffModsPositive ?? _emptyInt7;
    public int?[] EnsureResistanceBuffModsPositive() => _resistanceBuffModsPositive ??= new int?[7];
    private int?[]? _resistanceBuffModsNegative;
    public ReadOnlySpan<int?> ResistanceBuffModsNegative => _resistanceBuffModsNegative ?? _emptyInt7;
    public int?[] EnsureResistanceBuffModsNegative() => _resistanceBuffModsNegative ??= new int?[7];
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
    private int?[]? _powerCostModifier;
    public ReadOnlySpan<int?> PowerCostModifier => _powerCostModifier ?? _emptyInt7;
    public int?[] EnsurePowerCostModifier() => _powerCostModifier ??= new int?[7];
    private float?[]? _powerCostMultiplier;
    public ReadOnlySpan<float?> PowerCostMultiplier => _powerCostMultiplier ?? _emptyFloat7;
    public float?[] EnsurePowerCostMultiplier() => _powerCostMultiplier ??= new float?[7];
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

    // Dynamic Fields
    public WowGuid128? ChannelObject;
}
