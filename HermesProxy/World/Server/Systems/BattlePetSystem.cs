using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Translation for the modern client's battle-pet CMSGs. Behaviour only.</summary>
public static class BattlePetSystem
{

    /// <summary>
    /// Loads the account's collection favourites on first use.
    /// </summary>
    /// <remarks>
    /// Was a private instance helper on <c>WorldSocket</c>, shared by the battle-pet and toy
    /// handlers. It only ever needed the session, so it becomes a static that both the converted
    /// systems and the handlers still on the reflective path can call — one implementation rather
    /// than a copy on each side of the migration. It lands here because this is where it ends up
    /// once the battle-pet domain finishes converting.
    /// </remarks>
    public static CollectionFavorites EnsureCollectionFavorites(GlobalSessionData session)
    {
        var state = session.GameState;
        if (state.CollectionFavorites == null)
            state.CollectionFavorites = session.AccountMetaDataMgr.LoadCollectionFavorites();
        return state.CollectionFavorites;
    }

    [HandlesCmsg(Opcode.CMSG_BATTLE_PET_REQUEST_JOURNAL)]
    public static void HandleBattlePetRequestJournal(in EmptyClientPacket request, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        EnsureCollectionFavorites(ctx.GetSession());
        ctx.SendPacket(BattlePetJournal.FromSession(ctx.GetSession().GameState));
        CollectionSync.SendSummonedBattlePet(ctx.GetSession());
    }
}
