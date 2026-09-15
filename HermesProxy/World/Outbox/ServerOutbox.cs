using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Outbox;

/// <summary>Where the server outbox writes. Production writes to the session's legacy world connection.</summary>
public interface IServerWire
{
    void Write(WorldPacket packet);
}

/// <summary>
/// Packets from the proxy to the legacy server. Reached from handlers as <c>ctx.ToServer</c>.
/// </summary>
/// <remarks>
/// Legacy packets are fully built when they are handed over, so writing one reads no session
/// state and a timeout or delay may send it straight from the timer thread.
/// </remarks>
public sealed class ServerOutbox : PacketOutbox<WorldPacket>
{
    private readonly IServerWire _wire;

    public ServerOutbox(IServerWire wire, TimeProvider? time = null, OutboxOptions? options = null, Action? onOverflow = null)
        : base("server", time, options, onOverflow)
    {
        _wire = wire;
    }

    public override void Send(WorldPacket packet) => _wire.Write(packet);

    /// <summary>Sends the packet once the legacy server's next <paramref name="handledOpcode"/> has been handled.</summary>
    public void AfterHandled(Opcode handledOpcode, WorldPacket packet, in HoldOptions options = default)
        => When(OutboxEvent.OpcodeHandled(handledOpcode), packet, options);

    protected override void Drop(WorldPacket packet) => packet.Dispose();

    protected override bool PacketsAreTimerSafe => true;
}
