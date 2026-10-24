using System;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A vehicle the player summoned gives its owner a vehicle bar, and a 3.3.5a server sends no empty
/// SMSG_PET_SPELLS_MESSAGE when it despawns: only an ended charm clears the bar. The 3.4.3 client
/// takes the vehicle bar down on that message alone, so the proxy sends it when the vehicle whose
/// bar the client holds is destroyed.
/// </summary>
public sealed class VehicleBarAfterDestroyTests
{
    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);

    public VehicleBarAfterDestroyTests() => _harness.SetActivePlayer(new WowGuid64(2));

    // a received packet starts with its opcode, which the handlers never look at
    private static byte[] Destroy(WowGuid64 guid) => [0, 0, .. BitConverter.GetBytes(guid.Low), 0];

    private void Skip() => Assert.SkipUnless(ModernVersion.IsWotLKClassicOrLater && LegacyVersion.ExpansionVersion < 4,
        "the 3.4.3 client's vehicle bar, from a 3.3.5a server");

    [Fact]
    public void DestroyObject_OfTheVehicleWhoseBarIsShown_ClearsTheBar()
    {
        Skip();
        var gryphonLegacy = new WowGuid64(HighGuidTypeLegacy.Vehicle, 28864, 5207);
        var gryphon = _harness.AddKnownObject(gryphonLegacy, ObjectType.Unit);
        _harness.Session.GameState.CurrentPetGuid = gryphon;

        _harness.Deliver(Opcode.SMSG_DESTROY_OBJECT, Destroy(gryphonLegacy), _harness.Client.HandleDestroyObject);

        Assert.Equal(2, _harness.ClientWire.Sent.Count);
        Assert.Equal(gryphon, Assert.Single(Assert.IsType<UpdateObject>(_harness.ClientWire.Sent[0].Packet).DestroyedGuids));
        Assert.True(Assert.IsType<PetSpells>(_harness.ClientWire.Sent[1].Packet).PetGUID.IsEmpty());
        Assert.True(_harness.Session.GameState.CurrentPetGuid.IsEmpty());
    }

    [Fact]
    public void DestroyObject_OfAnotherVehicle_LeavesTheBar()
    {
        Skip();
        var cannon = _harness.AddKnownObject(new WowGuid64(HighGuidTypeLegacy.Vehicle, 28833, 100), ObjectType.Unit);
        _harness.Session.GameState.CurrentPetGuid = cannon;
        var otherLegacy = new WowGuid64(HighGuidTypeLegacy.Vehicle, 28864, 5207);
        _harness.AddKnownObject(otherLegacy, ObjectType.Unit);

        _harness.Deliver(Opcode.SMSG_DESTROY_OBJECT, Destroy(otherLegacy), _harness.Client.HandleDestroyObject);

        Assert.IsType<UpdateObject>(Assert.Single(_harness.ClientWire.Sent).Packet);
        Assert.Equal(cannon, _harness.Session.GameState.CurrentPetGuid);
    }

    [Fact]
    public void DestroyObject_OfAPetWhoseBarIsShown_LeavesTheBar()
    {
        Skip();
        var wolfLegacy = new WowGuid64(HighGuidTypeLegacy.Pet, 299, 12);
        var wolf = _harness.AddKnownObject(wolfLegacy, ObjectType.Unit);
        _harness.Session.GameState.CurrentPetGuid = wolf;

        _harness.Deliver(Opcode.SMSG_DESTROY_OBJECT, Destroy(wolfLegacy), _harness.Client.HandleDestroyObject);

        Assert.IsType<UpdateObject>(Assert.Single(_harness.ClientWire.Sent).Packet);
        Assert.Equal(wolf, _harness.Session.GameState.CurrentPetGuid);
    }
}
