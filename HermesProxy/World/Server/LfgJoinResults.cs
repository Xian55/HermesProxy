namespace HermesProxy.World.Server;

/// <summary>
/// Legacy 3.3.5a and V3_4_3 number lfg::LfgJoinResult completely differently, and the proxy
/// used to forward the legacy byte straight through. The V3_4_3 client does not recognise
/// those values, so every Dungeon Finder rejection was silently swallowed and the UI just
/// sat there. Sources: azerothcore-wotlk and TrinityCore/wotlk_classic,
/// src/server/game/DungeonFinding/LFGMgr.h.
/// </summary>
public static class LfgJoinResults
{
    // V3_4_3 values.
    public const byte ModernOk = 0x00;
    public const byte ModernGroupFull = 0x1F;
    public const byte ModernNoLfgObject = 0x21;
    public const byte ModernNoSlots = 0x22;
    public const byte ModernMismatchedSlots = 0x23;
    public const byte ModernDifferentRealms = 0x24;
    public const byte ModernMembersNotPresent = 0x25;
    public const byte ModernGetInfoTimeout = 0x26;
    public const byte ModernInvalidSlot = 0x27;
    public const byte ModernDeserterPlayer = 0x28;
    public const byte ModernDeserterParty = 0x29;
    public const byte ModernRandomCooldownPlayer = 0x2A;
    public const byte ModernRandomCooldownParty = 0x2B;
    public const byte ModernTooManyMembers = 0x2C;
    public const byte ModernCantUseDungeons = 0x2D;
    public const byte ModernRoleCheckFailed = 0x2E;

    /// <summary>
    /// Translates a legacy 3.3.5a join result into the value a V3_4_3 client understands.
    /// Unknown codes fall back to "internal LFG error" so the player still sees something.
    /// </summary>
    public static byte ToModern(byte legacyResult) => legacyResult switch
    {
        0 => ModernOk,                    // LFG_JOIN_OK
        1 => ModernRoleCheckFailed,       // LFG_JOIN_FAILED (role check)
        2 => ModernGroupFull,             // LFG_JOIN_GROUPFULL
        4 => ModernNoLfgObject,           // LFG_JOIN_INTERNAL_ERROR
        5 => ModernNoSlots,               // LFG_JOIN_NOT_MEET_REQS
        6 => 6,                           // LFG_JOIN_PARTY_NOT_MEET_REQS, same value on both
        7 => ModernMismatchedSlots,       // LFG_JOIN_MIXED_RAID_DUNGEON
        8 => ModernDifferentRealms,       // LFG_JOIN_MULTI_REALM
        9 => ModernMembersNotPresent,     // LFG_JOIN_DISCONNECTED
        10 => ModernGetInfoTimeout,       // LFG_JOIN_PARTY_INFO_FAILED
        11 => ModernInvalidSlot,          // LFG_JOIN_DUNGEON_INVALID
        12 => ModernDeserterPlayer,       // LFG_JOIN_DESERTER
        13 => ModernDeserterParty,        // LFG_JOIN_PARTY_DESERTER
        14 => ModernRandomCooldownPlayer, // LFG_JOIN_RANDOM_COOLDOWN
        15 => ModernRandomCooldownParty,  // LFG_JOIN_PARTY_RANDOM_COOLDOWN
        16 => ModernTooManyMembers,       // LFG_JOIN_TOO_MUCH_MEMBERS
        17 => ModernCantUseDungeons,      // LFG_JOIN_USING_BG_SYSTEM
        _ => ModernNoLfgObject,
    };
}
