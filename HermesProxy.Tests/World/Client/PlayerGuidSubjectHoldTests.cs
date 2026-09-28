using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.Tests.World.Outbox;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using Xunit;

namespace HermesProxy.Tests.World.Client;

[Collection("V343ValuesFilter")]
public class PlayerGuidSubjectHoldTests
{
    [Fact]
    public void PlayerPacketsWaitForCreate_AndOtherSubjectsKeepFlowing()
    {
        WorldClient.ForceV343GuidSubjectHoldForTests = true;
        try
        {
            var harness = new LegacyHandlerHarness(recordClientPackets: true);
            var player = WowGuid128.Create(HighGuidType703.Player, 531);
            var other = WowGuid128.Create(HighGuidType703.Player, 532);
            harness.Session.GameState.CurrentPlayerGuid = player;
            harness.Session.GameState.ClientHasPlayerObject = false;

            harness.Client.SendGuidSubjectPacket(new TestServerPacket(Opcode.SMSG_MOVE_UNSET_CAN_FLY, 1), player);
            harness.Client.SendGuidSubjectPacket(new TestServerPacket(Opcode.SMSG_POWER_UPDATE, 2), player);
            harness.Client.SendGuidSubjectPacket(new TestServerPacket(Opcode.SMSG_SPELL_GO, 3), player);
            harness.Client.SendGuidSubjectPacket(new TestServerPacket(Opcode.SMSG_POWER_UPDATE, 4), other);

            Assert.Equal([4], harness.ClientWire.Sent.Select(s => ((TestServerPacket)s.Packet).Id));

            // Filtering the create may register the GUID early; the hold releases only
            // after the send path marks the player object queued and raises GuidKnown.
            harness.Session.GameState.ClientKnownGuids.Add(player);
            Assert.Equal(3, harness.Session.ToClient.PendingCount);
            harness.Session.GameState.ClientHasPlayerObject = true;
            harness.Session.ToClient.Notify(OutboxEvent.GuidKnown(player));

            Assert.Equal([4, 1, 2, 3],
                harness.ClientWire.Sent.Select(s => ((TestServerPacket)s.Packet).Id));
        }
        finally
        {
            WorldClient.ForceV343GuidSubjectHoldForTests = null;
        }
    }

    [Fact]
    public void PlayerPacketAfterCreate_IsSentImmediately()
    {
        WorldClient.ForceV343GuidSubjectHoldForTests = true;
        try
        {
            var harness = new LegacyHandlerHarness(recordClientPackets: true);
            var player = WowGuid128.Create(HighGuidType703.Player, 531);
            harness.Session.GameState.CurrentPlayerGuid = player;
            harness.Session.GameState.ClientHasPlayerObject = true;

            harness.Client.SendGuidSubjectPacket(new TestServerPacket(Opcode.SMSG_POWER_UPDATE, 1), player);

            Assert.Single(harness.ClientWire.Sent);
            Assert.Equal(0, harness.Session.ToClient.PendingCount);
        }
        finally
        {
            WorldClient.ForceV343GuidSubjectHoldForTests = null;
        }
    }
}
