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
    Socket _socket;
    IPEndPoint? _remoteIPEndPoint;

    SocketAsyncEventArgs receiveSocketAsyncEventArgsWithCallback;
    SocketAsyncEventArgs receiveSocketAsyncEventArgs;

    byte[]? _callbackBuffer;
    byte[]? _asyncBuffer;
    const int BufferSize = 0x4000;

    public delegate void SocketReadCallback(SocketAsyncEventArgs args);

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

        _callbackBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        _asyncBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);

        receiveSocketAsyncEventArgsWithCallback = new SocketAsyncEventArgs();
        receiveSocketAsyncEventArgsWithCallback.SetBuffer(_callbackBuffer, 0, BufferSize);

        receiveSocketAsyncEventArgs = new SocketAsyncEventArgs();
        receiveSocketAsyncEventArgs.SetBuffer(_asyncBuffer, 0, BufferSize);
        receiveSocketAsyncEventArgs.Completed += (sender, args) => ProcessReadAsync(args);
    }

    /// <summary>Shortens the send deadline, so a test for a stalled peer doesn't take 30 seconds.</summary>
    internal void SetSendTimeout(TimeSpan timeout) => _socket.SendTimeout = (int)timeout.TotalMilliseconds;

    public virtual void Dispose()
    {
        _socket.Dispose();

        if (_callbackBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(_callbackBuffer);
            _callbackBuffer = null;
        }

        if (_asyncBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(_asyncBuffer);
            _asyncBuffer = null;
        }

        receiveSocketAsyncEventArgsWithCallback?.Dispose();
        receiveSocketAsyncEventArgs?.Dispose();
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

    public void AsyncReadWithCallback(SocketReadCallback callback)
    {
        if (!IsOpen())
            return;

        receiveSocketAsyncEventArgsWithCallback.Completed += (sender, args) => callback(args);
        receiveSocketAsyncEventArgsWithCallback.SetBuffer(0, BufferSize);
        if (!_socket.ReceiveAsync(receiveSocketAsyncEventArgsWithCallback))
            callback(receiveSocketAsyncEventArgsWithCallback);
    }

    public void AsyncRead()
    {
        if (!IsOpen())
            return;

        receiveSocketAsyncEventArgs.SetBuffer(0, BufferSize);
        if (!_socket.ReceiveAsync(receiveSocketAsyncEventArgs))
            ProcessReadAsync(receiveSocketAsyncEventArgs);
    }

    void ProcessReadAsync(SocketAsyncEventArgs args)
    {
        if (args.SocketError != SocketError.Success)
        {
            CloseSocket();
            return;
        }

        if (args.BytesTransferred == 0)
        {
            CloseSocket();
            return;
        }

        ReadHandler(args);
    }

    public abstract void ReadHandler(SocketAsyncEventArgs args);

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
        if (_socket == null || !_socket.Connected)
            return;

        try
        {
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
