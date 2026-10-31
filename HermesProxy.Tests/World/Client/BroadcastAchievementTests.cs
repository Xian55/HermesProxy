using System.Reflection;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A realm first. A server sends a name, a GUID, the achievement and whether the name is a link;
/// the 3.4.3 client reads seven bits of length, a bit choosing the chat string, the GUID, the
/// achievement and the name.
/// </summary>
public sealed class BroadcastAchievementTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static BroadcastAchievement Deliver(uint nameIsALink)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 77));

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_SERVER_FIRST_ACHIEVEMENT, packet =>
        {
            packet.WriteCString("One");
            packet.WriteGuid(new WowGuid64(HighGuidTypeLegacy.Player, 78));
            packet.WriteUInt32(457);
            packet.WriteUInt32(nameIsALink);
        });
        harness.Deliver(Opcode.SMSG_SERVER_FIRST_ACHIEVEMENT, wire, harness.Client.HandleServerFirstAchievement);
        return Assert.IsType<BroadcastAchievement>(Assert.Single(harness.ClientWire.Sent).Packet);
    }

    [Fact]
    public unsafe void ServerFirst_HasALegacyHandler()
        => Assert.True(GeneratedSmsgDispatch.Get(Opcode.SMSG_SERVER_FIRST_ACHIEVEMENT) != null);

    [Fact]
    public void Table_HasTheOpcodeThe343ClientReads()
        => Assert.Equal(0x2BBCu, (uint)global::HermesProxy.World.Enums.V3_4_3_54261.Opcode.SMSG_BROADCAST_ACHIEVEMENT);

    [Theory]
    [InlineData(1u, false)]
    [InlineData(0u, true)]
    public void ServerFirst_TurnsTheLinkFlagAround(uint nameIsALink, bool plainString)
    {
        var broadcast = Deliver(nameIsALink);

        Assert.Equal("One", broadcast.Name);
        Assert.Equal(457u, broadcast.AchievementID);
        Assert.Equal(plainString, broadcast.GuildAchievement);
    }

    [Fact]
    public void Write_MatchesTheClientsReader()
    {
        var packet = new BroadcastAchievement { Name = "One", AchievementID = 457, GuildAchievement = true };

        packet.Write();

        Assert.Equal(
            [
                0x07,                       // seven bits of name length, then the bit
                0x00, 0x00,                 // an empty GUID
                0xC9, 0x01, 0x00, 0x00,     // the achievement
                0x4F, 0x6E, 0x65,           // One
            ],
            ((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }
}
