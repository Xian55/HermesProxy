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


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    class BattlefieldList : ServerPacket, ISpanWritable
    {
        public BattlefieldList() : base(Opcode.SMSG_BATTLEFIELD_LIST) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(BattlemasterGuid);
            _worldPacket.WriteInt32(Verification);
            _worldPacket.WriteUInt32(BattlemasterListID);
            _worldPacket.WriteUInt8(MinLevel);
            _worldPacket.WriteUInt8(MaxLevel);
            _worldPacket.WriteInt32(BattlefieldInstances.Count);

            foreach (var field in BattlefieldInstances)
                _worldPacket.WriteInt32(field);

            _worldPacket.WriteBit(PvpAnywhere);
            _worldPacket.WriteBit(HasRandomWinToday);
            _worldPacket.FlushBits();
        }

        // Cap for battlefield instances - rarely more than a few
        private const int MaxInstances = 10;
        // GUID(18) + int + uint + 2 bytes + count(4) + instances(4 each) + 1 byte bits
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + 4 + 2 + 4 + MaxInstances * 4 + 1;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (BattlefieldInstances.Count > MaxInstances)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(BattlemasterGuid.Low, BattlemasterGuid.High);
            writer.WriteInt32(Verification);
            writer.WriteUInt32(BattlemasterListID);
            writer.WriteUInt8(MinLevel);
            writer.WriteUInt8(MaxLevel);
            writer.WriteInt32(BattlefieldInstances.Count);

            foreach (var field in BattlefieldInstances)
                writer.WriteInt32(field);

            writer.WriteBit(PvpAnywhere);
            writer.WriteBit(HasRandomWinToday);
            writer.FlushBits();
            return writer.Position;
        }

        public WowGuid128 BattlemasterGuid;
        public int Verification = 121761856;
        public uint BattlemasterListID;
        public byte MinLevel = 70;
        public byte MaxLevel = 70;
        public List<int> BattlefieldInstances = new();
        public bool PvpAnywhere;
        public bool HasRandomWinToday;
    }

    class BattlemasterJoin : ClientPacket
    {
        public BattlemasterJoin(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            long queueId = _worldPacket.ReadInt64();
            BattlefieldListId = (uint)(queueId & ~0x1F10000000000000);
            Roles = _worldPacket.ReadUInt8();
            BlacklistMap[0] = _worldPacket.ReadInt32();
            BlacklistMap[1] = _worldPacket.ReadInt32();
            BattlemasterGuid = _worldPacket.ReadPackedGuid128();
            Verification = _worldPacket.ReadInt32();
            BattlefieldInstanceID = _worldPacket.ReadInt32();
            JoinAsGroup = _worldPacket.HasBit();

        }

        public uint BattlefieldListId;
        public byte Roles;
        public int[] BlacklistMap = new int[2];
        public WowGuid128 BattlemasterGuid;
        public int Verification;
        public int BattlefieldInstanceID;
        public bool JoinAsGroup;
    }

    public class BattlefieldStatusNeedConfirmation : ServerPacket, ISpanWritable
    {
        public BattlefieldStatusNeedConfirmation() : base(Opcode.SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION) { }

        public override void Write()
        {
            Hdr.Write(_worldPacket);
            _worldPacket.WriteUInt32(Mapid);
            _worldPacket.WriteUInt32(Timeout);
            _worldPacket.WriteUInt8(Role);
        }

        // MaxSize: BattlefieldStatusHeader + 2 uints(8) + byte(1)
        // BattlefieldStatusHeader: RideTicket(34) + opt byte(1) + int(4) + 3 bytes(3) + uint(4) + max 4 bgIds(32) + bits(2 -> 1) = 79
        private const int MaxBattlefieldIds = 4;
        private const int HeaderSize = 34 + 1 + 4 + 3 + 4 + MaxBattlefieldIds * 8 + 1;
        public int MaxSize => HeaderSize + 9;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Hdr.BattlefieldListIDs.Count > MaxBattlefieldIds)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            // Inline BattlefieldStatusHeader write
            writer.WritePackedGuid128(Hdr.Ticket.RequesterGuid.Low, Hdr.Ticket.RequesterGuid.High);
            writer.WriteUInt32(Hdr.Ticket.Id);
            writer.WriteUInt32((uint)Hdr.Ticket.Type);
            writer.WriteInt64(Hdr.Ticket.Time);

            if (ModernVersion.AddedInClassicVersion(1, 14, 3, 2, 5, 4))
                writer.WriteUInt8(Hdr.Unk254);

            writer.WriteInt32(Hdr.BattlefieldListIDs.Count);
            writer.WriteUInt8(Hdr.RangeMin);
            writer.WriteUInt8(Hdr.RangeMax);
            writer.WriteUInt8(Hdr.ArenaTeamSize);
            writer.WriteUInt32(Hdr.InstanceID);

            foreach (ulong bgId in Hdr.BattlefieldListIDs)
            {
                ulong queueID = bgId | 0x1F10000000000000;
                writer.WriteUInt64(queueID);
            }

            writer.WriteBit(Hdr.IsArena);
            writer.WriteBit(Hdr.TournamentRules);
            writer.FlushBits();

            writer.WriteUInt32(Mapid);
            writer.WriteUInt32(Timeout);
            writer.WriteUInt8(Role);
            return writer.Position;
        }

        public BattlefieldStatusHeader Hdr = new();
        public uint Mapid;
        public uint Timeout;
        public byte Role;
    }

    public class BattlefieldStatusQueued : ServerPacket, ISpanWritable
    {
        public BattlefieldStatusQueued() : base(Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED) { }

        public override void Write()
        {
            Hdr.Write(_worldPacket);
            _worldPacket.WriteUInt32(AverageWaitTime);
            _worldPacket.WriteUInt32(WaitTime);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 3, 2, 5, 4))
                _worldPacket.WriteInt32(Unk254);

            _worldPacket.WriteBit(AsGroup);
            _worldPacket.WriteBit(EligibleForMatchmaking);
            _worldPacket.WriteBit(SuspendedQueue);
            _worldPacket.FlushBits();
        }

        // MaxSize: BattlefieldStatusHeader(79) + 2 uints(8) + opt int(4) + 3 bits(1) = 92
        private const int MaxBattlefieldIds = 4;
        private const int HeaderSize = 34 + 1 + 4 + 3 + 4 + MaxBattlefieldIds * 8 + 1;
        public int MaxSize => HeaderSize + 13;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Hdr.BattlefieldListIDs.Count > MaxBattlefieldIds)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            // Inline BattlefieldStatusHeader write
            writer.WritePackedGuid128(Hdr.Ticket.RequesterGuid.Low, Hdr.Ticket.RequesterGuid.High);
            writer.WriteUInt32(Hdr.Ticket.Id);
            writer.WriteUInt32((uint)Hdr.Ticket.Type);
            writer.WriteInt64(Hdr.Ticket.Time);

            if (ModernVersion.AddedInClassicVersion(1, 14, 3, 2, 5, 4))
                writer.WriteUInt8(Hdr.Unk254);

            writer.WriteInt32(Hdr.BattlefieldListIDs.Count);
            writer.WriteUInt8(Hdr.RangeMin);
            writer.WriteUInt8(Hdr.RangeMax);
            writer.WriteUInt8(Hdr.ArenaTeamSize);
            writer.WriteUInt32(Hdr.InstanceID);

            foreach (ulong bgId in Hdr.BattlefieldListIDs)
            {
                ulong queueID = bgId | 0x1F10000000000000;
                writer.WriteUInt64(queueID);
            }

            writer.WriteBit(Hdr.IsArena);
            writer.WriteBit(Hdr.TournamentRules);
            writer.FlushBits();

            writer.WriteUInt32(AverageWaitTime);
            writer.WriteUInt32(WaitTime);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 3, 2, 5, 4))
                writer.WriteInt32(Unk254);

            writer.WriteBit(AsGroup);
            writer.WriteBit(EligibleForMatchmaking);
            writer.WriteBit(SuspendedQueue);
            writer.FlushBits();
            return writer.Position;
        }

        public BattlefieldStatusHeader Hdr = new();
        public uint AverageWaitTime;
        public uint WaitTime;
        public int Unk254;
        public bool AsGroup;
        public bool EligibleForMatchmaking = true;
        public bool SuspendedQueue;
    }

    public class BattlefieldStatusFailed : ServerPacket, ISpanWritable
    {
        public BattlefieldStatusFailed() : base(Opcode.SMSG_BATTLEFIELD_STATUS_FAILED) { }

        public override void Write()
        {
            Ticket.Write(_worldPacket);

            ulong queueID = BattlefieldListId | 0x1F10000000000000;
            _worldPacket.WriteUInt64(queueID);
            _worldPacket.WriteInt32(Reason);
            _worldPacket.WritePackedGuid128(ClientID);
        }

        // RideTicket: GUID(18) + uint(4) + uint(4) + long(8) = 34
        // Rest: ulong(8) + int(4) + GUID(18) = 30
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 16 + 8 + 4 + PackedGuidHelper.MaxPackedGuid128Size;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            // Inline RideTicket write
            writer.WritePackedGuid128(Ticket.RequesterGuid.Low, Ticket.RequesterGuid.High);
            writer.WriteUInt32(Ticket.Id);
            writer.WriteUInt32((uint)Ticket.Type);
            writer.WriteInt64(Ticket.Time);

            ulong queueID = BattlefieldListId | 0x1F10000000000000;
            writer.WriteUInt64(queueID);
            writer.WriteInt32(Reason);
            writer.WritePackedGuid128(ClientID.Low, ClientID.High);
            return writer.Position;
        }

        public RideTicket Ticket = new();
        public byte Unk;
        public ulong BattlefieldListId;
        public WowGuid128 ClientID = WowGuid128.Empty;
        public int Reason;
    }

    public class BattlefieldStatusHeader
    {
        public void Write(WorldPacket data)
        {
            Ticket.Write(data);

            if (ModernVersion.AddedInClassicVersion(1, 14, 3, 2, 5, 4))
                data.WriteUInt8(Unk254);

            data.WriteInt32(BattlefieldListIDs.Count);
            data.WriteUInt8(RangeMin);
            data.WriteUInt8(RangeMax);
            data.WriteUInt8(ArenaTeamSize);
            data.WriteUInt32(InstanceID);

            foreach (ulong bgId in BattlefieldListIDs)
            {
                ulong queueID = bgId | 0x1F10000000000000;
                data.WriteUInt64(queueID);
            }

            data.WriteBit(IsArena);
            data.WriteBit(TournamentRules);
            data.FlushBits();
        }

        public RideTicket Ticket = new();
        public List<uint> BattlefieldListIDs = new();
        public byte Unk254;
        public byte RangeMin;
        public byte RangeMax = 70;
        public byte ArenaTeamSize;
        public uint InstanceID;
        public bool IsArena;
        public bool TournamentRules;
    }

    public class RideTicket
    {
        public void Read(WorldPacket data)
        {
            RequesterGuid = data.ReadPackedGuid128();
            Id = data.ReadUInt32();
            Type = (RideType)data.ReadUInt32();
            Time = data.ReadInt64();
        }

        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(RequesterGuid);
            data.WriteUInt32(Id);
            data.WriteUInt32((uint)Type);
            data.WriteInt64(Time);
        }

        public WowGuid128 RequesterGuid = WowGuid128.Empty;
        public uint Id;
        public RideType Type;
        public long Time;
    }

    public enum RideType
    {
        None = 0,
        Battlegrounds = 1,
        Lfg = 2
    }

    class BattlefieldPort : ClientPacket
    {
        public BattlefieldPort(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Ticket.Read(_worldPacket);
            AcceptedInvite = _worldPacket.HasBit();
        }

        public RideTicket Ticket = new();
        public bool AcceptedInvite;
    }

    public class BattlefieldStatusActive : ServerPacket, ISpanWritable
    {
        public BattlefieldStatusActive() : base(Opcode.SMSG_BATTLEFIELD_STATUS_ACTIVE) { }

        public override void Write()
        {
            Hdr.Write(_worldPacket);
            _worldPacket.WriteUInt32(Mapid);
            _worldPacket.WriteUInt32(ShutdownTimer);
            _worldPacket.WriteUInt32(StartTimer);
            _worldPacket.WriteBit(ArenaFaction != 0);
            _worldPacket.WriteBit(LeftEarly);
            _worldPacket.FlushBits();
        }

        // MaxSize: BattlefieldStatusHeader(79) + 3 uints(12) + 2 bits(1) = 92
        private const int MaxBattlefieldIds = 4;
        private const int HeaderSize = 34 + 1 + 4 + 3 + 4 + MaxBattlefieldIds * 8 + 1;
        public int MaxSize => HeaderSize + 13;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Hdr.BattlefieldListIDs.Count > MaxBattlefieldIds)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            // Inline BattlefieldStatusHeader write
            writer.WritePackedGuid128(Hdr.Ticket.RequesterGuid.Low, Hdr.Ticket.RequesterGuid.High);
            writer.WriteUInt32(Hdr.Ticket.Id);
            writer.WriteUInt32((uint)Hdr.Ticket.Type);
            writer.WriteInt64(Hdr.Ticket.Time);

            if (ModernVersion.AddedInClassicVersion(1, 14, 3, 2, 5, 4))
                writer.WriteUInt8(Hdr.Unk254);

            writer.WriteInt32(Hdr.BattlefieldListIDs.Count);
            writer.WriteUInt8(Hdr.RangeMin);
            writer.WriteUInt8(Hdr.RangeMax);
            writer.WriteUInt8(Hdr.ArenaTeamSize);
            writer.WriteUInt32(Hdr.InstanceID);

            foreach (ulong bgId in Hdr.BattlefieldListIDs)
            {
                ulong queueID = bgId | 0x1F10000000000000;
                writer.WriteUInt64(queueID);
            }

            writer.WriteBit(Hdr.IsArena);
            writer.WriteBit(Hdr.TournamentRules);
            writer.FlushBits();

            writer.WriteUInt32(Mapid);
            writer.WriteUInt32(ShutdownTimer);
            writer.WriteUInt32(StartTimer);
            writer.WriteBit(ArenaFaction != 0);
            writer.WriteBit(LeftEarly);
            writer.FlushBits();
            return writer.Position;
        }

        public BattlefieldStatusHeader Hdr = new();
        public uint Mapid;
        public uint ShutdownTimer;
        public uint StartTimer;
        public byte ArenaFaction;
        public bool LeftEarly;
    }

    public class BattlegroundInit : ServerPacket, ISpanWritable
    {
        public BattlegroundInit() : base(Opcode.SMSG_BATTLEGROUND_INIT) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Milliseconds);
            _worldPacket.WriteUInt16(BattlegroundPoints);
        }

        public int MaxSize => 6; // uint + ushort

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(Milliseconds);
            writer.WriteUInt16(BattlegroundPoints);
            return writer.Position;
        }

        public uint Milliseconds;
        public ushort BattlegroundPoints;
    }

    class RequestBattlefieldStatus : ClientPacket
    {
        public RequestBattlefieldStatus(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    class PVPLogDataRequest : ClientPacket
    {
        public PVPLogDataRequest(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class PVPMatchStatisticsMessage : ServerPacket
    {
        public PVPMatchStatisticsMessage() : base(Opcode.SMSG_PVP_MATCH_STATISTICS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteBit(Ratings != null);
            _worldPacket.WriteBit(ArenaTeams != null);
            _worldPacket.WriteBit(Winner != null);

            if (ArenaTeams != null)
                ArenaTeams.Write(_worldPacket);

            _worldPacket.WriteInt32(Statistics.Count);

            foreach (var count in PlayerCount)
                _worldPacket.WriteInt8(count);

            if (Ratings != null)
                Ratings.Write(_worldPacket);

            if (Winner != null)
                _worldPacket.WriteUInt8((byte)Winner);

            foreach (var player in Statistics)
                player.Write(_worldPacket);
        }

        public RatingData Ratings;
        public ArenaTeamsInfo ArenaTeams;
        public byte? Winner;
        public List<PVPMatchPlayerStatistics> Statistics = new();
        public sbyte[] PlayerCount = new sbyte[2];

        public class ArenaTeamsInfo
        {
            public void Write(WorldPacket data)
            {
                foreach (var str in Names)
                    data.WriteBits(str.GetByteCount(), 7);
                data.FlushBits();

                for (int i = 0; i < 2; i++)
                {
                    data.WritePackedGuid128(Guids[i]);
                    data.WriteString(Names[i]);
                }
            }

            public WowGuid128[] Guids = new WowGuid128[2];
            public string[] Names = new string[2];
        }

        public class RatingData
        {
            public void Write(WorldPacket data)
            {
                foreach (var id in Prematch)
                    data.WriteUInt32(id);

                foreach (var id in Postmatch)
                    data.WriteUInt32(id);

                foreach (var id in PrematchMMR)
                    data.WriteUInt32(id);
            }

            public uint[] Prematch = new uint[2];
            public uint[] Postmatch = new uint[2];
            public uint[] PrematchMMR = new uint[2];
        }

        public class HonorData
        {
            public void Write(WorldPacket data)
            {
                data.WriteUInt32(HonorKills);
                data.WriteUInt32(Deaths);
                data.WriteUInt32(ContributionPoints);
            }

            public uint HonorKills;
            public uint Deaths;
            public uint ContributionPoints;
        }

        public class PVPMatchPlayerStatistics
        {
            public void Write(WorldPacket data)
            {
                data.WritePackedGuid128(PlayerGUID);
                data.WriteUInt32(Kills);
                data.WriteUInt32(DamageDone);
                data.WriteUInt32(HealingDone);
                data.WriteInt32(Stats.Count);
                data.WriteInt32(PrimaryTalentTree);
                data.WriteUInt32((uint)Sex);
                data.WriteUInt32((uint)PlayerRace);
                data.WriteUInt32((uint)PlayerClass);
                data.WriteInt32(CreatureID);
                data.WriteInt32(HonorLevel);
                data.WriteInt32(Rank);

                foreach (var pvpStat in Stats)
                    data.WriteUInt32(pvpStat);

                data.WriteBit(Faction);
                data.WriteBit(IsInWorld);
                data.WriteBit(Honor != null);
                data.WriteBit(PreMatchRating.HasValue);
                data.WriteBit(RatingChange.HasValue);
                data.WriteBit(PreMatchMMR.HasValue);
                data.WriteBit(MmrChange.HasValue);
                data.FlushBits();

                if (Honor != null)
                    Honor.Write(data);

                if (PreMatchRating.HasValue)
                    data.WriteUInt32(PreMatchRating.Value);

                if (RatingChange.HasValue)
                    data.WriteInt32(RatingChange.Value);

                if (PreMatchMMR.HasValue)
                    data.WriteUInt32(PreMatchMMR.Value);

                if (MmrChange.HasValue)
                    data.WriteInt32(MmrChange.Value);
            }

            public WowGuid128 PlayerGUID;
            public uint Kills;
            public bool Faction;
            public bool IsInWorld = true;
            public HonorData Honor;
            public uint DamageDone;
            public uint HealingDone;
            public uint? PreMatchRating;
            public int? RatingChange;
            public uint? PreMatchMMR;
            public int? MmrChange;
            public List<uint> Stats = new();
            public int PrimaryTalentTree;
            public Gender Sex;
            public Race PlayerRace;
            public Class PlayerClass;
            public int CreatureID;
            public int HonorLevel = 1;
            public int Rank;
        }
    }

    class BattlefieldLeave : ClientPacket
    {
        public BattlefieldLeave(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    class BattlegroundPlayerPositions : ServerPacket, ISpanWritable
    {
        public BattlegroundPlayerPositions() : base(Opcode.SMSG_BATTLEGROUND_PLAYER_POSITIONS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(FlagCarriers.Count);
            foreach (var pos in FlagCarriers)
                pos.Write(_worldPacket);
        }

        // Cap for flag carriers - typically 2 (one per team), max 4 for safety
        private const int MaxFlagCarriers = 4;
        // Each position: GUID(18) + Vector2(8) + 2 bytes = 28
        private const int PositionSize = PackedGuidHelper.MaxPackedGuid128Size + 8 + 2;
        public int MaxSize => 4 + MaxFlagCarriers * PositionSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (FlagCarriers.Count > MaxFlagCarriers)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(FlagCarriers.Count);
            foreach (var pos in FlagCarriers)
            {
                writer.WritePackedGuid128(pos.Guid.Low, pos.Guid.High);
                writer.WriteVector2(pos.Pos);
                writer.WriteInt8(pos.IconID);
                writer.WriteInt8(pos.ArenaSlot);
            }
            return writer.Position;
        }

        public List<BattlegroundPlayerPosition> FlagCarriers = new();
    }

    public struct BattlegroundPlayerPosition
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(Guid);
            data.WriteVector2(Pos);
            data.WriteInt8(IconID);
            data.WriteInt8(ArenaSlot);
        }

        public WowGuid128 Guid;
        public Vector2 Pos;
        public sbyte IconID;
        public sbyte ArenaSlot;
    }

    class BattlegroundPlayerLeftOrJoined : ServerPacket, ISpanWritable
    {
        public BattlegroundPlayerLeftOrJoined(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(Guid.Low, Guid.High);
            return writer.Position;
        }

        public WowGuid128 Guid;
    }

    public class AreaSpiritHealerTime : ServerPacket, ISpanWritable
    {
        public AreaSpiritHealerTime() : base(Opcode.SMSG_AREA_SPIRIT_HEALER_TIME) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(HealerGuid);
            _worldPacket.WriteUInt32(TimeLeft);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4; // GUID + uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(HealerGuid.Low, HealerGuid.High);
            writer.WriteUInt32(TimeLeft);
            return writer.Position;
        }

        public WowGuid128 HealerGuid;
        public uint TimeLeft;
    }

    class PvPCredit : ServerPacket, ISpanWritable
    {
        public PvPCredit() : base(Opcode.SMSG_PVP_CREDIT) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(OriginalHonor);
            _worldPacket.WriteInt32(Honor);
            _worldPacket.WritePackedGuid128(Target);
            _worldPacket.WriteUInt32(Rank);
        }

        public int MaxSize => 12 + PackedGuidHelper.MaxPackedGuid128Size; // 2 ints + GUID + uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(OriginalHonor);
            writer.WriteInt32(Honor);
            writer.WritePackedGuid128(Target.Low, Target.High);
            writer.WriteUInt32(Rank);
            return writer.Position;
        }

        public int OriginalHonor;
        public int Honor;
        public WowGuid128 Target;
        public uint Rank;
    }

    class PlayerSkinned : ServerPacket, ISpanWritable
    {
        public PlayerSkinned() : base(Opcode.SMSG_PLAYER_SKINNED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteBit(FreeRepop);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 1; // 1 byte for bit

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteBit(FreeRepop);
            writer.FlushBits();
            return writer.Position;
        }

        public bool FreeRepop;
    }
}
