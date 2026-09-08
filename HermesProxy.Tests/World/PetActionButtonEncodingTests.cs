using HermesProxy.World.Client;
using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World;

// Issue #264: the legacy 3.3.5a action button packs (state:8 | spell:24) —
// UNIT_ACTION_BUTTON_ACTION(X) is X & 0x00FFFFFF on AzerothCore, mangos-wotlk and
// VMaNGOS alike. Masking the spell to 16 bits dropped the high byte of every id above
// 65535, so the Trial of the Champion mount's Thrust (68505) reached the client as 2969.
public class PetActionButtonEncodingTests
{
    // Argent Warhorse (35644) / Argent Battleworg (36558), verified against the native
    // 3.4.3 capture wrathion_343_toc_vehicle_actionbar_20260908.pkt.
    private const uint Thrust = 68505;        // 0x010B99 — needs 17 bits
    private const uint ShieldBreaker = 62575; // 0x00F46F — fits in 16, survived the old mask
    private const uint Charge = 68282;        // 0x010ABA
    private const uint Defend = 66482;        // 0x0103B2

    [Theory]
    [InlineData(Thrust)]
    [InlineData(ShieldBreaker)]
    [InlineData(Charge)]
    [InlineData(Defend)]
    public void LegacyVehicleButton_KeepsFullSpellIdAndUiPosition(uint spellId)
    {
        // Legacy vehicle bars put the UI position (index + 8) in the high byte.
        const byte uiPosition = 0x0A;
        uint legacy = ((uint)uiPosition << 24) | spellId;

        uint modern = WorldClient.TranslateLegacyPetActionButtonToV343(legacy);

        Assert.Equal(spellId, modern & 0x7FFFFF);
        Assert.Equal(uiPosition, (byte)(modern >> 23));
    }

    [Theory]
    [InlineData(0xC1u, 0x181u)] // ACT_ENABLED  -> AutoCastSpell
    [InlineData(0x81u, 0x101u)] // ACT_DISABLED -> ManualSpell
    [InlineData(0xC0u, 0x101u)] // plain spell  -> ManualSpell
    [InlineData(0x01u, 0x001u)] // ACT_PASSIVE  -> PassiveSpell
    public void LegacyPetButton_KeepsFullSpellId(uint legacyState, uint expectedSlot)
    {
        uint legacy = (legacyState << 24) | Thrust;

        uint modern = WorldClient.TranslateLegacyPetActionButtonToV343(legacy);

        Assert.Equal(Thrust, modern & 0x7FFFFF);
        Assert.Equal(expectedSlot, modern >> 23);
    }

    [Fact]
    public void LegacyCommandButton_MapsToCommandSlot()
    {
        // ACT_COMMAND carries a COMMAND_* value, not a spell id.
        uint modern = WorldClient.TranslateLegacyPetActionButtonToV343((0x07u << 24) | 2u);

        Assert.Equal(7u, modern >> 23);
        Assert.Equal(2u, modern & 0x7FFFFF);
    }

    [Fact]
    public void ModernSlotVocabulary_PassesThrough()
    {
        // TrinityCore wotlk_classic already emits modern slots; don't re-translate.
        uint modern = (0x181u << 23) | Thrust;

        Assert.Equal(modern, WorldClient.TranslateLegacyPetActionButtonToV343(modern));
    }

    [Theory]
    [InlineData(Thrust)]
    [InlineData(Charge)]
    [InlineData(Defend)]
    public void ModernToLegacy_RoundTripsFullSpellId(uint spellId)
    {
        uint legacy = (0xC1u << 24) | spellId;

        uint modern = WorldClient.TranslateLegacyPetActionButtonToV343(legacy);
        uint roundTripped = WorldSocket.TranslateV343PetActionToLegacy(modern);

        Assert.Equal(legacy, roundTripped);
    }

    [Fact]
    public void ModernToLegacy_VehicleSlotShipsCastableState()
    {
        // The client casts vehicle abilities through CMSG_PET_CAST_SPELL, so this path is a
        // fallback — but echoing the UI position back would hit the legacy handler's
        // "unknown PET flag" default and be dropped.
        uint legacy = WorldSocket.TranslateV343PetActionToLegacy((8u << 23) | Thrust);

        Assert.Equal(0x81u, legacy >> 24);
        Assert.Equal(Thrust, legacy & 0x00FFFFFF);
    }

    [Fact]
    public void EmptyButton_StaysEmpty()
    {
        Assert.Equal(0u, WorldClient.TranslateLegacyPetActionButtonToV343(0));
        Assert.Equal(0u, WorldSocket.TranslateV343PetActionToLegacy(0));
    }
}
