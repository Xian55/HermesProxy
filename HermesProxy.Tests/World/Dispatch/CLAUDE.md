# HermesProxy.Tests/World/Dispatch — inbound dispatch gates

Tests for the generated CMSG/SMSG dispatch and the codecs behind it. A wire regression on this
path doesn't throw; the client or legacy server silently does the wrong thing. These tests are
most of what stands between a codec edit and a playtest. See the project
[CLAUDE.md](../../CLAUDE.md) for runner conventions, and
[World/Dispatch/CLAUDE.md](../../../HermesProxy/World/Dispatch/CLAUDE.md) for the production side.

| File | Gate |
|---|---|
| `<Domain>CodecEquivalenceTests.cs` | each codec reads what the `ClientPacket.Read()` it replaced read: fields **and** final position |
| `CodecEquivalenceTests.cs` | the first slice's codecs, and the canonical helpers |
| `CodecMalformedCountTests.cs` | wire-supplied counts never drive an allocation |
| `DispatchRegistryTests.cs` | every claimed opcode resolves to a thunk; every system has a legal shape |
| `ShapeBOpcodeForwardingTests.cs` | opcodes sharing one shape-B body translate to distinct, correct legacy values |
| `SpanReaderParityTests.cs` | each `SpanPacketReader` primitive matches its `ByteBuffer` counterpart |
| `MovementReaderEquivalenceTests.cs` | the `WorldPacket` and span `ReadMovementInfoModern` copies agree |
| `OpcodeCoverageReportTests.cs` | renders `docs/opcode-coverage.md` from the attributes |
| `Reference/` | frozen oracles and the two port scripts |

The generated-table snapshots are not in this folder. They live in
`../../SourceGen/PacketDispatchGeneratorTests.*.verified.txt`.

## Writing a codec test

Copy the helpers from the domain file you are extending. They are deliberately identical:

```csharp
// Frame like the wire: WorldPacket's read-mode ctor consumes a 2-byte opcode prefix.
var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt8(3); });

var e = new Frozen.Foo(); e.Read(o);                  // oracle
var r = ReaderOver(f); FooCodec.Read(ref r, out var a); // production codec
Assert.Equal(e.Target, a.Target);
Assert.Equal(o.Remaining(), r.Remaining);             // position, not just fields
```

- **`ReaderOver` must go through `new WorldPacket(framed).GetRemainingSpan()`**, the accessor the
  dispatch site uses. Never write `framed.AsSpan(2)`. A helper that hardcodes the offset agrees
  with itself instead of production. That is how every converted packet once parsed two bytes
  early while the whole suite passed.
- **Assert the final position.** A codec that agrees on every field of one fixture but consumes a
  different byte count desynchronises everything after it in a real stream.
- **Cover every optional branch.** Each flag bit gets a case with it set and a case with it
  clear.
- `SpanPacketReader` is a `ref struct` and cannot be captured by a lambda. For an expected throw,
  use `try`/`catch`, not `Assert.Throws(() => …)`.

## Build-ranged codecs

`ModernVersion` and `LegacyVersion` are `static readonly`, fixed once per test process:
`TestModuleInitializer` sets **`V1_14_2_42597` / `V3_3_5a_12340`**. The per-class static
constructors that assign a build only when it is `Zero` are leftovers and change nothing. You
cannot switch builds per test.

So a `[PacketCodec]` pair is proven two ways:

| Side | Proven against | Test name |
|---|---|---|
| pre-WotLK | the frozen oracle, whose own version branch takes the V1_14 path | `Foo_PreWotLK_MatchesOracle` |
| V3_4_3 | explicit byte layouts, ideally bytes captured from a real client | `Foo_WotLKClassic_<what it proves>` |

When a test needs a build-specific opcode value, use the overloads that take the build
explicitly: `Opcodes.GetUniversalOpcode(uint, build)` and
`Opcodes.GetOpcodeValueForVersion(name, build)`. The process-wide statics won't help.

## Reference/

| File | What |
|---|---|
| `FrozenPackets.g.cs` | `ClientPacket.Read()` bodies as they were just before conversion, lifted mechanically |
| `FrozenClientPackets.cs` | the first slice's oracles, copied by hand |
| `freeze-oracles.py` | lifts a `ClientPacket` class into an oracle |
| `verify-handler-port.py` | diffs a moved handler body against its original at a git ref |

- **Never edit an oracle**, not even to "fix" it. Changing one to match a codec turns its test into
  a tautology. If the oracle is genuinely wrong, the codec's test should say so explicitly, next to
  a byte-level case.
- Field initializers are part of the oracle. They are how a skipped read that defaulted to
  non-zero gets caught.
- **Freeze before converting**, while the `ClientPacket` class still exists:
  `python freeze-oracles.py Foo Bar`. Its output is already indented. Paste it inside
  `FrozenPackets` in `FrozenPackets.g.cs`, before the class's closing brace. Appending with `>>`
  lands it outside the class. A `NOT FOUND` on stderr means the class wasn't matched; don't
  hand-write that oracle.
- **After moving a handler body**, run `verify-handler-port.py` before playtesting:
  - `--original-ref` names a commit where the original file still existed;
  - `Orig=Ported` covers a renamed method, and `Name#2` picks the second overload;
  - the exit code is the mismatch count.

## Generated artefacts

- **`docs/opcode-coverage.md`:** `OpcodeCoverageReportTests` rewrites it when stale and fails once.
  Review the diff, rerun, and commit it with the change.
- **Dispatch snapshots:** they read the generator output from `HermesProxy/obj/Generated/`, so
  build `HermesProxy` first. Before accepting, check that
  `diff --strip-trailing-cr <verified> <received> | grep '^<'` shows only what you meant to
  remove, which for an addition is nothing. A removed thunk is a handler that no longer
  dispatches. Snapshots are UTF-8 with BOM and CRLF; edit the `.verified.txt` in place rather than
  moving `.received.txt` over it.
- Never run `dotnet test --no-build` after a failed build. It tests stale binaries and reports
  green.
