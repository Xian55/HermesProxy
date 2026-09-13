using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's equipment-set CMSGs.
/// </summary>
/// <remarks>
/// All three are gated to V3_4_3 and drop silently on anything else, which is how they arrived —
/// the legacy opcodes exist only from 3.3.5a and the modern layouts were only ever verified
/// against the 3.4.3 client.
/// <para>
/// The two send paths translate a modern 128-bit item GUID per slot into the legacy packed 64-bit
/// form, with two sentinel values that are not GUIDs at all: a slot the set ignores goes out as
/// packed GUID 1, and an empty slot as a packed zero. Those are the values 3.3.5a's
/// HandleEquipmentSetSave and HandleEquipmentSetUse test for, so neither can be folded into the
/// ordinary conversion.
/// </para>
/// </remarks>
public static class EquipmentSetSystem
{
    const int EquipmentSetSlots = 19;

    [HandlesCmsg(Opcode.CMSG_SAVE_EQUIPMENT_SET)]
    public static void HandleSaveEquipmentSet(in SaveEquipmentSet save, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_SAVE_EQUIPMENT_SET);
        packet.WritePackedGuid(new WowGuid64(save.Set.Guid));
        packet.WriteUInt32(save.Set.SetID);
        packet.WriteCString(save.Set.SetName);
        packet.WriteCString(save.Set.SetIcon);

        for (int i = 0; i < EquipmentSetSlots; i++)
        {
            if ((save.Set.IgnoreMask & (1u << i)) != 0 || save.Set.Pieces[i] == EquipmentSetModern.IgnoredSlot)
                packet.WritePackedGuid(new WowGuid64(1));
            else if (save.Set.Pieces[i].IsEmpty())
                packet.WritePackedGuid(default);
            else
                packet.WritePackedGuid(save.Set.Pieces[i].To64());
        }

        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_DELETE_EQUIPMENT_SET)]
    public static void HandleDeleteEquipmentSet(in DeleteEquipmentSet delete, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_EQUIPMENT_SET_DELETE);
        packet.WritePackedGuid(new WowGuid64(delete.ID));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_USE_EQUIPMENT_SET)]
    public static void HandleUseEquipmentSet(in UseEquipmentSet use, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        ctx.GetSession().GameState.LastUsedEquipmentSetGuid = use.GUID;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_EQUIPMENT_SET_USE);
        for (int i = 0; i < EquipmentSetSlots; i++)
        {
            EquipmentSetItem slot = use.Items[i];
            if (slot.Item == EquipmentSetModern.IgnoredSlot)
                packet.WritePackedGuid(new WowGuid64(1));
            else if (slot.Item.IsEmpty())
                packet.WritePackedGuid(default);
            else
                packet.WritePackedGuid(slot.Item.To64());

            packet.WriteUInt8(slot.ContainerSlot);
            packet.WriteUInt8(slot.Slot);
        }

        ctx.SendPacketToServer(packet);
    }
}
