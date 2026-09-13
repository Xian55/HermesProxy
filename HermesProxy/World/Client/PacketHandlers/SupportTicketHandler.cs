using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Legacy 3.3.5a answers with a uint32 status; modern wants an int32. Same single value, so
    // the backend's own GMTicketSystemStatus config reaches the client unchanged.
    [HandlesSmsg(Opcode.SMSG_GM_TICKET_GET_SYSTEM_STATUS)]
    internal void HandleGmTicketGetSystemStatus(WorldPacket packet)
    {
        GMTicketSystemStatus status = new();
        status.Status = (int)packet.ReadUInt32();
        SendPacketToClient(status);
    }

    [HandlesSmsg(Opcode.SMSG_GM_TICKET_CREATE)]
    internal void HandleGmTicketCreate(WorldPacket packet)
    {
        var response = (LegacyGmTicketResponse) packet.ReadUInt32();
        bool isError = !(response is LegacyGmTicketResponse.CreateSuccess or LegacyGmTicketResponse.UpdateSuccess);
        Session.SendHermesTextMessage($"GM Ticket Status: {response}", isError);
    }
}
