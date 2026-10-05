<#
.SYNOPSIS
    Opens NPC gossip windows in the running WoW client by script and has the client say back the
    text each one shows, so a greeting can be checked without a person clicking.

.DESCRIPTION
    Posts keystrokes to the client window: a chat line that targets the NPC, a key bound to
    "interact with target" that opens its window, then /run lines that pick a menu row and SAY the
    greeting. Nothing here reads the screen. What happened is in the session's captures:

        legacy_*.pkt   the SAY lines, "G<n>:<text>"
        modern_*.pkt   the BroadcastText ids the proxy handed out for each NPC text
        hermes-*.log   which of those ids the client then asked for

    scripts/report-gossip-session.py reads all three back and compares sessions.

    The client must be in the world, with its chat box closed and empty and its window not
    minimised. -GoGuid and -Visit teleport with `.go creature <guid>`, so they need a GM account.
    Spawn 79664 is a Stormwind City Guard in the stock cMaNGOS and AzerothCore world databases.

    Works on the 1.14, 2.5 and 3.4.3 clients; the Lua takes whichever gossip API the client has.

.PARAMETER Options
    1-based rows of the guard's menu to open, in order, as the window lists them.

.PARAMETER Guard
    Which guard to target, and in which client locale. The names are kept in this file because a
    console code page turns a Cyrillic argument into question marks.

.PARAMETER GoGuid
    Creature spawn guid to teleport to before the guard menu. 0 stays where the character is.

.PARAMETER Visit
    "spawnGuid:NPC name" pairs. Teleports to each, opens its greeting and says it back as
    G<guid>:. For an NPC with a single greeting and no menu to walk.

.PARAMETER Quit
    Leave with /quit at the end. The client writes its cache (Cache/ADB, Cache/WDB) only on a
    clean exit; killing the process saves nothing, so a "warm cache" run after a kill is not one.

.EXAMPLE
    # Bank, then Auction House, then a clean exit
    ./scripts/drive-gossip.ps1 -GoGuid 79664 -Options 2,1 -Quit

.EXAMPLE
    # Same guard in a ruRU client
    ./scripts/drive-gossip.ps1 -Guard stormwind-ruRU -Options 2,1

.EXAMPLE
    # Two NPCs that only have a greeting
    ./scripts/drive-gossip.ps1 -Options @() -Visit '79806:Orphan Matron Nightingale','90442:Archmage Malin'

.NOTES
    A keybind opens the window because InteractUnit() is protected and cannot be called from
    /run, while a key bound to INTERACTTARGET is an ordinary key press. SetBinding is not saved,
    so the binding is gone at the next login.
#>
#Requires -Version 7.0
param(
    [int[]] $Options = @(2, 1),
    [ValidateSet('stormwind-enUS', 'stormwind-ruRU', 'orgrimmar-enUS')]
    [string] $Guard = 'stormwind-enUS',
    [int] $GoGuid = 0,
    [string[]] $Visit = @(),
    [switch] $Quit,

    # Client process to type into. WowClassic covers 1.14 / 2.5 / 3.4.3.
    [string] $ProcessName = 'WowClassic',

    # Per-character delay. The client drops characters that arrive faster than it reads them.
    [int] $CharDelayMs = 25
)

$ErrorActionPreference = 'Stop'

$guardName = @{
    'stormwind-enUS' = 'Stormwind City Guard'
    'stormwind-ruRU' = 'Штормградский стражник'
    'orgrimmar-enUS' = 'Orgrimmar Grunt'
}[$Guard]

# PostMessageW, not the default ANSI entry point: that one cannot carry a character outside the
# system code page, and a /target line with a Cyrillic name then silently matches nothing.
Add-Type @"
using System;
using System.Runtime.InteropServices;

public static class DriveGossipWin32
{
    [DllImport("user32.dll", EntryPoint = "PostMessageW", CharSet = CharSet.Unicode)]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public const uint WM_KEYDOWN  = 0x0100;
    public const uint WM_KEYUP    = 0x0101;
    public const uint WM_CHAR     = 0x0102;
    public const int  VK_RETURN   = 0x0D;
    public const int  VK_DIVIDE   = 0x6F;
}
"@

function Send-Key([IntPtr] $Hwnd, [int] $VirtualKey) {
    [DriveGossipWin32]::PostMessage($Hwnd, [DriveGossipWin32]::WM_KEYDOWN, [IntPtr]$VirtualKey, [IntPtr]::Zero) | Out-Null
    Start-Sleep -Milliseconds 50
    [DriveGossipWin32]::PostMessage($Hwnd, [DriveGossipWin32]::WM_KEYUP, [IntPtr]$VirtualKey, [IntPtr]::Zero) | Out-Null
}

# Enter opens the chat edit box, the text goes in, Enter sends it.
function Send-Line([IntPtr] $Hwnd, [string] $Line) {
    if ($Line.Length -gt 255) {
        throw "Line is $($Line.Length) chars; the client's chat box takes 255."
    }
    Send-Key $Hwnd ([DriveGossipWin32]::VK_RETURN)
    Start-Sleep -Milliseconds 250
    foreach ($c in $Line.ToCharArray()) {
        [DriveGossipWin32]::PostMessage($Hwnd, [DriveGossipWin32]::WM_CHAR, [IntPtr][int]$c, [IntPtr]::Zero) | Out-Null
        Start-Sleep -Milliseconds $CharDelayMs
    }
    Start-Sleep -Milliseconds 150
    Send-Key $Hwnd ([DriveGossipWin32]::VK_RETURN)
}

$proc = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue |
        Where-Object { $_.MainWindowHandle -ne 0 } |
        Select-Object -First 1
if (-not $proc) {
    throw "No '$ProcessName' process with a window found. Is the client running and in the world?"
}
$hwnd = $proc.MainWindowHandle

# The gossip API moved between client generations; each line takes whichever this client has.
$select = 'local g=C_GossipInfo;(SelectGossipOption or g.SelectOption)({0})'
# Cut at 200: SendChatMessage throws past 255 characters and then says nothing at all.
$say    = 'local g=C_GossipInfo;SendChatMessage(("G{0}:"..tostring((g and g.GetText or GetGossipText)())):sub(1,200),"SAY")'
$close  = 'local g=C_GossipInfo;(g and g.CloseGossip or CloseGossip)()'

function Open-Gossip([string] $NpcName) {
    Send-Line $hwnd "/target $NpcName"
    Start-Sleep -Milliseconds 700
    Send-Key $hwnd ([DriveGossipWin32]::VK_DIVIDE)
}

function Go-ToCreature([int] $SpawnGuid) {
    Send-Line $hwnd ".go creature $SpawnGuid"
    # Long enough for a cross-map teleport's loading screen.
    Start-Sleep -Seconds 6
}

if ($GoGuid -ne 0) {
    Go-ToCreature $GoGuid
}

Send-Line $hwnd '/run SetBinding("NUMPADDIVIDE","INTERACTTARGET")'
Start-Sleep -Milliseconds 700

foreach ($option in $Options) {
    Open-Gossip $guardName
    Start-Sleep -Milliseconds 1600
    Send-Line $hwnd ('/run ' + ($select -f $option))
    Start-Sleep -Milliseconds 1800
    Send-Line $hwnd ('/run ' + ($say -f $option))
    Start-Sleep -Milliseconds 1400
    Send-Line $hwnd ('/run ' + $close)
    Start-Sleep -Milliseconds 700
    Write-Host "option $option done"
}

foreach ($stop in $Visit) {
    $guid, $npc = $stop.Split(':', 2)
    Go-ToCreature ([int]$guid)
    Open-Gossip $npc
    Start-Sleep -Milliseconds 2200
    Send-Line $hwnd ('/run ' + ($say -f $guid))
    Start-Sleep -Milliseconds 1400
    Send-Line $hwnd ('/run ' + $close)
    Start-Sleep -Milliseconds 700
    Write-Host "visit $npc done"
}

if ($Quit) {
    Send-Line $hwnd '/quit'
    $deadline = (Get-Date).AddSeconds(60)
    while ((Get-Date) -lt $deadline -and (Get-Process -Name $ProcessName -ErrorAction SilentlyContinue)) {
        Start-Sleep -Milliseconds 500
    }
    Write-Host ("client exited by itself: " + (-not (Get-Process -Name $ProcessName -ErrorAction SilentlyContinue)))
}
