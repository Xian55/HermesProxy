using System;
using System.Collections.Generic;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    [HandlesSmsg(Opcode.SMSG_PHASE_SHIFT_CHANGE)]
    internal void HandlePhaseShiftChange(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            SendPacketToClient(ReadPhaseShiftChangeCata(packet));
            return;
        }

        uint mask = packet.ReadUInt32();
        var msg = PhaseShiftTranslation.ToModern(mask, GetSession().GameState.CurrentPlayerGuid);
        BattleGroundLogMessages.PhaseShift(_melLog, mask, msg.PhaseShiftFlags, msg.Phases.Count);
        SendPacketToClient(msg);
    }

    /// <summary>
    /// TrinityCore 4.3.4 PhaseShiftChange::Write: real phase ids and terrain swaps, the modern
    /// packet's content, behind a masked GUID. Each list is prefixed by its size in bytes, two
    /// per id. Read as 3.3.5a's one-word mask, Kezan's phases never reached the client.
    /// </summary>
    PhaseShiftChange ReadPhaseShiftChangeCata(WorldPacket packet)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> guid = stackalloc byte[8];
        var msg = new PhaseShiftChange();

        MaskedGuid.ReadMaskBits(packet, mask, [2, 3, 1, 6, 4, 5, 0, 7]);
        MaskedGuid.ReadBytes(packet, mask, guid, [7, 4]);
        // TrinityCore 4.3.4 fills this list with WorldMapArea ids (terrain_worldmap: Gilneas 545,
        // Hyjal 683, on every map). The modern client reads UiMapXMapArt phase ids here, and with
        // those it found no art for the map it was on: the world map threw on open
        // (MapCanvas_ScrollContainerMixin.lua:428, 'layers' nil). Without a mapping between the
        // two, the client keeps each map's default art.
        ReadIdList(packet, []);
        MaskedGuid.ReadByte(packet, mask, guid, 1);
        msg.PhaseShiftFlags = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, mask, guid, [2, 6]);
        ReadIdList(packet, msg.PreloadMapIDs);

        var phases = new List<ushort>();
        ReadIdList(packet, phases);
        foreach (ushort phase in phases)
            msg.Phases.Add((0, phase));

        MaskedGuid.ReadBytes(packet, mask, guid, [3, 0]);
        ReadIdList(packet, msg.VisibleMapIDs);
        MaskedGuid.ReadByte(packet, mask, guid, 5);

        msg.Client = new WowGuid64(MaskedGuid.ToUInt64(guid)).To128(GetSession().GameState);
        return msg;

        static void ReadIdList(WorldPacket packet, List<ushort> ids)
        {
            uint bytes = packet.ReadUInt32();
            for (uint i = 0; i < bytes / sizeof(ushort); i++)
                ids.Add(packet.ReadUInt16());
        }
    }
}
