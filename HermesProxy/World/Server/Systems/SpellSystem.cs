using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Translation for the modern client's spell CMSGs. Behaviour only.</summary>
public static class SpellSystem
{
    [HandlesCmsg(Opcode.CMSG_CANCEL_MOUNT_AURA)]
    public static void HandleCancelMountAura(in EmptyClientPacket cancel, in SessionContext ctx)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_MOUNT_AURA);
            ctx.SendPacketToServer(packet);
        }
        else
        {
            WowGuid128 guid = ctx.GetSession().GameState.CurrentPlayerGuid;
            var updateFields = ctx.GetSession().GameState.GetCachedObjectFieldsLegacy(guid);
            if (updateFields == null)
                return;

            for (byte i = 0; i < 32; i++)
            {
                var aura = ctx.GetSession().WorldClient!.ReadAuraSlot(i, guid, updateFields);
                if (aura == null)
                    continue;

                if (GameData.MountAuras.Contains(aura.SpellID))
                {
                    WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_AURA);
                    packet.WriteUInt32(aura.SpellID);
                    ctx.SendPacketToServer(packet);
                }
            }
        }
    }
}
