using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects;

public class QuestLog
{
    public int? QuestID;
    public uint? StateFlags;
    public short?[] ObjectiveProgress { get; } = new short?[24];
    public uint? EndTime;
    public uint? AcceptTime;
}
public class PlayerData
{
    // Same scheme as UnitData: an array nothing has written yet reads as a shared, never-written
    // stand-in exposed only as ReadOnlySpan, and writers materialise the real one through
    // Ensure*. A foreign player's Values delta rarely touches its quest log, gear or
    // customizations, so it no longer allocates three arrays for them.
    private static readonly QuestLog[] _emptyQuestLog = new QuestLog[QuestConst.MaxQuestLogSize];
    private static readonly VisibleItem?[] _emptyVisibleItem19 = new VisibleItem?[19];
    private static readonly ChrCustomizationChoice[] _emptyCustomizations36 = new ChrCustomizationChoice[36];

    public WowGuid128? DuelArbiter;
    public WowGuid128? WowAccount;
    public WowGuid128? LootTargetGUID;
    public uint? PlayerFlags;
    public uint? PlayerFlagsEx;
    public uint? GuildRankID;
    public uint? GuildDeleteDate;
    public int? GuildLevel;
    public byte? PartyType;
    public byte? NumBankSlots;
    public byte? NativeSex;
    public byte? Inebriation;
    public byte? PvpTitle;
    public byte? ArenaFaction;
    public byte? PvPRank;
    public uint? DuelTeam;
    public int? GuildTimeStamp;
    private QuestLog[]? _questLog;
    public ReadOnlySpan<QuestLog> QuestLog => _questLog ?? _emptyQuestLog;
    public QuestLog[] EnsureQuestLog() => _questLog ??= new QuestLog[QuestConst.MaxQuestLogSize];
    private VisibleItem?[]? _visibleItems;
    public ReadOnlySpan<VisibleItem?> VisibleItems => _visibleItems ?? _emptyVisibleItem19;
    public VisibleItem?[] EnsureVisibleItems() => _visibleItems ??= new VisibleItem?[19];
    public int? ChosenTitle;
    public int? FakeInebriation;
    public uint? VirtualPlayerRealm;
    public uint? CurrentSpecID;
    public int? TaxiMountAnimKitID;
    public float?[] AvgItemLevel { get; } = new float?[6];
    public uint? CurrentBattlePetBreedQuality;
    public int? HonorLevel;
    private ChrCustomizationChoice[]? _customizations;
    public ReadOnlySpan<ChrCustomizationChoice> Customizations => _customizations ?? _emptyCustomizations36;
    public ChrCustomizationChoice[] EnsureCustomizations() => _customizations ??= new ChrCustomizationChoice[36];

    // Set when a legacy appearance change (barber shop) rewrote Customizations, so the
    // Values path emits the dynamic field. Create always writes them, Values only on change.
    public bool HasCustomizationsUpdate;
}
