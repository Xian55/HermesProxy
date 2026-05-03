# WoW WotLK Classic (3.4.3.54261) Client Support in HermesProxy

The **WotLK Classic retail client (build 3.4.3.54261)** connects through HermesProxy to a legacy **WotLK 3.3.5a server emulator** (TrinityCore / CMaNGOS / AzerothCore). Auth, character-select, world-enter, and most gameplay work end-to-end on both backends. Support is still **experimental** — see "Open issues" below for the current rough edges. End-user setup is in the README *"WotLK Classic Quick Start"* section; this document is the dev-facing status + TODO doc.

---

## Backend strategy

Both backends are supported. `test-loop2.ps1` (the primary in-game smoke harness) defaults to cMangos and accepts a `-LocalTc` switch:

| Backend | Address | `test-loop2.ps1` |
|---|---|---|
| TrinityCore 3.3.5a (local repack) | `127.0.0.1:3724` | `-LocalTc` |
| CMaNGOS 3.3.5a | `192.168.88.55:3726` | (default) |

**TC is the primary dev oracle**: the third-party fork at `X:\Programming\HermesProxy-WOTLK` (origin `github.com/advocaite/HermesProxy-WOTLK`) is built and tested against TrinityCore. Cherry-picking V3_4_3-specific fixes from that fork is a known-good baseline for wire-format work.

**cMangos** also works for world-enter and most gameplay as of 2026-05-03. It has a small set of opcode-level translation gaps that TC doesn't exhibit (vendor sell, item use, special-ability targeting, quest-log refresh, raid kick) — see "Open issues". These are isolated translation bugs, not a fundamental backend block.

### Fork copy-cat policy

When cherry-picking from `HermesProxy-WOTLK`, take only V3_4_3-specific fixes. Do **not** regress the upstream perf/trim posture. The fork's snapshot stripped:
- `Directory.Packages.props` (central package management)
- ~384 lines of `BnetTcpSession.cs` pooled-buffer perf work
- `PublishTrimmed=true` + trimmer root config

These stay in upstream. Verify `dotnet publish -p:PublishTrimmed=true` after every PR.

---

## Why 3.4.3 was a large effort: new ObjectUpdate descriptor format

| Client | `ObjectUpdateBuilder` LOC | Update format |
|---|---|---|
| V1_14_1_40688 (Classic Era) | ~1,720 | Legacy DWORD-indexed UpdateFields + update-mask bitmap |
| V2_5_3_41750 (TBC Classic) | ~1,720 | Same legacy DWORD system |
| V3_4_3_54261 (WotLK Classic) | ~3,419 | **Descriptor-based change-set system** (matches retail Legion+) |

Blizzard modernized the object-update protocol for WotLK Classic to match current retail rather than ship 2008's `updatemask`-over-DWORD-array format. Each object type has a hand-written `WriteCreate{Object,Unit,Player,ActivePlayer,Item,…}Data` that walks a hierarchical tree of fields, emitting nested bit-masks (`WriteBits(blocksMask1, 16)` → per-block `WriteBits(block[b], 32)` → field values). Variable-size arrays carry per-element change bits. Visibility is first-class (`IsOwner`, `IsGameObjectOwner`); a `0x03 / 0x00` "update-field-flags" byte selects bucketed field sets per viewer. `ObjectTypeMask` has a new wire numbering (`0x20=Unit`, `0x40=Player`, `0x80=ActivePlayer`, …) distinct from the legacy `ObjectTypeBCC` byte.

Implication: there is no `UpdateFieldsArray.cs` to port for V3_4_3 — the descriptor system doesn't use one. Off-by-one errors in any `WriteBits(mask, N)` corrupt the entire object stream, so capture-based byte-level validation (see "Reference packet captures") is the only reliable check.

---

## Infrastructure baseline (v4.3.0)

These shipped before any WotLK-specific work; every entry below assumes them.

1. **Source generators are a proven pattern.** `HermesProxy.SourceGen` (netstandard2.0, `IsRoslynComponent`) emits `OpcodeTableGenerator` + `UpdateFieldTableGenerator` via flat `static readonly` arrays. Adding `V3_4_3_54261` enums is picked up at compile time — no trimmer-root edits needed.
2. **No more reflection-loaded opcode/update-field tables.** `ModernVersion` / `LegacyVersion` are `beforefieldinit`-clean `static readonly` containers. Branching on `ModernVersion.Build` is safe in any code path that runs after host startup.
3. **Per-connection DI via `ActivatorUtilities`.** `WorldSocket`, `RealmSocket`, `BnetRestApiSession`, `RealmManager` are constructed with DI-injected `IOptions<T>` option DTOs. Any 3.4.3-specific configuration must flow through an options DTO, not a static singleton.
4. **`<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>`** is configured in `HermesProxy.csproj`; generated `.g.cs` lands under `obj/GeneratedFiles/`.

---

## What's working / what's not (as of 2026-05-03)

| Subsystem | TC 3.3.5a | cMangos 3.3.5a | Notes |
|---|---|---|---|
| Auth + char-select | ✅ | ✅ | |
| World-enter + walk + camera | ✅ | ✅ | cMangos blocker resolved by recent fixes |
| New-char first login (post-cinematic movement) | ⚠️ | ✅ | TC: character can't move until logout + relog |
| Player render (incl. equipped items) | ✅ | ✅ | |
| Combat — auto-attack | ✅ | ✅ | packet-split fix (player Values → separate `SMSG_UPDATE_OBJECT`) |
| Combat — special abilities | ✅ | ❌ | cMangos: "invalid target" on e.g. Heroic Strike |
| Battle Shout / self-aura | ✅ | ✅ | |
| Action bar | ✅ | ✅ | embedded `ActivePlayer.ActionButtons` descriptor populated |
| Inventory equip / unequip / drag-drop | ✅ | ✅ | |
| Inventory — "on use" items | ✅ | ❌ | cMangos: food / consumables don't trigger |
| Vendor — buy | ✅ | ✅ | |
| Vendor — sell | ✅ | ❌ | cMangos: items go permanent grey |
| Looting (items + money) | ✅ | ✅ | |
| Quest log + tracker | ✅ | ⚠️ | cMangos: log desyncs on pickup; relog refreshes |
| Quest pickup + completion | ✅ | ✅ | gossip dialogs render fully |
| Quest map markers (mini + world) | ✅ | ✅ | |
| Hotfix data | ✅ | ✅ | wago.tools build 3.4.3.54261, ~700K records |
| Static GameObjects | ✅ | ✅ | mailboxes / doodads / chests |
| Zeppelins / elevators (MOTransport) | ❓ | ❓ | untested; filter still in place |
| Auras / aura ticks | ✅ | ✅ | live ticks animate |
| Spellbook (incl. trainer learning) | ✅ | ✅ | |
| Chat (`/say`, `/emote`) | ✅ | ✅ | |
| Party / raid (form, convert) | ✅ | ✅ | bits-first wire format |
| Party chat / raid chat / raid warning | ✅ | ✅ | |
| Raid promote-to-assistant | ❓ | ❌ | **HermesProxy crashes** — `CMSG_SET_ASSISTANT_LEADER` parse bug; likely TC too |
| Raid kick (single member) | ❓ | ❌ | cMangos: disbands whole raid; party uninvite is fine |
| Trade (between players) | ✅ | ✅ | |
| Mail — mailbox open + inbox list | ✅ | ❓ | TC: incoming mail visible; cMangos untested |
| Mail — money attachment display | ⚠️ | ❓ | TC: money amount renders garbled / wrong value in incoming mail |

---

## Done so far

| Date | Work | PR / commit |
|---|---|---|
| 2026-04-22 | Phase 0 — un-gate V3_3_5a legacy backend | PR #45 |
| 2026-04-23 | Phase 1 — V3_4_3_54261 enum scaffolding + CSV bootstrap | PR #46 |
| 2026-04-24 | Phase 2 — BNet login accepts 54261 | PR #47 |
| 2026-04-25 | Phase 3 — World login + Ed25519ctx encryption signing | PR #48 |
| 2026-04-25 | Phase 5 source-gen bootstrap (deferred behind hand-port) | PR #49 |
| 2026-04-26 | Phase 4 — `SMSG_ENUM_CHARACTERS_RESULT` 3.4.3 layout | `cb7cd7d` |
| 2026-04-26 | Phase 5a — `ObjectUpdateBuilder` hand-port (3,419 LOC) | PR #50 |
| 2026-04-26 | Phase 5a-7b — real WotLK hotfix data via wago.tools (~700K rows) | PR #51 |
| 2026-04-28 | Static GameObjects + empty-Values suppression (login unblocked) | `989b929` |
| 2026-04-29 → 2026-05-03 | ~30 follow-up `fix(v3_4_3): …` commits — see themed bullets below | feature branch |

Post-5a opcode-fix flood, grouped by theme:

- **Object updates** — static GameObject `BYTES_1` unpacker, ItemContainer scrub, player/creature Values split for combat DC (ported from fork commit `18caaf7`), `IsEmptyValuesDelta` probes ObjectData (corpse Lootable clear), partial Values updates keep all slot arrays.
- **Player rendering** — real ActionButtons in embedded `ActivePlayer` descriptor (`4058c08`), `ChrCustomization*` hotfix loaders (`fa78793`), strip retail-era `ChrCustomizationChoice` rows the legacy 3.3.5 server rejects (`dc59fcc`).
- **Party / raid** — bits-first wire format for `CMSG_PARTY_INVITE_RESPONSE` (`c0a6934`), `SMSG_PARTY_UPDATE` (`1d1ed8b`), `CMSG_PARTY_UNINVITE` (`34b63a8`); forward Raid bit on `CMSG_CONVERT_RAID` (`7a363ac`); legacy `SMSG_GROUP_LIST` + `PARTY_MEMBER_FULL_STATE` WotLK 3.0+ extras (`3ef5de3`).
- **Quest** — quest-giver wire layouts + V343 status enum + POI translator (`d981816`), owner-typed `PlayerData.QuestLog` in Create + Update (`88d79f5`), vanilla gossip QuestIcon translation + proactive quest-info query (`18ec089`), creature display Probability=100 + drop unconditional ForceGossip (`cd212c6`), `QuestConst.MaxQuestLogSize` (`60c0d8e`).
- **Vendor / item** — vendor merchant V3_4_3 packet layout + MuID round-trip (`384ed13`), `SMSG_ITEM_PUSH_RESULT` layout + descriptor slot translation (`7129b10`), inventory equip/swap CMSG translations + slot helpers (`b17d6cf`), `CMSG_USE_ITEM` legacy wire format (`db63f01`), `CMSG_SET_TRADE_ITEM` descriptor→legacy slot mapping (`8bdf50f`).
- **Chat** — CMSG + SMSG layouts + NUL trim + emote lang (`3b478e3`), `SMSG_QUERY_PLAYER_NAMES_RESPONSE` plural-form layout (`08821be`).
- **Movement** — `hasStandingOnGameObjectGUID` + `hasAdvFlying` bits (`cab673e`).
- **Auth / login** — port `V3_3_5a/ResponseCodes`, route 3.3.5a backends to it (`7687fdf`); pass `FirstLogin` through and synthesize start zone for new chars (`652ceaa`); drop early `SMSG_MOVE_SET_ACTIVE_MOVER` from `HandleLoginVerifyWorld` (`69999dc`).

Individual commits beyond what's listed here are auditable via `git log --grep="(v3_4_3|phase5)"`.

---

## Open issues

### Critical (proxy crash — both backends affected)

- **`CMSG_SET_ASSISTANT_LEADER` bits-first parse bug.** Stack trace: `IndexOutOfRangeException` in `ByteBuffer.HasBit()` at `Framework/IO/ByteBuffer.cs:353`, called from `SetAssistantLeader.Read()` at `HermesProxy/World/Server/Packets/GroupPackets.cs:426`, opcode `CMSG_SET_ASSISTANT_LEADER` (13906). Same family as the bits-first party fixes already shipped (`c0a6934`, `1d1ed8b`, `34b63a8`) — V3_4_3 client emits bits-first, upstream parser reads byte-first, runs off the end. Rewrite `SetAssistantLeader.Read()` bits-first. Repros on cMangos but the bug is in our parser, not the legacy server, so it should reproduce on TC too.
- **Druid Typhoon crashes HermesProxy** (fork issue #14). Class-ability spell that takes the proxy down. Need a stack-trace capture at next repro to localize.

### Critical (client crash — TC backend)

- **Hunter ammo invisible + mass-stack `.additem` crashes the V3_4_3 client (TC).** Logged in as a level-1 Draenei Hunter on local TC. Rough Arrow ([item 2512](https://www.wowhead.com/classic/item=2512/rough-arrow)) — the starter ammo every Hunter ships with — was not rendered in inventory or in the ammo slot, so auto-shot couldn't fire. After `.additem 2512 1000` (5 stacks of 200, max-stack ammo) the client crashed within seconds. Repro log: `HermesProxy/bin/Release/Logs/hermes-20260503_052938.log` (2026-05-03 05:29 → 05:41 session). The proxy stayed up — log ends with the client TCP closing (`05:41:43 | E | P | WorldSocket | Can't send SMSG_MONSTER_MOVE_TRANSPORT, socket is closed!`); no proxy-side exception or stack trace, so the malformed packet is being accepted by our writer and rejected by the client. Two likely culprits, possibly the same root cause: (a) `SMSG_ITEM_PUSH_RESULT` / inventory-delta layout drops the ammo slot or mis-tags a non-bag, non-bag-paperdoll slot for V3_4_3 (Hunter ammo lives in the dedicated ammo slot, not a bag); (b) a multi-stack item-push burst (`.additem N` where N > stack-size) emits N/stack consecutive pushes in one tick, and the V3_4_3 descriptor write for stacked-ammo `Item.StackCount` mis-encodes when the same `ItemID` lands in adjacent slots. Investigate next: WPP-parse the `modern_*.pkt` for this session, diff `SMSG_ITEM_PUSH_RESULT` + the relevant `Item` create blocks against a TC-direct capture of the same `.additem`. Cross-check `Errors\*.txt` in the WoW client install around 05:41:43 for the client-side crash signature (likely `jam mirror full update failure` — see `hermes-logs client jam`).

### Class ability gaps (carryover from fork issue [advocaite/HermesProxy-WOTLK#14](https://github.com/advocaite/HermesProxy-WOTLK/issues/14))

The fork received a thorough class-by-class test matrix from `kasperfriend` (2026-04-18, against fork build 2026-04-16, TC backend). The individual symptoms group into a small number of recurring patterns — most are likely single opcode-translation fixes that would unblock dozens of abilities at once. **These results predate our 2026-04-29 → 2026-05-03 work; re-verify against current HEAD before treating each as open.**

**Pattern A — self-cast dispel/cleanse rejects with "can't mount here"** (5+ classes affected). Likely a `CMSG_CAST_SPELL` target-encoding gap for self-targeted dispel/cure spells, or an `SMSG_CAST_FAILED` reason-code translation that maps a generic reject onto `SPELL_FAILED_NOT_MOUNTED`.
  - Paladin: Cleanse, Purify · Priest: Dispel Magic (self), Cure Disease · Shaman: Purge, Cleanse Spirit, Cure Toxins · Mage: Remove Curse · Druid: Cure Poison, Remove Curse

**Pattern B — ground-targeted AOE rejects with "item is not ready yet"** (4+ classes). All `DEST_LOCATION` / ground-click spells. Likely a `CMSG_CAST_SPELL` ground-target position-vector layout gap in V3_4_3.
  - Priest: Mass Dispel, Lightwell · Druid: Hurricane, Force of Nature · Mage: Flamestrike, Blizzard · Warlock: Shadowfury

**Pattern C — combo-point finishers reject with "requires combo points"**. Likely combo-point descriptor (`PlayerData.ComboPoints`) not propagating to the V3_4_3 client, or modern-client reads it from a different descriptor slot than what we write.
  - Rogue: ALL combo abilities · Druid (Cat): Ferocious Bite, Maim, Rip, Savage Roar

**Pattern D — bear-form rage abilities reject with "not enough rage"**. Same shape as Pattern C but for power-type swap on shapeshift; rage power isn't propagating in bear form.
  - Druid (Bear): Bash, Demoralizing Roar, Lacerate, Maul, Challenging Roar

**Pattern E — shapeshift / form-cancel broken** (locks character in form). Likely `SMSG_AURA_UPDATE` / aura-removal translation gap for cancelable auras.
  - Druid: ALL forms (Cat / Bear / Moonkin / Tree / Aquatic / Travel / Swift Flight) · Rogue: Stealth · Priest: Shadowform · Warlock: Metamorphosis · Shaman: Ghost Wolf

**Pattern F — pet/summon spell panel layout broken** (works but UI broken).
  - Warlock: Summon Imp, Eye of Kilrogg · Priest: Shadowfiend, Mind Vision, Mind Control · Death Knight: Raise Dead

**Pattern G — class-blocker DCs / character-create gating** (severe):
  - **Hunter — total DC on every world enter.** Class-specific hard block in the fork test matrix. **Update 2026-05-03: world-enter is no longer a hard block on TC** — successfully entered world as level-1 Draenei Hunter (`hermes-20260503_052938.log`). The remaining Hunter-specific failure mode has shifted to ammo rendering + mass-stack `.additem` client crash; see "Critical (client crash — TC backend)" above. The original `PetData`/`PetSpells` hypothesis can be deprioritized for class entry; pet-related Hunter symptoms (if any) need a fresh repro.
  - **Death Knight — cannot be created at character-select** (even with 80-level prerequisites met). Likely a class/race-availability flag in our `EnumCharactersResult` or `CMSG_CREATE_CHARACTER` path. DK abilities mostly do not work either; class is effectively blocked.
  - **Warlock — adding a soulshard to inventory causes DC.** Pet/inventory item type cross-talk; likely a soulshard `Item` create-data write issue.

**Pattern H — misc one-offs**:
  - Mage Slow Fall: "out of range" on self-cast → target-encoding bug, related to Pattern A.
  - Paladin Greater Blessings of Kings/Sanctuary: "you have already learned that spell" → spell-learn dedup translation.
  - Shaman weapon imbues (Windfury / Rockbiter / Frostbrand / Flametongue / Earthliving): "item is already enchanted" → `CMSG_ENCHANT_ITEM` (or similar) translation gap.
  - Holy Wrath / Holy Shock: animate but no damage / no heal → `SMSG_SPELL_GO` effect translation might be losing damage/heal payload for hybrid school spells.

**Suggested triage order**: Patterns A (1 fix unblocks 10+ spells) → C (combo points, ~5 spells) → E (form cancel, ~10 abilities) → B (ground AOE, ~6 spells) → G-Hunter (whole class blocked) → G-Warlock-soulshard. Patterns D/F/H drop out as the underlying descriptor/translation work in A/C/E lands.

### TC-specific gaps (cMangos works for these)

- **First login after character creation: character cannot move until relog (TC).** Create a fresh character on local TC, click Enter World, watch (or skip) the intro cinematic — once control returns, the character is frozen: no walk, no jump, no turn. Logging out and back in clears it; subsequent logins on the same character work normally. cMangos does not exhibit this — first-login post-creation works directly. Likely a `SMSG_MOVE_SET_ACTIVE_MOVER` / movement-permission-bag timing or ordering issue specific to the *first* world-enter (when the player record was just inserted), e.g. a movement-control packet that TC sends post-cinematic isn't being forwarded, or the relog path re-sends something the create-then-enter path skips. Note `69999dc` ("drop early `SMSG_MOVE_SET_ACTIVE_MOVER` from `HandleLoginVerifyWorld`") was the most recent active-mover change — re-check whether the drop is correct on the first-login-post-creation path or whether TC depends on the early send specifically for fresh chars.

### cMangos-specific gaps (TC works for these)

- **Quest log state desync.** Picked-up quests don't always appear in the log immediately; relogging refreshes. Quest tracker + map markers update live, so only the log-list view is affected. Likely a `SMSG_QUESTUPDATE_*` or `ActivePlayer.QuestLog` descriptor delta translation gap on cMangos's wire output.
- **Special abilities reject "invalid target."** Heroic Strike (and presumably other rage/energy "next attack" abilities) reject when used on a mob the player is auto-attacking. Likely a target-selection / GUID translation mismatch on cMangos's swing path.
- **Vendor sell.** Selling items leaves the slot rendered as a permanent grey item — the slot doesn't free up. Buy works, so the inventory packet scaffolding is there. Likely a `CMSG_SELL_ITEM` / `SMSG_SELL_ITEM_RESPONSE` translation gap on cMangos.
- **"On use" inventory items don't trigger.** Food (Shiny Red Apple), bandages, potions, etc. don't fire when right-clicked from inventory. Likely a `CMSG_USE_ITEM` legacy wire-format gap specific to cMangos's expected layout (the `db63f01` fix targeted modern→legacy translation; cMangos may want a different field order or slot index).
- **Raid kick disbands the whole raid (party uninvite is fine).** In raid mode, kicking a single member disbands the entire raid; in party mode, uninvite works. So the `34b63a8` `CMSG_PARTY_UNINVITE` fix is OK for party. The raid-only path likely takes a different opcode (`CMSG_GROUP_UNINVITE_GUID` or the V3_4_3-renamed variant) that translates to a legacy disband instead of single-member uninvite.

### Mail

- **Mail money attachment shows garbled value (TC).** Incoming mail with money attached opens and the mail is visible in the inbox, but the money amount field displays a wrong / mangled value. Likely a `SMSG_MAIL_LIST_RESULT` money-field width or endianness mismatch in the V3_4_3 layout, or the copper/silver/gold split isn't being honored correctly. Capture next to-do: WPP-parse a known-money mail (e.g. send self 1g 50s 25c) and diff the bytes against the V3_4_3 expected layout.

### Rendering / world-population

- **MOTransport (zeppelins, elevators).** Untested in-game as of 2026-05-03. The Transport / MOTransport filter at `UpdateHandler.cs:162/229` is still in place from the 5a-era unblock, so they're definitely not rendering even if the underlying issue has been quietly fixed. First step: verify on TC + cMangos. If still broken, decide between option A (aggregate position from later `GAMEOBJECT_POS_*` deltas) vs option B (Movement block) vs option C (capture-diff cMangos vs TC to find the real divergence).

### Combat & state propagation

- **No-handler warnings — silent state drops.** Each gates a UI feature; prioritize by user-visibility:
  - `SMSG_CRITERIA_UPDATE` — achievement progress
  - `SMSG_THREAT_UPDATE` — threat meter / boss frames
  - `SMSG_LOAD_EQUIPMENT_SET` — equipment manager
  - `SMSG_LEARNED_DANCE_MOVES` — `/dance` variants
  - `SMSG_INSTANCE_DIFFICULTY` — heroic/normal toggle
  - `SMSG_UPDATE_TALENT_DATA` — talent panel
  - `SMSG_ALL_ACHIEVEMENT_DATA` — achievement window initial population

### Infrastructure

- **Stale FIXME bookkeeping pass.** `phase5a-auras` at `SpellHandler.cs:1143` and `phase5a-7c` at `HighGuid.cs:45` are functionally resolved (aura ticks animate, static-GameObject path stable). Delete the markers + any dead stub code in a cleanup pass.
- **Source-gen restoration.** V3_4_3 `ObjectField.cs` descriptor attributes are temporarily stripped (FIXME on `ObjectField.cs:3` + `ObjectUpdateBuilderGeneratorTests.cs:22`). Restore once the hand-port stops iterating; validate byte-equivalence against fork HEAD as test oracle.
- **`HighGuid` unknown-legacy fallback diagnostic** (`HighGuid.cs:45`). Audit what cMangos / TC actually emit at world-enter and decide whether to suppress / map / keep the warn loud.
- **2306 ItemSparse rows skipped** (sbyte→short stat-width loader bug). Raid-tier WotLK gear with stats > 127 is silently dropped from the modern hotfix slot; legacy `SMSG_ITEM_QUERY_SINGLE_RESPONSE` still works, so tooltips render via that path. Fix: widen the loader to `short` (parse) + `WriteInt16` (wire).

### Lower priority — kept for reference

- **cMangos `ActivePlayer` descriptor synthesis.** Historical analysis (memory `project_v343_canary_state`) suggested cMangos doesn't populate descriptor stats (`CritPercentage`, `BlockPercentage`, `ModDamageDonePercent[7]`, `DisplayPower`, `AuraState`) that TC populates server-side. World-enter now works without this synthesis, so it's no longer a blocker — but it may still explain stat-display glitches (e.g. character pane crit% reading 0). Investigate only if a concrete bug surfaces; the fix path is per-class/per-level defaults synthesized in `UpdatePackets.cs` rather than relying on the legacy server.

---

## Cross-cutting constraints

- **Never** edit `HermesProxy/BnetServer/Networking/BnetTcpSession.cs` — it holds the pooled-buffer perf work.
- **Never** replace `Directory.Packages.props` — central package management stays.
- **Never** drop `PublishTrimmed=true` or the trimmer root config. Verify `dotnet publish -c Release -p:PublishTrimmed=true` succeeds at end of every phase.
- **Reuse existing crypto pattern**: `World/Client/VanillaWorldCrypt.cs` / `TbcWorldCrypt.cs` are standalone files — follow the same shape for `WotlkWorldCrypt.cs` rather than inlining.
- **Version-gate new code in shared files**: anything V3_4_3-specific must be guarded so V1_14 / V2_5 paths fall through unchanged. Run `dotnet test` after every PR; the suite has 296 tests covering the legacy paths.

---

## CSV/DBC data strategy

### File-name convention (from `HermesProxy/World/GameData.cs`)

Three patterns coexist under `HermesProxy/CSV/`:

| Pattern | Example | Keyed on |
|---|---|---|
| `{Name}{ModernVersion.ExpansionVersion}.csv` | `ItemSparse2.csv` | Modern-client expansion (1=Era, 2=TBC, 3=**WotLK**) |
| `{Name}{LegacyVersion.ExpansionVersion}.csv` | `BroadcastTexts2.csv` | Legacy-server expansion (same numbering) |
| `{Name}.csv` | `AreaNames.csv`, `LearnSpells.csv`, `RaceFaction.csv` | version-agnostic |

WotLK activates `*3.csv` lookups on both sides. Phase 1 bootstrapped the `ModernVersion`-keyed `*3.csv` set (~15 tables) from the fork as a placeholder — most were byte-identical TBC duplicates.

### Hotfix data (`HermesProxy/CSV/Hotfix/`)

Phase 5a-7b regenerated the 18 V3_4_3 hotfix CSVs from wago.tools at `?build=3.4.3.54261` (~700K records). Per-loader column projections were tightened during that regen — 5 of 18 tables (`SpellMisc`, `ItemSparse`, `Item`, `ItemDisplayInfo`, `CreatureDisplayInfo`) need explicit reorder + drop of wago-only columns; the other 13 match wago's column order verbatim.

For future regenerations of static `*3.csv` data:

1. `ItemSparse3.csv`, `Item3.csv`, `ItemEffect3.csv`, `ItemSpellsData3.csv` — tooltips, stats, names (most user-visible).
2. `ItemAppearance3.csv`, `ItemModifiedAppearance3.csv`, `ItemDisplayIdToFileDataId3.csv` — rendering.
3. `QuestV2_3.csv`, `SpellVisuals3.csv`, `BroadcastTexts3.csv` — quest text, spell anims, NPC dialogue.
4. `TaxiPath3.csv`, `TaxiNodes3.csv`, `TaxiPathNode3.csv` — flight paths.

Treat regeneration as a debugging loop, not a pre-emptive batch — only regen when a specific gameplay bug demands it.

### Regeneration verification (per `dbc-lookup` skill)

- Pull target table from wago.tools at `?build=3.4.3.54261`.
- Compare column order and types against what the `Load*` method in `World/GameData.cs` expects (each loader reads columns in a deterministic order — column-order mismatch between wago's current export schema and the loader is the common failure mode).
- Spot-check 3–5 known-WotLK rows per table to confirm the export actually contains 3.x data (e.g. ItemID 49426 *Emblem of Frost* exists in WotLK; absent rows = wrong build filter).

---

## Reference packet captures (TC 3.4.3 ground truth)

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

The `/parse-pkt` skill wraps the WPP invocation; the `/hermes-logs` skill slices `hermes-*.log` runtime logs.

---

## Reference fork (`HermesProxy-WOTLK`)

- **Origin**: `github.com/advocaite/HermesProxy-WOTLK`
- **Local clone**: `X:\Programming\HermesProxy-WOTLK` (full git history available — 46 commits at last sync, growing)
- **Policy**: cherry-pick V3_4_3-specific fixes for known-good TC baseline; do **not** replace upstream perf/trim posture (`BnetTcpSession.cs`, `Directory.Packages.props`, `PublishTrimmed`).
- **Recent ports from fork**: combat packet-split (commit `18caaf7`), aura parser, party bits-first layouts, quest-giver layouts.
- **Future use**: the fork's HEAD `ObjectUpdateBuilder.cs` is the test oracle for any future source-gen byte-equivalence work — generate-and-diff against it per object type.

The fork is actively shipped with nightly GitHub Releases binaries and has a public end-user base (OwnedCore release thread). End-to-end pipeline (3.4.3 modern client → 3.3.5a backend) is validated against TC in production via that fork; our work is tracing a known-working path, not blazing a speculative one.
