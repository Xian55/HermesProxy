using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums;

[Flags]
public enum GroupFlags
{
    None              = 0x000,
    FakeRaid          = 0x001,
    Raid              = 0x002,
    LfgRestricted     = 0x004, // Script_HasLFGRestrictions()
    Lfg               = 0x008,
    Destroyed         = 0x010,
    OnePersonParty    = 0x020, // Script_IsOnePersonParty()
    EveryoneAssistant = 0x040, // Script_IsEveryoneAssistant()
    GuildGroup        = 0x100,

    MaskBgRaid        = FakeRaid | Raid
}

public enum GroupType
{
    None         = 0,
    Normal       = 1,
    PvP          = 2, // battleground and arena
    WorldPvP     = 4,
}

[Flags]
public enum GroupMemberOnlineStatus
{
    Offline = 0x000,
    Online  = 0x001, // Lua_UnitIsConnected
    PVP     = 0x002, // Lua_UnitIsPVP
    Dead    = 0x004, // Lua_UnitIsDead
    Ghost   = 0x008, // Lua_UnitIsGhost
    PVPFFA  = 0x010, // Lua_UnitIsPVPFreeForAll
    Unk3    = 0x020, // used in calls from Lua_GetPlayerMapPosition/Lua_GetBattlefieldFlagPosition
    AFK     = 0x040, // Lua_UnitIsAFK
    DND     = 0x080, // Lua_UnitIsDND
    RAF     = 0x100,
    Vehicle = 0x200, // Lua_UnitInVehicle
}

[Flags]
public enum GroupMemberFlags
{
    None       = 0x00,
    Assistant  = 0x01,
    MainTank   = 0x02,
    MainAssist = 0x04
}

[Flags]
public enum GroupUpdateFlagVanilla : uint
{
    None             = 0x00000000,
    Status           = 0x00000001,
    CurrentHealth    = 0x00000002,
    MaxHealth        = 0x00000004,
    PowerType        = 0x00000008,
    CurrentPower     = 0x00000010,
    MaxPower         = 0x00000020,
    Level            = 0x00000040,
    Zone             = 0x00000080,
    Position         = 0x00000100,
    Auras            = 0x00000200,
    AurasNegative    = 0x00000400,
    PetGuid          = 0x00000800,
    PetName          = 0x00001000,
    PetModelId       = 0x00002000,
    PetCurrentHealth = 0x00004000,
    PetMaxHealth     = 0x00008000,
    PetPowerType     = 0x00010000,
    PetCurrentPower  = 0x00020000,
    PetMaxPower      = 0x00040000,
    PetAuras         = 0x00080000,
    PetAurasNegative = 0x00100000,
}

[Flags]
public enum GroupUpdateFlagTBC : uint
{
    None             = 0x00000000,       // nothing
    Status           = 0x00000001,       // uint16, flags
    CurrentHealth    = 0x00000002,       // uint16
    MaxHealth        = 0x00000004,       // uint16
    PowerType        = 0x00000008,       // uint8
    CurrentPower     = 0x00000010,       // uint16
    MaxPower         = 0x00000020,       // uint16
    Level            = 0x00000040,       // uint16
    Zone             = 0x00000080,       // uint16
    Position         = 0x00000100,       // uint16, uint16
    Auras            = 0x00000200,       // uint64 mask, for each bit set uint16 spellid + uint8 unk
    PetGuid          = 0x00000400,       // uint64 pet guid
    PetName          = 0x00000800,       // pet name, nullptr terminated string
    PetModelId       = 0x00001000,       // uint16, model id
    PetCurrentHealth = 0x00002000,       // uint16 pet cur health
    PetMaxHealth     = 0x00004000,       // uint16 pet max health
    PetPowerType     = 0x00008000,       // uint8 pet power type
    PetCurrentPower  = 0x00010000,       // uint16 pet cur power
    PetMaxPower      = 0x00020000,       // uint16 pet max power
    PetAuras         = 0x00040000,       // uint64 mask, for each bit set uint16 spellid + uint8 unk, pet auras...
    VehicleSeat      = 0x00080000,       // uint32 vehicle/mount seat id (WotLK addition; not present on TBC)
    Pet              = 0x0007FC00,       // all pet flags
    Full             = 0x000FFFFF,       // all known flags incl. WotLK VehicleSeat
}

/// <summary>
/// What a 4.3.4 party member update carries (TrinityCore 4.3.4 <c>GroupUpdateFlags</c>). Not the
/// 3.3.5a set with more bits: a WMO group id sits at 0x100 and moves the position, the auras and
/// the pet up one bit, the position has a height, and the phases follow the vehicle seat.
/// </summary>
[Flags]
public enum GroupUpdateFlagCata : uint
{
    None             = 0x00000000,
    Status           = 0x00000001,       // uint16
    CurrentHealth    = 0x00000002,       // uint32
    MaxHealth        = 0x00000004,       // uint32
    PowerType        = 0x00000008,       // uint8
    CurrentPower     = 0x00000010,       // uint16
    MaxPower         = 0x00000020,       // uint16
    Level            = 0x00000040,       // uint16
    Zone             = 0x00000080,       // uint16
    WmoGroupId       = 0x00000100,       // int16
    Position         = 0x00000200,       // int16 x, y, z
    Auras            = 0x00000400,       // uint8, uint64 mask, uint32, then spell, uint16 flags and maybe three values a set bit
    PetGuid          = 0x00000800,       // uint64
    PetName          = 0x00001000,       // string, zero terminated
    PetModelId       = 0x00002000,       // uint16
    PetCurrentHealth = 0x00004000,       // uint32
    PetMaxHealth     = 0x00008000,       // uint32
    PetPowerType     = 0x00010000,       // uint8
    PetCurrentPower  = 0x00020000,       // uint16
    PetMaxPower      = 0x00040000,       // uint16
    PetAuras         = 0x00080000,       // as Auras; sent without a pet too, empty
    VehicleSeat      = 0x00100000,       // int32
    Phase            = 0x00200000,       // uint32 flags, uint32 count, a uint16 each
    // 0x00400000 and up are set in a full state and carry nothing
}

enum PartyResultVanilla : uint
{
    Ok = 0,
    BadPlayerName = 1,      // "Cannot find '%s'."
    TargetNotInGroup = 2,   // "%s is not in your party."
    GroupFull = 3,          // "Your party is full."
    AlreadyInGroup = 4,     // "%s is already in a group."
    NotInGroup = 5,         // "You aren't in a party."
    NotLeader = 6,          // "You are not the party leader."
    PlayerWrongFaction = 7,
    IgnoringYou = 8,        // "%s is ignoring you." :´(
}

enum PartyResultModern : byte
{
    Ok = 0,
    BadPlayerName = 1,
    TargetNotInGroup = 2,
    TargetNotInInstance = 3,
    GroupFull = 4,
    AlreadyInGroup = 5,
    NotInGroup = 6,
    NotLeader = 7,
    PlayerWrongFaction = 8,
    IgnoringYou = 9,
    LfgPending = 12,
    InviteRestricted = 13,
    GroupSwapFailed = 14,
    InviteUnknownRealm = 15,
    InviteNoPartyServer = 16,
    InvitePartyBusy = 17,
    PartyTargetAmbiguous = 18,
    PartyLfgInviteRaidLocked = 19,
    PartyLfgBootLimit = 20,
    PartyLfgBootCooldown = 21,
    PartyLfgBootInProgress = 22,
    PartyLfgBootTooFewPlayers = 23,
    PartyLfgBootNotEligible = 24,
    RaidDisallowedByLevel = 25,
    PartyLfgBootInCombat = 26,
    VoteKickReasonNeeded = 27,
    PartyLfgBootDungeonComplete = 28,
    PartyLfgBootLootRolls = 29,
    PartyLfgTeleportInCombat = 30
}
