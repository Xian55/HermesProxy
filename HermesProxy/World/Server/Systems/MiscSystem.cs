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

    [HandlesCmsg(Opcode.CMSG_REQUEST_LFG_LIST_BLACKLIST)]
    public static void HandleRequestLFGListBlacklist(in EmptyClientPacket request, in SessionContext ctx)
    {
        // V3_4_3 (WotLK Classic) does NOT implement the Cataclysm+ Premade-Group
        // LFG List system. Confirmed by Wrathion 3.4.3 reference sniff
        // (World_solo_dungeon_finder_queue_parsed.txt): client polls
        // CMSG_REQUEST_LFG_LIST_BLACKLIST at login but server emits ZERO
        // SMSG_LFG_LIST_UPDATE_BLACKLIST packets. The static Cataclysm+ Activity
        // blacklist below uses ActivityID values (796-887) that don't exist in
        // V3_4_3 client DB2 — receiving it appears to route the modern client's
        // LFG UI toward the Premade-Group code path, hiding the Dungeon Finder
        // microbar eye icon and disabling the regular Queue button.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            return;

        // Static blacklist for the modern Premade-Group LFG List (Cataclysm+
        // activity browser, distinct from the WotLK Dungeon Finder queue).
        // AddBlacklist(activityID, reason):
        //   activityID — row in modern client's GroupFinderActivity.db2
        //                (786-934 range covers Cata/MoP/WoD/Legion/BfA dungeons,
        //                raids, scenarios, RBGs, etc).
        //   reason     — LfgLockStatus enum value telling client WHY hidden:
        //                  3    = LFG_LOCKSTATUS_TOO_HIGH_LEVEL
        //                  1031 = LFG_LOCKSTATUS_NOT_IN_SEASON
        // Snapshot lifted from a retail BfA-era sniff so the modern Premade
        // Group browser doesn't show entries the legacy backend can't deliver.
        // Skipped entirely for V3_4_3 (see early-return above) — Wrathion
        // 3.4.3 native server never sends this packet.
        // Gated ExpansionVersion > 1: Vanilla 1.14 client has no LFG List at all.
        LFGListUpdateBlacklist blacklist = new LFGListUpdateBlacklist();
        if (ModernVersion.ExpansionVersion > 1)
        {
            blacklist.AddBlacklist(796, 3);
            blacklist.AddBlacklist(797, 3);
            blacklist.AddBlacklist(798, 3);
            blacklist.AddBlacklist(799, 3);
            blacklist.AddBlacklist(800, 3);
            blacklist.AddBlacklist(801, 3);
            blacklist.AddBlacklist(802, 3);
            blacklist.AddBlacklist(803, 3);
            blacklist.AddBlacklist(804, 3);
            blacklist.AddBlacklist(805, 3);
            blacklist.AddBlacklist(806, 3);
            blacklist.AddBlacklist(807, 3);
            blacklist.AddBlacklist(808, 3);
            blacklist.AddBlacklist(809, 3);
            blacklist.AddBlacklist(810, 3);
            blacklist.AddBlacklist(811, 3);
            blacklist.AddBlacklist(812, 3);
            blacklist.AddBlacklist(813, 3);
            blacklist.AddBlacklist(814, 3);
            blacklist.AddBlacklist(815, 3);
            blacklist.AddBlacklist(816, 3);
            blacklist.AddBlacklist(817, 3);
            blacklist.AddBlacklist(818, 3);
            blacklist.AddBlacklist(820, 3);
            blacklist.AddBlacklist(827, 3);
            blacklist.AddBlacklist(828, 3);
            blacklist.AddBlacklist(829, 3);
            blacklist.AddBlacklist(835, 1031);
            blacklist.AddBlacklist(837, 3);
            blacklist.AddBlacklist(849, 1031);
            blacklist.AddBlacklist(850, 1031);
            blacklist.AddBlacklist(851, 1031);
            blacklist.AddBlacklist(852, 1031);
            blacklist.AddBlacklist(853, 3);
            blacklist.AddBlacklist(854, 3);
            blacklist.AddBlacklist(855, 3);
            blacklist.AddBlacklist(856, 3);
            blacklist.AddBlacklist(857, 3);
            blacklist.AddBlacklist(858, 3);
            blacklist.AddBlacklist(859, 3);
            blacklist.AddBlacklist(860, 3);
            blacklist.AddBlacklist(861, 3);
            blacklist.AddBlacklist(862, 3);
            blacklist.AddBlacklist(863, 3);
            blacklist.AddBlacklist(864, 3);
            blacklist.AddBlacklist(865, 3);
            blacklist.AddBlacklist(866, 3);
            blacklist.AddBlacklist(867, 3);
            blacklist.AddBlacklist(868, 3);
            blacklist.AddBlacklist(869, 3);
            blacklist.AddBlacklist(870, 3);
            blacklist.AddBlacklist(871, 3);
            blacklist.AddBlacklist(872, 3);
            blacklist.AddBlacklist(873, 3);
            blacklist.AddBlacklist(874, 3);
            blacklist.AddBlacklist(875, 3);
            blacklist.AddBlacklist(876, 3);
            blacklist.AddBlacklist(877, 3);
            blacklist.AddBlacklist(878, 3);
            blacklist.AddBlacklist(879, 3);
            blacklist.AddBlacklist(880, 3);
            blacklist.AddBlacklist(881, 3);
            blacklist.AddBlacklist(882, 3);
            blacklist.AddBlacklist(883, 3);
            blacklist.AddBlacklist(884, 3);
            blacklist.AddBlacklist(885, 3);
            blacklist.AddBlacklist(886, 3);
            blacklist.AddBlacklist(887, 3);
            blacklist.AddBlacklist(888, 3);
            blacklist.AddBlacklist(889, 3);
            blacklist.AddBlacklist(890, 3);
            blacklist.AddBlacklist(891, 3);
            blacklist.AddBlacklist(892, 3);
            blacklist.AddBlacklist(893, 3);
            blacklist.AddBlacklist(898, 3);
            blacklist.AddBlacklist(899, 3);
            blacklist.AddBlacklist(900, 3);
            blacklist.AddBlacklist(901, 3);
            blacklist.AddBlacklist(902, 1031);
            blacklist.AddBlacklist(917, 1031);
            blacklist.AddBlacklist(919, 3);
            blacklist.AddBlacklist(920, 3);
            blacklist.AddBlacklist(921, 3);
            blacklist.AddBlacklist(922, 3);
            blacklist.AddBlacklist(923, 3);
            blacklist.AddBlacklist(924, 3);
            blacklist.AddBlacklist(926, 3);
            blacklist.AddBlacklist(927, 3);
            blacklist.AddBlacklist(928, 3);
            blacklist.AddBlacklist(929, 3);
            blacklist.AddBlacklist(930, 3);
            blacklist.AddBlacklist(932, 3);
            blacklist.AddBlacklist(934, 3);
        }
        ctx.SendPacket(blacklist);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_CONQUEST_FORMULA_CONSTANTS)]
    public static void HandleRequestConquestFormulaConstants(in EmptyClientPacket request, in SessionContext ctx)
    {
        ConquestFormulaConstants response = new ConquestFormulaConstants();
        response.PvpMinCPPerWeek = 1500;
        response.PvpMaxCPPerWeek = 3000;
        response.PvpCPBaseCoefficient = 1511.26f;
        response.PvpCPExpCoefficient = 1639.28f;
        response.PvpCPNumerator = 0.00412f;
        ctx.SendPacket(response);
    }

    [HandlesCmsg(Opcode.CMSG_MOUNT_SPECIAL_ANIM)]
    public static void HandleMountSpecialAnim(in MountSpecial mount, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MOUNT_SPECIAL_ANIM);
        ctx.SendPacketToServer(packet);
    }
}
