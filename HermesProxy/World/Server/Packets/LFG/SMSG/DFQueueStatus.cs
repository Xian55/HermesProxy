using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DFQueueStatus : ServerPacket
{
    public RideTicket Ticket = new();
    public uint Slot;
    public uint AvgWaitTimeMe;
    public uint AvgWaitTime;
    public uint[] AvgWaitTimeByRole = new uint[3]; // Tank, Healer, DPS
    public byte[] LastNeeded = new byte[3];
    public uint QueuedTime;

    public DFQueueStatus() : base(Opcode.SMSG_LFG_QUEUE_STATUS) { }

    public override void Write()
    {
        Ticket.Write(_worldPacket);
        _worldPacket.WriteBit(false); // RideTicket trailing Unknown925
        _worldPacket.FlushBits();
        _worldPacket.WriteUInt32(Slot);
        _worldPacket.WriteUInt32(AvgWaitTimeMe);
        _worldPacket.WriteUInt32(AvgWaitTime);
        for (int i = 0; i < 3; i++)
        {
            _worldPacket.WriteUInt32(AvgWaitTimeByRole[i]);
            _worldPacket.WriteUInt8(LastNeeded[i]);
        }
        _worldPacket.WriteUInt32(QueuedTime);
    }
}
