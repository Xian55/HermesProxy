#!/usr/bin/env python3
"""Regenerate the static CSVs under HermesProxy/CSV from client DB2 data.

Companion to compare-hotfix-csv.py (mirror auditor) and diff-dbc-vs-db2.py
(polyfill finder). Those two *inspect*; this one *produces*.

Why this exists
---------------
Commit 9e8c1378 bootstrapped the WotLK `*3.csv` set by copying the TBC `*2.csv`
files verbatim, as a Phase-1 "load without crashing" measure. 19 of them were
still byte-identical to their TBC sibling, which meant the proxy was modelling
the 3.4.3 client's baked-in data with 2.5.3 data -- e.g. AuraSpells3 was missing
10,896 of the client's 25,899 aura spells, and TaxiNodes3 had none of the 122
Northrend flight nodes.

Every recipe below was derived by aligning the committed file against the wago
export it originally came from, so a regeneration is a row/value diff, never a
schema change.

Usage
-----
    python scripts/build-csv-from-db2.py --all 3 --build 3.4.3.54261
    python scripts/build-csv-from-db2.py Item3.csv --build 3.4.3.54261
    python scripts/build-csv-from-db2.py --audit --all 3

`--audit` builds each file in memory and reports how it differs from what is
committed, writing nothing.

Downloads are cached under .cache/db2/<build>/, shared with the other two
scripts, so repeat runs are offline.

Column order is load-bearing
----------------------------
Every consumer in World/GameData.cs parses these files *by column index*, not by
header name (`row[0].Span`, `row[1].Span`, ...). wotlk.md names column-order
drift as the common failure mode of a regeneration. The recipes therefore pin an
explicit ordered column list; a source column that has been renamed or removed
upstream raises instead of silently shifting every field one to the left.

Number formatting
-----------------
DB2 floats arrive at full float64 print width (`0.60000002384`) but the client
stores float32 and the committed CSVs carry the shortest form that round-trips
(`0.6`). `format_number` reproduces that, and collapses integral floats to bare
integers, matching the existing files. Everything the proxy parses goes through
`float.Parse` under InvariantCulture (Program.cs:49), so a decimal point is safe.

Builds
------
The suffix on a CSV is the expansion version, not the build:

    *1.csv -> V1_14  (Classic Era)  e.g. --build 1.14.2.42597
    *2.csv -> V2_5   (TBC Classic)  e.g. --build 2.5.3.41750
    *3.csv -> V3_4_3 (WotLK)        e.g. --build 3.4.3.54261
"""

from __future__ import annotations

import argparse
import csv
import io
import struct
import sys
import urllib.request
from dataclasses import dataclass, field
from pathlib import Path
from typing import Callable, Iterable, Sequence

REPO_ROOT = Path(__file__).resolve().parent.parent
CSV_DIR = REPO_ROOT / "HermesProxy" / "CSV"
HOTFIX_DIR = CSV_DIR / "Hotfix"
CACHE_DIR = REPO_ROOT / ".cache" / "db2"
WAGO = "https://wago.tools/db2/{table}/csv?build={build}"

# SpellEffect.Effect values that apply an aura. Mirrors the SPELL_EFFECT_APPLY_*
# family used by the client; AuraSpells is only ever asked "does this spell apply
# an aura at all" (SpellHandler.cs:626).
AURA_EFFECTS = {6, 27, 35, 65, 119, 128, 129, 143}

# csv.field_size_limit default is 128 KB; ItemSparse descriptions stay well under
# that, but raise it so a long localized string can never truncate a run.
csv.field_size_limit(1 << 22)


# The integer type each loader in World/GameData.cs parses a column with, where it
# is narrower than int. wago prints DB2 columns in their own signedness and width,
# which does not always match: ItemSparse.Flags_0 arrives as -2147483648 for a
# column the loader reads with uint.Parse, and ItemEffect.TriggerType arrives as
# 255 for an sbyte. Either one is an OverflowException that takes the host down
# from inside GameData's Parallel.Invoke, so every value is re-encoded into the
# type its consumer will parse. Only narrow columns are listed; int/long/float
# columns need no conversion.
#
# Regenerate this table when a loader's field types change:
#     grep -n 'Parse(row\[' HermesProxy/World/GameData.cs
LOADER_TYPES: dict[str, dict[str, str]] = {
    "ItemSparse3.csv": {
        "DurationInInventory": "uint", "BagFamily": "uint", "RequiredAbility": "uint",
        "SellPrice": "uint", "BuyPrice": "uint", "VendorStackCount": "uint", "Flags1": "uint",
        "Flags2": "uint", "Flags3": "uint", "Flags4": "uint", "MaxDurability": "uint",
        "ItemNameDescriptionId": "ushort", "RequiredTransmogHoliday": "ushort",
        "RequiredHoliday": "ushort", "LimitCategory": "ushort", "GemProperties": "ushort",
        "SocketMatchEnchantmentId": "ushort", "TotemCategoryId": "ushort",
        "InstanceBound": "ushort", "ZoneBound1": "ushort", "ZoneBound2": "ushort",
        "ItemSet": "ushort", "LockId": "ushort", "StartQuestId": "ushort", "PageText": "ushort",
        "Delay": "ushort", "RequiredReputationId": "ushort", "RequiredSkillRank": "ushort",
        "RequiredSkill": "ushort", "ItemLevel": "ushort", "AllowableClass": "short",
        "ItemRandomSuffixGroupId": "ushort", "RandomProperty": "ushort", "MinDamage1": "ushort",
        "MinDamage2": "ushort", "MinDamage3": "ushort", "MinDamage4": "ushort",
        "MinDamage5": "ushort", "MaxDamage1": "ushort", "MaxDamage2": "ushort",
        "MaxDamage3": "ushort", "MaxDamage4": "ushort", "MaxDamage5": "ushort",
        "Resistances1": "short", "Resistances2": "short", "Resistances3": "short",
        "Resistances4": "short", "Resistances5": "short", "Resistances6": "short",
        "Resistances7": "short", "ScalingStatDistributionId": "ushort", "ExpansionId": "byte",
        "ArtifactId": "byte", "SpellWeight": "byte", "SpellWeightCategory": "byte",
        "SocketType1": "byte", "SocketType2": "byte", "SocketType3": "byte", "SheatheType": "byte",
        "Material": "byte", "PageMaterial": "byte", "PageLanguage": "byte", "Bonding": "byte",
        "DamageType": "byte", "StatType1": "sbyte", "StatType2": "sbyte", "StatType3": "sbyte",
        "StatType4": "sbyte", "StatType5": "sbyte", "StatType6": "sbyte", "StatType7": "sbyte",
        "StatType8": "sbyte", "StatType9": "sbyte", "StatType10": "sbyte",
        "ContainerSlots": "byte", "RequiredReputationRank": "byte", "RequiredCityRank": "byte",
        "RequiredHonorRank": "byte", "InventoryType": "byte", "OverallQualityId": "byte",
        "AmmoType": "byte",
        # short, not sbyte: WotLK raid gear exceeds a signed byte (item 51219 is
        # +162 Str / +219 Sta) and both the store and the V3_4_3 wire field are 16-bit.
        "StatValue1": "short", "StatValue2": "short", "StatValue3": "short",
        "StatValue4": "short", "StatValue5": "short", "StatValue6": "short", "StatValue7": "short",
        "StatValue8": "short", "StatValue9": "short", "StatValue10": "short",
        "RequiredLevel": "sbyte",
    },
    "ItemEffect3.csv": {
        "LegacySlotIndex": "byte", "TriggerType": "sbyte", "Charges": "short",
        "SpellCategoryID": "ushort", "ChrSpecializationID": "ushort",
    },
    "Item3.csv": {
        "ClassId": "byte", "SubclassId": "byte", "Material": "byte", "InventoryType": "sbyte",
        "SheatheType": "byte", "RandomProperty": "ushort", "ItemRandomSuffixGroupId": "ushort",
        "SoundOverrideSubclassId": "sbyte", "ScalingStatDistributionId": "ushort",
        "ItemGroupSoundsId": "byte", "MaxDurability": "uint", "AmmoType": "byte",
        "DamageType1": "byte", "DamageType2": "byte", "DamageType3": "byte", "DamageType4": "byte",
        "DamageType5": "byte", "Resistances1": "short", "Resistances2": "short",
        "Resistances3": "short", "Resistances4": "short", "Resistances5": "short",
        "Resistances6": "short", "Resistances7": "short", "MinDamage1": "ushort",
        "MinDamage2": "ushort", "MinDamage3": "ushort", "MinDamage4": "ushort",
        "MinDamage5": "ushort", "MaxDamage1": "ushort", "MaxDamage2": "ushort",
        "MaxDamage3": "ushort", "MaxDamage4": "ushort", "MaxDamage5": "ushort",
    },
    "ItemAppearance3.csv": {"DisplayType": "byte"},
}

# The TBC file uses the same loader field types as the WotLK one.
LOADER_TYPES["ItemSparse2.csv"] = LOADER_TYPES["ItemSparse3.csv"]

INT_WIDTHS = {"sbyte": (8, True), "byte": (8, False), "short": (16, True), "ushort": (16, False),
              "uint": (32, False), "ulong": (64, False)}


def coerce_to_type(value: str, cs_type: str) -> str:
    """Re-encode an integer into the C# type the loader parses it with.

    Same bits, different reading -- the signedness trap compare-hotfix-csv.py
    documents. Values genuinely wider than the field (ItemSparse StatValue holds
    2500 in a column the loader reads as sbyte) wrap, which is exactly what the
    consumer already does: GenerateItemSparseUpdateIfNeeded compares
    `row.StatValue[i] != (sbyte)item.StatValues[i]`, casting the legacy side to
    sbyte too, so both sides truncate identically and the comparison still holds.
    """
    try:
        number = int(value)
    except ValueError:
        return value
    bits, signed = INT_WIDTHS[cs_type]
    wrapped = number & ((1 << bits) - 1)
    if signed and wrapped >= 1 << (bits - 1):
        wrapped -= 1 << bits
    return str(wrapped)


# --------------------------------------------------------------------------- io


def read_csv(path: Path) -> list[dict[str, str]]:
    with path.open(encoding="utf-8-sig", newline="") as handle:
        return list(csv.DictReader(handle))


def fetch_client_csv(table: str, build: str) -> Path:
    """Download a DB2 export from wago.tools, cached per build."""
    cached = CACHE_DIR / build / f"{table}.csv"
    if cached.exists() and cached.stat().st_size > 0:
        return cached
    cached.parent.mkdir(parents=True, exist_ok=True)
    url = WAGO.format(table=table, build=build)
    # wago.tools answers 403 to urllib's default User-Agent.
    request = urllib.request.Request(url, headers={"User-Agent": "HermesProxy-hotfix-audit/1.0"})
    with urllib.request.urlopen(request, timeout=300) as response:
        cached.write_bytes(response.read())
    if cached.stat().st_size == 0:
        cached.unlink(missing_ok=True)
        raise RuntimeError(f"wago.tools returned nothing for {table} @ {build}")
    return cached


def format_number(value: str | None) -> str:
    """Shortest float32 round-trip, integers untouched.

    wago prints DB2 floats at float64 width; the client and the committed CSVs
    use float32. Emitting the full width would bloat the files and imply
    precision the client does not have.
    """
    text = (value or "").strip()
    if not text:
        return text
    try:
        int(text)
        return text
    except ValueError:
        pass
    try:
        parsed = float(text)
    except ValueError:
        return text  # a real string column (names, descriptions)

    single = struct.unpack("f", struct.pack("f", parsed))[0]
    shortest = repr(single)
    for precision in range(1, 10):
        candidate = f"{single:.{precision}g}"
        if struct.unpack("f", struct.pack("f", float(candidate)))[0] == single:
            shortest = candidate
            break
    as_float = float(shortest)
    return str(int(as_float)) if as_float == int(as_float) else shortest


# ---------------------------------------------------------------------- recipes


Column = str | tuple[str, str]


@dataclass
class Recipe:
    """How one CSV under HermesProxy/CSV is built.

    source   wago DB2 table name, or None when `builder` supplies the rows.
    local    path relative to HermesProxy/CSV, used instead of a download.
    columns  ordered (our header, source column); a bare str means both match.
    where    row predicate, applied to the source row.
    dedupe   our-header whose duplicates collapse, last row winning.
    builder  full override, receives the resolved build and returns rows.
    """

    source: str | None = None
    local: str | None = None
    columns: Sequence[Column] = field(default_factory=list)
    quote_all: bool = False
    where: Callable[[dict[str, str]], bool] | None = None
    dedupe: str | None = None
    builder: Callable[[str], tuple[list[str], list[list[str]]]] | None = None
    note: str = ""

    def header(self) -> list[str]:
        return [c[0] if isinstance(c, tuple) else c for c in self.columns]

    def source_columns(self) -> list[str]:
        return [c[1] if isinstance(c, tuple) else c for c in self.columns]


ITEM_COLUMNS: list[Column] = [
    ("Id", "ID"), ("ClassId", "ClassID"), ("SubclassId", "SubclassID"), "Material", "InventoryType",
    "RequiredLevel", "SheatheType", ("RandomProperty", "RandomSelect"),
    ("ItemRandomSuffixGroupId", "ItemRandomSuffixGroupID"),
    ("SoundOverrideSubclassId", "Sound_override_subclassID"),
    ("ScalingStatDistributionId", "ScalingStatDistributionID"), ("IconFileDataId", "IconFileDataID"),
    ("ItemGroupSoundsId", "ItemGroupSoundsID"), ("ContentTuningId", "ContentTuningID"), "MaxDurability",
    ("AmmoType", "AmmunitionType"), ("DamageType1", "DamageType_0"), ("DamageType2", "DamageType_1"),
    ("DamageType3", "DamageType_2"), ("DamageType4", "DamageType_3"), ("DamageType5", "DamageType_4"),
    ("Resistances1", "Resistances_0"), ("Resistances2", "Resistances_1"), ("Resistances3", "Resistances_2"),
    ("Resistances4", "Resistances_3"), ("Resistances5", "Resistances_4"), ("Resistances6", "Resistances_5"),
    ("Resistances7", "Resistances_6"), ("MinDamage1", "MinDamage_0"), ("MinDamage2", "MinDamage_1"),
    ("MinDamage3", "MinDamage_2"), ("MinDamage4", "MinDamage_3"), ("MinDamage5", "MinDamage_4"),
    ("MaxDamage1", "MaxDamage_0"), ("MaxDamage2", "MaxDamage_1"), ("MaxDamage3", "MaxDamage_2"),
    ("MaxDamage4", "MaxDamage_3"), ("MaxDamage5", "MaxDamage_4"),
]

ITEM_SPARSE_COLUMNS: list[Column] = [
    ("Id", "ID"), "AllowableRace", ("Description", "Description_lang"), ("Name4", "Display3_lang"),
    ("Name3", "Display2_lang"), ("Name2", "Display1_lang"), ("Name1", "Display_lang"), "DmgVariance",
    "DurationInInventory", "QualityModifier", "BagFamily", ("RangeMod", "ItemRange"),
    *[(f"StatPercentageOfSocket{i + 1}", f"StatPercentageOfSocket_{i}") for i in range(10)],
    *[(f"StatPercentEditor{i + 1}", f"StatPercentEditor_{i}") for i in range(10)],
    "Stackable", "MaxCount", "RequiredAbility", "SellPrice", "BuyPrice", "VendorStackCount",
    "PriceVariance", "PriceRandomValue",
    *[(f"Flags{i + 1}", f"Flags_{i}") for i in range(4)],
    ("OppositeFactionItemId", "OppositeFactionItemID"), "MaxDurability",
    ("ItemNameDescriptionId", "ItemNameDescriptionID"), "RequiredTransmogHoliday", "RequiredHoliday",
    "LimitCategory", ("GemProperties", "Gem_properties"),
    ("SocketMatchEnchantmentId", "Socket_match_enchantment_ID"), ("TotemCategoryId", "TotemCategoryID"),
    "InstanceBound", ("ZoneBound1", "ZoneBound_0"), ("ZoneBound2", "ZoneBound_1"), "ItemSet",
    ("LockId", "LockID"), ("StartQuestId", "StartQuestID"), ("PageText", "PageID"), ("Delay", "ItemDelay"),
    ("RequiredReputationId", "MinFactionID"), "RequiredSkillRank", "RequiredSkill", "ItemLevel",
    "AllowableClass", ("ItemRandomSuffixGroupId", "ItemRandomSuffixGroupID"),
    ("RandomProperty", "RandomSelect"),
    *[(f"MinDamage{i + 1}", f"MinDamage_{i}") for i in range(5)],
    *[(f"MaxDamage{i + 1}", f"MaxDamage_{i}") for i in range(5)],
    *[(f"Resistances{i + 1}", f"Resistances_{i}") for i in range(7)],
    ("ScalingStatDistributionId", "ScalingStatDistributionID"), ("ExpansionId", "ExpansionID"),
    ("ArtifactId", "ArtifactID"), "SpellWeight", "SpellWeightCategory",
    *[(f"SocketType{i + 1}", f"SocketType_{i}") for i in range(3)],
    "SheatheType", "Material", ("PageMaterial", "PageMaterialID"), ("PageLanguage", "LanguageID"),
    "Bonding", "DamageType",
    *[(f"StatType{i + 1}", f"StatModifier_bonusStat_{i}") for i in range(10)],
    "ContainerSlots", ("RequiredReputationRank", "MinReputation"),
    ("RequiredCityRank", "RequiredPVPMedal"), ("RequiredHonorRank", "RequiredPVPRank"), "InventoryType",
    ("OverallQualityId", "OverallQualityID"), ("AmmoType", "AmmunitionType"),
    *[(f"StatValue{i + 1}", f"StatModifier_bonusAmount_{i}") for i in range(10)],
    "RequiredLevel",
]


def build_aura_spells(build: str) -> tuple[list[str], list[list[str]]]:
    """Every spell with an apply-aura effect, in SpellEffect order."""
    rows: list[list[str]] = []
    seen: set[str] = set()
    for row in read_csv(fetch_client_csv("SpellEffect", build)):
        if row.get("DifficultyID") != "0" or int(row["Effect"]) not in AURA_EFFECTS:
            continue
        spell_id = row["SpellID"]
        if spell_id in seen:
            continue
        seen.add(spell_id)
        rows.append([spell_id])
    return ["SpellId"], rows


def build_item_spells_data(build: str) -> tuple[list[str], list[list[str]]]:
    """Per-spell category and cooldowns, the reference GenerateItemEffectUpdateIfNeeded
    diffs the legacy item_template against.

    The *id set* is curated -- it is the spells reachable as an item's on-use
    effect -- and is kept as-is from the committed file; only the values are
    refreshed from the client. Widening it would change behaviour rather than fix
    it: an absent row means "no reference, leave the client's baked ItemEffect
    alone", whereas a present all-zero row asserts "the true cooldown is 0" and
    makes the proxy push an override.
    """
    existing = read_csv(CSV_DIR / "ItemSpellsData3.csv")
    categories = {
        r["SpellID"]: r["Category"]
        for r in read_csv(fetch_client_csv("SpellCategories", build))
        if r.get("DifficultyID") == "0"
    }
    cooldowns = {
        r["SpellID"]: (r["RecoveryTime"], r["CategoryRecoveryTime"])
        for r in read_csv(fetch_client_csv("SpellCooldowns", build))
        if r.get("DifficultyID") == "0"
    }
    rows = []
    for row in existing:
        spell_id = row["ID"]
        recovery, category_recovery = cooldowns.get(spell_id, ("0", "0"))
        rows.append([spell_id, categories.get(spell_id, "0"), recovery, category_recovery])
    return ["ID", "Category", "RecoveryTime", "CategoryRecoveryTime"], rows


def build_item_display_id_to_file_data_id(build: str) -> tuple[list[str], list[list[str]]]:
    """legacy DisplayID -> modern icon FileDataID. ADDITIVE ONLY.

    GetItemIconFileDataIdByDisplayId (GameData.cs:230) is called with a legacy
    ItemTemplate's DisplayID and returns 0 on a miss -- and that 0 is the early
    return that suppresses the Item, ItemSparse and ItemEffect hotfixes for the
    item entirely. So a missing key costs real behaviour, and a wrong value only
    costs an icon.

    Unlike every other recipe here, this file is not fully re-derivable: 14,445 of
    its 31,104 keys are display ids that no item in ItemIdToDisplayId3 references,
    so they came from a source we no longer have (most likely the legacy 3.3.5a
    ItemDisplayInfo.dbc). Replacing it wholesale would drop those and silently
    re-gate the hotfixes they unlock.

    This therefore keeps every committed row untouched and only adds keys that are
    currently missing, resolved by joining ItemIdToDisplayId3 (item -> legacy
    display id) against Item.db2 (item -> modern icon).
    """
    existing = read_csv(CSV_DIR / "ItemDisplayIdToFileDataId3.csv")
    mapping = {r["DisplayID"]: r["FileDataID"] for r in existing}

    display_by_item = {r["Entry"]: r["DisplayId"] for r in read_csv(CSV_DIR / "ItemIdToDisplayId3.csv")}
    icon_by_item = {r["ID"]: r["IconFileDataID"] for r in read_csv(fetch_client_csv("Item", build))}

    for item_id, display_id in display_by_item.items():
        if display_id in ("", "0") or display_id in mapping:
            continue
        icon = icon_by_item.get(item_id)
        if icon and icon != "0":
            mapping[display_id] = icon

    rows = [[display_id, icon] for display_id, icon in mapping.items()]
    rows.sort(key=lambda r: int(r[0]))
    return ["DisplayID", "FileDataID"], rows


def build_taxi_path(build: str) -> tuple[list[str], list[list[str]]]:
    """Flight paths, with dangling endpoints dropped.

    LoadTaxiPathNodesGraph resolves both endpoints with a raw indexer --
    `TaxiNodes[taxiPath.From]` (GameData.cs:2027-2028) -- so a path pointing at a
    node that TaxiNodes.db2 does not define is a KeyNotFoundException during
    startup's Parallel.Invoke, i.e. the host never comes up. TaxiPath.db2 at
    3.4.3.54261 has 12 such rows, most of them pointing at the placeholder node 0.
    A path we cannot resolve both ends of is not routable anyway.
    """
    nodes = {r["ID"] for r in read_csv(fetch_client_csv("TaxiNodes", build))}
    header = ["ID", "FromTaxiNode", "ToTaxiNode", "Cost"]
    rows = []
    for row in read_csv(fetch_client_csv("TaxiPath", build)):
        if row["FromTaxiNode"] not in nodes or row["ToTaxiNode"] not in nodes:
            continue
        rows.append([row[c] for c in header])
    return header, rows


RECIPES: dict[str, Recipe] = {
    "Item3.csv": Recipe(source="Item", columns=ITEM_COLUMNS),
    "ItemSparse3.csv": Recipe(source="ItemSparse", columns=ITEM_SPARSE_COLUMNS, quote_all=True),
    # Same table and projection, built with --build 2.5.3.41750. Present so the TBC
    # file can be regenerated too: it also carried sbyte-wrapped stat values.
    "ItemSparse2.csv": Recipe(source="ItemSparse", columns=ITEM_SPARSE_COLUMNS, quote_all=True),
    "ItemAppearance3.csv": Recipe(
        source="ItemAppearance",
        columns=["ID", "DisplayType", "ItemDisplayInfoID", "DefaultIconFileDataID", "UiOrder"],
    ),
    "ItemModifiedAppearance3.csv": Recipe(
        source="ItemModifiedAppearance",
        columns=["ID", "ItemID", "ItemAppearanceModifierID", "ItemAppearanceID", "OrderIndex",
                 "TransmogSourceTypeEnum"],
    ),
    "ItemEnchantVisuals3.csv": Recipe(
        source="SpellItemEnchantment",
        columns=[("EnchantId", "ID"), "ItemVisual"],
        where=lambda r: r["ItemVisual"] not in ("", "0"),
    ),
    "CreatureModelCollisionHeightsModern3.csv": Recipe(
        source="CreatureModelData",
        columns=[("ModelId", "ID"), "ModelScale", "CollisionHeight",
                 ("CollisionHeightMounted", "MountHeight")],
    ),
    "QuestV2_3.csv": Recipe(
        source="QuestV2",
        columns=["ID", "UniqueBitFlag"],
        # LoadQuestBits skips negative bits itself (GameData.cs:2107); drop them here too.
        where=lambda r: not r["UniqueBitFlag"].startswith("-"),
    ),
    "TaxiNodes3.csv": Recipe(
        source="TaxiNodes",
        columns=["ID", "ContinentID", ("X", "Pos_0"), ("Y", "Pos_1"), ("Z", "Pos_2")],
    ),
    "TaxiPath3.csv": Recipe(builder=build_taxi_path, quote_all=True,
                            note="endpoints filtered against TaxiNodes"),
    "TaxiPathNode3.csv": Recipe(
        source="TaxiPathNode",
        columns=["ID", "PathID", "NodeIndex", "ContinentID", ("LocX", "Loc_0"), ("LocY", "Loc_1"),
                 ("LocZ", "Loc_2"), "Flags", "Delay"],
        quote_all=True,
    ),
    # Already a correct 3.4.3 export in the repo -- reuse it rather than re-deriving.
    "ItemEffect3.csv": Recipe(
        local="Hotfix/ItemEffect3.csv",
        columns=["ID", "LegacySlotIndex", "TriggerType", "Charges", "CoolDownMSec",
                 "CategoryCoolDownMSec", "SpellCategoryID", "SpellID", "ChrSpecializationID",
                 "ParentItemID"],
        note="copied from CSV/Hotfix/ItemEffect3.csv",
    ),
    # Same source LoadSpellXSpellVisualHotfixes reads, so the two agree regardless
    # of which of the two parallel loaders assigns SpellVisuals last.
    "SpellVisuals3.csv": Recipe(
        local="Hotfix/SpellXSpellVisual3.csv",
        columns=[("SpellId", "SpellID"), ("SpellXSpellVisualId", "ID")],
        # LoadSpellVisuals uses dict.Add (GameData.cs:1339) and throws on a duplicate
        # key; the source has 477 duplicate SpellIDs. Last wins, matching the hotfix
        # loader's own overwrite at GameData.cs:2836-2839.
        dedupe="SpellId",
        note="projected from CSV/Hotfix/SpellXSpellVisual3.csv",
    ),
    "AuraSpells3.csv": Recipe(builder=build_aura_spells, note="SpellEffect apply-aura effects"),
    "ItemSpellsData3.csv": Recipe(
        builder=build_item_spells_data, note="SpellCategories + SpellCooldowns"
    ),
    "ItemDisplayIdToFileDataId3.csv": Recipe(
        builder=build_item_display_id_to_file_data_id,
        note="ItemIdToDisplayId3 joined with Item.IconFileDataID",
    ),
}


# --------------------------------------------------------------------- building


def build(name: str, recipe: Recipe, build_id: str) -> tuple[list[str], list[list[str]]]:
    if recipe.builder is not None:
        return recipe.builder(build_id)

    if recipe.local is not None:
        source_path = CSV_DIR / recipe.local
        if not source_path.exists():
            raise RuntimeError(f"{name}: local source {recipe.local} not found")
    else:
        source_path = fetch_client_csv(recipe.source or "", build_id)

    source_rows = read_csv(source_path)
    if not source_rows:
        raise RuntimeError(f"{name}: source {source_path.name} has no rows")

    wanted = recipe.source_columns()
    missing = [c for c in wanted if c not in source_rows[0]]
    if missing:
        # Fail loudly: the loaders parse by index, so a silently dropped column
        # shifts every later field and corrupts the data without any error.
        raise RuntimeError(f"{name}: source {source_path.name} is missing columns {missing}")

    header = recipe.header()
    types = LOADER_TYPES.get(name, {})
    rows: list[list[str]] = []
    for row in source_rows:
        if recipe.where is not None and not recipe.where(row):
            continue
        values = [format_number(row[c]) for c in wanted]
        for position, column in enumerate(header):
            cs_type = types.get(column)
            if cs_type is not None:
                values[position] = coerce_to_type(values[position], cs_type)
        rows.append(values)

    if recipe.dedupe is not None:
        index = header.index(recipe.dedupe)
        collapsed: dict[str, list[str]] = {}
        for row in rows:
            collapsed[row[index]] = row  # last wins
        rows = list(collapsed.values())

    return header, rows


def render(header: Sequence[str], rows: Iterable[Sequence[str]], quote_all: bool) -> str:
    buffer = io.StringIO(newline="")
    writer = csv.writer(
        buffer,
        lineterminator="\r\n",
        quoting=csv.QUOTE_ALL if quote_all else csv.QUOTE_MINIMAL,
    )
    writer.writerow(header)
    writer.writerows(rows)
    return buffer.getvalue()


def audit(name: str, rendered: str, target: Path) -> str:
    if not target.exists():
        return f"{name:<42} NEW ({rendered.count(chr(10)) - 1} rows)"

    current = target.read_text(encoding="utf-8-sig", newline="")
    if current == rendered:
        return f"{name:<42} up to date"

    def index_by_id(text: str) -> dict[str, tuple[str, ...]]:
        reader = csv.reader(io.StringIO(text, newline=""))
        next(reader, None)
        return {row[0]: tuple(row) for row in reader if row}

    old, new = index_by_id(current), index_by_id(rendered)
    added = len(new.keys() - old.keys())
    removed = len(old.keys() - new.keys())
    changed = sum(1 for key in old.keys() & new.keys() if old[key] != new[key])
    return (
        f"{name:<42} rows {len(old)} -> {len(new)}  "
        f"added={added} removed={removed} changed={changed}"
    )


def main() -> int:
    parser = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter
    )
    parser.add_argument("file", nargs="?", help="CSV under HermesProxy/CSV, e.g. Item3.csv")
    parser.add_argument("--all", metavar="SUFFIX", help="build every recipe ending in <SUFFIX>.csv")
    parser.add_argument("--build", default="3.4.3.54261", help="client build (default: %(default)s)")
    parser.add_argument("--audit", action="store_true", help="report drift, write nothing")
    args = parser.parse_args()

    if args.all:
        targets = [n for n in RECIPES if Path(n).stem.endswith(args.all)]
    elif args.file:
        targets = [Path(args.file).name]
    else:
        parser.error("give a file, or --all <suffix>")

    unknown = [n for n in targets if n not in RECIPES]
    if unknown:
        print(f"no recipe for: {', '.join(unknown)}", file=sys.stderr)
        print(f"known: {', '.join(sorted(RECIPES))}", file=sys.stderr)
        return 2

    failures = 0
    for name in sorted(targets):
        recipe = RECIPES[name]
        try:
            header, rows = build(name, recipe, args.build)
        except Exception as exc:  # noqa: BLE001 - report and continue over the batch
            print(f"{name:<42} FAILED ({exc})")
            failures += 1
            continue

        rendered = render(header, rows, recipe.quote_all)
        target = CSV_DIR / name
        if args.audit:
            print(audit(name, rendered, target))
            continue

        target.write_text(rendered, encoding="utf-8", newline="")
        suffix = f"  [{recipe.note}]" if recipe.note else ""
        print(f"{name:<42} {len(rows)} rows{suffix}")

    if failures:
        print(f"\n{failures} recipe(s) failed", file=sys.stderr)
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
