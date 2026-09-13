using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_TAXI_NODE_STATUS)]
    internal void HandleTaxiNodeStatus(WorldPacket packet)
    {
        TaxiNodeStatusPkt taxi = new();
        taxi.FlightMaster = packet.ReadGuid().To128(GetSession().GameState);
        bool learned = packet.ReadBool();
        taxi.Status = learned ? TaxiNodeStatus.Learned : TaxiNodeStatus.Unlearned;
        SendPacketToClient(taxi);
    }
    [HandlesSmsg(Opcode.SMSG_SHOW_TAXI_NODES)]
    internal void HandleShowTaxiNodes(WorldPacket packet)
    {
        uint playerFlags = GetSession().GameState.GetLegacyFieldValueUInt32(GetSession().GameState.CurrentPlayerGuid, PlayerField.PLAYER_FLAGS);
        if (playerFlags.HasAnyFlag(PlayerFlags.GM))
        {
            ChatPkt chat = new ChatPkt(GetSession(), ChatMessageTypeModern.System, "Disable GM mode before talking to taxi master or your game will freeze.");
            SendPacketToClient(chat);
            return;
        }

        ShowTaxiNodes taxi = new();
        bool hasWindowInfo = packet.ReadUInt32() != 0;
        if (hasWindowInfo)
        {
            taxi.WindowInfo = new();
            taxi.WindowInfo.UnitGUID = packet.ReadGuid().To128(GetSession().GameState);
            taxi.WindowInfo.CurrentNode = GetSession().GameState.CurrentTaxiNode = packet.ReadUInt32();
        }
        while (packet.CanRead())
        {
            byte nodesMask = packet.ReadUInt8();
            taxi.CanLandNodes.Add(nodesMask);
            taxi.CanUseNodes.Add(nodesMask);
        }
        GetSession().GameState.UsableTaxiNodes = taxi.CanUseNodes; // save for CMSG_ACTIVATE_TAXI_EXPRESS
        SendPacketToClient(taxi);
    }
    [HandlesSmsg(Opcode.SMSG_NEW_TAXI_PATH)]
    internal void HandleNewTaxiPath(WorldPacket packet)
    {
        NewTaxiPath taxi = new();
        SendPacketToClient(taxi);
    }
    [HandlesSmsg(Opcode.SMSG_ACTIVATE_TAXI_REPLY)]
    internal void HandleActivateTaxiReply(WorldPacket packet)
    {
        ActivateTaxiReply reply = (ActivateTaxiReply)packet.ReadUInt32();
        // Ok status needs to be sent after the monster move packet.
        if (reply != ActivateTaxiReply.Ok)
        {
            ActivateTaxiReplyPkt taxi = new();
            taxi.Reply = reply;
            SendPacketToClient(taxi);
            GetSession().GameState.IsWaitingForTaxiStart = false;
        }
    }
}
