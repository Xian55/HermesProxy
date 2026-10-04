using System;
using BenchmarkDotNet.Attributes;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Benchmarks;

/// <summary>
/// Movement translation in both directions, through the real handlers on a real session: the
/// highest-rate traffic the proxy carries. The packets are the ones
/// <c>MovementWireGoldenTests</c> pins byte for byte (see <see cref="MovementScenarios"/>), so a
/// number here is for output that test has already proved unchanged.
/// </summary>
/// <remarks>
/// <para>
/// 3.3.5a to 3.4.3, like <see cref="LegacyHandlerBenchmarks"/>. Each server arm is one received
/// legacy packet: parse, translate, serialize what the client is sent, then the per-packet outbox
/// tick. Each client arm is one received client packet: decode, translate, hand the legacy packet
/// to the sink.
/// </para>
/// <para>
/// Every arm goes through an entry point that does not name <c>MovementInfo</c>'s members, so
/// the same source measures the code before and after that type changes shape.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class MovementTranslationBenchmarks
{
    private LegacyHandlerHarness _harness = null!;
    private SessionContext _ctx;

    private byte[] _heartbeat = null!;
    private byte[] _heartbeatFallingOnTransport = null!;
    private byte[] _heartbeatOnSpline = null!;
    private byte[] _knockBack = null!;
    private byte[] _createCreature = null!;
    private byte[] _createGameObject = null!;
    private byte[] _createItem = null!;
    private ClientMoveScenario _playerMove = null!;
    private ClientMoveScenario _playerMoveFallingOnTransport = null!;
    private ClientPlayerMovement _decoded;

    private Action<WorldPacket> _handleMovement = null!;
    private Action<WorldPacket> _handleKnockBack = null!;
    private Action<WorldPacket> _handleUpdateObject = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;

        _harness = new LegacyHandlerHarness(recordClientPackets: false);
        _harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        _harness.AddKnownObject(MovementScenarios.OtherPlayer, ObjectType.Player);
        _ctx = new SessionContext(_harness.Session, socket: null, _harness.Client);
        var gameState = _harness.Session.GameState;

        _heartbeat = MovementScenarios.Heartbeat(MovementScenarios.Forward);
        _heartbeatFallingOnTransport = MovementScenarios.Heartbeat(MovementScenarios.FallingOnBoat);
        _heartbeatOnSpline = MovementScenarios.Heartbeat(MovementScenarios.ForwardOnSpline);
        _knockBack = MovementScenarios.KnockBack(MovementScenarios.FallingOnBoat);
        _createCreature = LegacyCreateWire.Build(MovementScenarios.CreatureCreate);
        _createGameObject = LegacyCreateWire.Build(MovementScenarios.ChestCreate);
        _createItem = LegacyCreateWire.Build(MovementScenarios.ItemCreate);

        _playerMove = new("forward", Opcode.CMSG_MOVE_HEARTBEAT, ClientMoveKind.PlayerMove,
            MovementScenarios.PlayerMove(MovementScenarios.ClientForward, gameState));
        _playerMoveFallingOnTransport = new("falling-on-boat", Opcode.CMSG_MOVE_HEARTBEAT, ClientMoveKind.PlayerMove,
            MovementScenarios.PlayerMove(MovementScenarios.ClientFallingOnBoat(gameState), gameState));

        var reader = new SpanPacketReader(_playerMoveFallingOnTransport.Framed.AsSpan(2));
        ClientPlayerMovementCodec.Read(ref reader, out _decoded);

        _handleMovement = _harness.Client.HandleMovementMessages;
        _handleKnockBack = _harness.Client.HandleMoveKnockBack;
        _handleUpdateObject = _harness.Client.HandleUpdateObject;

        // Each arm must reach its sink, or it is measuring an early return.
        Expect(ServerHeartbeat, client: 1, server: 0);
        Expect(ServerHeartbeat_FallingOnTransport, client: 1, server: 0);
        Expect(ServerHeartbeat_SplineDriven, client: 0, server: 0);
        Expect(ServerKnockBack, client: 1, server: 0);
        Expect(ClientPlayerMove, client: 0, server: 1);
        Expect(ClientPlayerMove_FallingOnTransport, client: 0, server: 1);

        // A create may bring other packets with it (an item's first one sets up currencies), so
        // these only have to reach the client at all.
        foreach (Action create in (Action[])[ServerCreateCreature, ServerCreateGameObject, ServerCreateItem])
        {
            int before = _harness.ClientWire.Count;
            create();
            if (_harness.ClientWire.Count == before)
                throw new InvalidOperationException($"{create.Method.Name} was not forwarded to the client.");
        }
    }

    private void Expect(Action arm, int client, int server)
    {
        int clientBefore = _harness.ClientWire.Count;
        int serverBefore = _harness.ServerWire.Count;
        arm();
        if (_harness.ClientWire.Count - clientBefore != client || _harness.ServerWire.Count - serverBefore != server)
            throw new InvalidOperationException($"{arm.Method.Name} sent {_harness.ClientWire.Count - clientBefore} client and " +
                $"{_harness.ServerWire.Count - serverBefore} server packets; expected {client} and {server}.");
    }

    /// <summary>Another player running forward: the commonest movement packet there is.</summary>
    [Benchmark(Baseline = true)]
    public void ServerHeartbeat()
        => _harness.Deliver(Opcode.MSG_MOVE_HEARTBEAT, _heartbeat, _handleMovement);

    /// <summary>The same with a transport block and a jump block, the longest a movement block gets.</summary>
    [Benchmark]
    public void ServerHeartbeat_FallingOnTransport()
        => _harness.Deliver(Opcode.MSG_MOVE_HEARTBEAT, _heartbeatFallingOnTransport, _handleMovement);

    /// <summary>A playerbot heartbeat mid-spline, which a 3.4.3 client must not be sent (issue #339).</summary>
    [Benchmark]
    public void ServerHeartbeat_SplineDriven()
        => _harness.Deliver(Opcode.MSG_MOVE_HEARTBEAT, _heartbeatOnSpline, _handleMovement);

    [Benchmark]
    public void ServerKnockBack()
        => _harness.Deliver(Opcode.MSG_MOVE_KNOCK_BACK, _knockBack, _handleKnockBack);

    /// <summary>A creature's create: the movement block with its speeds, then a short values block.</summary>
    [Benchmark]
    public void ServerCreateCreature()
        => _harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, _createCreature, _handleUpdateObject);

    /// <summary>A game object's create: stationary position and rotation.</summary>
    [Benchmark]
    public void ServerCreateGameObject()
        => _harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, _createGameObject, _handleUpdateObject);

    /// <summary>An item's create: no position, so no movement block. The create every login sends by the dozen.</summary>
    [Benchmark]
    public void ServerCreateItem()
        => _harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, _createItem, _handleUpdateObject);

    /// <summary>The player's own heartbeat, decoded and re-encoded for the legacy server.</summary>
    [Benchmark]
    public void ClientPlayerMove()
        => MovementScenarios.Dispatch(_playerMove, in _ctx);

    [Benchmark]
    public void ClientPlayerMove_FallingOnTransport()
        => MovementScenarios.Dispatch(_playerMoveFallingOnTransport, in _ctx);

    /// <summary>The modern movement read alone, without the handler or the legacy write.</summary>
    [Benchmark]
    public int ClientMovementRead()
    {
        var reader = new SpanPacketReader(_playerMoveFallingOnTransport.Framed.AsSpan(2));
        ClientPlayerMovementCodec.Read(ref reader, out var movement);
        return reader.Remaining + (int)movement.Guid.Low;
    }

    /// <summary>The modern movement write alone: a MoveUpdate serialized as the socket would.</summary>
    [Benchmark]
    public int MoveUpdateWrite()
    {
        var update = new MoveUpdate { MoverGUID = _decoded.Guid, MoveInfo = _decoded.MoveInfo };
        update.WritePacketData();
        int length = update.GetDataSpan().Length;
        update.ReleaseData();
        return length;
    }
}
