using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's item CMSGs. Behaviour only.
/// </summary>
public static class ItemSystem
{
    [HandlesCmsg(Opcode.CMSG_BUY_BACK_ITEM)]
    public static void HandleBuyBackItem(in BuyBackItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_BACK_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        byte slot = ModernVersion.AdjustModernInventorySlotToLegacy((byte)item.Slot);
        packet.WriteUInt32(slot);
        ctx.SendPacketToServer(packet);
    }
}
