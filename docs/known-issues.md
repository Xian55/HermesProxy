# Known Issues

Bugs and quirks that have workarounds. For things HermesProxy structurally cannot do — Warden-protected servers such as Warmane, for instance — see [known-limitations.md](known-limitations.md).

---

# Classic Era (1.14.x)

## Wand `Shoot` stops in melee range (1.14.x client)

Modern 1.14.x Classic clients have an `autoRangedCombat` setting, on by default, that swaps ranged attacks for melee once the target is in melee range. When a mob reaches you while you wand it, the client stops `Shoot` and starts auto-attack on its own. Pressing `Shoot` again restarts the wand, but the client switches back to melee a second or two later. The 1.12 client has no such setting, so on vanilla servers (VMaNGOS, Kronos, CMaNGOS) you can't finish a mob with your wand once it closes in.

This is the client's choice, not a proxy or server bug: the client sends the melee attack and the wand cancel itself, and the proxy forwards them unchanged.

**Workaround — run once in chat:**
```
/console autoRangedCombat 0
```
Or make it persistent by adding this line to `WTF/Config.wtf` before launch:
```
SET autoRangedCombat "0"
```

With the setting off, the wand keeps firing in melee range. This affects any wand user. Priests on 1.14+ clients also get a chat reminder from the proxy each time they log in.

Moving still interrupts a wand, as it always has on vanilla servers — walk, strafe or jump and you need to press `Shoot` again. That is unrelated to this setting. Background in [#80](https://github.com/Xian55/HermesProxy/issues/80).

---

# WotLK Classic (3.4.3) — beta

3.4.3 support is in beta. Most gameplay works end to end; the items below are the ones a player is likely to meet. Full audited status lives in [wotlk.md](../wotlk.md).

## Trading wedges after a "player is busy" refusal

If a trade is refused because the other player is busy (a mailbox or another window is open on their side), the initiating client cannot start any further trade for the rest of the session. The proxy's trade session and the client's trade UI fall permanently out of step.

**Workaround:** relog. Tracked in [#228](https://github.com/Xian55/HermesProxy/issues/228).

## Corpses stay as bodies instead of turning to bones (AzerothCore)

Delivering a corpse `CreateObject2` to the 3.4.3 client hard-crashes it, so the proxy withholds it. The visible cost is that a corpse keeps its pre-conversion model until you move out of range and back.

Cosmetic only — Remove Insignia and normal looting still work. Tracked in [#190](https://github.com/Xian55/HermesProxy/issues/190).

## Stable slots show as locked until you visit a stable master

3.3.5a has no update field for stable slot count; the number exists only in the reply to a stable-master interaction. At login the proxy has nothing to send, so owned slots render locked.

**Workaround:** open the stable window once. The count is then correct for the rest of the session, including live across purchases. Tracked in [#237](https://github.com/Xian55/HermesProxy/issues/237).

## Not bridged yet

| Feature | Issue |
|---|---|
| Barber shop | [#220](https://github.com/Xian55/HermesProxy/issues/220) |
| Calendar | [#222](https://github.com/Xian55/HermesProxy/issues/222) |
| Currency and honor panel (wired, unverified) | [#214](https://github.com/Xian55/HermesProxy/issues/214) |

The customer support window never finishes loading ([#230](https://github.com/Xian55/HermesProxy/issues/230)). A native 3.4.3 client does the same thing against a native server, so this is not a proxy defect.

## CMaNGOS WotLK backends

Dungeon Finder passes through without working ([#104](https://github.com/Xian55/HermesProxy/issues/104)) and MOTransports crash the client ([#101](https://github.com/Xian55/HermesProxy/issues/101)). Prefer TrinityCore or AzerothCore for 3.4.3 until those close.
