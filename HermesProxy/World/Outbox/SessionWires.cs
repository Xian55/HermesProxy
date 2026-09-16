using Framework.Constants;
using Framework.Logging;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Outbox;

/// <summary>Writes client-bound packets to the session's realm or instance socket.</summary>
internal sealed class SessionClientWire(GlobalSessionData session) : IClientWire
{
    public bool TryWrite(ConnectionType connection, ServerPacket packet)
    {
        // Resolved per write: both sockets are replaced over a session's life (change realm,
        // relog), and a closed one is left in place until something notices.
        var socket = connection == ConnectionType.Realm ? session.RealmSocket : session.InstanceSocket;
        if (socket == null || !socket.IsOpen())
            return false;

        socket.SendPacket(packet);
        return true;
    }
}

/// <summary>Writes server-bound packets through the session's current world client.</summary>
internal sealed class SessionServerWire(GlobalSessionData session) : IServerWire
{
    private static readonly Microsoft.Extensions.Logging.ILogger _log = Log.CreateMelLogger(Log.CategoryNetwork);

    public void Write(WorldPacket packet)
    {
        var client = session.WorldClient;
        if (client != null)
        {
            client.SendPacketToServer(packet);
            return;
        }

        OutboxLogMessages.WireUnavailable(_log, "server", packet.GetUniversalOpcode(false), "world client");
        packet.Dispose();
    }
}
