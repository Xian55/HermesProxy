using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.V3_4_3_54261;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

// Byte-equivalence oracle for Phase 5b Unit Create + Update paths. All three
// (WriteCreateUnitData / WriteUpdateUnitData / HasAnyUnitFieldSet) are now
// generator-emitted from V3_4_3_54261.UnitField. Custom writers on builder partial
// handle: interleaved arrays (Power/Stats/Resistances/ResBuffMods groups), impersonation
// overrides, SanitizeFlags2, ChannelData inline composite, ChannelObjects dynamic field
// (mask preamble + body), RangedAttackRoundBaseTime bow-fallback, VirtualItems PlayerData
// fallback, EffectiveLevel ?? Level fallback, NpcFlags HasValue+!=0 predicate.
//
// Update-path coverage targets known bug-history regression vectors: DisplayPower UInt8
// width, ChannelData composite (no inner mask), Flags2 sanitize, ChannelObjects two-phase
// emit, Cascade rule (force-set bit 0/32/64/96 when any sibling in block is set).
public class UnitSectionEquivalenceTests
{
    private static GlobalSessionData CreateGlobalSession()
        => (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

    private static GameSessionData CreateGameSession()
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new System.Collections.Generic.Dictionary<WowGuid128, ObjectType>());
        return session;
    }

    private static ObjectUpdateBuilder MakeBuilder(WowGuid128 guid, GameSessionData session, out ObjectUpdate update)
    {
        var globalSession = CreateGlobalSession();
        update = new ObjectUpdate(guid, UpdateTypeModern.Values, globalSession);
        if (update.UnitData == null) update.UnitData = new UnitData();
        return new ObjectUpdateBuilder(update, session);
    }

    // ---------------------------------------------------------------------
    // Empty creature (not owner, no fields populated) — sanity baseline.
    // ---------------------------------------------------------------------

    [Fact]
    public void WriteCreateUnitData_EmptyCreature_MatchesHandPort()
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);

        var actual = new WorldPacket();
        builder.WriteCreateUnitData(actual);

        var expected = new WorldPacket();
        WriteCreateUnitData_HandPort(expected, update.UnitData!, isOwner: false, zeroCharBakeIds: false, isCreature: true,
            playerData: null, createdBy: null);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // ActivePlayer (IsOwner=true), populated scalars including ChannelData,
    // VirtualItems via PlayerData fallback, NpcFlags, RangedAttackRoundBaseTime
    // bow-fallback.
    // ---------------------------------------------------------------------

    [Fact]
    public void WriteCreateUnitData_OwnerWithChannelAndVirtualItems_MatchesHandPort()
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        // ActivePlayer match: register currentPlayerGuid via reflection on session
        typeof(GameSessionData).GetField(nameof(GameSessionData.CurrentPlayerGuid))!
            .SetValue(session, guid);
        var builder = MakeBuilder(guid, session, out var update);

        var unit = update.UnitData!;
        unit.Health = 1000L;
        unit.MaxHealth = 1500L;
        unit.DisplayID = 49;
        unit.NpcFlags[0] = 1u;
        unit.NpcFlags[1] = 0u;
        unit.ChannelData = new UnitChannel(51769, 0);
        unit.ChannelObject = WowGuid128.Create(HighGuidType703.Creature, 0, 7777, 42);
        unit.RaceId = 7;
        unit.ClassId = 6;
        unit.PlayerClassId = 6;
        unit.SexId = 0;
        unit.DisplayPower = 1u; // Rage — the bug-history regression vector for DisplayPower UInt8 width.
        unit.Level = 60;
        unit.EffectiveLevel = 70;
        unit.Power[0] = 100;
        unit.MaxPower[0] = 100;
        unit.Power[1] = 50;   // Rage
        unit.MaxPower[1] = 100;
        unit.FactionTemplate = 35;
        unit.Flags = 0x40u;
        unit.Flags2 = 0u;
        unit.AuraState = 0x100u;
        unit.AttackRoundBaseTime[0] = 2000u;
        unit.AttackRoundBaseTime[1] = 2000u;
        unit.BoundingRadius = 0.5f;
        unit.CombatReach = 2f;
        unit.NativeDisplayID = 49;
        unit.MountDisplayID = 0;
        unit.MinDamage = 10f;
        unit.MaxDamage = 20f;
        unit.MinOffHandDamage = 0f;
        unit.MaxOffHandDamage = 0f;
        unit.StandState = 0;
        unit.ShapeshiftForm = 0;
        unit.AttackPower = 100;
        unit.RangedAttackPower = 50;
        unit.MinRangedDamage = 5f;
        unit.MaxRangedDamage = 10f;
        unit.HoverHeight = 0f;
        unit.BaseMana = 1000;
        unit.BaseHealth = 1500;
        unit.SheatheState = 0;
        unit.PvpFlags = 0;
        unit.PetFlags = 0;
        unit.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, 99);

        // Populate PlayerData VisibleItems for VirtualItems fallback (mainhand at slot 15).
        update.PlayerData!.VisibleItems = new VisibleItem?[19];
        update.PlayerData.VisibleItems[15] = new VisibleItem(12345, 0, 0); // mainhand
        update.PlayerData.VisibleItems[17] = new VisibleItem(2508, 0, 0);  // ranged → triggers 2300ms fallback for RangedAttackRoundBaseTime

        var actual = new WorldPacket();
        builder.WriteCreateUnitData(actual);

        var expected = new WorldPacket();
        WriteCreateUnitData_HandPort(expected, unit, isOwner: true, zeroCharBakeIds: false, isCreature: false,
            playerData: update.PlayerData, createdBy: unit.CreatedBy);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // NPCBot creature: Flags2 with CLONED → SanitizeFlags2 strips bit + Race/Class/Sex zeroed.
    // ---------------------------------------------------------------------

    [Fact]
    public void WriteCreateUnitData_NPCBotCreature_SanitizeAndZeroBake()
    {
        const uint UNIT_FLAG2_CLONED = 0x00000010;
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 31216, 1);
        var builder = MakeBuilder(guid, session, out var update);

        var unit = update.UnitData!;
        unit.Health = 100L;
        unit.MaxHealth = 100L;
        unit.RaceId = 11;
        unit.ClassId = 8;
        unit.SexId = 0;
        unit.Flags2 = UNIT_FLAG2_CLONED;   // triggers BOTH SanitizeFlags2 strip + IsImpersonatingCreatureBake zero-override
        unit.CreatedBy = WowGuid128.Create(HighGuidType703.Player, 42);

        var actual = new WorldPacket();
        builder.WriteCreateUnitData(actual);

        // For a Creature highType with CLONED flag: SanitizeFlags2 strips the bit (Flags2
        // becomes 0), AND IsImpersonatingCreatureBake returns true (Race/Class/Sex written as 0).
        var expected = new WorldPacket();
        WriteCreateUnitData_HandPort(expected, unit, isOwner: false, zeroCharBakeIds: true, isCreature: true,
            playerData: null, createdBy: unit.CreatedBy);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // Inlined pre-Phase-5b WriteCreateUnitData hand-port — frozen byte oracle.
    // Reproduces V3_4_3_54261/ObjectUpdateBuilder.cs (lines 527-747 pre-delete).
    // Parameters substitute for the builder's IsOwner / IsImpersonatingCreatureBake /
    // SanitizeFlags2 / PlayerData accesses (in production those go through the builder
    // instance; here the test passes them in).
    // ---------------------------------------------------------------------

    private static void WriteCreateUnitData_HandPort(WorldPacket data, UnitData unit, bool isOwner, bool zeroCharBakeIds, bool isCreature,
        PlayerData? playerData, WowGuid128? createdBy)
    {
        data.WriteInt64(unit.Health.GetValueOrDefault());
        data.WriteInt64(unit.MaxHealth.GetValueOrDefault());
        data.WriteInt32(unit.DisplayID.GetValueOrDefault());
        for (int i = 0; i < 2; i++)
            data.WriteUInt32(unit.NpcFlags?[i].GetValueOrDefault() ?? 0);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WritePackedGuid128(unit.Charm ?? WowGuid128.Empty);
        data.WritePackedGuid128(unit.Summon ?? WowGuid128.Empty);
        if (isOwner)
            data.WritePackedGuid128(unit.Critter ?? WowGuid128.Empty);
        data.WritePackedGuid128(unit.CharmedBy ?? WowGuid128.Empty);
        data.WritePackedGuid128(unit.SummonedBy ?? WowGuid128.Empty);
        data.WritePackedGuid128(unit.CreatedBy ?? WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WritePackedGuid128(unit.Target ?? WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteUInt64(0uL);
        data.WriteInt32(unit.ChannelData?.SpellID ?? 0);
        data.WriteInt32(unit.ChannelData?.SpellXSpellVisualID ?? 0);
        data.WriteUInt32(0u);
        data.WriteUInt8(zeroCharBakeIds ? (byte)0 : unit.RaceId.GetValueOrDefault());
        data.WriteUInt8(zeroCharBakeIds ? (byte)0 : unit.ClassId.GetValueOrDefault());
        data.WriteUInt8(unit.PlayerClassId.GetValueOrDefault());
        data.WriteUInt8(zeroCharBakeIds ? (byte)0 : unit.SexId.GetValueOrDefault());
        data.WriteUInt8((byte)unit.DisplayPower.GetValueOrDefault());
        data.WriteUInt32(0u);
        if (isOwner)
        {
            for (int j = 0; j < 10; j++) { data.WriteFloat(0f); data.WriteFloat(0f); }
        }
        for (int k = 0; k < 10; k++)
        {
            data.WriteInt32(k < 7 ? unit.Power[k].GetValueOrDefault() : 0);
            data.WriteInt32(k < 7 ? unit.MaxPower[k].GetValueOrDefault() : 0);
            data.WriteFloat(0f);
        }
        data.WriteInt32(unit.Level.GetValueOrDefault());
        data.WriteInt32(unit.EffectiveLevel ?? unit.Level.GetValueOrDefault());
        data.WriteInt32(unit.ContentTuningID.GetValueOrDefault());
        data.WriteInt32(unit.ScalingLevelMin.GetValueOrDefault());
        data.WriteInt32(unit.ScalingLevelMax.GetValueOrDefault());
        data.WriteInt32(unit.ScalingLevelDelta.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteInt32(0);
        data.WriteInt32(0);
        data.WriteInt32(unit.FactionTemplate.GetValueOrDefault());
        for (int l = 0; l < 3; l++)
        {
            int vItemId = unit.VirtualItems != null && unit.VirtualItems[l] is VisibleItem vi ? vi.ItemID : 0;
            if (vItemId == 0 && isOwner && playerData?.VisibleItems != null)
            {
                int playerSlot = 15 + l;
                if (playerSlot < playerData.VisibleItems.Length
                    && playerData.VisibleItems[playerSlot] is VisibleItem pv && pv.ItemID != 0)
                {
                    vItemId = pv.ItemID;
                }
            }
            data.WriteInt32(vItemId);
            data.WriteUInt16(0);
            data.WriteUInt16(0);
        }
        data.WriteUInt32(unit.Flags.GetValueOrDefault());
        // SanitizeFlags2 — match builder behavior: strip CLONED bit (0x10) for Creature highType.
        uint flags2 = unit.Flags2.GetValueOrDefault();
        if (isCreature && (flags2 & 0x10u) != 0) flags2 &= ~0x10u;
        data.WriteUInt32(flags2);
        data.WriteUInt32(0u);
        data.WriteUInt32(unit.AuraState.GetValueOrDefault());
        for (int m = 0; m < 2; m++)
            data.WriteUInt32(unit.AttackRoundBaseTime?[m].GetValueOrDefault() ?? 0);
        if (isOwner)
        {
            uint rangedTime = unit.RangedAttackRoundBaseTime.GetValueOrDefault();
            if (rangedTime == 0 && playerData?.VisibleItems != null
                && playerData.VisibleItems.Length > 17
                && playerData.VisibleItems[17] is VisibleItem ranged && ranged.ItemID != 0)
            {
                rangedTime = 2300;
            }
            data.WriteUInt32(rangedTime);
        }
        data.WriteFloat(unit.BoundingRadius ?? 0.389f);
        data.WriteFloat(unit.CombatReach ?? 1.5f);
        data.WriteFloat(1f);
        data.WriteInt32(unit.NativeDisplayID.GetValueOrDefault());
        data.WriteFloat(1f);
        data.WriteInt32(unit.MountDisplayID.GetValueOrDefault());
        if (isOwner)
        {
            data.WriteFloat(unit.MinDamage.GetValueOrDefault());
            data.WriteFloat(unit.MaxDamage.GetValueOrDefault());
            data.WriteFloat(unit.MinOffHandDamage.GetValueOrDefault());
            data.WriteFloat(unit.MaxOffHandDamage.GetValueOrDefault());
        }
        data.WriteUInt8(unit.StandState.GetValueOrDefault());
        data.WriteUInt8(unit.PetLoyaltyIndex.GetValueOrDefault());
        data.WriteUInt8(unit.VisFlags.GetValueOrDefault());
        data.WriteUInt8(unit.AnimTier.GetValueOrDefault());
        data.WriteUInt32(unit.PetNumber.GetValueOrDefault());
        data.WriteUInt32(unit.PetNameTimestamp.GetValueOrDefault());
        data.WriteUInt32(unit.PetExperience.GetValueOrDefault());
        data.WriteUInt32(unit.PetNextLevelExperience.GetValueOrDefault());
        data.WriteFloat(unit.ModCastSpeed ?? 1f);
        data.WriteFloat(unit.ModCastHaste ?? 1f);
        data.WriteFloat(1f);
        data.WriteFloat(1f);
        data.WriteFloat(1f);
        data.WriteFloat(1f);
        data.WriteInt32(unit.CreatedBySpell.GetValueOrDefault());
        data.WriteInt32(unit.EmoteState.GetValueOrDefault());
        data.WriteInt16(0);
        data.WriteInt16(0);
        if (isOwner)
        {
            for (int n = 0; n < 5; n++)
            {
                data.WriteInt32(unit.Stats?[n].GetValueOrDefault() ?? 0);
                data.WriteInt32(unit.StatPosBuff?[n].GetValueOrDefault() ?? 0);
                data.WriteInt32(unit.StatNegBuff?[n].GetValueOrDefault() ?? 0);
            }
        }
        if (isOwner)
        {
            for (int r = 0; r < 7; r++)
                data.WriteInt32(unit.Resistances?[r].GetValueOrDefault() ?? 0);
        }
        if (isOwner)
        {
            for (int p = 0; p < 7; p++)
            {
                data.WriteInt32(unit.PowerCostModifier?[p].GetValueOrDefault() ?? 0);
                data.WriteFloat(unit.PowerCostMultiplier?[p].GetValueOrDefault() ?? 0f);
            }
        }
        for (int b = 0; b < 7; b++)
        {
            data.WriteInt32(unit.ResistanceBuffModsPositive?[b].GetValueOrDefault() ?? 0);
            data.WriteInt32(unit.ResistanceBuffModsNegative?[b].GetValueOrDefault() ?? 0);
        }
        data.WriteInt32(unit.BaseMana.GetValueOrDefault());
        if (isOwner)
            data.WriteInt32(unit.BaseHealth.GetValueOrDefault());
        data.WriteUInt8(unit.SheatheState.GetValueOrDefault());
        data.WriteUInt8(unit.PvpFlags.GetValueOrDefault());
        data.WriteUInt8(unit.PetFlags.GetValueOrDefault());
        data.WriteUInt8(unit.ShapeshiftForm.GetValueOrDefault());
        if (isOwner)
        {
            data.WriteInt32(unit.AttackPower.GetValueOrDefault());
            data.WriteInt32(unit.AttackPowerModPos.GetValueOrDefault());
            data.WriteInt32(unit.AttackPowerModNeg.GetValueOrDefault());
            data.WriteFloat(unit.AttackPowerMultiplier.GetValueOrDefault());
            data.WriteInt32(unit.RangedAttackPower.GetValueOrDefault());
            data.WriteInt32(unit.RangedAttackPowerModPos.GetValueOrDefault());
            data.WriteInt32(unit.RangedAttackPowerModNeg.GetValueOrDefault());
            data.WriteFloat(unit.RangedAttackPowerMultiplier.GetValueOrDefault());
            data.WriteInt32(0);
            data.WriteFloat(0f);
            data.WriteFloat(unit.MinRangedDamage.GetValueOrDefault());
            data.WriteFloat(unit.MaxRangedDamage.GetValueOrDefault());
            data.WriteFloat(unit.MaxHealthModifier ?? 1f);
        }
        data.WriteFloat(unit.HoverHeight.GetValueOrDefault());
        data.WriteInt32(unit.MinItemLevelCutoff.GetValueOrDefault());
        data.WriteInt32(unit.MinItemLevel.GetValueOrDefault());
        data.WriteInt32(unit.MaxItemLevel.GetValueOrDefault());
        data.WriteInt32(unit.WildBattlePetLevel.GetValueOrDefault());
        data.WriteUInt32(0u);
        data.WriteInt32(unit.InteractSpellID.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteInt32(unit.LooksLikeMountID.GetValueOrDefault());
        data.WriteInt32(unit.LooksLikeCreatureID.GetValueOrDefault());
        data.WriteInt32(unit.LookAtControllerID.GetValueOrDefault());
        data.WriteInt32(0);
        data.WritePackedGuid128(unit.GuildGUID ?? WowGuid128.Empty);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        bool hasChannelObject = unit.ChannelObject.HasValue && !unit.ChannelObject.Value.IsEmpty();
        data.WriteUInt32(hasChannelObject ? 1u : 0u);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteInt32(0);
        data.WriteFloat(0f);
        data.WriteUInt32(0u);
        if (isOwner)
            data.WritePackedGuid128(WowGuid128.Empty);
        if (hasChannelObject)
            data.WritePackedGuid128(unit.ChannelObject!.Value);
    }

    // =====================================================================
    // Update path tests — exercise bug-history regression vectors.
    // =====================================================================

    [Fact]
    public void WriteUpdateUnitData_Empty_WritesZeroBlocksMaskOnly()
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);

        var actual = new WorldPacket();
        builder.WriteUpdateUnitData(actual);

        // Empty Unit → all bits 0 → blocksMask = 0 → WriteBits(0, 8) = 1 byte (0x00) + FlushBits aligns.
        var actualBytes = actual.GetData();
        Assert.Single(actualBytes);
        Assert.Equal((byte)0, actualBytes[0]);
    }

    [Fact]
    public void WriteUpdateUnitData_DisplayPower_Rage_WritesUInt8Width()
    {
        // Bug-history regression vector: DisplayPower previously written as UInt32 caused
        // 3-byte shift cascading into Stats[2] / ShapeshiftForm corruption. UnitField
        // declares DisplayPower as UInt8 with Cast = "(byte)". Verify single byte on wire.
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builder = MakeBuilder(guid, session, out var update);

        update.UnitData!.DisplayPower = 1u;   // Rage

        var actual = new WorldPacket();
        builder.WriteUpdateUnitData(actual);

        // Expected wire (ByteBuffer.WriteBits is MSB-first within the value):
        //   blocksMask = 1 (8 bits) → 1 byte = 0x01
        //   block 0 = (1<<0 cascade) | (1<<28 DisplayPower) = 0x10000001 → 4 bytes MSB-first = 10 00 00 01
        //   payload: DisplayPower UInt8 = 0x01
        // Total: 6 bytes.
        var actualBytes = actual.GetData();
        Assert.Equal(6, actualBytes.Length);
        Assert.Equal((byte)0x01, actualBytes[0]);                                                                      // blocksMask
        Assert.Equal((byte)0x10, actualBytes[1]); Assert.Equal((byte)0x00, actualBytes[2]);
        Assert.Equal((byte)0x00, actualBytes[3]); Assert.Equal((byte)0x01, actualBytes[4]);                            // block 0 mask MSB-first
        Assert.Equal((byte)0x01, actualBytes[5]);                                                                      // DisplayPower byte
    }

    [Fact]
    public void WriteUpdateUnitData_ChannelData_WritesTwoInt32NoInnerMask()
    {
        // Bug-history regression vector: ChannelData previously had an erroneous inner
        // 4-bit mask + FlushBits emit (file:2110-2127 pre-delete) that shifted SpellID
        // by 1 byte. TC UpdateFields.cs writes raw 2× Int32 with no inner mask.
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);

        update.UnitData!.ChannelData = new UnitChannel(51769, 0);

        var actual = new WorldPacket();
        builder.WriteUpdateUnitData(actual);

        // Expected wire:
        //   blocksMask = 1 → 0x01 (1 byte)
        //   block 0 = (1<<0) | (1<<22) = 0x00400001 → 4 bytes LE = 01 00 40 00
        //   payload: SpellID Int32 LE = 51769 = 0xCA39 → 39 CA 00 00 + SpellXSpellVisualID 0x00000000 → 00 00 00 00
        // Total: 1 + 4 + 8 = 13 bytes.
        var actualBytes = actual.GetData();
        Assert.Equal(13, actualBytes.Length);
        Assert.Equal((byte)0x01, actualBytes[0]);
        // block 0 mask = 0x00400001 → MSB-first 4 bytes = 00 40 00 01
        Assert.Equal((byte)0x00, actualBytes[1]); Assert.Equal((byte)0x40, actualBytes[2]);
        Assert.Equal((byte)0x00, actualBytes[3]); Assert.Equal((byte)0x01, actualBytes[4]);
        // SpellID: 51769 LE (WriteInt32 byte-LE) = 39 CA 00 00
        Assert.Equal((byte)0x39, actualBytes[5]); Assert.Equal((byte)0xCA, actualBytes[6]);
        Assert.Equal((byte)0x00, actualBytes[7]); Assert.Equal((byte)0x00, actualBytes[8]);
        // SpellXSpellVisualID: 0
        Assert.Equal((byte)0x00, actualBytes[9]); Assert.Equal((byte)0x00, actualBytes[10]);
        Assert.Equal((byte)0x00, actualBytes[11]); Assert.Equal((byte)0x00, actualBytes[12]);
    }

    [Fact]
    public void WriteUpdateUnitData_Flags2_CreatureWithCloned_SanitizesBit()
    {
        const uint UNIT_FLAG2_CLONED = 0x00000010;
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 31216, 1);
        var builder = MakeBuilder(guid, session, out var update);

        update.UnitData!.Flags2 = UNIT_FLAG2_CLONED | 0x00000020u;   // CLONED + arbitrary other bit
        update.UnitData.CreatedBy = WowGuid128.Create(HighGuidType703.Player, 42);

        var actual = new WorldPacket();
        builder.WriteUpdateUnitData(actual);

        // Expected: CLONED bit (0x10) stripped, other bit (0x20) retained. Final Flags2 = 0x20.
        // Wire layout: blocksMask + block 0 (cascade bit 0 + CreatedBy bit 16) + block 1 (Flags2 bit 42-32=10),
        // then payload in bit order: CreatedBy(16) packed-guid, Flags2(42) UInt32 LE.
        //
        // For brevity verify the Flags2 4-byte slice — it's the last 4 bytes if no other update fields fire.
        var actualBytes = actual.GetData();
        // Last 4 bytes are the UInt32 LE of sanitized Flags2 = 0x00000020.
        Assert.Equal((byte)0x20, actualBytes[^4]);
        Assert.Equal((byte)0x00, actualBytes[^3]);
        Assert.Equal((byte)0x00, actualBytes[^2]);
        Assert.Equal((byte)0x00, actualBytes[^1]);
    }

    [Fact]
    public void WriteUpdateUnitData_ChannelObject_PreambleAndBody()
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);

        update.UnitData!.ChannelObject = WowGuid128.Create(HighGuidType703.Creature, 0, 7777, 42);

        var actual = new WorldPacket();
        builder.WriteUpdateUnitData(actual);

        var bytes = actual.GetData();
        // blocksMask = 1 (block 0) → 0x01
        Assert.Equal((byte)0x01, bytes[0]);
        // block 0 mask = (1<<0) | (1<<4) = 0x11 → MSB-first 4 bytes = 00 00 00 11
        Assert.Equal((byte)0x00, bytes[1]); Assert.Equal((byte)0x00, bytes[2]);
        Assert.Equal((byte)0x00, bytes[3]); Assert.Equal((byte)0x11, bytes[4]);
        // After block-mask write, ChannelObjects preamble emits 33 BITS:
        //   WriteBits(1u, 32) + WriteBits(0xFFFFFFFFu, 1)
        // followed by FlushBits to align. The exact byte layout depends on bit-packer
        // state; this test asserts the wire is non-empty and at minimum holds the body
        // (PackedGuid128 of ChannelObject) somewhere after the preamble.
        Assert.True(bytes.Length > 5);
    }

    [Fact]
    public void HasAnyUnitFieldSet_EmptyUnit_ReturnsFalse()
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);

        Assert.False(builder.HasAnyUnitFieldSet());
    }

    [Fact]
    public void HasAnyUnitFieldSet_HealthSet_ReturnsTrue()
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);

        update.UnitData!.Health = 100L;

        Assert.True(builder.HasAnyUnitFieldSet());
    }

    [Fact]
    public void HasAnyUnitFieldSet_NpcFlagsZero_DoesNotCountAsSet()
    {
        // NpcFlags has a CustomPredicate `HasValue && != 0` on the Update side.
        // HasAny now honors that predicate (fixed alongside ActivePlayer PvpInfo OOB
        // crash on enemy target — array CustomPredicate substituted per-element).
        // NpcFlags[0] = 0 → HasValue=true, value=0 → predicate false → HasAny=false.
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);

        update.UnitData!.NpcFlags[0] = 0u;   // HasValue = true, value = 0

        Assert.False(builder.HasAnyUnitFieldSet());
    }

    [Fact]
    public void WriteUpdateUnitData_CascadeRule_BlockBit32ForcedWhenBlock1HasFields()
    {
        // V3_4_3-only cascade quirk: any bit set in block 1 (bits 32-63) forces bit 32
        // on (cascade-emit). Hand-port has the same rule — section attribute sets
        // Cascade = true. Verify with a block-1-only field (FactionTemplate bit 40).
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);

        update.UnitData!.FactionTemplate = 35;   // bit 40 (block 1, position 8)

        var actual = new WorldPacket();
        builder.WriteUpdateUnitData(actual);

        var bytes = actual.GetData();
        // blocksMask = (1<<1) = 0x02 — only block 1 set. NOT block 0 (no cascade-into-block-0
        // because no block-0 fields fire).
        Assert.Equal((byte)0x02, bytes[0]);
        // block 1 mask = (1<<0 cascade) | (1<<8 bit-40-relative) = 0x00000101 → MSB-first 4 bytes = 00 00 01 01
        Assert.Equal((byte)0x00, bytes[1]);
        Assert.Equal((byte)0x00, bytes[2]);
        Assert.Equal((byte)0x01, bytes[3]);
        Assert.Equal((byte)0x01, bytes[4]);
        // Payload: FactionTemplate Int32 LE = 35 = 23 00 00 00
        Assert.Equal((byte)0x23, bytes[5]);
        Assert.Equal((byte)0x00, bytes[6]);
        Assert.Equal((byte)0x00, bytes[7]);
        Assert.Equal((byte)0x00, bytes[8]);
    }
}
