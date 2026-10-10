# Combat — server packet layouts, 1.14.0.40618

### SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION (0x2922)

- Modern: 10530 (0x2922) · 1.12.1: — · Area: Combat
- Layout: `{ { { guid u32*2 u64 } u32 u8*3 u32 loop[ u64 ] u8 } u32*2 u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 8 · 8 · 8; byte 29: 1 · 1 · (6 unused)
- HermesProxy: `BattlefieldStatusNeedConfirmation` — matches

### SMSG_BATTLEFIELD_STATUS_ACTIVE (0x2923)

- Modern: 10531 (0x2923) · 1.12.1: — · Area: Combat
- Layout: `{ { { guid u32*2 u64 } u32 u8*3 u32 loop[ u64 ] u8 } u32*3 u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 8 · 8 · 8; byte 29: 1 · 1 · (6 unused); byte 42: 1 · 1 · 1 · (5 unused)
- HermesProxy: `BattlefieldStatusActive` — differs

### SMSG_BATTLEFIELD_STATUS_QUEUED (0x2924)

- Modern: 10532 (0x2924) · 1.12.1: 744 (0x2e8) · Area: Combat
- Layout: `{ { { guid u32*2 u64 } u32 u8*3 u32 loop[ u64 ] u8 } u32*2 u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 8 · 8 · 8; byte 29: 1 · 1 · (6 unused); byte 38: 1 · 1 · 1 · (5 unused)
- HermesProxy: `BattlefieldStatusQueued` — matches

### SMSG_BATTLEFIELD_STATUS_NONE (0x2925)

- Modern: 10533 (0x2925) · 1.12.1: — · Area: Combat
- Layout: `{ { guid u32*2 u64 } }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_BATTLEFIELD_STATUS_FAILED (0x2926)

- Modern: 10534 (0x2926) · 1.12.1: — · Area: Combat
- Layout: `{ { guid u32*2 u64 } u64 u32 guid }`
- Size: 28 bytes (packed GUIDs not counted)
- HermesProxy: `BattlefieldStatusFailed` — matches

### SMSG_BATTLEFIELD_LIST (0x2927)

- Modern: 10535 (0x2927) · 1.12.1: 573 (0x23d) · Area: Combat
- Layout: `{ guid u32*2 u8*2 u32 loop[ u32 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 8; byte 16: 1 · 1 · (6 unused)
- HermesProxy: `BattlefieldList` — matches

### SMSG_BATTLEGROUND_PLAYER_POSITIONS (0x2928)

- Modern: 10536 (0x2928) · 1.12.1: — · Area: Combat
- Layout: `{ u32 loop[ guid u32*2 u8*2 ] }`
- HermesProxy: `BattlegroundPlayerPositions` — matches

### SMSG_BATTLEGROUND_PLAYER_JOINED (0x292b)

- Modern: 10539 (0x292b) · 1.12.1: 748 (0x2ec) · Area: Combat
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLEGROUND_PLAYER_LEFT (0x292c)

- Modern: 10540 (0x292c) · 1.12.1: 749 (0x2ed) · Area: Combat
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLEFIELD_PORT_DENIED (0x292d)

- Modern: 10541 (0x292d) · 1.12.1: — · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLEGROUND_INFO_THROTTLED (0x292e)

- Modern: 10542 (0x292e) · 1.12.1: — · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLEFIELD_STATUS_WAIT_FOR_GROUPS (0x292f)

- Modern: 10543 (0x292f) · 1.12.1: — · Area: Combat
- Layout: `{ { { guid u32*2 u64 } u32 u8*3 u32 loop[ u64 ] u8 } u32*2 loop[ u8*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 8 · 8 · 8; byte 29: 1 · 1 · (6 unused); byte 38: 8 · 16

### SMSG_RATED_PVP_INFO (0x2930)

- Modern: 10544 (0x2930) · 1.12.1: — · Area: Combat
- Layout: `{ loop[ u32*14 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 56: 1 · (7 unused); byte 113: 1 · (7 unused); byte 170: 1 · (7 unused); byte 227: 1 · (7 unused); byte 284: 1 · (7 unused); byte 341: 1 · (7 unused)
- HermesProxy: `RatedPvpInfo` — differs

### SMSG_INSPECT_HONOR_STATS (0x2932)

- Modern: 10546 (0x2932) · 1.12.1: — · Area: Combat
- Layout: `{ guid u8 u16*8 u32*6 u8 }`
- Size: 42 bytes (packed GUIDs not counted)
- HermesProxy: `InspectHonorStatsResultTBC`; `InspectHonorStatsResultClassic`; `InspectHonorStatsResultWotLKClassic` — matches

### SMSG_PVP_MATCH_STATISTICS (0x2933)

- Modern: 10547 (0x2933) · 1.12.1: — · Area: Combat
- Layout: `{ { u8 opt[ loop[ opt[ u8 ] alt[ u8 ] ] loop[ guid bytes ] ] u32 alt[ ] loop[ u8 ] loop[ u32*3 ] opt[ u8 ] loop[ { guid u32*11 loop[ u32 ] u8 opt[ u32*3 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · (5 unused); byte 5: 8 · 8
- HermesProxy: `PVPMatchStatisticsMessage` — matches

### SMSG_WARGAME_REQUEST_SUCCESSFULLY_SENT_TO_OPPONENT (0x2934)

- Modern: 10548 (0x2934) · 1.12.1: — · Area: Combat
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_PVP_OPTIONS_ENABLED (0x2936)

- Modern: 10550 (0x2936) · 1.12.1: — · Area: Combat
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### SMSG_REQUEST_PVP_REWARDS_RESPONSE (0x2937)

- Modern: 10551 (0x2937) · 1.12.1: — · Area: Combat
- Layout: `{ { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } u8 { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 1 · 1 · 1 · (4 unused) · 1 · 1 · 1 · 1 · 1 · 1 · (2 unused); byte 50: 1 · 1 · 1 · 1 · (4 unused); byte 75: 1 · 1 · 1 · 1 · (4 unused); byte 100: 1 · 1 · 1 · 1 · (4 unused); byte 125: 1 · 1 · 1 · 1 · (4 unused); byte 150: 1 · 1 · 1 · 1 · (4 unused); byte 175: 1 · 1 · 1 · 1 · (4 unused); byte 200: 1 · 1 · 1 · 1 · (4 unused)

### SMSG_REQUEST_SCHEDULED_PVP_INFO_RESPONSE (0x2938)

- Modern: 10552 (0x2938) · 1.12.1: — · Area: Combat
- Layout: `{ { u32*3 u8*5 bytes bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · 9 · 10 · 14

### SMSG_BREAK_TARGET (0x293a)

- Modern: 10554 (0x293a) · 1.12.1: — · Area: Combat
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BreakTarget` — matches

### SMSG_ATTACK_START (0x293b)

- Modern: 10555 (0x293b) · 1.12.1: 323 (0x143) · Area: Combat
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SAttackStart` — matches

### SMSG_ATTACK_STOP (0x293c)

- Modern: 10556 (0x293c) · 1.12.1: 324 (0x144) · Area: Combat
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `SAttackStop` — matches

### SMSG_COMBAT_EVENT_FAILED (0x293d)

- Modern: 10557 (0x293d) · 1.12.1: — · Area: Combat
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DUEL_REQUESTED (0x293e)

- Modern: 10558 (0x293e) · 1.12.1: 359 (0x167) · Area: Combat
- Layout: `{ guid guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `DuelRequested` — matches

### SMSG_DUEL_ARRANGED (0x293f)

- Modern: 10559 (0x293f) · 1.12.1: — · Area: Combat
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DUEL_OUT_OF_BOUNDS (0x2940)

- Modern: 10560 (0x2940) · 1.12.1: 360 (0x168) · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `DuelOutOfBounds` — matches

### SMSG_DUEL_IN_BOUNDS (0x2941)

- Modern: 10561 (0x2941) · 1.12.1: 361 (0x169) · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `DuelInBounds` — matches

### SMSG_DUEL_COUNTDOWN (0x2942)

- Modern: 10562 (0x2942) · 1.12.1: 695 (0x2b7) · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `DuelCountdown` — matches

### SMSG_DUEL_COMPLETE (0x2943)

- Modern: 10563 (0x2943) · 1.12.1: 362 (0x16a) · Area: Combat
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `DuelComplete` — matches

### SMSG_DUEL_WINNER (0x2944)

- Modern: 10564 (0x2944) · 1.12.1: 363 (0x16b) · Area: Combat
- Layout: `{ u8*2 u32*2 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 6 · 1 · (3 unused)
- HermesProxy: `DuelWinner` — matches

### SMSG_CAN_DUEL_RESULT (0x2945)

- Modern: 10565 (0x2945) · 1.12.1: — · Area: Combat
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)
- HermesProxy: `CanDuelResult` — matches

### SMSG_CLEAR_TARGET (0x2946)

- Modern: 10566 (0x2946) · 1.12.1: — · Area: Combat
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ClearTarget` — matches

### SMSG_RESET_RANGED_COMBAT_TIMER (0x2947)

- Modern: 10567 (0x2947) · 1.12.1: — · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_PVP_CREDIT (0x2948)

- Modern: 10568 (0x2948) · 1.12.1: 652 (0x28c) · Area: Combat
- Layout: `{ u32*2 guid u32 }`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `PvPCredit` — matches

### SMSG_CANCEL_COMBAT (0x2949)

- Modern: 10569 (0x2949) · 1.12.1: 334 (0x14e) · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `CancelCombat` — matches

### SMSG_ATTACK_SWING_ERROR (0x294a)

- Modern: 10570 (0x294a) · 1.12.1: — · Area: Combat
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 3 · (5 unused)
- HermesProxy: `AttackSwingError` — matches

### SMSG_ATTACK_SWING_LANDED_LOG (0x294b)

- Modern: 10571 (0x294b) · 1.12.1: — · Area: Combat
- Layout: `{ { u64 u32*3 u8*2 loop[ u32*3 ] } u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 9 · (7 unused)

### SMSG_BATTLEGROUND_POINTS (0x294c)

- Modern: 10572 (0x294c) · 1.12.1: — · Area: Combat
- Layout: `{ u16 u8 }`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_BATTLEGROUND_INIT (0x294d)

- Modern: 10573 (0x294d) · 1.12.1: — · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (6 bytes): +0: u32, +4: u16
- HermesProxy: `BattlegroundInit` — matches

### SMSG_MAP_OBJECTIVES_INIT (0x294e)

- Modern: 10574 (0x294e) · 1.12.1: — · Area: Combat
- Layout: `{ u32 loop[ { guid u32*2 u8 opt[ u64 u32 ] } ] }`

### SMSG_BOSS_KILL (0x294f)

- Modern: 10575 (0x294f) · 1.12.1: — · Area: Combat
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_ATTACKER_STATE_UPDATE (0x2950)

- Modern: 10576 (0x2950) · 1.12.1: 330 (0x14a) · Area: Combat
- Layout: `{ u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `AttackerStateUpdate` — matches

### SMSG_PVP_MATCH_START (0x2951)

- Modern: 10577 (0x2951) · 1.12.1: — · Area: Combat
- Layout: `{ u32*3 u8 u32 u64 loop[ { guid u8 u32*3 u32*7 loop[ u32 ] loop[ u32 ] u32 loop[ u32 ] loop[ u32 ] loop[ guid u32 ] loop[ { u32*5 loop[ u32 ] loop[ u32 ] } ] u8 opt[ u32 u32 loop[ u32 u16 u8 ] loop[ u32 u8*2 ] ] opt[ u32*4 u32 loop[ u32 ] loop[ u32*2 ] loop[ u32*3 ] ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_PVP_MATCH_INITIALIZE (0x2953)

- Modern: 10579 (0x2953) · 1.12.1: — · Area: Combat
- Layout: `{ u32 u8 u64*2 u8 u32 u8 }`
- Size: 27 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 26: 1 · 1 · (6 unused)
