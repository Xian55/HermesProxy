# General — server packet layouts, 3.4.3.54261

### — (0x256c)

- Modern: 9580 (0x256c) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_AUTH_RESPONSE (0x256d)

- Modern: 9581 (0x256d) · 3.3.5a: 494 (0x1ee) · Area: General
- Layout: `{ u32 u8 opt[ { u32*2 u32 u8*2 u32*4 u64 loop[ { u8 u32 loop[ u8*4 ] } ] u8 { u32*3 u8 } opt[ u16 ] opt[ u16 ] opt[ u64 ] loop[ u8*2 ] loop[ u32 { u8*3 bytes bytes } ] loop[ u32*2 loop[ u8*2 ] u8*3 bytes bytes ] } ] opt[ u32*2 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)
- HermesProxy: `AuthResponse` — differs (#361)

### SMSG_WAIT_QUEUE_UPDATE (0x256e)

- Modern: 9582 (0x256e) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `WaitQueueUpdate` — matches

### SMSG_WAIT_QUEUE_FINISH (0x256f)

- Modern: 9583 (0x256f) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `WaitQueueFinish` — matches

### SMSG_ALL_ACHIEVEMENT_DATA (0x2570)

- Modern: 9584 (0x2570) · 3.3.5a: 1149 (0x47d) · Area: General
- Layout: `{ u32 u32 loop[ u32 { u32 } guid u32*2 ] loop[ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } ] }`
- HermesProxy: `AllAchievementData` — matches

### SMSG_ALL_ACCOUNT_CRITERIA (0x2571)

- Modern: 9585 (0x2571) · 3.3.5a: — · Area: General
- Layout: `u32 loop[ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } ]`
- HermesProxy: `AllAccountCriteria`; `EmptyAllAccountCriteria` — matches

### — (0x2572)

- Modern: 9586 (0x2572) · 3.3.5a: — · Area: General
- Layout: `guid { u32 u32 loop[ u32 { u32 } guid u32*2 ] loop[ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } ] }`

### SMSG_SETUP_CURRENCY (0x2573)

- Modern: 9587 (0x2573) · 3.3.5a: — · Area: General
- Layout: `u32 loop[ { u32*2 u8*2 opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u64 ] } ]`
- HermesProxy: `SetupCurrency`; `EmptySetupCurrency` — matches

### — (0x2574)

- Modern: 9588 (0x2574) · 3.3.5a: — · Area: General
- Layout: `{ u32*4 loop[ u32*2 ] u8*2 opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u64 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused)

### — (0x2575)

- Modern: 9589 (0x2575) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x2576)

- Modern: 9590 (0x2576) · 3.3.5a: — · Area: General
- Layout: `u32 bytes`

### — (0x2577)

- Modern: 9591 (0x2577) · 3.3.5a: — · Area: General
- Layout: `u32 bytes`

### SMSG_PHASE_SHIFT_CHANGE (0x2578)

- Modern: 9592 (0x2578) · 3.3.5a: 1148 (0x47c) · Area: General
- Layout: `{ guid { u32*2 guid loop[ u16*2 ] } u32 bytes u32 bytes u32 bytes }`
- HermesProxy: `PhaseShiftChange` — matches

### — (0x2579)

- Modern: 9593 (0x2579) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x257a)

- Modern: 9594 (0x257a) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_MOUNT_RESULT (0x257b)

- Modern: 9595 (0x257b) · 3.3.5a: 366 (0x16e) · Area: General
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x257c)

- Modern: 9596 (0x257c) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BIND_POINT_UPDATE (0x257d)

- Modern: 9597 (0x257d) · 3.3.5a: 341 (0x155) · Area: General
- Layout: `struct`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +8: u32, +12: u32, +16: u32
- HermesProxy: `BindPointUpdate` — matches

### SMSG_RESURRECT_REQUEST (0x257e)

- Modern: 9598 (0x257e) · 3.3.5a: 347 (0x15b) · Area: General
- Layout: `guid u32*3 loop[ u8 ] bits(3) opt[ u8 ] opt[ u8 ] bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · 1 · 1 · (3 unused)
- HermesProxy: `ResurrectRequest` — matches

### SMSG_INITIAL_SETUP (0x2580)

- Modern: 9600 (0x2580) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (2 bytes): +0: u8, +1: u8
- HermesProxy: `InitialSetup` — matches

### SMSG_TRADE_UPDATED (0x2581)

- Modern: 9601 (0x2581) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32*3 u64 u32*4 alt[ ] loop[ { u8 u32 guid { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u8 opt[ u32*2 guid u32*3 u8 loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] ] } ] }`
- HermesProxy: `TradeUpdated` — matches

### SMSG_TRADE_STATUS (0x2582)

- Modern: 9602 (0x2582) · 3.3.5a: 288 (0x120) · Area: General
- Layout: `u8 bits(5) opt[ opt[ u8 ] u32*2 ] opt[ u32 ] opt[ guid guid ] opt[ u8 ] opt[ u32*2 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 5 · (2 unused)
- HermesProxy: `TradeStatusPkt` — matches

### SMSG_ENUM_CHARACTERS_RESULT (0x2583)

- Modern: 9603 (0x2583) · 3.3.5a: 59 (0x3b) · Area: General
- Layout: `{ u8 u32 u32*2 u32 u32 opt[ u32 ] loop[ u32*2 ] loop[ u32*2 ] loop[ { guid u64 u8*4 u32 u8 u32*5 guid u32*8 loop[ u32*3 u8*2 ] u64 u16 u32*6 loop[ u32*2 ] loop[ u32 ] u8*3 loop[ opt[ u8 ] alt[ u8 ] ] loop[ { opt[ bytes ] } ] bytes } ] loop[ u32 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · (1 unused)
- HermesProxy: `EnumCharactersResult` — matches

### SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT (0x2585)

- Modern: 9605 (0x2585) · 3.3.5a: — · Area: General
- Layout: `u8 bits(6) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)
- HermesProxy: `GenerateRandomCharacterNameResult` — matches; see #361

### — (0x2586)

- Modern: 9606 (0x2586) · 3.3.5a: — · Area: General
- Layout: `u32*3 u8`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### — (0x2587)

- Modern: 9607 (0x2587) · 3.3.5a: — · Area: General
- Layout: `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_PET_MODE (0x2588)

- Modern: 9608 (0x2588) · 3.3.5a: 378 (0x17a) · Area: General
- Layout: `guid u16 u8`
- Size: 3 bytes (packed GUIDs not counted)

### — (0x2589)

- Modern: 9609 (0x2589) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_ROLE_CHANGED_INFORM (0x258a)

- Modern: 9610 (0x258a) · 3.3.5a: — · Area: General
- Layout: `u8 guid guid u8*2`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 5: 8 · 8
- HermesProxy: `RoleChangedInform` — matches

### — (0x258b)

- Modern: 9611 (0x258b) · 3.3.5a: — · Area: General
- Layout: `u8 guid`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x258c)

- Modern: 9612 (0x258c) · 3.3.5a: — · Area: General
- Layout: `u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8

### SMSG_SUMMON_RAID_MEMBER_VALIDATE_FAILED (0x258d)

- Modern: 9613 (0x258d) · 3.3.5a: — · Area: General
- Layout: `u32 loop[ guid u32 ]`

### — (0x258e)

- Modern: 9614 (0x258e) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x258f)

- Modern: 9615 (0x258f) · 3.3.5a: — · Area: General
- Layout: `u8 u32 loop[ u32 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2590)

- Modern: 9616 (0x2590) · 3.3.5a: — · Area: General
- Layout: `guid u64*2 u8`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_PET_STABLE_RESULT (0x2593)

- Modern: 9619 (0x2593) · 3.3.5a: 627 (0x273) · Area: General
- Layout: `struct`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `PetStableResult` — matches

### SMSG_NEW_WORLD (0x2594)

- Modern: 9620 (0x2594) · 3.3.5a: 62 (0x3e) · Area: General
- Layout: `struct`
- Struct fields the client uses (36 bytes): +0: u32, +4: 8 bytes, +12: u32, +16: u32, +20: u32, +24: f32, +24: 8 bytes, +28: f32, +32: u32, +32: f32
- HermesProxy: `NewWorld` — matches

### — (0x2595)

- Modern: 9621 (0x2595) · 3.3.5a: — · Area: General
- Layout: `u32 { u32*6 } u32*4`
- Size: 44 bytes (packed GUIDs not counted)

### — (0x2596)

- Modern: 9622 (0x2596) · 3.3.5a: — · Area: General
- Layout: `u32 { u32*6 } u32*4`
- Size: 44 bytes (packed GUIDs not counted)

### SMSG_LOGIN_VERIFY_WORLD (0x2597)

- Modern: 9623 (0x2597) · 3.3.5a: 566 (0x236) · Area: General
- Layout: `struct`
- Struct fields the client uses (24 bytes): +0: u32, +4: 8 bytes, +12: u32, +16: u32, +20: u32
- HermesProxy: `LoginVerifyWorld` — matches

### — (0x2598)

- Modern: 9624 (0x2598) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x2599)

- Modern: 9625 (0x2599) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: 8 bytes

### — (0x259a)

- Modern: 9626 (0x259a) · 3.3.5a: — · Area: General
- Layout: `guid u8*2 opt[ { u64 u32*20 } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 1 · (7 unused)

### — (0x259b)

- Modern: 9627 (0x259b) · 3.3.5a: — · Area: General
- Layout: `guid { u64 u32*20 } u32*2`
- Size: 96 bytes (packed GUIDs not counted)

### — (0x259c)

- Modern: 9628 (0x259c) · 3.3.5a: — · Area: General
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x259d)

- Modern: 9629 (0x259d) · 3.3.5a: — · Area: General
- Layout: `u32 u8 opt[ u32 ] opt[ u32 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### — (0x259e)

- Modern: 9630 (0x259e) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### — (0x259f)

- Modern: 9631 (0x259f) · 3.3.5a: — · Area: General
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x25a0)

- Modern: 9632 (0x25a0) · 3.3.5a: — · Area: General
- Layout: `u8 u32 bits(4) loop[ guid u32*4 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 5: 4 · (4 unused)

### — (0x25a4)

- Modern: 9636 (0x25a4) · 3.3.5a: — · Area: General
- Layout: `u32 loop[ u16 ]`

### — (0x25a5)

- Modern: 9637 (0x25a5) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +8: 8 bytes, +16: u32

### — (0x25a7)

- Modern: 9639 (0x25a7) · 3.3.5a: — · Area: General
- Layout: `guid { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused) · 6 · (2 unused)

### SMSG_SUSPEND_TOKEN (0x25a8)

- Modern: 9640 (0x25a8) · 3.3.5a: — · Area: General
- Layout: `u32 bits(2)`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)
- HermesProxy: `SuspendToken` — matches

### SMSG_RESUME_TOKEN (0x25a9)

- Modern: 9641 (0x25a9) · 3.3.5a: — · Area: General
- Layout: `u32 bits(2)`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)
- HermesProxy: `ResumeToken` — matches

### — (0x25aa)

- Modern: 9642 (0x25aa) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x25ab)

- Modern: 9643 (0x25ab) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x25ac)

- Modern: 9644 (0x25ac) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ u32 ] }`

### SMSG_WORLD_SERVER_INFO (0x25ad)

- Modern: 9645 (0x25ad) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8 opt[ u32 ] opt[ u64 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · 1 · 1 · 1 · (3 unused)
- HermesProxy: `WorldServerInfo` — differs (#361)

### SMSG_ACCOUNT_MOUNT_UPDATE (0x25ae)

- Modern: 9646 (0x25ae) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 loop[ u32 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `AccountMountUpdate`; `EmptyAccountMountUpdate` — matches

### — (0x25af)

- Modern: 9647 (0x25af) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_ACCOUNT_TOY_UPDATE (0x25b0)

- Modern: 9648 (0x25b0) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32*3 loop[ u32 ] loop[ u8 ] loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `AccountToyUpdate`; `EmptyAccountToyUpdate` — matches

### — (0x25b6)

- Modern: 9654 (0x25b6) · 3.3.5a: — · Area: General
- Layout: `{ u32*5 loop[ u32 ] loop[ u32 ] }`

### SMSG_VENDOR_INVENTORY (0x25b8)

- Modern: 9656 (0x25b8) · 3.3.5a: 415 (0x19f) · Area: General
- Layout: `{ { guid u8 u32 opt[ ] loop[ u64 u32*7 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] } }`
- HermesProxy: `VendorInventory` — matches

### — (0x25ba)

- Modern: 9658 (0x25ba) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### — (0x25bb)

- Modern: 9659 (0x25bb) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOAD_CUF_PROFILES (0x25bc)

- Modern: 9660 (0x25bc) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ { u8*4 u16*2 u8*5 u16*3 bytes } ] }`
- HermesProxy: `LoadCUFProfiles` — not checked

### SMSG_PARTY_INVITE (0x25bd)

- Modern: 9661 (0x25bd) · 3.3.5a: 111 (0x6f) · Area: General
- Layout: `{ { u8*2 u32 { u8*3 bytes bytes } guid guid u16 u8 u32*2 bytes loop[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 6 · 1 · (3 unused); byte 6: 1 · 1 · 8 · 8 · (6 unused)
- HermesProxy: `PartyInvite` — differs (#369)

### SMSG_FEATURE_SYSTEM_STATUS (0x25bf)

- Modern: 9663 (0x25bf) · 3.3.5a: 969 (0x3c9) · Area: General
- Layout: `{ { u8 u32*9 u64 u32*5 u16*2 u32 loop[ u32*2 ] u8*6 { u8 u32*22 } opt[ u32*3 ] opt[ u32 loop[ u8 ] ] u8 guid guid opt[ { u8 u32*4 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 73: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (4 unused) · 1 · (7 unused); byte 168: 1 · (7 unused)
- HermesProxy: `FeatureSystemStatus` — matches

### SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN (0x25c0)

- Modern: 9664 (0x25c0) · 3.3.5a: — · Area: General
- Layout: `{ { u8*4 opt[ u8*2 ] opt[ { u8 u32*4 } ] u32*2 u64 u32*9 u16*2 u32*2 u32 opt[ u32 ] opt[ { opt[ bytes ] } ] loop[ u32 ] loop[ u32*2 ] loop[ u32 u8 bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (1 unused)
- HermesProxy: `FeatureSystemStatusGlueScreen` — differs (#369)

### SMSG_SEASON_INFO (0x25c1)

- Modern: 9665 (0x25c1) · 3.3.5a: — · Area: General
- Layout: `{ u32*6 u8 }`
- Size: 25 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 1 · 1 · (5 unused)
- HermesProxy: `SeasonInfo` — differs (#369)

### — (0x25c3)

- Modern: 9667 (0x25c3) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_GAME_OBJECT_CUSTOM_ANIM (0x25c4)

- Modern: 9668 (0x25c4) · 3.3.5a: 179 (0xb3) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `GameObjectCustomAnim` — matches

### SMSG_GAME_OBJECT_DESPAWN (0x25c5)

- Modern: 9669 (0x25c5) · 3.3.5a: 533 (0x215) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GameObjectDespawn` — matches

### — (0x25c6)

- Modern: 9670 (0x25c6) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 bytes }`

### — (0x25c7)

- Modern: 9671 (0x25c7) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_XP_GAIN_ABORTED (0x25c9)

- Modern: 9673 (0x25c9) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_PRINT_NOTIFICATION (0x25ca)

- Modern: 9674 (0x25ca) · 3.3.5a: 459 (0x1cb) · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 12 · (4 unused)
- HermesProxy: `PrintNotification` — matches

### — (0x25cb)

- Modern: 9675 (0x25cb) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### — (0x25cc)

- Modern: 9676 (0x25cc) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_TRANSFER_PENDING (0x25cd)

- Modern: 9677 (0x25cd) · 3.3.5a: 63 (0x3f) · Area: General
- Layout: `{ u32*4 u8 opt[ u32*2 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · 1 · (6 unused)
- HermesProxy: `TransferPending` — matches

### — (0x25d0)

- Modern: 9680 (0x25d0) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x25d1)

- Modern: 9681 (0x25d1) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 4 · (4 unused)

### — (0x25d3)

- Modern: 9683 (0x25d3) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32*2 loop[ u16 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### — (0x25d4)

- Modern: 9684 (0x25d4) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32*2 loop[ u16 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### — (0x25d6)

- Modern: 9686 (0x25d6) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32*2 loop[ u32*3 loop[ u16 ] loop[ u16 u8 ] ] }`

### SMSG_UPDATE_TALENT_DATA (0x25d7)

- Modern: 9687 (0x25d7) · 3.3.5a: 1216 (0x4c0) · Area: General
- Layout: `{ { u32 u8 u32 loop[ u8 u32 u8 u32 u8 loop[ u32 u8 ] loop[ u16 ] ] u8 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 9: 1 · (7 unused)
- HermesProxy: `UpdateTalentData`; `EmptyTalentData` — matches

### — (0x25d8)

- Modern: 9688 (0x25d8) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (2 bytes): +0: u16

### — (0x25db)

- Modern: 9691 (0x25db) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x25dc)

- Modern: 9692 (0x25dc) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x25dd)

- Modern: 9693 (0x25dd) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x25df)

- Modern: 9695 (0x25df) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_UPDATE_ACTION_BUTTONS (0x25e0)

- Modern: 9696 (0x25e0) · 3.3.5a: 297 (0x129) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1441 bytes): +1440: u8
- HermesProxy: `UpdateActionButtons` — matches

### — (0x25e1)

- Modern: 9697 (0x25e1) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x25e2)

- Modern: 9698 (0x25e2) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 bytes }`

### — (0x25e3)

- Modern: 9699 (0x25e3) · 3.3.5a: — · Area: General
- Layout: `{ guid { loop[ guid u32*2 u16 u8*3 loop[ { guid u32*3 u16*2 u32*5 u16*2 u8 u32 u32 u32 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] u8 bytes } ] ] loop[ u32 u32 loop[ { u32*4 u8 } ] loop[ u32*2 ] opt[ u16*2 u32*3 u8*2 guid u8 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 8 · 8 · 2 · (6 unused); byte 29: 8 · 8 · 2 · (6 unused); byte 72: 8 · 8; byte 76: 1 · 1 · (6 unused)

### — (0x25e4)

- Modern: 9700 (0x25e4) · 3.3.5a: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u16*2 u8 u32 u32 u32 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] u8 bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### — (0x25e5)

- Modern: 9701 (0x25e5) · 3.3.5a: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u16*2 u8 u32 u32 u32 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] u8 bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### — (0x25e6)

- Modern: 9702 (0x25e6) · 3.3.5a: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u16*2 u8 u32 u32 u32 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] u8 bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### — (0x25e7)

- Modern: 9703 (0x25e7) · 3.3.5a: — · Area: General
- Layout: `{ guid { u8 loop[ u8 ] loop[ u32 ] u32 loop[ guid u16*2 u32*2 u16 u8*2 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · 1 · 1 · (4 unused)

### — (0x25e8)

- Modern: 9704 (0x25e8) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x25ea)

- Modern: 9706 (0x25ea) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8 loop[ { guid u32*3 u16*4 u32*4 u8*3 bytes opt[ guid u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### — (0x25eb)

- Modern: 9707 (0x25eb) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x25ec)

- Modern: 9708 (0x25ec) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8 loop[ guid u32 u8*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_BATTLE_PET_JOURNAL_LOCK_ACQUIRED (0x25ed)

- Modern: 9709 (0x25ed) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `BattlePetJournalLockAcquired` — matches

### — (0x25ee)

- Modern: 9710 (0x25ee) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PET_JOURNAL (0x25ef)

- Modern: 9711 (0x25ef) · 3.3.5a: — · Area: General
- Layout: `{ u16 u32 u32 u8 loop[ guid u32 u8*2 ] loop[ { guid u32*3 u16*4 u32*4 u8*3 bytes opt[ guid u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · (7 unused)
- HermesProxy: `BattlePetJournal` — matches

### — (0x25f0)

- Modern: 9712 (0x25f0) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x25f1)

- Modern: 9713 (0x25f1) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x25f2)

- Modern: 9714 (0x25f2) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x25f3)

- Modern: 9715 (0x25f3) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PARTY_UPDATE (0x25f4)

- Modern: 9716 (0x25f4) · 3.3.5a: — · Area: General
- Layout: `{ { u16 u8*2 u32 guid u32 guid u8 u32 alt[ ] u8 loop[ u8*2 guid u8*5 bytes { opt[ bytes ] } ] opt[ u8 guid u8 ] opt[ u32 loop[ u32 ] ] opt[ u8 u32*2 u8 u32 u8*4 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8; byte 21: 1 · 1 · 1 · (5 unused)
- HermesProxy: `PartyUpdate` — matches

### SMSG_READY_CHECK_STARTED (0x25f6)

- Modern: 9718 (0x25f6) · 3.3.5a: — · Area: General
- Layout: `{ u8 guid guid u64 }`
- Size: 9 bytes (packed GUIDs not counted)
- HermesProxy: `ReadyCheckStarted` — matches

### SMSG_READY_CHECK_RESPONSE (0x25f7)

- Modern: 9719 (0x25f7) · 3.3.5a: — · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `ReadyCheckResponse` — matches

### SMSG_READY_CHECK_COMPLETED (0x25f8)

- Modern: 9720 (0x25f8) · 3.3.5a: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `ReadyCheckCompleted` — matches

### — (0x2604)

- Modern: 9732 (0x2604) · 3.3.5a: — · Area: General
- Layout: `{ u64 u32 }`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x2605)

- Modern: 9733 (0x2605) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### — (0x2606)

- Modern: 9734 (0x2606) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ u64 u32 ] }`

### SMSG_RESPEC_WIPE_CONFIRM (0x2612)

- Modern: 9746 (0x2612) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 guid }`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `RespecWipeConfirm` — matches

### SMSG_LOOT_RESPONSE (0x2614)

- Modern: 9748 (0x2614) · 3.3.5a: 352 (0x160) · Area: General
- Layout: `{ { guid guid u8*4 u32*2 u32 u8 loop[ { u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } ] loop[ u32*2 u8*2 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 8 · 8 · 8 · 8; byte 20: 1 · 1 · (6 unused)
- HermesProxy: `LootResponse` — matches

### SMSG_LOOT_REMOVED (0x2615)

- Modern: 9749 (0x2615) · 3.3.5a: 354 (0x162) · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `LootRemoved` — matches

### SMSG_COIN_REMOVED (0x2617)

- Modern: 9751 (0x2617) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CoinRemoved` — matches

### — (0x2618)

- Modern: 9752 (0x2618) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x2619)

- Modern: 9753 (0x2619) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x261a)

- Modern: 9754 (0x261a) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOOT_RELEASE (0x261b)

- Modern: 9755 (0x261b) · 3.3.5a: 353 (0x161) · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LootReleaseResponse` — matches

### SMSG_LOOT_MONEY_NOTIFY (0x261c)

- Modern: 9756 (0x261c) · 3.3.5a: 355 (0x163) · Area: General
- Layout: `{ u64*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)
- HermesProxy: `LootMoneyNotify` — matches

### SMSG_LOOT_START_ROLL (0x261d)

- Modern: 9757 (0x261d) · 3.3.5a: 673 (0x2a1) · Area: General
- Layout: `{ guid u32*2 u8 u32*3 u8 u32 { u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 28: 2 · 3 · 1 · (2 unused); byte 41: 1 · (7 unused) · 6 · (2 unused); byte 47: 8 · 8
- HermesProxy: `StartLootRoll` — differs (#360)

### SMSG_LOOT_ROLL (0x261e)

- Modern: 9758 (0x261e) · 3.3.5a: 674 (0x2a2) · Area: General
- Layout: `{ guid guid u32 u8 u32 { u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 13: 2 · 3 · 1 · (2 unused); byte 26: 1 · (7 unused) · 6 · (2 unused); byte 32: 8 · 8 · 1 · 1 · (6 unused)
- HermesProxy: `LootRollBroadcast` — differs (#360)

### SMSG_LOOT_MASTER_LIST (0x261f)

- Modern: 9759 (0x261f) · 3.3.5a: 676 (0x2a4) · Area: General
- Layout: `{ guid u32 loop[ guid ] }`
- HermesProxy: `MasterLootCandidateList` — matches

### SMSG_LOOT_ROLLS_COMPLETE (0x2620)

- Modern: 9760 (0x2620) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `LootRollsComplete` — matches

### SMSG_LOOT_ALL_PASSED (0x2621)

- Modern: 9761 (0x2621) · 3.3.5a: 670 (0x29e) · Area: General
- Layout: `{ guid u32 { u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 2 · 3 · 1 · (2 unused); byte 19: 1 · (7 unused) · 6 · (2 unused); byte 25: 8 · 8
- HermesProxy: `LootAllPassed` — differs (#360)

### SMSG_LOOT_ROLL_WON (0x2622)

- Modern: 9762 (0x2622) · 3.3.5a: 671 (0x29f) · Area: General
- Layout: `{ guid guid u32 u8 u32 { u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 13: 2 · 3 · 1 · (2 unused); byte 26: 1 · (7 unused) · 6 · (2 unused); byte 32: 8 · 8 · 1 · (7 unused)
- HermesProxy: `LootRollWon` — differs (#360)

### SMSG_ITEM_PUSH_RESULT (0x2623)

- Modern: 9763 (0x2623) · 3.3.5a: 358 (0x166) · Area: General
- Layout: `{ guid u8 u32*9 guid u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 41: 1 · 1 · 1 · 3 · 1 · 1; byte 54: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `ItemPushResult` — differs (#360)

### SMSG_DISPLAY_TOAST (0x2624)

- Modern: 9764 (0x2624) · 3.3.5a: — · Area: General
- Layout: `{ u64 u8 u32 u8 opt[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 13: 1 · 2 · 1 · 1 · (3 unused); byte 26: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `DisplayToast` — differs (#360)

### SMSG_SET_PET_SPECIALIZATION (0x2625)

- Modern: 9765 (0x2625) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (2 bytes): +0: u16

### — (0x2627)

- Modern: 9767 (0x2627) · 3.3.5a: — · Area: General
- Layout: `{ u64 u32 loop[ u32*3 u64*3 u32*2 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u8 ] }`

### — (0x2628)

- Modern: 9768 (0x2628) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### — (0x2629)

- Modern: 9769 (0x2629) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### — (0x262a)

- Modern: 9770 (0x262a) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### — (0x262b)

- Modern: 9771 (0x262b) · 3.3.5a: — · Area: General
- Layout: `{ { guid u32*7 u32 u32 u32 guid loop[ u32 ] u8 loop[ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } ] loop[ u32 u8 ] loop[ u32 u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 44: 1 · (7 unused)

### — (0x262c)

- Modern: 9772 (0x262c) · 3.3.5a: — · Area: General
- Layout: `{ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 42: 1 · (7 unused)

### SMSG_GROUP_NEW_LEADER (0x262d)

- Modern: 9773 (0x262d) · 3.3.5a: 121 (0x79) · Area: General
- Layout: `{ u8*3 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 9 · (7 unused)
- HermesProxy: `GroupNewLeader` — matches

### SMSG_SEND_RAID_TARGET_UPDATE_ALL (0x262e)

- Modern: 9774 (0x262e) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 loop[ guid u8 ] }`
- HermesProxy: `SendRaidTargetUpdateAll` — matches

### SMSG_SEND_RAID_TARGET_UPDATE_SINGLE (0x262f)

- Modern: 9775 (0x262f) · 3.3.5a: — · Area: General
- Layout: `{ u8*2 guid guid }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8
- HermesProxy: `SendRaidTargetUpdateSingle` — matches

### SMSG_RANDOM_ROLL (0x2630)

- Modern: 9776 (0x2630) · 3.3.5a: — · Area: General
- Layout: `{ guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `RandomRoll` — matches

### SMSG_INSPECT_RESULT (0x2631)

- Modern: 9777 (0x2631) · 3.3.5a: 277 (0x115) · Area: General
- Layout: `{ { { guid u32*2 u8*4 u32 bytes loop[ u32*2 ] loop[ { guid u8 u32*2 loop[ u32 ] { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u32*3 u8 ] loop[ u32 u8 ] loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] } ] } u32*2 u8 u16*2 u32*2 loop[ u16 ] { u32 u8 u32 loop[ u8 u32 u8 u32 u8 loop[ u32 u8 ] loop[ u16 ] ] u8 } u8 loop[ { u8 u32*17 u8 } ] opt[ guid u32*2 ] opt[ u32 ] u32*2 { u32*3 opt[ u32 ] opt[ u32*3 ] opt[ u32 ] loop[ u32*4 ] u8*2 bytes } } }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 6 · (2 unused) · 8 · 8 · 8; byte 48: 1 · (7 unused) · 1 · 1 · (6 unused) · 8; byte 119: 1 · (7 unused) · 8; byte 189: 1 · (7 unused) · 8; byte 259: 1 · (7 unused) · 8; byte 329: 1 · (7 unused) · 8; byte 399: 1 · (7 unused) · 8; byte 469: 1 · (7 unused) · 8; byte 539: 1 · (7 unused); byte 560: 9 · (7 unused)
- HermesProxy: `InspectResult` — differs

### — (0x2632)

- Modern: 9778 (0x2632) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x2633)

- Modern: 9779 (0x2633) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ { u32*2 loop[ u32*9 loop[ u32*3 ] ] } ] }`

### SMSG_RAID_INSTANCE_INFO (0x2634)

- Modern: 9780 (0x2634) · 3.3.5a: 716 (0x2cc) · Area: General
- Layout: `{ u32 loop[ u32*2 u64 u32*2 u8 ] }`
- HermesProxy: `RaidInstanceInfo` — matches

### — (0x2635)

- Modern: 9781 (0x2635) · 3.3.5a: — · Area: General
- Layout: `{ u8*2 u32 { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2636)

- Modern: 9782 (0x2636) · 3.3.5a: — · Area: General
- Layout: `{ u32*4 guid u32*4 u8 }`
- Size: 33 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 34: 1 · (7 unused)

### — (0x2637)

- Modern: 9783 (0x2637) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x2638)

- Modern: 9784 (0x2638) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_MAIL_COMMAND_RESULT (0x263b)

- Modern: 9787 (0x263b) · 3.3.5a: 569 (0x239) · Area: General
- Layout: `{ u64 u32*3 u64 u32 }`
- Size: 32 bytes (packed GUIDs not counted)
- HermesProxy: `MailCommandResult` — matches

### SMSG_NOTIFY_RECEIVED_MAIL (0x263c)

- Modern: 9788 (0x263c) · 3.3.5a: 645 (0x285) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `NotifyReceivedMail` — matches

### — (0x263d)

- Modern: 9789 (0x263d) · 3.3.5a: — · Area: General
- Layout: `{ u64 u8 opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 4 · 1 · (3 unused)

### SMSG_ADDON_INFO (0x2642)

- Modern: 9794 (0x2642) · 3.3.5a: 751 (0x2ef) · Area: General
- Layout: `{ guid { u32 u16 u8 } }`
- Size: 7 bytes (packed GUIDs not counted)

### SMSG_ACHIEVEMENT_EARNED (0x2643)

- Modern: 9795 (0x2643) · 3.3.5a: 1128 (0x468) · Area: General
- Layout: `{ guid guid u32 { u32 } u32*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)
- HermesProxy: `AchievementEarnedPkt` — matches

### — (0x2645)

- Modern: 9797 (0x2645) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x2646)

- Modern: 9798 (0x2646) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 opt[ u8 ] opt[ u8 ] loop[ { u8 u32 loop[ u8*4 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · (5 unused)

### SMSG_CONTROL_UPDATE (0x2647)

- Modern: 9799 (0x2647) · 3.3.5a: 345 (0x159) · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)
- HermesProxy: `ControlUpdate` — matches

### — (0x2648)

- Modern: 9800 (0x2648) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ u32 u8 guid ] }`

### — (0x264b)

- Modern: 9803 (0x264b) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x264c)

- Modern: 9804 (0x264c) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x264e)

- Modern: 9806 (0x264e) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ guid u32 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_CORPSE_LOCATION (0x264f)

- Modern: 9807 (0x264f) · 3.3.5a: — · Area: General
- Layout: `{ u8 guid u32*5 guid }`
- Size: 21 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `CorpseLocation` — matches

### — (0x2651)

- Modern: 9809 (0x2651) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x2655)

- Modern: 9813 (0x2655) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x2670)

- Modern: 9840 (0x2670) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 loop[ u32 u8*4 ] }`

### — (0x2671)

- Modern: 9841 (0x2671) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 guid u32*3 u8*2 }`
- Size: 18 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 8 · 8

### SMSG_SET_TIME_ZONE_INFORMATION (0x2677)

- Modern: 9847 (0x2677) · 3.3.5a: — · Area: General
- Layout: `{ u8*3 bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 7 · 7 · (3 unused)
- HermesProxy: `SetTimeZoneInformation` — differs (#361)

### — (0x2678)

- Modern: 9848 (0x2678) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_TEXT_EMOTE (0x267a)

- Modern: 9850 (0x267a) · 3.3.5a: 261 (0x105) · Area: General
- Layout: `{ guid guid u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `STextEmote` — matches

### — (0x267b)

- Modern: 9851 (0x267b) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_TAXI_NODE_STATUS (0x267c)

- Modern: 9852 (0x267c) · 3.3.5a: 427 (0x1ab) · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 2 · (6 unused)
- HermesProxy: `TaxiNodeStatusPkt` — matches

### SMSG_ACTIVATE_TAXI_REPLY (0x267d)

- Modern: 9853 (0x267d) · 3.3.5a: 430 (0x1ae) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)
- HermesProxy: `ActivateTaxiReplyPkt` — matches

### SMSG_NEW_TAXI_PATH (0x267e)

- Modern: 9854 (0x267e) · 3.3.5a: 431 (0x1af) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `NewTaxiPath` — matches

### — (0x2681)

- Modern: 9857 (0x2681) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: f32

### — (0x2682)

- Modern: 9858 (0x2682) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_LOGOUT_RESPONSE (0x2683)

- Modern: 9859 (0x2683) · 3.3.5a: 76 (0x4c) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `LogoutResponse` — matches

### SMSG_LOGOUT_COMPLETE (0x2684)

- Modern: 9860 (0x2684) · 3.3.5a: 77 (0x4d) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `LogoutComplete` — matches

### SMSG_LOGOUT_CANCEL_ACK (0x2685)

- Modern: 9861 (0x2685) · 3.3.5a: 79 (0x4f) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `LogoutCancelAck` — matches

### SMSG_INSTANCE_RESET (0x2686)

- Modern: 9862 (0x2686) · 3.3.5a: 798 (0x31e) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `InstanceReset` — matches

### SMSG_INSTANCE_RESET_FAILED (0x2687)

- Modern: 9863 (0x2687) · 3.3.5a: 799 (0x31f) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)
- HermesProxy: `InstanceResetFailed` — matches

### SMSG_UPDATE_LAST_INSTANCE (0x2688)

- Modern: 9864 (0x2688) · 3.3.5a: 800 (0x320) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `UpdateLastInstance` — matches

### — (0x2689)

- Modern: 9865 (0x2689) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x268b)

- Modern: 9867 (0x268b) · 3.3.5a: — · Area: General
- Layout: `{ { u32*2 u32 u32 u32 loop[ u64 u32*3 ] loop[ u32*5 ] loop[ u64*2 u8*3 guid u8 ] loop[ u64 u8 u32*3 u64 guid u8 bytes ] } }`

### — (0x268c)

- Modern: 9868 (0x268c) · 3.3.5a: — · Area: General
- Layout: `{ { u8 guid u64 u8 u32*4 u64 u32 u8*3 loop[ guid u64 u8*4 u32 u8 bytes ] bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 40: 8 · 11 · (5 unused)

### — (0x268d)

- Modern: 9869 (0x268d) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ guid u8 ] }`

### — (0x268e)

- Modern: 9870 (0x268e) · 3.3.5a: — · Area: General
- Layout: `{ guid u64*2 u8*3 u32 u8 }`
- Size: 24 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 8 · 8 · 8; byte 25: 1 · (7 unused)

### — (0x268f)

- Modern: 9871 (0x268f) · 3.3.5a: — · Area: General
- Layout: `{ guid u64 u32 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### — (0x2690)

- Modern: 9872 (0x2690) · 3.3.5a: — · Area: General
- Layout: `{ guid u64 u32*2 u8 u32 u8 }`
- Size: 22 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 23: 1 · (7 unused)

### — (0x2691)

- Modern: 9873 (0x2691) · 3.3.5a: — · Area: General
- Layout: `{ guid u64 u8*2 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 1 · (7 unused)

### — (0x2692)

- Modern: 9874 (0x2692) · 3.3.5a: — · Area: General
- Layout: `{ u64 u32*2 u8 u32 u64*2 u8*2 guid guid u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 37: 8 · 8

### — (0x2693)

- Modern: 9875 (0x2693) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x2694)

- Modern: 9876 (0x2694) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x2695)

- Modern: 9877 (0x2695) · 3.3.5a: — · Area: General
- Layout: `{ u64 u32 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### — (0x2696)

- Modern: 9878 (0x2696) · 3.3.5a: — · Area: General
- Layout: `{ u64*2 u32*5 u8*4 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 36: 8 · 8 · 11 · 1 · (4 unused)

### — (0x2697)

- Modern: 9879 (0x2697) · 3.3.5a: — · Area: General
- Layout: `{ guid u64 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · 8 · (7 unused)

### — (0x2698)

- Modern: 9880 (0x2698) · 3.3.5a: — · Area: General
- Layout: `{ u64 u8 bytes }`

### — (0x2699)

- Modern: 9881 (0x2699) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (24 bytes): +0: 8 bytes, +8: u32, +12: u32, +16: u32, +20: u32

### — (0x269a)

- Modern: 9882 (0x269a) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x269b)

- Modern: 9883 (0x269b) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x269c)

- Modern: 9884 (0x269c) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x269d)

- Modern: 9885 (0x269d) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x269e)

- Modern: 9886 (0x269e) · 3.3.5a: — · Area: General
- Layout: `{ u8*4 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8 · 9 · (7 unused)

### SMSG_SPECIAL_MOUNT_ANIM (0x269f)

- Modern: 9887 (0x269f) · 3.3.5a: 370 (0x172) · Area: General
- Layout: `{ guid u32*2 loop[ u32 ] }`
- HermesProxy: `SpecialMountAnim` — matches

### SMSG_PET_ACTION_SOUND (0x26a0)

- Modern: 9888 (0x26a0) · 3.3.5a: 804 (0x324) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PetActionSound` — matches

### — (0x26a1)

- Modern: 9889 (0x26a1) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*4 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_GM_TICKET_SYSTEM_STATUS (0x26a2)

- Modern: 9890 (0x26a2) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `GMTicketSystemStatus` — matches

### SMSG_GM_TICKET_CASE_STATUS (0x26a3)

- Modern: 9891 (0x26a3) · 3.3.5a: — · Area: General
- Layout: `{ { u32 loop[ u32 u64 u32 u16 u64 u32 u8*3 bytes bytes ] } }`
- HermesProxy: `GMTicketCaseStatus` — matches

### SMSG_SET_DUNGEON_DIFFICULTY (0x26a4)

- Modern: 9892 (0x26a4) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `DungeonDifficultySet` — matches

### — (0x26a5)

- Modern: 9893 (0x26a5) · 3.3.5a: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 11 · (5 unused)

### SMSG_WEATHER (0x26a6)

- Modern: 9894 (0x26a6) · 3.3.5a: 756 (0x2f4) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `WeatherPkt` — matches

### SMSG_START_LIGHTNING_STORM (0x26a7)

- Modern: 9895 (0x26a7) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `StartLightningStorm` — matches

### — (0x26a8)

- Modern: 9896 (0x26a8) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_UPDATE_INSTANCE_OWNERSHIP (0x26a9)

- Modern: 9897 (0x26a9) · 3.3.5a: 811 (0x32b) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `UpdateInstanceOwnership` — matches

### — (0x26aa)

- Modern: 9898 (0x26aa) · 3.3.5a: — · Area: General
- Layout: `{ guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_COMPLAINT_RESULT (0x26ab)

- Modern: 9899 (0x26ab) · 3.3.5a: 968 (0x3c8) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### — (0x26b0)

- Modern: 9904 (0x26b0) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_DISMOUNT (0x26b1)

- Modern: 9905 (0x26b1) · 3.3.5a: 940 (0x3ac) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `Dismount` — matches

### SMSG_EQUIPMENT_SET_ID (0x26b2)

- Modern: 9906 (0x26b2) · 3.3.5a: 311 (0x137) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +8: u32, +12: u32
- HermesProxy: `EquipmentSetID` — matches

### SMSG_PET_TAME_FAILURE (0x26b3)

- Modern: 9907 (0x26b3) · 3.3.5a: 371 (0x173) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `PetTameFailure` — matches

### SMSG_AI_REACTION (0x26b5)

- Modern: 9909 (0x26b5) · 3.3.5a: 316 (0x13c) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `AIReaction` — matches

### — (0x26b6)

- Modern: 9910 (0x26b6) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_RESET_FAILED_NOTIFY (0x26b7)

- Modern: 9911 (0x26b7) · 3.3.5a: 918 (0x396) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `ResetFailedNotify` — matches

### — (0x26b8)

- Modern: 9912 (0x26b8) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_COOLDOWN_EVENT (0x26b9)

- Modern: 9913 (0x26b9) · 3.3.5a: 309 (0x135) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `CooldownEvent` — matches

### SMSG_CLEAR_COOLDOWN (0x26ba)

- Modern: 9914 (0x26ba) · 3.3.5a: 478 (0x1de) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)
- HermesProxy: `ClearCooldown` — matches

### SMSG_OVERRIDE_LIGHT (0x26bb)

- Modern: 9915 (0x26bb) · 3.3.5a: 1042 (0x412) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### SMSG_ENABLE_BARBER_SHOP (0x26bc)

- Modern: 9916 (0x26bc) · 3.3.5a: 1063 (0x427) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `EnableBarberShop` — matches

### — (0x26bd)

- Modern: 9917 (0x26bd) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BARBER_SHOP_RESULT (0x26be)

- Modern: 9918 (0x26be) · 3.3.5a: 1064 (0x428) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `BarberShopResult` — differs (#369)

### SMSG_PETITION_SHOW_LIST (0x26bf)

- Modern: 9919 (0x26bf) · 3.3.5a: 444 (0x1bc) · Area: General
- Layout: `{ guid u32 loop[ u32*5 ] }`
- HermesProxy: `ServerPetitionShowList` — matches

### SMSG_PETITION_SHOW_SIGNATURES (0x26c0)

- Modern: 9920 (0x26c0) · 3.3.5a: 447 (0x1bf) · Area: General
- Layout: `{ guid guid guid u32*2 loop[ guid u32 ] }`
- HermesProxy: `ServerPetitionShowSignatures` — matches

### — (0x26c1)

- Modern: 9921 (0x26c1) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 6 · (2 unused)

### SMSG_CROSSED_INEBRIATION_THRESHOLD (0x26c2)

- Modern: 9922 (0x26c2) · 3.3.5a: 961 (0x3c1) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `CrossedInebriationThreshold` — matches

### — (0x26c4)

- Modern: 9924 (0x26c4) · 3.3.5a: — · Area: General
- Layout: `{ u8 { guid u32 u8*2 opt[ u8*4 bytes bytes bytes bytes bytes ] bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 7: 8 · 1 · (7 unused)

### SMSG_SELL_RESPONSE (0x26c5)

- Modern: 9925 (0x26c5) · 3.3.5a: 417 (0x1a1) · Area: General
- Layout: `{ guid u32*2 loop[ guid ] }`
- HermesProxy: `SellResponse` — matches

### SMSG_BUY_SUCCEEDED (0x26c6)

- Modern: 9926 (0x26c6) · 3.3.5a: 420 (0x1a4) · Area: General
- Layout: `{ guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `BuySucceeded` — matches

### SMSG_BUY_FAILED (0x26c7)

- Modern: 9927 (0x26c7) · 3.3.5a: 421 (0x1a5) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `BuyFailed` — matches

### SMSG_TOTEM_CREATED (0x26c8)

- Modern: 9928 (0x26c8) · 3.3.5a: 1043 (0x413) · Area: General
- Layout: `{ u8 guid u32*3 u8 }`
- Size: 14 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 15: 1 · (7 unused)
- HermesProxy: `TotemCreated` — matches

### — (0x26ca)

- Modern: 9930 (0x26ca) · 3.3.5a: — · Area: General
- Layout: `{ u8*2 guid }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8

### — (0x26cb)

- Modern: 9931 (0x26cb) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SHOW_TAXI_NODES (0x26cd)

- Modern: 9933 (0x26cd) · 3.3.5a: 425 (0x1a9) · Area: General
- Layout: `{ u8 u32*2 opt[ guid u32 ] loop[ u64 ] loop[ u64 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `ShowTaxiNodes` — differs

### SMSG_MINIMAP_PING (0x26ce)

- Modern: 9934 (0x26ce) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MinimapPing` — matches

### SMSG_FISH_NOT_HOOKED (0x26cf)

- Modern: 9935 (0x26cf) · 3.3.5a: 456 (0x1c8) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `FishNotHooked` — matches

### SMSG_FISH_ESCAPED (0x26d0)

- Modern: 9936 (0x26d0) · 3.3.5a: 457 (0x1c9) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `FishEscaped` — matches

### SMSG_HEALTH_UPDATE (0x26d1)

- Modern: 9937 (0x26d1) · 3.3.5a: 1151 (0x47f) · Area: General
- Layout: `{ guid u64 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `HealthUpdate` — matches

### SMSG_POWER_UPDATE (0x26d2)

- Modern: 9938 (0x26d2) · 3.3.5a: 1152 (0x480) · Area: General
- Layout: `{ guid u32 loop[ u32 u8 ] }`
- HermesProxy: `PowerUpdate` — matches

### SMSG_DEATH_RELEASE_LOC (0x26d3)

- Modern: 9939 (0x26d3) · 3.3.5a: 888 (0x378) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: u32, +4: 8 bytes, +12: u32
- HermesProxy: `DeathReleaseLoc` — matches

### — (0x26d4)

- Modern: 9940 (0x26d4) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYED_TIME (0x26d5)

- Modern: 9941 (0x26d5) · 3.3.5a: 461 (0x1cd) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `PlayedTime` — matches

### SMSG_TITLE_EARNED (0x26d7)

- Modern: 9943 (0x26d7) · 3.3.5a: 883 (0x373) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x26d8)

- Modern: 9944 (0x26d8) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_HIGHEST_THREAT_UPDATE (0x26d9)

- Modern: 9945 (0x26d9) · 3.3.5a: 1154 (0x482) · Area: General
- Layout: `{ guid guid u32 loop[ guid u64 ] }`
- HermesProxy: `HighestThreatUpdate` — matches

### SMSG_THREAT_UPDATE (0x26da)

- Modern: 9946 (0x26da) · 3.3.5a: 1155 (0x483) · Area: General
- Layout: `{ guid u32 loop[ guid u64 ] }`
- HermesProxy: `ThreatUpdate` — matches

### SMSG_THREAT_REMOVE (0x26db)

- Modern: 9947 (0x26db) · 3.3.5a: 1156 (0x484) · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ThreatRemove` — matches

### SMSG_THREAT_CLEAR (0x26dc)

- Modern: 9948 (0x26dc) · 3.3.5a: 1157 (0x485) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ThreatClear` — matches

### — (0x26dd)

- Modern: 9949 (0x26dd) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CANCEL_AUTO_REPEAT (0x26de)

- Modern: 9950 (0x26de) · 3.3.5a: 668 (0x29c) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CancelAutoRepeat` — matches

### SMSG_TRAINER_LIST (0x26df)

- Modern: 9951 (0x26df) · 3.3.5a: 433 (0x1b1) · Area: General
- Layout: `{ { guid u32*3 loop[ u32*4 loop[ u32 ] u8*2 ] u8*2 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · (5 unused)
- HermesProxy: `TrainerList` — matches

### SMSG_TRAINER_BUY_FAILED (0x26e0)

- Modern: 9952 (0x26e0) · 3.3.5a: 436 (0x1b4) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `TrainerBuyFailed` — matches

### SMSG_CRITERIA_UPDATE (0x26e1)

- Modern: 9953 (0x26e1) · 3.3.5a: 1130 (0x46a) · Area: General
- Layout: `{ u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 42: 1 · (7 unused)
- HermesProxy: `CriteriaUpdatePkt` — matches

### — (0x26e2)

- Modern: 9954 (0x26e2) · 3.3.5a: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x26e3)

- Modern: 9955 (0x26e3) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 u32 loop[ u32*2 ] u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 7: 6 · (2 unused)

### SMSG_QUERY_TIME_RESPONSE (0x26e4)

- Modern: 9956 (0x26e4) · 3.3.5a: 463 (0x1cf) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes
- HermesProxy: `QueryTimeResponse` — matches

### SMSG_LOG_XP_GAIN (0x26e5)

- Modern: 9957 (0x26e5) · 3.3.5a: 464 (0x1d0) · Area: General
- Layout: `{ guid u32 u8 u32*2 }`
- Size: 13 bytes (packed GUIDs not counted)
- HermesProxy: `LogXPGain` — differs (#369)

### SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA (0x26e6)

- Modern: 9958 (0x26e6) · 3.3.5a: 1181 (0x49d) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `OnCancelExpectedRideVehicleAura` — matches

### SMSG_CRITERIA_DELETED (0x26e7)

- Modern: 9959 (0x26e7) · 3.3.5a: 1182 (0x49e) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `CriteriaDeletedPkt` — matches

### SMSG_ACHIEVEMENT_DELETED (0x26e8)

- Modern: 9960 (0x26e8) · 3.3.5a: 1183 (0x49f) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_LEVEL_UP_INFO (0x26e9)

- Modern: 9961 (0x26e9) · 3.3.5a: 468 (0x1d4) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (76 bytes): +0: u32, +4: u32, +8: u32, +48: u32, +52: u32, +56: u32, +60: u32, +64: u32, +68: u32, +72: u32
- HermesProxy: `LevelUpInfo` — matches

### — (0x26eb)

- Modern: 9963 (0x26eb) · 3.3.5a: — · Area: General
- Layout: `{ guid { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused) · 6 · (2 unused); byte 28: 1 · (7 unused) · 6 · (2 unused)

### SMSG_AUCTION_HELLO_RESPONSE (0x26ee)

- Modern: 9966 (0x26ee) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)
- HermesProxy: `AuctionHelloResponse` — matches

### — (0x26ef)

- Modern: 9967 (0x26ef) · 3.3.5a: — · Area: General
- Layout: `{ u32*6 loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 loop[ u32*3 u8 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] }`

### SMSG_AUCTION_COMMAND_RESULT (0x26f0)

- Modern: 9968 (0x26f0) · 3.3.5a: 603 (0x25b) · Area: General
- Layout: `{ u32*4 guid u64*2 u32 }`
- Size: 36 bytes (packed GUIDs not counted)
- HermesProxy: `AuctionCommandResult` — matches

### SMSG_AUCTION_WON_NOTIFICATION (0x26f1)

- Modern: 9969 (0x26f1) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 guid { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `AuctionWonNotification` — matches

### SMSG_AUCTION_OUTBID_NOTIFICATION (0x26f2)

- Modern: 9970 (0x26f2) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 guid { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u64*2 }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `AuctionOutbidNotification` — matches

### SMSG_AUCTION_CLOSED_NOTIFICATION (0x26f3)

- Modern: 9971 (0x26f3) · 3.3.5a: — · Area: General
- Layout: `{ u32 u64 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused) · 6 · (2 unused); byte 30: 1 · (7 unused)
- HermesProxy: `AuctionClosedNotification` — matches

### SMSG_AUCTION_OWNER_BID_NOTIFICATION (0x26f4)

- Modern: 9972 (0x26f4) · 3.3.5a: — · Area: General
- Layout: `{ u32 u64 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u64 guid }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `AuctionOwnerBidNotification` — matches

### SMSG_AUCTION_REMOVED_NOTIFICATION (0x26f5)

- Modern: 9973 (0x26f5) · 3.3.5a: 653 (0x28d) · Area: General
- Layout: `{ u32*2 loop[ { u64 u32 u64 u32 u64 u32*4 opt[ alt[ guid ] alt[ u32 ] ] u8*3 loop[ { u8 u64 u32*4 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] loop[ u32*3 u8 ] } ] bytes bytes } ] }`

### SMSG_SET_VEHICLE_REC_ID (0x26f7)

- Modern: 9975 (0x26f7) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_PENDING_RAID_LOCK (0x26f8)

- Modern: 9976 (0x26f8) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)
- HermesProxy: `PendingRaidLock` — matches

### SMSG_DESTRUCTIBLE_BUILDING_DAMAGE (0x26f9)

- Modern: 9977 (0x26f9) · 3.3.5a: 50 (0x32) · Area: General
- Layout: `{ guid guid guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `DestructibleBuildingDamage` — matches

### — (0x26fa)

- Modern: 9978 (0x26fa) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x26fc)

- Modern: 9980 (0x26fc) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2700)

- Modern: 9984 (0x2700) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_CREATE_CHAR (0x2701)

- Modern: 9985 (0x2701) · 3.3.5a: 58 (0x3a) · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `CreateChar` — matches

### SMSG_DELETE_CHAR (0x2702)

- Modern: 9986 (0x2702) · 3.3.5a: 60 (0x3c) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `DeleteChar` — matches

### SMSG_TRANSFER_ABORTED (0x2703)

- Modern: 9987 (0x2703) · 3.3.5a: 64 (0x40) · Area: General
- Layout: `{ u32 u8 u32 u8 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 9: 6 · (2 unused)
- HermesProxy: `TransferAborted` — matches

### SMSG_PET_GUIDS (0x2704)

- Modern: 9988 (0x2704) · 3.3.5a: 1194 (0x4aa) · Area: General
- Layout: `{ u32 loop[ guid ] }`
- HermesProxy: `PetGuids` — matches

### SMSG_CHARACTER_LOGIN_FAILED (0x2705)

- Modern: 9989 (0x2705) · 3.3.5a: 65 (0x41) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `CharacterLoginFailed`; `CharacterLoginFailed`; `WorldSocket` — matches

### — (0x2706)

- Modern: 9990 (0x2706) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### — (0x2707)

- Modern: 9991 (0x2707) · 3.3.5a: — · Area: General
- Layout: `{ u64 u32 loop[ { u32*5 loop[ u32 { u32 u16 u8 } u64 u32 loop[ guid u32 loop[ guid { u32 u16 u8 } ] ] ] } ] }`

### — (0x2708)

- Modern: 9992 (0x2708) · 3.3.5a: — · Area: General
- Layout: `{ u32 { u32 u16 u8 } u64 u32 alt[ ] u8 loop[ { guid u8 u32 u16*2 u32*4 u8*2 u32 u32 u32 loop[ { u32*3 u8 } ] loop[ u32*2 ] loop[ { u32*6 u8 opt[ u32 ] opt[ u32 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 23: 1 · (7 unused)

### SMSG_UPDATE_ACCOUNT_DATA (0x2709)

- Modern: 9993 (0x2709) · 3.3.5a: 524 (0x20c) · Area: General
- Layout: `{ guid u64 u32 u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 4 · (4 unused)
- HermesProxy: `UpdateAccountData` — matches

### SMSG_ACCOUNT_DATA_TIMES (0x270a)

- Modern: 9994 (0x270a) · 3.3.5a: 521 (0x209) · Area: General
- Layout: `{ guid u64 loop[ u64 ] }`
- HermesProxy: `AccountDataTimes` — matches

### — (0x270b)

- Modern: 9995 (0x270b) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### — (0x270c)

- Modern: 9996 (0x270c) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_LOGIN_SET_TIME_SPEED (0x270d)

- Modern: 9997 (0x270d) · 3.3.5a: 66 (0x42) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32, +8: f32
- HermesProxy: `LoginSetTimeSpeed` — matches

### SMSG_LOAD_EQUIPMENT_SET (0x270e)

- Modern: 9998 (0x270e) · 3.3.5a: 1212 (0x4bc) · Area: General
- Layout: `{ u32 loop[ { u32 u64 u32*2 loop[ guid u32 ] u32*6 u8*3 opt[ u32 ] bytes bytes } ] }`
- HermesProxy: `LoadEquipmentSet`; `EmptyEquipmentSetList` — matches

### SMSG_START_MIRROR_TIMER (0x270f)

- Modern: 9999 (0x270f) · 3.3.5a: 473 (0x1d9) · Area: General
- Layout: `{ u32*5 u8 }`
- Size: 21 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)
- HermesProxy: `StartMirrorTimer` — matches

### SMSG_PAUSE_MIRROR_TIMER (0x2710)

- Modern: 10000 (0x2710) · 3.3.5a: 474 (0x1da) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `PauseMirrorTimer` — matches

### SMSG_STOP_MIRROR_TIMER (0x2711)

- Modern: 10001 (0x2711) · 3.3.5a: 475 (0x1db) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `StopMirrorTimer` — matches

### — (0x2712)

- Modern: 10002 (0x2712) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*4 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_ENCHANTMENT_LOG (0x2713)

- Modern: 10003 (0x2713) · 3.3.5a: 471 (0x1d7) · Area: General
- Layout: `{ guid guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `EnchantmentLog` — matches

### SMSG_SERVER_TIME_OFFSET (0x2714)

- Modern: 10004 (0x2714) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes
- HermesProxy: `ServerTimeOffset` — matches

### — (0x2716)

- Modern: 10006 (0x2716) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x2717)

- Modern: 10007 (0x2717) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2718)

- Modern: 10008 (0x2718) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2719)

- Modern: 10009 (0x2719) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x271a)

- Modern: 10010 (0x271a) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_STAND_STATE_UPDATE (0x271c)

- Modern: 10012 (0x271c) · 3.3.5a: 669 (0x29d) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8
- HermesProxy: `StandStateUpdate` — matches

### SMSG_SET_FORCED_REACTIONS (0x271d)

- Modern: 10013 (0x271d) · 3.3.5a: 677 (0x2a5) · Area: General
- Layout: `{ u32 loop[ u32*2 ] }`
- HermesProxy: `SetForcedReactions` — matches

### SMSG_GAME_OBJECT_RESET_STATE (0x271e)

- Modern: 10014 (0x271e) · 3.3.5a: 679 (0x2a7) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GameObjectResetState` — matches

### SMSG_SUMMON_REQUEST (0x2721)

- Modern: 10017 (0x2721) · 3.3.5a: 683 (0x2ab) · Area: General
- Layout: `{ guid u32*2 u8*2 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 1 · (7 unused)
- HermesProxy: `SummonRequest` — matches

### SMSG_INSPECT_PVP (0x2722)

- Modern: 10018 (0x2722) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 u8 loop[ { u8 u32*17 u8 } ] loop[ guid u32*5 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 2 · (6 unused)
- HermesProxy: `InspectPvP` — differs (#363)

### — (0x2723)

- Modern: 10019 (0x2723) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_INITIALIZE_FACTIONS (0x2724)

- Modern: 10020 (0x2724) · 3.3.5a: 290 (0x122) · Area: General
- Layout: `{ loop[ u16 u32 ] loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2658: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused)
- HermesProxy: `InitializeFactions` — matches; see #361

### — (0x2725)

- Modern: 10021 (0x2725) · 3.3.5a: — · Area: General
- Layout: `{ loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused)

### — (0x2726)

- Modern: 10022 (0x2726) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_SOCKET_GEMS_SUCCESS (0x2727)

- Modern: 10023 (0x2727) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SocketGemsSuccess` — matches

### — (0x2728)

- Modern: 10024 (0x2728) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_SET_FACTION_VISIBLE (0x272a)

- Modern: 10026 (0x272a) · 3.3.5a: 291 (0x123) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x272b)

- Modern: 10027 (0x272b) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SET_FACTION_STANDING (0x272c)

- Modern: 10028 (0x272c) · 3.3.5a: 292 (0x124) · Area: General
- Layout: `{ u32*2 loop[ u32*2 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `SetFactionStanding` — matches

### — (0x2730)

- Modern: 10032 (0x2730) · 3.3.5a: — · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### — (0x2731)

- Modern: 10033 (0x2731) · 3.3.5a: — · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### — (0x2732)

- Modern: 10034 (0x2732) · 3.3.5a: — · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### — (0x2733)

- Modern: 10035 (0x2733) · 3.3.5a: — · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### — (0x2734)

- Modern: 10036 (0x2734) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 3 · (5 unused)

### SMSG_SET_PROFICIENCY (0x2735)

- Modern: 10037 (0x2735) · 3.3.5a: 295 (0x127) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8
- HermesProxy: `SetProficiency` — matches

### SMSG_COOLDOWN_CHEAT (0x2739)

- Modern: 10041 (0x2739) · 3.3.5a: 481 (0x1e1) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `CooldownCheat` — differs (#369)

### SMSG_AREA_SPIRIT_HEALER_TIME (0x2740)

- Modern: 10048 (0x2740) · 3.3.5a: 740 (0x2e4) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `AreaSpiritHealerTime` — matches

### SMSG_LOOT_LIST (0x2741)

- Modern: 10049 (0x2741) · 3.3.5a: 1017 (0x3f9) · Area: General
- Layout: `{ guid guid u8 opt[ guid ] opt[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)
- HermesProxy: `LootList` — matches

### — (0x2742)

- Modern: 10050 (0x2742) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x2744)

- Modern: 10052 (0x2744) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_DURABILITY_DAMAGE_DEATH (0x2745)

- Modern: 10053 (0x2745) · 3.3.5a: 701 (0x2bd) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `DurabilityDamageDeath` — matches

### SMSG_INIT_WORLD_STATES (0x2746)

- Modern: 10054 (0x2746) · 3.3.5a: 706 (0x2c2) · Area: General
- Layout: `{ u32*4 loop[ u32*2 ] }`
- HermesProxy: `InitWorldStates`; `EmptyInitWorldStates` — matches

### SMSG_UPDATE_WORLD_STATE (0x2748)

- Modern: 10056 (0x2748) · 3.3.5a: 707 (0x2c3) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `UpdateWorldState` — matches

### SMSG_PET_ACTION_FEEDBACK (0x2749)

- Modern: 10057 (0x2749) · 3.3.5a: 710 (0x2c6) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_CORPSE_RECLAIM_DELAY (0x274a)

- Modern: 10058 (0x274a) · 3.3.5a: 617 (0x269) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `CorpseReclaimDelay` — matches

### — (0x274b)

- Modern: 10059 (0x274b) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_PETITION_SIGN_RESULTS (0x274c)

- Modern: 10060 (0x274c) · 3.3.5a: 449 (0x1c1) · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 4 · (4 unused)
- HermesProxy: `PetitionSignResults` — matches

### SMSG_TURN_IN_PETITION_RESULT (0x274e)

- Modern: 10062 (0x274e) · 3.3.5a: 453 (0x1c5) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)
- HermesProxy: `TurnInPetitionResult` — matches

### SMSG_USE_EQUIPMENT_SET_RESULT (0x274f)

- Modern: 10063 (0x274f) · 3.3.5a: 1238 (0x4d6) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (9 bytes): +0: 8 bytes, +8: u8
- HermesProxy: `UseEquipmentSetResult` — matches

### — (0x2751)

- Modern: 10065 (0x2751) · 3.3.5a: — · Area: General
- Layout: `{ guid u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 9 · (7 unused)

### — (0x2753)

- Modern: 10067 (0x2753) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x2754)

- Modern: 10068 (0x2754) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_ITEM_ENCHANT_TIME_UPDATE (0x2755)

- Modern: 10069 (0x2755) · 3.3.5a: 491 (0x1eb) · Area: General
- Layout: `{ guid u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `ItemEnchantTimeUpdate` — matches

### SMSG_MAIL_LIST_RESULT (0x2756)

- Modern: 10070 (0x2756) · 3.3.5a: 571 (0x23b) · Area: General
- Layout: `{ u32*2 loop[ { u64 u32 u64 u32 u64 u32*4 opt[ alt[ guid ] alt[ u32 ] ] u8*3 loop[ { u8 u64 u32*4 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] loop[ u32*3 u8 ] } ] bytes bytes } ] }`
- HermesProxy: `MailListResult` — matches

### SMSG_MAIL_QUERY_NEXT_TIME_RESULT (0x2757)

- Modern: 10071 (0x2757) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 loop[ guid u32*2 u8 u32 ] }`
- HermesProxy: `MailQueryNextTimeResult` — matches

### SMSG_PARTY_MEMBER_PARTIAL_STATE (0x2758)

- Modern: 10072 (0x2758) · 3.3.5a: 126 (0x7e) · Area: General
- Layout: `{ { u8*3 opt[ { u8 opt[ u8 bytes ] opt[ guid ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ { u32 loop[ { u32 u16 u32*2 loop[ u32 ] } ] } ] } ] guid loop[ u8 ] opt[ u16 ] opt[ u8 ] opt[ u16 ] opt[ u32 ] opt[ u32 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u32 ] opt[ { u16*3 } ] opt[ u32 ] opt[ { u32 loop[ { u32 u16 u32*2 loop[ u32 ] } ] } ] opt[ { u32*2 guid loop[ u16*2 ] } ] opt[ { u32*3 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `PartyMemberPartialState` — matches

### SMSG_PARTY_MEMBER_FULL_STATE (0x2759)

- Modern: 10073 (0x2759) · 3.3.5a: 754 (0x2f2) · Area: General
- Layout: `{ u8 { loop[ u8 ] u16 u8 u16 u32*2 u16*6 u32 u16*3 u32*2 { u32*2 guid loop[ u16*2 ] } u32*3 loop[ { u32 u16 u32*2 loop[ u32 ] } ] u8 opt[ guid u32*4 loop[ { u32 u16 u32*2 loop[ u32 ] } ] u8 bytes ] } guid }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused) · 8 · 8; byte 68: 1 · (7 unused)
- HermesProxy: `PartyMemberFullState` — matches

### SMSG_PARTY_KILL_LOG (0x275a)

- Modern: 10074 (0x275a) · 3.3.5a: 501 (0x1f5) · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PartyKillLog` — matches

### — (0x275b)

- Modern: 10075 (0x275b) · 3.3.5a: — · Area: General
- Layout: `{ guid guid u32 u8 opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)

### — (0x275c)

- Modern: 10076 (0x275c) · 3.3.5a: — · Area: General
- Layout: `{ u32 guid guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x275d)

- Modern: 10077 (0x275d) · 3.3.5a: — · Area: General
- Layout: `{ u32*3 alt[ ] loop[ { guid u32*2 u8*4 u32 bytes loop[ u32*2 ] loop[ { guid u8 u32*2 loop[ u32 ] { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u32*3 u8 ] loop[ u32 u8 ] loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] } ] } ] }`

### — (0x275e)

- Modern: 10078 (0x275e) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_EXPLORATION_EXPERIENCE (0x275f)

- Modern: 10079 (0x275f) · 3.3.5a: 504 (0x1f8) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32
- HermesProxy: `ExplorationExperience` — matches

### SMSG_ARENA_TEAM_ROSTER (0x2760)

- Modern: 10080 (0x2760) · 3.3.5a: 846 (0x34e) · Area: General
- Layout: `{ { u32*9 u8 loop[ guid u8 u32 u8*2 u32*5 u8 bytes opt[ u32 ] opt[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 36: 1 · (7 unused)
- HermesProxy: `ArenaTeamRosterResponse` — matches

### SMSG_ARENA_TEAM_INVITE (0x2761)

- Modern: 10081 (0x2761) · 3.3.5a: 848 (0x350) · Area: General
- Layout: `{ guid u32 guid u8*2 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 6 · 7 · (3 unused)
- HermesProxy: `ArenaTeamInvite` — matches

### SMSG_ARENA_TEAM_EVENT (0x2762)

- Modern: 10082 (0x2762) · 3.3.5a: 855 (0x357) · Area: General
- Layout: `{ u8 loop[ opt[ u8 ] alt[ u8 ] opt[ u8 ] ] bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 9 · 9 · 9 · (5 unused)
- HermesProxy: `ArenaTeamEvent` — matches

### SMSG_ARENA_TEAM_COMMAND_RESULT (0x2763)

- Modern: 10083 (0x2763) · 3.3.5a: 841 (0x349) · Area: General
- Layout: `{ u8*4 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8 · 7 · 6 · (3 unused)
- HermesProxy: `ArenaTeamCommandResult` — matches

### SMSG_ARENA_TEAM_STATS (0x2764)

- Modern: 10084 (0x2764) · 3.3.5a: 859 (0x35b) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (28 bytes): +0: u32, +4: u32, +8: u32, +12: u32, +16: u32, +20: u32, +24: u32

### SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT (0x2765)

- Modern: 10085 (0x2765) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u8 loop[ { guid guid u32 u8*4 u64 u32 u8*2 bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `GetAccountCharacterListResult` — matches

### — (0x2766)

- Modern: 10086 (0x2766) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u8 loop[ { guid guid u32 u8*4 u64 u32 u8*2 bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_CHARACTER_RENAME_RESULT (0x2767)

- Modern: 10087 (0x2767) · 3.3.5a: 712 (0x2c8) · Area: General
- Layout: `{ u8*2 opt[ guid ] bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 1 · 6 · (1 unused)
- HermesProxy: `CharacterRenameResult` — matches

### — (0x2768)

- Modern: 10088 (0x2768) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)

### — (0x2769)

- Modern: 10089 (0x2769) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### — (0x276a)

- Modern: 10090 (0x276a) · 3.3.5a: — · Area: General
- Layout: `{ u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_PRE_RESSURECT (0x276b)

- Modern: 10091 (0x276b) · 3.3.5a: 1172 (0x494) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_PLAY_SOUND (0x276c)

- Modern: 10092 (0x276c) · 3.3.5a: 722 (0x2d2) · Area: General
- Layout: `{ u32 guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `PlaySound` — matches

### SMSG_PLAY_MUSIC (0x276d)

- Modern: 10093 (0x276d) · 3.3.5a: 631 (0x277) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `PlayMusic` — matches

### SMSG_PLAY_OBJECT_SOUND (0x276e)

- Modern: 10094 (0x276e) · 3.3.5a: 632 (0x278) · Area: General
- Layout: `{ u32 guid guid u32*4 }`
- Size: 20 bytes (packed GUIDs not counted)
- HermesProxy: `PlayObjectSound` — matches

### — (0x276f)

- Modern: 10095 (0x276f) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2770)

- Modern: 10096 (0x2770) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x2771)

- Modern: 10097 (0x2771) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2772)

- Modern: 10098 (0x2772) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2774)

- Modern: 10100 (0x2774) · 3.3.5a: — · Area: General
- Layout: `{ guid { u32*8 loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u16 ] loop[ u16 ] } }`

### — (0x2775)

- Modern: 10101 (0x2775) · 3.3.5a: — · Area: General
- Layout: `{ { u32*3 alt[ ] u32*2 alt[ ] u32 loop[ { u32 u64*2 u32*5 loop[ u32 ] loop[ u32 ] u8 opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] loop[ { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] loop[ u32*2 u8 u32*3 u8 { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] } bytes { opt[ bytes ] } ] loop[ u32*5 u8*2 opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] } }`

### — (0x2776)

- Modern: 10102 (0x2776) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 loop[ { u64 u32*3 u64*3 u8 bytes } ] }`

### — (0x2777)

- Modern: 10103 (0x2777) · 3.3.5a: — · Area: General
- Layout: `{ { u32 u8*2 alt[ ] loop[ { u64 u32*2 guid guid u32*2 u64 u32 u8 opt[ { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 11 · (5 unused)

### — (0x2778)

- Modern: 10104 (0x2778) · 3.3.5a: — · Area: General
- Layout: `{ u32 guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x2779)

- Modern: 10105 (0x2779) · 3.3.5a: — · Area: General
- Layout: `{ { u64 u32*2 guid guid u32*2 u64 u32 u8 opt[ { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 40: 1 · 1 · (6 unused)

### — (0x277a)

- Modern: 10106 (0x277a) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x277b)

- Modern: 10107 (0x277b) · 3.3.5a: — · Area: General
- Layout: `{ { u64 u32 alt[ ] loop[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] } }`

### — (0x277c)

- Modern: 10108 (0x277c) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x277d)

- Modern: 10109 (0x277d) · 3.3.5a: — · Area: General
- Layout: `{ u32 guid }`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x277e)

- Modern: 10110 (0x277e) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_INSTANCE_SAVE_CREATED (0x2780)

- Modern: 10112 (0x2780) · 3.3.5a: 715 (0x2cb) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `InstanceSaveCreated` — matches

### — (0x2781)

- Modern: 10113 (0x2781) · 3.3.5a: — · Area: General
- Layout: `{ u32*4 loop[ { guid u8 u32*3 u32*7 loop[ u32 ] loop[ u32 ] u32*2 loop[ u32 ] loop[ u32 ] loop[ guid u32 ] loop[ u32 ] loop[ { u32*5 loop[ u32 ] loop[ u32 ] } ] u8 opt[ u32 u32 loop[ u32 u16 u8 ] loop[ u32 u8*2 ] ] opt[ { u32*4 u32 loop[ u32 ] loop[ u32*2 ] loop[ u32*3 ] } ] } ] }`

### — (0x2782)

- Modern: 10114 (0x2782) · 3.3.5a: — · Area: General
- Layout: `{ u32*4 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)

### — (0x2783)

- Modern: 10115 (0x2783) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +8: u32, +12: u32

### — (0x2784)

- Modern: 10116 (0x2784) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +8: u32

### — (0x2786)

- Modern: 10118 (0x2786) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ { u64 u32*3 u64*3 u8 bytes } ] }`

### — (0x2787)

- Modern: 10119 (0x2787) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: 8 bytes, +8: u32

### — (0x2788)

- Modern: 10120 (0x2788) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +8: u32, +12: u32, +16: u32

### SMSG_SET_ALL_TASK_PROGRESS / SMSG_CONQUEST_FORMULA_CONSTANTS (0x2789)

- Modern: 10121 (0x2789) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: u32, +4: u32, +8: u32, +12: u32, +16: u32
- HermesProxy: `SetAllTaskProgress`; `ConquestFormulaConstants` — differs (#369)

### SMSG_CONTACT_LIST (0x278c)

- Modern: 10124 (0x278c) · 3.3.5a: 103 (0x67) · Area: General
- Layout: `{ { u32 u8 loop[ guid guid u32*3 u8 u32*3 u8*2 bytes ] } }`
- HermesProxy: `ContactList` — matches

### SMSG_FRIEND_STATUS (0x278d)

- Modern: 10125 (0x278d) · 3.3.5a: 104 (0x68) · Area: General
- Layout: `{ u8 guid guid u32 u8 u32*3 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 10 · 1 · (5 unused)
- HermesProxy: `FriendStatusPkt` — matches

### — (0x278e)

- Modern: 10126 (0x278e) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x278f)

- Modern: 10127 (0x278f) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 9 · (7 unused)

### — (0x2790)

- Modern: 10128 (0x2790) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_GROUP_DECLINE (0x2791)

- Modern: 10129 (0x2791) · 3.3.5a: 116 (0x74) · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)
- HermesProxy: `GroupDecline` — matches

### — (0x2792)

- Modern: 10130 (0x2792) · 3.3.5a: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### SMSG_GROUP_UNINVITE (0x2793)

- Modern: 10131 (0x2793) · 3.3.5a: 119 (0x77) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GroupUninvite` — matches

### SMSG_GROUP_DESTROYED (0x2794)

- Modern: 10132 (0x2794) · 3.3.5a: 124 (0x7c) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GroupDestroyed` — matches

### — (0x2795)

- Modern: 10133 (0x2795) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PARTY_COMMAND_RESULT (0x2796)

- Modern: 10134 (0x2796) · 3.3.5a: 127 (0x7f) · Area: General
- Layout: `{ u8*3 u32 guid bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · 4 · 6 · (5 unused)
- HermesProxy: `PartyCommandResult` — matches

### — (0x2797)

- Modern: 10135 (0x2797) · 3.3.5a: — · Area: General
- Layout: `{ u8*3 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · 9 · (6 unused)

### SMSG_GOSSIP_POI (0x2798)

- Modern: 10136 (0x2798) · 3.3.5a: 548 (0x224) · Area: General
- Layout: `{ u32*8 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 32: 6 · (2 unused)
- HermesProxy: `GossipPOI` — matches

### SMSG_READ_ITEM_RESULT_OK (0x27a1)

- Modern: 10145 (0x27a1) · 3.3.5a: 174 (0xae) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ReadItemResultOK` — matches

### SMSG_READ_ITEM_RESULT_FAILED (0x27a9)

- Modern: 10153 (0x27a9) · 3.3.5a: 175 (0xaf) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 2 · (6 unused)
- HermesProxy: `ReadItemResultFailed` — matches

### — (0x27aa)

- Modern: 10154 (0x27aa) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 2 · (6 unused)

### — (0x27ac)

- Modern: 10156 (0x27ac) · 3.3.5a: — · Area: General
- Layout: `{ u8 guid u8 opt[ u8*3 u32 bytes loop[ u32*2 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 3: 1 · (7 unused)

### SMSG_RAID_DIFFICULTY_SET (0x27ad)

- Modern: 10157 (0x27ad) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8
- HermesProxy: `RaidDifficultySet` — matches

### — (0x27ae)

- Modern: 10158 (0x27ae) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_RAID_GROUP_ONLY (0x27af)

- Modern: 10159 (0x27af) · 3.3.5a: 646 (0x286) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32
- HermesProxy: `RaidGroupOnly` — matches

### — (0x27b0)

- Modern: 10160 (0x27b0) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x27b1)

- Modern: 10161 (0x27b1) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x27b2)

- Modern: 10162 (0x27b2) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x27b3)

- Modern: 10163 (0x27b3) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x27b4)

- Modern: 10164 (0x27b4) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x27b5)

- Modern: 10165 (0x27b5) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x27b6)

- Modern: 10166 (0x27b6) · 3.3.5a: — · Area: General
- Layout: `{ u32*4 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)

### — (0x27b7)

- Modern: 10167 (0x27b7) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x27b8)

- Modern: 10168 (0x27b8) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x27b9)

- Modern: 10169 (0x27b9) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### — (0x27ba)

- Modern: 10170 (0x27ba) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x27bb)

- Modern: 10171 (0x27bb) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x27bc)

- Modern: 10172 (0x27bc) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### — (0x27bd)

- Modern: 10173 (0x27bd) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_TUTORIAL_FLAGS (0x27be)

- Modern: 10174 (0x27be) · 3.3.5a: 253 (0xfd) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (32 bytes): +0: u32, +0: 8 bytes, +4: u32, +8: u32, +8: 8 bytes, +12: u32, +16: u32, +16: 8 bytes, +20: u32, +24: u32, +24: 8 bytes, +28: u32
- HermesProxy: `TutorialFlags` — matches

### — (0x27bf)

- Modern: 10175 (0x27bf) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x27c0)

- Modern: 10176 (0x27c0) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 loop[ u32 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### — (0x27c1)

- Modern: 10177 (0x27c1) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x27c2)

- Modern: 10178 (0x27c2) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x27c3)

- Modern: 10179 (0x27c3) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x27c4)

- Modern: 10180 (0x27c4) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 opt[ u32 ] opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · 1 · (5 unused)

### SMSG_ITEM_COOLDOWN (0x27c8)

- Modern: 10184 (0x27c8) · 3.3.5a: 176 (0xb0) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `ItemCooldown` — matches

### SMSG_EMOTE (0x27c9)

- Modern: 10185 (0x27c9) · 3.3.5a: 259 (0x103) · Area: General
- Layout: `{ guid u32 { u32*2 loop[ u32 ] } }`
- HermesProxy: `EmoteMessage` — matches

### SMSG_TRIGGER_CINEMATIC (0x27ca)

- Modern: 10186 (0x27ca) · 3.3.5a: 250 (0xfa) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `TriggerCinematic` — matches

### SMSG_UPDATE_OBJECT (0x27cb)

- Modern: 10187 (0x27cb) · 3.3.5a: 169 (0xa9) · Area: General
- Layout: `{ { u32 u16 u8 opt[ u16 u32 loop[ guid ] ] u32 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `UpdateObject`; `PetStableUpdate` — matches

### SMSG_COMPRESSED_UPDATE_OBJECT (0x27cc)

- Modern: 10188 (0x27cc) · 3.3.5a: 502 (0x1f6) · Area: General
- Layout: `{ u32 bytes u32 bytes }`

### SMSG_DESTROY_OBJECT (0x27cd)

- Modern: 10189 (0x27cd) · 3.3.5a: 170 (0xaa) · Area: General
- Layout: `{ u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x27ce)

- Modern: 10190 (0x27ce) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32*2 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x27d2)

- Modern: 10194 (0x27d2) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x27d3)

- Modern: 10195 (0x27d3) · 3.3.5a: — · Area: General
- Layout: `{ u32*4 loop[ u64 ] loop[ u64 ] }`

### — (0x27d4)

- Modern: 10196 (0x27d4) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 loop[ u64 ] loop[ u64 ] }`

### — (0x27d5)

- Modern: 10197 (0x27d5) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +12: u32, +16: u32

### — (0x27d6)

- Modern: 10198 (0x27d6) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +12: u32, +16: u32

### — (0x27d7)

- Modern: 10199 (0x27d7) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +4: u32

### — (0x27d8)

- Modern: 10200 (0x27d8) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x27d9)

- Modern: 10201 (0x27d9) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +12: u32

### — (0x27da)

- Modern: 10202 (0x27da) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (24 bytes): +0: 8 bytes, +12: u32, +16: u32, +20: u32

### — (0x27db)

- Modern: 10203 (0x27db) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +4: u32

### — (0x27dc)

- Modern: 10204 (0x27dc) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### — (0x27dd)

- Modern: 10205 (0x27dd) · 3.3.5a: — · Area: General
- Layout: `{ u32*3 u64*2 u32 u8 }`
- Size: 33 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 32: 1 · 1 · (6 unused)

### — (0x27de)

- Modern: 10206 (0x27de) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +4: u32, +8: u32

### — (0x27df)

- Modern: 10207 (0x27df) · 3.3.5a: — · Area: General
- Layout: `{ u32*3 loop[ u64*3 u32*2 ] }`

### — (0x27ee)

- Modern: 10222 (0x27ee) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x27f1)

- Modern: 10225 (0x27f1) · 3.3.5a: — · Area: General
- Layout: `{ u32*4 loop[ { guid guid u32 u8*4 u64 u32 u8*2 bytes bytes } ] }`

### — (0x27f2)

- Modern: 10226 (0x27f2) · 3.3.5a: — · Area: General
- Layout: `{ { u32*4 loop[ u32*6 u8*2 u32 u8*2 bytes ] } }`

### — (0x27f3)

- Modern: 10227 (0x27f3) · 3.3.5a: — · Area: General
- Layout: `{ u32 { guid u32*2 u64 u8 loop[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 2 · (6 unused)

### — (0x27f4)

- Modern: 10228 (0x27f4) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 guid guid u32 guid u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 6 · (2 unused)

### — (0x27f5)

- Modern: 10229 (0x27f5) · 3.3.5a: — · Area: General
- Layout: `{ u8 alt[ ] loop[ { guid u32*2 u64 u8 loop[ u32 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · (2 unused)

### — (0x27fc)

- Modern: 10236 (0x27fc) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2803)

- Modern: 10243 (0x2803) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 loop[ u32 u8 ] }`

### — (0x2804)

- Modern: 10244 (0x2804) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2806)

- Modern: 10246 (0x2806) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_BATTLENET_RESPONSE (0x2807)

- Modern: 10247 (0x2807) · 3.3.5a: — · Area: General
- Layout: `{ u32 { u64*2 u32 } u32 bytes }`
- HermesProxy: `BattlenetResponse` — matches

### SMSG_BATTLENET_NOTIFICATION (0x2808)

- Modern: 10248 (0x2808) · 3.3.5a: — · Area: General
- Layout: `{ u64*2 u32*2 bytes }`
- HermesProxy: `BattlenetNotification` — matches

### SMSG_BATTLE_NET_CONNECTION_STATUS (0x2809)

- Modern: 10249 (0x2809) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 2 · 1 · (5 unused)
- HermesProxy: `ConnectionStatus` — matches

### SMSG_CHANGE_REALM_TICKET_RESPONSE (0x280a)

- Modern: 10250 (0x280a) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `ChangeRealmTicketResponse` — differs (#369)

### — (0x2813)

- Modern: 10259 (0x2813) · 3.3.5a: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### — (0x2814)

- Modern: 10260 (0x2814) · 3.3.5a: — · Area: General
- Layout: `{ u64*2 guid guid guid u32*2 u8 loop[ { u8 opt[ opt[ opt[ u32*3 u8 u32 u8 ] { { guid u32*2 u64 u8 } u32 u8 guid guid guid guid guid u32*4 u32 u32 u32 u32 u64 u8 guid { loop[ u32 u8 ] } u32 loop[ guid ] loop[ guid ] loop[ guid ] { u32*3 u8 bits(2) opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] opt[ u8 ] } u8 { u32*3 loop[ u32*4 u8 ] } loop[ { guid u8*3 u32 u8*2 } ] } ] u32*3 loop[ u32 ] loop[ u32 ] loop[ u8 ] u8 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 30: 1 · 1 · (6 unused)

### — (0x2815)

- Modern: 10261 (0x2815) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### — (0x2816)

- Modern: 10262 (0x2816) · 3.3.5a: — · Area: General
- Layout: `{ { guid guid u32 u32 u32 loop[ u32 ] loop[ u32 ] loop[ u64 ] u64 u8 u32*3 u8*5 guid u32 u64 u32 u8 bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 37: 8 · 9 · 1 · 1 · 1 · (4 unused) · 9 · 1 · 1 · (5 unused)

### — (0x2817)

- Modern: 10263 (0x2817) · 3.3.5a: — · Area: General
- Layout: `{ u32 u64*2 u32 u8 }`
- Size: 25 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)

### — (0x2818)

- Modern: 10264 (0x2818) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u64*2 u8 }`
- Size: 25 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)

### — (0x2819)

- Modern: 10265 (0x2819) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 4

### — (0x281a)

- Modern: 10266 (0x281a) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: 8 bytes, +8: u32

### — (0x281c)

- Modern: 10268 (0x281c) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 guid u32 loop[ guid u8*2 bytes ] }`

### — (0x281d)

- Modern: 10269 (0x281d) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +8: u32, +12: u32

### — (0x281e)

- Modern: 10270 (0x281e) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 u64*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 7 · (1 unused)

### — (0x281f)

- Modern: 10271 (0x281f) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 u8*3 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 5: 1 · 5 · 1 · 1 · 4 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2820)

- Modern: 10272 (0x2820) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 u8 u64*2 guid u8*3 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 5 · 1 · 1 · 7 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2821)

- Modern: 10273 (0x2821) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x2822)

- Modern: 10274 (0x2822) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x2823)

- Modern: 10275 (0x2823) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x2824)

- Modern: 10276 (0x2824) · 3.3.5a: — · Area: General
- Layout: `u32*2 u64 bits(6) bits(7) opt[ u8 ] bytes bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 6 · 7 · 1 · (2 unused)

### — (0x2825)

- Modern: 10277 (0x2825) · 3.3.5a: — · Area: General
- Layout: `bits(7) loop[ u8 ] loop[ u8 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · (1 unused) · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### — (0x2826)

- Modern: 10278 (0x2826) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x2827)

- Modern: 10279 (0x2827) · 3.3.5a: — · Area: General
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x2828)

- Modern: 10280 (0x2828) · 3.3.5a: — · Area: General
- Layout: `{ u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · 1 · 1 · (5 unused)

### — (0x282a)

- Modern: 10282 (0x282a) · 3.3.5a: — · Area: General
- Layout: `guid u8 opt[ u8 opt[ bits(2) u32 ] ]`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### — (0x282b)

- Modern: 10283 (0x282b) · 3.3.5a: — · Area: General
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2846)

- Modern: 10310 (0x2846) · 3.3.5a: — · Area: General
- Layout: `{ u8 u32 loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2847)

- Modern: 10311 (0x2847) · 3.3.5a: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 12 · (4 unused)

### — (0x284a)

- Modern: 10314 (0x284a) · 3.3.5a: — · Area: General
- Layout: `{ { u64 u32 alt[ ] loop[ { u8*6 bytes bytes bytes bytes bytes guid guid guid u64 u32 u8*5 bytes } ] } }`

### — (0x285a)

- Modern: 10330 (0x285a) · 3.3.5a: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x285b)

- Modern: 10331 (0x285b) · 3.3.5a: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x285c)

- Modern: 10332 (0x285c) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_AUCTION_LIST_ITEMS_RESULT (0x2863)

- Modern: 10339 (0x2863) · 3.3.5a: 604 (0x25c) · Area: General
- Layout: `{ u32*3 u8 loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 loop[ u32*3 u8 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)
- HermesProxy: `AuctionListItemsResult` — differs (#367)

### — (0x2868)

- Modern: 10344 (0x2868) · 3.3.5a: — · Area: General
- Layout: `{ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 42: 1 · (7 unused)

### — (0x286b)

- Modern: 10347 (0x286b) · 3.3.5a: — · Area: General
- Layout: `{ u32 u32 loop[ { u32 u64*2 u32 u8 } ] loop[ { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] }`

### — (0x286c)

- Modern: 10348 (0x286c) · 3.3.5a: — · Area: General
- Layout: `{ u8 { u32 u64*2 u32 u8 } { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*3 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 3 · (5 unused); byte 25: 1 · (7 unused); byte 71: 8 · 1 · 1 · 7 · 1 · (6 unused)

### — (0x2877)

- Modern: 10359 (0x2877) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT (0x287c)

- Modern: 10364 (0x287c) · 3.3.5a: 605 (0x25d) · Area: General
- Layout: `{ u32*3 loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 loop[ u32*3 u8 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] }`

### SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT (0x287d)

- Modern: 10365 (0x287d) · 3.3.5a: 613 (0x265) · Area: General
- Layout: `{ u32*3 loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 loop[ u32*3 u8 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] }`

### SMSG_AREA_TRIGGER_MESSAGE (0x2880)

- Modern: 10368 (0x2880) · 3.3.5a: 696 (0x2b8) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32
- HermesProxy: `AreaTriggerMessage` — differs (#362)

### — (0x2882)

- Modern: 10370 (0x2882) · 3.3.5a: — · Area: General
- Layout: `{ u32 u8*2 { opt[ bytes ] } { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 5 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2883)

- Modern: 10371 (0x2883) · 3.3.5a: — · Area: General
- Layout: `{ u32 loop[ u64*2 ] }`

### — (0x2884)

- Modern: 10372 (0x2884) · 3.3.5a: — · Area: General
- Layout: `{ u32*2 loop[ { u32 u16 u8 } u64 u32 ] }`

### — (0x2886)

- Modern: 10374 (0x2886) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### — (0x2888)

- Modern: 10376 (0x2888) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### — (0x2889)

- Modern: 10377 (0x2889) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BINDER_CONFIRM / SMSG_SHOW_BANK / SMSG_SPIRIT_HEALER_CONFIRM / SMSG_PLAYER_TABARD_VENDOR_ACTIVATE (0x288a)

- Modern: 10378 (0x288a) · 3.3.5a: 747 (0x2eb), 440 (0x1b8), 546 (0x222) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `BinderConfirm`; `PlayerTabardVendorActivate`; `ShowBank`; `SpiritHealerConfirm` — matches

### — (0x288b)

- Modern: 10379 (0x288b) · 3.3.5a: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x288c)

- Modern: 10380 (0x288c) · 3.3.5a: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x288e)

- Modern: 10382 (0x288e) · 3.3.5a: — · Area: General
- Layout: `{ guid u32*3 guid guid u8 loop[ { u8 guid guid u8*2 u32 loop[ u32 ] bytes loop[ u32*2 ] } ] loop[ { u8 guid guid u8*2 u32 loop[ u32 ] bytes loop[ u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 2 · 1 · 1 · (4 unused)

### — (0x288f)

- Modern: 10383 (0x288f) · 3.3.5a: — · Area: General
- Layout: `{ u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · (2 unused)

### — (0x2890)

- Modern: 10384 (0x2890) · 3.3.5a: — · Area: General
- Layout: `{ guid u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 6 · (2 unused)

### SMSG_SOCIAL_CONTRACT_REQUEST_RESPONSE (0x2892)

- Modern: 10386 (0x2892) · 3.3.5a: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x28a7)

- Modern: 10407 (0x28a7) · 3.3.5a: — · Area: General
- Layout: `{ u32*4 bytes }`

### — (0x28ab)

- Modern: 10411 (0x28ab) · 3.3.5a: — · Area: General
- Layout: `{ loop[ u8*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### — (0x28ac)

- Modern: 10412 (0x28ac) · 3.3.5a: — · Area: General
- Layout: `{ u8 opt[ u8 { opt[ bytes ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
