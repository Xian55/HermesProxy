[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Path,

    [int]$TopOpcodes = 15,
    [int]$ErrorCap = 50
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Path)) {
    Write-Error "File not found: $Path"
    exit 2
}

$file = Get-Item -LiteralPath $Path
$sizeKB = [Math]::Round($file.Length / 1KB, 1)

# FileShare.ReadWrite — works while HermesProxy is still writing
$stream = [System.IO.File]::Open($file.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
$reader = [System.IO.StreamReader]::new($stream, [System.Text.Encoding]::UTF8)

$totalLines = 0
$levelCounts = @{ 'D' = 0; 'I' = 0; 'W' = 0; 'E' = 0; 'V' = 0 }
$catCounts = @{ 'S' = 0; 'N' = 0; 'T' = 0; 'P' = 0 }
$dirCounts = @{ 'C>P S' = 0; 'C P>S' = 0; 'C P<S' = 0; 'C<P S' = 0 }
$opcodeCounts = @{}
$unhandled = @{}
$disconnects = New-Object System.Collections.Generic.List[string]
$errors = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]
$header = @{
    ModernBuild = $null
    LegacyBuild = $null
    ModernOpcodes = $null
    LegacyOpcodes = $null
    ExternalIp = $null
    Services = New-Object System.Collections.Generic.List[string]
}
$firstTs = $null
$lastTs = $null
$lastLine = $null

$lineRx = [regex]'^(?<ts>\d{2}:\d{2}:\d{2}) \| (?<lvl>[DIWEV]) \| (?<cat>[SNTP]) \| (?<src>[^\|]+?)\s*\|(?: (?<dir>C[<>]P [<>]?S|C P[<>]S) \|)? (?<msg>.*)$'
$opRx = [regex]'"(?<op>[CS]?MSG_[A-Z0-9_]+)"'
$disconnectRx = [regex]'Accepting connection|TLS handshake failed|Disconnecting|Closed connection|Unhandled exception|Disconnect:'

while (-not $reader.EndOfStream) {
    $line = $reader.ReadLine()
    if ($null -eq $line) { break }
    $totalLines++
    $lastLine = $line

    $m = $lineRx.Match($line)
    if (-not $m.Success) { continue }

    $ts = $m.Groups['ts'].Value
    $lvl = $m.Groups['lvl'].Value
    $cat = $m.Groups['cat'].Value
    $msg = $m.Groups['msg'].Value
    $dir = $m.Groups['dir'].Value

    if ($null -eq $firstTs) { $firstTs = $ts }
    $lastTs = $ts

    $levelCounts[$lvl]++
    $catCounts[$cat]++
    if ($dir) { $dirCounts[$dir]++ }

    # Header parsing (cheap — only attempted on first ~30 lines via index)
    if ($totalLines -le 30) {
        if ($msg -match 'Modern \(Client\) Build:\s*"([^"]+)"') { $header.ModernBuild = $Matches[1] }
        elseif ($msg -match 'Legacy \(Server\) Build:\s*"([^"]+)"') { $header.LegacyBuild = $Matches[1] }
        elseif ($msg -match 'Loaded (\d+) modern opcodes') { $header.ModernOpcodes = [int]$Matches[1] }
        elseif ($msg -match 'Loaded (\d+) legacy opcodes') { $header.LegacyOpcodes = [int]$Matches[1] }
        elseif ($msg -match 'External IP: (.+)') { $header.ExternalIp = $Matches[1] }
        elseif ($msg -match 'Starting (\S+) service on (.+?)\.\.\.') { [void]$header.Services.Add("$($Matches[1]) → $($Matches[2])") }
    }

    if ($cat -eq 'P') {
        $opMatch = $opRx.Match($msg)
        if ($opMatch.Success) {
            $op = $opMatch.Groups['op'].Value
            if ($opcodeCounts.ContainsKey($op)) { $opcodeCounts[$op]++ } else { $opcodeCounts[$op] = 1 }
        }
        if ($msg -match 'No handler for opcode "([^"]+)"') {
            $name = $Matches[1]
            if ($unhandled.ContainsKey($name)) { $unhandled[$name]++ } else { $unhandled[$name] = 1 }
        }
    }

    if ($disconnectRx.IsMatch($msg)) {
        [void]$disconnects.Add($line)
    }

    if ($lvl -eq 'E' -and $errors.Count -lt $ErrorCap) { [void]$errors.Add($line) }
    elseif ($lvl -eq 'W' -and $warnings.Count -lt $ErrorCap) { [void]$warnings.Add($line) }
}

$reader.Dispose()
$stream.Dispose()

# Aborted-session detection
$aborted = ($totalLines -le 20 -and $lastLine -match '^\d{2}:\d{2}:\d{2} \| [WE] \|')

# Duration
$duration = $null
if ($firstTs -and $lastTs) {
    try {
        $t1 = [TimeSpan]::Parse($firstTs)
        $t2 = [TimeSpan]::Parse($lastTs)
        $delta = $t2 - $t1
        if ($delta.Ticks -lt 0) { $delta = $delta.Add([TimeSpan]::FromDays(1)) }
        $duration = $delta.ToString()
    } catch {}
}

# --- Output (markdown) ---
$out = New-Object System.Text.StringBuilder
[void]$out.AppendLine("# Session summary — $($file.Name)")
[void]$out.AppendLine("")
[void]$out.AppendLine("**File:** ``$($file.FullName)``  ")
[void]$out.AppendLine("**Size:** $sizeKB KB  ")
[void]$out.AppendLine("**Lines:** $totalLines  ")
if ($firstTs -and $lastTs) {
    [void]$out.AppendLine("**Span:** $firstTs → $lastTs (duration $duration)  ")
}

if ($aborted) {
    [void]$out.AppendLine("")
    [void]$out.AppendLine("**Session aborted at handshake.** (≤20 lines, ends in W/E)")
    [void]$out.AppendLine("")
    [void]$out.AppendLine("Last line:")
    [void]$out.AppendLine("``````")
    [void]$out.AppendLine($lastLine)
    [void]$out.AppendLine("``````")
    Write-Output $out.ToString()
    exit 0
}

[void]$out.AppendLine("")
[void]$out.AppendLine("## Header")
if ($header.ModernBuild) { [void]$out.AppendLine("- Modern (client) build: ``$($header.ModernBuild)``") }
if ($header.LegacyBuild) { [void]$out.AppendLine("- Legacy (server) build: ``$($header.LegacyBuild)``") }
if ($null -ne $header.ModernOpcodes) { [void]$out.AppendLine("- Modern opcodes loaded: $($header.ModernOpcodes)") }
if ($null -ne $header.LegacyOpcodes) { [void]$out.AppendLine("- Legacy opcodes loaded: $($header.LegacyOpcodes)") }
if ($header.ExternalIp) { [void]$out.AppendLine("- External IP: $($header.ExternalIp)") }
if ($header.Services.Count -gt 0) {
    [void]$out.AppendLine("- Services:")
    foreach ($s in $header.Services) { [void]$out.AppendLine("  - $s") }
}

[void]$out.AppendLine("")
[void]$out.AppendLine("## Counts")
[void]$out.AppendLine("")
[void]$out.AppendLine("| Level | Count |     | Category | Count |     | Direction | Count |")
[void]$out.AppendLine("|---|---:|---|---|---:|---|---|---:|")
$levelLabels = [ordered]@{ 'V' = 'Verbose'; 'D' = 'Debug'; 'I' = 'Info'; 'W' = 'Warn'; 'E' = 'Error' }
$catLabels = [ordered]@{ 'S' = 'Server'; 'N' = 'Network'; 'T' = 'Storage'; 'P' = 'Packet' }
$dirLabels = [ordered]@{ 'C>P S' = 'C → P (from client)'; 'C<P S' = 'P → C (to client)'; 'C P>S' = 'P → S (to server)'; 'C P<S' = 'S → P (from server)' }
$rows = [Math]::Max($levelLabels.Count, [Math]::Max($catLabels.Count, $dirLabels.Count))
$lvlKeys = @($levelLabels.Keys); $catKeys = @($catLabels.Keys); $dirKeys = @($dirLabels.Keys)
for ($i = 0; $i -lt $rows; $i++) {
    $lvlPart = if ($i -lt $lvlKeys.Count) { "$($levelLabels[$lvlKeys[$i]]) ($($lvlKeys[$i])) | $($levelCounts[$lvlKeys[$i]])" } else { ' | ' }
    $catPart = if ($i -lt $catKeys.Count) { "$($catLabels[$catKeys[$i]]) ($($catKeys[$i])) | $($catCounts[$catKeys[$i]])" } else { ' | ' }
    $dirPart = if ($i -lt $dirKeys.Count) { "$($dirLabels[$dirKeys[$i]]) | $($dirCounts[$dirKeys[$i]])" } else { ' | ' }
    [void]$out.AppendLine("| $lvlPart |   | $catPart |   | $dirPart |")
}

if ($opcodeCounts.Count -gt 0) {
    [void]$out.AppendLine("")
    [void]$out.AppendLine("## Top $TopOpcodes opcodes")
    [void]$out.AppendLine("")
    [void]$out.AppendLine("| Opcode | Count |")
    [void]$out.AppendLine("|---|---:|")
    $opcodeCounts.GetEnumerator() | Sort-Object -Property Value -Descending | Select-Object -First $TopOpcodes | ForEach-Object {
        [void]$out.AppendLine("| ``$($_.Key)`` | $($_.Value) |")
    }
}

if ($unhandled.Count -gt 0) {
    [void]$out.AppendLine("")
    [void]$out.AppendLine("## Unhandled opcodes")
    [void]$out.AppendLine("")
    [void]$out.AppendLine("| Opcode | Count |")
    [void]$out.AppendLine("|---|---:|")
    $unhandled.GetEnumerator() | Sort-Object -Property Value -Descending | ForEach-Object {
        [void]$out.AppendLine("| ``$($_.Key)`` | $($_.Value) |")
    }
}

if ($disconnects.Count -gt 0) {
    [void]$out.AppendLine("")
    [void]$out.AppendLine("## Disconnect / lifecycle events")
    [void]$out.AppendLine("")
    [void]$out.AppendLine("``````")
    foreach ($d in $disconnects) { [void]$out.AppendLine($d) }
    [void]$out.AppendLine("``````")
}

if ($errors.Count -gt 0) {
    [void]$out.AppendLine("")
    [void]$out.AppendLine("## Errors ($($errors.Count) shown, cap $ErrorCap)")
    [void]$out.AppendLine("")
    [void]$out.AppendLine("``````")
    foreach ($e in $errors) { [void]$out.AppendLine($e) }
    [void]$out.AppendLine("``````")
}

if ($warnings.Count -gt 0) {
    [void]$out.AppendLine("")
    [void]$out.AppendLine("## Warnings ($($warnings.Count) shown, cap $ErrorCap)")
    [void]$out.AppendLine("")
    [void]$out.AppendLine("``````")
    foreach ($w in $warnings) { [void]$out.AppendLine($w) }
    [void]$out.AppendLine("``````")
}

Write-Output $out.ToString()
