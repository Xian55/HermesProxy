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

    public override void Write() { }

    public override string ToString() => $"{GetUniversalOpcode()}#{Id}";
}

internal readonly record struct ClientWrite(TestServerPacket Packet, ConnectionType? On, int ThreadId);

/// <summary>Records client writes in order, with the thread that made each one.</summary>
internal sealed class RecordingClientWire : IClientWire
{
    private readonly Lock _lock = new();
    private readonly List<ClientWrite> _writes = [];

    /// <summary>Runs after each write is recorded, on the writing thread.</summary>
    public Action<TestServerPacket>? OnWrite { get; set; }

    public void Write(ServerPacket packet) => Record((TestServerPacket)packet, null);

    public void WriteOn(ConnectionType connection, ServerPacket packet) => Record((TestServerPacket)packet, connection);

    public ClientWrite[] Writes
    {
        get
        {
            lock (_lock)
                return [.. _writes];
        }
    }

    public int[] Ids => Writes.Select(w => w.Packet.Id).ToArray();

    private void Record(TestServerPacket packet, ConnectionType? on)
    {
        lock (_lock)
            _writes.Add(new ClientWrite(packet, on, Environment.CurrentManagedThreadId));

        OnWrite?.Invoke(packet);
        // The fake never serializes, so return the rental the constructor took.
        packet.Discard();
    }
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

    public void Write(WorldPacket packet)
    {
        lock (_lock)
            _writes.Add(new ServerWrite(packet, Environment.CurrentManagedThreadId));
    }
}

internal static class OutboxTestExtensions
{
    private static readonly FieldInfo DisposedField =
        typeof(ByteBuffer).GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("ByteBuffer._disposed not found; update the test helper.");

    public static bool IsDisposed(this ByteBuffer buffer) => (bool)DisposedField.GetValue(buffer)!;

    public static WorldPacket Legacy(uint opcode) => new(opcode);
}
