using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's reputation CMSGs.
/// </summary>
/// <remarks>
/// The modern client splits at-war into two opcodes carrying no state; legacy has one opcode with a
/// bool. The two handlers below therefore write the same legacy opcode with a hard-coded true and
/// false rather than reading anything from the packet.
/// </remarks>
public static class ReputationSystem
{
    [HandlesCmsg(Opcode.CMSG_SET_FACTION_AT_WAR)]
    public static void HandleSetFactionAtWar(in SetFactionAtWar faction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_FACTION_AT_WAR);
        packet.WriteUInt32(faction.FactionIndex);
        packet.WriteBool(true);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_FACTION_NOT_AT_WAR)]
    public static void HandleSetFactionNotAtWar(in SetFactionNotAtWar faction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_FACTION_AT_WAR);
        packet.WriteUInt32(faction.FactionIndex);
        packet.WriteBool(false);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_FACTION_INACTIVE)]
    public static void HandleSetFactionInactive(in SetFactionInactive faction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_FACTION_INACTIVE);
        packet.WriteUInt32(faction.FactionIndex);
        packet.WriteBool(faction.State);
        ctx.SendPacketToServer(packet);
    }

    /// <remarks>
    /// Declared as a second <c>HandleSetFactionInactive</c> overload before the conversion, which
    /// was a copy-paste misnomer — it has always handled CMSG_SET_WATCHED_FACTION. The body is
    /// unchanged; only the name is.
    /// </remarks>
    [HandlesCmsg(Opcode.CMSG_SET_WATCHED_FACTION)]
    public static void HandleSetWatchedFaction(in SetWatchedFaction faction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_WATCHED_FACTION);
        packet.WriteUInt32(faction.FactionIndex);
        ctx.SendPacketToServer(packet);
    }
}
