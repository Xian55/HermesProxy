using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

// The calendar of a 3.3.5a server (AzerothCore CalendarMgr::Send* and
// WorldSession::HandleCalendarGetCalendar) and of a 4.3.4 server (TrinityCore 4.3.4) for the 3.4.3
// and 4.4.2 clients. The two servers write most of them alike; the calendar list, an event and an
// invitation alert carry the guild's GUID on 4.3.4 and are read by the *Cata methods. The 1.12 and
// 2.4.3 servers have no calendar.
public partial class WorldClient
{
    [HandlesSmsg(Opcode.SMSG_CALENDAR_SEND_NUM_PENDING)]
    internal void HandleCalendarSendNumPending(WorldPacket packet)
    {
        CalendarSendNumPending pending = new();
        pending.NumPending = packet.ReadUInt32();
        SendPacketToClient(pending);
    }

    // Counts past these are a misread, not a calendar: a 3.3.5a server keeps a hundred invitations
    // an event and thirty events a player.
    private const int MaxCalendarEntries = 4096;

    /// <summary>
    /// A 4.3.4 guild's GUID as the id the 4.4.2 client knows the guild's calendar by: the guild's
    /// number, as TrinityCore cata_classic sends it.
    /// </summary>
    private static ulong CalendarClubId(ulong guildGuid) => guildGuid & 0x000FFFFFFFFFFFFFul;

    private WowGuid128 CalendarPlayer(WowGuid64 guid)
        => guid.IsEmpty() ? WowGuid128.Empty : guid.To128(GetSession().GameState);

    /// <summary>
    /// The raid save a 3.3.5a server describes as a map, a difficulty, the seconds left and the
    /// save's GUID, as the 3.4.3 client numbers it: the instance by its number, the difficulty by
    /// the map's own (a 25-player raid is 4 there, not 1).
    /// </summary>
    private static CalendarRaidLockout CalendarLockout(uint mapId, uint legacyDifficulty, uint secondsLeft, ulong saveGuid)
    {
        CalendarRaidLockout lockout = new();
        lockout.MapId = (int)mapId;
        lockout.DifficultyId = GameData.ModernInstanceDifficulty(mapId, legacyDifficulty).DifficultyId;
        lockout.ExpireTime = (int)secondsLeft;
        lockout.InstanceId = (uint)saveGuid;
        return lockout;
    }

    // A raid's reset period is whole days; the seconds until a reset almost never are.
    private const int SecondsPerDay = 86400;

    /// <summary>
    /// The raids' resets after the saves: a moment the server counts from, then each raid's map and
    /// two numbers. AzerothCore sends what a 3.3.5a client reads there, the period and an offset
    /// from that moment; TrinityCore (3.3.5a and 4.3.4) sends the seconds until the next reset in
    /// the period's place, an offset of 0 and a fixed moment. A whole number of days is taken for
    /// AzerothCore's period; otherwise the period is the client's own (MapDifficulty). A raid the
    /// client does not know is left out.
    /// </summary>
    private static void ReadCalendarRaidResets(WorldPacket packet, long serverNow, CalendarSendCalendar calendar)
    {
        long origin = packet.ReadUInt32();
        uint count = packet.ReadUInt32();
        if (count > MaxCalendarEntries)
            return;
        for (uint i = 0; i < count; i++)
        {
            uint mapId = packet.ReadUInt32();
            int periodOrSecondsLeft = packet.ReadInt32();
            int offset = packet.ReadInt32();
            uint difficultyId = GameData.ModernInstanceDifficulty(mapId, 0).DifficultyId;
            if (difficultyId == 0 || periodOrSecondsLeft <= 0)
                continue;

            if (periodOrSecondsLeft % SecondsPerDay == 0)
            {
                calendar.RaidResets.Add(CalendarRaidReset.At((int)mapId, difficultyId, periodOrSecondsLeft, origin + offset));
                continue;
            }
            if (GameData.InstanceResetPeriods.TryGetValue((mapId, difficultyId), out uint period))
                calendar.RaidResets.Add(CalendarRaidReset.At((int)mapId, difficultyId, (int)period, serverNow + periodOrSecondsLeft));
        }
    }

    /// <summary>
    /// <c>HandleCalendarGetCalendar</c>: the invitations, the events, the time, then the raids the
    /// player is saved to, the raids' reset times and the holidays the server changed. The client
    /// is sent all but the holidays, which it has in a table of its own.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_SEND_CALENDAR)]
    internal void HandleCalendarSendCalendar(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleCalendarSendCalendarCata(packet);
            return;
        }

        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarSendCalendar calendar = new();

        uint inviteCount = packet.ReadUInt32();
        if (inviteCount > MaxCalendarEntries)
            return;
        for (uint i = 0; i < inviteCount; i++)
        {
            CalendarListInvite invite = new();
            invite.EventId = packet.ReadUInt64();
            invite.InviteId = packet.ReadUInt64();
            invite.Status = packet.ReadUInt8();
            invite.Moderator = packet.ReadUInt8();
            invite.InviteType = packet.ReadUInt8();
            invite.InviterGuid = CalendarPlayer(packet.ReadPackedGuid());
            calendar.Invites.Add(invite);
        }

        uint eventCount = packet.ReadUInt32();
        if (eventCount > MaxCalendarEntries)
            return;
        for (uint i = 0; i < eventCount; i++)
        {
            CalendarListEvent calendarEvent = new();
            calendarEvent.EventId = packet.ReadUInt64();
            calendarEvent.EventName = packet.ReadCString();
            calendarEvent.EventType = (byte)packet.ReadUInt32();
            calendarEvent.Date = clock.ToClient(packet.ReadUInt32());
            calendarEvent.Flags = packet.ReadUInt32();
            calendarEvent.TextureId = packet.ReadInt32();
            calendarEvent.OwnerGuid = CalendarPlayer(packet.ReadPackedGuid());
            calendar.Events.Add(calendarEvent);
        }

        long serverNow = packet.ReadUInt32();   // the server's time in seconds
        calendar.ServerTime = packet.ReadUInt32();  // and as a packed date

        uint lockoutCount = packet.ReadUInt32();
        if (lockoutCount > MaxCalendarEntries)
            return;
        for (uint i = 0; i < lockoutCount; i++)
        {
            uint mapId = packet.ReadUInt32();
            uint difficulty = packet.ReadUInt32();
            uint secondsLeft = packet.ReadUInt32();
            calendar.RaidLockouts.Add(CalendarLockout(mapId, difficulty, secondsLeft, packet.ReadUInt64()));
        }
        ReadCalendarRaidResets(packet, serverNow, calendar);
        SendPacketToClient(calendar);
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>HandleCalendarGetCalendar</c>: the invitations, the events, the time,
    /// then the raids the player is saved to, the raids' reset times and the holidays the server
    /// changed. The client is sent all but the holidays, which it has in a table of its own. A save
    /// is 3.3.5a's (map, difficulty, seconds left, save id); without it a lock made before the
    /// calendar's first opening of the session never showed there.
    /// </summary>
    internal void HandleCalendarSendCalendarCata(WorldPacket packet)
    {
        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarSendCalendar calendar = new();

        uint inviteCount = packet.ReadUInt32();
        if (inviteCount > MaxCalendarEntries)
            return;
        for (uint i = 0; i < inviteCount; i++)
        {
            CalendarListInvite invite = new();
            invite.EventId = packet.ReadUInt64();
            invite.InviteId = packet.ReadUInt64();
            invite.Status = packet.ReadUInt8();
            invite.Moderator = packet.ReadUInt8();
            invite.InviteType = packet.ReadUInt8();
            invite.InviterGuid = CalendarPlayer(packet.ReadPackedGuid());
            calendar.Invites.Add(invite);
        }

        uint eventCount = packet.ReadUInt32();
        if (eventCount > MaxCalendarEntries)
            return;
        for (uint i = 0; i < eventCount; i++)
        {
            CalendarListEvent calendarEvent = new();
            calendarEvent.EventId = packet.ReadUInt64();
            calendarEvent.EventName = packet.ReadCString();
            calendarEvent.EventType = (byte)packet.ReadUInt32();
            calendarEvent.Date = clock.ToClient(packet.ReadUInt32());
            calendarEvent.Flags = packet.ReadUInt32();
            calendarEvent.TextureId = packet.ReadInt32();
            calendarEvent.EventClubId = CalendarClubId(packet.ReadUInt64());
            calendarEvent.OwnerGuid = CalendarPlayer(packet.ReadPackedGuid());
            calendar.Events.Add(calendarEvent);
        }

        long serverNow = packet.ReadUInt32();   // the server's time in seconds
        calendar.ServerTime = packet.ReadUInt32();  // and as a packed date

        uint lockoutCount = packet.ReadUInt32();
        if (lockoutCount > MaxCalendarEntries)
            return;
        for (uint i = 0; i < lockoutCount; i++)
        {
            uint mapId = packet.ReadUInt32();
            uint difficulty = packet.ReadUInt32();
            uint secondsLeft = packet.ReadUInt32();
            calendar.RaidLockouts.Add(CalendarLockout(mapId, difficulty, secondsLeft, packet.ReadUInt64()));
        }
        ReadCalendarRaidResets(packet, serverNow, calendar);
        SendPacketToClient(calendar);
    }

    /// <summary>
    /// <c>CalendarMgr::SendCalendarEvent</c>: the event with its two texts in the middle, then
    /// everybody invited, each with the note they left. The guild a guild event is for is its
    /// number, which the modern client takes as the event's club, as TrinityCore wotlk_classic
    /// sends it.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_SEND_EVENT)]
    internal void HandleCalendarSendEvent(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleCalendarSendEventCata(packet);
            return;
        }

        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarSendEvent calendarEvent = new();
        calendarEvent.SendType = packet.ReadUInt8();
        calendarEvent.OwnerGuid = CalendarPlayer(packet.ReadPackedGuid());
        calendarEvent.EventId = packet.ReadUInt64();
        calendarEvent.EventName = packet.ReadCString();
        calendarEvent.Description = packet.ReadCString();
        calendarEvent.EventType = packet.ReadUInt8();
        packet.ReadUInt8();                     // repeats
        packet.ReadUInt32();                    // most invitations
        calendarEvent.TextureId = packet.ReadInt32();
        calendarEvent.Flags = packet.ReadUInt32();
        calendarEvent.Date = clock.ToClient(packet.ReadUInt32());
        calendarEvent.LockDate = clock.ToClient(packet.ReadUInt32());
        calendarEvent.EventClubId = packet.ReadUInt32();

        uint inviteCount = packet.ReadUInt32();
        if (inviteCount > MaxCalendarEntries)
            return;
        for (uint i = 0; i < inviteCount; i++)
        {
            CalendarEventInvite invite = new();
            invite.Guid = CalendarPlayer(packet.ReadPackedGuid());
            invite.Level = packet.ReadUInt8();
            invite.Status = packet.ReadUInt8();
            invite.Moderator = packet.ReadUInt8();
            invite.InviteType = packet.ReadUInt8();
            invite.InviteId = packet.ReadUInt64();
            invite.ResponseTime = clock.ToClient(packet.ReadUInt32());
            invite.Notes = packet.ReadCString();
            calendarEvent.Invites.Add(invite);
        }
        SendPacketToClient(calendarEvent);
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>CalendarMgr::SendCalendarEvent</c>: the event with its two texts in the
    /// middle, then everybody invited, each with the note they left.
    /// </summary>
    internal void HandleCalendarSendEventCata(WorldPacket packet)
    {
        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarSendEvent calendarEvent = new();
        calendarEvent.SendType = packet.ReadUInt8();
        calendarEvent.OwnerGuid = CalendarPlayer(packet.ReadPackedGuid());
        calendarEvent.EventId = packet.ReadUInt64();
        calendarEvent.EventName = packet.ReadCString();
        calendarEvent.Description = packet.ReadCString();
        calendarEvent.EventType = packet.ReadUInt8();
        packet.ReadUInt8();                     // repeats
        packet.ReadUInt32();                    // most invitations
        calendarEvent.TextureId = packet.ReadInt32();
        calendarEvent.Flags = packet.ReadUInt32();
        calendarEvent.Date = clock.ToClient(packet.ReadUInt32());
        calendarEvent.LockDate = clock.ToClient(packet.ReadUInt32());
        calendarEvent.EventClubId = CalendarClubId(packet.ReadUInt64());

        uint inviteCount = packet.ReadUInt32();
        if (inviteCount > MaxCalendarEntries)
            return;
        for (uint i = 0; i < inviteCount; i++)
        {
            CalendarEventInvite invite = new();
            invite.Guid = CalendarPlayer(packet.ReadPackedGuid());
            invite.Level = packet.ReadUInt8();
            invite.Status = packet.ReadUInt8();
            invite.Moderator = packet.ReadUInt8();
            invite.InviteType = packet.ReadUInt8();
            invite.InviteId = packet.ReadUInt64();
            invite.ResponseTime = clock.ToClient(packet.ReadUInt32());
            invite.Notes = packet.ReadCString();
            calendarEvent.Invites.Add(invite);
        }
        SendPacketToClient(calendarEvent);
    }

    /// <summary>
    /// <c>CalendarMgr::SendCalendarEventInvite</c>: an invitee added to an event's list. The
    /// answer's time is there only when there is an answer. The last number is 0 for somebody who
    /// signed up and 1 for somebody who was invited; a 3.3.5a server clears the client's wait
    /// after a sign-up with a packet of its own.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_EVENT_INVITE)]
    internal void HandleCalendarEventInvite(WorldPacket packet)
    {
        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarInviteAdded added = new();
        added.InviteGuid = CalendarPlayer(packet.ReadPackedGuid());
        added.EventId = packet.ReadUInt64();
        added.InviteId = packet.ReadUInt64();
        added.Level = packet.ReadUInt8();
        added.Status = packet.ReadUInt8();
        if (packet.ReadBool())
            added.ResponseTime = clock.ToClient(packet.ReadUInt32());
        bool invited = packet.ReadBool();
        added.Type = (byte)(invited ? 0 : 1);
        added.ClearPending = invited;
        SendPacketToClient(added);
    }

    /// <summary>
    /// <c>CalendarMgr::SendCalendarEventInviteAlert</c>. It does not say whose guild the event is
    /// for.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_EVENT_INVITE_ALERT)]
    internal void HandleCalendarEventInviteAlert(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleCalendarEventInviteAlertCata(packet);
            return;
        }

        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarInviteAlert alert = new();
        alert.EventId = packet.ReadUInt64();
        alert.EventName = packet.ReadCString();
        alert.Date = clock.ToClient(packet.ReadUInt32());
        alert.Flags = packet.ReadUInt32();
        alert.EventType = (byte)packet.ReadUInt32();
        alert.TextureId = packet.ReadInt32();
        alert.InviteId = packet.ReadUInt64();
        alert.Status = packet.ReadUInt8();
        alert.ModeratorStatus = packet.ReadUInt8();
        alert.OwnerGuid = CalendarPlayer(packet.ReadPackedGuid());
        alert.InvitedByGuid = CalendarPlayer(packet.ReadPackedGuid());
        SendPacketToClient(alert);
    }

    internal void HandleCalendarEventInviteAlertCata(WorldPacket packet)
    {
        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarInviteAlert alert = new();
        alert.EventId = packet.ReadUInt64();
        alert.EventName = packet.ReadCString();
        alert.Date = clock.ToClient(packet.ReadUInt32());
        alert.Flags = packet.ReadUInt32();
        alert.EventType = (byte)packet.ReadUInt32();
        alert.TextureId = packet.ReadInt32();
        alert.InviteId = packet.ReadUInt64();
        alert.EventClubId = CalendarClubId(packet.ReadUInt64());
        alert.Status = packet.ReadUInt8();
        alert.ModeratorStatus = packet.ReadUInt8();
        alert.OwnerGuid = CalendarPlayer(packet.ReadPackedGuid());
        alert.InvitedByGuid = CalendarPlayer(packet.ReadPackedGuid());
        SendPacketToClient(alert);
    }

    /// <summary>
    /// <c>CalendarMgr::SendCalendarEventStatus</c>. It carries the invitee's rank as well, which
    /// the 3.4.3 client has a packet of its own for.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_EVENT_STATUS)]
    internal void HandleCalendarEventStatus(WorldPacket packet)
    {
        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarInviteStatus status = new();
        status.InviteGuid = CalendarPlayer(packet.ReadPackedGuid());
        status.EventId = packet.ReadUInt64();
        status.Date = clock.ToClient(packet.ReadUInt32());
        status.Flags = packet.ReadUInt32();
        status.Status = packet.ReadUInt8();
        packet.ReadUInt8();                     // rank
        status.ResponseTime = clock.ToClient(packet.ReadUInt32());
        status.ClearPending = true;
        SendPacketToClient(status);
    }

    /// <summary><c>CalendarMgr::SendCalendarEventInviteRemove</c>.</summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_EVENT_INVITE_REMOVED)]
    internal void HandleCalendarEventInviteRemoved(WorldPacket packet)
    {
        CalendarInviteRemoved removed = new();
        removed.InviteGuid = CalendarPlayer(packet.ReadPackedGuid());
        removed.EventId = packet.ReadUInt64();
        removed.Flags = packet.ReadUInt32();
        removed.ClearPending = packet.ReadBool();
        SendPacketToClient(removed);
    }

    /// <summary>
    /// <c>CalendarMgr::SendCalendarEventInviteRemoveAlert</c>: the same four numbers on both sides.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_EVENT_INVITE_REMOVED_ALERT)]
    internal void HandleCalendarEventInviteRemovedAlert(WorldPacket packet)
    {
        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarInviteRemovedAlert alert = new();
        alert.EventId = packet.ReadUInt64();
        alert.Date = clock.ToClient(packet.ReadUInt32());
        alert.Flags = packet.ReadUInt32();
        alert.Status = packet.ReadUInt8();
        SendPacketToClient(alert);
    }

    /// <summary><c>CalendarMgr::SendCalendarEventModeratorStatusAlert</c>.</summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_EVENT_MODERATOR_STATUS_ALERT)]
    internal void HandleCalendarEventModeratorStatusAlert(WorldPacket packet)
    {
        CalendarModeratorStatus status = new();
        status.InviteGuid = CalendarPlayer(packet.ReadPackedGuid());
        status.EventId = packet.ReadUInt64();
        status.Status = packet.ReadUInt8();
        status.ClearPending = packet.ReadBool();
        SendPacketToClient(status);
    }

    /// <summary><c>CalendarMgr::SendCalendarEventRemovedAlert</c>: the flag comes first.</summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_EVENT_REMOVED_ALERT)]
    internal void HandleCalendarEventRemovedAlert(WorldPacket packet)
    {
        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarEventRemovedAlert alert = new();
        alert.ClearPending = packet.ReadBool();
        alert.EventId = packet.ReadUInt64();
        alert.Date = clock.ToClient(packet.ReadUInt32());
        SendPacketToClient(alert);
    }

    /// <summary>
    /// <c>CalendarMgr::SendCalendarEventUpdateAlert</c>. It does not say whose guild the event is
    /// for; the client keeps what it knew.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_EVENT_UPDATED_ALERT)]
    internal void HandleCalendarEventUpdatedAlert(WorldPacket packet)
    {
        CalendarClock clock = CalendarClock.For(GetSession());
        CalendarEventUpdatedAlert alert = new();
        alert.ClearPending = packet.ReadBool();
        alert.EventId = packet.ReadUInt64();
        alert.OriginalDate = clock.ToClient(packet.ReadUInt32());
        alert.Flags = packet.ReadUInt32();
        alert.Date = clock.ToClient(packet.ReadUInt32());
        alert.EventType = packet.ReadUInt8();
        alert.TextureId = packet.ReadInt32();
        alert.EventName = packet.ReadCString();
        alert.Description = packet.ReadCString();
        SendPacketToClient(alert);
    }

    /// <summary>
    /// <c>CalendarMgr::SendCalendarCommandResult</c>: a zero, an empty text, the name some refusals
    /// speak of, and the reason last.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_COMMAND_RESULT)]
    internal void HandleCalendarCommandResult(WorldPacket packet)
    {
        CalendarCommandResult result = new();
        packet.ReadUInt32();
        packet.ReadCString();
        result.Name = packet.ReadCString();
        result.Result = ModernCalendarError(packet.ReadUInt32());
        result.Command = 1;     // as TrinityCore wotlk_classic sends it, for want of knowing better
        SendPacketToClient(result);
    }

    /// <summary>
    /// A legacy server's reason for refusing (AzerothCore and TrinityCore 4.3.4 <c>CalendarError</c>,
    /// numbered alike) as the 3.4.3 and 4.4.2 clients number it (<c>Enum.CalendarErrorType</c> of
    /// their interface code). The clients have one reason more in the eleventh place, "not in that
    /// community", so 10 to 36 are one higher there; before and after it the numbers agree.
    /// TrinityCore cata_classic sends the old numbers, and "no player of that name" then reads
    /// "already invited".
    /// </summary>
    internal static byte ModernCalendarError(uint error) => error switch
    {
        >= 10 and <= 36 => (byte)(error + 1),
        _ => (byte)error,
    };

    [HandlesSmsg(Opcode.SMSG_CALENDAR_CLEAR_PENDING_ACTION)]
    internal void HandleCalendarClearPendingAction(WorldPacket packet)
    {
        SendPacketToClient(new CalendarClearPendingAction());
    }

    /// <summary>
    /// <c>Guild::MassInviteToEvent</c>: the members a guild invitation picked, each with a number
    /// that AzerothCore leaves 0 and the 3.4.3 client reads as a level.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_FILTER_GUILD)]
    internal void HandleCalendarFilterGuild(WorldPacket packet)
    {
        uint count = packet.ReadUInt32();
        if (count > MaxCalendarEntries)
            return;

        CalendarCommunityInvite invites = new();
        for (uint i = 0; i < count; i++)
        {
            WowGuid128 member = CalendarPlayer(packet.ReadPackedGuid());
            byte level = packet.ReadUInt8();
            invites.Invites.Add((member, level));
        }
        SendPacketToClient(invites);
    }

    /// <summary>
    /// <c>WorldSession::SendCalendarRaidLockout</c>: a save made (the server's time first) or
    /// dropped (no time), as the map, difficulty, seconds left and the save's GUID.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_RAID_LOCKOUT_ADDED)]
    internal void HandleCalendarRaidLockoutAdded(WorldPacket packet)
    {
        uint serverTime = packet.ReadUInt32();
        uint mapId = packet.ReadUInt32();
        uint difficulty = packet.ReadUInt32();
        uint secondsLeft = packet.ReadUInt32();
        CalendarRaidLockout lockout = CalendarLockout(mapId, difficulty, secondsLeft, packet.ReadUInt64());

        CalendarRaidLockoutAdded added = new();
        added.InstanceId = lockout.InstanceId;
        added.ServerTime = serverTime;
        added.MapId = lockout.MapId;
        added.DifficultyId = lockout.DifficultyId;
        added.TimeRemaining = lockout.ExpireTime;
        SendPacketToClient(added);
    }

    [HandlesSmsg(Opcode.SMSG_CALENDAR_RAID_LOCKOUT_REMOVED)]
    internal void HandleCalendarRaidLockoutRemoved(WorldPacket packet)
    {
        uint mapId = packet.ReadUInt32();
        uint difficulty = packet.ReadUInt32();
        uint secondsLeft = packet.ReadUInt32();
        CalendarRaidLockout lockout = CalendarLockout(mapId, difficulty, secondsLeft, packet.ReadUInt64());

        CalendarRaidLockoutRemoved removed = new();
        removed.InstanceId = lockout.InstanceId;
        removed.MapId = lockout.MapId;
        removed.DifficultyId = lockout.DifficultyId;
        SendPacketToClient(removed);
    }

    /// <summary>
    /// <c>WorldSession::SendCalendarRaidLockoutUpdated</c>: the same five numbers on both sides,
    /// the difficulty renumbered.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CALENDAR_RAID_LOCKOUT_UPDATED)]
    internal void HandleCalendarRaidLockoutUpdated(WorldPacket packet)
    {
        CalendarRaidLockoutUpdated updated = new();
        updated.ServerTime = packet.ReadUInt32();
        uint mapId = packet.ReadUInt32();
        updated.MapId = (int)mapId;
        updated.DifficultyId = GameData.ModernInstanceDifficulty(mapId, packet.ReadUInt32()).DifficultyId;
        updated.OldTimeRemaining = packet.ReadInt32();
        updated.NewTimeRemaining = packet.ReadInt32();
        SendPacketToClient(updated);
    }
}
