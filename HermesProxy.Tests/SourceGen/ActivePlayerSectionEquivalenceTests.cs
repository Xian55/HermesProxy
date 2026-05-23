using System;
using System.Runtime.CompilerServices;
using Framework.Util;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.V3_4_3_54261;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

// Byte-equivalence oracle for Phase 5b ActivePlayer Update path. The pre-Phase-5b
// hand-port `WriteUpdateActivePlayerData` is inlined here as the frozen oracle.
// Generator-emitted `WriteUpdateActivePlayerData` must produce byte-identical output
// across the scenario matrix.
//
// New generator features exercised:
//   - BlockMaskShape.UInt32PlusBits16 — 48-block mask (WriteUInt32 + WriteBits(16)).
//   - DescriptorMaskMutator — InvSlots fan-out + GlyphsDirty capture-and-clear.
//   - DescriptorUpdatePostFlush — explicit FlushBits after Block 102 scalar writes.
//   - DescriptorMaskPreamble + scalar UpdateField CustomWriter — KnownTitles
//     dynamic field (preamble between blocks-mask + FlushBits; body at bit 3).
//   - CustomPredicate for scalar fields (KnownTitles, Skill nested).
//   - CustomField WriteOnly — InvSlots group + Glyphs group bodies driven by
//     mask-mutators rather than per-field source predicates.
//   - PerElement + CustomWriter on nested-struct arrays (RestInfo, PvpInfo).
//
// Create path is not byte-eq tested here — `WriteCreateActivePlayerAll` is the
// hand-port body moved verbatim onto the builder partial; byte-eq is trivial by
// construction (no logic change, no generator transformation).
public class ActivePlayerSectionEquivalenceTests
{
    private static GlobalSessionData CreateGlobalSession()
        => (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

    private static GameSessionData CreateGameSession()
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new System.Collections.Generic.Dictionary<WowGuid128, ObjectType>());
        // Default glyph slot ids + glyphs from production init.
        typeof(GameSessionData).GetField(nameof(GameSessionData.ActiveGlyphSlotIds))!
            .SetValue(session, new uint[] { 21, 22, 23, 24, 25, 26 });
        typeof(GameSessionData).GetField(nameof(GameSessionData.ActiveGlyphs))!
            .SetValue(session, new ushort[6]);
        return session;
    }

    private static ObjectUpdateBuilder MakeBuilder(WowGuid128 guid, GameSessionData session, out ObjectUpdate update)
    {
        var globalSession = CreateGlobalSession();
        update = new ObjectUpdate(guid, UpdateTypeModern.Values, globalSession);
        if (update.ActivePlayerData == null) update.ActivePlayerData = new ActivePlayerData();
        // Register as ActivePlayer in session so _realObjectType = ActivePlayer → IsOwner=true.
        typeof(GameSessionData).GetField(nameof(GameSessionData.CurrentPlayerGuid))!
            .SetValue(session, guid);
        return new ObjectUpdateBuilder(update, session);
    }

    public static System.Collections.Generic.IEnumerable<object[]> UpdateScenarios()
    {
        yield return new object[] { "empty", (Action<ActivePlayerData, GameSessionData>)((_, _) => { }) };
        yield return new object[] { "coinage-only", (Action<ActivePlayerData, GameSessionData>)((a, _) => { a.Coinage = 12345uL; }) };
        yield return new object[] { "xp-levelup", (Action<ActivePlayerData, GameSessionData>)((a, _) => { a.XP = 99999; a.NextLevelXP = 200000; }) };
        yield return new object[] { "block0-scalars", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.Coinage = 1uL;
            a.XP = 2;
            a.NextLevelXP = 3;
            a.TrialXP = 4;
            a.CharacterPoints = 5;
            a.MaxTalentTiers = 6;
            a.TrackCreatureMask = 7u;
            a.MainhandExpertise = 8f;
            a.OffhandExpertise = 9f;
        }) };
        yield return new object[] { "farsight", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.FarsightObject = WowGuid128.Create(HighGuidType703.Player, 99);
        }) };
        yield return new object[] { "known-titles-single", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.KnownTitles[0] = 0x1u;
        }) };
        yield return new object[] { "known-titles-spread", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.KnownTitles[0] = 0xAAAAu;
            a.KnownTitles[5] = 0x5555u;
            a.KnownTitles[11] = 0xFFFFu;
        }) };
        yield return new object[] { "block38-floats", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.CritPercentage = 25.5f;
            a.DodgePercentage = 12.3f;
            a.ParryPercentage = 7.7f;
            a.Mastery = 1f;
        }) };
        yield return new object[] { "block70-bytes-words", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.GrantableLevels = 5;
            a.MultiActionBars = 0x0F;
            a.AmmoID = 12345u;
            a.TodayHonorableKills = 99;
            a.ThisWeekContribution = 5000u;
            a.WatchedFactionIndex = 67;
            a.PetSpellPower = 250;
        }) };
        yield return new object[] { "block102-mid-flush", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.AuraVision = 1;
            a.NumBackpackSlots = 16;
            a.OverrideSpellsID = 0;
            a.LootSpecID = 250u;
            a.PvPTierMaxFromWins = 1u;
            a.PvPRankProgress = 50;
        }) };
        yield return new object[] { "professionskill", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.ProfessionSkillLine[0] = 164; // Blacksmithing
            a.ProfessionSkillLine[1] = 202; // Engineering
        }) };
        yield return new object[] { "trackresource", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.TrackResourceMask[0] = 0xFFu;
            a.TrackResourceMask[1] = 0x12u;
        }) };
        yield return new object[] { "shared-269-multi", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.SpellCritPercentage[0] = 1f;
            a.SpellCritPercentage[3] = 3f;
            a.ModDamageDonePos[2] = 100;
            a.ModDamageDoneNeg[4] = -50;
            a.ModDamageDonePercent[6] = 1.5f;
        }) };
        yield return new object[] { "explored-zones", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.ExploredZones[0] = 0xDEADBEEFCAFEBABEuL;
            a.ExploredZones[100] = 0x1234567890ABCDEFuL;
            a.ExploredZones[239] = 0xFFFFFFFFFFFFFFFFuL;
        }) };
        yield return new object[] { "rest-info-full", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.RestInfo[0] = new RestInfo { Threshold = 100u, StateID = 1u };
            a.RestInfo[1] = new RestInfo { Threshold = 200u, StateID = 2u };
        }) };
        yield return new object[] { "rest-info-partial", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.RestInfo[0] = new RestInfo { Threshold = 50u };
            a.RestInfo[1] = new RestInfo { StateID = 1u };
        }) };
        yield return new object[] { "weapon-multipliers", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.WeaponDmgMultipliers[0] = 1.5f;
            a.WeaponDmgMultipliers[2] = 2f;
            a.WeaponAtkSpeedMultipliers[1] = 1.2f;
        }) };
        yield return new object[] { "buyback", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.BuybackPrice[0] = 100u;
            a.BuybackPrice[6] = 500u;
            a.BuybackTimestamp[3] = 1700000000u;
        }) };
        yield return new object[] { "combat-ratings", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.CombatRatings[0] = 100;
            a.CombatRatings[15] = 500;
            a.CombatRatings[31] = 999;
        }) };
        yield return new object[] { "pvp-info-rating", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.PvpInfo[0] = new PVPInfo { Rating = 1500, WeeklyPlayed = 20, SeasonPlayed = 100 };
        }) };
        yield return new object[] { "pvp-info-disqualified", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.PvpInfo[1] = new PVPInfo { Disqualified = true };
            a.PvpInfo[2] = new PVPInfo { Rating = 2000, WeeklyBestWinPvpTierID = 7, Field_28 = 1 };
        }) };
        yield return new object[] { "bank-flags", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.BagSlotFlags[0] = 1u;
            a.BankBagSlotFlags[5] = 2u;
        }) };
        yield return new object[] { "quest-completed", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.QuestCompleted[0] = 0xFFuL;
            a.QuestCompleted[500] = 0xAB00uL;
            a.QuestCompleted[874] = 0x1uL;
        }) };
        yield return new object[] { "glyphs-dirty", (Action<ActivePlayerData, GameSessionData>)((a, gs) =>
        {
            gs.ActiveGlyphsDirty = true;
            gs.ActiveGlyphs[0] = 4444;
            gs.ActiveGlyphs[5] = 7777;
            a.GlyphsEnabled = 0x3F;
            gs.GlyphsEnabled = 0x3F;
        }) };
        yield return new object[] { "glyphs-enabled-only", (Action<ActivePlayerData, GameSessionData>)((a, gs) =>
        {
            // Level-up scenario: only the unlock bitmask changed; equipped glyphs untouched.
            a.GlyphsEnabled = 0x03; // L15: slots 0,1 unlocked
            gs.GlyphsEnabled = 0x03;
        }) };
        yield return new object[] { "skill-rank-up", (Action<ActivePlayerData, GameSessionData>)((a, _) =>
        {
            a.Skill = new SkillInfo();
            a.Skill.SkillLineID[0] = 164;
            a.Skill.SkillRank[0] = 100;
            a.Skill.SkillMaxRank[0] = 225;
        }) };
        yield return new object[] { "all-features-mixed", (Action<ActivePlayerData, GameSessionData>)((a, gs) =>
        {
            a.Coinage = 999uL;
            a.XP = 1234;
            a.Honor = 500;
            a.HonorNextLevel = 1000;
            a.KnownTitles[0] = 0x1u;
            a.ProfessionSkillLine[0] = 164;
            a.ExploredZones[10] = 0xFFFFuL;
            a.CombatRatings[5] = 75;
            a.SpellCritPercentage[2] = 5f;
            a.RestInfo[0] = new RestInfo { Threshold = 50u, StateID = 1u };
            a.PvpInfo[3] = new PVPInfo { Rating = 1800 };
            gs.ActiveGlyphsDirty = true;
            a.GlyphsEnabled = 0x3F;
            gs.GlyphsEnabled = 0x3F;
        }) };
    }

    [Theory]
    [MemberData(nameof(UpdateScenarios))]
    public void WriteUpdateActivePlayerData_GeneratedMatchesHandPort(string _label, Action<ActivePlayerData, GameSessionData> populate)
    {
        // Generator-emitted path.
        var sessionActual = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builderActual = MakeBuilder(guid, sessionActual, out var updateActual);
        populate(updateActual.ActivePlayerData!, sessionActual);

        var actualPacket = new WorldPacket();
        builderActual.WriteUpdateActivePlayerData(actualPacket);

        // Frozen hand-port oracle — fresh session so GlyphsDirty consume isolates per call.
        var sessionExpected = CreateGameSession();
        var dataExpected = new ActivePlayerData();
        populate(dataExpected, sessionExpected);

        var expectedPacket = new WorldPacket();
        WriteUpdateActivePlayerData_HandPort(expectedPacket, dataExpected, sessionExpected);

        Assert.Equal(expectedPacket.GetData(), actualPacket.GetData());
    }

    // ===========================================================================
    // HasAny regression tests — MaskMutator HasAnyPredicate wiring. Without these
    // the Values update is skipped when only InvSlots / GlyphsDirty changed
    // (loot/bag-pickup → items invisible until relog; glyph swap → stale glyph UI).
    // ===========================================================================

    [Fact]
    public void HasAnyActivePlayerFieldSet_InvSlotsOnly_ReturnsTrue()
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builder = MakeBuilder(guid, session, out var update);

        update.ActivePlayerData!.PackSlots[0] = WowGuid128.Create(HighGuidType703.Item, 42);
        // No scalar field set. Without HasAnyPredicate on InvSlots mask mutator,
        // HasAny returns false → ActivePlayer Values update skipped → modern client
        // never receives bag refresh.
        Assert.True(builder.HasAnyActivePlayerFieldSet());
    }

    [Fact]
    public void HasAnyActivePlayerFieldSet_GlyphsDirty_ReturnsTrue()
    {
        var session = CreateGameSession();
        session.ActiveGlyphsDirty = true;
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builder = MakeBuilder(guid, session, out var update);

        Assert.True(builder.HasAnyActivePlayerFieldSet());
    }

    [Fact]
    public void HasAnyActivePlayerFieldSet_GlyphsEnabledSet_ReturnsTrue()
    {
        // Level-up scenario: legacy PLAYER_GLYPHS_ENABLED bitmask flipped (slot
        // unlock at L15/30/50/70/80). Must fan into ActivePlayer Values delta or
        // unlock UI lags until relog.
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builder = MakeBuilder(guid, session, out var update);
        update.ActivePlayerData!.GlyphsEnabled = 0x03;

        Assert.True(builder.HasAnyActivePlayerFieldSet());
    }

    [Fact]
    public void HasAnyActivePlayerFieldSet_AllEmpty_ReturnsFalse()
    {
        var session = CreateGameSession();
        session.ActiveGlyphsDirty = false;
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builder = MakeBuilder(guid, session, out var update);

        Assert.False(builder.HasAnyActivePlayerFieldSet());
    }

    // ===========================================================================
    // Frozen pre-Phase-5b hand-port oracle. Identical to
    // V3_4_3_54261/ObjectUpdateBuilder.cs:1434-2075 from commit e5b3bda before
    // the generator migration deleted it. `_gameState` references rewritten to
    // the explicit `gameState` parameter; static helpers resolved via using-static
    // on ObjectUpdateBuilder (internal). Debug-log line dropped (no wire bytes).
    // ===========================================================================
    internal static void WriteUpdateActivePlayerData_HandPort(WorldPacket data, ActivePlayerData a, GameSessionData gameState)
    {
        Span<uint> blocksBuf = stackalloc uint[48];
        var blocks = new StackBitMask(blocksBuf);
        int knownTitlesCount = 0;
        ulong[] knownTitles64 = new ulong[6];
        if (a.KnownTitles != null)
        {
            bool hasAnyTitle = false;
            for (int i = 0; i < a.KnownTitles.Length; i++)
                if (a.KnownTitles[i].HasValue) { hasAnyTitle = true; break; }
            if (hasAnyTitle)
            {
                knownTitlesCount = 6;
                for (int i = 0; i < 6; i++)
                {
                    uint lo = (i * 2 < a.KnownTitles.Length && a.KnownTitles[i * 2].HasValue) ? a.KnownTitles[i * 2]!.Value : 0;
                    uint hi = (i * 2 + 1 < a.KnownTitles.Length && a.KnownTitles[i * 2 + 1].HasValue) ? a.KnownTitles[i * 2 + 1]!.Value : 0;
                    knownTitles64[i] = (ulong)lo | ((ulong)hi << 32);
                }
                blocks.SetBit(0); blocks.SetBit(3);
            }
        }

        bool hasGlyphChanges = gameState.ActiveGlyphsDirty;
        if (hasGlyphChanges)
            gameState.ActiveGlyphsDirty = false;

        if (a.FarsightObject != null) { blocks.SetBit(0); blocks.SetBit(26); }
        if (a.Coinage.HasValue) { blocks.SetBit(0); blocks.SetBit(28); }
        if (a.XP.HasValue) { blocks.SetBit(0); blocks.SetBit(29); }
        if (a.NextLevelXP.HasValue) { blocks.SetBit(0); blocks.SetBit(30); }
        if (a.TrialXP.HasValue) { blocks.SetBit(0); blocks.SetBit(31); }
        if (a.Skill != null && ObjectUpdateBuilder.HasAnySkillChanged(a.Skill)) { blocks.SetBit(0); blocks.SetBit(32); }
        if (a.CharacterPoints.HasValue) { blocks.SetBit(0); blocks.SetBit(33); }
        if (a.MaxTalentTiers.HasValue) { blocks.SetBit(0); blocks.SetBit(34); }
        if (a.TrackCreatureMask.HasValue) { blocks.SetBit(0); blocks.SetBit(35); }
        if (a.MainhandExpertise.HasValue) { blocks.SetBit(0); blocks.SetBit(36); }
        if (a.OffhandExpertise.HasValue) { blocks.SetBit(0); blocks.SetBit(37); }

        if (a.RangedExpertise.HasValue) { blocks.SetBit(38); blocks.SetBit(39); }
        if (a.CombatRatingExpertise.HasValue) { blocks.SetBit(38); blocks.SetBit(40); }
        if (a.BlockPercentage.HasValue) { blocks.SetBit(38); blocks.SetBit(41); }
        if (a.DodgePercentage.HasValue) { blocks.SetBit(38); blocks.SetBit(42); }
        if (a.DodgePercentageFromAttribute.HasValue) { blocks.SetBit(38); blocks.SetBit(43); }
        if (a.ParryPercentage.HasValue) { blocks.SetBit(38); blocks.SetBit(44); }
        if (a.ParryPercentageFromAttribute.HasValue) { blocks.SetBit(38); blocks.SetBit(45); }
        if (a.CritPercentage.HasValue) { blocks.SetBit(38); blocks.SetBit(46); }
        if (a.RangedCritPercentage.HasValue) { blocks.SetBit(38); blocks.SetBit(47); }
        if (a.OffhandCritPercentage.HasValue) { blocks.SetBit(38); blocks.SetBit(48); }
        if (a.ShieldBlock.HasValue) { blocks.SetBit(38); blocks.SetBit(49); }
        if (a.Mastery.HasValue) { blocks.SetBit(38); blocks.SetBit(51); }
        if (a.Speed.HasValue) { blocks.SetBit(38); blocks.SetBit(52); }
        if (a.Avoidance.HasValue) { blocks.SetBit(38); blocks.SetBit(53); }
        if (a.Sturdiness.HasValue) { blocks.SetBit(38); blocks.SetBit(54); }
        if (a.Versatility.HasValue) { blocks.SetBit(38); blocks.SetBit(55); }
        if (a.VersatilityBonus.HasValue) { blocks.SetBit(38); blocks.SetBit(56); }
        if (a.PvpPowerDamage.HasValue) { blocks.SetBit(38); blocks.SetBit(57); }
        if (a.PvpPowerHealing.HasValue) { blocks.SetBit(38); blocks.SetBit(58); }
        if (a.ModHealingDonePos.HasValue) { blocks.SetBit(38); blocks.SetBit(59); }
        if (a.ModHealingPercent.HasValue) { blocks.SetBit(38); blocks.SetBit(60); }
        if (a.ModHealingDonePercent.HasValue) { blocks.SetBit(38); blocks.SetBit(61); }
        if (a.ModPeriodicHealingDonePercent.HasValue) { blocks.SetBit(38); blocks.SetBit(62); }
        if (a.ModSpellPowerPercent.HasValue) { blocks.SetBit(38); blocks.SetBit(63); }
        if (a.ModResiliencePercent.HasValue) { blocks.SetBit(38); blocks.SetBit(64); }
        if (a.OverrideSpellPowerByAPPercent.HasValue) { blocks.SetBit(38); blocks.SetBit(65); }
        if (a.OverrideAPBySpellPowerPercent.HasValue) { blocks.SetBit(38); blocks.SetBit(66); }
        if (a.ModTargetResistance.HasValue) { blocks.SetBit(38); blocks.SetBit(67); }
        if (a.ModTargetPhysicalResistance.HasValue) { blocks.SetBit(38); blocks.SetBit(68); }
        if (a.LocalFlags.HasValue) { blocks.SetBit(38); blocks.SetBit(69); }

        if (a.GrantableLevels.HasValue) { blocks.SetBit(70); blocks.SetBit(71); }
        if (a.MultiActionBars.HasValue) { blocks.SetBit(70); blocks.SetBit(72); }
        if (a.LifetimeMaxRank.HasValue) { blocks.SetBit(70); blocks.SetBit(73); }
        if (a.NumRespecs.HasValue) { blocks.SetBit(70); blocks.SetBit(74); }
        if (a.AmmoID.HasValue) { blocks.SetBit(70); blocks.SetBit(75); }
        if (a.PvpMedals.HasValue) { blocks.SetBit(70); blocks.SetBit(76); }
        if (a.TodayHonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(77); }
        if (a.TodayDishonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(78); }
        if (a.YesterdayHonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(79); }
        if (a.YesterdayDishonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(80); }
        if (a.LastWeekHonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(81); }
        if (a.LastWeekDishonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(82); }
        if (a.ThisWeekHonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(83); }
        if (a.ThisWeekDishonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(84); }
        if (a.ThisWeekContribution.HasValue) { blocks.SetBit(70); blocks.SetBit(85); }
        if (a.LifetimeHonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(86); }
        if (a.LifetimeDishonorableKills.HasValue) { blocks.SetBit(70); blocks.SetBit(87); }
        if (a.YesterdayContribution.HasValue) { blocks.SetBit(70); blocks.SetBit(89); }
        if (a.LastWeekContribution.HasValue) { blocks.SetBit(70); blocks.SetBit(90); }
        if (a.LastWeekRank.HasValue) { blocks.SetBit(70); blocks.SetBit(91); }
        if (a.WatchedFactionIndex.HasValue) { blocks.SetBit(70); blocks.SetBit(92); }
        if (a.MaxLevel.HasValue) { blocks.SetBit(70); blocks.SetBit(93); }
        if (a.ScalingPlayerLevelDelta.HasValue) { blocks.SetBit(70); blocks.SetBit(94); }
        if (a.MaxCreatureScalingLevel.HasValue) { blocks.SetBit(70); blocks.SetBit(95); }
        if (a.PetSpellPower.HasValue) { blocks.SetBit(70); blocks.SetBit(96); }
        if (a.UiHitModifier.HasValue) { blocks.SetBit(70); blocks.SetBit(97); }
        if (a.UiSpellHitModifier.HasValue) { blocks.SetBit(70); blocks.SetBit(98); }
        if (a.HomeRealmTimeOffset.HasValue) { blocks.SetBit(70); blocks.SetBit(99); }
        if (a.ModPetHaste.HasValue) { blocks.SetBit(70); blocks.SetBit(100); }
        if (a.LocalRegenFlags.HasValue) { blocks.SetBit(70); blocks.SetBit(101); }

        if (a.AuraVision.HasValue) { blocks.SetBit(102); blocks.SetBit(103); }
        if (a.NumBackpackSlots.HasValue) { blocks.SetBit(102); blocks.SetBit(104); }
        if (a.OverrideSpellsID.HasValue) { blocks.SetBit(102); blocks.SetBit(105); }
        if (a.LfgBonusFactionID.HasValue) { blocks.SetBit(102); blocks.SetBit(106); }
        if (a.LootSpecID.HasValue) { blocks.SetBit(102); blocks.SetBit(107); }
        if (a.OverrideZonePVPType.HasValue) { blocks.SetBit(102); blocks.SetBit(108); }
        if (a.Honor.HasValue) { blocks.SetBit(102); blocks.SetBit(109); }
        if (a.HonorNextLevel.HasValue) { blocks.SetBit(102); blocks.SetBit(110); }
        if (a.PvPTierMaxFromWins.HasValue) { blocks.SetBit(102); blocks.SetBit(112); }
        if (a.PvPLastWeeksTierMaxFromWins.HasValue) { blocks.SetBit(102); blocks.SetBit(113); }
        if (a.PvPRankProgress.HasValue) { blocks.SetBit(102); blocks.SetBit(114); }
        if (a.GlyphsEnabled.HasValue) { blocks.SetBit(102); blocks.SetBit(120); }

        for (int i = 0; i < 141; i++)
        {
            if (ObjectUpdateBuilder.GetModernInvSlot(a, i) != null)
            {
                blocks.SetBit(124);
                blocks.SetBit(125 + i);
            }
        }

        if (a.TrackResourceMask != null)
            for (int i = 0; i < 2; i++)
                if (a.TrackResourceMask[i].HasValue) { blocks.SetBit(266); blocks.SetBit(267 + i); }

        if (a.SpellCritPercentage != null)
            for (int i = 0; i < 7; i++)
                if (a.SpellCritPercentage[i].HasValue) { blocks.SetBit(269); blocks.SetBit(270 + i); }
        if (a.ModDamageDonePos != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDonePos[i].HasValue) { blocks.SetBit(269); blocks.SetBit(277 + i); }
        if (a.ModDamageDoneNeg != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDoneNeg[i].HasValue) { blocks.SetBit(269); blocks.SetBit(284 + i); }
        if (a.ModDamageDonePercent != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDonePercent[i].HasValue) { blocks.SetBit(269); blocks.SetBit(291 + i); }

        if (a.ExploredZones != null)
            for (int i = 0; i < 240; i++)
                if (a.ExploredZones[i].HasValue) { blocks.SetBit(298); blocks.SetBit(299 + i); }

        if (a.RestInfo != null)
            for (int i = 0; i < 2; i++)
                if (a.RestInfo[i] != null && (a.RestInfo[i].Threshold.HasValue || a.RestInfo[i].StateID.HasValue))
                { blocks.SetBit(539); blocks.SetBit(540 + i); }

        if (a.WeaponDmgMultipliers != null)
            for (int i = 0; i < 3; i++)
                if (a.WeaponDmgMultipliers[i].HasValue) { blocks.SetBit(542); blocks.SetBit(543 + i); }
        if (a.WeaponAtkSpeedMultipliers != null)
            for (int i = 0; i < 3; i++)
                if (a.WeaponAtkSpeedMultipliers[i].HasValue) { blocks.SetBit(542); blocks.SetBit(546 + i); }

        if (a.BuybackPrice != null)
            for (int i = 0; i < 12; i++)
                if (a.BuybackPrice[i].HasValue) { blocks.SetBit(549); blocks.SetBit(550 + i); }
        if (a.BuybackTimestamp != null)
            for (int i = 0; i < 12; i++)
                if (a.BuybackTimestamp[i].HasValue) { blocks.SetBit(549); blocks.SetBit(562 + i); }

        if (a.CombatRatings != null)
            for (int i = 0; i < 32; i++)
                if (a.CombatRatings[i].HasValue) { blocks.SetBit(574); blocks.SetBit(575 + i); }

        if (a.NoReagentCostMask != null)
            for (int i = 0; i < 4; i++)
                if (a.NoReagentCostMask[i].HasValue) { blocks.SetBit(615); blocks.SetBit(616 + i); }

        if (a.ProfessionSkillLine != null)
            for (int i = 0; i < 2; i++)
                if (a.ProfessionSkillLine[i].HasValue) { blocks.SetBit(620); blocks.SetBit(621 + i); }

        if (a.BagSlotFlags != null)
            for (int i = 0; i < 4; i++)
                if (a.BagSlotFlags[i].HasValue) { blocks.SetBit(623); blocks.SetBit(624 + i); }

        if (a.BankBagSlotFlags != null)
            for (int i = 0; i < 7; i++)
                if (a.BankBagSlotFlags[i].HasValue) { blocks.SetBit(628); blocks.SetBit(629 + i); }

        if (a.QuestCompleted != null)
            for (int i = 0; i < 875; i++)
                if (a.QuestCompleted[i].HasValue) { blocks.SetBit(636); blocks.SetBit(637 + i); }

        if (a.PvpInfo != null)
            for (int i = 0; i < Math.Min(a.PvpInfo.Length, 7); i++)
                if (a.PvpInfo[i] != null && (a.PvpInfo[i].Rating != 0 || a.PvpInfo[i].SeasonPlayed != 0 || a.PvpInfo[i].Disqualified))
                { blocks.SetBit(607); blocks.SetBit(608 + i); }

        if (hasGlyphChanges)
        {
            blocks.SetBit(1512);
            for (int i = 0; i < PlayerConst.MaxGlyphSlots; i++)
            {
                blocks.SetBit(1513 + i);
                blocks.SetBit(1519 + i);
            }
        }

        uint blocksMask0 = 0;
        for (int b = 0; b < 32; b++)
            if (blocks[b] != 0) blocksMask0 |= (1u << b);
        uint blocksMask1 = 0;
        for (int b = 32; b < 48; b++)
            if (blocks[b] != 0) blocksMask1 |= (1u << (b - 32));

        data.WriteUInt32(blocksMask0);
        data.WriteBits(blocksMask1, 16);
        for (int b = 0; b < 48; b++)
        {
            bool blockSet = (b < 32) ? ((blocksMask0 & (1u << b)) != 0) : ((blocksMask1 & (1u << (b - 32))) != 0);
            if (blockSet)
                data.WriteBits(blocks[b], 32);
        }

        if (blocks.IsBitSet(3))
        {
            data.WriteBits((uint)knownTitlesCount, 32);
            for (int i = 0; i < knownTitlesCount; i++)
                data.WriteBit(true);
        }
        data.FlushBits();

        if (blocks.IsBitSet(3))
        {
            for (int i = 0; i < knownTitlesCount; i++)
                data.WriteUInt64(knownTitles64[i]);
        }

        if (blocks.IsBitSet(0))
        {
            if (blocks.IsBitSet(26)) data.WritePackedGuid128(a.FarsightObject!.Value);
            if (blocks.IsBitSet(28)) data.WriteUInt64(a.Coinage!.Value);
            if (blocks.IsBitSet(29)) data.WriteInt32(a.XP!.Value);
            if (blocks.IsBitSet(30)) data.WriteInt32(a.NextLevelXP!.Value);
            if (blocks.IsBitSet(31)) data.WriteInt32(a.TrialXP!.Value);
            if (blocks.IsBitSet(32)) ObjectUpdateBuilder.WriteUpdateSkillInfo(data, a.Skill!);
            if (blocks.IsBitSet(33)) data.WriteInt32(a.CharacterPoints!.Value);
            if (blocks.IsBitSet(34)) data.WriteInt32(a.MaxTalentTiers!.Value);
            if (blocks.IsBitSet(35)) data.WriteUInt32(a.TrackCreatureMask!.Value);
            if (blocks.IsBitSet(36)) data.WriteFloat(a.MainhandExpertise!.Value);
            if (blocks.IsBitSet(37)) data.WriteFloat(a.OffhandExpertise!.Value);
        }
        if (blocks.IsBitSet(38))
        {
            if (blocks.IsBitSet(39)) data.WriteFloat(a.RangedExpertise!.Value);
            if (blocks.IsBitSet(40)) data.WriteFloat(a.CombatRatingExpertise!.Value);
            if (blocks.IsBitSet(41)) data.WriteFloat(a.BlockPercentage!.Value);
            if (blocks.IsBitSet(42)) data.WriteFloat(a.DodgePercentage!.Value);
            if (blocks.IsBitSet(43)) data.WriteFloat(a.DodgePercentageFromAttribute!.Value);
            if (blocks.IsBitSet(44)) data.WriteFloat(a.ParryPercentage!.Value);
            if (blocks.IsBitSet(45)) data.WriteFloat(a.ParryPercentageFromAttribute!.Value);
            if (blocks.IsBitSet(46)) data.WriteFloat(a.CritPercentage!.Value);
            if (blocks.IsBitSet(47)) data.WriteFloat(a.RangedCritPercentage!.Value);
            if (blocks.IsBitSet(48)) data.WriteFloat(a.OffhandCritPercentage!.Value);
            if (blocks.IsBitSet(49)) data.WriteInt32(a.ShieldBlock!.Value);
            if (blocks.IsBitSet(51)) data.WriteFloat(a.Mastery!.Value);
            if (blocks.IsBitSet(52)) data.WriteFloat(a.Speed!.Value);
            if (blocks.IsBitSet(53)) data.WriteFloat(a.Avoidance!.Value);
            if (blocks.IsBitSet(54)) data.WriteFloat(a.Sturdiness!.Value);
            if (blocks.IsBitSet(55)) data.WriteInt32(a.Versatility!.Value);
            if (blocks.IsBitSet(56)) data.WriteFloat(a.VersatilityBonus!.Value);
            if (blocks.IsBitSet(57)) data.WriteFloat(a.PvpPowerDamage!.Value);
            if (blocks.IsBitSet(58)) data.WriteFloat(a.PvpPowerHealing!.Value);
            if (blocks.IsBitSet(59)) data.WriteInt32(a.ModHealingDonePos!.Value);
            if (blocks.IsBitSet(60)) data.WriteFloat(a.ModHealingPercent!.Value);
            if (blocks.IsBitSet(61)) data.WriteFloat(a.ModHealingDonePercent!.Value);
            if (blocks.IsBitSet(62)) data.WriteFloat(a.ModPeriodicHealingDonePercent!.Value);
            if (blocks.IsBitSet(63)) data.WriteFloat(a.ModSpellPowerPercent!.Value);
            if (blocks.IsBitSet(64)) data.WriteFloat(a.ModResiliencePercent!.Value);
            if (blocks.IsBitSet(65)) data.WriteFloat(a.OverrideSpellPowerByAPPercent!.Value);
            if (blocks.IsBitSet(66)) data.WriteFloat(a.OverrideAPBySpellPowerPercent!.Value);
            if (blocks.IsBitSet(67)) data.WriteInt32(a.ModTargetResistance!.Value);
            if (blocks.IsBitSet(68)) data.WriteInt32(a.ModTargetPhysicalResistance!.Value);
            if (blocks.IsBitSet(69)) data.WriteUInt32(a.LocalFlags!.Value);
        }
        if (blocks.IsBitSet(70))
        {
            if (blocks.IsBitSet(71)) data.WriteUInt8(a.GrantableLevels!.Value);
            if (blocks.IsBitSet(72)) data.WriteUInt8(a.MultiActionBars!.Value);
            if (blocks.IsBitSet(73)) data.WriteUInt8(a.LifetimeMaxRank!.Value);
            if (blocks.IsBitSet(74)) data.WriteUInt8(a.NumRespecs!.Value);
            if (blocks.IsBitSet(75)) data.WriteInt32((int)a.AmmoID!.Value);
            if (blocks.IsBitSet(76)) data.WriteUInt32(a.PvpMedals!.Value);
            if (blocks.IsBitSet(77)) data.WriteUInt16(a.TodayHonorableKills!.Value);
            if (blocks.IsBitSet(78)) data.WriteUInt16(a.TodayDishonorableKills!.Value);
            if (blocks.IsBitSet(79)) data.WriteUInt16(a.YesterdayHonorableKills!.Value);
            if (blocks.IsBitSet(80)) data.WriteUInt16(a.YesterdayDishonorableKills!.Value);
            if (blocks.IsBitSet(81)) data.WriteUInt16(a.LastWeekHonorableKills!.Value);
            if (blocks.IsBitSet(82)) data.WriteUInt16(a.LastWeekDishonorableKills!.Value);
            if (blocks.IsBitSet(83)) data.WriteUInt16(a.ThisWeekHonorableKills!.Value);
            if (blocks.IsBitSet(84)) data.WriteUInt16(a.ThisWeekDishonorableKills!.Value);
            if (blocks.IsBitSet(85)) data.WriteUInt32(a.ThisWeekContribution!.Value);
            if (blocks.IsBitSet(86)) data.WriteUInt32(a.LifetimeHonorableKills!.Value);
            if (blocks.IsBitSet(87)) data.WriteUInt32(a.LifetimeDishonorableKills!.Value);
            if (blocks.IsBitSet(89)) data.WriteUInt32(a.YesterdayContribution!.Value);
            if (blocks.IsBitSet(90)) data.WriteUInt32(a.LastWeekContribution!.Value);
            if (blocks.IsBitSet(91)) data.WriteUInt32(a.LastWeekRank!.Value);
            if (blocks.IsBitSet(92)) data.WriteInt32(a.WatchedFactionIndex!.Value);
            if (blocks.IsBitSet(93)) data.WriteInt32(a.MaxLevel!.Value);
            if (blocks.IsBitSet(94)) data.WriteInt32(a.ScalingPlayerLevelDelta!.Value);
            if (blocks.IsBitSet(95)) data.WriteInt32(a.MaxCreatureScalingLevel!.Value);
            if (blocks.IsBitSet(96)) data.WriteInt32(a.PetSpellPower!.Value);
            if (blocks.IsBitSet(97)) data.WriteFloat(a.UiHitModifier!.Value);
            if (blocks.IsBitSet(98)) data.WriteFloat(a.UiSpellHitModifier!.Value);
            if (blocks.IsBitSet(99)) data.WriteInt32(a.HomeRealmTimeOffset!.Value);
            if (blocks.IsBitSet(100)) data.WriteFloat(a.ModPetHaste!.Value);
            if (blocks.IsBitSet(101)) data.WriteUInt8(a.LocalRegenFlags!.Value);
        }
        if (blocks.IsBitSet(102))
        {
            if (blocks.IsBitSet(103)) data.WriteUInt8(a.AuraVision!.Value);
            if (blocks.IsBitSet(104)) data.WriteUInt8(a.NumBackpackSlots!.Value);
            if (blocks.IsBitSet(105)) data.WriteInt32(a.OverrideSpellsID!.Value);
            if (blocks.IsBitSet(106)) data.WriteInt32(a.LfgBonusFactionID!.Value);
            if (blocks.IsBitSet(107)) data.WriteUInt16((ushort)a.LootSpecID!.Value);
            if (blocks.IsBitSet(108)) data.WriteUInt32(a.OverrideZonePVPType!.Value);
            if (blocks.IsBitSet(109)) data.WriteInt32(a.Honor!.Value);
            if (blocks.IsBitSet(110)) data.WriteInt32(a.HonorNextLevel!.Value);
            if (blocks.IsBitSet(112)) data.WriteInt32((int)a.PvPTierMaxFromWins!.Value);
            if (blocks.IsBitSet(113)) data.WriteInt32((int)a.PvPLastWeeksTierMaxFromWins!.Value);
            if (blocks.IsBitSet(114)) data.WriteUInt8(a.PvPRankProgress!.Value);
        }
        if (blocks.IsBitSet(120)) data.WriteUInt8(a.GlyphsEnabled!.Value);
        data.FlushBits();

        if (blocks.IsBitSet(124))
        {
            for (int i = 0; i < 141; i++)
                if (blocks.IsBitSet(125 + i))
                {
                    WowGuid128 guid = ObjectUpdateBuilder.GetModernInvSlot(a, i) ?? WowGuid128.Empty;
                    data.WritePackedGuid128(guid);
                }
        }
        if (blocks.IsBitSet(266))
        {
            for (int i = 0; i < 2; i++)
                if (blocks.IsBitSet(267 + i))
                    data.WriteUInt32(a.TrackResourceMask![i]!.Value);
        }
        if (blocks.IsBitSet(269))
        {
            for (int i = 0; i < 7; i++)
                if (blocks.IsBitSet(270 + i)) data.WriteFloat(a.SpellCritPercentage![i]!.Value);
            for (int i = 0; i < 7; i++)
                if (blocks.IsBitSet(277 + i)) data.WriteInt32(a.ModDamageDonePos![i]!.Value);
            for (int i = 0; i < 7; i++)
                if (blocks.IsBitSet(284 + i)) data.WriteInt32(a.ModDamageDoneNeg![i]!.Value);
            for (int i = 0; i < 7; i++)
                if (blocks.IsBitSet(291 + i)) data.WriteFloat(a.ModDamageDonePercent![i]!.Value);
        }
        if (blocks.IsBitSet(298))
        {
            for (int i = 0; i < 240; i++)
                if (blocks.IsBitSet(299 + i)) data.WriteUInt64(a.ExploredZones![i]!.Value);
        }
        if (blocks.IsBitSet(539))
        {
            for (int i = 0; i < 2; i++)
            {
                if (blocks.IsBitSet(540 + i))
                {
                    var ri = a.RestInfo![i];
                    uint restMask = 0;
                    if (ri != null && ri.Threshold.HasValue) restMask |= 2;
                    if (ri != null && ri.StateID.HasValue) restMask |= 4;
                    if (restMask != 0) restMask |= 1;
                    data.WriteBits(restMask, 3);
                    data.FlushBits();
                    if ((restMask & 2) != 0) data.WriteUInt32(ri!.Threshold!.Value);
                    if ((restMask & 4) != 0) data.WriteUInt8((byte)ri!.StateID!.Value);
                }
            }
        }
        if (blocks.IsBitSet(542))
        {
            for (int i = 0; i < 3; i++)
                if (blocks.IsBitSet(543 + i)) data.WriteFloat(a.WeaponDmgMultipliers![i]!.Value);
            for (int i = 0; i < 3; i++)
                if (blocks.IsBitSet(546 + i)) data.WriteFloat(a.WeaponAtkSpeedMultipliers![i]!.Value);
        }
        if (blocks.IsBitSet(549))
        {
            for (int i = 0; i < 12; i++)
                if (blocks.IsBitSet(550 + i)) data.WriteUInt32(a.BuybackPrice![i]!.Value);
            for (int i = 0; i < 12; i++)
                if (blocks.IsBitSet(562 + i)) data.WriteInt64((long)a.BuybackTimestamp![i]!.Value);
        }
        if (blocks.IsBitSet(574))
        {
            for (int i = 0; i < 32; i++)
                if (blocks.IsBitSet(575 + i)) data.WriteInt32(a.CombatRatings![i]!.Value);
        }
        if (blocks.IsBitSet(615))
        {
            for (int i = 0; i < 4; i++)
                if (blocks.IsBitSet(616 + i)) data.WriteUInt32(a.NoReagentCostMask![i]!.Value);
        }
        if (blocks.IsBitSet(620))
        {
            for (int i = 0; i < 2; i++)
                if (blocks.IsBitSet(621 + i)) data.WriteInt32(a.ProfessionSkillLine![i]!.Value);
        }
        if (blocks.IsBitSet(623))
        {
            for (int i = 0; i < 4; i++)
                if (blocks.IsBitSet(624 + i)) data.WriteUInt32(a.BagSlotFlags![i]!.Value);
        }
        if (blocks.IsBitSet(628))
        {
            for (int i = 0; i < 7; i++)
                if (blocks.IsBitSet(629 + i)) data.WriteUInt32(a.BankBagSlotFlags![i]!.Value);
        }
        if (blocks.IsBitSet(636))
        {
            for (int i = 0; i < 875; i++)
                if (blocks.IsBitSet(637 + i)) data.WriteUInt64(a.QuestCompleted![i]!.Value);
        }
        if (blocks.IsBitSet(1512))
        {
            for (int i = 0; i < PlayerConst.MaxGlyphSlots; i++)
                if (blocks.IsBitSet(1513 + i)) data.WriteUInt32(gameState.ActiveGlyphSlotIds[i]);
            for (int i = 0; i < PlayerConst.MaxGlyphSlots; i++)
                if (blocks.IsBitSet(1519 + i)) data.WriteUInt32((uint)(gameState.ActiveGlyphs[i]));
        }
        if (blocks.IsBitSet(607))
        {
            for (int i = 0; i < 7; i++)
            {
                if (blocks.IsBitSet(608 + i))
                {
                    PVPInfo? pi = (a.PvpInfo != null && i < a.PvpInfo.Length) ? a.PvpInfo[i] : null;
                    uint pvpMask = 0;
                    if (pi != null)
                    {
                        if (pi.Disqualified) pvpMask |= (1u << 1);
                        if (pi.WeeklyPlayed != 0) pvpMask |= (1u << 4);
                        if (pi.WeeklyWon != 0) pvpMask |= (1u << 5);
                        if (pi.SeasonPlayed != 0) pvpMask |= (1u << 6);
                        if (pi.SeasonWon != 0) pvpMask |= (1u << 7);
                        if (pi.Rating != 0) pvpMask |= (1u << 8);
                        if (pi.WeeklyBestRating != 0) pvpMask |= (1u << 9);
                        if (pi.SeasonBestRating != 0) pvpMask |= (1u << 10);
                        if (pi.PvpTierID != 0) pvpMask |= (1u << 11);
                        if (pi.WeeklyBestWinPvpTierID != 0) pvpMask |= (1u << 12);
                        if (pi.Field_28 != 0) pvpMask |= (1u << 13);
                        if (pi.Field_2C != 0) pvpMask |= (1u << 14);
                    }
                    if (pvpMask != 0) pvpMask |= 1;
                    data.WriteBits(pvpMask, 19);
                    if ((pvpMask & (1u << 1)) != 0) data.WriteBit(pi!.Disqualified);
                    data.FlushBits();
                    if ((pvpMask & 1) != 0)
                    {
                        if ((pvpMask & (1u << 4)) != 0) data.WriteUInt32(pi!.WeeklyPlayed);
                        if ((pvpMask & (1u << 5)) != 0) data.WriteUInt32(pi!.WeeklyWon);
                        if ((pvpMask & (1u << 6)) != 0) data.WriteUInt32(pi!.SeasonPlayed);
                        if ((pvpMask & (1u << 7)) != 0) data.WriteUInt32(pi!.SeasonWon);
                        if ((pvpMask & (1u << 8)) != 0) data.WriteUInt32(pi!.Rating);
                        if ((pvpMask & (1u << 9)) != 0) data.WriteUInt32(pi!.WeeklyBestRating);
                        if ((pvpMask & (1u << 10)) != 0) data.WriteUInt32(pi!.SeasonBestRating);
                        if ((pvpMask & (1u << 11)) != 0) data.WriteUInt32(pi!.PvpTierID);
                        if ((pvpMask & (1u << 12)) != 0) data.WriteUInt32(pi!.WeeklyBestWinPvpTierID);
                        if ((pvpMask & (1u << 13)) != 0) data.WriteUInt32(pi!.Field_28);
                        if ((pvpMask & (1u << 14)) != 0) data.WriteUInt32(pi!.Field_2C);
                    }
                }
            }
        }

        data.FlushBits();
    }
}
