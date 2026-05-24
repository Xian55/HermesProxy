using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LFGOfferContinue : ServerPacket
{
    public uint Slot;

    public LFGOfferContinue() : base(Opcode.SMSG_LFG_OFFER_CONTINUE) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(Slot);
    }
}
