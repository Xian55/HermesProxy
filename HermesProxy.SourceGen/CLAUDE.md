# HermesProxy.SourceGen — handbook

Roslyn source generators. They emit code that lands directly on the wire, so a mistake here
does not throw — it produces a subtly malformed packet that the client silently drops or
mis-renders. Treat every change as a wire-format change.

## The four generators

| Generator | Emits | Driven by |
|---|---|---|
| `ObjectUpdateBuilderGenerator` | `WriteCreate{Section}Data` / `WriteUpdate{Section}Data` / `HasAny{Section}FieldSet` on the per-version `ObjectUpdateBuilder` | `[DescriptorSection]` enums in `World/Enums/<build>/*Field.cs` |
| `OpcodeTableGenerator` | Opcode lookup tables | per-version `Opcode.cs` enums |
| `UpdateFieldTableGenerator` | Legacy update-field tables | per-version update-field enums |
| `PacketDispatchGenerator` | `GeneratedCmsgDispatch` / `GeneratedSmsgDispatch` function-pointer tables | `[HandlesCmsg]` / `[HandlesSmsg]` / `[PacketCodec]` — see `HermesProxy/World/Dispatch/CLAUDE.md` |

This handbook covers the descriptor generator. The dispatch generator's contract, diagnostics
(HPSG004–007) and snapshot rules are in the `World/Dispatch` and `HermesProxy.Tests/World/Dispatch`
handbooks.

Generated output lands in `HermesProxy/obj/Generated/HermesProxy.SourceGen/...`. Read it when
debugging — it is the actual code that runs, and diffing it is usually faster than reasoning
about the attributes.

---

# Part 1 — How the descriptor generator is built

Read this before changing `ObjectUpdateBuilderGenerator.cs`. The rest of the file is easy to
edit and easy to break, and both come from the same property: it is a string emitter with no
type checking between the attribute you write and the code that comes out.

## The pipeline

Four stages, one per named group of methods in the file:

```
[Descriptor*] attribute        World/Enums/<build>/*Field.cs   — what you write
        │
        │  Read*(AttributeData)                                — parse: named args → values
        ▼
  *Entry record                                                — the generator's own model
        │
        │  Emit*(StringBuilder, …)                             — print C# as text
        ▼
  *.g.cs                                                       — what actually runs
```

* **`BuildModel`** walks every `V*` namespace under `HermesProxy.World.Enums` and picks up any
  enum carrying `[DescriptorSection]`. There is no registry and no naming convention to satisfy —
  adding the attribute is what wires a section in.
* **`BuildSection`** walks the enum's members in **declaration order** and turns each decorated
  member into an entry. Create-path entries land in one ordered `CreateSequence` list (a
  `(Kind, Payload)` tuple stream, because fields, placeholders and bit-placeholders interleave);
  Update-path entries land in separate lists keyed by changesMask bit.
* **`Read*`** methods parse one `AttributeData` into one record. Constructor arguments are
  positional (`attrData.ConstructorArguments[n]`); everything else is a `switch` over
  `attrData.NamedArguments`.
* **`Emit*`** methods print text. `EmitCreate` walks `CreateSequence` in order; `EmitUpdate`
  branches on `MaskMode` into `EmitUpdateFlat` or `EmitUpdateBlocks`; `EmitHasAny` emits the
  "is anything set" predicate.

**Declaration order is wire order** on the Create path. The generator emits Create writes in the
order members appear, so migrating a hand-written block is a strict top-down walk — no bit
arithmetic. The Update path is the opposite: it is ordered by changesMask bit, and
`WriteOrder` exists for the cases where write order and bit order disagree.

## The mirrored-enum rule

The generator targets `netstandard2.0` and loads into the compiler. It **cannot reference the
HermesProxy assembly**, so every attribute enum exists twice:

| Real | Mirror |
|---|---|
| `World/Objects/Version/Attributes/DescriptorAttributes.cs` | private enums at the bottom of `ObjectUpdateBuilderGenerator.cs` |

The mirrors are matched **by ordinal**, not by name — `(DescriptorType)typeOrdinal.Value` casts
the boxed constant straight across. So:

> **Adding a member to `DescriptorType`, `MaskMode`, `ArrayMode` or `BlockMaskShape` means adding
> it to both copies, at the same ordinal. Append; never insert or reorder.** An inserted member
> silently reinterprets every existing declaration as its neighbour, and nothing errors — you get
> wrong bytes.

`FieldVisibility` is the exception: it is `[Flags]`, so the generator works with the raw `int`
and names the flags only when printing the guard. Adding a flag there means one line in
`GateFor`, and the real enum stays the single definition.

## Emitting names

The generator prints fully-qualified names, because the generated file has no `using`
directives — the `*FullName` consts at the top of the class exist for exactly this. When you
emit a reference to a type, add a const rather than inlining the string; that is what makes a
namespace rename a one-line change.

## Diagnostics

`HPSG003` fires when a `SourceProperty` names a member the section's `DataType` does not have,
and the field is skipped. That is the only structural check.

There is no diagnostic for the failure mode that actually costs a debugging session: a field in
the *wrong position*. Nothing in the generator knows the wire layout, so nothing can. That is
what the equivalence tests are for — see Part 3.

---

# Part 2 — Adding a capability

The recipe, in the order that keeps the build green at every step.

1. **Write the declaration you wish existed** in a `*Field.cs` enum. Start from the shape you
   want at the call site, not from the generator.
2. **Add the attribute member** in `DescriptorAttributes.cs`. A named property (settable, default
   = "off") if it is optional; a constructor argument only if it is mandatory. Document what the
   *generated code* looks like, not what the property means — the next reader is trying to
   predict output.
3. **If it introduces an enum**, mirror it in the generator at matching ordinals, per the rule
   above. Prefer flags handled as `int` (like `FieldVisibility`) over a mirrored enum when the
   values are a set rather than a choice; it avoids the ordinal coupling entirely.
4. **Parse it** — one `case` in the relevant `Read*` method, plus a local initialised to the
   "off" value. `VisibilityOrdinal` shows the trap: a byte-backed enum boxes as `byte`, not
   `int`, so `as int?` silently yields null and your feature does nothing.
5. **Carry it on the record.** Prefer carrying the *resolved decision* over the raw input —
   `CreateFieldEntry.Gate` is a `string?` holding the guard condition, not a pair of flags the
   emit sites would each have to interpret. Emit sites should print, not decide.
6. **Emit it.** Grep for the sibling feature and match its indentation handling; the create-path
   emitters thread a `baseIndent` local through every branch, and a missed one produces
   compiling, misindented, correct code that reviews badly.
7. **Prove the wire did not move.** Part 3.

## Where the seams are

Some things belong in the generator and some do not. The split that has held so far:

| Belongs in the generator | Belongs in the hand-written `ObjectUpdateBuilder` |
|---|---|
| per-field writes, order, masks, guards | the packet envelope around the values blob |
| anything repeated across sections | anything that needs session state (`_gameState`) |
| anything a section's enum can express | computed values, interleaved arrays, fallbacks |

The envelope is the load-bearing case. Each version's `WriteToPacket` / `WriteValuesCreate`
writes the object header, the visibility byte and (on 5.5-engine builds) the fragment mask, then
calls the generated per-section methods. Keeping the envelope hand-written is what let 3.4.3 and
2.5.6 differ structurally without the generator learning about either.

For the third row, the house pattern is a **positioned hook**: a `DescriptorCreatePlaceholder`
with `CustomWriter` set emits `{CustomWriter}(data, src);` at that point in the sequence, so the
callback keeps its place in wire order while the enum stays the index of what gets written. See
`UnitField`'s `UNIT_STATS_INTERLEAVED_CUSTOM` / `UNIT_POWER_INTERLEAVED_CUSTOM` and the thirteen
`WriteCreateActivePlayer*` hooks.

Reach for a hook when the declarative form cannot express:

- **computed values** — `GetModernInvSlot` fans legacy slot arrays into the modern flat layout
- **session fallbacks** — `?? _gameState.SummonedBattlePetGuid` is not a literal default
- **interleaved arrays** — N parallel arrays woven through one index loop. `ArrayCount` writes one
  array's elements consecutively, which is a different wire shape

## Attribute vocabulary

Declared in `World/Objects/Version/Attributes/DescriptorAttributes.cs`.

**Section**

- `[DescriptorSection]` — marks the enum. `DataType`, `MaskMode` (`Blocks` / `Flat`), `MaskWidth`,
  `BlockMaskShape` (`Bits` / `UInt32PlusBits16`), `Cascade`. `SectionName` defaults to `DataType`
  minus its `Data` suffix and drives the emitted method names. Block count is not declared — the
  generator derives it as `(maxBit / 32) + 1` from the highest `bit` any update field claims.

**Create path**

- `[DescriptorCreateField]` — a write from a source property. `ArrayCount`, `ArrayMode`
  (`Grouped` / `PerElement`), `DefaultExpression`, `DefaultExpressionByIndex`, `Visibility`,
  `Cast`, `CustomWriter`
- `[DescriptorCreatePlaceholder]` — a constant write (natural zero, or a literal like `"1f"`).
  `Count` repeats it in a loop, for the zero-fill runs like `QuestCompleted[875]`. With
  `CustomWriter` set it becomes a positioned hook instead
- `[DescriptorCreateBitsPlaceholder]` — `WriteBits(value, n)`, plus `FlushBits` unless `Flush=false`

Arrays longer than `LoopEmitThreshold` (8) with a uniform fallback emit as a loop rather than
unrolled writes.

**Update path**

- `[DescriptorUpdateField]` — keyed by changesMask `bit`. `ParentBit`, `WriteOrder`,
  `CustomPredicate`, `MaskOnly`
- `[DescriptorMaskPreamble]`, `[DescriptorMaskMutator]`, `[DescriptorUpdateBitsPreamble]`,
  `[DescriptorUpdatePostFlush]`, `[DescriptorCustomField]` — mask shaping

Members with no descriptor attribute are skipped, so an enum can carry non-wire entries
(sentinels, unmapped slots).

## Create-path visibility

From the 5.5 engine on, the values blob for a create carries a leading visibility byte — the
client's `UF::UpdateFieldFlag` (`Owner` 0x01, `PartyMember` 0x02, `UnitAll` 0x04, `Empath` 0x08) —
and the client gates its own reads on it.

**The byte is a contract, not a hint.** Declaring a bit obliges the writer to emit every field
that bit gates. Declare `PartyMember` and skip one of its fields and the client reads the next
group from the wrong offset, drifts, and faults. So visibility and field-filling are one job.

`Visibility` on a create-path attribute says which groups a field belongs to. The generator emits
the write under a test against the builder's `FieldVisibilityFlags`, which is the same byte that
leads the blob — writer and reader gate on one value.

V3_4_3 is on this model too. It only ever declares `Owner | PartyMember` or nothing, so its
`FieldVisibilityFlags` follows `IsOwner` and the whole thing reads as a boolean; that is why a
`bool OwnerOnly` sufficed until 2.5.6 split the groups apart into 0x01 / 0x03 / 0x07.

---

# Part 3 — The safety net

Two independent layers, catching different things. Use both.

1. **Byte-equivalence tests** (`HermesProxy.Tests/SourceGen/*SectionEquivalenceTests.cs`) — a
   frozen copy of the hand-port is kept in the test project as an oracle, and
   `Assert.Equal(expected.GetData(), actual.GetData())` proves the generated writer produces
   identical bytes across a scenario matrix. This is what makes migration safe.
2. **Verify snapshots** (`ObjectUpdateBuilderGeneratorTests.*.verified.txt`) — pin the generated
   *source*. A deliberate generator change fails these by design.

A green equivalence test plus a snapshot diff limited to your intended lines is strong evidence.
Neither alone is. And neither proves the *oracle* is right — for anything user-visible, finish
with a play-test.

## Accepting a snapshot

Confirm before accepting, then accept minimally:

```bash
diff --strip-trailing-cr <verified>.txt <received>.txt | grep '^[<>]' | grep -v '<the change you meant>'
```

An empty result means the diff contains only what you intended. `--strip-trailing-cr` is not
optional: snapshots are UTF-8 **with BOM, CRLF**, the received file is not, and a plain `diff`
reports every line as changed and hides the real one.

Then edit the `.verified.txt` **in place** (binary substitution preserves the CRLF and BOM)
rather than moving `.received.txt` over it — a move rewrites the line endings and turns a 70-line
review into an 8694-line one.

## Building

A running proxy or a live language server holds `HermesProxy.SourceGen.dll`, and MSBuild then
fails the *copy* with MSB3021/3027 **while reporting no `error CS`**. Kill the holder
(`csharp-ls`, `dotnet`) and rebuild; do not read the compiler text alone.

---

# Part 4 — State of the port

## ActivePlayer Create migration — done

What used to be one ~1800-line `WriteCreateActivePlayerAll` mega-writer, declared as a single
placeholder, is now fully declarative. It came apart in four slices, each one: add members in
wire order, delete the matching lines from the remainder, run the equivalence test, accept the
snapshot diff. The remainder shrank 293 → 177 → 91 → 0 lines.

What is left is thirteen named, single-purpose `WriteCreateActivePlayer*` hooks, positioned by
`DescriptorCreatePlaceholder(CustomWriter = …)`. Each exists for a shape the per-field form
genuinely cannot express — interleaved parallel arrays (`Skill`, `DamageDone`,
`WeaponMultipliers`, `Buyback`), session-sourced state (`Glyphs`, `HeirloomCounts`,
`SummonedBattlePet`), computed values (`InvSlots`, `KnownTitlesCount`, `DynamicPayloads`), a
nested struct with a non-zero default (`RestInfo`), a cast that has to precede the null-coalesce
(`PvPTierMax`), and bit-level element framing (`PvpInfo`).

`ActivePlayerSectionEquivalenceTests.WriteCreateActivePlayerData_HandPort` is the frozen
pre-migration oracle. **Do not "fix" that copy** — its value is that it does not change. It still
pins the whole Create wire, so future edits to these members stay honest.

Still worth doing: several literal zeros are marked `live property exists, TODO per-element
read` — `NoReagentCostMask`, `BagSlotFlags`, `BankBagSlotFlags`, `QuestCompleted`,
`ExploredZones` payloads, `PvpInfo`. A hand-written zero in this block is what caused the
action-bar-reset bug (`8087167e`) while the generated Update path was correct; now that the
positions are named members, filling one in is a one-line change.

## Wiring a new section

All nine object sections a WotLK backend actually sends are wired on both paths. Corpse and
DynamicObject were the last two, and wiring them was not just a refactor — neither had an Update
path and their Values deltas were being parsed and then dropped.

If you wire another section, remember the third step: it needs a probe in `IsEmptyValuesDelta`
(`World/Server/Packets/UpdatePackets.cs`), or its single-field deltas are filtered as "empty"
before the builder ever runs.

## Not yet wired

- All 12 `*DynamicField` enums — dynamic fields go through custom callbacks
- `AreaTrigger` / `Conversation` / `SceneObject` — enums exist, nothing wired, likely dead for a
  WotLK backend

## What a 5.5-engine port needs from here

Less generator work than it looks, because the seam rule puts most of it in the hand-written
builder. Checked against the code rather than assumed:

- **The values-update envelope is not generator work.** On 5.5 the update carries `u8 IsOwned`,
  `u8 HasFragmentUpdates`, a changed-fragment mask (one bit per updateable fragment, two if it is
  indirect), and then a `u32` type flag — and inside that, the per-section blocks the generator
  already emits, unchanged. The `changedMask` V3_4_3's `WriteValuesUpdate` already writes **is**
  that `u32` type flag, same values (Object 0x1, Item 0x2, Container 0x4, Unit 0x20, Player 0x40,
  ActivePlayer 0x80, GameObject 0x100, DynamicObject 0x200, Corpse 0x400). The envelope around it
  is what differs, and that belongs in each version's builder.
- **The empty-delta rule lives in that envelope too.** 5.5 requires a non-zero changed-fragment
  mask plus a type flag even when nothing changed; writing a zero mask tears `CGObject` down, and
  the next update then calls a lazy constructor that is NULL for it. The generator's per-section
  `if (blocksMask == 0) return;` is a different thing and is not implicated — it fires *after* the
  section's blocks mask is written and flushed, so a section reached with nothing set still emits a
  well-formed empty mask.
- **`HasAny{Section}FieldSet` is an update-path predicate** over `UpdateFields`, and visibility is a
  create-path concept. They do not interact.

What is genuinely unknown is which shapes the attribute vocabulary cannot express once real 2.5.6
descriptors are written against it. Two candidates are already visible in the reference material:
an inline count-prefixed pair array (`PlayerData.QuestLogExtraMap` — a `u32` count then that many
`{questID, slotIndex}` pairs) and nested-struct array elements at a fixed stride (`VisibleItem`, 23
bytes). Both are expressible today through `CustomWriter` hooks. Whether they are common enough on
5.5 to earn first-class attributes is a question the port answers — not one to guess at now.
