using System;
using System.Collections.Generic;
using System.Threading;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Outbox;

/// <summary>Where the client outbox writes. Production writes to the session's modern sockets.</summary>
public interface IClientWire
{
    /// <summary>
    /// Writes the packet to <paramref name="connection"/> if that socket is up. False when it is not,
    /// in which case the packet was not touched and the outbox keeps it.
    /// </summary>
    bool TryWrite(ConnectionType connection, ServerPacket packet);
}

/// <summary>
/// Packets from the proxy to the modern client. Reached from handlers as <c>ctx.ToClient</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Parks.</b> A packet whose socket isn't attached yet waits here, not in a queue on the
/// session:
/// <list type="bullet">
/// <item>the realm socket before <c>ENTER_ENCRYPTED_MODE_ACK</c>, or between change-realm sockets;</item>
/// <item>the instance socket before the client is asked to connect. Only instance packets wait, and
/// realm packets keep flowing;</item>
/// <item>the instance socket after the client was told to connect but before it did. <b>Everything</b>
/// waits then, in one queue, so nothing overtakes a packet that was held back. The old path did this
/// with a thread sleeping for up to 30 s.</item>
/// </list>
/// A parked packet is serialized on the thread that parks it: writing an UpdateObject reads session
/// state, and the thread that eventually drains the park is not that thread. One drainer at a time
/// empties the parks; packets sent while it runs queue behind it.
/// </para>
/// <para>
/// <b>Holds.</b> A packet held by an event or gate is kept typed and serialized when released. For
/// the same reason a held client packet is never written from the timer thread: one released by a
/// deadline waits for <see cref="PacketOutbox{TPacket}.Tick"/>.
/// </para>
/// </remarks>
public sealed class ClientOutbox : PacketOutbox<ServerPacket>
{
    private enum InstanceState : byte
    {
        /// <summary>The client has not been asked to connect. Instance packets wait on their own.</summary>
        NotRequested,
        /// <summary>Asked, not connected yet. Every packet waits, in order.</summary>
        Connecting,
        Attached,
    }

    private readonly record struct Parked(ServerPacket Packet, ConnectionType Connection, long ParkedAt);

    private static readonly Microsoft.Extensions.Logging.ILogger _log = Log.CreateMelLogger(Log.CategoryNetwork);

    /// <summary>A parked packet older than this is dropped: the connection it waits for isn't coming.</summary>
    public static readonly TimeSpan ParkTimeout = TimeSpan.FromSeconds(30);

    /// <summary>Most packets the parks may hold before new ones are dropped.</summary>
    public const int MaxParked = 8192;

    private const int DrainBatch = 64;
    // A socket thread draining a long backlog hands the rest to the pool, so its own next read
    // is not held up by the whole login burst.
    private const int DrainBatchesBeforeHandOff = 8;

    private readonly IClientWire _wire;
    private readonly TimeProvider _time;
    private readonly Lock _routeLock = new();
    private readonly LinkedList<Parked> _parked = new();
    private readonly Queue<Parked> _uninstanced = new();
    private bool _realmAttached;
    private InstanceState _instance;
    private bool _draining;
    // True while both sockets are attached and nothing is parked or draining, so a send can go
    // straight to the wire without taking the route lock.
    private volatile bool _clear;

    public ClientOutbox(IClientWire wire, TimeProvider? time = null, OutboxOptions? options = null, Action? onOverflow = null)
        : base("client", time, options, onOverflow)
    {
        _wire = wire;
        _time = time ?? TimeProvider.System;
    }

    /// <summary>Packets waiting for a socket.</summary>
    public int ParkedCount
    {
        get
        {
            lock (_routeLock)
                return _parked.Count + _uninstanced.Count;
        }
    }

    /// <summary>
    /// Writes the packet to the connection its own type names, or parks it until that socket is
    /// attached. Then releases anything held until this opcode was sent.
    /// </summary>
    public override void Send(ServerPacket packet) => Route(packet.GetConnection(), packet);

    /// <summary>
    /// Writes the packet to <paramref name="connection"/> regardless of the packet's own type, or
    /// parks it until that socket is attached.
    /// </summary>
    public void SendOn(ConnectionType connection, ServerPacket packet) => Route(connection, packet);

    /// <summary>Sends the packet right after the next packet with <paramref name="sentOpcode"/> is sent.</summary>
    public void After(Opcode sentOpcode, ServerPacket packet, in HoldOptions options = default)
        => When(OutboxEvent.OpcodeSent(sentOpcode), packet, options);

    /// <summary>Runs <paramref name="release"/> right after the next packet with <paramref name="sentOpcode"/> is sent.</summary>
    public void After(Opcode sentOpcode, Action release, in HoldOptions options = default)
        => When(OutboxEvent.OpcodeSent(sentOpcode), release, options);

    /// <summary>
    /// Sends the packet once the legacy <c>SMSG_UPDATE_OBJECT</c> batch now being handled has been
    /// sent in full.
    /// </summary>
    public void AfterBatch(ServerPacket packet, in HoldOptions options = default)
        => When(OutboxEvent.Signal(OutboxSignal.UpdateBatchEnd), packet, options);

    // ---- connection lifecycle -----------------------------------------------------------------

    /// <summary>
    /// The socket for <paramref name="connection"/> is up and encrypted. Parked packets start going
    /// out, on this thread, before this returns unless the backlog is long.
    /// </summary>
    public void Attach(ConnectionType connection)
    {
        bool drain = false;
        lock (_routeLock)
        {
            if (connection == ConnectionType.Realm)
                _realmAttached = true;
            else
                _instance = InstanceState.Attached;

            if (!_draining && HasWritableParked())
            {
                _draining = true;
                drain = true;
            }
            UpdateClear();
        }

        if (drain)
            Drain();
    }

    /// <summary>The socket for <paramref name="connection"/> is gone. Its packets park from now on.</summary>
    public void Detach(ConnectionType connection)
    {
        lock (_routeLock)
        {
            if (connection == ConnectionType.Realm)
                _realmAttached = false;
            else
                _instance = InstanceState.NotRequested;
            UpdateClear();
        }
    }

    /// <summary>
    /// The client has been told to open the instance connection. Until it attaches, every packet
    /// parks in one queue, so a realm packet can't overtake an instance packet sent before it.
    /// </summary>
    public void BeginInstanceConnect()
    {
        lock (_routeLock)
        {
            if (_instance != InstanceState.Attached)
                _instance = InstanceState.Connecting;
            UpdateClear();
        }
    }

    /// <summary>Drops every parked packet. Called when the state they were built from is gone.</summary>
    public void DiscardParked()
    {
        List<Parked>? dropped = null;
        lock (_routeLock)
        {
            if (_parked.Count + _uninstanced.Count == 0)
                return;
            dropped = [.. _uninstanced, .. _parked];
            _parked.Clear();
            _uninstanced.Clear();
            UpdateClear();
        }

        foreach (var parked in dropped)
            parked.Packet.Discard();
        OutboxLogMessages.ParkDiscarded(_log, dropped.Count);
    }

    /// <summary>Drops parked packets that have waited longer than <see cref="ParkTimeout"/>.</summary>
    public void TickParks()
    {
        if (_clear)
            return;

        List<Parked>? expired = null;
        long now = _time.GetTimestamp();
        lock (_routeLock)
        {
            // Only the head-of-line park expires: an instance packet parked before the client was
            // asked to connect legitimately waits for as long as the player stays at character select.
            while (_parked.First is { } first && _time.GetElapsedTime(first.Value.ParkedAt, now) >= ParkTimeout)
            {
                (expired ??= []).Add(first.Value);
                _parked.RemoveFirst();
            }
            if (expired != null)
                UpdateClear();
        }

        if (expired == null)
            return;

        foreach (var parked in expired)
            parked.Packet.Discard();
        OutboxLogMessages.ParkExpired(_log, expired.Count, ParkTimeout.TotalSeconds, expired[0].Packet.GetUniversalOpcode());
    }

    // ---- routing ------------------------------------------------------------------------------

    private void Route(ConnectionType connection, ServerPacket packet)
    {
        // Read before the write: a hold registered by another thread mid-write waits for the next
        // occurrence, which is the documented meaning of After.
        bool trackOpcode = HasPending;
        Opcode opcode = trackOpcode ? packet.GetUniversalOpcode() : default;

        if (_clear && _wire.TryWrite(connection, packet))
        {
            if (trackOpcode)
                Notify(OutboxEvent.OpcodeSent(opcode));
            return;
        }

        RouteSlow(connection, packet, trackOpcode, opcode);
    }

    private void RouteSlow(ConnectionType connection, ServerPacket packet, bool trackOpcode, Opcode opcode)
    {
        // Serialize here, on the sending thread, before the packet can be parked: the thread that
        // drains a park must not be the one writing UpdateObject state. A direct write reuses the
        // bytes, so doing it first changes nothing when the packet goes straight out.
        packet.WritePacketData();

        bool writeNow = false;
        bool refused = false;
        lock (_routeLock)
        {
            if (!_draining && _parked.Count == 0 && CanWrite(connection))
                writeNow = true;
            else if (_parked.Count + _uninstanced.Count >= MaxParked)
                refused = true;
            else
                Park(connection, packet);
        }

        if (refused)
        {
            OutboxLogMessages.ParkOverflow(_log, MaxParked, packet.GetUniversalOpcode());
            packet.Discard();
            return;
        }

        if (!writeNow)
            return;

        if (_wire.TryWrite(connection, packet))
        {
            if (trackOpcode)
                Notify(OutboxEvent.OpcodeSent(opcode));
            return;
        }

        // Attached a moment ago, closed now. Park behind anything that queued meanwhile.
        lock (_routeLock)
        {
            MarkDetached(connection);
            Park(connection, packet);
        }
    }

    private void Park(ConnectionType connection, ServerPacket packet)
    {
        var parked = new Parked(packet, connection, _time.GetTimestamp());
        if (connection == ConnectionType.Instance && _instance == InstanceState.NotRequested)
            _uninstanced.Enqueue(parked);
        else
            _parked.AddLast(parked);
        UpdateClear();
        if (_log.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            OutboxLogMessages.Parked(_log, packet.GetUniversalOpcode(), connection, _parked.Count + _uninstanced.Count);
    }

    private void Drain()
    {
        var batch = new List<Parked>(DrainBatch);
        for (int batches = 0; ; batches++)
        {
            if (batches == DrainBatchesBeforeHandOff)
            {
                ThreadPool.UnsafeQueueUserWorkItem(static outbox => outbox.Drain(), this, preferLocal: false);
                return;
            }

            batch.Clear();
            lock (_routeLock)
            {
                while (batch.Count < DrainBatch)
                {
                    if (_instance == InstanceState.Attached && _uninstanced.TryDequeue(out var early))
                        batch.Add(early);
                    else if (_parked.First is { } head && CanWrite(head.Value.Connection))
                    {
                        batch.Add(head.Value);
                        _parked.RemoveFirst();
                    }
                    else
                        break;
                }

                if (batch.Count == 0)
                {
                    _draining = false;
                    UpdateClear();
                    return;
                }
            }

            for (int i = 0; i < batch.Count; i++)
            {
                var parked = batch[i];
                bool trackOpcode = HasPending;
                if (_wire.TryWrite(parked.Connection, parked.Packet))
                {
                    if (trackOpcode)
                        Notify(OutboxEvent.OpcodeSent(parked.Packet.GetUniversalOpcode()));
                    continue;
                }

                // The socket went away mid-drain. Put the unwritten rest back at the head, in order.
                lock (_routeLock)
                {
                    MarkDetached(parked.Connection);
                    for (int j = batch.Count - 1; j >= i; j--)
                        _parked.AddFirst(batch[j]);
                    _draining = false;
                    UpdateClear();
                }
                return;
            }
        }
    }

    // ---- state (under _routeLock) -------------------------------------------------------------

    private bool CanWrite(ConnectionType connection)
        => connection == ConnectionType.Realm ? _realmAttached : _instance == InstanceState.Attached;

    private bool HasWritableParked()
        => (_instance == InstanceState.Attached && _uninstanced.Count > 0)
           || (_parked.First is { } head && CanWrite(head.Value.Connection));

    private void MarkDetached(ConnectionType connection)
    {
        if (connection == ConnectionType.Realm)
            _realmAttached = false;
        else if (_instance == InstanceState.Attached)
            _instance = InstanceState.NotRequested;
    }

    private void UpdateClear()
        => _clear = _realmAttached && _instance == InstanceState.Attached && !_draining
                    && _parked.Count == 0 && _uninstanced.Count == 0;

    protected override void Drop(ServerPacket packet) => packet.Discard();

    protected override bool PacketsAreTimerSafe => false;
}
