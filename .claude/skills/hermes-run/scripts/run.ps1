[CmdletBinding()]
param(
    [ValidateSet('debug', 'release')]
    [string]$Mode = 'debug',

    [Alias('Profile')]
    [string]$ConfigProfile,
    [string]$ConfigPath,
    [string]$Environment,
    [switch]$ListProfiles,

    [string]$ServerAddress = '192.168.88.55',
    [int]$ServerPort = 3726,
    [int]$BNetPort = 1119,
    [int]$RestPort = 8082,
    [int]$RealmPort = 8085,
    [int]$InstancePort = 8087,
    [string]$ClientBuild = 'V3_4_3_54261',
    [string]$ServerBuild = 'V3_3_5a_12340',
    [switch]$Metrics,
    [int]$MetricsInterval = 0,
    [switch]$NoBuild,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

$projectRoot = (& git -C $PSScriptRoot rev-parse --show-toplevel 2>$null)
if (-not $projectRoot) {
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
}
$projectRoot = $projectRoot.Trim()

$hermesDir = Join-Path $projectRoot 'HermesProxy'
$profilesDir = Join-Path $hermesDir 'profiles'

if (-not (Test-Path -LiteralPath (Join-Path $hermesDir 'HermesProxy.csproj'))) {
    Write-Error "Could not locate HermesProxy.csproj under '$projectRoot'."
    exit 1
}

# ---- Profile discovery ------------------------------------------------------

function Get-AvailableProfiles {
    $items = @()
    if (Test-Path -LiteralPath $profilesDir) {
        Get-ChildItem -LiteralPath $profilesDir -Filter '*.json' -File |
            ForEach-Object { $items += [pscustomobject]@{ Name = $_.BaseName; Path = $_.FullName; Source = 'profiles/' } }
    }
    Get-ChildItem -LiteralPath $hermesDir -Filter 'appsettings.*.json' -File |
        Where-Object { $_.BaseName -notin @('appsettings', 'appsettings.Development') } |
        ForEach-Object {
            $name = $_.BaseName -replace '^appsettings\.', ''
            $items += [pscustomobject]@{ Name = $name; Path = $_.FullName; Source = 'appsettings.<env>.json' }
        }
    return $items
}

if ($ListProfiles) {
    $items = Get-AvailableProfiles
    if (-not $items) {
        Write-Host 'No profiles found.'
        Write-Host "  Create one at: $profilesDir\<name>.json"
        exit 0
    }
    Write-Host 'Available profiles:' -ForegroundColor Green
    $items | Sort-Object Source, Name | Format-Table Name, Source, Path -AutoSize | Out-Host
    exit 0
}

if ($ConfigProfile -and $ConfigPath) {
    Write-Error '-Profile and -ConfigPath are mutually exclusive.'
    exit 2
}

$resolvedConfig = $null
if ($ConfigPath) {
    if (-not (Test-Path -LiteralPath $ConfigPath)) {
        Write-Error "ConfigPath '$ConfigPath' does not exist."
        exit 2
    }
    $resolvedConfig = (Resolve-Path -LiteralPath $ConfigPath).ProviderPath
}
elseif ($ConfigProfile) {
    $candidates = @(
        (Join-Path $profilesDir "$ConfigProfile.json"),
        (Join-Path $hermesDir "appsettings.$ConfigProfile.json")
    ) | Where-Object { Test-Path -LiteralPath $_ }
    $candidates = @($candidates)  # force array even when single match

    if ($candidates.Count -eq 0) {
        Write-Error "Profile '$ConfigProfile' not found. Searched:`n  $profilesDir\$ConfigProfile.json`n  $hermesDir\appsettings.$ConfigProfile.json`nUse -ListProfiles to see what's available."
        exit 2
    }
    if ($candidates.Count -gt 1) {
        Write-Error "Profile '$ConfigProfile' is ambiguous — found in both profiles/ and appsettings.<env>.json form:`n  $($candidates -join "`n  ")`nRename or delete one."
        exit 2
    }
    $resolvedConfig = $candidates[0]
}

# ---- Build dotnet args ------------------------------------------------------

$config = if ($Mode -eq 'release') { 'Release' } else { 'Debug' }
$logLevelHigh = if ($Mode -eq 'release') { 'Information' } else { 'Verbose' }
$logLevelPacket = if ($Mode -eq 'release') { 'Information' } else { 'Debug' }

$dotnetArgs = @(
    'run',
    '--project', 'HermesProxy',
    '-c', $config
)
if ($NoBuild) { $dotnetArgs += '--no-build' }

# When a profile/config-path is in use, only emit --set for explicitly-passed params.
# The profile file is the base; user overrides win. Mode still governs build config
# but log-level overrides yield to the profile.
$useProfile = [bool]$resolvedConfig
$bound = $PSBoundParameters

function Add-Set {
    param(
        [Parameter(Mandatory)][string]$Key,
        [Parameter(Mandatory)][AllowEmptyString()][string]$Value,
        [string]$ParamName,
        [switch]$AlwaysEmit
    )
    if ($AlwaysEmit -or -not $useProfile) {
        $script:appArgs += @('--set', "$Key=$Value")
        return
    }
    if ($ParamName -and $bound.ContainsKey($ParamName)) {
        $script:appArgs += @('--set', "$Key=$Value")
    }
}

$appArgs = @()
if ($resolvedConfig) {
    $appArgs += @('--config', $resolvedConfig)
}

Add-Set -Key 'ClientBuild'                 -Value $ClientBuild   -ParamName 'ClientBuild'
Add-Set -Key 'ServerBuild'                 -Value $ServerBuild   -ParamName 'ServerBuild'
Add-Set -Key 'ServerAddress'               -Value $ServerAddress -ParamName 'ServerAddress'
Add-Set -Key 'ServerPort'                  -Value $ServerPort    -ParamName 'ServerPort'
Add-Set -Key 'BNetPort'                    -Value $BNetPort      -ParamName 'BNetPort'
Add-Set -Key 'RestPort'                    -Value $RestPort      -ParamName 'RestPort'
Add-Set -Key 'RealmPort'                   -Value $RealmPort     -ParamName 'RealmPort'
Add-Set -Key 'InstancePort'                -Value $InstancePort  -ParamName 'InstancePort'
Add-Set -Key 'DebugOutput'                 -Value 'true'
Add-Set -Key 'Log.Packet.MinimumLevel'     -Value $logLevelPacket
Add-Set -Key 'Log.Server.MinimumLevel'     -Value $logLevelHigh
Add-Set -Key 'Log.Console.MinimumLevel'    -Value $logLevelHigh

if ($MetricsInterval -gt 0) { $appArgs += @('--metrics-interval', "$MetricsInterval") }
elseif ($Metrics) { $appArgs += '--metrics' }

# ---- Environment overlay (Mode B) -------------------------------------------

if ($Environment) {
    $env:DOTNET_ENVIRONMENT = $Environment
    Write-Host "[hermes-run] DOTNET_ENVIRONMENT set to '$Environment' (will layer appsettings.$Environment.json on top, if copied to output)." -ForegroundColor Yellow
}

# ---- Echo + run -------------------------------------------------------------

$fullCmd = @('dotnet') + $dotnetArgs + @('--') + $appArgs
Write-Host "[hermes-run] cwd     : $projectRoot"
Write-Host "[hermes-run] config  : $config (mode=$Mode)"
if ($resolvedConfig) {
    Write-Host "[hermes-run] profile : $resolvedConfig"
}
elseif (-not $Environment) {
    Write-Host "[hermes-run] target  : $ServerAddress`:$ServerPort"
}
if ($Environment) {
    Write-Host "[hermes-run] env     : DOTNET_ENVIRONMENT=$Environment"
}
Write-Host "[hermes-run] cmd     :"
Write-Host ('  ' + ($fullCmd -join ' '))

if ($DryRun) {
    Write-Host "[hermes-run] -DryRun set, exiting without launch."
    exit 0
}

Push-Location $projectRoot
try {
    & dotnet @dotnetArgs '--' @appArgs
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
