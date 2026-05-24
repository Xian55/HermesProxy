namespace HermesProxy.World.Server.Packets;

public class DFGetSystemInfoPkt : ClientPacket
{
    public bool Player;

    public DFGetSystemInfoPkt(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Player = _worldPacket.HasBit();
        // optional PartyIndex byte follows — unused
    }
}
