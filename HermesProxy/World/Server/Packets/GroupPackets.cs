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
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HermesProxy.World.Server.Packets;

public readonly record struct PartyInviteClient(
    byte PartyIndex,
    uint VirtualRealmAddress,
    WowGuid128 TargetGUID,
    string TargetName,
    string TargetRealm);

class PartyCommandResult : ServerPacket, ISpanWritable
{
    public PartyCommandResult() : base(Opcode.SMSG_PARTY_COMMAND_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteBits(Name.GetByteCount(), 9);
        _worldPacket.WriteBits(Command, 4);
        _worldPacket.WriteBits(Result, 6);

        _worldPacket.WriteUInt32(ResultData);
        _worldPacket.WritePackedGuid128(ResultGUID);
        _worldPacket.WriteString(Name);
    }

    // MaxSize: bits (9+4+6=19 -> 3) + uint (4) + GUID (18) + name (24) = 49
    public int MaxSize => 3 + 4 + PackedGuidHelper.MaxPackedGuid128Size + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 9);
        writer.WriteBits(Command, 4);
        writer.WriteBits(Result, 6);

        writer.WriteUInt32(ResultData);
        writer.WritePackedGuid128(ResultGUID.Low, ResultGUID.High);
        writer.WriteString(Name);
        return writer.Position;
    }

    public string Name = string.Empty;
    public byte Command;
    public byte Result;
    public uint ResultData;
    public WowGuid128 ResultGUID = WowGuid128.Empty;
}

class GroupDecline : ServerPacket, ISpanWritable
{
    public GroupDecline() : base(Opcode.SMSG_GROUP_DECLINE) { }

    public override void Write()
    {
        _worldPacket.WriteBits(Name.GetByteCount(), 9);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(Name);
    }

    // MaxSize: 9 bits (2 bytes) + max player name bytes
    public int MaxSize => 2 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 9);
        writer.FlushBits();
        writer.WriteString(Name);
        return writer.Position;
    }

    public string Name = string.Empty;
}

class PartyInvite : ServerPacket
{
    public PartyInvite() : base(Opcode.SMSG_PARTY_INVITE) { }

    public override void Write()
    {
        _worldPacket.WriteBit(CanAccept);
        _worldPacket.WriteBit(MightCRZYou);
        _worldPacket.WriteBit(IsXRealm);
        _worldPacket.WriteBit(MustBeBNetFriend);
        _worldPacket.WriteBit(AllowMultipleRoles);
        _worldPacket.WriteBit(QuestSessionActive);
        _worldPacket.WriteBits(InviterName.GetByteCount(), 6);

        InviterRealm.Write(_worldPacket);

        _worldPacket.WritePackedGuid128(InviterGUID);
        _worldPacket.WritePackedGuid128(InviterBNetAccountId);
        _worldPacket.WriteUInt16(Unk1);

        // WotLK Classic narrowed ProposedRoles to a single byte. V1_14 and V2_5 still read the
        // full uint32 -- WowPacketParser reads it as a byte in its V3_4_0/V4_4_0 modules and as
        // an int32 in every earlier one, and CypherCore ClassicWOTLK writes a byte. Sending four
        // bytes to a 3.4.3 client shifts everything after it, and since InviterName is declared
        // by the 6-bit length prefix above but written last, the client reads the name from the
        // wrong offset and renders it empty. Issue #199.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            _worldPacket.WriteUInt8((byte)ProposedRoles);
        else
            _worldPacket.WriteUInt32(ProposedRoles);

        _worldPacket.WriteInt32(LfgSlots.Count);
        _worldPacket.WriteInt32(LfgCompletedMask);

        _worldPacket.WriteString(InviterName);

        foreach (int LfgSlot in LfgSlots)
            _worldPacket.WriteInt32(LfgSlot);
    }

    public bool CanAccept = true;
    public bool MightCRZYou;
    public bool IsXRealm;
    public bool MustBeBNetFriend;
    public bool AllowMultipleRoles;
    public bool QuestSessionActive;
    public ushort Unk1 = 4904;

    // Inviter
    public VirtualRealmInfo InviterRealm;
    public WowGuid128 InviterGUID;
    public WowGuid128 InviterBNetAccountId;
    public string InviterName = string.Empty;

    // Lfg
    public uint ProposedRoles;
    public int LfgCompletedMask;
    public List<int> LfgSlots = new();
}

public readonly record struct PartyInviteResponse(byte PartyIndex, bool Accept, uint? RolesDesired);

public class PartyUpdate : ServerPacket
{
    public PartyUpdate() : base(Opcode.SMSG_PARTY_UPDATE) { }

    public override void Write()
    {
        _worldPacket.WriteUInt16((ushort)PartyFlags);
        _worldPacket.WriteUInt8(PartyIndex);
        _worldPacket.WriteUInt8((byte)PartyType);
        _worldPacket.WriteInt32(MyIndex);
        _worldPacket.WritePackedGuid128(PartyGUID);
        _worldPacket.WriteInt32(SequenceNum);
        _worldPacket.WritePackedGuid128(LeaderGUID);
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            _worldPacket.WriteUInt8(LeaderFactionGroup);
        _worldPacket.WriteInt32(PlayerList.Count);
        _worldPacket.WriteBit(LfgInfos != null);
        _worldPacket.WriteBit(LootSettings != null);
        _worldPacket.WriteBit(DifficultySettings != null);
        _worldPacket.FlushBits();

        foreach (var playerInfo in PlayerList)
            playerInfo.Write(_worldPacket);

        if (LootSettings != null)
            LootSettings.Write(_worldPacket);

        if (DifficultySettings != null)
            DifficultySettings.Write(_worldPacket);

        if (LfgInfos != null)
            LfgInfos.Write(_worldPacket);
    }

    public GroupFlags PartyFlags;
    public byte PartyIndex;
    public GroupType PartyType;

    public WowGuid128 PartyGUID;
    public WowGuid128 LeaderGUID;
    public byte LeaderFactionGroup;

    public int MyIndex;
    public int SequenceNum;

    public List<PartyPlayerInfo> PlayerList = new();

    public PartyLFGInfo LfgInfos = null!;
    public PartyLootSettings LootSettings = null!;
    public PartyDifficultySettings DifficultySettings = null!;

    // SendPacket writes then disposes the buffer. Clone before a second emit.
    public PartyUpdate CloneUnwritten()
    {
        return new PartyUpdate
        {
            PartyFlags = PartyFlags,
            PartyIndex = PartyIndex,
            PartyType = PartyType,
            PartyGUID = PartyGUID,
            LeaderGUID = LeaderGUID,
            LeaderFactionGroup = LeaderFactionGroup,
            MyIndex = MyIndex,
            SequenceNum = SequenceNum,
            PlayerList = new List<PartyPlayerInfo>(PlayerList),
            LfgInfos = LfgInfos,
            LootSettings = LootSettings,
            DifficultySettings = DifficultySettings,
        };
    }
}

public struct PartyPlayerInfo
{
    public void Write(WorldPacket data)
    {
        data.WriteBits(Name.GetByteCount(), 6);
        data.WriteBits(VoiceStateID.GetByteCount() + 1, 6);

        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            bool isConnected = Connected || Status != GroupMemberOnlineStatus.Offline;
            data.WriteBit(isConnected);
            data.WriteBit(VoiceChatSilenced);
            data.WriteBit(FromSocialQueue);
        }
        else
        {
            data.WriteBit(FromSocialQueue);
            data.WriteBit(VoiceChatSilenced);
        }

        data.WritePackedGuid128(GUID);

        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            data.WriteUInt8((byte)Status);

        data.WriteUInt8(Subgroup);
        data.WriteUInt8((byte)Flags);
        data.WriteUInt8(RolesAssigned);
        data.WriteUInt8((byte)ClassId);

        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            data.WriteUInt8(FactionGroup);

        data.WriteString(Name);
        if (!VoiceStateID.IsEmpty())
            data.WriteString(VoiceStateID);
    }

    public WowGuid128 GUID;
    public string Name;
    public string VoiceStateID;   // same as bgs.protocol.club.v1.MemberVoiceState.id
    public Class ClassId;
    public GroupMemberOnlineStatus Status;
    public byte Subgroup;
    public GroupMemberFlags Flags;
    public byte RolesAssigned;
    public byte FactionGroup;
    public bool FromSocialQueue;
    public bool VoiceChatSilenced;
    public bool Connected;
}

public class PartyLFGInfo
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8(MyFlags);
        data.WriteUInt32(Slot);
        data.WriteUInt32(MyRandomSlot);
        data.WriteUInt8(MyPartialClear);
        data.WriteFloat(MyGearDiff);
        data.WriteUInt8(MyStrangerCount);
        data.WriteUInt8(MyKickVoteCount);
        data.WriteUInt8(BootCount);
        data.WriteBit(Aborted);
        data.WriteBit(MyFirstReward);
        data.FlushBits();
    }

    public byte MyFlags;
    public uint Slot;
    public byte BootCount;
    public uint MyRandomSlot;
    public bool Aborted;
    public byte MyPartialClear;
    public float MyGearDiff;
    public byte MyStrangerCount;
    public byte MyKickVoteCount;
    public bool MyFirstReward;
}

public class PartyLootSettings
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8((byte)Method);
        data.WritePackedGuid128(LootMaster);
        data.WriteUInt8(Threshold);
    }

    public LootMethod Method;
    public WowGuid128 LootMaster;
    public byte Threshold;
}

public class PartyDifficultySettings
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32((uint)DungeonDifficultyID);
        data.WriteUInt32((uint)RaidDifficultyID);
        data.WriteUInt32((uint)LegacyRaidDifficultyID);
    }

    public DifficultyModern DungeonDifficultyID;
    public DifficultyModern RaidDifficultyID;
    public DifficultyModern LegacyRaidDifficultyID;
}

public readonly record struct LeaveGroup(sbyte PartyIndex);

public readonly record struct PartyUninvite(byte PartyIndex, WowGuid128 TargetGUID, string Reason);

class GroupUninvite : ServerPacket, ISpanWritable
{
    public GroupUninvite() : base(Opcode.SMSG_GROUP_UNINVITE) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

class GroupDestroyed : ServerPacket, ISpanWritable
{
    public GroupDestroyed() : base(Opcode.SMSG_GROUP_DESTROYED) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

public readonly record struct SetAssistantLeader(byte PartyIndex, WowGuid128 TargetGUID, bool Apply);

public readonly record struct SetEveryoneIsAssistant(byte PartyIndex, bool Apply);

public readonly record struct SetPartyLeader(sbyte PartyIndex, WowGuid128 TargetGUID);

public readonly record struct SetRole(byte PartyIndex, WowGuid128 ChangedUnit, byte Role);

public class RoleChangedInform : ServerPacket, ISpanWritable
{
    public RoleChangedInform() : base(Opcode.SMSG_ROLE_CHANGED_INFORM) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(PartyIndex);
        _worldPacket.WritePackedGuid128(From);
        _worldPacket.WritePackedGuid128(ChangedUnit);
        _worldPacket.WriteUInt8(OldRole);
        _worldPacket.WriteUInt8(NewRole);
    }

    public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 2;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8(PartyIndex);
        writer.WritePackedGuid128(From.Low, From.High);
        writer.WritePackedGuid128(ChangedUnit.Low, ChangedUnit.High);
        writer.WriteUInt8(OldRole);
        writer.WriteUInt8(NewRole);
        return writer.Position;
    }

    public byte PartyIndex;
    public WowGuid128 From;
    public WowGuid128 ChangedUnit;
    public byte OldRole;
    public byte NewRole;
}

class GroupNewLeader : ServerPacket, ISpanWritable
{
    public GroupNewLeader() : base(Opcode.SMSG_GROUP_NEW_LEADER) { }

    public override void Write()
    {
        _worldPacket.WriteInt8(PartyIndex);
        _worldPacket.WriteBits(Name.GetByteCount(), 9);
        _worldPacket.WriteString(Name);
    }

    // MaxSize: int8 (1) + 9 bits (2 bytes) + max player name bytes
    public int MaxSize => 1 + 2 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt8(PartyIndex);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 9);
        writer.WriteString(Name);
        return writer.Position;
    }

    public sbyte PartyIndex;
    public string Name = string.Empty;
}

public readonly record struct ConvertRaid(bool Raid);

public readonly record struct DoReadyCheck(sbyte PartyIndex);

class ReadyCheckStarted : ServerPacket, ISpanWritable
{
    public ReadyCheckStarted() : base(Opcode.SMSG_READY_CHECK_STARTED) { }

    public override void Write()
    {
        _worldPacket.WriteInt8(PartyIndex);
        _worldPacket.WritePackedGuid128(PartyGUID);
        _worldPacket.WritePackedGuid128(InitiatorGUID);
        _worldPacket.WriteUInt64(Duration);
    }

    public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 8; // sbyte + 2 GUIDs + ulong

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt8((sbyte)PartyIndex);
        writer.WritePackedGuid128(PartyGUID.Low, PartyGUID.High);
        writer.WritePackedGuid128(InitiatorGUID.Low, InitiatorGUID.High);
        writer.WriteUInt64(Duration);
        return writer.Position;
    }

    public sbyte PartyIndex;
    public WowGuid128 PartyGUID;
    public WowGuid128 InitiatorGUID;
    public ulong Duration = 35000;
}

public readonly record struct ReadyCheckResponseClient(byte PartyIndex, bool IsReady);

class ReadyCheckResponse : ServerPacket, ISpanWritable
{
    public ReadyCheckResponse() : base(Opcode.SMSG_READY_CHECK_RESPONSE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(PartyGUID);
        _worldPacket.WritePackedGuid128(Player);

        _worldPacket.WriteBit(IsReady);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 1; // 2 GUIDs + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(PartyGUID.Low, PartyGUID.High);
        writer.WritePackedGuid128(Player.Low, Player.High);
        writer.WriteBit(IsReady);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 PartyGUID;
    public WowGuid128 Player;
    public bool IsReady;
}

class ReadyCheckCompleted : ServerPacket, ISpanWritable
{
    public ReadyCheckCompleted() : base(Opcode.SMSG_READY_CHECK_COMPLETED) { }

    public override void Write()
    {
        _worldPacket.WriteInt8(PartyIndex);
        _worldPacket.WritePackedGuid128(PartyGUID);
    }

    public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size; // sbyte + GUID

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt8((sbyte)PartyIndex);
        writer.WritePackedGuid128(PartyGUID.Low, PartyGUID.High);
        return writer.Position;
    }

    public sbyte PartyIndex;
    public WowGuid128 PartyGUID;
}

public readonly record struct UpdateRaidTarget(sbyte PartyIndex, WowGuid128 Target, sbyte Symbol);

class SendRaidTargetUpdateSingle : ServerPacket, ISpanWritable
{
    public SendRaidTargetUpdateSingle() : base(Opcode.SMSG_SEND_RAID_TARGET_UPDATE_SINGLE) { }

    public override void Write()
    {
        _worldPacket.WriteInt8(PartyIndex);
        _worldPacket.WriteInt8(Symbol);
        _worldPacket.WritePackedGuid128(Target);
        _worldPacket.WritePackedGuid128(ChangedBy);
    }

    public int MaxSize => 2 + PackedGuidHelper.MaxPackedGuid128Size * 2; // 2 sbytes + 2 GUIDs

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt8((sbyte)PartyIndex);
        writer.WriteInt8((sbyte)Symbol);
        writer.WritePackedGuid128(Target.Low, Target.High);
        writer.WritePackedGuid128(ChangedBy.Low, ChangedBy.High);
        return writer.Position;
    }

    public sbyte PartyIndex;
    public sbyte Symbol;
    public WowGuid128 Target;
    public WowGuid128 ChangedBy;
}

class SendRaidTargetUpdateAll : ServerPacket, ISpanWritable
{
    // WoW has exactly 8 raid target markers (skull, cross, square, moon, triangle, diamond, circle, star)
    private const int MaxRaidTargets = 8;

    public SendRaidTargetUpdateAll() : base(Opcode.SMSG_SEND_RAID_TARGET_UPDATE_ALL) { }

    public override void Write()
    {
        _worldPacket.WriteInt8(PartyIndex);
        _worldPacket.WriteInt32(TargetIcons.Count);

        foreach (var pair in TargetIcons)
        {
            _worldPacket.WritePackedGuid128(pair.Item2);
            _worldPacket.WriteInt8(pair.Item1);
        }
    }

    // MaxSize: sbyte (1) + int (4) + 8 * (PackedGuid128 (18) + sbyte (1)) = 157
    public int MaxSize => 1 + 4 + MaxRaidTargets * (PackedGuidHelper.MaxPackedGuid128Size + 1);

    public int WriteToSpan(Span<byte> buffer)
    {
        // Hard cap - should never exceed 8
        if (TargetIcons.Count > MaxRaidTargets)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt8(PartyIndex);
        writer.WriteInt32(TargetIcons.Count);

        foreach (var pair in TargetIcons)
        {
            writer.WritePackedGuid128(pair.Item2.Low, pair.Item2.High);
            writer.WriteInt8(pair.Item1);
        }

        return writer.Position;
    }

    public sbyte PartyIndex;
    public List<Tuple<sbyte, WowGuid128>> TargetIcons = new();
}

class SummonRequest : ServerPacket, ISpanWritable
{
    public SummonRequest() : base(Opcode.SMSG_SUMMON_REQUEST, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(SummonerGUID);
        _worldPacket.WriteUInt32(SummonerVirtualRealmAddress);
        _worldPacket.WriteInt32(AreaID);
        _worldPacket.WriteUInt8((byte)Reason);
        _worldPacket.WriteBit(SkipStartingArea);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 10; // GUID + uint + int + byte + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(SummonerGUID.Low, SummonerGUID.High);
        writer.WriteUInt32(SummonerVirtualRealmAddress);
        writer.WriteInt32(AreaID);
        writer.WriteUInt8((byte)Reason);
        writer.WriteBit(SkipStartingArea);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 SummonerGUID;
    public uint SummonerVirtualRealmAddress;
    public int AreaID;
    public SummonReason Reason;
    public bool SkipStartingArea;

    public enum SummonReason
    {
        Spell = 0,
        Scenario = 1
    }
}

public readonly record struct SummonResponse(WowGuid128 SummonerGUID, bool Accept);

public readonly record struct RequestPartyMemberStats(byte PartyIndex, WowGuid128 TargetGUID);

class PartyMemberPartialState : ServerPacket
{
    public PartyMemberPartialState() : base(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE) { }

    public override void Write()
    {
        _worldPacket.WriteBit(ForEnemyChanged);
        _worldPacket.WriteBit(SetPvPInactive); // adds GroupMemberStatusFlag 0x0020 if true, removes 0x0020 if false
        _worldPacket.WriteBit(Unk901_1);

        _worldPacket.WriteBit(PartyType != null);
        _worldPacket.WriteBit(StatusFlags.HasValue);
        _worldPacket.WriteBit(PowerType.HasValue);
        _worldPacket.WriteBit(OverrideDisplayPower.HasValue);
        _worldPacket.WriteBit(CurrentHealth.HasValue);
        _worldPacket.WriteBit(MaxHealth.HasValue);
        _worldPacket.WriteBit(CurrentPower.HasValue);
        _worldPacket.WriteBit(MaxPower.HasValue);
        _worldPacket.WriteBit(Level.HasValue);
        _worldPacket.WriteBit(Spec.HasValue);
        _worldPacket.WriteBit(ZoneID.HasValue);
        _worldPacket.WriteBit(WmoGroupID.HasValue);
        _worldPacket.WriteBit(WmoDoodadPlacementID.HasValue);
        _worldPacket.WriteBit(Position.HasValue);
        _worldPacket.WriteBit(VehicleSeatRecID.HasValue);
        _worldPacket.WriteBit(Auras is { Count: > 0 });
        _worldPacket.WriteBit(Pet != null);
        _worldPacket.WriteBit(Phase != null);
        _worldPacket.WriteBit(Unk901_2 != null);
        _worldPacket.FlushBits();

        if (Pet != null)
            Pet.WritePartial(_worldPacket);

        _worldPacket.WritePackedGuid128(AffectedGUID);
        if (PartyType != null)
        {
            _worldPacket.WriteUInt8(PartyType.PartyType1);
            _worldPacket.WriteUInt8(PartyType.PartyType2);
        }

        if (StatusFlags.HasValue)
            _worldPacket.WriteUInt16(StatusFlags.Value);
        if (PowerType.HasValue)
            _worldPacket.WriteUInt8(PowerType.Value);
        if (OverrideDisplayPower.HasValue)
            _worldPacket.WriteUInt16(OverrideDisplayPower.Value);
        if (CurrentHealth.HasValue)
            _worldPacket.WriteUInt32(CurrentHealth.Value);
        if (MaxHealth.HasValue)
            _worldPacket.WriteUInt32(MaxHealth.Value);
        if (CurrentPower.HasValue)
            _worldPacket.WriteUInt16(CurrentPower.Value);
        if (MaxPower.HasValue)
            _worldPacket.WriteUInt16(MaxPower.Value);
        if (Level.HasValue)
            _worldPacket.WriteUInt16(Level.Value);
        if (Spec.HasValue)
            _worldPacket.WriteUInt16(Spec.Value);
        if (ZoneID.HasValue)
            _worldPacket.WriteUInt16(ZoneID.Value);
        if (WmoGroupID.HasValue)
            _worldPacket.WriteUInt16(WmoGroupID.Value);
        if (WmoDoodadPlacementID.HasValue)
            _worldPacket.WriteUInt32(WmoDoodadPlacementID.Value);
        if (Position.HasValue)
        {
            _worldPacket.WriteInt16(Position.Value.X);
            _worldPacket.WriteInt16(Position.Value.Y);
            _worldPacket.WriteInt16(Position.Value.Z);
        }
        if (VehicleSeatRecID.HasValue)
            _worldPacket.WriteUInt32(VehicleSeatRecID.Value);
        if (Auras is { Count: > 0 })
        {
            _worldPacket.WriteInt32(Auras.Count);
            foreach (var aura in Auras)
                aura.Write(_worldPacket);
        }
        if (Phase != null)
            Phase.Write(_worldPacket);

        if (Unk901_2 != null)
            Unk901_2.Write(_worldPacket);
    }

    public WowGuid128 AffectedGUID;
    public bool ForEnemyChanged;
    public bool SetPvPInactive;
    public bool Unk901_1;
    public PartyTypeChange PartyType = null!;
    public ushort? StatusFlags;
    public byte? PowerType;
    public ushort? OverrideDisplayPower;
    public uint? CurrentHealth;
    public uint? MaxHealth;
    public ushort? CurrentPower;
    public ushort? MaxPower;
    public ushort? Level;
    public ushort? Spec;
    public ushort? ZoneID;
    public ushort? WmoGroupID;
    public uint? WmoDoodadPlacementID;
    public Vector3_UInt16? Position;
    public uint? VehicleSeatRecID;
    // Left null until a packet actually carries auras. The parse paths already guard with
    // `??= new`, so the old `= new()` initializer only ever allocated a list that stayed
    // empty on the ~95% of updates that carry position/health alone.
    public List<PartyMemberAuraStates>? Auras;
    public PartyMemberPetStats Pet = null!;
    public PartyMemberPhaseStates Phase = null!;
    public UnkStruct901_2 Unk901_2 = null!;

    public class PartyTypeChange
    {
        public byte PartyType1;
        public byte PartyType2;
    }
    /// Struct, not a class: one of these is built for nearly every partial state (position is
    /// the flag AzerothCore sets on every player tick), so a heap object here was a per-packet
    /// allocation on the hottest path in the proxy. Held as Vector3_UInt16? — a nullable value
    /// type, so the "is present" gate costs no allocation either.
    public struct Vector3_UInt16
    {
        public short X;
        public short Y;
        public short Z;
    }
    public class UnkStruct901_2
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Unk902_3);
            data.WriteUInt32(Unk902_4);
            data.WriteUInt32(Unk902_5);
        }
        public uint Unk902_3;
        public uint Unk902_4;
        public uint Unk902_5;
    }
}

class PartyMemberFullState : ServerPacket
{
    public PartyMemberFullState() : base(Opcode.SMSG_PARTY_MEMBER_FULL_STATE)
    {
        Phases.PhaseShiftFlags = 8;
    }

    public override void Write()
    {
        _worldPacket.WriteBit(ForEnemy);
        _worldPacket.FlushBits();

        for (byte i = 0; i < 2; i++)
            _worldPacket.WriteInt8(PartyType[i]);

        _worldPacket.WriteInt16((short)StatusFlags);
        _worldPacket.WriteUInt8(PowerType);
        _worldPacket.WriteInt16((short)PowerDisplayID);
        _worldPacket.WriteInt32(CurrentHealth);
        _worldPacket.WriteInt32(MaxHealth);
        _worldPacket.WriteUInt16(CurrentPower);
        _worldPacket.WriteUInt16(MaxPower);
        _worldPacket.WriteUInt16(Level);
        _worldPacket.WriteUInt16(SpecID);
        _worldPacket.WriteUInt16(ZoneID);
        _worldPacket.WriteUInt16(WmoGroupID);
        _worldPacket.WriteInt32(WmoDoodadPlacementID);
        _worldPacket.WriteInt16(PositionX);
        _worldPacket.WriteInt16(PositionY);
        _worldPacket.WriteInt16(PositionZ);
        _worldPacket.WriteInt32(VehicleSeat);
        _worldPacket.WriteInt32(Auras.Count);

        Phases.Write(_worldPacket);
        ChromieTime.Write(_worldPacket);

        foreach (PartyMemberAuraStates aura in Auras)
            aura.Write(_worldPacket);

        _worldPacket.WriteBit(Pet != null);
        _worldPacket.FlushBits();

        if (Pet != null)
            Pet.WriteFull(_worldPacket);

        _worldPacket.WritePackedGuid128(MemberGuid);
    }

    public bool ForEnemy;
    public ushort Level;
    public GroupMemberOnlineStatus StatusFlags;

    public int CurrentHealth;
    public int MaxHealth;

    public byte PowerType;
    public ushort CurrentPower;
    public ushort MaxPower;

    public ushort ZoneID;
    public short PositionX;
    public short PositionY;
    public short PositionZ;

    public int VehicleSeat;

    public PartyMemberPhaseStates Phases = new();
    public List<PartyMemberAuraStates> Auras = new();
    public PartyMemberPetStats Pet = null!;

    public ushort PowerDisplayID;
    public ushort SpecID;
    public ushort WmoGroupID;
    public int WmoDoodadPlacementID;
    public sbyte[] PartyType = new sbyte[2];
    public CTROptions ChromieTime;
    public WowGuid128 MemberGuid;
}

public struct CTROptions
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(ContentTuningConditionMask);
        data.WriteInt32(Unused901);
        data.WriteUInt32(ExpansionLevelMask);
    }

    public uint ContentTuningConditionMask;
    public int Unused901;
    public uint ExpansionLevelMask;
}

public class PartyMemberPhaseStates
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(PhaseShiftFlags);
        data.WriteInt32(Phases.Count);
        data.WritePackedGuid128(PersonalGUID);
        foreach (var phase in Phases)
            phase.Write(data);
    }

    public uint PhaseShiftFlags;
    public List<PartyMemberPhase> Phases = new List<PartyMemberPhase>();
    public WowGuid128 PersonalGUID = WowGuid128.Empty;

    public struct PartyMemberPhase
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt16(PhaseFlags);
            data.WriteUInt16(Id);
        }
        public ushort PhaseFlags;
        public ushort Id;
    }
}

public class PartyMemberAuraStates
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(SpellId);
        data.WriteUInt16(AuraFlags);
        data.WriteUInt32(ActiveFlags);
        data.WriteInt32(Points.Count);
        foreach (var point in Points)
            data.WriteFloat(point);
    }

    public uint SpellId;
    public ushort AuraFlags;
    public uint ActiveFlags;
    public List<float> Points = new();
}

public class PartyMemberPetStats
{
    public void WritePartial(WorldPacket data)
    {
        data.WriteBit(NewPetGuid != default);
        data.WriteBit(NewPetName != null);
        data.WriteBit(DisplayID.HasValue);
        data.WriteBit(MaxHealth.HasValue);
        data.WriteBit(Health.HasValue);
        data.WriteBit(Auras.Count > 0);
        data.FlushBits();

        if (NewPetName != null)
        {
            data.WriteBits(NewPetName.GetByteCount(), 8);
            data.WriteString(NewPetName);
        }
        if (NewPetGuid != default)
            data.WritePackedGuid128(NewPetGuid);
        if (DisplayID.HasValue)
            data.WriteUInt32(DisplayID.Value);
        if (MaxHealth.HasValue)
            data.WriteUInt32(MaxHealth.Value);
        if (Health.HasValue)
            data.WriteUInt32(Health.Value);
        if (Auras.Count > 0)
        {
            data.WriteInt32(Auras.Count);
            foreach (var aura in Auras)
                aura.Write(data);
        }
    }

    public void WriteFull(WorldPacket data)
    {
        if (NewPetGuid == default)
            NewPetGuid = WowGuid128.Empty;
        if (NewPetName == null)
            NewPetName = "";
        if (DisplayID == null)
            DisplayID = 0;
        if (MaxHealth == null)
            MaxHealth = 0;
        if (Health == null)
            Health = 0;
        if (Auras == null)
            Auras = new List<PartyMemberAuraStates>();

        data.WritePackedGuid128(NewPetGuid);
        data.WriteUInt32(DisplayID.Value);
        data.WriteUInt32(Health.Value);
        data.WriteUInt32(MaxHealth.Value);
        data.WriteInt32(Auras.Count);
        Auras.ForEach(p => p.Write(data));

        data.WriteBits(NewPetName.GetByteCount(), 8);
        data.FlushBits();
        data.WriteString(NewPetName);
    }

    public WowGuid128 NewPetGuid;
    public string NewPetName = string.Empty;
    public uint? DisplayID;
    public uint? MaxHealth;
    public uint? Health;
    public List<PartyMemberAuraStates> Auras = new();
}

public readonly record struct MinimapPingClient(Vector2 Position, sbyte PartyIndex);

class MinimapPing : ServerPacket, ISpanWritable
{
    public MinimapPing() : base(Opcode.SMSG_MINIMAP_PING) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(SenderGUID);
        _worldPacket.WriteVector2(Position);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 8; // GUID + Vector2 (2 floats)

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(SenderGUID.Low, SenderGUID.High);
        writer.WriteVector2(Position);
        return writer.Position;
    }

    public WowGuid128 SenderGUID;
    public Vector2 Position;
}

public readonly record struct RandomRollClient(int Min, int Max, byte PartyIndex);

public class RandomRoll : ServerPacket, ISpanWritable
{
    public RandomRoll() : base(Opcode.SMSG_RANDOM_ROLL) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Roller);
        _worldPacket.WritePackedGuid128(RollerWowAccount);
        _worldPacket.WriteInt32(Min);
        _worldPacket.WriteInt32(Max);
        _worldPacket.WriteInt32(Result);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 12; // 2 GUIDs + 3 ints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Roller.Low, Roller.High);
        writer.WritePackedGuid128(RollerWowAccount.Low, RollerWowAccount.High);
        writer.WriteInt32(Min);
        writer.WriteInt32(Max);
        writer.WriteInt32(Result);
        return writer.Position;
    }

    public WowGuid128 Roller;
    public WowGuid128 RollerWowAccount;
    public int Min;
    public int Max;
    public int Result;
}

public readonly record struct ChangeSubGroup(WowGuid128 TargetGUID, sbyte PartyIndex, byte NewSubGroup);

public readonly record struct SwapSubGroups(sbyte PartyIndex, WowGuid128 FirstTarget, WowGuid128 SecondTarget);
