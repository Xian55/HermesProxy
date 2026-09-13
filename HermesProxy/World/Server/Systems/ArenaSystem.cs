using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's arena-team CMSGs.
/// </summary>
/// <remarks>
/// Three handlers here are shape B, each serving a pair of opcodes and forwarding the one it was
/// dispatched with: remove/promote, disband/leave, and accept/decline. That makes the
/// universal-to-legacy opcode table the thing deciding what the server does — a wrong entry
/// produces a well-formed packet of the right size that performs a different action — so the arena
/// block is pinned in ShapeBOpcodeForwardingTests.
/// <para>
/// Two of these never reach the legacy server. The roster request answers locally when the player
/// has no team on that bracket (or the backend predates arenas), and the team query is served
/// entirely from cached <c>ArenaTeams</c> state.
/// </para>
/// </remarks>
public static class ArenaSystem
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(WorldSocket).PadRight(15);
    private static readonly string _netDirSend = Log.FormatDir(LogNetDir.P2S);

    [HandlesCmsg(Opcode.CMSG_ARENA_TEAM_ROSTER)]
    public static void HandleArenaTeamRoster(in ArenaTeamRosterRequest arena, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) ||
            ctx.GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex] == 0)
        {
            ArenaTeamRosterResponse response = new ArenaTeamRosterResponse();
            response.TeamSize = ModernVersion.GetArenaTeamSizeFromIndex(arena.TeamIndex);
            ctx.SendPacket(response);
        }
        else
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ARENA_TEAM_QUERY);
            packet.WriteUInt32(ctx.GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex]);
            ctx.SendPacketToServer(packet);

            WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ARENA_TEAM_ROSTER);
            packet2.WriteUInt32(ctx.GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex]);
            ctx.SendPacketToServer(packet2);
        }
    }

    [HandlesCmsg(Opcode.CMSG_ARENA_TEAM_QUERY)]
    public static void HandleArenaTeamQuery(in ArenaTeamQuery arena, in SessionContext ctx)
    {
        ArenaTeamData? team;
        if (ctx.GetSession().GameState.ArenaTeams.TryGetValue(arena.TeamId, out team))
        {
            ArenaTeamQueryResponse response = new ArenaTeamQueryResponse();
            response.TeamId = arena.TeamId;
            response.Emblem = new ArenaTeamEmblem();
            response.Emblem.TeamId = arena.TeamId;
            response.Emblem.TeamSize = team.TeamSize;
            response.Emblem.BackgroundColor = team.BackgroundColor;
            response.Emblem.EmblemStyle = team.EmblemStyle;
            response.Emblem.EmblemColor = team.EmblemColor;
            response.Emblem.BorderStyle = team.BorderStyle;
            response.Emblem.BorderColor = team.BorderColor;
            response.Emblem.TeamName = team.Name;
            ctx.SendPacket(response);
        }
    }

    /// <remarks>
    /// Answered entirely from session state — the legacy server has no equivalent request, because
    /// 3.3.5a has no personal rated system to ask about. The values come from the arena-team player
    /// fields, which UpdateHandler mirrors into the bracket cache as they arrive.
    /// <para>
    /// The client re-asks every time the PvP panel opens and will not draw its bracket tiles until
    /// it gets an answer, so this must reply even when the player has no team at all: seven empty
    /// brackets is a valid answer and silence is not.
    /// </para>
    /// <para>
    /// A bracket whose every value is zero is drawn as no bracket at all — verified against a live
    /// 3.4.3 client, which left the tile blank on a real team sitting at rating 0 and drew it as
    /// soon as the reply carried a rating. That is the client reading zero as "never played", not
    /// a gap here: the native server sends the same zeros, and AzerothCore only starts teams at 0
    /// because Arena.ArenaStartRating defaults to it.
    /// </para>
    /// </remarks>
    [HandlesCmsg(Opcode.CMSG_REQUEST_RATED_PVP_INFO)]
    public static void HandleRequestRatedPvpInfo(in RequestRatedPvpInfo request, in SessionContext ctx)
    {
        RatedPvpInfo response = new RatedPvpInfo();
        response.Brackets = ctx.GetSession().GameState.CurrentArenaBrackets;
        ctx.SendPacket(response);
    }

    [HandlesCmsg(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA)]
    public static void HandleBattlematerJoinArena(in BattlemasterJoinArena join, in SessionContext ctx)
    {
        // 3.4.3 has no invite opcode, so rated Join as Group injects CMSG_ARENA_TEAM_INVITE.
        uint teamId = join.TeamIndex < ctx.GetSession().GameState.CurrentArenaTeamIds.Length
            ? ctx.GetSession().GameState.CurrentArenaTeamIds[join.TeamIndex]
            : 0;
        if (teamId != 0)
            InvitePartyToArenaTeam(in ctx, teamId);

        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA);
        packet.WriteGuid(join.Guid.To64());
        packet.WriteUInt8(join.TeamIndex);
        packet.WriteBool(true); // As Group
        packet.WriteBool(true); // Is Rated
        WorldSocketLogMessages.BattlemasterJoinArena(
            _melLog, _sourceFile, _netDirSend, join.TeamIndex, teamId);
        ctx.SendPacketToServer(packet);
    }

    static void InvitePartyToArenaTeam(in SessionContext ctx, uint teamId)
    {
        var group = ctx.GetSession().GameState.GetCurrentGroup();
        if (group == null)
            return;

        var self = ctx.GetSession().GameState.CurrentPlayerGuid;
        foreach (var member in group.PlayerList)
        {
            if (member.GUID == self)
                continue;

            string name = member.Name;
            if (string.IsNullOrEmpty(name))
                name = ctx.GetSession().GameState.GetPlayerName(member.GUID);
            if (string.IsNullOrEmpty(name))
                continue;

            WorldPacket invite = new WorldPacket(Opcode.CMSG_ARENA_TEAM_INVITE);
            invite.WriteUInt32(teamId);
            invite.WriteCString(name);
            ctx.SendPacketToServer(invite);
            WorldSocketLogMessages.ArenaTeamPartyInvite(
                _melLog, _sourceFile, _netDirSend, teamId, name);
        }
    }

    [HandlesCmsg(Opcode.CMSG_BATTLEMASTER_JOIN_SKIRMISH)]
    public static void HandleBattlematerJoinSkirmish(in BattlemasterJoinSkirmish join, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA);
        packet.WriteGuid(join.Guid.To64());
        packet.WriteUInt8(join.TeamSize);
        packet.WriteBool(join.AsGroup);
        packet.WriteBool(false); // Is Rated
        WorldSocketLogMessages.BattlemasterJoinSkirmish(
            _melLog, _sourceFile, _netDirSend, join.TeamSize, join.AsGroup);
        ctx.SendPacketToServer(packet);
    }

    /// <remarks>
    /// Named HandleArenaUnimplemented before the conversion, which it never was — it forwards both
    /// kick and promote, each carrying the target's name resolved from the GUID. The body is
    /// unchanged; only the name is.
    /// </remarks>
    [HandlesCmsg(Opcode.CMSG_ARENA_TEAM_REMOVE)]
    [HandlesCmsg(Opcode.CMSG_ARENA_TEAM_LEADER)]
    public static void HandleArenaTeamMemberCommand(Opcode opcode, in ArenaTeamRemove arena, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteUInt32(arena.TeamId);
        packet.WriteCString(ctx.GetSession().GameState.GetPlayerName(arena.PlayerGuid));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ARENA_TEAM_DISBAND)]
    [HandlesCmsg(Opcode.CMSG_ARENA_TEAM_LEAVE)]
    public static void HandleArenaTeamLeave(Opcode opcode, in ArenaTeamLeave arena, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteUInt32(arena.TeamId);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ARENA_TEAM_ACCEPT)]
    [HandlesCmsg(Opcode.CMSG_ARENA_TEAM_DECLINE)]
    public static void HandleArenaTeamInviteResponse(Opcode opcode, in ArenaTeamAccept arena, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        ctx.SendPacketToServer(packet);
    }
}
