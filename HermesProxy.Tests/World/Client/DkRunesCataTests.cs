using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A death knight's runes as TrinityCore 4.3.4 sends them, from a capture of a level 55 death knight
/// (player 5) on a training dummy. Its SMSG_SPELL_GO rune list has the mask before the cast, the
/// mask after it and a cooldown byte for every rune ((1 - cooldown) * 255: 255 ready, 0 just used),
/// where 3.3.5a has a byte only for the runes the cast used.
/// </summary>
/// <remarks>Runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public sealed class DkRunesCataTests
{
    // Icy Touch: before 0x3F, after 0x2F (rune 5, a Frost rune, used).
    private const string IcyTouch =
        "166E0105010500A5B100000009240000000000FF8E43100132070000D86030F10002000000F33207D86030F100000000" +
        "3F2FFFFFFFFF00FF";

    // Icy Touch again, with rune 5 left by the server's regeneration a sliver above zero: it
    // spent it as ready but had never marked it ready, so the mask is 0x2F before and after.
    private const string IcyTouchSameMask =
        "166E0105010500A5B10000000924000000000041C54E10017E0900008BAC30F10002000000F37E098BAC30F100000000" +
        "2F2FFFFFFFFF00FF";

    // Plague Strike that missed: the runes are given back, so the cast used none.
    private const string PlagueStrikeMissed =
        "166E010501050096B1000000092400000000006D8E4D10000145090000D86030F10102000000F34509D86030F100000000" +
        "2E2E2CFFFFFF56FF";

    // SMSG_POWER_UPDATE: player 5 at full runic power (1000, which the client shows as 100).
    private const string FullRunicPower = "074A010501000000" + "06E8030000";

    private static LegacyHandlerHarness InWorld(uint playerLow = 5)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, playerLow));
        harness.Session.GameState.RuneState = new RuneStateData();
        return harness;
    }

    private static SpellCastData Go(LegacyHandlerHarness harness, string logged)
    {
        harness.Deliver(Opcode.SMSG_SPELL_GO, Convert.FromHexString(logged), harness.Client.HandleSpellGo);
        return Assert.IsType<SpellGo>(Assert.Single(harness.ClientWire.Sent, s => s.Packet is SpellGo).Packet).Cast;
    }

    [Fact]
    public void IcyTouch_PutsTheFrostRuneItUsedOnCooldown()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = InWorld();
        var cast = Go(harness, IcyTouch);

        Assert.Equal(0x2F, cast.RemainingRunes.Count);
        Assert.Equal([255, 255, 255, 255, 0, 255], cast.RemainingRunes.Cooldowns);
        Assert.Contains(cast.RemainingPower, p => p.Type == PowerType.RuneFrost);
        Assert.Equal([255, 255, 255, 255, 0, 255], harness.Session.GameState.RuneState!.Cooldowns);
    }

    [Fact]
    public void IcyTouch_WithTheSameMaskBeforeAndAfter_StillUsesTheRune()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var cast = Go(InWorld(), IcyTouchSameMask);

        Assert.Equal(0x2F, cast.RemainingRunes.Count);
        Assert.Equal([255, 255, 255, 255, 0, 255], cast.RemainingRunes.Cooldowns);
        Assert.Contains(cast.RemainingPower, p => p.Type == PowerType.RuneFrost);
    }

    [Fact]
    public void MissedStrike_UsesNoRune()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var cast = Go(InWorld(), PlagueStrikeMissed);

        Assert.Equal(0x2E, cast.RemainingRunes.Count);
        Assert.Equal([0x2C, 255, 255, 255, 0x56, 255], cast.RemainingRunes.Cooldowns);
        Assert.DoesNotContain(cast.RemainingPower, p => p.Type is PowerType.RuneBlood or PowerType.RuneFrost or PowerType.RuneUnholy);
    }

    [Fact]
    public void RuneCast_TellsTheClientTheRunicPowerTheServerLastSent()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = InWorld();
        harness.Deliver(Opcode.SMSG_POWER_UPDATE, Convert.FromHexString(FullRunicPower), harness.Client.HandlePowerUpdate);
        var cast = Go(harness, IcyTouch);

        Assert.Equal(1000, Assert.Single(cast.RemainingPower, p => p.Type == PowerType.RunicPower).Cost);
    }

    [Fact]
    public void AnotherDeathKnightsCast_LeavesThePlayersRunesAlone()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = InWorld(playerLow: 6);
        var cast = Go(harness, IcyTouch);

        Assert.Null(cast.RemainingRunes);
        Assert.All(harness.Session.GameState.RuneState!.Cooldowns, c => Assert.Equal(255, c));
    }

    // TrinityCore's rune types are Blood 0, Unholy 1, Frost 2, Death 3; the 4.4.2 client's
    // Blood 0, Frost 1, Unholy 2, Death 3.
    [Theory]
    [InlineData("144F0003", 0, 3)] // Blood Tap: a Blood rune turns Death
    [InlineData("144F0000", 0, 0)] // and back
    [InlineData("144F0201", 2, 2)] // an Unholy rune back from Death
    [InlineData("144F0402", 4, 1)] // a Frost rune back from Death
    public void ConvertRune_GoesOnInTheClientsRuneTypes(string logged, uint index, uint rune)
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = InWorld();
        harness.Deliver(Opcode.SMSG_CONVERT_RUNE, Convert.FromHexString(logged), harness.Client.HandleConvertRune);

        var sent = Assert.Single(harness.ClientWire.Sent);
        var convert = Assert.IsType<ConvertRune>(sent.Packet);
        Assert.Equal(index, convert.Index);
        Assert.Equal(rune, convert.Rune);
        // an empty rune list, then the index and the type
        Assert.Equal([0, 0, 0, 0, 0, 0, (byte)index, 0, 0, 0, (byte)rune, 0, 0, 0], sent.Bytes);
    }

    [Fact]
    public void AddRunePower_GoesOnAndReadiesTheRunes()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var harness = InWorld();
        harness.Session.GameState.RuneState!.Cooldowns[2] = 0;
        // one of Empower Rune Weapon's four
        harness.Deliver(Opcode.SMSG_ADD_RUNE_POWER, Convert.FromHexString("156904000000"), harness.Client.HandleAddRunePower);

        var sent = Assert.Single(harness.ClientWire.Sent);
        Assert.Equal(4u, Assert.IsType<AddRunePower>(sent.Packet).AddedRunesMask);
        Assert.Equal([4, 0, 0, 0], sent.Bytes);
        Assert.Equal(255, harness.Session.GameState.RuneState.Cooldowns[2]);
    }
}
