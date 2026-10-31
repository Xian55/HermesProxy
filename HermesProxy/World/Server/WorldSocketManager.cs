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
using Framework.Networking;
using System;
using System.Net.Sockets;

namespace HermesProxy.World.Server;

public sealed class WorldSocketManager : SocketManager<WorldSocket>
{
    public WorldSocketManager(IServiceProvider services)
        : base(services)
    {
    }

    public override bool StartNetwork(string bindIp, int port)
    {
        _tcpNoDelay = true;

        // -1 means use default
        _socketSendBufferSize = -1;

        return base.StartNetwork(bindIp, port);
    }

    public override void OnSocketOpen(Socket sock)
    {
        Log.Print(LogType.Network, $"Instance socket open.");

        // set some options here
        if (_socketSendBufferSize >= 0)
            sock.SendBufferSize = _socketSendBufferSize;

        // Set TCP_NODELAY.
        sock.NoDelay = _tcpNoDelay;

        base.OnSocketOpen(sock);
    }

    int _socketSendBufferSize;
    bool _tcpNoDelay;
}
