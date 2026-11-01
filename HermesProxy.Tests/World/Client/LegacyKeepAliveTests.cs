using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using HermesProxy.Tests.Support;
using HermesProxy.World.Client;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Client;

public class LegacyKeepAliveTests
{
    // u16 size, u32 opcode, then the serial and the latency, in the order the build has them.
    private const int PingFrameSize = 14;
    private const double StepSeconds = 0.1;

    /// <summary>
    /// A WorldClient on a real socket and a fake clock: the keep-alive timer is the production one,
    /// and what counts is what reaches the server's end.
    /// </summary>
    private sealed class Connection : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly Socket _proxySide = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly Socket _serverSide;
        private readonly FakeTimeProvider _time = new();
        private readonly WorldClient _client;
        private double _now;
        private uint _serial;

        /// <summary>Every ping the server got, when, and whether it was the proxy's own.</summary>
        public List<(double At, bool KeepAlive)> Pings { get; } = [];

        public Connection()
        {
            _listener.Start();
            _proxySide.Connect((IPEndPoint)_listener.LocalEndpoint);
            _serverSide = _listener.AcceptSocket();

            _client = new LegacyHandlerHarness(recordClientPackets: false).Client;
            _client.Clock = _time;
            typeof(WorldClient)
                .GetField("_clientSocket", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_client, _proxySide);
            _client.StartKeepAliveTimer();
        }

        /// <summary>Time passes, with the client pinging every 30 s from <paramref name="firstClientPing"/> on.</summary>
        public void Run(double until, double? firstClientPing = null)
        {
            double? nextClientPing = firstClientPing;
            while (_now < until)
            {
                _now += StepSeconds;
                _time.Advance(TimeSpan.FromSeconds(StepSeconds));

                if (_now >= nextClientPing)
                {
                    _client.SendPing(++_serial, 0);
                    nextClientPing += 30;
                }

                DrainTheServersEnd();
            }
        }

        private void DrainTheServersEnd()
        {
            byte[] frame = new byte[PingFrameSize];
            while (_serverSide.Available >= PingFrameSize)
            {
                _serverSide.Receive(frame);
                Pings.Add((_now, ((BitConverter.ToUInt32(frame, 6) | BitConverter.ToUInt32(frame, 10)) & 0x80000000) != 0));
            }
        }

        public void Dispose()
        {
            _client.StopKeepAliveTimer();
            _serverSide.Dispose();
            _proxySide.Dispose();
            _listener.Dispose();
        }
    }

    /// <summary>
    /// TrinityCore's WorldSocket::HandlePing, which AzerothCore, cMaNGOS and VMaNGOS share: a gap
    /// under 27 s counts, any other resets, and the third in a row closes the connection of an
    /// account that is not a GM (MaxOverspeedPings = 2, the default). The ping it happened at, or
    /// null when the connection lives.
    /// </summary>
    private static double? KickedAt(IEnumerable<double> pings)
    {
        int overspeed = 0;
        double? last = null;
        foreach (double ping in pings)
        {
            if (last != null)
            {
                overspeed = ping - last < 27 ? overspeed + 1 : 0;
                if (overspeed > 2)
                    return ping;
            }
            last = ping;
        }
        return null;
    }

    [Fact]
    public void KickedAt_TheReportersPings_KicksOnTheFourth()
    {
        Assert.Equal(77.9, KickedAt([30.1, 47.9, 60.1, 77.9]));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(12.3)]
    [InlineData(17.8)]
    [InlineData(25)]
    [InlineData(47.9)]
    public void Run_ClientPingsOutOfStep_ServerNeverKicks(double firstClientPing)
    {
        using var connection = new Connection();

        connection.Run(until: 300, firstClientPing);

        Assert.Null(KickedAt(connection.Pings.Select(p => p.At)));
    }

    [Theory]
    [InlineData(12.3)]
    [InlineData(30)]        // on the timer's own beat, as a client that started with the connection is
    public void Run_ClientPinging_NoKeepAliveGoesOut(double firstClientPing)
    {
        using var connection = new Connection();

        connection.Run(until: 300, firstClientPing);

        Assert.NotEmpty(connection.Pings);
        Assert.DoesNotContain(connection.Pings, p => p.KeepAlive);
    }

    [Fact]
    public void Run_ClientSilent_KeepAlivesHoldTheConnectionOpen()
    {
        using var connection = new Connection();

        connection.Run(until: 300);

        Assert.True(connection.Pings.Count >= 8, $"{connection.Pings.Count} keep-alives in 5 minutes");
        Assert.All(connection.Pings, p => Assert.True(p.KeepAlive));

        double[] times = connection.Pings.Select(p => p.At).ToArray();
        Assert.True(times[0] <= 35.5, $"first keep-alive at {times[0]}");
        foreach (var (earlier, later) in times.Zip(times.Skip(1)))
            Assert.InRange(later - earlier, 29.5, 35.5);
    }

    [Fact]
    public void Run_ClientStopsAndStartsAgain_KeepAliveFillsTheGap_ServerNeverKicks()
    {
        using var connection = new Connection();

        connection.Run(until: 95, firstClientPing: 5);      // in the world
        connection.Run(until: 200);                         // logged out to the character screen
        connection.Run(until: 400, firstClientPing: 203);   // and back in

        Assert.Contains(connection.Pings, p => p.KeepAlive);
        Assert.Null(KickedAt(connection.Pings.Select(p => p.At)));
    }
}
