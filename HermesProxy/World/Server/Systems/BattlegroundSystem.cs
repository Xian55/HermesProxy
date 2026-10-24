using System.Collections.Generic;
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
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleBattlemasterJoinOpcode: the battleground as a GUID and no
            // battlemaster, since 4.x queues from anywhere.
            ulong battleground = CataBattlegroundGuid(join.BattlefieldListId);
            ctx.GetSession().GameState.BattlefieldJoinRoles = join.Roles;
            packet.WriteInt32(join.BattlefieldInstanceID);
            MaskedGuid.WriteMaskBits(packet, battleground, [2, 0, 3, 1, 5]);
            packet.WriteBit(join.JoinAsGroup);
            MaskedGuid.WriteMaskBits(packet, battleground, [4, 6, 7]);
            packet.FlushBits();
            MaskedGuid.WriteBytes(packet, battleground, [2, 6, 4, 3, 7, 0, 5, 1]);
            ctx.SendPacketToServer(packet);
            return;
        }

        packet.WriteGuid(join.BattlemasterGuid.To64());
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt32(GameData.GetMapIdFromBattlegroundId(join.BattlefieldListId));
        else
            packet.WriteUInt32(join.BattlefieldListId);
        packet.WriteInt32(join.BattlefieldInstanceID);
        packet.WriteBool(join.JoinAsGroup);
        ctx.SendPacketToServer(packet);
    }

    /// <summary>
    /// A 4.3.4 battleground GUID: the battlemaster list id under the battleground high type
    /// (TrinityCore 4.3.4 <c>ObjectGuid(HighGuid::BattleGround, id)</c>, 0x1F1 shifted by 52). The
    /// modern client's queue id is the same number.
    /// </summary>
    internal static ulong CataBattlegroundGuid(uint battlefieldListId)
        => battlefieldListId == 0 ? 0 : (0x1F1ul << 52) | battlefieldListId;

    /// <summary>
    /// The role an invitation names, from the roles its join was sent with: the Dungeon Finder's
    /// mask in the join (tank 2, healer 4, damage 8), a number in the invitation (0, 1, 2).
    /// </summary>
    internal static byte InvitationRole(byte joinRoles)
        => (joinRoles & 2) != 0 ? (byte)0 : (joinRoles & 4) != 0 ? (byte)1 : (byte)2;

    [HandlesCmsg(Opcode.CMSG_BATTLEFIELD_LIST)]
    public static void HandleBattlefieldList(in BattlefieldListRequest request, in SessionContext ctx)
    {
        // V3_4_3-only: forwarding the PvP-UI BG-list query is part of the 54261 fix.
        // V1_14/V2_5 keep their original behaviour (request not forwarded) — no side effect.
        if (!ModernVersion.IsWotLKClassicOrLater)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_LIST);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleBattlefieldListOpcode: the list id and nothing after it
            packet.WriteUInt32((uint)request.ListID);
            ctx.SendPacketToServer(packet);
            return;
        }

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
            if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            {
                ulong battlefield = ctx.GetSession().GameState.BattlefieldMgrGuids.GetValueOrDefault(mgrBattleId);
                WriteLegacyBfInviteResponse(mgrPacket, mgrKind, battlefield, port.AcceptedInvite);
            }
            else
            {
                mgrPacket.WriteUInt32(mgrBattleId);
                mgrPacket.WriteUInt8(port.AcceptedInvite ? (byte)1 : (byte)0);
            }
            BattleGroundLogMessages.PortAsMgr(
                _melLog, port.Ticket.Id, port.AcceptedInvite, mgrOpcode.ToString(), mgrBattleId);
            ctx.GetSession().ToClient.Cancel(HermesProxy.World.Client.WorldClient.BattlefieldInviteExpiryKey(port.Ticket.Id));
            ctx.SendPacketToServer(mgrPacket);
            if (!port.AcceptedInvite && mgrKind == BattlefieldMgrTicketKind.Queue && HermesProxy.World.Client.WorldClient.IsCataLegacy)
                LeaveCataBattlefieldQueue(port.Ticket.Id, ctx.GameState.BattlefieldMgrGuids.GetValueOrDefault(mgrBattleId), ctx);
            else if (!port.AcceptedInvite)
                ctx.GetSession().GameState.RemoveBattleFieldQueue(port.Ticket.Id);
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_PORT);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleBattleFieldPortOpcode: the queue slot names the battle. The
            // join time, the int after the slot and the GUID are read and not used.
            uint queueSlot = port.Ticket.Id == 0 ? 0 : port.Ticket.Id - 1;
            ulong player = ctx.GetSession().GameState.CurrentPlayerGuid.To64().Low;
            packet.WriteUInt32(ctx.GetSession().GameState.GetBattleFieldQueueJoinTime(port.Ticket.Id));
            packet.WriteUInt32(queueSlot);
            packet.WriteUInt32(0);
            MaskedGuid.WriteMaskBits(packet, player, [0, 1, 5, 6, 7, 4, 3, 2]);
            packet.WriteBit(port.AcceptedInvite);
            packet.FlushBits();
            MaskedGuid.WriteBytes(packet, player, [1, 3, 5, 7, 0, 2, 6, 4]);
            ctx.SendPacketToServer(packet);
            return;
        }

        uint bgTypeId = ctx.GetSession().GameState.GetBattleFieldQueueType(port.Ticket.Id);

        // arenatype byte. The legacy server derives BattlegroundQueueTypeId from
        // (bgTypeId, arenaType). A non-zero type on a real BG misses the queue (#102).
        // A hardcoded 2 on arenas misses 3v3/5v5. V1_14/V2_5 keep the prior constant.
        bool isArena = GameData.Battlegrounds.TryGetValue(bgTypeId, out var bg) && bg.IsArena;
        byte queuedArenaType = ctx.GetSession().GameState.GetBattleFieldQueueArenaType(port.Ticket.Id);
        byte arenaType = BattlefieldQueueArenaType.ForLegacyPort(
            ModernVersion.IsWotLKClassicOrLater, isArena, queuedArenaType);

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
        // 4.3.4 split the two-way opcode into a request and an answer
        WorldPacket packet = new WorldPacket(HermesProxy.World.Client.WorldClient.IsCataLegacy
            ? Opcode.CMSG_PVP_LOG_DATA
            : Opcode.MSG_PVP_LOG_DATA);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_BATTLEFIELD_LEAVE)]
    public static void HandleBattlefieldLeave(in BattlefieldLeave leave, in SessionContext ctx)
    {
        var gameState = ctx.GetSession().GameState;
        uint zoneBattle = BattlefieldMgrTranslation.BattleIdForZone(gameState.CurrentZoneId);
        bool hasMgrTicket = zoneBattle != 0 &&
            (gameState.GetBattleFieldQueueType(BattlefieldMgrTranslation.TicketFor(zoneBattle, BattlefieldMgrTicketKind.Entry)) != 0 ||
            gameState.GetBattleFieldQueueType(BattlefieldMgrTranslation.TicketFor(zoneBattle, BattlefieldMgrTicketKind.Queue)) != 0);
        if (BattlefieldMgrTranslation.ShouldRouteLeaveToMgr(gameState.CurrentZoneId, hasMgrTicket))
        {
            WorldPacket exit = new WorldPacket(Opcode.CMSG_BF_MGR_QUEUE_EXIT_REQUEST);
            if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
                WriteLegacyBfExitRequest(exit, gameState.BattlefieldMgrGuids.GetValueOrDefault(zoneBattle));
            else
                exit.WriteUInt32(zoneBattle);
            BattleGroundLogMessages.LeaveAsMgr(_melLog, zoneBattle);
            ctx.SendPacketToServer(exit);
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_LEAVE);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleBattlefieldLeaveOpcode reads nothing: the server takes the
            // player out of the battleground they are in.
            ctx.SendPacketToServer(packet);
            return;
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            uint bgTypeId = ctx.GetSession().GameState.GetBattleFieldQueueType(1);
            bool isArena = GameData.Battlegrounds.TryGetValue(bgTypeId, out var bg) && bg.IsArena;
            byte arenaType = BattlefieldQueueArenaType.ForLegacyPort(
                ModernVersion.IsWotLKClassicOrLater, isArena,
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
    /// <summary>
    /// TrinityCore 4.3.4 HandleBfQueueInviteResponse / HandleBfEntryInviteResponse: the
    /// battlefield's GUID masked, the answer among its bits. The server finds the battle by the
    /// player's zone, not by the GUID.
    /// </summary>
    internal static void WriteLegacyBfInviteResponse(WorldPacket packet, BattlefieldMgrTicketKind kind, ulong battlefield, bool accepted)
    {
        if (kind == BattlefieldMgrTicketKind.Queue)
        {
            MaskedGuid.WriteMaskBits(packet, battlefield, [2, 0, 4, 3, 5, 7]);
            packet.WriteBit(accepted);
            MaskedGuid.WriteMaskBits(packet, battlefield, [1, 6]);
            packet.FlushBits();
            MaskedGuid.WriteBytes(packet, battlefield, [1, 3, 2, 4, 6, 7, 0, 5]);
        }
        else
        {
            MaskedGuid.WriteMaskBits(packet, battlefield, [6, 1]);
            packet.WriteBit(accepted);
            MaskedGuid.WriteMaskBits(packet, battlefield, [5, 3, 2, 0, 7, 4]);
            packet.FlushBits();
            MaskedGuid.WriteBytes(packet, battlefield, [0, 3, 4, 2, 1, 6, 7, 5]);
        }
    }

    /// <summary>
    /// The 4.4.2 client's Leave Queue on a battle it is queued for is the same refusal as on an
    /// offered queue. TrinityCore 4.3.4 acts on CMSG_BF_MGR_QUEUE_INVITE_RESPONSE only when it
    /// accepts, and takes a player out of the queue only on CMSG_BF_MGR_QUEUE_EXIT_REQUEST
    /// (Battlefield::AskToLeaveQueue, a no-op for one who never joined). It answers neither, so the
    /// ticket also goes from the client's list here, as a server of the client's own era answers
    /// with STATUS_NONE.
    /// </summary>
    static void LeaveCataBattlefieldQueue(uint ticketId, ulong battlefield, in SessionContext ctx)
    {
        WorldPacket exit = new WorldPacket(Opcode.CMSG_BF_MGR_QUEUE_EXIT_REQUEST);
        WriteLegacyBfExitRequest(exit, battlefield);
        ctx.SendPacketToServer(exit);

        if (ctx.GetSession().WorldClient is { } client)
            client.DropBattlefieldMgrTicket(ticketId);
        else
            ctx.GameState.RemoveBattleFieldQueue(ticketId);
    }

    /// <summary>TrinityCore 4.3.4 HandleBfExitRequest: the battlefield's GUID, nothing else.</summary>
    internal static void WriteLegacyBfExitRequest(WorldPacket packet, ulong battlefield)
    {
        MaskedGuid.WriteMaskBits(packet, battlefield, [2, 0, 3, 7, 4, 5, 6, 1]);
        packet.FlushBits();
        MaskedGuid.WriteBytes(packet, battlefield, [5, 2, 0, 1, 4, 3, 7, 6]);
    }

    /// <summary>
    /// The PvP window asks which kinds of PvP there are. Only a 4.x server knows the request
    /// (TrinityCore 4.3.4: CMSG_REQUEST_PVP_OPTIONS_ENABLED, empty).
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_GET_PVP_OPTIONS_ENABLED)]
    public static void HandleGetPvpOptionsEnabled(in EmptyClientPacket request, in SessionContext ctx)
    {
        if (!HermesProxy.World.Client.WorldClient.IsCataLegacy)
            return;

        ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_GET_PVP_OPTIONS_ENABLED));
    }

    /// <summary>
    /// The PvP window asks what a battleground pays. Only a 4.x server knows the request
    /// (TrinityCore 4.3.4: CMSG_REQUEST_PVP_REWARDS, empty); WorldClient translates the answer.
    /// A 3.3.5a server says it in its battlefield lists, and the proxy answers with the last one.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_REQUEST_PVP_REWARDS)]
    public static void HandleRequestPvpRewards(in EmptyClientPacket request, in SessionContext ctx)
    {
        if (!HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            if (ctx.GameState.WotLKBattlegroundRewards is { } rewards)
                ctx.SendPacketToClient(HermesProxy.World.Client.WorldClient.BuildPvpRewardsWotLK(rewards));
            return;
        }

        ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_REQUEST_PVP_REWARDS));
    }
}
