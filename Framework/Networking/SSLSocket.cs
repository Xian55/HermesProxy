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
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Networking;

public abstract class SSLSocket : ISocket, IDisposable
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melNet = Log.CreateMelLogger(Log.CategoryNetwork);
    private static readonly string _sourceFile = nameof(SSLSocket).PadRight(15);
    private const string _netDirNone = "";

    Socket _socket;
    internal SslStream _stream;
    IPEndPoint? _remoteEndPoint;
    byte[]? _receiveBuffer;

    // SslStream throws NotSupportedException for a WriteAsync that starts while another is pending,
    // and callers send without awaiting, so writes queue here. SemaphoreSlim releases async waiters
    // in arrival order, which keeps frames in the order they were sent.
    readonly SemaphoreSlim _writeLock = new(1, 1);

    protected SSLSocket(Socket socket)
    {
        _socket = socket;
        _remoteEndPoint = _socket.RemoteEndPoint as IPEndPoint;
        _receiveBuffer = new byte[ushort.MaxValue];

        _stream = new SslStream(new NetworkStream(socket), false);
    }

    public virtual void Dispose()
    {
        _receiveBuffer = null!;
        _stream.Dispose();
    }

    public abstract void Accept();

    public virtual bool Update()
    {
        return _socket.Connected;
    }

    public IPEndPoint? GetRemoteIpEndPoint()
    {
        return _remoteEndPoint;
    }

    public async Task AsyncRead()
    {
        if (!IsOpen() || _receiveBuffer is null)
            return;

        try
        {
            var receiveBuffer = _receiveBuffer;
            var result = await _stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
            if (result == 0)
            {
                CloseSocket();
                return;
            }

            // ReadHandler re-arms the loop itself by awaiting AsyncRead again, so awaiting it here
            // would chain every read into the previous one and never unwind. It has to stay fire
            // and forget — but the task must still be observed: a fault that goes unwatched also
            // skips ReadHandler's own AsyncRead, leaving the connection open and never reading.
            var handlerTask = ReadHandler(receiveBuffer, result);
            if (!handlerTask.IsCompletedSuccessfully)
                ObserveFault(handlerTask, GetRemoteIpEndPoint());
        }
        catch (Exception ex)
        {
            Log.outException(ex);
        }
    }

    private static void ObserveFault(Task handlerTask, IPEndPoint? endPoint)
    {
        handlerTask.ContinueWith(
            static (task, state) =>
            {
                var ex = task.Exception!.GetBaseException();
                SslSocketLogMessages.ReadHandlerFaulted(_melNet, ex, _sourceFile, _netDirNone,
                    state?.ToString() ?? "<unknown>", ex.Message);
            },
            endPoint,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    public async Task AsyncHandshake(X509Certificate2 certificate)
    {
        try
        {
            await _stream.AuthenticateAsServerAsync(certificate, false, SslProtocols.Tls12, false);
        }
        catch (Exception ex) when (ex is AuthenticationException || ex is System.IO.IOException)
        {
            // WoW retail opens BNet probe connections that arrive without a valid TLS
            // ClientHello (AuthenticationException) or close mid-handshake (IOException
            // "unexpected EOF"). Either way it's a probe — log a single line, no stack.
            Log.Print(LogType.Warn, $"TLS handshake failed for {GetRemoteIpEndPoint()}: {ex.Message}");
            CloseSocket();
            return;
        }
        catch (Exception ex)
        {
            Log.outException(ex);
            CloseSocket();
            return;
        }

        await AsyncRead();
    }

    public abstract Task ReadHandler(byte[] data, int receivedLength);

    public Task AsyncWrite(byte[] data) => AsyncWrite(data, data.Length, returnToPool: false);

    /// <summary>
    /// Writes the first <paramref name="length"/> bytes of <paramref name="buffer"/> once any write
    /// already in flight has finished. With <paramref name="returnToPool"/> this call owns the
    /// buffer and hands it back to <see cref="ArrayPool{T}.Shared"/> after the write completes or fails.
    /// </summary>
    public async Task AsyncWrite(byte[] buffer, int length, bool returnToPool)
    {
        await _writeLock.WaitAsync();
        try
        {
            if (IsOpen())
                await _stream.WriteAsync(buffer.AsMemory(0, length));
        }
        catch (Exception ex)
        {
            Log.outException(ex);
        }
        finally
        {
            _writeLock.Release();
            if (returnToPool)
                ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Closes once the writes queued before this call are on the wire, so a final notification is
    /// not cut off. Closes anyway after <paramref name="timeout"/>: a peer that stopped reading must
    /// not be able to hold the connection open.
    /// </summary>
    public async Task CloseSocketAfterWrites(TimeSpan timeout)
    {
        bool acquired = await _writeLock.WaitAsync(timeout);
        try
        {
            CloseSocket();
        }
        finally
        {
            if (acquired)
                _writeLock.Release();
        }
    }

    public void CloseSocket()
    {
        try
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        catch (Exception ex)
        {
            Log.Print(LogType.Network, $"WorldSocket.CloseSocket: {GetRemoteIpEndPoint()} errored when shutting down socket: {ex.Message}");
        }
    }

    public virtual void OnClose() { Dispose(); }

    public bool IsOpen() { return _socket.Connected; }

    public void SetNoDelay(bool enable)
    {
        _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, enable);
    }
}
