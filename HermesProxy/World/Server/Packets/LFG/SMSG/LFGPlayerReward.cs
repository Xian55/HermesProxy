using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class LFGPlayerRewardItem
{
    public uint ItemID;
    public uint Quantity;
    public int BonusCurrency;
    public bool IsCurrency;
}

public class LFGPlayerReward : ServerPacket
{
    public uint QueuedSlot;
    public uint ActualSlot;
    public int RewardMoney;
    public int AddedXP;
    public List<LFGPlayerRewardItem> Rewards = new();

    public LFGPlayerReward() : base(Opcode.SMSG_LFG_PLAYER_REWARD) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(QueuedSlot);
        _worldPacket.WriteUInt32(ActualSlot);
        _worldPacket.WriteInt32(RewardMoney);
        _worldPacket.WriteInt32(AddedXP);
        _worldPacket.WriteUInt32((uint)Rewards.Count);
        foreach (var r in Rewards)
        {
            _worldPacket.WriteUInt32(r.ItemID);
            _worldPacket.WriteUInt32(r.Quantity);
            _worldPacket.WriteInt32(r.BonusCurrency);
            _worldPacket.WriteBit(r.IsCurrency);
            _worldPacket.FlushBits();
        }
    }
}
