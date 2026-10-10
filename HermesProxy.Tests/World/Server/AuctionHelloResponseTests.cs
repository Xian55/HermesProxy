using System;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads SMSG_AUCTION_HELLO_RESPONSE as the auctioneer, two delivery delays, the auction
/// house id, then the OpenForBusiness bit. Sent with the house id first, it held house 0 and ignored every
/// outbid notice, so the Bids tab kept showing the player as the high bidder.
/// </summary>
[Collection("V343ValuesFilter")]
public class AuctionHelloResponseTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // Captured from a 3.4.3 session on AzerothCore playerbots: Auctioneer Fitch, Alliance auction house 2.
    private const string Guid = "03A73303C083080420";

    private static AuctionHelloResponse Packet() => new()
    {
        Guid = new WowGuid128(0x0333, 0x20000400000883C0),
        AuctionHouseID = 2,
    };

    private static string ViaWrite(AuctionHelloResponse packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    private static string ViaSpan(AuctionHelloResponse packet)
    {
        Span<byte> buffer = stackalloc byte[packet.MaxSize];
        return Convert.ToHexString(buffer[..packet.WriteToSpan(buffer)]);
    }

    [Fact]
    public void Write_OnV343_PutsTheHouseIdAfterTheDeliveryDelays()
    {
        AuctionHelloResponse.ForceV343ForTests = true;
        try
        {
            const string expected = Guid + "00000000" + "00000000" + "02000000" + "80";
            Assert.Equal(expected, ViaWrite(Packet()));
            Assert.Equal(expected, ViaSpan(Packet()));
        }
        finally
        {
            AuctionHelloResponse.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_BeforeV343_SendsTheHouseIdAndTheBitOnly()
    {
        AuctionHelloResponse.ForceV343ForTests = false;
        try
        {
            const string expected = Guid + "02000000" + "80";
            Assert.Equal(expected, ViaWrite(Packet()));
            Assert.Equal(expected, ViaSpan(Packet()));
        }
        finally
        {
            AuctionHelloResponse.ForceV343ForTests = null;
        }
    }
}
