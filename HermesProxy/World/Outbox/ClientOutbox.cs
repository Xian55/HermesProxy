using System;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Outbox;

/// <summary>Where the client outbox writes. Production writes to the session's modern sockets.</summary>
public interface IClientWire
{
    /// <summary>Writes the packet to the connection its <see cref="ServerPacket.GetConnection"/> names.</summary>
    void Write(ServerPacket packet);

    /// <summary>Writes the packet to <paramref name="connection"/> regardless of the packet's own type.</summary>
    void WriteOn(ConnectionType connection, ServerPacket packet);
}

/// <summary>
/// Packets from the proxy to the modern client. Reached from handlers as <c>ctx.ToClient</c>.
/// </summary>
/// <remarks>
/// Held packets are kept typed and serialized when they are released, because writing some of
/// them (UpdateObject) reads session state. For the same reason a held client packet is never
/// written from the timer thread: one released by a timeout waits for <see cref="PacketOutbox{TPacket}.Tick"/>.
/// </remarks>
public sealed class ClientOutbox : PacketOutbox<ServerPacket>
{
    private readonly IClientWire _wire;

    public ClientOutbox(IClientWire wire, TimeProvider? time = null, OutboxOptions? options = null, Action? onOverflow = null)
        : base("client", time, options, onOverflow)
    {
        _wire = wire;
    }

    /// <summary>Writes the packet now, then releases anything held until this opcode was sent.</summary>
    public override void Send(ServerPacket packet)
    {
        // Read before the write: a hold registered by another thread mid-write waits for the next
        // occurrence, which is the documented meaning of After.
        bool trackOpcode = HasPending;
        Opcode opcode = trackOpcode ? packet.GetUniversalOpcode() : default;
        _wire.Write(packet);
        if (trackOpcode)
            Notify(OutboxEvent.OpcodeSent(opcode));
    }

    /// <summary>
    /// Writes the packet to <paramref name="connection"/> now. For replies that must go back on the
    /// socket a request arrived on even when the packet's own type names the other one.
    /// </summary>
    public void SendOn(ConnectionType connection, ServerPacket packet)
    {
        bool trackOpcode = HasPending;
        Opcode opcode = trackOpcode ? packet.GetUniversalOpcode() : default;
        _wire.WriteOn(connection, packet);
        if (trackOpcode)
            Notify(OutboxEvent.OpcodeSent(opcode));
    }

    /// <summary>Sends the packet right after the next packet with <paramref name="sentOpcode"/> is sent.</summary>
    public void After(Opcode sentOpcode, ServerPacket packet, in HoldOptions options = default)
        => When(OutboxEvent.OpcodeSent(sentOpcode), packet, options);

    /// <summary>Runs <paramref name="release"/> right after the next packet with <paramref name="sentOpcode"/> is sent.</summary>
    public void After(Opcode sentOpcode, Action release, in HoldOptions options = default)
        => When(OutboxEvent.OpcodeSent(sentOpcode), release, options);

    /// <summary>
    /// Sends the packet once the current legacy <c>SMSG_UPDATE_OBJECT</c> batch has been sent in full.
    /// </summary>
    public void AfterBatch(ServerPacket packet, in HoldOptions options = default)
        => When(OutboxEvent.Signal(OutboxSignal.UpdateBatchEnd), packet, options);

    protected override void Drop(ServerPacket packet) => packet.Discard();

    protected override bool PacketsAreTimerSafe => false;
}
