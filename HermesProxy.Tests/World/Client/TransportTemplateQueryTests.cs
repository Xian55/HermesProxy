using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The proxy asks the legacy server for a transport's or destructible building's template itself,
/// because the modern client never does. Each session, and each login of it, needs the answer:
/// the path id reaches the client only in the relayed response, and the model id is kept in that
/// login's GameState.
/// </summary>
public sealed class TransportTemplateQueryTests
{
    private const uint Transport = 164871;
    private const uint OtherTransport = 176495;

    // u16 size, u32 opcode, u32 entry, u64 guid
    private const int QueryFrameSize = 18;

    /// <summary>A session whose WorldClient writes to a real socket; the server's end reads what it sent.</summary>
    private sealed class Session : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly Socket _proxySide = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly Socket _serverSide;

        public LegacyHandlerHarness Harness { get; } = new(recordClientPackets: false);

        public Session()
        {
            _listener.Start();
            _proxySide.Connect((IPEndPoint)_listener.LocalEndpoint);
            _serverSide = _listener.AcceptSocket();
            typeof(WorldClient)
                .GetField("_clientSocket", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(Harness.Client, _proxySide);
        }

        /// <summary>The entries of every gameobject query the server received, in order.</summary>
        public List<uint> QueriedEntries()
        {
            _proxySide.Shutdown(SocketShutdown.Send);

            var received = new List<byte>();
            byte[] chunk = new byte[256];
            int read;
            while ((read = _serverSide.Receive(chunk)) > 0)
                received.AddRange(chunk.AsSpan(0, read));

            Assert.Equal(0, received.Count % QueryFrameSize);
            var entries = new List<uint>();
            for (int offset = 0; offset < received.Count; offset += QueryFrameSize)
            {
                byte[] frame = received.GetRange(offset, QueryFrameSize).ToArray();
                Assert.Equal(LegacyVersion.GetCurrentOpcode(Opcode.CMSG_QUERY_GAME_OBJECT), BinaryPrimitives.ReadUInt32LittleEndian(frame.AsSpan(2)));
                entries.Add(BinaryPrimitives.ReadUInt32LittleEndian(frame.AsSpan(6)));
            }
            return entries;
        }

        public void Dispose()
        {
            _serverSide.Dispose();
            _proxySide.Dispose();
            _listener.Dispose();
        }
    }

    [Fact]
    public void RequestTransportTemplate_AsksOncePerEntry()
    {
        using var session = new Session();

        session.Harness.Client.RequestTransportTemplate(Transport);
        session.Harness.Client.RequestTransportTemplate(OtherTransport);
        session.Harness.Client.RequestTransportTemplate(Transport);
        session.Harness.Client.RequestTransportTemplate(0);

        Assert.Equal([Transport, OtherTransport], session.QueriedEntries());
    }

    [Fact]
    public void RequestTransportTemplate_AnotherSessionOnTheProxy_AsksAgain()
    {
        using var first = new Session();
        using var second = new Session();

        first.Harness.Client.RequestTransportTemplate(Transport);
        second.Harness.Client.RequestTransportTemplate(Transport);

        Assert.Equal([Transport], first.QueriedEntries());
        Assert.Equal([Transport], second.QueriedEntries());
    }

    [Fact]
    public void RequestTransportTemplate_AfterARelog_AsksAgain()
    {
        using var session = new Session();

        session.Harness.Client.RequestTransportTemplate(Transport);
        session.Harness.Session.ReplaceGameState();
        session.Harness.Client.RequestTransportTemplate(Transport);

        Assert.Equal([Transport, Transport], session.QueriedEntries());
    }
}
