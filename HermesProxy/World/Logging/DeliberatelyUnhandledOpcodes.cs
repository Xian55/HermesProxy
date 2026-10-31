using System.Collections.Frozen;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Logging;

/// <summary>
/// Opcodes that reach the proxy and are left alone on purpose: there is nothing on the other side
/// to translate them into, or the proxy already does the job itself.
///
/// A packet without a handler is a warning, and at every login some two dozen of them were of
/// this kind, in every session of every client. A warning that is always there hides the one
/// that is new. These are logged at Debug; whatever still warns is a packet nobody has looked at
/// or one that is known to be missing.
///
/// An entry needs its reason. A request the proxy could answer, or a packet the other side does
/// have a place for, does not belong here: it belongs in a handler, and until then in the log.
/// </summary>
internal static class DeliberatelyUnhandledOpcodes
{
    internal static readonly FrozenSet<Opcode> FromClient = new HashSet<Opcode>
    {
        // What the client reports about itself. No legacy server has anywhere to put it.
        Opcode.CMSG_REPORT_CLIENT_VARIABLES,
        Opcode.CMSG_REPORT_ENABLED_ADDONS,
        Opcode.CMSG_REPORT_KEYBINDING_EXECUTION_COUNTS,
        Opcode.CMSG_LOG_STREAMING_ERROR,
        // Settings of the client that a modern server keeps and a legacy one never had: the
        // screen-flash accessibility switch, and the turn speed it announces at login.
        Opcode.CMSG_OVERRIDE_SCREEN_FLASH,
        Opcode.CMSG_MOVE_SET_TURN_RATE_CHEAT,
        // The in-game shop, paid character services, character undelete and the social contract:
        // account services of Blizzard's own servers.
        Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST,
        Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST,
        Opcode.CMSG_UPDATE_VAS_PURCHASE_STATES,
        Opcode.CMSG_GET_UNDELETE_CHARACTER_COOLDOWN_STATUS,
        Opcode.CMSG_SOCIAL_CONTRACT_REQUEST,
        // The token's price history and the account's notifications: the same kind of service.
        Opcode.CMSG_COMMERCE_TOKEN_GET_LOG,
        Opcode.CMSG_GET_ACCOUNT_NOTIFICATIONS,
        // Requests even a server of the client's own leaves unanswered (TrinityCore cata_classic:
        // no handler): that the player used /follow (TrinityCore 4.3.4 has the opcode and no
        // handler either), the crowd control spell of an arena opponent, and the names of guild
        // club members by their member id, asked when the guild chat opens (the names come with
        // the roster and the name queries).
        Opcode.CMSG_USED_FOLLOW,
        Opcode.CMSG_REQUEST_CROWD_CONTROL_SPELL,
        Opcode.CMSG_QUERY_PLAYER_NAME_BY_COMMUNITY_ID,
        // The auction sales waiting for their money, asked whenever the auction house opens:
        // TrinityCore wotlk_classic and cata_classic do not register the opcode, and every 3.3.5a
        // server answers it with an empty list.
        Opcode.CMSG_AUCTION_LIST_PENDING_SALES,
        // Things a modern server is asked for and a legacy server says unasked, or not at all: a
        // countdown that is running (a 4.3.4 server starts its own with SMSG_START_TIMER, the older
        // ones have none), and the forced reactions, which every legacy server sends at login.
        Opcode.CMSG_QUERY_COUNTDOWN_TIMER,
        Opcode.CMSG_REQUEST_FORCED_REACTIONS,
        // What a group has that a member new to it was not told: its target icons and its raid
        // markers (TrinityCore cata_classic, HandleRequestPartyJoinUpdates). A legacy server gives
        // the icons only when asked its own way, and the proxy asks by itself whenever a party
        // update names a group it did not know (GroupHandler, RequestRaidTargetIconList). Raid
        // markers have no translation yet.
        Opcode.CMSG_REQUEST_PARTY_JOIN_UPDATES,
        // Bookkeeping of the modern connection that ends at the proxy: the client's word that it
        // has worked off the packets queued for it, how many time sync answers it threw away, and
        // its answer to the suspend token the proxy itself sent around a teleport.
        Opcode.CMSG_QUEUED_MESSAGES_END,
        Opcode.CMSG_DISCARDED_TIME_SYNC_ACKS,
        Opcode.CMSG_SUSPEND_TOKEN_RESPONSE,
    }.ToFrozenSet();

    internal static readonly FrozenSet<Opcode> FromServer = new HashSet<Opcode>
    {
        // The proxy tells the client these itself: its own cache version and its own hotfixes.
        Opcode.SMSG_CACHE_VERSION,
        Opcode.SMSG_HOTFIX_NOTIFY_BLOB,
        // Another player's collision height once its order is acked: the proxy has it from that
        // player's fields, and sends it with the next movement that player reports.
        Opcode.MSG_MOVE_SET_COLLISION_HGT,
        // No modern client has an opcode for these: the dance studio that never shipped, the
        // guild reputation cap of the week and the guild finder's applicant count.
        Opcode.SMSG_LEARNED_DANCE_MOVES,
        Opcode.SMSG_GUILD_REPUTATION_WEEKLY_CAP,
        Opcode.SMSG_LF_GUILD_APPLICANT_LIST_CHANGED,
        // A trainer's word that a spell was bought, which TrinityCore, AzerothCore and CMaNGOS also
        // send after an innkeeper made the inn home: no modern client has an opcode for it. The
        // spell arrives as a learned spell, the new home as SMSG_BINDPOINT_UPDATE.
        Opcode.SMSG_TRAINER_BUY_SUCCEEDED,
        // The answer to buying a bank bag slot, which TrinityCore sends on success too: no modern
        // client has an opcode for it, and the new slot count arrives in the player's fields.
        Opcode.SMSG_BUY_BANK_SLOT_RESULT,
        // Gone from every modern client's table, and from TrinityCore wotlk_classic: the 3.3.5a raid
        // browser's word that a search is on, an arena opponent's unit, and from 1.12 and 2.4.3 the
        // rest timer (resting arrives in the player's flags) and a channelled spell's targets.
        Opcode.SMSG_LFG_UPDATE_SEARCH,
        Opcode.SMSG_ARENA_OPPONENT_UPDATE,
        Opcode.SMSG_SET_REST_START,
        Opcode.SMSG_SPELL_UPDATE_CHAIN_TARGETS,
    }.ToFrozenSet();

    public static bool IsFromClient(Opcode opcode) => FromClient.Contains(opcode);

    public static bool IsFromServer(Opcode opcode) => FromServer.Contains(opcode);
}
