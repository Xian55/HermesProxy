namespace HermesProxy.World.Server.Packets;

public class DFSetRolesPkt : ClientPacket
{
    public byte Roles;

    public DFSetRolesPkt(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Roles = _worldPacket.ReadUInt8();
        // optional PartyIndex byte — unused
    }
}
