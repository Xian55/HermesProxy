using HermesProxy.World;
using HermesProxy.World.Client;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The two wire forms of the <c>SMSG_GROUP_LIST</c> group-type flags. 2.x (2.4.3 / CMaNGOS)
/// sends isRaid and isBattleground as two separate bytes; 3.3.x packs them into one bitmask
/// (BG = 0x01, Raid = 0x02, Lfg = 0x08). Reading the 2.x form as packed shifted every following
/// field by one byte, so the member count was misread and the member loop ran off the end.
/// </summary>
public class GroupListFlagFormatTests
{
    private static WorldPacket Packet(params byte[] bytes) => new(0u, bytes);

    [Fact]
    public void PackedFormat_DecodesRaidBattlegroundAndLfgBits()
    {
        using var packet = Packet(0x01 | 0x02 | 0x08);
        var (raid, battleground, lfg) = WorldClient.ReadGroupTypeFlags(packet, packed: true);
        Assert.True(raid);
        Assert.True(battleground);
        Assert.True(lfg);
    }

    /// <summary>
    /// TrinityCore Group::RemoveMember tells the one who left with a list whose type byte is 0x10.
    /// It is all a battleground raid sends on the way out.
    /// </summary>
    [Theory]
    [InlineData(0x10, true)]
    [InlineData(0x03, false)]
    [InlineData(0x00, false)]
    public void PackedFormat_RemovalMarkIsBit0x10(byte groupType, bool expected)
    {
        using var packet = Packet(groupType);
        WorldClient.ReadGroupTypeFlags(packet, packed: true, out bool removed);
        Assert.Equal(expected, removed);
    }

    [Fact]
    public void PackedFormat_NoBitsIsAHomeGroup()
    {
        using var packet = Packet(0x00);
        var (raid, battleground, lfg) = WorldClient.ReadGroupTypeFlags(packet, packed: true);
        Assert.False(raid);
        Assert.False(battleground);
        Assert.False(lfg);
    }

    [Fact]
    public void TwoByteFormat_ReadsRaidThenBattleground()
    {
        using var packet = Packet(1, 0);
        var (raid, battleground, lfg) = WorldClient.ReadGroupTypeFlags(packet, packed: false);
        Assert.True(raid);
        Assert.False(battleground);
        Assert.False(lfg);
    }

    [Fact]
    public void TwoByteFormat_BattlegroundIsTheSecondByte()
    {
        using var packet = Packet(0, 1);
        var (raid, battleground, lfg) = WorldClient.ReadGroupTypeFlags(packet, packed: false);
        Assert.False(raid);
        Assert.True(battleground);
        Assert.False(lfg);
    }
}
