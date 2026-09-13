using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_MGR_ENTRY_INVITE)]
    internal void HandleBattlefieldMgrEntryInvite(WorldPacket packet)
    {
        uint battleId = packet.ReadUInt32();
        uint zoneId = packet.ReadUInt32();
        uint expireUnix = packet.ReadUInt32();

        uint ticketId = BattlefieldMgrTranslation.TicketFor(battleId, BattlefieldMgrTicketKind.Entry);
        uint listId = BattlefieldMgrTranslation.ListIdForBattle(battleId);
        uint timeoutMs = BattlefieldMgrTranslation.TimeoutMs(expireUnix, Time.UnixTime);

        GetSession().GameState.StoreBattleFieldQueueType(ticketId, listId);

        var confirm = new BattlefieldStatusNeedConfirmation();
        FillMgrHeader(confirm.Hdr, ticketId, listId);
        confirm.Mapid = BattlefieldMgrTranslation.LegacyWintergraspMapId;
        confirm.Timeout = timeoutMs;

        BattleGroundLogMessages.EntryInvite(
            _melLog, "ENTRY_INVITE", battleId, zoneId, expireUnix, ticketId, listId, timeoutMs);
        SendPacketToClient(confirm);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_MGR_QUEUE_INVITE)]
    internal void HandleBattlefieldMgrQueueInvite(WorldPacket packet)
    {
        uint battleId = packet.ReadUInt32();
        byte warmup = packet.ReadUInt8();

        uint ticketId = BattlefieldMgrTranslation.TicketFor(battleId, BattlefieldMgrTicketKind.Queue);
        uint listId = BattlefieldMgrTranslation.ListIdForBattle(battleId);
        GetSession().GameState.StoreBattleFieldQueueType(ticketId, listId);

        var confirm = new BattlefieldStatusNeedConfirmation();
        FillMgrHeader(confirm.Hdr, ticketId, listId);
        confirm.Mapid = BattlefieldMgrTranslation.LegacyWintergraspMapId;
        confirm.Timeout = BattlefieldMgrTranslation.DefaultInviteTimeoutMs;

        BattleGroundLogMessages.QueueInvite(_melLog, "QUEUE_INVITE", battleId, warmup, ticketId);
        SendPacketToClient(confirm);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_MGR_QUEUE_REQUEST_RESPONSE)]
    internal void HandleBattlefieldMgrQueueRequestResponse(WorldPacket packet)
    {
        uint battleId = packet.ReadUInt32();
        packet.ReadUInt32();
        byte canQueue = packet.ReadUInt8();
        byte loggingIn = packet.ReadUInt8();
        byte warmup = packet.ReadUInt8();

        uint ticketId = BattlefieldMgrTranslation.TicketFor(battleId, BattlefieldMgrTicketKind.Queue);
        uint listId = BattlefieldMgrTranslation.ListIdForBattle(battleId);
        GetSession().GameState.StoreBattleFieldQueueType(ticketId, listId);

        BattleGroundLogMessages.QueueResponse(
            _melLog, "QUEUE_RESPONSE", battleId, canQueue, loggingIn, warmup);

        if (canQueue == 0)
        {
            SendMgrFailed(ticketId, listId);
            GetSession().GameState.RemoveBattleFieldQueue(ticketId);
            return;
        }

        var queued = new BattlefieldStatusQueued();
        FillMgrHeader(queued.Hdr, ticketId, listId);
        queued.EligibleForMatchmaking = true;
        SendPacketToClient(queued);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_MGR_ENTERING)]
    internal void HandleBattlefieldMgrEntering(WorldPacket packet)
    {
        uint battleId = packet.ReadUInt32();
        packet.ReadUInt8();
        packet.ReadUInt8();
        packet.ReadUInt8();

        uint ticketId = BattlefieldMgrTranslation.TicketFor(battleId, BattlefieldMgrTicketKind.Entry);
        uint listId = BattlefieldMgrTranslation.ListIdForBattle(battleId);
        GetSession().GameState.StoreBattleFieldQueueType(ticketId, listId);

        var active = new BattlefieldStatusActive();
        FillMgrHeader(active.Hdr, ticketId, listId);
        active.Mapid = BattlefieldMgrTranslation.LegacyWintergraspMapId;

        BattleGroundLogMessages.Entered(
            _melLog, "ENTERING", battleId, BattlefieldMgrTranslation.LegacyWintergraspMapId);
        SendPacketToClient(active);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_MGR_EJECTED)]
    internal void HandleBattlefieldMgrEjected(WorldPacket packet)
    {
        uint battleId = packet.ReadUInt32();
        byte reason = packet.ReadUInt8();
        byte status = packet.ReadUInt8();
        byte relocated = packet.ReadUInt8();

        uint entryTicket = BattlefieldMgrTranslation.TicketFor(battleId, BattlefieldMgrTicketKind.Entry);
        uint queueTicket = BattlefieldMgrTranslation.TicketFor(battleId, BattlefieldMgrTicketKind.Queue);
        uint listId = BattlefieldMgrTranslation.ListIdForBattle(battleId);

        BattleGroundLogMessages.Ejected(_melLog, "EJECTED", battleId, reason, status, relocated);

        SendMgrFailed(entryTicket, listId);
        SendMgrFailed(queueTicket, listId);
        GetSession().GameState.RemoveBattleFieldQueue(entryTicket);
        GetSession().GameState.RemoveBattleFieldQueue(queueTicket);
    }

    void FillMgrHeader(BattlefieldStatusHeader hdr, uint ticketId, uint listId)
    {
        hdr.Ticket.Id = ticketId;
        hdr.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
        hdr.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(ticketId);
        hdr.Ticket.Type = RideType.Battlegrounds;
        hdr.BattlefieldListIDs.Add(listId);
        hdr.RangeMin = BattlefieldMgrTranslation.MinLevel;
        hdr.RangeMax = BattlefieldMgrTranslation.MaxLevel;
    }

    internal void ClearWintergraspMgrState()
    {
        uint entryTicket = BattlefieldMgrTranslation.TicketFor(
            BattlefieldMgrTranslation.WintergraspBattleId, BattlefieldMgrTicketKind.Entry);
        uint queueTicket = BattlefieldMgrTranslation.TicketFor(
            BattlefieldMgrTranslation.WintergraspBattleId, BattlefieldMgrTicketKind.Queue);
        uint listId = BattlefieldMgrTranslation.ListIdForBattle(
            BattlefieldMgrTranslation.WintergraspBattleId);

        if (GetSession().GameState.GetBattleFieldQueueType(entryTicket) != 0)
            SendMgrFailed(entryTicket, listId);
        if (GetSession().GameState.GetBattleFieldQueueType(queueTicket) != 0)
            SendMgrFailed(queueTicket, listId);

        GetSession().GameState.RemoveBattleFieldQueue(entryTicket);
        GetSession().GameState.RemoveBattleFieldQueue(queueTicket);
    }

    void SendMgrFailed(uint ticketId, uint listId)
    {
        var failed = new BattlefieldStatusFailed();
        failed.Ticket.Id = ticketId;
        failed.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
        failed.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(ticketId);
        failed.Ticket.Type = RideType.Battlegrounds;
        failed.BattlefieldListId = listId;
        failed.Reason = 30;
        SendPacketToClient(failed);
    }
}
