using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's NPC-interaction CMSGs: gossip, bankers, trainers and the
/// rest of the "click the thing and tell the server" family.
/// </summary>
/// <remarks>
/// <see cref="HandleInteractWithNPC"/> is the reason <c>InteractWithNPC</c> converted as its own
/// unit: nine opcodes here take it, and seven more take it in the taxi, auction and guild systems.
/// It is shape B, so the opcode that distinguishes a banker from a spirit healer reaches the body
/// as a literal rather than being read back off the packet.
/// </remarks>
public static class NpcSystem
{
    [HandlesCmsg(Opcode.CMSG_BANKER_ACTIVATE)]
    [HandlesCmsg(Opcode.CMSG_BINDER_ACTIVATE)]
    [HandlesCmsg(Opcode.CMSG_LIST_INVENTORY)]
    [HandlesCmsg(Opcode.CMSG_SPELL_CLICK)]
    [HandlesCmsg(Opcode.CMSG_SPIRIT_HEALER_ACTIVATE)]
    [HandlesCmsg(Opcode.CMSG_TRAINER_LIST)]
    [HandlesCmsg(Opcode.CMSG_BATTLEMASTER_HELLO)]
    [HandlesCmsg(Opcode.CMSG_AREA_SPIRIT_HEALER_QUERY)]
    [HandlesCmsg(Opcode.CMSG_AREA_SPIRIT_HEALER_QUEUE)]
    public static void HandleInteractWithNPC(Opcode opcode, in InteractWithNPC interact, in SessionContext ctx)
    {
        if (ctx.GetSession().GameState.AwaitingQuestRewardId != 0
            && interact.CreatureGUID != ctx.GetSession().GameState.AwaitingQuestGiver)
            ctx.GetSession().GameState.ClearQuestRewardWait();

        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteGuid(interact.CreatureGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_TALK_TO_GOSSIP)]
    public static void HandleTalkToGossip(in InteractWithNPC interact, in SessionContext ctx)
    {
        // V3_4_3 re-talks to the same NPC right after RequestItems. Replay once
        // so the frame stays bound. A later talk is Cancel / the multi-quest list.
        if (ModernVersion.Build == HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261
            && ctx.GetSession().GameState.AwaitingQuestRewardId != 0)
        {
            var state = ctx.GetSession().GameState;
            var last = state.LastRequestItems;
            if (last != null && interact.CreatureGUID == state.AwaitingQuestGiver)
            {
                if (state.JustSentRequestItems)
                {
                    state.JustSentRequestItems = false;
                    ctx.SendPacket(last);
                    return;
                }

                QuestSystem.ReturnQuestFrameToGossip(in ctx, state.AwaitingQuestRewardId, state.AwaitingQuestGiver, "decline-request-items");
                return;
            }

            state.ClearQuestRewardWait();
        }

        if (ModernVersion.Build == HermesProxy.Enums.ClientVersionBuild.V3_4_3_54261
            && ctx.GetSession().GameState.QuestDetailsOpen)
        {
            QuestSystem.ReturnDetailsToGossip(in ctx, "decline-talk");
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_TALK_TO_GOSSIP);
        packet.WriteGuid(interact.CreatureGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GOSSIP_SELECT_OPTION)]
    public static void HandleGossipSelectOption(in GossipSelectOption gossip, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GOSSIP_SELECT_OPTION);
        packet.WriteGuid(gossip.GossipUnit.To64());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt32(gossip.GossipID);
        packet.WriteUInt32(gossip.GossipIndex);
        if (!String.IsNullOrEmpty(gossip.PromotionCode))
            packet.WriteCString(gossip.PromotionCode);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_BUY_BANK_SLOT)]
    public static void HandleBuyBankSlot(in BuyBankSlot bank, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_BANK_SLOT);
        packet.WriteGuid(bank.Guid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_TRAINER_BUY_SPELL)]
    public static void HandleTrainerBuySpell(in TrainerBuySpell buy, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_TRAINER_BUY_SPELL);
        packet.WriteGuid(buy.TrainerGUID.To64());
        // The class this replaced overwrote its own SpellID field here. A data-only packet is
        // readonly, so the remapped id lives in a local; nothing else about the body changed.
        uint spellId = buy.SpellID;
        if (ModernVersion.ExpansionVersion > 1 &&
            LegacyVersion.ExpansionVersion <= 1)
        {
            // in vanilla the server sends learn spell with effect 36
            // in expansions the server sends the actual spell
            spellId = ctx.GetSession().GameState.GetLearnSpellFromRealSpell(spellId);
        }
        packet.WriteUInt32(spellId);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CONFIRM_RESPEC_WIPE)]
    public static void HandleConfirmRespecWipe(in ConfirmRespecWipe respec, in SessionContext ctx)
    {
        switch (respec.RespecType)
        {
            case SpecResetType.Talents:
            {
                WorldPacket packet = new WorldPacket(Opcode.MSG_TALENT_WIPE_CONFIRM);
                packet.WriteGuid(respec.TrainerGUID.To64());
                ctx.SendPacketToServer(packet);
                break;
            }
            case SpecResetType.PetTalents:
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_UNLEARN);
                packet.WriteGuid(respec.TrainerGUID.To64());
                ctx.SendPacketToServer(packet);
                break;
            }
            default:
            {
                Log.Print(LogType.Error, $"Unhandled respec type {respec.RespecType}.");
                break;
            }
        }
    }
}
