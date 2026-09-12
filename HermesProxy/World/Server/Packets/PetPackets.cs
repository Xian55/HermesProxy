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
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HermesProxy.World.Server.Packets;

public class PetSpells : ServerPacket
{
    public PetSpells() : base(Opcode.SMSG_PET_SPELLS_MESSAGE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(PetGUID);
        _worldPacket.WriteUInt16(CreatureFamily);
        _worldPacket.WriteInt16(Specialization);
        _worldPacket.WriteUInt32(TimeLimit);
        _worldPacket.WriteUInt16((ushort)((byte)CommandState | (Flag << 16)));
        _worldPacket.WriteUInt8((byte)ReactState);

        foreach (uint actionButton in ActionButtons)
            _worldPacket.WriteUInt32(actionButton);

        _worldPacket.WriteInt32(Actions.Count);
        _worldPacket.WriteInt32(Cooldowns.Count);
        _worldPacket.WriteInt32(SpellHistory.Count);

        foreach (uint action in Actions)
            _worldPacket.WriteUInt32(action);

        foreach (PetSpellCooldown cooldown in Cooldowns)
        {
            _worldPacket.WriteUInt32(cooldown.SpellID);
            _worldPacket.WriteUInt32(cooldown.Duration);
            _worldPacket.WriteUInt32(cooldown.CategoryDuration);
            _worldPacket.WriteFloat(cooldown.ModRate);
            _worldPacket.WriteUInt16(cooldown.Category);
        }

        foreach (PetSpellHistory history in SpellHistory)
        {
            _worldPacket.WriteUInt32(history.CategoryID);
            _worldPacket.WriteUInt32(history.RecoveryTime);
            _worldPacket.WriteFloat(history.ChargeModRate);
            _worldPacket.WriteInt8(history.ConsumedCharges);
        }
    }

    public WowGuid128 PetGUID;
    public ushort CreatureFamily;
    public short Specialization = -1;
    public uint TimeLimit;
    public ReactStates ReactState;
    public CommandStates CommandState;
    public byte Flag;

    public uint[] ActionButtons = new uint[10];

    public List<uint> Actions = new();
    public List<PetSpellCooldown> Cooldowns = new();
    public List<PetSpellHistory> SpellHistory = new();
}

public class PetSpellCooldown
{
    public uint SpellID;
    public uint Duration;
    public uint CategoryDuration;
    public float ModRate = 1.0f;
    public ushort Category;
}

public class PetSpellHistory
{
    public uint CategoryID;
    public uint RecoveryTime;
    public float ChargeModRate = 1.0f;
    public sbyte ConsumedCharges;
}

public class PetClearSpells : ServerPacket, ISpanWritable
{
    public PetClearSpells() : base(Opcode.SMSG_PET_CLEAR_SPELLS, ConnectionType.Instance) { }

    public override void Write()
    {
    }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

// V3_4_3 SMSG_SET_PET_SPECIALIZATION (opcode 0x2625 / 9765). Body is a single uint16
// SpecID. CypherCore (X:/Programming/CypherCoreClassicWOTLK/Source/Game/Networking/Packets/PetPackets.cs:263)
// emits this after PetSpells when the pet's specialization changes — the V3_4_3 client
// uses it to bind the pet's spell list to a spellbook tab. Legacy 3.3.5a doesn't send
// this opcode, so we synthesize it after every SMSG_PET_SPELLS_MESSAGE forward.
public sealed class SetPetSpecialization : ServerPacket
{
    public ushort SpecID;
    public SetPetSpecialization() : base(Opcode.SMSG_SET_PET_SPECIALIZATION, ConnectionType.Instance) { }
    public override void Write() => _worldPacket.WriteUInt16(SpecID);
}

// V3_4_3 wire format verified against CypherCore native sniff (World_pet_actionbar_portrait.pkt
// SMSG_PET_LEARNED_SPELLS opcode 0x2C4C, body = uint32 count + uint32[] spells, 8 bytes for 1 spell).
// Legacy 3.3.5a opcode 1177 ships ONE spell per packet — wrap into length-1 list at the handler.
public class PetLearnedSpells : ServerPacket
{
    public PetLearnedSpells() : base(Opcode.SMSG_PET_LEARNED_SPELLS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Spells.Count);
        foreach (uint spell in Spells)
            _worldPacket.WriteUInt32(spell);
    }

    public List<uint> Spells = new();
}

public class PetUnlearnedSpells : ServerPacket
{
    public PetUnlearnedSpells() : base(Opcode.SMSG_PET_UNLEARNED_SPELLS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Spells.Count);
        foreach (uint spell in Spells)
            _worldPacket.WriteUInt32(spell);
    }

    public List<uint> Spells = new();
}

public readonly record struct PetAction(WowGuid128 PetGUID, uint Action, WowGuid128 TargetGUID, Vector3 ActionPosition);

public readonly record struct PetStopAttack(WowGuid128 PetGUID);

public readonly record struct PetSetAction(WowGuid128 PetGUID, uint Index, uint Action);

class PetActionSound : ServerPacket, ISpanWritable
{
    public PetActionSound() : base(Opcode.SMSG_PET_ACTION_SOUND) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(UnitGUID);
        _worldPacket.WriteUInt32(Action);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4; // GUID + uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(UnitGUID.Low, UnitGUID.High);
        writer.WriteUInt32(Action);
        return writer.Position;
    }

    public WowGuid128 UnitGUID;
    public uint Action;
}

public readonly record struct PetRename(PetRenameData RenameData);

public struct PetRenameData
{
    public WowGuid128 PetGUID;
    public int PetNumber;
    public string NewName;
    public bool HasDeclinedNames;
    public DeclinedName DeclinedNames;
}

public readonly record struct PetAbandon(WowGuid128 PetGUID);

public readonly record struct RequestStabledPets(WowGuid128 StableMaster);

class PetStableList : ServerPacket, ISpanWritable
{
    public PetStableList() : base(Opcode.SMSG_PET_STABLE_LIST, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(StableMaster);
        _worldPacket.WriteInt32(Pets.Count);
        _worldPacket.WriteUInt8(NumStableSlots);
        foreach (PetStableInfo pet in Pets)
        {
            _worldPacket.WriteUInt32(pet.PetNumber);
            _worldPacket.WriteUInt32(pet.CreatureID);
            _worldPacket.WriteUInt32(pet.DisplayID);
            _worldPacket.WriteUInt32(pet.ExperienceLevel);
            _worldPacket.WriteUInt8(pet.LoyaltyLevel);
            _worldPacket.WriteUInt8(pet.PetFlags);
            _worldPacket.WriteBits(pet.PetName.GetByteCount(), 8);
            _worldPacket.WriteString(pet.PetName);
        }
    }

    // Cap for stable slots - max 5 stable slots + active
    private const int MaxPets = 6;
    // Cap for pet name - 8 bits = max 256
    private const int MaxPetNameBytes = 64;
    // Per pet: 4 uints(16) + 2 bytes(2) + bits(1) + name
    private const int PetInfoSize = 16 + 2 + 1 + MaxPetNameBytes;
    // GUID(18) + count(4) + byte(1) + pets
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + 1 + MaxPets * PetInfoSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Pets.Count > MaxPets)
            return -1;

        // Pre-validate name lengths
        foreach (var pet in Pets)
        {
            if (Encoding.UTF8.GetByteCount(pet.PetName) > MaxPetNameBytes)
                return -1;
        }

        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(StableMaster.Low, StableMaster.High);
        writer.WriteInt32(Pets.Count);
        writer.WriteUInt8(NumStableSlots);
        foreach (PetStableInfo pet in Pets)
        {
            writer.WriteUInt32(pet.PetNumber);
            writer.WriteUInt32(pet.CreatureID);
            writer.WriteUInt32(pet.DisplayID);
            writer.WriteUInt32(pet.ExperienceLevel);
            writer.WriteUInt8(pet.LoyaltyLevel);
            writer.WriteUInt8(pet.PetFlags);
            writer.WriteBits((uint)Encoding.UTF8.GetByteCount(pet.PetName), 8);
            writer.WriteString(pet.PetName);
        }
        return writer.Position;
    }

    public WowGuid128 StableMaster;
    public byte NumStableSlots;
    public List<PetStableInfo> Pets = new();
}

/// <summary>
/// V3_4_3 replacement for <see cref="PetStableList"/>. The 3.4.3 protocol has no
/// SMSG_PET_STABLE_LIST — the stable moved into ActivePlayerData, and the native server
/// delivers it as a hand-built SMSG_UPDATE_OBJECT Values block on the player
/// (3.4.3_Source Player.cpp:27589 Player::SendStable). The descriptor path cannot express
/// it, so this mirrors the native writer byte for byte.
///
/// Layout verified against a native Wrathion capture and confirmed in-client with four
/// stabled pets. Do NOT trust WowPacketParser here: its ReadUpdateStableInfo
/// (UpdateFieldsHandler343.cs:2434) reads PetSlot first and expects a trailing
/// PackedGuid128 that is not on the wire, so it shifts every field by one and throws.
/// </summary>
class PetStableUpdate : ServerPacket
{
    public PetStableUpdate() : base(Opcode.SMSG_UPDATE_OBJECT, ConnectionType.Instance) { }

    public override void Write()
    {
        // The framing carries two lengths that are only known once the body exists, and
        // ByteBuffer has no in-place patch, so the payload is measured before framing.
        byte[] guidBytes = BuildPackedGuid(PlayerGuid);
        byte[] payload = BuildFieldPayload();

        // dataSize counts from the UpdateType byte onward — native's `pkt.size() - 11`.
        uint dataSize = (uint)(1 + guidBytes.Length + 4 + payload.Length);

        _worldPacket.WriteUInt32(1);                    // NumObjUpdates
        _worldPacket.WriteUInt16((ushort)MapId);
        _worldPacket.WriteUInt8(0);                     // HasRemovedObjects
        _worldPacket.WriteUInt32(dataSize);
        _worldPacket.WriteUInt8(0);                     // UpdateType: Values
        _worldPacket.WriteBytes(guidBytes);
        _worldPacket.WriteUInt32((uint)payload.Length);
        _worldPacket.WriteBytes(payload);
    }

    private static byte[] BuildPackedGuid(WowGuid128 guid)
    {
        WorldPacket buffer = new WorldPacket();
        buffer.WritePackedGuid128(guid);
        return Trim(buffer);
    }

    /// <summary>
    /// Everything after the field-size prefix. The leading constants are the changed-mask
    /// cascade that selects ActivePlayerData's PetStable block; they are reproduced from
    /// the native writer rather than derived, because the descriptor generator has no
    /// PetStable definition to derive them from.
    /// </summary>
    private byte[] BuildFieldPayload()
    {
        WorldPacket buffer = new WorldPacket();
        buffer.WriteUInt32(128);
        buffer.WriteUInt32(8);
        buffer.WriteUInt16(0);
        buffer.WriteUInt32(1073741828);
        buffer.WriteUInt8(128);
        buffer.WriteUInt32(224);

        // Low 3 bits carry the entry count, the rest is the slot mask; 31 means "empty".
        buffer.WriteUInt8(Pets.Count > 0 ? (byte)(32 * Pets.Count + 32 - 1) : (byte)31);

        foreach (PetStableInfo pet in Pets)
        {
            buffer.WriteBits(255u, 8);                  // update-all mask for this entry
            buffer.FlushBits();

            buffer.WriteUInt32(pet.PetNumber);
            buffer.WriteUInt32(pet.CreatureID);
            buffer.WriteUInt32(pet.DisplayID);
            buffer.WriteUInt32(pet.ExperienceLevel);
            buffer.WriteUInt8(pet.PetSlot);
            buffer.WriteUInt8(pet.PetFlags);

            buffer.WriteBits((uint)pet.PetName.GetByteCount(), 8);
            buffer.WriteString(pet.PetName);
            buffer.FlushBits();
        }

        // Omitted when the list was not opened at a stable master, matching native: a
        // "call stabled pet" style spell has no object to attribute the window to.
        if (!StableMaster.IsEmpty())
            buffer.WritePackedGuid128(StableMaster);

        return Trim(buffer);
    }

    private static byte[] Trim(WorldPacket buffer)
    {
        buffer.FlushBits();
        byte[] data = buffer.GetData();
        int size = (int)buffer.GetSize();
        if (data.Length == size)
            return data;
        byte[] exact = new byte[size];
        Array.Copy(data, exact, size);
        return exact;
    }

    public WowGuid128 PlayerGuid;
    public WowGuid128 StableMaster;
    public uint MapId;
    public List<PetStableInfo> Pets = new();
}

class PetStableInfo
{
    public uint PetNumber;
    public uint CreatureID;
    public uint DisplayID;
    public uint ExperienceLevel;
    public byte LoyaltyLevel = 1;
    /// <summary>V3_4_3 slot byte; 0xFF marks the first entry (native Player::SendStable).</summary>
    public byte PetSlot;
    public byte PetFlags;
    public string PetName = string.Empty;
}

public readonly record struct BuyStableSlot(WowGuid128 StableMaster);

public class PetGuids : ServerPacket, ISpanWritable
{
    public PetGuids() : base(Opcode.SMSG_PET_GUIDS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Guids.Count);
        foreach (var guid in Guids)
            _worldPacket.WritePackedGuid128(guid);
    }

    // Cap for pet GUIDs - max 5 stable slots + active pet
    private const int MaxPets = 6;
    // count(4) + GUIDs(18 each)
    public int MaxSize => 4 + MaxPets * PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Guids.Count > MaxPets)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Guids.Count);
        foreach (var guid in Guids)
            writer.WritePackedGuid128(guid.Low, guid.High);
        return writer.Position;
    }

    public List<WowGuid128> Guids = new List<WowGuid128>();
}

class PetStableResult : ServerPacket, ISpanWritable
{
    public PetStableResult() : base(Opcode.SMSG_PET_STABLE_RESULT, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(Result);
    }

    public int MaxSize => 1; // byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8(Result);
        return writer.Position;
    }

    public byte Result;
}

sealed class PetTameFailure : ServerPacket, ISpanWritable
{
    public PetTameFailure() : base(Opcode.SMSG_PET_TAME_FAILURE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(Reason);
    }

    public int MaxSize => 1;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8(Reason);
        return writer.Position;
    }

    public byte Reason;
}

public readonly record struct StablePet(WowGuid128 StableMaster);

public readonly record struct UnstablePet(uint PetNumber, WowGuid128 StableMaster);

public readonly record struct StableSwapPet(uint PetNumber, WowGuid128 StableMaster);

public readonly record struct PetCancelAura(WowGuid128 PetGUID, uint SpellID);

