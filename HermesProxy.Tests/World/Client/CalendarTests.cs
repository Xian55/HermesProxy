using System;
using System.IO;
using System.Reflection;
using System.Text;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The calendar of a 3.3.5a server for the 3.4.3 client. The incoming fixtures are written after
/// AzerothCore's writers (an event made by player 2, "Probe event", for 15 October 2026 at 20:30);
/// the outgoing bytes follow the 3.4.3 client's layouts.
/// </summary>
/// <remarks>
/// In the collection that owns <see cref="GameData"/>'s instance difficulty tables: the raid
/// lockouts read them, and another class swaps them.
/// </remarks>
[Collection("V343ValuesFilter")]
public sealed class CalendarTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // 15 October 2026, a Thursday, 20:30, as the server packs it
    private const uint EventDate = 0x1A93A51E;

    // An Icecrown Citadel save as AzerothCore writes its GUID: the instance high GUID over its number.
    private const long IcecrownSave = 0x1F42000000000017;

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly WowGuid128 _player;
    private readonly WowGuid128 _other;

    public CalendarTests()
    {
        _player = _harness.SetActivePlayer(new WowGuid64(2));
        _other = _harness.AddKnownObject(new WowGuid64(0x21), ObjectType.Player);
    }

    private T Deliver<T>(string hex, Action<WorldPacket> handler) where T : ServerPacket
    {
        _harness.Deliver(Opcode.SMSG_CALENDAR_SEND_EVENT, [0, 0, .. Convert.FromHexString(hex.Replace(" ", ""))], handler);
        return Assert.IsType<T>(_harness.ClientWire.Sent[^1].Packet);
    }

    // The raid lockouts and resets number their difficulty by the map's rows in the client's data,
    // and a reset takes its period from there.
    private T DeliverWithDifficulties<T>(string hex, Action<WorldPacket> handler) where T : ServerPacket
    {
        var (defaults, sizes, periods) = (GameData.DefaultInstanceDifficulty, GameData.InstanceDifficultySizes, GameData.InstanceResetPeriods);
        string csv = Path.Combine("CSV", "MapDifficulty3.csv");
        GameData.DefaultInstanceDifficulty = GameData.ParseMapDifficulties(csv);
        GameData.InstanceDifficultySizes = GameData.ParseMapDifficultySizes(csv);
        GameData.InstanceResetPeriods = GameData.ParseMapResetPeriods(csv);
        try
        {
            return Deliver<T>(hex, handler);
        }
        finally
        {
            (GameData.DefaultInstanceDifficulty, GameData.InstanceDifficultySizes, GameData.InstanceResetPeriods) = (defaults, sizes, periods);
        }
    }

    private static byte[] Written(ServerPacket packet)
    {
        packet.Write();
        return ((WorldPacket)WorldPacketField.GetValue(packet)!).GetData();
    }

    private static string Text(string text) => Convert.ToHexString(Encoding.UTF8.GetBytes(text)) + "00";

    private static string Ints(params int[] values)
        => string.Concat(Array.ConvertAll(values, v => Convert.ToHexString(BitConverter.GetBytes(v))));

    private static string Longs(params long[] values)
        => string.Concat(Array.ConvertAll(values, v => Convert.ToHexString(BitConverter.GetBytes(v))));

    [Fact]
    public void HandleCalendarSendCalendar_KeepsTheInvitationsEventsAndSavedRaids()
    {
        // HandleCalendarGetCalendar: one invitation, one event, the time, a saved raid (Icecrown
        // Citadel 25), the server's reset base, a raid's reset and no holiday
        var calendar = DeliverWithDifficulties<CalendarSendCalendar>(
            Ints(1) + Longs(1, 7) + "03" + "02" + "00" + "0102"
            + Ints(1) + Longs(1) + Text("Probe event") + Ints(3) + "1EA5931A" + Ints(1, -1) + "0102"
            + Ints(0x6ACEFA23) + "EA58931A"
            + Ints(1) + Ints(631, 1, 3600) + Longs(IcecrownSave)
            + Ints(0x43B237F0) + Ints(1) + Ints(631, 604800, 0)
            + Ints(0),
            _harness.Client.HandleCalendarSendCalendar);

        Assert.Equal(0x1A9358EAu, calendar.ServerTime);
        CalendarListInvite invite = Assert.Single(calendar.Invites);
        Assert.Equal(1ul, invite.EventId);
        Assert.Equal(7ul, invite.InviteId);
        Assert.Equal(3, invite.Status);
        Assert.Equal(2, invite.Moderator);
        Assert.Equal(0, invite.InviteType);
        Assert.Equal(_player, invite.InviterGuid);
        CalendarListEvent calendarEvent = Assert.Single(calendar.Events);
        Assert.Equal("Probe event", calendarEvent.EventName);
        Assert.Equal(3, calendarEvent.EventType);
        Assert.Equal(EventDate, calendarEvent.Date);
        Assert.Equal(1u, calendarEvent.Flags);
        Assert.Equal(-1, calendarEvent.TextureId);
        Assert.Equal(0ul, calendarEvent.EventClubId);
        Assert.Equal(_player, calendarEvent.OwnerGuid);
        CalendarRaidLockout lockout = Assert.Single(calendar.RaidLockouts);
        Assert.Equal(0x17ul, lockout.InstanceId);
        Assert.Equal(631, lockout.MapId);
        Assert.Equal(4u, lockout.DifficultyId);     // 25 normal
        Assert.Equal(3600, lockout.ExpireTime);
        // AzerothCore's weekly reset at the base, 16 hours after the client's own weekly moment
        Assert.Equal(new CalendarRaidReset(631, 3, 604800, 57600), Assert.Single(calendar.RaidResets));
    }

    [Fact]
    public void HandleCalendarSendCalendar_TrinityCoresReset_IsTheSecondsLeftOnTheClientsPeriod()
    {
        // TrinityCore 3.3.5a: the seconds until Icecrown Citadel's next reset, no offset, a fixed base
        var calendar = DeliverWithDifficulties<CalendarSendCalendar>(
            Ints(0) + Ints(0)
            + Ints(0x6ACEFA23) + "EA58931A"
            + Ints(0)
            + Ints(0x43B237F0) + Ints(1) + Ints(631, 200000, 0)
            + Ints(0),
            _harness.Client.HandleCalendarSendCalendar);

        // the next reset is the server's time and those seconds, a week apart from then on
        Assert.Equal(new CalendarRaidReset(631, 3, 604800, 245747), Assert.Single(calendar.RaidResets));
    }

    [Fact]
    public void HandleCalendarSendCalendar_AResetOfAMapTheClientDoesNotKnow_IsLeftOut()
    {
        var calendar = DeliverWithDifficulties<CalendarSendCalendar>(
            Ints(0) + Ints(0)
            + Ints(0x6ACEFA23) + "EA58931A"
            + Ints(0)
            + Ints(0x43B237F0) + Ints(2) + Ints(1, 604800, 0) + Ints(631, 0, 0)
            + Ints(0),
            _harness.Client.HandleCalendarSendCalendar);

        Assert.Empty(calendar.RaidResets);
    }

    [Fact]
    public void HandleCalendarSendCalendar_DatesGoInTheClientsOwnTime()
    {
        // a UTC server and a client two hours east: the server's 20:30 is 22:30 on the client,
        // and the calendar's own time stays the server's, as a native server sends it
        _harness.Session.ClientUtcOffset = TimeSpan.FromHours(2);
        _harness.Session.GameState.LegacyServerUtcOffset = TimeSpan.Zero;
        var calendar = Deliver<CalendarSendCalendar>(
            Ints(0)
            + Ints(1) + Longs(1) + Text("Probe event") + Ints(3) + "1EA5931A" + Ints(1, -1) + "0102"
            + Ints(0x6ACEFA23) + "EA58931A"
            + Ints(0) + Ints(0x43B237F0) + Ints(0) + Ints(0),
            _harness.Client.HandleCalendarSendCalendar);

        Assert.Equal(EventDate + (2u << 6), Assert.Single(calendar.Events).Date);
        Assert.Equal(0x1A9358EAu, calendar.ServerTime);
    }

    [Fact]
    public void CalendarSendCalendar_Layout()
    {
        var calendar = new CalendarSendCalendar { ServerTime = 0x1A9358EA };
        calendar.RaidLockouts.Add(new CalendarRaidLockout { InstanceId = 0x17, MapId = 631, DifficultyId = 4, ExpireTime = 3600 });
        calendar.RaidResets.Add(new CalendarRaidReset(631, 3, 604800, 57600));
        calendar.Invites.Add(new CalendarListInvite { EventId = 1, InviteId = 7, Status = 3, Moderator = 2, InviteType = 0, InviterGuid = WowGuid128.Empty });
        calendar.Events.Add(new CalendarListEvent { EventId = 1, EventType = 3, Date = EventDate, Flags = 1, TextureId = -1, OwnerGuid = WowGuid128.Empty, EventName = "Probe" });

        Assert.Equal(Convert.FromHexString(
            "EA58931A" + Ints(1, 1, 1, 1)                           // the time; invitations, events, saved raids, resets
            + Longs(0x17) + Ints(631, 4, 3600)                      // the saved raid
            + Ints(631, 0, 604800, 57600, 3)                        // a reset: map, 0, period, offset, difficulty
            + Longs(1, 7) + "030200" + "0000" + "00"                // an invitation: no inviter, no flag
            + Longs(1) + "03" + "1EA5931A" + Ints(1) + Ints(-1)     // an event: type, date, 32-bit flags, texture
            + Longs(0) + "0000" + "05" + Convert.ToHexString(Encoding.UTF8.GetBytes("Probe"))),
            Written(calendar));
    }

    [Fact]
    public void HandleCalendarSendEvent_AnEventAsTheServerSentIt()
    {
        // CalendarMgr::SendCalendarEvent, sent when the event was made: no guild
        var calendarEvent = Deliver<CalendarSendEvent>(
            "01 0102 0100000000000000" + Text("Probe event") + Text("made by the probe")
            + "01 00 64000000 FFFFFFFF 01000000 1EA5931A 00300000 00000000 01000000"
            + "0102 50 03 02 00 0100000000000000 00300000 00",
            _harness.Client.HandleCalendarSendEvent);

        Assert.Equal(1, calendarEvent.SendType);
        Assert.Equal(_player, calendarEvent.OwnerGuid);
        Assert.Equal(1ul, calendarEvent.EventId);
        Assert.Equal("Probe event", calendarEvent.EventName);
        Assert.Equal("made by the probe", calendarEvent.Description);
        Assert.Equal(1, calendarEvent.EventType);
        Assert.Equal(-1, calendarEvent.TextureId);
        Assert.Equal(1u, calendarEvent.Flags);
        Assert.Equal(EventDate, calendarEvent.Date);
        Assert.Equal(0x3000u, calendarEvent.LockDate);
        Assert.Equal(0ul, calendarEvent.EventClubId);
        CalendarEventInvite invite = Assert.Single(calendarEvent.Invites);
        Assert.Equal(_player, invite.Guid);
        Assert.Equal(80, invite.Level);
        Assert.Equal(3, invite.Status);
        Assert.Equal(2, invite.Moderator);
        Assert.Equal(0, invite.InviteType);
        Assert.Equal(1ul, invite.InviteId);
        Assert.Equal("", invite.Notes);
    }

    [Fact]
    public void HandleCalendarSendEvent_AGuildEvent_IsTheGuildsNumber()
    {
        var calendarEvent = Deliver<CalendarSendEvent>(
            "00 0102 0500000000000000" + Text("Raid") + Text("")
            + "00 00 64000000 00000000 00040000 1EA5931A 00000000 2B000000 00000000",
            _harness.Client.HandleCalendarSendEvent);

        Assert.Equal(0x400u, calendarEvent.Flags);
        Assert.Equal(43ul, calendarEvent.EventClubId);
    }

    [Fact]
    public void CalendarSendEvent_Layout()
    {
        var calendarEvent = new CalendarSendEvent { SendType = 1, EventId = 1, EventType = 3, TextureId = -1, Flags = 1, Date = EventDate, EventName = "Ab", Description = "C" };
        calendarEvent.Invites.Add(new CalendarEventInvite { Guid = WowGuid128.Empty, InviteId = 1, Level = 80, Status = 3, Moderator = 2, ResponseTime = 0, Notes = "" });

        Assert.Equal(Convert.FromHexString(
            "01" + "0000" + Longs(1) + "03" + Ints(-1) + Ints(1) + "1EA5931A" + Ints(0) + Longs(0) + Ints(1)
            + "020020"                                              // eight bits of name length, eleven of description
            + "0000" + Longs(1) + "50030200" + Ints(0) + "00"       // the invitation, no notes
            + Convert.ToHexString(Encoding.UTF8.GetBytes("AbC"))),
            Written(calendarEvent));
    }

    [Fact]
    public void HandleCalendarEventInvite_TheMakersOwn_IsNotAnInvitationFromSomebodyElse()
    {
        // CalendarMgr::SendCalendarEventInvite for the maker of event 1: no answer time, and 0 last
        var added = Deliver<CalendarInviteAdded>("0102 0100000000000000 0100000000000000 50 03 00 00",
            _harness.Client.HandleCalendarEventInvite);

        Assert.Equal(_player, added.InviteGuid);
        Assert.Equal(1ul, added.EventId);
        Assert.Equal(1ul, added.InviteId);
        Assert.Equal(80, added.Level);
        Assert.Equal(3, added.Status);
        Assert.Equal(0u, added.ResponseTime);
        Assert.Equal(1, added.Type);
        Assert.False(added.ClearPending);
    }

    [Fact]
    public void HandleCalendarEventInvite_AnInvitation_ClearsTheWait()
    {
        var added = Deliver<CalendarInviteAdded>("0121 0100000000000000 0200000000000000 50 00 01 1EA5931A 01",
            _harness.Client.HandleCalendarEventInvite);

        Assert.Equal(_other, added.InviteGuid);
        Assert.Equal(80, added.Level);
        Assert.Equal(0, added.Status);
        Assert.Equal(EventDate, added.ResponseTime);
        Assert.Equal(0, added.Type);
        Assert.True(added.ClearPending);
    }

    [Fact]
    public void HandleCalendarEventInviteAlert_AnAlertAsTheServerSentIt()
    {
        // no guild after the invitation, as a 4.3.4 server has
        var alert = Deliver<CalendarInviteAlert>(
            "0100000000000000" + Text("Probe event") + "1EA5931A 01000000 01000000 FFFFFFFF 0100000000000000 03 02 0102 0121",
            _harness.Client.HandleCalendarEventInviteAlert);

        Assert.Equal(1ul, alert.EventId);
        Assert.Equal("Probe event", alert.EventName);
        Assert.Equal(EventDate, alert.Date);
        Assert.Equal(1u, alert.Flags);
        Assert.Equal(1, alert.EventType);
        Assert.Equal(-1, alert.TextureId);
        Assert.Equal(1ul, alert.InviteId);
        Assert.Equal(3, alert.Status);
        Assert.Equal(2, alert.ModeratorStatus);
        Assert.Equal(_player, alert.OwnerGuid);
        Assert.Equal(_other, alert.InvitedByGuid);
    }

    [Fact]
    public void CalendarInviteAlert_Layout_HasNoBitAfterTheNamesLength()
    {
        var alert = new CalendarInviteAlert { EventId = 1, Date = EventDate, Flags = 1, EventType = 1, TextureId = -1, InviteId = 1, Status = 3, ModeratorStatus = 2, EventName = "Probe" };

        Assert.Equal(Convert.FromHexString(
            Longs(1) + "1EA5931A" + Ints(1) + "01" + Ints(-1) + Longs(0, 1) + "0302" + "0000" + "0000"
            + "05" + Convert.ToHexString(Encoding.UTF8.GetBytes("Probe"))),
            Written(alert));
    }

    [Fact]
    public void HandleCalendarEventUpdatedAlert_AnAlertAsTheServerSentIt()
    {
        // the event moved from 20:30 to 21:15 and was renamed
        var alert = Deliver<CalendarEventUpdatedAlert>(
            "01 0100000000000000 1EA5931A 01000000 4FA5931A 01 FFFFFFFF" + Text("Probe event two") + Text("changed") + "00 64000000 00000000",
            _harness.Client.HandleCalendarEventUpdatedAlert);

        Assert.True(alert.ClearPending);
        Assert.Equal(1ul, alert.EventId);
        Assert.Equal(EventDate, alert.OriginalDate);
        Assert.Equal(0x1A93A54Fu, alert.Date);
        Assert.Equal(1u, alert.Flags);
        Assert.Equal(1, alert.EventType);
        Assert.Equal(-1, alert.TextureId);
        Assert.Equal("Probe event two", alert.EventName);
        Assert.Equal("changed", alert.Description);
    }

    [Fact]
    public void CalendarEventUpdatedAlert_Layout()
    {
        var alert = new CalendarEventUpdatedAlert { EventId = 1, OriginalDate = EventDate, Date = 0x1A93A54F, Flags = 1, TextureId = -1, EventType = 1, ClearPending = true, EventName = "Ab", Description = "C" };

        Assert.Equal(Convert.FromHexString(
            Longs(0, 1) + "1EA5931A" + "4FA5931A" + Ints(0) + Ints(1) + Ints(-1) + "01"
            + "020030"                                              // name and description lengths, then the flag
            + Convert.ToHexString(Encoding.UTF8.GetBytes("AbC"))),
            Written(alert));
    }

    [Fact]
    public void HandleCalendarEventStatus_LeavesTheRankOut()
    {
        // CalendarMgr::SendCalendarEventStatus: invitee 0x21 confirmed (3), moderator rank 1
        var status = Deliver<CalendarInviteStatus>("0121 0100000000000000 1EA5931A 01000000 03 01 4FA5931A",
            _harness.Client.HandleCalendarEventStatus);

        Assert.Equal(_other, status.InviteGuid);
        Assert.Equal(1ul, status.EventId);
        Assert.Equal(EventDate, status.Date);
        Assert.Equal(1u, status.Flags);
        Assert.Equal(3, status.Status);
        Assert.Equal(0x1A93A54Fu, status.ResponseTime);
        Assert.True(status.ClearPending);
    }

    [Fact]
    public void CalendarInviteStatus_AndCommunityInvite_Layouts()
    {
        var status = new CalendarInviteStatus { EventId = 1, Date = EventDate, Flags = 1, Status = 3, ResponseTime = 0x1A93A54F, ClearPending = true };
        Assert.Equal(Convert.FromHexString("0000" + Longs(1) + "1EA5931A" + Ints(1) + "03" + "4FA5931A" + "80"), Written(status));

        var invites = new CalendarCommunityInvite();
        invites.Invites.Add((WowGuid128.Empty, 80));
        Assert.Equal(Convert.FromHexString(Ints(1) + "0000" + "50"), Written(invites));
    }

    [Fact]
    public void HandleCalendarEventRemovedAlert_TheFlagComesFirst()
    {
        var alert = Deliver<CalendarEventRemovedAlert>("01 0100000000000000 4FA5931A", _harness.Client.HandleCalendarEventRemovedAlert);

        Assert.True(alert.ClearPending);
        Assert.Equal(1ul, alert.EventId);
        Assert.Equal(0x1A93A54Fu, alert.Date);
        Assert.Equal(Convert.FromHexString(Longs(1) + "4FA5931A" + "80"), Written(alert));
    }

    [Fact]
    public void HandleCalendarEventInviteRemoved_AndTheModeratorStatus()
    {
        var removed = Deliver<CalendarInviteRemoved>("0121 0100000000000000 01000000 01", _harness.Client.HandleCalendarEventInviteRemoved);
        Assert.Equal(_other, removed.InviteGuid);
        Assert.Equal(1u, removed.Flags);
        Assert.True(removed.ClearPending);

        var moderator = Deliver<CalendarModeratorStatus>("0121 0100000000000000 01 01", _harness.Client.HandleCalendarEventModeratorStatusAlert);
        Assert.Equal(_other, moderator.InviteGuid);
        Assert.Equal(1, moderator.Status);
        Assert.True(moderator.ClearPending);

        var alert = Deliver<CalendarInviteRemovedAlert>("0100000000000000 4FA5931A 01000000 09", _harness.Client.HandleCalendarEventInviteRemovedAlert);
        Assert.Equal(Convert.FromHexString(Longs(1) + "4FA5931A" + Ints(1) + "09"), Written(alert));
    }

    [Fact]
    public void HandleCalendarCommandResult_ARefusalAsTheServerSentIt()
    {
        // "no player of that name" (11 on the server)
        var result = Deliver<CalendarCommandResult>("00000000 00 00 0B000000", _harness.Client.HandleCalendarCommandResult);

        Assert.Equal(12, result.Result);
        Assert.Equal("", result.Name);
        Assert.Equal(Convert.FromHexString("01" + "0C" + "0000"), Written(result));
    }

    [Fact]
    public void HandleCalendarCommandResult_KeepsTheNameARefusalSpeaksOf()
    {
        // already invited (10 on the server), with the player's name
        var result = Deliver<CalendarCommandResult>("00000000 00" + Text("Warriok") + "0A000000", _harness.Client.HandleCalendarCommandResult);

        Assert.Equal(11, result.Result);
        Assert.Equal("Warriok", result.Name);
    }

    [Theory]
    [InlineData(0u, 0)]
    [InlineData(6u, 6)]      // the event is gone
    [InlineData(9u, 9)]      // not in the guild
    [InlineData(10u, 11)]    // already invited
    [InlineData(11u, 12)]    // no player of that name
    [InlineData(29u, 30)]    // no invitation
    [InlineData(36u, 37)]    // the event is on another server
    [InlineData(38u, 38)]    // no invitations within the guild
    [InlineData(40u, 40)]    // no moderator
    public void ModernCalendarError_ShiftsWhereTheClientHasAReasonMore(uint error, byte expected)
        => Assert.Equal(expected, WorldClient.ModernCalendarError(error));

    [Fact]
    public void HandleCalendarFilterGuild_TheMembersAMassInvitationPicked()
    {
        var invites = Deliver<CalendarCommunityInvite>(Ints(2) + "0121 00" + "0102 00", _harness.Client.HandleCalendarFilterGuild);

        Assert.Equal([(_other, (byte)0), (_player, (byte)0)], invites.Invites);
    }

    [Fact]
    public void HandleCalendarRaidLockoutAdded_TheSaveAndTheServersTime()
    {
        // WorldSession::SendCalendarRaidLockout(save, add = true): Icecrown Citadel 25, an hour left
        var added = DeliverWithDifficulties<CalendarRaidLockoutAdded>("EA58931A" + Ints(631, 1, 3600) + Longs(IcecrownSave),
            _harness.Client.HandleCalendarRaidLockoutAdded);

        Assert.Equal(Convert.FromHexString(Longs(0x17) + "EA58931A" + Ints(631, 4, 3600)), Written(added));
    }

    [Fact]
    public void HandleCalendarRaidLockoutRemoved_TheSaveWithoutTime()
    {
        var removed = DeliverWithDifficulties<CalendarRaidLockoutRemoved>(Ints(631, 3, 0) + Longs(IcecrownSave),
            _harness.Client.HandleCalendarRaidLockoutRemoved);

        Assert.Equal(Convert.FromHexString(Longs(0x17) + Ints(631, 6)), Written(removed));     // 25 heroic
    }

    [Fact]
    public void HandleCalendarRaidLockoutUpdated_TheDifficultyRenumbered()
    {
        // an extended save: an hour left before, a week and an hour after
        var updated = DeliverWithDifficulties<CalendarRaidLockoutUpdated>("EA58931A" + Ints(631, 2, 3600, 608400),
            _harness.Client.HandleCalendarRaidLockoutUpdated);

        Assert.Equal(Convert.FromHexString("EA58931A" + Ints(631, 5, 3600, 608400)), Written(updated));     // 10 heroic
    }

    [Fact]
    public void WriteLegacyAddEvent_AsItGoesToTheServer()
    {
        using var packet = new WorldPacket();
        CalendarSystem.WriteLegacyAddEvent(packet, new CalendarAddEvent(1, -1, 0x1A93BD1E, 1, [new CalendarAddEventInvite(_other, 0, 0)], "Probe event", "made by the probe"));

        Assert.Equal(Convert.FromHexString(
            Text("Probe event") + Text("made by the probe") + "01 00 64000000 FFFFFFFF 1EBD931A 00000000 01000000 01000000 0121 00 00".Replace(" ", "")),
            packet.GetDataSpan().ToArray());
    }

    [Fact]
    public void WriteLegacyAddEvent_AGuildAnnouncement_EndsBeforeTheInvitations()
    {
        using var packet = new WorldPacket();
        CalendarSystem.WriteLegacyAddEvent(packet, new CalendarAddEvent(3, -1, 0x1A93A51E, 0x40, [new CalendarAddEventInvite(_player, 3, 2)], "N", ""));

        Assert.Equal(Convert.FromHexString(Text("N") + "00" + "03 00 64000000 FFFFFFFF 1EA5931A 00000000 40000000".Replace(" ", "")),
            packet.GetDataSpan().ToArray());
    }

    [Fact]
    public void WriteLegacyUpdateEvent_AsItGoesToTheServer()
    {
        using var packet = new WorldPacket();
        CalendarSystem.WriteLegacyUpdateEvent(packet, new CalendarUpdateEvent(1, 1, 1, -1, 0x1A93A54F, 1, "Probe event two", "changed"));

        Assert.Equal(Convert.FromHexString(
            Longs(1, 1) + Text("Probe event two") + Text("changed") + "01 00 64000000 FFFFFFFF 4FA5931A 00000000 01000000".Replace(" ", "")),
            packet.GetDataSpan().ToArray());
    }
}
