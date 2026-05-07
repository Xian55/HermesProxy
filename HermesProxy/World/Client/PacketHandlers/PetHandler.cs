using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_PET_SPELLS_MESSAGE)]
    void HandlePetSpellsMessage(WorldPacket packet)
    {
        WowGuid64 guid = packet.ReadGuid();
        GetSession().GameState.CurrentPetGuid = guid.To128(GetSession().GameState);
        GetSession().GameState.ClearPendingPetCasts();

        // Equal to "Clear spells" pre cataclysm
        if (guid.IsEmpty())
        {
            PetClearSpells clear = new();
            SendPacketToClient(clear);
            return;
        }

        PetSpells spells = new();
        spells.PetGUID = guid.To128(GetSession().GameState);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            spells.CreatureFamily = packet.ReadUInt16();
        else
        {
            // For pre-3.1.0 servers (Vanilla/TBC), CreatureFamily is not in the packet.
            // Look it up from the creature template using the pet's entry ID.
            uint creatureEntry = GetSession().GameState.GetItemId(spells.PetGUID);
            if (creatureEntry != 0)
            {
                CreatureTemplate? template = GameData.GetCreatureTemplate(creatureEntry);
                if (template != null)
                    spells.CreatureFamily = (ushort)template.Family;
            }
        }

        spells.TimeLimit = packet.ReadUInt32();
        spells.ReactState = (ReactStates)packet.ReadUInt8();
        spells.CommandState = (CommandStates)packet.ReadUInt8();
        packet.ReadUInt8(); // unused
        spells.Flag = packet.ReadUInt8();

        const int maxCreatureSpells = 10;
        bool translateActionEncoding = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;
        for (int i = 0; i < maxCreatureSpells; i++) // Read pet/vehicle spell ids
        {
            uint raw = packet.ReadUInt32();
            // ActionButton encoding differs: 3.3.5a uses (state:8 | reserved:8 | spell:16),
            // V3_4_3 modern uses (slot:9 | spell:23). Translate only for V3_4_3 — V1_14
            // and V2_5 modern clients haven't been verified to use the same modern format,
            // so preserve the verbatim forward there.
            spells.ActionButtons[i] = translateActionEncoding
                ? TranslateLegacyPetActionButtonToV343(raw)
                : raw;
        }

        byte spellCount = packet.ReadUInt8();
        for (int i = 0; i < spellCount; i++)
            spells.Actions.Add(packet.ReadUInt32());

        byte cdCount = packet.ReadUInt8();
        for (int i = 0; i < cdCount; i++)
        {
            PetSpellCooldown cooldown = new();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                cooldown.SpellID = packet.ReadUInt32();
            else
                cooldown.SpellID = packet.ReadUInt16();

            cooldown.Category = packet.ReadUInt16();
            cooldown.Duration = packet.ReadUInt32();
            cooldown.CategoryDuration = packet.ReadUInt32();

            spells.Cooldowns.Add(cooldown);
        }
        
        SendPacketToClient(spells);
    }

    [PacketHandler(Opcode.SMSG_PET_ACTION_SOUND)]
    void HandlePetActionSound(WorldPacket packet)
    {
        PetActionSound sound = new PetActionSound();
        sound.UnitGUID = packet.ReadGuid().To128(GetSession().GameState);
        sound.Action = packet.ReadUInt32();
        SendPacketToClient(sound);
    }

    [PacketHandler(Opcode.SMSG_PET_BROKEN)]
    void HandlePetBroken(WorldPacket packet)
    {
        PrintNotification notify = new PrintNotification();
        notify.NotifyText = "Your pet has run away";
        SendPacketToClient(notify);
    }

    [PacketHandler(Opcode.SMSG_PET_UNLEARN_CONFIRM)]
    void HandlePetUnlearnConfirm(WorldPacket packet)
    {
        RespecWipeConfirm respec = new RespecWipeConfirm();
        respec.TrainerGUID = packet.ReadGuid().To128(GetSession().GameState);
        respec.Cost = packet.ReadUInt32();
        respec.RespecType = SpecResetType.PetTalents;
        SendPacketToClient(respec);
    }

    [PacketHandler(Opcode.MSG_LIST_STABLED_PETS)]
    void HandleListStabledPets(WorldPacket packet)
    {
        PetGuids pets = new PetGuids();
        var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(GetSession().GameState.CurrentPlayerGuid);
        int UNIT_FIELD_SUMMON = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMON);
        if (UNIT_FIELD_SUMMON >= 0 && updateFields != null && updateFields.ContainsKey(UNIT_FIELD_SUMMON))
        {
            WowGuid128 guid = GetGuidValue(updateFields, UnitField.UNIT_FIELD_SUMMON).To128(GetSession().GameState);
            if (!guid.IsEmpty())
                pets.Guids.Add(guid);
        }
        SendPacketToClient(pets);

        PetStableList stable = new PetStableList();
        stable.StableMaster = packet.ReadGuid().To128(GetSession().GameState);
        byte count = packet.ReadUInt8();
        stable.NumStableSlots = packet.ReadUInt8();
        for (byte i = 0; i < count; i++)
        {
            PetStableInfo pet = new PetStableInfo();
            pet.PetNumber = packet.ReadUInt32();
            pet.CreatureID = packet.ReadUInt32();
            pet.ExperienceLevel = packet.ReadUInt32();
            pet.PetName = packet.ReadCString();
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                pet.LoyaltyLevel = (byte)packet.ReadUInt32();
            pet.PetFlags = packet.ReadUInt8();

            if (pet.PetFlags != 1)
                pet.PetFlags = 3;

            CreatureTemplate? template = GameData.GetCreatureTemplate(pet.CreatureID);
            if (template != null)
                pet.DisplayID = template.Display.CreatureDisplay[0].CreatureDisplayID;
            else
            {
                WorldPacket query = new WorldPacket(Opcode.CMSG_QUERY_CREATURE);
                query.WriteUInt32(pet.CreatureID);
                query.WriteGuid(WowGuid64.Empty);
                SendPacket(query);
            }

            stable.Pets.Add(pet);
        }
        SendPacketToClient(stable);
    }

    [PacketHandler(Opcode.SMSG_PET_STABLE_RESULT)]
    void HandlePetStableResult(WorldPacket packet)
    {
        PetStableResult stable = new PetStableResult();
        stable.Result = packet.ReadUInt8();
        SendPacketToClient(stable);
    }

    [PacketHandler(Opcode.SMSG_PET_TAME_FAILURE)]
    void HandlePetTameFailure(WorldPacket packet)
    {
        PetTameFailure tameFailure = new PetTameFailure();
        tameFailure.Reason = packet.ReadUInt8();
        SendPacketToClient(tameFailure);
    }

    // Translate a 3.3.5a ActionButton (8-bit state byte at bits 24-31, 16-bit spell at
    // bits 0-15) into the V3_4_3 modern wire layout (9-bit slot at bits 23-31, 23-bit
    // spell at bits 0-22). Without this, low-spell-id buttons happen to read with the
    // right spell value but a junk slot, and high-spell-id auto-cast buttons render as
    // blank icons because slot 0xC1 doesn't map to anything the modern client knows.
    //
    // Slot mapping mirrors cMangos's ActiveStates enum and matches WPP V3_4_4
    // ReadPetAction344 modern slot vocabulary {0, 1, 6, 7, 0x101, 0x181}.
    private static uint TranslateLegacyPetActionButtonToV343(uint legacy)
    {
        if (legacy == 0)
            return 0;

        byte legacyState = (byte)((legacy >> 24) & 0xFF);
        ushort spellId = (ushort)(legacy & 0xFFFF);

        uint v343Slot = legacyState switch
        {
            0x07 => 7,      // CommandState (Attack/Follow/Stay)
            0x06 => 6,      // ReactState (Aggressive/Defensive/Passive)
            0x01 => 0x1,    // PassiveSpell
            0xC1 => 0x181,  // AutoCastSpell (enabled with autocast)
            0xC0 => 0x101,  // ManualSpell (active, no autocast)
            0x81 => 0x101,  // Disabled — keep spell visible, autocast off
            _    => 0,
        };

        return (v343Slot << 23) | spellId;
    }
}
