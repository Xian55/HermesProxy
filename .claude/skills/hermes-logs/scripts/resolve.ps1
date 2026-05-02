[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('latest', 'match', 'screenshot', 'pkt', 'client-root', 'client-log', 'client-crashes')]
    [string]$Mode,

    [int]$Count = 1,
    [string]$Pattern,
    [string]$LogPath,
    [string]$Name
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
$logsDirs = @(
    (Join-Path $projectRoot 'HermesProxy\bin\Release\Logs'),
    (Join-Path $projectRoot 'HermesProxy\bin\Debug\Logs')
) | Where-Object { Test-Path $_ }

function Get-AllLogs {
    foreach ($dir in $logsDirs) {
        Get-ChildItem -LiteralPath $dir -Filter 'hermes-*.log' -File -ErrorAction SilentlyContinue
    }
}

function Get-ClientRoot {
    # client-* subcommands are optional — they only work if the user has opted in by setting
    # this env var. Exit code 3 means "feature not configured" (distinct from 1=not-found,
    # 2=bad-args). Caller relays the message; never guess a path.
    $root = $env:HERMES_TOOLS_CLIENT_WOTLK
    if ([string]::IsNullOrWhiteSpace($root)) {
        Write-Error 'Client log subcommands are optional and require HERMES_TOOLS_CLIENT_WOTLK to be set to your WoW 3.4.3 client root (the folder containing Logs\ and Errors\, typically ...\_classic_). Persist it via [Environment]::SetEnvironmentVariable(''HERMES_TOOLS_CLIENT_WOTLK'', ''<path>'', ''User'').'
        exit 3
    }
    if (-not (Test-Path -LiteralPath $root)) {
        Write-Error "HERMES_TOOLS_CLIENT_WOTLK points to a path that does not exist: $root"
        exit 3
    }
    return $root
}

function Get-LogStartTime {
    param([string]$Path)
    $name = [System.IO.Path]::GetFileNameWithoutExtension($Path)
    if ($name -match '^hermes-(\d{8})_(\d{6})') {
        return [DateTime]::ParseExact("$($Matches[1])$($Matches[2])", 'yyyyMMddHHmmss', $null)
    }
    if ($name -match '^hermes-(\d{8})') {
        return [DateTime]::ParseExact($Matches[1], 'yyyyMMdd', $null)
    }
    return (Get-Item -LiteralPath $Path).LastWriteTime
}

switch ($Mode) {
    'latest' {
        $logs = Get-AllLogs | Sort-Object LastWriteTime -Descending | Select-Object -First $Count
        if (-not $logs) { exit 1 }
        $logs | ForEach-Object { $_.FullName }
        exit 0
    }

    'match' {
        if ([string]::IsNullOrEmpty($Pattern)) {
            Write-Error '-Pattern required for -Mode match'
            exit 2
        }
        $log = Get-AllLogs |
            Where-Object { $_.Name -like "*$Pattern*" } |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
        if (-not $log) { exit 1 }
        $log.FullName
        exit 0
    }

    'screenshot' {
        if ([string]::IsNullOrEmpty($LogPath)) {
            Write-Error '-LogPath required for -Mode screenshot'
            exit 2
        }
        $basename = [System.IO.Path]::GetFileNameWithoutExtension($LogPath)
        $candidates = @(
            (Join-Path $projectRoot "HermesProxy\screenlogs\$basename.png"),
            (Join-Path $projectRoot "screenlogs\$basename.png")
        )
        foreach ($p in $candidates) {
            if (Test-Path -LiteralPath $p) {
                (Resolve-Path -LiteralPath $p).Path
                exit 0
            }
        }
        exit 1
    }

    'pkt' {
        if ([string]::IsNullOrEmpty($LogPath)) {
            Write-Error '-LogPath required for -Mode pkt'
            exit 2
        }

        # Look for PacketsLog dir near the log
        $logDir = Split-Path -Parent $LogPath
        $binDir = Split-Path -Parent $logDir   # bin/Release or bin/Debug
        $pktDir = Join-Path $binDir 'PacketsLog'
        if (-not (Test-Path -LiteralPath $pktDir)) {
            # fallback: scan both Release and Debug
            $pktDir = $null
            foreach ($dir in @(
                    (Join-Path $projectRoot 'HermesProxy\bin\Release\PacketsLog'),
                    (Join-Path $projectRoot 'HermesProxy\bin\Debug\PacketsLog'))) {
                if (Test-Path -LiteralPath $dir) { $pktDir = $dir; break }
            }
        }
        if (-not $pktDir) { exit 1 }

        # Extract session token (yyyyMMdd_HHmmss) from log basename — current format embeds it
        # in both hermes-<token>.log and {modern|legacy}_<build>_<token>_<seq>.pkt for exact match.
        $logBase = [System.IO.Path]::GetFileNameWithoutExtension($LogPath)
        $sessionToken = $null
        if ($logBase -match '^hermes-(\d{8}_\d{6})') { $sessionToken = $Matches[1] }

        $found = $false

        # Phase 1 — exact session-token match (new format).
        if ($sessionToken) {
            foreach ($stream in @('modern', 'legacy')) {
                $rx = "^${stream}_\d+_${sessionToken}_\d+$"
                $hit = Get-ChildItem -LiteralPath $pktDir -Filter '*.pkt' -File -ErrorAction SilentlyContinue |
                    Where-Object { $_.BaseName -match $rx } |
                    Sort-Object Name |
                    Select-Object -First 1
                if ($hit) {
                    $hit.FullName
                    $found = $true
                }
            }
        }

        if ($found) { exit 0 }

        # Phase 2 — fallback to unix-time proximity (old format, pre-StartupStamp embedding).
        $logStart = Get-LogStartTime -Path $LogPath
        $logUnix = [DateTimeOffset]::new($logStart, [TimeZoneInfo]::Local.GetUtcOffset($logStart)).ToUnixTimeSeconds()

        $pkts = Get-ChildItem -LiteralPath $pktDir -Filter '*.pkt' -File -ErrorAction SilentlyContinue |
            ForEach-Object {
                if ($_.BaseName -match '^(modern|legacy)_(\d+)_(\d+)_(\d+)$') {
                    [PSCustomObject]@{
                        Stream    = $Matches[1]
                        Path      = $_.FullName
                        DeltaSec  = [Math]::Abs([long]$Matches[3] - $logUnix)
                    }
                }
            }

        foreach ($stream in @('modern', 'legacy')) {
            $closest = $pkts |
                Where-Object { $_.Stream -eq $stream -and $_.DeltaSec -le 600 } |
                Sort-Object DeltaSec |
                Select-Object -First 1
            if ($closest) {
                $closest.Path
                $found = $true
            }
        }
        if (-not $found) { exit 1 }
        exit 0
    }

    'client-root' {
        Get-ClientRoot
        exit 0
    }

    'client-log' {
        if ([string]::IsNullOrWhiteSpace($Name)) {
            Write-Error '-Name required for -Mode client-log (e.g. Client, Hotfix, Connection)'
            exit 2
        }
        $root = Get-ClientRoot
        $candidate = Join-Path $root "Logs\$Name"
        if (-not $candidate.EndsWith('.log')) { $candidate = "$candidate.log" }
        if (-not (Test-Path -LiteralPath $candidate)) {
            Write-Error "Client log not found: $candidate"
            exit 1
        }
        (Resolve-Path -LiteralPath $candidate).Path
        exit 0
    }

    'client-crashes' {
        $root = Get-ClientRoot
        $errorsDir = Join-Path $root 'Errors'
        if (-not (Test-Path -LiteralPath $errorsDir)) {
            Write-Error "Errors directory not found under client root: $errorsDir"
            exit 1
        }
        $hits = Get-ChildItem -LiteralPath $errorsDir -Filter '*.txt' -File -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First $Count
        if (-not $hits) { exit 1 }
        $hits | ForEach-Object { $_.FullName }
        exit 0
    }
}
