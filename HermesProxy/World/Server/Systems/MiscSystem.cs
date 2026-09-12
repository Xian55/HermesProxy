using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's miscellaneous CMSGs. Behaviour only.
/// </summary>
public static class MiscSystem
{
    [HandlesCmsg(Opcode.CMSG_TIME_SYNC_RESPONSE)]
    public static void HandleTimeSyncResponse(in TimeSyncResponse response, in SessionContext ctx)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_TIME_SYNC_RESPONSE);
            packet.WriteUInt32(response.SequenceIndex);
            packet.WriteUInt32(response.ClientTime);
            ctx.SendPacketToServer(packet);
        }
    }

    [HandlesCmsg(Opcode.CMSG_AREA_TRIGGER)]
    public static void HandleAreaTrigger(in AreaTriggerPkt at, in SessionContext ctx)
    {
        if (at.Entered == false)
            return;

        // Reconcile post-Cataclysm DB2 ids back to the 3.3.5a-era ids the
        // legacy server's areatrigger_teleport table is keyed on. V3_4_3 only.
        // Table is data-driven: CSV/AreaTriggerRemap*.csv.
        uint idToForward = at.AreaTriggerID;
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 &&
            GameData.AreaTriggerModernToLegacy.TryGetValue(at.AreaTriggerID, out var legacyId))
        {
            idToForward = legacyId;
        }

        ctx.GetSession().GameState.LastEnteredAreaTrigger = idToForward;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AREA_TRIGGER);
        packet.WriteUInt32(idToForward);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_SELECTION)]
    public static void HandleSetSelection(in SetSelection selection, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_SELECTION);
        packet.WriteGuid(selection.TargetGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REPOP_REQUEST)]
    public static void HandleRepopRequest(in RepopRequest repop, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REPOP_REQUEST);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteBool(repop.CheckInstance);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT)]
    public static void HandleQueryCorpseLocationFromClient(in QueryCorpseLocationFromClient query, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_CORPSE_QUERY);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_RECLAIM_CORPSE)]
    public static void HandleReclaimCorpse(in ReclaimCorpse corpse, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_RECLAIM_CORPSE);
        packet.WriteGuid(corpse.CorpseGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_STAND_STATE_CHANGE)]
    public static void HandleStandStateChange(in StandStateChange state, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_STAND_STATE_CHANGE);
        packet.WriteUInt32(state.StandState);
        ctx.SendPacketToServer(packet);
    }

    /// <summary>
    /// Three opcodes, one body — the packet carries no payload and the opcode itself is the
    /// message. Shape B: the generator emits one thunk per opcode, each passing its own literal,
    /// so what used to be a runtime <c>GetUniversalOpcode()</c> off the packet is now a constant
    /// the JIT can see.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_OPENING_CINEMATIC)]
    [HandlesCmsg(Opcode.CMSG_NEXT_CINEMATIC_CAMERA)]
    [HandlesCmsg(Opcode.CMSG_COMPLETE_CINEMATIC)]
    public static void HandleCinematicPacket(Opcode opcode, in ClientCinematicPkt cinematic, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_FAR_SIGHT)]
    public static void HandleFarSight(in FarSight sight, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_FAR_SIGHT);
        packet.WriteBool(sight.Enable);
        ctx.SendPacketToServer(packet);
        ctx.GetSession().GameState.IsInFarSight = sight.Enable;
    }

    [HandlesCmsg(Opcode.CMSG_TUTORIAL_FLAG)]
    public static void HandleTutorialFlag(in TutorialSetFlag tutorial, in SessionContext ctx)
    {
        switch (tutorial.Action)
        {
            case TutorialAction.Clear:
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_TUTORIAL_CLEAR);
                ctx.SendPacketToServer(packet);
                break;
            }
            case TutorialAction.Reset:
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_TUTORIAL_RESET);
                ctx.SendPacketToServer(packet);
                break;
            }
            case TutorialAction.Update:
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_TUTORIAL_FLAG);
                packet.WriteUInt32(tutorial.TutorialBit);
                ctx.SendPacketToServer(packet);
                break;
            }
        }
    }

    [HandlesCmsg(Opcode.CMSG_OBJECT_UPDATE_FAILED)]
    public static void HandleObjectUpdateFailed(in ObjectUpdateFailed fail, in SessionContext ctx)
    {
        // Phase 5a-7c diagnostic: surface the modern high-guid type so we can correlate
        // failures to specific object kinds (Transport / GameObject / Item / Unit / etc.)
        // when the client rejects what the proxy serialized.
        Log.Print(LogType.Error,
            $"CMSG_OBJECT_UPDATE_FAILED guid={fail.ObjectGuid} highType={fail.ObjectGuid.GetHighType()} entry={fail.ObjectGuid.GetEntry()}.");
    }

    [HandlesCmsg(Opcode.CMSG_SET_DUNGEON_DIFFICULTY)]
    public static void HandleSetDungeonDifficulty(in SetDungeonDifficulty difficulty, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_SET_DUNGEON_DIFFICULTY);
        uint dificultyId = (byte)((DifficultyModern)difficulty.DifficultyID).CastEnum<DifficultyLegacy>();
        packet.WriteUInt32(dificultyId);
        ctx.SendPacketToServer(packet);

        // 2.4.3 server does not send response to same client on difficulty change
        DungeonDifficultySet difficultySet = new();
        difficultySet.DifficultyID = (int)difficulty.DifficultyID;
        ctx.SendPacket(difficultySet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_RAID_DIFFICULTY)]
    public static void HandleSetRaidDifficulty(in SetRaidDifficulty difficulty, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_SET_RAID_DIFFICULTY);
        packet.WriteUInt32(RaidDifficulties.ToLegacy(difficulty.DifficultyID));
        ctx.SendPacketToServer(packet);

        // AC solo (no group) SetRaidDifficulty is silent, so the UI would snap back without an
        // echo. In a group both AC and TC broadcast MSG_SET_RAID_DIFFICULTY, which the client
        // handler already turns into SMSG_RAID_DIFFICULTY_SET - echoing as well produced two
        // packets per click and a duplicate "Raid Difficulty set to..." line in chat.
        if (ctx.GetSession().GameState.GetCurrentGroup() == null)
        {
            RaidDifficultySet difficultySet = new();
            difficultySet.DifficultyID = difficulty.DifficultyID;
            difficultySet.Legacy = difficulty.Legacy;
            ctx.SendPacket(difficultySet);
        }
    }
}
