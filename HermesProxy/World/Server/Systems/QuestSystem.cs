using Framework.Logging;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Quest translation for the modern client. Currently only the pieces the NPC system needs.
/// </summary>
/// <remarks>
/// These two were private instance helpers on <c>WorldSocket</c>, called from both
/// <c>QuestHandler</c> and <c>NPCHandler</c>. Converting the NPC side would otherwise have left a
/// copy on each side of the migration, so they move here as statics that both the converted system
/// and the handlers still on the reflective path call — the same shape
/// <c>BattlePetSystem.EnsureCollectionFavorites</c> took in the previous slice. The rest of
/// <c>QuestHandler</c>'s fifteen opcodes land here when that file converts.
/// </remarks>
public static class QuestSystem
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(WorldSocket).PadRight(15);
    private static readonly string _netDirRecv = Log.FormatDir(LogNetDir.C2P);

    // 3.4.3 Decline is TALK_TO_GOSSIP, not a cancel opcode. Dismiss the
    // parchment, then put back this NPC's cached list only.
    internal static void ReturnDetailsToGossip(in SessionContext ctx, string action)
    {
        var state = ctx.GetSession().GameState;
        int questId = (int)(state.LastQuestDetails?.QuestID ?? 0);
        WowGuid128 npc = state.LastQuestDetails?.QuestGiverGUID ?? default;
        ReturnQuestFrameToGossip(in ctx, (uint)questId, npc, action);
    }

    internal static void ReturnQuestFrameToGossip(in SessionContext ctx, uint questId, WowGuid128 npc, string action)
    {
        var state = ctx.GetSession().GameState;
        var gossip = state.LastGossip;
        var list = state.LastQuestList;
        state.CloseQuestDetails();
        state.ClearQuestRewardWait();

        ctx.SendPacket(new QuestGiverInvalidQuest
        {
            Reason = QuestFailedReasons.None,
            SendErrorMessage = false
        });

        if (gossip != null && npc != default && gossip.GossipGUID == npc)
            ctx.SendPacket(gossip);
        else if (list != null && npc != default && list.QuestGiverGUID == npc)
            ctx.SendPacket(list);

        WorldSocketLogMessages.QuestClose(_melLog, _sourceFile, _netDirRecv, (int)questId, action);
    }
}
