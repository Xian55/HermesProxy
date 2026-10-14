using System.Collections.Generic;
using Bgs.Protocol;
using Bgs.Protocol.Club.V1;
using Framework.Constants;
using Framework.IO;
using Framework.Logging;
using Google.Protobuf;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Server.Packets;
using Membership = Bgs.Protocol.Club.V1.Membership;

namespace HermesProxy.World;

/// <summary>
/// The player's guild as a Battle.net club, which is how the 4.4.2 client knows its guild. Its
/// guild window is the Communities frame and opens only on a guild club, and the roster in it, the
/// right-click promote, demote and remove and their slash commands all find a member among the
/// club's members. A legacy server has no clubs, so the proxy makes one out of what it knows of the
/// guild: its id and name, and the members of the last roster.
/// </summary>
/// <remarks>
/// Modelled on TrinityCore cata_classic's ClubMembershipService, ClubService and the club
/// notifications in its Guild.cpp, and checked against that server's packets. The club id is the
/// guild id and a member's id the player's realm and counter (ClubUtils CreateClubMemberId). The
/// client gets the club in the answer to its membership subscription when the guild's name is known
/// by then, else in a club-added notification as soon as it is. Once it has subscribed to the club
/// itself, each roster is compared with what it was last told, and the difference goes out as
/// member added, removed and role changed notifications.
/// </remarks>
internal sealed class GuildClub
{
    // The default club roles, as TrinityCore numbers them for a guild.
    private const uint RoleOwner = 1;
    private const uint RoleLeader = 2;
    private const uint RoleModerator = 3;
    private const uint RoleMember = 4;

    private const ulong GuildStreamId = 1;
    private const ulong OfficerStreamId = 2;

    // GR_RIGHT_OFFCHATLISTEN | GR_RIGHT_OFFCHATSPEAK: a rank with either is a moderator of the club,
    // which is what lets it see the officer stream.
    private const uint OfficerChatRights = 0x04 | 0x08;

    // Before a roster the rank of a member who has just joined is not known; it is no officer rank.
    private const int UnknownRank = -1;

    private static readonly Microsoft.Extensions.Logging.ILogger _log = Log.CreateMelLogger(Log.CategoryServer);

    /// <summary>Whether this proxy's client takes its guild as a club.</summary>
    public static bool Served => ModernVersion.AddedInVersion(4, 4, 0);

    /// <summary>
    /// The player's id as a club member, which the 4.4.2 client also finds in the character list,
    /// the player object, name answers and roster entries. 0 for the clients without clubs.
    /// </summary>
    public static ulong MemberIdOf(WowGuid128 player)
        => Served && player != default ? ((ulong)(player.GetRealmId() & 0xFFF) << 48) | player.GetCounter() : 0;

    private readonly record struct ClubMember(int RankId, bool Online, string Note);

    private readonly GameSessionData _game;

    // What the client has been told: that it is subscribed to its memberships, the club it was
    // given (0 for none), whether it subscribed to that club, and the role of every member it was
    // given.
    private bool _membershipSubscribed;
    private ulong _givenClub;
    private bool _clubSubscribed;
    private readonly Dictionary<WowGuid128, uint> _givenRoles = [];

    // What the legacy server has told the proxy.
    private uint _rosterGuild;
    private bool _rosterKnown;
    private readonly Dictionary<WowGuid128, ClubMember> _roster = [];
    private readonly Dictionary<int, uint> _rankRights = [];
    private uint _createTime;
    private ClubRemovedReason _leaveReason = ClubRemovedReason.MemberLeft;

    // Lines this client sent through the guild window whose echo has not come back yet.
    private readonly List<string> _ownLines = [];

    private uint _notificationToken;

    public GuildClub(GameSessionData game)
    {
        _game = game;
    }

    private WowGuid128 Self => _game.CurrentPlayerGuid;
    private uint GuildId => _game.GetPlayerGuildId(Self);

    /// <summary>ClubMembershipService.Subscribe: the client's guild club, if it has one yet.</summary>
    public void SubscribeMemberships(Membership.SubscribeResponse response)
    {
        _membershipSubscribed = true;
        _givenClub = 0;
        _clubSubscribed = false;
        _givenRoles.Clear();

        // Not in a guild, a stock realm answers with nothing at all, not even an empty state.
        if (!TryDescribe(out ClubDescription? club))
        {
            if (GuildId != 0)
                GuildClubLogMessages.NameNotKnownYet(_log, GuildId);
            return;
        }

        response.State = new ClubMembershipState
        {
            MentionView = new ViewMarker { LastReadTime = 0, LastMessageTime = 0 },
        };
        response.State.Description.Add(new ClubMembershipDescription { MemberId = IdOf(Self), Club = club });
        _givenClub = club.Id;
        GuildClubLogMessages.ClubGiven(_log, club.Id, club.Name, "membership answer");
    }

    public void UnsubscribeMemberships()
    {
        _membershipSubscribed = false;
        _givenClub = 0;
        _clubSubscribed = false;
        _givenRoles.Clear();
    }

    /// <summary>ClubService.Subscribe: the club itself and the player as its member.</summary>
    public BattlenetRpcErrorCode Subscribe(ulong clubId)
    {
        if (clubId == 0 || clubId != _givenClub || !TryDescribe(out ClubDescription? description))
        {
            GuildClubLogMessages.RequestForUnknownClub(_log, "Subscribe", clubId, _givenClub);
            return BattlenetRpcErrorCode.Denied;
        }

        Club club = new()
        {
            Id = clubId,
            Type = GuildClubType(),
            Name = description.Name,
            PrivacyLevel = PrivacyLevel.Open,
            VisibilityLevel = VisibilityLevel.Private,
            MemberCount = description.MemberCount,
        };
        club.Leader.Add(description.Leader);

        uint role = RoleOf(_roster.TryGetValue(Self, out ClubMember self) ? self.RankId : UnknownRank);
        Member member = new()
        {
            Id = IdOf(Self),
            PresenceLevel = PresenceLevel.Rich,
            WhisperLevel = WhisperLevel.Open,
            Active = true,
        };
        member.Role.Add(role);

        Notify(OriginalHash.ClubListener, (uint)ClubListenerMethods.OnSubscribe,
            new SubscribeNotification { AgentId = IdOf(Self), ClubId = clubId, Club = club, Member = member });

        SubscriberStateChangedNotification state = new() { ClubId = clubId };
        state.Assignment.Add(new SubscriberStateAssignment { MemberId = IdOf(Self), Active = true });
        Notify(OriginalHash.ClubListener, (uint)ClubListenerMethods.OnSubscriberStateChanged, state);

        _clubSubscribed = true;
        _givenRoles[Self] = role;

        // Without a roster the club has only the player in it until the client asks for one,
        // which it does when its guild window opens.
        if (!_rosterKnown)
            RequestRoster();
        return BattlenetRpcErrorCode.Ok;
    }

    private void RequestRoster()
    {
        // The older servers send the creation date and the account count apart from the roster.
        if (!Client.WorldClient.IsCataLegacy)
            _game.GlobalSession.ToServer.Send(new WorldPacket(Opcode.CMSG_GUILD_INFO));
        _game.GlobalSession.ToServer.Send(new WorldPacket(Opcode.CMSG_GUILD_GET_ROSTER));
    }

    public void Unsubscribe(ulong clubId)
    {
        if (clubId == _givenClub)
            _clubSubscribed = false;
    }

    public static bool IsGuildClubType(UniqueClubType? type) => type != null && type.Name == "guild";

    public static UniqueClubType GuildClubType() => new() { Program = 5730135, Name = "guild" }; // "WoW"

    /// <summary>ClubService.GetMembers: everyone in the last roster, or just the player before one.</summary>
    public BattlenetRpcErrorCode GetMembers(ulong clubId, GetMembersResponse response)
    {
        if (clubId == 0 || clubId != _givenClub)
        {
            GuildClubLogMessages.RequestForUnknownClub(_log, "GetMembers", clubId, _givenClub);
            return BattlenetRpcErrorCode.Denied;
        }

        _givenRoles.Clear();
        foreach (var (guid, member) in _roster)
        {
            response.Member.Add(MemberOf(guid, member));
            _givenRoles[guid] = RoleOf(member.RankId);
        }
        if (!_roster.ContainsKey(Self))
        {
            ClubMember self = new(UnknownRank, true, "");
            response.Member.Add(MemberOf(Self, self));
            _givenRoles[Self] = RoleOf(self.RankId);
        }

        GuildClubLogMessages.MembersGiven(_log, clubId, response.Member.Count, _rosterKnown);
        return BattlenetRpcErrorCode.Ok;
    }

    /// <summary>ClubService.GetStreams: the guild and officer chat, as a stock realm has them.</summary>
    public BattlenetRpcErrorCode GetStreams(ulong clubId, GetStreamsResponse response)
    {
        if (clubId == 0 || clubId != _givenClub)
        {
            GuildClubLogMessages.RequestForUnknownClub(_log, "GetStreams", clubId, _givenClub);
            return BattlenetRpcErrorCode.Denied;
        }

        response.Stream.Add(StreamOf(clubId, GuildStreamId, "Guild", "COMMUNITIES_GUILD_GENERAL_CHANNEL_NAME",
            [RoleOwner, RoleLeader, RoleModerator, RoleMember]));
        response.Stream.Add(StreamOf(clubId, OfficerStreamId, "Officer", "COMMUNITIES_GUILD_OFFICER_CHANNEL_NAME",
            [RoleOwner, RoleLeader, RoleModerator]));
        response.View.Add(new StreamView { ClubId = clubId, StreamId = GuildStreamId });
        response.View.Add(new StreamView { ClubId = clubId, StreamId = OfficerStreamId });
        return BattlenetRpcErrorCode.Ok;
    }

    public static bool IsGuildStream(ulong streamId) => streamId is GuildStreamId or OfficerStreamId;

    /// <summary>
    /// ClubService.CreateMessage: a line typed into the guild window's chat. It goes to the legacy
    /// server as guild or officer chat, as a stock realm hands it to its chat handler, and comes
    /// back to the client as the stream's message. Without the message in the answer the client
    /// shows an empty line with no author.
    /// </summary>
    public BattlenetRpcErrorCode CreateMessage(ulong clubId, ulong streamId, string content, CreateMessageResponse response)
    {
        if (!IsGivenClub(clubId) || !IsGuildStream(streamId))
        {
            GuildClubLogMessages.RequestForUnknownClub(_log, "CreateMessage", clubId, _givenClub);
            return BattlenetRpcErrorCode.Denied;
        }
        if (content.Length == 0)
            return BattlenetRpcErrorCode.Denied;

        // A legacy server takes no message in the universal language from a player.
        Race race = _game.CurrentPlayerInfo?.RaceId ?? Race.None;
        uint language = (uint)(GameData.IsHordeRace(race) ? Language.Orcish : Language.Common);
        Server.Systems.ChatSystem.SendToLegacy(_game.GlobalSession,
            streamId == OfficerStreamId ? ChatMessageTypeModern.Officer : ChatMessageTypeModern.Guild, language, content);

        _ownLines.Add(content);
        if (_ownLines.Count > 32)
            _ownLines.RemoveAt(0);
        response.Message = MessageOf(Self, content);
        return BattlenetRpcErrorCode.Ok;
    }

    /// <summary>
    /// A guild or officer chat line from the legacy server. The guild window shows only its
    /// stream's messages, so each line becomes one, as on a stock realm a line typed into another
    /// member's guild window does; there it is only those, but a legacy server cannot tell the two
    /// apart. The echo of this client's own guild-window line is left out: the answer to it already
    /// put the line in the stream.
    /// </summary>
    public void ChatReceived(bool officer, WowGuid128 sender, string text)
    {
        if (!Served || !_clubSubscribed)
            return;

        if (sender == Self && _ownLines.Count > 0 && _ownLines[0].StartsWith(text, System.StringComparison.Ordinal))
        {
            // A long line reaches the server in parts, each echoed on its own.
            string rest = _ownLines[0][text.Length..].TrimStart();
            if (rest.Length == 0)
                _ownLines.RemoveAt(0);
            else
                _ownLines[0] = rest;
            return;
        }

        StreamMessageAddedNotification added = new()
        {
            AgentId = IdOf(sender),
            ClubId = _givenClub,
            StreamId = officer ? OfficerStreamId : GuildStreamId,
            Message = MessageOf(sender, text),
        };
        Notify(OriginalHash.ClubListener, (uint)ClubListenerMethods.OnStreamMessageAdded, added);
    }

    private StreamMessage MessageOf(WowGuid128 author, string text)
    {
        ulong now = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000;
        StreamMessage message = new()
        {
            Id = new MessageId { Epoch = now, Position = 0 },
            Author = new MemberDescription { Id = IdOf(author) },
        };
        message.ContentChain.Add(new ContentChain { Content = text, EditTime = now });
        return message;
    }

    /// <summary>A member's rank in the last roster, or -1 when the proxy has none for them.</summary>
    public int RankOf(WowGuid128 player)
        => _rosterKnown && _roster.TryGetValue(player, out ClubMember member) ? member.RankId : -1;

    public bool IsGivenClub(ulong clubId) => clubId != 0 && clubId == _givenClub;

    /// <summary>
    /// The guild may have become known, changed or gone: the player's guild id, the guild's name or
    /// a roster arrived. Gives the client the club it lacks or takes away the one it no longer has.
    /// </summary>
    public void Sync()
    {
        if (!Served)
            return;

        uint guildId = GuildId;
        if (_rosterGuild != guildId)
        {
            _rosterGuild = guildId;
            _rosterKnown = false;
            _roster.Clear();
            _rankRights.Clear();
            _createTime = 0;
        }

        if (!_membershipSubscribed)
            return;

        if (_givenClub != 0 && _givenClub != guildId)
            Withdraw();

        if (_givenClub == 0 && TryDescribe(out ClubDescription? club))
        {
            Membership.ClubAddedNotification added = new()
            {
                Membership = new ClubMembershipDescription { MemberId = IdOf(Self), Club = club },
            };
            Notify(OriginalHash.ClubMembershipListener, (uint)ClubMembershipListenerMethods.OnClubAdded, added);
            _givenClub = club.Id;
            _leaveReason = ClubRemovedReason.MemberLeft;
            GuildClubLogMessages.ClubGiven(_log, club.Id, club.Name, "club added");
        }
    }

    /// <summary>A whole roster: the members to compare the client's club with from now on.</summary>
    public void RosterReceived(GuildRoster roster)
    {
        if (!Served)
            return;

        Sync();
        _roster.Clear();
        foreach (GuildRosterMemberData member in roster.MemberData)
            _roster[member.Guid] = new ClubMember(member.RankID, (member.Status & 1) != 0, member.Note);
        _rosterKnown = true;
        if (roster.CreateDate != 0)
            _createTime = roster.CreateDate;

        // A guild just created gets its name only now on some paths; the club follows it.
        Sync();
        PublishChanges(ClubRemovedReason.MemberLeft);
    }

    /// <summary>The rights of each rank, which make a member a moderator of the club or not.</summary>
    public void RanksReceived(GuildRanks ranks)
    {
        if (!Served)
            return;

        _rankRights.Clear();
        foreach (GuildRankData rank in ranks.Ranks)
            _rankRights[rank.RankID] = rank.Flags;
        PublishChanges(ClubRemovedReason.MemberLeft);
    }

    public void MemberJoined(WowGuid128 player)
    {
        if (!Served || player == default)
            return;

        if (player == Self)
        {
            Sync();
            return;
        }
        if (!_rosterKnown || _roster.ContainsKey(player))
            return;
        _roster[player] = new ClubMember(UnknownRank, true, "");
        PublishChanges(ClubRemovedReason.MemberLeft);
    }

    public void MemberLeft(WowGuid128 player, bool removed)
    {
        if (!Served || player == default)
            return;

        ClubRemovedReason reason = removed ? ClubRemovedReason.MemberKicked : ClubRemovedReason.MemberLeft;
        if (player == Self)
        {
            // The guild id goes to 0 in an update of its own, which is when the club goes.
            _leaveReason = reason;
            return;
        }
        if (_roster.Remove(player))
            PublishChanges(reason);
    }

    public void Disbanded()
    {
        if (Served)
            _leaveReason = ClubRemovedReason.DestroyedByMember;
    }

    public void MemberRankChanged(WowGuid128 player, int rankId)
    {
        if (!Served || !_roster.TryGetValue(player, out ClubMember member))
            return;
        _roster[player] = member with { RankId = rankId };
        PublishChanges(ClubRemovedReason.MemberLeft);
    }

    public void LeaderChanged(WowGuid128 oldLeader, WowGuid128 newLeader)
    {
        if (!Served)
            return;

        // The roster that follows has the old leader's real rank; until then it is the one below.
        if (_roster.TryGetValue(oldLeader, out ClubMember old))
            _roster[oldLeader] = old with { RankId = 1 };
        if (_roster.TryGetValue(newLeader, out ClubMember leader))
            _roster[newLeader] = leader with { RankId = 0 };
        PublishChanges(ClubRemovedReason.MemberLeft);
    }

    private void Withdraw()
    {
        ulong clubId = _givenClub;
        if (_clubSubscribed)
            Notify(OriginalHash.ClubListener, (uint)ClubListenerMethods.OnUnsubscribe,
                new UnsubscribeNotification { ClubId = clubId });

        Notify(OriginalHash.ClubMembershipListener, (uint)ClubMembershipListenerMethods.OnClubRemoved,
            new Membership.ClubRemovedNotification { MemberId = IdOf(Self), ClubId = clubId, Reason = _leaveReason });

        GuildClubLogMessages.ClubWithdrawn(_log, clubId, _leaveReason);
        _givenClub = 0;
        _clubSubscribed = false;
        _givenRoles.Clear();
        _leaveReason = ClubRemovedReason.MemberLeft;
    }

    /// <summary>Tells a subscribed client how the roster differs from the members it was given.</summary>
    private void PublishChanges(ClubRemovedReason removedReason)
    {
        if (!_clubSubscribed || !_rosterKnown)
            return;

        ulong clubId = _givenClub;
        MemberAddedNotification added = new() { ClubId = clubId };
        MemberRoleChangedNotification roles = new() { ClubId = clubId };
        foreach (var (guid, member) in _roster)
        {
            uint role = RoleOf(member.RankId);
            if (!_givenRoles.TryGetValue(guid, out uint given))
                added.Member.Add(MemberOf(guid, member));
            else if (given != role)
            {
                RoleAssignment assignment = new() { MemberId = IdOf(guid) };
                assignment.Role.Add(role);
                roles.Assignment.Add(assignment);
            }
            else
                continue;
            _givenRoles[guid] = role;
        }

        MemberRemovedNotification removed = new() { ClubId = clubId };
        List<WowGuid128>? gone = null;
        foreach (WowGuid128 guid in _givenRoles.Keys)
        {
            if (!_roster.ContainsKey(guid))
                (gone ??= []).Add(guid);
        }
        if (gone != null)
        {
            foreach (WowGuid128 guid in gone)
            {
                removed.Member.Add(new MemberRemovedAssignment { Id = IdOf(guid), Reason = removedReason });
                _givenRoles.Remove(guid);
            }
        }

        if (added.Member.Count > 0)
            Notify(OriginalHash.ClubListener, (uint)ClubListenerMethods.OnMemberAdded, added);
        if (removed.Member.Count > 0)
            Notify(OriginalHash.ClubListener, (uint)ClubListenerMethods.OnMemberRemoved, removed);
        if (roles.Assignment.Count > 0)
            Notify(OriginalHash.ClubListener, (uint)ClubListenerMethods.OnMemberRoleChanged, roles);

        if (added.Member.Count + removed.Member.Count + roles.Assignment.Count > 0)
            GuildClubLogMessages.MembersChanged(_log, clubId, added.Member.Count, removed.Member.Count, roles.Assignment.Count);
    }

    /// <summary>The guild as a club description, once both its id and its name are known.</summary>
    private bool TryDescribe([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ClubDescription? club)
    {
        club = null;
        uint guildId = GuildId;
        if (guildId == 0)
            return false;

        string? name = GuildName(guildId);
        if (name == null)
            return false;

        club = new ClubDescription
        {
            Id = guildId,
            Type = GuildClubType(),
            Name = name,
            PrivacyLevel = PrivacyLevel.Open,
            VisibilityLevel = VisibilityLevel.Private,
            MemberCount = (uint)(_roster.Count > 0 ? _roster.Count : 1),
            // Without these two a stock realm's guild clubs misbehave, as its ClubMembershipService notes.
            Timezone = "",
            Locale = "",
        };
        if (_createTime != 0)
            club.CreationTime = (ulong)_createTime * 1_000_000;
        foreach (var (guid, member) in _roster)
        {
            if (member.RankId == 0)
                club.Leader.Add(new MemberDescription { Id = IdOf(guid) });
        }
        return true;
    }

    private string? GuildName(uint guildId)
    {
        WowGuid128 guild = WowGuid128.Create(HighGuidType703.Guild, guildId);
        foreach (var (name, guid) in _game.GlobalSession.GuildsByName)
        {
            if (guid == guild)
                return name;
        }
        return null;
    }

    private uint RoleOf(int rankId)
    {
        if (rankId == 0)
            return RoleOwner;
        return _rankRights.TryGetValue(rankId, out uint rights) && (rights & OfficerChatRights) != 0
            ? RoleModerator
            : RoleMember;
    }

    private Member MemberOf(WowGuid128 guid, ClubMember member)
    {
        Member result = new()
        {
            Id = IdOf(guid),
            PresenceLevel = PresenceLevel.Rich,
            WhisperLevel = WhisperLevel.Open,
            Note = member.Note,
            Active = member.Online,
        };
        result.Role.Add(RoleOf(member.RankId));
        return result;
    }

    // The account is the one name answers give the player, the session's own for its characters.
    private MemberId IdOf(WowGuid128 player) => new()
    {
        UniqueId = MemberIdOf(player),
        AccountId = _game.GlobalSession.GetGameAccountGuidForPlayer(player).GetCounter(),
    };

    private static Stream StreamOf(ulong clubId, ulong streamId, string name, string globalStringsTag, uint[] roles)
    {
        Stream stream = new()
        {
            ClubId = clubId,
            Id = streamId,
            Name = name,
            Access = new StreamAccess(),
            VoiceLevel = StreamVoiceLevel.VoiceLevelDisabled,
        };
        stream.Attribute.Add(new Bgs.Protocol.V2.Attribute
        {
            Name = "global_strings_tag",
            Value = new Bgs.Protocol.V2.Variant { StringValue = globalStringsTag },
        });
        stream.Access.Role.Add(roles);
        return stream;
    }

    private void Notify(OriginalHash listener, uint methodId, IMessage message)
    {
        MethodCall method = new();
        method.SetServiceHash((uint)listener);
        // The two top bits are the client and server flags a stock realm sets on every listener call.
        method.SetMethodId(methodId | 0xC0000000);
        method.ObjectId = 1;
        method.Token = _notificationToken++;
        if (_log.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
            GuildClubLogMessages.NotificationSent(_log, listener, methodId, method.Token, message.ToString() ?? "");
        _game.GlobalSession.ToClient.Send(new BattlenetNotification
        {
            Method = method,
            Data = new ByteBuffer(message.ToByteArray()),
        });
    }
}
