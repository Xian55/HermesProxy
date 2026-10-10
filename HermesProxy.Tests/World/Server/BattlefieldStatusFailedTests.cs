using System;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads SMSG_BATTLEFIELD_STATUS_FAILED as the ride ticket, its trailing flag bit
/// (flushed), the queue id, the reason and the client GUID. Without the flag byte it read the reason
/// as 0, so a deserter's join failure printed nothing, and it read one byte past the end (#363).
/// </summary>
[Collection("V343ValuesFilter")]
public class BattlefieldStatusFailedTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // Captured from a 3.4.3 session on AzerothCore playerbots before the fix: Dk, flagged as a
    // Deserter, tried to join a battleground (reason 2).
    private const string Ticket = "03A0BF020408" + "01000000" + "01000000" + "EE7CC96A00000000";
    private const string Rest = "0600000000001" + "01F" + "02000000" + "0000";

    private static BattlefieldStatusFailed Packet() => new()
    {
        Ticket = new RideTicket
        {
            RequesterGuid = new WowGuid128(0x02BF, 0x0800040000000000),
            Id = 1,
            Type = RideType.Battlegrounds,
            Time = 0x6AC97CEE,
        },
        BattlefieldListId = 6,
        Reason = 2,
    };

    private static string ViaWrite(BattlefieldStatusFailed packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    private static string ViaSpan(BattlefieldStatusFailed packet)
    {
        var buffer = new byte[packet.MaxSize];
        int written = packet.WriteToSpan(buffer);
        Assert.True(written >= 0);
        return Convert.ToHexString(buffer.AsSpan(0, written));
    }

    [Fact]
    public void Write_OnV343_PutsTheTicketFlagByteBeforeTheQueueId()
    {
        BattlefieldStatusFailed.ForceV343ForTests = true;
        try
        {
            Assert.Equal(Ticket + "00" + Rest, ViaWrite(Packet()));
        }
        finally
        {
            BattlefieldStatusFailed.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void WriteToSpan_OnV343_MatchesWrite()
    {
        BattlefieldStatusFailed.ForceV343ForTests = true;
        try
        {
            Assert.Equal(ViaWrite(Packet()), ViaSpan(Packet()));
        }
        finally
        {
            BattlefieldStatusFailed.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_BeforeV343_MatchesTheCapture()
    {
        BattlefieldStatusFailed.ForceV343ForTests = false;
        try
        {
            Assert.Equal(Ticket + Rest, ViaWrite(Packet()));
        }
        finally
        {
            BattlefieldStatusFailed.ForceV343ForTests = null;
        }
    }
}
