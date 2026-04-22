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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace Framework.Networking;

// TSocketType is constructed via ActivatorUtilities.CreateInstance(services, socket) in
// OnSocketOpen. The trimmer can't see that call path, so the constructor gets stripped from
// published (PublishTrimmed=true) binaries — MissingMethodException at first inbound
// connection. The annotation tells the trimmer to preserve public ctors on any T that
// SocketManager<T> is closed over. Same fix family as the Singleton<T> annotation in PR #36.
public class SocketManager<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSocketType> where TSocketType : ISocket
{
    private readonly IServiceProvider _services;

    public AsyncAcceptor Acceptor = null!;
    NetworkThread<TSocketType>[] _threads = null!;
    int _threadCount;

    public bool IsListening => Acceptor.IsListening;

    public SocketManager(IServiceProvider services)
    {
        _services = services;
    }

    public virtual bool StartNetwork(string bindIp, int port, int threadCount = 1)
    {
        Cypher.Assert(threadCount > 0);

        Acceptor = new AsyncAcceptor();
        if (!Acceptor.Start(bindIp, port))
        {
            Log.Print(LogType.Network, "StartNetwork failed to Start AsyncAcceptor");
            return false;
        }

        _threadCount = threadCount;
        _threads = new NetworkThread<TSocketType>[GetNetworkThreadCount()];

        for (int i = 0; i < _threadCount; ++i)
        {
            _threads[i] = new NetworkThread<TSocketType>();
            _threads[i].Start();
        }

        _ = Acceptor.AsyncAcceptSocket(OnSocketOpen);

        return true;
    }

    public virtual void StopNetwork()
    {
        Acceptor.Close();

        if (_threadCount != 0)
            for (int i = 0; i < _threadCount; ++i)
                _threads[i].Stop();

        Wait();

        Acceptor = null!;
        _threads = null!;
        _threadCount = 0;
    }

    void Wait()
    {
        if (_threadCount != 0)
            for (int i = 0; i < _threadCount; ++i)
                _threads[i].Wait();
    }

    public virtual void OnSocketOpen(Socket sock)
    {
        try
        {
            TSocketType newSocket = ActivatorUtilities.CreateInstance<TSocketType>(_services, sock);
            newSocket.Accept();

            _threads[SelectThreadWithMinConnections()].AddSocket(newSocket);
        }
        catch (Exception err)
        {
            Log.outException(err);
        }
    }

    public int GetNetworkThreadCount() { return _threadCount; }

    uint SelectThreadWithMinConnections()
    {
        uint min = 0;

        for (uint i = 1; i < _threadCount; ++i)
            if (_threads[i].GetConnectionCount() < _threads[min].GetConnectionCount())
                min = i;

        return min;
    }
}
