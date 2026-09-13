namespace HermesProxy.World.Server.Packets;

/// <remarks>An optional PartyIndex byte follows on the wire and is not read.</remarks>
public readonly record struct DFSetRolesPkt(byte Roles);
