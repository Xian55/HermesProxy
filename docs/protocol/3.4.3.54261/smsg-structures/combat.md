# Combat — server packet layouts, 3.4.3.54261

### SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION (0x2922)

- Modern: 10530 (0x2922) · 3.3.5a: — · Area: Combat
- Layout: `{ { guid u32*2 u64 u8 } u32 u8*3 u32 loop[ u64 ] u8 } u32*2 u8`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 23: 8 · 8 · 8; byte 30: 1 · 1 · (6 unused)
- HermesProxy: `BattlefieldStatusNeedConfirmation` — differs

### SMSG_BATTLEFIELD_STATUS_ACTIVE (0x2923)

- Modern: 10531 (0x2923) · 3.3.5a: — · Area: Combat
- Layout: `{ { guid u32*2 u64 u8 } u32 u8*3 u32 loop[ u64 ] u8 } u32*3 u8`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 23: 8 · 8 · 8; byte 30: 1 · 1 · (6 unused); byte 43: 1 · 1 · 1 · (5 unused)
- HermesProxy: `BattlefieldStatusActive` — differs

### SMSG_BATTLEFIELD_STATUS_QUEUED (0x2924)

- Modern: 10532 (0x2924) · 3.3.5a: 744 (0x2e8) · Area: Combat
- Layout: `{ { guid u32*2 u64 u8 } u32 u8*3 u32 loop[ u64 ] u8 } u32*3 u8`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 23: 8 · 8 · 8; byte 30: 1 · 1 · (6 unused); byte 43: 1 · 1 · 1 · (5 unused)
- HermesProxy: `BattlefieldStatusQueued` — differs

### SMSG_BATTLEFIELD_STATUS (0x2925)

- Modern: 10533 (0x2925) · 3.3.5a: 724 (0x2d4) · Area: Combat
- Layout: `guid u32*2 u64 u8`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_BATTLEFIELD_STATUS_FAILED (0x2926)

- Modern: 10534 (0x2926) · 3.3.5a: — · Area: Combat
- Layout: `guid u32*2 u64 u8 u64 u32 guid`
- Size: 29 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)
- HermesProxy: `BattlefieldStatusFailed` — differs (#363)

### SMSG_BATTLEFIELD_LIST (0x2927)

- Modern: 10535 (0x2927) · 3.3.5a: 573 (0x23d) · Area: Combat
- Layout: `guid u32*2 u8*2 u32 loop[ u32 ] u8`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 8; byte 16: 1 · 1 · (6 unused)
- HermesProxy: `BattlefieldList` — matches

### SMSG_BATTLEGROUND_PLAYER_POSITIONS (0x2928)

- Modern: 10536 (0x2928) · 3.3.5a: — · Area: Combat
- Layout: `u32 loop[ guid u32*2 u8*2 ]`
- HermesProxy: `BattlegroundPlayerPositions` — matches

### SMSG_BATTLEGROUND_PLAYER_JOINED (0x292b)

- Modern: 10539 (0x292b) · 3.3.5a: 748 (0x2ec) · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLEGROUND_PLAYER_LEFT (0x292c)

- Modern: 10540 (0x292c) · 3.3.5a: 749 (0x2ed) · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x292d)

- Modern: 10541 (0x292d) · 3.3.5a: — · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x292e)

- Modern: 10542 (0x292e) · 3.3.5a: — · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x292f)

- Modern: 10543 (0x292f) · 3.3.5a: — · Area: Combat
- Layout: `{ { guid u32*2 u64 u8 } u32 u8*3 u32 loop[ u64 ] u8 } u32*2 u8*4 { u8 loop[ u8*3 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 23: 8 · 8 · 8; byte 30: 1 · 1 · (6 unused); byte 39: 8 · 16; byte 42: 8 · 1 · (7 unused) · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### SMSG_RATED_PVP_INFO (0x2931)

- Modern: 10545 (0x2931) · 3.3.5a: — · Area: Combat
- Layout: `loop[ { u32*19 u8 } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 76: 1 · (7 unused); byte 153: 1 · (7 unused); byte 230: 1 · (7 unused); byte 307: 1 · (7 unused); byte 384: 1 · (7 unused); byte 461: 1 · (7 unused); byte 538: 1 · (7 unused)
- HermesProxy: `RatedPvpInfo` — matches

### SMSG_INSPECT_HONOR_STATS (0x2933)

- Modern: 10547 (0x2933) · 3.3.5a: — · Area: Combat
- Layout: `guid u8 u16*8 u32*6 u8`
- Size: 42 bytes (packed GUIDs not counted)
- HermesProxy: `InspectHonorStatsResultTBC`; `InspectHonorStatsResultClassic`; `InspectHonorStatsResultWotLKClassic` — matches

### SMSG_PVP_MATCH_STATISTICS (0x2934)

- Modern: 10548 (0x2934) · 3.3.5a: — · Area: Combat
- Layout: `{ u8 opt[ u8*2 guid bytes guid bytes ] u32 alt[ ] u8*2 opt[ u32*6 ] opt[ u8 ] loop[ { guid u32*5 u8 u32*5 loop[ u32 ] u8 opt[ u32*3 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · (5 unused); byte 5: 8 · 8
- HermesProxy: `PVPMatchStatisticsMessage` — matches

### — (0x2935)

- Modern: 10549 (0x2935) · 3.3.5a: — · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x2937)

- Modern: 10551 (0x2937) · 3.3.5a: — · Area: Combat
- Layout: `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### — (0x2938)

- Modern: 10552 (0x2938) · 3.3.5a: — · Area: Combat
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2939)

- Modern: 10553 (0x2939) · 3.3.5a: — · Area: Combat
- Layout: `{ u8 u32*3 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*3 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*3 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*3 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 21: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 43: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 65: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 87: 1 · 1 · 1 · 1 · (4 unused)

### — (0x293a)

- Modern: 10554 (0x293a) · 3.3.5a: — · Area: Combat
- Layout: `{ u32*3 u8*5 bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · 9 · 10 · 14

### SMSG_BREAK_TARGET (0x293c)

- Modern: 10556 (0x293c) · 3.3.5a: 338 (0x152) · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BreakTarget` — matches

### SMSG_ATTACK_START (0x293d)

- Modern: 10557 (0x293d) · 3.3.5a: 323 (0x143) · Area: Combat
- Layout: `guid guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SAttackStart` — matches

### SMSG_ATTACK_STOP (0x293e)

- Modern: 10558 (0x293e) · 3.3.5a: 324 (0x144) · Area: Combat
- Layout: `guid guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `SAttackStop` — matches

### — (0x293f)

- Modern: 10559 (0x293f) · 3.3.5a: — · Area: Combat
- Layout: `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DUEL_REQUESTED (0x2940)

- Modern: 10560 (0x2940) · 3.3.5a: 359 (0x167) · Area: Combat
- Layout: `guid guid guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `DuelRequested` — differs (#363)

### — (0x2941)

- Modern: 10561 (0x2941) · 3.3.5a: — · Area: Combat
- Layout: `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DUEL_OUT_OF_BOUNDS (0x2942)

- Modern: 10562 (0x2942) · 3.3.5a: 360 (0x168) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `DuelOutOfBounds` — matches

### SMSG_DUEL_IN_BOUNDS (0x2943)

- Modern: 10563 (0x2943) · 3.3.5a: 361 (0x169) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `DuelInBounds` — matches

### SMSG_DUEL_COUNTDOWN (0x2944)

- Modern: 10564 (0x2944) · 3.3.5a: 695 (0x2b7) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `DuelCountdown` — matches

### SMSG_DUEL_COMPLETE (0x2945)

- Modern: 10565 (0x2945) · 3.3.5a: 362 (0x16a) · Area: Combat
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `DuelComplete` — matches

### SMSG_DUEL_WINNER (0x2946)

- Modern: 10566 (0x2946) · 3.3.5a: 363 (0x16b) · Area: Combat
- Layout: `bits(6) bits(6) opt[ u8 ] u32*2 bytes bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 6 · 1 · (3 unused)
- HermesProxy: `DuelWinner` — matches

### SMSG_CAN_DUEL_RESULT (0x2947)

- Modern: 10567 (0x2947) · 3.3.5a: — · Area: Combat
- Layout: `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · (6 unused)
- HermesProxy: `CanDuelResult` — differs (#369)

### SMSG_CLEAR_TARGET (0x2948)

- Modern: 10568 (0x2948) · 3.3.5a: 959 (0x3bf) · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ClearTarget` — matches

### — (0x2949)

- Modern: 10569 (0x2949) · 3.3.5a: — · Area: Combat
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_PVP_CREDIT (0x294a)

- Modern: 10570 (0x294a) · 3.3.5a: 652 (0x28c) · Area: Combat
- Layout: `u32*2 guid u32 u8`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)
- HermesProxy: `PvPCredit` — differs (#363)

### SMSG_CANCEL_COMBAT (0x294b)

- Modern: 10571 (0x294b) · 3.3.5a: 334 (0x14e) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `CancelCombat` — matches

### SMSG_ATTACK_SWING_ERROR (0x294c)

- Modern: 10572 (0x294c) · 3.3.5a: — · Area: Combat
- Layout: `bits(3)`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 3 · (5 unused)
- HermesProxy: `AttackSwingError` — matches

### — (0x294d)

- Modern: 10573 (0x294d) · 3.3.5a: — · Area: Combat
- Layout: `{ u64 u32*3 u8*2 loop[ u32*3 ] } u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 9 · (7 unused)

### — (0x294e)

- Modern: 10574 (0x294e) · 3.3.5a: — · Area: Combat
- Layout: `u16 u8`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_BATTLEGROUND_INIT (0x294f)

- Modern: 10575 (0x294f) · 3.3.5a: — · Area: Combat
- Layout: `struct`
- Struct fields the client uses (6 bytes): +0: u32, +4: u16
- HermesProxy: `BattlegroundInit` — matches

### — (0x2950)

- Modern: 10576 (0x2950) · 3.3.5a: — · Area: Combat
- Layout: `u32 loop[ { guid u32*2 u8 opt[ u64 u32 ] } ]`

### — (0x2951)

- Modern: 10577 (0x2951) · 3.3.5a: — · Area: Combat
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_ATTACKER_STATE_UPDATE (0x2952)

- Modern: 10578 (0x2952) · 3.3.5a: 330 (0x14a) · Area: Combat
- Layout: `u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `AttackerStateUpdate` — matches

### — (0x2953)

- Modern: 10579 (0x2953) · 3.3.5a: — · Area: Combat
- Layout: `u32*2 u8*2 u32 u64 loop[ { guid u8 u32*3 u32*7 loop[ u32 ] loop[ u32 ] u32*2 loop[ u32 ] loop[ u32 ] loop[ guid u32 ] loop[ u32 ] loop[ { u32*5 loop[ u32 ] loop[ u32 ] } ] u8 opt[ u32 u32 loop[ u32 u16 u8 ] loop[ u32 u8*2 ] ] opt[ { u32*4 u32 loop[ u32 ] loop[ u32*2 ] loop[ u32*3 ] } ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 8 · 1 · (7 unused)

### — (0x2956)

- Modern: 10582 (0x2956) · 3.3.5a: — · Area: Combat
- Layout: `u32 u8 u64*2 u8 u32 u8 opt[ u32*3 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 26: 1 · 1 · 1 · (5 unused)
