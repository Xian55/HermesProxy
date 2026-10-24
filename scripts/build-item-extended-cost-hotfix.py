#!/usr/bin/env python3
"""Build CSV/Hotfix/ItemExtendedCost{3,4}.csv: a 3.3.5a server's vendor prices for a modern client.

A vendor list names an item's price by its ItemExtendedCost id alone, and the client looks the id
up in its own table. Wrath Classic and Cataclysm Classic changed a good part of 3.3.5a's rows:
row 386 (the Alliance Battle Standard) is 15300 honor on 3.3.5a, 7650 on the 3.4.3 client and
2550 on the 4.4.2 client. The server charges its own price, so the client showed one price and
the server took another, or refused a purchase the client had offered.

This writes the 3.3.5a rows the client has differently, or not at all, in the client's own layout
(TrinityCore wotlk_classic / cata_classic ItemExtendedCostLoadInfo), which the proxy sends as
hotfixes when its server is 3.3.5a:

    - honor (1901) and arena points (1900) are currencies; the 4.4.2 client counts honor in
      hundredths (CurrencyTypes flag 0x8), arena points whole;
    - a token item the client knows as a currency (CSV/CurrencyTypes{N}.csv: Emblem of Frost,
      Stone Keeper's Shard...) becomes that currency, as the client's own rows have it, so a row
      that differs only in that form is left alone;
    - the rating and bracket requirements are 3.3.5a's; Flags, MinFactionID, MinReputation and
      RequiredAchievement are 0, which 3.3.5a has no columns for.

A 4.3.4 server has the same problem on a smaller scale: three of its ids are missing from the
4.4.2 client (2285, 175 honor, at the arena vendors among them) and 71 rows ask for another rating.
With --cata-db2 the source is 4.3.4's own ItemExtendedCost.db2 and the result goes to
ItemExtendedCostCata4.csv, which the proxy sends in front of a 4.3.4 server: the 4.3.4 row as it
is, with its Honor Points (392) as the 4.4.2 client's (1901). Both count in hundredths.

Usage
-----
    python scripts/build-item-extended-cost-hotfix.py --build 3.4.3.54261
    python scripts/build-item-extended-cost-hotfix.py --build 4.4.2.60895 --dbc-dir "D:/wow335a/dbc"
    python scripts/build-item-extended-cost-hotfix.py --build 4.4.2.60895 --cata-db2 "D:/wow434/ItemExtendedCost.db2"

The 3.3.5a DBC directory is found as in diff-dbc-vs-db2.py: --dbc-dir, else the
HERMES_TOOLS_DBC_335A environment variable. The 4.3.4 file is the one a 4.3.4 server reads from
its dbc directory (WDB2, build 15595). DB2 downloads are cached under .cache/db2/<build>/.
"""

from __future__ import annotations

import argparse
import csv
import os
import struct
import sys
import urllib.request
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
CSV_DIR = REPO_ROOT / "HermesProxy" / "CSV"
CACHE_DIR = REPO_ROOT / ".cache" / "db2"
WAGO = "https://wago.tools/db2/{table}/csv?build={build}"
DBC_ENV = "HERMES_TOOLS_DBC_335A"

HONOR = 1901
ARENA_POINTS = 1900
HONOR_CATA = 392  # 4.3.4's Honor Points, "Honor Deprecated 3" to the 4.4.2 client

# 3.3.5a ItemExtendedCost.dbc, 16 fields of 4 bytes.
LEGACY_FIELDS = (["ID", "HonorPoints", "ArenaPoints", "ArenaBracket"]
                 + [f"ItemID_{i}" for i in range(5)] + [f"ItemCount_{i}" for i in range(5)]
                 + ["RequiredArenaRating", "ItemPurchaseGroup"])

# The order GameData.LoadItemExtendedCostHotfixes reads, which is the hotfix row's.
HEADER = (["ID", "RequiredArenaRating", "ArenaBracket", "Flags", "MinFactionID", "MinReputation",
           "RequiredAchievement"]
          + [f"ItemID_{i}" for i in range(5)] + [f"ItemCount_{i}" for i in range(5)]
          + [f"CurrencyID_{i}" for i in range(5)] + [f"CurrencyCount_{i}" for i in range(5)])


def resolve_dbc_dir(explicit: str | None) -> Path:
    candidate = explicit or os.environ.get(DBC_ENV)
    if not candidate:
        sys.exit(f"No 3.3.5a DBC directory. Pass --dbc-dir, or set {DBC_ENV}.")
    path = Path(candidate)
    if not path.is_dir():
        sys.exit(f"{DBC_ENV}/--dbc-dir points at '{path}', which is not a directory")
    return path


def read_legacy(path: Path) -> dict[int, dict[str, int]]:
    data = path.read_bytes()
    if data[:4] != b"WDBC":
        raise ValueError(f"{path.name}: not a WDBC file (magic {data[:4]!r})")
    count, fields, size, _ = struct.unpack_from("<4I", data, 4)
    if fields != len(LEGACY_FIELDS):
        raise ValueError(f"{path.name}: {fields} fields, expected {len(LEGACY_FIELDS)} (3.3.5a)")
    rows = {}
    for index in range(count):
        values = struct.unpack_from(f"<{fields}i", data, 20 + index * size)
        rows[values[0]] = dict(zip(LEGACY_FIELDS, values))
    return rows


def read_cata(path: Path) -> dict[int, tuple[int, ...]]:
    """4.3.4's ItemExtendedCost.db2 (WDB2), 31 fields of 4 bytes: TrinityCore 4.3.4 ItemExtendedCostEntry."""
    data = path.read_bytes()
    if data[:4] != b"WDB2":
        raise ValueError(f"{path.name}: not a WDB2 file (magic {data[:4]!r})")
    count, fields, size, _, _, build, _, min_id, max_id = struct.unpack_from("<9I", data, 4)
    if fields != 31 or build != 15595:
        raise ValueError(f"{path.name}: {fields} fields of build {build}, expected 31 of 15595")
    offset = 48 + ((max_id - min_id + 1) * 6 if max_id else 0)
    rows = {}
    for index in range(count):
        values = struct.unpack_from(f"<{fields}i", data, offset + index * size)
        rows[values[0]] = values
    return rows


def cata_row(row: tuple[int, ...]) -> list[int]:
    """A 4.3.4 row in HEADER's order. Its honor and arena point fields (1, 2) are unused there."""
    items = [(row[4 + i], row[9 + i]) for i in range(5) if row[4 + i]]
    currencies = [(HONOR if row[16 + i] == HONOR_CATA else row[16 + i], row[21 + i])
                  for i in range(5) if row[16 + i]]
    # RequiredPersonalArenaRating, RequiredArenaSlot, RequirementFlags, RequiredFactionId,
    # RequiredFactionStanding, RequiredAchievement
    return padded(row[0], row[14], row[3], row[28], row[26], row[27], row[30], items, currencies)


def padded(row_id, rating, bracket, flags, faction, reputation, achievement, items, currencies) -> list[int]:
    if len(items) > 5 or len(currencies) > 5:
        raise ValueError(f"row {row_id}: more than five items or currencies")
    items = items + [(0, 0)] * (5 - len(items))
    currencies = currencies + [(0, 0)] * (5 - len(currencies))
    return ([row_id, rating, bracket, flags, faction, reputation, achievement]
            + [i for i, _ in items] + [c for _, c in items]
            + [i for i, _ in currencies] + [c for _, c in currencies])


def same(target: list[int], client: dict[str, str]) -> bool:
    """Whether the client's row asks for what the target row does, slot order aside."""
    def pairs(values, ids, counts):
        return sorted((values[ids + i], values[counts + i]) for i in range(5) if values[ids + i])
    theirs = [int(client[c]) for c in HEADER]
    return (target[1:7] == theirs[1:7]
            and pairs(target, 7, 12) == pairs(theirs, 7, 12)
            and pairs(target, 17, 22) == pairs(theirs, 17, 22))


def fetch_client_csv(table: str, build: str) -> Path:
    cached = CACHE_DIR / build / f"{table}.csv"
    if cached.exists() and cached.stat().st_size > 0:
        return cached
    cached.parent.mkdir(parents=True, exist_ok=True)
    request = urllib.request.Request(WAGO.format(table=table, build=build),
                                     headers={"User-Agent": "HermesProxy-hotfix-audit/1.0"})
    with urllib.request.urlopen(request, timeout=300) as response:
        cached.write_bytes(response.read())
    return cached


def read_csv(path: Path) -> list[dict[str, str]]:
    with path.open(encoding="utf-8-sig", newline="") as handle:
        return list(csv.DictReader(handle))


def legacy_row(row: dict[str, int], tokens: dict[int, int], honor_scale: int) -> list[int]:
    """A 3.3.5a row in HEADER's order; 3.3.5a has no flags, faction or achievement columns."""
    items, currencies = [], []
    if row["HonorPoints"]:
        currencies.append((HONOR, row["HonorPoints"] * honor_scale))
    if row["ArenaPoints"]:
        currencies.append((ARENA_POINTS, row["ArenaPoints"]))
    for i in range(5):
        item, count = row[f"ItemID_{i}"], row[f"ItemCount_{i}"]
        if not item:
            continue
        if item in tokens:
            currencies.append((tokens[item], count))
        else:
            items.append((item, count))
    return padded(row["ID"], row["RequiredArenaRating"], row["ArenaBracket"], 0, 0, 0, 0, items, currencies)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--build", required=True, help="client build, 3.4.3.54261 or 4.4.2.60895")
    parser.add_argument("--dbc-dir", help=f"3.3.5a DBC directory (default: ${DBC_ENV})")
    parser.add_argument("--cata-db2", help="a 4.3.4 server's ItemExtendedCost.db2, for ItemExtendedCostCata4.csv")
    args = parser.parse_args()

    expansion = int(args.build.split(".")[0])
    if expansion not in (3, 4) or (args.cata_db2 and expansion != 4):
        sys.exit("--build must be a 3.4.3 or a 4.4.2 build, and a 4.4.2 one with --cata-db2")
    client = {int(r["ID"]): r for r in read_csv(fetch_client_csv("ItemExtendedCost", args.build))}

    if args.cata_db2:
        server = {row_id: cata_row(row) for row_id, row in read_cata(Path(args.cata_db2)).items()}
        target = CSV_DIR / "Hotfix" / "ItemExtendedCostCata4.csv"
    else:
        tokens = {int(r["ItemId"]): int(r["CurrencyId"])
                  for r in read_csv(CSV_DIR / f"CurrencyTypes{expansion}.csv") if r["ItemId"] != "0"}
        honor_scale = 100 if expansion >= 4 else 1
        legacy = read_legacy(resolve_dbc_dir(args.dbc_dir) / "ItemExtendedCost.dbc")
        server = {row_id: legacy_row(row, tokens, honor_scale) for row_id, row in legacy.items()}
        target = CSV_DIR / "Hotfix" / f"ItemExtendedCost{expansion}.csv"

    out = [row for row_id, row in sorted(server.items())
           if row_id != 0 and not (row_id in client and same(row, client[row_id]))]
    with target.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle, lineterminator="\n")
        writer.writerow(HEADER)
        writer.writerows(out)
    missing = sum(1 for r in out if r[0] not in client)
    print(f"{target.relative_to(REPO_ROOT)}: {len(out)} rows ({missing} the client lacks) of {len(server) - (0 in server)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
