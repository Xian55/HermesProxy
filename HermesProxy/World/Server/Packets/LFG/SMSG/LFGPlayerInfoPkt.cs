using Framework.Constants;
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public struct LFGBlackListSlot
{
    public uint Slot;
    public uint Reason;
    public int SubReason1;
    public int SubReason2;
    public uint SoftLock;
}

public struct LFGBlackList
{
    public WowGuid128? PlayerGuid;
    public List<LFGBlackListSlot> Slots;
}

public struct LFGPlayerQuestRewardItem
{
    public int ItemID;
    public int Quantity;
}

public struct LFGPlayerQuestRewardCurrency
{
    public int CurrencyID;
    public int Quantity;
}

public struct LFGPlayerQuestReward
{
    public byte Mask;
    public int RewardMoney;
    public int RewardXP;
    public List<LFGPlayerQuestRewardItem>? Items;
    public List<LFGPlayerQuestRewardCurrency>? Currency;
    public List<LFGPlayerQuestRewardCurrency>? BonusCurrency;

    public void Write(WorldPacket data)
    {
        data.WriteUInt8(Mask);
        data.WriteInt32(RewardMoney);
        data.WriteInt32(RewardXP);
        data.WriteUInt32((uint)(Items?.Count ?? 0));
        data.WriteUInt32((uint)(Currency?.Count ?? 0));
        data.WriteUInt32((uint)(BonusCurrency?.Count ?? 0));
        if (Items != null)
            foreach (var i in Items)
            {
                data.WriteInt32(i.ItemID);
                data.WriteInt32(i.Quantity);
            }
        if (Currency != null)
            foreach (var c in Currency)
            {
                data.WriteInt32(c.CurrencyID);
                data.WriteInt32(c.Quantity);
            }
        if (BonusCurrency != null)
            foreach (var c in BonusCurrency)
            {
                data.WriteInt32(c.CurrencyID);
                data.WriteInt32(c.Quantity);
            }
        // Optional RewardSpellID, Unused1, Unused2, Honor — all absent
        data.WriteBit(false);
        data.WriteBit(false);
        data.WriteBit(false);
        data.WriteBit(false);
        data.FlushBits();
    }
}

public struct LFGPlayerDungeonInfo
{
    public uint Slot;
    public int CompletionQuantity;
    public int CompletionLimit;
    public int CompletionCurrencyID;
    public int SpecificQuantity;
    public int SpecificLimit;
    public int OverallQuantity;
    public int OverallLimit;
    public int PurseWeeklyQuantity;
    public int PurseWeeklyLimit;
    public int PurseQuantity;
    public int PurseLimit;
    public int Quantity;
    public uint CompletedMask;
    public uint EncounterMask;
    public bool FirstReward;
    public bool ShortageEligible;
    public LFGPlayerQuestReward Rewards;

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(Slot);
        data.WriteInt32(CompletionQuantity);
        data.WriteInt32(CompletionLimit);
        data.WriteInt32(CompletionCurrencyID);
        data.WriteInt32(SpecificQuantity);
        data.WriteInt32(SpecificLimit);
        data.WriteInt32(OverallQuantity);
        data.WriteInt32(OverallLimit);
        data.WriteInt32(PurseWeeklyQuantity);
        data.WriteInt32(PurseWeeklyLimit);
        data.WriteInt32(PurseQuantity);
        data.WriteInt32(PurseLimit);
        data.WriteInt32(Quantity);
        data.WriteUInt32(CompletedMask);
        data.WriteUInt32(EncounterMask);
        data.WriteUInt32(0); // ShortageReward count
        data.WriteBit(FirstReward);
        data.WriteBit(ShortageEligible);
        data.FlushBits();
        Rewards.Write(data);
    }
}

public class LFGPlayerInfoPkt : ServerPacket
{
    public List<LFGPlayerDungeonInfo> Dungeons = new();
    public LFGBlackList BlackList;

    // ConnectionType.Instance: modern V3_4_3 client expects SMSG_LFG_PLAYER_INFO
    // on the instance pipe (ConnIdx 1). Sent on world pipe → client silently
    // ignores it and the LFG state machine never initializes (queue button
    // stays disabled, microbar eye icon hidden). Confirmed via byte-diff of
    // emitted modern_*.pkt against Wrathion 3.4.3 reference sniff
    // World_solo_dungeon_finder_queue_parsed.txt.
    public LFGPlayerInfoPkt() : base(Opcode.SMSG_LFG_PLAYER_INFO, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32((uint)Dungeons.Count);
        _worldPacket.WriteBit(BlackList.PlayerGuid != null);
        _worldPacket.WriteUInt32((uint)(BlackList.Slots?.Count ?? 0));
        if (BlackList.PlayerGuid is { } blGuid)
            _worldPacket.WritePackedGuid128(blGuid);
        if (BlackList.Slots != null)
            foreach (var slot in BlackList.Slots)
            {
                _worldPacket.WriteUInt32(slot.Slot);
                _worldPacket.WriteUInt32(slot.Reason);
                _worldPacket.WriteInt32(slot.SubReason1);
                _worldPacket.WriteInt32(slot.SubReason2);
                _worldPacket.WriteUInt32(slot.SoftLock);
            }
        foreach (var d in Dungeons)
            d.Write(_worldPacket);
    }
}
