using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    [PacketHandler(Opcode.CMSG_PET_ACTION)]
    void HandlePetAction(PetAction act)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_ACTION);
        packet.WriteGuid(act.PetGUID.To64(GetSession().GameState));
        // V3_4_3 client packs Action in modern slot-shifted layout; legacy 3.3.5a server
        // expects the older state-byte layout. Translate only for V3_4_3 — V1_14 / V2_5
        // modern clients use different/uncertain Action layouts; preserve their behavior.
        uint legacyAction = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            ? TranslateV343PetActionToLegacy(act.Action)
            : act.Action;
        packet.WriteUInt32(legacyAction);
        packet.WriteGuid(act.TargetGUID.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_PET_STOP_ATTACK)]
    void HandlePetStopAttack(PetStopAttack stop)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_STOP_ATTACK);
        packet.WriteGuid(stop.PetGUID.To64(GetSession().GameState));
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_PET_SET_ACTION)]
    void HandlePetStopAttack(PetSetAction action)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_SET_ACTION);
        packet.WriteGuid(action.PetGUID.To64(GetSession().GameState));
        packet.WriteUInt32(action.Index);
        // Same gating as CMSG_PET_ACTION above — translate only for V3_4_3.
        uint legacyAction = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            ? TranslateV343PetActionToLegacy(action.Action)
            : action.Action;
        packet.WriteUInt32(legacyAction);
        SendPacketToServer(packet);
    }

    // Inverse of WorldClient.PetHandler.TranslateLegacyPetActionButtonToV343 — repack
    // the V3_4_3 client's Action field (slot:9 in bits 23-31, spell:23 in bits 0-22)
    // into the 3.3.5a legacy server's expected layout (state:8 in bits 24-31, spell:16
    // in bits 0-15). Without this, an Attack click ships as Action=(7<<23)|2 = 0x03800002,
    // which the legacy server reads as state_byte=0x03 (unknown) and ignores.
    //
    // Slot mapping mirrors the legacy ActiveStates enum (cmangos `ActiveStates`):
    //   0x07=ACT_COMMAND, 0x06=ACT_REACTION, 0x01=ACT_PASSIVE,
    //   0x81=ACT_DISABLED, 0xC1=ACT_ENABLED (autocast on)
    private static uint TranslateV343PetActionToLegacy(uint v343Action)
    {
        if (v343Action == 0)
            return 0;

        uint v343Slot = v343Action >> 23;
        uint spellId = v343Action & 0x7FFFFF;

        byte legacyState = v343Slot switch
        {
            7      => 0x07, // CommandState
            6      => 0x06, // ReactState
            0x1    => 0x01, // PassiveSpell
            0x181  => 0xC1, // AutoCastSpell (enabled with autocast)
            0x101  => 0x81, // ManualSpell (active, no autocast)
            0      => 0xC0, // plain SpellID — enabled active spell
            _      => 0x00,
        };

        // Legacy stores spell_id in low 16 bits; clamp.
        ushort legacySpell = (ushort)(spellId & 0xFFFF);
        return ((uint)legacyState << 24) | legacySpell;
    }

    [PacketHandler(Opcode.CMSG_PET_RENAME)]
    void HandlePetRename(PetRename pet)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_RENAME);
        packet.WriteGuid(pet.RenameData.PetGUID.To64(GetSession().GameState));
        packet.WriteCString(pet.RenameData.NewName);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            packet.WriteBool(pet.RenameData.HasDeclinedNames);
            if (pet.RenameData.HasDeclinedNames)
            {
                for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                    packet.WriteCString(pet.RenameData.DeclinedNames.name[i]);
            }
        }
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_REQUEST_STABLED_PETS)]
    void HandleRequestStabledPets(RequestStabledPets stable)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_LIST_STABLED_PETS);
        packet.WriteGuid(stable.StableMaster.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_BUY_STABLE_SLOT)]
    void HandleBuyStableSlot(BuyStableSlot stable)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_STABLE_SLOT);
        packet.WriteGuid(stable.StableMaster.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_PET_ABANDON)]
    void HandlePetAbandon(PetAbandon pet)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_ABANDON);
        packet.WriteGuid(pet.PetGUID.To64(GetSession().GameState));
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_STABLE_PET)]
    void HandleStablePet(StablePet pet)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_STABLE_PET);
        packet.WriteGuid(pet.StableMaster.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_UNSTABLE_PET)]
    void HandleUnstablePet(UnstablePet pet)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_UNSTABLE_PET);
        packet.WriteGuid(pet.StableMaster.To64());
        packet.WriteUInt32(pet.PetNumber);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_STABLE_SWAP_PET)]
    void HandleStableSwapPet(StableSwapPet pet)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_STABLE_SWAP_PET);
        packet.WriteGuid(pet.StableMaster.To64());
        packet.WriteUInt32(pet.PetNumber);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_PET_CANCEL_AURA)]
    void HandlePetCancelAura(PetCancelAura cancel)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_CANCEL_AURA);
        packet.WriteGuid(cancel.PetGUID.To64(GetSession().GameState));
        packet.WriteUInt32(cancel.SpellID);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_REQUEST_PET_INFO)]
    void HandleRequestPetInfo(PetInfoRequest r)
    {
        // CMSG_REQUEST_PET_INFO
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_PET_INFO);
        SendPacketToServer(packet);

    }
}

