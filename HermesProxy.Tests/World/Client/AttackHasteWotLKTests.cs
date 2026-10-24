using System.Collections.Generic;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A 3.3.5a server sends the hasted attack times, base × its haste multiplier, in whole
/// milliseconds, and no multiplier.
/// The 3.4.3 and 4.4.2 clients show melee and ranged haste from the multipliers (ModHaste,
/// ModRangedHaste), which stayed at 1: GetMeleeHaste() read 0% under any haste. The proxy takes
/// them out of the attack times with the base the server used.
/// </summary>
/// <remarks>
/// Runs under a 3.4.3 or a 4.4.2 modern build with a 3.3.5a server (<c>HERMES_TEST_MODERN_BUILD</c>);
/// a 4.3.4 server sends the multipliers itself.
/// </remarks>
public sealed class AttackHasteWotLKTests
{
    private const uint Axe = 9_410_001;   // 3.70 speed
    private const uint Sigil = 9_410_002; // a relic, no delay
    private const uint Bow = 9_410_003;   // 2.80 speed
    private const uint Unknown = 9_410_004;

    private static bool ProxyWorksHasteOut => ModernVersion.IsWotLKClassicOrLater && !WorldClient.IsCataLegacy;

    private sealed record Equipped(uint MainHand, uint Ranged, uint MainHandDurability = 100);

    private static UnitData Deliver(Equipped? equipped, uint? meleeTime, uint? rangedTime, byte form = 0, uint unitFlags = 0)
    {
        GameData.StoreItemTemplate(Axe, new ItemTemplate { Delay = 3700 });
        GameData.StoreItemTemplate(Sigil, new ItemTemplate { Delay = 0 });
        GameData.StoreItemTemplate(Bow, new ItemTemplate { Delay = 2800 });

        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 8);
        harness.SetActivePlayer(player);

        var fields = new List<(int Field, uint Value)>
        {
            (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_2), (uint)form << 24),
            (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FLAGS), unitFlags),
        };
        if (meleeTime is { } melee)
            fields.Add((LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BASEATTACKTIME), melee));
        if (rangedTime is { } ranged)
            fields.Add((LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RANGEDATTACKTIME), ranged));
        if (equipped != null)
        {
            Equip(harness, fields, EquipmentSlot.MainHand, equipped.MainHand, equipped.MainHandDurability);
            Equip(harness, fields, EquipmentSlot.Ranged, equipped.Ranged, 100);
        }

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
        {
            packet.WriteUInt32(1);
            LegacyPacketBuilder.WriteValuesBlock(packet, player, LegacyVersion.GetUpdateField(PlayerField.PLAYER_END), [.. fields]);
        }), harness.Client.HandleUpdateObject);

        var sent = Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<UpdateObject>());
        return Assert.Single(sent.ObjectUpdates).UnitData;
    }

    private static void Equip(LegacyHandlerHarness harness, List<(int Field, uint Value)> fields, byte slot, uint entry, uint durability)
    {
        var itemGuid = new WowGuid64(HighGuidTypeLegacy.Item, 200 + (uint)slot);
        var item = harness.AddKnownObject(itemGuid, ObjectType.Item);
        lock (harness.Session.GameState.ObjectCacheLock)
        {
            var cached = harness.Session.GameState.ObjectCacheLegacy[item];
            cached[LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY)] = new UpdateField(entry);
            cached[LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_DURABILITY)] = new UpdateField(durability);
            cached[LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_MAXDURABILITY)] = new UpdateField(100u);
        }
        int invSlot = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_INV_SLOT_HEAD) + slot * 2;
        fields.Add((invSlot, (uint)itemGuid.Low));
        fields.Add((invSlot + 1, (uint)(itemGuid.Low >> 32)));
    }

    [Fact]
    public void AWeapon_GivesTheAttackTimeOverItsDelay()
    {
        if (!ProxyWorksHasteOut)
            return;

        var unit = Deliver(new Equipped(Axe, Bow), meleeTime: 2258, rangedTime: 2240);

        Assert.Equal(2258.0 / 3700, (double)unit.ModHaste!.Value, 5);
        Assert.Equal(2240.0 / 2800, (double)unit.ModRangedHaste!.Value, 5);
    }

    [Fact]
    public void NoWeapon_ARelic_ABrokenOrADisarmedOne_CountFromTwoSeconds()
    {
        if (!ProxyWorksHasteOut)
            return;

        Assert.Equal(0.8, (double)Deliver(null, 1600, 1600).ModHaste!.Value, 5);
        Assert.Equal(0.8, (double)Deliver(new Equipped(Axe, Sigil), 2960, 1600).ModRangedHaste!.Value, 5);
        Assert.Equal(0.8, (double)Deliver(new Equipped(Axe, Sigil, MainHandDurability: 0), 1600, null).ModHaste!.Value, 5);
        Assert.Equal(0.8, (double)Deliver(new Equipped(Axe, Sigil), 1600, null, unitFlags: (uint)UnitFlags.Disarmed).ModHaste!.Value, 5);
    }

    [Fact]
    public void CatAndBearForm_CountFromTheFormsOwnSpeed()
    {
        if (!ProxyWorksHasteOut)
            return;

        Assert.Equal(0.8, (double)Deliver(new Equipped(Axe, Sigil), 800, null, form: 1).ModHaste!.Value, 5);
        Assert.Equal(0.8, (double)Deliver(new Equipped(Axe, Sigil), 2000, null, form: 8).ModHaste!.Value, 5);
    }

    [Fact]
    public void AWeaponWhoseDelayIsNotKnown_LeavesHasteAlone()
    {
        if (!ProxyWorksHasteOut)
            return;

        Assert.Null(Deliver(new Equipped(Unknown, Sigil), 2258, null).ModHaste);
    }

    [Fact]
    public void AnUpdateWithoutTheAttackTimes_LeavesHasteAlone()
    {
        if (!ProxyWorksHasteOut)
            return;

        var unit = Deliver(new Equipped(Axe, Bow), meleeTime: null, rangedTime: null);

        Assert.Null(unit.ModHaste);
        Assert.Null(unit.ModRangedHaste);
    }
}
