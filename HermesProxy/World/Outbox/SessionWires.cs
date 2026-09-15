using Framework.Constants;
using Framework.Logging;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Outbox;

/// <summary>
/// Writes client-bound packets through the session's current world client, which routes each one
/// to the realm or instance socket and still owns the older delay queues until they move here.
/// </summary>
internal sealed class SessionClientWire(GlobalSessionData session) : IClientWire
{
    private static readonly Microsoft.Extensions.Logging.ILogger _log = Log.CreateMelLogger(Log.CategoryNetwork);

    public void Write(ServerPacket packet)
    {
        // Resolved per write: the world client is replaced on change realm and absent before
        // the legacy connection exists.
        var client = session.WorldClient;
        if (client != null)
        {
            client.SendPacketToClient(packet);
            return;
        }

        OutboxLogMessages.WireUnavailable(_log, "client", packet.GetUniversalOpcode(), "world client");
        packet.Discard();
    }

    public void WriteOn(ConnectionType connection, ServerPacket packet)
    {
        var socket = connection == ConnectionType.Realm ? session.RealmSocket : session.InstanceSocket;
        if (socket != null)
        {
            socket.SendPacket(packet);
            return;
        }

        OutboxLogMessages.WireUnavailable(_log, "client", packet.GetUniversalOpcode(), connection == ConnectionType.Realm ? "realm socket" : "instance socket");
        packet.Discard();
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
