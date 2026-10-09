using System;
using System.Reflection;
using System.Text;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads SMSG_SET_TIME_ZONE_INFORMATION as three 7-bit lengths, flushed, then three
/// strings (ServerTimeTZ, GameTimeTZ, ServerRegionalTZ), as TrinityCore's 3.4.3 writer sends it. Sent
/// with two, the client took the first character of the first zone as the third length (#361).
/// </summary>
[Collection("V343ValuesFilter")]
public class SetTimeZoneInformationTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static SetTimeZoneInformation Packet(string zone) => new()
    {
        ServerTimeTZ = zone,
        GameTimeTZ = zone,
        ServerRegionalTZ = zone,
    };

    private static byte[] ViaWrite(SetTimeZoneInformation packet)
    {
        packet.Write();
        return ((WorldPacket)WorldPacketField.GetValue(packet)!).GetData();
    }

    private static byte[] ViaSpan(SetTimeZoneInformation packet)
    {
        var buffer = new byte[packet.MaxSize];
        int written = packet.WriteToSpan(buffer);
        Assert.True(written >= 0);
        return buffer.AsSpan(0, written).ToArray();
    }

    [Fact]
    public void Write_OnV343_SendsThreeLengthsThenThreeZones()
    {
        SetTimeZoneInformation.ForceV343ForTests = true;
        try
        {
            byte[] data = ViaWrite(Packet("Europe/Paris"));

            // 12, 12, 12 as three 7-bit fields: 0001100 0001100 0001100 + 3 padding bits.
            byte[] zone = Encoding.UTF8.GetBytes("Europe/Paris");
            byte[] expected = [0x18, 0x30, 0x60, .. zone, .. zone, .. zone];
            Assert.Equal(Convert.ToHexString(expected), Convert.ToHexString(data));
        }
        finally
        {
            SetTimeZoneInformation.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_OnV343_WithEmptyZones_StillSendsTheLengths()
    {
        SetTimeZoneInformation.ForceV343ForTests = true;
        try
        {
            Assert.Equal("000000", Convert.ToHexString(ViaWrite(Packet(""))));
        }
        finally
        {
            SetTimeZoneInformation.ForceV343ForTests = null;
        }
    }

    [Theory]
    [InlineData("Europe/Paris")]
    [InlineData("")]
    public void WriteToSpan_OnV343_MatchesWrite(string zone)
    {
        SetTimeZoneInformation.ForceV343ForTests = true;
        try
        {
            Assert.Equal(Convert.ToHexString(ViaWrite(Packet(zone))), Convert.ToHexString(ViaSpan(Packet(zone))));
        }
        finally
        {
            SetTimeZoneInformation.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_BeforeV343_KeepsTwoZones()
    {
        SetTimeZoneInformation.ForceV343ForTests = false;
        try
        {
            byte[] zone = Encoding.UTF8.GetBytes("Europe/Paris");
            byte[] expected = [0x18, 0x30, .. zone, .. zone];
            Assert.Equal(Convert.ToHexString(expected), Convert.ToHexString(ViaWrite(Packet("Europe/Paris"))));
        }
        finally
        {
            SetTimeZoneInformation.ForceV343ForTests = null;
        }
    }
}
