#!/usr/bin/env python3
"""Build CSV/Hotfix/CurrencyTypesCata4.csv: a 4.3.4 server's currency caps for the 4.4.2 client.

Cataclysm Classic gave Conquest Points and Valor Points no weekly cap: its CurrencyTypes rows have
no MaxEarnablePerWeek and no ComputedWeeklyMaximum flag, and count a season's total instead
(UseTotalEarnedForEarned, and a maximum that grows with the weeks of the season). 4.3.4 has weekly
caps: Valor Points earn at most 1000 a week (MaxEarnablePerWeek), and Conquest Points as many as the
server works out from the player's rating (ComputedWeeklyMaximum), which a 4.3.4 server sends in
SMSG_SETUP_CURRENCY. The 4.4.2 client takes a weekly cap from its own row alone, so its tooltip
showed "Season Total Earned" and no cap.

This writes the client's rows of the currencies whose weekly cap or season total 4.3.4 has
otherwise, in the client's own layout (TrinityCore cata_classic CurrencyTypesLoadInfo), with
4.3.4's caps:

    - MaxQty and MaxEarnablePerWeek are 4.3.4's;
    - of the flags, ComputedWeeklyMaximum and DynamicMaximum are 4.3.4's, and the two that scale
      the maximum with the season are cleared: 4.3.4 has neither;
    - UseTotalEarnedForEarned is set where 4.3.4 tracks the quantity (TrackQuantity) and cleared
      elsewhere. The 4.3.4 tooltip shows the tracked amount, which a 4.3.4 server sends as the
      season's total, as "Season Total Earned" when there is one. The 4.4.2 tooltip shows the total
      earned under that name with the flag, and the quantity under it without;
    - everything else, the names and the other flags among them, is the client's.

The proxy sends them as hotfixes in front of a 4.3.4 server. 4.3.4's Honor Points (392) are left
out: the 4.4.2 client counts honor in its own 1901.

Usage
-----
    python scripts/build-currency-types-hotfix.py --build 4.4.2.60895 --cata-dbc "D:/wow434/CurrencyTypes.dbc"

The 4.3.4 file is the one a 4.3.4 server reads from its dbc directory (WDBC, 11 fields). DB2
downloads are cached under .cache/db2/<build>/.
"""

from __future__ import annotations

import argparse
import csv
import struct
import sys
import urllib.request
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
TARGET = REPO_ROOT / "HermesProxy" / "CSV" / "Hotfix" / "CurrencyTypesCata4.csv"
CACHE_DIR = REPO_ROOT / ".cache" / "db2"
WAGO = "https://wago.tools/db2/{table}/csv?build={build}"

HONOR_CATA = 392

COMPUTED_WEEKLY_MAXIMUM = 0x4
TRACK_QUANTITY = 0x80
DYNAMIC_MAXIMUM = 0x40000
CAP_FLAGS = COMPUTED_WEEKLY_MAXIMUM | DYNAMIC_MAXIMUM
# CurrencyTypesFlagsB: UseTotalEarnedForEarned, ScaleMaxQuantityBySeasonWeeks,
# ScaleMaxQuantityByWeeksSinceStart.
USE_TOTAL_EARNED = 0x01
SEASON_FLAGS = USE_TOTAL_EARNED | 0x40 | 0x80

# The order GameData.LoadCurrencyTypesHotfixes reads, which is the hotfix row's after the ID.
HEADER = ["ID", "Name_lang", "Description_lang", "CategoryID", "InventoryIconFileID", "SpellWeight",
          "SpellCategory", "MaxQty", "MaxEarnablePerWeek", "Quality", "FactionID", "AwardConditionID",
          "MaxQtyWorldStateID", "Unknown1154", "Flags_0", "Flags_1"]
# wago's names for the three columns TrinityCore cata_classic has names for.
WAGO_NAMES = {"AwardConditionID": "Field_1_15_3_55112_010", "MaxQtyWorldStateID": "Field_1_15_3_55112_011",
              "Unknown1154": "Field_1_15_4_56400_012"}


def read_cata(path: Path) -> dict[int, tuple[int, int, int]]:
    """4.3.4's CurrencyTypes.dbc (TrinityCore 4.3.4 CurrencyTypesEntry): MaxQty, MaxEarnablePerWeek, Flags."""
    data = path.read_bytes()
    if data[:4] != b"WDBC":
        raise ValueError(f"{path.name}: not a WDBC file (magic {data[:4]!r})")
    count, fields, size, _ = struct.unpack_from("<4I", data, 4)
    if fields != 11:
        raise ValueError(f"{path.name}: {fields} fields, expected 11 (4.3.4)")
    rows = {}
    for index in range(count):
        values = struct.unpack_from(f"<{fields}I", data, 20 + index * size)
        rows[values[0]] = (values[7], values[8], values[9])
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


def weekly(max_weekly: int, flags: int, flags_b: int) -> tuple[int, int, int]:
    return max_weekly, flags & CAP_FLAGS, flags_b & SEASON_FLAGS


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--build", required=True, help="client build, 4.4.2.60895")
    parser.add_argument("--cata-dbc", required=True, help="a 4.3.4 server's CurrencyTypes.dbc")
    args = parser.parse_args()
    if not args.build.startswith("4."):
        sys.exit("--build must be a 4.4.2 build")

    server = read_cata(Path(args.cata_dbc))
    out = []
    for row in read_csv(fetch_client_csv("CurrencyTypes", args.build)):
        currency = int(row["ID"])
        if currency not in server or currency == HONOR_CATA:
            continue
        flags, flags_b = int(row["Flags_0"]), int(row["Flags_1"])
        max_qty, max_weekly, server_flags = server[currency]
        server_flags_b = USE_TOTAL_EARNED if server_flags & TRACK_QUANTITY else 0
        if weekly(int(row["MaxEarnablePerWeek"]), flags, flags_b) == weekly(max_weekly, server_flags, server_flags_b):
            continue
        hotfix = {name: row[WAGO_NAMES.get(name, name)] for name in HEADER}
        hotfix["MaxQty"] = max_qty
        hotfix["MaxEarnablePerWeek"] = max_weekly
        hotfix["Flags_0"] = (flags & ~CAP_FLAGS) | (server_flags & CAP_FLAGS)
        hotfix["Flags_1"] = (flags_b & ~SEASON_FLAGS) | server_flags_b
        out.append([hotfix[name] for name in HEADER])

    with TARGET.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle, lineterminator="\n")
        writer.writerow(HEADER)
        writer.writerows(sorted(out, key=lambda r: int(r[0])))
    print(f"{TARGET.relative_to(REPO_ROOT)}: {len(out)} rows: {', '.join(str(r[0]) for r in out)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
