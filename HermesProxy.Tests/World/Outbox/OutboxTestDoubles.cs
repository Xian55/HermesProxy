using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Framework.Constants;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;

namespace HermesProxy.Tests.World.Outbox;

/// <summary>A client packet with an id, so tests can assert exactly which packets went out.</summary>
internal sealed class TestServerPacket(Opcode opcode, int id, ConnectionType connection = ConnectionType.Realm)
    : ServerPacket(opcode, connection)
{
    public int Id { get; } = id;

    /// <summary>Thread that serialized the packet, or 0 if it never was.</summary>
    public int SerializedOnThread { get; private set; }

    public override void Write() => SerializedOnThread = Environment.CurrentManagedThreadId;

    public override string ToString() => $"{GetUniversalOpcode()}#{Id}";
}

internal readonly record struct ClientWrite(TestServerPacket Packet, ConnectionType On, int ThreadId);

/// <summary>Records client writes in order, with the thread that made each one.</summary>
internal sealed class RecordingClientWire : IClientWire
{
    private readonly Lock _lock = new();
    private readonly List<ClientWrite> _writes = [];

    public volatile bool RealmOpen = true;
    public volatile bool InstanceOpen = true;

    /// <summary>Runs after each write is recorded, on the writing thread.</summary>
    public Action<TestServerPacket>? OnWrite { get; set; }

    public bool TryWrite(ConnectionType connection, ServerPacket packet)
    {
        if (!(connection == ConnectionType.Realm ? RealmOpen : InstanceOpen))
            return false;

        var test = (TestServerPacket)packet;
        lock (_lock)
            _writes.Add(new ClientWrite(test, connection, Environment.CurrentManagedThreadId));

        OnWrite?.Invoke(test);
        // The fake never frames the bytes, so return the rental the constructor took.
        packet.Discard();
        return true;
    }

    public ClientWrite[] Writes
    {
        get
        {
            lock (_lock)
                return [.. _writes];
        }
    }

    public int[] Ids => Writes.Select(w => w.Packet.Id).ToArray();
}

internal readonly record struct ServerWrite(WorldPacket Packet, int ThreadId);

/// <summary>Records server writes in order. Leaves packets undisposed so tests can inspect them.</summary>
internal sealed class RecordingServerWire : IServerWire
{
    private readonly Lock _lock = new();
    private readonly List<ServerWrite> _writes = [];

    public ServerWrite[] Writes
    {
        get
        {
            lock (_lock)
                return [.. _writes];
        }
    }

    /// <summary>Runs after each write is recorded, on the writing thread.</summary>
    public Action<WorldPacket>? OnWrite { get; set; }

    public void Write(WorldPacket packet)
    {
        lock (_lock)
            _writes.Add(new ServerWrite(packet, Environment.CurrentManagedThreadId));
        OnWrite?.Invoke(packet);
    }
}

internal static class OutboxTestExtensions
{
    private static readonly FieldInfo DisposedField =
        typeof(ByteBuffer).GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("ByteBuffer._disposed not found; update the test helper.");

    public static bool IsDisposed(this ByteBuffer buffer) => (bool)DisposedField.GetValue(buffer)!;

    public static WorldPacket Legacy(uint opcode) => new(opcode);

    /// <summary>A client outbox with both sockets attached, as in the world.</summary>
    public static ClientOutbox InWorld(RecordingClientWire wire, TimeProvider? time = null)
    {
        var outbox = new ClientOutbox(wire, time);
        outbox.Attach(ConnectionType.Realm);
        outbox.Attach(ConnectionType.Instance);
        return outbox;
    }
}
