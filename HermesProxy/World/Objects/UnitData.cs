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

    /// <summary>
    /// True when any field in this block has been written.
    /// </summary>
    /// <remarks>
    /// The V3_4_3 Values filter drops a delta that says nothing — cMangos emits those as
    /// bookkeeping and the client answers the resulting 13-byte body with
    /// CMSG_OBJECT_UPDATE_FAILED — so this has to answer for every field. It lived in
    /// UpdatePackets.IsEmptyValuesDelta as a hand-picked list of 47, and the 69 fields that list
    /// left out were dropped whenever they arrived on their own: ShapeshiftForm, so a warrior who
    /// logged in in Battle Stance got the empty non-stance action bar (issue #300), and with it
    /// SheatheState, EmoteState, ComboTarget, the pet fields and every haste and attack-power mod.
    /// UnitDataProbeTests walks the field list by reflection so a field added later cannot be
    /// left out again.
    /// </remarks>
    public bool HasAnyValue()
    {
        // The fields a real delta most often carries, so the common case returns on the
        // first branches instead of walking the whole block.
        if (Health.HasValue || MaxHealth.HasValue || DisplayID.HasValue) return true;
        if (Flags.HasValue || Flags2.HasValue || Flags3.HasValue) return true;
        if (AuraState.HasValue || Level.HasValue || FactionTemplate.HasValue) return true;

        if (Charm.HasValue || Summon.HasValue || Critter.HasValue) return true;
        if (CharmedBy.HasValue || SummonedBy.HasValue || CreatedBy.HasValue) return true;
        if (DemonCreator.HasValue || LookAtControllerTarget.HasValue || Target.HasValue) return true;
        if (BattlePetCompanionGUID.HasValue || BattlePetDBID.HasValue || ChannelData.HasValue) return true;
        if (SummonedByHomeRealm.HasValue || RaceId.HasValue || ClassId.HasValue) return true;
        if (PlayerClassId.HasValue || SexId.HasValue || DisplayPower.HasValue) return true;
        if (OverrideDisplayPowerID.HasValue || EffectiveLevel.HasValue || ContentTuningID.HasValue) return true;
        if (ScalingLevelMin.HasValue || ScalingLevelMax.HasValue || ScalingLevelDelta.HasValue) return true;
        if (ScalingFactionGroup.HasValue || ScalingHealthItemLevelCurveID.HasValue || ScalingDamageItemLevelCurveID.HasValue) return true;
        if (RangedAttackRoundBaseTime.HasValue || BoundingRadius.HasValue || CombatReach.HasValue) return true;
        if (DisplayScale.HasValue || NativeDisplayID.HasValue || NativeXDisplayScale.HasValue) return true;
        if (MountDisplayID.HasValue || MinDamage.HasValue || MaxDamage.HasValue) return true;
        if (MinOffHandDamage.HasValue || MaxOffHandDamage.HasValue || StandState.HasValue) return true;
        if (PetLoyaltyIndex.HasValue || VisFlags.HasValue || AnimTier.HasValue) return true;
        if (PetNumber.HasValue || PetNameTimestamp.HasValue || PetExperience.HasValue) return true;
        if (PetNextLevelExperience.HasValue || ModCastSpeed.HasValue || ModCastHaste.HasValue) return true;
        if (ModHaste.HasValue || ModRangedHaste.HasValue || ModHasteRegen.HasValue) return true;
        if (ModTimeRate.HasValue || CreatedBySpell.HasValue || EmoteState.HasValue) return true;
        if (TrainingPointsUsed.HasValue || TrainingPointsTotal.HasValue || BaseMana.HasValue) return true;
        if (BaseHealth.HasValue || SheatheState.HasValue || PvpFlags.HasValue) return true;
        if (PetFlags.HasValue || ShapeshiftForm.HasValue || AttackPower.HasValue) return true;
        if (AttackPowerModPos.HasValue || AttackPowerModNeg.HasValue || AttackPowerMultiplier.HasValue) return true;
        if (RangedAttackPower.HasValue || RangedAttackPowerModPos.HasValue || RangedAttackPowerModNeg.HasValue) return true;
        if (RangedAttackPowerMultiplier.HasValue || AttackSpeedAura.HasValue || Lifesteal.HasValue) return true;
        if (MinRangedDamage.HasValue || MaxRangedDamage.HasValue || MaxHealthModifier.HasValue) return true;
        if (HoverHeight.HasValue || MinItemLevelCutoff.HasValue || MinItemLevel.HasValue) return true;
        if (MaxItemLevel.HasValue || WildBattlePetLevel.HasValue || BattlePetCompanionNameTimestamp.HasValue) return true;
        if (InteractSpellID.HasValue || StateSpellVisualID.HasValue || StateAnimID.HasValue) return true;
        if (StateAnimKitID.HasValue || StateWorldEffectsID.HasValue || ScaleDuration.HasValue) return true;
        if (LooksLikeMountID.HasValue || LooksLikeCreatureID.HasValue || LookAtControllerID.HasValue) return true;
        if (GuildGUID.HasValue || ComboTarget.HasValue || ChannelObject.HasValue) return true;

        for (int i = 0; i < Power.Length; i++)
            if (Power[i].HasValue) return true;
        for (int i = 0; i < MaxPower.Length; i++)
            if (MaxPower[i].HasValue) return true;
        for (int i = 0; i < ModPowerRegen.Length; i++)
            if (ModPowerRegen[i].HasValue) return true;
        for (int i = 0; i < VirtualItems.Length; i++)
            if (VirtualItems[i].HasValue) return true;
        for (int i = 0; i < AttackRoundBaseTime.Length; i++)
            if (AttackRoundBaseTime[i].HasValue) return true;
        // A zero NpcFlags slot is the server clearing a flag it never set. The probe has always
        // read that as nothing to say, and the flood it guards against is made of exactly those.
        for (int i = 0; i < NpcFlags.Length; i++)
            if (NpcFlags[i].HasValue && NpcFlags[i] != 0) return true;
        for (int i = 0; i < Stats.Length; i++)
            if (Stats[i].HasValue) return true;
        for (int i = 0; i < StatPosBuff.Length; i++)
            if (StatPosBuff[i].HasValue) return true;
        for (int i = 0; i < StatNegBuff.Length; i++)
            if (StatNegBuff[i].HasValue) return true;
        for (int i = 0; i < Resistances.Length; i++)
            if (Resistances[i].HasValue) return true;
        for (int i = 0; i < ResistanceBuffModsPositive.Length; i++)
            if (ResistanceBuffModsPositive[i].HasValue) return true;
        for (int i = 0; i < ResistanceBuffModsNegative.Length; i++)
            if (ResistanceBuffModsNegative[i].HasValue) return true;
        for (int i = 0; i < PowerCostModifier.Length; i++)
            if (PowerCostModifier[i].HasValue) return true;
        for (int i = 0; i < PowerCostMultiplier.Length; i++)
            if (PowerCostMultiplier[i].HasValue) return true;

        return false;
    }
}
