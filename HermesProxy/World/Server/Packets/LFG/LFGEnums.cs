namespace HermesProxy.World.Server.Packets;

/// <summary>
/// Legacy 3.3.5a LFG dungeon lock-status codes — `lockStatus` field in
/// SMSG_LFG_PLAYER_INFO / SMSG_LFG_PARTY_INFO. Mirrors TrinityCore
/// `wotlk_classic/src/server/game/DungeonFinding/LFG.h:97-110`.
/// </summary>
public enum LFGLockStatus : uint
{
    OK                     = 0,
    InsufficientExpansion  = 1,
    TooLowLevel            = 2,
    TooHighLevel           = 3,
    TooLowGearScore        = 4,
    TooHighGearScore       = 5,
    RaidLocked             = 6,
    AttunementTooLowLevel  = 1001,
    AttunementTooHighLevel = 1002,
    QuestNotCompleted      = 1022,
    MissingItem            = 1025,
    NotInSeason            = 1031,
    MissingAchievement     = 1034,
}

/// <summary>
/// Modern V3_4_3 LFG soft-lock hint — `SoftLock` field on `LFGBlackListSlot`.
/// Mirrors TrinityCore `LFG.h:123` LfgSoftLock. Any value > 0 tells the
/// modern client to HIDE the dungeon entry from the LFG panel entirely
/// (level / season / expansion mismatch). `None` leaves it visible as a
/// locked row, which clutters the UI and empirically suppresses the Queue
/// button on V3_4_3. Only `Unk2` (=2) appears in real Wrathion 3.4.3 sniffs.
/// </summary>
public enum LFGSoftLock : uint
{
    None = 0,
    Unk1 = 1,
    Unk2 = 2,
}
