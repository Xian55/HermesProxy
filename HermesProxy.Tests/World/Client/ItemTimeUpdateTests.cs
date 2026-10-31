using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>The time a timed item has left, which a server sends when it is added and at login.</summary>
public sealed class ItemTimeUpdateTests
{
    [Fact]
    public unsafe void TimeUpdate_HasALegacyHandler()
        => Assert.True(GeneratedSmsgDispatch.Get(Opcode.SMSG_ITEM_TIME_UPDATE) != null);

    [Fact]
    public void Table_HasTheOpcodeThe343ClientReads()
        => Assert.Equal(0x2754u, (uint)global::HermesProxy.World.Enums.V3_4_3_54261.Opcode.SMSG_ITEM_TIME_UPDATE);

    [Fact]
    public void TimeUpdate_WaitsForThePlayerWhoseItemsComeWithIt()
        => Assert.True(new ItemTimeUpdate().NeedsObject(new WowGuid128()));

    [Fact]
    public void TimeUpdate_NamesTheItemAndTheTimeLeft()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 77));
        var itemGuid = new WowGuid64(HighGuidTypeLegacy.Item, 78);
        WowGuid128 item = harness.AddKnownObject(itemGuid, ObjectType.Item);

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_ITEM_TIME_UPDATE, packet =>
        {
            packet.WriteGuid(itemGuid);
            packet.WriteUInt32(1800);
        });
        harness.Deliver(Opcode.SMSG_ITEM_TIME_UPDATE, wire, harness.Client.HandleItemTimeUpdate);

        var time = Assert.IsType<ItemTimeUpdate>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(item, time.ItemGuid);
        Assert.Equal(1800u, time.DurationLeft);
    }
}
