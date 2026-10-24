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

Usage
-----
    python scripts/build-item-extended-cost-hotfix.py --build 3.4.3.54261
    python scripts/build-item-extended-cost-hotfix.py --build 4.4.2.60895 --dbc-dir "D:/wow335a/dbc"

The 3.3.5a DBC directory is found as in diff-dbc-vs-db2.py: --dbc-dir, else the
HERMES_TOOLS_DBC_335A environment variable. DB2 downloads are cached under .cache/db2/<build>/.
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


def cost(rating: int, bracket: int, items, currencies):
    return rating, bracket, tuple(sorted(items)), tuple(sorted(currencies))


def legacy_cost(row: dict[str, int], tokens: dict[int, int], honor_scale: int):
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
    return items, currencies


def client_cost(row: dict[str, str]):
    items = [(int(row[f"ItemID_{i}"]), int(row[f"ItemCount_{i}"]))
             for i in range(5) if row[f"ItemID_{i}"] not in ("", "0")]
    currencies = [(int(row[f"CurrencyID_{i}"]), int(row[f"CurrencyCount_{i}"]))
                  for i in range(5) if row[f"CurrencyID_{i}"] not in ("", "0")]
    return cost(int(row["RequiredArenaRating"]), int(row["ArenaBracket"]), items, currencies)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--build", required=True, help="client build, 3.4.3.54261 or 4.4.2.60895")
    parser.add_argument("--dbc-dir", help=f"3.3.5a DBC directory (default: ${DBC_ENV})")
    args = parser.parse_args()

    expansion = int(args.build.split(".")[0])
    if expansion not in (3, 4):
        sys.exit("--build must be a 3.4.3 or a 4.4.2 build")
    honor_scale = 100 if expansion >= 4 else 1

    legacy = read_legacy(resolve_dbc_dir(args.dbc_dir) / "ItemExtendedCost.dbc")
    client = {int(r["ID"]): r for r in read_csv(fetch_client_csv("ItemExtendedCost", args.build))}
    tokens = {int(r["ItemId"]): int(r["CurrencyId"])
              for r in read_csv(CSV_DIR / f"CurrencyTypes{expansion}.csv") if r["ItemId"] != "0"}

    out = []
    for row_id in sorted(legacy):
        if row_id == 0:
            continue
        row = legacy[row_id]
        items, currencies = legacy_cost(row, tokens, honor_scale)
        if len(items) > 5 or len(currencies) > 5:
            raise ValueError(f"row {row_id}: more than five items or currencies")
        wanted = cost(row["RequiredArenaRating"], row["ArenaBracket"], items, currencies)
        if row_id in client and client_cost(client[row_id]) == wanted:
            continue
        items += [(0, 0)] * (5 - len(items))
        currencies += [(0, 0)] * (5 - len(currencies))
        out.append([row_id, row["RequiredArenaRating"], row["ArenaBracket"], 0, 0, 0, 0]
                   + [i for i, _ in items] + [c for _, c in items]
                   + [i for i, _ in currencies] + [c for _, c in currencies])

    target = CSV_DIR / "Hotfix" / f"ItemExtendedCost{expansion}.csv"
    with target.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle, lineterminator="\n")
        writer.writerow(HEADER)
        writer.writerows(out)
    missing = sum(1 for r in out if r[0] not in client)
    print(f"{target.relative_to(REPO_ROOT)}: {len(out)} rows ({missing} the client lacks) of {len(legacy) - 1}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
