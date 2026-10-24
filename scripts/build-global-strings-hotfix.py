#!/usr/bin/env python3
"""Build CSV/Hotfix/GlobalStrings4.csv: the honor chat lines of the 4.4.2 client, for a 3.3.5a server.

The 4.4.2 client prints an honor gain in one of two ways, by the expansion level it was told
(rva 1e15190). At Cataclysm's it turns the amount into points (a float, a hundredth of what it was
sent) and formats it with its own strings, which take a "%.2f". Below that, which is what the
proxy announces in front of a 3.3.5a server, it passes the amount on as a whole number to the same
strings, as the Wrath client did with its "%d" ones (rva 1e15220). A whole number read for "%.2f"
prints 0.00: "You have been awarded 0.00 honor points." whatever the server gave.

This writes the three strings involved (COMBATLOG_HONORAWARD, COMBATLOG_HONORGAIN and
COMBATLOG_HONORGAIN_NO_RANK) in each of the client's locales, as the client has them with "%.2f"
made "%d". The proxy sends them as GlobalStrings hotfixes to a 4.4.2 client in front of a 3.3.5a
server, in the client's locale.

Usage
-----
    python scripts/build-global-strings-hotfix.py --build 4.4.2.60895

DB2 downloads are cached under .cache/db2/<build>/<locale>/.
"""

from __future__ import annotations

import argparse
import csv
import sys
import urllib.request
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
TARGET = REPO_ROOT / "HermesProxy" / "CSV" / "Hotfix" / "GlobalStrings4.csv"
CACHE_DIR = REPO_ROOT / ".cache" / "db2"
WAGO = "https://wago.tools/db2/{table}/csv?build={build}&locale={locale}"

TAGS = ("COMBATLOG_HONORAWARD", "COMBATLOG_HONORGAIN", "COMBATLOG_HONORGAIN_NO_RANK")
LOCALES = ("enUS", "deDE", "frFR", "esES", "esMX", "ruRU", "koKR", "zhCN", "zhTW", "ptBR", "itIT")


def fetch_client_csv(table: str, build: str, locale: str) -> Path:
    cached = CACHE_DIR / build / locale / f"{table}.csv"
    if cached.exists() and cached.stat().st_size > 0:
        return cached
    cached.parent.mkdir(parents=True, exist_ok=True)
    request = urllib.request.Request(WAGO.format(table=table, build=build, locale=locale),
                                     headers={"User-Agent": "HermesProxy-hotfix-audit/1.0"})
    with urllib.request.urlopen(request, timeout=300) as response:
        cached.write_bytes(response.read())
    return cached


def read_tags(path: Path) -> dict[str, dict[str, str]]:
    with path.open(encoding="utf-8-sig", newline="") as handle:
        return {row["BaseTag"]: row for row in csv.DictReader(handle) if row["BaseTag"] in TAGS}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--build", required=True, help="client build, 4.4.2.60895")
    args = parser.parse_args()
    if not args.build.startswith("4."):
        sys.exit("--build must be a 4.4.2 build")

    by_locale = {locale: read_tags(fetch_client_csv("GlobalStrings", args.build, locale)) for locale in LOCALES}
    out = []
    for tag in TAGS:
        base = by_locale["enUS"][tag]
        texts = []
        for locale in LOCALES:
            row = by_locale[locale][tag]
            if row["ID"] != base["ID"] or row["TagText_lang"].count("%.2f") != 1:
                sys.exit(f"{tag} in {locale}: id {row['ID']}, text {row['TagText_lang']!r}")
            texts.append(row["TagText_lang"].replace("%.2f", "%d"))
        out.append([base["ID"], tag, base["Flags"], *texts])

    with TARGET.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle, lineterminator="\n")
        writer.writerow(["ID", "BaseTag", "Flags", *LOCALES])
        writer.writerows(sorted(out, key=lambda r: int(r[0])))
    print(f"{TARGET.relative_to(REPO_ROOT)}: {len(out)} strings in {len(LOCALES)} locales")
    return 0


if __name__ == "__main__":
    sys.exit(main())
