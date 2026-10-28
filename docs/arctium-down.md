# When arctium.io is down

The Classic clients HermesProxy supports (1.14, 2.5, 3.4.3 and 4.4.2) can't start without a version server that still knows their build, and the Arctium launchers use Arctium's, at `ngdp.arctium.io`. While that server is down, the client closes at startup. This page shows how to stand in for it on your own PC, which takes a few minutes and keeps working whether or not Arctium comes back.

## Symptoms

The client quits before the login screen, or shows "CASC system unable to initialize". On 4.4.2, the newest report in the game's `Errors` folder begins with:

```
ERROR #134 (0x85100086) Fatal condition!
Description: Failed to download required build info asset
```

and the Arctium Game Launcher prints `http://ngdp.arctium.io/EU/wow/60895/versions not reachable. Falling back to:` followed by Blizzard's addresses.

## Cause

On every start the client asks a version server which build to load and which configuration files describe it. Blizzard's version server lists only the newest Classic builds (5.5.4 since October 2026), and Blizzard's download servers no longer carry the configuration files of any build HermesProxy supports, so the launcher points the client at Arctium's version server instead. When that server is unreachable, the launcher falls back to Blizzard's and the client tries to load a build it isn't.

The proxy plays no part: this happens before the client connects to it.

## Workaround: answer the client yourself

Your installation keeps its build's configuration files in `Data\config`, and a version file naming them is all the client needs. Where yours lacks one, the script below fetches it from [mdX7/tact_configs](https://github.com/mdX7/tact_configs), a public archive of the configuration files Blizzard has published, and it takes the rest of Blizzard's answer for your build from the same author's [mdX7/ngdp_data](https://github.com/mdX7/ngdp_data).

1. Open PowerShell in the game folder that holds `Data`, set `$version` on the first line to your client's version, and run:
   ```powershell
   $version = '4.4.2.60895'
   $out = "$env:LOCALAPPDATA\HermesProxy\ngdp-$version"
   if (-not (Test-Path Data\data)) { throw 'Run this in the folder that holds Data' }
   New-Item -ItemType Directory -Force $out | Out-Null
   [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
   $product = if ($version -like '1.*') { 'wow_classic_era' } else { 'wow_classic' }
   $build = $version.Split('.')[-1]
   # Blizzard's answer for this build, as archived by github.com/mdX7/ngdp_data
   $archived = try { (Invoke-WebRequest "https://raw.githubusercontent.com/mdX7/ngdp_data/master/EU/$product/$build/versions" -UseBasicParsing).Content -split '\r?\n' } catch { }
   $eu = $archived | Where-Object { $_ -like 'eu|*' } | Select-Object -First 1
   if (-not $eu) { throw "No archived version answer for $version; check `$version" }
   $fields = $eu.Split('|')
   # the configuration files this installation was set up with, where it says
   $mine = if (Test-Path .build.info) { Import-Csv .build.info -Delimiter '|' | Where-Object { $_.'Version!STRING:0' -eq $version } | Select-Object -First 1 }
   if ($mine) { $fields[1], $fields[2] = $mine.'Build Key!HEX:16', $mine.'CDN Key!HEX:16' }
   else {
       $buildFile = Get-Item -LiteralPath "Data\config\$($fields[1].Substring(0, 2))\$($fields[1].Substring(2, 2))\$($fields[1])" -ErrorAction SilentlyContinue
       # the CDN configuration the client wrote together with it
       $cdn = if ($buildFile) { Get-ChildItem Data\config -Recurse -File | Where-Object { (Get-Content -LiteralPath $_.FullName -TotalCount 1) -eq '# CDN Configuration' -and
           [Math]::Abs(($_.LastWriteTime - $buildFile.LastWriteTime).TotalMinutes) -lt 5 } | Select-Object -First 1 }
       if ($cdn) { $fields[2] = $cdn.Name }
   }
   foreach ($hash in $fields[1], $fields[2]) {
       $path = "$($hash.Substring(0, 2))\$($hash.Substring(2, 2))\$hash"
       if (Test-Path "Data\config\$path") { continue }
       # Blizzard's servers no longer carry it; github.com/mdX7/tact_configs keeps a copy
       New-Item -ItemType Directory -Force (Split-Path "Data\config\$path") | Out-Null
       Invoke-WebRequest "https://raw.githubusercontent.com/mdX7/tact_configs/master/tpr/wow/config/$($path.Replace('\', '/'))" -OutFile "Data\config\$path" -UseBasicParsing
       # a configuration file is named by the MD5 of its contents
       if ((Get-FileHash "Data\config\$path" -Algorithm MD5).Hash -ne $hash) { Remove-Item "Data\config\$path"; throw "The copy of $hash does not match its name" }
       "fetched $hash from tact_configs"
   }
   "build configuration: $($fields[1])"
   "CDN configuration:   $($fields[2])"
   $row = '|' + ($fields[1..($fields.Count - 1)] -join '|')
   $lines = @($archived[0], '## seqn = 1') + @('us', 'eu', 'kr', 'tw', 'cn' | ForEach-Object { $_ + $row })
   [IO.File]::WriteAllText("$out\versions", ($lines -join "`n") + "`n")
   Invoke-WebRequest "http://eu.patch.battle.net:1119/$product/cdns" -OutFile "$out\cdns" -UseBasicParsing
   ```
   Your client's version is in the `.build.info` file next to `Data`, where there is one, and is the File version on the Details tab of `WowClassic.exe`'s properties. The script writes two files, `versions` and `cdns`, and prints the two configuration files it chose and any it had to fetch. This is needed once per installation. ngdp_data has Blizzard's answer for the last build of every supported patch (1.14.0.40618, 1.14.1.42032, 1.14.2.42597, 2.5.2.41510, 2.5.3.42598, 3.4.3.54261, 4.4.2.60895) and for most earlier ones; the script says so when it has none for yours.
2. Serve that folder while you play, for example with Python, from the same PowerShell window (in a new one, set `$version` again first):
   ```powershell
   python -m http.server 8099 --bind 127.0.0.1 --directory "$env:LOCALAPPDATA\HermesProxy\ngdp-$version"
   ```
3. Start the launcher with two more options, both of them:
   ```
   --versionurl http://127.0.0.1:8099/versions --cdnsurl "http://127.0.0.1:8099/cdns?"
   ```
   Keep the `?`. The launcher writes the address over the client's built-in one without ending it, and the `?` turns what is left of the old address into a query string the file server ignores.

The launcher then lists `http://127.0.0.1:8099/versions` as its version file, and the client starts as before. Game data still comes from your installation; only the version answer is local, so this keeps working whether or not Arctium's server is up.

### Older launchers

Arctium WoW Launcher 1.0.0 and Arctium Game Launcher 1.5.0 take these options. The older Arctium WoW Launcher that came with the 1.14 and 2.5 installations tested for this page takes neither: it asks `ngdp.arctium.io` itself, so it can't be pointed anywhere else. If yours doesn't print `Version file: http://127.0.0.1:8099/versions` under `Game CDN connection info:` when it starts, start the client with a newer launcher instead, telling it where the client is. For example, with Arctium WoW Launcher 1.0.0 from a 3.4.3 installation, for a 2.5 client:

```
"<3.4.3 folder>\Arctium WoW Launcher.exe" --version=Classic --path "<folder that holds WowClassic.exe>" --binary WowClassic.exe --versionurl http://127.0.0.1:8099/versions --cdnsurl "http://127.0.0.1:8099/cdns?"
```

Use `--version=ClassicEra` for a 1.14 client, and keep any other options you start it with, such as `--staticseed` or `-config=`. This was tried with 1.14.2.42597 and 2.5.3.42328.

## What the files say

`versions` holds one line per region, as Blizzard's version server sent it for your build:

```
eu|<build configuration>|<CDN configuration>||60895|4.4.2.60895|f337a1b6ece3dacf3ec807287f3c2aa6
```

The script takes that line from ngdp_data and puts in the build and CDN configuration your installation uses: the ones `.build.info` names, or, in an installation without one, the archived build configuration and the CDN configuration the client wrote within minutes of it. The CDN configuration differs between installations, which is why the script reads yours rather than taking the archived one. When the configuration your installation names isn't in `Data\config`, the script fetches it from tact_configs. Configuration files are named by the MD5 of their contents, so the script keeps a fetched file only when its MD5 matches its name, which makes it exactly the file Blizzard published, whoever hosted the copy.

The last field is the product configuration; without it the client stops with "CASC system unable to initialize". `cdns` is Blizzard's own list of download servers, unchanged.

Only configuration files are archived this way. Blizzard has also deleted part of these builds' game data, so a client that isn't installed any more can't be downloaded from Blizzard again: keep a copy of your installation.
