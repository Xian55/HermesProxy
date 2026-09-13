namespace HermesProxy.World.Server.Packets;

public readonly record struct DFJoinPkt(bool QueueAsGroup, byte Roles, uint[] Slots);
