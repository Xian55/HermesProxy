<#
.SYNOPSIS
    PR #328: plays a launcher against a running HermesProxy to check how login tickets are handled.

.DESCRIPTION
    A launcher logs in through the proxy's REST login page, gets a ticket, and hands it to the
    client, which sends it in its first Battle.net Logon request (cached_web_credentials). This
    script does both halves itself, against an already-running proxy and its real legacy server:

      [1] REST login           -> ticket T1, legacy login done by the proxy
      [2] Logon with T1        -> must succeed: T1's session is live
      [3] REST login again     -> ticket T2. Same account, so the proxy tears T1's session down
      [4] Logon with T1        -> must be refused: its session, and legacy login, are gone
      [5] Logon with T2        -> must succeed
      [6] Logon, unknown ticket -> must be refused

    Before the stale-ticket fix, step 4 was accepted and the client got as far as world auth
    before failing. Exits 0 when every step matches, 1 otherwise.

    Frames are built with the proxy's own Bgs.Protocol types, so nothing is hand-rolled here.

    Step 3 logs the account in a second time, so anyone playing on it through this proxy is
    disconnected. Use an account nobody is on.

.EXAMPLE
    ./scripts/fake-launcher.ps1 -Account TESTER -Build 54261
    Prompts for the password. -Build is the client build the proxy was started for.

.EXAMPLE
    $pw = Read-Host -AsSecureString
    ./scripts/fake-launcher.ps1 -Account TESTER -Password $pw -Build 54261 -Platform Wn64 -Locale enUS
#>
param(
    [Parameter(Mandatory)] [string] $Account,
    [SecureString] $Password,
    [Parameter(Mandatory)] [int] $Build,
    [string] $Platform = 'Wn64',
    [string] $Locale = 'enUS',
    [string] $ProxyHost = '127.0.0.1',
    [int]    $BnetPort = 1119,
    [int]    $RestPort = 8081,
    [string] $BinPath
)

$ErrorActionPreference = 'Stop'

if (-not $Password) {
    $Password = Read-Host -AsSecureString "Password for $Account"
}

if (-not $BinPath) {
    $projectRoot = (& git -C $PSScriptRoot rev-parse --show-toplevel 2>$null)
    if (-not $projectRoot) { throw "Not inside a git work tree; pass -BinPath explicitly." }
    $BinPath = Join-Path $projectRoot 'HermesProxy/bin/Release'
}
if (-not (Test-Path (Join-Path $BinPath 'Framework.dll'))) {
    throw "Framework.dll not found under '$BinPath'. Build Release first, or pass -BinPath."
}

[Reflection.Assembly]::LoadFrom((Join-Path $BinPath 'Google.Protobuf.dll')) | Out-Null
[Reflection.Assembly]::LoadFrom((Join-Path $BinPath 'Framework.dll')) | Out-Null

$validate = [System.Net.Security.RemoteCertificateValidationCallback] { param($s, $c, $ch, $e) $true }

function Open-Tls {
    param([int] $Port, [int] $TimeoutMs)
    $tcp = [System.Net.Sockets.TcpClient]::new($ProxyHost, $Port)
    $ssl = [System.Net.Security.SslStream]::new($tcp.GetStream(), $false, $validate)
    $ssl.AuthenticateAsClient($ProxyHost)
    $ssl.ReadTimeout = $TimeoutMs
    return [pscustomobject]@{ Tcp = $tcp; Ssl = $ssl }
}

function Close-Tls {
    param($Conn)
    $Conn.Ssl.Dispose()
    $Conn.Tcp.Dispose()
}

function Invoke-RestLogin {
    $plain = [System.Net.NetworkCredential]::new('', $Password).Password
    $body = @{
        version     = ''
        program_id  = 'WoW'
        platform_id = $Platform
        inputs      = @(
            @{ input_id = 'account_name'; value = $Account },
            @{ input_id = 'password'; value = $plain }
        )
    } | ConvertTo-Json -Compress -Depth 4
    $plain = $null

    $bodyLength = [Text.Encoding]::UTF8.GetByteCount($body)
    $request = "POST /bnetserver/login/$Platform/$Build/$Locale/ HTTP/1.1`r`n" +
               "Host: ${ProxyHost}:$RestPort`r`nContent-Type: application/json`r`n" +
               "Content-Length: $bodyLength`r`n`r`n$body`r`n"
    $bytes = [Text.Encoding]::UTF8.GetBytes($request)
    $body = $null; $request = $null

    # The legacy login happens inside this request, so give the backend time to answer.
    $conn = Open-Tls $RestPort 15000
    try {
        # One write: the proxy parses a request from a single read.
        $conn.Ssl.Write($bytes, 0, $bytes.Length)
        [Array]::Clear($bytes)
        $buf = [byte[]]::new(16384)
        $text = [Text.StringBuilder]::new()
        while (-not $text.ToString().Contains('"authentication_state"')) {
            $n = $conn.Ssl.Read($buf, 0, $buf.Length)
            if ($n -le 0) { break }
            [void]$text.Append([Text.Encoding]::UTF8.GetString($buf, 0, $n))
        }
    } finally { Close-Tls $conn }

    $response = $text.ToString()
    $start = $response.IndexOf('{')
    if ($start -lt 0) { throw "REST login: no JSON in the reply" }
    $result = $response.Substring($start) | ConvertFrom-Json
    if (-not $result.login_ticket) {
        throw "REST login failed: $($result.error_code) $($result.error_message)"
    }
    return [string]$result.login_ticket
}

function Send-Frame {
    param($Ssl, [uint32] $ServiceHash, [uint32] $MethodId, [uint32] $Token, $Message)
    $payload = [Google.Protobuf.MessageExtensions]::ToByteArray($Message)
    $header = [Bgs.Protocol.Header]::new()
    $header.ServiceId = 0
    $header.ServiceHash = $ServiceHash
    $header.MethodId = $MethodId
    $header.Token = $Token
    $header.Size = $payload.Length
    $headerBytes = [Google.Protobuf.MessageExtensions]::ToByteArray($header)

    $frame = [byte[]]::new(2 + $headerBytes.Length + $payload.Length)
    # 2-byte big-endian header length prefix
    $frame[0] = [byte](($headerBytes.Length -shr 8) -band 0xFF)
    $frame[1] = [byte]($headerBytes.Length -band 0xFF)
    [Array]::Copy($headerBytes, 0, $frame, 2, $headerBytes.Length)
    [Array]::Copy($payload, 0, $frame, 2 + $headerBytes.Length, $payload.Length)
    $Ssl.Write($frame, 0, $frame.Length)
}

function Read-Exactly {
    param($Ssl, [int] $Count)
    $buf = [byte[]]::new($Count)
    $offset = 0
    while ($offset -lt $Count) {
        $n = $Ssl.Read($buf, $offset, $Count - $offset)
        if ($n -le 0) { throw [System.IO.EndOfStreamException]::new() }
        $offset += $n
    }
    return , $buf
}

function Invoke-BnetLogon {
    param([string] $Ticket)
    $hashes = [Framework.Constants.OriginalHash]
    $logonToken = 2

    $conn = Open-Tls $BnetPort 5000
    try {
        $connect = [Bgs.Protocol.Connection.V1.ConnectRequest]::new()
        $connect.UseBindlessRpc = $true
        Send-Frame $conn.Ssl ([uint32]$hashes::ConnectionService) 1 1 $connect

        $logon = [Bgs.Protocol.Authentication.V1.LogonRequest]::new()
        $logon.Program = 'WoW'
        $logon.Platform = $Platform
        $logon.Locale = $Locale
        $logon.ApplicationVersion = $Build
        $logon.CachedWebCredentials = [Google.Protobuf.ByteString]::CopyFromUtf8($Ticket)
        Send-Frame $conn.Ssl ([uint32]$hashes::AuthenticationService) 1 $logonToken $logon

        $status = $null
        $complete = $null
        try {
            # An accepted logon also gets OnLogonComplete; wait for both or for a refusal.
            while ($null -eq $status -or ($null -eq $complete -and $status -eq 'Ok')) {
                $lenBytes = Read-Exactly $conn.Ssl 2
                $header = [Bgs.Protocol.Header]::Parser.ParseFrom((Read-Exactly $conn.Ssl (($lenBytes[0] -shl 8) -bor $lenBytes[1])))
                $payload = Read-Exactly $conn.Ssl ([int]$header.Size)
                if ($header.ServiceId -eq 0xFE -and $header.Token -eq $logonToken) {
                    $status = [string][Framework.Constants.BattlenetRpcErrorCode]$header.Status
                }
                elseif ($header.ServiceHash -eq [uint32]$hashes::AuthenticationListener -and $header.MethodId -eq 5) {
                    $complete = [Bgs.Protocol.Authentication.V1.LogonResult]::Parser.ParseFrom($payload)
                }
            }
        }
        catch [System.IO.IOException] { }  # timeout, reset or EOF: judge what arrived
    } finally { Close-Tls $conn }

    $accepted = $status -eq 'Ok' -and $null -ne $complete -and $complete.ErrorCode -eq 0
    $detail = if ($complete) { "OnLogonComplete error_code=$($complete.ErrorCode)" } else { 'no OnLogonComplete' }
    return [pscustomobject]@{
        Accepted = $accepted
        Text     = "reply=$(if ($status) { $status } else { '<none>' }), $detail"
    }
}

function Format-Ticket([string] $Ticket) {
    if ($Ticket.Length -gt 10) { return $Ticket.Substring(0, 10) + '...' }
    return $Ticket
}

$failures = 0
function Write-Step([string] $Step, $Result, [bool] $ExpectAccepted) {
    $ok = $Result.Accepted -eq $ExpectAccepted
    if (-not $ok) { $script:failures++ }
    $want = if ($ExpectAccepted) { 'accept' } else { 'refuse' }
    $mark = if ($ok) { 'ok  ' } else { 'FAIL' }
    Write-Host ("[{0}] {1,-28} -> {2}  (want {3})" -f $mark, $Step, $Result.Text, $want)
}

Write-Host "[fake-launcher] proxy $ProxyHost bnet:$BnetPort rest:$RestPort, account $Account, build $Build"

$t1 = Invoke-RestLogin
Write-Host "[1]  REST login                    -> T1 $(Format-Ticket $t1)"
Write-Step '[2] Logon, T1 (live)' (Invoke-BnetLogon $t1) $true

$t2 = Invoke-RestLogin
Write-Host "[3]  REST login again (drops T1)   -> T2 $(Format-Ticket $t2)"
# Session teardown runs on the proxy's side of the REST request; let it land.
Start-Sleep -Milliseconds 500

Write-Step '[4] Logon, T1 (stale)' (Invoke-BnetLogon $t1) $false
Write-Step '[5] Logon, T2 (live)' (Invoke-BnetLogon $t2) $true
Write-Step '[6] Logon, unknown ticket' (Invoke-BnetLogon ('HP-' + [guid]::NewGuid().ToString('N'))) $false

if ($failures -gt 0) {
    Write-Host "[fake-launcher] FAIL: $failures step(s) did not match"
    exit 1
}
Write-Host "[fake-launcher] PASS"
exit 0
