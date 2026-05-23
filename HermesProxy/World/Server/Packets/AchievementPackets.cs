using Framework.Constants;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

// Modern V3_4_3 (build 54261) achievement packets, wire layout per TrinityCore
// 3.4.3 source: src/server/game/Server/Packets/AchievementPackets.{h,cpp} +
// PacketUtilities.h Duration<int64> / Timestamp<int64>.
// CriteriaProgressPkt is shared with AllAccountCriteria — defined in MiscPackets.cs.

public struct EarnedAchievement
{
    public uint Id;
    public long Date;                  // unix time; WritePackedTime packs to wire UInt32
    public WowGuid128 Owner;
    public uint VirtualRealmAddress;
    public uint NativeRealmAddress;

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(Id);
        data.WritePackedTime(Date);
        data.WritePackedGuid128(Owner);
        data.WriteUInt32(VirtualRealmAddress);
        data.WriteUInt32(NativeRealmAddress);
    }
}

public class AllAchievementData : ServerPacket
{
    public List<EarnedAchievement> Earned = new();
    public List<CriteriaProgressPkt> Progress = new();

    public AllAchievementData() : base(Opcode.SMSG_ALL_ACHIEVEMENT_DATA, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Earned.Count);
        _worldPacket.WriteInt32(Progress.Count);
        foreach (var earned in Earned)
            earned.Write(_worldPacket);
        foreach (var progress in Progress)
            progress.Write(_worldPacket);
    }
}

public class CriteriaUpdatePkt : ServerPacket
{
    public uint CriteriaID;
    public ulong Quantity;
    public WowGuid128 PlayerGUID;
    public uint Flags;
    public long CurrentTime;           // unix -> WritePackedTime
    public long ElapsedTime;           // Duration<Seconds> = Int64
    public long CreationTime;          // Timestamp<int64> = Int64 (fork wrote UInt32; TC source = 8 B)
    public ulong? RafAcceptanceID;

    public CriteriaUpdatePkt() : base(Opcode.SMSG_CRITERIA_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(CriteriaID);
        _worldPacket.WriteUInt64(Quantity);
        _worldPacket.WritePackedGuid128(PlayerGUID);
        _worldPacket.WriteUInt32(0u);  // Unused_10_1_5
        _worldPacket.WriteUInt32(Flags);
        _worldPacket.WritePackedTime(CurrentTime);
        _worldPacket.WriteInt64(ElapsedTime);
        _worldPacket.WriteInt64(CreationTime);
        _worldPacket.WriteBit(RafAcceptanceID.HasValue);
        _worldPacket.FlushBits();
        if (RafAcceptanceID.HasValue)
            _worldPacket.WriteUInt64(RafAcceptanceID.Value);
    }
}

public class AchievementEarnedPkt : ServerPacket
{
    public WowGuid128 Sender;
    public WowGuid128 Earner;
    public uint AchievementID;
    public long Time;                  // unix -> WritePackedTime
    public uint EarnerNativeRealm;
    public uint EarnerVirtualRealm;
    public bool Initial;

    public AchievementEarnedPkt() : base(Opcode.SMSG_ACHIEVEMENT_EARNED, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Sender);
        _worldPacket.WritePackedGuid128(Earner);
        _worldPacket.WriteUInt32(AchievementID);
        _worldPacket.WritePackedTime(Time);
        _worldPacket.WriteUInt32(EarnerNativeRealm);
        _worldPacket.WriteUInt32(EarnerVirtualRealm);
        _worldPacket.WriteBit(Initial);
        _worldPacket.FlushBits();
    }
}
