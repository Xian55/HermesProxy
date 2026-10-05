#!/usr/bin/env python3
"""Regenerate HermesProxy/CSV/BroadcastTexts{N}.csv from legacy world databases.

Why this is not a recipe in build-csv-from-db2.py
-------------------------------------------------
The Classic clients ship no BroadcastText rows. wago.tools has no such table for
1.14.2, 2.5.3 or 3.4.3, and the client asks the proxy for every id it is handed
(CMSG_DB_QUERY_BULK), then keeps the answer in its own cache. So this file is not an
override of client data and nothing in it is sent unless the client asks. It is a
reverse index:

    greeting text the legacy server sent  ->  the id the client will know it by

Its job is to keep a stock greeting on the id the original data gave it, and to let the
proxy answer for that id without having seen the text first. A text that is missing still
gets a stable id, derived from the text itself (World/BroadcastTextRegistry.cs).

BroadcastTexts3.csv was a verbatim copy of the TBC file and matched 29.5% of the
greetings a stock AzerothCore sends.

What a row has to match
-----------------------
The lookup compares against what arrives in SMSG_NPC_TEXT_UPDATE, which is not the
`broadcast_text` row as stored:

- TrinityCore, AzerothCore and cMaNGOS all send the other gender's text when one is
  empty, so male == female on the wire for most rows. A row keeps its real, possibly
  empty, pair, and the lookup fills it the same way before comparing.
- TrinityCore and AzerothCore take language and emotes from `npc_text`, not from the
  broadcast row. cMaNGOS takes them from the broadcast row when
  `npc_text_broadcast_text` maps the entry, and from `npc_text` otherwise.
- The proxy trims trailing whitespace before comparing, so texts are stored trimmed.

The file holds one row per id, so two greetings that share a text but differ in
language or emotes need two ids. The second one takes another broadcast row carrying
the same text, preferring the one whose own language and emotes agree.

Usage
-----
    python scripts/build-broadcast-texts-csv.py 3 --audit \\
        trinity=trinity-world.sql \\
        azerothcore=<azerothcore>/data/sql/base/db_world \\
        cmangos=cmangos-world.sql
    python scripts/build-broadcast-texts-csv.py 3 trinity=... azerothcore=... cmangos=...

Each source is `label=path`. The path is a .sql dump, or a directory holding
`npc_text.sql` and `broadcast_text.sql`. The schema is detected from the columns, so
the label only names the source in the report. Order matters once: when two backends
each send their own variant of a text and one id is left, the source listed first
gets it. To dump the tables from a live server:

    mariadb-dump --compact --default-character-set=utf8mb4 <world> \\
        npc_text broadcast_text > trinity-world.sql
    mariadb-dump --compact --default-character-set=utf8mb4 <world> \\
        npc_text npc_text_broadcast_text broadcast_text > cmangos-world.sql

`--audit` prints the report and writes nothing. The report replays the proxy's lookup
against both the committed file and the regenerated one, per source:

    hit    a row carries the greeting: the same text for both genders, language, emotes
    miss   no row; the proxy derives an id from the text
    lost   hit with the committed file, not with the regenerated one. Must be 0.
"""

from __future__ import annotations

import argparse
import csv
import io
import re
import sys
from collections import Counter, defaultdict
from dataclasses import dataclass, field
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
CSV_DIR = REPO_ROOT / "HermesProxy" / "CSV"

HEADER = [
    "Entry", "MaleText", "FemaleText", "LanguageId",
    "EmoteId1", "EmoteId2", "EmoteId3", "EmoteDelay1", "EmoteDelay2", "EmoteDelay3",
]
TABLES = {"npc_text", "broadcast_text", "npc_text_broadcast_text"}
OPTIONS = 8
# The servers answer an unknown text id with this in all eight slots; the proxy drops it
# from every slot but the first (QueryHandler.HandleQueryNpcTextResponse).
PLACEHOLDER = "Greetings $N"

CREATE_RE = re.compile(r"CREATE TABLE (?:IF NOT EXISTS )?`(\w+)` \((.*?)\n\)", re.S)
COLUMN_RE = re.compile(r"^\s*`(\w+)`", re.M)
INSERT_RE = re.compile(r"INSERT INTO `(\w+)`\s*(?:\(([^)]*)\)\s*)?VALUES\s*")
SKIP_RE = re.compile(r"[\s,]*")
VALUE_RE = re.compile(r"'([^'\\]*(?:(?:\\.|'')[^'\\]*)*)'|([^,()']+)", re.S)
ESCAPE_RE = re.compile(r"\\(.)|''", re.S)
MYSQL_ESCAPES = {"n": "\n", "r": "\r", "t": "\t", "0": "\0", "b": "\b", "Z": "\x1a"}

Row = dict[str, "str | None"]
# (male, female, language, delays, emotes), exactly as BroadcastTextRegistry.Resolve receives them.
Identity = tuple[str, str, int, tuple[int, int, int], tuple[int, int, int]]


def unescape(raw: str) -> str:
    if "\\" not in raw and "''" not in raw:
        return raw
    return ESCAPE_RE.sub(
        lambda m: "'" if m.group(1) is None else MYSQL_ESCAPES.get(m.group(1), m.group(1)), raw
    )


def read_sql(path: Path, tables: dict[str, list[Row]]) -> None:
    text = path.read_text(encoding="utf-8")
    columns = {
        m.group(1).lower(): [c.lower() for c in COLUMN_RE.findall(m.group(2))]
        for m in CREATE_RE.finditer(text)
    }

    pos = 0
    while True:
        # Searched from the end of the previous statement, never inside one, so an
        # "INSERT INTO" within a text value cannot start a bogus statement.
        insert = INSERT_RE.search(text, pos)
        if insert is None:
            return
        name = insert.group(1).lower()
        if insert.group(2):
            header = [c.strip(" `").lower() for c in insert.group(2).split(",")]
        else:
            header = columns.get(name, [])
        keep = name in TABLES
        if keep and not header:
            sys.exit(f"{path}: INSERT INTO `{name}` without a column list or CREATE TABLE")

        pos = insert.end()
        while True:
            pos = SKIP_RE.match(text, pos).end()
            if text[pos] == ";":
                pos += 1
                break
            if text[pos] != "(":
                sys.exit(f"{path}: unexpected {text[pos:pos + 40]!r} in `{name}` values")
            pos += 1
            values: list[str | None] = []
            while True:
                value = VALUE_RE.match(text, pos)
                if value is None:
                    sys.exit(f"{path}: cannot parse {text[pos:pos + 40]!r} in `{name}`")
                if value.group(1) is not None:
                    values.append(unescape(value.group(1)))
                else:
                    bare = value.group(2).strip()
                    values.append(None if bare == "NULL" else bare)
                pos = value.end()
                if text[pos] == ",":
                    pos += 1
                    continue
                pos += 1  # ')'
                break
            if keep:
                if len(values) != len(header):
                    sys.exit(f"{path}: `{name}` row has {len(values)} values for {len(header)} columns")
                tables[name].append(dict(zip(header, values)))


def read_source(path: Path) -> dict[str, list[Row]]:
    tables: dict[str, list[Row]] = defaultdict(list)
    if path.is_dir():
        files = [path / f"{name}.sql" for name in sorted(TABLES) if (path / f"{name}.sql").exists()]
    elif path.is_file():
        files = [path]
    else:
        sys.exit(f"{path}: not a file or directory")
    for file in files:
        read_sql(file, tables)
    for required in ("npc_text", "broadcast_text"):
        if not tables[required]:
            sys.exit(f"{path}: no `{required}` rows found")
    return tables


def pick(row: Row, *names: str) -> str | None:
    for name in names:
        if name in row:
            return row[name]
    raise KeyError(f"none of {names} in columns {sorted(row)}")


def number(value: str | None, mask: int) -> int:
    return int(value or 0) & mask


def clean(text: str) -> str:
    # QueryHandler: packet.ReadCString().TrimEnd().Replace("\0", "")
    return text.rstrip().replace("\0", "")


def on_wire(male: str, female: str) -> tuple[str, str]:
    """What the proxy compares: the servers' gender fill-in, then the proxy's trim."""
    return clean(male or female), clean(female or male)


@dataclass(frozen=True)
class Broadcast:
    male: str
    female: str
    language: int
    delays: tuple[int, int, int]
    emotes: tuple[int, int, int]


@dataclass
class Source:
    label: str
    broadcast: dict[int, Broadcast]
    # identity -> how many npc_text slots send it, and which broadcast id each declared
    identities: dict[Identity, Counter[int]] = field(default_factory=dict)
    slots: int = 0


def load_source(label: str, path: Path) -> Source:
    tables = read_source(path)

    broadcast: dict[int, Broadcast] = {}
    for row in tables["broadcast_text"]:
        broadcast[int(pick(row, "id") or 0)] = Broadcast(
            male=pick(row, "maletext", "text") or "",
            female=pick(row, "femaletext", "text1") or "",
            language=number(pick(row, "languageid"), 0xFFFFFFFF),
            delays=tuple(number(row[f"emotedelay{j}"], 0xFFFF) for j in (1, 2, 3)),
            emotes=tuple(number(row[f"emoteid{j}"], 0xFFFF) for j in (1, 2, 3)),
        )

    source = Source(label, broadcast)
    # text id -> eight (male, female, language, delays, emotes, declared broadcast id)
    gossip: dict[int, list[tuple[str, str, int, tuple, tuple, int]]] = {}

    for row in tables["npc_text"]:
        slots = []
        for i in range(OPTIONS):
            if f"emotedelay{i}_0" in row:  # TrinityCore
                delays = tuple(number(row[f"emotedelay{i}_{j}"], 0xFFFF) for j in range(3))
                emotes = tuple(number(row[f"emote{i}_{j}"], 0xFFFF) for j in range(3))
            else:  # AzerothCore, cMaNGOS: em{i}_0..5 is delay, emote, delay, emote, ...
                delays = tuple(number(row[f"em{i}_{2 * j}"], 0xFFFF) for j in range(3))
                emotes = tuple(number(row[f"em{i}_{2 * j + 1}"], 0xFFFF) for j in range(3))
            declared = int(row.get(f"broadcasttextid{i}") or 0)
            text = broadcast.get(declared)
            if text is not None:
                male, female = text.male, text.female
            else:
                declared = 0
                male, female = row[f"text{i}_0"] or "", row[f"text{i}_1"] or ""
            slots.append((male, female, number(row[f"lang{i}"], 0xFFFFFFFF), delays, emotes, declared))
        gossip[int(pick(row, "id") or 0)] = slots

    # cMaNGOS: an entry here replaces all eight slots, and takes language and emotes
    # from the broadcast row (ObjectMgr::LoadGossipText).
    for row in tables["npc_text_broadcast_text"]:
        slots = []
        for i in range(OPTIONS):
            declared = int(row[f"broadcasttextid{i}"] or 0)
            text = broadcast.get(declared)
            if text is not None:
                slots.append((text.male, text.female, text.language, text.delays, text.emotes, declared))
            else:
                slots.append(("", "", 0, (0, 0, 0), (0, 0, 0), 0))
        gossip[int(pick(row, "id") or 0)] = slots

    for slots in gossip.values():
        for i, (male, female, language, delays, emotes, declared) in enumerate(slots):
            wire_male, wire_female = on_wire(male, female)
            if not wire_male and not wire_female:
                continue
            if i != 0 and wire_male == PLACEHOLDER and wire_female == PLACEHOLDER:
                continue
            source.slots += 1
            identity = (wire_male, wire_female, language, delays, emotes)
            source.identities.setdefault(identity, Counter())[declared] += 1

    return source


def assign(
    sources: list[Source], settled: dict[Identity, int]
) -> tuple[dict[int, tuple[Identity, Broadcast]], Counter[str]]:
    """Give every greeting not in `settled` one free broadcast id that carries its text."""
    by_text: dict[tuple[str, str], dict[int, Broadcast]] = defaultdict(dict)
    for source in sources:
        for entry, text in source.broadcast.items():
            if entry:
                by_text[on_wire(text.male, text.female)].setdefault(entry, text)

    # identity -> (backends sending it, position of the first of them, npc_text slots)
    weight: dict[Identity, tuple[int, int, int]] = {}
    declared: dict[Identity, Counter[int]] = defaultdict(Counter)
    for position, source in enumerate(sources):
        for identity, ids in source.identities.items():
            used_by, first, slots = weight.get(identity, (0, position, 0))
            weight[identity] = (used_by + 1, first, slots + sum(ids.values()))
            declared[identity].update(ids)

    rows: dict[int, tuple[Identity, Broadcast]] = {}
    outcome: Counter[str] = Counter()
    # A contested id goes to the greeting more backends send, then to the backend
    # listed first.
    taken = set(settled.values())
    for identity in sorted(weight, key=lambda i: (-weight[i][0], weight[i][1], -weight[i][2], i)):
        if identity in settled:
            outcome["kept its committed id"] += 1
            continue
        candidates = by_text.get((identity[0], identity[1]), {})
        if not candidates:
            outcome["no broadcast row carries the text"] += 1
            continue

        def agrees(entry: int) -> bool:
            text = candidates[entry]
            return (text.language, text.delays, text.emotes) == identity[2:]

        wanted = [e for e, _ in declared[identity].most_common() if e in candidates]
        others = sorted(candidates.keys() - set(wanted))
        order = (
            [e for e in wanted if agrees(e)] + [e for e in others if agrees(e)]
            + [e for e in wanted if not agrees(e)] + [e for e in others if not agrees(e)]
        )
        entry = next((e for e in order if e not in taken), None)
        if entry is None:
            outcome["every id with the text is taken"] += 1
            continue
        taken.add(entry)
        rows[entry] = (identity, candidates[entry])
        outcome["assigned a new id"] += 1

    return rows, outcome


def as_csv_rows(rows: dict[int, tuple[Identity, Broadcast]]) -> list[list[str]]:
    result = []
    for entry in sorted(rows):
        (_, _, language, delays, emotes), text = rows[entry]
        result.append(
            [str(entry), clean(text.male), clean(text.female), str(language)]
            + [str(e) for e in emotes] + [str(d) for d in delays]
        )
    return result


class Lookup:
    """BroadcastTextRegistry: the lowest id whose wire text, language and emotes all match."""

    def __init__(self, csv_rows: list[list[str]]) -> None:
        self.ids: dict[Identity, int] = {}
        for row in csv_rows:
            entry = int(row[0])
            emotes = tuple(int(v) for v in row[4:7])
            delays = tuple(int(v) for v in row[7:10])
            identity = (*on_wire(row[1], row[2]), int(row[3]), delays, emotes)
            if entry < self.ids.get(identity, entry + 1):
                self.ids[identity] = entry

    def find(self, identity: Identity) -> int | None:
        return self.ids.get(identity)

    def classify(self, identity: Identity) -> str:
        return "miss" if self.find(identity) is None else "hit"


def render(csv_rows: list[list[str]]) -> str:
    buffer = io.StringIO(newline="")
    writer = csv.writer(buffer, lineterminator="\n", quoting=csv.QUOTE_ALL)
    writer.writerow(HEADER)
    writer.writerows(csv_rows)
    return buffer.getvalue()


def read_csv(path: Path) -> list[list[str]]:
    if not path.exists():
        return []
    with path.open(encoding="utf-8-sig", newline="") as handle:
        reader = csv.reader(handle)
        next(reader, None)
        return [row for row in reader if row]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("expansion", choices=["1", "2", "3"], help="legacy expansion: the N in BroadcastTexts{N}.csv")
    parser.add_argument("sources", nargs="+", metavar="label=path", help="world database dump(s) for that expansion")
    parser.add_argument("--audit", action="store_true", help="print the report, write nothing")
    args = parser.parse_args()
    csv.field_size_limit(1 << 30)

    sources = []
    for spec in args.sources:
        label, separator, path = spec.partition("=")
        if not separator:
            parser.error(f"'{spec}' is not label=path")
        sources.append(load_source(label, Path(path)))

    target = CSV_DIR / f"BroadcastTexts{args.expansion}.csv"
    old_rows = read_csv(target)
    old_lookup = Lookup(old_rows)

    # A greeting the committed file already resolves keeps its row untouched: clients
    # hold that id in their cache, with that language and those emotes.
    settled = {
        identity: old_lookup.find(identity)
        for source in sources
        for identity in source.identities
        if old_lookup.classify(identity) == "hit"
    }
    kept = set(settled.values())
    rows, outcome = assign(sources, settled)
    new_rows = sorted(
        [row for row in old_rows if int(row[0]) in kept] + as_csv_rows(rows), key=lambda r: int(r[0])
    )

    print(f"{'source':<14}{'broadcast':>10}{'slots':>8}{'greetings':>11}")
    for source in sources:
        print(f"{source.label:<14}{len(source.broadcast):>10}{source.slots:>8}{len(source.identities):>11}")
    print(f"\ndistinct greetings across sources: {sum(outcome.values())}")
    for reason, count in outcome.most_common():
        print(f"  {count:>6}  {reason}")

    new_lookup = Lookup(new_rows)
    print(f"\nlookup replay{'':<2}{'committed: hit  miss':>24}{'regenerated: hit  miss':>26}{'lost':>6}")
    lost_total = 0
    for source in sources:
        before: Counter[str] = Counter()
        after: Counter[str] = Counter()
        lost = 0
        for identity in source.identities:
            was, now = old_lookup.classify(identity), new_lookup.classify(identity)
            before[was] += 1
            after[now] += 1
            lost += was == "hit" and now != "hit"
        lost_total += lost
        print(
            f"{source.label:<14}"
            f"{before['hit']:>19}{before['miss']:>6}"
            f"{after['hit']:>21}{after['miss']:>6}{lost:>6}"
        )

    old_ids = {row[0]: row for row in old_rows}
    new_ids = {row[0]: row for row in new_rows}
    changed = sum(1 for key in old_ids.keys() & new_ids.keys() if old_ids[key] != new_ids[key])
    rendered = render(new_rows)
    print(
        f"\n{target.name}: rows {len(old_rows)} -> {len(new_rows)}  "
        f"added={len(new_ids.keys() - old_ids.keys())} removed={len(old_ids.keys() - new_ids.keys())} "
        f"changed={changed}  size={len(rendered.encode('utf-8')) / 1e6:.2f} MB"
    )

    if lost_total:
        print(f"\n{lost_total} greeting(s) matched before and no longer do; not writing", file=sys.stderr)
        return 1
    if not args.audit:
        target.write_text(rendered, encoding="utf-8", newline="")
        print(f"wrote {target.relative_to(REPO_ROOT)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
