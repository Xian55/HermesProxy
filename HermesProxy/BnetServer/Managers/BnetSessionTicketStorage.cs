// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using HermesProxy;
using System.Collections.Concurrent;

namespace BNetServer;

public static class BnetSessionTicketStorage
{
    public static readonly ConcurrentDictionary<string, GlobalSessionData> SessionsByName = new();
    public static readonly ConcurrentDictionary<string, GlobalSessionData> SessionsByTicket = new();
    public static readonly ConcurrentDictionary<ulong, GlobalSessionData> SessionsByKey = new();

    public static void AddNewSessionByName(string name, GlobalSessionData session)
    {
        if (SessionsByName.TryGetValue(name, out var existing))
            existing.OnDisconnect();

        SessionsByName[name] = session;
    }

    public static void AddNewSessionByTicket(string loginTicket, GlobalSessionData session)
    {
        if (SessionsByTicket.TryGetValue(loginTicket, out var existing))
            existing.OnDisconnect();

        SessionsByTicket[loginTicket] = session;
    }

    public static void AddNewSessionByKey(ulong connectKey, GlobalSessionData session)
    {
        if (SessionsByKey.TryGetValue(connectKey, out var existing))
            existing.OnDisconnect();

        SessionsByKey[connectKey] = session;
    }
}
