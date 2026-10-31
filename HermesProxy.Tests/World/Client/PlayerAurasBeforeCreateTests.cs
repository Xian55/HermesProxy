using System;
using System.IO;
using System.Linq;
using BNetServer.Networking;
using Framework.Realm;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// CMaNGOS sends the player's auras one by one ahead of the player's create, and the client drops
/// them: it has no such object yet. The server's whole list follows the create, and that one looks
/// like a repeat of what the proxy tracked, so the proxy sends the list itself right after the
/// create. The create rides in a batch with many others, except for the few that are held for an
/// item the proxy does not know yet, and a login that was not held came in without its buffs.
/// </summary>
public sealed class PlayerAurasBeforeCreateTests : IDisposable
{
    private const uint WaterWalking = 546;

    private readonly string _accountName = "HermesTests_" + Guid.NewGuid().ToString("N");

    public void Dispose()
    {
        var accountDir = Path.GetFullPath(Path.Combine("AccountData", _accountName));
        if (Directory.Exists(accountDir))
            Directory.Delete(accountDir, recursive: true);
    }

    private LegacyHandlerHarness AtLogin()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        var realm = new Realm();
        realm.SetName("TestRealm");
        harness.Session.GameState.CurrentPlayerInfo = new OwnCharacterInfo { Name = "Testchar", Realm = realm };
        harness.Session.AccountMetaDataMgr = new AccountMetaDataManager(_accountName);
        harness.Session.GameAccountInfo = new GameAccountInfo("Test#1");
        harness.Session.GameState.CurrentPlayerStorage.LoadCurrentPlayer();
        harness.Session.GameState.ClientHasPlayerObject = false;
        return harness;
    }

    // One aura in slot 0: positive, no caster, no duration.
    private static void WriteAura(WorldPacket packet)
    {
        packet.WritePackedGuid(MovementScenarios.ActivePlayer);
        packet.WriteUInt8(0);
        packet.WriteUInt32(WaterWalking);
        packet.WriteUInt8(0x19);
        packet.WriteUInt8(1);
        packet.WriteUInt8(0);
    }

    private static void DeliverAura(LegacyHandlerHarness harness, Opcode opcode)
        => harness.Deliver(opcode, LegacyPacketBuilder.Build(opcode, WriteAura), harness.Client.HandleAuraUpdate);

    private static LegacyCreate PlayerCreate => new()
    {
        Guid = MovementScenarios.ActivePlayer,
        Type = ObjectTypeLegacy.Player,
        Self = true,
        Living = MovementScenarios.Forward,
    };

    private static LegacyCreate CreatureCreate => new()
    {
        Guid = MovementScenarios.Creature,
        Type = ObjectTypeLegacy.Unit,
        Entry = 13358,
        Living = MovementScenarios.Forward,
    };

    private static void SkipUnlessLegacySendsAuraPackets()
        => Assert.SkipUnless(
            ModernVersion.IsWotLKClassicOrLater && LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056),
            "only 3.4.3 and later clients get the sync, and only 3.x and later servers send aura packets");

    private static void AssertListFollowedTheCreate(LegacyHandlerHarness harness)
    {
        var sent = harness.ClientWire.Sent.Select(s => s.Packet).ToList();
        int create = sent.FindIndex(p => p is UpdateObject update && update.ObjectUpdates.Any(u =>
            u.CreateData != null && u.Guid == harness.Session.GameState.CurrentPlayerGuid));
        Assert.True(create >= 0, "the player's create was not sent");

        var after = sent.Skip(create + 1).OfType<AuraUpdate>().ToList();
        var aura = Assert.Single(Assert.Single(after).Auras);
        Assert.Equal(WaterWalking, aura.AuraData!.SpellID);
    }

    [Fact]
    public void AuraSentBeforeACreateAmongOthers_FollowsTheCreate()
    {
        SkipUnlessLegacySendsAuraPackets();
        var harness = AtLogin();
        DeliverAura(harness, Opcode.SMSG_AURA_UPDATE);

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyCreateWire.Build(PlayerCreate, CreatureCreate),
            harness.Client.HandleUpdateObject);
        DeliverAura(harness, Opcode.SMSG_AURA_UPDATE_ALL);

        AssertListFollowedTheCreate(harness);
    }

    [Fact]
    public void AuraSentBeforeACreateOnItsOwn_FollowsTheCreate()
    {
        SkipUnlessLegacySendsAuraPackets();
        var harness = AtLogin();
        DeliverAura(harness, Opcode.SMSG_AURA_UPDATE);

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyCreateWire.Build(PlayerCreate),
            harness.Client.HandleUpdateObject);
        DeliverAura(harness, Opcode.SMSG_AURA_UPDATE_ALL);

        AssertListFollowedTheCreate(harness);
    }
}
