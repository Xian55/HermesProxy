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

    [PacketHandler(Opcode.CMSG_DF_LEAVE)]
    void HandleDFLeave(DFLeavePkt packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        WorldPacket leave = new WorldPacket(Opcode.CMSG_LFG_LEAVE);
        SendPacketToServer(leave);

        // V3_4_3 client uses a single CMSG_DF_LEAVE for both "leave queue"
        // (while waiting) and "leave dungeon" (when already inside the
        // instance). Legacy 3.3.5a splits these into CMSG_LFG_LEAVE (queue) +
        // CMSG_LFG_TELEPORT(out=1) (instance exit). Without the second packet,
        // the player is removed from the LFG group but stays inside the
        // dungeon. Send unconditionally — when not currently in an LFG
        // instance, legacy server's HandleLfgTeleportOpcode→TeleportPlayer
        // gracefully no-ops.
        WorldPacket teleport = new WorldPacket(Opcode.CMSG_LFG_TELEPORT);
        teleport.WriteUInt8(1); // out = true
        SendPacketToServer(teleport);
    }

    [PacketHandler(Opcode.CMSG_DF_SET_ROLES)]
    void HandleDFSetRoles(DFSetRolesPkt packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        WorldPacket legacy = new WorldPacket(Opcode.CMSG_LFG_SET_ROLES);
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
