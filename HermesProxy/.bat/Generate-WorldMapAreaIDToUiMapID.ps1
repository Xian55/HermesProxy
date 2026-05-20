# Generate-WorldMapAreaIDToUiMapID.ps1
# Builds CSV/WorldMapAreaIDToUiMapID.csv used by HermesProxy to translate
# legacy 3.3.5a WorldMapAreaID values into V3_4_3.54261 UiMap.db2 IDs during
# SMSG_QUEST_POI_QUERY_RESPONSE translation.
#
# The 8.1.5 BfA source produces BfA UiMap IDs (e.g. Elwynn = 37) which the
# V3_4_3 Classic client does NOT recognize — Classic uses its own UiMap.db2
# numbering (Elwynn = 1429). This script needs a V3_4_3-built source pair:
#
# Source files (download into the same folder as this script):
#   UIMapIDToWorldMapAreaID.lua  - V3_4_3-era Blizzard deprecated mapping (Classic
#                                   Era framexml or hand-built WMA->UiMap table)
#   UiMap.3.4.3.csv               - https://wago.tools/db2/UiMap?build=3.4.3.54261&page=1
#                                   (filter to Classic Era zone rows; used for Name column)
#
# Until a Classic-Era WMA->UiMap source surfaces, the CSV is maintained by hand —
# add rows as new zones are verified against native TC 3.4.3 sniffs.
#
# Output:
#   ../CSV/WorldMapAreaIDToUiMapID.csv with header `WorldMapAreaID,UiMapID,Name`.

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$luaFile   = Join-Path $scriptDir 'UIMapIDToWorldMapAreaID.lua'
$csvFile   = Join-Path $scriptDir 'UiMap.8.1.0.27826.csv'
$outFile   = Join-Path $scriptDir '..\CSV\WorldMapAreaIDToUiMapID.csv'

if (-not (Test-Path $luaFile)) {
    throw "Source file not found: $luaFile`n  Download from https://www.townlong-yak.com/framexml/30477/Blizzard_Deprecated/UIMapIDToWorldMapAreaID.lua/get"
}

# Build UiMapID -> Name lookup (optional, used only for the Name column).
$names = @{}
if (Test-Path $csvFile) {
    Import-Csv $csvFile | ForEach-Object {
        if ($_.Name_lang -and $_.ID) {
            $names[[int]$_.ID] = $_.Name_lang
        }
    }
    Write-Host "Loaded $($names.Count) UiMap names from $csvFile"
} else {
    Write-Host "UiMap CSV not found ($csvFile) - Name column will be empty"
}

# Parse UiMapID -> WorldMapAreaID lines from the Lua source. Each row looks like
#   `1,4,...` (UiMapID,WorldMapAreaID,...). We invert the mapping to
#   WorldMapAreaID -> UiMapID, picking the smallest UiMapID when a single
#   WorldMapAreaID maps to several UiMapIDs (mirrors the upstream script).
$mapping = @{}
Select-String -Path $luaFile -Pattern '^\d+,' | ForEach-Object {
    $cols = $_.Line -split ','
    if ($cols.Count -ge 2) {
        $uiMap    = [int]$cols[0]
        $worldMap = [int]$cols[1]
        if ($uiMap -gt 0 -and $worldMap -gt 0) {
            if (-not $mapping.ContainsKey($worldMap) -or $uiMap -lt $mapping[$worldMap]) {
                $mapping[$worldMap] = $uiMap
            }
        }
    }
}

# Make sure the destination directory exists.
$outDir = Split-Path -Parent $outFile
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

$rows = @('WorldMapAreaID,UiMapID,Name')
$rows += $mapping.GetEnumerator() | Sort-Object { [int]$_.Name } | ForEach-Object {
    $name = ''
    if ($names.ContainsKey($_.Value)) {
        # Strip commas from names so the CSV stays simple to parse.
        $name = ($names[$_.Value] -replace ',', '')
    }
    "$($_.Name),$($_.Value),$name"
}

$rows | Set-Content $outFile -Encoding UTF8

Write-Host "Generated $outFile with $($mapping.Count) entries"
