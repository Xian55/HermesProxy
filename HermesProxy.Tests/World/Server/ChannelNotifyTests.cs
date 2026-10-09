using System;
using System.Reflection;
using System.Text;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_CHANNEL_NOTIFY as the 3.4.3 client reads it: 6-bit type, 7-bit channel length, 6-bit sender
/// length, sender guid, sender account guid, sender realm, target guid, target realm, channel id, the
/// two member flags only for a mode change, then channel and sender. TrinityCore 3.4.3 writes the same.
/// </summary>
public class ChannelNotifyTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private const string EmptyGuid = "0000";
    private static readonly string Channel = Convert.ToHexString(Encoding.UTF8.GetBytes("hermescp"));

    private static string ViaWrite(ChannelNotify packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    [Fact]
    public void Write_WrongPassword_SendsOnlyTheChannel()
    {
        var notice = new ChannelNotify { Type = ChatNotify.WrongPassword, Channel = "hermescp" };

        // 000100 0001000 000000 + 5 padding bits.
        string expected = "104000" + EmptyGuid + EmptyGuid + "00000000" + EmptyGuid + "00000000" + "00000000" + Channel;
        Assert.Equal(expected, ViaWrite(notice));
    }

    [Fact]
    public void Write_ModeChange_AddsTheOldAndNewFlagsBeforeTheStrings()
    {
        var notice = new ChannelNotify { Type = ChatNotify.ModeChange, Channel = "hermescp", OldFlags = 0, NewFlags = 3 };

        // 001100 0001000 000000 + 5 padding bits.
        string expected = "304000" + EmptyGuid + EmptyGuid + "00000000" + EmptyGuid + "00000000" + "00000000" + "0003" + Channel;
        Assert.Equal(expected, ViaWrite(notice));
    }

    [Fact]
    public void Write_ChannelOwner_SendsTheOwnerNameAsSender()
    {
        var notice = new ChannelNotify { Type = ChatNotify.ChannelOwner, Channel = "hermescp", Sender = "Xian" };

        // 001011 0001000 000100 + 5 padding bits.
        string expected = "2C4080" + EmptyGuid + EmptyGuid + "00000000" + EmptyGuid + "00000000" + "00000000"
            + Channel + Convert.ToHexString(Encoding.UTF8.GetBytes("Xian"));
        Assert.Equal(expected, ViaWrite(notice));
    }

    [Theory]
    [InlineData(ChatNotify.VoiceOff, 0x23)]
    [InlineData(ChatNotify.TrialRestricted, 0x25)]
    [InlineData(ChatNotify.NotAllowedInChannel, 0x26)]
    public void Write_UsesThe343NumberingAfterVoiceOff(ChatNotify type, int wireType)
    {
        byte[] data = Convert.FromHexString(ViaWrite(new ChannelNotify { Type = type }));

        Assert.Equal(wireType, data[0] >> 2);
    }
}
