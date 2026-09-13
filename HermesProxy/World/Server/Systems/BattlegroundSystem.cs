using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's battleground and battlefield CMSGs.
/// </summary>
/// <remarks>
/// Two of these fork on where the request is really going. Wintergrasp is a "battlefield" rather
/// than a battleground on 3.3.5a and answers a different opcode family entirely, so
/// <see cref="HandleBattlefieldPort"/> and <see cref="HandleBattlefieldLeave"/> decode the ticket
/// (or the current zone) first and route to CMSG_BF_MGR_* when it belongs there.
/// <para>
/// The arena-type byte on the legacy port is load-bearing: the server derives its queue type from
/// (bgTypeId, arenaType), so a non-zero value on a real battleground misses the queue entirely —
/// that was issue #102 — while a hard-coded 2 on an arena misses 3v3 and 5v5.
/// </para>
/// </remarks>
public static class BattlegroundSystem
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);

    [HandlesCmsg(Opcode.CMSG_BATTLEMASTER_JOIN)]
    public static void HandleBattlefieldJoin(in BattlemasterJoin join, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN);
        packet.WriteGuid(join.BattlemasterGuid.To64());
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt32(GameData.GetMapIdFromBattlegroundId(join.BattlefieldListId));
        else
            packet.WriteUInt32(join.BattlefieldListId);
        packet.WriteInt32(join.BattlefieldInstanceID);
        packet.WriteBool(join.JoinAsGroup);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_BATTLEFIELD_LIST)]
    public static void HandleBattlefieldList(in BattlefieldListRequest request, in SessionContext ctx)
    {
        // V3_4_3-only: forwarding the PvP-UI BG-list query is part of the 54261 fix.
        // V1_14/V2_5 keep their original behaviour (request not forwarded) — no side effect.
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_LIST);
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt32(GameData.GetMapIdFromBattlegroundId((uint)request.ListID));
        else
            packet.WriteUInt32((uint)request.ListID);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt8(1); // fromWhere: 1 = PvP UI (lua RequestBattlegroundInstanceInfo)

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
            packet.WriteUInt8(0); // xpLocked

        Log.Print(LogType.Debug, $"[BG] CMSG_BATTLEFIELD_LIST request: client ListID={request.ListID} -> forwarding legacy bgTypeId={request.ListID} (size={packet.GetSize()}).");
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_BATTLEFIELD_PORT)]
    public static void HandleBattlefieldPort(in BattlefieldPort port, in SessionContext ctx)
    {
        if (BattlefieldMgrTranslation.TryDecodeTicket(port.Ticket.Id, out uint mgrBattleId, out var mgrKind))
        {
            Opcode mgrOpcode = mgrKind == BattlefieldMgrTicketKind.Queue
                ? Opcode.CMSG_BF_MGR_QUEUE_INVITE_RESPONSE
                : Opcode.CMSG_BF_MGR_ENTRY_INVITE_RESPONSE;
            WorldPacket mgrPacket = new WorldPacket(mgrOpcode);
            mgrPacket.WriteUInt32(mgrBattleId);
            mgrPacket.WriteUInt8(port.AcceptedInvite ? (byte)1 : (byte)0);
            BattleGroundLogMessages.PortAsMgr(
                _melLog, port.Ticket.Id, port.AcceptedInvite, mgrOpcode.ToString(), mgrBattleId);
            ctx.SendPacketToServer(mgrPacket);
            if (!port.AcceptedInvite)
                ctx.GetSession().GameState.RemoveBattleFieldQueue(port.Ticket.Id);
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_PORT);
        uint bgTypeId = ctx.GetSession().GameState.GetBattleFieldQueueType(port.Ticket.Id);

        // arenatype byte. The legacy server derives BattlegroundQueueTypeId from
        // (bgTypeId, arenaType). A non-zero type on a real BG misses the queue (#102).
        // A hardcoded 2 on arenas misses 3v3/5v5. V1_14/V2_5 keep the prior constant.
        bool isArena = GameData.Battlegrounds.TryGetValue(bgTypeId, out var bg) && bg.IsArena;
        byte queuedArenaType = ctx.GetSession().GameState.GetBattleFieldQueueArenaType(port.Ticket.Id);
        byte arenaType = BattlefieldQueueArenaType.ForLegacyPort(
            ModernVersion.Build == ClientVersionBuild.V3_4_3_54261, isArena, queuedArenaType);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            packet.WriteUInt8(arenaType);
            packet.WriteUInt8(ctx.GetSession().GameState.GetBattleFieldQueueBracketId(port.Ticket.Id));
            packet.WriteUInt32(bgTypeId);
            packet.WriteUInt16(0x1F90);
            packet.WriteBool(port.AcceptedInvite);
        }
        else
        {
            packet.WriteUInt32(bgTypeId);
            packet.WriteBool(port.AcceptedInvite);
        }
        Log.Print(LogType.Debug, $"[BG] CMSG_BATTLEFIELD_PORT: ticketId={port.Ticket.Id} AcceptedInvite={port.AcceptedInvite} -> legacy bgTypeId={bgTypeId} arenatype={arenaType} action={(port.AcceptedInvite ? 1 : 0)} size={packet.GetSize()}.");
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_BATTLEFIELD_STATUS)]
    public static void HandleRequestBattlefieldStatus(in RequestBattlefieldStatus log, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_STATUS);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PVP_LOG_DATA)]
    public static void HandlePvPLogData(in PVPLogDataRequest log, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_PVP_LOG_DATA);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_BATTLEFIELD_LEAVE)]
    public static void HandleBattlefieldLeave(in BattlefieldLeave leave, in SessionContext ctx)
    {
        uint entryTicket = BattlefieldMgrTranslation.TicketFor(
            BattlefieldMgrTranslation.WintergraspBattleId, BattlefieldMgrTicketKind.Entry);
        uint queueTicket = BattlefieldMgrTranslation.TicketFor(
            BattlefieldMgrTranslation.WintergraspBattleId, BattlefieldMgrTicketKind.Queue);
        bool hasMgrTicket = ctx.GetSession().GameState.GetBattleFieldQueueType(entryTicket) != 0 ||
            ctx.GetSession().GameState.GetBattleFieldQueueType(queueTicket) != 0;
        if (BattlefieldMgrTranslation.ShouldRouteLeaveToMgr(ctx.GetSession().GameState.CurrentZoneId, hasMgrTicket))
        {
            WorldPacket exit = new WorldPacket(Opcode.CMSG_BF_MGR_QUEUE_EXIT_REQUEST);
            exit.WriteUInt32(BattlefieldMgrTranslation.WintergraspBattleId);
            BattleGroundLogMessages.LeaveAsMgr(_melLog, BattlefieldMgrTranslation.WintergraspBattleId);
            ctx.SendPacketToServer(exit);
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_LEAVE);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            uint bgTypeId = ctx.GetSession().GameState.GetBattleFieldQueueType(1);
            bool isArena = GameData.Battlegrounds.TryGetValue(bgTypeId, out var bg) && bg.IsArena;
            byte arenaType = BattlefieldQueueArenaType.ForLegacyPort(
                ModernVersion.Build == ClientVersionBuild.V3_4_3_54261, isArena,
                ctx.GetSession().GameState.GetBattleFieldQueueArenaType(1));
            packet.WriteUInt8(arenaType);
            packet.WriteUInt8(ctx.GetSession().GameState.GetBattleFieldQueueBracketId(1));
            packet.WriteUInt32(bgTypeId);
            packet.WriteUInt16(0x1F90);
        }
        else
            packet.WriteUInt32((uint)ctx.GetSession().GameState.CurrentMapId!);
        ctx.SendPacketToServer(packet);
    }
}
