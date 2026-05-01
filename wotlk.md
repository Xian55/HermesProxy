
# WoW WotLK Classic (3.4.3.54261) Client Support in HermesProxy

## Context

The goal is to extend HermesProxy so the **WotLK Classic retail client (build 3.4.3.54261)** can connect as a modern client and have its traffic translated to a legacy **WotLK 3.3.5a server emulator** (TrinityCore/AzerothCore/CMaNGOS) on the backend. An existing third-party fork at `X:\Programming\HermesProxy-WOTLK` (origin `github.com/advocaite/HermesProxy-WOTLK`) already implements this (WIP), but it regressed several upstream improvements (stripped `Directory.Packages.props`, deleted ~384 lines of `BnetTcpSession` pooled-buffer perf work, stripped `PublishTrimmed` trim-safety config). The plan below **cherry-picks the WotLK-specific additions** from the fork and rebuilds them on top of current upstream without bringing the regressions.

As of 2026-04-22 the fork has been checked out with its **full 46-commit git history** (previously believed to be a static snapshot). This enables commit-by-commit cherry-picking in Phase 5 rather than a single-shot big-bang port — see "Phase 5" and "Reference fork" sections below for the concrete implication.

---

## Why this is a large effort: 3.4.3 uses a new ObjectUpdate format

Per-version investigation of upstream vs. fork `ObjectUpdateBuilder.cs`:

| Client | Lines | Update format |
|---|---|---|
| V1_14_1_40688 (Classic Era) | 1720 | Legacy DWORD-indexed UpdateFields + update-mask bitmap |
| V2_5_3_41750 (TBC Classic) | 1720 | Same legacy DWORD system |
| V3_4_3_54261 (WotLK Classic) | 3419 | **New descriptor-based change-set system** |

Blizzard modernized the object-update protocol for WotLK Classic to match current retail (Legion/BfA/SL/DF) rather than ship 2008's `updatemask`-over-DWORD-array format. Concretely:

- **Old (Classic Era / TBC Classic)**: `UpdateFieldsArray m_fields` of fixed-size DWORDs, indexed by `PLAYER_FIELD_GOLD` etc. `m_updateMask` bitmap marks dirty DWORDs; serializer writes the mask then each dirty value in index order.
- **New (WotLK Classic 3.4.3)**: No flat field array. Each object type has hand-written `WriteCreate{Object,Unit,Player,ActivePlayer,Item,...}Data` methods that walk a **hierarchical tree of fields**, emitting nested bit-masks: `WriteBits(blocksMask1, 16)` → per-block `WriteBits(block[b], 32)` → field values. Variable-size arrays carry per-element change bits. Visibility is first-class (`IsOwner`, `IsGameObjectOwner`) and a `0x03 / 0x00` "update-field-flags" byte selects bucketed field sets per viewer. `ObjectTypeMask` has a new wire numbering (`0x20=Unit`, `0x40=Player`, `0x80=ActivePlayer`, ...) distinct from the old `ObjectTypeBCC` byte.

Implication: **there is no `UpdateFieldsArray.cs` to port** for V3_4_3 — the descriptor system doesn't use one. The ~3,419-line `ObjectUpdateBuilder.cs` itself is the whole thing, and every object type's serializer is hand-written bit-packing logic. Off-by-one errors in `WriteBits(mask, N)` corrupt the entire object stream, so this phase needs careful testing against ground-truth captures.

---

## Infrastructure landed in v4.3.0 (2026-04) that shapes this plan

These changes shipped before any WotLK-specific work started; every phase below assumes them.

1. **Source generators are a proven pattern.** `HermesProxy.SourceGen` (netstandard2.0, `IsRoslynComponent`) already emits `OpcodeTableGenerator` + `UpdateFieldTableGenerator` via flat `static readonly` arrays. Phase 5 Approach B **extends this existing project** with a third generator — it does not bootstrap one from scratch. Revised effort: **~2-3 days** to bootstrap, not ~1 week.
2. **No more reflection-loaded opcode/update-field tables.** `ModernVersion.LoadUFDictionariesInto` and the reflective enum-loading path are gone. `ModernVersion` / `LegacyVersion` are `beforefieldinit`-clean `static readonly` containers. Adding `V3_4_3_54261` in Phase 1 does not require trimmer-root edits — the generator picks it up at compile time.
3. **Per-connection DI via `ActivatorUtilities`.** `WorldSocket`, `RealmSocket`, `BnetRestApiSession`, `RealmManager` are all constructed with DI-injected `IOptions<T>` option DTOs (`ClientOptions`, `LegacyServerOptions`, `ProxyNetworkOptions`, `DiagnosticsOptions`, `LoggingOptions`). Any 3.4.3-specific configuration surface must flow through an options DTO, not a new static singleton. The removed `Framework.Settings` static class is not coming back.
4. **`ModernVersion.Build` / `LegacyVersion.Build` are `static readonly`**, populated once in `ProxyHostedService.StartAsync` via the internal `VersionBootstrap` holder. Branching on `ModernVersion.Build` is safe in any code path that runs after host startup (i.e. effectively everywhere the proxy accepts connections).

---

## Phased roadmap

Each phase ships in a single PR and leaves `master` buildable and runnable for the existing 1.14/2.5 clients.

### Phase 0 — Un-gate WotLK 3.3.5a legacy server backend

**Scope**: Flip the single `return false` guard and wire expansion-3 auto-selection. Validates that existing 3.3.5a plumbing (already present as `World/Enums/V3_3_5_12340/Opcode.cs`, routing in `Opcodes.cs:22,89`) actually works end-to-end before stacking a new client on top.

**Files** (upstream edits):
- `HermesProxy/VersionChecker.cs:38` — `case V3_3_5a_12340: return true;`
- `HermesProxy/VersionChecker.cs:106-114` — add `3 => ClientVersionBuild.V3_3_5a_12340` to `GetBestLegacyVersion` switch.

**CSV data** (copy from `X:\Programming\HermesProxy-WOTLK\HermesProxy\CSV\`): once `LegacyVersion.ExpansionVersion == 3`, `GameData.cs` will look up a handful of `*3.csv` files. Ship these with the Phase 0 PR:
- `BroadcastTexts3.csv`, `CreatureModelCollisionHeightsModern3.csv`, `SpellEffectPoints3.csv`, `StackableAuras3.csv`, `AuraSpells3.csv` — acceptable as fork's TBC-duplicate copies for character-select scope.
- `Transports3.csv` — fork has real WotLK data (take it).
- `BuildAuthSeeds.csv` — add the V3_4_3_54261 row from fork (seed hex is already in our `appsettings.json`).

See "CSV/DBC data strategy" section below for the full story and why not every fork `*3.csv` is safe to copy.

**Milestone**: An existing 2.5.x TBC Classic or 1.14.x Era client pointed at HermesProxy with `ServerBuild=V3_3_5a_12340` against a real WotLK 3.3.5a TrinityCore/AzerothCore backend reaches character-select. Unhandled legacy 3.3.5a opcodes surface in logs; file them as follow-ups, don't block this PR.

**Risk**: `V3_3_5_12340/Opcode.cs` enum may be incomplete or stale. Movement packets changed in 3.x (new `MSG_MOVE_*` transport packing) — expect initial crashes there.

---

### Phase 1 — Register build 54261 with scaffolding (stubs only)

**Scope**: Make 3.4.3 a known-compilable client build. All enums/routing exist; real packet logic stays stubbed.

**Upstream files to edit**:
- `Framework/Constants/ClientVersionBuild.cs` — add `V3_4_3_54261 = 54261`.
- `HermesProxy/VersionChecker.cs` — cases in `IsSupportedModernVersion`, `ModernVersion.GetUpdateFieldsDefiningBuild`, `GetResponseCodesEnum`, `GetAccountDataCount` (13 per fork), `GetGameObjectStateAnimId` (1772 per fork), `AdjustInventorySlot` (4 slot-range branches: equipment 0-18, bags 30-33→19-22, backpack 35-50→23-38, bank 59-137→39-117).
- `HermesProxy/World/Enums/Opcodes.cs` — cases in `GetOpcodesDefiningBuild` (returns itself) and `GetOpcodesEnumForVersion` (returns `typeof(V3_4_3_54261.Opcode)`).

**New files** (ports from `X:\Programming\HermesProxy-WOTLK` — the fork's **initial port commit `cc12fd6`** "opps i did a boo boo" is the verbatim baseline, +4,951 lines across 28 files):
- `HermesProxy/World/Enums/V3_4_3_54261/` — 26 files: `Opcode.cs`, `ResponseCodes.cs`, `CreateObjectBits.cs` (18 flags), `{ActivePlayer,AreaTrigger,Container,Conversation,Corpse,DynamicObject,GameObject,Item,Object,Player,SceneObject,Unit}{Field,DynamicField}.cs`. Verbatim data ports.
- `HermesProxy/World/Objects/Version/V3_4_3_54261/CreateObjectBits.cs` — direct port.
- `HermesProxy/World/Objects/Version/V3_4_3_54261/ObjectUpdateBuilder.cs` — **stub only**: constructor + all `WriteCreate*Data` / `WriteValuesUpdate` methods throw `NotImplementedException`. Real body ships in Phase 5.
- **No `UpdateFieldsArray.cs`** — the descriptor protocol doesn't use one.

**CSV data** (bootstrap layer — see "CSV/DBC data strategy" section for the phased regeneration plan): copy the full `ModernVersion.ExpansionVersion`-keyed `*3.csv` set from the fork as the initial baseline (~15 files: `Item3`, `ItemSparse3`, `ItemAppearance3`, `ItemEffect3`, `ItemDisplayIdToFileDataId3`, `ItemModifiedAppearance3`, `ItemSpellsData3`, `ItemEnchantVisuals3`, `ItemIdToDisplayId3`, `Gems3`, `QuestV2_3`, `SpellVisuals3`, `MeleeSpells3`, `AutoRepeatSpells3`, `MountSpells3`, `TaxiPath3`, `TaxiNodes3`, `TaxiPathNode3`). Twelve of these are byte-for-byte TBC duplicates in the fork; that's acceptable for Phase 1 load-no-crash scope, but **must be regenerated from wago.tools before Phase 5 gameplay** — see strategy section.

**Milestone**: App still runs for 1.14.x / 2.5.x. Setting `ClientBuild=V3_4_3_54261` loads without exceptions. `dotnet test` passes unchanged. `dotnet publish -p:PublishTrimmed=true` still works.

**Risk (obsolete as of v4.3.0)**: the earlier concern about reflection-loaded enum types being trimmed is no longer relevant. `ModernVersion.LoadUFDictionariesInto` and the reflective enum-loading path were removed in v4.3.0 in favor of compile-time `static readonly` flat arrays emitted by `HermesProxy.SourceGen` (see `OpcodeTableGenerator` / `UpdateFieldTableGenerator`). Adding `V3_4_3_54261` as a new per-version enum namespace is picked up by the generator automatically — no `TrimmerRootDescriptor.xml` edits needed.

---

### Phase 2 — BNet / REST login accepts 54261

**Scope**: The 3.4.3 client's Battle.net handshake completes and routes to `WorldSocket` setup.

**Pre-phase investigation** (required before starting): Diff fork's `*.pb.cs` vs upstream `Framework/Realm/**` and `HermesProxy/BnetServer/**`. Fork regenerated all protobuf bindings. Likely just `protoc` version churn (timestamps / compiler comments), but if real `.proto` message differences exist — e.g. WotLK Classic's realm category string — we must apply the minimal schema delta, not port wholesale.

**Files**:
- `HermesProxy/BnetServer/Services/*` — audit `AuthenticationService` / `GameUtilitiesService` for `ClientVersionBuild` range guards.
- `HermesProxy/Realm/RealmManager.cs` — realm-category handling for expansion-3 realms. Note: `RealmManager` is now DI-constructed with `ClientOptions` + `ProxyNetworkOptions` (v4.3.0); any WotLK-specific realm state should flow through those option DTOs, not the removed `Framework.Settings` statics.
- **DO NOT TOUCH**: `HermesProxy/BnetServer/Networking/BnetTcpSession.cs` (preserves your pooled-buffer work).

**Milestone**: 3.4.3 client completes BNet auth, gets realm list, picks a realm, attempts world connection (expected to stall at world-socket layer).

---

### Phase 3 — World login: AUTH_SESSION, AUTH_RESPONSE, encryption

**Scope**: World-socket handshake and per-connection encryption.

**Files**:
- `HermesProxy/World/Client/WotlkWorldCrypt.cs` **(NEW)** — port from fork. RC4 + HMAC-SHA1 with 3.3.5a HMAC seed. Follows the existing `VanillaWorldCrypt.cs` / `TbcWorldCrypt.cs` file pattern (upstream already splits these into separate files).
- `HermesProxy/World/Server/WorldSocket.cs` — AUTH_SESSION layout branch for 3.4.3 (new fields: `LoginServerID`, `RegionID`, `BattlegroupID`, `DosResponse`). `WorldSocket` is constructed via `ActivatorUtilities` (v4.3.0) — any 3.4.3-specific per-session state should be injected through the existing `IOptions<T>` path, not static singletons.
- `HermesProxy/World/Server/Packets/AuthenticationPackets.cs` — `AuthResponse` 3.4.3 layout (`AccountDataTimes[13]`).
- `HermesProxy/World/Server/PacketHandlers/AuthenticationHandler.cs` — dispatch by `ModernVersion.Build` (now a `static readonly` field — safe to branch on at any time after host startup).

**Milestone**: 3.4.3 client reaches character-select screen (empty list OK). No crypto errors.

---

### Phase 4 — Character enumeration

**Scope**: `CMSG_ENUM_CHARACTERS` (opcode 13801) → `SMSG_ENUM_CHARACTERS_RESULT` in WotLK Classic struct layout.

**Files**:
- `HermesProxy/World/Server/Packets/CharacterPackets.cs` — `EnumCharactersResult` write branch (guild GUID field, customization blob).
- `HermesProxy/World/Server/PacketHandlers/CharacterHandler.cs` — version branch.

**Milestone**: Existing 3.3.5a characters appear in 3.4.3 client's character-select.

---

### Phase 5 — World enter: player object update (biggest phase)

**Scope**: `CMSG_PLAYER_LOGIN` → player loads into world. This is where the descriptor-tree `ObjectUpdateBuilder` stops being a stub. **Strongly consider the source-generator strategy below** rather than hand-porting the fork's 3,400 lines verbatim.

**Approach A — Commit-by-commit cherry-pick from fork (fast, maintenance-heavy)**:
- `HermesProxy/World/Objects/Version/V3_4_3_54261/ObjectUpdateBuilder.cs` — full body. The fork's git history now gives us ~16 incremental follow-up commits on top of the initial port (`cc12fd6`), each targeting a specific feature:
  - loot (`1b0a143`, `5a238fb`), combat (`82d2f5e`), levelup (`ad0b0b0`),
    glyphs/talents (`abebf37`), banks/professions (`48590f9`), transport (`f16d350`),
    stats (`aa8c408`), quests (`19ce6cd`, `598c9ae`), chat (`9d5c382`), dc-fix (`156b0c6`), …
- Per-commit review tells us which patches are well-scoped vs. omnibus ("massive fields update and bank fix") that need decomposition. Adapt each to current upstream APIs (verify `WorldPacket.WriteBits` signature, `BitBuffer` / `RoBitBuffer` usage).
- ~3,400 lines of hand-written bit-packing. High risk of off-by-one bugs, but commit-sized patches localize the blast radius.
- Easier to debug each specific opcode issue because code is explicit.

**Approach B — Source-generated descriptor serializers (Recommended for long-term)**:
- **Extend the existing `HermesProxy.SourceGen` project** (shipped in v4.3.0 — netstandard2.0, already referenced by `HermesProxy.csproj` with `OutputItemType="Analyzer"` + `ReferenceOutputAssembly="false"`). It already emits `OpcodeTableGenerator` and `UpdateFieldTableGenerator`. Phase 5 Approach B adds a **third generator** (e.g. `DescriptorSerializerGenerator`) — plumbing is done; only the new generator class + attribute vocabulary are new work.
- **Descriptor attribute vocabulary** (on the existing per-version field enum files):
  - `[UpdateField(Type, Size)]` — already present on field enums; generator reads these.
  - `[UpdateFieldArray(ElementType, ChangeBitWidth)]` for variable-size arrays (QuestLog, VisibleItem, auras).
  - `[UpdateFieldStruct(Type, InnerBlockCount)]` for nested structs (PvPInfo, RestInfo).
  - `[OwnerVisible] / [PartyVisible] / [PublicField]` for the 0x03/0x00 viewer-filter bucketing.
- **Generator emits**: partial methods `WriteCreateObjectData`, `WriteCreateUnitData`, … on a partial `ObjectUpdateBuilder` class. Generated code does the block-mask loops and typed-scalar writes; hand-written partials handle the irregular ~10% (variable arrays, nested structs).
- **Testing strategy leveraging the fork**: the fork's hand-written `ObjectUpdateBuilder.cs` becomes the "known-good output" reference. Generate-and-diff against the fork's version per object type; byte-level drift = generator bug. This is a higher-signal baseline than writing golden files from scratch.
- **Snapshot tests** (`dotnet test --filter Category=GeneratedSerializers`) compare generated `.g.cs` against committed golden files; catches regressions in the generator itself.
- **Benefits**:
  - ~500 lines of descriptor attributes + ~400 lines of hand-written irregular-case partials + ~300 lines of generator logic (vs ~3,400 hand-written).
  - Adding a new field is a 1-line enum edit; serializer regenerates at build time.
  - Many off-by-one bugs become build errors (generator validates `BlockCount * 32 >= MaxFieldIndex`).
  - Reusable: if Blizzard ships 3.4.4+ with field-layout tweaks, only enum edits needed.
- **Revised upfront cost**: **~2-3 days** (was "~1 week") to bootstrap the new generator against the smallest object type (plain `Object`). The netstandard2.0 project, Analyzer wiring, Polyfills, and build-time invocation already exist — the new generator drops into an existing skeleton. Another ~1-2 weeks to expand to Unit/Player/ActivePlayer.
- **Debug tip**: `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` is already configured in `HermesProxy.csproj`; generated `.g.cs` lands under `obj/GeneratedFiles/`.

**Dispatcher**:
- `HermesProxy/World/Objects/UpdateObject.cs` or equivalent — route 3.4.3 to the new builder.

**Tests**:
- `HermesProxy.Tests/World/ObjectUpdateTests.cs` — add 3.4.3 test cases.
- `HermesProxy.Tests/World/UpdateFieldsArrayTests.cs` — **parameterize** for legacy versions only. V3_4_3 does not use the legacy field-array system and should live in a new `V3_4_3_ObjectUpdateBuilderTests.cs` with byte-level golden captures.

**Milestone**: 3.4.3 client enters world, sees its own character at correct position, can move.

**Risk**: off-by-one in any `WriteBits(mask, N)` corrupts the stream. Mitigation: generate ground-truth captures from CMaNGOS via WowPacketParser and diff HermesProxy output byte-for-byte in tests. Approach B (source-gen) materially reduces this risk class by making width mismatches compile-time errors.

**Recommended path — Hybrid (A → then incrementally B)**:

After v0.1 shipped (character-select through Phases 0–4), straight Approach B was scoped as "~2–3 days bootstrap, then 1–2 weeks expansion before world-enter works." That's a long block of the protocol being unreachable while scaffolding the generator. A hybrid path gets users into the world faster AND ends up at the same clean declarative codebase:

1. **Phase 5a — Hand-port verbatim.** Copy the fork's ~3,400-line `ObjectUpdateBuilder` wholesale into `HermesProxy/World/Objects/Version/V3_4_3_54261/ObjectUpdateBuilder.cs`, adapting call sites to current upstream APIs (`WorldPacket.WriteBits` signature, `BitBuffer` / `RoBitBuffer` usage). One big PR, high-confidence working baseline. **World-enter works after this commit.** Everything after 5a is refactoring-to-cleaner-infra, not protocol progress.

2. **Phases 5b–5e — Incrementally replace sections with generator output.** Each sub-phase expands the source generator's capability along one complexity dimension, using the 5a hand-port as the **test oracle**: snapshot-capture the hand-port's byte output for a known `{ObjectData,UnitData,PlayerData,…}` input, replace the hand-written section with generated code, assert byte-equivalence. This is exactly the "generate-and-diff against the fork's hand-written version" strategy from Approach B above, except now the "fork's hand-written version" lives in our own repo and is unit-testable.

    | Sub-phase | Scope | Generator capability gained | What 5a code gets deleted |
    |---|---|---|---|
    | **5b** | Create-path scalars for all object types (Object already seeded by the bootstrap PR; add Item, Unit, Player, ActivePlayer, GameObject, DynamicObject, Corpse, Container, AreaTrigger, SceneObject, Conversation). | Existing `[DescriptorCreateField]` vocabulary. No new generator features — just more annotations and more `WriteCreate*Data` emissions. | All 12 hand-written `WriteCreate*Data` methods |
    | **5c** | Update-path bit-mask cascade (`uint mask = 0; if (field.HasValue) mask \|= N; data.WriteBits(mask, N)`). | New `[DescriptorUpdateField(bitIndex)]` attribute + mask-width validation in the generator. | All 4 hand-written `WriteValuesUpdate*Data` methods — the big ones (`WriteUpdateUnitData` 618 LoC, `WriteUpdateActivePlayerData` 816 LoC) |
    | **5d** | Variable-size arrays (QuestLog, VisibleItem, Power/MaxPower, skills) + nested structs (RestInfo, SkillInfo, PvPInfo). | New `[DescriptorArray(elementCount)]` + `[DescriptorStruct]` attributes. | `WriteUpdateSkillInfo` helper, per-element array loops, nested-struct blocks |
    | **5e** | Viewer-filter bucketing (0x03 owner / 0x00 non-owner byte + `IsOwner`-guarded per-field writes). | New `[DescriptorOwnerOnly]` / `[DescriptorPartyOnly]` attributes. | All `if (IsOwner)` per-field conditionals |

    **At the end of 5e, the hand-port is entirely replaced by attribute-annotated enums + generator emission.** Each 5b–5e PR is self-contained and reviewable; gameplay continues working throughout because 5a stays canonical until each section is proven byte-equivalent.

3. **Bootstrap PR context**: the Phase 5 generator scaffolding (project plumbing, `[DescriptorCreateField]` attribute, `ObjectUpdateBuilderGenerator`, Verify-based snapshot test, `WriteCreateObjectData` for V3_4_3_54261) was seeded on a separate branch to prove the generator pipeline before 5a ships. That seed feeds directly into 5b when the time comes.

**Fallback**: if any 5b–5e PR surfaces a fork pattern that genuinely resists declarative description (e.g. some hand-crafted bit-field that can't be expressed as a clean attribute), that section stays in 5a's hand-port form indefinitely. The vocabulary expands to cover ~90% of the fork, the remaining ~10% stays hand-written — both coexist on the same partial class. This is exactly what wotlk.md's Approach B original notes anticipated with the "hand-written partials handle the irregular ~10%" clause.

---

### Phase 5a — DONE (PR #50, merge `5b9fcc0`, 2026-04-26)

Shipped: V3_4_3.54261 client connects through HermesProxy to CMaNGOS 3.3.5a, authenticates, sees the character list, enters the world, plays, and quits cleanly. V1_14 (Vanilla) and V2_5 (TBC) paths smoke-tested unaffected; `dotnet test` 296/296 passing.

Phase 5a deliberately left **scaffolding workarounds** so subsequent sub-phases have obvious entry points. Each one is tagged in source with a `FIXME(phase5*)` marker — `grep -rn "FIXME(phase5" --include="*.cs"` produces the full list:

| Sub-phase | FIXME count | Where | What it gates |
|---|---|---|---|
| **5a-7b** | DONE | — | V3_4_3 hotfix data — landed in 5a-7b PR; real item names + spell names now reach the client |
| **5a-7c-i** | PARTIAL | — | Static GameObjects (mailboxes/doodads/chests) accepted by V3_4_3 client via `GAMEOBJECT_BYTES_1` unpacker. Transport/MOTransport re-filtered for V3_4_3 because cmangos's create-block position is empirically (0,0,0) — V3_4_3 client rejects. Empty Values updates suppressed for V3_4_3 to stop Player-reject loop until 5a-7d. Login unblocked; zeppelins/elevators absent (MOTransport position fix). |
| **5a-7c-ii** | NEXT | 1× `UpdateHandler.cs:162/229` (ItemContainer branches), 1× `UpdateHandler.cs:1211` (`GetSlotGuidValue`) | cmangos's 0x4700 ItemContainer high-guid for equipped items — needs `WriteCreateItemData` treatment. Players still appear naked. |
| **MOTransport position fix** | NEW | `UpdateHandler.cs:162/229` (Transport/MOTransport branches), `UpdateHandler.cs:~1098` (StationaryObject reader) | Read MOTransport position from cmangos's later GAMEOBJECT_POS_X/Y/Z update-field deltas, OR convert Stationary→Movement create block. Unblocks zeppelins/elevators rendering. |
| **5a-7c-iv** | 1 | `HighGuid.cs:45` | Unknown legacy-high fallback warn — only relevant after we audit what cmangos actually emits at world-enter. |
| **5a-7d** | 1 | `ObjectUpdateBuilder.cs:1188` (`WriteValuesUpdate`), `UpdatePackets.cs:Write` (drops empty Values for V3_4_3) | Values / partial-update path — no live HP bars, aura ticks, combat propagation. The empty-block suppress in `UpdateObject.Write` is paired with the stub and gets lifted alongside the real implementation. |
| **5b** | 2 | `ObjectField.cs`, `ObjectUpdateBuilderGeneratorTests.cs` | Source-generator restoration → byte-equivalence audit lane against the 5a hand-port |

All V3_4_3-specific code (filters, hotfix stub, etc.) is gated on `ModernVersion.Build == ClientVersionBuild.V3_4_3_54261` so V1_14/V2_5 paths fall through unchanged.

---

### Phase 5a-7 — sub-phase follow-ups (next on deck)

Recommended execution order: **~~7b~~ → 7c-i (PARTIAL) → 7c-ii → 7c-iii → 7d → 5b**. 7b shipped 2026-04-26. 7c-i landed the static-GameObject create path + the empty-Values suppression on 2026-04-28; remaining MOTransport position work pulled out into 7c-iii. 7c-ii (Item / 0x4700 ItemContainer) is next on deck — naked-AFK → equipped player; 7c-iii unblocks zeppelins/elevators rendering; 7d makes combat feel live; 5b sets up the source-gen lane for the eventual hand-port deletion.

#### Phase 5a-7b — DONE (real hotfix data via wago.tools, 2026-04-26)

Shipped: all 18 `HermesProxy/CSV/Hotfix/*3.csv` files regenerated from wago.tools at `?build=3.4.3.54261` (the previous files were byte-identical TBC carryovers labeled WotLK). Data set: `AreaTrigger`, `SkillLine`, `SkillLineAbility`, `SkillRaceClassInfo`, `Spell`, `SpellName`, `SpellLevels`, `SpellAuraOptions`, `SpellMisc`, `SpellEffect`, `SpellXSpellVisual`, `Item`, `ItemSparse`, `ItemEffect`, `ItemDisplayInfo`, `CreatureDisplayInfo`, `CreatureDisplayInfoExtra`, `CreatureDisplayInfoOption`. Total ~700K hotfix records.

Code changes:
- `HotfixHandler.HandleHotfixRequest` — V3_4_3-gated empty-response stub deleted; V3_4_3 path now falls through to the existing `GameData.Hotfixes.TryGetValue` per-record reply loop (same path TBC/Era already use).
- `GameData.cs` `Hotfix*Begin` constants — bumped from 10K to 100K spacing. The 10K spacing fit TBC stub data but collided under WotLK row counts (e.g. SkillLineAbility @ 10244 rows overflows into Spell's 140000 base). Bases now start at 1M with 100K gaps so every WotLK table fits comfortably.
- Column projection per loader's positional `row[N]` order: 13 of 18 tables match wago's column order verbatim; 5 (`SpellMisc`, `ItemSparse`, `Item`, `ItemDisplayInfo`, `CreatureDisplayInfo`) need explicit reorder + drop of wago-only columns. Negative bitmask values (`RaceMask=-1` "all races", `Attributes_*` flag bitmasks with high bit set) coerced via 2's-complement to the loader's unsigned types so `uint.Parse("4294967295")` accepts them — same wire bits as `(uint)-1`.
- `AvailableHotfixes` packet — V3_4_3 send path suppresses the per-record enumeration (sends `count=0`). At ~700K records the full list produced a ~5.6 MB `SMSG_AVAILABLE_HOTFIXES` that stalled the V3_4_3 client at the glue-screen loading bar (character preview never rendered). The client lazy-fetches what it needs via `CMSG_DB_QUERY_BULK` and `CMSG_HOTFIX_REQUEST` — both already return real data. V1_14 / V2_5 paths keep the legacy enumeration since their record counts are tiny.

Known limitations:
- **2306 ItemSparse rows skipped** (5% of WotLK item set, mostly Naxx/Ulduar/ICC raid gear with stats > 127). Cause: loader parses `StatValue1..10` as `sbyte` (TBC-era field width). Future work: widen loader to `short` (parse) + `WriteInt16` (wire) — fork's loader uses `WriteInt16` for these but the upstream `WriteInt8` is a latent TBC bug. Affects raid-tier tooltips only; quest gear / dungeon gear unaffected.
- The skipped raid items still get tooltips via the legacy `SMSG_ITEM_QUERY_SINGLE_RESPONSE` path; only the modern `SMSG_HOTFIX_CONNECT` slot is empty for them.

Verification:
- `dotnet build` clean (43 pre-existing HPSG001 warnings, 0 errors)
- `dotnet test` 296 passed / 1 skipped (pre-existing 5b-gated generator test) / 0 failed
- HermesProxy boots with `--set ClientBuild=V3_4_3_54261` and `LoadEverything()` completes in ~1.8 s; `[Hotfix] CMSG_HOTFIX_REQUEST` log line shows `GameData.Hotfixes total available = ~700000`
- `grep -rn "FIXME(phase5a-7b)" --include="*.cs"` returns no hits

#### Phase 5a-7c-i — PARTIAL (static GameObjects + Values suppression, 2026-04-28)

Shipped: V3_4_3 client accepts `CreateObject1`/`CreateObject2` for **static `GameObject`** types (mailboxes, doodads, chests). The `Player` reject-on-empty-Values loop is also gone. **Login is unblocked.** Zeppelins/elevators (Transport/MOTransport) are temporarily filtered out — see MOTransport position fix.

Root cause (verified empirically against canonical CypherCore capture and proxy-side WPP capture):
- 3.3.5a cmangos / TC335 / AzerothCore servers pack `State` (byte 0), `TypeID` (byte 1), `ArtKit` (byte 2), `AnimProgress` (byte 3) into a single `GAMEOBJECT_BYTES_1` uint32 field rather than per-byte individual `UpdateFields` slots. Without an unpacker, `WriteCreateGameObjectData` shipped `TypeID=0` (`GAMEOBJECT_TYPE_DOOR`) and the V3_4_3 client rejected static GO creates with `CMSG_OBJECT_UPDATE_FAILED`.
- For MOTransports specifically, the unpacker fixes the `TypeID`/`State` fields, but cmangos's create-block position is empirically `(0,0,0)` for these (legacy server defers position to subsequent update-field deltas). The V3_4_3 client rejects a Stationary GameObject create with `Position=(0,0,0)` and `Orientation!=0`, looping forever as the legacy server retries. **Properly fixing this is MOTransport position fix scope.**
- The empty `WriteValuesUpdate` stub at `ObjectUpdateBuilder.cs:1188` was emitting a `mask=0` Values block that the V3_4_3 client also rejected with `CMSG_OBJECT_UPDATE_FAILED highType=Player`, triggering a server resend → infinite loop. Suppressing empty Values entirely for V3_4_3 stops this loop until 5a-7d implements the real body.

Code changes:
- `UpdateHandler.cs` GameObject branch (~line 3061) — added `GAMEOBJECT_BYTES_1` unpacker that extracts `State`/`TypeID`/`ArtKit`/`PercentHealth` from the packed uint32 before falling through to the existing individual-field reads. TC343 `GameObjectData` renamed byte 3 from `AnimProgress` to `PercentHealth` — the unpacker writes into `PercentHealth` to match.
- `UpdateHandler.cs:162/229` — for V3_4_3, Static `GameObject` branch forwards (the unpacker populates the fields). `Transport`/`MOTransport` branches re-filter with a `[Phase5a7cTrace] Skipping ... pending MOTransport position fix` log line. `ItemContainer` (0x4700) skip stays — that's 5a-7c-ii's scope.
- `UpdatePackets.cs UpdateObject.Write` — for V3_4_3, drops Values updates from `ObjectUpdates` before counting/serializing. Paired with the empty-stub at `ObjectUpdateBuilder.cs:1188`; both go away when 5a-7d implements the real Values body.
- `ObjectUpdateBuilder.cs:131` — removed the unconditional `CreateObjectBits.GameObject` set; canonical CypherCore capture shows the bit must stay false for typical GameObjects.
- `MovementInfo.cs:47` — `Rotation` defaults to `Quaternion.Identity` (was implicit (0,0,0,0)) so static GOs without a server-supplied rotation pass V3_4_3 sanity checks.
- `MiscHandler.HandleObjectUpdateFailed` enhanced to log `highType` + `entry` of the failing modern guid — diagnostic gold mine for any remaining client-side rejections.

Verification:
- `dotnet build -c Release` clean (43 pre-existing HPSG001 warnings, 0 errors)
- `dotnet test` 296/0/1 (no new tests)
- Smoke test: V3_4_3 client → CMaNGOS WotLK backend, world-enter completes. Proxy log shows `[Phase5a7cTrace] Skipping {Transport|MOTransport} for V3_4_3 (Position=0 placeholder; pending MOTransport position fix)` once per zeppelin/elevator at world-enter, then **silence** — no recurring `CMSG_OBJECT_UPDATE_FAILED` lines.
- Diagnostic capture workflow: a proxy-side WPP-parsed capture is at `HermesProxy\bin\Release\PacketsLog\modern_54261_*_parsed.txt` after enabling `DiagnosticsOptions.PacketsLog`; the canonical CypherCore reference is at `World_parsed.txt:18181-18225` (static GO CreateObject1).

#### Phase 5a-7c-ii — Item / 0x4700 ItemContainer (next)

**Why next**: cmangos packs equipped/container items into a non-standard `0x4700` `ItemContainer` high-guid that the V3_4_3 client doesn't understand. `WriteCreateItemData` exists at `ObjectUpdateBuilder.cs:307` but isn't exercised because the type is filtered. Also gates `GetSlotGuidValue` returning `Empty` for those items at `UpdateHandler.cs:1211` (so the player's inventory slots show the right guids). Fixing this unblocks the "naked character" issue at character-select.

**Scope**: walk `WriteCreateItemData`, capture-validate against WPP V3_4_0_45166 reference parser, fix divergence. Drop the `ItemContainer` filter at `UpdateHandler.cs:84/162/229`. Restore the real legacy guid in `GetSlotGuidValue` for `ItemContainer` (delete the `Empty` fallback).

#### Phase MOTransport position fix — Transport / MOTransport position (deferred from 5a-7c-i)

**Why**: zeppelins, elevators, boats. These are filtered for V3_4_3 in 5a-7c-i because cmangos's create-block position is `(0,0,0)` — see the FIXME at `UpdateHandler.cs:162/229` (Transport/MOTransport branches). Without position, the V3_4_3 client rejects every Stationary GameObject create with `CMSG_OBJECT_UPDATE_FAILED`.

**Scope** (one of these — investigate which is correct empirically):
- **Option A — Aggregate position from update-field deltas before forwarding**: cmangos likely sends the actual position via `GAMEOBJECT_POS_X/Y/Z` Values updates after the CreateObject. Buffer the CreateObject until those arrive, populate `MoveInfo.Position`, then forward. Requires `UpdateHandler.cs` state per pending-MOTransport-guid.
- **Option B — Convert MOTransport CreateObject to use a Movement block** instead of Stationary. Set `CreateObjectBits.MovementUpdate` instead of `Stationary`, populate `MoveInfo` with full living-block defaults. May require examining how V3_4_3 actually serializes a moving Transport — check fork's `ObjectUpdateBuilder.cs:2165` `MovementTransport` / `ServerTime` flag conditions.
- **Option C — Capture cmangos's actual create-block bytes** and confirm whether Position really is sent as zero or whether `UpdateHandler.cs:1098-1103` (`UpdateFlag.StationaryObject` branch) misses a flag. Use cmangos's own packet logger or `tcpdump` between proxy and legacy server.

**Drop the filter** at `UpdateHandler.cs:162/229` once position is correctly populated. Smoke test: zeppelins / Stormwind harbor boats / Undercity elevator render and animate correctly for the V3_4_3 client.

#### Phase 5a-7d — Implement Values / partial-update path

**Why third**: gates live combat / HP bars / aura ticks / movement deltas. Currently empty (`WriteUInt32(0)`) so cmangos has to re-send full CreateObject blocks instead of partial deltas; the client gets correct state but with extra latency / bandwidth.

**Scope**: ~1700 LOC of the fork's bit-mask serialization for partial updates, ported using the same pattern as 5a's WriteCreate path. Source: `X:\Programming\HermesProxy-WOTLK\HermesProxy\World\Objects\Version\V3_4_3_54261\ObjectUpdateBuilder.cs` (the `WriteValuesUpdate*Data` family of methods).

**Estimated scope**: 1 large PR or 2 medium PRs (UnitData/PlayerData together, ActivePlayerData solo). High off-by-one risk — needs WPP capture-diff oracle.

#### Phase 5b — Source-generator restoration

**Why last**: pure infrastructure change with no user-visible impact. Unblocked once 5a is stable and 7b/7c/7d aren't iterating heavily on the hand-port (which would invalidate the byte-equivalence snapshots). Restores `[DescriptorCreateField]` attributes in `HermesProxy/World/Enums/V3_4_3_54261/ObjectField.cs`, re-enables the `WriteCreateObjectData_V3_4_3_54261` Verify snapshot test, then per the 5b–5e roadmap above, incrementally replaces hand-written sections with generator output using the 5a hand-port as the byte-equivalence test oracle.

---

### Phase 6 — Surrounding objects

**Scope**: Units/creatures, GameObjects, Items from other players. No new files; bugfixing the Phase 5 builder in a populated area.

**Milestone**: NPCs and other players render correctly in Dalaran / Orgrimmar.

---

### Phase 7 — Inventory remapping

**Scope**: Wire `AdjustInventorySlot` at the right translation points for bag/bank/keyring/buyback.

**Files**:
- `HermesProxy/World/Server/WorldSocket.cs` — call sites per fork.
- `HermesProxy/World/Server/PacketHandlers/ItemHandler.cs` — bidirectional slot mapping.

**Milestone**: Bags/bank display items; moves work in both directions.

---

### Phase 8 — Spells and auras

**Scope**: Per-version spell-book serialization and WotLK aura flag conversion.

**Files**:
- `HermesProxy/World/Server/Packets/ModernInitialSpells.cs` — 3.4.3 adds `isFavorite`, `isPassive` bits per spell entry.
- `HermesProxy/VersionChecker.cs` / `ModernVersion.ConvertAuraFlags` — branch for WotLK (AuraFlagsWotLK → AuraFlagsModern, active-effect bit extraction, negative-vs-positive rule).

**Milestone**: Spellbook populates, auras display with correct coloring.

---

### Phase 9+ — Feature expansion

Ordered by user-facing impact: combat/damage → quests → chat → grouping → trading → mail → guild → auction → social. Each is a small handler-level PR. No further framework changes expected.

---

## Cross-cutting constraints

- **Never** edit `HermesProxy/BnetServer/Networking/BnetTcpSession.cs` — it holds the pooled-buffer work from recent PRs.
- **Never** replace `Directory.Packages.props` — central package management stays.
- **Never** drop `PublishTrimmed=true` or the trimmer root config — recent commits (`e6c340e`, `7cfb87b`, `3f4c548`, `91b4c7a`, `dabab91`) exist specifically to keep this working. Verify `dotnet publish -p:PublishTrimmed=true` at end of every phase.
- **Reuse existing crypto pattern**: `World/Client/VanillaWorldCrypt.cs` / `TbcWorldCrypt.cs` are standalone files — follow the same shape for `WotlkWorldCrypt.cs` rather than inlining.
- **DBC/CSV data**: **first hit is Phase 0, not Phase 6.** `GameData.cs` uses `LegacyVersion.ExpansionVersion`-keyed and `ModernVersion.ExpansionVersion`-keyed file-name patterns, so switching to a WotLK backend activates `*3.csv` lookups immediately. See the dedicated "CSV/DBC data strategy" section below — not a blanket "don't copy fork", it's a nuanced per-file decision (most of the fork's `*3.csv` files are TBC duplicates; a few are genuine WotLK data).

---

## CSV/DBC data strategy for V3_4_3_54261

### File-name convention (from `HermesProxy/World/GameData.cs`)

Three patterns coexist under `HermesProxy/CSV/`:

| Pattern | Example | Keyed on |
|---|---|---|
| `{Name}{ModernVersion.ExpansionVersion}.csv` | `ItemSparse2.csv` | Modern-client expansion (1=Era, 2=TBC, 3=**WotLK**) |
| `{Name}{LegacyVersion.ExpansionVersion}.csv` | `BroadcastTexts2.csv` | Legacy-server expansion (same numbering) |
| `{Name}.csv` | `AreaNames.csv`, `LearnSpells.csv`, `RaceFaction.csv` | version-agnostic |

Adding WotLK activates `*3.csv` lookups on both sides:
- **Phase 0 (legacy WotLK only, modern still 2.5.x/1.14.x)** triggers the `LegacyVersion`-keyed subset: `BroadcastTexts3`, `CreatureModelCollisionHeightsModern3`, `Transports3`, `SpellEffectPoints3`, `StackableAuras3`, `AuraSpells3`.
- **Phase 1 (modern 3.4.3 registered)** triggers the full `ModernVersion`-keyed set (~15 tables).

### What the fork ships — and its gotcha

`X:\Programming\HermesProxy-WOTLK\HermesProxy\CSV\` includes a full `*3.csv` set. md5-comparing all 24 `*2.csv`/`*3.csv` pairs surfaces a shortcut:

- **21 of 24 are byte-identical to `*2.csv`** (TBC data labeled WotLK): `Item3`, `ItemSparse3`, `ItemAppearance3`, `ItemEffect3`, `ItemDisplayIdToFileDataId3`, `ItemModifiedAppearance3`, `ItemSpellsData3`, `ItemEnchantVisuals3`, `AuraSpells3`, `BroadcastTexts3`, `CreatureModelCollisionHeightsModern3`, `MeleeSpells3`, `MountSpells3`, `QuestV2_3`, `SpellEffectPoints3`, `SpellVisuals3`, `StackableAuras3`, `TaxiNodes3`, `TaxiPath3`, `TaxiPathNode3`, `AutoRepeatSpells3`.
- **3 legitimately different** — take these as-is from fork:
  - `Gems3.csv` — WotLK gem colors/cuts.
  - `ItemIdToDisplayId3.csv` — expanded WotLK item catalogue.
  - `Transports3.csv` — WotLK transports (Dalaran, ICC, …).
- `BuildAuthSeeds.csv` — fork adds the V3_4_3_54261 row (seed already present in our `appsettings.json`).
- Fork also ships `MountSpells2.csv` / `MountSpells3.csv` that upstream doesn't — verify the loader exists before porting.

### Phased strategy

1. **Phase 0** — copy the 6 `LegacyVersion`-keyed `*3.csv` files (plus `Transports3.csv` from fork's real WotLK data) and extend `BuildAuthSeeds.csv`. ~10 minutes. No wago.tools regen yet.
2. **Phase 1 (Step A, bootstrap)** — copy the full `ModernVersion`-keyed `*3.csv` set from the fork. Accepts TBC-placeholder data for 12 tables so the app loads and Phase 1's "no crash" milestone passes.
3. **Phase 1 (Step B, pre-Phase-5 regen)** — regenerate from wago.tools at `build=3.4.3.54261` via the `dbc-lookup` skill, in descending impact order:
   1. `ItemSparse3.csv` — tooltips, stats, names (most user-visible).
   2. `Item3.csv` — class/subclass taxonomy.
   3. `ItemEffect3.csv`, `ItemSpellsData3.csv` — item use-effects.
   4. `ItemAppearance3.csv`, `ItemModifiedAppearance3.csv`, `ItemDisplayIdToFileDataId3.csv` — rendering.
   5. `QuestV2_3.csv` — quest text/rewards.
   6. `SpellVisuals3.csv` — spell animations.
   7. `BroadcastTexts3.csv` — NPC dialogue.
   8. `TaxiPath3.csv`, `TaxiNodes3.csv`, `TaxiPathNode3.csv` — flight paths.
4. **Phase 5+** — as gameplay bugs surface against CMaNGOS wotlk, regenerate the specific failing table. Treat as a debugging loop, not a pre-emptive batch. The remaining small TBC-duplicate tables (`AuraSpells3`, `AutoRepeatSpells3`, `MeleeSpells3`, `MountSpells3`, `SpellEffectPoints3`, `StackableAuras3`, `CreatureModelCollisionHeightsModern3`, `ItemEnchantVisuals3`) stay as carry-overs until a concrete bug demands regeneration.

### Regeneration verification (per `dbc-lookup` skill)

- Pull target table from wago.tools at `?build=3.4.3.54261`.
- Compare column order and types against what the `Load*` method in `World/GameData.cs` expects (each loader reads columns in a deterministic order — a column-order mismatch between wago's current export schema and the loader is the common failure mode).
- Spot-check 3-5 known-WotLK rows per table to confirm the export actually contains 3.x data (e.g. ItemID 49426 *Emblem of Frost* exists in WotLK; absent rows = wrong build filter).

---

## Critical files to reuse (existing in upstream)

- `HermesProxy/VersionChecker.cs` — central routing hub; the `ModernVersion` / `LegacyVersion` sibling classes sit here too.
- `HermesProxy/World/Enums/Opcodes.cs` — opcode enum routing.
- `HermesProxy/World/Enums/V3_3_5_12340/Opcode.cs` — legacy WotLK opcode enum (already shipped, used by Phase 0).
- `HermesProxy/World/Client/LegacyWorldCrypt.cs`, `VanillaWorldCrypt.cs`, `TbcWorldCrypt.cs` — crypto pattern to follow in Phase 3.
- `HermesProxy/World/Objects/Version/V2_5_3_41750/ObjectUpdateBuilder.cs` — reference for *legacy* builder shape (NOT the template for V3_4_3, but useful for understanding `WriteToPacket` dispatch).

## Reference fork (read-only)

- `X:\Programming\HermesProxy-WOTLK` — source of V3_4_3-specific code to cherry-pick. Origin `github.com/advocaite/HermesProxy-WOTLK`. **Now has full 46-commit git history** (previously believed to be a static snapshot). Do not merge wholesale — the fork regressed the upstream perf/trim work we want to preserve.
- **Key commits**:
  - `cc12fd6` ("opps i did a boo boo") — the initial V3_4_3_54261 port. Adds the 26 `World/Enums/V3_4_3_54261/` files + `CreateObjectBits.cs` + the full 3,419-line `ObjectUpdateBuilder.cs` in one shot. +4,951 lines across 28 files. **This is the Phase 1 baseline.**
  - ~16 follow-up commits each patch `ObjectUpdateBuilder.cs` for a specific feature — see Phase 5 Approach A for the full list. These are the **Phase 5 cherry-pick sequence** if we go hand-port.
- **For Approach B (source-gen)**: the fork's hand-written `ObjectUpdateBuilder.cs` at HEAD is the "known-good output" reference for generator validation. Generate-and-diff against it per object type.

### Fork is actively shipped and has a public user base (2026-04-23 note)

An OwnedCore release thread ([link](https://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-emulator-servers/wow-emu-general-releases/1104335-wow-3-4-3-classic-working-client-any-server-new-hermesproxy.html)) is promoting this fork to end-users under the title *"WoW 3.4.3 Classic Working Client for any server with NEW HermesProxy"*. Relevant facts:

- Fork is actively shipping **nightly binaries via GitHub Releases** — latest release `build-20260420-023438` (2026-04-20), 10 releases total, 43 commits at the time of check.
- Public scope matches the plan here exactly: 3.4.3 modern client → 3.3.5a legacy backend (plus passthrough support for 1.14.x/2.5.x → 1.12.1/2.4.3).
- **The end-to-end pipeline is validated in production** — users are running 3.4.3 clients against 3.3.5a emulators right now via this fork. Our Phases 0-5 are not speculative; they're tracing a known-working path.

**Implications for our plan:**

1. **User-base expectation management** — when our v0.1 WotLK support ships, users coming from the fork will compare feature parity. Ship the v0.1 PR with an explicit "works / doesn't work" matrix so expectations are clear (character-select: yes; world-enter: no; etc.).
2. **No plan change** — we're still cherry-picking from the same fork. The commit-by-commit map (`cc12fd6` + ~16 follow-ups) remains the Phase 5 guide.
3. **Look at the fork's CI** before opening Phase 1 PR. If they're producing nightly release binaries successfully, their `.github/workflows/` may contain patterns worth borrowing against our own `Release.yml` (e.g. self-contained publish flags for Windows/Linux/macOS). **Do not** adopt anything that would regress our `PublishTrimmed=true` / `BnetTcpSession.cs` / `Directory.Packages.props` posture.

---

## Reference packet captures (TC 3.4.3 server-side)

For ground-truth V3_4_3 wire-format references (canonical for diffs against HermesProxy output), capture on a working TrinityCore 3.4.3.54261 server — no proxy in the path.

1. In TC's `worldserver.conf`, set `PacketLogFile = "World.pkt"` (extension must be `.pkt`; output lands at `LogsDir/World.pkt`, default `Logs/`). Restart `worldserver.exe`.
2. Play with the V3_4_3 client. Stop with `server shutdown 1` to flush.
3. Parse with the WPP fork at `X:\Programming\RioMcBoo\WowPacketParser` (already pinned to `LangVersion=12`):

   ```powershell
   $wpp = "X:\Programming\RioMcBoo\WowPacketParser\WowPacketParser\bin\Release\WowPacketParser.exe"
   & $wpp "<TC build dir>\Logs\World.pkt"
   ```

   Output is `World.txt` next to the input — full field-level decode. `V3_4_3_54261` is auto-detected from the PKT 3.1 header; force-set `<add key="ClientBuild" value="V3_4_3_54261"/>` in WPP's `App.config` if needed. Filter via `<add key="Filters" value="SMSG_UPDATE_OBJECT,..."/>` to narrow opcodes.

Captures contain SRP6 session keys + account hashes — **do not commit or share**. Rotate `World.pkt` between sessions to keep diffs clean.

Fallback: HermesProxy's own `SniffFile.cs` writes PKT 2.1 to `PacketsLog/` (gated by `DiagnosticsOptions.PacketsLog`, default on) — useful for debugging proxy output, not for ground-truth TC behavior.

---

## Decisions (confirmed with user 2026-04-21)

1. **Primary backend target**: **CMaNGOS wotlk** (https://github.com/cmangos/mangos-wotlk). All smoke tests and byte-level captures come from a local CMaNGOS instance running against client build 3.3.5a/12340. TrinityCore 3.3.5 / AzerothCore are post-1.0 hardening only. Note: CMaNGOS emits subtly different legacy UpdateFields values than TrinityCore in some places (aura serialization, guild-perks, item slots) — when Phase 5+ work starts, capture-based validation must use CMaNGOS packet dumps, not TrinityCore dumps.
2. **Initial done-scope**: **Reach character-select (through Phase 4)**. This is the ship-it bar for a "v0.1 WotLK Classic support" milestone. Phases 5+ (world entry, gameplay) are follow-on work after the character-select milestone lands.
3. **Phase 0 ships as a standalone PR first** before any 3.4.3 client work begins. Validates existing 3.3.5a plumbing with a known-good 2.5.x client before stacking new-client risk on top.
4. **Protobuf binding strategy**: in Phase 2, do a 30-minute diff audit of fork's `*.pb.cs` vs upstream. Port only real `.proto` schema changes; ignore compiler-version churn.

## Consequence of the "character-select" scope

The phase list is unchanged, but the **critical path to v0.1** is Phases 0 → 1 → 2 → 3 → 4. Phase 5 (ObjectUpdateBuilder, ~3,400 lines of bit-packing) is **deferred** past v0.1. This dramatically de-risks the initial milestone — the biggest unknown in the whole effort (the descriptor-tree serializer) doesn't block the first ship.

However: **Phase 3's AUTH_RESPONSE** and **Phase 4's SMSG_ENUM_CHARACTERS_RESULT** both use the new descriptor-tree format for *their specific structures* (account-data blob, character entries). Those structures are small enough (~dozens of lines each) to write by hand from WowPacketParser reference without needing the full ObjectUpdateBuilder. The initial milestone is therefore achievable without any work on `ObjectUpdateBuilder.cs` beyond the stub.

### Concrete Week 1 task order

Day 1 — Phase 0 groundwork
1. Read `World/Enums/V3_3_5_12340/Opcode.cs` + `UpdateFields.cs` end-to-end; note gaps vs fork's equivalents.
2. Flip `VersionChecker.cs:38` to `return true`.
3. Add expansion-3 case in `GetBestLegacyVersion`.
4. Build.

Day 2 — Phase 0 smoke test
5. Spin up CMaNGOS wotlk backend locally (https://github.com/cmangos/mangos-wotlk + cmangos/classic-db or equivalent world DB).
6. Connect with 2.5.x TBC Classic client, `ServerBuild=V3_3_5a_12340`.
7. Triage unhandled-opcode logs; open Phase 0 PR.

Day 3 — Phase 1 data port
8. Add `ClientVersionBuild.V3_4_3_54261 = 54261`.
9. Port 26 files under `World/Enums/V3_4_3_54261/` verbatim from fork (data, not logic).
10. Port `World/Objects/Version/V3_4_3_54261/CreateObjectBits.cs`.
11. Create stub `ObjectUpdateBuilder.cs` (all `NotImplementedException`).

Day 4 — Phase 1 routing
12. Add cases in `Opcodes.GetOpcodesDefiningBuild` / `GetOpcodesEnumForVersion`.
13. Add cases in `ModernVersion.GetUpdateFieldsDefiningBuild`, `GetResponseCodesEnum`, `GetAccountDataCount`, `GetGameObjectStateAnimId`, `AdjustInventorySlot`.
14. Add case in `VersionChecker.IsSupportedModernVersion`.

Day 5 — Phase 1 verification
15. Build with `PublishTrimmed=true`; confirm the new per-version enums are picked up by `HermesProxy.SourceGen` at compile time (no trimmer-root edits needed as of v4.3.0 — reflection-based loading is gone). Inspect `obj/GeneratedFiles/` for the emitted `V3_4_3_54261` entries.
16. Run full test suite — must pass unchanged.
17. Add opcode-round-trip smoke test for V3_4_3_54261.
18. Open Phase 1 PR.

---

## Verification (end-to-end)

- **Build**: `dotnet build` + `dotnet publish -c Release -p:PublishTrimmed=true` must succeed after every phase.
- **Tests**: `dotnet test` must pass after every phase. Parameterize existing version-specific tests per Phase 1; add V3_4_3-specific tests in Phase 5.
- **Manual per phase**:
  - Phase 0: 2.5.x/1.14.x client → HermesProxy → 3.3.5a TrinityCore; reach character-select.
  - Phase 2: 3.4.3 client completes BNet auth, sees realm list.
  - Phase 3: 3.4.3 client reaches (empty) character-select.
  - Phase 4: character list populated.
  - Phase 5a (DONE, PR #50): enter world, see own character (naked, no GameObjects/Transports — those are gated by FIXME(phase5a-7c)).
  - Phase 5a-7b (DONE, 2026-04-26): real item names + tooltips on character-select equipment slots; ~700K hotfix records loaded from wago.tools build 3.4.3.54261.
  - Phase 5a-7c (NEXT): equipped items render, mailboxes/chests/zeppelins visible.
  - Phase 5a-7d: live HP bars, aura ticks, combat propagation.
  - Phase 6: NPCs and other players render.
  - Phase 7: bags/bank usable.
  - Phase 8: spellbook + auras correct.
- **Capture-based validation** (Phase 5+): use WowPacketParser (per memory: reference repo for protocol truth) against captures from **CMaNGOS wotlk** (the chosen backend) to diff HermesProxy's object-update output byte-for-byte. Do not mix TrinityCore and CMaNGOS captures in the same test suite — their legacy field emissions differ.
