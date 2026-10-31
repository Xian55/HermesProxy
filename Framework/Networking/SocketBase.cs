/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Framework.Logging;
using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Networking;

public interface ISocket
{
    void Accept();
    bool Update();
    bool IsOpen();
    void CloseSocket();
}

public abstract class SocketBase : ISocket, IDisposable
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melNet = Log.CreateMelLogger(Log.CategoryNetwork);
    private static readonly string _sourceFile = nameof(SocketBase).PadRight(15);
    private const string _netDirNone = "";

    Socket _socket;
    IPEndPoint? _remoteIPEndPoint;
    Task _receiveLoop = Task.CompletedTask;
    int _closed;
    const int BufferSize = 0x4000;

    internal Task ReceiveLoop => _receiveLoop;

    /// <summary>
    /// How long a blocking send may sit unacknowledged before the connection is given up on.
    /// A client that has stopped reading — stalled, suspended, gone without a FIN — otherwise
    /// pins the sending thread for as long as it likes, and with one instance serving many
    /// players those threads are the pool's.
    /// </summary>
    public static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(30);

    protected SocketBase(Socket socket)
    {
        _socket = socket;
        _socket.SendTimeout = (int)SendTimeout.TotalMilliseconds;
        // A client whose machine goes away without closing the connection leaves a session that
        // holds its game state for as long as the process runs. The client's own pings don't help:
        // nothing here checks that they keep arriving.
        NetworkUtils.EnableKeepAlive(socket, idleSeconds: 60, intervalSeconds: 10, retryCount: 3);
        _remoteIPEndPoint = _socket.RemoteEndPoint as IPEndPoint;
    }

    /// <summary>Shortens the send deadline, so a test for a stalled peer doesn't take 30 seconds.</summary>
    internal void SetSendTimeout(TimeSpan timeout) => _socket.SendTimeout = (int)timeout.TotalMilliseconds;

    public virtual void Dispose()
    {
        _socket.Dispose();
    }

    public abstract void Accept();

    public virtual bool Update()
    {
        return IsOpen();
    }

    public IPEndPoint? GetRemoteIpAddress()
    {
        return _remoteIPEndPoint;
    }

    /// <summary>Starts reading. <see cref="ReadHandler"/> then gets every read, one at a time.</summary>
    protected void BeginReceive() => _receiveLoop = ReceiveLoopAsync();

    // One loop per connection, so that everything a connection can end with ends here.
    private async Task ReceiveLoopAsync()
    {
        // Accept runs on the listener's loop, which must not end up reading this connection.
        await Task.Yield();

        byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        try
        {
            while (IsOpen())
            {
                int received = await _socket.ReceiveAsync(buffer.AsMemory(0, BufferSize), SocketFlags.None);
                if (received == 0)
                    break;

                ReadHandler(buffer, received);
            }
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            // The peer reset the connection, or CloseSocket closed it under this read.
        }
        catch (Exception ex)
        {
            SocketBaseLogMessages.ReadFailed(_melNet, ex, _sourceFile, _netDirNone,
                GetRemoteIpAddress()?.ToString() ?? "<unknown>", ex.Message);
        }
        finally
        {
            // Here and not in Dispose: only now is no read using it.
            ArrayPool<byte>.Shared.Return(buffer);
        }

        try
        {
            CloseSocket();
        }
        catch (Exception ex)
        {
            SocketBaseLogMessages.CloseFailed(_melNet, ex, _sourceFile, _netDirNone,
                GetRemoteIpAddress()?.ToString() ?? "<unknown>", ex.Message);
        }
    }

    /// <summary>
    /// Takes one read: the first <paramref name="length"/> bytes of <paramref name="data"/>, which
    /// are only valid during the call. Throwing closes this connection and no other.
    /// </summary>
    public abstract void ReadHandler(byte[] data, int length);

    public void AsyncWrite(byte[] data) => AsyncWrite(data.AsSpan());

    /// <summary>
    /// Despite the name this is a blocking send: the caller's buffer is free to reuse or
    /// return to a pool as soon as this returns.
    /// </summary>
    public void AsyncWrite(ReadOnlySpan<byte> data)
    {
        if (!IsOpen())
            return;

        try
        {
            _socket.Send(data);
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
        {
            // A timed-out send has already put an unknown number of bytes on the wire, so the
            // frame boundary is lost and nothing further can be written to this peer. Close it
            // and let the session tear down the way any other dropped connection does.
            Log.Print(LogType.Network,
                $"Send to {GetRemoteIpAddress()} timed out after {SendTimeout.TotalSeconds:F0} s; closing the connection");
            CloseSocket();
        }
    }

    public void CloseSocket()
    {
        // Once, and not only while Connected: a reset leaves that false, which used to skip OnClose.
        if (Interlocked.Exchange(ref _closed, 1) != 0)
            return;

        try
        {
            if (_socket.Connected)
                _socket.Shutdown(SocketShutdown.Both);

            _socket.Close();
        }
        catch (Exception ex)
        {
            Log.Print(LogType.Network, $"WorldSocket.CloseSocket: {GetRemoteIpAddress()} errored when shutting down socket: {ex.Message}");
        }

        OnClose();
    }

    public virtual void OnClose() { Dispose(); }

    public bool IsOpen() { return _socket.Connected; }

    public void SetNoDelay(bool enable)
    {
        _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, enable);
    }
}
