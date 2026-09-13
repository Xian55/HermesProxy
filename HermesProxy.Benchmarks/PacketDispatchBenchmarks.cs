using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Client;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Benchmarks;

// Inbound modern-client dispatch, from framed bytes to a populated packet object.
//
// *_Activator reproduces WorldSocket.PacketHandler.Invoke as it ran before the dispatch
// migration: Activator.CreateInstance over the packet type (boxes the argument array), Read(),
// then a closure delegate hop with a downcast. *_Direct drops the reflection so what remains is
// the ClientPacket + WorldPacket objects and the parse itself. *_Codec is the production path
// today for a converted packet: the real codec over a SpanPacketReader, no packet object, no
// WorldPacket.
//
// Once a packet converts, its ClientPacket class is gone from HermesProxy — so the *_Activator
// and *_Direct arms for converted packets run against frozen copies kept in this file. Deleting
// those arms instead would silently retire the comparison at exactly the moment it starts being
// worth reporting; keeping them means every slice can still ship a measured before/after.
//
// The handler target is a dummy object because the real handlers are instance methods on
// WorldSocket and need a live session; the delegate shape is what costs, not the target.
[MemoryDiagnoser]
[ShortRunJob]
public class PacketDispatchBenchmarks
{
    private static readonly object HandlerTarget = new();
    private static uint s_sink;

    // Frozen pre-migration copies of the converted packets, so the baseline arms keep measuring
    // the path this work replaces. Do not edit to match the codecs — the point is that they do
    // not change.
    private sealed class FrozenBuyBackItem : ClientPacket
    {
        public FrozenBuyBackItem(WorldPacket packet) : base(packet) { }
        public override void Read()
        {
            VendorGUID = _worldPacket.ReadPackedGuid128();
            Slot = _worldPacket.ReadUInt32();
        }
        public WowGuid128 VendorGUID;
        public uint Slot;
    }

    private sealed class FrozenSetActionButton : ClientPacket
    {
        public FrozenSetActionButton(WorldPacket packet) : base(packet) { }
        public override void Read()
        {
            Action = _worldPacket.ReadUInt16();
            Type = _worldPacket.ReadUInt16();
            Index = _worldPacket.ReadUInt8();
        }
        public ushort Action;
        public ushort Type;
        public byte Index;
    }

    private sealed class FrozenChatMessageWhisper : ClientPacket
    {
        public FrozenChatMessageWhisper(WorldPacket packet) : base(packet) { }
        public override void Read()
        {
            Language = _worldPacket.ReadUInt32();
            uint targetLen = _worldPacket.ReadBits<uint>(9);
            uint textLen = _worldPacket.ReadBits<uint>(11);
            Target = _worldPacket.ReadString(targetLen);
            Text = _worldPacket.ReadString(textLen);
        }
        public uint Language;
        public string Target = string.Empty;
        public string Text = string.Empty;
    }

    private sealed class FrozenAttackSwing : ClientPacket
    {
        public FrozenAttackSwing(WorldPacket packet) : base(packet) { }
        public override void Read()
        {
            Victim = _worldPacket.ReadPackedGuid128();
        }
        public WowGuid128 Victim;
    }

    /// <summary>
    /// Frozen verbatim from <c>HotfixPackets.cs</c> before conversion. The only benchmarked packet
    /// with a count-driven loop: the old reader grew its list from empty, the codec pre-sizes it
    /// against what the wire can actually hold, and nothing else here exercises that difference.
    /// </summary>
    private sealed class FrozenDBQueryBulk : ClientPacket
    {
        public FrozenDBQueryBulk(WorldPacket packet) : base(packet) { }
        public override void Read()
        {
            TableHash = (DB2Hash)_worldPacket.ReadUInt32();

            uint count = _worldPacket.ReadBits<uint>(13);
            for (uint i = 0; i < count; ++i)
            {
                Queries.Add(_worldPacket.ReadUInt32());
            }
        }
        public DB2Hash TableHash;
        public List<uint> Queries = new();
    }

    private byte[] _buyBackItem = null!;
    private byte[] _setActionButton = null!;
    private byte[] _attackSwing = null!;
    private byte[] _whisper = null!;
    private byte[] _dbQueryBulk = null!;

    private Action<object, ClientPacket> _buyBackItemHandler = null!;
    private Action<object, ClientPacket> _setActionButtonHandler = null!;
    private Action<object, ClientPacket> _attackSwingHandler = null!;
    private Action<object, ClientPacket> _whisperHandler = null!;
    private Action<object, ClientPacket> _dbQueryBulkHandler = null!;
    private FrozenDictionary<Opcode, Action<WorldPacket>> _legacyHandlers = null!;
    private WorldPacket _legacyPacket = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;

        var vendor = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 77);

        _buyBackItem = Frame(w => { w.WritePackedGuid128(vendor); w.WriteUInt32(3); });
        _setActionButton = Frame(w => { w.WriteUInt16(1234); w.WriteUInt16(0); w.WriteUInt8(12); });
        _attackSwing = Frame(w => w.WritePackedGuid128(vendor));
        _whisper = Frame(w =>
        {
            w.WriteUInt32(7);
            w.WriteBits(6, 9);
            w.WriteBits(21, 11);
            w.WriteString("Thrall");
            w.WriteString("hello from the proxy!");
        });

        // 40 ids: what a login-time bulk query actually looks like, and enough that the
        // pre-size-vs-grow difference is not lost in noise.
        _dbQueryBulk = Frame(w =>
        {
            w.WriteUInt32((uint)DB2Hash.BroadcastText);
            w.WriteBits(40u, 13);
            for (uint i = 0; i < 40; i++)
                w.WriteUInt32(1000 + i);
        });

        _buyBackItemHandler = Wrap<FrozenBuyBackItem>(static (_, p) => s_sink = p.Slot);
        _setActionButtonHandler = Wrap<FrozenSetActionButton>(static (_, p) => s_sink = p.Action);
        _attackSwingHandler = Wrap<FrozenAttackSwing>(static (_, p) => s_sink = (uint)p.Victim.Low);
        _whisperHandler = Wrap<FrozenChatMessageWhisper>(static (_, p) => s_sink = (uint)(p.Text.Length + p.Target.Length));
        _dbQueryBulkHandler = Wrap<FrozenDBQueryBulk>(static (_, p) => s_sink = (uint)p.Queries.Count);

        // Legacy-side fixtures. Phase A left handler bodies untouched and changed only how the
        // handler is found and called, so that is all these measure: a FrozenDictionary hash plus
        // a closed-delegate invoke against a table index plus an indirect call. Both targets are
        // no-ops, because the work either side of the call is identical by construction.
        _legacyPacket = new WorldPacket(_attackSwing);
        _legacyHandlers = new Dictionary<Opcode, Action<WorldPacket>>
        {
            [Opcode.SMSG_ATTACK_START] = static _ => s_sink++,
        }.ToFrozenDictionary();

        // Fail loudly if the span parse disagrees with the ByteBuffer parse; a wrong floor
        // is worse than no floor.
        using (var reference = new FrozenChatMessageWhisper(new WorldPacket(_whisper)))
        {
            reference.Read();
            var r = new SpanPacketReader(_whisper.AsSpan(2));
            ChatMessageWhisperCodecWotLKClassic.Read(ref r, out var actual);
            if (actual.Target != reference.Target || actual.Text != reference.Text)
                throw new InvalidOperationException($"Codec parse mismatch: '{actual.Target}'/'{actual.Text}' vs '{reference.Target}'/'{reference.Text}'");
        }
    }

    // Same closure shape as WorldSocket.PacketHandler.CreateDelegate<P1>.
    private static Action<object, ClientPacket> Wrap<P1>(Action<object, P1> typed) where P1 : ClientPacket
        => (target, p) => typed(target, (P1)p);

    // WorldPacket(byte[]) consumes a 2-byte opcode prefix before the body, exactly like the
    // buffer WorldSocket.ReadData hands to PacketHandler.Invoke.
    private static byte[] Frame(Action<WorldPacket> body)
    {
        using var payload = new WorldPacket(1u);
        body(payload);
        byte[] data = payload.GetData();
        var framed = new byte[data.Length + 2];
        data.CopyTo(framed, 2);
        return framed;
    }

    private static uint InvokeViaActivator(Type packetType, byte[] frame, Action<object, ClientPacket> handler)
    {
        var worldPacket = new WorldPacket(frame);
        using var clientPacket = (ClientPacket)Activator.CreateInstance(packetType, worldPacket)!;
        clientPacket.Read();
        handler(HandlerTarget, clientPacket);
        return s_sink;
    }

    // ---- BuyBackItem: packed GUID + uint ----

    [Benchmark(Baseline = true)]
    public uint BuyBackItem_Activator() => InvokeViaActivator(typeof(FrozenBuyBackItem), _buyBackItem, _buyBackItemHandler);

    [Benchmark]
    public uint BuyBackItem_Direct()
    {
        using var packet = new FrozenBuyBackItem(new WorldPacket(_buyBackItem));
        packet.Read();
        _buyBackItemHandler(HandlerTarget, packet);
        return s_sink;
    }

    /// The production path for this packet since the item slice: the real codec, not a
    /// hand-written approximation of one.
    [Benchmark]
    public uint BuyBackItem_Codec()
    {
        var r = new SpanPacketReader(_buyBackItem.AsSpan(2));
        BuyBackItemCodec.Read(ref r, out var packet);
        s_sink = packet.Slot + (uint)packet.VendorGUID.Low;
        return s_sink;
    }

    /// <summary>
    /// What production actually executes: the generated table's bounds check, load and indirect
    /// call, on top of the same codec work <see cref="BuyBackItem_Codec"/> measures.
    /// </summary>
    /// <remarks>
    /// The lookup is the real one - <c>GeneratedCmsgDispatch.Get</c> against the real table - but
    /// the call lands on a local thunk of identical signature rather than the generated one,
    /// because the generated thunk goes on to invoke the system handler and that needs a live
    /// session and sockets. The excluded part is exactly what <see cref="BuyBackItem_Codec"/>
    /// excludes too, so the pair is comparable and the gap between them is the dispatch overhead
    /// this design was chosen for. If that gap is not close to zero, the thunk is not inlining and
    /// that is a finding in itself.
    /// </remarks>
    [Benchmark]
    public unsafe uint BuyBackItem_Generated()
    {
        var fn = GeneratedCmsgDispatch.Get(Opcode.CMSG_BUY_BACK_ITEM);
        if (fn == null)
            throw new InvalidOperationException("CMSG_BUY_BACK_ITEM is not in the generated table.");

        var r = new SpanPacketReader(_buyBackItem.AsSpan(2));
        // Through a function pointer, not by name - an inlined direct call would measure the
        // wrong thing. The target is the local thunk rather than fn for the reason in the remarks.
        delegate*<ref SpanPacketReader, in SessionContext, void> call = &BuyBackItemThunk;
        call(ref r, in s_ctx);
        return s_sink;
    }

    private static SessionContext s_ctx;

    private static void BuyBackItemThunk(ref SpanPacketReader r, in SessionContext ctx)
    {
        BuyBackItemCodec.Read(ref r, out var packet);
        s_sink = packet.Slot + (uint)packet.VendorGUID.Low;
    }

    // ---- SetActionButton: three small integers ----

    [Benchmark]
    public uint SetActionButton_Activator() => InvokeViaActivator(typeof(FrozenSetActionButton), _setActionButton, _setActionButtonHandler);

    [Benchmark]
    public uint SetActionButton_Direct()
    {
        using var packet = new FrozenSetActionButton(new WorldPacket(_setActionButton));
        packet.Read();
        _setActionButtonHandler(HandlerTarget, packet);
        return s_sink;
    }

    /// The production path for this packet since the character slice: the real codec, not a
    /// hand-written approximation of one.
    [Benchmark]
    public uint SetActionButton_Codec()
    {
        var r = new SpanPacketReader(_setActionButton.AsSpan(2));
        SetActionButtonCodec.Read(ref r, out var packet);
        s_sink = (uint)(packet.Action + packet.Type + packet.Index);
        return s_sink;
    }

    // ---- AttackSwing: single packed GUID, the most frequent combat CMSG ----

    [Benchmark]
    public uint AttackSwing_Activator() => InvokeViaActivator(typeof(FrozenAttackSwing), _attackSwing, _attackSwingHandler);

    [Benchmark]
    public uint AttackSwing_Direct()
    {
        using var packet = new FrozenAttackSwing(new WorldPacket(_attackSwing));
        packet.Read();
        _attackSwingHandler(HandlerTarget, packet);
        return s_sink;
    }

    /// <inheritdoc cref="BuyBackItem_Codec"/>
    [Benchmark]
    public uint AttackSwing_Codec()
    {
        var r = new SpanPacketReader(_attackSwing.AsSpan(2));
        AttackSwingCodec.Read(ref r, out var packet);
        s_sink = (uint)packet.Victim.Low;
        return s_sink;
    }

    // ---- ChatMessageWhisper: bit-packed lengths + two strings (strings must allocate) ----

    [Benchmark]
    public uint Whisper_Activator() => InvokeViaActivator(typeof(FrozenChatMessageWhisper), _whisper, _whisperHandler);

    [Benchmark]
    public uint Whisper_Direct()
    {
        using var packet = new FrozenChatMessageWhisper(new WorldPacket(_whisper));
        packet.Read();
        _whisperHandler(HandlerTarget, packet);
        return s_sink;
    }

    /// <summary>
    /// The production path since the chat slice. The two strings are the floor — they are handed
    /// to SendMessageChat* as strings, so no amount of span work in the codec removes them.
    /// </summary>
    /// <remarks>
    /// Reads the payload span directly, exactly as <see cref="BuyBackItem_Codec"/> does, rather
    /// than building a <c>WorldPacket</c> to call <c>GetRemainingSpan()</c> on. This used to do
    /// the latter and never dispose it, which measured something entirely different:
    /// <c>ByteBuffer</c> has a finalizer, so an undisposed one is queued for finalization,
    /// survives Gen0, and is promoted — the arm reported 217 ns with Gen1 and Gen2 collections no
    /// other arm showed, and read as the converted path being slower than the reflection it
    /// replaced. Production disposes in <c>HandleGeneratedPacket</c>'s finally, so it never paid
    /// that. All *_Codec arms exclude the WorldPacket because production already has one by the
    /// time dispatch runs; it is not work the conversion added or removed.
    /// </remarks>
    [Benchmark]
    public uint Whisper_Codec()
    {
        var r = new SpanPacketReader(_whisper.AsSpan(2));
        ChatMessageWhisperCodecWotLKClassic.Read(ref r, out var packet);
        s_sink = (uint)(packet.Text.Length + packet.Target.Length);
        return s_sink;
    }

    private uint Whisper_SpanUnused()
    {
        var r = new SpanPacketReader(_whisper.AsSpan(2));
        r.ReadUInt32();
        int targetLen = (int)r.ReadBits<uint>(9);
        int textLen = (int)r.ReadBits<uint>(11);
        string target = r.ReadString(targetLen);
        string text = r.ReadString(textLen);
        s_sink = (uint)(text.Length + target.Length);
        return s_sink;
    }
    // ---- DBQueryBulk: a 13-bit count driving a uint32 loop ----

    [Benchmark]
    public uint DBQueryBulk_Activator() => InvokeViaActivator(typeof(FrozenDBQueryBulk), _dbQueryBulk, _dbQueryBulkHandler);

    [Benchmark]
    public uint DBQueryBulk_Direct()
    {
        using var packet = new FrozenDBQueryBulk(new WorldPacket(_dbQueryBulk));
        packet.Read();
        _dbQueryBulkHandler(HandlerTarget, packet);
        return s_sink;
    }

    /// <summary>
    /// The list is the floor here, the same way the two strings are for Whisper: the handler takes
    /// a <c>List&lt;uint&gt;</c>, so the codec cannot get to zero. What it can do is pre-size the
    /// list against what the wire can actually hold instead of growing it from empty, which is the
    /// difference this pair is here to show.
    /// </summary>
    [Benchmark]
    public uint DBQueryBulk_Codec()
    {
        var r = new SpanPacketReader(_dbQueryBulk.AsSpan(2));
        DBQueryBulkCodec.Read(ref r, out var packet);
        s_sink = (uint)packet.Queries.Count;
        return s_sink;
    }

    // ---- Whisper through the generated table ----

    /// <summary>
    /// A second <c>_Generated</c> arm, so the dispatch overhead figure does not rest on
    /// <see cref="BuyBackItem_Generated"/> alone. Same construction: the real lookup, then an
    /// indirect call to a local thunk of identical signature.
    /// </summary>
    [Benchmark]
    public unsafe uint Whisper_Generated()
    {
        var fn = GeneratedCmsgDispatch.Get(Opcode.CMSG_CHAT_MESSAGE_WHISPER);
        if (fn == null)
            throw new InvalidOperationException("CMSG_CHAT_MESSAGE_WHISPER is not in the generated table.");

        var r = new SpanPacketReader(_whisper.AsSpan(2));
        delegate*<ref SpanPacketReader, in SessionContext, void> call = &WhisperThunk;
        call(ref r, in s_ctx);
        return s_sink;
    }

    private static void WhisperThunk(ref SpanPacketReader r, in SessionContext ctx)
    {
        ChatMessageWhisperCodecWotLKClassic.Read(ref r, out var packet);
        s_sink = (uint)(packet.Text.Length + packet.Target.Length);
    }

    // ---- Legacy (SMSG) dispatch mechanism ----

    /// <summary>
    /// What legacy Phase A replaced: a <c>FrozenDictionary</c> hash plus a closed-delegate invoke,
    /// per packet, per WorldClient.
    /// </summary>
    /// <remarks>
    /// Phase A moved 443 handlers to the generated table without touching a single handler body,
    /// so the only thing that changed is how the handler is found and called — and that is all
    /// this pair measures. Both targets are no-ops on purpose: the parse and translate either side
    /// of the call are identical by construction, so including them would bury the difference
    /// under work that did not change. The claim being tested is that the legacy conversion is
    /// close to free per packet; this is the arm that can refute it.
    /// </remarks>
    [Benchmark]
    public uint LegacySmsg_Dictionary()
    {
        if (_legacyHandlers.TryGetValue(Opcode.SMSG_ATTACK_START, out var handler))
            handler(_legacyPacket);
        return s_sink;
    }

    /// <summary>The generated legacy table: one bounds check, one load, one indirect call.</summary>
    [Benchmark]
    public unsafe uint LegacySmsg_Table()
    {
        // The real lookup against the real table, so both arms pay for finding the handler.
        // Calling only the local thunk after that would measure a bare call against a hash.
        var fn = GeneratedSmsgDispatch.Get(Opcode.SMSG_ATTACK_START);
        if (fn == null)
            throw new InvalidOperationException("SMSG_ATTACK_START is not in the generated table.");

        delegate*<WorldClient, WorldPacket, void> call = &LegacySmsgThunk;
        call(null!, _legacyPacket);
        return s_sink;
    }

    // Null target is deliberate and safe: the thunk never dereferences the client, and the point
    // is the call shape, not the callee. The real generated thunk invokes an instance handler that
    // needs a live session and sockets.
    private static void LegacySmsgThunk(WorldClient client, WorldPacket packet) => s_sink++;
}
