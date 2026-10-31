using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The movement states a 3.3.5a server sends in one packet at login, as AzerothCore and CMaNGOS write
/// them in Player::SendInitialPacketsAfterAddToMap: the size of the list, then per move a length, an
/// opcode and the move's own bytes.
/// </summary>
public sealed class MultipleMovesTests
{
    [Fact]
    public void Wrapper_HandsEveryMoveToItsOwnHandler()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var playerGuid = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        WowGuid128 player = harness.SetActivePlayer(playerGuid);

        byte[] packed = [0x01, 77];
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_MULTIPLE_MOVES, packet =>
        {
            packet.WriteUInt32(2 * (1u + 2 + (uint)packed.Length + 4));
            foreach (Opcode move in new[] { Opcode.SMSG_MOVE_ROOT, Opcode.SMSG_MOVE_SET_WATER_WALK })
            {
                packet.WriteUInt8((byte)(2 + packed.Length + 4));
                packet.WriteUInt16((ushort)LegacyVersion.GetCurrentOpcode(move));
                packet.WriteBytes(packed);
                packet.WriteUInt32(5);
            }
        });
        harness.Deliver(Opcode.SMSG_MULTIPLE_MOVES, wire, harness.Client.HandleMultipleMoves);

        var flags = harness.ClientWire.Sent.Select(sent => Assert.IsType<MoveSetFlag>(sent.Packet)).ToList();
        Assert.Equal(
            [Opcode.SMSG_MOVE_ROOT, Opcode.SMSG_MOVE_SET_WATER_WALK],
            flags.Select(flag => flag.GetUniversalOpcode()));
        Assert.All(flags, flag => Assert.Equal(player, flag.MoverGUID));
    }

    // CMaNGOS finishes a mounted login on the ack of this order, and sends it nowhere else.
    [Fact]
    public void Wrapper_HandsOnTheHeightOrderOfAMountedLogin()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var playerGuid = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        harness.SetActivePlayer(playerGuid);

        byte[] packed = [0x01, 77];
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_MULTIPLE_MOVES, packet =>
        {
            packet.WriteUInt32(1u + 2 + (uint)packed.Length + 4 + 4);
            packet.WriteUInt8((byte)(2 + packed.Length + 4 + 4));
            packet.WriteUInt16((ushort)LegacyVersion.GetCurrentOpcode(Opcode.SMSG_MOVE_SET_COLLISION_HGT));
            packet.WriteBytes(packed);
            packet.WriteUInt32(5);
            packet.WriteFloat(2.8f);
        });
        harness.Deliver(Opcode.SMSG_MULTIPLE_MOVES, wire, harness.Client.HandleMultipleMoves);

        var order = Assert.IsType<MoveSetCollisionHeight>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Equal(5u | MoveSetCollisionHeight.ServerOrderBit, order.SequenceIndex);
        Assert.Equal(2.8f, order.Height);
    }
}
