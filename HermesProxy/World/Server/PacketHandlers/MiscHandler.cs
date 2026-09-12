using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{

    [PacketHandler(Opcode.CMSG_MOUNT_SPECIAL_ANIM)]
    void HandleMountSpecialAnim(MountSpecial mount)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MOUNT_SPECIAL_ANIM);
        SendPacketToServer(packet);
    }




}
