using HermesProxy.World;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The minutes the modern client prints for a 4.3.4 battleground's end time, and when the proxy has
/// to send the next lower number.
/// </summary>
public sealed class BattlegroundClockTests
{
    private const long Now = 1_791_836_000;

    // The proxy's idea of the server's time runs up to two seconds late, so an end time reads as
    // that much further away than it is. The cases below say what is really left.
    private const long Late = 2;

    private static long EndIn(int secondsLeft) => Now + Late + secondsLeft;

    [Theory]
    [InlineData(15 * 60, 15)]     // just started
    [InlineData(15 * 60 - 1, 15)] // a second in: still the fifteenth minute
    [InlineData(14 * 60, 14)]
    [InlineData(61, 2)]
    [InlineData(60, 1)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    [InlineData(-30, 0)]          // the server's end time has passed
    public void MinutesLeft_RoundsUp(int secondsLeft, int expected)
        => Assert.Equal(expected, BattlegroundClock.MinutesLeft(EndIn(secondsLeft), Now));

    /// <summary>
    /// Seen against TrinityCore 4.3.4: the clock is set to fifteen minutes from the server's "now",
    /// the proxy reads that as a second more, and without slack the battle starts at "16".
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MinutesLeft_AFreshFifteenMinutes_Reads15HoweverLateTheProxyIs(int late)
        => Assert.Equal(15, BattlegroundClock.MinutesLeft(Now + 15 * 60 + late, Now));

    [Theory]
    [InlineData(15 * 60, 60)]
    [InlineData(15 * 60 - 1, 59)]
    [InlineData(14 * 60 + 1, 1)]
    [InlineData(60, 60)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    [InlineData(-30, 0)]
    public void SecondsToNextMinute_EndsWhereTheNumberChanges(int secondsLeft, int expected)
        => Assert.Equal(expected, BattlegroundClock.SecondsToNextMinute(EndIn(secondsLeft), Now));

    [Theory]
    [InlineData(15 * 60)]
    [InlineData(14 * 60 + 1)]
    [InlineData(61)]
    public void AfterThatManySeconds_TheNumberIsOneLower(int secondsLeft)
    {
        long end = EndIn(secondsLeft);
        int wait = BattlegroundClock.SecondsToNextMinute(end, Now);

        Assert.Equal(BattlegroundClock.MinutesLeft(end, Now) - 1, BattlegroundClock.MinutesLeft(end, Now + wait));
        Assert.Equal(BattlegroundClock.MinutesLeft(end, Now), BattlegroundClock.MinutesLeft(end, Now + wait - 1));
    }

    [Theory]
    [InlineData(1_791_836_762, true)] // what TrinityCore 4.3.4 sent for Warsong Gulch
    [InlineData(25, false)]           // minutes, as it sends for Twin Peaks
    [InlineData(0, false)]
    public void IsEndTime_TellsATimeFromACountOfMinutes(int value, bool expected)
        => Assert.Equal(expected, BattlegroundClock.IsEndTime(value));
}
