using System.Runtime.CompilerServices;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server;

namespace HermesProxy.World.Dispatch;

/// <summary>
/// Everything a packet system is allowed to reach: one session, and the two sockets it spans.
/// </summary>
/// <remarks>
/// <para>
/// Per session, not per packet. It holds references, so it costs nothing to build and is passed
/// <see langword="in"/> so dispatch copies a pointer rather than the struct.
/// </para>
/// <para>
/// <b>Why this and not ambient statics.</b> Handlers today are instance methods that reach
/// <c>GetSession()</c>, <c>SendPacketToClient</c> and friends through <see langword="this"/>.
/// Moving a handler to a static system removes <see langword="this"/>, so every one of those
/// calls stops compiling until it is pointed at a context — which is exactly the property that
/// makes ~2,200 mechanical edits reviewable. The forwarders below therefore carry the *same
/// names and signatures* as the instance members they replace, so converting a body is a
/// prefix, not a rewrite.
/// </para>
/// <para>
/// <b>What deliberately stays static.</b> <c>ModernVersion</c>, <c>LegacyVersion</c> and
/// <c>GameData</c>. The first two are <see langword="static readonly"/>, so the JIT folds the
/// ~1,260 version comparisons into constants; moving them onto an instance would turn every one
/// into a load plus a branch on the hot path this work exists to speed up. <c>GameData</c> is
/// load-once reference data. Session state is the part that was genuinely ambient, and it is the
/// part this carries.
/// </para>
/// <para>
/// <b>Lifetime.</b> Built when a session binds to a socket, not in the socket constructor —
/// <c>_globalSession</c> is assigned later, in <c>HandleAuthSession</c> and
/// <c>ConnectToWorldServer</c>. <see cref="IsBound"/> exists so the dispatch site can assert
/// that ordering held rather than dereferencing null.
/// </para>
/// </remarks>
public readonly struct SessionContext
{
    /// <summary>The session. Null until the owning socket binds one.</summary>
    public readonly GlobalSessionData Session;

    /// <summary>The modern-client socket a CMSG arrived on. Null on the legacy path.</summary>
    public readonly WorldSocket? Socket;

    /// <summary>
    /// The legacy-emulator connection, when the context was built by one. Null on the modern
    /// path — and stale-proof by never being the thing the send forwarders read: on the modern
    /// side the session binds in HandleAuthSession, well before the world client connects, so a
    /// captured reference would be null for the whole login window.
    /// </summary>
    public readonly WorldClient? Client;

    public SessionContext(GlobalSessionData session, WorldSocket? socket, WorldClient? client)
    {
        Session = session;
        Socket = socket;
        Client = client;
    }

    /// <summary>False before the owning socket has bound a session.</summary>
    public bool IsBound
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Session != null;
    }

    // ---- forwarders, named exactly as the instance members handlers call today ----

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GlobalSessionData GetSession() => Session;

    public GameSessionData GameState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Session.GameState;
    }

    /// <summary>
    /// Proxy to legacy emulator. Mirrors <c>WorldSocket.SendPacketToServer</c>, including its
    /// behaviour when the legacy connection is gone — dropping with an error beats throwing
    /// inside a handler.
    /// </summary>
    public void SendPacketToServer(WorldPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
    {
        WorldClient? client = Session.WorldClient;
        if (client != null)
            client.SendPacketToServer(packet, delayUntilOpcode);
        else
            Framework.Logging.Log.Print(Framework.Logging.LogType.Error,
                $"Attempt to send opcode {packet.GetUniversalOpcode(false)} ({packet.GetOpcode()}) while WorldClient is disconnected!");
    }

    /// <summary>
    /// Proxy to modern client, routed by connection type. Mirrors
    /// <c>WorldClient.SendPacketToClient</c>. Resolved from the session on every call rather than
    /// from <see cref="Client"/>, for the same reason <see cref="SendPacketToServer"/> is: the
    /// world client is attached to the session after the modern socket binds it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SendPacketToClient(ServerPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
        => (Client ?? Session.WorldClient)!.SendPacketToClient(packet, delayUntilOpcode);

    /// <summary>Send on the socket this packet arrived on. Mirrors <c>WorldSocket.SendPacket</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SendPacket(ServerPacket packet) => Socket!.SendPacket(packet);

    /// <summary>
    /// Packets to the modern client: sent now, or held until an event, a gate or a deadline.
    /// See <c>World/Outbox/CLAUDE.md</c> for which call fits which situation.
    /// </summary>
    public ClientOutbox ToClient
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Session.ToClient;
    }

    /// <summary>Packets to the legacy server: sent now, or held. See <c>World/Outbox/CLAUDE.md</c>.</summary>
    public ServerOutbox ToServer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Session.ToServer;
    }
}
