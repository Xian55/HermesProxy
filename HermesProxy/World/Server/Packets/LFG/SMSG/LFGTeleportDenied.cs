using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

/// <summary>
/// Why the server refused an LFG teleport — dead, falling, exhausted, no return location, and
/// so on. Legacy 3.3.5a sends the reason as a uint32; V3_4_3 packs it into 4 bits
/// (TC 3.4.3 LFGPackets.cpp LFGTeleportDenied::Write). The numbering is identical on both
/// sides (LfgTeleportError vs LfgTeleportResult: 0,1,2,3,4,6,8), so the value passes straight
/// through. Without this the refusal was dropped and the player got no explanation at all.
/// </summary>
public class LFGTeleportDenied : ServerPacket
{
    public byte Reason;

    public LFGTeleportDenied() : base(Opcode.SMSG_LFG_TELEPORT_DENIED) { }

    public override void Write()
    {
        _worldPacket.WriteBits(Reason, 4);
        _worldPacket.FlushBits();
    }
}
