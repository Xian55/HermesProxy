# World/Server/Packets — outbound server packets

`ServerPacket` classes, written to the modern client. Inbound readers live next door in
[Codecs/](Codecs/CLAUDE.md). A field written in the wrong place compiles, passes review and makes
the client drop the packet without a word, so treat every edit here as a wire-format change.

## Packets about an object

A packet about one object overrides `NeedsObject`, so the client outbox can hold it until the client
has that object (see [Outbox/CLAUDE.md](../../Outbox/CLAUDE.md)):

```csharp
public WowGuid128 MoverGUID;
public override bool NeedsObject(WowGuid128 guid) => guid == MoverGUID;
```

Declare it for a guid the packet acts on, not one it mentions: a whisper or a guild roster can name an
object the client will never have, and holding those would wait for nothing.
`PacketsNeedingAnObjectTests` fails for a packet with a `MoverGUID` or `CasterUnit` field that has no
override.

## Build-dependent layouts

When a packet's layout differs between client builds, give each shape **its own sealed layout
type** and list them with their build ranges. Don't add an `if (ModernVersion.Build == …)` inside
`Write()` or `WriteToSpan()`:

```csharp
sealed class GossipPOI : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<GossipPOI>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new RetailLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.Zero, new FlatLayout()));

    private static readonly ServerPacketLayout<GossipPOI> Layout = Layouts.ForRunningClient();

    public override void Write() => Layout.Write(this, _worldPacket);
    public int MaxSize => Layout.MaxSize;
    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    internal sealed class RetailLayout : ServerPacketLayout<GossipPOI> { … }
    internal sealed class FlatLayout : ServerPacketLayout<GossipPOI> { … }
}
```

- **Picked once.** The static initializer resolves the running client's layout; per packet that
  costs a field load and a call. Call sites keep constructing the packet class.
- **Ranges** are [`AddedIn`, `RemovedIn`), compared by expansion.major.minor like `[PacketCodec]`.
  `Zero` leaves a side open. A layout added in 3.4.3 therefore also serves 4.4.x until a 4.4.x
  layout takes over: end the old range and start the new one at the same build.
- **Sealed is enforced.** `HPSG009` fails the build for a class deriving from
  `ServerPacketLayout<>` that is not sealed. Code two layouts share goes in a static helper, not a
  base class.
- **Coverage is tested.** `ServerPacketLayoutTests` finds every `ServerPacketLayouts<>` field and
  checks that each supported build, plus 4.4.2, gets exactly one layout. A gap or an overlap would
  otherwise be a type initializer exception on the first packet of that kind, in the field.
- **Prove the move.** When converting a packet that branched, keep the old branches as a frozen
  oracle in a test and assert both serialisers of every layout against it (`GossipPOILayoutTests`).
  Remember the coverage report: `docs/opcode-coverage.md` counts the equality sites and the test
  regenerates it.
