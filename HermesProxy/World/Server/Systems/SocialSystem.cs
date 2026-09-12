using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's friend and ignore CMSGs.
/// </summary>
/// <remarks>
/// Vanilla split the contact list into separate friend and ignore opcodes and had no contact notes
/// at all, which is what the <c>V2_0_1_6180</c> guards below are. <see cref="HandleDelFriend"/> is
/// shape B: the same body serves remove-friend and remove-ignore, and the opcode it forwards is the
/// one it received.
/// </remarks>
public static class SocialSystem
{
    [HandlesCmsg(Opcode.CMSG_CONTACT_LIST)]
    public static void HandleContactList(in ContactListRequest contacts, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_FRIEND_LIST);
            ctx.SendPacketToServer(packet);
        }
        else
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CONTACT_LIST);
            packet.WriteUInt32((uint)contacts.Flags);
            ctx.SendPacketToServer(packet);
        }
    }

    [HandlesCmsg(Opcode.CMSG_ADD_FRIEND)]
    public static void HandleAddFriend(in AddFriend friend, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ADD_FRIEND);
        packet.WriteCString(friend.Name);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteCString(friend.Note);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ADD_IGNORE)]
    public static void HandleAddIgnore(in AddIgnore ignore, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ADD_IGNORE);
        packet.WriteCString(ignore.Name);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_DEL_FRIEND)]
    [HandlesCmsg(Opcode.CMSG_DEL_IGNORE)]
    public static void HandleDelFriend(Opcode opcode, in DelFriend friend, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteGuid(friend.Guid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_CONTACT_NOTES)]
    public static void HandleSetContactNotes(in SetContactNotes friend, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_CONTACT_NOTES);
        packet.WriteGuid(friend.Guid.To64());
        packet.WriteCString(friend.Notes);
        ctx.SendPacketToServer(packet);
    }
}
