using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LFGDisabled : ServerPacket
{
    public LFGDisabled() : base(Opcode.SMSG_LFG_DISABLED) { }
    public override void Write() { }
}
