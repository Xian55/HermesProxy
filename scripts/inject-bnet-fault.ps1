<#
.SYNOPSIS
    Issue #266 fault injector for the BNet listener.

.DESCRIPTION
    Speaks the BNet TLS framing to an already-running HermesProxy and sends deliberately broken
    frames, so the guards in BnetTcpSession / BnetRestApiSession fire against a real session rather
    than only in unit tests. Frames are built with the proxy's own Bgs.Protocol.Header, so the
    encoding is whatever the production code produces — nothing is hand-rolled here.

    Cases:
      Baseline    well-formed Connect. Control: must come back with a ConnectResponse.
      BadPayload  valid header, undecodable payload, then a valid frame. The bad frame is dropped
                  and the session must still answer the frame behind it (EventId 801).
      BadHeader   undecodable header, then a valid frame. Framing is lost, so the connection is
                  closed and the trailing frame must NOT be dispatched (EventId 802).
      Overflow    partial frame, then a read that pushes the pooled buffer past its cap. The
                  connection is closed with a logged reason (EventId 803).

    The REST guard (EventId 850) needs no script:
      curl -k https://127.0.0.1:<RestPort>/bnetserver/login/ -H "Accept: a" -H "Accept: b"
    A duplicate header makes HttpHelper.ParseRequest throw; the session must close, not hang.

.EXAMPLE
    ./scripts/inject-bnet-fault.ps1 -Case Baseline
    ./scripts/inject-bnet-fault.ps1 -Case BadPayload
#>
param(
    [ValidateSet('BadPayload', 'BadHeader', 'Baseline', 'Overflow')]
    [string] $Case = 'BadPayload',
    [string] $ProxyHost = '127.0.0.1',
    [int]    $Port = 1119,
    [string] $BinPath
)

$ErrorActionPreference = 'Stop'

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

$CONNECTION_SERVICE = 0x65446991

function New-Frame {
    param([byte[]] $HeaderBytes, [byte[]] $Payload)
    $frame = New-Object byte[] (2 + $HeaderBytes.Length + $Payload.Length)
    # 2-byte big-endian header length prefix
    $frame[0] = [byte](($HeaderBytes.Length -shr 8) -band 0xFF)
    $frame[1] = [byte]($HeaderBytes.Length -band 0xFF)
    [Array]::Copy($HeaderBytes, 0, $frame, 2, $HeaderBytes.Length)
    if ($Payload.Length -gt 0) {
        [Array]::Copy($Payload, 0, $frame, 2 + $HeaderBytes.Length, $Payload.Length)
    }
    return $frame
}

function New-ConnectHeaderBytes {
    param([uint32] $Token, [uint32] $PayloadSize)
    $h = New-Object Bgs.Protocol.Header
    $h.ServiceId = 0
    $h.ServiceHash = $CONNECTION_SERVICE
    $h.MethodId = 1          # ConnectionService/Connect
    $h.Token = $Token
    $h.Size = $PayloadSize
    return [Google.Protobuf.MessageExtensions]::ToByteArray($h)
}

function New-ValidConnectFrame {
    param([uint32] $Token)
    $req = New-Object Bgs.Protocol.Connection.V1.ConnectRequest
    $req.UseBindlessRpc = $true
    $payload = [Google.Protobuf.MessageExtensions]::ToByteArray($req)
    return New-Frame (New-ConnectHeaderBytes $Token ([uint32]$payload.Length)) $payload
}

# Build the byte stream for the chosen case.
$out = New-Object System.IO.MemoryStream
switch ($Case) {
    'Baseline' {
        # Control: a single well-formed Connect. Must get a ConnectResponse back.
        $f = New-ValidConnectFrame 100
        $out.Write($f, 0, $f.Length)
    }
    'BadPayload' {
        # Valid header, payload that is not decodable protobuf (wire type 7 does not exist).
        # Expect guard 801: the frame is skipped and the NEXT frame still answers.
        $junk = [byte[]] @(0x0F, 0x0F)
        $f1 = New-Frame (New-ConnectHeaderBytes 101 ([uint32]$junk.Length)) $junk
        $out.Write($f1, 0, $f1.Length)
        $f2 = New-ValidConnectFrame 102
        $out.Write($f2, 0, $f2.Length)
    }
    'BadHeader' {
        # Header bytes that will not decode at all -> framing is lost.
        # Expect guard 802: buffer cleared, connection closed.
        $junkHeader = [byte[]] @(0x0F, 0x0F, 0x0F)
        $f = New-Frame $junkHeader ([byte[]] @())
        $out.Write($f, 0, $f.Length)
        # A valid frame behind it, which must NOT be dispatched.
        $f2 = New-ValidConnectFrame 103
        $out.Write($f2, 0, $f2.Length)
    }
    'Overflow' {
        # Handled below as two separate writes; nothing to build here.
    }
}
$payloadBytes = $out.ToArray()

$client = New-Object System.Net.Sockets.TcpClient($ProxyHost, $Port)
$client.NoDelay = $true
$validate = [System.Net.Security.RemoteCertificateValidationCallback] { param($s, $c, $ch, $e) $true }
$ssl = New-Object System.Net.Security.SslStream($client.GetStream(), $false, $validate)
$ssl.AuthenticateAsClient($ProxyHost)
Write-Host "[inject] case=$Case TLS up, sending $($payloadBytes.Length) bytes"

if ($Case -eq 'Overflow') {
    # First write: a frame header claiming 65535 header bytes but sending only a few, so the
    # parser reports Incomplete and those bytes stay resident in the pooled buffer.
    $stub = [byte[]] @(0xFF, 0xFF, 0x01, 0x02, 0x03)
    $ssl.Write($stub, 0, $stub.Length)
    $ssl.Flush()
    Start-Sleep -Milliseconds 300
    # Second write: fills the rest of a 65535-byte read. Appending it to the bytes already held
    # exceeds PooledByteBuffer's 65536 cap, so EnsureCapacity throws inside ReadHandler.
    $blob = New-Object byte[] 65535
    $ssl.Write($blob, 0, $blob.Length)
    $ssl.Flush()
    Write-Host "[inject] overflow: sent $($stub.Length) + $($blob.Length) bytes"
} else {
    $ssl.Write($payloadBytes, 0, $payloadBytes.Length)
    $ssl.Flush()
}

# Read whatever comes back within a short window.
$ssl.ReadTimeout = 3000
$buf = New-Object byte[] 4096
$total = 0
try {
    while ($true) {
        $n = $ssl.Read($buf, $total, $buf.Length - $total)
        if ($n -le 0) { break }
        $total += $n
        if ($total -ge $buf.Length) { break }
    }
    Write-Host "[inject] peer closed after $total bytes"
} catch [System.IO.IOException] {
    if ($total -gt 0) {
        Write-Host "[inject] read timeout, got $total bytes"
    } else {
        Write-Host "[inject] no reply (timeout or reset): $($_.Exception.InnerException.Message)"
    }
}

if ($total -gt 0) {
    $hdrLen = ($buf[0] -shl 8) -bor $buf[1]
    $hdrBytes = New-Object byte[] $hdrLen
    [Array]::Copy($buf, 2, $hdrBytes, 0, $hdrLen)
    $reply = [Bgs.Protocol.Header]::Parser.ParseFrom($hdrBytes)
    Write-Host "[inject] reply header: serviceId=$($reply.ServiceId) methodId=$($reply.MethodId) token=$($reply.Token) status=$($reply.Status) size=$($reply.Size)"
} else {
    Write-Host "[inject] no reply frame"
}

$ssl.Dispose()
$client.Dispose()
