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

## What's working / what's not (as of 2026-05-10)

| Subsystem | TC 3.3.5a | cMangos 3.3.5a | Notes |
|---|---|---|---|
| Auth + char-select | ✅ | ✅ | |
| World-enter + walk + camera | ✅ | ✅ | cMangos blocker resolved by recent fixes |
| New-char first login (post-cinematic movement) | ✅ | ❓ | TC: fixed (`21d0e1a` — `CMSG_LOADING_SCREEN_NOTIFY` `0xFFFFFFFF` sentinel was poisoning `CurrentMapId`). Caveat: skipping the cinematic too early can still disconnect (timing-related). cMangos: untested with the TC fix. |
| Player render (incl. equipped items) | ✅ | ✅ | |
| Combat — auto-attack | ✅ | ✅ | packet-split fix (player Values → separate `SMSG_UPDATE_OBJECT`) |
| Player Armor + magical resistances | ✅ | ❓ | `e084fb4` — V3_3_5a `UNIT_FIELD_RESISTANCES` rename fallback; was reading 0 |
| Combat — special abilities | ✅ | ❌ | cMangos: "invalid target" on e.g. Heroic Strike |
| Channel spells (cast bar / kneel anim / ESC unblock) | ✅ | ❓ | `fc238f9` — wire-format only; cMangos untested |
| Channel spells (loop animation) | ✅ | ❓ | fixed by writing `ChannelObjects` DynamicUpdateField (bit 4) in V3_4_3 ObjectUpdateBuilder; cMangos retest pending |
| Spell-failure error text (e.g. NotShapeshift) | ✅ | ❓ | `ed5e470` — per-build `SpellCastResult` dispatch; was reading wrong text like "in flight" |
| Flying projectiles (arrows / fireball / missiles) | ✅ | ✅ | `49ace55` |
| Death Knight character create | ✅ | ❓ | `43957ff` + `39cf991` — DK class offered in create UI on TC; cMangos untested |
| Death Knight runes + runic power | ✅ | ❓ | rune cooldown swirl, RunicPower in power slot 0 (`d2bb304` + `2664a0e` + `cd93bf6` + `c9223a2`) |
| **Death Knight starter quest chain (full)** | ✅ | ❓ | 2026-05-10 — entire Acherus → Scarlet Enclave chain completed end-to-end on TC (incl. quest 12779 Frostbrood Vanquisher / *An End to All Things* and quest 12800 *The Lich King's Command* / Light's Hope battle, the two scripted-event quests that needed the bulk of the May session's fixes). cMangos retest pending |
| Battle Shout / self-aura | ✅ | ✅ | |
| Shapeshift / form cancel (Bear Form verified) | ✅ | ❓ | `7cbc550` — re-emit player auras after CreateObject; other Druid forms / Rogue Stealth not exhaustively tested |
| Action bar — main bar populated on login | ✅ | ✅ | embedded `ActivePlayer.ActionButtons` descriptor populated |
| Action bar — drag spell / macro / item to any bar (incl. Bar 4) | ✅ | ❓ | 2026-05-10 — V3_4_3 wire is `int32 packed (low24=action, high8=type) + uint8 idx`, mis-decoded as `Int16 Action + Int16 Type`. Macros / items / mounts / equipment-sets / companions all collapsed to "spell with truncated id" and crashed the V3_4_3 client (`reason=7`) when rendered on a side bar. Fix is version-gated to V3_4_3 only — V1_14/V2_5 keep the old direct-pack path. cMangos retest pending |
| Action bar — Always Show Action Bars checkbox persistence | ✅ | ✅ | Natural account-data round-trip; the `alwaysShowActionBars` CVar is in GlobalConfigCache (type 0) which the V3_4_3 client uploads + reads back |
| Action bar — Bar 2 / 3 / 4 / 5 visibility checkbox persistence | ⚠️ | ❓ | 2026-05-10 — user must re-tick each session. The V3_4_3 client UI ignores the legacy `bottomLeftActionBar` / `rightActionBar` etc. CVars for these bars (almost certainly uses Edit Mode account-data types 13/14 added in V3_4_4+, which V3_4_3.54261's HermesProxy enum doesn't yet wire). Phase 7 augmenter + Phase 8 timestamp bump inject the legacy CVars defensively (no harm, no help). See the "Action bar visibility & macro persistence (V3_4_3) — May 2026" section below |
| Sporadic V3_4_3 client `CMSG_LOG_DISCONNECT(reason=7)` under load | ❌ | ❓ | Not introduced by anything proxy-side — ring buffer dump in `WorldSocket` (search log for `[ActionBarTrace] reason=7`) shows only routine combat / hotfix-burst traffic before each crash. Client × HermesProxy stability issue, needs client-side telemetry to pin down |
| Inventory equip / unequip / drag-drop | ✅ | ✅ | |
| Inventory — "on use" items | ✅ | ❌ | cMangos: food / consumables don't trigger |
| Vendor — buy | ✅ | ✅ | |
| Vendor — sell | ✅ | ❌ | 2026-05-10 — TC sell-response wire layout fix (`0f92e44`); coppers awarded + slot freed end-to-end on TC. cMangos still goes permanent grey (separate bug, untested today) |
| Professions — secondary train + skinning use | ✅ | ❓ | 2026-05-09 — trained Fishing / Skinning / Leatherworking; bought Skinning Knife from vendor; skinned beasts and looted hides on TC. cMangos retest pending |
| Looting (items + money) | ✅ | ❓ | 2026-05-13 — five-part V3_4_3 loot translation: (1) suppress per-item `SMSG_LOOT_RELEASE` that TC 3.3.5 emits during auto-loot and synthesize a single closing one on drain; (2) stamp `Owner = looting player GUID` on outbound `SMSG_LOOT_RELEASE` (modern client validates); (3) per-corpse `RemainingLootSlots` list with legacy-slot translation (TC echoes the clicked slot byte for every drained item instead of the real one); (4) defer the coin-path close synth from `SMSG_LOOT_CLEAR_MONEY` to `SMSG_LOOT_MONEY_NOTIFY` so the modern client gets `COIN_REMOVED → MONEY_NOTIFY → RELEASE` in order; (5) pre-claim gold via injected `CMSG_LOOT_MONEY` before forwarding `CMSG_AUTOSTORE_LOOT_ITEM` and suppress the now-redundant client `CMSG_LOOT_MONEY` to dodge TC's session-close-on-item race that orphaned coins. cMangos retest pending |
| Quest log + tracker | ✅ | ⚠️ | cMangos: log desyncs on pickup; relog refreshes |
| Quest pickup + completion | ✅ | ✅ | gossip dialogs render fully |
| Quest map markers (mini-map + world-map dots) | ✅ | ✅ | |
| Quest area highlight on world map (POI blob mesh) | ✅ | ❓ | 2026-05-17 — blue polygon overlay renders. Fix at `HermesProxy/World/Client/PacketHandlers/QuestHandler.cs:510`: pass raw legacy WorldMapAreaID through as `UiMapID` (NO `GameData.GetUiMapId` translation — V3_4_3 Classic client keeps legacy WMA semantics in the wire field despite the modern name), map legacy `FloorID` slot to modern `Flags` (not `Unk3`/`Unk4`), discard `Unk3`+`Unk4`, synthesize `QuestObjectiveID` from cached `QuestTemplate.Objectives[StorageIndex==ObjectiveIndex].Id`. Diff-verified against HermesProxy-WOTLK fork (which renders): fork-with-raw-WMA emits `UiMapID=41` for Teldrassil → polygon renders; ours-with-`GetUiMapId` emitted `UiMapID=57` → polygon dropped. Click-to-navigate from quest tracker remains broken on both proxies (legacy quest_template POIContinent/POIx/POIy=0 — see Open issues). cMangos untested |
| Hotfix data | ✅ | ✅ | wago.tools build 3.4.3.54261, ~700K records |
| Static GameObjects | ✅ | ✅ | mailboxes / doodads / chests |
| Quest-objective GameObject interaction | ✅ | ❓ | `39cf991` — DK runeblade rack (object 190584, quest 12619) interacts and advances quest state on TC; cMangos untested |
| Spell-click NPCs ("right-click to ride") | ✅ | ❓ | 2026-05-10 — `52f07c9` wires `CMSG_SPELL_CLICK` into the existing HandleInteractWithNPC stack; Acherus Scourge Gryphon (DK starter chain) and similar click-to-mount NPCs respond on TC. Was being silently dropped. cMangos untested |
| Vehicles — mount + dismount + seat-change + action button | ✅ | ❓ | 2026-05-10 — three-part fix for *Grand Theft Palomino* (quest 12680) on TC: Leave Vehicle button (`d0dd989` rewrites `CMSG_MOVE_DISMISS_VEHICLE` → `CMSG_REQUEST_VEHICLE_EXIT`), missing PREV/NEXT/EXIT seat opcodes (`d0dd989`), and vehicle action-button slot translation (`02e9e51` — TC packs `slot_index_in_UI` in the high byte, not a CharmInfo state). cMangos untested |
| Trajectory casts (cannons / arc projectiles) | ✅ | ❓ | 2026-05-10 — `3931123` writes the 9-byte trailer (elevation + speed + has-movement-data) when `cast_flags & 0x02` set; verified on TC with Scarlet Cannon (spell 52435). Without it the legacy 3.3.5 `HandleClientCastFlags` silently dropped the cast. cMangos untested |
| Active-mover handover (vehicle / charm / Eye of Acherus) | ✅ | ❓ | 2026-05-10 — `95aacb6` synthesises `SMSG_MOVE_SET_ACTIVE_MOVER` alongside `SMSG_CONTROL_UPDATE`; without it, mid-session control transfers (Eye of Acherus possess, vehicles, charm) left WASD swallowed on TC. cMangos untested |
| Zeppelins / elevators (MOTransport) | ❓ | ❓ | untested; filter still in place |
| Auras / aura ticks | ✅ | ✅ | live ticks animate |
| Spellbook (incl. trainer learning) | ✅ | ✅ | |
| Pet — summon, portrait, attack/follow/stay | ✅ | ❓ | `f8f784e` Pet GUID translation; `913818c` ActionButton/Action wire encoding; `de77eee` UI binding (race + ordering). cMangos retest pending — full pet stack not re-verified after `de77eee` |
| Pet — spellbook + action bar | ⚠️ | ❓ | `fdb9cff` forwards `SMSG_PET_LEARNED_SPELLS` / `SMSG_PET_UNLEARNED_SPELLS`; pet ActionButtons synthesized from PetSpells body. **Known gap (TC):** Cat pet's *Prowl* doesn't appear on the action bar. cMangos retest pending |
| Pet — character-sheet stats | ✅ | ❓ | follow-up Values UpdateObject (PetStatsValuesSynth) re-emits IsOwner-gated stats (Stats[5] / AP / MinDamage / Resistances). cMangos retest pending |
| Pet — spellbook "Pet" tab in player UI | ❌ | ❓ | 6 iterations of capture-diff against CypherCore native didn't crack it (count, Specialization, slot encoding, ordering all matched native). Parked — see "Open issues" |
| Talent panel — unspent talent point counter + tree population | ✅ | ✅ | translates legacy SMSG_UPDATE_TALENT_DATA → modern (V3_4_3 Rank=u8, no PrimarySpecialization) with cache for relog re-emit |
| Talent panel — dual-spec switch | ✅ | ✅ | per-group emit, SpecID encoding matches TC SendTalentsInfoData |
| Pet talents | ✅ | ❓ | CMSG_PET_LEARN_TALENT (modern 0x3554 → legacy 0x47A) translates with PetGUID modern→legacy; learn UI works on TC |
| Glyphs — display + slot unlock | ✅ | ❓ | reads PLAYER_GLYPHS_ENABLED bitmask + PLAYER_FIELD_GLYPHS_1..6 from legacy update fields; emits SMSG_ACTIVE_GLYPHS with (SpellID, GlyphID) pairs via GlyphProperties3.csv lookup |
| Glyphs — apply (CMSG_USE_ITEM) | ✅ | ❓ | forwards SpellCastRequest.Misc[0] as glyphIndex; was hardcoded 0 → every glyph went to slot 0 |
| Glyphs — remove (CMSG_REMOVE_GLYPH) | ✅ | ❓ | modern 13056 → legacy 0x48A, identical payload (uint8 GlyphSlot) |
| Glyphs — dual-spec swap UnitData refresh | ✅ | ❓ | dirty-flag drives ObjectUpdateBuilder to re-emit GlyphSlots in player Values update; without this, "already applied this glyph" check fired against stale (previous-spec) glyphs |
| Chat (`/say`, `/emote`) | ✅ | ✅ | |
| Party / raid (form, convert) | ✅ | ✅ | bits-first wire format |
| Party chat / raid chat / raid warning | ✅ | ✅ | |
| Raid promote-to-assistant | ❓ | ❌ | **HermesProxy crashes** — `CMSG_SET_ASSISTANT_LEADER` parse bug; likely TC too |
| Raid kick (single member) | ❓ | ❌ | cMangos: disbands whole raid; party uninvite is fine |
| Trade (between players) | ✅ | ✅ | |
| Char-list — auto-select newly created character | ✅ | ❓ | `a515bd0` — character pre-selected in char-list after create; QoL |
| Mail — open + inbox + take attachments + COD pay + delete | ✅ | ❓ | 2026-05-09 — V3_4_3 wire-format fix (`d3004b1`); 12-attachment + 1-COD mail end-to-end on TC; V1_14/V2_5 regression-clean; cMangos untested |

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
| 2026-04-29 → 2026-05-05 | ~38 follow-up `fix(v3_4_3): …` commits — see themed bullets below | feature branch |
| 2026-05-06 → 2026-05-08 | ~14 follow-up commits — DK runes, pet UI binding, aura re-emit, channel-loop anim | feature branch |
| 2026-05-09 | Player-verified on TC: secondary-profession train (Fishing / Skinning / Leatherworking), tool-vendor buy, skinning-cast → corpse-loot chain end-to-end | feature branch |
| 2026-05-09 | Mail packet family translated to V3_4_3 wire format (Int64 ids + switch-on-SenderType); mailbox open / take / COD / delete end-to-end on TC; V1_14/V2_5 regression-clean | `d3004b1` |
| 2026-05-10 | DK starter chain + Quimby vehicle quest unblocked on TC: spell-click NPCs forwarded (`52f07c9`), `SMSG_MOVE_SET_ACTIVE_MOVER` synthesised on control transfer (`95aacb6`), vehicle exit + seat-change opcodes (`d0dd989`), TC vehicle action-button slot encoding (`02e9e51`), trajectory cast trailer when `cast_flags & 0x02` (`3931123`), `SMSG_SELL_RESPONSE` wire layout (`0f92e44`). All TC-only verification — cMangos retest pending across the cluster. | feature branch |
| 2026-05-10 | Action-button macros / items / mounts / equipment-sets / companions on V3_4_3 — type-byte mis-extraction in `HandleSetActionButton` was collapsing every non-spell action to "spell with truncated id" and crashing the V3_4_3 client with `reason=7` when rendered on a side bar. Version-gated repack (V3_4_3 = recombine the int32-packed wire; V1_14/V2_5 = original direct path). TC-verified; V1_14/V2_5 keep prior behaviour by branch. | (commit pending) |
| 2026-05-10 | DK quest 12779 *An End to All Things* — Frostbrood Vanquisher (npc 28670) action-bar slot 2 was rendering spell 53112 (the passive vehicle-control aura) as a clickable button. Native Blizzard left that slot empty. Loaded a per-expansion `PassiveSpells*.csv` (~7,556 entries from cMaNGOS spell.dbc + manual override for 53112) and made `TranslateLegacyPetActionButtonToV343` return slot=0 for matched IDs. | `7ed6c30` |
| 2026-05-10 | DK quest 12800 *The Lich King's Command* (Light's Hope battle) unblocked. Five layered fixes for the in-world-hang / OOM cascade in this dense scripted-event area: (a) translate the fixed-size `SMSG_THREAT_REMOVE` / `SMSG_THREAT_CLEAR` opcodes (the variable-length THREAT_UPDATE pair stays unhandled — an earlier wire-format guess caused a 9 GB WOWGUID OOM); (b) split multi-CreateObject `SMSG_UPDATE_OBJECT` envelopes into per-Create packets for V3_4_3 (the 11-Create batch at 8771 bytes triggered a multi-GB allocation in the client; 5-Create batches survived. Total wire bytes unchanged; per-packet alloc bounded); (c) `AuraDataInfo` converted to `record class` with structural auto-Equals over dedup-eligible fields, `Points` / `EstimatedPoints` typed as `ImmutableArray<float>` for compiler-synthesized structural compare, tick fields kept as plain instance fields (excluded from auto-Equals) — `HandleAuraUpdate` now skips no-op `isAll` resyncs against the cached `KnownAuras[guid]` map; (d) route `SMSG_SPELL_FAILURE` through the existing `HandleSpellFailedOther` (multi-attribute decoration; restores own-cast failure UI); (e) defensive try/catch around the debug-output `ReadFloat` block in `HandleSpellNonMeleeDamageLog` — TC 3.4.3 sometimes ships `debugOutput=1` without the trailing floats and the resulting `ArgumentOutOfRangeException` previously killed the WorldClient receive loop mid-combat. Quest verified end-to-end on TC. | feature branch |
| 2026-05-10 | Hotfix protocol — `SMSG_AVAILABLE_HOTFIXES` per-record field is `UniqueID`, not `TableHash`. The proxy was emitting `(HotfixId, TableHash)` where the V3_4_3 client expects `(PushID, UniqueID)`; client read `TableHash` (a column-set FNV hash) as `UniqueID` (the cache validator), found it didn't match its `Cache/WDB/HotfixCache.bin` entry, and re-requested every hotfix via `CMSG_HOTFIX_REQUEST` on every login (~82 KB `SMSG_HOTFIX_CONNECT` payload each session). Per WPP V2_5+ reference (V3_4_3 inherits this layout). Verified end-to-end: warm-cache login emits 0 `SMSG_HOTFIX_CONNECT` and the client sends 0 `CMSG_HOTFIX_REQUEST`. | feature branch |
| 2026-05-10 | `LoadHotfixes` parallelized — V3_4_3 startup time dropped from ~2095 ms to ~600 ms. The 18-21 sub-loaders ran sequentially even though the outer `GameData` loader pipeline uses `Parallel.Invoke`. Converted `GameData.Hotfixes` from `Dictionary` to `ConcurrentDictionary` (each sub-loader writes to a disjoint `HotfixId` key range via the `HotfixXxxBegin` constants — no actual contention, lock-free fast path), swapped 22 `.Add(key, value)` call sites to indexer assignment, wrapped `LoadHotfixes` body in `Parallel.Invoke`, widened `GetFirstFreeId<T>` to `IDictionary<uint, T>`. V1_14 / V2_5 unchanged (smaller CSVs; parallel overhead is a wash). | feature branch |
| 2026-05-10 | Diagnostic surface — `[ActionBarTrace] reason=7` ring buffer in `WorldSocket` expanded from 40 → 512 entries with a 48-byte hex preview per entry. Captures ~4 sec of pre-disconnect history (vs ~0.3 sec before) and surfaces enough of each packet body to byte-diff against expected wire shapes when investigating sudden-event triggers. The hex preview is what pinned the multi-CreateObject OOM trigger above. | feature branch |
| **2026-05-10 — milestone** | **Death Knight starter chain completed end-to-end on TC.** Whole Acherus → Scarlet Enclave arc playable from class-create through hand-in of the final Light's Hope quest, including the two scripted-event quests that drove most of this session's work (12779 *An End to All Things* — Frostbrood Vanquisher dragon vehicle; 12800 *The Lich King's Command* — Light's Hope mass-mob battle). | feature branch |
| 2026-05-13 | V3_4_3 loot translation — multi-item drain + coin loss on TC 3.3.5 master. Five layered fixes: (a) mid-drain `SMSG_LOOT_RELEASE` suppression + single closing synth on full drain (TC emits one release per item, V3_4_3 client treats each as "close" and ignores the rest); (b) per-corpse `RemainingLootSlots` list + legacy-slot translation, since TC's auto-loot echoes the *clicked* slot byte in every `SMSG_LOOT_REMOVED` instead of the real one — modern client otherwise erased one icon twice and left the other on screen; (c) outbound `SMSG_LOOT_RELEASE.Owner` rewritten to `state.CurrentPlayerGuid` (V3_4_3 server convention; legacy corpse GUID was silently rejected); (d) coin-path close synth moved from `HandleLootCelarMoney` to `HandleLootMoneyNotify` so the wire order is `COIN_REMOVED → MONEY_NOTIFY → RELEASE` as the modern client expects; (e) pre-claim coins via injected `CMSG_LOOT_MONEY` before forwarding `CMSG_AUTOSTORE_LOOT_ITEM`, and suppress the client's redundant follow-up, to dodge TC's session-close-on-item race that orphaned coins (relog confirmed the gold was permanently lost before this fix). cMangos retest pending. | feature branch |

Post-5a opcode-fix flood, grouped by theme:

- **Object updates** — static GameObject `BYTES_1` unpacker, ItemContainer scrub, player/creature Values split for combat DC (ported from fork commit `18caaf7`), `IsEmptyValuesDelta` probes ObjectData (corpse Lootable clear), partial Values updates keep all slot arrays.
- **Player rendering** — real ActionButtons in embedded `ActivePlayer` descriptor (`4058c08`), `ChrCustomization*` hotfix loaders (`fa78793`), strip retail-era `ChrCustomizationChoice` rows the legacy 3.3.5 server rejects (`dc59fcc`).
- **Party / raid** — bits-first wire format for `CMSG_PARTY_INVITE_RESPONSE` (`c0a6934`), `SMSG_PARTY_UPDATE` (`1d1ed8b`), `CMSG_PARTY_UNINVITE` (`34b63a8`); forward Raid bit on `CMSG_CONVERT_RAID` (`7a363ac`); legacy `SMSG_GROUP_LIST` + `PARTY_MEMBER_FULL_STATE` WotLK 3.0+ extras (`3ef5de3`).
- **Quest** — quest-giver wire layouts + V343 status enum + POI translator (`d981816`), owner-typed `PlayerData.QuestLog` in Create + Update (`88d79f5`), vanilla gossip QuestIcon translation + proactive quest-info query (`18ec089`), creature display Probability=100 + drop unconditional ForceGossip (`cd212c6`), `QuestConst.MaxQuestLogSize` (`60c0d8e`).
- **Vendor / item** — vendor merchant V3_4_3 packet layout + MuID round-trip (`384ed13`), `SMSG_ITEM_PUSH_RESULT` layout + descriptor slot translation (`7129b10`), inventory equip/swap CMSG translations + slot helpers (`b17d6cf`), `CMSG_USE_ITEM` legacy wire format (`db63f01`), `CMSG_SET_TRADE_ITEM` descriptor→legacy slot mapping (`8bdf50f`).
- **Hunter ammo + quiver bag render** — fixed Hunter starter ammo (Rough Arrow / item 2512) not visible in inventory or ammo paperdoll slot, and the Light Quiver bag not openable, on TC. Two issues, both in `WriteCreateItemData` / `WriteCreateActivePlayerData` for V3_4_3: (a) `WriteCreateActivePlayerData` had a literal `WriteInt32(0)` at the AmmoID slot instead of `(int)active.AmmoID.GetValueOrDefault()`, so AmmoID always landed as 0 in the player Create block; (b) `WriteCreateItemData` / `WriteEmptyItemCreate` over-wrote by 7 bytes — emitting a Retail-only `itemModifiersCount` uint32 and a 4-byte `WriteInt32(0)` where WPP/V3_4_3 client reads only a 6-bit `ItemModifiers` count. The 7-byte over-shift mangled `ContainerData.Slots[36]` and `NumSlots` (Light Quiver showed `NumSlots=0` and garbage Slot guids on the wire). Verified end-to-end: WPP-diff against canonical CypherCore reference shows `AmmoID: 2512` and `NumSlots: 6` in our Create-block output; in-game bag opens, arrows render, auto-shot fires (`bag_ok_arrow_ok` screenshot 2026-05-03). Bug inherited from fork (which has identical Item-Create layout); fork's TC users likely never noticed because they didn't play a Hunter to the ammo slot.
- **Container Values updates (bag slot population + drag-out clear)** — implemented the Container Values-update wire path for V3_4_3 so post-Create bag changes reach the client: (a) added `WriteUpdateContainerData` matching WPP V3_4_0_45166 `ReadUpdateContainerData` byte-for-byte (2-bit `blocksMask`, up to 2×32-bit `changesMask` blocks, then `NumSlots` and `PackedGuid128` `Slots[i]` per set bit), (b) wired `hasContainerChanges` (`changedMask |= 0x04`) into the `WriteValuesUpdate` dispatcher, (c) made `ObjectUpdateBuilder` consult `_gameState.OriginalObjectTypes` for non-Create updates so bag GUIDs (HighGuid::Item but ObjectType::Container) get the Container bit in `_objectTypeMask`, and (d) added a `ContainerData` probe to `IsEmptyValuesDelta` so Container Values updates that only carry `Slots[]` clears aren't classified as empty and dropped by `FilterV3_4_3Values`. Net effect: `.additem 2512 N` populates new quiver slots immediately, and dragging an item out of an equipped bag clears the source slot in the V3_4_3 UI without requiring a relog (previously left a "grey-outlined ghost" there).
- **Chat** — CMSG + SMSG layouts + NUL trim + emote lang (`3b478e3`), `SMSG_QUERY_PLAYER_NAMES_RESPONSE` plural-form layout (`08821be`).
- **Movement** — `hasStandingOnGameObjectGUID` + `hasAdvFlying` bits (`cab673e`).
- **Auth / login** — port `V3_3_5a/ResponseCodes`, route 3.3.5a backends to it (`7687fdf`); pass `FirstLogin` through and synthesize start zone for new chars (`652ceaa`); drop early `SMSG_MOVE_SET_ACTIVE_MOVER` from `HandleLoginVerifyWorld` (`69999dc`); filter the `0xFFFFFFFF` sentinel from `CMSG_LOADING_SCREEN_NOTIFY.MapID` so the authoritative `CurrentMapId` from `SMSG_LOGIN_VERIFY_WORLD` / `SMSG_NEW_WORLD` survives the loading-screen exit on TC first-login post-creation (`21d0e1a`).
- **Spell casting** — `CMSG_CAST_SPELL` was throwing `IndexOutOfRange` on V3_4_3 because (a) `SpellCastRequest` was missing the V3_4_1+ extras (`removedModificationsCount` count-only uint32, `hasCraftingOrderID` bit-only) and (b) `SpellTargetData` needed a `packet.ResetBitReader()` between sections — without it, 9 cached bits from `SpellCastRequest` got consumed by `Target`'s 39-bit prefix, shifting the byte stream by 1 byte and making the Unit `PackedGuid128` mask read past end-of-packet. New `ByteBuffer.ResetBitReader()` mirrors WPP's helper (`7af45f8`); `SpellCastRequest` reads gated to `V3_4_3_54261` (`557310f`). Symptom in-game was "Invalid target" on the DK runeblade rack interaction (object 190584); plausibly also fixes Pattern A self-cast-dispel and Pattern B ground-targeted-AOE rejections, but those were not re-tested as of writing.
- **Channel spells** — `UnitChannel.ChannelData` Values write was emitting a 4-bit `WriteBits(3 or 7, 4) + FlushBits` prefix that injected 1 byte of garbage before `SpellID`, shifting the V3_4_3 client's read so it parsed random `(ChannelData) SpellID: 13252976` instead of the real spell. CypherCore's `UnitChannel.WriteUpdate` writes `SpellID` + `SpellXSpellVisualID` directly with no inner bit prefix; removed the prefix (`fc238f9`). Cast bar now renders, kneel anim plays, ESC unblocks. **Loop animation also resolved 2026-05-06 (`f0e1ba2`)** — the V3_4_3 `ObjectUpdateBuilder` was silently dropping `UnitData.ChannelObjects` (bit 4 of UnitData `changesMask`) even though `UpdateHandler.cs:1918` already populated it from `UNIT_FIELD_CHANNEL_OBJECT`. Writing the `DynamicUpdateField` in both Create and Values paths (with a `WriteCompleteDynamicFieldUpdateMask` for the bit-mask body) restored the channel-loop animation; also added a `ChannelObject` probe to `IsEmptyValuesDelta` so a channel-end clear isn't filtered out.
- **GameObject interactivity (DK runeblade rack and similar quest GOs)** — Four layered ingest fixes for V3_4_3 (`39cf991`): (1) seed `0xFFFF0000` path-progress in the high 16 bits of `ObjectData.DynamicFlags` on CREATE only — V3_4_3 packs path-progress there and 3.3.5a never writes it, so the modern client treated every static GO as a 0%-progress path object and refused to render; (2) `GAMEOBJECT_DYN_FLAGS` lookup fallback to `GAMEOBJECT_DYNAMIC` for V3_3_5a (the rename silently dropped the legacy server's per-player Activate/Sparkle bits); (3) split stored `ParentRotation` (DB quaternion) from live `HasRotation` (re-derived from `MoveInfo.Position.Orientation`) — cMangos's stored quaternion is desynced for some entries (runeforge faces 34° instead of 304°); (4) drop `GAMEOBJECT_BYTES_1` byte-3 for V3_4_3 — the slot was renamed `AnimProgress` → `PercentHealth` and the writer's `?? 0` fallback now emits a valid HP. Companion fix `ceeada5`: `WriteCreateGameObjectData` defaults `PercentHealth` to 0 (CypherCore's default for non-destructibles) instead of 100, and `ParentRotation` to identity only as a fallback.
- **Item hotfix / icon stability** — `Item*` hotfix RecordIDs must align with the V3_4_3.54261 baked-in DBC slot or the client stores the row but doesn't re-route the tooltip "Use:" line through it. Loaded `CSV/Hotfix/ItemEffect3.csv` into a `(itemId, slot) → RecordID` lookup and reuse the baked RecordID when present, so the wire packet becomes an UPDATE (re-read) instead of a stranded INSERT (`4fa85c1`). Also: skip the `Item` hotfix entirely when DisplayID has no FileDataID mapping in our V3_4_3 hotfix table — TC's DisplayID 50887 (Battle-worn Sword) doesn't exist in V3_4_3 `ItemDisplayInfo` and the V3_4_3 client has `IconFileDataID=135410` baked in, so sending a broken hotfix replaces 135410 with 0 → red "?" icon. `CMSG_DB_QUERY_BULK` is the correct fallback path for genuinely missing rows. Class 12 (quest items) is now flagged as needing `ItemAppearance` + `ItemModifiedAppearance` hotfixes — without them the modern client falls back to red "?" for looted quest items. `GetFirstFreeId` went from O(N) per call to amortised O(1) by threading a per-store cursor `ref uint` (saves ~50M `ContainsKey` calls for 10K hotfixes, append-only callers).
- **Death Knight unlock** — DK didn't appear in the V3_4_3 create-character UI even with the level-55 prerequisite met, because the modern client populates that class list from `SMSG_AUTH_RESPONSE.AvailableClasses` rather than `EnumCharactersResult`. Inject `ClassID=6` into every WotLK race with `MinActiveExpansionLevel=2` (`43957ff`); existing `MaxCharacterLevel` propagation continues to gate the level-55-on-account requirement. Combined with `39cf991` (runeblade rack interactivity for quest 12619) the class is no longer entirely blocked.
- **Projectile rendering (arrows / fireball / missiles)** — Two wire-format bugs (`49ace55`): `SpellTargetData` flag width was 26 bits but V3_4_3 expects 28 (WPP's V3_4_0 module gates the 28 at V3_4_1+); the 2-bit shortfall slid the next has-bit fields into Flags and the client saw `TargetFlags=UnitParty` on hostile-creature targets. And `SMSG_SPELL_GO` needs `CastFlag.HasTrajectory` (`0x2`) ORed in — the 3.3.5a server doesn't set it, but the V3_4_3 client requires it on `SpellGo` (not `SpellStart`) to play the missile visual. Both gated to V3_4_3_54261.
- **Death Knight runes + runic power** — DK is now playable end-to-end on TC. `d2bb304` maps DK `RunicPower` to power slot 0 (V3_4_3 reads runic power from slot 0, not the pre-WotLK rage/energy convention); `2664a0e` wires per-rune state (RuneCooldown[0..5] + RuneRegen + active-rune mask) through to the modern client; `cd93bf6` renders the rune cooldown swirl with correct semantics (server emits elapsed-since-start, client reads remaining-time delta — needed `cooldown - elapsed` instead of forwarding raw); `c9223a2` fixes `SpellCastData` field order + `SpellMissStatus` byte alignment so rune-spending spells animate; `a97004a` demotes the high-rate `SMSG_RESYNC_RUNES` to Trace. Combined with the existing DK class-create unlock (`43957ff`), DK is no longer blocked at any layer.
- **Pet system (V3_4_3)** — pet portrait, action bar, spellbook, and stats now render correctly on V3_4_3 against backends that encode `pet_number` in the GUID's entry slot (cMaNGOS-style, including the user's local TC repack). Five layered fixes: `f8f784e` translates Pet GUID `entry` from `pet_number` → `creature_template.entry` (`GameSessionData` gains `PetRealEntryByLegacyGuid` / `PetLegacyGuidByModern` / `PetModernGuidByNumber` maps + a `WowGuid128` Pet branch consults them; post-parse rewrite at `UpdateHandler.StoreObjectUpdateInternal` after `OBJECT_FIELD_ENTRY` reads); `913818c` translates pet ActionButton/Action wire encoding 3.3.5a↔modern (legacy `state:8 | reserved:8 | spell:16` ↔ modern `slot:9 | spell:23` with state-byte→slot mapping `{0x07,0x06,0x01,0xC1,0xC0,0x81} → {7,6,1,0x181,0x101,0x101}`); `fdb9cff` forwards `SMSG_PET_LEARNED_SPELLS` / `SMSG_PET_UNLEARNED_SPELLS` (length-1 packets, V3_4_3 opcodes 11340/11341); `de77eee` adds the UI-binding race + ordering layer — pre-Create `SMSG_PET_SPELLS_MESSAGE` caching (deferred-flush after pet's CreateObject), pet-batch hold + merge into the player's deferred-flush on existing-pet login (so pet→owner bind sees a populated player), `ResolveStalePetGuid` walking `Summon` / `SummonedBy` / `Charm` / `CharmedBy` / `CreatedBy` / `Target` / `ChannelObject` to rebind at merge time, `PetStatsValuesSynth` follow-up Values UpdateObject re-emitting IsOwner-gated stats (`Stats[5]` / `AttackPower` / `MinDamage` / Resistances) via the bit-mask Values path (no IsOwner gate there), and synthesized `SMSG_PET_LEARNED_SPELLS` from the PetSpells body so the spellbook tab populates on resummon. All gated to V3_4_3_54261; `ResolveStalePetGuid` is a no-op on TC native repacks (PetModernGuidByNumber stays empty).
- **Aura sync (post-CreateObject re-emit)** — `7cbc550` makes Druid Bear Form cancelable on V3_4_3. Root cause: the V3_4_3 client silently drops `SMSG_AURA_UPDATE` packets for units it hasn't `CreateObject`-ed yet, so a player logged out in Bear Form gets the form aura before its own CreateObject and the buff bar stays empty. A prior attempt to populate the deferred-flush from the cached `UNIT_FIELD_AURA` UpdateFields produced 7 entries of garbage (e.g. `BoundingRadius` float `1.0f` read as spell ID `1065353216`) because TC delivers shapeshift / debuff state via `SMSG_AURA_UPDATE` only — that cache is empty for SMSG-only state. Working fix: three coupled changes — `HandleAuraUpdate` translates legacy `NoCaster` (0x08) → modern `AuraFlagsModern.NoCaster` + adds `Cancelable` for self-cast positive auras; `AuraDataInfo` writer skips the `CastUnit` `PackedGuid128` when `NoCaster` is set (CypherCore-canonical wire shape); `GameSessionData.KnownAuras` (per-session live `Dictionary<WowGuid128, Dictionary<byte, AuraInfo>>`) is patched incrementally by `HandleAuraUpdate` and drives `BuildPlayerAuraSync(playerGuid)` at the deferred-flush sites in `UpdateHandler.cs:580` + `QueryHandler.cs:597` to re-emit live aura state AFTER the player's CreateObject lands. Other state likely subject to the same drop pattern (audit when symptoms appear): `SMSG_SET_EXTRA_AURA_INFO`, `SMSG_AURA_POINTS_DEPLETED`, possibly cooldown / equipment-bonus opcodes.
- **Spell-error text correctness** — `ed5e470` adds `SpellCastResultV343` (CypherCore-verbatim) + per-build dispatch in `VersionChecker.ConvertSpellCastResult` on `ModernVersion.Build == V3_4_3_54261`. The V3_4_3 client uses a renumbered `SpellCastResult` enum that diverges from `SpellCastResultClassic` (V1_14 / V2_5-era) starting at index 16 (`CantBeSalvaged` vs `CantBeDisenchanted`) and again at 31 (V3_4_3 inserted `DisabledByPowerScaling`); the per-name CastEnum<> mapping was silently emitting wire codes the V3_4_3 client interpreted as completely unrelated rejects (e.g. `NotShapeshift = 89` mapped to `NotOnTaxi = 89` → "you are in flight" on every Bear Form cast). V1_14 / V2_5 paths unchanged. Mirrors the pre-existing `ResponseCodes` per-build dispatch (`VersionChecker.cs:624-632`); other classic-shared enums likely affected by the same drift (`InventoryResult`, `PartyResultModern`, `LootSlotTypeModern`, `ArenaTeamCommandErrorModern`, `QuestGiverStatusModern`, `DifficultyModern`) — audit when symptoms appear.
- **Resistances + DisplayPower wire fixes** — Two ObjectUpdate writer/reader bugs that landed in the same evening. `e084fb4`: V3_3_5a's `UnitField` enum doesn't define the parent `UNIT_FIELD_RESISTANCES` / `_RESISTANCEBUFFMODSPOSITIVE` / `_RESISTANCEBUFFMODSNEGATIVE` — only the per-element variants (`_ARMOR / _HOLY / _FIRE / _NATURE / _FROST / _SHADOW / _ARCANE`) at contiguous offsets `0x5D-0x71`. The legacy reader's `GetUpdateField(UNIT_FIELD_RESISTANCES)` returned `-1` silently and the entire Resistances + ResistanceBuffMods arrays stayed null, so the V3_4_3 client read Armor (`Resistances[0]`) as 0 and the Stamina-tooltip Health-bonus calc broke. Added a fallback to `GetUpdateField(_ARMOR)` when the parent name resolves to `-1`; mirrors the pre-existing `GAMEOBJECT_DYN_FLAGS → GAMEOBJECT_DYNAMIC` fallback. `8c6f90c`: `WriteUpdateUnitData` wrote `DisplayPower` as `WriteUInt32` when V3_4_3 expects `WriteUInt8` — the 3-byte over-write cascaded forward, corrupting `ShapeshiftForm` and `Stats[2]` in every Values delta. Trust WPP's `UpdateFieldsHandler343` for V3_4_3 field sizes — the `*Classic` writers are V1_14 / V2_5-shaped.
- **Char-list auto-select** — `a515bd0` auto-selects the newly created character on the char list. After `CMSG_CREATE_CHARACTER` succeeded, the V3_4_3 client dropped to no-selection instead of highlighting the freshly created row, forcing an extra click. QoL only; no protocol change.
- **GameObject Values changesMask** — `e34a0a1` drops a stray 1-bit prefix from the `WriteUpdateGameObjectData` `changesMask` write that was shifting all subsequent fields by 1 bit; mirrors the `UnitChannel` no-prefix fix (`fc238f9`).
- **Mail packet family — V3_4_3 wire format** — `d3004b1` rewrites the mail SMSG/CMSG layouts for V3_4_3. Symptom: opening the mailbox froze the modern client and rendered the inbox as garbage (`Enclosed amount: 1.69e+15`, every attachment "Unknown"). Cause: `MailListEntry` / `MailAttachedItem` / `MailCommandResult` (SMSG writers) and `MailDelete` / `MailMarkAsRead` / `MailReturnToSender` / `MailTakeItem` / `MailTakeMoney` / `MailCreateTextItem` (CMSG readers) all used a pre-V3_4_3 layout — Int32 `MailID`/`AttachID`, UInt8 `SenderType`, and a sender-presence-bit pair that the V3_4_3 client doesn't know about. CypherCore `Source/Game/Networking/Packets/MailPackets.cs:286-450` confirms V3_4_3 wants Int64 ids, UInt32 `SenderType`, and a `switch (SenderType)` unconditional sender write (Normal → `WritePackedGuid128`, Auction/Creature/GameObject → `WriteInt32(AltSenderID)`). The Int32→Int64 + UInt8→UInt32 widening shifted every entry by 7 bytes, cascading into junk down the rest of the list. Fix is version-gated on `ModernVersion.Build == V3_4_3_54261`; storage fields widened to `long`; server-side handler now casts `(uint)mail.MailID` etc. when forwarding to the legacy server. Verified end-to-end on TC: `hermes-20260509_175638.log` shows clean list + 6×take-item + 6×delete; vanilla regression guard in `hermes-20260509_194751.log` shows V1_14 mail send/list/take still works (vanilla split intact).

Individual commits beyond what's listed here are auditable via `git log --grep="(v3_4_3|phase5)"`.

---

## Open issues

### Critical (proxy crash — both backends affected)

- **`CMSG_SET_ASSISTANT_LEADER` bits-first parse bug.** Stack trace: `IndexOutOfRangeException` in `ByteBuffer.HasBit()` at `Framework/IO/ByteBuffer.cs:353`, called from `SetAssistantLeader.Read()` at `HermesProxy/World/Server/Packets/GroupPackets.cs:426`, opcode `CMSG_SET_ASSISTANT_LEADER` (13906). Same family as the bits-first party fixes already shipped (`c0a6934`, `1d1ed8b`, `34b63a8`) — V3_4_3 client emits bits-first, upstream parser reads byte-first, runs off the end. Rewrite `SetAssistantLeader.Read()` bits-first. Repros on cMangos but the bug is in our parser, not the legacy server, so it should reproduce on TC too.
- **Druid Typhoon crashes HermesProxy** (fork issue #14). Class-ability spell that takes the proxy down. Need a stack-trace capture at next repro to localize. Possibly related to the `SpellCastRequest` parse bug fixed in `557310f` (same opcode family); re-verify against current HEAD before triaging further.

### Critical (client crash — TC backend)

- **`.additem 2512 1000` crashed the V3_4_3 client (TC)** — *root cause was the same `WriteCreateItemData` layout bug fixed 2026-05-03 (see "Hunter ammo + bag render — RESOLVED" below). The mass-stack `.additem` issue was a downstream symptom of the same 7-byte over-write that mangled descriptor offsets; the additem command itself never reached the proxy in the original repro log because the client was already in a corrupt-state from prior Item creates. Re-verify by repeating the `.additem 2512 1000` test on current HEAD; if it still crashes, file a fresh entry with a new repro log.*

### Class ability gaps (carryover from fork issue [advocaite/HermesProxy-WOTLK#14](https://github.com/advocaite/HermesProxy-WOTLK/issues/14))

The fork received a thorough class-by-class test matrix from `kasperfriend` (2026-04-18, against fork build 2026-04-16, TC backend). The individual symptoms group into a small number of recurring patterns — most are likely single opcode-translation fixes that would unblock dozens of abilities at once. **These results predate our 2026-04-29 → 2026-05-03 work; re-verify against current HEAD before treating each as open.**

**Pattern A — self-cast dispel/cleanse rejects with "can't mount here"** (5+ classes affected). Likely a `CMSG_CAST_SPELL` target-encoding gap for self-targeted dispel/cure spells, or an `SMSG_CAST_FAILED` reason-code translation that maps a generic reject onto `SPELL_FAILED_NOT_MOUNTED`. **May be resolved by `557310f` (`SpellCastRequest` V3_4_1+ wire-fields + `ResetBitReader`) — that fix corrected an `IndexOutOfRange` on every `CMSG_CAST_SPELL` and was confirmed to clear at least the DK runeblade rack "Invalid target" symptom. Re-verify Pattern A against current HEAD before treating it as open.**
  - Paladin: Cleanse, Purify · Priest: Dispel Magic (self), Cure Disease · Shaman: Purge, Cleanse Spirit, Cure Toxins · Mage: Remove Curse · Druid: Cure Poison, Remove Curse

**Pattern B — ground-targeted AOE rejects with "item is not ready yet"** (4+ classes). All `DEST_LOCATION` / ground-click spells. Likely a `CMSG_CAST_SPELL` ground-target position-vector layout gap in V3_4_3. **May also be resolved by `557310f` — the same `SpellCastRequest` / `SpellTargetData` bit-stream desync that produced "Invalid target" likely produced bogus `TargetFlags` reads on ground-target spells too. Re-verify before treating as open.**
  - Priest: Mass Dispel, Lightwell · Druid: Hurricane, Force of Nature · Mage: Flamestrike, Blizzard · Warlock: Shadowfury

**Pattern C — combo-point finishers reject with "requires combo points"**. Likely combo-point descriptor (`PlayerData.ComboPoints`) not propagating to the V3_4_3 client, or modern-client reads it from a different descriptor slot than what we write.
  - Rogue: ALL combo abilities · Druid (Cat): Ferocious Bite, Maim, Rip, Savage Roar

**Pattern D — bear-form rage abilities reject with "not enough rage"**. Same shape as Pattern C but for power-type swap on shapeshift; rage power isn't propagating in bear form.
  - Druid (Bear): Bash, Demoralizing Roar, Lacerate, Maul, Challenging Roar

**Pattern E — shapeshift / form-cancel broken** (locks character in form). Likely `SMSG_AURA_UPDATE` / aura-removal translation gap for cancelable auras. **Bear Form RESOLVED 2026-05-06 (`7cbc550`)** — root cause was the V3_4_3 client silently dropping pre-CreateObject `SMSG_AURA_UPDATE`, so persistent auras never made it to the buff bar; fix re-emits player auras from a per-session `KnownAuras` tracker after the player's CreateObject. Re-verify the other forms / Stealth / Shadowform / Metamorphosis / Ghost Wolf against current HEAD before treating as open.
  - Druid: ALL forms (Cat / Bear / Moonkin / Tree / Aquatic / Travel / Swift Flight) · Rogue: Stealth · Priest: Shadowform · Warlock: Metamorphosis · Shaman: Ghost Wolf

**Pattern F — pet/summon spell panel layout broken** (works but UI broken).
  - Warlock: Summon Imp, Eye of Kilrogg · Priest: Shadowfiend, Mind Vision, Mind Control · Death Knight: Raise Dead

**Pattern G — class-blocker DCs / character-create gating** (severe):
  - **Hunter — total DC on every world enter.** Class-specific hard block in the fork test matrix. **Update 2026-05-03: RESOLVED on TC.** Hunter login + ammo render + auto-shot all work end-to-end. The fork's "DC on world enter" symptom and our follow-up "ammo invisible / quiver not openable / `.additem` client crash" symptoms were a single root cause: `WriteCreateItemData` was over-writing 7 bytes per Item descriptor (Retail-only fields the V3_4_3 client doesn't read), corrupting all subsequent descriptor offsets — which manifests differently depending on what items the hunter has equipped. Fix detailed in the "Hunter ammo + quiver bag render" entry under "Done so far". Ammo decrement on each shot not yet verified — that's an Item Update path test.
  - **Death Knight — cannot be created at character-select.** **RESOLVED 2026-05-03/05.** Two fixes: (a) `43957ff` injects `ClassID=6` into `SMSG_AUTH_RESPONSE.AvailableClasses` for every WotLK race with `MinActiveExpansionLevel=2` — the V3_4_3 create UI populates from there, not `EnumCharactersResult`, so the class was absent from the UI entirely regardless of `MaxCharacterLevel`; (b) `39cf991` makes the runeblade rack quest GO (object 190584, DK starting quest 12619) render and interact correctly. DK abilities not yet exhaustively tested, but the class is no longer blocked at create or at the runeblade rack. **DK gameplay (runes + runic power + cooldown swirl) also working as of 2026-05-06** — see `Death Knight runes + runic power` row in the status table (`d2bb304` + `2664a0e` + `cd93bf6` + `c9223a2`).
  - **Warlock — adding a soulshard to inventory causes DC.** Pet/inventory item type cross-talk; likely a soulshard `Item` create-data write issue.

**Pattern H — misc one-offs**:
  - Mage Slow Fall: "out of range" on self-cast → target-encoding bug, related to Pattern A.
  - Paladin Greater Blessings of Kings/Sanctuary: "you have already learned that spell" → spell-learn dedup translation.
  - Shaman weapon imbues (Windfury / Rockbiter / Frostbrand / Flametongue / Earthliving): "item is already enchanted" → `CMSG_ENCHANT_ITEM` (or similar) translation gap.
  - Holy Wrath / Holy Shock: animate but no damage / no heal → `SMSG_SPELL_GO` effect translation might be losing damage/heal payload for hybrid school spells.

**Suggested triage order**: Patterns A (1 fix unblocks 10+ spells) → C (combo points, ~5 spells) → E (form cancel, ~10 abilities) → B (ground AOE, ~6 spells) → G-Hunter (whole class blocked) → G-Warlock-soulshard. Patterns D/F/H drop out as the underlying descriptor/translation work in A/C/E lands.

### TC-specific gaps (cMangos works for these)

- ~~**First login after character creation: character cannot move until relog (TC).**~~ **RESOLVED 2026-05-03 (`21d0e1a`).** Root cause: `CMSG_LOADING_SCREEN_NOTIFY.MapID` is `uint`; a `>= 0` guard was tautological and let the client's `0xFFFFFFFF` "exit loading screen" sentinel slip into `GameState.CurrentMapId`. `UpdatePackets` truncated that to `(ushort)0xFFFF=65535`, poisoning every subsequent `SMSG_UPDATE_OBJECT.MapID`; the V3_4_3 client read that as an invalid map and refused to apply Values updates, so the camera stayed anchored at the cinematic-start frame with no character control. Filtering the sentinel keeps the authoritative `CurrentMapId` from `SMSG_LOGIN_VERIFY_WORLD` / `SMSG_NEW_WORLD` intact across the loading-screen exit. Verified end-to-end on TC: fresh Human character plays the cinematic, camera releases, character moves, logout works. **Caveat (still open)**: skipping the cinematic too early can still disconnect the client (reason 7) — separate timing-related issue where the legacy server's world-emit burst is mid-flight when `CMSG_COMPLETE_CINEMATIC` fires.

### cMangos-specific gaps (TC works for these)

- **Quest log state desync.** Picked-up quests don't always appear in the log immediately; relogging refreshes. Quest tracker + map markers update live, so only the log-list view is affected. Likely a `SMSG_QUESTUPDATE_*` or `ActivePlayer.QuestLog` descriptor delta translation gap on cMangos's wire output.
- **Special abilities reject "invalid target."** Heroic Strike (and presumably other rage/energy "next attack" abilities) reject when used on a mob the player is auto-attacking. Likely a target-selection / GUID translation mismatch on cMangos's swing path.
- **Vendor sell.** Selling items leaves the slot rendered as a permanent grey item — the slot doesn't free up. Buy works, so the inventory packet scaffolding is there. Likely a `CMSG_SELL_ITEM` / `SMSG_SELL_ITEM_RESPONSE` translation gap on cMangos.
- **"On use" inventory items don't trigger.** Food (Shiny Red Apple), bandages, potions, etc. don't fire when right-clicked from inventory. Likely a `CMSG_USE_ITEM` legacy wire-format gap specific to cMangos's expected layout (the `db63f01` fix targeted modern→legacy translation; cMangos may want a different field order or slot index).
- **Raid kick disbands the whole raid (party uninvite is fine).** In raid mode, kicking a single member disbands the entire raid; in party mode, uninvite works. So the `34b63a8` `CMSG_PARTY_UNINVITE` fix is OK for party. The raid-only path likely takes a different opcode (`CMSG_GROUP_UNINVITE_GUID` or the V3_4_3-renamed variant) that translates to a legacy disband instead of single-member uninvite.

### Loot

- ~~**Multi-item loot: clicking the first item blocks the second (TC observed).**~~ **RESOLVED 2026-05-13.** Root cause was three independent V3_4_3 wire-translation bugs stacking, plus a coin race specific to TC 3.3.5 master's auto-loot. With a corpse / chest that has 2+ lootable items, TC 3.3.5 master's auto-loot drains everything from a single `CMSG_AUTOSTORE_LOOT_ITEM` but echoes the *clicked* slot byte in every per-item `SMSG_LOOT_REMOVED`, so the modern client erased slot N twice and never erased slot N+1. Compounding that, TC sends one `SMSG_LOOT_RELEASE` per item (premature close) and stamps `Owner` as the corpse GUID, both of which the V3_4_3 client rejects. Fix detailed in the row above; commit message has the full breakdown. The FasterLooting + Auto Loot deterministic-repro tip from `2026-05-09` is now obsolete (multi-item loot works without it).
- ~~**Repro tip — Auto Loot + FasterLooting addon makes the multi-item bug consistently reproducible (TC, 2026-05-09).**~~ **RESOLVED 2026-05-13.** See above — the underlying multi-item path now correctly translates per-item `SMSG_LOOT_REMOVED` regardless of how many clicks the client fires in rapid succession.

### Mail

- ~~**Mail money attachment shows garbled value (TC).**~~ **RESOLVED 2026-05-09 (`d3004b1`).** Root cause was wider than just money rendering: the entire V3_4_3 SMSG_MAIL_LIST_RESULT entry layout was misaligned by 7 bytes (Int32 `MailID` vs expected Int64 + UInt8 `SenderType` vs expected UInt32 + bit-flagged sender presence vs expected switch-on-SenderType). The "1.69e+15 copper" garbage value was a misaligned read of `Flags` + `DaysLeft` floats interpreted as money. Fix detailed in the "Mail packet family — V3_4_3 wire format" entry under "Done so far".

### Item tooltips (observed 2026-05-09)

- **Random-enchantment ("of the …") suffix stats missing from tooltip — cosmetic (TC).** Item 8178 *Training Sword* (random suffix, e.g. "of the Bear") shows only the base stats in its in-game tooltip; the suffix bonus stats correctly apply to the character pane, so the server-side `ItemRandomProperties` / `ItemRandomSuffix` payload is reaching the player and being honored. The gap is purely in tooltip rendering — likely the V3_4_3 client reads suffix stats from a different `ItemSparse` / `ItemEffect` hotfix slot than where we're emitting, or the `SMSG_ITEM_QUERY_SINGLE_RESPONSE` / `Item` create-data field carrying the random-suffix index is being dropped for V3_4_3. cMangos untested. Triage starting points: WPP-diff a known-suffix item tooltip query against a CypherCore-native capture; check `ItemSparse3.csv` / `ItemRandomProperties` / `ItemRandomSuffix` paths.

### Quest UI (observed 2026-05-09)

- **Default Quest Helper: click-to-navigate from quest tracker doesn't open the map.** Blue polygon mesh now renders correctly (fixed 2026-05-17). Remaining gap: clicking a quest entry / numbered objective in the tracker UI does not auto-open the world map at the relevant zone. Same behaviour observed running the HermesProxy-WOTLK fork against the same TC 3.3.5 backend, so this is not a regression — it's the proxy's shared data gap. Root cause: legacy `SMSG_QUEST_QUERY_RESPONSE` carries `POIContinent` / `POIx` / `POIy` / `POIPriority` (forwarded in `HermesProxy/World/Client/PacketHandlers/QueryHandler.cs:167`), but TC 3.3.5 `quest_template` has these zeroed for most quests, so the modern client has no target coordinates to navigate to. Possible fix: synthesize `POIContinent` / `POIx` / `POIy` from the first POI blob point of the first non-wildcard objective when the legacy values are 0 (the POI data we already translate carries map + coords). cMangos untested.

### Rendering / world-population

- **MOTransport (zeppelins, elevators).** Untested in-game as of 2026-05-03. The Transport / MOTransport filter at `UpdateHandler.cs:162/229` is still in place from the 5a-era unblock, so they're definitely not rendering even if the underlying issue has been quietly fixed. First step: verify on TC + cMangos. If still broken, decide between option A (aggregate position from later `GAMEOBJECT_POS_*` deltas) vs option B (Movement block) vs option C (capture-diff cMangos vs TC to find the real divergence).

### Combat & state propagation

- **Channel-spell loop animation (TC fixed 2026-05-06; cMangos retest pending).** Root cause was hypothesis (a): the V3_4_3 `ChannelObjects` DynamicUpdateField (bit 4 of UnitData changesMask) was never being written by `ObjectUpdateBuilder.cs`, so the modern client received an empty channel-target list and dropped the loop animation after the start anim. The legacy reader (`UpdateHandler.cs:1918`) was already populating `UnitData.ChannelObject` from `UNIT_FIELD_CHANNEL_OBJECT`; the data was being silently dropped at the V3_4_3 writer. Fix: write `uint32(ChannelObjects.size())` + the GUID body in the create path, and `WriteCompleteDynamicFieldUpdateMask` + GUID body (before Health, per TC ordering) in the values path; also added `ChannelObject` probe to `IsEmptyValuesDelta` so a channel-end clear isn't dropped.
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

---

## Action bar visibility & macro persistence (V3_4_3) — May 2026

### Solved

- **Macros on action bars (all bars 1-12) persist across logout/login.**
  Root cause was the modern V3_4_3 wire format for `CMSG_SET_ACTION_BUTTON`
  (Int32 packed action+type) being mis-decoded as two independent uint16
  fields by the CypherCore-derived `SetActionButton.Read()`. Macros, items,
  mounts, equipment sets and companions were all collapsing to a SPELL with a
  truncated id, and the V3_4_3 client crashed the second or third time it
  tried to render the resulting bogus slot on a side bar (most reproducibly
  Action Bar 4).

  Fix: version-gated repack in `HandleSetActionButton`
  (`HermesProxy/World/Server/PacketHandlers/CharacterHandler.cs`):

  | Client | Wire layout (per WPP) | Repack |
  |---|---|---|
  | V3_4_3 (V3_4_0 module) | Int32 packed (low24=action, high8=type) + Byte | Recombine: `actionReal = Action \| ((Type&0xFF)<<16)`, `typeReal = (Type>>8)&0xFF` |
  | V1_14 / V2_5 (V1_13_2 module) | Int16 Action + Int16 Type + Byte (independent fields) | Direct: `actionReal = Action`, `typeReal = Type & 0xFF` |

  Trace logging at `[V343Trace][SaveButton]` shows wire bytes, build branch
  and the resolved `actionReal/typeReal/packedLE` triple per click.

- **`alwaysShowActionBars` checkbox persists across logout/login.** Worked
  through the natural account-data round-trip — the V3_4_3 client uploads
  this CVar to `GlobalConfigCache` (account-data type 0) and reads it back.
  No proxy work needed; verified in `data-0.bin`.

- **`MultiActionBars` legacy descriptor field** is correctly extracted from
  `PLAYER_FIELD_BYTES` byte 2 and written to the V3_4_3 modern descriptor at
  bit 72 in block 70 (matching `trinitywotlk` reference, *not* the newer
  `wotlk_classic` repo's bit 78 which is V3_4_4+). Bit 4 of this byte
  (`alwaysShowActionBars`) IS read by the V3_4_3 client UI; bits 0-3 (the
  four extra bars) are not.

### Open / not solved

- **Action Bar 2 / 3 / 4 / 5 visibility checkboxes do NOT persist.** User
  must re-tick each bar after every login. Cause: the V3_4_3 client UI does
  not use the legacy `bottomLeftActionBar` / `bottomRightActionBar` /
  `rightActionBar` / `rightActionBar2` CVars for bar visibility — strongly
  suspected to be the Edit Mode account-data system that V3_4_4+ formalises
  via types 13 (`GLOBAL_EDIT_MODE_CACHE`) and 14
  (`PER_CHARACTER_EDIT_MODE_CACHE`). HermesProxy's `AccountDataType` enum
  tops out at 12, so these new types aren't wired.

  We injected the legacy CVars defensively (Phase 7 augmenter +
  Phase 8 timestamp bump in `ClientConfigHandler` /
  `WorldSocket.SendAccountDataTimes`, both V3_4_3-gated). The push reaches
  the client (verified by the `[ActionBarTrace] augmented type-0 response
  with action-bar CVars` log line) but the bar-visibility UI ignores it.

  Real fix needs a packet capture showing the V3_4_3 client's actual Edit
  Mode wire format — out of scope until that capture exists.

  Workaround: user re-toggles each session.

- **Sporadic `CMSG_LOG_DISCONNECT(reason=7)` under load** (combat / hotfix
  bursts / bar toggles). The ring buffer in `WorldSocket` dumps the last 40
  SMSG packets sent before each disconnect (search log for
  `[ActionBarTrace] reason=7`). Across multiple captured disconnects the
  buffer never showed an obviously malformed packet from the proxy — looks
  like a V3_4_3 client × HermesProxy stability issue under the 3.3.5
  backend that needs client-side telemetry to pin down.

### Diagnostic logging left in place

All emit at `LogType.Trace` under the `[ActionBarTrace]` prefix
(`[V343Trace][SaveButton]` for the action-button path):

| Where | What |
|---|---|
| `World/Server/PacketHandlers/ClientConfigHandler.cs` | `HandleUpdateAccountData` (DataType + size + decompressed text preview); `HandleRequestAccountData` (DataType + slot present); type-0 response augmentation log |
| `World/Server/PacketHandlers/CharacterHandler.cs` (~line 247) | `HandleSetActionBarToggles` (Mask byte hex+binary); `HandleSetActionButton` (slot→bar mapping + wire+resolved values+legacy packedLE) |
| `World/Server/WorldSocket.cs` (~line 1184) | `SendAccountDataTimes` (non-zero slot timestamps); type-0 timestamp bump; ring-buffer dump on `CMSG_LOG_DISCONNECT(reason=7)` |
| `World/Client/PacketHandlers/UpdateHandler.cs:3171` | `MultiActionBars` byte extracted from `PLAYER_FIELD_BYTES` (hex + binary + raw32 + guid) |

Persisted state: per-character `MultiActionBarsMask` (nullable byte) on
`PlayerSettings.InternalStorage` / `settings.json` — captures the last
non-zero `CMSG_SET_ACTION_BAR_TOGGLES` mask the user sent, so Phase 7+8
can replay it on next login (currently inert for bars 2-5 per the open
issue above).
