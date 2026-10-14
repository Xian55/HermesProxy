using System.Text;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// A channel owner's commands as they go to a 4.3.4 server, after TrinityCore 4.3.4's
/// ChannelHandler: the lengths as bits, then the strings without terminators, in an order of
/// its own per command. A channel and a player name of different lengths, so that each order
/// shows.
/// </summary>
public sealed class CataChannelCommandTests
{
    private const string Channel = "hpcata";     // 6
    private const string Name = "Bo";            // 2

    private static byte[] Expected(byte first, byte second, string a, string b)
        => [first, second, .. Encoding.UTF8.GetBytes(a), .. Encoding.UTF8.GetBytes(b)];

    private static byte[] Written(Opcode opcode)
    {
        using var packet = new WorldPacket();
        ChatSystem.WritePlayerCommandCata(packet, opcode, Channel, Name);
        return packet.GetDataSpan().ToArray();
    }

    [Theory]
    [InlineData(Opcode.CMSG_CHAT_CHANNEL_MODERATOR)]
    [InlineData(Opcode.CMSG_CHAT_CHANNEL_SET_OWNER)]
    [InlineData(Opcode.CMSG_CHAT_CHANNEL_BAN)]
    public void ChannelEightBitsPlayerSeven_ThePlayerFirst(Opcode opcode)
        => Assert.Equal(Expected(0x06, 0x04, Name, Channel), Written(opcode));

    [Fact]
    public void Kick_ChannelEightBitsPlayerSeven_TheChannelFirst()
        => Assert.Equal(Expected(0x06, 0x04, Channel, Name), Written(Opcode.CMSG_CHAT_CHANNEL_KICK));

    [Fact]
    public void Unmoderator_PlayerSevenBitsChannelEight_TheChannelFirst()
        => Assert.Equal(Expected(0x04, 0x0C, Channel, Name), Written(Opcode.CMSG_CHAT_CHANNEL_UNMODERATOR));

    [Fact]
    public void Invite_PlayerSevenBitsChannelEight_ThePlayerFirst()
        => Assert.Equal(Expected(0x04, 0x0C, Name, Channel), Written(Opcode.CMSG_CHAT_CHANNEL_INVITE));

    [Fact]
    public void Unban_ChannelSevenBitsPlayerEight_ThePlayerFirst()
        => Assert.Equal(Expected(0x0C, 0x04, Name, Channel), Written(Opcode.CMSG_CHAT_CHANNEL_UNBAN));

    [Fact]
    public void ChannelName_EightBitsThenTheName()
    {
        using var packet = new WorldPacket();
        ChatSystem.WriteChannelNameCata(packet, Channel);
        Assert.Equal<byte>([0x06, .. Encoding.UTF8.GetBytes(Channel)], packet.GetDataSpan().ToArray());
    }
}
