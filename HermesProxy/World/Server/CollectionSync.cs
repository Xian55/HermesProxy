using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server;

public static class CollectionSync
{
    public static void StampSummonedBattlePet(ObjectUpdate update, GameSessionData state)
    {
        if (!state.SummonedBattlePetGuid.IsEmpty())
        {
            update.EnsureActivePlayerData().SummonedBattlePetGUID = state.SummonedBattlePetGuid;
            if (!state.SummonedCompanionCreatureGuid.IsEmpty())
                update.UnitData.Critter = state.SummonedCompanionCreatureGuid;
        }
    }

    public static void StampCompanionCreature(ObjectUpdate update, GlobalSessionData session, WowGuid64 legacyGuid)
    {
        var state = session.GameState;
        if (update.UnitData == null)
            return;
        uint entry = (uint)(update.ObjectData?.EntryID ?? 0);
        if (entry == 0 || !GameData.TryGetSpeciesByCreatureId(entry, out var species))
            return;

        var player = state.CurrentPlayerGuid;
        bool summonedByUs = update.UnitData.SummonedBy == player || update.UnitData.CreatedBy == player;
        bool ownerMissing = (update.UnitData.SummonedBy == null || update.UnitData.SummonedBy.Value.IsEmpty())
            && (update.UnitData.CreatedBy == null || update.UnitData.CreatedBy.Value.IsEmpty());
        bool ours = !player.IsEmpty()
            && (summonedByUs || ownerMissing || !state.SummonedBattlePetGuid.IsEmpty());
        if (!ours)
            return;

        var journalGuid = WowGuid128.Create(HighGuidType703.BattlePet, species.SpeciesId);
        update.UnitData.BattlePetCompanionGUID = journalGuid;
        update.UnitData.BattlePetDBID = species.SpeciesId;
        update.UnitData.WildBattlePetLevel = 1;

        state.SummonedBattlePetGuid = journalGuid;
        state.SummonedCompanionCreatureGuid = update.Guid;
        state.SummonedCompanionLegacyGuid = legacyGuid;
        if (GameData.TryGetSummonSpellForSpecies(species.SpeciesId, out uint spellId))
            state.BattlePetGuidToSummonSpell[journalGuid] = spellId;
        state.CurrentPlayerStorage?.Settings?.SetLastSummonedPetSpecies(species.SpeciesId);

        SendSummonedBattlePet(session);
    }

    public static void SendSummonedBattlePet(GlobalSessionData session)
    {
        if (session.GameState.CurrentPlayerGuid.IsEmpty())
            return;

        var state = session.GameState;
        var updateData = new ObjectUpdate(state.CurrentPlayerGuid, UpdateTypeModern.Values, session);
        updateData.EnsureActivePlayerData().SummonedBattlePetGUID = state.SummonedBattlePetGuid;
        updateData.UnitData.Critter = state.SummonedCompanionCreatureGuid;
        var updatePacket = new UpdateObject(state);
        updatePacket.ObjectUpdates.Add(updateData);
        session.WorldClient?.SendPacketToClient(updatePacket);
    }

    // WotLK "Learning" (55884) — same visual mount/companion items use when consumed
    public const uint LearningSpellId = 55884;
    public const uint LearningSpellXSpellVisualId = 346509;

    public static void PlayToyLearnVisual(GlobalSessionData session)
    {
        var state = session.GameState;
        var player = state.CurrentPlayerGuid;
        if (player.IsEmpty() || session.WorldClient == null)
            return;

        uint mapId = (uint)(state.CurrentMapId ?? 0);
        var castId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, mapId, LearningSpellId, LearningSpellId + player.GetCounter());

        var cast = new SpellCastData
        {
            CasterGUID = player,
            CasterUnit = player,
            CastID = castId,
            SpellID = (int)LearningSpellId,
            SpellXSpellVisualID = LearningSpellXSpellVisualId,
            CastTime = 0,
        };
        cast.Target.Flags = SpellCastTargetFlags.Unit;
        cast.Target.Unit = player;
        cast.HitTargets.Add(player);

        session.WorldClient.SendPacketToClient(new SpellStart { Cast = cast });
        session.WorldClient.SendPacketToClient(new SpellGo { Cast = cast });
    }

    private static readonly Microsoft.Extensions.Logging.ILogger _melObjLife =
        Framework.Logging.Log.CreateMelLogger(Framework.Logging.Log.CategoryServer);

    private static readonly HoldKey ToysSyncKey = new(HoldKeyKind.ToysSync);

    // Released at the end of the batch that gives the client the player. A sync still waiting after
    // the timeout re-checks: it sends if the player is known by then and waits again otherwise.
    private static readonly HoldOptions ToysSyncHold = new(
        Timeout: TimeSpan.FromSeconds(60),
        OnTimeout: OutboxTimeoutAction.Release,
        Key: ToysSyncKey);

    public static void SendToys(GlobalSessionData session)
    {
        var state = session.GameState;
        if (state.CurrentPlayerGuid.IsEmpty() || session.WorldClient == null)
            return;

        // The Toys list is an ActivePlayerData field, so publishing it means sending a
        // Values delta on the player guid. This builds its own UpdateObject and hands it
        // straight to the client, bypassing UpdatePackets.FilterV3_4_3Values — which is
        // where ClientKnownGuids normally stops pre-create Values from going out. During
        // login the collection sync runs before the player's CreateObject has been
        // forwarded, so an account with toys shipped a delta for an object the client did
        // not have: it answered CMSG_OBJECT_UPDATE_FAILED and then stopped instantiating
        // newly created objects (pets, spell-spawned GameObjects) until the next zone
        // change rebuilt the grid. Defer instead, and let UpdateHandler flush once the
        // player create has actually gone out.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            && !state.ClientKnownGuids.Contains(state.CurrentPlayerGuid))
        {
            // The release reads the toy state it sends then, so one waiting sync covers every
            // request made meanwhile.
            session.ToClient.Cancel(ToysSyncKey);
            session.ToClient.When(OutboxEvent.GuidKnown(state.CurrentPlayerGuid), () =>
            {
                World.Logging.ObjectLifecycleLogMessages.ToysFlushed(
                    _melObjLife, state.CurrentPlayerGuid.Low, state.CurrentPlayerGuid.High);
                RefreshUsableToys(session);
            }, ToysSyncHold);
            World.Logging.ObjectLifecycleLogMessages.ToysDeferred(
                _melObjLife, state.CurrentPlayerGuid.Low, state.CurrentPlayerGuid.High);
            return;
        }

        var usable = state.GetUsableToysOrdered();
        state.LastSentUsableToys = usable;
        var updateData = new ObjectUpdate(state.CurrentPlayerGuid, UpdateTypeModern.Values, session);
        var toys = new List<int>(usable.Length);
        for (int i = 0; i < usable.Length; i++)
            toys.Add((int)usable[i]);
        updateData.EnsureActivePlayerData().Toys = toys;
        var updatePacket = new UpdateObject(state);
        updatePacket.ObjectUpdates.Add(updateData);
        session.WorldClient.SendPacketToClient(updatePacket);
    }

    public static void RefreshUsableToys(GlobalSessionData session)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;
        var state = session.GameState;
        if (state.CurrentPlayerGuid.IsEmpty() || session.WorldClient == null)
            return;

        var usable = state.GetUsableToysOrdered();
        if (ToysMatch(state.LastSentUsableToys, usable))
            return;

        SendToys(session);
        session.WorldClient.SendPacketToClient(AccountToyUpdate.FromSession(state));
    }

    static bool ToysMatch(uint[] left, uint[] right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
                return false;
        }
        return true;
    }
}
