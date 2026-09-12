using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Codecs: bytes in, packet out, nothing else.
//
// One type per packet, found by the dispatch generator by convention — packet `N.Foo` means codec
// `N.FooCodec` — and verified to exist, so the convention fails at build time rather than silently.
// They are hand-converted from the ClientPacket.Read() bodies rather than generated, because those
// bodies carry version branches and nested reads that no attribute vocabulary infers; the generator
// does dispatch only.
//
// `out` rather than a return value: for a two-field struct it makes no difference, but packets like
// ClientPlayerMovement carry a ~180-byte MovementInfo where return-value optimisation is not
// guaranteed, and one shape for all of them is worth more than a per-packet judgement call.

public static class AttackSwingCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out AttackSwing packet)
        => packet = new AttackSwing(r.ReadPackedGuid128());
}

public static class AttackStopCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out AttackStop packet)
        => packet = default;
}

public static class SetSheathedCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetSheathed packet)
    {
        int sheathState = r.ReadInt32();
        bool animate = r.HasBit();
        packet = new SetSheathed(sheathState, animate);
    }
}

public static class BuyBackItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BuyBackItem packet)
    {
        // Read order is the wire order and must stay in it — the struct's parameter order is
        // incidental, but evaluation order of constructor arguments is not something to rely on
        // when the reader is stateful, so each field is read into a local first.
        WowGuid128 vendor = r.ReadPackedGuid128();
        uint slot = r.ReadUInt32();
        packet = new BuyBackItem(vendor, slot);
    }
}
