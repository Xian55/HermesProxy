using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    const int EquipmentSetSlots = 19;

    [HandlesSmsg(Opcode.SMSG_LOAD_EQUIPMENT_SET)]
    internal void HandleLoadEquipmentSet(WorldPacket packet)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        LoadEquipmentSet load = new();
        uint count = packet.ReadUInt32();
        for (uint i = 0; i < count; i++)
        {
            EquipmentSetData set = new();
            set.Type = 0;
            set.Guid = packet.ReadPackedGuid().Low;
            set.SetID = packet.ReadUInt32();
            set.SetName = packet.ReadCString();
            set.SetIcon = packet.ReadCString();

            for (int slot = 0; slot < EquipmentSetSlots; slot++)
            {
                WowGuid64 item = packet.ReadPackedGuid();
                if (item.Low == 1)
                {
                    set.IgnoreMask |= 1u << slot;
                    set.Pieces[slot] = EquipmentSetModern.IgnoredSlot;
                }
                else if (!item.IsEmpty())
                    set.Pieces[slot] = item.To128(GetSession().GameState);
            }

            load.Sets.Add(set);
        }

        SendPacketToClient(load);
    }

    [HandlesSmsg(Opcode.SMSG_EQUIPMENT_SET_ID)]
    internal void HandleEquipmentSetId(WorldPacket packet)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        EquipmentSetID id = new();
        id.SetID = packet.ReadUInt32();
        id.GUID = packet.ReadPackedGuid().Low;
        id.Type = 0;
        SendPacketToClient(id);
    }

    [HandlesSmsg(Opcode.SMSG_USE_EQUIPMENT_SET_RESULT)]
    internal void HandleUseEquipmentSetResult(WorldPacket packet)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        UseEquipmentSetResult result = new();
        result.Reason = packet.ReadUInt8();
        result.GUID = GetSession().GameState.LastUsedEquipmentSetGuid;
        SendPacketToClient(result);
    }
}
