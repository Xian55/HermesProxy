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
using System.Threading.Tasks;

namespace Framework.Networking;

// TSocketType is constructed via ActivatorUtilities.CreateInstance(services, socket) in
// OnSocketOpen. The trimmer can't see that call path, so the constructor gets stripped from
// published (PublishTrimmed=true) binaries — MissingMethodException at first inbound
// connection. The annotation tells the trimmer to preserve public ctors on any T that
// SocketManager<T> is closed over. Same fix family as the Singleton<T> annotation in PR #36.
public class SocketManager<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSocketType> where TSocketType : ISocket
{
    private readonly IServiceProvider _services;

    AsyncAcceptor? _acceptor;
    ConnectionSweeper<TSocketType>? _connections;

    public bool IsListening => _acceptor is { IsListening: true };

    public SocketManager(IServiceProvider services)
    {
        _services = services;
    }

    public virtual bool StartNetwork(string bindIp, int port)
    {
        var acceptor = new AsyncAcceptor();
        if (!acceptor.Start(bindIp, port))
        {
            Log.Print(LogType.Network, "StartNetwork failed to Start AsyncAcceptor");
            return false;
        }

        var connections = new ConnectionSweeper<TSocketType>(ConnectionSweeper<TSocketType>.DefaultInterval);
        connections.Start();

        _acceptor = acceptor;
        _connections = connections;

        acceptor.BeginAccept(OnSocketOpen);

        return true;
    }

    /// <summary>
    /// Safe to call on a listener that already ended or never started, so it does not depend on
    /// <see cref="IsListening"/>: the connections of a listener that ended by itself still have to
    /// be closed.
    /// </summary>
    public async Task StopNetworkAsync()
    {
        if (_acceptor is null)
            return;

        // First, and awaited: once it returns no handler can add a connection behind the sweeper.
        await _acceptor.DisposeAsync();
        await _connections!.DisposeAsync();

        _acceptor = null;
        _connections = null;
    }

    public virtual void OnSocketOpen(Socket sock)
    {
        TSocketType newSocket = ActivatorUtilities.CreateInstance<TSocketType>(_services, sock);
        newSocket.Accept();

        _connections!.Add(newSocket);
    }
}
