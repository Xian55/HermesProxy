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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Networking;

public delegate void SocketAcceptDelegate(Socket newSocket);

/// <summary>
/// Owns one listening socket and the one task that accepts on it, from <see cref="BeginAccept"/>
/// to <see cref="DisposeAsync"/>.
/// </summary>
/// <remarks>
/// That task handles every failure it can meet, so it never faults, and <see cref="DisposeAsync"/>
/// waits for it, so it is never abandoned. A fire-and-forget chain of accepts did neither: stopping
/// the listener faulted the pending one, and nobody saw that until a GC reported it (#540).
/// </remarks>
public sealed class AsyncAcceptor : IAsyncDisposable
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melNet = Log.CreateMelLogger(Log.CategoryNetwork);
    private static readonly string _sourceFile = nameof(AsyncAcceptor).PadRight(15);
    private const string _netDirNone = "";

    private static readonly TimeSpan AcceptRetryDelay = TimeSpan.FromSeconds(1);

    readonly CancellationTokenSource _stopping = new();
    TcpListener _listener = null!;
    Task _acceptLoop = Task.CompletedTask;

    public bool IsListening => !_acceptLoop.IsCompleted;

    internal Task AcceptLoop => _acceptLoop;

    internal IPEndPoint LocalEndPoint => (IPEndPoint)_listener.LocalEndpoint;

    public bool Start(string ip, int port)
    {
        if (!IPAddress.TryParse(ip, out IPAddress? bindIP))
        {
            Log.Print(LogType.Error, $"Server can't be started: Invalid IP-Address: {ip}");
            return false;
        }

        try
        {
            _listener = new TcpListener(bindIP, port);
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.Start();
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AccessDenied)
        {
            Log.Print(LogType.Error,
                $"Cannot bind {ip}:{port} - port is held with exclusive access (WSAEACCES). " +
                "Common Windows causes: (1) the WoW client still has an open TCP connection to a " +
                "previous proxy instance - close and reopen the client to release it; " +
                "(2) the port falls inside a Hyper-V / WinNAT excluded range - verify with " +
                "'netsh int ipv4 show excludedportrange protocol=tcp'; " +
                "(3) another process is bound here with SO_EXCLUSIVEADDRUSE. " +
                "Or change the listen port via ProxyNetworkOptions.");
            return false;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            Log.Print(LogType.Error,
                $"Cannot bind {ip}:{port} - another process is already listening here (WSAEADDRINUSE). " +
                "Stop the conflicting process or change the listen port via ProxyNetworkOptions.");
            return false;
        }
        catch (SocketException ex)
        {
            Log.outException(ex);
            return false;
        }

        return true;
    }

    public void BeginAccept(SocketAcceptDelegate onAccept) =>
        _acceptLoop = AcceptLoopAsync(_listener.AcceptSocketAsync, onAccept, AcceptRetryDelay, _stopping.Token);

    internal static async Task AcceptLoopAsync(
        Func<CancellationToken, ValueTask<Socket>> accept, SocketAcceptDelegate onAccept, TimeSpan retryDelay, CancellationToken stopping)
    {
        try
        {
            while (true)
            {
                Socket socket;
                try
                {
                    socket = await accept(stopping);
                }
                catch (SocketException ex) when (ex.SocketErrorCode is SocketError.ConnectionReset or SocketError.ConnectionAborted)
                {
                    // The peer gave up in the backlog. That costs this one connection, and waiting
                    // here would let a peer that keeps doing it stall everybody else's accept.
                    AsyncAcceptorLogMessages.ConnectionLostInBacklog(_melNet, _sourceFile, _netDirNone, ex.SocketErrorCode);
                    continue;
                }
                catch (SocketException ex)
                {
                    // Out of file descriptors, say. Those come back, so the listener has to outlive
                    // this, but accepting again at once would spin and flood the log.
                    AsyncAcceptorLogMessages.AcceptFailed(_melNet, ex, _sourceFile, _netDirNone, ex.SocketErrorCode, retryDelay);
                    await Task.Delay(retryDelay, stopping).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                    continue;
                }

                Dispatch(socket, onAccept);
            }
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
            // DisposeAsync asked for this.
        }
        catch (Exception ex)
        {
            AsyncAcceptorLogMessages.AcceptLoopEnded(_melNet, ex, _sourceFile, _netDirNone, ex.Message);
        }
    }

    // The one failure boundary of the accept path: a connection that cannot be set up is lost
    // alone, and the listener goes on.
    private static void Dispatch(Socket socket, SocketAcceptDelegate onAccept)
    {
        try
        {
            onAccept(socket);
        }
        catch (Exception ex)
        {
            AsyncAcceptorLogMessages.ConnectionSetupFailed(_melNet, ex, _sourceFile, _netDirNone, ex.Message);
            socket.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_stopping.IsCancellationRequested)
            return;

        // Cancelling first means no accept ever runs into a closed socket, so stopping has one
        // outcome on every platform. Waiting means no handler is still running once this returns.
        await _stopping.CancelAsync();
        await _acceptLoop;

        // Without this the listener stays bound and the port stays taken.
        _listener?.Dispose();
        _stopping.Dispose();
    }
}
