namespace HermesProxy.World.Server.Packets;

public class DFLeavePkt : ClientPacket
{
    public DFLeavePkt(WorldPacket packet) : base(packet) { }
    public override void Read() { }
}
