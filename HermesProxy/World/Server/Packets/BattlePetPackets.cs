using Framework.Constants;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;
using System.Linq;

namespace HermesProxy.World.Server.Packets;

// SMSG_BATTLE_PET_JOURNAL — V3_4_3.54261 writer from lineagedr/3.4.3_Source
// BattlePetPackets.cpp BattlePetJournal::Write (HasJournalLock after counts,
// before the lists). Do not copy current TrinityCore master; that moved the bit.
//
//   uint16  Trap
//   uint32  Slots.size()
//   uint32  Pets.size()
//   bit     HasJournalLock
//   FlushBits
//   Slots[] then Pets[]
public class BattlePetJournal : ServerPacket
{
    public const int SlotCount = 3;

    public BattlePetJournal() : base(Opcode.SMSG_BATTLE_PET_JOURNAL, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt16(Trap);
        _worldPacket.WriteUInt32((uint)Slots.Count);
        _worldPacket.WriteUInt32((uint)Pets.Count);
        _worldPacket.WriteBit(HasJournalLock);
        _worldPacket.FlushBits();

        foreach (var slot in Slots)
            slot.Write(_worldPacket);

        foreach (var pet in Pets)
            pet.Write(_worldPacket);
    }

    public ushort Trap;
    public bool HasJournalLock = true;
    public List<BattlePetSlot> Slots = [];
    public List<BattlePetInfo> Pets = [];

    public static BattlePetJournal FromSession(GameSessionData state)
    {
        var journal = new BattlePetJournal
        {
            Trap = 0,
            HasJournalLock = true,
        };

        for (byte i = 0; i < SlotCount; i++)
        {
            journal.Slots.Add(new BattlePetSlot
            {
                PetGuid = WowGuid128.Create(HighGuidType703.BattlePet, 0),
                CollarID = 0,
                Index = i,
                Locked = true,
            });
        }

        state.BattlePetGuidToSummonSpell.Clear();
        foreach (uint spellId in state.KnownSpells.OrderBy(id => id))
        {
            if (!GameData.TryGetBattlePetSpecies(spellId, out var species))
                continue;

            var guid = WowGuid128.Create(HighGuidType703.BattlePet, species.SpeciesId);
            state.BattlePetGuidToSummonSpell[guid] = spellId;
            ushort flags = 0;
            if (state.CollectionFavorites?.FavoritePetSpecies.Contains(species.SpeciesId) == true)
                flags |= BattlePetInfo.FavoriteFlag;

            journal.Pets.Add(new BattlePetInfo
            {
                Guid = guid,
                Species = species.SpeciesId,
                CreatureID = species.CreatureId,
                DisplayID = 0,
                Breed = 3,
                Level = 1,
                Exp = 0,
                Flags = flags,
                Power = 0,
                Health = 1,
                MaxHealth = 1,
                Speed = 0,
                Quality = 0,
                Name = string.Empty,
            });
        }

        return journal;
    }
}

public class BattlePetSlot
{
    public WowGuid128 PetGuid = WowGuid128.Empty;
    public uint CollarID;
    public byte Index;
    public bool Locked = true;

    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(PetGuid);
        data.WriteUInt32(CollarID);
        data.WriteUInt8(Index);
        data.WriteBit(Locked);
        data.FlushBits();
    }
}

public class BattlePetInfo
{
    public const ushort FavoriteFlag = 0x1;

    public WowGuid128 Guid;
    public uint Species;
    public uint CreatureID;
    public uint DisplayID;
    public ushort Breed;
    public ushort Level;
    public ushort Exp;
    public ushort Flags;
    public uint Power;
    public uint Health;
    public uint MaxHealth;
    public uint Speed;
    public byte Quality;
    public string Name = string.Empty;
    public bool HasOwnerInfo;

    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(Guid);
        data.WriteUInt32(Species);
        data.WriteUInt32(CreatureID);
        data.WriteUInt32(DisplayID);
        data.WriteUInt16(Breed);
        data.WriteUInt16(Level);
        data.WriteUInt16(Exp);
        data.WriteUInt16(Flags);
        data.WriteUInt32(Power);
        data.WriteUInt32(Health);
        data.WriteUInt32(MaxHealth);
        data.WriteUInt32(Speed);
        data.WriteUInt8(Quality);
        data.WriteBits((uint)Name.Length, 7);
        data.WriteBit(HasOwnerInfo);
        data.WriteBit(false);
        data.FlushBits();
        data.WriteString(Name);
    }
}

public readonly record struct BattlePetSummon(WowGuid128 PetGuid);

public readonly record struct BattlePetSetFlags(WowGuid128 PetGuid, ushort Flags, byte ControlType)
{
    public const byte ControlApply = 1;
    public const byte ControlRemove = 2;
}

public readonly record struct DismissCritter(WowGuid128 CritterGUID);
