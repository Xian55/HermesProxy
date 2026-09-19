using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using Framework.Constants;
using HermesProxy.Configuration.Options;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;

namespace HermesProxy.Tests.Support;

/// <summary>
/// Runs legacy SMSG handlers in-process against a real <see cref="GlobalSessionData"/>, with the
/// sockets replaced by sinks. Client packets are serialized exactly as
/// <c>WorldSocket.SendPacket</c> does before framing, so a handler's cost includes the bytes it
/// produces. Also compiled into HermesProxy.Benchmarks, which is why it depends on nothing from
/// xUnit.
/// </summary>
/// <remarks>
/// The legacy and modern builds are whatever <c>VersionBootstrap</c> was given before first use:
/// V3_3_5a to V1_14_2 under the test module initializer, V3_3_5a to V3_4_3 in the benchmarks.
/// </remarks>
internal sealed class LegacyHandlerHarness
{
    public GlobalSessionData Session { get; }
    public WorldClient Client { get; }
    public SinkClientWire ClientWire { get; }
    public SinkServerWire ServerWire { get; } = new();

    public LegacyHandlerHarness(bool recordClientPackets)
    {
        Session = new GlobalSessionData(
            new ClientOptions(),
            new LegacyServerOptions(),
            new ProxyNetworkOptions(),
            new DiagnosticsOptions { PacketsLog = false },
            new ThrottlingOptions());

        ClientWire = new SinkClientWire(recordClientPackets);
        var toClient = new ClientOutbox(ClientWire);
        toClient.Attach(ConnectionType.Realm);
        toClient.Attach(ConnectionType.Instance);
        toClient.RunDeadlinesOn(Session.Executor);
        SetAutoPropertyBackingField(Session, nameof(GlobalSessionData.ToClient), toClient);

        var toServer = new ServerOutbox(ServerWire);
        toServer.RunDeadlinesOn(Session.Executor);
        toServer.SetGate(OutboxGate.SwingAnswered, open: true);
        SetAutoPropertyBackingField(Session, nameof(GlobalSessionData.ToServer), toServer);

        Client = new WorldClient();
        typeof(WorldClient)
            .GetField("_globalSession", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(Client, Session);
        Session.WorldClient = Client;

        // Alterac Valley, where the allocation traces these benchmarks follow were taken.
        Session.GameState.CurrentMapId = 30;
        Session.GameState.IsInWorld = true;
    }

    /// <summary>
    /// Leaves an object in the state its create would have: its type recorded, a field cache to
    /// merge Values deltas into, and known to the client.
    /// </summary>
    public WowGuid128 AddKnownObject(WowGuid64 legacyGuid, ObjectType type)
    {
        var guid = legacyGuid.To128(Session.GameState);
        Session.GameState.StoreOriginalObjectType(guid, type);
        lock (Session.GameState.ObjectCacheLock)
            Session.GameState.ObjectCacheLegacy[guid] = [];
        Session.GameState.ClientKnownGuids.Add(guid);
        return guid;
    }

    public WowGuid128 SetActivePlayer(WowGuid64 legacyGuid)
    {
        var guid = AddKnownObject(legacyGuid, ObjectType.Player);
        Session.GameState.CurrentPlayerGuid = guid;
        return guid;
    }

    /// <summary>
    /// Hands one received packet to <paramref name="handler"/> the way the receive loop and the
    /// session executor do: a pooled read-mode packet, disposed after the handler, followed by the
    /// per-packet outbox tick.
    /// </summary>
    public void Deliver(Opcode opcode, byte[] wire, Action<WorldPacket> handler)
    {
        using (var packet = LegacyPacketBuilder.Receive(wire))
            handler(packet);
        Session.OnLegacyPacketHandled(opcode);
    }

    private static void SetAutoPropertyBackingField(object target, string property, object value)
        => target.GetType()
            .GetField($"<{property}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(target, value);
}

internal readonly record struct SentClientPacket(ServerPacket Packet, Opcode Opcode, byte[] Bytes);

/// <summary>Serializes each client packet as the socket would, then drops the bytes.</summary>
internal sealed class SinkClientWire(bool record) : IClientWire
{
    public List<SentClientPacket> Sent { get; } = [];
    public int Count { get; private set; }

    public bool TryWrite(ConnectionType connection, ServerPacket packet)
    {
        packet.WritePacketData();
        if (record)
            Sent.Add(new SentClientPacket(packet, packet.GetUniversalOpcode(), packet.GetDataSpan().ToArray()));
        packet.ReleaseData();
        Count++;
        return true;
    }
}

internal sealed class SinkServerWire : IServerWire
{
    public int Count { get; private set; }

    public void Write(WorldPacket packet)
    {
        Count++;
        packet.Dispose();
    }
}

/// <summary>Builds legacy server packets as they arrive off the wire, opcode first.</summary>
internal static class LegacyPacketBuilder
{
    public static byte[] Build(Opcode opcode, Action<WorldPacket> writeBody)
    {
        using var packet = new WorldPacket();
        packet.WriteUInt16((ushort)LegacyVersion.GetCurrentOpcode(opcode));
        writeBody(packet);
        return packet.GetDataSpan().ToArray();
    }

    /// <summary>A received packet, in a pooled rental as <c>WorldClient.ReceiveLoop</c> leaves it.</summary>
    public static WorldPacket Receive(byte[] wire)
    {
        byte[] rented = ArrayPool<byte>.Shared.Rent(wire.Length);
        wire.CopyTo(rented, 0);
        var packet = new WorldPacket(rented, wire.Length, isPooled: true);
        packet.SetReceiveTime(Environment.TickCount);
        return packet;
    }

    /// <summary>
    /// One Values block of a legacy SMSG_UPDATE_OBJECT: mask words, then the set fields in index
    /// order. The mask spans the object's whole field range, as TrinityCore, AzerothCore and
    /// cMaNGOS all size it (<c>UpdateMask::SetCount(m_valuesCount)</c>), however few fields changed.
    /// </summary>
    public static void WriteValuesBlock(WorldPacket packet, WowGuid64 guid, int fieldEnd, params ReadOnlySpan<(int Field, uint Value)> fields)
    {
        packet.WriteUInt8((byte)UpdateTypeLegacy.Values);
        packet.WritePackedGuid(guid);

        int maxField = 0;
        foreach (var (field, _) in fields)
            maxField = Math.Max(maxField, field);

        int words = (fieldEnd + 31) / 32;
        Span<uint> mask = stackalloc uint[words];
        foreach (var (field, _) in fields)
            mask[field / 32] |= 1u << (field % 32);

        packet.WriteUInt8((byte)words);
        foreach (uint word in mask)
            packet.WriteUInt32(word);

        for (int index = 0; index <= maxField; index++)
        {
            foreach (var (field, value) in fields)
            {
                if (field == index)
                {
                    packet.WriteUInt32(value);
                    break;
                }
            }
        }
    }
}
