namespace HermesProxy.World.Server.Packets;

/// <summary>
/// The minimap LFG eye's "Teleport to Dungeon" / "Teleport out of Dungeon" entry. Distinct from
/// CMSG_DF_LEAVE, which drops the queue or the group — this only moves the player in or out of
/// the instance while the LFG association stays intact, which is how a native 3.3.5a client
/// behaves too (the eye stays up after teleporting out, offering the trip back).
/// TC 3.4.3: WorldPackets::LFG::DFTeleport, a single TeleportOut bit.
/// </summary>
public class DFTeleportPkt : ClientPacket
{
    public bool TeleportOut;

    public DFTeleportPkt(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TeleportOut = _worldPacket.HasBit();
    }
}
