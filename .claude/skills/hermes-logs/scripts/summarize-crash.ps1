[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Path,

    [int]$StackFrames = 20
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Path)) {
    Write-Error "File not found: $Path"
    exit 2
}

$file = Get-Item -LiteralPath $Path
$sizeKB = [Math]::Round($file.Length / 1KB, 1)

# Read the whole file — crash .txt files are typically 50-200 KB; cheap.
$lines = [System.IO.File]::ReadAllLines($file.FullName)

$header = [ordered]@{}
$exceptionType = $null
$errorCode = $null
$errorDescription = $null
$stack = New-Object System.Collections.Generic.List[string]
$keyTags = [ordered]@{}
$threadCount = 0

# Header section: lines 1-9 are key:value pairs separated by ':'.
# After "------" line, the body starts.
$headerEndIdx = -1
for ($i = 0; $i -lt $lines.Count -and $i -lt 12; $i++) {
    if ($lines[$i] -match '^([A-Za-z]+):\s+(.+)$') {
        $header[$Matches[1]] = $Matches[2].Trim()
    }
    if ($lines[$i] -match '^-{20,}$') {
        $headerEndIdx = $i
        break
    }
}

# Walk body
$inAssertionStack = $false
foreach ($line in $lines) {
    if ($line -match '^ERROR\s+#(\S+)\s+\(([^)]+)\)\s+(.+)$') {
        $errorCode = "$($Matches[1]) ($($Matches[2])) — $($Matches[3])"
    }
    elseif ($line -match '^Exception:\s+(.+)$') {
        $exceptionType = $Matches[1].Trim()
    }
    elseif ($line -match '^<ErrorDescription>\s*(.+)$') {
        $errorDescription = $Matches[1].Trim()
    }
    elseif ($line -match '^<Exception\.Assertion:>$') { $inAssertionStack = $true; continue }
    elseif ($line -match '^<:Exception\.Assertion>$') { $inAssertionStack = $false; continue }
    elseif ($inAssertionStack -and $line -match '^DBG-ADDR<([^>]+)>\("([^"]+)"\)') {
        if ($stack.Count -lt $StackFrames) {
            [void]$stack.Add("$($Matches[2])  $($Matches[1])")
        }
    }
    elseif ($line -match '^<(Version|WowProject|Application|Wow\.Platform|Exception\.Platform|Addons\.Current|Addons\.Current\.Function|LuaErrors|Addons\.HasAny\.Loaded|CVar\.lastAddonVersion)>\s*(.*)$') {
        $keyTags[$Matches[1]] = $Matches[2].Trim()
    }
    elseif ($line -match '^---\s*Thread ID:\s*(\S+)') {
        $threadCount++
    }
}

$out = New-Object System.Text.StringBuilder
[void]$out.AppendLine("# Crash report — $($file.Name)")
[void]$out.AppendLine("")
[void]$out.AppendLine("**File:** ``$($file.FullName)``  ")
[void]$out.AppendLine("**Size:** $sizeKB KB  ")
[void]$out.AppendLine("**Lines:** $($lines.Count)  ")
[void]$out.AppendLine("")

[void]$out.AppendLine("## Header")
foreach ($k in $header.Keys) {
    [void]$out.AppendLine("- **${k}:** $($header[$k])")
}
[void]$out.AppendLine("")

[void]$out.AppendLine("## Exception")
if ($errorCode) { [void]$out.AppendLine("- **Error:** $errorCode") }
if ($exceptionType) { [void]$out.AppendLine("- **Type:** $exceptionType") }
if ($errorDescription) { [void]$out.AppendLine("- **Description:** $errorDescription") }
[void]$out.AppendLine("")

if ($stack.Count -gt 0) {
    [void]$out.AppendLine("## Stack (top $($stack.Count) frames, raw addresses)")
    [void]$out.AppendLine("")
    [void]$out.AppendLine("``````")
    for ($i = 0; $i -lt $stack.Count; $i++) {
        [void]$out.AppendLine(("  #{0,-2} {1}" -f $i, $stack[$i]))
    }
    [void]$out.AppendLine("``````")
    [void]$out.AppendLine("")
    [void]$out.AppendLine("_Symbols not present (stripped retail build). Pair with the .dmp via WinDbg/dotnet-dump if you need names._")
    [void]$out.AppendLine("")
}

if ($keyTags.Count -gt 0) {
    [void]$out.AppendLine("## Key tags")
    foreach ($k in $keyTags.Keys) {
        [void]$out.AppendLine("- **${k}:** $($keyTags[$k])")
    }
    [void]$out.AppendLine("")
}

if ($threadCount -gt 0) {
    [void]$out.AppendLine("## Threads")
    [void]$out.AppendLine("- $threadCount thread stack(s) recorded (full per-thread dumps in raw file).")
    [void]$out.AppendLine("")
}

# Companion .dmp lookup
$dmpPath = [System.IO.Path]::ChangeExtension($file.FullName, '.dmp')
if (Test-Path -LiteralPath $dmpPath) {
    $dmpKB = [Math]::Round((Get-Item -LiteralPath $dmpPath).Length / 1KB, 1)
    [void]$out.AppendLine("## Minidump")
    [void]$out.AppendLine("- ``$dmpPath`` ($dmpKB KB)")
}

Write-Output $out.ToString()
