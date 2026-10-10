using HermesProxy.Enums;
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

    // The 3.4.3 client reads each reward as an item bit and a currency bit, the item as an item
    // instance, the quantity, the bonus quantity, then the currency id (TrinityCore 3.4.3
    // LFGPlayerRewards). Sent the other layout, it took its two bits from the item id: Emblem of
    // Triumph (47241) became an item instance of id 0x020000B8, and it read past the end (#364).
    private static bool Is343Layout => ForceV343ForTests ?? ModernVersion.IsWotLKClassicOrLater;
    // The test process never runs as V3_4_3; tests set this to reach the 3.4.3 layout.
    internal static bool? ForceV343ForTests;

    public override void Write()
    {
        _worldPacket.WriteUInt32(QueuedSlot);
        _worldPacket.WriteUInt32(ActualSlot);
        _worldPacket.WriteInt32(RewardMoney);
        _worldPacket.WriteInt32(AddedXP);
        _worldPacket.WriteUInt32((uint)Rewards.Count);
        if (Is343Layout)
        {
            foreach (var r in Rewards)
            {
                _worldPacket.WriteBit(!r.IsCurrency);
                _worldPacket.WriteBit(r.IsCurrency);
                _worldPacket.FlushBits();
                if (!r.IsCurrency)
                {
                    ItemInstance item = new() { ItemID = r.ItemID };
                    item.Write(_worldPacket);
                }
                _worldPacket.WriteUInt32(r.Quantity);
                _worldPacket.WriteInt32(r.BonusCurrency);
                if (r.IsCurrency)
                    _worldPacket.WriteInt32((int)r.ItemID);
            }
            return;
        }
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
