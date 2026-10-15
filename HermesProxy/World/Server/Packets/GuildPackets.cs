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


using System;
using System.Text;
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HermesProxy.World.Server.Packets;

public class GuildCommandResult : ServerPacket, ISpanWritable
{
    public GuildCommandResult() : base(Opcode.SMSG_GUILD_COMMAND_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32((uint)Result);
        _worldPacket.WriteUInt32((uint)Command);

        _worldPacket.WriteBits(Name.GetByteCount(), 8);
        _worldPacket.WriteString(Name);
    }

    // MaxSize: 2 uints (8) + bits (8 -> 1) + guild name (48) = 57
    public int MaxSize => 8 + 1 + GameLimits.MaxGuildNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32((uint)Result);
        writer.WriteUInt32((uint)Command);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 8);
        writer.WriteString(Name);
        return writer.Position;
    }

    public string Name = string.Empty;
    public GuildCommandError Result;
    public GuildCommandType Command;
}

public readonly record struct QueryGuildInfo(WowGuid128 GuildGuid, WowGuid128 PlayerGuid);

public readonly record struct GuildGetRanks(WowGuid128 GuildGuid);

public class QueryGuildInfoResponse : ServerPacket
{
    public QueryGuildInfoResponse() : base(Opcode.SMSG_QUERY_GUILD_INFO_RESPONSE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(GuildGUID);
        if (ModernVersion.RemovedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
            _worldPacket.WritePackedGuid128(PlayerGuid);
        _worldPacket.WriteBit(HasGuildInfo);
        _worldPacket.FlushBits();

        if (HasGuildInfo)
        {
            _worldPacket.WritePackedGuid128(Info.GuildGuid);
            _worldPacket.WriteUInt32(Info.VirtualRealmAddress);
            _worldPacket.WriteInt32(Info.Ranks.Count);
            _worldPacket.WriteUInt32(Info.EmblemStyle);
            _worldPacket.WriteUInt32(Info.EmblemColor);
            _worldPacket.WriteUInt32(Info.BorderStyle);
            _worldPacket.WriteUInt32(Info.BorderColor);
            _worldPacket.WriteUInt32(Info.BackgroundColor);
            _worldPacket.WriteBits(Info.GuildName.GetByteCount(), 7);
            _worldPacket.FlushBits();

            foreach (var rank in Info.Ranks)
            {
                _worldPacket.WriteUInt32(rank.RankID);
                _worldPacket.WriteUInt32(rank.RankOrder);

                _worldPacket.WriteBits(rank.RankName.GetByteCount(), 7);
                _worldPacket.WriteString(rank.RankName);
            }

            _worldPacket.WriteString(Info.GuildName);
        }

    }

    public WowGuid128 GuildGUID;
    public WowGuid128 PlayerGuid;
    public GuildInfo Info = new();
    public bool HasGuildInfo;

    public class GuildInfo
    {
        public WowGuid128 GuildGuid;

        public uint VirtualRealmAddress; // a special identifier made from the Index, BattleGroup and Region.

        public uint EmblemStyle;
        public uint EmblemColor;
        public uint BorderStyle;
        public uint BorderColor;
        public uint BackgroundColor;
        public List<RankInfo> Ranks = new();
        public string GuildName = "";

        public struct RankInfo
        {
            public RankInfo(uint id, uint order, string name)
            {
                RankID = id;
                RankOrder = order;
                RankName = name;
            }

            public uint RankID;
            public uint RankOrder;
            public string RankName = string.Empty;
        }
    }
}

public class GuildPermissionsQueryResults : ServerPacket
{
    public GuildPermissionsQueryResults() : base(Opcode.SMSG_GUILD_PERMISSIONS_QUERY_RESULTS) { }

    public override void Write()
    {
        // 3.4.3 / Wrathion GuildPackets.cpp GuildPermissionsQueryResults::Write.
        // AC 3.3.5 MSG_GUILD_PERMISSIONS is the same fields minus the Tab.count
        // uint32 (it writes a fixed 6-tab array after int8 NumTabs).
        _worldPacket.WriteUInt32(RankID);
        _worldPacket.WriteInt32(Flags);
        _worldPacket.WriteInt32(WithdrawGoldLimit);
        _worldPacket.WriteInt32(NumTabs);
        _worldPacket.WriteUInt32((uint)Tab.Count);
        foreach (var tab in Tab)
        {
            _worldPacket.WriteInt32(tab.Flags);
            _worldPacket.WriteInt32(tab.WithdrawItemLimit);
        }
    }

    public uint RankID;
    public int Flags;
    public int WithdrawGoldLimit;
    public int NumTabs;
    public List<GuildRankTabPermissions> Tab = new();
}

public struct GuildRankTabPermissions
{
    public int Flags;
    public int WithdrawItemLimit;
}

public class GuildRoster : ServerPacket
{
    public GuildRoster() : base(Opcode.SMSG_GUILD_ROSTER) { }

    internal static readonly ServerPacketLayouts<ServerPacketLayout<GuildRoster>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new EarlierLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<GuildRoster> Layout = Layouts.ForRunningClient();

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>Up to 3.4.3: the writer this packet had before it had layouts, as it was.</summary>
    internal sealed class EarlierLayout : ServerPacketLayout<GuildRoster>
    {
        public override void Write(GuildRoster packet, WorldPacket data)
        {
            data.WriteUInt32(packet.NumAccounts);
            data.WritePackedTime(packet.CreateDate);
            data.WriteInt32(packet.GuildFlags);
            data.WriteInt32(packet.MemberData.Count);
            data.WriteBits(packet.WelcomeText.GetByteCount(), 11);
            data.WriteBits(packet.InfoText.GetByteCount(), 11);
            data.FlushBits();

            foreach (GuildRosterMemberData member in packet.MemberData)
                member.Write(data);

            data.WriteString(packet.WelcomeText);
            data.WriteString(packet.InfoText);
        }
    }

    /// <summary>
    /// 4.4.2 (docs/protocol, read from the client, and TrinityCore cata_classic
    /// GuildRosterMemberData): the packet is the same, a member is not. It has no SorEligible bit
    /// and carries a dungeon score summary, empty here, between its bit fields and its strings.
    /// </summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<GuildRoster>
    {
        public override void Write(GuildRoster packet, WorldPacket data)
        {
            data.WriteUInt32(packet.NumAccounts);
            data.WritePackedTime(packet.CreateDate);
            data.WriteInt32(packet.GuildFlags);
            data.WriteInt32(packet.MemberData.Count);
            data.WriteBits(packet.WelcomeText.GetByteCount(), 11);
            data.WriteBits(packet.InfoText.GetByteCount(), 11);
            data.FlushBits();

            foreach (GuildRosterMemberData member in packet.MemberData)
                member.WriteCataClassic(data);

            data.WriteString(packet.WelcomeText);
            data.WriteString(packet.InfoText);
        }
    }

    public List<GuildRosterMemberData> MemberData = new List<GuildRosterMemberData>();
    public string WelcomeText = string.Empty;
    public string InfoText = string.Empty;
    public uint CreateDate;
    public uint NumAccounts;
    public int GuildFlags = 2;
}

public class GuildRosterMemberData
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(Guid);
        data.WriteInt32(RankID);
        data.WriteInt32(AreaID);
        data.WriteInt32(PersonalAchievementPoints);
        data.WriteInt32(GuildReputation);
        data.WriteFloat(LastSave);

        for (byte i = 0; i < 2; i++)
            Profession[i].Write(data);

        data.WriteUInt32(VirtualRealmAddress);
        data.WriteUInt8(Status);
        data.WriteUInt8(Level);
        data.WriteUInt8((byte)ClassID);
        data.WriteUInt8((byte)SexID);

        // V3_4_3 (lineagedr/3.4.3_Source GuildPackets.cpp GuildRosterMemberData)
        // inserts GuildClubMemberID + RaceID before the name/note bits.
        // Without them the client reads those 9 bytes as the name length
        // and the MOTD/info strings after the members land on garbage.
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            data.WriteUInt64(GuildClubMemberID);
            data.WriteUInt8((byte)RaceID);
        }

        data.WriteBits(Name.GetByteCount(), 6);
        data.WriteBits(Note.GetByteCount(), 8);
        data.WriteBits(OfficerNote.GetByteCount(), 8);
        data.WriteBit(Authenticated);
        data.WriteBit(SorEligible);
        data.FlushBits();

        data.WriteString(Name);
        data.WriteString(Note);
        data.WriteString(OfficerNote);
    }

    /// <summary>The 4.4.2 member, see <see cref="GuildRoster.CataClassicLayout"/>.</summary>
    internal void WriteCataClassic(WorldPacket data)
    {
        data.WritePackedGuid128(Guid);
        data.WriteInt32(RankID);
        data.WriteInt32(AreaID);
        data.WriteInt32(PersonalAchievementPoints);
        data.WriteInt32(GuildReputation);
        data.WriteFloat(LastSave);

        for (byte i = 0; i < 2; i++)
            Profession[i].Write(data);

        data.WriteUInt32(VirtualRealmAddress);
        data.WriteUInt8(Status);
        data.WriteUInt8(Level);
        data.WriteUInt8((byte)ClassID);
        data.WriteUInt8((byte)SexID);
        data.WriteUInt64(GuildClubMemberID);
        data.WriteUInt8((byte)RaceID);

        data.WriteBits(Name.GetByteCount(), 6);
        data.WriteBits(Note.GetByteCount(), 8);
        data.WriteBits(OfficerNote.GetByteCount(), 8);
        data.WriteBit(Authenticated);
        data.FlushBits();

        data.WriteFloat(0);                         // DungeonScore.OverallScoreCurrentSeason
        data.WriteFloat(0);                         // DungeonScore.LadderScoreCurrentSeason
        data.WriteUInt32(0);                        // DungeonScore.Runs

        data.WriteString(Name);
        data.WriteString(Note);
        data.WriteString(OfficerNote);
    }

    public WowGuid128 Guid;
    public long WeeklyXP;
    public long TotalXP;
    public int RankID;
    public int AreaID;
    public int PersonalAchievementPoints = -1;
    public int GuildReputation = -1;
    public int GuildRepToCap;
    public float LastSave;
    public string Name = string.Empty;
    public uint VirtualRealmAddress;
    public string Note = string.Empty;
    public string OfficerNote = string.Empty;
    public byte Status;
    public byte Level;
    public Class ClassID;
    public Gender SexID;
    public bool Authenticated;
    public bool SorEligible;
    public ulong GuildClubMemberID;
    public Race RaceID = Race.None;
    public GuildRosterProfessionData[] Profession = new GuildRosterProfessionData[2];
}

public struct GuildRosterProfessionData
{
    public void Write(WorldPacket data)
    {
        data.WriteInt32(DbID);
        data.WriteInt32(Rank);
        data.WriteInt32(Step);
    }

    public int DbID;
    public int Rank;
    public int Step;
}

public class GuildRanks : ServerPacket
{
    public GuildRanks() : base(Opcode.SMSG_GUILD_RANKS) { }

    internal static readonly ServerPacketLayouts<ServerPacketLayout<GuildRanks>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new EarlierLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<GuildRanks> Layout = Layouts.ForRunningClient();

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>Up to 3.4.3: six bank tabs a rank.</summary>
    internal sealed class EarlierLayout : ServerPacketLayout<GuildRanks>
    {
        public override void Write(GuildRanks packet, WorldPacket data)
        {
            data.WriteInt32(packet.Ranks.Count);
            foreach (GuildRankData rank in packet.Ranks)
                rank.Write(data, GuildConst.MaxBankTabs);
        }
    }

    /// <summary>4.4.2: a Cataclysm guild bank has eight tabs (TrinityCore cata_classic GuildRankData).</summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<GuildRanks>
    {
        public override void Write(GuildRanks packet, WorldPacket data)
        {
            data.WriteInt32(packet.Ranks.Count);
            foreach (GuildRankData rank in packet.Ranks)
                rank.Write(data, GuildConst.MaxBankTabsCata);
        }
    }

    public List<GuildRankData> Ranks = new List<GuildRankData>();
}

public class GuildRankData
{
    public void Write(WorldPacket data, int bankTabs)
    {
        data.WriteUInt8(RankID);
        data.WriteUInt32(RankOrder);
        data.WriteUInt32(Flags);
        data.WriteInt32(WithdrawGoldLimit);

        for (byte i = 0; i < bankTabs; i++)
        {
            data.WriteUInt32(TabFlags[i]);
            data.WriteUInt32(TabWithdrawItemLimit[i]);
        }

        data.WriteBits(RankName.GetByteCount(), 7);
        data.WriteString(RankName);
    }

    public byte RankID;
    public uint RankOrder;
    public uint Flags;
    public int WithdrawGoldLimit;
    public string RankName = string.Empty;
    // Room for the eight tabs of a Cataclysm bank; the older layouts write the first six.
    public uint[] TabFlags = new uint[GuildConst.MaxBankTabsCata];
    public uint[] TabWithdrawItemLimit = new uint[GuildConst.MaxBankTabsCata];
}

/// <summary>
/// A member's note changed. Only a 4.3.4 server says so; the older ones leave it to the next
/// roster. Layout from the 4.4.2 client (docs/protocol) and TrinityCore cata_classic.
/// </summary>
public sealed class GuildMemberUpdateNote : ServerPacket
{
    public GuildMemberUpdateNote() : base(Opcode.SMSG_GUILD_MEMBER_UPDATE_NOTE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Member);
        _worldPacket.WriteBits(Note.GetByteCount(), 8);
        _worldPacket.WriteBit(IsPublic);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(Note);
    }

    public WowGuid128 Member;
    public string Note = "";
    public bool IsPublic;
}

public class GuildSendRankChange : ServerPacket, ISpanWritable
{
    public GuildSendRankChange() : base(Opcode.SMSG_GUILD_SEND_RANK_CHANGE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Officer);
        _worldPacket.WritePackedGuid128(Other);
        _worldPacket.WriteUInt32(RankID);

        _worldPacket.WriteBit(Promote);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 5; // 2 GUIDs + uint + bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Officer.Low, Officer.High);
        writer.WritePackedGuid128(Other.Low, Other.High);
        writer.WriteUInt32(RankID);
        writer.WriteBit(Promote);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 Other;
    public WowGuid128 Officer;
    public bool Promote;
    public uint RankID;
}

public class GuildEventMotd : ServerPacket, ISpanWritable
{
    public GuildEventMotd() : base(Opcode.SMSG_GUILD_EVENT_MOTD) { }

    public override void Write()
    {
        _worldPacket.WriteBits(MotdText.GetByteCount(), 11);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(MotdText);
    }

    // Cap for MOTD text - usually short messages
    private const int MaxMotdBytes = 256;
    // 11 bits(2) + text
    public int MaxSize => 2 + MaxMotdBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int motdBytes = Encoding.UTF8.GetByteCount(MotdText);
        if (motdBytes > MaxMotdBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)motdBytes, 11);
        writer.WriteString(MotdText);
        return writer.Position;
    }

    public string MotdText = string.Empty;
}

public class GuildEventPlayerJoined : ServerPacket, ISpanWritable
{
    public GuildEventPlayerJoined() : base(Opcode.SMSG_GUILD_EVENT_PLAYER_JOINED) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteUInt32(VirtualRealmAddress);

        _worldPacket.WriteBits(Name.GetByteCount(), 6);
        _worldPacket.WriteString(Name);
    }

    // MaxSize: GUID (18) + uint (4) + 6 bits (1) + player name bytes (24) = 47
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + 1 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        writer.WriteUInt32(VirtualRealmAddress);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 6);
        writer.WriteString(Name);
        return writer.Position;
    }

    public WowGuid128 Guid;
    public uint VirtualRealmAddress;
    public string Name = string.Empty;
}

public class GuildEventPlayerLeft : ServerPacket, ISpanWritable
{
    public GuildEventPlayerLeft() : base(Opcode.SMSG_GUILD_EVENT_PLAYER_LEFT) { }

    public override void Write()
    {
        _worldPacket.WriteBit(Removed);
        _worldPacket.WriteBits(LeaverName.GetByteCount(), 6);

        if (Removed)
        {
            _worldPacket.WriteBits(RemoverName.GetByteCount(), 6);
            _worldPacket.WritePackedGuid128(RemoverGUID);
            _worldPacket.WriteUInt32(RemoverVirtualRealmAddress);
            _worldPacket.WriteString(RemoverName);
        }

        _worldPacket.WritePackedGuid128(LeaverGUID);
        _worldPacket.WriteUInt32(LeaverVirtualRealmAddress);
        _worldPacket.WriteString(LeaverName);
    }

    // MaxSize (worst case with Removed=true):
    // bits (1+6+6=13 -> 2 bytes) + RemoverGUID (18) + uint (4) + RemoverName (24)
    // + LeaverGUID (18) + uint (4) + LeaverName (24) = 94
    public int MaxSize => 2 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 8 + GameLimits.MaxPlayerNameBytes * 2;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBit(Removed);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(LeaverName), 6);

        if (Removed)
        {
            writer.WriteBits((uint)Encoding.UTF8.GetByteCount(RemoverName), 6);
            writer.WritePackedGuid128(RemoverGUID.Low, RemoverGUID.High);
            writer.WriteUInt32(RemoverVirtualRealmAddress);
            writer.WriteString(RemoverName);
        }

        writer.WritePackedGuid128(LeaverGUID.Low, LeaverGUID.High);
        writer.WriteUInt32(LeaverVirtualRealmAddress);
        writer.WriteString(LeaverName);
        return writer.Position;
    }

    public bool Removed;
    public WowGuid128 RemoverGUID;
    public uint RemoverVirtualRealmAddress;
    public string RemoverName = string.Empty;
    public WowGuid128 LeaverGUID;
    public uint LeaverVirtualRealmAddress;
    public string LeaverName = string.Empty;
}

public class GuildEventNewLeader : ServerPacket, ISpanWritable
{
    public GuildEventNewLeader() : base(Opcode.SMSG_GUILD_EVENT_NEW_LEADER) { }

    public override void Write()
    {
        _worldPacket.WriteBit(SelfPromoted);
        _worldPacket.WriteBits(OldLeaderName.GetByteCount(), 6);
        _worldPacket.WriteBits(NewLeaderName.GetByteCount(), 6);

        _worldPacket.WritePackedGuid128(OldLeaderGUID);
        _worldPacket.WriteUInt32(OldLeaderVirtualRealmAddress);
        _worldPacket.WritePackedGuid128(NewLeaderGUID);
        _worldPacket.WriteUInt32(NewLeaderVirtualRealmAddress);

        _worldPacket.WriteString(OldLeaderName);
        _worldPacket.WriteString(NewLeaderName);
    }

    // MaxSize: bits (1+6+6=13 -> 2 bytes) + 2 GUIDs (36) + 2 uints (8) + 2 names (48) = 94
    public int MaxSize => 2 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 8 + GameLimits.MaxPlayerNameBytes * 2;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBit(SelfPromoted);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(OldLeaderName), 6);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(NewLeaderName), 6);

        writer.WritePackedGuid128(OldLeaderGUID.Low, OldLeaderGUID.High);
        writer.WriteUInt32(OldLeaderVirtualRealmAddress);
        writer.WritePackedGuid128(NewLeaderGUID.Low, NewLeaderGUID.High);
        writer.WriteUInt32(NewLeaderVirtualRealmAddress);

        writer.WriteString(OldLeaderName);
        writer.WriteString(NewLeaderName);
        return writer.Position;
    }

    public bool SelfPromoted;
    public WowGuid128 NewLeaderGUID;
    public uint NewLeaderVirtualRealmAddress;
    public string NewLeaderName = string.Empty;
    public WowGuid128 OldLeaderGUID;
    public uint OldLeaderVirtualRealmAddress;
    public string OldLeaderName = string.Empty;
}

public class GuildEventDisbanded : ServerPacket, ISpanWritable
{
    public GuildEventDisbanded() : base(Opcode.SMSG_GUILD_EVENT_DISBANDED) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

public class GuildEventRanksUpdated : ServerPacket, ISpanWritable
{
    public GuildEventRanksUpdated() : base(Opcode.SMSG_GUILD_EVENT_RANKS_UPDATED) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

public class GuildEventPresenceChange : ServerPacket, ISpanWritable
{
    public GuildEventPresenceChange() : base(Opcode.SMSG_GUILD_EVENT_PRESENCE_CHANGE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteUInt32(VirtualRealmAddress);

        _worldPacket.WriteBits(Name.GetByteCount(), 6);
        _worldPacket.WriteBit(LoggedOn);
        _worldPacket.WriteBit(Mobile);

        _worldPacket.WriteString(Name);
    }

    // MaxSize: GUID (18) + uint (4) + bits (6+1+1=8 -> 1 byte) + name (24) = 47
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + 1 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        writer.WriteUInt32(VirtualRealmAddress);

        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 6);
        writer.WriteBit(LoggedOn);
        writer.WriteBit(Mobile);

        writer.WriteString(Name);
        return writer.Position;
    }

    public WowGuid128 Guid;
    public uint VirtualRealmAddress;
    public bool LoggedOn;
    public bool Mobile;
    public string Name = string.Empty;
}

public class GuildEventTabAdded : ServerPacket, ISpanWritable
{
    public GuildEventTabAdded() : base(Opcode.SMSG_GUILD_EVENT_TAB_ADDED) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

public class GuildEventTabModified : ServerPacket, ISpanWritable
{
    public GuildEventTabModified() : base(Opcode.SMSG_GUILD_EVENT_TAB_MODIFIED) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Tab);

        _worldPacket.WriteBits(Name.GetByteCount(), 7);
        _worldPacket.WriteBits(Icon.GetByteCount(), 9);
        _worldPacket.FlushBits();

        _worldPacket.WriteString(Name);
        _worldPacket.WriteString(Icon);
    }

    // Cap for tab name and icon path
    private const int MaxNameBytes = 64;
    private const int MaxIconBytes = 256;
    // int(4) + 16 bits(2) + name + icon
    public int MaxSize => 4 + 2 + MaxNameBytes + MaxIconBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int nameBytes = Encoding.UTF8.GetByteCount(Name);
        int iconBytes = Encoding.UTF8.GetByteCount(Icon);
        if (nameBytes > MaxNameBytes || iconBytes > MaxIconBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Tab);
        writer.WriteBits((uint)nameBytes, 7);
        writer.WriteBits((uint)iconBytes, 9);
        writer.FlushBits();
        writer.WriteString(Name);
        writer.WriteString(Icon);
        return writer.Position;
    }

    public int Tab;
    public string Name = string.Empty;
    public string Icon = string.Empty;
}

public class GuildEventBankMoneyChanged : ServerPacket, ISpanWritable
{
    public GuildEventBankMoneyChanged() : base(Opcode.SMSG_GUILD_EVENT_BANK_MONEY_CHANGED) { }

    public override void Write()
    {
        _worldPacket.WriteUInt64(Money);
    }

    public int MaxSize => 8; // ulong

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt64(Money);
        return writer.Position;
    }

    public ulong Money;
}

public class GuildEventTabTextChanged : ServerPacket, ISpanWritable
{
    public GuildEventTabTextChanged() : base(Opcode.SMSG_GUILD_EVENT_TAB_TEXT_CHANGED) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Tab);
    }

    public int MaxSize => 4; // int

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Tab);
        return writer.Position;
    }

    public int Tab;
}

public readonly record struct GuildUpdateMotdText(string MotdText);

public readonly record struct GuildUpdateInfoText(string InfoText);

/// <param name="IsPublic">0 == Officer, 1 == Public.</param>
public readonly record struct GuildSetMemberNote(WowGuid128 NoteeGUID, bool IsPublic, string Note);

public readonly record struct GuildPromoteMember(WowGuid128 Promotee);

public readonly record struct GuildDemoteMember(WowGuid128 Demotee);

public readonly record struct GuildAssignMemberRank(WowGuid128 Member, int RankOrder);

public readonly record struct GuildOfficerRemoveMember(WowGuid128 Removee);

public readonly record struct GuildInviteByName(string Name, uint ArenaTeamId);

public class GuildInvite : ServerPacket, ISpanWritable
{
    public GuildInvite() : base(Opcode.SMSG_GUILD_INVITE) { }

    public override void Write()
    {
        _worldPacket.WriteBits(InviterName.GetByteCount(), 6);
        _worldPacket.WriteBits(GuildName.GetByteCount(), 7);
        _worldPacket.WriteBits(OldGuildName.GetByteCount(), 7);

        _worldPacket.WriteUInt32(InviterVirtualRealmAddress);
        _worldPacket.WriteUInt32(GuildVirtualRealmAddress);
        _worldPacket.WritePackedGuid128(GuildGUID);
        _worldPacket.WriteUInt32(OldGuildVirtualRealmAddress);
        _worldPacket.WritePackedGuid128(OldGuildGUID);
        _worldPacket.WriteUInt32(EmblemStyle);
        _worldPacket.WriteUInt32(EmblemColor);
        _worldPacket.WriteUInt32(BorderStyle);
        _worldPacket.WriteUInt32(BorderColor);
        _worldPacket.WriteUInt32(BackgroundColor);
        _worldPacket.WriteInt32(AchievementPoints);

        _worldPacket.WriteString(InviterName);
        _worldPacket.WriteString(GuildName);
        _worldPacket.WriteString(OldGuildName);
    }

    // MaxSize: bits (6+7+7=20 -> 3 bytes) + 3 uints (12) + 2 GUIDs (36) + 6 uints (24)
    // + player name (24) + guild name (48) + old guild name (48) = 195
    public int MaxSize => 3 + 12 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 24 +
        GameLimits.MaxPlayerNameBytes + GameLimits.MaxGuildNameBytes * 2;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(InviterName), 6);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(GuildName), 7);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(OldGuildName), 7);

        writer.WriteUInt32(InviterVirtualRealmAddress);
        writer.WriteUInt32(GuildVirtualRealmAddress);
        writer.WritePackedGuid128(GuildGUID.Low, GuildGUID.High);
        writer.WriteUInt32(OldGuildVirtualRealmAddress);
        writer.WritePackedGuid128(OldGuildGUID.Low, OldGuildGUID.High);
        writer.WriteUInt32(EmblemStyle);
        writer.WriteUInt32(EmblemColor);
        writer.WriteUInt32(BorderStyle);
        writer.WriteUInt32(BorderColor);
        writer.WriteUInt32(BackgroundColor);
        writer.WriteInt32(AchievementPoints);

        writer.WriteString(InviterName);
        writer.WriteString(GuildName);
        writer.WriteString(OldGuildName);
        return writer.Position;
    }

    public WowGuid128 GuildGUID;
    public WowGuid128 OldGuildGUID = WowGuid128.Empty;
    public uint EmblemColor;
    public uint EmblemStyle;
    public uint BorderStyle;
    public uint BorderColor;
    public uint BackgroundColor;
    public int AchievementPoints = -1;
    public uint GuildVirtualRealmAddress;
    public uint OldGuildVirtualRealmAddress;
    public uint InviterVirtualRealmAddress;
    public string InviterName = string.Empty;
    public string GuildName = string.Empty;
    public string OldGuildName = "";
}

public class GuildInviteDeclined : ServerPacket, ISpanWritable
{
    public GuildInviteDeclined() : base(Opcode.SMSG_GUILD_INVITE_DECLINED) { }

    public override void Write()
    {
        _worldPacket.WriteBits(InviterName.GetByteCount(), 6);
        _worldPacket.WriteBit(AutoDecline);
        _worldPacket.FlushBits();

        _worldPacket.WriteUInt32(InviterVirtualRealmAddress);
        _worldPacket.WriteString(InviterName);

    }

    // MaxSize: bits (6+1=7 -> 1 byte) + uint (4) + player name (24) = 29
    public int MaxSize => 1 + 4 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(InviterName), 6);
        writer.WriteBit(AutoDecline);
        writer.FlushBits();

        writer.WriteUInt32(InviterVirtualRealmAddress);
        writer.WriteString(InviterName);
        return writer.Position;
    }

    public bool AutoDecline;
    public uint InviterVirtualRealmAddress;
    public string InviterName = string.Empty;
}

/// <summary>
/// One rank's complete permission state. The 3.4.3 control panel sends one of these per
/// changed setting when Apply is clicked, each carrying the whole rank.
/// </summary>
/// <remarks>
/// The two per-tab arrays are <see cref="GuildBankTabLimits"/> inline arrays rather than
/// <c>uint[]</c>, so the packet is one flat value with nothing on the heap. That also
/// matters for correctness here and not only for allocation: this is the one CMSG the proxy
/// *stores* — the outbox's <c>Coalesce</c> hold keeps the newest per rank for 100 ms, captured
/// in its release — and a value type is copied into that capture, where a reference to a pooled
/// array would not be.
/// </remarks>
public readonly record struct GuildSetRankPermissions(
    uint RankID,
    uint RankOrder,
    uint Flags,
    int WithdrawGoldLimit,
    GuildBankTabLimits TabFlags,
    GuildBankTabLimits TabWithdrawItemLimit,
    uint OldFlags,
    string RankName);

/// <summary>
/// Per-tab storage for <see cref="GuildSetRankPermissions"/>, one entry per bank tab.
/// </summary>
/// <remarks>
/// Inline, following <c>PacketTag</c>. The synthesized record equality on the packet that holds
/// two of these falls back to <c>ValueType.Equals</c>, which reflects and boxes — nothing
/// compares these packets today, and a hot path that wanted to would need
/// <see cref="System.MemoryExtensions.SequenceEqual{T}(System.ReadOnlySpan{T}, System.ReadOnlySpan{T})"/>
/// over the spans instead.
/// </remarks>
[InlineArray(GuildConst.MaxBankTabsCata)]
public struct GuildBankTabLimits
{
    private uint _element0;
}

public readonly record struct GuildAddRank(string Name, int RankOrder);

public readonly record struct GuildDeleteRank(int RankOrder);

public readonly record struct GuildSetGuildMaster(string NewMasterName);

public class PlayerTabardVendorActivate : ServerPacket, ISpanWritable
{
    public PlayerTabardVendorActivate() : base(Opcode.SMSG_PLAYER_TABARD_VENDOR_ACTIVATE) { }

    public override void Write()
    {
        // V3_4_3 wire-opcode 10378 is SMSG_NPC_INTERACTION_OPEN_RESULT
        // (Guid + Int32 InteractionType + bit Success). Same shape as
        // ShowBank / BinderConfirm. WPP V3_4_3_51666 has no
        // SMSG_PLAYER_TABARD_VENDOR_ACTIVATE; type GuildTabardVendor (14)
        // opens TabardFrame. AC still sends MSG_TABARDVENDOR_ACTIVATE
        // with no guild check; the no-guild error is on save.
        _worldPacket.WritePackedGuid128(DesignerGUID);
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            _worldPacket.WriteInt32((int)PlayerInteractionType.GuildTabardVendor);
            _worldPacket.WriteBit(true);
            _worldPacket.FlushBits();
        }
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size
        + (ModernVersion.IsWotLKClassicOrLater ? 5 : 0);

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(DesignerGUID.Low, DesignerGUID.High);
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            writer.WriteInt32((int)PlayerInteractionType.GuildTabardVendor);
            writer.WriteBit(true);
            writer.FlushBits();
        }
        return writer.Position;
    }

    public WowGuid128 DesignerGUID;
}

public readonly record struct SaveGuildEmblem(
    WowGuid128 DesignerGUID,
    uint EmblemStyle,
    uint EmblemColor,
    uint BorderStyle,
    uint BorderColor,
    uint BackgroundColor);

public class PlayerSaveGuildEmblem : ServerPacket, ISpanWritable
{
    public PlayerSaveGuildEmblem() : base(Opcode.SMSG_PLAYER_SAVE_GUILD_EMBLEM) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32((uint)Error);
    }

    public int MaxSize => 4; // uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32((uint)Error);
        return writer.Position;
    }

    public GuildEmblemError Error;
}

public readonly record struct SetAutoDeclineGuildInvites(bool GuildInvitesShouldGetBlocked);

public readonly record struct GuildBankAtivate(WowGuid128 BankGuid, bool FullUpdate);

public class GuildBankItemInfo
{
    public ItemInstance Item = new();
    public int Slot;
    public int Count;
    public int EnchantmentID;
    public int Charges;
    public int OnUseEnchantmentID;
    public uint Flags;
    public bool Locked;
    public List<ItemGemData> SocketEnchant = new();
}

public struct GuildBankTabInfo
{
    public int TabIndex;
    public string Name;
    public string Icon;
}

public class GuildBankQueryResults : ServerPacket
{
    public GuildBankQueryResults() : base(Opcode.SMSG_GUILD_BANK_QUERY_RESULTS)
    {
        ItemInfo = new List<GuildBankItemInfo>();
        TabInfo = new List<GuildBankTabInfo>();
    }

    public override void Write()
    {
        _worldPacket.WriteUInt64(Money);
        _worldPacket.WriteInt32(Tab);
        _worldPacket.WriteInt32(WithdrawalsRemaining);
        _worldPacket.WriteInt32(TabInfo.Count);
        _worldPacket.WriteInt32(ItemInfo.Count);
        _worldPacket.WriteBit(FullUpdate);
        _worldPacket.FlushBits();

        foreach (GuildBankTabInfo tab in TabInfo)
        {
            _worldPacket.WriteInt32(tab.TabIndex);
            _worldPacket.WriteBits(tab.Name.GetByteCount(), 7);
            _worldPacket.WriteBits(tab.Icon.GetByteCount(), 9);
            _worldPacket.FlushBits();

            _worldPacket.WriteString(tab.Name);
            _worldPacket.WriteString(tab.Icon);
        }

        foreach (GuildBankItemInfo item in ItemInfo)
        {
            _worldPacket.WriteInt32(item.Slot);
            _worldPacket.WriteInt32(item.Count);
            _worldPacket.WriteInt32(item.EnchantmentID);
            _worldPacket.WriteInt32(item.Charges);
            _worldPacket.WriteInt32(item.OnUseEnchantmentID);
            _worldPacket.WriteUInt32(item.Flags);

            item.Item.Write(_worldPacket);

            _worldPacket.WriteBits(item.SocketEnchant.Count, 2);
            _worldPacket.WriteBit(item.Locked);
            _worldPacket.FlushBits();

            foreach (ItemGemData socketEnchant in item.SocketEnchant)
                socketEnchant.Write(_worldPacket);
        }
    }

    public List<GuildBankItemInfo> ItemInfo;
    public List<GuildBankTabInfo> TabInfo;
    public int WithdrawalsRemaining;
    public int Tab;
    public ulong Money;
    public bool FullUpdate;
}

public readonly record struct GuildBankQueryTab(WowGuid128 BankGuid, byte Tab, bool FullUpdate);

public readonly record struct GuildBankDepositMoney(WowGuid128 BankGuid, ulong Money);

public readonly record struct GuildBankTextQuery(int Tab);

public class GuildBankTextQueryResult : ServerPacket, ISpanWritable
{
    public GuildBankTextQueryResult() : base(Opcode.SMSG_GUILD_BANK_TEXT_QUERY_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Tab);

        _worldPacket.WriteBits(Text.GetByteCount(), 14);
        _worldPacket.WriteString(Text);
    }

    // Cap for bank tab text - usually short descriptions
    private const int MaxTextBytes = 512;
    // int(4) + 14 bits(2) + text
    public int MaxSize => 4 + 2 + MaxTextBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int textBytes = Text != null ? Encoding.UTF8.GetByteCount(Text) : 0;
        if (textBytes > MaxTextBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Tab);
        writer.WriteBits((uint)textBytes, 14);
        if (Text != null)
            writer.WriteString(Text);
        return writer.Position;
    }

    public int Tab;
    public string Text = string.Empty;
}

public readonly record struct GuildBankUpdateTab(WowGuid128 BankGuid, byte BankTab, string Name, string Icon);

public readonly record struct GuildBankLogQuery(int Tab);

public class GuildBankLogEntry
{
    public WowGuid128 PlayerGUID;
    public uint TimeOffset;
    public sbyte EntryType;
    public ulong? Money;
    public int? ItemID;
    public int? Count;
    public sbyte? OtherTab;
}

public class GuildBankLogQueryResults : ServerPacket
{
    public GuildBankLogQueryResults() : base(Opcode.SMSG_GUILD_BANK_LOG_QUERY_RESULTS)
    {
        Entry = new List<GuildBankLogEntry>();
    }

    public override void Write()
    {
        _worldPacket.WriteInt32(Tab);
        _worldPacket.WriteInt32(Entry.Count);
        _worldPacket.WriteBit(WeeklyBonusMoney.HasValue);
        _worldPacket.FlushBits();

        foreach (GuildBankLogEntry logEntry in Entry)
        {
            _worldPacket.WritePackedGuid128(logEntry.PlayerGUID);
            _worldPacket.WriteUInt32(logEntry.TimeOffset);
            _worldPacket.WriteInt8(logEntry.EntryType);

            _worldPacket.WriteBit(logEntry.Money.HasValue);
            _worldPacket.WriteBit(logEntry.ItemID.HasValue);
            _worldPacket.WriteBit(logEntry.Count.HasValue);
            _worldPacket.WriteBit(logEntry.OtherTab.HasValue);
            _worldPacket.FlushBits();

            if (logEntry.Money.HasValue)
                _worldPacket.WriteUInt64(logEntry.Money.Value);

            if (logEntry.ItemID.HasValue)
                _worldPacket.WriteInt32(logEntry.ItemID.Value);

            if (logEntry.Count.HasValue)
                _worldPacket.WriteInt32(logEntry.Count.Value);

            if (logEntry.OtherTab.HasValue)
                _worldPacket.WriteInt8(logEntry.OtherTab.Value);
        }

        if (WeeklyBonusMoney.HasValue)
            _worldPacket.WriteUInt64(WeeklyBonusMoney.Value);
    }

    public int Tab;
    public List<GuildBankLogEntry> Entry;
    public ulong? WeeklyBonusMoney;
}

public readonly record struct GuildBankSetTabText(int Tab, string TabText);

public readonly record struct GuildBankBuyTab(WowGuid128 BankGuid, byte BankTab);

public class GuildBankRemainingWithdrawMoney : ServerPacket, ISpanWritable
{
    public GuildBankRemainingWithdrawMoney() : base(Opcode.SMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY) { }

    public override void Write()
    {
        _worldPacket.WriteInt64(RemainingWithdrawMoney);
    }

    public int MaxSize => 8; // long

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt64(RemainingWithdrawMoney);
        return writer.Position;
    }

    public long RemainingWithdrawMoney;
}

public readonly record struct GuildBankWithdrawMoney(WowGuid128 BankGuid, ulong Money);

/// <param name="ContainerSlot">Absent when the item sits in the backpack rather than a bag.</param>
public readonly record struct AutoGuildBankItem(
    WowGuid128 BankGuid,
    byte BankTab,
    byte BankSlot,
    byte? ContainerSlot,
    byte ContainerItemSlot);

public readonly record struct SplitItemToGuildBank(
    WowGuid128 BankGuid,
    byte BankTab,
    byte BankSlot,
    byte? ContainerSlot,
    byte ContainerItemSlot,
    uint StackCount);

public readonly record struct AutoStoreGuildBankItem(WowGuid128 BankGuid, byte BankTab, byte BankSlot);

public readonly record struct MoveGuildBankItem(
    WowGuid128 BankGuid,
    byte BankTab1,
    byte BankSlot1,
    byte BankTab2,
    byte BankSlot2);

public readonly record struct SplitGuildBankItem(
    WowGuid128 BankGuid,
    byte BankTab1,
    byte BankSlot1,
    byte BankTab2,
    byte BankSlot2,
    uint StackCount);

public readonly record struct RequestGuildPartyState(WowGuid128 GuildGuid);

/// <summary>
/// Whether the group the player is in counts as a guild group, and what it earns. 4.4.2 only.
/// </summary>
public sealed class GuildPartyState : ServerPacket
{
    public GuildPartyState() : base(Opcode.SMSG_GUILD_PARTY_STATE) { }

    public override void Write()
    {
        _worldPacket.WriteBit(InGuildParty);
        _worldPacket.FlushBits();
        _worldPacket.WriteInt32(NumMembers);
        _worldPacket.WriteInt32(NumRequired);
        _worldPacket.WriteFloat(GuildXPEarnedMult);
    }

    public bool InGuildParty;
    public int NumMembers;
    public int NumRequired;
    public float GuildXPEarnedMult;
}

/// <summary>
/// The guild challenges of the week: how many of each kind are done, how many count, and what
/// one pays. The 4.4.2 client has six kinds (none, dungeon, raid, rated battleground and two a
/// 4.3.4 server does not have), four numbers each.
/// </summary>
/// <remarks>
/// The gold comes before the gold at the level cap. TrinityCore cata_classic writes them the
/// other way round, and the client's guild window then showed 250 for a dungeon and 125 at the
/// cap, where the server had 125 and 250.
/// </remarks>
public sealed class GuildChallengeUpdate : ServerPacket
{
    public const int Types = 6;

    public GuildChallengeUpdate() : base(Opcode.SMSG_GUILD_CHALLENGE_UPDATE) { }

    public override void Write()
    {
        foreach (int count in CurrentCount)
            _worldPacket.WriteInt32(count);
        foreach (int count in MaxCount)
            _worldPacket.WriteInt32(count);
        foreach (int gold in Gold)
            _worldPacket.WriteInt32(gold);
        foreach (int gold in MaxLevelGold)
            _worldPacket.WriteInt32(gold);
    }

    public readonly int[] CurrentCount = new int[Types];
    public readonly int[] MaxCount = new int[Types];
    public readonly int[] MaxLevelGold = new int[Types];
    public readonly int[] Gold = new int[Types];
}

public readonly record struct GuildSetAchievementTracking(uint[] AchievementIds);

public readonly record struct RequestGuildRewardsList(long CurrentVersion);

/// <summary>
/// The day's bank withdrawal limits start anew; the client asks for its own again. Empty.
/// </summary>
public sealed class GuildMemberDailyReset : ServerPacket
{
    public GuildMemberDailyReset() : base(Opcode.SMSG_GUILD_MEMBER_DAILY_RESET) { }

    public override void Write() { }
}

/// <summary>
/// What the guild has earned: the record of a player's achievement list, a count before them.
/// </summary>
public sealed class AllGuildAchievements : ServerPacket
{
    public AllGuildAchievements() : base(Opcode.SMSG_ALL_GUILD_ACHIEVEMENTS) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Earned.Count);
        foreach (EarnedAchievement earned in Earned)
            earned.Write(_worldPacket);
    }

    public List<EarnedAchievement> Earned = [];
}

/// <summary>
/// The guild log: who invited, promoted or removed whom, who joined and who left.
/// </summary>
public sealed class GuildEventLogQueryResults : ServerPacket
{
    public GuildEventLogQueryResults() : base(Opcode.SMSG_GUILD_EVENT_LOG_QUERY_RESULTS) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Entries.Count);
        foreach (GuildEventEntry entry in Entries)
        {
            _worldPacket.WritePackedGuid128(entry.PlayerGUID);
            _worldPacket.WritePackedGuid128(entry.OtherGUID);
            _worldPacket.WriteUInt8(entry.TransactionType);
            _worldPacket.WriteUInt8(entry.RankID);
            _worldPacket.WriteUInt32(entry.TransactionDate);
        }
    }

    public List<GuildEventEntry> Entries = [];
}

public struct GuildEventEntry
{
    public WowGuid128 PlayerGUID;
    public WowGuid128 OtherGUID;
    public byte TransactionType;
    public byte RankID;
    public uint TransactionDate;    // seconds ago
}

/// <summary>
/// What the guild vendor sells, and the reputation, achievement and gold each asks for.
/// </summary>
public sealed class GuildRewardList : ServerPacket
{
    public GuildRewardList() : base(Opcode.SMSG_GUILD_REWARD_LIST) { }

    public override void Write()
    {
        _worldPacket.WriteInt64(Version);
        _worldPacket.WriteInt32(RewardItems.Count);
        foreach (GuildRewardItem item in RewardItems)
        {
            _worldPacket.WriteUInt32(item.ItemID);
            _worldPacket.WriteUInt32(item.Unk4);
            _worldPacket.WriteInt32(item.AchievementsRequired.Count);
            _worldPacket.WriteUInt64(item.RaceMask);
            _worldPacket.WriteInt32(item.MinGuildLevel);
            _worldPacket.WriteInt32(item.MinGuildRep);
            _worldPacket.WriteUInt64(item.Cost);
            foreach (uint achievementId in item.AchievementsRequired)
                _worldPacket.WriteUInt32(achievementId);
        }
    }

    public long Version;
    public List<GuildRewardItem> RewardItems = [];
}

public sealed class GuildRewardItem
{
    public uint ItemID;
    public uint Unk4;
    public List<uint> AchievementsRequired = [];
    public ulong RaceMask;
    public int MinGuildLevel;
    public int MinGuildRep;
    public ulong Cost;
}

public readonly record struct GuildQueryNews(WowGuid128 GuildGuid);

public readonly record struct GuildQueryRecipes(WowGuid128 GuildGuid);

// What the guild's members can craft: per profession (skill line) one bit per recipe, 300 bytes.
public sealed class GuildKnownRecipes : ServerPacket
{
    public const int RecipeBytes = 300;

    public GuildKnownRecipes() : base(Opcode.SMSG_GUILD_KNOWN_RECIPES) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32((uint)Recipes.Count);
        foreach (var (skillLineId, bits) in Recipes)
        {
            _worldPacket.WriteInt32(skillLineId);
            _worldPacket.WriteBytes(bits);
        }
    }

    public List<(int SkillLineId, byte[] Bits)> Recipes = [];
}

public readonly record struct GuildNewsUpdateSticky(WowGuid128 GuildGuid, int NewsId, bool Sticky);

/// <summary>
/// The guild window's news: who earned, looted, made or bought what, and what the guild reached.
/// The whole list in answer to a request, one entry when something happens or is pinned.
/// </summary>
public sealed class GuildNews : ServerPacket
{
    public GuildNews() : base(Opcode.SMSG_GUILD_NEWS) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Events.Count);
        foreach (GuildNewsEvent news in Events)
        {
            _worldPacket.WriteInt32(news.Id);
            _worldPacket.WritePackedTime(news.CompletedDate);
            _worldPacket.WriteInt32(news.Type);
            _worldPacket.WriteInt32(news.Flags);
            foreach (int data in news.Data)
                _worldPacket.WriteInt32(data);
            _worldPacket.WritePackedGuid128(news.MemberGuid);
            _worldPacket.WriteInt32(news.MemberList.Count);
            foreach (WowGuid128 member in news.MemberList)
                _worldPacket.WritePackedGuid128(member);
            _worldPacket.WriteBit(news.Item != null);
            _worldPacket.FlushBits();
            news.Item?.Write(_worldPacket);
        }
    }

    public List<GuildNewsEvent> Events = [];
}

public sealed class GuildNewsEvent
{
    public int Id;
    public long CompletedDate;      // unix time; WritePackedTime packs it
    public int Type;
    public int Flags;
    public readonly int[] Data = new int[2];
    public WowGuid128 MemberGuid = WowGuid128.Empty;
    public List<WowGuid128> MemberList = [];
    public ItemInstance? Item;
}
