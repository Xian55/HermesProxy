using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

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

        // SMSG_PET_LEARNED_SPELLS must arrive BEFORE the pet's CreateObject — the V3_4_3
        // client's HasPetSpells() (which gates the spellbook pet-tab visibility) registers
        // spells from LEARNED_SPELLS into a buffer that gets bound at pet-create time.
        // Once the pet exists, post-create LEARNED_SPELLS go to action-bar bindings only,
        // not the spellbook. Capture-diff (World_native_pet_parsed.txt Numbers 1916-1924)
        // shows native order: 4× LEARNED_SPELLS at T=06.880-881 → pet CreateObject at
        // T=06.885 → PET_SPELLS_MESSAGE at T=06.891. Iter-10: emit LEARNED_SPELLS NOW,
        // before any cache-or-forward branch, so they reach the client before pet's
        // CreateObject regardless of which path PetSpells takes.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            EmitSynthesizedPetLearnedSpells(spells);

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

        // Specialization=0 matches CypherCore native (`Source/Game/Networking/Packets/PetPackets.cs:79`
        // writes WriteUInt16((ushort)Specialization) where m_petSpecialization defaults to 0).
        // Default `-1` here writes 0xFFFF on the wire — the V3_4_3 client may treat that as
        // an invalid spec and refuse to render the spellbook tab.
        spells.Specialization = 0;
        Log.Print(LogType.Trace,
            $"[PetSpellbookTab] emit summary: PetGUID={spells.PetGUID} family={spells.CreatureFamily} spec={spells.Specialization} → SMSG_PET_SPELLS_MESSAGE");
        SendPacketToClient(spells);
    }

    private void EmitSynthesizedPetLearnedSpells(PetSpells spells)
    {
        var seen = ExtractPetCastableSpellIds(spells);
        int totalActionButtons = 0;
        foreach (uint b in spells.ActionButtons) if (b != 0) totalActionButtons++;
        Log.Print(LogType.Trace,
            $"[PetSpellbookTab] castable count={seen.Count} ids=[{string.Join(",", seen)}] (filtered from {totalActionButtons} non-zero ActionButtons + {spells.Actions.Count} Actions; slot ∈ {{0x101, 0x181}})");
        if (seen.Count == 0) return;

        foreach (uint spellId in seen)
        {
            var learned = new PetLearnedSpells();
            learned.Spells.Add(spellId);
            SendPacketToClient(learned);
        }
    }

    // ActionButton slot encoding (modern V3_4_3 after TranslateLegacyPetActionButtonToV343):
    //   0x000 = Passive / Show
    //   0x001 = Hidden
    //   0x006 = ReactState (command/react byte, not a spell)
    //   0x007 = CommandState
    //   0x101 = Manual cast — castable spell, autocast off
    //   0x181 = AutoCast — castable spell, autocast on
    //
    // BOTH 0x101 (manual) and 0x181 (autocast) represent castable pet spells that should
    // appear in the spellbook tab. Whether autocast is on or off is a per-spell user toggle
    // (right-click in WoW UI), not a "this isn't a spell" signal. CypherCore native happens
    // to ship all 4 castables in 0x101 state, but TC pets routinely have autocast enabled
    // on default abilities (Bite, Claw, etc.) — they ship as 0x181.
    //
    // Iter-7 mistakenly restricted to 0x101 only (capture-diff saw native's 4×0x101 and
    // generalized incorrectly). Iter-9 fixed: include 0x181 too.
    //
    // The wider Actions list (passives + family abilities) is intentionally NOT replicated
    // as LEARNED_SPELLS — those aren't castable spellbook entries.
    // Public-static so QueryHandler.FlushDeferredUpdatesFor can call it on cached PendingPetSpells.
    public static HashSet<uint> ExtractPetCastableSpellIds(PetSpells spells)
    {
        var seen = new HashSet<uint>();
        foreach (uint button in spells.ActionButtons)
        {
            if (button == 0) continue;
            uint slot = button >> 23;
            uint spellId = button & 0x7FFFFF;
            if (spellId == 0) continue;
            if (slot != 0x101 && slot != 0x181) continue;  // castable: manual OR autocast
            seen.Add(spellId);
        }
        return seen;
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
            return legacy;

        // Otherwise treat as CMaNGOS legacy state-byte format.
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
