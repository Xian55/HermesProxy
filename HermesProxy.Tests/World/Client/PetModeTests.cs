using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// SMSG_PET_MODE packs a pet's orders into a u32 on a legacy server and three bytes on a modern
/// client, in another order. CMaNGOS sends it when the owner mounts and the pet stays, and again on
/// dismount.
/// </summary>
public sealed class PetModeTests
{
    private static readonly WowGuid64 Pet = new(HighGuidTypeLegacy.Pet, 416, 1000);

    private static (PetMode Mode, byte[] Bytes) Deliver(uint flags)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 8));
        harness.AddKnownObject(Pet, ObjectType.Unit);

        harness.Deliver(Opcode.SMSG_PET_MODE, LegacyPacketBuilder.Build(Opcode.SMSG_PET_MODE, p =>
        {
            p.WriteGuid(Pet);
            p.WriteUInt32(flags);
        }), harness.Client.HandlePetMode);

        var sent = Assert.Single(harness.ClientWire.Sent);
        return (Assert.IsType<PetMode>(sent.Packet), sent.Bytes);
    }

    /// <summary>CMaNGOS <c>Player::ResolvePendingUnmount</c>: aggressive (2), told to follow (1).</summary>
    [Fact]
    public void HandlePetMode_ReactAndCommand_GoToTheirOwnFields()
    {
        var (mode, _) = Deliver(2 | 1 * 0x100);

        Assert.Equal((ReactStates)2, mode.ReactState);
        Assert.Equal((CommandStates)1, mode.CommandState);
        Assert.Equal(0, mode.Flag);
    }

    /// <summary>CMaNGOS PET_MODE_DISABLE_ACTIONS, for the pet of a mounted owner.</summary>
    [Fact]
    public void HandlePetMode_DisableActions_IsTheFlag()
    {
        var (mode, _) = Deliver(0x8000000);

        Assert.Equal(8, mode.Flag);
        Assert.Equal((ReactStates)0, mode.ReactState);
        Assert.Equal((CommandStates)0, mode.CommandState);
    }

    /// <summary>The client reads a u16 of command and flag, then a byte of react state.</summary>
    [Fact]
    public void Write_AfterTheGuid_IsCommandFlagReact()
    {
        var (_, bytes) = Deliver(2 | 1 * 0x100 | 0x8000000);

        Assert.Equal([1, 8, 2], bytes[^3..]);
    }
}
