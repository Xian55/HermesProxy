namespace HermesProxy.World.Server.Packets;

public class DFJoinPkt : ClientPacket
{
    public bool QueueAsGroup;
    public byte Roles;
    public uint[] Slots = System.Array.Empty<uint>();

    public DFJoinPkt(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QueueAsGroup = _worldPacket.HasBit();
        bool hasPartyIndex = _worldPacket.HasBit();
        _worldPacket.HasBit(); // Mercenary
        Roles = _worldPacket.ReadUInt8();
        uint slotCount = _worldPacket.ReadUInt32();
        if (hasPartyIndex)
            _worldPacket.ReadUInt8();
        Slots = new uint[slotCount];
        for (int i = 0; i < slotCount; i++)
            Slots[i] = _worldPacket.ReadUInt32();
    }
}
