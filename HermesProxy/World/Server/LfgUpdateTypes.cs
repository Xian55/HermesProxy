namespace HermesProxy.World.Server;

/// <summary>
/// Legacy 3.3.5a and V3_4_3 number lfg::LfgUpdateType differently, and the value does not even
/// live in the same field: TC 3.4.3 writes a constant queue type into LFGUpdateStatus.SubType
/// and puts the update type in Reason (Handlers/LFGHandler.cpp SendLfgUpdateStatus). The proxy
/// used to forward the raw legacy byte as SubType and leave Reason at 0, so the client read the
/// update type as a queue type and the real reason as LFG_UPDATETYPE_DEFAULT. The Dungeon
/// Finder panel therefore never tracked state — Leave stayed greyed out after a queue was
/// dropped, and the in-dungeon LFG eye never appeared after a group formed.
/// Sources: azerothcore-wotlk and TrinityCore/wotlk_classic src/server/game/DungeonFinding/LFG.h.
/// </summary>
public static class LfgUpdateTypes
{
    /// <summary>
    /// LFGUpdateStatus.SubType is an lfg::LfgQueueType, not an update type — LFG_QUEUE_DUNGEON
    /// is 1 (LFG.h), and only the dungeon queue is implemented, matching TC 3.4.3's own "other
    /// types not implemented" comment. Confirmed on the wire: the 3.4.3.54261 reference sniff
    /// World_5_man_party_join_dungeon_finder_parsed.txt:596176 shows SubType 1 / Reason 6.
    /// </summary>
    public const byte ModernQueueDungeon = 1;

    // V3_4_3 lfg::LfgUpdateType values, as carried in LFGUpdateStatus.Reason.
    public const byte ModernDefault = 0;
    public const byte ModernLeaderUnk1 = 1;
    public const byte ModernRolecheckAborted = 4;
    public const byte ModernJoinQueue = 6;
    public const byte ModernRolecheckFailed = 7;
    public const byte ModernRemovedFromQueue = 8;
    public const byte ModernProposalFailed = 9;
    public const byte ModernProposalDeclined = 10;
    public const byte ModernGroupFound = 11;
    public const byte ModernAddedToQueue = 13;
    public const byte ModernProposalBegin = 15;
    public const byte ModernUpdateStatus = 16;
    public const byte ModernGroupMemberOffline = 17;
    public const byte ModernGroupDisband = 18;

    /// <summary>
    /// Translates a legacy 3.3.5a update type into the value a V3_4_3 client understands.
    /// Legacy 2 (LEAVE_RAIDBROWSER) and 3 (JOIN_RAIDBROWSER) have no modern counterpart, so
    /// they fall through to the default alongside anything else unrecognised.
    /// </summary>
    public static byte ToModern(byte legacyUpdateType) => legacyUpdateType switch
    {
        0 => ModernDefault,             // LFG_UPDATETYPE_DEFAULT
        1 => ModernLeaderUnk1,          // LFG_UPDATETYPE_LEADER_UNK1
        4 => ModernRolecheckAborted,    // LFG_UPDATETYPE_ROLECHECK_ABORTED
        5 => ModernJoinQueue,           // LFG_UPDATETYPE_JOIN_QUEUE
        6 => ModernRolecheckFailed,     // LFG_UPDATETYPE_ROLECHECK_FAILED
        7 => ModernRemovedFromQueue,    // LFG_UPDATETYPE_REMOVED_FROM_QUEUE
        8 => ModernProposalFailed,      // LFG_UPDATETYPE_PROPOSAL_FAILED
        9 => ModernProposalDeclined,    // LFG_UPDATETYPE_PROPOSAL_DECLINED
        10 => ModernGroupFound,         // LFG_UPDATETYPE_GROUP_FOUND
        12 => ModernAddedToQueue,       // LFG_UPDATETYPE_ADDED_TO_QUEUE
        13 => ModernProposalBegin,      // LFG_UPDATETYPE_PROPOSAL_BEGIN
        14 => ModernUpdateStatus,       // LFG_UPDATETYPE_UPDATE_STATUS
        15 => ModernGroupMemberOffline, // LFG_UPDATETYPE_GROUP_MEMBER_OFFLINE
        16 => ModernGroupDisband,       // LFG_UPDATETYPE_GROUP_DISBAND_UNK16
        _ => ModernDefault,
    };

    /// <summary>
    /// True when the update type means the player is still attached to an LFG object at all.
    /// Mirrors TC 3.4.3's <c>LfgJoined = updateType != LFG_UPDATETYPE_REMOVED_FROM_QUEUE</c>,
    /// evaluated against the already-translated modern value.
    /// </summary>
    public static bool IsStillLfgJoined(byte modernUpdateType) => modernUpdateType != ModernRemovedFromQueue;
}
