namespace HermesProxy.World;

/// <summary>
/// The "Time Remaining" line of Warsong Gulch and Twin Peaks, world state 4248.
/// </summary>
/// <remarks>
/// A 4.3.4 server sets it once, to the moment the battle runs out by its own clock (TrinityCore
/// 4.3.4 BattlegroundWS: game time plus fifteen minutes), and its client counts down against the
/// server's time. The modern client prints the number as it comes: its WorldStateUI text for both
/// maps is "Time Remaining: %4248w min.", the minutes a 3.3.5a server sends, one update a minute.
/// </remarks>
internal static class BattlegroundClock
{
    public const uint State = 4248;
    public const uint ActiveState = 4247;

    // A value below this is a count of minutes already: TrinityCore 4.3.4 sends Twin Peaks' so.
    private const int EarliestEndTime = 1_000_000_000;

    // The server's time is known to the second and from a reply that took a moment to arrive, so
    // the proxy's idea of it runs a little late. Without this a clock set to exactly fifteen
    // minutes reads as a second more and shows 16 (seen against TrinityCore 4.3.4).
    private const int ClockSlack = 2;

    public static bool IsEndTime(int value) => value >= EarliestEndTime;

    /// <summary>What is left, in minutes, rounded up.</summary>
    public static int MinutesLeft(long endTime, long serverNow)
    {
        long left = endTime - serverNow - ClockSlack;
        return left <= 0 ? 0 : (int)((left + 59) / 60);
    }

    /// <summary>Seconds until <see cref="MinutesLeft"/> gives the next lower number; 0 when it has run out.</summary>
    public static int SecondsToNextMinute(long endTime, long serverNow)
    {
        long left = endTime - serverNow - ClockSlack;
        return left <= 0 ? 0 : (int)((left - 1) % 60) + 1;
    }
}
