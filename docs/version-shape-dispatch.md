# Version-Dependent Packet Shape

> **Status:** design memo, not a description of intended behaviour. Nothing here is scheduled.
> **Written:** 2026-08-28 · **Revised:** 2026-09-05
> **Verified against:** `master` @ 1fa406fb (post-`v4.4.0`, WotLK Classic merged). Every count below
> was re-measured on that commit. The first revision of this memo was written against
> `feature/wotlk-classic-v3.4.3` and several of its numbers and API claims were wrong; the
> corrections are called out inline so the earlier conclusions can be re-checked rather than trusted.

It records why the codebase is full of exact-build comparisons, what breaks when a new modern client
arrives, and how that connects to the discriminated-union work on `perf/union-object-update`.

## The problem

Modern-side version conditionals in `HermesProxy/`, by form:

| form | count | degrades on an unknown build? |
|---|---:|---|
| `ModernVersion.Build == …` | 182 | no — falls into `else` |
| `ModernVersion.Build != …` | 38 | no — falls into `if` |
| `ModernVersion.AddedInVersion(…)` | 44 | yes |
| `ModernVersion.ExpansionVersion` compared | 51 | yes |
| `ModernVersion.AddedInClassicVersion(…)` | 9 | yes |
| `ModernVersion.IsClassicVersionBuild()` | 2 | yes |
| `ModernVersion.RemovedInVersion(…)` | 1 | yes |
| **total** | **327** | **220 of them do not** |

Distribution of the 182 `==` sites:

| location | count | kind |
|---|---:|---|
| `World/Server/Packets` | 88 | wire structure |
| `World/Client/PacketHandlers` | 49 | translation behaviour |
| `World/Server/PacketHandlers` | 18 | translation behaviour |
| `World` | 14 | mixed |
| `World/Objects` | 10 | mixed |
| `World/Server`, root | 3 | mixed |

Only the first group is about *shape* — how bytes are laid out. The rest decide *what* to send,
which is a separate concern and out of scope for this memo.

> **Correction.** The first revision counted only `==`, reported 192, and drew its whole argument from
> that number. Two things were missed: the `!=` sites, which fail the same way, and the ~107 ordered
> comparisons, which do not. The cliff described below applies to 220 sites, not 327 — a smaller
> problem than the raw census suggests, but a sharper one, because the 220 are concentrated in
> exactly the wire-shape code where silent misreads are worst.

## The failure mode

Exact equality means a future modern build — Cataclysm Classic, or even a V3_4_4 patch — falls
into every `else` branch, which is the **V1_14 / V2_5 layout**. That is not graceful
degradation. Cata Classic's wire format is far closer to WotLK Classic than to TBC Classic, so
the fallback is wrong for essentially every packet with a version branch, and it fails silently:
no exception, no log, just misread fields.

Worth stating plainly, because the whole file reads like paranoia otherwise: nothing is broken
today. This is a cliff we walk off the day a V4_x build is added to
`VersionChecker.IsSupportedModernVersion`.

## Why it looks like this

`ModernVersion` is not, in fact, missing ordering helpers. `VersionChecker.cs:683-753` already
provides the full set:

```csharp
public static bool AddedInVersion(byte expansion, byte major, byte minor);
public static bool AddedInVersion(byte retailExpansion,     byte retailMajor,     byte retailMinor,
                                  byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor,
                                  byte classicExpansion,    byte classicMajor,    byte classicMinor);
public static bool RemovedInVersion(/* same nine */);
public static bool AddedInClassicVersion(/* six */);
public static bool RemovedInClassicVersion(/* six */);
public static bool IsVersion(byte expansion, byte major, byte minor);
public static bool IsClassicVersionBuild();
public static bool InVersion(ClientVersionBuild a, ClientVersionBuild b);
public static bool AddedInVersion(ClientVersionBuild build);    // Build >= build
public static bool RemovedInVersion(ClientVersionBuild build);  // Build <  build
```

> **Correction.** The first revision claimed `ModernVersion` had "none — only `Build`,
> `ExpansionVersion`, `MajorVersion`, `MinorVersion`", and built its headline recommendation on
> that. False: these helpers arrived with 857bbe3c ("Add support for 1.14.1 client"), long before the
> memo. The recommendation below is correspondingly narrower.

So the 220 exact comparisons are not the result of a missing API. They are the result of a
*confusing* one, and of one genuinely dangerous overload.

### The nine-argument form is already a branch — badly spelled

```csharp
public static bool AddedInVersion(byte retailExpansion, byte retailMajor, byte retailMinor,
                                  byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor,
                                  byte classicExpansion, byte classicMajor, byte classicMinor)
{
    if (ExpansionVersion == 1)                             return AddedInVersion(classicEra…);
    else if (ExpansionVersion == 2 || ExpansionVersion == 3) return AddedInVersion(classic…);
    return AddedInVersion(retail…);
}
```

That `if` chain *is* WPP's `ClientBranch`, expressed as three magic integers and nine positional
`byte` parameters. It has 53 call sites, all of which read like this:

```csharp
if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))   // MovementInfo.cs:293
```

Nothing at the call site says which triple is which branch, and the compiler cannot help — every
parameter is `byte`. The concept is right; the spelling is the problem.

### The trap, and it is live

```csharp
public static bool AddedInVersion(ClientVersionBuild build) => Build >= build;
```

`ClientVersionBuild` is keyed by build number, and build numbers do not order by expansion across
the original/Classic split:

| build | value |
|---|---:|
| `V4_3_4_15595` — original Cataclysm, 2012 | 15595 |
| `V3_4_3_54261` — WotLK Classic, 2023 | 54261 |

So `Build >= V3_4_3_54261` is *false* for original Cataclysm and *true* for Cata Classic. It
happens to work within the Classic re-release line, where builds increase over time, but the
predicate is not expressing what the caller means. The honest ordering key is
`(ExpansionVersion, MajorVersion, MinorVersion)`, parsed from the enum name and already cached —
which is precisely what the byte-triple overloads use.

This overload has exactly **one** modern-side caller today:

```
HermesProxy/World/Client/PacketHandlers/CharacterHandler.cs:392
    ModernVersion.AddedInVersion(ClientVersionBuild.V1_14_0_39802)
```

One site is a cheap fix. Left alone, it is the seed of the next 50.

## What actually varies

`World/Server/Packets` holds **651** packet classes — 276 `ClientPacket`, 375 `ServerPacket`.
Of those, **78 contain a modern-version conditional** (28 read-side, 50 write-side), plus **14
helper/nested classes** that are serialised into other packets:

| side | count | classes |
|---|---:|---|
| `ClientPacket` (Read) | 28 | `AddIgnore`, `AuctionListItems`, `AuctionSellItem`, `BuyItem`, `CTextEmote`, `ChatMessage`, `ChatMessageAFK`, `ChatMessageChannel`, `ChatMessageDND`, `ChatMessageEmote`, `ChatMessageWhisper`, `DoReadyCheck`, `LootRoll`, `MailCreateTextItem`, `MailDelete`, `MailMarkAsRead`, `MailReturnToSender`, `MailTakeItem`, `MailTakeMoney`, `MountSpecial`, `PartyInviteResponse`, `PartyUninvite`, `PlayerLogin`, `ReadyCheckResponseClient`, `SetAssistantLeader`, `SetEveryoneIsAssistant`, `SetRole`, `SupportTicketSubmitComplaint` |
| `ServerPacket` (Write) | 50 | incl. `AuctionHelloResponse`, `ChatPkt`, `GossipPOI`, `InitWorldStates`, `InventoryChangeFailure`, `MonsterMove`, `QueryQuestInfoResponse`, `ShowTaxiNodes`, `StartLootRoll`, `SupercededSpells`, `UpdateObject`, `VendorInventory`, … |
| helper / nested | 14 | `CharacterInfo`, `GuildRosterMemberData`, `MailListEntry`, `ObjectUpdate`, `PVPMatchPlayerStatistics`, `RideTicket`, `SpellCastData`, `SpellCastRequest`, `SpellTargetData`, `VendorItem`, … |

> **Correction.** The first revision said "11 of 648 packet classes", listed 13 names under that
> heading, and included two — `AuctionListOwnerItems` and `SpellGo` — that carry no conditional at
> all. `SpellGo`'s variance lives in the shared `SpellCastData` helper, which is a *worse* case than
> a self-contained one, not a smaller one. The real ratio is **92 of 651**, roughly 14%.
>
> This inverts the first revision's own trade argument. "Duplicating all 637 to isolate 11 is a bad
> trade" does not survive the corrected number: 92 conditional-bearing types out of 651 is well
> past the point where a per-version representation pays for itself, and the 14 shared helpers mean
> the variance is not even confined to the classes that declare it.

The duplication hazard the first revision cited is real and measured. `World/Objects/Version/`
holds five hand-maintained `ObjectUpdateBuilder.cs` copies:

| version dir | lines |
|---|---:|
| `V1_14_0_40237` | 1720 |
| `V1_14_1_40688` | 1720 |
| `V2_5_2_39570` | 1708 |
| `V2_5_3_41750` | 1720 |
| `V3_4_3_54261` | 1747 |

`V1_14_0_40237` and `V1_14_1_40688` differ by **38 diff lines out of 1720** and must be kept in
sync by hand. That is the cost of per-version duplication done manually; it is an argument for
*generating* the per-version forms, not for keeping one class with branches inside.

## What is actually objectionable

Not the branch's condition — its placement, and the fact that it now multiplies. From
`ChatPackets.cs:269`:

```csharp
public override void Read()
{
    Language = _worldPacket.ReadUInt32();
    ChannelGUID = _worldPacket.ReadPackedGuid128();

    if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
    {
        uint targetLen343 = _worldPacket.ReadBits<uint>(9);
        uint textLen343 = _worldPacket.ReadBits<uint>(11);
        if (_worldPacket.HasBit())
            IsSecure = _worldPacket.HasBit();
        Target = _worldPacket.ReadString(targetLen343);
        Text = _worldPacket.ReadString(textLen343);
        return;
    }

    uint targetLen = _worldPacket.ReadBits<uint>(9);
    uint textLen = _worldPacket.ReadBits<uint>(9);
    Target = _worldPacket.ReadString(targetLen);
    Text = _worldPacket.ReadString(textLen);
}
```

One method holds two layouts, so neither reads as a coherent packet, and the shape is
re-decided on every read.

### The write side is worse: the branch is written twice

Most `ServerPacket`s now implement `ISpanWritable` (`Framework/IO/ISpanWritable.cs`) — 285 classes
in `World/Server/Packets` declare it, giving each one *two* serialisers: `Write()` against
`WorldPacket`, and `WriteToSpan(Span<byte>)` against `SpanPacketWriter`. Where such a packet also
has a version branch, the branch exists in both. `GossipPOI` (`NPCPackets.cs:643`) carries four
layouts in one class:

```csharp
public override void Write()
{
    if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261) { /* flat layout, _worldPacket */ }
    /* retail layout, _worldPacket */
}

public int WriteToSpan(Span<byte> buffer)
{
    var writer = new SpanPacketWriter(buffer);
    if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261) { /* flat layout, writer */ }
    /* retail layout, writer */
}
```

**25 classes** duplicate a version branch across `Write()` and `WriteToSpan()` this way:
`ArenaTeamRosterResponse`, `AuctionHelloResponse`, `BattlefieldStatusQueued`, `BinderConfirm`,
`ChatPkt`, `EmoteMessage`, `GossipComplete`, `GossipPOI`, `InitializeFactions`,
`InventoryChangeFailure`, `LearnedSpells`, `MailCommandResult`, `MonsterMove`, `PlayObjectSound`,
`PlaySound`, `PlayerTabardVendorActivate`, `QueryPlayerNameResponse`, `QuestGiverStatusMultiple`,
`QuestGiverStatusPkt`, `SellResponse`, `ShowBank`, `ShowTaxiNodes`, `SpecialMountAnim`,
`SpiritHealerConfirm`, `SupercededSpells`.

This is the same hand-sync hazard as the two `ObjectUpdateBuilder.cs` copies, at a smaller scale
and with no diff to check it against. A wire fix applied to `Write()` and forgotten in
`WriteToSpan()` shows up only on whichever path the send pipeline happens to take.

Swapping `==` for a range predicate fixes the forward-compatibility cliff but leaves all of this
untouched. **The requirement is that the shape is chosen once, not per packet, and expressed
once, not per serialiser.**

## Prior art

### Already in this repo: `PacketHandlerAttribute`

`HermesProxy/World/PacketHandlerAttribute.cs` already implements version-ranged registration with
the predicate in the attribute constructor:

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class PacketHandlerAttribute : Attribute
{
    public PacketHandlerAttribute(Opcode opcode) { Opcode = opcode; }

    /// <summary>[addedInVersion, +inf[</summary>
    public PacketHandlerAttribute(Opcode opcode, ClientVersionBuild addedInVersion)
    {
        if (LegacyVersion.AddedInVersion(addedInVersion))
            Opcode = opcode;
    }

    /// <summary>[addedInVersion, removedInVersion[</summary>
    public PacketHandlerAttribute(Opcode opcode, ClientVersionBuild addedInVersion, ClientVersionBuild removedInVersion)
    {
        if (LegacyVersion.InVersion(addedInVersion, removedInVersion))
            Opcode = opcode;
    }

    public Opcode Opcode { get; private set; }
}
```

`AllowMultiple`, unset `Opcode` skips registration, table built once by reflection at startup —
the whole pattern, in production, with **28 ranged call sites**. It is scoped to `LegacyVersion`
only. There is no `ModernVersion` equivalent.

> **Correction.** The first revision presented this pattern as WowPacketParser prior art to be
> "transplanted with little ceremony", without noticing it was already transplanted. The open work
> is not the mechanism — it is extending the existing attribute to the modern axis, and to packet
> *classes* rather than only handler methods.

### WowPacketParser: `ClientBranch`

WPP parses roughly twenty client versions across Retail, Classic, TBC, WotLK and Cata. Its
contribution we do *not* have is a named branch:

```csharp
public enum ClientBranch { Retail = 0, Classic = 1, TBC = 2, WotLK = 3, Cata = 4, MoP = 5 }

public static bool AddedInVersion(ClientBranch branch, ClientVersionBuild build)
    => _branch == branch && AddedInVersion(build);
```

Original Cataclysm and WotLK Classic are different branches, so their build numbers are never
compared — the trap above cannot be expressed. Our nine-`byte` overload achieves the same effect
by accident, unnamed and unreadable. Naming it is the cheap half of the fix.

### Borrow the concepts, not the mechanism

WPP is an offline sniff parser. It is free to allocate, and it does. This proxy is not — see
`docs/perf.md` and the zero-allocation rules in `CLAUDE.md`. Three specific reasons not to copy
its dispatch verbatim:

- **The predicate runs in an attribute constructor.** That only works because WPP builds its
  attributes reflectively at runtime, after `ClientVersion` is set. It makes initialisation
  order load-bearing — a hazard this codebase already has, since `ModernVersion.RequireBuild()`
  throws when accessed before `VersionBootstrap` assigns, and the test suite has to set it from
  a module initializer for exactly that reason. Note that `PacketHandlerAttribute` above already
  takes on this hazard; extending it to `ModernVersion` extends the hazard too.
- **It is invisible to source generators.** A generator cannot evaluate
  `ClientVersion.AddedInVersion(...)` at compile time. Since the object-update path is already
  generated, any version range a generator must see has to be expressible as *literal attribute
  arguments* — data, not a predicate. The descriptor attributes in
  `World/Objects/Version/Attributes/DescriptorAttributes.cs` are written that way and are the
  better model.
- **Reflective dispatch allocates per packet.** WPP's does; so does ours, today.

So: take `ClientBranch` as a *name* for a discrimination the code already performs. Take the idea
of version-ranged registration resolved once — we have it, on one axis. Do not take the
runtime-reflection machinery.

## What "better" looks like

Our own read dispatch has the allocation problem we would be criticising:

```csharp
// WorldSocket.cs:1465
using var clientPacket = (ClientPacket)Activator.CreateInstance(packetType, packet)!;
clientPacket.Read();
methodCaller(session, clientPacket);
```

That is a reflective allocation for **every inbound packet**, plus a delegate hop through
`CreateDelegate<P1>`'s closure, on a class that exists only to be read once and disposed. Any
redesign that fixes version dispatch should fix this at the same time — they are the same code
path, and doing them separately means touching it twice.

The pieces for something better already exist in the repo, further along than the first revision
credited:

- `SpanPacketReader` / `SpanPacketWriter` — `ref struct` readers over `Span<byte>`, benchmarked
  in `docs/perf.md:88` at 0.08 ns per `ReadInt64` against 157.98 ns for `ByteBuffer`, zero
  allocation.
- `ISpanWritable` — the span write path is not a proposal, it is **deployed across 285 packet
  classes** (`docs/ispanwritable.md`, whose own 272/321 figure is now stale). The write side
  already has a second, allocation-free serialiser per packet. What it lacks is a reason for that
  serialiser to exist *per version* instead of branching internally.
- The direct-indexed opcode tables in `LegacyVersion` / `ModernVersion` — arrays indexed by
  opcode, built once, no dictionary hashing.
- `ObjectUpdateBuilderGenerator` (`HermesProxy.SourceGen/`) — the proof that per-version wire
  layout can be declared as attributes and emitted as straight-line code with no runtime branch.
- `PacketHandlerAttribute` — ranged registration resolved once, on the legacy axis.

Sketch of the target, to be argued with rather than accepted:

- Version ranges are **literal attribute data** on the packet or layout declaration, so both the
  generator and any runtime table can read them.
- The generator emits one `Read` and one `WriteToSpan` per applicable version, plus a dispatch
  table per version. The 25 dual-branch classes collapse: one layout declaration, two emitted
  serialisers, no hand-sync.
- Startup selects the table once — a direct-indexed array, same scheme as the opcode tables.
- Per packet: index the array, call a static method against a `SpanPacketReader`. No
  `Activator`, no reflection, no per-packet class instance, no version check.

The open question is how far to push this. Turning packet classes into `ref struct` readers is a
much larger change than fixing version dispatch, and the two are separable — the version work
can land on the existing class-based packets first and the allocation work second. But the
generated-dispatch design should not *preclude* the second step, which arguing it through now is
meant to ensure.

## The write path has no central table

`ServerPacket`s are constructed at call sites — `World/Client/PacketHandlers` alone holds 297
construction sites for `ServerPacket`-derived types — and `ServerPacket`'s constructor resolves the opcode eagerly:

```csharp
// Packet.cs:88
protected ServerPacket(Opcode universalOpcode)
{
    uint opcode = ModernVersion.GetCurrentOpcode(universalOpcode);
    if (opcode == 0)
        throw new UnmappedOpcodeException(universalOpcode, isModern: true);
    _worldPacket = new WorldPacket(opcode);
}
```

So the read side can adopt a version-resolved type transparently — `_clientPacketTable` is built
once at startup and `CreateDelegate<P1>` casts `(P1)p`, which a version-specific subtype
satisfies — while the write side cannot. 375 `ServerPacket` classes are named directly by their
call sites. A version-variant write path needs either a factory (`ServerPackets.GossipPOI()`
returning the resolved variant) or a startup-resolved delegate per packet type, and that decision
is a prerequisite for step 3 below, not a detail inside it.

## Out of scope: the legacy axis is 1.4× larger

This memo is entirely about `ModernVersion`. The legacy side is bigger:

| | modern | legacy |
|---|---:|---:|
| version conditionals | 327 | **461** |
| concentrated in | `World/Server/Packets` (151) | `World/Client/PacketHandlers` (291) |

Legacy shape variance is real and documented elsewhere — quest-log stride is 3 fields/slot on
vanilla, 4 on TBC, 5 on WotLK+; the gossip quest-icon encoding changes at `V3_0_2`. The legacy
axis differs in two ways that may or may not justify separate treatment:

- It is bounded. The supported legacy builds are a closed set (vanilla 1.12.x, TBC 2.4.3, WotLK
  3.3.5a); no new ones are coming. The forward-compatibility cliff does not exist there.
- It already has the ranged-attribute mechanism, at opcode granularity.

Whether legacy shape dispatch wants the same treatment, a lighter one, or none, is unanswered.
Stating it as out of scope is a choice, not an observation — and it should be made deliberately,
because "fix version dispatch" that touches 327 of 788 sites is a partial answer.

## Dependency that changed since April

`perf/union-object-update` (7ecd0822) was last touched **2026-04-17**, when `ObjectUpdateBuilder`
was entirely hand-written. It no longer is. The ActivePlayer Create path is source-generated, and
`ObjectUpdateBuilderGenerator` *emits* field accesses directly. The exposure roughly doubled when
WotLK Classic merged to `master` on 2026-09-05:

| symbol | memo v1 (`feature/wotlk-classic-v3.4.3`, 2026-08-28) | `master` @ 1fa406fb |
|---|---:|---:|
| `UnitData` | 154 | **395** |
| `ActivePlayerData` | 106 | **282** |
| `CorpseData` | 23 | **63** |
| `DynamicObjectData` | 11 | **27** |

Converting `ObjectUpdate` to a union is therefore no longer a source-edit alone — the generator
has to emit union-aware access or the generated file will not compile. That cuts both ways: it
is a new dependency for the union branch, but one emit-template change covers the generated sites
rather than hand-editing each.

> **Correction (2026-09-12).** Both claims above are out of date, and the conclusion was wrong.
> The branch was rebased on **2026-09-09** onto `30f78fed` (PR #270) and is now `85f0c536`, only
> 30 commits behind `master` — not stale since April. More importantly it **did not need the
> generator to change at all**: public accessor properties (`UnitData`, `PlayerData`, …) pattern-match
> the union internally, so all 280+ call sites including every generated one compile unchanged. The
> final diff is three files — `World/Server/Packets/UpdatePackets.cs` plus `ObjectSpecificDataTests.cs`
> and an edit to `ItemSectionEquivalenceTests.cs`. The counts in the table above are still right;
> what they cost is not.
>
> One property the accessors carry forward: they return `null!` for the wrong variant, matching the
> prior `null!`-suppressed fields. So the union currently makes invalid *construction*
> unrepresentable, not invalid *access*.
>
> All four live branches share the base commit
> `7cbc9dd6 Build: Bump target framework to .NET 11 with C# 15 preview language`:
> `perf/union-object-update`, `perf/union-trade-status`, `perf/union-inventory-failure`,
> `perf/union-socket-address`. Each uses `sealed record` (reference-type) variants — correct for the
> outbound and event-rate packets they convert, but a reason to measure before reaching for unions on
> the inbound per-packet path. `perf/union-prep` (`6386462b`, 2026-04-15) is 561 commits behind with
> nothing ahead; it is dead.

## Direction: discriminated unions

Same argument as `perf/union-object-update`, which replaces `ObjectUpdate`'s eight nullable data
fields with an `ObjectSpecificData` union so that invalid combinations are unrepresentable at the
type level.

Version variants are the same shape of problem:

```
ChatMessageChannel
├─ V1_14  { Language, ChannelGUID, Target, Text }
├─ V2_5   { Language, ChannelGUID, Target, Text }
└─ V3_4_3 { Language, ChannelGUID, Target, Text, IsSecure }
```

`IsSecure` is the tell. It was added to the shared class by the #177 fix and is meaningless for
two of the three versions that use it — representable, invalid, silently ignored. Under a union
it does not exist outside the variant that has it, and the variant is selected once at
construction.

## Suggested sequencing

1. **Name the branch on `ModernVersion`.** ✅ **Done 2026-09-12.** `ClientBranch`
   (`Framework/Constants/ClientBranch.cs`) plus `VersionChecker.GetBranch(expansion, major)` and a
   `Branch` field on both `LegacyVersion` and `ModernVersion`. The `ExpansionVersion == 1 / 2 || 3 /
   else` chain inside `AddedInVersion(9 bytes)`, `AddedInClassicVersion(6 bytes)` and
   `IsClassicVersionBuild()` now switches on it — that chain had **no arm for expansion 4**, so a
   Cataclysm Classic client silently took the *retail* triple at all 53 ranged sites. Equivalence over
   every supported build is pinned by `HermesProxy.Tests/World/ClientBranchTests.cs`.
   Rather than `[Obsolete]`-ing the raw-build overloads (step 2 below assumed one caller; there are
   five, and four are branch-guarded and sound), they now call a `[Conditional("DEBUG")]`
   `VersionChecker.AssertComparableBranch`, which turns a cross-branch comparison into a loud test
   failure at zero Release cost.
   This is a *rename of an existing behaviour*, not new logic — the `ExpansionVersion == 1 / 2||3 /
   else` chain already implements it. 53 call sites become readable; the trap becomes
   inexpressible. Small, mechanical, independently verifiable against the current predicate.
2. **Remove or `[Obsolete]` the raw-build overloads** `ModernVersion.AddedInVersion(ClientVersionBuild)`
   and `RemovedInVersion(ClientVersionBuild)`. One caller today (`CharacterHandler.cs:392`). Doing
   this after step 1 costs nothing and closes the trap permanently.
3. **Decide the write-path dispatch shape** — factory, startup-resolved delegate, or generated
   table. Prerequisite for step 5, and it is where the 375-class blast radius lives.
4. **Refresh `perf/union-object-update`** onto `master` and make `ObjectUpdateBuilderGenerator`
   union-aware. Enabling step for the union direction, and where the real work is. Note the
   reference counts above: this is a materially larger change than it was in April.
5. **Apply the variant pattern to the 78 divergent packet classes and 14 shared helpers**, with
   the version range as literal attribute data and the concrete shape resolved once at startup.
   Prioritise the 25 dual-branch `ISpanWritable` classes — they pay twice today.
6. **Leave the handler-level checks alone.** They are behaviour, not shape, and need their own
   analysis.

Two things deliberately *not* recommended:

- **Mechanically rewriting the 220 exact comparisons to range predicates.** Unverifiable while no
  V4 client exists to test against, some sites may be genuinely build-specific, and a blind sweep
  across that many wire-format branches is exactly the change that breaks things invisibly.
- **Copying WPP's dispatch.** Take the `ClientBranch` naming; leave the runtime reflection and the
  attribute-constructor predicate — noting that `PacketHandlerAttribute` already carries the
  latter on the legacy axis, so "we do not do that here" is not true today.

`ChatMessageChannel` remains the natural first read-side case for step 5 — small, and PR #178
verified both layouts against a live client. `GossipPOI` is the natural first write-side case: it
is the clearest instance of the same branch existing in two serialisers.

## Open questions

- Does the packet redesign stop at version dispatch, or continue into `ref struct` readers and
  the removal of per-packet `Activator.CreateInstance`? They are separable; the first should not
  preclude the second.
- Do version variants become union cases, or per-version types selected at table-build time?
  Unions make invalid field combinations unrepresentable, which is the stronger property, but
  they depend on the C# 15 / .NET 11 move that `perf/union-object-update` already assumes.
- Is a named branch alone sufficient for the handler-level checks too, or do those want a
  capability-style predicate ("does this build have X") rather than a version comparison?
- Does the legacy axis (461 sites) get the same treatment, a lighter one, or an explicit
  decision to leave it as-is? Its closed build set is an argument for leaving it; its size is an
  argument against.
- Should `ISpanWritable`'s `Write()` / `WriteToSpan()` pair survive at all, or is the generated
  per-version path the point at which the `WorldPacket` serialiser is retired? 285 classes
  currently maintain both by hand.
