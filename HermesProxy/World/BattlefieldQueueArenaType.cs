namespace HermesProxy.World;

internal static class BattlefieldQueueArenaType
{
    // AC CMSG_BATTLEFIELD_PORT looks up the queue as (bgTypeId, arenaType).
    // 0 = battleground. 2/3/5 = arena team size. A hardcoded 2 made 3v3/5v5
    // Enter Battle miss the queue the player was actually in.
    public static byte ForLegacyPort(bool isV343, bool isArena, byte queuedArenaType)
    {
        if (!isV343)
            return 2;
        if (!isArena)
            return 0;
        return queuedArenaType is 2 or 3 or 5 ? queuedArenaType : (byte)2;
    }

    // AC GroupJoinBattlegroundResult is negative. 3.4.3 STATUS_FAILED uses the
    // positive codes from the same family (party size = 3, not -3).
    public static int ToModernJoinError(int acResult) =>
        acResult < 0 ? -acResult : acResult;

    // Official 3.3.5 / Classic GlobalStrings for SMSG_GROUP_JOINED_BATTLEGROUND, shown where the
    // client prints nothing itself. The 3.4.3 client prints the negative codes from
    // BATTLEFIELD_STATUS_FAILED; it only seemed not to while that packet lacked its ticket flag.
    public static string? JoinErrorText(int acResult, string? playerName)
    {
        return acResult switch
        {
            0 => "Your group has joined a battleground queue, but you are not eligible",
            -2 => "You cannot join the battleground yet because you or one of your party members is flagged as a Deserter.",
            -3 => "Incorrect party size for this arena.",
            -4 => "You can only be queued for 2 battles at once",
            -5 => "You cannot queue for a rated match while queued for other battles",
            -6 => "You cannot queue for another battle while queued for a rated arena match",
            -7 => "Your team has left the arena queue",
            -8 => "You can't do that in a battleground.",
            -10 => "Cannot join the queue unless all members of your party are in the same battleground level range.",
            -11 => string.IsNullOrEmpty(playerName)
                ? "A party member was unavailable to join the queue."
                : $"{playerName} was unavailable to join the queue.",
            -12 => "Join as a group failed. Every grouped player must be on the same arena team.",
            -13 => "You cannot queue for a battleground or arena while using the dungeon system.",
            -14 => "Can't do that while in a Random Battleground queue.",
            -15 => "Can't queue for Random Battleground while in another Battleground queue.",
            _ => acResult <= 0 ? "Join as a group failed" : null,
        };
    }
}
