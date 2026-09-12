using System;
using System.Collections.Generic;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's pet CMSGs: the pet bar, stabling and pet naming.
/// </summary>
/// <remarks>
/// Bodies were moved from <c>World/Server/PacketHandlers/PetHandler.cs</c>, not retyped;
/// <c>verify-handler-port.py</c> diffs each one against the original.
/// </remarks>
public static class PetSystem
{
    [HandlesCmsg(Opcode.CMSG_PET_ACTION)]
    public static void HandlePetAction(in PetAction act, in SessionContext ctx)
    {
        // V3_4_3 client packs Action in modern slot-shifted layout; legacy 3.3.5a server
        // expects the older state-byte layout. Translate only for V3_4_3 — V1_14 / V2_5
        // modern clients use different/uncertain Action layouts; preserve their behavior.
        uint legacyAction = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            ? TranslateV343PetActionToLegacy(act.Action)
            : act.Action;

        // Vehicle / pet bar spell-button clicks fail silently on the modern client when
        // the legacy server returns SMSG_PET_CAST_FAILED — the client-side cast-fail
        // handler dequeues PendingPetCasts by spell ID, but only CMSG_PET_CAST_SPELL
        // enqueues there. CMSG_PET_ACTION (used for the vehicle action bar) never did,
        // so failures were dropped and the action button locked until /reload.
        // Enqueue here for spell-bearing slots (plain spell / manual / autocast). Skip
        // command buttons (Attack/Stay/Follow/React) — they have no spell ID.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            uint v343Slot = act.Action >> 23;
            if (v343Slot == 0 || v343Slot == 0x101 || v343Slot == 0x181)
            {
                uint spellId = act.Action & 0x7FFFFF;
                if (spellId != 0)
                {
                    ClientCastRequest castRequest = new ClientCastRequest
                    {
                        Timestamp = Environment.TickCount,
                        SpellId = spellId,
                        ClientGUID = WowGuid128.Empty,
                        ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal,
                            (uint)(ctx.GetSession().GameState.CurrentMapId ?? 0), spellId, 20000u + (uint)Environment.TickCount),
                        HasStarted = true,
                    };
                    ctx.GetSession().GameState.PendingPetCasts.Enqueue(castRequest);
                }
            }
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_ACTION);
        packet.WriteGuid(act.PetGUID.To64(ctx.GetSession().GameState));
        packet.WriteUInt32(legacyAction);
        packet.WriteGuid(act.TargetGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PET_STOP_ATTACK)]
    public static void HandlePetStopAttack(in PetStopAttack stop, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_STOP_ATTACK);
        packet.WriteGuid(stop.PetGUID.To64(ctx.GetSession().GameState));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PET_SET_ACTION)]
    public static void HandlePetSetAction(in PetSetAction action, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_SET_ACTION);
        packet.WriteGuid(action.PetGUID.To64(ctx.GetSession().GameState));
        packet.WriteUInt32(action.Index);
        // Same gating as CMSG_PET_ACTION above — translate only for V3_4_3.
        uint legacyAction = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            ? TranslateV343PetActionToLegacy(action.Action)
            : action.Action;
        packet.WriteUInt32(legacyAction);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PET_RENAME)]
    public static void HandlePetRename(in PetRename pet, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_RENAME);
        packet.WriteGuid(pet.RenameData.PetGUID.To64(ctx.GetSession().GameState));
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
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_STABLED_PETS)]
    public static void HandleRequestStabledPets(in RequestStabledPets stable, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_LIST_STABLED_PETS);
        packet.WriteGuid(stable.StableMaster.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_BUY_STABLE_SLOT)]
    public static void HandleBuyStableSlot(in BuyStableSlot stable, in SessionContext ctx)
    {
        ctx.GetSession().GameState.LastStableMaster = stable.StableMaster;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_STABLE_SLOT);
        packet.WriteGuid(stable.StableMaster.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PET_ABANDON)]
    public static void HandlePetAbandon(in PetAbandon pet, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_ABANDON);
        packet.WriteGuid(pet.PetGUID.To64(ctx.GetSession().GameState));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_STABLE_PET)]
    public static void HandleStablePet(in StablePet pet, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_STABLE_PET);
        packet.WriteGuid(pet.StableMaster.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_UNSTABLE_PET)]
    public static void HandleUnstablePet(in UnstablePet pet, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_UNSTABLE_PET);
        packet.WriteGuid(pet.StableMaster.To64());
        packet.WriteUInt32(pet.PetNumber);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_STABLE_SWAP_PET)]
    public static void HandleStableSwapPet(in StableSwapPet pet, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_STABLE_SWAP_PET);
        packet.WriteGuid(pet.StableMaster.To64());
        packet.WriteUInt32(pet.PetNumber);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PET_CANCEL_AURA)]
    public static void HandlePetCancelAura(in PetCancelAura cancel, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_CANCEL_AURA);
        packet.WriteGuid(cancel.PetGUID.To64(ctx.GetSession().GameState));
        packet.WriteUInt32(cancel.SpellID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_PET_INFO)]
    public static void HandleRequestPetInfo(in EmptyClientPacket r, in SessionContext ctx)
    {
        // CMSG_REQUEST_PET_INFO
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_PET_INFO);
        ctx.SendPacketToServer(packet);

    }

    internal static uint TranslateV343PetActionToLegacy(uint v343Action)
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
            // Vehicle bar positions (8..17). The V3_4_3 client casts these through
            // CMSG_PET_CAST_SPELL rather than CMSG_PET_ACTION (confirmed in the native
            // capture), so this is a fallback only — echoing the position back would land
            // in the legacy handler's "unknown PET flag" default and be dropped, so ship a
            // castable state instead.
            >= 8 and <= 17 => 0x81,
            _      => 0x00,
        };

        // Legacy stores spell_id in the low 24 bits (UNIT_ACTION_BUTTON_ACTION).
        uint legacySpell = spellId & 0x00FFFFFF;
        return ((uint)legacyState << 24) | legacySpell;
    }
}
