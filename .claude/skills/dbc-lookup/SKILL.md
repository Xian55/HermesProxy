---
name: dbc-lookup
description: Look up WoW DBC/DB2 data from wago.tools for verifying packet structures and field values
user-invocable: true
---

# WoW DBC/DB2 Lookup

Look up WoW client data (DBC/DB2 tables) from wago.tools as the source of truth for packet structures, field values, visual IDs, spell data, etc.

## Usage

`/dbc-lookup <TableName> [filter]`

Examples:
- `/dbc-lookup SpellItemEnchantment ID=323`
- `/dbc-lookup ItemVisuals ID=26`
- `/dbc-lookup SpellItemEnchantment Name_lang=Rockbiter`

## Data Source

**wago.tools** is the authoritative source for WoW client DB2 data.

- **Browse (JS-rendered, WebFetch won't work on HTML):**
  `https://wago.tools/db2/{TableName}?build=1.14.2.42597&filter[{Field}]={Value}`

- **Download CSV (use this for programmatic access):**
  `https://wago.tools/db2/{TableName}/csv?build=1.14.2.42597`

## Supported Builds

Infer the build number from the client version. Use the **latest build** in the range for lookups.

| Version | Expansion     | Recommended Build |
|---------|---------------|-------------------|
| 1.14.0  | Classic Era   | 40618             |
| 1.14.1  | Classic Era   | 42032             |
| 1.14.2  | Classic Era   | 42597             |
| 2.5.2   | TBC Classic   | 41510             |
| 2.5.3   | TBC Classic   | 42598             |
| 3.4.3   | Wotlk Classic | 54261             |

Default to **42597** (1.14.2) unless the user specifies otherwise.

## How to Query

### Step 1: Download the CSV

Use WebFetch to download the CSV from the `/csv` endpoint:
```
WebFetch: https://wago.tools/db2/{TableName}/csv?build=1.14.2.42597
```

If WebFetch returns too much data or truncates, download via curl and process locally:
```bash
curl -sL "https://wago.tools/db2/{TableName}/csv?build=1.14.2.42597" -o /tmp/dbc_{TableName}.csv
```

### Step 2: Filter with xan

Use the locally installed `xan` CLI tool (CSV processor, similar to jq) to filter and query:

```bash
# Filter by exact ID
xan search -s ID "^323$" /tmp/dbc_{TableName}.csv

# Filter by name (partial match)
xan search -s Name_lang "Rockbiter" /tmp/dbc_{TableName}.csv

# Select specific columns
xan select ID,Name_lang,ItemVisual /tmp/dbc_{TableName}.csv

# Combine filter + select
xan search -s ID "^323$" /tmp/dbc_{TableName}.csv | xan select ID,Name_lang,ItemVisual

# View column headers
xan headers /tmp/dbc_{TableName}.csv

# Pretty print results
xan search -s ID "^323$" /tmp/dbc_{TableName}.csv | xan table
```

## Common Tables

| Table | Use Case |
|-------|----------|
| `SpellItemEnchantment` | Enchant IDs, visuals, effects |
| `ItemVisuals` | Visual model file IDs (5 attachment slots) |
| `Spell` | Spell data, effects, mechanics |
| `SpellVisual` | Spell visual effects |
| `Item` | Item entries |
| `ItemSparse` | Item names, stats, flags |
| `Map` | Map IDs and names |
| `AreaTable` | Zone/area definitions |
| `ChrRaces` | Race data |
| `ChrClasses` | Class data |

## Notes

- The wago.tools HTML pages are JS-rendered — **WebFetch will NOT work** on the browse URLs. Always use the `/csv` endpoint.
- For large tables, download once and cache in `/tmp/dbc_{TableName}.csv` to avoid repeated downloads.
- The `xan` tool is installed locally via https://github.com/medialab/xan
- Build `42597` = client 1.14.2 (Classic Era). For TBC client data, use the appropriate TBC build number.
