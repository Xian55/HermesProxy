/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HermesProxy.World.Server.Packets;

/// <summary>
/// Helper methods for writing combat-related structures to SpanPacketWriter.
/// </summary>
internal static class SpellPacketHelpers
{
    /// <summary>
    /// Writes SpellCastLogData to a SpanPacketWriter.
    /// Returns false if PowerData exceeds maxPowerEntries (triggers fallback).
    /// </summary>
    public static bool WriteSpellCastLogData(ref SpanPacketWriter writer, SpellCastLogData logData, int maxPowerEntries)
    {
        if (logData.PowerData.Count > maxPowerEntries)
            return false;

        writer.WriteInt64(logData.Health);
        writer.WriteInt32(logData.AttackPower);
        writer.WriteInt32(logData.SpellPower);
        writer.WriteUInt32(logData.Armor);
        writer.WriteBits((uint)logData.PowerData.Count, 9);
        writer.FlushBits();

        foreach (var powerData in logData.PowerData)
        {
            writer.WriteInt32(powerData.PowerType);
            writer.WriteInt32(powerData.Amount);
            writer.WriteInt32(powerData.Cost);
        }

        return true;
    }

    /// <summary>
    /// Writes ContentTuningParams to a SpanPacketWriter.
    /// </summary>
    public static void WriteContentTuningParams(ref SpanPacketWriter writer, ContentTuningParams tuning)
    {
        writer.WriteFloat(tuning.PlayerItemLevel);
        writer.WriteFloat(tuning.TargetItemLevel);
        writer.WriteInt16(tuning.PlayerLevelDelta);
        writer.WriteUInt16(tuning.ScalingHealthItemLevelCurveID);
        writer.WriteUInt8(tuning.TargetLevel);
        writer.WriteUInt8(tuning.Expansion);
        writer.WriteUInt8(tuning.TargetMinScalingLevel);
        writer.WriteUInt8(tuning.TargetMaxScalingLevel);
        writer.WriteInt8(tuning.TargetScalingLevelDelta);
        writer.WriteUInt32((uint)tuning.Flags);
        writer.WriteBits((uint)tuning.TuningType, 4);
        writer.FlushBits();
    }

    // Size constants for MaxSize calculations
    public const int SpellCastLogDataFixedSize = 22; // 8 + 4 + 4 + 4 + 2 (bits flushed)
    public const int SpellLogPowerDataSize = 12;     // 3 ints
    public const int ContentTuningParamsSize = 22;   // 8 + 2 + 2 + 5 + 4 + 1 (bits flushed)
}

public class SendKnownSpells : ServerPacket, ISpanWritable
{
    public SendKnownSpells() : base(Opcode.SMSG_SEND_KNOWN_SPELLS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBit(InitialLogin);
        _worldPacket.WriteInt32(KnownSpells.Count);
        _worldPacket.WriteInt32(FavoriteSpells.Count);

        foreach (var spellId in KnownSpells)
            _worldPacket.WriteUInt32(spellId);

        foreach (var spellId in FavoriteSpells)
            _worldPacket.WriteUInt32(spellId);
    }

    // MaxSize: bit(1) + 2 counts(8) + spells + favorites
    // Reduced from 1024/128 to 256/16 based on typical usage (161-165 bytes = ~40 spells)
    private const int MaxKnownSpells = 256;
    private const int MaxFavoriteSpells = 16;
    public int MaxSize => 1 + 8 + MaxKnownSpells * 4 + MaxFavoriteSpells * 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (KnownSpells.Count > MaxKnownSpells || FavoriteSpells.Count > MaxFavoriteSpells)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBit(InitialLogin);
        writer.WriteInt32(KnownSpells.Count);
        writer.WriteInt32(FavoriteSpells.Count);

        foreach (var spellId in KnownSpells)
            writer.WriteUInt32(spellId);

        foreach (var spellId in FavoriteSpells)
            writer.WriteUInt32(spellId);

        return writer.Position;
    }

    public bool InitialLogin;
    public List<uint> KnownSpells = new();
    public List<uint> FavoriteSpells = new(); // tradeskill recipes
}

public class SupercededSpells : ServerPacket, ISpanWritable
{
    public SupercededSpells() : base(Opcode.SMSG_SUPERCEDED_SPELLS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(SpellID.Count);
        _worldPacket.WriteInt32(Superceded.Count);
        _worldPacket.WriteInt32(FavoriteSpellID.Count);

        foreach (var spellId in SpellID)
            _worldPacket.WriteUInt32(spellId);

        foreach (var spellId in Superceded)
            _worldPacket.WriteUInt32(spellId);

        foreach (var spellId in FavoriteSpellID)
            _worldPacket.WriteInt32(spellId);
    }

    // Cap for spell lists - usually 1 spell superceded at a time
    private const int MaxSpellsPerList = 8;
    // 3 counts(12) + 3 lists of spells(96)
    public int MaxSize => 12 + MaxSpellsPerList * 4 * 3;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (SpellID.Count > MaxSpellsPerList ||
            Superceded.Count > MaxSpellsPerList ||
            FavoriteSpellID.Count > MaxSpellsPerList)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(SpellID.Count);
        writer.WriteInt32(Superceded.Count);
        writer.WriteInt32(FavoriteSpellID.Count);

        foreach (var spellId in SpellID)
            writer.WriteUInt32(spellId);

        foreach (var spellId in Superceded)
            writer.WriteUInt32(spellId);

        foreach (var spellId in FavoriteSpellID)
            writer.WriteInt32(spellId);

        return writer.Position;
    }

    public List<uint> SpellID = new();
    public List<uint> Superceded = new();
    public List<int> FavoriteSpellID = new();
}

public class LearnedSpells : ServerPacket, ISpanWritable
{
    // Practical cap - usually 1 spell per trainer click, hunter pet quest ~5
    private const int MaxSpells = 8;

    public LearnedSpells() : base(Opcode.SMSG_LEARNED_SPELLS, ConnectionType.Instance) { }

    public override void Write()
    {
        // V3_4_3 (WotLK Classic) layout differs from modern retail. Per WotLK
        // client (and HermesProxy-WOTLK fork LearnedSpells.cs:27-44):
        //   int32 Count + uint32 SpecializationID + 1-bit SuppressMessaging +
        //   per-spell { int32 SpellID + 4 bits (IsFavorite, _, HasSuperceded,
        //   _) + optional int32 SupercededID }.
        // Our retail layout (flat list + separate favorites + trailing bit)
        // doesn't match the V3_4_3 cascade — without the per-spell bit gates,
        // the client mis-parses every entry and the spellbook silently ignores
        // newly-learned trainer spells (until logout/login resets via
        // SMSG_SEND_KNOWN_SPELLS, which has its own correct format).
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            _worldPacket.WriteInt32(Spells.Count);
            _worldPacket.WriteUInt32(SpecializationID);
            _worldPacket.WriteBit(SuppressMessaging);
            _worldPacket.FlushBits();
            foreach (uint spell in Spells)
            {
                _worldPacket.WriteInt32((int)spell);
                _worldPacket.WriteBit(FavoriteSpellID.Contains((int)spell));
                _worldPacket.WriteBit(false);   // field_8 — reserved
                _worldPacket.WriteBit(false);   // HasSuperceded — none for this path
                _worldPacket.WriteBit(false);   // HasTraitDefinitionID — N/A in WotLK
                _worldPacket.FlushBits();
            }
            return;
        }

        _worldPacket.WriteInt32(Spells.Count);
        _worldPacket.WriteInt32(FavoriteSpellID.Count);
        _worldPacket.WriteUInt32(SpecializationID);

        foreach (uint spell in Spells)
            _worldPacket.WriteUInt32(spell);

        foreach (int spell in FavoriteSpellID)
            _worldPacket.WriteInt32(spell);

        _worldPacket.WriteBit(SuppressMessaging);
        _worldPacket.FlushBits();
    }

    // MaxSize: 3 int32 (12) + 2 lists capped at 8 (64) + 1 bit byte = 77
    public int MaxSize => 12 + MaxSpells * 4 * 2 + 1;

    public int WriteToSpan(Span<byte> buffer)
    {
        // V3_4_3 uses a different bit-packed cascade (see Write()). Fall back to
        // the boxed Write path for V3_4_3 by returning -1 — the framework will
        // detect the overflow sentinel and re-emit via Write().
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            return -1;

        if (Spells.Count > MaxSpells || FavoriteSpellID.Count > MaxSpells)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Spells.Count);
        writer.WriteInt32(FavoriteSpellID.Count);
        writer.WriteUInt32(SpecializationID);

        foreach (uint spell in Spells)
            writer.WriteUInt32(spell);

        foreach (int spell in FavoriteSpellID)
            writer.WriteInt32(spell);

        writer.WriteBit(SuppressMessaging);
        writer.FlushBits();
        return writer.Position;
    }

    public List<uint> Spells = new();
    public List<int> FavoriteSpellID = new();
    public uint SpecializationID;
    public bool SuppressMessaging;
}

public class SendUnlearnSpells : ServerPacket, ISpanWritable
{
    private const int MaxSpells = 8;

    public SendUnlearnSpells() : base(Opcode.SMSG_SEND_UNLEARN_SPELLS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Spells.Count);
        foreach (var spell in Spells)
            _worldPacket.WriteUInt32(spell);
    }

    // MaxSize: int32 count (4) + 8 spells (32) = 36
    public int MaxSize => 4 + MaxSpells * 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Spells.Count > MaxSpells)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Spells.Count);
        foreach (var spell in Spells)
            writer.WriteUInt32(spell);
        return writer.Position;
    }

    public List<uint> Spells = new();
}

public class UnlearnedSpells : ServerPacket, ISpanWritable
{
    private const int MaxSpells = 8;

    public UnlearnedSpells() : base(Opcode.SMSG_UNLEARNED_SPELLS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Spells.Count);
        foreach (uint spellId in Spells)
            _worldPacket.WriteUInt32(spellId);

        _worldPacket.WriteBit(SuppressMessaging);
        _worldPacket.FlushBits();
    }

    // MaxSize: int32 count (4) + 8 spells (32) + 1 bit byte = 37
    public int MaxSize => 4 + MaxSpells * 4 + 1;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Spells.Count > MaxSpells)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Spells.Count);
        foreach (uint spellId in Spells)
            writer.WriteUInt32(spellId);
        writer.WriteBit(SuppressMessaging);
        writer.FlushBits();
        return writer.Position;
    }

    public List<uint> Spells = new();
    public bool SuppressMessaging;
}

public class SendSpellHistory : ServerPacket, ISpanWritable
{
    // Practical cap for spell history at login
    private const int MaxEntries = 64;
    // Each entry: 6 fixed fields (24) + bits (1) = 25 bytes (unused optionals)
    private const int MaxEntrySize = 25;

    public SendSpellHistory() : base(Opcode.SMSG_SEND_SPELL_HISTORY, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Entries.Count);
        Entries.ForEach(p => p.Write(_worldPacket));
    }

    // MaxSize: count (4) + 64 entries (25 each) = 1604
    public int MaxSize => 4 + MaxEntries * MaxEntrySize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Entries.Count > MaxEntries)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Entries.Count);
        foreach (var entry in Entries)
        {
            writer.WriteUInt32(entry.SpellID);
            writer.WriteUInt32(entry.ItemID);
            writer.WriteUInt32(entry.Category);
            writer.WriteInt32(entry.RecoveryTime);
            writer.WriteInt32(entry.CategoryRecoveryTime);
            writer.WriteFloat(entry.ModRate);
            writer.WriteBit(false); // unused622_1
            writer.WriteBit(false); // unused622_2
            writer.WriteBit(entry.OnHold);
            writer.FlushBits();
            // unused622_1 and unused622_2 are never set
        }
        return writer.Position;
    }

    public List<SpellHistoryEntry> Entries = new();
}

public class SpellHistoryEntry
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(SpellID);
        data.WriteUInt32(ItemID);
        data.WriteUInt32(Category);
        data.WriteInt32(RecoveryTime);
        data.WriteInt32(CategoryRecoveryTime);
        data.WriteFloat(ModRate);
        data.WriteBit(unused622_1.HasValue);
        data.WriteBit(unused622_2.HasValue);
        data.WriteBit(OnHold);
        data.FlushBits();

        if (unused622_1.HasValue)
            data.WriteUInt32(unused622_1.Value);
        if (unused622_2.HasValue)
            data.WriteUInt32(unused622_2.Value);
    }

    public uint SpellID;
    public uint ItemID;
    public uint Category;
    public int RecoveryTime;
    public int CategoryRecoveryTime;
    public float ModRate = 1.0f;
    public bool OnHold;
    uint? unused622_1 = null;   // This field is not used for anything in the client in 6.2.2.20444
    uint? unused622_2 = null;   // This field is not used for anything in the client in 6.2.2.20444
}

public class SendSpellCharges : ServerPacket, ISpanWritable
{
    // Practical cap for spell charges - limited charge categories
    private const int MaxEntries = 16;
    // Each entry: uint + uint + float + byte = 13 bytes
    private const int EntrySize = 13;

    public SendSpellCharges() : base(Opcode.SMSG_SEND_SPELL_CHARGES, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Entries.Count);
        Entries.ForEach(p => p.Write(_worldPacket));
    }

    // MaxSize: count (4) + 16 entries (13 each) = 212
    public int MaxSize => 4 + MaxEntries * EntrySize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Entries.Count > MaxEntries)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Entries.Count);
        foreach (var entry in Entries)
        {
            writer.WriteUInt32(entry.Category);
            writer.WriteUInt32(entry.NextRecoveryTime);
            writer.WriteFloat(entry.ChargeModRate);
            writer.WriteUInt8(entry.ConsumedCharges);
        }
        return writer.Position;
    }

    public List<SpellChargeEntry> Entries = new();
}

public class SpellChargeEntry
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(Category);
        data.WriteUInt32(NextRecoveryTime);
        data.WriteFloat(ChargeModRate);
        data.WriteUInt8(ConsumedCharges);
    }

    public uint Category;
    public uint NextRecoveryTime;
    public float ChargeModRate = 1.0f;
    public byte ConsumedCharges;
}

class CancelAura : ClientPacket
{
    public CancelAura(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        SpellID = _worldPacket.ReadUInt32();
        CasterGUID = _worldPacket.ReadPackedGuid128();
    }

    public uint SpellID;
    public WowGuid128 CasterGUID;
}

class CancelAutoRepeatSpell : ClientPacket
{
    public CancelAutoRepeatSpell(WorldPacket packet) : base(packet) { }

    public override void Read() { }
}

public class CancelAutoRepeat : ServerPacket, ISpanWritable
{
    public CancelAutoRepeat() : base(Opcode.SMSG_CANCEL_AUTO_REPEAT) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        return writer.Position;
    }

    public WowGuid128 Guid;
}

public class AuraUpdate : ServerPacket
{
    public AuraUpdate(WowGuid128 guid, bool all) : base(Opcode.SMSG_AURA_UPDATE, ConnectionType.Instance) 
    {
        UnitGUID = guid;
        UpdateAll = all;
    }

    public override void Write()
    {
        _worldPacket.WriteBit(UpdateAll);
        _worldPacket.WriteBits(Auras.Count, 9);
        foreach (AuraInfo aura in Auras)
            aura.Write(_worldPacket);

        _worldPacket.WritePackedGuid128(UnitGUID);

        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            $"[AuraUpdateTrace][write] guid={UnitGUID} updateAll={UpdateAll} aurasCount={Auras.Count} packetBytes={_worldPacket.GetSize()}");
    }

    public bool UpdateAll;
    public List<AuraInfo> Auras = new();
    public WowGuid128 UnitGUID;
}

public struct AuraInfo
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8(Slot);
        data.WriteBit(AuraData != null);
        data.FlushBits();

        if (AuraData != null)
            AuraData.Write(data);
    }

    public byte Slot;
    public AuraDataInfo AuraData;
}

public class AuraDataInfo
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(CastID);
        data.WriteUInt32(SpellID);
        data.WriteUInt32(SpellXSpellVisualID);
        data.WriteUInt16((ushort)Flags);
        data.WriteUInt32(ActiveFlags);
        data.WriteUInt16(CastLevel);
        data.WriteUInt8(Applications);
        data.WriteInt32(ContentTuningID);
        data.WriteBit(CastUnit != default);
        data.WriteBit(Duration.HasValue);
        data.WriteBit(Remaining.HasValue);
        data.WriteBit(TimeMod.HasValue);
        data.WriteBits(Points.Count, 6);
        data.WriteBits(EstimatedPoints.Count, 6);
        data.WriteBit(ContentTuning != null);

        if (ContentTuning != null)
            ContentTuning.Write(data);

        if (CastUnit != default)
            data.WritePackedGuid128(CastUnit);

        if (Duration.HasValue)
            data.WriteInt32(Duration.Value);

        if (Remaining.HasValue)
            data.WriteInt32(Remaining.Value);

        if (TimeMod.HasValue)
            data.WriteFloat(TimeMod.Value);

        foreach (var point in Points)
            data.WriteFloat(point);

        foreach (var point in EstimatedPoints)
            data.WriteFloat(point);
    }

    public WowGuid128 CastID;
    public uint SpellID;
    public uint SpellXSpellVisualID;
    public AuraFlagsModern Flags;
    public uint ActiveFlags;
    public ushort CastLevel = 1;
    public byte Applications = 1;
    public int ContentTuningID;
    ContentTuningParams ContentTuning = null!;
    public WowGuid128 CastUnit;
    public int? Duration;
    public int? Remaining;
    float? TimeMod = null;
    public List<float> Points = new();
    public List<float> EstimatedPoints = new();
}

class ContentTuningParams
{
    public void Write(WorldPacket data)
    {
        data.WriteFloat(PlayerItemLevel);
        data.WriteFloat(TargetItemLevel);
        data.WriteInt16(PlayerLevelDelta);
        data.WriteUInt16(ScalingHealthItemLevelCurveID);
        data.WriteUInt8(TargetLevel);
        data.WriteUInt8(Expansion);
        data.WriteUInt8(TargetMinScalingLevel);
        data.WriteUInt8(TargetMaxScalingLevel);
        data.WriteInt8(TargetScalingLevelDelta);
        data.WriteUInt32((uint)Flags);
        data.WriteBits(TuningType, 4);
        data.FlushBits();
    }

    public ContentTuningType TuningType;
    public short PlayerLevelDelta;
    public float PlayerItemLevel;
    public float TargetItemLevel = 0.0f;
    public ushort ScalingHealthItemLevelCurveID;
    public byte TargetLevel;
    public byte Expansion;
    public byte TargetMinScalingLevel;
    public byte TargetMaxScalingLevel;
    public sbyte TargetScalingLevelDelta;
    public ContentTuningFlags Flags = ContentTuningFlags.NoLevelScaling | ContentTuningFlags.NoItemLevelScaling;

    public enum ContentTuningType
    {
        CreatureToPlayerDamage = 1,
        PlayerToCreatureDamage = 2,
        CreatureToCreatureDamage = 4,
        PlayerToSandboxScaling = 7, // NYI
        PlayerToPlayerExpectedStat = 8
    }

    public enum ContentTuningFlags
    {
        NoLevelScaling = 0x1,
        NoItemLevelScaling = 0x2
    }
}

public class CastSpell : ClientPacket
{
    public CastSpell(WorldPacket packet) : base(packet)
    {
        Cast = new SpellCastRequest();
    }

    public override void Read()
    {
        Cast.Read(_worldPacket);
    }

    public SpellCastRequest Cast;
}

public class PetCastSpell : ClientPacket
{
    public PetCastSpell(WorldPacket packet) : base(packet)
    {
        Cast = new SpellCastRequest();
    }

    public override void Read()
    {
        PetGUID = _worldPacket.ReadPackedGuid128();
        Cast.Read(_worldPacket);
    }

    public WowGuid128 PetGUID;
    public SpellCastRequest Cast;
}

public class UseItem : ClientPacket
{
    public UseItem(WorldPacket packet) : base(packet)
    {
        Cast = new SpellCastRequest();
    }

    public override void Read()
    {
        PackSlot = _worldPacket.ReadUInt8();
        Slot = _worldPacket.ReadUInt8();
        CastItem = _worldPacket.ReadPackedGuid128();
        Cast.Read(_worldPacket);
    }

    public byte PackSlot;
    public byte Slot;
    public WowGuid128 CastItem;
    public SpellCastRequest Cast;
}

public class SpellCastRequest
{
    public void Read(WorldPacket data)
    {
        CastID = data.ReadPackedGuid128();
        Misc[0] = data.ReadUInt32();
        Misc[1] = data.ReadUInt32();
        SpellID = data.ReadUInt32();

        SpellXSpellVisualID = data.ReadUInt32();

        MissileTrajectory.Read(data);
        CraftingNPC = data.ReadPackedGuid128();

        var optionalReagents = data.ReadUInt32();
        var optionalCurrencies = data.ReadUInt32();

        for (var i = 0; i < optionalReagents; ++i)
        {
            var reagent = new SpellOptionalReagent();
            reagent.Read(data);
            OptionalReagents.Add(reagent);
        }

        for (var i = 0; i < optionalCurrencies; ++i)
        {
            var currency = new SpellExtraCurrencyCost();
            currency.Read(data);
            OptionalCurrencies.Add(currency);
        }

        SendCastFlags = data.ReadBits<uint>(5);
        if (data.HasBit())
            MoveUpdate = new();
        var weightCount = data.ReadBits<uint>(2);
        Target.Read(data);

        if (MoveUpdate != null)
        {
            MoverGUID = data.ReadPackedGuid128();
            MoveUpdate.ReadMovementInfoModern(data);
        }

        for (var i = 0; i < weightCount; ++i)
        {
            data.ResetBitPos();
            SpellWeight weight;
            weight.Type = data.ReadBits<uint>(2);
            weight.ID = data.ReadInt32();
            weight.Quantity = data.ReadUInt32();
            Weight.Add(weight);
        }
    }

    public WowGuid128 CastID;
    public uint SpellID;
    public uint SpellXSpellVisualID;
    public uint SendCastFlags;
    public SpellTargetData Target = new();
    public MissileTrajectoryRequest MissileTrajectory;
    public WowGuid128 MoverGUID;
    public MovementInfo MoveUpdate = null!;
    public List<SpellWeight> Weight = new();
    public List<SpellOptionalReagent> OptionalReagents = new(3);
    public List<SpellExtraCurrencyCost> OptionalCurrencies = new(5 /*MAX_ITEM_EXT_COST_CURRENCIES*/);
    public WowGuid128 CraftingNPC;
    public uint[] Misc = new uint[2];
}

public struct MissileTrajectoryRequest
{
    public void Read(WorldPacket data)
    {
        Pitch = data.ReadFloat();
        Speed = data.ReadFloat();
    }

    public float Pitch;
    public float Speed;
}

public struct SpellWeight
{
    public uint Type;
    public int ID;
    public uint Quantity;
}

public struct SpellOptionalReagent
{
    public void Read(WorldPacket data)
    {
        ItemID = data.ReadInt32();
        Slot = data.ReadInt32();
        Count = data.ReadInt32();
    }

    public int ItemID;
    public int Slot;
    public int Count;
}

public struct SpellExtraCurrencyCost
{
    public void Read(WorldPacket data)
    {
        CurrencyID = data.ReadInt32();
        Slot = data.ReadInt32();
        Count = data.ReadInt32();
    }

    public int CurrencyID;
    public int Slot;
    public int Count;
}

public class CancelCast : ClientPacket
{
    public CancelCast(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        CastID = _worldPacket.ReadPackedGuid128();
        SpellID = _worldPacket.ReadUInt32();
    }

    public uint SpellID;
    public WowGuid128 CastID;
}

class CancelChannelling : ClientPacket
{
    public CancelChannelling(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        SpellID = _worldPacket.ReadInt32();
        Reason = _worldPacket.ReadInt32();
    }

    public int SpellID;
    public int Reason;       // 40 = /run SpellStopCasting(), 16 = movement/AURA_INTERRUPT_FLAG_MOVE, 41 = turning/AURA_INTERRUPT_FLAG_TURNING
                             // does not match SpellCastResult enum
}

class SpellPrepare : ServerPacket, ISpanWritable
{
    public SpellPrepare() : base(Opcode.SMSG_SPELL_PREPARE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(ClientCastID);
        _worldPacket.WritePackedGuid128(ServerCastID);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2; // 2 packed GUIDs

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(ClientCastID.Low, ClientCastID.High);
        writer.WritePackedGuid128(ServerCastID.Low, ServerCastID.High);
        return writer.Position;
    }

    public WowGuid128 ClientCastID;
    public WowGuid128 ServerCastID;
}

class CastFailed : ServerPacket, ISpanWritable
{
    public CastFailed() : base(Opcode.SMSG_CAST_FAILED, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(CastID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32(SpellXSpellVisualID);
        _worldPacket.WriteUInt32(Reason);
        _worldPacket.WriteInt32(FailedArg1);
        _worldPacket.WriteInt32(FailedArg2);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 * 4 + 4 * 2; // GUID + 4 uint32 + 2 int32

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(CastID.Low, CastID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32(SpellXSpellVisualID);
        writer.WriteUInt32(Reason);
        writer.WriteInt32(FailedArg1);
        writer.WriteInt32(FailedArg2);
        return writer.Position;
    }

    public WowGuid128 CastID;
    public uint SpellID;
    public uint Reason;
    public int FailedArg1 = -1;
    public int FailedArg2 = -1;
    public uint SpellXSpellVisualID;
}

class PetCastFailed : ServerPacket, ISpanWritable
{
    public PetCastFailed() : base(Opcode.SMSG_PET_CAST_FAILED, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(CastID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32(Reason);
        _worldPacket.WriteInt32(FailedArg1);
        _worldPacket.WriteInt32(FailedArg2);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 * 3 + 4 * 2; // GUID + 3 uint32 + 2 int32

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(CastID.Low, CastID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32(Reason);
        writer.WriteInt32(FailedArg1);
        writer.WriteInt32(FailedArg2);
        return writer.Position;
    }

    public WowGuid128 CastID;
    public uint SpellID;
    public uint Reason;
    public int FailedArg1 = -1;
    public int FailedArg2 = -1;
}

public class SpellFailure : ServerPacket, ISpanWritable
{
    public SpellFailure() : base(Opcode.SMSG_SPELL_FAILURE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(CasterUnit);
        _worldPacket.WritePackedGuid128(CastID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32(SpellXSpellVisualID);
        _worldPacket.WriteUInt16(Reason);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 4 * 2 + 2; // 2 GUIDs + 2 uint32 + ushort

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(CasterUnit.Low, CasterUnit.High);
        writer.WritePackedGuid128(CastID.Low, CastID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32(SpellXSpellVisualID);
        writer.WriteUInt16(Reason);
        return writer.Position;
    }

    public WowGuid128 CasterUnit;
    public WowGuid128 CastID;
    public uint SpellID;
    public uint SpellXSpellVisualID;
    public ushort Reason;
}

public class SpellFailedOther : ServerPacket, ISpanWritable
{
    public SpellFailedOther() : base(Opcode.SMSG_SPELL_FAILED_OTHER, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(CasterUnit);
        _worldPacket.WritePackedGuid128(CastID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32(SpellXSpellVisualID);
        _worldPacket.WriteUInt8(Reason);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 4 * 2 + 1; // 2 GUIDs + 2 uint32 + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(CasterUnit.Low, CasterUnit.High);
        writer.WritePackedGuid128(CastID.Low, CastID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32(SpellXSpellVisualID);
        writer.WriteUInt8(Reason);
        return writer.Position;
    }

    public WowGuid128 CasterUnit;
    public WowGuid128 CastID;
    public uint SpellID;
    public uint SpellXSpellVisualID;
    public byte Reason;
}

public class SpellStart : ServerPacket
{
    public SpellCastData Cast;

    public SpellStart() : base(Opcode.SMSG_SPELL_START, ConnectionType.Instance)
    {
        Cast = new SpellCastData();
    }

    public override void Write()
    {
        Cast.Write(_worldPacket);
    }
}

class SpellGo : ServerPacket
{
    public SpellGo() : base(Opcode.SMSG_SPELL_GO, ConnectionType.Instance) { }

    public override void Write()
    {
        Cast.Write(_worldPacket);

        _worldPacket.WriteBit(LogData != null);
        if (LogData != null)
            LogData.Write(_worldPacket);
        _worldPacket.FlushBits();
    }

    public SpellCastData Cast = new();
    public SpellCastLogData LogData = null!;
}

public class SpellCastData
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(CasterGUID);
        data.WritePackedGuid128(CasterUnit);
        data.WritePackedGuid128(CastID);
        data.WritePackedGuid128(OriginalCastID);
        data.WriteInt32(SpellID);
        data.WriteUInt32(SpellXSpellVisualID);
        data.WriteUInt32(CastFlags);
        data.WriteUInt32(CastFlagsEx);
        data.WriteUInt32(CastTime);

        MissileTrajectory.Write(data);

        data.WriteUInt8(DestLocSpellCastIndex);

        Immunities.Write(data);
        Predict.Write(data);

        data.WriteBits(HitTargets.Count, 16);
        data.WriteBits(MissTargets.Count, 16);
        data.WriteBits(MissStatus.Count, 16);
        data.WriteBits(RemainingPower.Count, 9);
        data.WriteBit(RemainingRunes != null);
        data.WriteBits(TargetPoints.Count, 16);
        data.WriteBit(AmmoDisplayId != null);
        data.WriteBit(AmmoInventoryType != null);
        data.FlushBits();

        foreach (SpellMissStatus missStatus in MissStatus)
            missStatus.Write(data);

        Target.Write(data);

        foreach (WowGuid128 hitTarget in HitTargets)
            data.WritePackedGuid128(hitTarget);

        foreach (WowGuid128 missTarget in MissTargets)
            data.WritePackedGuid128(missTarget);

        foreach (SpellPowerData power in RemainingPower)
            power.Write(data);

        if (RemainingRunes != null)
            RemainingRunes.Write(data);

        foreach (TargetLocation targetLoc in TargetPoints)
            targetLoc.Write(data);

        if (AmmoDisplayId != null)
            data.WriteInt32((int)AmmoDisplayId);

        if (AmmoInventoryType != null)
            data.WriteInt32((int)AmmoInventoryType);
    }

    public WowGuid128 CasterGUID;
    public WowGuid128 CasterUnit;
    public WowGuid128 CastID = WowGuid128.Empty;
    public WowGuid128 OriginalCastID = WowGuid128.Empty;
    public int SpellID;
    public uint SpellXSpellVisualID;
    public uint CastFlags;
    public uint CastFlagsEx;
    public uint CastTime;
    public List<WowGuid128> HitTargets = new();
    public List<WowGuid128> MissTargets = new();
    public List<SpellMissStatus> MissStatus = new();
    public SpellTargetData Target = new();
    public List<SpellPowerData> RemainingPower = new();
    public RuneData RemainingRunes = null!;
    public MissileTrajectoryResult MissileTrajectory;
    public int? AmmoDisplayId;
    public int? AmmoInventoryType;
    public byte DestLocSpellCastIndex;
    public List<TargetLocation> TargetPoints = new();
    public CreatureImmunities Immunities;
    public SpellHealPrediction Predict = new();
}

public struct SpellMissStatus
{
    public SpellMissStatus(SpellMissInfo reason, SpellMissInfo reflectStatus)
    {
        Reason = reason;
        ReflectStatus = reflectStatus;
    }

    public void Write(WorldPacket data)
    {
        data.WriteBits((byte)Reason, 4);
        if (Reason == SpellMissInfo.Reflect)
            data.WriteBits(ReflectStatus, 4);

        data.FlushBits();
    }

    public SpellMissInfo Reason;
    public SpellMissInfo ReflectStatus;
}

public class TargetLocation
{
    public void Read(WorldPacket data)
    {
        Transport = data.ReadPackedGuid128();
        Location = data.ReadVector3();
    }

    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(Transport);
        data.WriteVector3(Location);
    }

    public WowGuid128 Transport = WowGuid128.Empty;
    public Vector3 Location;
}

public class SpellTargetData
{
    public void Read(WorldPacket data)
    {
        // V3_4_3 client uses 28-bit target flags (WPP V3_4_0 module gates 28 at V3_4_1+).
        int flagBits = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 ? 28 : 26;
        Flags = (SpellCastTargetFlags)data.ReadBits<uint>(flagBits);
        if (data.HasBit())
            SrcLocation = new();
        if (data.HasBit())
            DstLocation = new();
        if (data.HasBit())
            Orientation = new();
        if (data.HasBit())
            MapID = new();
        uint nameLength = data.ReadBits<uint>(7);

        Unit = data.ReadPackedGuid128();
        Item = data.ReadPackedGuid128();

        if (SrcLocation != null)
            SrcLocation.Read(data);

        if (DstLocation != null)
            DstLocation.Read(data);

        if (Orientation != null)
            Orientation = data.ReadFloat();

        if (MapID != null)
            MapID = data.ReadInt32();

        Name = data.ReadString(nameLength);
    }

    public void Write(WorldPacket data)
    {
        // V3_4_3 client uses 28-bit target flags (WPP V3_4_0 module gates 28 at V3_4_1+).
        int flagBits = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 ? 28 : 26;
        data.WriteBits((uint)Flags, flagBits);
        data.WriteBit(SrcLocation != null);
        data.WriteBit(DstLocation != null);
        data.WriteBit(Orientation.HasValue);
        data.WriteBit(MapID.HasValue);
        data.WriteBits(Name.GetByteCount(), 7);
        data.FlushBits();

        data.WritePackedGuid128(Unit);
        data.WritePackedGuid128(Item);

        if (SrcLocation != null)
            SrcLocation.Write(data);

        if (DstLocation != null)
            DstLocation.Write(data);

        if (Orientation.HasValue)
            data.WriteFloat(Orientation.Value);

        if (MapID.HasValue)
            data.WriteInt32(MapID.Value);

        data.WriteString(Name);
    }

    public SpellCastTargetFlags Flags;
    public WowGuid128 Unit;
    public WowGuid128 Item;
    public TargetLocation SrcLocation = null!;
    public TargetLocation DstLocation = null!;
    public float? Orientation;
    public int? MapID;
    public string Name = "";
}

public struct SpellPowerData
{
    public void Write(WorldPacket data)
    {
        data.WriteInt32(Cost);
        data.WriteInt8((sbyte)Type);
    }

    public int Cost;
    public PowerType Type;
}

public class RuneData
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8(Start);
        data.WriteUInt8(Count);
        data.WriteInt32(Cooldowns.Count);

        foreach (byte cd in Cooldowns)
            data.WriteUInt8(cd);
    }

    public byte Start;
    public byte Count;
    public List<byte> Cooldowns = new();
}

public struct MissileTrajectoryResult
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(TravelTime);
        data.WriteFloat(Pitch);
    }

    public uint TravelTime;
    public float Pitch;
}

public struct CreatureImmunities
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(School);
        data.WriteUInt32(Value);
    }

    public uint School;
    public uint Value;
}

public class SpellHealPrediction
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(Points);
        data.WriteUInt8(Type);
        data.WritePackedGuid128(BeaconGUID);
    }

    public WowGuid128 BeaconGUID = WowGuid128.Empty;
    public uint Points;
    public byte Type;
}

class LearnTalent : ClientPacket
{
    public LearnTalent(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TalentID = _worldPacket.ReadUInt32();
        Rank = _worldPacket.ReadUInt16();
    }

    public uint TalentID;
    public ushort Rank;
}

public class SpellCooldownPkt : ServerPacket, ISpanWritable
{
    // Practical cap for cooldowns at login
    private const int MaxCooldowns = 64;

    public SpellCooldownPkt() : base(Opcode.SMSG_SPELL_COOLDOWN, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Caster);
        _worldPacket.WriteUInt8(Flags);
        _worldPacket.WriteInt32(SpellCooldowns.Count);
        foreach (var cd in SpellCooldowns)
            cd.Write(_worldPacket);
    }

    // MaxSize: GUID (18) + byte (1) + count (4) + 64 cooldowns (12 each) = 791
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 1 + 4 + MaxCooldowns * 12;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (SpellCooldowns.Count > MaxCooldowns)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Caster.Low, Caster.High);
        writer.WriteUInt8(Flags);
        writer.WriteInt32(SpellCooldowns.Count);
        foreach (var cd in SpellCooldowns)
        {
            writer.WriteUInt32(cd.SpellID);
            writer.WriteUInt32(cd.ForcedCooldown);
            writer.WriteFloat(cd.ModRate);
        }
        return writer.Position;
    }

    public List<SpellCooldownStruct> SpellCooldowns = new();
    public WowGuid128 Caster;
    public byte Flags;
}

public class SpellCooldownStruct
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(SpellID);
        data.WriteUInt32(ForcedCooldown);
        data.WriteFloat(ModRate);
    }

    public uint SpellID;
    public uint ForcedCooldown;
    public float ModRate = 1.0f;
}

public class CooldownEvent : ServerPacket, ISpanWritable
{
    public CooldownEvent() : base(Opcode.SMSG_COOLDOWN_EVENT, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteBit(IsPet);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 5; // uint + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(SpellID);
        writer.WriteBit(IsPet);
        writer.FlushBits();
        return writer.Position;
    }

    public bool IsPet;
    public uint SpellID;
}

public class ClearCooldown : ServerPacket, ISpanWritable
{
    public ClearCooldown() : base(Opcode.SMSG_CLEAR_COOLDOWN, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteBit(ClearOnHold);
        _worldPacket.WriteBit(IsPet);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 5; // uint + 1 byte for 2 bits

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(SpellID);
        writer.WriteBit(ClearOnHold);
        writer.WriteBit(IsPet);
        writer.FlushBits();
        return writer.Position;
    }

    public bool IsPet;
    public uint SpellID;
    public bool ClearOnHold;
}

public class CooldownCheat : ServerPacket, ISpanWritable
{
    public CooldownCheat() : base(Opcode.SMSG_COOLDOWN_CHEAT, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        return writer.Position;
    }

    public WowGuid128 Guid;
}

class SpellNonMeleeDamageLog : ServerPacket, ISpanWritable
{
    private const int MaxPowerDataEntries = 10;

    public SpellNonMeleeDamageLog() : base(Opcode.SMSG_SPELL_NON_MELEE_DAMAGE_LOG, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(TargetGUID);
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WritePackedGuid128(CastID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32(SpellXSpellVisualID);
        _worldPacket.WriteInt32(Damage);
        _worldPacket.WriteInt32(OriginalDamage);
        _worldPacket.WriteInt32(Overkill);
        _worldPacket.WriteUInt8(SchoolMask);
        _worldPacket.WriteInt32(Absorbed);
        _worldPacket.WriteInt32(Resisted);
        _worldPacket.WriteInt32(ShieldBlock);

        _worldPacket.WriteBit(Periodic);
        _worldPacket.WriteBits((uint)Flags, 7);
        _worldPacket.WriteBit(false); // Debug info

        _worldPacket.WriteBit(LogData != null);
        _worldPacket.WriteBit(ContentTuning != null);
        _worldPacket.FlushBits();

        if (LogData != null)
            LogData.Write(_worldPacket);

        if (ContentTuning != null)
            ContentTuning.Write(_worldPacket);
    }

    // MaxSize: 3 GUIDs (54) + fixed values (33) + bits (2) + SpellCastLogData (142) + ContentTuning (22) = 253
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 3 + 33 + 2 +
        SpellPacketHelpers.SpellCastLogDataFixedSize + MaxPowerDataEntries * SpellPacketHelpers.SpellLogPowerDataSize +
        SpellPacketHelpers.ContentTuningParamsSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(TargetGUID.Low, TargetGUID.High);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WritePackedGuid128(CastID.Low, CastID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32(SpellXSpellVisualID);
        writer.WriteInt32(Damage);
        writer.WriteInt32(OriginalDamage);
        writer.WriteInt32(Overkill);
        writer.WriteUInt8(SchoolMask);
        writer.WriteInt32(Absorbed);
        writer.WriteInt32(Resisted);
        writer.WriteInt32(ShieldBlock);

        writer.WriteBit(Periodic);
        writer.WriteBits((uint)Flags, 7);
        writer.WriteBit(false); // Debug info
        writer.WriteBit(LogData != null);
        writer.WriteBit(ContentTuning != null);
        writer.FlushBits();

        if (LogData != null)
        {
            if (!SpellPacketHelpers.WriteSpellCastLogData(ref writer, LogData, MaxPowerDataEntries))
                return -1;
        }

        if (ContentTuning != null)
            SpellPacketHelpers.WriteContentTuningParams(ref writer, ContentTuning);

        return writer.Position;
    }

    public WowGuid128 TargetGUID;
    public WowGuid128 CasterGUID;
    public WowGuid128 CastID;
    public uint SpellID;
    public uint SpellXSpellVisualID;
    public int Damage;
    public int OriginalDamage;
    public int Overkill = -1;
    public byte SchoolMask;
    public int ShieldBlock;
    public int Resisted;
    public bool Periodic;
    public int Absorbed;
    public SpellHitType Flags;
    public SpellCastLogData LogData = null!;
    public ContentTuningParams ContentTuning = null!;
}

class SpellHealLog : ServerPacket, ISpanWritable
{
    private const int MaxPowerDataEntries = 10;

    public SpellHealLog() : base(Opcode.SMSG_SPELL_HEAL_LOG, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(TargetGUID);
        _worldPacket.WritePackedGuid128(CasterGUID);

        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteInt32(HealAmount);
        _worldPacket.WriteInt32(OriginalHealAmount);
        _worldPacket.WriteUInt32(OverHeal);
        _worldPacket.WriteUInt32(Absorbed);

        _worldPacket.WriteBit(Crit);

        _worldPacket.WriteBit(CritRollMade.HasValue);
        _worldPacket.WriteBit(CritRollNeeded.HasValue);
        _worldPacket.WriteBit(LogData != null);
        _worldPacket.WriteBit(ContentTuning != null);
        _worldPacket.FlushBits();

        if (LogData != null)
            LogData.Write(_worldPacket);

        if (CritRollMade.HasValue)
            _worldPacket.WriteFloat(CritRollMade.Value);

        if (CritRollNeeded.HasValue)
            _worldPacket.WriteFloat(CritRollNeeded.Value);

        if (ContentTuning != null)
            ContentTuning.Write(_worldPacket);
    }

    // MaxSize: 2 GUIDs (36) + 5 values (20) + bits (1) + SpellCastLogData (142) + 2 floats (8) + ContentTuning (22) = 229
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 20 + 1 +
        SpellPacketHelpers.SpellCastLogDataFixedSize + MaxPowerDataEntries * SpellPacketHelpers.SpellLogPowerDataSize +
        8 + SpellPacketHelpers.ContentTuningParamsSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(TargetGUID.Low, TargetGUID.High);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteInt32(HealAmount);
        writer.WriteInt32(OriginalHealAmount);
        writer.WriteUInt32(OverHeal);
        writer.WriteUInt32(Absorbed);

        writer.WriteBit(Crit);
        writer.WriteBit(CritRollMade.HasValue);
        writer.WriteBit(CritRollNeeded.HasValue);
        writer.WriteBit(LogData != null);
        writer.WriteBit(ContentTuning != null);
        writer.FlushBits();

        if (LogData != null)
        {
            if (!SpellPacketHelpers.WriteSpellCastLogData(ref writer, LogData, MaxPowerDataEntries))
                return -1;
        }

        if (CritRollMade.HasValue)
            writer.WriteFloat(CritRollMade.Value);

        if (CritRollNeeded.HasValue)
            writer.WriteFloat(CritRollNeeded.Value);

        if (ContentTuning != null)
            SpellPacketHelpers.WriteContentTuningParams(ref writer, ContentTuning);

        return writer.Position;
    }

    public WowGuid128 CasterGUID;
    public WowGuid128 TargetGUID;
    public uint SpellID;
    public int HealAmount;
    public int OriginalHealAmount;
    public uint OverHeal;
    public uint Absorbed;
    public bool Crit;
    public float? CritRollMade;
    public float? CritRollNeeded;
    public SpellCastLogData LogData = null!;
    public ContentTuningParams ContentTuning = null!;
}

public class SpellDelayed : ServerPacket, ISpanWritable
{
    public SpellDelayed() : base(Opcode.SMSG_SPELL_DELAYED, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WriteInt32(Delay);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4; // GUID + int

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteInt32(Delay);
        return writer.Position;
    }

    public WowGuid128 CasterGUID;
    public int Delay;
}

public class SpellChannelStart : ServerPacket, ISpanWritable
{
    public SpellChannelStart() : base(Opcode.SMSG_SPELL_CHANNEL_START, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32(SpellXSpellVisualID);
        _worldPacket.WriteUInt32(Duration);
        _worldPacket.WriteBit(InterruptImmunities != null);
        _worldPacket.WriteBit(HealPrediction != null);
        _worldPacket.FlushBits();

        if (InterruptImmunities != null)
            InterruptImmunities.Write(_worldPacket);

        if (HealPrediction != null)
            HealPrediction.Write(_worldPacket);
    }

    // MaxSize: GUID (18) + 3 uints (12) + bits (1) + InterruptImmunities (8) + HealPrediction (41) = 80
    // HealPrediction: TargetGUID (18) + Points (4) + Type (1) + BeaconGUID (18) = 41
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 12 + 1 + 8 + 41;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32(SpellXSpellVisualID);
        writer.WriteUInt32(Duration);
        writer.WriteBit(InterruptImmunities != null);
        writer.WriteBit(HealPrediction != null);
        writer.FlushBits();

        if (InterruptImmunities != null)
        {
            writer.WriteInt32(InterruptImmunities.SchoolImmunities);
            writer.WriteInt32(InterruptImmunities.Immunities);
        }

        if (HealPrediction != null)
        {
            writer.WritePackedGuid128(HealPrediction.TargetGUID.Low, HealPrediction.TargetGUID.High);
            writer.WriteUInt32(HealPrediction.Predict.Points);
            writer.WriteUInt8(HealPrediction.Predict.Type);
            writer.WritePackedGuid128(HealPrediction.Predict.BeaconGUID.Low, HealPrediction.Predict.BeaconGUID.High);
        }

        return writer.Position;
    }

    public WowGuid128 CasterGUID;
    public uint SpellID;
    public uint SpellXSpellVisualID;
    public uint Duration;
    public SpellChannelStartInterruptImmunities InterruptImmunities = null!;
    public SpellTargetedHealPrediction HealPrediction = null!;
}

public class SpellChannelStartInterruptImmunities
{
    public void Write(WorldPacket data)
    {
        data.WriteInt32(SchoolImmunities);
        data.WriteInt32(Immunities);
    }

    public int SchoolImmunities;
    public int Immunities;
}

public class SpellTargetedHealPrediction
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(TargetGUID);
        Predict.Write(data);
    }

    public WowGuid128 TargetGUID;
    public SpellHealPrediction Predict = null!;
}

public class SpellChannelUpdate : ServerPacket, ISpanWritable
{
    public SpellChannelUpdate() : base(Opcode.SMSG_SPELL_CHANNEL_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WriteInt32(TimeRemaining);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4; // GUID + int

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteInt32(TimeRemaining);
        return writer.Position;
    }

    public WowGuid128 CasterGUID;
    public int TimeRemaining;
}

class SpellPeriodicAuraLog : ServerPacket
{
    public SpellPeriodicAuraLog() : base(Opcode.SMSG_SPELL_PERIODIC_AURA_LOG, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(TargetGUID);
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteInt32(Effects.Count);
        _worldPacket.WriteBit(LogData != null);
        _worldPacket.FlushBits();

        foreach (var effect in Effects)
            effect.Write(_worldPacket);

        if (LogData != null)
            LogData.Write(_worldPacket);
    }

    public WowGuid128 TargetGUID;
    public WowGuid128 CasterGUID;
    public uint SpellID;
    public SpellCastLogData LogData = null!;
    public List<SpellLogEffect> Effects = new();

    public class PeriodicalAuraLogEffectDebugInfo
    {
        public float CritRollMade { get; set; }
        public float CritRollNeeded { get; set; }
    }

    public class SpellLogEffect
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Effect);
            data.WriteInt32(Amount);
            data.WriteInt32(OriginalDamage);
            data.WriteUInt32(OverHealOrKill);
            data.WriteUInt32(SchoolMaskOrPower);
            data.WriteUInt32(AbsorbedOrAmplitude);
            data.WriteUInt32(Resisted);

            data.WriteBit(Crit);
            data.WriteBit(DebugInfo != null);
            data.WriteBit(ContentTuning != null);
            data.FlushBits();

            if (ContentTuning != null)
                ContentTuning.Write(data);

            if (DebugInfo != null)
            {
                data.WriteFloat(DebugInfo.CritRollMade);
                data.WriteFloat(DebugInfo.CritRollNeeded);
            }
        }

        public uint Effect;
        public int Amount;
        public int OriginalDamage;
        public uint OverHealOrKill;
        public uint SchoolMaskOrPower;
        public uint AbsorbedOrAmplitude;
        public uint Resisted;
        public bool Crit;
        public PeriodicalAuraLogEffectDebugInfo DebugInfo = null!;
        public ContentTuningParams ContentTuning = null!;
    }
}

class SpellEnergizeLog : ServerPacket, ISpanWritable
{
    // Cap for power types in SpellCastLogData
    private const int MaxPowerDataEntries = 10;

    public SpellEnergizeLog() : base(Opcode.SMSG_SPELL_ENERGIZE_LOG, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(TargetGUID);
        _worldPacket.WritePackedGuid128(CasterGUID);

        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32((uint)Type);
        _worldPacket.WriteInt32(Amount);
        _worldPacket.WriteInt32(OverEnergize);

        _worldPacket.WriteBit(LogData != null);
        _worldPacket.FlushBits();

        if (LogData != null)
            LogData.Write(_worldPacket);
    }

    // MaxSize: 2 GUIDs (36) + 4 ints (16) + bit (1) + SpellCastLogData (22 + 10*12) = 195
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 16 + 1 + 22 + MaxPowerDataEntries * 12;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(TargetGUID.Low, TargetGUID.High);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32((uint)Type);
        writer.WriteInt32(Amount);
        writer.WriteInt32(OverEnergize);
        writer.WriteBit(LogData != null);
        writer.FlushBits();

        if (LogData != null)
        {
            if (!SpellPacketHelpers.WriteSpellCastLogData(ref writer, LogData, MaxPowerDataEntries))
                return -1;
        }

        return writer.Position;
    }

    public WowGuid128 TargetGUID;
    public WowGuid128 CasterGUID;
    public uint SpellID;
    public PowerType Type;
    public int Amount;
    public int OverEnergize;
    public SpellCastLogData LogData = null!;
}

class SpellDamageShield : ServerPacket, ISpanWritable
{
    private const int MaxPowerDataEntries = 10;

    public SpellDamageShield() : base(Opcode.SMSG_SPELL_DAMAGE_SHIELD, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(VictimGUID);
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteInt32(Damage);
        _worldPacket.WriteInt32(OriginalDamage);
        _worldPacket.WriteUInt32(OverKill);
        _worldPacket.WriteUInt32(SchoolMask);
        _worldPacket.WriteUInt32(LogAbsorbed);

        _worldPacket.WriteBit(LogData != null);
        _worldPacket.FlushBits();

        if (LogData != null)
            LogData.Write(_worldPacket);
    }

    // MaxSize: 2 GUIDs (36) + 6 ints (24) + bit (1) + SpellCastLogData (22 + 10*12) = 203
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 24 + 1 +
        SpellPacketHelpers.SpellCastLogDataFixedSize + MaxPowerDataEntries * SpellPacketHelpers.SpellLogPowerDataSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(VictimGUID.Low, VictimGUID.High);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteInt32(Damage);
        writer.WriteInt32(OriginalDamage);
        writer.WriteUInt32(OverKill);
        writer.WriteUInt32(SchoolMask);
        writer.WriteUInt32(LogAbsorbed);
        writer.WriteBit(LogData != null);
        writer.FlushBits();

        if (LogData != null)
        {
            if (!SpellPacketHelpers.WriteSpellCastLogData(ref writer, LogData, MaxPowerDataEntries))
                return -1;
        }

        return writer.Position;
    }

    public WowGuid128 VictimGUID;
    public WowGuid128 CasterGUID;
    public uint SpellID;
    public int Damage;
    public int OriginalDamage;
    public uint OverKill;
    public uint SchoolMask;
    public uint LogAbsorbed;
    public SpellCastLogData LogData = null!;
}

class EnvironmentalDamageLog : ServerPacket, ISpanWritable
{
    private const int MaxPowerDataEntries = 10;

    public EnvironmentalDamageLog() : base(Opcode.SMSG_ENVIRONMENTAL_DAMAGE_LOG) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Victim);
        _worldPacket.WriteUInt8((byte)Type);
        _worldPacket.WriteInt32(Amount);
        _worldPacket.WriteInt32(Resisted);
        _worldPacket.WriteInt32(Absorbed);

        _worldPacket.WriteBit(LogData != null);
        _worldPacket.FlushBits();

        if (LogData != null)
            LogData.Write(_worldPacket);
    }

    // MaxSize: 1 GUID (18) + byte + 3 ints (13) + bit (1) + SpellCastLogData (22 + 10*12) = 174
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 13 + 1 +
        SpellPacketHelpers.SpellCastLogDataFixedSize + MaxPowerDataEntries * SpellPacketHelpers.SpellLogPowerDataSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Victim.Low, Victim.High);
        writer.WriteUInt8((byte)Type);
        writer.WriteInt32(Amount);
        writer.WriteInt32(Resisted);
        writer.WriteInt32(Absorbed);
        writer.WriteBit(LogData != null);
        writer.FlushBits();

        if (LogData != null)
        {
            if (!SpellPacketHelpers.WriteSpellCastLogData(ref writer, LogData, MaxPowerDataEntries))
                return -1;
        }

        return writer.Position;
    }

    public WowGuid128 Victim;
    public EnvironmentalDamage Type;
    public int Amount;
    public int Resisted;
    public int Absorbed;
    public SpellCastLogData LogData = null!;
}

public class SpellInstakillLog : ServerPacket, ISpanWritable
{
    public SpellInstakillLog() : base(Opcode.SMSG_SPELL_INSTAKILL_LOG, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(TargetGUID);
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WriteUInt32(SpellID);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 4; // 2 GUIDs + uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(TargetGUID.Low, TargetGUID.High);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteUInt32(SpellID);
        return writer.Position;
    }

    public WowGuid128 TargetGUID;
    public WowGuid128 CasterGUID;
    public uint SpellID;
}

class SpellDispellLog : ServerPacket, ISpanWritable
{
    public SpellDispellLog() : base(Opcode.SMSG_SPELL_DISPELL_LOG, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBit(IsSteal);
        _worldPacket.WriteBit(IsBreak);
        _worldPacket.WritePackedGuid128(TargetGUID);
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WriteUInt32(DispelledBySpellID);

        _worldPacket.WriteInt32(DispellData.Count);
        foreach (var data in DispellData)
        {
            _worldPacket.WriteUInt32(data.SpellID);
            _worldPacket.WriteBit(data.Harmful);
            _worldPacket.WriteBit(data.Rolled.HasValue);
            _worldPacket.WriteBit(data.Needed.HasValue);
            if (data.Rolled.HasValue)
                _worldPacket.WriteInt32(data.Rolled.Value);
            if (data.Needed.HasValue)
                _worldPacket.WriteInt32(data.Needed.Value);

            _worldPacket.FlushBits();
        }
    }

    // Cap for dispells - typically 1-3 auras dispelled at once
    private const int MaxDispells = 8;
    // 1 byte (2 bits) + 2 GUIDs(36) + uint(4) + count(4) + entries(13 max each)
    // Each entry: SpellID(4) + 1 byte (3 bits flushed) + Rolled(4) + Needed(4) = 13
    public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 4 + 4 + MaxDispells * 13;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (DispellData.Count > MaxDispells)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBit(IsSteal);
        writer.WriteBit(IsBreak);
        writer.WritePackedGuid128(TargetGUID.Low, TargetGUID.High);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteUInt32(DispelledBySpellID);

        writer.WriteInt32(DispellData.Count);
        foreach (var data in DispellData)
        {
            writer.WriteUInt32(data.SpellID);
            writer.WriteBit(data.Harmful);
            writer.WriteBit(data.Rolled.HasValue);
            writer.WriteBit(data.Needed.HasValue);
            if (data.Rolled.HasValue)
                writer.WriteInt32(data.Rolled.Value);
            if (data.Needed.HasValue)
                writer.WriteInt32(data.Needed.Value);

            writer.FlushBits();
        }
        return writer.Position;
    }

    public bool IsSteal;
    public bool IsBreak;
    public WowGuid128 TargetGUID;
    public WowGuid128 CasterGUID;
    public uint DispelledBySpellID;
    public List<SpellDispellData> DispellData = new();
}

struct SpellDispellData
{
    public uint SpellID;
    public bool Harmful;
    public int? Rolled;
    public int? Needed;
}

class PlaySpellVisualKit : ServerPacket, ISpanWritable
{
    public PlaySpellVisualKit() : base(Opcode.SMSG_PLAY_SPELL_VISUAL_KIT) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Unit);
        _worldPacket.WriteUInt32(KitRecID);
        _worldPacket.WriteUInt32(KitType);
        _worldPacket.WriteUInt32(Duration);
        _worldPacket.WriteBit(MountedVisual);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 13; // GUID + 3 uint + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Unit.Low, Unit.High);
        writer.WriteUInt32(KitRecID);
        writer.WriteUInt32(KitType);
        writer.WriteUInt32(Duration);
        writer.WriteBit(MountedVisual);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 Unit;
    public uint KitRecID;
    public uint KitType;
    public uint Duration;
    public bool MountedVisual = false;
}

class ResurrectRequest : ServerPacket, ISpanWritable
{
    public ResurrectRequest() : base(Opcode.SMSG_RESURRECT_REQUEST) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(CasterGUID);
        _worldPacket.WriteUInt32(CasterVirtualRealmAddress);
        _worldPacket.WriteUInt32(PetNumber);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteBits(Name.GetByteCount(), 11);
        _worldPacket.WriteBit(UseTimer);
        _worldPacket.WriteBit(Sickness);
        _worldPacket.FlushBits();

        _worldPacket.WriteString(Name);
    }

    // MaxSize: GUID (18) + 3 uints (12) + bits (11+1+1=13 -> 2) + name (24) = 56
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 12 + 2 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(CasterGUID.Low, CasterGUID.High);
        writer.WriteUInt32(CasterVirtualRealmAddress);
        writer.WriteUInt32(PetNumber);
        writer.WriteUInt32(SpellID);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 11);
        writer.WriteBit(UseTimer);
        writer.WriteBit(Sickness);
        writer.FlushBits();

        writer.WriteString(Name);
        return writer.Position;
    }

    public WowGuid128 CasterGUID;
    public uint CasterVirtualRealmAddress;
    public uint PetNumber;
    public uint SpellID;
    public bool UseTimer = false;
    public bool Sickness;
    public string Name = string.Empty;
}

public class ResurrectResponse : ClientPacket
{
    public ResurrectResponse(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        CasterGUID = _worldPacket.ReadPackedGuid128();
        Response = _worldPacket.ReadUInt32();
    }

    public WowGuid128 CasterGUID;
    public uint Response;
}

class SelfRes : ClientPacket
{
    public SelfRes(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        SpellId = _worldPacket.ReadUInt32();
    }

    public uint SpellId;
}

class TotemCreated : ServerPacket, ISpanWritable
{
    public TotemCreated() : base(Opcode.SMSG_TOTEM_CREATED) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(Slot);
        _worldPacket.WritePackedGuid128(Totem);
        _worldPacket.WriteUInt32(Duration);
        _worldPacket.WriteUInt32(SpellId);
        _worldPacket.WriteFloat(TimeMod);
        _worldPacket.WriteBit(CannotDismiss);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size + 12 + 1; // byte + GUID + 2 uint + float + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8(Slot);
        writer.WritePackedGuid128(Totem.Low, Totem.High);
        writer.WriteUInt32(Duration);
        writer.WriteUInt32(SpellId);
        writer.WriteFloat(TimeMod);
        writer.WriteBit(CannotDismiss);
        writer.FlushBits();
        return writer.Position;
    }

    public byte Slot;
    public WowGuid128 Totem;
    public uint Duration;
    public uint SpellId;
    public float TimeMod = 1;
    public bool CannotDismiss = false;
}

class TotemDestroyed : ClientPacket
{
    public TotemDestroyed(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Slot = _worldPacket.ReadUInt8();
        Guid = _worldPacket.ReadPackedGuid128();
    }
    public byte Slot;
    public WowGuid128 Guid;
}

public class SetSpellModifier : ServerPacket, ISpanWritable
{
    public SetSpellModifier(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Modifiers.Count);
        foreach (SpellModifierInfo spellMod in Modifiers)
            spellMod.Write(_worldPacket);
    }

    private const int MaxModifiers = 8;
    private const int MaxDataPerModifier = 8;
    // 4 (count) + 8 * (1 + 4 + 8 * 5) = 4 + 8 * 45 = 364 bytes
    public int MaxSize => 4 + MaxModifiers * (1 + 4 + MaxDataPerModifier * 5);

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Modifiers.Count > MaxModifiers)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Modifiers.Count);
        foreach (SpellModifierInfo spellMod in Modifiers)
        {
            if (spellMod.ModifierData.Count > MaxDataPerModifier)
                return -1;

            writer.WriteUInt8(spellMod.ModIndex);
            writer.WriteInt32(spellMod.ModifierData.Count);
            foreach (SpellModifierData modData in spellMod.ModifierData)
            {
                writer.WriteInt32(modData.ModifierValue);
                writer.WriteUInt8(modData.ClassIndex);
            }
        }
        return writer.Position;
    }

    public List<SpellModifierInfo> Modifiers = new();
}

public class SpellModifierInfo
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8(ModIndex);
        data.WriteInt32(ModifierData.Count);
        foreach (SpellModifierData modData in ModifierData)
            modData.Write(data);
    }

    public byte ModIndex;
    public List<SpellModifierData> ModifierData = new();
}

public struct SpellModifierData
{
    public void Write(WorldPacket data)
    {
        data.WriteInt32(ModifierValue);
        data.WriteUInt8(ClassIndex);
    }

    public int ModifierValue;
    public byte ClassIndex;
}
