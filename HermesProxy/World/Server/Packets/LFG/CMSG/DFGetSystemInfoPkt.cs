namespace HermesProxy.World.Server.Packets;

/// <remarks>An optional PartyIndex byte follows the bit on the wire and is not read.</remarks>
public readonly record struct DFGetSystemInfoPkt(bool Player);
