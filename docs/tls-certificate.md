# TLS certificate

By default the proxy serves the well-known TrinityCore development certificate (`CN=*.*`, issued by the TrinityCore Battle.net Aurora CA). This is what virtually every patched modern client expects: Arctium-launched clients skip certificate validation entirely, and clients patched with a TrinityCore-style connection patcher are pinned to exactly this CA chain. **Leave the default alone unless you know you need something else.** The startup log prints the active certificate's subject, issuer and expiry.

`CertificatePfxPath` exists for setups that validate against the **system trust store** instead (e.g. [wowpatch](https://github.com/motivewc/wowpatch)). Those need a certificate that chains to a trusted root CA plus a hostname (not an IP) in the `portal` CVar:

- **Hosted proxy (recommended for server operators):** run HermesProxy on a host with a real domain name and point `CertificatePfxPath` at a publicly trusted certificate for that domain (e.g. Let's Encrypt). Players set `portal "yourdomain.com"` and need no client-side certificate setup at all.
- **Local proxy with a custom CA:** generate your own CA and a `localhost` leaf certificate, install the CA into the local trust store, and point `CertificatePfxPath` at the leaf pfx. Never distribute a CA's **private key** — anyone holding it can forge trusted certificates for every user who installed that CA.
