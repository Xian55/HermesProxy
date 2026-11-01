# Running HermesProxy in Docker

The image is `ghcr.io/xian55/hermesproxy`, for amd64 and arm64. `latest` is the newest release, and each release also has its version as a tag, such as `4.7.0`. A prerelease has its version too, such as `4.7.0-feature-x-abc1234`, and `feature-x` follows the newest build of that branch. Everything is configured from outside: nothing in the image needs changing.

```bash
docker run -d --name hermes \
  -p 1119:1119 -p 8081:8081 -p 8084:8084 -p 8086:8086 \
  -e HERMES_ClientOptions__ClientBuild=V3_4_3_54261 \
  -e HERMES_LegacyServerOptions__Address=192.168.1.20 \
  -e HERMES_ProxyNetworkOptions__ExternalAddress=192.168.1.50 \
  -v hermes-logs:/app/Logs \
  -v hermes-accounts:/app/AccountData \
  -v hermes-packets:/app/PacketsLog \
  ghcr.io/xian55/hermesproxy:latest
```

Here `192.168.1.20` is the legacy server and `192.168.1.50` is the Docker host. In the client's `WTF/Config.wtf`, set `SET portal "192.168.1.50"`. Then start the client as the [README](../README.md#usage-instructions) describes.

## Which address goes where

| Setting | What it must be |
|---|---|
| `ProxyNetworkOptions:ExternalAddress` | The IP address the client uses to reach the Docker host. The proxy sends it to the client as the address to connect to. |
| `portal` in `Config.wtf` | The same address. |
| `LegacyServerOptions:Address` | The legacy auth server, as seen from inside the container. A compose service name works. |
| The realm's address on the legacy server | An address the **proxy container** can reach, because the proxy connects to exactly what the server's realm list says. On AzerothCore and TrinityCore this is `realmlist.address`. The default `127.0.0.1` makes the proxy connect to itself. The Docker host's LAN address suits both the proxy and legacy clients. |

- **`ExternalAddress` is never `0.0.0.0`.** The proxy refuses to start with it, because a client told to connect to `0.0.0.0` drops back to the login screen as it enters the world. The image already listens on every interface: `BindAddress` is `0.0.0.0` in it.
- **The default `127.0.0.1` only suits a client on the Docker host itself.** The proxy logs a warning when it starts this way.
- **`ExternalAddress` is an IP address**, even if the `portal` holds a host name.

## Ports

Publish `1119`, `8081`, `8084` and `8086`. The proxy does not listen on `3724`: it connects out to the legacy server on that port.

Use the same number on both sides of each mapping. The proxy tells the client its configured ports, so `11190:1119` sends the client to the wrong one. To use a different port, change the setting as well:

```yaml
environment:
  HERMES_ProxyNetworkOptions__BNetPort: "1120"
ports:
  - 1120:1120
```

A BNet port other than `1119` also goes in the portal: `SET portal "192.168.1.50:1120"`.

In a Compose override file, `ports` is added to the list instead of replacing it, so the original port stays published. Write `ports: !override` there to replace the list.

## Next to AzerothCore

Add the proxy to the stack's `docker-compose.override.yml`:

```yaml
services:
  hermes-proxy:
    image: ghcr.io/xian55/hermesproxy:latest
    restart: unless-stopped
    environment:
      HERMES_ClientOptions__ClientBuild: V3_4_3_54261
      HERMES_LegacyServerOptions__Build: V3_3_5a_12340
      HERMES_LegacyServerOptions__Address: ac-authserver
      HERMES_LegacyServerOptions__Port: "3724"
      HERMES_ProxyNetworkOptions__ExternalAddress: 192.168.1.50  # the Docker host
    ports:
      - 1119:1119
      - 8081:8081
      - 8084:8084
      - 8086:8086
    volumes:
      - hermes-logs:/app/Logs
      - hermes-accounts:/app/AccountData
      - hermes-packets:/app/PacketsLog
    networks:
      - ac-network

volumes:
  hermes-logs:
  hermes-accounts:
  hermes-packets:
```

Set `realmlist.address` as described [above](#which-address-goes-where).

## Configuring from outside

Environment variables override the `appsettings.json` in the image, and command-line arguments override both.

**Environment variables** cover every setting. `__` separates the section from the key. [configuration.md](configuration.md) lists them all, and so does `docker run --rm ghcr.io/xian55/hermesproxy --help`. The common ones:

| Variable | Meaning |
|---|---|
| `HERMES_ClientOptions__ClientBuild` | The client's build, e.g. `V3_4_3_54261`. |
| `HERMES_ClientOptions__SeedHex` | The seed your launcher uses. |
| `HERMES_ClientOptions__ReportedOS` | `OSX` by default. Some servers only accept `Win`. |
| `HERMES_LegacyServerOptions__Build` | The server's build, or `auto`. |
| `HERMES_LegacyServerOptions__Address` | The legacy auth server. |
| `HERMES_LegacyServerOptions__Port` | Its port. |
| `HERMES_LegacyServerOptions__Password` | Cataclysm Classic 4.4.x only: the legacy account's password, as these clients log in with SRP and never send it. |
| `HERMES_ProxyNetworkOptions__ExternalAddress` | See [above](#which-address-goes-where). |
| `HERMES_ProxyNetworkOptions__BNetPort`, `__RestPort`, `__RealmPort`, `__InstancePort` | See [Ports](#ports). |
| `HERMES_LoggingOptions__MinimumLevel` | `Debug` or `Verbose` when chasing a problem. |

**A Compose override file** holds what differs on one host. Compose loads `docker-compose.override.yml` by itself and merges `environment` key by key, so the file only needs those keys:

```yaml
services:
  hermes-proxy:
    environment:
      HERMES_ProxyNetworkOptions__ExternalAddress: 192.168.1.50
```

**An env file** keeps a realm's settings in one plain file, which suits several setups to switch between:

```bash
# my-realm.env
HERMES_ClientOptions__ClientBuild=V3_4_3_54261
HERMES_LegacyServerOptions__Address=ac-authserver
```

```yaml
services:
  hermes-proxy:
    env_file: my-realm.env
```

With plain Docker it is `docker run --env-file my-realm.env ...`. A value in `environment:` beats the same key in the file. Keep passwords out of any file you commit.

**Command-line arguments** go through `command:`, e.g. `command: ["--LoggingOptions:PacketLevel=Debug"]`.

## Two proxies side by side

Each proxy is its own service, for example one per client version. Give the second one four other ports, in both its settings and its port mappings, and its own volumes. Its clients add the BNet port to the portal, as in [Ports](#ports).

## Data

| Volume | Holds |
|---|---|
| `/app/AccountData` | Per-account client settings. Keep this one across upgrades. |
| `/app/Logs` | One log file per run, split at 256 MB, of which a run keeps its newest 4. `docker logs` shows the same lines. They are kept for 30 days: `HERMES_LoggingOptions__RetentionDays` changes that, and `0` keeps them forever. |
| `/app/PacketsLog` | Packet captures, one per session, which is what a bug report starts from. They are kept for 30 days: `HERMES_DiagnosticsOptions__PacketsLogRetentionDays` changes that, and `0` keeps them forever. |

The proxy runs as UID 1654. Named volumes work as they are. A bind-mounted host directory has to be writable by that UID first: `chown 1654 ./logs`.

The image has no shell and no time zone data, so it runs in UTC. Use `docker logs` and `docker cp` rather than `docker exec`.

## Building the image yourself

```bash
git clone https://github.com/Xian55/HermesProxy.git
cd HermesProxy
docker build -t hermesproxy .
```

The build needs BuildKit and a full clone, as the version comes from the git history. It builds the commit at `HEAD`, so uncommitted changes are not included.
