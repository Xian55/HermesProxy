using Bgs.Protocol.Club.V1;
using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for the 4.4.2 client's guild club (<see cref="GuildClub"/>): when the
/// client is given its club, loses it, and is told how its members changed. Whether the guild
/// window opens at all comes down to the first of these.
/// </summary>
internal static partial class GuildClubLogMessages
{
    // EventId 1620-1629 reserved for the guild club.

    [LoggerMessage(EventId = 1620, Level = LogLevel.Debug,
        Message = "Guild club {ClubId} '{Name}' given to the client ({How}).")]
    public static partial void ClubGiven(ILogger logger, ulong ClubId, string Name, string How);

    [LoggerMessage(EventId = 1621, Level = LogLevel.Debug,
        Message = "The client subscribed to its memberships while the name of guild {GuildId} is not known yet; the club follows when it is.")]
    public static partial void NameNotKnownYet(ILogger logger, uint GuildId);

    [LoggerMessage(EventId = 1622, Level = LogLevel.Debug,
        Message = "Guild club {ClubId} taken away from the client ({Reason}).")]
    public static partial void ClubWithdrawn(ILogger logger, ulong ClubId, ClubRemovedReason Reason);

    [LoggerMessage(EventId = 1623, Level = LogLevel.Debug,
        Message = "Guild club {ClubId}: {Count} member(s) given to the client (roster known: {RosterKnown}).")]
    public static partial void MembersGiven(ILogger logger, ulong ClubId, int Count, bool RosterKnown);

    [LoggerMessage(EventId = 1624, Level = LogLevel.Debug,
        Message = "Guild club {ClubId}: told the client of {Added} member(s) added, {Removed} removed and {Roles} role change(s).")]
    public static partial void MembersChanged(ILogger logger, ulong ClubId, int Added, int Removed, int Roles);

    [LoggerMessage(EventId = 1625, Level = LogLevel.Warning,
        Message = "The client asked {Method} for club {ClubId}, but the club it was given is {GivenClub}.")]
    public static partial void RequestForUnknownClub(ILogger logger, string Method, ulong ClubId, ulong GivenClub);

    // The caller builds the JSON only when Trace is on.
    [LoggerMessage(EventId = 1626, Level = LogLevel.Trace,
        Message = "Club notification {Listener}/m:{MethodId} token {Token}: {Content}")]
    public static partial void NotificationSent(ILogger logger, Framework.Constants.OriginalHash Listener, uint MethodId, uint Token, string Content);
}
