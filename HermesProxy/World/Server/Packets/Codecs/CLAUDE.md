# World/Server/Packets/Codecs — inbound CMSG readers

Bytes in, packet out, nothing else. Each codec turns a modern-client payload into the
`readonly record struct` its system takes. The generated dispatch table calls it before the handler
in `World/Server/Systems`. See [World/Dispatch/CLAUDE.md](../../../Dispatch/CLAUDE.md) for how the
two are wired, and the root [CLAUDE.md](../../../../../CLAUDE.md) for solution-wide conventions.

A codec that reads the right fields in the wrong order compiles, passes review, and "works" until
a value happens to be non-zero. The client or the legacy server then quietly does the wrong thing,
and nothing throws. Treat every edit here as a wire-format change.

## Layout

- One file per domain, `<Domain>Codecs.cs`, mirroring `Systems/<Domain>System.cs`.
  `InboundCodecs.cs` and `FinalBatchCodecs.cs` hold opcodes that were converted in slices outside
  the domain files.
- The packet record structs live next door, in `World/Server/Packets/<Domain>Packets.cs`.
- Codecs are hand-written, not generated. The old `Read()` bodies carried version branches and
  nested reads that no attribute vocabulary infers; the generator does dispatch only.

## The shape

```csharp
public static class FooCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]   // small, fixed-size reads
    public static void Read(ref SpanPacketReader r, out Foo packet)
    {
        WowGuid128 target = r.ReadPackedGuid128();          // one local per field, in wire order
        uint slot = r.ReadUInt32();
        packet = new Foo(target, slot);
    }
}
```

- **The namespace is `HermesProxy.World.Server.Packets`, not `…Codecs`.** The generator finds the
  codec for `N.Foo` as `N.FooCodec`, so it must share the packet's namespace. The IDE's
  "namespace does not match folder" hint is expected here; do not act on it.
- **Read each field into a local, then construct.** The reader is stateful, so wire order must be
  statement order. Don't rely on the evaluation order of constructor arguments.
- **`out`, not a return value**, for every codec. Some packets carry a ~180-byte `MovementInfo`
  where return-value optimisation is not guaranteed, and one shape for all is better than
  deciding per packet.
- **Defaults matter.** A positional record struct defaults to all zero. If the `ClientPacket` it
  replaced had an initializer (`Animate = true`, `WowGuid128.Empty`) and some path skips that read,
  the codec must set the default explicitly.
- A read that `SpanPacketReader` lacks (`ReadGuid`, `ReadPackedGuid`, `ReadEntry`, …) comes from
  `World/Dispatch/SpanPacketReaderExtensions.cs`.

## Build-dependent layouts

When a packet's layout differs between client builds, write **two codecs and a range**. Don't add
an `if (ModernVersion.Build == …)` inside one reader:

```csharp
[PacketCodec(typeof(Foo), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class FooCodecWotLKClassic { … }

[PacketCodec(typeof(Foo), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class FooCodecPreWotLKClassic { … }
```

The selection happens once, when the table is built. An equality branch sends a build the code has
never seen down the `else` path, silently. Once any `[PacketCodec]` exists for a packet, the
`FooCodec` naming convention no longer applies to it, so rename the original rather than leaving it
beside the pair.

Known 3.4.3 pattern: party and raid packets moved `HasPartyIndex` to a leading bit plus an optional
byte, so the two layouts differ from the first byte. `ChangeSubGroup` is the exception, with the bit
after its fixed fields. Check every packet individually.

**Where layouts come from.** WowPacketParser is the usual reference, but several of its 3.4
parsers are registered at 3.4.4 (59817) and do not hold for 54261 (`ReadyCheckResponseClient` is a
documented case). A native 3.4.3 server source or a client capture settles it. Leave a comment in
the codec when a reference was wrong.

## Bits

- Byte-level reads (`ReadUInt8`, `ReadPackedGuid128`, …) reset the bit position, so a bit read after
  them starts a fresh byte. This matches TrinityCore's `ByteBuffer`.
- Call `r.ResetBitReader()` where the wire pads between two bit sections. Without it the second
  section consumes leftover cached bits, and the byte stream falls one byte behind.

## Wire-supplied counts

A count read from the packet is untrusted. Pre-sizing on it hands a corrupt count a
multi-gigabyte allocation before the first element read can fail.

- **`List<T>` capacity:** clamp with `CodecHelpers.WireCountCapacity(count, in r, minElementBytes)`.
  Capacity is only a hint, so clamping changes no result.
- **Arrays** are the packet field itself, so clamping would silently drop elements. Validate and
  throw instead.

`CodecMalformedCountTests` pins both. Any new counted read needs a case there.

## Empty payloads

Opcodes with no body take `EmptyClientPacket`. Its codec logs at Debug when bytes are left over:
that means our layout belief is wrong. It must never assert, because a `Trace.Assert` is compiled
into Release and once aborted the whole proxy over one client packet.

## Proving a codec

Every codec added or changed gets a case in
`HermesProxy.Tests/World/Dispatch/<Domain>CodecEquivalenceTests.cs`: field values **and** final
reader position, over a reader built the way production builds it. See that folder's
[CLAUDE.md](../../../../../HermesProxy.Tests/World/Dispatch/CLAUDE.md).
