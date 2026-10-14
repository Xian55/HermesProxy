using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's calendar CMSGs, for a server that has a calendar: 3.0 on.
/// The requests are AzerothCore's, TrinityCore's and TrinityCore 4.3.4's <c>HandleCalendar*</c>
/// readers, which agree on all of them but the report. The dates the client picks are in its own
/// time and go to the server in the server's (<see cref="CalendarClock"/>).
/// </summary>
public static class CalendarSystem
{
    private static bool ServerHasCalendar => LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056);

    /// <summary>
    /// Asked at login and whenever the calendar button is drawn anew. Empty on both sides.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_GET_NUM_PENDING)]
    public static void HandleCalendarGetNumPending(in EmptyClientPacket request, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_CALENDAR_GET_NUM_PENDING));
    }

    // AzerothCore CalendarMgr.h: an event repeats never, and takes a hundred invitations.
    private const byte RepeatNever = 0;
    private const uint MaxInvites = 100;

    // CALENDAR_FLAG_WITHOUT_INVITES: a guild announcement. Its request ends before the invitations.
    private const uint FlagWithoutInvites = 0x40;

    /// <summary>
    /// The calendar is opened, or the guild's news asked for (its list shows the guild's events).
    /// Empty on both sides; a 3.3.5a server has it as CMSG_CALENDAR_GET_CALENDAR.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_GET)]
    public static void HandleCalendarGet(in EmptyClientPacket request, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_CALENDAR_GET_CALENDAR));
    }

    [HandlesCmsg(Opcode.CMSG_CALENDAR_GET_EVENT)]
    public static void HandleCalendarGetEvent(in CalendarGetEvent request, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_GET_EVENT);
        packet.WriteUInt64(request.EventId);
        ctx.SendPacketToServer(packet);
    }

    /// <summary>
    /// <c>HandleCalendarAddEvent</c>: the two texts first, then the numbers, and the invitations
    /// last unless the event is a guild announcement.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_ADD_EVENT)]
    public static void HandleCalendarAddEvent(in CalendarAddEvent add, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_ADD_EVENT);
        WriteLegacyAddEvent(packet, add with { Time = CalendarClock.For(ctx.GetSession()).ToServer(add.Time) });
        ctx.SendPacketToServer(packet);
    }

    internal static void WriteLegacyAddEvent(WorldPacket packet, CalendarAddEvent add)
    {
        packet.WriteCString(add.Title);
        packet.WriteCString(add.Description);
        packet.WriteUInt8(add.EventType);
        packet.WriteUInt8(RepeatNever);
        packet.WriteUInt32(MaxInvites);
        packet.WriteInt32(add.TextureId);
        packet.WriteUInt32(add.Time);
        packet.WriteUInt32(0);      // a second date the server keeps and nothing reads
        packet.WriteUInt32(add.Flags);
        if ((add.Flags & FlagWithoutInvites) != 0)
            return;

        packet.WriteInt32(add.Invites.Length);
        foreach (CalendarAddEventInvite invite in add.Invites)
        {
            packet.WritePackedGuid(invite.Guid.To64());
            packet.WriteUInt8(invite.Status);
            packet.WriteUInt8(invite.Moderator);
        }
    }

    /// <summary>
    /// <c>HandleCalendarUpdateEvent</c>: the event and the sender's own invitation, then what the
    /// request that makes an event has, without invitations.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_UPDATE_EVENT)]
    public static void HandleCalendarUpdateEvent(in CalendarUpdateEvent update, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_UPDATE_EVENT);
        WriteLegacyUpdateEvent(packet, update with { Time = CalendarClock.For(ctx.GetSession()).ToServer(update.Time) });
        ctx.SendPacketToServer(packet);
    }

    internal static void WriteLegacyUpdateEvent(WorldPacket packet, CalendarUpdateEvent update)
    {
        packet.WriteUInt64(update.EventId);
        packet.WriteUInt64(update.ModeratorId);
        packet.WriteCString(update.Title);
        packet.WriteCString(update.Description);
        packet.WriteUInt8(update.EventType);
        packet.WriteUInt8(RepeatNever);
        packet.WriteUInt32(MaxInvites);
        packet.WriteInt32(update.TextureId);
        packet.WriteUInt32(update.Time);
        packet.WriteUInt32(0);
        packet.WriteUInt32(update.Flags);
    }

    /// <summary>AzerothCore's <c>HandleCalendarRemoveEvent</c> reads the event and no more.</summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_REMOVE_EVENT)]
    public static void HandleCalendarRemoveEvent(in CalendarRemoveEvent remove, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_REMOVE_EVENT);
        packet.WriteUInt64(remove.EventId);
        packet.WriteUInt64(remove.ModeratorId);
        packet.WriteUInt32(remove.Flags);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CALENDAR_COPY_EVENT)]
    public static void HandleCalendarCopyEvent(in CalendarCopyEvent copy, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_COPY_EVENT);
        packet.WriteUInt64(copy.EventId);
        packet.WriteUInt64(copy.ModeratorId);
        packet.WriteUInt32(CalendarClock.For(ctx.GetSession()).ToServer(copy.Date));
        ctx.SendPacketToServer(packet);
    }

    /// <summary>
    /// <c>HandleCalendarEventInvite</c>: a player invited by name, to an event that exists or to
    /// one still being made.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_INVITE)]
    public static void HandleCalendarInvite(in CalendarInvite invite, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_EVENT_INVITE);
        packet.WriteUInt64(invite.EventId);
        packet.WriteUInt64(invite.ModeratorId);
        packet.WriteCString(LegacyPlayerName.StripRealmSuffixToString(invite.Name));
        packet.WriteBool(invite.Creating);
        packet.WriteBool(invite.IsSignUp);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CALENDAR_EVENT_SIGN_UP)]
    public static void HandleCalendarEventSignUp(in CalendarEventSignUp signUp, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_EVENT_SIGN_UP);
        packet.WriteUInt64(signUp.EventId);
        packet.WriteBool(signUp.Tentative);
        ctx.SendPacketToServer(packet);
    }

    /// <summary><c>HandleCalendarEventRsvp</c>: the answer is 32 bits wide there.</summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_RSVP)]
    public static void HandleCalendarRsvp(in CalendarRsvp rsvp, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_EVENT_RSVP);
        packet.WriteUInt64(rsvp.EventId);
        packet.WriteUInt64(rsvp.InviteId);
        packet.WriteUInt32(rsvp.Status);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CALENDAR_REMOVE_INVITE)]
    public static void HandleCalendarRemoveInvite(in CalendarRemoveInvite remove, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_EVENT_REMOVE_INVITE);
        packet.WritePackedGuid(remove.Guid.To64());
        packet.WriteUInt64(remove.InviteId);
        packet.WriteUInt64(remove.ModeratorId);
        packet.WriteUInt64(remove.EventId);
        ctx.SendPacketToServer(packet);
    }

    /// <summary>
    /// A moderator answers for an invitee, or changes the invitee's rank. One layout on both sides
    /// (<c>HandleCalendarEventStatus</c>, <c>HandleCalendarEventModeratorStatus</c>).
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_STATUS)]
    public static void HandleCalendarStatus(in CalendarInviteeStatus status, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        ctx.SendPacketToServer(BuildLegacyInviteeStatus(Opcode.CMSG_CALENDAR_EVENT_STATUS, status));
    }

    [HandlesCmsg(Opcode.CMSG_CALENDAR_MODERATOR_STATUS)]
    public static void HandleCalendarModeratorStatus(in CalendarInviteeStatus status, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        ctx.SendPacketToServer(BuildLegacyInviteeStatus(Opcode.CMSG_CALENDAR_EVENT_MODERATOR_STATUS, status));
    }

    private static WorldPacket BuildLegacyInviteeStatus(Opcode opcode, CalendarInviteeStatus status)
    {
        WorldPacket packet = new WorldPacket(opcode);
        packet.WritePackedGuid(status.Guid.To64());
        packet.WriteUInt64(status.EventId);
        packet.WriteUInt64(status.InviteId);
        packet.WriteUInt64(status.ModeratorId);
        packet.WriteUInt8(status.Status);
        return packet;
    }

    /// <summary>
    /// A 3.3.5a client names the event before the player it reports (AzerothCore
    /// <c>CalendarComplain</c>, WowPacketParser before 4.3.4), and no invitation; a 4.3.4 one the
    /// player first, then the event and the invitation.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_COMPLAIN)]
    public static void HandleCalendarComplain(in CalendarComplain complain, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_COMPLAIN);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            packet.WriteGuid(complain.InvitedByGuid.To64());
            packet.WriteUInt64(complain.EventId);
            packet.WriteUInt64(complain.InviteId);
        }
        else
        {
            packet.WriteUInt64(complain.EventId);
            packet.WriteGuid(complain.InvitedByGuid.To64());
        }
        ctx.SendPacketToServer(packet);
    }

    /// <summary>
    /// "Invite the guild" in a new event: the server answers with the members that fit. A 3.3.5a
    /// server has it as CMSG_CALENDAR_GUILD_FILTER, three 32-bit numbers.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CALENDAR_COMMUNITY_INVITE)]
    public static void HandleCalendarCommunityInvite(in CalendarCommunityInviteRequest filter, in SessionContext ctx)
    {
        if (!ServerHasCalendar)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CALENDAR_GUILD_FILTER);
        packet.WriteUInt32(filter.MinLevel);
        packet.WriteUInt32(filter.MaxLevel);
        packet.WriteUInt32(filter.MaxRankOrder);
        ctx.SendPacketToServer(packet);
    }
}
