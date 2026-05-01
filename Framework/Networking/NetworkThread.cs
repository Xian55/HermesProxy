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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Framework.Networking;

public class NetworkThread<TSocketType> where TSocketType : ISocket
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melNet = Log.CreateMelLogger(Log.CategoryNetwork);
    private static readonly string _sourceFile = "NetworkThread".PadRight(15);
    private const string _netDirNone = "";

    int _connections;
    volatile bool _stopped;

    Thread? _thread;

    List<TSocketType> _Sockets = new List<TSocketType>();
    ConcurrentQueue<TSocketType> _newSockets = new ConcurrentQueue<TSocketType>();

    public void Stop()
    {
        _stopped = true;
    }

    public bool Start()
    {
        if (_thread != null)
            return false;

        _thread = new Thread(Run);
        _thread.Start();
        return true;
    }

    public void Wait()
    {
        _thread?.Join();
        _thread = null;
    }

    public int GetConnectionCount()
    {
        return _connections;
    }

    public virtual void AddSocket(TSocketType sock)
    {
        Interlocked.Increment(ref _connections);
        _newSockets.Enqueue(sock);
        SocketAdded(sock);
    }

    protected virtual void SocketAdded(TSocketType sock) { }

    protected virtual void SocketRemoved(TSocketType sock) { }

    void AddNewSockets()
    {
        // Drain the queue - lock-free, no allocations
        while (_newSockets.TryDequeue(out var socket))
        {
            if (!socket.IsOpen())
            {
                SocketRemoved(socket);
                Interlocked.Decrement(ref _connections);
            }
            else
            {
                _Sockets.Add(socket);
            }
        }
    }

    void Run()
    {
        NetworkThreadLogMessages.NetworkThreadStarting(_melNet, _sourceFile, _netDirNone);

        int sleepTime = 10;
        while (!_stopped)
        {
            Thread.Sleep(sleepTime);

            uint tickStart = Time.GetMSTime();

            AddNewSockets();

            // Iterate backwards to allow O(1) removal without skipping elements
            for (int i = _Sockets.Count - 1; i >= 0; i--)
            {
                TSocketType socket = _Sockets[i];
                if (!socket.Update())
                {
                    if (socket.IsOpen())
                        socket.CloseSocket();

                    SocketRemoved(socket);

                    --_connections;

                    // O(1) removal: swap with last element and remove from end
                    int lastIndex = _Sockets.Count - 1;
                    if (i != lastIndex)
                    {
                        _Sockets[i] = _Sockets[lastIndex];
                    }
                    _Sockets.RemoveAt(lastIndex);
                }
            }

            uint diff = Time.GetMSTimeDiffToNow(tickStart);
            sleepTime = (int)(diff > 10 ? 0 : 10 - diff);
        }

        Log.Print(LogType.Network, "Network Thread exits");

        // Close every accepted socket on shutdown. Without this the peer (e.g. the WoW client)
        // sees no FIN and stays ESTABLISHED, which causes the OS to hold the listening port -
        // the next proxy run then fails to bind with WSAEACCES until the peer is restarted.
        // Note: deliberately not firing SocketRemoved or decrementing _connections here -
        // those represent the normal-disconnect lifecycle, not a forced shutdown.
        while (_newSockets.TryDequeue(out var pending))
        {
            if (pending.IsOpen())
            {
                try { pending.CloseSocket(); } catch { /* best effort */ }
            }
        }
        foreach (var socket in _Sockets)
        {
            if (socket.IsOpen())
            {
                try { socket.CloseSocket(); } catch { /* best effort */ }
            }
        }
        _Sockets.Clear();
    }
}
