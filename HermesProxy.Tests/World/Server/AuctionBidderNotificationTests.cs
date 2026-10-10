using System;
using System.Reflection;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads the first field of SMSG_AUCTION_OUTBID_NOTIFICATION and
/// SMSG_AUCTION_WON_NOTIFICATION as the auction house id, and applies an outbid only when it matches
/// the open auction house. A fixed 2 matched only house 2, so outbids at the Horde and Blackwater
/// auction houses were ignored.
/// </summary>
[Collection("V343ValuesFilter")]
public class AuctionBidderNotificationTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // Captured from a 3.4.3 session on AzerothCore playerbots: Warriok outbid Dk on auction 1
    // (Primitive Kilt) with 20000, minimum increment 682. The house id goes in front of these bytes.
    private const string AuctionAndItem = "01000000" + "03A0E0070408" + "9900000000000000000000000000";
    private const string Amounts = "204E000000000000" + "AA02000000000000";

    private const uint BlackwaterHouse = 7;

    private static AuctionBidderNotification Info() => new()
    {
        AuctionHouseID = BlackwaterHouse,
        AuctionID = 1,
        Bidder = new WowGuid128(0x07E0, 0x0800040000000000),
        Item = new ItemInstance { ItemID = 153 },
    };

    private static AuctionOutbidNotification Outbid() => new()
    {
        Info = Info(),
        BidAmount = 20000,
        MinIncrement = 682,
    };

    private static AuctionWonNotification Won() => new() { Info = Info() };

    private static string ViaWrite(ServerPacket packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    private static string ViaSpan(ISpanWritable packet)
    {
        Span<byte> buffer = stackalloc byte[packet.MaxSize];
        return Convert.ToHexString(buffer[..packet.WriteToSpan(buffer)]);
    }

    [Theory]
    [InlineData(true, "07000000")]
    [InlineData(false, "02000000")]
    public void Outbid_SendsTheServersHouseIdOnlyOnV343(bool v343, string house)
    {
        AuctionBidderNotification.ForceV343ForTests = v343;
        try
        {
            string expected = house + AuctionAndItem + Amounts;
            Assert.Equal(expected, ViaWrite(Outbid()));
            Assert.Equal(expected, ViaSpan(Outbid()));
        }
        finally
        {
            AuctionBidderNotification.ForceV343ForTests = null;
        }
    }

    [Theory]
    [InlineData(true, "07000000")]
    [InlineData(false, "02000000")]
    public void Won_SendsTheServersHouseIdOnlyOnV343(bool v343, string house)
    {
        AuctionBidderNotification.ForceV343ForTests = v343;
        try
        {
            string expected = house + AuctionAndItem;
            Assert.Equal(expected, ViaWrite(Won()));
            Assert.Equal(expected, ViaSpan(Won()));
        }
        finally
        {
            AuctionBidderNotification.ForceV343ForTests = null;
        }
    }
}
