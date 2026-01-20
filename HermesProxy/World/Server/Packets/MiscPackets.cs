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
using System.Collections.Generic;
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets
{
    public class EmptyClientPacket : ClientPacket
    {
        public EmptyClientPacket(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            System.Diagnostics.Trace.Assert(!_worldPacket.CanRead());
        }
    }

    public class BindPointUpdate : ServerPacket, ISpanWritable
    {
        public BindPointUpdate() : base(Opcode.SMSG_BIND_POINT_UPDATE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteVector3(BindPosition);
            _worldPacket.WriteUInt32(BindMapID);
            _worldPacket.WriteUInt32(BindAreaID);
        }

        public int MaxSize => 12 + 4 + 4; // Vector3 (12) + 2x uint32 (8)

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteVector3(BindPosition);
            writer.WriteUInt32(BindMapID);
            writer.WriteUInt32(BindAreaID);
            return writer.Position;
        }

        public uint BindMapID = 0xFFFFFFFF;
        public Vector3 BindPosition;
        public uint BindAreaID;
    }

    public class PlayerBound : ServerPacket, ISpanWritable
    {
        public PlayerBound() : base(Opcode.SMSG_PLAYER_BOUND) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(BinderGUID);
            _worldPacket.WriteUInt32(AreaID);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4; // Packed GUID + uint32

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(BinderGUID.Low, BinderGUID.High);
            writer.WriteUInt32(AreaID);
            return writer.Position;
        }

        public WowGuid128 BinderGUID;
        public uint AreaID;
    }

    public class ServerTimeOffset : ServerPacket, ISpanWritable
    {
        public ServerTimeOffset() : base(Opcode.SMSG_SERVER_TIME_OFFSET) { }

        public override void Write()
        {
            _worldPacket.WriteInt64(Time);
        }

        public int MaxSize => 8;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt64(Time);
            return writer.Position;
        }

        public long Time;
    }

    public class TutorialSetFlag : ClientPacket
    {
        public TutorialSetFlag(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Action = (TutorialAction)_worldPacket.ReadBits<byte>(2);
            if (Action == TutorialAction.Update)
                TutorialBit = _worldPacket.ReadUInt32();
        }

        public TutorialAction Action;
        public uint TutorialBit;
    }

    public class TutorialFlags : ServerPacket, ISpanWritable
    {
        public TutorialFlags() : base(Opcode.SMSG_TUTORIAL_FLAGS) { }

        public override void Write()
        {
            for (byte i = 0; i < (int)Tutorials.Max; ++i)
                _worldPacket.WriteUInt32(TutorialData[i]);
        }

        public int MaxSize => (int)Tutorials.Max * 4; // 8 uint32s = 32 bytes

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            for (byte i = 0; i < (int)Tutorials.Max; ++i)
                writer.WriteUInt32(TutorialData[i]);
            return writer.Position;
        }

        public uint[] TutorialData = new uint[(int)Tutorials.Max];
    }

    public class CorpseReclaimDelay : ServerPacket, ISpanWritable
    {
        public CorpseReclaimDelay() : base(Opcode.SMSG_CORPSE_RECLAIM_DELAY, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Remaining);
        }

        public int MaxSize => 4;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(Remaining);
            return writer.Position;
        }

        public uint Remaining;
    }

    public class SetupCurrency : ServerPacket, ISpanWritable
    {
        public SetupCurrency() : base(Opcode.SMSG_SETUP_CURRENCY, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Data.Count);

            foreach (Record data in Data)
            {
                _worldPacket.WriteUInt32(data.Type);
                _worldPacket.WriteUInt32(data.Quantity);

                _worldPacket.WriteBit(data.WeeklyQuantity.HasValue);
                _worldPacket.WriteBit(data.MaxWeeklyQuantity.HasValue);
                _worldPacket.WriteBit(data.TrackedQuantity.HasValue);
                _worldPacket.WriteBit(data.MaxQuantity.HasValue);
                _worldPacket.WriteBit(data.Unused901.HasValue);
                _worldPacket.WriteBits(data.Flags, 5);
                _worldPacket.FlushBits();

                if (data.WeeklyQuantity.HasValue)
                    _worldPacket.WriteUInt32(data.WeeklyQuantity.Value);
                if (data.MaxWeeklyQuantity.HasValue)
                    _worldPacket.WriteUInt32(data.MaxWeeklyQuantity.Value);
                if (data.TrackedQuantity.HasValue)
                    _worldPacket.WriteUInt32(data.TrackedQuantity.Value);
                if (data.MaxQuantity.HasValue)
                    _worldPacket.WriteInt32(data.MaxQuantity.Value);
                if (data.Unused901.HasValue)
                    _worldPacket.WriteInt32(data.Unused901.Value);
            }
        }

        // Cap for currencies - reduced from 128 to 16 based on typical usage (0 observed at login)
        private const int MaxCurrencies = 16;
        // Per currency max: 2 uints(8) + bits(2) + 5 optional uints(20) = 30 bytes
        private const int MaxRecordSize = 30;
        // count(4) + currencies
        public int MaxSize => 4 + MaxCurrencies * MaxRecordSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Data.Count > MaxCurrencies)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(Data.Count);

            foreach (Record data in Data)
            {
                writer.WriteUInt32(data.Type);
                writer.WriteUInt32(data.Quantity);

                writer.WriteBit(data.WeeklyQuantity.HasValue);
                writer.WriteBit(data.MaxWeeklyQuantity.HasValue);
                writer.WriteBit(data.TrackedQuantity.HasValue);
                writer.WriteBit(data.MaxQuantity.HasValue);
                writer.WriteBit(data.Unused901.HasValue);
                writer.WriteBits(data.Flags, 5);
                writer.FlushBits();

                if (data.WeeklyQuantity.HasValue)
                    writer.WriteUInt32(data.WeeklyQuantity.Value);
                if (data.MaxWeeklyQuantity.HasValue)
                    writer.WriteUInt32(data.MaxWeeklyQuantity.Value);
                if (data.TrackedQuantity.HasValue)
                    writer.WriteUInt32(data.TrackedQuantity.Value);
                if (data.MaxQuantity.HasValue)
                    writer.WriteInt32(data.MaxQuantity.Value);
                if (data.Unused901.HasValue)
                    writer.WriteInt32(data.Unused901.Value);
            }
            return writer.Position;
        }

        public List<Record> Data = new();

        public struct Record
        {
            public uint Type;
            public uint Quantity;
            public uint? WeeklyQuantity;       // Currency count obtained this Week.
            public uint? MaxWeeklyQuantity;    // Weekly Currency cap.
            public uint? TrackedQuantity;
            public int? MaxQuantity;
            public int? Unused901;
            public byte Flags;                      // 0 = none,
        }
    }

    class AllAccountCriteria : ServerPacket, ISpanWritable
    {
        public AllAccountCriteria() : base(Opcode.SMSG_ALL_ACCOUNT_CRITERIA, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Progress.Count);
            foreach (var progress in Progress)
                progress.Write(_worldPacket);
        }

        // Cap for account criteria - reduced from 256 to 32 based on typical usage (0 observed)
        private const int MaxCriteria = 32;
        // Per criteria: uint(4) + ulong(8) + GUID(18) + PackedTime(4) + 2 uints(8) + bits(1) + optional ulong(8) = 51 bytes max
        private const int MaxCriteriaSize = 51;
        // count(4) + criteria
        public int MaxSize => 4 + MaxCriteria * MaxCriteriaSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Progress.Count > MaxCriteria)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(Progress.Count);
            foreach (var progress in Progress)
            {
                writer.WriteUInt32(progress.Id);
                writer.WriteUInt64(progress.Quantity);
                writer.WritePackedGuid128(progress.Player.Low, progress.Player.High);
                writer.WriteUInt32(Time.GetPackedTimeFromUnixTime(progress.Date));
                writer.WriteUInt32(progress.TimeFromStart);
                writer.WriteUInt32(progress.TimeFromCreate);
                writer.WriteBits(progress.Flags, 4);
                writer.WriteBit(progress.RafAcceptanceID.HasValue);
                writer.FlushBits();

                if (progress.RafAcceptanceID.HasValue)
                    writer.WriteUInt64(progress.RafAcceptanceID.Value);
            }
            return writer.Position;
        }

        public List<CriteriaProgressPkt> Progress = new();
    }

    public struct CriteriaProgressPkt
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Id);
            data.WriteUInt64(Quantity);
            data.WritePackedGuid128(Player);
            data.WritePackedTime(Date);
            data.WriteUInt32(TimeFromStart);
            data.WriteUInt32(TimeFromCreate);
            data.WriteBits(Flags, 4);
            data.WriteBit(RafAcceptanceID.HasValue);
            data.FlushBits();

            if (RafAcceptanceID.HasValue)
                data.WriteUInt64(RafAcceptanceID.Value);
        }

        public uint Id;
        public ulong Quantity;
        public WowGuid128 Player;
        public uint Flags;
        public long Date;
        public uint TimeFromStart;
        public uint TimeFromCreate;
        public ulong? RafAcceptanceID;
    }

    public class TimeSyncRequest : ServerPacket, ISpanWritable
    {
        public TimeSyncRequest() : base(Opcode.SMSG_TIME_SYNC_REQUEST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(SequenceIndex);
        }

        public int MaxSize => 4; // uint32

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(SequenceIndex);
            return writer.Position;
        }

        public uint SequenceIndex;
    }

    public class TimeSyncResponse : ClientPacket
    {
        public TimeSyncResponse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            SequenceIndex = _worldPacket.ReadUInt32();
            ClientTime = _worldPacket.ReadUInt32();
        }

        public uint ClientTime; // Client ticks in ms
        public uint SequenceIndex; // Same index as in request
    }

    public class WeatherPkt : ServerPacket, ISpanWritable
    {
        public WeatherPkt() : base(Opcode.SMSG_WEATHER, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32((uint)WeatherID);
            _worldPacket.WriteFloat(Intensity);
            _worldPacket.WriteBit(Abrupt);

            _worldPacket.FlushBits();
        }

        public int MaxSize => 4 + 4 + 1; // uint32 + float + 1 byte for bit

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32((uint)WeatherID);
            writer.WriteFloat(Intensity);
            writer.WriteBit(Abrupt);
            writer.FlushBits();
            return writer.Position;
        }

        public bool Abrupt;
        public float Intensity;
        public WeatherState WeatherID;
    }

    class StartLightningStorm : ServerPacket, ISpanWritable
    {
        public StartLightningStorm() : base(Opcode.SMSG_START_LIGHTNING_STORM, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(LightningStormId);
        }

        public int MaxSize => 4; // uint32

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(LightningStormId);
            return writer.Position;
        }

        public uint LightningStormId;
    }

    public class LoginSetTimeSpeed : ServerPacket, ISpanWritable
    {
        public LoginSetTimeSpeed() : base(Opcode.SMSG_LOGIN_SET_TIME_SPEED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(ServerTime);
            _worldPacket.WriteUInt32(GameTime);
            _worldPacket.WriteFloat(NewSpeed);
            _worldPacket.WriteInt32(ServerTimeHolidayOffset);
            _worldPacket.WriteInt32(GameTimeHolidayOffset);
        }

        public int MaxSize => 20; // 2 uints + float + 2 ints

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(ServerTime);
            writer.WriteUInt32(GameTime);
            writer.WriteFloat(NewSpeed);
            writer.WriteInt32(ServerTimeHolidayOffset);
            writer.WriteInt32(GameTimeHolidayOffset);
            return writer.Position;
        }

        public uint ServerTime;
        public uint GameTime;
        public float NewSpeed;
        public int ServerTimeHolidayOffset;
        public int GameTimeHolidayOffset;
    }

    class AreaTriggerPkt : ClientPacket
    {
        public AreaTriggerPkt(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            AreaTriggerID = _worldPacket.ReadUInt32();
            Entered = _worldPacket.HasBit();
            FromClient = _worldPacket.HasBit();
        }

        public uint AreaTriggerID;
        public bool Entered;
        public bool FromClient;
    }

    class AreaTriggerMessage : ServerPacket, ISpanWritable
    {
        public AreaTriggerMessage() : base(Opcode.SMSG_AREA_TRIGGER_MESSAGE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(AreaTriggerID);
        }

        public int MaxSize => 4; // uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(AreaTriggerID);
            return writer.Position;
        }

        public uint AreaTriggerID = 0;
    }

    public class SetSelection : ClientPacket
    {
        public SetSelection(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TargetGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 TargetGUID;
    }

    public class WorldServerInfo : ServerPacket, ISpanWritable
    {
        public WorldServerInfo() : base(Opcode.SMSG_WORLD_SERVER_INFO, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(DifficultyID);
            _worldPacket.WriteUInt8(IsTournamentRealm);
            _worldPacket.WriteBit(XRealmPvpAlert);
            _worldPacket.WriteBit(RestrictedAccountMaxLevel.HasValue);
            _worldPacket.WriteBit(RestrictedAccountMaxMoney.HasValue);
            _worldPacket.WriteBit(InstanceGroupSize.HasValue);
            _worldPacket.FlushBits();

            if (RestrictedAccountMaxLevel.HasValue)
                _worldPacket.WriteUInt32(RestrictedAccountMaxLevel.Value);

            if (RestrictedAccountMaxMoney.HasValue)
                _worldPacket.WriteUInt64(RestrictedAccountMaxMoney.Value);

            if (InstanceGroupSize.HasValue)
                _worldPacket.WriteUInt32(InstanceGroupSize.Value);
        }

        // uint(4) + byte(1) + 4 bits(1) + optional uint(4) + optional ulong(8) + optional uint(4) = 22
        public int MaxSize => 4 + 1 + 1 + 4 + 8 + 4;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(DifficultyID);
            writer.WriteUInt8(IsTournamentRealm);
            writer.WriteBit(XRealmPvpAlert);
            writer.WriteBit(RestrictedAccountMaxLevel.HasValue);
            writer.WriteBit(RestrictedAccountMaxMoney.HasValue);
            writer.WriteBit(InstanceGroupSize.HasValue);
            writer.FlushBits();

            if (RestrictedAccountMaxLevel.HasValue)
                writer.WriteUInt32(RestrictedAccountMaxLevel.Value);

            if (RestrictedAccountMaxMoney.HasValue)
                writer.WriteUInt64(RestrictedAccountMaxMoney.Value);

            if (InstanceGroupSize.HasValue)
                writer.WriteUInt32(InstanceGroupSize.Value);

            return writer.Position;
        }

        public uint DifficultyID;
        public byte IsTournamentRealm;
        public bool XRealmPvpAlert;
        public uint? RestrictedAccountMaxLevel;
        public ulong? RestrictedAccountMaxMoney;
        public uint? InstanceGroupSize;
    }

    public class SetDungeonDifficulty : ClientPacket
    {
        public SetDungeonDifficulty(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            DifficultyID = _worldPacket.ReadUInt32();
        }

        public uint DifficultyID;
    }

    public class DungeonDifficultySet : ServerPacket, ISpanWritable
    {
        public DungeonDifficultySet() : base(Opcode.SMSG_SET_DUNGEON_DIFFICULTY) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(DifficultyID);
        }

        public int MaxSize => 4; // int

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(DifficultyID);
            return writer.Position;
        }

        public int DifficultyID;
    }

    public class SetAllTaskProgress : ServerPacket, ISpanWritable
    {
        public SetAllTaskProgress() : base(Opcode.SMSG_SET_ALL_TASK_PROGRESS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Tasks.Count);
            foreach (var task in Tasks)
                task.Write(_worldPacket);
        }

        // Cap for tasks - reduced from 64 to 8 based on typical usage (0 observed)
        private const int MaxTasks = 8;
        // Cap for progress items per task
        private const int MaxProgressPerTask = 16;
        // Per task: 4 uints(16) + count(4) + progress items(2 each) = 52 bytes max
        private const int MaxTaskSize = 16 + 4 + MaxProgressPerTask * 2;
        // count(4) + tasks
        public int MaxSize => 4 + MaxTasks * MaxTaskSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Tasks.Count > MaxTasks)
                return -1;

            // Pre-validate progress counts
            foreach (var task in Tasks)
            {
                if (task.Progress.Count > MaxProgressPerTask)
                    return -1;
            }

            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(Tasks.Count);
            foreach (var task in Tasks)
            {
                writer.WriteUInt32(task.TaskID);
                writer.WriteUInt32(task.FailureTime);
                writer.WriteUInt32(task.Flags);
                writer.WriteUInt32(task.Unk);
                writer.WriteInt32(task.Progress.Count);
                foreach (ushort progress in task.Progress)
                    writer.WriteUInt16(progress);
            }
            return writer.Position;
        }

        public List<TaskProgress> Tasks = new List<TaskProgress>();
    }

    public class TaskProgress
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(TaskID);
            data.WriteUInt32(FailureTime);
            data.WriteUInt32(Flags);
            data.WriteUInt32(Unk);
            data.WriteInt32(Progress.Count);
            foreach (ushort progress in Progress)
                data.WriteUInt16(progress);
        }
        public uint TaskID;
        public uint FailureTime;
        public uint Flags;
        public uint Unk;
        public List<ushort> Progress = new List<ushort>();
    }

    public class InitialSetup : ServerPacket, ISpanWritable
    {
        public InitialSetup() : base(Opcode.SMSG_INITIAL_SETUP, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8(ServerExpansionLevel);
            _worldPacket.WriteUInt8(ServerExpansionTier);
        }

        public int MaxSize => 2; // 2 bytes

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt8(ServerExpansionLevel);
            writer.WriteUInt8(ServerExpansionTier);
            return writer.Position;
        }

        public byte ServerExpansionLevel;
        public byte ServerExpansionTier;
    }

    public class RepopRequest : ClientPacket
    {
        public RepopRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            CheckInstance = _worldPacket.HasBit();
        }

        public bool CheckInstance;
    }

    public class QueryCorpseLocationFromClient : ClientPacket
    {
        public QueryCorpseLocationFromClient(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Player = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Player;
    }

    public class CorpseLocation : ServerPacket, ISpanWritable
    {
        public CorpseLocation() : base(Opcode.SMSG_CORPSE_LOCATION) { }

        public override void Write()
        {
            _worldPacket.WriteBit(Valid);
            _worldPacket.FlushBits();

            _worldPacket.WritePackedGuid128(Player);
            _worldPacket.WriteInt32(ActualMapID);
            _worldPacket.WriteVector3(Position);
            _worldPacket.WriteInt32(MapID);
            _worldPacket.WritePackedGuid128(Transport);
        }

        public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 4 + 12 + 4; // bit + 2 GUIDs + int + Vector3 + int

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteBit(Valid);
            writer.FlushBits();
            writer.WritePackedGuid128(Player.Low, Player.High);
            writer.WriteInt32(ActualMapID);
            writer.WriteVector3(Position);
            writer.WriteInt32(MapID);
            writer.WritePackedGuid128(Transport.Low, Transport.High);
            return writer.Position;
        }

        public WowGuid128 Player;
        public WowGuid128 Transport;
        public Vector3 Position;
        public int ActualMapID;
        public int MapID;
        public bool Valid;
    }

    public class DeathReleaseLoc : ServerPacket, ISpanWritable
    {
        public DeathReleaseLoc() : base(Opcode.SMSG_DEATH_RELEASE_LOC) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(MapID);
            _worldPacket.WriteVector3(Location);
        }

        public int MaxSize => 16; // int + Vector3 (3 floats)

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(MapID);
            writer.WriteVector3(Location);
            return writer.Position;
        }

        public int MapID;
        public Vector3 Location;
    }

    public class ReclaimCorpse : ClientPacket
    {
        public ReclaimCorpse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            CorpseGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 CorpseGUID;
    }

    public class StandStateChange : ClientPacket
    {
        public StandStateChange(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            StandState = _worldPacket.ReadUInt32();
        }

        public uint StandState;
    }

    public class StandStateUpdate : ServerPacket, ISpanWritable
    {
        public StandStateUpdate() : base(Opcode.SMSG_STAND_STATE_UPDATE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(AnimKitID);
            _worldPacket.WriteUInt8(StandState);
        }

        public int MaxSize => 5; // uint + byte

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(AnimKitID);
            writer.WriteUInt8(StandState);
            return writer.Position;
        }

        public uint AnimKitID;
        public byte StandState;
    }

    public class ExplorationExperience : ServerPacket, ISpanWritable
    {
        public ExplorationExperience() : base(Opcode.SMSG_EXPLORATION_EXPERIENCE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(AreaID);
            _worldPacket.WriteUInt32(Experience);
        }

        public int MaxSize => 8; // 2 uints

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(AreaID);
            writer.WriteUInt32(Experience);
            return writer.Position;
        }

        public uint AreaID;
        public uint Experience;
    }

    public class PlayMusic : ServerPacket, ISpanWritable
    {
        public PlayMusic() : base(Opcode.SMSG_PLAY_MUSIC) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(SoundEntryID);
        }

        public int MaxSize => 4; // uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(SoundEntryID);
            return writer.Position;
        }

        public uint SoundEntryID;
    }

    class PlaySound : ServerPacket, ISpanWritable
    {
        public PlaySound() : base(Opcode.SMSG_PLAY_SOUND) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(SoundEntryID);
            _worldPacket.WritePackedGuid128(SourceObjectGuid);
            if (ModernVersion.AddedInVersion(9, 0, 1, 1, 14, 0, 2, 5, 1))
                _worldPacket.WriteInt32(BroadcastTextId);
        }

        // MaxSize: uint + GUID + optional int = 4 + 18 + 4 = 26
        public int MaxSize => 4 + PackedGuidHelper.MaxPackedGuid128Size + 4;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(SoundEntryID);
            writer.WritePackedGuid128(SourceObjectGuid.Low, SourceObjectGuid.High);
            if (ModernVersion.AddedInVersion(9, 0, 1, 1, 14, 0, 2, 5, 1))
                writer.WriteInt32(BroadcastTextId);
            return writer.Position;
        }

        public uint SoundEntryID;
        public WowGuid128 SourceObjectGuid;
        public int BroadcastTextId;
    }

    class PlayObjectSound : ServerPacket, ISpanWritable
    {
        public PlayObjectSound() : base(Opcode.SMSG_PLAY_OBJECT_SOUND) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(SoundEntryID);
            _worldPacket.WritePackedGuid128(SourceObjectGUID);
            _worldPacket.WritePackedGuid128(TargetObjectGUID);
            _worldPacket.WriteVector3(Position);
            if (ModernVersion.AddedInVersion(9, 0, 1, 1, 14, 0, 2, 5, 1))
                _worldPacket.WriteInt32(BroadcastTextID);
        }

        // MaxSize: uint + 2 GUIDs + Vector3 + optional int = 4 + 18*2 + 12 + 4 = 56
        public int MaxSize => 4 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 12 + 4;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(SoundEntryID);
            writer.WritePackedGuid128(SourceObjectGUID.Low, SourceObjectGUID.High);
            writer.WritePackedGuid128(TargetObjectGUID.Low, TargetObjectGUID.High);
            writer.WriteVector3(Position);
            if (ModernVersion.AddedInVersion(9, 0, 1, 1, 14, 0, 2, 5, 1))
                writer.WriteInt32(BroadcastTextID);
            return writer.Position;
        }

        public uint SoundEntryID;
        public WowGuid128 SourceObjectGUID;
        public WowGuid128 TargetObjectGUID;
        public Vector3 Position = new();
        public int BroadcastTextID;
    }

    public class TriggerCinematic : ServerPacket, ISpanWritable
    {
        public TriggerCinematic() : base(Opcode.SMSG_TRIGGER_CINEMATIC) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(CinematicID);
        }

        public int MaxSize => 4; // uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(CinematicID);
            return writer.Position;
        }

        public uint CinematicID;
    }

    class ClientCinematicPkt : ClientPacket
    {
        public ClientCinematicPkt(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    class FarSight : ClientPacket
    {
        public FarSight(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Enable = _worldPacket.HasBit();
        }

        public bool Enable;
    }

    class MountSpecial : ClientPacket
    {
        public MountSpecial(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            SpellVisualKitIDs = new int[_worldPacket.ReadUInt32()];
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                SequenceVariation = _worldPacket.ReadInt32();
            for (var i = 0; i < SpellVisualKitIDs.Length; ++i)
                SpellVisualKitIDs[i] = _worldPacket.ReadInt32();
        }

        public int[] SpellVisualKitIDs;
        public int SequenceVariation;
    }

    class SpecialMountAnim : ServerPacket, ISpanWritable
    {
        public SpecialMountAnim() : base(Opcode.SMSG_SPECIAL_MOUNT_ANIM, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(UnitGUID);
            if (ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
            {
                _worldPacket.WriteInt32(SpellVisualKitIDs.Count);
                if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                    _worldPacket.WriteInt32(SequenceVariation);
                foreach (var id in SpellVisualKitIDs)
                    _worldPacket.WriteInt32(id);
            }
        }

        // Cap for spell visual kit IDs - rarely more than 1-2
        private const int MaxSpellVisualKitIDs = 4;
        // GUID(18) + count(4) + SequenceVariation(4) + IDs(4 each)
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + 4 + MaxSpellVisualKitIDs * 4;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (SpellVisualKitIDs.Count > MaxSpellVisualKitIDs)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(UnitGUID.Low, UnitGUID.High);
            if (ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
            {
                writer.WriteInt32(SpellVisualKitIDs.Count);
                if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                    writer.WriteInt32(SequenceVariation);
                foreach (var id in SpellVisualKitIDs)
                    writer.WriteInt32(id);
            }
            return writer.Position;
        }

        public WowGuid128 UnitGUID;
        public List<int> SpellVisualKitIDs = new();
        public int SequenceVariation;
    }

    public class StartMirrorTimer : ServerPacket, ISpanWritable
    {
        public StartMirrorTimer() : base(Opcode.SMSG_START_MIRROR_TIMER) { }

        public override void Write()
        {
            _worldPacket.WriteInt32((int)Timer);
            _worldPacket.WriteInt32(Value);
            _worldPacket.WriteInt32(MaxValue);
            _worldPacket.WriteInt32(Scale);
            _worldPacket.WriteInt32(SpellID);
            _worldPacket.WriteBit(Paused);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 21; // 5 ints + 1 byte for bit

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32((int)Timer);
            writer.WriteInt32(Value);
            writer.WriteInt32(MaxValue);
            writer.WriteInt32(Scale);
            writer.WriteInt32(SpellID);
            writer.WriteBit(Paused);
            writer.FlushBits();
            return writer.Position;
        }

        public MirrorTimerType Timer;
        public int Value;
        public int MaxValue;
        public int Scale;
        public int SpellID;
        public bool Paused;
    }

    public class PauseMirrorTimer : ServerPacket, ISpanWritable
    {
        public PauseMirrorTimer() : base(Opcode.SMSG_PAUSE_MIRROR_TIMER) { }

        public override void Write()
        {
            _worldPacket.WriteInt32((int)Timer);
            _worldPacket.WriteBit(Paused);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 5; // int + 1 byte for bit

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32((int)Timer);
            writer.WriteBit(Paused);
            writer.FlushBits();
            return writer.Position;
        }

        public MirrorTimerType Timer;
        public bool Paused;
    }

    public class StopMirrorTimer : ServerPacket, ISpanWritable
    {
        public StopMirrorTimer() : base(Opcode.SMSG_STOP_MIRROR_TIMER) { }

        public override void Write()
        {
            _worldPacket.WriteInt32((int)Timer);
        }

        public int MaxSize => 4; // int

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32((int)Timer);
            return writer.Position;
        }

        public MirrorTimerType Timer;
    }

    public class LFGListUpdateBlacklist : ServerPacket, ISpanWritable
    {
        public LFGListUpdateBlacklist() : base(Opcode.SMSG_LFG_LIST_UPDATE_BLACKLIST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Blacklist.Count);
            foreach (var entry in Blacklist)
                entry.Write(_worldPacket);
        }

        // Cap for blacklist entries - reduced from 128 to 16 based on typical usage (0 observed)
        private const int MaxBlacklistEntries = 16;
        // Per entry: 2 ints = 8 bytes
        public int MaxSize => 4 + MaxBlacklistEntries * 8;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Blacklist.Count > MaxBlacklistEntries)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(Blacklist.Count);
            foreach (var entry in Blacklist)
            {
                writer.WriteInt32(entry.ActivityID);
                writer.WriteInt32(entry.Reason);
            }
            return writer.Position;
        }

        public void AddBlacklist(int activity, int reason)
        {
            LFGListBlacklistEntry entry = new LFGListBlacklistEntry();
            entry.ActivityID = activity;
            entry.Reason = reason;
            Blacklist.Add(entry);
        }

        public List<LFGListBlacklistEntry> Blacklist = new List<LFGListBlacklistEntry>();
    }

    public struct LFGListBlacklistEntry
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(ActivityID);
            data.WriteInt32(Reason);
        }

        public int ActivityID;
        public int Reason;
    }

    public class ConquestFormulaConstants : ServerPacket, ISpanWritable
    {
        public ConquestFormulaConstants() : base(Opcode.SMSG_CONQUEST_FORMULA_CONSTANTS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(PvpMinCPPerWeek);
            _worldPacket.WriteInt32(PvpMaxCPPerWeek);
            _worldPacket.WriteFloat(PvpCPBaseCoefficient);
            _worldPacket.WriteFloat(PvpCPExpCoefficient);
            _worldPacket.WriteFloat(PvpCPNumerator);
        }

        public int MaxSize => 20; // 2 ints + 3 floats

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(PvpMinCPPerWeek);
            writer.WriteInt32(PvpMaxCPPerWeek);
            writer.WriteFloat(PvpCPBaseCoefficient);
            writer.WriteFloat(PvpCPExpCoefficient);
            writer.WriteFloat(PvpCPNumerator);
            return writer.Position;
        }

        public int PvpMinCPPerWeek;
        public int PvpMaxCPPerWeek;
        public float PvpCPBaseCoefficient;
        public float PvpCPExpCoefficient;
        public float PvpCPNumerator;
    }

    public class SeasonInfo : ServerPacket, ISpanWritable
    {
        public SeasonInfo() : base(Opcode.SMSG_SEASON_INFO) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(MythicPlusSeasonID);
            _worldPacket.WriteInt32(CurrentSeason);
            _worldPacket.WriteInt32(PreviousSeason);
            _worldPacket.WriteInt32(ConquestWeeklyProgressCurrencyID);
            _worldPacket.WriteInt32(PvpSeasonID);
            _worldPacket.WriteBit(WeeklyRewardChestsEnabled);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 21; // 5 ints + 1 byte for bit

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(MythicPlusSeasonID);
            writer.WriteInt32(CurrentSeason);
            writer.WriteInt32(PreviousSeason);
            writer.WriteInt32(ConquestWeeklyProgressCurrencyID);
            writer.WriteInt32(PvpSeasonID);
            writer.WriteBit(WeeklyRewardChestsEnabled);
            writer.FlushBits();
            return writer.Position;
        }

        public int MythicPlusSeasonID;
        public int PreviousSeason;
        public int CurrentSeason;
        public int PvpSeasonID;
        public int ConquestWeeklyProgressCurrencyID;
        public bool WeeklyRewardChestsEnabled;
    }

    public class InvalidatePlayer : ServerPacket, ISpanWritable
    {
        public InvalidatePlayer() : base(Opcode.SMSG_INVALIDATE_PLAYER) { }

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

    public class ZoneUnderAttack : ServerPacket, ISpanWritable
    {
        public ZoneUnderAttack() : base(Opcode.SMSG_ZONE_UNDER_ATTACK) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(AreaID);
        }

        public int MaxSize => 4; // int

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(AreaID);
            return writer.Position;
        }

        public int AreaID;
    }
}
