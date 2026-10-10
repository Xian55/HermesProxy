using System;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads each SMSG_LFG_PLAYER_REWARD reward as an item bit and a currency bit, an item
/// instance, the quantity, the bonus quantity and the currency id (TrinityCore 3.4.3 LFGPlayerRewards).
/// The other layout gave it an item instance of id 0x020000B8 for an Emblem of Triumph (#364).
/// </summary>
[Collection("V343ValuesFilter")]
public class LFGPlayerRewardTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // From a 3.4.3 session on 2026-08-28: 14g 80s, 33100 XP, two Emblems of Triumph.
    private const string Header = "05010006" + "CB000001" + "20420200" + "4C810000" + "01000000";

    private static LFGPlayerReward Packet(bool currency = false) => new()
    {
        QueuedSlot = 0x06000105,
        ActualSlot = 0x010000CB,
        RewardMoney = 148000,
        AddedXP = 33100,
        Rewards = [new LFGPlayerRewardItem { ItemID = 47241, Quantity = 2, IsCurrency = currency }],
    };

    private static string ViaWrite(LFGPlayerReward packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    [Fact]
    public void Write_OnV343_SendsAnItemRewardAsAnItemInstance()
    {
        LFGPlayerReward.ForceV343ForTests = true;
        try
        {
            // item bit, no currency bit; item 47241, seed 0, properties 0, no bonus, no modifiers;
            // quantity 2, bonus 0.
            string item = "89B80000" + "00000000" + "00000000" + "00" + "00";
            Assert.Equal(Header + "80" + item + "02000000" + "00000000", ViaWrite(Packet()));
        }
        finally
        {
            LFGPlayerReward.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_OnV343_SendsACurrencyRewardAfterTheQuantities()
    {
        LFGPlayerReward.ForceV343ForTests = true;
        try
        {
            Assert.Equal(Header + "40" + "02000000" + "00000000" + "89B80000", ViaWrite(Packet(currency: true)));
        }
        finally
        {
            LFGPlayerReward.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_BeforeV343_MatchesTheCapture()
    {
        LFGPlayerReward.ForceV343ForTests = false;
        try
        {
            Assert.Equal(Header + "89B80000" + "02000000" + "00000000" + "00", ViaWrite(Packet()));
        }
        finally
        {
            LFGPlayerReward.ForceV343ForTests = null;
        }
    }
}
