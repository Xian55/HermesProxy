using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects;

public class SkillInfo
{
    /// <summary>Skill slots in the client's descriptor. Every one of the seven parallel
    /// arrays below is this long, and the builders walk all of them by index.</summary>
    public const int MaxSkills = 256;

    public ushort?[] SkillLineID = new ushort?[MaxSkills];
    public ushort?[] SkillStep = new ushort?[MaxSkills];
    public ushort?[] SkillRank = new ushort?[MaxSkills];
    public ushort?[] SkillStartingRank = new ushort?[MaxSkills];
    public ushort?[] SkillMaxRank = new ushort?[MaxSkills];
    public short?[] SkillTempBonus = new short?[MaxSkills];
    public ushort?[] SkillPermBonus = new ushort?[MaxSkills];
}
public class RestInfo
{
    public uint? StateID;
    public uint? Threshold;
}
/// <summary>
/// One PvP bracket's standing. Mirrors the native V3_4_3 <c>UF::PVPInfo</c> field for field and
/// in the same order, which is also the order both descriptor writers emit.
/// </summary>
/// <remarks>
/// <see cref="Bracket"/> is the field the client keys on: its <c>GetPvpInfoForBracket</c> scans
/// the array for the element whose Bracket matches, rather than indexing it, so an element with
/// no Bracket set is unreachable no matter which slot it occupies. It is nullable because 0 is
/// the 2v2 bracket — a plain <c>!= 0</c> mask test would drop precisely the bracket most players
/// have a team in.
/// </remarks>
public class PVPInfo
{
    public sbyte? Bracket;
    public int PvpRatingID;
    public uint WeeklyPlayed;
    public uint WeeklyWon;
    public uint SeasonPlayed;
    public uint SeasonWon;
    public uint Rating;
    public uint WeeklyBestRating;
    public uint SeasonBestRating;
    public uint PvpTierID;
    public uint WeeklyBestWinPvpTierID;
    public uint Field_28;
    public uint Field_2C;
    public uint WeeklyRoundsPlayed;
    public uint WeeklyRoundsWon;
    public uint SeasonRoundsPlayed;
    public uint SeasonRoundsWon;
    public bool Disqualified;
}
public class ActivePlayerData
{
    /// <summary>Explored-zone bitmask words. 240 x 64 bits.</summary>
    public const int ExploredZoneWords = 240;

    /// <summary>InvSlots length in the client's descriptor. equipped gear and bags</summary>
    public const int InvSlotCount = 23;
    /// <summary>PackSlots length in the client's descriptor. main bag 16 default + 8 vulpera racial</summary>
    public const int PackSlotCount = 24;
    /// <summary>BankSlots length in the client's descriptor. only 24 in vanilla</summary>
    public const int BankSlotCount = 28;
    /// <summary>BankBagSlots length in the client's descriptor. only 6 in vanilla</summary>
    public const int BankBagSlotCount = 7;
    /// <summary>BuyBackSlots length in the client's descriptor.</summary>
    public const int BuyBackSlotCount = 12;
    /// <summary>KeyringSlots length in the client's descriptor.</summary>
    public const int KeyringSlotCount = 32;

    /// <summary>Completed-quest bitmask words. The descriptor field is 1750 uint32s, so 875
    /// uint64s = 56,000 quest bits — indexed by QuestV2.UniqueBitFlag, not by quest id.</summary>
    public const int QuestCompletedWords = 875;

    public WowGuid128?[]? InvSlots; // equipped gear and bags
    public WowGuid128?[]? PackSlots; // main bag 16 default + 8 vulpera racial
    public WowGuid128?[]? BankSlots; // only 24 in vanilla
    public WowGuid128?[]? BankBagSlots; // only 6 in vanilla
    public WowGuid128?[]? BuyBackSlots;
    public WowGuid128?[]? KeyringSlots;
    public WowGuid128? FarsightObject;
    public WowGuid128? ComboTarget;
    public WowGuid128? SummonedBattlePetGUID;
    public uint?[] KnownTitles = new uint?[12];
    public ulong? Coinage;
    // Purchased stable slots. 3.3.5a has no update field for this — the count only ever
    // arrives inside MSG_LIST_STABLED_PETS — so the proxy pushes it as a Values update
    // once a stable master is visited, or the client renders every slot locked (#224).
    public byte? NumStableSlots;
    public int? XP;
    public int? NextLevelXP;
    public int? TrialXP;
    public SkillInfo? Skill;
    public int? CharacterPoints;
    public int? MaxTalentTiers;
    public uint? TrackCreatureMask;
    public uint?[] TrackResourceMask = new uint?[2];
    public float? MainhandExpertise;
    public float? OffhandExpertise;
    public float? RangedExpertise;
    public float? CombatRatingExpertise;
    public float? BlockPercentage;
    public float? DodgePercentage;
    public float? DodgePercentageFromAttribute;
    public float? ParryPercentage;
    public float? ParryPercentageFromAttribute;
    public float? CritPercentage;
    public float? RangedCritPercentage;
    public float? OffhandCritPercentage;
    public float?[] SpellCritPercentage = new float?[7];
    public int? ShieldBlock;
    public float? Mastery;
    public float? Speed;
    public float? Avoidance;
    public float? Sturdiness;
    public int? Versatility;
    public float? VersatilityBonus;
    public float? PvpPowerDamage;
    public float? PvpPowerHealing;
    public ulong?[]? ExploredZones;
    public RestInfo[] RestInfo = new RestInfo[2];
    public int?[] ModDamageDonePos = new int?[7];
    public int?[] ModDamageDoneNeg= new int?[7];
    public float?[] ModDamageDonePercent = new float?[7];
    public int? ModHealingDonePos;
    public float? ModHealingPercent;
    public float? ModHealingDonePercent;
    public float? ModPeriodicHealingDonePercent;
    public float?[] WeaponDmgMultipliers = new float?[3];
    public float?[] WeaponAtkSpeedMultipliers = new float?[3];
    public float? ModSpellPowerPercent;
    public float? ModResiliencePercent;
    public float? OverrideSpellPowerByAPPercent;
    public float? OverrideAPBySpellPowerPercent;
    public int? ModTargetResistance;
    public int? ModTargetPhysicalResistance;
    public uint? LocalFlags;
    public byte? GrantableLevels;
    public byte? MultiActionBars;
    public byte? LifetimeMaxRank;
    public byte? NumRespecs;
    public uint? AmmoID;
    public uint? PvpMedals;
    public uint?[] BuybackPrice = new uint?[12];
    public uint?[] BuybackTimestamp = new uint?[12];
    public ushort? TodayHonorableKills;
    public ushort? TodayDishonorableKills;
    public ushort? YesterdayHonorableKills;
    public ushort? YesterdayDishonorableKills;
    public ushort? LastWeekHonorableKills;
    public ushort? LastWeekDishonorableKills;
    public ushort? ThisWeekHonorableKills;
    public ushort? ThisWeekDishonorableKills;
    public uint? ThisWeekContribution;
    public uint? LifetimeHonorableKills;
    public uint? LifetimeDishonorableKills;
    public uint? YesterdayContribution;
    public uint? LastWeekContribution;
    public uint? LastWeekRank;
    public int? WatchedFactionIndex;
    public int?[] CombatRatings { get; } = new int?[32];
    public PVPInfo[] PvpInfo { get; } = new PVPInfo[6];
    public int? MaxLevel;
    public int? ScalingPlayerLevelDelta;
    public int? MaxCreatureScalingLevel;
    public uint?[] NoReagentCostMask { get; } = new uint?[4];
    public int? PetSpellPower;
    public int?[] ProfessionSkillLine { get; } = new int?[2];
    public float? UiHitModifier;
    public float? UiSpellHitModifier;
    public int? HomeRealmTimeOffset;
    public float? ModPetHaste;
    public byte? LocalRegenFlags;
    public byte? AuraVision;
    public byte? NumBackpackSlots;
    public int? OverrideSpellsID;
    public int? LfgBonusFactionID;
    public uint? LootSpecID;
    public uint? OverrideZonePVPType;
    public uint?[] BagSlotFlags { get; } = new uint?[4];
    public uint?[] BankBagSlotFlags { get; } = new uint?[7];
    public ulong?[]? QuestCompleted; // Field has sizeof 1750 uints => 875 ulongs
    public int? Honor;
    public int? HonorNextLevel;
    public uint? PvPTierMaxFromWins;
    public uint? PvPLastWeeksTierMaxFromWins;
    public bool? InsertItemsLeftToRight;
    public byte? PvPRankProgress;
    public byte? GlyphsEnabled;

    // Dynamic Fields
    public List<uint> SelfResSpells = null!;
    public bool HasDailyQuestsUpdate;
    public List<int>? Toys;

    // These three are 25,224 of the ~29,720 bytes of arrays this class used to allocate up
    // front, and a delta that touches one scalar owner field needs none of them. They follow
    // the EnsureActivePlayerData/EnsureContainerData pattern: write sites call Ensure*, read
    // sites keep testing the field for null, so "never written" stays distinguishable from
    // "written as empty" — which is what the builders' emit gates key off.
    public SkillInfo EnsureSkill() => Skill ??= new SkillInfo();

    public ulong?[] EnsureExploredZones() => ExploredZones ??= new ulong?[ExploredZoneWords];

    public ulong?[] EnsureQuestCompleted() => QuestCompleted ??= new ulong?[QuestCompletedWords];

    public WowGuid128?[] EnsureInvSlots() => InvSlots ??= new WowGuid128?[InvSlotCount];

    public WowGuid128?[] EnsurePackSlots() => PackSlots ??= new WowGuid128?[PackSlotCount];

    public WowGuid128?[] EnsureBankSlots() => BankSlots ??= new WowGuid128?[BankSlotCount];

    public WowGuid128?[] EnsureBankBagSlots() => BankBagSlots ??= new WowGuid128?[BankBagSlotCount];

    public WowGuid128?[] EnsureBuyBackSlots() => BuyBackSlots ??= new WowGuid128?[BuyBackSlotCount];

    public WowGuid128?[] EnsureKeyringSlots() => KeyringSlots ??= new WowGuid128?[KeyringSlotCount];
}
