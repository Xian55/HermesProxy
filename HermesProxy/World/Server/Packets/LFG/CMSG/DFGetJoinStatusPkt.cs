namespace HermesProxy.World.Server.Packets;

public class DFGetJoinStatusPkt : ClientPacket
{
    public DFGetJoinStatusPkt(WorldPacket packet) : base(packet) { }
    public override void Read() { }
}
