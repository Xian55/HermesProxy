using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

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
            GetSession().GameState.PendingPetSpells = null;
            GetSession().GameState.PendingPetSpellsLegacyGuid = null;
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
        {
            uint raw = packet.ReadUInt32();
            // Actions list uses the same encoding as ActionButtons. Native TC 3.4.3
            // sniff shows entries packed as (slot:9 | spell:23) — without translation
            // the V3_4_3 client mis-decodes the spell IDs and refuses to bind the
            // spellbook pet tab. ActionButtons already get this treatment above.
            spells.Actions.Add(translateActionEncoding
                ? TranslateLegacyPetActionButtonToV343(raw)
                : raw);
        }

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

        // Real SMSG_PET_LEARNED_SPELLS from the legacy server (opcode 0x499) is
        // forwarded by HandlePetLearnedSpells at the bottom of this file — fires
        // only on actual learn events (tame, level-up). Native TC 3.4.3 sniff
        // confirms no LEARNED is emitted on re-summon / zone / login when the
        // pet's spells are already known.

        // V3_4_3 + cmangos: SMSG_PET_SPELLS_MESSAGE arrives BEFORE the pet's
        // CreateObject. spells.PetGUID was just translated against an empty pet
        // map, so its entry slot is pet_number (e.g. 9568) instead of
        // creature_template.entry (e.g. 2031). The pet's CreateObject will later
        // ship with the corrected GUID — so the client receives a spells message
        // for a unit GUID that never appears, and never binds the pet UI.
        // Hold the parsed message; UpdateHandler.HandleUpdateObject flushes it
        // (with re-translated PetGUID) right after the pet's CreateObject is
        // sent. If the pet is already in ClientKnownGuids (TC backends, or a
        // second SMSG_PET_SPELLS_MESSAGE on the same pet), forward immediately.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 &&
            !GetSession().GameState.ClientKnownGuids.Contains(spells.PetGUID))
        {
            GetSession().GameState.PendingPetSpells = spells;
            GetSession().GameState.PendingPetSpellsLegacyGuid = guid;
            Log.Print(LogType.Trace,
                $"[PetSpellsHold] caching SMSG_PET_SPELLS_MESSAGE for legacy guid={guid} stalePetGUID={spells.PetGUID} (pet not yet in ClientKnownGuids; LEARNED_SPELLS already emitted ahead of CreateObject)");
            return;
        }

        // Specialization stays at its default -1 (0xFFFF on wire). Native TC 3.4.3
        // sniff (World_hunter_pet_tame_pet_actionbar_pet_spellbook) shows every
        // SMSG_PET_SPELLS_MESSAGE — tame, re-summon, login, zone — carries -1 for
        // hunter pets; warlock pets are what use 0..N spec values.
        Log.Print(LogType.Trace,
            $"[PetSpellbookTab] emit summary: PetGUID={spells.PetGUID} family={spells.CreatureFamily} spec={spells.Specialization} → SMSG_PET_SPELLS_MESSAGE");
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

    [PacketHandler(Opcode.SMSG_PET_LEARNED_SPELLS)]
    void HandlePetLearnedSpells(WorldPacket packet)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        var learned = new PetLearnedSpells();
        learned.Spells.Add(packet.ReadUInt32());
        SendPacketToClient(learned);
    }

    [PacketHandler(Opcode.SMSG_PET_UNLEARNED_SPELLS)]
    void HandlePetUnlearnedSpells(WorldPacket packet)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        var unlearned = new PetUnlearnedSpells();
        unlearned.Spells.Add(packet.ReadUInt32());
        SendPacketToClient(unlearned);
    }

    // Translate a pet ActionButton from the legacy backend's wire format into V3_4_3
    // modern (`slot:9 | spell:23`). Two backends ship different formats:
    //
    //   - **CMaNGOS 3.3.5a** uses pre-WotLK-Classic encoding: `state:8 | reserved:8 | spell:16`
    //     where `state ∈ {0x01 Passive, 0x06 React, 0x07 Command, 0x81 Disabled, 0xC1 Enabled}`
    //     (Unit.h:869-874 ActiveStates). We unpack and remap.
    //
    //   - **TrinityCore wotlk_classic** has been forward-ported and emits **modern V3_4_3 slot
    //     values directly** (`UnitDefines.h:504-507`): `ACT_DISABLED = 0x101`, `ACT_ENABLED = 0x181`,
    //     `ACT_COMMAND = 0x07`, `ACT_REACTION = 0x06`, `ACT_PASSIVE = 0x01`. The wire packing
    //     is already `slot:9 | spell:23` — no translation needed.
    //
    // We auto-detect: if the high 9 bits are a modern slot value, pass through. Otherwise
    // treat the high byte as a CMaNGOS state byte and translate.
    //
    // Without this, TC's already-modern manual-cast buttons (slot=0x101, high byte=0x80)
    // hit the legacy switch's `_ => 0` fallback — the slot becomes 0 ("Passive"), which the
    // pet spellbook tab logic ignores. Spell IDs survive (low 16 bits) so the action bar
    // still displays icons, but the spellbook tab never renders.
    private static uint TranslateLegacyPetActionButtonToV343(uint legacy)
    {
        if (legacy == 0)
            return 0;

        // Modern V3_4_3 slot vocabulary (matches WPP ReadPetAction344): pass through.
        uint maybeModernSlot = legacy >> 23;
        if (maybeModernSlot == 0x000 || maybeModernSlot == 0x001 ||
            maybeModernSlot == 0x006 || maybeModernSlot == 0x007 ||
            maybeModernSlot == 0x101 || maybeModernSlot == 0x181)
        {
            // TC's CharmInfo ships vehicle action-bar abilities with slot=0x000 ("Decide"
            // in CypherCore's enum). The V3_4_3 client doesn't render slot=0 as a clickable
            // button — IsSpell returns false. Rewrite slot=0 + non-zero spellId to ManualCast
            // (0x101) so vehicles like the Havenshire Mare (Grand Theft Palomino quest 12680)
            // get their abilities (52264 Charge, 52268 Buck) shown as clickable bar entries.
            // True passive auras live in the Actions list, not ActionButtons, so this won't
            // misclassify pet passives.
            uint modernSpellId = legacy & 0x7FFFFF;
            if (maybeModernSlot == 0x000 && modernSpellId != 0)
                return (0x101u << 23) | modernSpellId;
            return legacy;
        }

        // Otherwise treat as CMaNGOS legacy state-byte format.
        byte legacyState = (byte)((legacy >> 24) & 0xFF);
        ushort spellId = (ushort)(legacy & 0xFFFF);

        // TC packs vehicle action buttons as (slot_index:8 | 0:8 | spellId:16) — the high
        // byte is a UI position (0x08..0x11 for the vehicle bar), not a CharmInfo state.
        // Anything outside the known CharmInfo state values that still carries a non-zero
        // spell ID must be a vehicle/charm castable; map to ManualCast (0x101) so the V3_4_3
        // client renders it as a clickable bar entry. Empty slots (high byte = index,
        // spell = 0) stay 0.
        // EXCEPTION: vehicle slots may also hold passive control auras (e.g. Frostbrood
        // Vanquisher Flight 53112 in DK quest 12779). TC's VehicleSpellInitialize ships
        // every m_spells[] entry — including IsPassive() ones — into the action bar with
        // the slot_idx-in-high-byte encoding. Native Blizzard wow showed an empty slot for
        // these (the passive aura is cast on the vehicle server-side, no action button).
        // Drop the entry (return 0) for known passives via GameData.PassiveSpells so the
        // V3_4_3 client renders the slot as empty.
        if (legacyState != 0x07 && legacyState != 0x06 && legacyState != 0x01 &&
            legacyState != 0xC1 && legacyState != 0xC0 && legacyState != 0x81 &&
            spellId != 0 && GameData.PassiveSpells.Contains(spellId))
        {
            return 0;
        }

        uint v343Slot = legacyState switch
        {
            0x07 => 7,      // CommandState (Attack/Follow/Stay)
            0x06 => 6,      // ReactState (Aggressive/Defensive/Passive)
            0x01 => 0x1,    // PassiveSpell
            0xC1 => 0x181,  // AutoCastSpell (enabled with autocast)
            0xC0 => 0x101,  // ManualSpell (active, no autocast)
            0x81 => 0x101,  // Disabled — keep spell visible, autocast off
            _    => spellId != 0 ? 0x101u : 0u,  // TC vehicle button (e.g. Havenshire Mare 28606 → 52264 Charge)
        };

        return (v343Slot << 23) | spellId;
    }
}
