using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
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
    /// handlers. It only ever needed the session, so it became a static that
    /// <see cref="BattlePetSystem"/> and <see cref="ToySystem"/> both call — one implementation
    /// rather than a copy in each.
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

    [HandlesCmsg(Opcode.CMSG_BATTLE_PET_SUMMON)]
    public static void HandleBattlePetSummon(in BattlePetSummon summon, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        if (!ctx.GetSession().GameState.BattlePetGuidToSummonSpell.TryGetValue(summon.PetGuid, out uint spellId)
            || spellId == 0)
            return;

        bool dismiss = ctx.GetSession().GameState.SummonedBattlePetGuid == summon.PetGuid
            && !summon.PetGuid.IsEmpty();

        if (dismiss)
        {
            DismissSummonedCompanion(in ctx);
            return;
        }

        // Stamp the journal GUID before the legacy cast so the companion
        // CreateObject can carry BattlePetCompanionGUID (GetSummonedPetGUID).
        SetSummonedBattlePet(in ctx, summon.PetGuid, (uint)summon.PetGuid.GetCounter());
        CastCompanionSpell(in ctx, spellId);
    }

    [HandlesCmsg(Opcode.CMSG_BATTLE_PET_SET_FLAGS)]
    public static void HandleBattlePetSetFlags(in BattlePetSetFlags setFlags, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        var favorites = EnsureCollectionFavorites(ctx.GetSession());
        uint speciesId = (uint)setFlags.PetGuid.GetCounter();
        if (speciesId == 0)
            return;

        if ((setFlags.Flags & BattlePetInfo.FavoriteFlag) != 0)
        {
            if (setFlags.ControlType == BattlePetSetFlags.ControlRemove)
                favorites.FavoritePetSpecies.Remove(speciesId);
            else
                favorites.FavoritePetSpecies.Add(speciesId);
            ctx.GetSession().AccountMetaDataMgr.SaveCollectionFavorites(favorites);
        }

        ctx.SendPacket(BattlePetJournal.FromSession(ctx.GetSession().GameState));
    }

    [HandlesCmsg(Opcode.CMSG_MOUNT_SET_FAVORITE)]
    public static void HandleMountSetFavorite(in MountSetFavorite setFavorite, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        var favorites = EnsureCollectionFavorites(ctx.GetSession());
        if (setFavorite.IsFavorite)
            favorites.FavoriteMountSpells.Add(setFavorite.MountSpellID);
        else
            favorites.FavoriteMountSpells.Remove(setFavorite.MountSpellID);
        ctx.GetSession().AccountMetaDataMgr.SaveCollectionFavorites(favorites);
        ctx.SendPacket(AccountMountUpdate.FromSession(ctx.GetSession().GameState));
    }

    static void CastCompanionSpell(in SessionContext ctx, uint spellId)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CAST_SPELL);
        packet.WriteUInt8(0);
        packet.WriteUInt32(spellId);
        packet.WriteUInt8(0);
        packet.WriteUInt32(0);
        ctx.SendPacketToServer(packet);
    }

    static void DismissSummonedCompanion(in SessionContext ctx)
    {
        var state = ctx.GetSession().GameState;
        if (!state.SummonedCompanionLegacyGuid.IsEmpty())
        {
            // AC 3.3.5a ObjectGuid >> reads a raw uint64. Packed guid is ignored.
            // Recast of the companion spell replaces the pet, it does not dismiss.
            WorldPacket packet = new WorldPacket(Opcode.CMSG_DISMISS_CRITTER);
            packet.WriteGuid(state.SummonedCompanionLegacyGuid);
            ctx.SendPacketToServer(packet);
        }

        SetSummonedBattlePet(in ctx, WowGuid128.Empty, 0);
        state.SummonedCompanionCreatureGuid = WowGuid128.Empty;
    }

    static void SetSummonedBattlePet(in SessionContext ctx, WowGuid128 guid, uint speciesId)
    {
        var session = ctx.GetSession();
        session.GameState.SummonedBattlePetGuid = guid;
        if (guid.IsEmpty())
        {
            session.GameState.SummonedCompanionCreatureGuid = WowGuid128.Empty;
            session.GameState.SummonedCompanionLegacyGuid = WowGuid64.Empty;
        }
        session.GameState.CurrentPlayerStorage?.Settings?.SetLastSummonedPetSpecies(speciesId);

        CollectionSync.SendSummonedBattlePet(session);
    }

    [HandlesCmsg(Opcode.CMSG_DISMISS_CRITTER)]
    public static void HandleDismissCritter(in DismissCritter dismiss, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_DISMISS_CRITTER);
        packet.WriteGuid(dismiss.CritterGUID.To64(ctx.GetSession().GameState));
        ctx.SendPacketToServer(packet);
        SetSummonedBattlePet(in ctx, WowGuid128.Empty, 0);
    }
}
