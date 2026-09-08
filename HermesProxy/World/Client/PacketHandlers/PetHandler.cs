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

        Span<uint> rawButtons = stackalloc uint[maxCreatureSpells];
        bool isVehicleBar = false;
        for (int i = 0; i < maxCreatureSpells; i++) // Read pet/vehicle spell ids
        {
            uint raw = packet.ReadUInt32();
            rawButtons[i] = raw;
            isVehicleBar |= IsLegacyVehicleBarSlot((byte)(raw >> 24));
        }
        isVehicleBar &= translateActionEncoding;

        for (int i = 0; i < maxCreatureSpells; i++)
        {
            uint raw = rawButtons[i];

            // A legacy vehicle bar carries the UI position (i + 8) in the high byte rather
            // than a CharmInfo state. Native V3_4_3 keeps that position — every slot ships
            // as MAKE_UNIT_ACTION_BUTTON_VEHICLE(spellId, i + 8), including the empty ones
            // (Player.cpp:21560/21582), so the whole bar is rebuilt index-first here instead
            // of per-button. See the reference capture wrathion_343_toc_vehicle_actionbar.
            if (isVehicleBar)
            {
                uint vehicleSpellId = raw & LegacyActionButtonSpellMask;

                // Passive control auras (e.g. Frostbrood Vanquisher Flight 53112) are cast on
                // the vehicle server-side; the retail bar shows an empty slot for them. Keep
                // the slot so the position still lines up with native.
                if (vehicleSpellId != 0 && GameData.PassiveSpells.Contains(vehicleSpellId))
                    vehicleSpellId = 0;

                spells.ActionButtons[i] = ((uint)(i + VehicleActionBarFirstSlot) << 23) | vehicleSpellId;
                continue;
            }

            // ActionButton encoding differs: 3.3.5a uses (state:8 | spell:24),
            // V3_4_3 modern uses (slot:9 | spell:23). Translate only for V3_4_3 — V1_14
            // and V2_5 modern clients haven't been verified to use the same modern format,
            // so preserve the verbatim forward there.
            spells.ActionButtons[i] = translateActionEncoding
                ? TranslateLegacyPetActionButtonToV343(raw)
                : raw;
        }

        // Native V3_4_3 ships Specialization = 0 for vehicles (Player::VehicleSpellInitialize)
        // and -1 for hunter pets (see the emit comment below).
        if (isVehicleBar)
            spells.Specialization = 0;

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

        // Parsed into plain locals rather than straight into PetStableList: on V3_4_3 that
        // packet's opcode resolves to 0 and its ServerPacket constructor throws, which used
        // to abort this handler before anything reached the client (issue #224).
        WowGuid128 stableMaster = packet.ReadGuid().To128(GetSession().GameState);
        byte count = packet.ReadUInt8();
        byte numStableSlots = packet.ReadUInt8();
        // The client greys out slots it believes are unpurchased and only ever learns the count
        // from ActivePlayerData. Cache it for the next CreateObject, and push a Values update
        // now so an already-logged-in player sees a slot unlock the moment it is bought.
        GetSession().GameState.NumStableSlots = numStableSlots;
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            ObjectUpdate slotUpdate = new ObjectUpdate(GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
            slotUpdate.EnsureActivePlayerData().NumStableSlots = numStableSlots;
            UpdateObject slotPacket = new UpdateObject(GetSession().GameState);
            slotPacket.ObjectUpdates.Add(slotUpdate);
            SendPacketToClient(slotPacket);
        }
        List<PetStableInfo> stabledPets = new();
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

            stabledPets.Add(pet);
        }

        // V3_4_3 deleted SMSG_PET_STABLE_LIST; the stable lives in ActivePlayerData and the
        // native server ships it as a hand-built SMSG_UPDATE_OBJECT (issue #224).
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            SendPacketToClient(BuildStableUpdate(stableMaster, stabledPets));
            return;
        }

        PetStableList stable = new PetStableList();
        stable.StableMaster = stableMaster;
        stable.NumStableSlots = numStableSlots;
        stable.Pets.AddRange(stabledPets);
        SendPacketToClient(stable);
    }

    /// <summary>
    /// Reshapes the decoded legacy stable list into the V3_4_3 ActivePlayerData block.
    /// Native writes the stabled pets first — the first entry carrying the 0xFF sentinel —
    /// and the currently summoned pet last in slot 0, so the client can tell which row is
    /// the active companion rather than a stabled one.
    /// </summary>
    private PetStableUpdate BuildStableUpdate(WowGuid128 stableMaster, List<PetStableInfo> pets)
    {
        PetStableUpdate update = new PetStableUpdate();
        update.PlayerGuid = GetSession().GameState.CurrentPlayerGuid;
        update.StableMaster = stableMaster;
        update.MapId = GetSession().GameState.CurrentMapId ?? 0;

        // Legacy flag 1 marks the pet that is currently out; everything else is stabled.
        int stabledCount = 0;
        foreach (PetStableInfo pet in pets)
        {
            if (pet.PetFlags == 1)
                continue;

            pet.PetSlot = stabledCount == 0 ? (byte)0xFF : (byte)(stabledCount + 1);
            update.Pets.Add(pet);
            stabledCount++;
        }

        foreach (PetStableInfo pet in pets)
        {
            if (pet.PetFlags != 1)
                continue;

            pet.PetSlot = 0;
            update.Pets.Add(pet);
        }

        return update;
    }

    [PacketHandler(Opcode.SMSG_PET_STABLE_RESULT)]
    void HandlePetStableResult(WorldPacket packet)
    {
        PetStableResult stable = new PetStableResult();
        stable.Result = packet.ReadUInt8();
        SendPacketToClient(stable);

        // Legacy answers a slot purchase with this result and nothing else — it never
        // re-sends the stable list, and 3.3.5a has no update field carrying the count. The
        // modern client only unlocks a slot from ActivePlayerData, so without re-requesting
        // the list here the newly bought slot stays red until the window is reopened (#224).
        const byte StableSuccessBuySlot = 0x0A;
        if (stable.Result == StableSuccessBuySlot &&
            GetSession().GameState.LastStableMaster is { } stableMaster)
        {
            WorldPacket relist = new WorldPacket(Opcode.MSG_LIST_STABLED_PETS);
            relist.WriteGuid(stableMaster.To64());
            SendPacket(relist);
        }
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
    // 3.3.5a packs the action button as (state:8 | spell:24) — AzerothCore, mangos-wotlk and
    // VMaNGOS all define UNIT_ACTION_BUTTON_ACTION(X) as X & 0x00FFFFFF. Masking to 16 bits
    // silently drops the high byte of every spell id above 65535, which is most of the 3.2+
    // vehicle content (issue #264: Argent Warhorse 68505 arrived as 2969).
    private const uint LegacyActionButtonSpellMask = 0x00FFFFFF;

    // Modern V3_4_3 packs (slot:9 | spell:23).
    private const uint V343ActionButtonSpellMask = 0x007FFFFF;

    // TC/AC vehicle bars use the UI position (index + 8) as the high byte, which cannot
    // collide with the CharmInfo ActiveStates values (0x00, 0x01, 0x06, 0x07, 0x81, 0xC0, 0xC1).
    private const int VehicleActionBarFirstSlot = 8;
    private const byte VehicleActionBarLastSlot = VehicleActionBarFirstSlot + 9;

    private static bool IsLegacyVehicleBarSlot(byte legacyState)
        => legacyState >= VehicleActionBarFirstSlot && legacyState <= VehicleActionBarLastSlot;

    internal static uint TranslateLegacyPetActionButtonToV343(uint legacy)
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
            uint modernSpellId = legacy & V343ActionButtonSpellMask;
            if (maybeModernSlot == 0x000 && modernSpellId != 0)
                return (0x101u << 23) | modernSpellId;
            return legacy;
        }

        // Otherwise treat as CMaNGOS legacy state-byte format.
        byte legacyState = (byte)((legacy >> 24) & 0xFF);
        uint spellId = legacy & LegacyActionButtonSpellMask;

        // TC packs vehicle action buttons as (slot_index:8 | spellId:24) — the high byte is
        // a UI position (0x08..0x11 for the vehicle bar), not a CharmInfo state. The
        // ActionButtons array is rebuilt index-first by the caller; this path only sees such
        // entries via the Actions list, where the position is preserved as the modern slot.
        // Anything else outside the known CharmInfo state values that still carries a
        // non-zero spell ID is treated as a charm castable and mapped to ManualCast (0x101)
        // so the V3_4_3 client renders it as a clickable bar entry.
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
            _ when IsLegacyVehicleBarSlot(legacyState) => legacyState, // vehicle bar: keep the UI position
            _    => spellId != 0 ? 0x101u : 0u,  // unknown state carrying a spell — render as castable
        };

        return (v343Slot << 23) | (spellId & V343ActionButtonSpellMask);
    }
}
