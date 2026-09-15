# World/Dispatch — inbound packet dispatch

The contract between hand-written packet code and the generated dispatch tables. Three files, and
the generator that reads them lives in `HermesProxy.SourceGen/PacketDispatchGenerator.cs`. See the
root [CLAUDE.md](../../../CLAUDE.md) for solution-wide conventions and
`docs/version-shape-dispatch.md` for why it is built this way.

| File | Holds |
|---|---|
| `DispatchAttributes.cs` | `[HandlesCmsg]`, `[HandlesSmsg]`, `[PacketCodec]` |
| `SessionContext.cs` | What a static system may reach: the session and its two sockets |
| `SpanPacketReaderExtensions.cs` | `WorldPacket` reads that `SpanPacketReader` cannot offer from Framework (GUIDs, entries) |

## How a packet reaches a handler

```
CMSG  WorldSocket.HandlePacket
        per-build opcode ──(name)──▶ universal Opcode
        GeneratedCmsgDispatch.Get(opcode)          null slot ⇒ "No handler for opcode", dropped
        thunk: XCodec.Read(ref reader, out X)      World/Server/Packets/Codecs
               XSystem.HandleX(in X, in ctx)       World/Server/Systems

SMSG  WorldClient
        GeneratedSmsgDispatch.Get(opcode)
        thunk: client.HandleX(WorldPacket)         World/Client/PacketHandlers, instance methods
```

The tables are `static readonly` arrays of function pointers indexed by universal opcode, built
once per process. Every inbound opcode goes through them; there is no reflective fallback any more.
The only CMSGs outside the table are the connection-level ones `WorldSocket.ReadData`
switches on first (ping, auth, keep-alive, encryption ack, connect-to), which still use
`ClientPacket`.

The SMSG side deliberately has no codec layer: its handlers read straight off the `WorldPacket`,
and converting one is only a matter of adding the attribute.

## The opcode name is the join key

A per-build `World/Enums/<build>/Opcode.cs` entry maps to the universal `Opcode` **by name**, and
the handler claims the universal name. If the two names differ, the packet is named in the log and
still reaches nothing. #290 was exactly that: 3.4.3 mapped 13903 as WowPacketParser's
`CMSG_CHANGE_SUB_GROUP` while the handler claimed `CMSG_GROUP_CHANGE_SUB_GROUP`. Before adding a
name to the universal enum, search `[HandlesCmsg(` and the other builds' `Opcode.cs` for a synonym.
An entry declared `= 0u` means "not mapped", not "opcode zero".

## Attributes

| Attribute | On | Range axis |
|---|---|---|
| `[HandlesCmsg(Opcode)]` | `public static` method in a `*System` class | modern (client build) |
| `[HandlesSmsg(Opcode)]` | `WorldClient` instance method | legacy (server build) |
| `[PacketCodec(typeof(T))]` | codec class | modern, or legacy with `AgainstLegacyVersion = true` |

- **`AddedIn` is inclusive, `RemovedIn` exclusive, `Zero` unbounded.** They are literal attribute
  data, never a call like `LegacyVersion.InVersion(...)`, because the generator reads them off the
  symbol and emits the selection as code. That code runs once, when the table is built.
- Both handler attributes allow several per method. One body can serve several opcodes.
- Per-build layout differences go in a **`[PacketCodec]` pair** (`FooCodecPreWotLKClassic` /
  `FooCodecWotLKClassic`), never an `if (ModernVersion.Build == …)` inside one `Read`. An equality
  branch sends a build it has never seen down the `else` path.

## Handler shapes

The generator accepts exactly two signatures (HPSG007 otherwise):

| Shape | Signature | Use when |
|---|---|---|
| A | `(in TPacket, in SessionContext)` | one opcode, one body |
| B | `(Opcode, in TPacket, in SessionContext)` | one body serves several opcodes, or needs the opcode for logging |

A shape-B body that forwards with `new WorldPacket(opcode)` leaves the universal → legacy opcode
table as the only thing deciding what goes on the wire. A wrong entry there sends a well-formed
packet that makes the server do something else. `ShapeBOpcodeForwardingTests` pins the contiguous
blocks where an off-by-one would still land on a real opcode.

## Generator diagnostics

| Id | Fires when |
|---|---|
| HPSG004 | two handlers claim one opcode over overlapping build ranges |
| HPSG005 | an unranged handler shadows a ranged one for the same opcode |
| HPSG006 | a system's packet type has no codec: no `[PacketCodec]`, and no `FooCodec` with `static void Read(ref SpanPacketReader, out Foo)` |
| HPSG007 | a system is not `public static`, or has neither shape above |

None of them can see a codec that reads the right fields in the wrong order. That is what
`HermesProxy.Tests/World/Dispatch` is for.

## SessionContext rules

- **One per session, passed `in`.** It holds references only, so passing it costs a pointer.
- **Forwarders keep the old instance-member names** (`GetSession()`, `SendPacketToServer`,
  `SendPacketToClient`, `SendPacket`), so moving a handler body only means prefixing calls with
  `ctx.` instead of rewriting them.
- **`ctx.ToClient` / `ctx.ToServer`** are the session's outboxes, for sending now or holding a
  packet until something happens. See [World/Outbox/CLAUDE.md](../Outbox/CLAUDE.md).
- **Resolve the world client per call** through the session. It attaches after the modern socket
  binds, so a reference captured at bind time is null for the whole login window.
- `ctx.Socket` is null on the legacy path. `ctx.IsBound` is false until the socket has bound a
  session.
- **`ModernVersion`, `LegacyVersion` and `GameData` stay static.** They are `static readonly`, so
  the JIT folds version checks into constants. Moving them onto the context would put a load and
  a branch on every hot-path check.

## Reader rules

- The dispatch site builds the reader from **`packet.GetRemainingSpan()`**. `GetDataSpan()` starts
  at index 0 and re-reads the 2-byte opcode the `WorldPacket` constructor already consumed, which
  shifts every field by two bytes with nothing thrown. `GetData()` returns the whole
  bucket-rounded pool rental (#248). The same rule applies to any test helper.
- `SpanPacketReaderExtensions` members each **mirror a `WorldPacket` member byte for byte** and
  carry its name. Add one only as a mirror of an existing read; a converted body must keep its
  meaning.

## Checking a dispatch change

1. Build. HPSG004–007 are errors.
2. Run the tests. `PacketDispatchGeneratorTests` snapshots both generated tables, and
   `OpcodeCoverageReportTests` regenerates `docs/opcode-coverage.md`. **The snapshot diff must
   contain no removed (`<`) lines** unless you meant to drop an opcode. Overwriting an existing
   `*System.cs` instead of merging into it deletes handlers, and the snapshot diff is the only thing
   that shows it.
3. Playtest the opcode. In the log, a `Received` with no matching `Sending` is the signature of a
   handler returning early on mis-parsed data.
