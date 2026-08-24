using System;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Modern client -> Legacy server (LFG / Dungeon Finder).
    // LFG (Dungeon Finder) was added in WotLK 3.3.0; pre-WotLK legacy backends
    // do not implement these opcodes. Gate accordingly.

    [PacketHandler(Opcode.CMSG_DF_GET_SYSTEM_INFO)]
    void HandleDFGetSystemInfo(DFGetSystemInfoPkt packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        WorldPacket legacy = new WorldPacket(packet.Player
            ? Opcode.CMSG_LFG_PLAYER_LOCK_INFO_REQUEST
            : Opcode.CMSG_LFG_PARTY_LOCK_INFO_REQUEST);
        SendPacketToServer(legacy);
    }

    [PacketHandler(Opcode.CMSG_DF_GET_JOIN_STATUS)]
    void HandleDFGetJoinStatus(DFGetJoinStatusPkt packet)
    {
        // No equivalent legacy request; client polls this — drop silently.
    }

    [PacketHandler(Opcode.CMSG_DF_JOIN)]
    void HandleDFJoin(DFJoinPkt packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        // Legacy 3.3.5a CMSG_LFG_JOIN:
        //   uint32 Roles
        //   uint8  NoPartialClear
        //   uint8  Achievements
        //   uint8  slotCount
        //   uint32 Slots[slotCount]
        //   uint8  needsCount (always 3)
        //   uint8  Needs[3]
        //   cstr   Comment
        Log.Print(LogType.Debug,
            $"LFG[diag]: CMSG_DF_JOIN roles=0x{packet.Roles:X8} slots=[{string.Join(", ", packet.Slots)}]");

        GetSession().GameState.LfgRequestedRoles = (byte)(packet.Roles & 0xFF);

        // Titan Rune / other post-3.3.5 LFGDungeons IDs. A legacy backend drops
        // CMSG_LFG_JOIN for those with no SMSG_LFG_JOIN_RESULT, so the client sits
        // on Find Group forever. Answer for the backend instead. Real 3.3.5
        // specifics are forwarded even if they were never listed in PLAYER_INFO.
        if (LfgSlots.TryFindUnknownDungeon(packet.Slots, out uint unknownDungeonId))
        {
            Log.Print(LogType.Debug,
                $"LFG[diag]: rejecting CMSG_DF_JOIN, dungeon {unknownDungeonId} is unknown to the {LegacyVersion.Build} backend");
            SendDFJoinFailure(LfgJoinResults.ModernInvalidSlot);
            return;
        }

        WorldPacket legacy = new WorldPacket(Opcode.CMSG_LFG_JOIN);
        legacy.WriteUInt32(packet.Roles);
        legacy.WriteUInt8(0); // NoPartialClear
        legacy.WriteUInt8(0); // Achievements
        legacy.WriteUInt8((byte)packet.Slots.Length);
        foreach (var slot in packet.Slots)
            legacy.WriteUInt32(slot);
        legacy.WriteUInt8(3);
        legacy.WriteUInt8(0);
        legacy.WriteUInt8(0);
        legacy.WriteUInt8(0);
        legacy.WriteCString(string.Empty);
        SendPacketToServer(legacy);
    }

    private void SendDFJoinFailure(byte modernResult)
    {
        DFJoinResult response = new DFJoinResult
        {
            Ticket = new RideTicket
            {
                RequesterGuid = GetSession().GameState.CurrentPlayerGuid,
                Id = 1,
                Type = RideType.Lfg,
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            },
            Result = modernResult,
            ResultDetail = 0,
        };
        SendPacket(response);
    }

    [PacketHandler(Opcode.CMSG_DF_LEAVE)]
    void HandleDFLeave(DFLeavePkt packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        // Queue leave only. This used to also inject CMSG_LFG_TELEPORT(out=1) on the belief
        // that V3_4_3 overloads CMSG_DF_LEAVE for "leave dungeon" because CMSG_DF_TELEPORT did
        // not exist before V3_4_4. That is wrong: CMSG_DF_TELEPORT is 0x3619 in V3_4_3_54261
        // and the client does send it for the minimap eye's teleport entries (see
        // HandleDFTeleport). Injecting the teleport here meant clicking "Leave Queue" while
        // standing inside a dungeon yanked the player out of the instance without them ever
        // asking for it. Legacy LFGMgr::LeaveLfg has no LFG_STATE_DUNGEON case at all, so
        // leaving from inside is a server-side no-op — which is what a native client sees too.
        WorldPacket leave = new WorldPacket(Opcode.CMSG_LFG_LEAVE);
        SendPacketToServer(leave);
    }

    [PacketHandler(Opcode.CMSG_DF_TELEPORT)]
    void HandleDFTeleport(DFTeleportPkt packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        // The eye's teleport entry, which CMSG_DF_LEAVE does not cover: leaving drops the
        // queue/group, this just moves the player across the instance boundary and leaves the
        // LFG association alone. Without this the "Teleport to Dungeon" option was dropped on
        // the floor, so a player who had teleported out could never get back in.
        Log.Print(LogType.Debug, $"LFG[diag]: CMSG_DF_TELEPORT out={packet.TeleportOut}");

        WorldPacket legacy = new WorldPacket(Opcode.CMSG_LFG_TELEPORT);
        legacy.WriteUInt8((byte)(packet.TeleportOut ? 1 : 0));
        SendPacketToServer(legacy);
    }

    [PacketHandler(Opcode.CMSG_DF_SET_ROLES)]
    void HandleDFSetRoles(DFSetRolesPkt packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        WorldPacket legacy = new WorldPacket(Opcode.CMSG_LFG_SET_ROLES);
        Log.Print(LogType.Debug, $"LFG[diag]: CMSG_DF_SET_ROLES roles=0x{packet.Roles:X2}");

        GetSession().GameState.LfgRequestedRoles = packet.Roles;
        legacy.WriteUInt8(packet.Roles);
        SendPacketToServer(legacy);
    }

    [PacketHandler(Opcode.CMSG_DF_PROPOSAL_RESPONSE)]
    void HandleDFProposalResponse(DFProposalResponsePkt packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        WorldPacket legacy = new WorldPacket(Opcode.CMSG_LFG_PROPOSAL_RESULT);
        legacy.WriteUInt32(packet.ProposalID);
        legacy.WriteUInt8((byte)(packet.Accepted ? 1 : 0));
        SendPacketToServer(legacy);
    }

    [PacketHandler(Opcode.CMSG_LFG_LIST_GET_STATUS)]
    void HandleLFGListGetStatus(LFGListGetStatusPkt packet)
    {
        // Modern LFG list (browsable groups) — no legacy equivalent. Drop.
    }
}
