using System.Linq;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A MaNGOS-style pet guid has the pet number in its entry slot, and the modern guid needs the
/// creature entry, which only the pet's create tells. The spell bar comes before that create, so the
/// current pet has to move to the real guid with it: left on the number, nothing about the pet ever
/// compares equal to the current pet.
/// </summary>
public sealed class PetGuidAfterCreateTests
{
    private const uint PetNumber = 9568;
    private const uint CreatureEntry = 2031;

    private static readonly WowGuid64 Pet = new(HighGuidTypeLegacy.Pet, PetNumber, 327);
    private static readonly WowGuid128 RealGuid = WowGuid128.Create(HighGuidType703.Pet, 0, CreatureEntry, 327);

    private static LegacyCreate PetCreate => MovementScenarios.CreatureCreate with { Guid = Pet, Entry = CreatureEntry };

    private static LegacyHandlerHarness SpellBarNamedThePet()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        var state = harness.Session.GameState;
        state.CurrentPetGuid = Pet.To128(state);
        state.ClientHasPetObject = false;
        return harness;
    }

    private static void DeliverCreate(LegacyHandlerHarness harness, LegacyCreate create)
        => harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyCreateWire.Build(create), harness.Client.HandleUpdateObject);

    private static void SkipUnlessClientTracksThePet()
        => Assert.SkipUnless(ModernVersion.IsWotLKClassicOrLater, "Only clients from 3.4.3 on track which objects they have.");

    [Fact]
    public void Create_MovesTheCurrentPetToItsRealGuid()
    {
        var harness = SpellBarNamedThePet();

        DeliverCreate(harness, PetCreate);

        Assert.Equal(RealGuid, harness.Session.GameState.CurrentPetGuid);
    }

    [Fact]
    public void Create_OfAnotherPet_LeavesTheCurrentOne()
    {
        var harness = SpellBarNamedThePet();
        var numbered = harness.Session.GameState.CurrentPetGuid;

        DeliverCreate(harness, PetCreate with { Guid = new WowGuid64(HighGuidTypeLegacy.Pet, PetNumber + 1, 328) });

        Assert.Equal(numbered, harness.Session.GameState.CurrentPetGuid);
    }

    [Fact]
    public void WrittenCreate_EndsTheWaitForThePet()
    {
        SkipUnlessClientTracksThePet();
        var harness = SpellBarNamedThePet();

        DeliverCreate(harness, PetCreate);

        Assert.True(harness.Session.GameState.ClientHasPetObject);
        Assert.Equal(default, new SessionClientWire(harness.Session).AwaitedPet);
    }

    /// <summary>
    /// A create can be held after it was read, here until the client has the player. A packet that
    /// comes in between carries the real guid and has to follow that create.
    /// </summary>
    [Fact]
    public void PacketAboutThePet_FollowsACreateThatWasHeld()
    {
        SkipUnlessClientTracksThePet();
        var harness = SpellBarNamedThePet();
        var state = harness.Session.GameState;
        state.ClientKnownGuids.Remove(state.CurrentPlayerGuid);
        DeliverCreate(harness, PetCreate);

        harness.Session.ToClient.Send(new HealthUpdate(RealGuid));
        Assert.DoesNotContain(harness.ClientWire.Sent, s => s.Packet is HealthUpdate);

        state.ClientKnownGuids.Add(state.CurrentPlayerGuid);
        harness.Session.ToClient.Notify(OutboxEvent.GuidKnown(state.CurrentPlayerGuid));

        var sent = harness.ClientWire.Sent.Select(s => s.Packet).ToList();
        int create = sent.FindIndex(p => p is UpdateObject);
        int health = sent.FindIndex(p => p is HealthUpdate);
        Assert.InRange(create, 0, int.MaxValue);
        Assert.True(health > create, $"create at {create}, health at {health}");
    }

    /// <summary>
    /// The clients before 3.4.3 skip the filter that notes the pet's create. Waiting on them would
    /// hold every packet about the pet for good.
    /// </summary>
    [Fact]
    public void ClientThatDoesNotTrackThePet_AwaitsNothing()
    {
        var harness = SpellBarNamedThePet();
        var wire = new SessionClientWire(harness.Session);

        Assert.Equal(ModernVersion.IsWotLKClassicOrLater ? harness.Session.GameState.CurrentPetGuid : default, wire.AwaitedPet);
    }
}
