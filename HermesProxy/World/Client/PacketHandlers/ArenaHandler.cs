using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_ARENA_TEAM_QUERY_RESPONSE)]
    void HandleArenaTeamQueryResponse(WorldPacket packet)
    {
        uint teamId = packet.ReadUInt32();
        ArenaTeamData? team;
        if (!GetSession().GameState.ArenaTeams.TryGetValue(teamId, out team))
        {
            team = new ArenaTeamData();
            GetSession().GameState.ArenaTeams.Add(teamId, team);
        }

        team.Name = packet.ReadCString();
        team.TeamSize = packet.ReadUInt32();
        team.BackgroundColor = packet.ReadUInt32();
        team.EmblemStyle = packet.ReadUInt32();
        team.EmblemColor = packet.ReadUInt32();
        team.BorderStyle = packet.ReadUInt32();
        team.BorderColor = packet.ReadUInt32();

        // Forward it as well as caching it. The modern client is never told the team exists
        // otherwise: it does not send CMSG_ARENA_TEAM_QUERY on 3.4.3, which was the only thing
        // that turned this cache into a packet, so the roster below arrived describing members of
        // a team the client had no name, size or emblem for — and the arena panel rendered three
        // empty brackets. Legacy order is QUERY_RESPONSE -> STATS -> ROSTER, so sending here puts
        // the team identity ahead of its members.
        ArenaTeamQueryResponse response = new ArenaTeamQueryResponse();
        response.TeamId = teamId;
        response.Emblem = new ArenaTeamEmblem();
        response.Emblem.TeamId = teamId;
        response.Emblem.TeamSize = team.TeamSize;
        response.Emblem.BackgroundColor = team.BackgroundColor;
        response.Emblem.EmblemStyle = team.EmblemStyle;
        response.Emblem.EmblemColor = team.EmblemColor;
        response.Emblem.BorderStyle = team.BorderStyle;
        response.Emblem.BorderColor = team.BorderColor;
        response.Emblem.TeamName = team.Name;
        SendPacketToClient(response);
    }

    [PacketHandler(Opcode.SMSG_ARENA_TEAM_STATS)]
    void HandleArenaTeamStats(WorldPacket packet)
    {
        uint teamId = packet.ReadUInt32();
        ArenaTeamData? team;
        if (!GetSession().GameState.ArenaTeams.TryGetValue(teamId, out team))
        {
            team = new ArenaTeamData();
            GetSession().GameState.ArenaTeams.Add(teamId, team);
        }

        team.Rating = packet.ReadUInt32();
        team.WeekPlayed = packet.ReadUInt32();
        team.WeekWins = packet.ReadUInt32();
        team.SeasonPlayed = packet.ReadUInt32();
        team.SeasonWins = packet.ReadUInt32();
        team.Rank = packet.ReadUInt32();
    }

    [PacketHandler(Opcode.SMSG_ARENA_TEAM_ROSTER)]
    void HandleArenaTeamRoster(WorldPacket packet)
    {
        ArenaTeamRosterResponse arena = new ArenaTeamRosterResponse();
        arena.TeamId = packet.ReadUInt32();

        // The flag and the two per-member floats below arrived together in 3.0.8; reading the
        // flag without keeping it left the floats unconsumed, so on a 3.3.5a roster every member
        // after the first was read 8 bytes out of alignment.
        var hiddenRating = false;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
            hiddenRating = packet.ReadBool();

        var count = packet.ReadUInt32();
        arena.TeamSize = packet.ReadUInt32();

        for (var i = 0; i < count; i++)
        {
            ArenaTeamMember member = new ArenaTeamMember();
            PlayerCache cache = new PlayerCache();
            member.MemberGUID = packet.ReadGuid().To128(GetSession().GameState);
            member.Online = packet.ReadBool();
            member.Name = cache.Name = packet.ReadCString();
            member.Captain = packet.ReadInt32();
            member.Level = cache.Level = packet.ReadUInt8();
            member.ClassId = cache.ClassId = (Class)packet.ReadUInt8();
            GetSession().GameState.UpdatePlayerCache(member.MemberGUID, cache);
            member.WeekGamesPlayed = packet.ReadUInt32();
            member.WeekGamesWon = packet.ReadUInt32();
            member.SeasonGamesPlayed = packet.ReadUInt32();
            member.SeasonGamesWon = packet.ReadUInt32();
            member.PersonalRating = packet.ReadUInt32();
            if (hiddenRating)
            {
                // Hidden rating, see LUA GetArenaTeamGdfInfo - gdf = Gaussian Density Filter
                // Introduced in Patch 3.0.8
                member.dword60 = packet.ReadFloat();
                member.dword68 = packet.ReadFloat();
            }
            arena.Members.Add(member);
        }

        ArenaTeamData? team;
        if (GetSession().GameState.ArenaTeams.TryGetValue(arena.TeamId, out team))
        {
            arena.TeamPlayed = team.WeekPlayed;
            arena.TeamWins = team.WeekWins;
            arena.SeasonPlayed = team.SeasonPlayed;
            arena.SeasonWins = team.SeasonWins;
            arena.TeamRating = team.Rating;
            arena.PlayerRating = team.Rank;
        }

        SendPacketToClient(arena);
    }

    [PacketHandler(Opcode.SMSG_ARENA_TEAM_EVENT)]
    void HandleArenaTeamEvent(WorldPacket packet)
    {
        ArenaTeamEvent arena = new ArenaTeamEvent();
        var eventType = (ArenaTeamEventLegacy)packet.ReadUInt8();
        arena.Event = eventType.CastEnum<ArenaTeamEventModern>();
        byte count = packet.ReadUInt8();
        for (byte i = 0; i < count; i++)
        {
            string str = packet.ReadCString();
            switch (i)
            {
                case 0:
                    arena.Param1 = str;
                    break;
                case 1:
                    arena.Param2 = str;
                    break;
                case 2:
                    arena.Param3 = str;
                    break;
            }
        }
        if (packet.CanRead())
            packet.ReadGuid();
        SendPacketToClient(arena);
    }

    [PacketHandler(Opcode.SMSG_ARENA_TEAM_COMMAND_RESULT)]
    void HandleArenaTeamCommandResult(WorldPacket packet)
    {
        ArenaTeamCommandResult arena = new ArenaTeamCommandResult();
        arena.Action = (ArenaTeamCommandType)packet.ReadUInt32();
        arena.TeamName = packet.ReadCString();
        arena.PlayerName = packet.ReadCString();
        var errorType = (ArenaTeamCommandErrorLegacy)packet.ReadUInt32();
        arena.Error = errorType.CastEnum<ArenaTeamCommandErrorModern>();
        SendPacketToClient(arena);
    }

    [PacketHandler(Opcode.SMSG_ARENA_TEAM_INVITE)]
    void HandleArenaTeamInvite(WorldPacket packet)
    {
        ArenaTeamInvite arena = new ArenaTeamInvite();
        arena.PlayerName = packet.ReadCString();
        arena.TeamName = packet.ReadCString();
        arena.PlayerGuid = GetSession().GameState.GetPlayerGuidByName(arena.PlayerName);
        arena.PlayerVirtualAddress = GetSession().RealmId.GetAddress();
        arena.TeamGuid = WowGuid128.Create(HighGuidType703.ArenaTeam, 1);
        SendPacketToClient(arena);
    }
}
