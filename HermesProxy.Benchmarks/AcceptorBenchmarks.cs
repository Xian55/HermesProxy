using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Framework.Networking;

namespace HermesProxy.Benchmarks;

/// <summary>
/// One connection from connect to handler, over loopback, on the real listener. Sockets dominate
/// both arms, so <see cref="Floor"/> is the bare accept loop and the difference is what
/// <see cref="AsyncAcceptor"/> adds per client.
/// </summary>
[MemoryDiagnoser]
public class AcceptorBenchmarks
{
    private readonly SemaphoreSlim _accepted = new(0, 1);
    private readonly CancellationTokenSource _stopping = new();

    // Reset on close: a benchmark that left every connection in TIME_WAIT would run out of ports.
    private readonly LingerOption _resetOnClose = new(enable: true, seconds: 0);

    private AsyncAcceptor _acceptor = null!;
    private TcpListener _floor = null!;
    private IPEndPoint _acceptorEndpoint = null!;
    private IPEndPoint _floorEndpoint = null!;
    private Task _floorLoop = null!;

    [GlobalSetup]
    public void Setup()
    {
        _floor = new TcpListener(IPAddress.Loopback, 0);
        _floor.Start();
        _floorEndpoint = (IPEndPoint)_floor.LocalEndpoint;
        _floorLoop = FloorLoop();

        // The acceptor takes a port number, so borrow a free one from the OS.
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        _acceptorEndpoint = (IPEndPoint)probe.LocalEndpoint;
        probe.Stop();

        _acceptor = new AsyncAcceptor();
        _acceptor.Start("127.0.0.1", _acceptorEndpoint.Port);
        _acceptor.BeginAccept(OnAccepted);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _acceptor.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _stopping.Cancel();
        _floorLoop.GetAwaiter().GetResult();
        _floor.Stop();
    }

    [Benchmark(Baseline = true)]
    public void Floor() => Connect(_floorEndpoint);

    [Benchmark]
    public void Acceptor() => Connect(_acceptorEndpoint);

    private void Connect(IPEndPoint endpoint)
    {
        using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.LingerState = _resetOnClose;
        client.Connect(endpoint);
        _accepted.Wait();
    }

    private void OnAccepted(Socket socket)
    {
        socket.Dispose();
        _accepted.Release();
    }

    private async Task FloorLoop()
    {
        try
        {
            while (true)
                OnAccepted(await _floor.AcceptSocketAsync(_stopping.Token));
        }
        catch (OperationCanceledException)
        {
        }
    }
}
