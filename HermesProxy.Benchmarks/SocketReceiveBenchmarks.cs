using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Framework.Networking;

namespace HermesProxy.Benchmarks;

/// <summary>
/// One frame from the peer's send to the reader's handler, over loopback, on the real
/// <see cref="SocketBase"/> read path. <see cref="Floor"/> is a bare receive loop, so the difference
/// is what the read path adds per frame.
/// </summary>
[MemoryDiagnoser]
public class SocketReceiveBenchmarks
{
    private const int FrameSize = 256;

    private readonly byte[] _frame = new byte[FrameSize];
    private readonly SemaphoreSlim _received = new(0, 1);
    private readonly CancellationTokenSource _stopping = new();

    private Socket _floorPeer = null!;
    private Socket _floorSocket = null!;
    private Socket _readerPeer = null!;
    private Reader _reader = null!;
    private Task _floorLoop = null!;

    [GlobalSetup]
    public void Setup()
    {
        (_floorPeer, _floorSocket) = Connect();
        _floorLoop = FloorLoop();

        (_readerPeer, Socket accepted) = Connect();
        _reader = new Reader(accepted, _received);
        _reader.Accept();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _stopping.Cancel();
        _floorLoop.GetAwaiter().GetResult();
        _reader.CloseSocket();
        _readerPeer.Dispose();
        _floorPeer.Dispose();
        _floorSocket.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void Floor()
    {
        _floorPeer.Send(_frame);
        _received.Wait();
    }

    [Benchmark]
    public void Receive()
    {
        _readerPeer.Send(_frame);
        _received.Wait();
    }

    private static (Socket Peer, Socket Accepted) Connect()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        peer.Connect(listener.LocalEndpoint);
        var accepted = listener.AcceptSocket();
        accepted.NoDelay = true;
        listener.Stop();
        return (peer, accepted);
    }

    private async Task FloorLoop()
    {
        var buffer = new byte[0x4000];
        try
        {
            while (await _floorSocket.ReceiveAsync(buffer.AsMemory(), SocketFlags.None, _stopping.Token) > 0)
                _received.Release();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private sealed class Reader(Socket socket, SemaphoreSlim received) : SocketBase(socket)
    {
        public override void Accept() => BeginReceive();

        public override void ReadHandler(byte[] data, int length) => received.Release();
    }
}
