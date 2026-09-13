/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using HermesProxy.Enums;
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HermesProxy.World.Server.Packets;

/// <summary>
/// Gate for SMSG_PVP_SEASON, whose V3_4_3 layout was corrected against the native captures under
/// <c>refs/native-captures/</c> (25 bytes, six int32).
/// </summary>
/// <remarks>
/// <c>ModernVersion.Build</c> is <see langword="static readonly"/> and the test process runs as one
/// fixed build, so <c>SpanWriteParityTests</c> cannot otherwise reach the V3_4_3 arms — which is
/// precisely where a Write/WriteToSpan pair drifts unnoticed (#156, #162). Production never sets it.
/// </remarks>
internal static class PvpWire
{
    internal static bool? ForceV343ForTests;

    public static bool IsV343 =>
        ForceV343ForTests ?? ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;
}

public readonly record struct ArenaTeamRosterRequest(uint TeamIndex);

public readonly record struct ArenaTeamQuery(uint TeamId);

/// <summary>CMSG_REQUEST_RATED_PVP_INFO carries no fields; the native Read() body is empty too.</summary>
public readonly record struct RequestRatedPvpInfo;

class ArenaTeamRosterResponse : ServerPacket, ISpanWritable
{
    public ArenaTeamRosterResponse() : base(Opcode.SMSG_ARENA_TEAM_ROSTER) { }

    /// <remarks>
    /// The leading field is the team id, and the member records do belong on the wire, despite
    /// both looking wrong against the native 3.4.3 source. Wrathion names that field TeamSlot and
    /// writes a fixed 37-byte header with no members
    /// (<c>WorldPackets::Arena::ArenaTeamRosterResponse::Write</c>), and the native captures under
    /// <c>refs/native-captures/</c> agree — but Wrathion's handler is a stub that answers every
    /// bracket with an empty team and leaves that field at 0, so it never exercises either
    /// question. Sending the bracket index there was tried on a live 3.4.3 client against a real
    /// team and the bracket went from "team exists" to empty: the team sat in bracket 0, so the
    /// index written was the literal 0 the client reads as "no team".
    /// </remarks>
    public override void Write()
    {
        _worldPacket.WriteUInt32(TeamId);
        _worldPacket.WriteUInt32(TeamSize);
        _worldPacket.WriteUInt32(TeamPlayed);
        _worldPacket.WriteUInt32(TeamWins);
        _worldPacket.WriteUInt32(SeasonPlayed);
        _worldPacket.WriteUInt32(SeasonWins);
        _worldPacket.WriteUInt32(TeamRating);
        _worldPacket.WriteUInt32(PlayerRating);
        _worldPacket.WriteInt32(Members.Count);
        if (ModernVersion.AddedInClassicVersion(1, 14, 2, 2, 5, 3))
        {
            _worldPacket.WriteBit(UnkBit);
            _worldPacket.FlushBits();
        }
        foreach (var member in Members)
            member.Write(_worldPacket);
    }

    // Cap for arena team members (max 5 for 5v5, but allow some buffer)
    private const int MaxMembers = 10;
    // Per member: GUID(18) + bool(1) + int(4) + 2 bytes(2) + 5 uints(20) + bits(1) + name(48) + 2 floats(8) = 102 bytes max
    private const int MaxMemberSize = 102;
    // 8 uints(32) + count(4) + bit(1) + members
    public int MaxSize => 32 + 4 + 1 + MaxMembers * MaxMemberSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Members.Count > MaxMembers)
            return -1;

        // Pre-validate name lengths
        foreach (var member in Members)
        {
            if (Encoding.UTF8.GetByteCount(member.Name) > GameLimits.MaxPlayerNameBytes)
                return -1;
        }

        var writer = new SpanPacketWriter(buffer);

        writer.WriteUInt32(TeamId);
        writer.WriteUInt32(TeamSize);
        writer.WriteUInt32(TeamPlayed);
        writer.WriteUInt32(TeamWins);
        writer.WriteUInt32(SeasonPlayed);
        writer.WriteUInt32(SeasonWins);
        writer.WriteUInt32(TeamRating);
        writer.WriteUInt32(PlayerRating);
        writer.WriteInt32(Members.Count);
        if (ModernVersion.AddedInClassicVersion(1, 14, 2, 2, 5, 3))
        {
            writer.WriteBit(UnkBit);
            writer.FlushBits();
        }
        foreach (var member in Members)
        {
            writer.WritePackedGuid128(member.MemberGUID.Low, member.MemberGUID.High);
            writer.WriteBool(member.Online);
            writer.WriteInt32(member.Captain);
            writer.WriteUInt8(member.Level);
            writer.WriteUInt8((byte)member.ClassId);
            writer.WriteUInt32(member.WeekGamesPlayed);
            writer.WriteUInt32(member.WeekGamesWon);
            writer.WriteUInt32(member.SeasonGamesPlayed);
            writer.WriteUInt32(member.SeasonGamesWon);
            writer.WriteUInt32(member.PersonalRating);

            writer.WriteBits((uint)Encoding.UTF8.GetByteCount(member.Name), 6);
            writer.WriteBit(member.dword60 != null);
            writer.WriteBit(member.dword68 != null);
            writer.FlushBits();

            writer.WriteString(member.Name);
            if (member.dword60 != null)
                writer.WriteFloat((float)member.dword60);
            if (member.dword68 != null)
                writer.WriteFloat((float)member.dword68);
        }
        return writer.Position;
    }

    public uint TeamId;
    public uint TeamSize;
    public uint TeamPlayed;
    public uint TeamWins;
    public uint SeasonPlayed;
    public uint SeasonWins;
    public uint TeamRating;
    public uint PlayerRating;
    public bool UnkBit;
    public List<ArenaTeamMember> Members = new List<ArenaTeamMember>();
}

struct ArenaTeamMember
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(MemberGUID);
        data.WriteBool(Online); // ???????
        data.WriteInt32(Captain);
        data.WriteUInt8(Level);
        data.WriteUInt8((byte)ClassId);
        data.WriteUInt32(WeekGamesPlayed);
        data.WriteUInt32(WeekGamesWon);
        data.WriteUInt32(SeasonGamesPlayed);
        data.WriteUInt32(SeasonGamesWon);
        data.WriteUInt32(PersonalRating);

        data.WriteBits(Name.GetByteCount(), 6);
        data.WriteBit(dword60 != null);
        data.WriteBit(dword68 != null);
        data.FlushBits();

        data.WriteString(Name);
        if (dword60 != null)
            data.WriteFloat((float)dword60);
        if (dword68 != null)
            data.WriteFloat((float)dword68);
    }

    public WowGuid128 MemberGUID;
    public bool Online;
    public int Captain;
    public byte Level;
    public Class ClassId;
    public uint WeekGamesPlayed;
    public uint WeekGamesWon;
    public uint SeasonGamesPlayed;
    public uint SeasonGamesWon;
    public uint PersonalRating;
    public string Name;
    public float? dword60;
    public float? dword68;
}

/// <summary>
/// SMSG_RATED_PVP_INFO — the reply to CMSG_REQUEST_RATED_PVP_INFO.
/// </summary>
/// <remarks>
/// This is how the 3.4.3 PvP panel fills its bracket tiles. WotLK Classic replaced arena teams
/// with a personal rating per bracket, so the client asks with <c>RequestRatedInfo</c> whenever
/// the panel opens and reads the answer back through <c>GetPersonalRatedInfo(bracketIndex)</c>.
/// Leaving the request unanswered is what left the tiles blank while the arena-team packets were
/// all arriving correctly — those feed the team roster view, not the tiles.
/// <para>
/// Seven fixed bracket records, each nineteen int32 then the Disqualified bit and a flush —
/// exactly <c>WorldPackets::Battleground::RatedPvpInfo::Write</c> and its BracketInfo operator on
/// the native 3.4.3 server. There are no counts and no bits outside each record, so the packet is
/// a fixed 7 × 77 bytes.
/// </para>
/// <para>
/// Only five of the nineteen have a 3.3.5a source; the rest are left zero, which is also what the
/// native server sends for them since it only fills the same five.
/// </para>
/// </remarks>
class RatedPvpInfo : ServerPacket, ISpanWritable
{
    public RatedPvpInfo() : base(Opcode.SMSG_RATED_PVP_INFO) { }

    public const int BracketCount = 7;

    public override void Write()
    {
        for (int i = 0; i < BracketCount; i++)
            WriteBracket(_worldPacket, i < Brackets.Length ? Brackets[i] : null);
    }

    private static void WriteBracket(WorldPacket data, RatedBracketInfo? b)
    {
        data.WriteInt32((int)(b?.PersonalRating ?? 0));
        data.WriteInt32(0);                                   // Ranking
        data.WriteInt32((int)(b?.SeasonPlayed ?? 0));
        data.WriteInt32((int)(b?.SeasonWon ?? 0));
        data.WriteInt32(0);                                   // Unused1
        data.WriteInt32(0);                                   // Unused2
        data.WriteInt32((int)(b?.WeeklyPlayed ?? 0));
        data.WriteInt32((int)(b?.WeeklyWon ?? 0));
        data.WriteInt32(0);                                   // RoundsSeasonPlayed
        data.WriteInt32(0);                                   // RoundsSeasonWon
        data.WriteInt32(0);                                   // RoundsWeeklyPlayed
        data.WriteInt32(0);                                   // RoundsWeeklyWon
        data.WriteInt32(0);                                   // BestWeeklyRating
        data.WriteInt32(0);                                   // LastWeeksBestRating
        data.WriteInt32(0);                                   // BestSeasonRating
        data.WriteInt32(0);                                   // PvpTierID
        data.WriteInt32(0);                                   // Unused3
        data.WriteInt32(0);                                   // Unused4
        data.WriteInt32(0);                                   // Rank
        data.WriteBit(false);                                 // Disqualified
        data.FlushBits();
    }

    public int MaxSize => BracketCount * (19 * 4 + 1);

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        for (int i = 0; i < BracketCount; i++)
        {
            RatedBracketInfo? b = i < Brackets.Length ? Brackets[i] : null;
            writer.WriteInt32((int)(b?.PersonalRating ?? 0));
            writer.WriteInt32(0);
            writer.WriteInt32((int)(b?.SeasonPlayed ?? 0));
            writer.WriteInt32((int)(b?.SeasonWon ?? 0));
            writer.WriteInt32(0);
            writer.WriteInt32(0);
            writer.WriteInt32((int)(b?.WeeklyPlayed ?? 0));
            writer.WriteInt32((int)(b?.WeeklyWon ?? 0));
            for (int n = 0; n < 11; n++)
                writer.WriteInt32(0);
            writer.WriteBit(false);
            writer.FlushBits();
        }
        return writer.Position;
    }

    public RatedBracketInfo[] Brackets = [];
}

class ArenaTeamQueryResponse : ServerPacket, ISpanWritable
{
    public ArenaTeamQueryResponse() : base(Opcode.SMSG_QUERY_ARENA_TEAM_RESPONSE) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(TeamId);
        _worldPacket.WriteBit(Emblem != null);
        _worldPacket.FlushBits();

        if (Emblem != null)
            Emblem.Write(_worldPacket);
    }

    // uint(4) + bit(1) + optional ArenaTeamEmblem: 7 uints(28) + bits(1) + team name(48) = 82 bytes max
    public int MaxSize => 4 + 1 + 28 + 1 + GameLimits.MaxArenaTeamNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Emblem != null && Encoding.UTF8.GetByteCount(Emblem.TeamName) > GameLimits.MaxArenaTeamNameBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(TeamId);
        writer.WriteBit(Emblem != null);
        writer.FlushBits();

        if (Emblem != null)
        {
            writer.WriteUInt32(Emblem.TeamId);
            writer.WriteUInt32(Emblem.TeamSize);
            writer.WriteUInt32(Emblem.BackgroundColor);
            writer.WriteUInt32(Emblem.EmblemStyle);
            writer.WriteUInt32(Emblem.EmblemColor);
            writer.WriteUInt32(Emblem.BorderStyle);
            writer.WriteUInt32(Emblem.BorderColor);
            writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Emblem.TeamName), 7);
            writer.FlushBits();
            writer.WriteString(Emblem.TeamName);
        }
        return writer.Position;
    }

    public uint TeamId;
    public ArenaTeamEmblem Emblem = null!;
}

public class ArenaTeamEmblem
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(TeamId);
        data.WriteUInt32(TeamSize);
        data.WriteUInt32(BackgroundColor);
        data.WriteUInt32(EmblemStyle);
        data.WriteUInt32(EmblemColor);
        data.WriteUInt32(BorderStyle);
        data.WriteUInt32(BorderColor);
        data.WriteBits(TeamName.GetByteCount(), 7);
        data.FlushBits();
        data.WriteString(TeamName);
    }

    public uint TeamId;
    public uint TeamSize;
    public uint BackgroundColor;
    public uint EmblemStyle;
    public uint EmblemColor;
    public uint BorderStyle;
    public uint BorderColor;
    public string TeamName = string.Empty;
}

public readonly record struct BattlemasterJoinArena(WowGuid128 Guid, byte TeamIndex, byte Roles);

public readonly record struct BattlemasterJoinSkirmish(
    WowGuid128 Guid, byte Roles, byte TeamSize, bool AsGroup, bool Requeue);

public readonly record struct ArenaTeamRemove(uint TeamId, WowGuid128 PlayerGuid);

public readonly record struct ArenaTeamLeave(uint TeamId);

class ArenaTeamEvent : ServerPacket, ISpanWritable
{
    public ArenaTeamEvent() : base(Opcode.SMSG_ARENA_TEAM_EVENT) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8((byte)Event);
        _worldPacket.WriteBits(Param1.GetByteCount(), 9);
        _worldPacket.WriteBits(Param2.GetByteCount(), 9);
        _worldPacket.WriteBits(Param3.GetByteCount(), 9);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(Param1);
        _worldPacket.WriteString(Param2);
        _worldPacket.WriteString(Param3);
    }

    // Cap for param strings - usually player/team names
    private const int MaxParamBytes = 64;
    // byte(1) + 27 bits(4) + 3 strings
    public int MaxSize => 1 + 4 + MaxParamBytes * 3;

    public int WriteToSpan(Span<byte> buffer)
    {
        int param1Bytes = Encoding.UTF8.GetByteCount(Param1);
        int param2Bytes = Encoding.UTF8.GetByteCount(Param2);
        int param3Bytes = Encoding.UTF8.GetByteCount(Param3);
        if (param1Bytes > MaxParamBytes || param2Bytes > MaxParamBytes || param3Bytes > MaxParamBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8((byte)Event);
        writer.WriteBits((uint)param1Bytes, 9);
        writer.WriteBits((uint)param2Bytes, 9);
        writer.WriteBits((uint)param3Bytes, 9);
        writer.FlushBits();
        writer.WriteString(Param1);
        writer.WriteString(Param2);
        writer.WriteString(Param3);
        return writer.Position;
    }

    public ArenaTeamEventModern Event;
    public string Param1 = "";
    public string Param2 = "";
    public string Param3 = "";
}

class ArenaTeamCommandResult : ServerPacket, ISpanWritable
{
    public ArenaTeamCommandResult() : base(Opcode.SMSG_ARENA_TEAM_COMMAND_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8((byte)Action);
        _worldPacket.WriteUInt8((byte)Error);
        _worldPacket.WriteBits(TeamName.GetByteCount(), 7);
        _worldPacket.WriteBits(PlayerName.GetByteCount(), 6);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(TeamName);
        _worldPacket.WriteString(PlayerName);
    }

    // MaxSize: 2 bytes + bits (7+6=13 -> 2) + team name (48) + player name (24) = 76
    public int MaxSize => 2 + 2 + GameLimits.MaxArenaTeamNameBytes + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8((byte)Action);
        writer.WriteUInt8((byte)Error);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(TeamName), 7);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(PlayerName), 6);
        writer.FlushBits();
        writer.WriteString(TeamName);
        writer.WriteString(PlayerName);
        return writer.Position;
    }

    public ArenaTeamCommandType Action;
    public ArenaTeamCommandErrorModern Error;
    public string TeamName = string.Empty;
    public string PlayerName = string.Empty;
}

class ArenaTeamInvite : ServerPacket, ISpanWritable
{
    public ArenaTeamInvite() : base(Opcode.SMSG_ARENA_TEAM_INVITE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(PlayerGuid);
        _worldPacket.WriteUInt32(PlayerVirtualAddress);
        _worldPacket.WritePackedGuid128(TeamGuid);
        _worldPacket.WriteBits(PlayerName.GetByteCount(), 6);
        _worldPacket.WriteBits(TeamName.GetByteCount(), 7);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(PlayerName);
        _worldPacket.WriteString(TeamName);
    }

    // MaxSize: 2 GUIDs (36) + uint (4) + bits (6+7=13 -> 2) + player name (24) + team name (48) = 114
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 4 + 2 + GameLimits.MaxPlayerNameBytes + GameLimits.MaxArenaTeamNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(PlayerGuid.Low, PlayerGuid.High);
        writer.WriteUInt32(PlayerVirtualAddress);
        writer.WritePackedGuid128(TeamGuid.Low, TeamGuid.High);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(PlayerName), 6);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(TeamName), 7);
        writer.FlushBits();
        writer.WriteString(PlayerName);
        writer.WriteString(TeamName);
        return writer.Position;
    }

    public WowGuid128 PlayerGuid;
    public uint PlayerVirtualAddress;
    public WowGuid128 TeamGuid;
    public string PlayerName = string.Empty;
    public string TeamName = string.Empty;
}

public readonly record struct ArenaTeamAccept(WowGuid128 PlayerGuid, WowGuid128 TeamGuid);
