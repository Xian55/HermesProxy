# General — server packet layouts, 1.14.0.40618

### SMSG_AUTH_FAILED (0x256c)

- Modern: 9580 (0x256c) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_AUTH_RESPONSE (0x256d)

- Modern: 9581 (0x256d) · 1.12.1: 494 (0x1ee) · Area: General
- Layout: `{ u32 u8 opt[ { u32*2 u32 u8*2 u32*4 u64 loop[ { u8 u32 loop[ u8*3 ] } ] u8 { u32*3 u8 } opt[ u16 ] opt[ u16 ] opt[ u64 ] loop[ { u32 u8*3 bytes bytes } ] loop[ u32*2 loop[ u8*2 ] u8*3 bytes bytes ] } ] opt[ { u32*2 u8 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)
- HermesProxy: `AuthResponse` — differs

### SMSG_WAIT_QUEUE_UPDATE (0x256e)

- Modern: 9582 (0x256e) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `WaitQueueUpdate` — matches

### SMSG_WAIT_QUEUE_FINISH (0x256f)

- Modern: 9583 (0x256f) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `WaitQueueFinish` — matches

### SMSG_ALL_ACHIEVEMENT_DATA (0x2570)

- Modern: 9584 (0x2570) · 1.12.1: — · Area: General
- Layout: `{ u32 u32 loop[ { u32 { u32 } guid u32*2 } ] loop[ { u32 u64 guid { u32 } u64*2 u8 opt[ u64 ] } ] }`
- HermesProxy: `AllAchievementData` — differs

### SMSG_ALL_ACCOUNT_CRITERIA (0x2571)

- Modern: 9585 (0x2571) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u32 u64 guid { u32 } u64*2 u8 opt[ u64 ] } ] }`
- HermesProxy: `AllAccountCriteria`; `EmptyAllAccountCriteria` — matches

### SMSG_RESPOND_INSPECT_ACHIEVEMENTS (0x2572)

- Modern: 9586 (0x2572) · 1.12.1: — · Area: General
- Layout: `{ guid u32 u32 loop[ { u32 { u32 } guid u32*2 } ] loop[ { u32 u64 guid { u32 } u64*2 u8 opt[ u64 ] } ] }`

### SMSG_SETUP_CURRENCY (0x2573)

- Modern: 9587 (0x2573) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u32*2 u8*2 opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] } ] }`
- HermesProxy: `SetupCurrency`; `EmptySetupCurrency` — matches

### SMSG_SET_CURRENCY (0x2574)

- Modern: 9588 (0x2574) · 1.12.1: — · Area: General
- Layout: `{ { u32*3 u8 opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_RESET_WEEKLY_CURRENCY (0x2575)

- Modern: 9589 (0x2575) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_MESSAGE_BOX (0x2576)

- Modern: 9590 (0x2576) · 1.12.1: — · Area: General
- Layout: `{ u32 bytes }`

### SMSG_WARDEN3_DATA (0x2577)

- Modern: 9591 (0x2577) · 1.12.1: — · Area: General
- Layout: `{ u32 bytes }`

### SMSG_PHASE_SHIFT_CHANGE (0x2578)

- Modern: 9592 (0x2578) · 1.12.1: — · Area: General
- Layout: `{ { guid { u32*2 guid loop[ u16*2 ] } u32 bytes u32 bytes u32 bytes } }`
- HermesProxy: `PhaseShiftChange` — matches

### SMSG_PRELOAD_CHILD_MAP (0x2579)

- Modern: 9593 (0x2579) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_UNLOAD_CHILD_MAP (0x257a)

- Modern: 9594 (0x257a) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_MOUNT_RESULT (0x257b)

- Modern: 9595 (0x257b) · 1.12.1: 366 (0x16e) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_DISMOUNT_RESULT (0x257c)

- Modern: 9596 (0x257c) · 1.12.1: 367 (0x16f) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BIND_POINT_UPDATE (0x257d)

- Modern: 9597 (0x257d) · 1.12.1: 341 (0x155) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: u8, +12: u32, +16: u32
- HermesProxy: `BindPointUpdate` — matches

### SMSG_RESURRECT_REQUEST (0x257e)

- Modern: 9598 (0x257e) · 1.12.1: 347 (0x15b) · Area: General
- Layout: `{ guid u32*3 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · 1 · 1 · (3 unused)
- HermesProxy: `ResurrectRequest` — matches

### SMSG_INITIAL_SETUP (0x2580)

- Modern: 9600 (0x2580) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (2 bytes): +0: u8, +1: u8
- HermesProxy: `InitialSetup` — matches

### SMSG_TRADE_UPDATED (0x2581)

- Modern: 9601 (0x2581) · 1.12.1: — · Area: General
- Layout: `{ { u8 u32*3 u64 u32*4 alt[ ] loop[ { u8 u32 guid { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u8 opt[ u32*2 guid u32*3 u8 loop[ { u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } } ] ] } ] } }`
- HermesProxy: `TradeUpdated` — matches

### SMSG_TRADE_STATUS (0x2582)

- Modern: 9602 (0x2582) · 1.12.1: 288 (0x120) · Area: General
- Layout: `{ u8 opt[ u32*2 ] opt[ u32 ] opt[ guid guid ] opt[ u8 ] opt[ u32*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 5 · (2 unused)
- HermesProxy: `TradeStatusPkt` — matches

### SMSG_ENUM_CHARACTERS_RESULT (0x2583)

- Modern: 9603 (0x2583) · 1.12.1: 59 (0x3b) · Area: General
- Layout: `{ { u8 u32 u32*2 u32 opt[ u32 ] loop[ { u32*2 } ] loop[ { guid u64 u8*4 u32 u8 u32*5 guid u32*6 loop[ u32 ] loop[ u32*3 u8*2 ] u64 u16 u32*6 loop[ { u32*2 } ] loop[ u32 ] u8*2 loop[ opt[ u8 ] alt[ u8 ] ] loop[ { opt[ bytes ] } ] bytes } ] loop[ u32 u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · (1 unused)
- HermesProxy: `EnumCharactersResult` — matches

### SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT (0x2585)

- Modern: 9605 (0x2585) · 1.12.1: — · Area: General
- Layout: `{ u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)
- HermesProxy: `GenerateRandomCharacterNameResult` — matches

### SMSG_SETUP_RESEARCH_HISTORY (0x2586)

- Modern: 9606 (0x2586) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ u64 u32*2 ] }`

### SMSG_RESEARCH_COMPLETE (0x2587)

- Modern: 9607 (0x2587) · 1.12.1: — · Area: General
- Layout: `{ u64 u32*2 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_ARCHAEOLOGY_SURVERY_CAST (0x2588)

- Modern: 9608 (0x2588) · 1.12.1: — · Area: General
- Layout: `{ u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_PET_NEWLY_TAMED (0x2589)

- Modern: 9609 (0x2589) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_PET_SLOT_UPDATED (0x258b)

- Modern: 9611 (0x258b) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_PET_MODE (0x258c)

- Modern: 9612 (0x258c) · 1.12.1: 378 (0x17a) · Area: General
- Layout: `{ guid u16 u8 }`
- Size: 3 bytes (packed GUIDs not counted)

### SMSG_DIFFERENT_INSTANCE_FROM_PARTY (0x258d)

- Modern: 9613 (0x258d) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_ROLE_CHANGED_INFORM (0x258e)

- Modern: 9614 (0x258e) · 1.12.1: — · Area: General
- Layout: `{ u8 guid guid u32*2 }`
- Size: 9 bytes (packed GUIDs not counted)
- HermesProxy: `RoleChangedInform` — differs

### SMSG_ROLE_POLL_INFORM (0x258f)

- Modern: 9615 (0x258f) · 1.12.1: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_SUMMON_RAID_MEMBER_VALIDATE_FAILED (0x2590)

- Modern: 9616 (0x2590) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ guid u32 ] }`

### SMSG_GROUP_ACTION_THROTTLED (0x2591)

- Modern: 9617 (0x2591) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_REQUEST_CEMETERY_LIST_RESPONSE (0x2592)

- Modern: 9618 (0x2592) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_CHECK_WARGAME_ENTRY (0x2593)

- Modern: 9619 (0x2593) · 1.12.1: — · Area: General
- Layout: `{ guid u64*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_PET_ADDED (0x2596)

- Modern: 9622 (0x2596) · 1.12.1: — · Area: General
- Layout: `{ u32*5 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 8 · 8

### SMSG_PET_STABLE_LIST (0x2597)

- Modern: 9623 (0x2597) · 1.12.1: — · Area: General
- Layout: `{ guid u32 u8 loop[ u32*4 u8*3 bytes ] }`
- HermesProxy: `PetStableList` — matches

### SMSG_PET_STABLE_RESULT (0x2598)

- Modern: 9624 (0x2598) · 1.12.1: 627 (0x273) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `PetStableResult` — matches

### SMSG_NEW_WORLD (0x2599)

- Modern: 9625 (0x2599) · 1.12.1: 62 (0x3e) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (36 bytes): +0: u32, +4: u32, +4: 8 bytes, +8: u32, +12: u32, +12: f32, +16: u32, +20: u32, +24: f32, +28: f32, +32: f32
- HermesProxy: `NewWorld` — matches

### SMSG_LOGIN_VERIFY_WORLD (0x259a)

- Modern: 9626 (0x259a) · 1.12.1: 566 (0x236) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (24 bytes): +0: u32, +4: u32, +4: 8 bytes, +8: u32, +12: u32, +12: f32, +16: u32, +20: u32
- HermesProxy: `LoginVerifyWorld` — matches

### SMSG_ABORT_NEW_WORLD (0x259b)

- Modern: 9627 (0x259b) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_NOTIFY_MONEY (0x259c)

- Modern: 9628 (0x259c) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes

### SMSG_ITEM_PURCHASE_REFUND_RESULT (0x259d)

- Modern: 9629 (0x259d) · 1.12.1: — · Area: General
- Layout: `{ guid u8*2 opt[ { u64 loop[ u32*2 ] loop[ u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 1 · (7 unused)

### SMSG_SET_ITEM_PURCHASE_DATA (0x259e)

- Modern: 9630 (0x259e) · 1.12.1: — · Area: General
- Layout: `{ guid { u64 loop[ u32*2 ] loop[ u32*2 ] } u32*2 }`

### SMSG_ITEM_EXPIRE_PURCHASE_REFUND (0x259f)

- Modern: 9631 (0x259f) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DISPLAY_GAME_ERROR (0x25a0)

- Modern: 9632 (0x25a0) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_SET_MAX_WEEKLY_QUANTITY (0x25a1)

- Modern: 9633 (0x25a1) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_PETITION_ALREADY_SIGNED (0x25a2)

- Modern: 9634 (0x25a2) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_RAID_MARKERS_CHANGED (0x25a3)

- Modern: 9635 (0x25a3) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 u8 loop[ { guid u32*4 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 5: 4 · (4 unused)

### SMSG_STREAMING_MOVIES (0x25a4)

- Modern: 9636 (0x25a4) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ u16 ] }`

### SMSG_START_TIMER (0x25a5)

- Modern: 9637 (0x25a5) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +8: 8 bytes, +16: u32

### SMSG_DISENCHANT_CREDIT (0x25a6)

- Modern: 9638 (0x25a6) · 1.12.1: — · Area: General
- Layout: `{ guid { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused) · 6 · (2 unused)

### SMSG_SUSPEND_TOKEN (0x25a7)

- Modern: 9639 (0x25a7) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)
- HermesProxy: `SuspendToken` — matches

### SMSG_RESUME_TOKEN (0x25a8)

- Modern: 9640 (0x25a8) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)
- HermesProxy: `ResumeToken` — matches

### SMSG_ADD_ITEM_PASSIVE (0x25a9)

- Modern: 9641 (0x25a9) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_REMOVE_ITEM_PASSIVE (0x25aa)

- Modern: 9642 (0x25aa) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SEND_ITEM_PASSIVES (0x25ab)

- Modern: 9643 (0x25ab) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ u32 ] }`

### SMSG_WORLD_SERVER_INFO (0x25ac)

- Modern: 9644 (0x25ac) · 1.12.1: — · Area: General
- Layout: `{ u32 u8*2 opt[ u32 ] opt[ u64 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 8 · 1 · 1 · 1 · 1 · (4 unused)
- HermesProxy: `WorldServerInfo` — matches

### SMSG_ACCOUNT_TOY_UPDATE (0x25ae)

- Modern: 9646 (0x25ae) · 1.12.1: — · Area: General
- Layout: `{ u8 u32*3 loop[ u32 ] loop[ u8 ] loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `AccountToyUpdate`; `EmptyAccountToyUpdate` — matches

### SMSG_RUNE_REGEN_DEBUG (0x25b3)

- Modern: 9651 (0x25b3) · 1.12.1: — · Area: General
- Layout: `{ u32*5 loop[ u32 ] loop[ u32 ] }`

### SMSG_VENDOR_INVENTORY (0x25b5)

- Modern: 9653 (0x25b5) · 1.12.1: 415 (0x19f) · Area: General
- Layout: `{ guid u8 u32 alt[ ] loop[ { u32*3 u64 u32*4 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u8 } ] }`
- HermesProxy: `VendorInventory` — matches

### SMSG_SET_PLAY_HOVER_ANIM (0x25b7)

- Modern: 9655 (0x25b7) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_CLEAR_BOSS_EMOTES (0x25b8)

- Modern: 9656 (0x25b8) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOAD_CUF_PROFILES (0x25b9)

- Modern: 9657 (0x25b9) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u8*4 u16*2 u8*5 u16*3 bytes } ] }`
- HermesProxy: `LoadCUFProfiles` — not checked

### SMSG_PARTY_INVITE (0x25ba)

- Modern: 9658 (0x25ba) · 1.12.1: 111 (0x6f) · Area: General
- Layout: `{ { u8*2 { u32 u8*3 bytes bytes } guid guid u16 u32*3 bytes loop[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 6 · (4 unused); byte 6: 1 · 1 · 8 · 8 · (6 unused)
- HermesProxy: `PartyInvite` — matches

### SMSG_FEATURE_SYSTEM_STATUS (0x25bc)

- Modern: 9660 (0x25bc) · 1.12.1: — · Area: General
- Layout: `{ { u8 u32*12 u64 u32*3 u8*5 { u8 u32*22 } opt[ u32*3 ] opt[ u32 loop[ u8 ] ] { u8 guid guid } opt[ { u8 u32*4 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 69: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (6 unused) · 1 · (7 unused); byte 163: 1 · (7 unused)
- HermesProxy: `FeatureSystemStatus` — matches

### SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN (0x25bd)

- Modern: 9661 (0x25bd) · 1.12.1: — · Area: General
- Layout: `{ { u8*3 opt[ { u8 u32*4 } ] u32*2 u64 u32*7 opt[ u32 ] loop[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (1 unused)
- HermesProxy: `FeatureSystemStatusGlueScreen` — differs

### SMSG_SEASON_INFO (0x25be)

- Modern: 9662 (0x25be) · 1.12.1: — · Area: General
- Layout: `{ { u32*5 u8 } }`
- Size: 21 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)
- HermesProxy: `SeasonInfo` — matches

### SMSG_GAME_OBJECT_ACTIVATE_ANIM_KIT (0x25bf)

- Modern: 9663 (0x25bf) · 1.12.1: — · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_GAME_OBJECT_CUSTOM_ANIM (0x25c0)

- Modern: 9664 (0x25c0) · 1.12.1: 179 (0xb3) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `GameObjectCustomAnim` — matches

### SMSG_GAME_OBJECT_DESPAWN (0x25c1)

- Modern: 9665 (0x25c1) · 1.12.1: 533 (0x215) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GameObjectDespawn` — matches

### SMSG_MAP_OBJ_EVENTS (0x25c2)

- Modern: 9666 (0x25c2) · 1.12.1: — · Area: General
- Layout: `{ u32*2 bytes }`

### SMSG_MISSILE_CANCEL (0x25c3)

- Modern: 9667 (0x25c3) · 1.12.1: — · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_XP_GAIN_ABORTED (0x25c4)

- Modern: 9668 (0x25c4) · 1.12.1: — · Area: General
- Layout: `{ guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_PRINT_NOTIFICATION (0x25c5)

- Modern: 9669 (0x25c5) · 1.12.1: 459 (0x1cb) · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 12 · (4 unused)
- HermesProxy: `PrintNotification` — matches

### SMSG_CUSTOM_LOAD_SCREEN (0x25c6)

- Modern: 9670 (0x25c6) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_SPELL_VISUAL_LOAD_SCREEN (0x25c7)

- Modern: 9671 (0x25c7) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_TRANSFER_PENDING (0x25c8)

- Modern: 9672 (0x25c8) · 1.12.1: 63 (0x3f) · Area: General
- Layout: `{ u32*4 u8 opt[ u32*2 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · 1 · (6 unused)
- HermesProxy: `TransferPending` — matches

### SMSG_ADJUST_SPLINE_DURATION (0x25cb)

- Modern: 9675 (0x25cb) · 1.12.1: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_LEARN_TALENT_FAILED (0x25cc)

- Modern: 9676 (0x25cc) · 1.12.1: — · Area: General
- Layout: `{ u8 u32*2 loop[ u16 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_LEARN_PVP_TALENT_FAILED (0x25cd)

- Modern: 9677 (0x25cd) · 1.12.1: — · Area: General
- Layout: `{ u8 u32*2 loop[ { u16 u8 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_UPDATE_TALENT_DATA (0x25cf)

- Modern: 9679 (0x25cf) · 1.12.1: — · Area: General
- Layout: `{ { u8 u32*2 loop[ u32*3 loop[ u16 ] loop[ u16 u8 ] ] } }`
- HermesProxy: `EmptyTalentData`; `UpdateTalentData` — differs

### SMSG_UPDATE_PRIMARY_SPEC (0x25d0)

- Modern: 9680 (0x25d0) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (2 bytes): +0: u16

### SMSG_SHOW_NEUTRAL_PLAYER_FACTION_SELECT_UI (0x25d3)

- Modern: 9683 (0x25d3) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_NEUTRAL_PLAYER_FACTION_SELECT_RESULT (0x25d4)

- Modern: 9684 (0x25d4) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x25d5)

- Modern: 9685 (0x25d5) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_SET_CHR_UPGRADE_TIER (0x25d7)

- Modern: 9687 (0x25d7) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_DONT_AUTO_PUSH_SPELLS_TO_ACTION_BAR (0x25d9)

- Modern: 9689 (0x25d9) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_SCENE_OBJECT_EVENT (0x25da)

- Modern: 9690 (0x25da) · 1.12.1: — · Area: General
- Layout: `{ guid { u8 bytes } }`

### SMSG_SCENE_OBJECT_PET_BATTLE_INITIAL_UPDATE (0x25db)

- Modern: 9691 (0x25db) · 1.12.1: — · Area: General
- Layout: `{ guid { loop[ guid u32*2 u16 u8*3 loop[ { guid u32*3 u16*2 u32*5 u16*2 u8 u32 u32 u32 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] u8 bytes } ] ] loop[ u32 u32 loop[ { u32*4 u8 } ] loop[ u32*2 ] opt[ u16*2 u32*3 u8*2 guid u8 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 8 · 8 · 2 · (6 unused); byte 29: 8 · 8 · 2 · (6 unused); byte 72: 8 · 8; byte 76: 1 · 1 · (6 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_FIRST_ROUND (0x25dc)

- Modern: 9692 (0x25dc) · 1.12.1: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u16*2 u8 u32 u32 u32 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] u8 bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_ROUND_RESULT (0x25dd)

- Modern: 9693 (0x25dd) · 1.12.1: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u16*2 u8 u32 u32 u32 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] u8 bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_REPLACEMENTS_MADE (0x25de)

- Modern: 9694 (0x25de) · 1.12.1: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u16*2 u8 u32 u32 u32 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] u8 bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_FINAL_ROUND (0x25df)

- Modern: 9695 (0x25df) · 1.12.1: — · Area: General
- Layout: `{ guid { u8 loop[ u8 ] loop[ u32 ] u32 loop[ guid u16*2 u32*2 u16 u8*2 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · 1 · 1 · (4 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_FINISHED (0x25e0)

- Modern: 9696 (0x25e0) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PET_UPDATES (0x25e2)

- Modern: 9698 (0x25e2) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 loop[ { guid u32*3 u16*4 u32*4 u8*3 bytes opt[ guid u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_BATTLE_PET_TRAP_LEVEL (0x25e3)

- Modern: 9699 (0x25e3) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PET_BATTLE_SLOT_UPDATES (0x25e4)

- Modern: 9700 (0x25e4) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 loop[ guid u32 u8*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_BATTLE_PET_JOURNAL_LOCK_ACQUIRED (0x25e5)

- Modern: 9701 (0x25e5) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `BattlePetJournalLockAcquired` — matches

### SMSG_BATTLE_PET_JOURNAL_LOCK_DENIED (0x25e6)

- Modern: 9702 (0x25e6) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PET_JOURNAL (0x25e7)

- Modern: 9703 (0x25e7) · 1.12.1: — · Area: General
- Layout: `{ u16 u32 u32 u8 loop[ guid u32 u8*2 ] loop[ { guid u32*3 u16*4 u32*4 u8*3 bytes opt[ guid u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · (7 unused)
- HermesProxy: `BattlePetJournal` — matches

### SMSG_BATTLE_PET_DELETED (0x25e8)

- Modern: 9704 (0x25e8) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PET_REVOKED (0x25e9)

- Modern: 9705 (0x25e9) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PET_RESTORED (0x25ea)

- Modern: 9706 (0x25ea) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PETS_HEALED (0x25eb)

- Modern: 9707 (0x25eb) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PET_LICENSE_CHANGED (0x25ec)

- Modern: 9708 (0x25ec) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PARTY_UPDATE (0x25ed)

- Modern: 9709 (0x25ed) · 1.12.1: — · Area: General
- Layout: `{ { u16 u8*2 u32 guid u32 guid u32 alt[ ] u8 loop[ u8*2 guid u8*5 bytes { opt[ bytes ] } ] opt[ { u8 guid u8 } ] opt[ u32 loop[ u32 ] ] opt[ { u8 u32*2 u8 u32 u8*4 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8; byte 20: 1 · 1 · 1 · (5 unused)
- HermesProxy: `PartyUpdate` — matches

### SMSG_READY_CHECK_STARTED (0x25ee)

- Modern: 9710 (0x25ee) · 1.12.1: — · Area: General
- Layout: `{ u8 guid guid u64 }`
- Size: 9 bytes (packed GUIDs not counted)
- HermesProxy: `ReadyCheckStarted` — matches

### SMSG_READY_CHECK_RESPONSE (0x25ef)

- Modern: 9711 (0x25ef) · 1.12.1: — · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `ReadyCheckResponse` — matches

### SMSG_READY_CHECK_COMPLETED (0x25f0)

- Modern: 9712 (0x25f0) · 1.12.1: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `ReadyCheckCompleted` — matches

### SMSG_START_ELAPSED_TIMER (0x25fc)

- Modern: 9724 (0x25fc) · 1.12.1: — · Area: General
- Layout: `{ { u64 u32 } }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_STOP_ELAPSED_TIMER (0x25fd)

- Modern: 9725 (0x25fd) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_START_ELAPSED_TIMERS (0x25fe)

- Modern: 9726 (0x25fe) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u64 u32 } ] }`

### SMSG_RESPEC_WIPE_CONFIRM (0x260a)

- Modern: 9738 (0x260a) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 guid }`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `RespecWipeConfirm` — matches

### SMSG_LOOT_RESPONSE (0x260c)

- Modern: 9740 (0x260c) · 1.12.1: 352 (0x160) · Area: General
- Layout: `{ { guid guid u8*4 u32*2 u32 u8 loop[ { u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } ] loop[ u32*2 u8*2 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 8 · 8 · 8 · 8; byte 20: 1 · 1 · (6 unused)
- HermesProxy: `LootResponse` — matches

### SMSG_LOOT_REMOVED (0x260d)

- Modern: 9741 (0x260d) · 1.12.1: 354 (0x162) · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `LootRemoved` — matches

### SMSG_COIN_REMOVED (0x260f)

- Modern: 9743 (0x260f) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CoinRemoved` — matches

### SMSG_AE_LOOT_TARGETS (0x2610)

- Modern: 9744 (0x2610) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_AE_LOOT_TARGET_ACK (0x2611)

- Modern: 9745 (0x2611) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOOT_RELEASE_ALL (0x2612)

- Modern: 9746 (0x2612) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOOT_RELEASE (0x2613)

- Modern: 9747 (0x2613) · 1.12.1: 353 (0x161) · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LootReleaseResponse` — matches

### SMSG_LOOT_MONEY_NOTIFY (0x2614)

- Modern: 9748 (0x2614) · 1.12.1: 355 (0x163) · Area: General
- Layout: `{ u64*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)
- HermesProxy: `LootMoneyNotify` — matches

### SMSG_LOOT_START_ROLL (0x2615)

- Modern: 9749 (0x2615) · 1.12.1: 673 (0x2a1) · Area: General
- Layout: `{ guid u32*2 u8*2 { u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 8 · 2 · 3 · 1 · (2 unused); byte 25: 1 · (7 unused) · 6 · (2 unused); byte 31: 8 · 8
- HermesProxy: `StartLootRoll` — matches

### SMSG_LOOT_ROLL (0x2616)

- Modern: 9750 (0x2616) · 1.12.1: 674 (0x2a2) · Area: General
- Layout: `{ guid guid u32 u8 { u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 8 · 2 · 3 · 1 · (2 unused); byte 22: 1 · (7 unused) · 6 · (2 unused); byte 28: 8 · 8 · 1 · (7 unused)
- HermesProxy: `LootRollBroadcast` — matches

### SMSG_LOOT_MASTER_LIST (0x2617)

- Modern: 9751 (0x2617) · 1.12.1: 676 (0x2a4) · Area: General
- Layout: `{ guid u32 loop[ guid ] }`
- HermesProxy: `MasterLootCandidateList` — matches

### SMSG_LOOT_ROLLS_COMPLETE (0x2618)

- Modern: 9752 (0x2618) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `LootRollsComplete` — matches

### SMSG_LOOT_ALL_PASSED (0x2619)

- Modern: 9753 (0x2619) · 1.12.1: 670 (0x29e) · Area: General
- Layout: `{ guid { u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 2 · 3 · 1 · (2 unused); byte 15: 1 · (7 unused) · 6 · (2 unused); byte 21: 8 · 8
- HermesProxy: `LootAllPassed` — matches

### SMSG_LOOT_ROLL_WON (0x261a)

- Modern: 9754 (0x261a) · 1.12.1: 671 (0x29f) · Area: General
- Layout: `{ guid guid u32 u8 { u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 8 · 2 · 3 · 1 · (2 unused); byte 22: 1 · (7 unused) · 6 · (2 unused); byte 28: 8 · 8 · 1 · (7 unused)
- HermesProxy: `LootRollWon` — differs

### SMSG_ITEM_PUSH_RESULT (0x261b)

- Modern: 9755 (0x261b) · 1.12.1: 358 (0x166) · Area: General
- Layout: `{ guid u8 u32*9 guid u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 41: 1 · 1 · 3 · 1 · 1 · (1 unused); byte 54: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `ItemPushResult` — matches

### SMSG_DISPLAY_TOAST (0x261c)

- Modern: 9756 (0x261c) · 1.12.1: — · Area: General
- Layout: `{ u64 u8 u32 u8 opt[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32*2 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 13: 1 · 2 · 1 · 1 · (3 unused); byte 26: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `DisplayToast` — differs

### SMSG_SET_PET_SPECIALIZATION (0x261d)

- Modern: 9757 (0x261d) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (2 bytes): +0: u16

### SMSG_BLACK_MARKET_OPEN_RESULT (0x261e)

- Modern: 9758 (0x261e) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_BLACK_MARKET_REQUEST_ITEMS_RESULT (0x261f)

- Modern: 9759 (0x261f) · 1.12.1: — · Area: General
- Layout: `{ u64 u32 loop[ u32*3 u64*3 u32*2 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u8 ] }`

### SMSG_BLACK_MARKET_BID_ON_ITEM_RESULT (0x2620)

- Modern: 9760 (0x2620) · 1.12.1: — · Area: General
- Layout: `{ u32*2 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### SMSG_BLACK_MARKET_OUTBID (0x2621)

- Modern: 9761 (0x2621) · 1.12.1: — · Area: General
- Layout: `{ u32*2 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### SMSG_BLACK_MARKET_WON (0x2622)

- Modern: 9762 (0x2622) · 1.12.1: — · Area: General
- Layout: `{ u32*2 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### SMSG_SCENARIO_STATE (0x2623)

- Modern: 9763 (0x2623) · 1.12.1: — · Area: General
- Layout: `{ { u32*7 u32 u32 u32 guid loop[ { u32 } ] u8 loop[ { u32 u64 guid { u32 } u64*2 u8 opt[ u64 ] } ] loop[ { u32 u8 } ] loop[ u32 u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 42: 1 · (7 unused)

### SMSG_SCENARIO_PROGRESS_UPDATE (0x2624)

- Modern: 9764 (0x2624) · 1.12.1: — · Area: General
- Layout: `{ { u32 u64 guid { u32 } u64*2 u8 opt[ u64 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 34: 4 · 1 · (3 unused)

### SMSG_GROUP_NEW_LEADER (0x2625)

- Modern: 9765 (0x2625) · 1.12.1: 121 (0x79) · Area: General
- Layout: `{ u8*3 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 9 · (7 unused)
- HermesProxy: `GroupNewLeader` — matches

### SMSG_SEND_RAID_TARGET_UPDATE_ALL (0x2626)

- Modern: 9766 (0x2626) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 loop[ guid u8 ] }`
- HermesProxy: `SendRaidTargetUpdateAll` — matches

### SMSG_SEND_RAID_TARGET_UPDATE_SINGLE (0x2627)

- Modern: 9767 (0x2627) · 1.12.1: — · Area: General
- Layout: `{ u8*2 guid guid }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8
- HermesProxy: `SendRaidTargetUpdateSingle` — matches

### SMSG_RANDOM_ROLL (0x2628)

- Modern: 9768 (0x2628) · 1.12.1: — · Area: General
- Layout: `{ guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `RandomRoll` — matches

### SMSG_INSPECT_RESULT (0x2629)

- Modern: 9769 (0x2629) · 1.12.1: 277 (0x115) · Area: General
- Layout: `{ { { guid u32*2 u8*4 u32 bytes loop[ { u32*2 } ] loop[ { guid u8 u32*2 loop[ u32 ] { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u32*3 u8 ] loop[ u32 u8 ] loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] } ] } u32*3 u8 u16*2 u32*2 loop[ u16 ] loop[ u8 ] u8 loop[ { u8 u32*10 u8 } ] opt[ { guid u32*2 } ] opt[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 6 · (2 unused) · 8 · 8 · 8; byte 43: 1 · 1 · (6 unused) · 8; byte 85: 1 · (7 unused) · 8; byte 127: 1 · (7 unused) · 8; byte 169: 1 · (7 unused) · 8; byte 211: 1 · (7 unused) · 8; byte 253: 1 · (7 unused) · 8; byte 295: 1 · (7 unused)
- HermesProxy: `InspectResult` — matches

### SMSG_ARENA_CROWD_CONTROL_SPELL_RESULT (0x262a)

- Modern: 9770 (0x262a) · 1.12.1: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_SCENARIO_POIS (0x262b)

- Modern: 9771 (0x262b) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u32*2 loop[ u32*9 loop[ u32*3 ] ] } ] }`

### SMSG_RAID_INSTANCE_INFO (0x262c)

- Modern: 9772 (0x262c) · 1.12.1: 716 (0x2cc) · Area: General
- Layout: `{ u32 loop[ u32*2 u64 u32*2 u8 ] }`
- HermesProxy: `RaidInstanceInfo` — matches

### SMSG_CONSOLE_WRITE (0x262d)

- Modern: 9773 (0x262d) · 1.12.1: — · Area: General
- Layout: `{ u8*2 u32 { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_PLAY_SCENE (0x262e)

- Modern: 9774 (0x262e) · 1.12.1: — · Area: General
- Layout: `{ { u32*4 guid u32*4 } u8 }`
- Size: 33 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 34: 1 · (7 unused)

### SMSG_CANCEL_SCENE (0x262f)

- Modern: 9775 (0x262f) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BATTLE_PET_ERROR (0x2630)

- Modern: 9776 (0x2630) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_MAIL_COMMAND_RESULT (0x2633)

- Modern: 9779 (0x2633) · 1.12.1: 569 (0x239) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: u32, +4: u32, +8: u32, +12: u32, +16: u32
- HermesProxy: `MailCommandResult` — matches

### SMSG_NOTIFY_RECEIVED_MAIL (0x2634)

- Modern: 9780 (0x2634) · 1.12.1: 645 (0x285) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `NotifyReceivedMail` — matches

### SMSG_ADD_BATTLENET_FRIEND_RESPONSE (0x2635)

- Modern: 9781 (0x2635) · 1.12.1: — · Area: General
- Layout: `{ u64 u8 opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 4 · 1 · (3 unused)

### SMSG_ADDON_LIST_REQUEST (0x263a)

- Modern: 9786 (0x263a) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_ACHIEVEMENT_EARNED (0x263b)

- Modern: 9787 (0x263b) · 1.12.1: — · Area: General
- Layout: `{ guid guid u32 { u32 } u32*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)
- HermesProxy: `AchievementEarnedPkt` — matches

### SMSG_BONUS_ROLL_EMPTY (0x263d)

- Modern: 9789 (0x263d) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_UPDATE_EXPANSION_LEVEL (0x263e)

- Modern: 9790 (0x263e) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 opt[ u8 ] opt[ u8 ] loop[ { u8 u32 loop[ u8*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · (5 unused)

### SMSG_CONTROL_UPDATE (0x263f)

- Modern: 9791 (0x263f) · 1.12.1: 345 (0x159) · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)
- HermesProxy: `ControlUpdate` — matches

### SMSG_ARENA_PREP_OPPONENT_SPECIALIZATIONS (0x2640)

- Modern: 9792 (0x2640) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ u32*2 guid ] }`

### SMSG_FORCE_OBJECT_RELINK (0x2642)

- Modern: 9794 (0x2642) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DISPLAY_PROMOTION (0x2643)

- Modern: 9795 (0x2643) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SERVER_FIRST_ACHIEVEMENTS (0x2645)

- Modern: 9797 (0x2645) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { guid u32 } ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_CORPSE_LOCATION (0x2646)

- Modern: 9798 (0x2646) · 1.12.1: — · Area: General
- Layout: `{ u8 guid u32*5 guid }`
- Size: 21 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `CorpseLocation` — matches

### SMSG_REFRESH_COMPONENT (0x2648)

- Modern: 9800 (0x2648) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_DEBUG_MENU_MANAGER_FULL_UPDATE (0x264c)

- Modern: 9804 (0x264c) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOSS_OF_CONTROL_AURA_UPDATE (0x2667)

- Modern: 9831 (0x2667) · 1.12.1: — · Area: General
- Layout: `{ guid u32 loop[ u8*4 ] }`

### SMSG_ADD_LOSS_OF_CONTROL (0x2668)

- Modern: 9832 (0x2668) · 1.12.1: — · Area: General
- Layout: `{ guid u32 guid u32*3 u8*2 }`
- Size: 18 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 8 · 8

### SMSG_SET_TIME_ZONE_INFORMATION (0x266e)

- Modern: 9838 (0x266e) · 1.12.1: — · Area: General
- Layout: `{ u8*2 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 7 · (2 unused)
- HermesProxy: `SetTimeZoneInformation` — matches

### SMSG_BATTLE_PET_CAGE_DATE_ERROR (0x266f)

- Modern: 9839 (0x266f) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_TEXT_EMOTE (0x2671)

- Modern: 9841 (0x2671) · 1.12.1: 261 (0x105) · Area: General
- Layout: `{ guid guid u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `STextEmote` — matches

### SMSG_PET_GOD_MODE (0x2672)

- Modern: 9842 (0x2672) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_TAXI_NODE_STATUS (0x2673)

- Modern: 9843 (0x2673) · 1.12.1: 427 (0x1ab) · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 2 · (6 unused)
- HermesProxy: `TaxiNodeStatusPkt` — matches

### SMSG_ACTIVATE_TAXI_REPLY (0x2674)

- Modern: 9844 (0x2674) · 1.12.1: 430 (0x1ae) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)
- HermesProxy: `ActivateTaxiReplyPkt` — matches

### SMSG_NEW_TAXI_PATH (0x2675)

- Modern: 9845 (0x2675) · 1.12.1: 431 (0x1af) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `NewTaxiPath` — matches

### SMSG_SHOW_BANK (0x2676)

- Modern: 9846 (0x2676) · 1.12.1: 440 (0x1b8) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ShowBank` — matches

### SMSG_GAME_SPEED_SET (0x2678)

- Modern: 9848 (0x2678) · 1.12.1: 71 (0x47) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: f32

### SMSG_SERVER_TIME (0x2679)

- Modern: 9849 (0x2679) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_LOGOUT_RESPONSE (0x267a)

- Modern: 9850 (0x267a) · 1.12.1: 76 (0x4c) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `LogoutResponse` — matches

### SMSG_LOGOUT_COMPLETE (0x267b)

- Modern: 9851 (0x267b) · 1.12.1: 77 (0x4d) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `LogoutComplete` — matches

### SMSG_LOGOUT_CANCEL_ACK (0x267c)

- Modern: 9852 (0x267c) · 1.12.1: 79 (0x4f) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `LogoutCancelAck` — matches

### SMSG_INSTANCE_RESET (0x267d)

- Modern: 9853 (0x267d) · 1.12.1: 798 (0x31e) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `InstanceReset` — matches

### SMSG_INSTANCE_RESET_FAILED (0x267e)

- Modern: 9854 (0x267e) · 1.12.1: 799 (0x31f) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)
- HermesProxy: `InstanceResetFailed` — matches

### SMSG_UPDATE_LAST_INSTANCE (0x267f)

- Modern: 9855 (0x267f) · 1.12.1: 800 (0x320) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `UpdateLastInstance` — matches

### SMSG_KICK_REASON (0x2680)

- Modern: 9856 (0x2680) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CALENDAR_SEND_CALENDAR (0x2682)

- Modern: 9858 (0x2682) · 1.12.1: — · Area: General
- Layout: `{ { u32*2 u32 u32 loop[ u64*2 u8*3 guid ] loop[ u64 u32*3 ] loop[ u64 u8 u32*3 u64 guid u8 bytes ] } }`

### SMSG_CALENDAR_SEND_EVENT (0x2683)

- Modern: 9859 (0x2683) · 1.12.1: — · Area: General
- Layout: `{ { u8 guid u64 u8 u32*4 u64 u32 u8*3 loop[ guid u64 u8*4 u32 u8 bytes ] bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 40: 8 · 11 · (5 unused)

### SMSG_CALENDAR_COMMUNITY_INVITE (0x2684)

- Modern: 9860 (0x2684) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ guid u8 ] }`

### SMSG_CALENDAR_INVITE_ADDED (0x2685)

- Modern: 9861 (0x2685) · 1.12.1: — · Area: General
- Layout: `{ guid u64*2 u8*3 u32 u8 }`
- Size: 24 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 8 · 8 · 8; byte 25: 1 · (7 unused)

### SMSG_CALENDAR_INVITE_REMOVED (0x2686)

- Modern: 9862 (0x2686) · 1.12.1: — · Area: General
- Layout: `{ guid u64 u32 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### SMSG_CALENDAR_INVITE_STATUS (0x2687)

- Modern: 9863 (0x2687) · 1.12.1: — · Area: General
- Layout: `{ guid u64 u32*2 u8 u32 u8 }`
- Size: 22 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 23: 1 · (7 unused)

### SMSG_CALENDAR_MODERATOR_STATUS (0x2688)

- Modern: 9864 (0x2688) · 1.12.1: — · Area: General
- Layout: `{ guid u64 u8*2 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 1 · (7 unused)

### SMSG_CALENDAR_INVITE_ALERT (0x2689)

- Modern: 9865 (0x2689) · 1.12.1: — · Area: General
- Layout: `{ { u64 u32*2 u8 u32 u64*2 u8*2 guid guid u8 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 37: 8 · 8

### SMSG_CALENDAR_INVITE_STATUS_ALERT (0x268a)

- Modern: 9866 (0x268a) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (17 bytes): +0: 8 bytes, +8: u32, +12: u16, +16: u8

### SMSG_CALENDAR_INVITE_REMOVED_ALERT (0x268b)

- Modern: 9867 (0x268b) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (17 bytes): +0: 8 bytes, +8: u32, +12: u16, +16: u8

### SMSG_CALENDAR_EVENT_REMOVED_ALERT (0x268c)

- Modern: 9868 (0x268c) · 1.12.1: — · Area: General
- Layout: `{ u64 u32 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_CALENDAR_EVENT_UPDATED_ALERT (0x268d)

- Modern: 9869 (0x268d) · 1.12.1: — · Area: General
- Layout: `{ u64*2 u32*5 u8*4 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 36: 8 · 8 · 11 · 1 · (4 unused)

### SMSG_CALENDAR_INVITE_NOTES (0x268e)

- Modern: 9870 (0x268e) · 1.12.1: — · Area: General
- Layout: `{ guid u64 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · 8 · (7 unused)

### SMSG_CALENDAR_INVITE_NOTES_ALERT (0x268f)

- Modern: 9871 (0x268f) · 1.12.1: — · Area: General
- Layout: `{ u64 u8 bytes }`

### SMSG_CALENDAR_RAID_LOCKOUT_ADDED (0x2690)

- Modern: 9872 (0x2690) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (24 bytes): +0: 8 bytes, +8: u32, +12: u32, +16: u32, +20: u32

### SMSG_CALENDAR_RAID_LOCKOUT_REMOVED (0x2691)

- Modern: 9873 (0x2691) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +8: u32, +12: u32

### SMSG_CALENDAR_RAID_LOCKOUT_UPDATED (0x2692)

- Modern: 9874 (0x2692) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: u32, +4: u32, +8: u32, +12: u32, +16: u32

### SMSG_CALENDAR_SEND_NUM_PENDING (0x2693)

- Modern: 9875 (0x2693) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_CALENDAR_CLEAR_PENDING_ACTION (0x2694)

- Modern: 9876 (0x2694) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CALENDAR_COMMAND_RESULT (0x2695)

- Modern: 9877 (0x2695) · 1.12.1: — · Area: General
- Layout: `{ u8*4 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8 · 9 · (7 unused)

### SMSG_SPECIAL_MOUNT_ANIM (0x2696)

- Modern: 9878 (0x2696) · 1.12.1: 370 (0x172) · Area: General
- Layout: `{ guid u32 loop[ u32 ] }`
- HermesProxy: `SpecialMountAnim` — matches

### SMSG_PET_ACTION_SOUND (0x2697)

- Modern: 9879 (0x2697) · 1.12.1: 804 (0x324) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PetActionSound` — matches

### SMSG_PET_DISMISS_SOUND (0x2698)

- Modern: 9880 (0x2698) · 1.12.1: 805 (0x325) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: u32, +4: 8 bytes, +12: f32

### SMSG_GM_TICKET_SYSTEM_STATUS (0x2699)

- Modern: 9881 (0x2699) · 1.12.1: 539 (0x21b) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `GMTicketSystemStatus` — matches

### SMSG_GM_TICKET_CASE_STATUS (0x269a)

- Modern: 9882 (0x269a) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u32 u64 u32 u16 u64 u32 u8*3 bytes bytes } ] }`
- HermesProxy: `GMTicketCaseStatus` — matches

### SMSG_SET_DUNGEON_DIFFICULTY (0x269b)

- Modern: 9883 (0x269b) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `DungeonDifficultySet` — matches

### SMSG_WHO_IS (0x269c)

- Modern: 9884 (0x269c) · 1.12.1: 101 (0x65) · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 11 · (5 unused)

### SMSG_WEATHER (0x269d)

- Modern: 9885 (0x269d) · 1.12.1: 756 (0x2f4) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `WeatherPkt` — matches

### SMSG_START_LIGHTNING_STORM (0x269e)

- Modern: 9886 (0x269e) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `StartLightningStorm` — matches

### SMSG_END_LIGHTNING_STORM (0x269f)

- Modern: 9887 (0x269f) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_UPDATE_INSTANCE_OWNERSHIP (0x26a0)

- Modern: 9888 (0x26a0) · 1.12.1: 811 (0x32b) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `UpdateInstanceOwnership` — matches

### SMSG_NOTIFY_MISSILE_TRAJECTORY_COLLISION (0x26a1)

- Modern: 9889 (0x26a1) · 1.12.1: — · Area: General
- Layout: `{ guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_COMPLAINT_RESULT (0x26a2)

- Modern: 9890 (0x26a2) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_SUMMON_CANCEL (0x26a7)

- Modern: 9895 (0x26a7) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_DISMOUNT (0x26a8)

- Modern: 9896 (0x26a8) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `Dismount` — matches

### SMSG_EQUIPMENT_SET_ID (0x26a9)

- Modern: 9897 (0x26a9) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +8: u32, +12: u32
- HermesProxy: `EquipmentSetID` — matches

### SMSG_PET_TAME_FAILURE (0x26aa)

- Modern: 9898 (0x26aa) · 1.12.1: 371 (0x173) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `PetTameFailure` — matches

### SMSG_AI_REACTION (0x26ac)

- Modern: 9900 (0x26ac) · 1.12.1: 316 (0x13c) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `AIReaction` — matches

### SMSG_OFFER_PETITION_ERROR (0x26ad)

- Modern: 9901 (0x26ad) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_RESET_FAILED_NOTIFY (0x26ae)

- Modern: 9902 (0x26ae) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `ResetFailedNotify` — matches

### SMSG_ADD_RUNE_POWER (0x26af)

- Modern: 9903 (0x26af) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_COOLDOWN_EVENT (0x26b0)

- Modern: 9904 (0x26b0) · 1.12.1: 309 (0x135) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `CooldownEvent` — matches

### SMSG_CLEAR_COOLDOWN (0x26b1)

- Modern: 9905 (0x26b1) · 1.12.1: 478 (0x1de) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)
- HermesProxy: `ClearCooldown` — matches

### SMSG_OVERRIDE_LIGHT (0x26b2)

- Modern: 9906 (0x26b2) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### SMSG_PETITION_SHOW_LIST (0x26b5)

- Modern: 9909 (0x26b5) · 1.12.1: 444 (0x1bc) · Area: General
- Layout: `{ guid u32 loop[ u32*5 ] }`
- HermesProxy: `ServerPetitionShowList` — matches

### SMSG_PETITION_SHOW_SIGNATURES (0x26b6)

- Modern: 9910 (0x26b6) · 1.12.1: 447 (0x1bf) · Area: General
- Layout: `{ guid guid guid u32*2 loop[ guid u32 ] }`
- HermesProxy: `ServerPetitionShowSignatures` — matches

### SMSG_RECRUIT_A_FRIEND_FAILURE (0x26b7)

- Modern: 9911 (0x26b7) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 6 · (2 unused)

### SMSG_CROSSED_INEBRIATION_THRESHOLD (0x26b8)

- Modern: 9912 (0x26b8) · 1.12.1: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `CrossedInebriationThreshold` — matches

### SMSG_PET_NAME_INVALID (0x26ba)

- Modern: 9914 (0x26ba) · 1.12.1: 376 (0x178) · Area: General
- Layout: `{ u8 { guid u32 u8*2 loop[ opt[ u8 ] alt[ u8 ] ] loop[ bytes ] bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 7: 8 · 1 · (7 unused)

### SMSG_SELL_RESPONSE (0x26bb)

- Modern: 9915 (0x26bb) · 1.12.1: 417 (0x1a1) · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SellResponse` — matches

### SMSG_BUY_SUCCEEDED (0x26bc)

- Modern: 9916 (0x26bc) · 1.12.1: 420 (0x1a4) · Area: General
- Layout: `{ guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `BuySucceeded` — matches

### SMSG_BUY_FAILED (0x26bd)

- Modern: 9917 (0x26bd) · 1.12.1: 421 (0x1a5) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `BuyFailed` — matches

### SMSG_TOTEM_CREATED (0x26be)

- Modern: 9918 (0x26be) · 1.12.1: — · Area: General
- Layout: `{ u8 guid u32*3 u8 }`
- Size: 14 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 15: 1 · (7 unused)
- HermesProxy: `TotemCreated` — matches

### SMSG_TOTEM_MOVED (0x26c0)

- Modern: 9920 (0x26c0) · 1.12.1: — · Area: General
- Layout: `{ u8*2 guid }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8

### SMSG_TRIGGER_MOVIE (0x26c1)

- Modern: 9921 (0x26c1) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SHOW_TAXI_NODES (0x26c3)

- Modern: 9923 (0x26c3) · 1.12.1: 425 (0x1a9) · Area: General
- Layout: `{ u8 u32*2 opt[ guid u32 ] loop[ u8 ] loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `ShowTaxiNodes` — matches

### SMSG_MINIMAP_PING (0x26c4)

- Modern: 9924 (0x26c4) · 1.12.1: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MinimapPing` — matches

### SMSG_FISH_NOT_HOOKED (0x26c5)

- Modern: 9925 (0x26c5) · 1.12.1: 456 (0x1c8) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `FishNotHooked` — matches

### SMSG_FISH_ESCAPED (0x26c6)

- Modern: 9926 (0x26c6) · 1.12.1: 457 (0x1c9) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `FishEscaped` — matches

### SMSG_HEALTH_UPDATE (0x26c7)

- Modern: 9927 (0x26c7) · 1.12.1: — · Area: General
- Layout: `{ guid u64 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `HealthUpdate` — matches

### SMSG_POWER_UPDATE (0x26c8)

- Modern: 9928 (0x26c8) · 1.12.1: — · Area: General
- Layout: `{ guid u32 loop[ u32 u8 ] }`
- HermesProxy: `PowerUpdate` — matches

### SMSG_DEATH_RELEASE_LOC (0x26c9)

- Modern: 9929 (0x26c9) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: u32, +4: 8 bytes, +12: u32
- HermesProxy: `DeathReleaseLoc` — matches

### SMSG_FORCED_DEATH_UPDATE (0x26ca)

- Modern: 9930 (0x26ca) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYED_TIME (0x26cb)

- Modern: 9931 (0x26cb) · 1.12.1: 461 (0x1cd) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `PlayedTime` — matches

### SMSG_TITLE_EARNED (0x26cd)

- Modern: 9933 (0x26cd) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_TITLE_LOST (0x26ce)

- Modern: 9934 (0x26ce) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_HIGHEST_THREAT_UPDATE (0x26cf)

- Modern: 9935 (0x26cf) · 1.12.1: — · Area: General
- Layout: `{ guid guid u32 loop[ guid u64 ] }`
- HermesProxy: `HighestThreatUpdate` — matches

### SMSG_THREAT_UPDATE (0x26d0)

- Modern: 9936 (0x26d0) · 1.12.1: — · Area: General
- Layout: `{ guid u32 loop[ guid u64 ] }`
- HermesProxy: `ThreatUpdate` — matches

### SMSG_THREAT_REMOVE (0x26d1)

- Modern: 9937 (0x26d1) · 1.12.1: — · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ThreatRemove` — matches

### SMSG_THREAT_CLEAR (0x26d2)

- Modern: 9938 (0x26d2) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ThreatClear` — matches

### SMSG_TOTEM_DURATION_CHANGED (0x26d3)

- Modern: 9939 (0x26d3) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CANCEL_AUTO_REPEAT (0x26d4)

- Modern: 9940 (0x26d4) · 1.12.1: 668 (0x29c) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CancelAutoRepeat` — matches

### SMSG_TRAINER_LIST (0x26d5)

- Modern: 9941 (0x26d5) · 1.12.1: 433 (0x1b1) · Area: General
- Layout: `{ { guid u32*3 loop[ u32*4 loop[ u32 ] u8*2 ] u8*2 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · (5 unused)
- HermesProxy: `TrainerList` — matches

### SMSG_TRAINER_BUY_FAILED (0x26d6)

- Modern: 9942 (0x26d6) · 1.12.1: 436 (0x1b4) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `TrainerBuyFailed` — matches

### SMSG_CRITERIA_UPDATE (0x26d7)

- Modern: 9943 (0x26d7) · 1.12.1: — · Area: General
- Layout: `{ u32 u64 guid u32 { u32 } u64*2 u8 opt[ u64 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 38: 1 · (7 unused)
- HermesProxy: `CriteriaUpdatePkt` — differs

### SMSG_CHAR_CUSTOMIZE_FAILURE (0x26d8)

- Modern: 9944 (0x26d8) · 1.12.1: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_CHAR_CUSTOMIZE_SUCCESS (0x26d9)

- Modern: 9945 (0x26d9) · 1.12.1: — · Area: General
- Layout: `{ guid u8 u32 loop[ { u32*2 } ] u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 7: 6 · (2 unused)

### SMSG_QUERY_TIME_RESPONSE (0x26da)

- Modern: 9946 (0x26da) · 1.12.1: 463 (0x1cf) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes
- HermesProxy: `QueryTimeResponse` — matches

### SMSG_LOG_XP_GAIN (0x26db)

- Modern: 9947 (0x26db) · 1.12.1: 464 (0x1d0) · Area: General
- Layout: `{ guid u32 u8 u32*2 u8 }`
- Size: 14 bytes (packed GUIDs not counted)
- HermesProxy: `LogXPGain` — matches

### SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA (0x26dc)

- Modern: 9948 (0x26dc) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `OnCancelExpectedRideVehicleAura` — matches

### SMSG_CRITERIA_DELETED (0x26dd)

- Modern: 9949 (0x26dd) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `CriteriaDeletedPkt` — matches

### SMSG_ACHIEVEMENT_DELETED (0x26de)

- Modern: 9950 (0x26de) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_LEVEL_UP_INFO (0x26df)

- Modern: 9951 (0x26df) · 1.12.1: 468 (0x1d4) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (60 bytes): +0: u32, +4: u32, +8: u32, +32: u32, +36: u32, +40: u32, +44: u32, +48: u32, +52: u32, +56: u32
- HermesProxy: `LevelUpInfo` — differs

### SMSG_ITEM_CHANGED (0x26e0)

- Modern: 9952 (0x26e0) · 1.12.1: — · Area: General
- Layout: `{ guid { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused) · 6 · (2 unused); byte 28: 1 · (7 unused) · 6 · (2 unused)

### SMSG_AUCTION_HELLO_RESPONSE (0x26e3)

- Modern: 9955 (0x26e3) · 1.12.1: — · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `AuctionHelloResponse` — matches

### SMSG_AUCTION_REPLICATE_RESPONSE (0x26e4)

- Modern: 9956 (0x26e4) · 1.12.1: — · Area: General
- Layout: `{ u32*6 loop[ { u8 u8 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 loop[ u32*3 u8 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] opt[ { { loop[ opt[ u8 ] alt[ u8 ] ] bits(4) } opt[ u8 ] opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] opt[ u16 ] opt[ u16 ] } ] } ] }`

### SMSG_AUCTION_COMMAND_RESULT (0x26e5)

- Modern: 9957 (0x26e5) · 1.12.1: 603 (0x25b) · Area: General
- Layout: `{ u32*4 guid u64*2 u32 }`
- Size: 36 bytes (packed GUIDs not counted)
- HermesProxy: `AuctionCommandResult` — matches

### SMSG_AUCTION_WON_NOTIFICATION (0x26e6)

- Modern: 9958 (0x26e6) · 1.12.1: — · Area: General
- Layout: `{ u32*2 guid { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `AuctionWonNotification` — matches

### SMSG_AUCTION_OUTBID_NOTIFICATION (0x26e7)

- Modern: 9959 (0x26e7) · 1.12.1: — · Area: General
- Layout: `{ u32*2 guid { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u64*2 }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `AuctionOutbidNotification` — matches

### SMSG_AUCTION_CLOSED_NOTIFICATION (0x26e8)

- Modern: 9960 (0x26e8) · 1.12.1: — · Area: General
- Layout: `{ u32 u64 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused) · 6 · (2 unused); byte 30: 1 · (7 unused)
- HermesProxy: `AuctionClosedNotification` — matches

### SMSG_AUCTION_OWNER_BID_NOTIFICATION (0x26e9)

- Modern: 9961 (0x26e9) · 1.12.1: — · Area: General
- Layout: `{ u32 u64 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u64 guid }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `AuctionOwnerBidNotification` — matches

### SMSG_AUCTION_LIST_PENDING_SALES_RESULT (0x26ea)

- Modern: 9962 (0x26ea) · 1.12.1: — · Area: General
- Layout: `{ u32*2 loop[ { u32 u8 u64 u32 u64 u32*4 u8*3 loop[ { u8 u32*5 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] loop[ u32*3 u8 ] } ] opt[ guid ] opt[ u32 ] bytes bytes } ] }`

### SMSG_SET_VEHICLE_REC_ID (0x26ec)

- Modern: 9964 (0x26ec) · 1.12.1: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetVehicleRecID` — matches

### SMSG_PENDING_RAID_LOCK (0x26ed)

- Modern: 9965 (0x26ed) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)
- HermesProxy: `PendingRaidLock` — matches

### SMSG_INSTANCE_GROUP_SIZE_CHANGED (0x26ef)

- Modern: 9967 (0x26ef) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GOD_MODE (0x26f1)

- Modern: 9969 (0x26f1) · 1.12.1: 35 (0x23) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_BINDER_CONFIRM (0x26f2)

- Modern: 9970 (0x26f2) · 1.12.1: 747 (0x2eb) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BinderConfirm` — matches

### SMSG_SET_FACTION_AT_WAR (0x26f5)

- Modern: 9973 (0x26f5) · 1.12.1: 787 (0x313) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_CREATE_CHAR (0x26f6)

- Modern: 9974 (0x26f6) · 1.12.1: 58 (0x3a) · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `CreateChar` — matches

### SMSG_DELETE_CHAR (0x26f7)

- Modern: 9975 (0x26f7) · 1.12.1: 60 (0x3c) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `DeleteChar` — matches

### SMSG_TRANSFER_ABORTED (0x26f8)

- Modern: 9976 (0x26f8) · 1.12.1: 64 (0x40) · Area: General
- Layout: `{ u32 u8 u32 u8 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 9: 6 · (2 unused)
- HermesProxy: `TransferAborted` — matches

### SMSG_PET_GUIDS (0x26f9)

- Modern: 9977 (0x26f9) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ guid ] }`
- HermesProxy: `PetGuids` — matches

### SMSG_CHARACTER_LOGIN_FAILED (0x26fa)

- Modern: 9978 (0x26fa) · 1.12.1: 65 (0x41) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8
- HermesProxy: `CharacterLoginFailed`; `CharacterLoginFailed`; `WorldSocket` — matches

### SMSG_COMMENTATOR_STATE_CHANGED (0x26fb)

- Modern: 9979 (0x26fb) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_COMMENTATOR_MAP_INFO (0x26fc)

- Modern: 9980 (0x26fc) · 1.12.1: — · Area: General
- Layout: `{ u64 u32 loop[ { u32*5 loop[ u32 { u32 u16 u8 } u64 u32 loop[ guid u32 loop[ guid { u32 u16 u8 } ] ] ] } ] }`

### SMSG_COMMENTATOR_PLAYER_INFO (0x26fd)

- Modern: 9981 (0x26fd) · 1.12.1: — · Area: General
- Layout: `{ u32 { u32 u16 u8 } u64 u32 alt[ ] u8 loop[ { guid u8 u32 u16*2 u32*5 u32 u32 loop[ { u32*3 u8 } ] loop[ { u32*2 } ] loop[ { u32*6 u8 opt[ u32 ] opt[ u32 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 23: 1 · (7 unused)

### SMSG_UPDATE_ACCOUNT_DATA (0x26fe)

- Modern: 9982 (0x26fe) · 1.12.1: 524 (0x20c) · Area: General
- Layout: `{ guid u64 u32 u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 4 · (4 unused)
- HermesProxy: `UpdateAccountData` — matches

### SMSG_ACCOUNT_DATA_TIMES (0x26ff)

- Modern: 9983 (0x26ff) · 1.12.1: 521 (0x209) · Area: General
- Layout: `{ guid u64 loop[ u64 ] }`
- HermesProxy: `AccountDataTimes` — matches

### SMSG_GAME_TIME_UPDATE (0x2700)

- Modern: 9984 (0x2700) · 1.12.1: 67 (0x43) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_GAME_TIME_SET (0x2701)

- Modern: 9985 (0x2701) · 1.12.1: 69 (0x45) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_LOGIN_SET_TIME_SPEED (0x2702)

- Modern: 9986 (0x2702) · 1.12.1: 66 (0x42) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32, +8: f32
- HermesProxy: `LoginSetTimeSpeed` — matches

### SMSG_LOAD_EQUIPMENT_SET (0x2703)

- Modern: 9987 (0x2703) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u32 u64 u32*2 loop[ guid u32 ] loop[ u32 ] u32*4 u8*3 opt[ u32 ] bytes bytes } ] }`
- HermesProxy: `LoadEquipmentSet`; `EmptyEquipmentSetList` — matches

### SMSG_START_MIRROR_TIMER (0x2704)

- Modern: 9988 (0x2704) · 1.12.1: 473 (0x1d9) · Area: General
- Layout: `{ u32*5 u8 }`
- Size: 21 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)
- HermesProxy: `StartMirrorTimer` — matches

### SMSG_PAUSE_MIRROR_TIMER (0x2705)

- Modern: 9989 (0x2705) · 1.12.1: 474 (0x1da) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `PauseMirrorTimer` — matches

### SMSG_STOP_MIRROR_TIMER (0x2706)

- Modern: 9990 (0x2706) · 1.12.1: 475 (0x1db) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `StopMirrorTimer` — matches

### SMSG_CORPSE_TRANSPORT_QUERY (0x2707)

- Modern: 9991 (0x2707) · 1.12.1: — · Area: General
- Layout: `{ guid u32*4 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_ENCHANTMENT_LOG (0x2708)

- Modern: 9992 (0x2708) · 1.12.1: 471 (0x1d7) · Area: General
- Layout: `{ guid guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `EnchantmentLog` — matches

### SMSG_SERVER_TIME_OFFSET (0x2709)

- Modern: 9993 (0x2709) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes
- HermesProxy: `ServerTimeOffset` — matches

### SMSG_SPIRIT_HEALER_CONFIRM (0x270a)

- Modern: 9994 (0x270a) · 1.12.1: 546 (0x222) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SpiritHealerConfirm` — matches

### SMSG_AREA_TRIGGER_NO_CORPSE (0x270b)

- Modern: 9995 (0x270b) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_TALENTS_INVOLUNTARILY_RESET (0x270c)

- Modern: 9996 (0x270c) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SPEC_INVOLUNTARILY_CHANGED (0x270d)

- Modern: 9997 (0x270d) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_PAGE_TEXT (0x270e)

- Modern: 9998 (0x270e) · 1.12.1: 479 (0x1df) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_GAME_OBJECT_UI_LINK (0x270f)

- Modern: 9999 (0x270f) · 1.12.1: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_STAND_STATE_UPDATE (0x2711)

- Modern: 10001 (0x2711) · 1.12.1: 669 (0x29d) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8
- HermesProxy: `StandStateUpdate` — matches

### SMSG_SET_FORCED_REACTIONS (0x2712)

- Modern: 10002 (0x2712) · 1.12.1: 677 (0x2a5) · Area: General
- Layout: `{ u32 loop[ u32*2 ] }`
- HermesProxy: `SetForcedReactions` — matches

### SMSG_GAME_OBJECT_RESET_STATE (0x2713)

- Modern: 10003 (0x2713) · 1.12.1: 679 (0x2a7) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GameObjectResetState` — matches

### SMSG_SUMMON_REQUEST (0x2716)

- Modern: 10006 (0x2716) · 1.12.1: 683 (0x2ab) · Area: General
- Layout: `{ guid u32*2 u8*2 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 1 · (7 unused)
- HermesProxy: `SummonRequest` — matches

### SMSG_INSPECT_PVP (0x2717)

- Modern: 10007 (0x2717) · 1.12.1: — · Area: General
- Layout: `{ guid u8 loop[ { u8 u32*10 u8 } ] loop[ { guid u32*5 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 3 · 2 · (3 unused)
- HermesProxy: `InspectPvP` — matches

### — (0x2718)

- Modern: 10008 (0x2718) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_INITIALIZE_FACTIONS (0x2719)

- Modern: 10009 (0x2719) · 1.12.1: 290 (0x122) · Area: General
- Layout: `{ loop[ u8 u32 ] loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2000: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `InitializeFactions` — matches

### SMSG_FACTION_BONUS_INFO (0x271a)

- Modern: 10010 (0x271a) · 1.12.1: — · Area: General
- Layout: `{ loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_CAMERA_EFFECT (0x271b)

- Modern: 10011 (0x271b) · 1.12.1: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_SOCKET_GEMS_SUCCESS (0x271c)

- Modern: 10012 (0x271c) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SocketGemsSuccess` — matches

### SMSG_SOCKET_GEMS_FAILURE (0x271d)

- Modern: 10013 (0x271d) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_SET_FACTION_VISIBLE (0x271f)

- Modern: 10015 (0x271f) · 1.12.1: 291 (0x123) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SET_FACTION_NOT_VISIBLE (0x2720)

- Modern: 10016 (0x2720) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SET_FACTION_STANDING (0x2721)

- Modern: 10017 (0x2721) · 1.12.1: 292 (0x124) · Area: General
- Layout: `{ u32*3 loop[ u32*2 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)
- HermesProxy: `SetFactionStanding` — matches

### SMSG_SET_AI_ANIM_KIT (0x2725)

- Modern: 10021 (0x2725) · 1.12.1: — · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### SMSG_PLAY_ONE_SHOT_ANIM_KIT (0x2726)

- Modern: 10022 (0x2726) · 1.12.1: — · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### SMSG_SET_MOVEMENT_ANIM_KIT (0x2727)

- Modern: 10023 (0x2727) · 1.12.1: — · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### SMSG_SET_MELEE_ANIM_KIT (0x2728)

- Modern: 10024 (0x2728) · 1.12.1: — · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### SMSG_SET_ANIM_TIER (0x2729)

- Modern: 10025 (0x2729) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 3 · (5 unused)

### SMSG_SET_PROFICIENCY (0x272a)

- Modern: 10026 (0x272a) · 1.12.1: 295 (0x127) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8
- HermesProxy: `SetProficiency` — matches

### SMSG_COOLDOWN_CHEAT (0x272e)

- Modern: 10030 (0x272e) · 1.12.1: 481 (0x1e1) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `CooldownCheat` — differs

### SMSG_AREA_SPIRIT_HEALER_TIME (0x2735)

- Modern: 10037 (0x2735) · 1.12.1: 740 (0x2e4) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `AreaSpiritHealerTime` — matches

### SMSG_LOOT_LIST (0x2736)

- Modern: 10038 (0x2736) · 1.12.1: — · Area: General
- Layout: `{ guid guid u8 opt[ guid ] opt[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)
- HermesProxy: `LootList` — matches

### SMSG_DESTROY_ARENA_UNIT (0x2737)

- Modern: 10039 (0x2737) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_UI_HEALING_RANGE_MODIFIED (0x2739)

- Modern: 10041 (0x2739) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: f32

### SMSG_FEIGN_DEATH_RESISTED (0x273a)

- Modern: 10042 (0x273a) · 1.12.1: 692 (0x2b4) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_DURABILITY_DAMAGE_DEATH (0x273b)

- Modern: 10043 (0x273b) · 1.12.1: 701 (0x2bd) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `DurabilityDamageDeath` — matches

### SMSG_INIT_WORLD_STATES (0x273c)

- Modern: 10044 (0x273c) · 1.12.1: 706 (0x2c2) · Area: General
- Layout: `{ u32*4 loop[ u32*2 ] }`
- HermesProxy: `InitWorldStates`; `EmptyInitWorldStates` — matches

### SMSG_UPDATE_WORLD_STATE (0x273d)

- Modern: 10045 (0x273d) · 1.12.1: 707 (0x2c3) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `UpdateWorldState` — matches

### SMSG_PET_ACTION_FEEDBACK (0x273e)

- Modern: 10046 (0x273e) · 1.12.1: 710 (0x2c6) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_CORPSE_RECLAIM_DELAY (0x273f)

- Modern: 10047 (0x273f) · 1.12.1: 617 (0x269) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `CorpseReclaimDelay` — matches

### SMSG_REATTACH_RESURRECT (0x2740)

- Modern: 10048 (0x2740) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_PETITION_SIGN_RESULTS (0x2741)

- Modern: 10049 (0x2741) · 1.12.1: 449 (0x1c1) · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 4 · (4 unused)
- HermesProxy: `PetitionSignResults` — matches

### SMSG_TURN_IN_PETITION_RESULT (0x2743)

- Modern: 10051 (0x2743) · 1.12.1: 453 (0x1c5) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)
- HermesProxy: `TurnInPetitionResult` — matches

### SMSG_USE_EQUIPMENT_SET_RESULT (0x2744)

- Modern: 10052 (0x2744) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `UseEquipmentSetResult` — matches

### SMSG_FORCE_ANIM (0x2746)

- Modern: 10054 (0x2746) · 1.12.1: — · Area: General
- Layout: `{ guid u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 9 · (7 unused)

### SMSG_INVALID_PROMOTION_CODE (0x2748)

- Modern: 10056 (0x2748) · 1.12.1: 487 (0x1e7) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_ITEM_TIME_UPDATE (0x2749)

- Modern: 10057 (0x2749) · 1.12.1: 490 (0x1ea) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_ITEM_ENCHANT_TIME_UPDATE (0x274a)

- Modern: 10058 (0x274a) · 1.12.1: 491 (0x1eb) · Area: General
- Layout: `{ guid u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `ItemEnchantTimeUpdate` — matches

### SMSG_MAIL_LIST_RESULT (0x274b)

- Modern: 10059 (0x274b) · 1.12.1: 571 (0x23b) · Area: General
- Layout: `{ u32*2 loop[ { u32 u8 u64 u32 u64 u32*4 u8*3 loop[ { u8 u32*5 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] loop[ u32*3 u8 ] } ] opt[ guid ] opt[ u32 ] bytes bytes } ] }`
- HermesProxy: `MailListResult` — matches

### SMSG_MAIL_QUERY_NEXT_TIME_RESULT (0x274c)

- Modern: 10060 (0x274c) · 1.12.1: — · Area: General
- Layout: `{ u32*2 loop[ { guid u32*2 u8 u32 } ] }`
- HermesProxy: `MailQueryNextTimeResult` — matches

### SMSG_PARTY_MEMBER_PARTIAL_STATE (0x274d)

- Modern: 10061 (0x274d) · 1.12.1: 126 (0x7e) · Area: General
- Layout: `{ { u8*3 opt[ { u8 opt[ u8 bytes ] opt[ guid ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ { u32 loop[ { u32 u16 u32*2 loop[ u32 ] } ] } ] } ] guid loop[ u8 ] opt[ u16 ] opt[ u8 ] opt[ u16 ] opt[ u32 ] opt[ u32 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u32 ] opt[ { u16*3 } ] opt[ u32 ] opt[ { u32 loop[ { u32 u16 u32*2 loop[ u32 ] } ] } ] opt[ { u32*2 guid loop[ u16*2 ] } ] opt[ { u32*3 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `PartyMemberPartialState` — matches

### SMSG_PARTY_MEMBER_FULL_STATE (0x274e)

- Modern: 10062 (0x274e) · 1.12.1: 754 (0x2f2) · Area: General
- Layout: `{ u8 { loop[ u8 ] u16 u8 u16 u32*2 u16*6 u32 u16*3 u32*4 guid loop[ u16*2 ] u32*3 loop[ { u32 u16 u32*2 loop[ u32 ] } ] u8 opt[ guid u32*4 loop[ { u32 u16 u32*2 loop[ u32 ] } ] u8 bytes ] } guid }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused) · 8 · 8; byte 68: 1 · (7 unused)
- HermesProxy: `PartyMemberFullState` — matches

### SMSG_PARTY_KILL_LOG (0x274f)

- Modern: 10063 (0x274f) · 1.12.1: 501 (0x1f5) · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PartyKillLog` — matches

### SMSG_PROC_RESIST (0x2750)

- Modern: 10064 (0x2750) · 1.12.1: 608 (0x260) · Area: General
- Layout: `{ guid guid u32 u8 opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)

### SMSG_ISLAND_AZERITE_GAIN (0x2751)

- Modern: 10065 (0x2751) · 1.12.1: — · Area: General
- Layout: `{ u32 guid guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_ISLAND_COMPLETE (0x2752)

- Modern: 10066 (0x2752) · 1.12.1: — · Area: General
- Layout: `{ u32*3 alt[ ] loop[ { guid u32*2 u8*4 u32 bytes loop[ { u32*2 } ] loop[ { guid u8 u32*2 loop[ u32 ] { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u32*3 u8 ] loop[ u32 u8 ] loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] } ] } ] }`

### SMSG_WARFRONT_COMPLETE (0x2753)

- Modern: 10067 (0x2753) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_EXPLORATION_EXPERIENCE (0x2754)

- Modern: 10068 (0x2754) · 1.12.1: 504 (0x1f8) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32
- HermesProxy: `ExplorationExperience` — matches

### SMSG_ARENA_TEAM_ROSTER (0x2755)

- Modern: 10069 (0x2755) · 1.12.1: — · Area: General
- Layout: `{ u32*9 loop[ { guid u8 u32 u8*2 u32*5 u8 bytes opt[ u32 ] opt[ u32 ] } ] }`
- HermesProxy: `ArenaTeamRosterResponse` — matches

### SMSG_ARENA_TEAM_INVITE (0x2756)

- Modern: 10070 (0x2756) · 1.12.1: — · Area: General
- Layout: `{ guid u32 guid u8*2 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 6 · 7 · (3 unused)
- HermesProxy: `ArenaTeamInvite` — matches

### SMSG_ARENA_TEAM_EVENT (0x2757)

- Modern: 10071 (0x2757) · 1.12.1: — · Area: General
- Layout: `{ u8 loop[ opt[ u8*2 ] alt[ u8 ] ] loop[ bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 9 · 9 · 9 · (5 unused)
- HermesProxy: `ArenaTeamEvent` — matches

### SMSG_ARENA_TEAM_COMMAND_RESULT (0x2758)

- Modern: 10072 (0x2758) · 1.12.1: — · Area: General
- Layout: `{ u8*4 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8 · 7 · 6 · (3 unused)
- HermesProxy: `ArenaTeamCommandResult` — matches

### SMSG_ARENA_TEAM_STATS (0x2759)

- Modern: 10073 (0x2759) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (28 bytes): +0: u32, +4: u32, +8: u32, +12: u32, +16: u32, +20: u32, +24: u32

### SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT (0x275a)

- Modern: 10074 (0x275a) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u8 loop[ { guid guid u32 u8*4 u64 u8*2 bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `GetAccountCharacterListResult` — matches

### SMSG_LIVE_REGION_GET_ACCOUNT_CHARACTER_LIST_RESULT (0x275b)

- Modern: 10075 (0x275b) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u8 loop[ { guid guid u32 u8*4 u64 u8*2 bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_CHARACTER_RENAME_RESULT (0x275c)

- Modern: 10076 (0x275c) · 1.12.1: 712 (0x2c8) · Area: General
- Layout: `{ u8*2 opt[ guid ] bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 1 · 6 · (1 unused)
- HermesProxy: `CharacterRenameResult` — matches

### SMSG_MODIFY_COOLDOWN (0x275d)

- Modern: 10077 (0x275d) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)

### SMSG_UPDATE_COOLDOWN (0x275e)

- Modern: 10078 (0x275e) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### SMSG_UPDATE_CHARGE_CATEGORY_COOLDOWN (0x275f)

- Modern: 10079 (0x275f) · 1.12.1: — · Area: General
- Layout: `{ u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_PRE_RESSURECT (0x2760)

- Modern: 10080 (0x2760) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PreRessurect` — matches

### SMSG_PLAY_SOUND (0x2761)

- Modern: 10081 (0x2761) · 1.12.1: 722 (0x2d2) · Area: General
- Layout: `{ u32 guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `PlaySound` — matches

### SMSG_PLAY_MUSIC (0x2762)

- Modern: 10082 (0x2762) · 1.12.1: 631 (0x277) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `PlayMusic` — matches

### SMSG_PLAY_OBJECT_SOUND (0x2763)

- Modern: 10083 (0x2763) · 1.12.1: 632 (0x278) · Area: General
- Layout: `{ u32 guid guid u32*4 }`
- Size: 20 bytes (packed GUIDs not counted)
- HermesProxy: `PlayObjectSound` — matches

### SMSG_PLAY_SPEAKERBOT_SOUND (0x2764)

- Modern: 10084 (0x2764) · 1.12.1: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_STOP_SPEAKERBOT_SOUND (0x2765)

- Modern: 10085 (0x2765) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_LIVE_REGION_CHARACTER_COPY_RESULT (0x2766)

- Modern: 10086 (0x2766) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_LIVE_REGION_ACCOUNT_RESTORE_RESULT (0x2767)

- Modern: 10087 (0x2767) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SHOW_TRADE_SKILL_RESPONSE (0x2769)

- Modern: 10089 (0x2769) · 1.12.1: — · Area: General
- Layout: `{ guid { u32*8 loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u16 ] loop[ u16 ] } }`

### SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE (0x276a)

- Modern: 10090 (0x276a) · 1.12.1: — · Area: General
- Layout: `{ { u32*3 alt[ ] u32*2 alt[ ] u32 loop[ { u32 u64*2 u32*5 loop[ u32 ] loop[ u32 ] u8 opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] loop[ { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] loop[ { u32*2 u8 u32*2 u8 loop[ u8 ] bytes { opt[ bytes ] } } ] loop[ { u32*5 u8*2 opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] } }`

### SMSG_BATTLE_PAY_GET_PURCHASE_LIST_RESPONSE (0x276b)

- Modern: 10091 (0x276b) · 1.12.1: — · Area: General
- Layout: `{ u32*2 loop[ { u64 u32*3 u64*2 u32 u8 bytes } ] }`

### SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE (0x276c)

- Modern: 10092 (0x276c) · 1.12.1: — · Area: General
- Layout: `{ u32 u8*2 loop[ { u64 u32*2 guid guid u32*2 u64 u8 opt[ { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 11 · (5 unused)

### SMSG_BATTLE_PAY_DISTRIBUTION_UNREVOKED (0x276d)

- Modern: 10093 (0x276d) · 1.12.1: — · Area: General
- Layout: `{ u32 guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PAY_DISTRIBUTION_UPDATE (0x276e)

- Modern: 10094 (0x276e) · 1.12.1: — · Area: General
- Layout: `{ { u64 u32*2 guid guid u32*2 u64 u8 opt[ { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 36: 1 · 1 · (6 unused)

### SMSG_BATTLE_PAY_DELIVERY_STARTED (0x276f)

- Modern: 10095 (0x276f) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PAY_DELIVERY_ENDED (0x2770)

- Modern: 10096 (0x2770) · 1.12.1: — · Area: General
- Layout: `{ u64 u32 alt[ ] loop[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] }`

### SMSG_BATTLE_PAY_MOUNT_DELIVERED (0x2771)

- Modern: 10097 (0x2771) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BATTLE_PAY_BATTLE_PET_DELIVERED (0x2772)

- Modern: 10098 (0x2772) · 1.12.1: — · Area: General
- Layout: `{ u32 guid }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PAY_COLLECTION_ITEM_DELIVERED (0x2773)

- Modern: 10099 (0x2773) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_INSTANCE_SAVE_CREATED (0x2775)

- Modern: 10101 (0x2775) · 1.12.1: 715 (0x2cb) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `InstanceSaveCreated` — matches

### SMSG_ENCOUNTER_START (0x2776)

- Modern: 10102 (0x2776) · 1.12.1: — · Area: General
- Layout: `{ u32*4 loop[ { guid u8 u32*3 u32*7 loop[ u32 ] loop[ u32 ] u32 loop[ u32 ] loop[ u32 ] loop[ guid u32 ] loop[ { u32*5 loop[ u32 ] loop[ u32 ] } ] u8 opt[ u32 u32 loop[ u32 u16 u8 ] loop[ u32 u8*2 ] ] opt[ u32*4 u32 loop[ u32 ] loop[ u32*2 ] loop[ u32*3 ] ] } ] }`

### SMSG_ENCOUNTER_END (0x2777)

- Modern: 10103 (0x2777) · 1.12.1: — · Area: General
- Layout: `{ u32*4 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)

### SMSG_BATTLE_PAY_START_PURCHASE_RESPONSE (0x2778)

- Modern: 10104 (0x2778) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +8: u32, +12: u32

### SMSG_BATTLE_PAY_START_DISTRIBUTION_ASSIGN_TO_TARGET_RESPONSE (0x2779)

- Modern: 10105 (0x2779) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +8: u32

### SMSG_BATTLE_PAY_PURCHASE_UPDATE (0x277b)

- Modern: 10107 (0x277b) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u64 u32*3 u64*2 u32 u8 bytes } ] }`

### SMSG_BATTLE_PAY_CONFIRM_PURCHASE (0x277c)

- Modern: 10108 (0x277c) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: 8 bytes, +8: u32

### SMSG_BATTLE_PAY_ACK_FAILED (0x277d)

- Modern: 10109 (0x277d) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +8: u32, +12: u32, +16: u32

### SMSG_CONQUEST_FORMULA_CONSTANTS (0x277e)

- Modern: 10110 (0x277e) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: u32, +4: u32, +8: u32, +12: u32, +16: u32
- HermesProxy: `ConquestFormulaConstants` — matches

### SMSG_CONTACT_LIST (0x2781)

- Modern: 10113 (0x2781) · 1.12.1: — · Area: General
- Layout: `{ { u32 u8 loop[ guid guid u32*3 u8 u32*3 u8*2 bytes ] } }`
- HermesProxy: `ContactList` — matches

### SMSG_FRIEND_STATUS (0x2782)

- Modern: 10114 (0x2782) · 1.12.1: 104 (0x68) · Area: General
- Layout: `{ u8 guid guid u32 u8 u32*3 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 10 · 1 · (5 unused)
- HermesProxy: `FriendStatusPkt` — matches

### SMSG_CHARACTER_OBJECT_TEST_RESPONSE (0x2783)

- Modern: 10115 (0x2783) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BATTLENET_CHALLENGE_START (0x2784)

- Modern: 10116 (0x2784) · 1.12.1: — · Area: General
- Layout: `{ u32 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 9 · (7 unused)

### SMSG_BATTLENET_CHALLENGE_ABORT (0x2785)

- Modern: 10117 (0x2785) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_UPDATE_TASK_PROGRESS (0x2786)

- Modern: 10118 (0x2786) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u32*5 loop[ u16 ] } ] }`

### SMSG_SET_ALL_TASK_PROGRESS (0x2787)

- Modern: 10119 (0x2787) · 1.12.1: — · Area: General
- Layout: `{ u32 loop[ { u32*5 loop[ u16 ] } ] }`
- HermesProxy: `SetAllTaskProgress` — matches

### SMSG_SET_TASK_COMPLETE (0x2788)

- Modern: 10120 (0x2788) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GROUP_DECLINE (0x2789)

- Modern: 10121 (0x2789) · 1.12.1: 116 (0x74) · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)
- HermesProxy: `GroupDecline` — matches

### SMSG_GROUP_UNINVITE (0x278a)

- Modern: 10122 (0x278a) · 1.12.1: 119 (0x77) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GroupUninvite` — matches

### SMSG_GROUP_DESTROYED (0x278b)

- Modern: 10123 (0x278b) · 1.12.1: 124 (0x7c) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GroupDestroyed` — matches

### SMSG_GROUP_AUTO_KICK (0x278c)

- Modern: 10124 (0x278c) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PARTY_COMMAND_RESULT (0x278d)

- Modern: 10125 (0x278d) · 1.12.1: 127 (0x7f) · Area: General
- Layout: `{ u8*3 u32 guid bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · 4 · 6 · (5 unused)
- HermesProxy: `PartyCommandResult` — matches

### SMSG_GOSSIP_POI (0x278e)

- Modern: 10126 (0x278e) · 1.12.1: 548 (0x224) · Area: General
- Layout: `{ u32*7 u8*3 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 28: 14 · 6 · (4 unused)
- HermesProxy: `GossipPOI` — matches

### SMSG_READ_ITEM_RESULT_OK (0x2797)

- Modern: 10135 (0x2797) · 1.12.1: 174 (0xae) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ReadItemResultOK` — matches

### SMSG_READ_ITEM_RESULT_FAILED (0x279f)

- Modern: 10143 (0x279f) · 1.12.1: 175 (0xaf) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 2 · (6 unused)
- HermesProxy: `ReadItemResultFailed` — matches

### SMSG_SCENARIO_VACATE (0x27a0)

- Modern: 10144 (0x27a0) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 2 · (6 unused)

### SMSG_SHOW_MAILBOX (0x27a1)

- Modern: 10145 (0x27a1) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CHAR_FACTION_CHANGE_RESULT (0x27a2)

- Modern: 10146 (0x27a2) · 1.12.1: — · Area: General
- Layout: `{ u8 guid u8 opt[ { u8*3 u32 bytes loop[ { u32*2 } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 3: 1 · (7 unused)

### SMSG_RAID_DIFFICULTY_SET (0x27a3)

- Modern: 10147 (0x27a3) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8
- HermesProxy: `RaidDifficultySet` — matches

### SMSG_XP_GAIN_ENABLED (0x27a4)

- Modern: 10148 (0x27a4) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_RAID_GROUP_ONLY (0x27a5)

- Modern: 10149 (0x27a5) · 1.12.1: 646 (0x286) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32
- HermesProxy: `RaidGroupOnly` — matches

### SMSG_INSTANCE_ENCOUNTER_ENGAGE_UNIT (0x27a6)

- Modern: 10150 (0x27a6) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_INSTANCE_ENCOUNTER_DISENGAGE_UNIT (0x27a7)

- Modern: 10151 (0x27a7) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_INSTANCE_ENCOUNTER_CHANGE_PRIORITY (0x27a8)

- Modern: 10152 (0x27a8) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_INSTANCE_ENCOUNTER_TIMER_START (0x27a9)

- Modern: 10153 (0x27a9) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_START (0x27aa)

- Modern: 10154 (0x27aa) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_COMPLETE (0x27ab)

- Modern: 10155 (0x27ab) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_INSTANCE_ENCOUNTER_START (0x27ac)

- Modern: 10156 (0x27ac) · 1.12.1: — · Area: General
- Layout: `{ u32*4 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)

### SMSG_INSTANCE_ENCOUNTER_UPDATE_SUPPRESS_RELEASE (0x27ad)

- Modern: 10157 (0x27ad) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_INSTANCE_ENCOUNTER_UPDATE_ALLOW_RELEASE_IN_PROGRESS (0x27ae)

- Modern: 10158 (0x27ae) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_UPDATE (0x27af)

- Modern: 10159 (0x27af) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_INSTANCE_ENCOUNTER_END (0x27b0)

- Modern: 10160 (0x27b0) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_INSTANCE_ENCOUNTER_IN_COMBAT_RESURRECTION (0x27b1)

- Modern: 10161 (0x27b1) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_INSTANCE_ENCOUNTER_GAIN_COMBAT_RESURRECTION_CHARGE (0x27b2)

- Modern: 10162 (0x27b2) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_INSTANCE_ENCOUNTER_PHASE_SHIFT_CHANGED (0x27b3)

- Modern: 10163 (0x27b3) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_TUTORIAL_FLAGS (0x27b4)

- Modern: 10164 (0x27b4) · 1.12.1: 253 (0xfd) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (32 bytes): +0: u32, +0: 8 bytes, +4: u32, +8: u32, +8: 8 bytes, +12: u32, +16: u32, +16: 8 bytes, +20: u32, +24: u32, +24: 8 bytes, +28: u32
- HermesProxy: `TutorialFlags` — matches

### SMSG_CHARACTER_UPGRADE_STARTED (0x27b5)

- Modern: 10165 (0x27b5) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CHARACTER_UPGRADE_COMPLETE (0x27b6)

- Modern: 10166 (0x27b6) · 1.12.1: — · Area: General
- Layout: `{ guid u32 loop[ u32 ] }`

### SMSG_CHARACTER_UPGRADE_ABORTED (0x27b7)

- Modern: 10167 (0x27b7) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CHARACTER_CHECK_UPGRADE_RESULT (0x27b8)

- Modern: 10168 (0x27b8) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_CHARACTER_UPGRADE_MANUAL_UNREVOKE_RESULT (0x27b9)

- Modern: 10169 (0x27b9) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_UPDATE_CHARACTER_FLAGS (0x27ba)

- Modern: 10170 (0x27ba) · 1.12.1: — · Area: General
- Layout: `{ guid u8 opt[ u32 ] opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · 1 · (5 unused)

### SMSG_ITEM_COOLDOWN (0x27be)

- Modern: 10174 (0x27be) · 1.12.1: 176 (0xb0) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `ItemCooldown` — matches

### SMSG_DAMAGE_CALC_LOG (0x27bf)

- Modern: 10175 (0x27bf) · 1.12.1: 636 (0x27c) · Area: General
- Layout: `{ guid guid u32*3 u8 loop[ u8 u32*3 bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)

### SMSG_EMOTE (0x27c0)

- Modern: 10176 (0x27c0) · 1.12.1: 259 (0x103) · Area: General
- Layout: `{ guid u32*2 loop[ u32 ] }`
- HermesProxy: `EmoteMessage` — matches

### SMSG_TRIGGER_CINEMATIC (0x27c1)

- Modern: 10177 (0x27c1) · 1.12.1: 250 (0xfa) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `TriggerCinematic` — matches

### SMSG_UPDATE_OBJECT (0x27c2)

- Modern: 10178 (0x27c2) · 1.12.1: 169 (0xa9) · Area: General
- Layout: `{ { u32 u16 u8 opt[ u16 u32 loop[ guid ] ] u32 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `UpdateObject`; `PetStableUpdate` — matches

### SMSG_REALM_LOOKUP_INFO (0x27c3)

- Modern: 10179 (0x27c3) · 1.12.1: — · Area: General
- Layout: `{ u32 bytes u32 bytes }`

### SMSG_UNDELETE_CHARACTER_RESPONSE (0x27c4)

- Modern: 10180 (0x27c4) · 1.12.1: — · Area: General
- Layout: `{ u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_UNDELETE_COOLDOWN_STATUS_RESPONSE (0x27c5)

- Modern: 10181 (0x27c5) · 1.12.1: — · Area: General
- Layout: `{ u8 u32*2 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SET_LOOT_METHOD_FAILED (0x27c9)

- Modern: 10185 (0x27c9) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_COMMERCE_TOKEN_GET_COUNT_RESPONSE (0x27ca)

- Modern: 10186 (0x27ca) · 1.12.1: — · Area: General
- Layout: `{ u32*4 loop[ u64 ] loop[ u64 ] }`

### SMSG_COMMERCE_TOKEN_UPDATE (0x27cb)

- Modern: 10187 (0x27cb) · 1.12.1: — · Area: General
- Layout: `{ u32*2 loop[ u64 ] loop[ u64 ] }`

### SMSG_COMMERCE_TOKEN_GET_MARKET_PRICE_RESPONSE (0x27cc)

- Modern: 10188 (0x27cc) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +12: u32, +16: u32

### — (0x27cd)

- Modern: 10189 (0x27cd) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +12: u32, +16: u32

### — (0x27ce)

- Modern: 10190 (0x27ce) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +4: u32

### — (0x27cf)

- Modern: 10191 (0x27cf) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CONSUMABLE_TOKEN_CAN_VETERAN_BUY_RESPONSE (0x27d0)

- Modern: 10192 (0x27d0) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CONSUMABLE_TOKEN_BUY_CHOICE_REQUIRED (0x27d1)

- Modern: 10193 (0x27d1) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (24 bytes): +0: 8 bytes, +12: u32, +16: u32, +20: u32

### SMSG_CONSUMABLE_TOKEN_BUY_AT_MARKET_PRICE_RESPONSE (0x27d2)

- Modern: 10194 (0x27d2) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +4: u32

### SMSG_GET_REMAINING_GAME_TIME_RESPONSE (0x27d3)

- Modern: 10195 (0x27d3) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_CONSUMABLE_TOKEN_REDEEM_CONFIRM_REQUIRED (0x27d4)

- Modern: 10196 (0x27d4) · 1.12.1: — · Area: General
- Layout: `{ u32*3 u64*2 u32 u8 }`
- Size: 33 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 32: 1 · 1 · (6 unused)

### SMSG_CONSUMABLE_TOKEN_REDEEM_RESPONSE (0x27d5)

- Modern: 10197 (0x27d5) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +4: u32, +8: u32

### SMSG_COMMERCE_TOKEN_GET_LOG_RESPONSE (0x27d6)

- Modern: 10198 (0x27d6) · 1.12.1: — · Area: General
- Layout: `{ u32*3 loop[ { u64*3 u32*2 } ] }`

### SMSG_SCENARIO_COMPLETED (0x27e5)

- Modern: 10213 (0x27e5) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x27e8)

- Modern: 10216 (0x27e8) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_GET_VAS_ACCOUNT_CHARACTER_LIST_RESULT (0x27e9)

- Modern: 10217 (0x27e9) · 1.12.1: — · Area: General
- Layout: `{ u32*3 loop[ { guid guid u32 u8*4 u64 u8*2 bytes bytes } ] }`

### SMSG_GET_VAS_TRANSFER_TARGET_REALM_LIST_RESULT (0x27ea)

- Modern: 10218 (0x27ea) · 1.12.1: — · Area: General
- Layout: `{ u32*4 loop[ { u32*6 u8 u32 u8*2 bytes } ] }`

### SMSG_VAS_PURCHASE_STATE_UPDATE (0x27eb)

- Modern: 10219 (0x27eb) · 1.12.1: — · Area: General
- Layout: `{ u32 { guid u32*2 u64 u8 loop[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 2 · (6 unused)

### SMSG_VAS_PURCHASE_COMPLETE (0x27ec)

- Modern: 10220 (0x27ec) · 1.12.1: — · Area: General
- Layout: `{ u32*2 guid guid u32 guid u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 6 · (2 unused)

### SMSG_ENUM_VAS_PURCHASE_STATES_RESPONSE (0x27ed)

- Modern: 10221 (0x27ed) · 1.12.1: — · Area: General
- Layout: `{ u8 alt[ ] loop[ { guid u32*2 u64 u8 loop[ u32 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · (2 unused)

### SMSG_ADVENTURE_MAP_OPEN_NPC (0x27ee)

- Modern: 10222 (0x27ee) · 1.12.1: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_SCENARIO_UI_UPDATE (0x27fa)

- Modern: 10234 (0x27fa) · 1.12.1: — · Area: General
- Layout: `{ u32*2 loop[ u32 u8 ] }`

### SMSG_SCENARIO_SHOW_CRITERIA (0x27fb)

- Modern: 10235 (0x27fb) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GAME_OBJECT_SET_STATE_LOCAL (0x27fd)

- Modern: 10237 (0x27fd) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_BATTLENET_RESPONSE (0x27fe)

- Modern: 10238 (0x27fe) · 1.12.1: — · Area: General
- Layout: `{ u32 { u64*2 u32 } u32 bytes }`
- HermesProxy: `BattlenetResponse` — matches

### SMSG_BATTLENET_NOTIFICATION (0x27ff)

- Modern: 10239 (0x27ff) · 1.12.1: — · Area: General
- Layout: `{ { u64*2 u32 } u32 bytes }`
- HermesProxy: `BattlenetNotification` — matches

### SMSG_BATTLE_NET_CONNECTION_STATUS (0x2800)

- Modern: 10240 (0x2800) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 2 · 1 · (5 unused)
- HermesProxy: `ConnectionStatus` — matches

### SMSG_CHANGE_REALM_TICKET_RESPONSE (0x2801)

- Modern: 10241 (0x2801) · 1.12.1: — · Area: General
- Layout: `{ u32 u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `ChangeRealmTicketResponse` — differs

### SMSG_FAILED_QUEST_TURN_IN (0x280a)

- Modern: 10250 (0x280a) · 1.12.1: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### SMSG_QUEUE_SUMMARY_UPDATE (0x280b)

- Modern: 10251 (0x280b) · 1.12.1: — · Area: General
- Layout: `{ u64*2 guid { guid guid u32 u8 loop[ { u8 opt[ opt[ opt[ u32*3 u8 u32 u8 ] { guid u32*2 u64 u32 guid guid guid guid guid u32*4 u32 u32 u32 u32 u64 u8 guid loop[ guid ] loop[ guid ] loop[ guid ] { loop[ u32 ] u32*2 u8*4 bytes bytes bytes opt[ u32 ] } loop[ guid u8*3 ] } ] u32*3 loop[ u32 ] loop[ u32 ] loop[ u8 ] u8 ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 26: 1 · 1 · (6 unused)

### SMSG_INVENTORY_FIXUP_COMPLETE (0x280c)

- Modern: 10252 (0x280c) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_CONFIRM_PARTY_INVITE (0x280d)

- Modern: 10253 (0x280d) · 1.12.1: — · Area: General
- Layout: `{ guid guid { u32 u32 u32 loop[ u32 ] loop[ u32 ] loop[ u64 ] } u64 u8 u32*2 u8*2 { u8*2 guid u32 u64 u8 bytes } bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 33: 9 · 1 · 1 · (5 unused) · 9 · 1 · 1 · (5 unused)

### SMSG_CAN_REDEEM_TOKEN_FOR_BALANCE_RESPONSE (0x280e)

- Modern: 10254 (0x280e) · 1.12.1: — · Area: General
- Layout: `{ u32 u64*2 u32 u8 }`
- Size: 25 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)

### SMSG_BATTLE_PAY_VALIDATE_PURCHASE_RESPONSE (0x280f)

- Modern: 10255 (0x280f) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u64*2 u8 }`
- Size: 25 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)

### SMSG_VAS_GET_SERVICE_STATUS_RESPONSE (0x2810)

- Modern: 10256 (0x2810) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 4

### SMSG_VAS_GET_QUEUE_MINUTES_RESPONSE (0x2811)

- Modern: 10257 (0x2811) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: 8 bytes, +8: u32

### SMSG_VAS_CHECK_TRANSFER_OK_RESPONSE (0x2813)

- Modern: 10259 (0x2813) · 1.12.1: — · Area: General
- Layout: `{ u32*2 guid u32 loop[ { guid u8*2 bytes } ] }`

### SMSG_CONTRIBUTION_LAST_UPDATE_RESPONSE (0x2814)

- Modern: 10260 (0x2814) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +8: u32, +12: u32

### SMSG_GENERATE_SSO_TOKEN_RESPONSE (0x2815)

- Modern: 10261 (0x2815) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u64*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 7 · (1 unused)

### SMSG_VOICE_LOGIN_RESPONSE (0x2816)

- Modern: 10262 (0x2816) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 u8*3 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 5: 1 · 5 · 1 · 1 · 4 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_VOICE_CHANNEL_INFO_RESPONSE (0x2817)

- Modern: 10263 (0x2817) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 u8 u64*2 guid u8*3 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 5 · 1 · 1 · 7 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_UPDATE_CELESTIAL_BODY (0x2818)

- Modern: 10264 (0x2818) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_WARDEN3_ENABLED (0x2819)

- Modern: 10265 (0x2819) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_WARDEN3_DISABLED (0x281a)

- Modern: 10266 (0x281a) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PAY_START_CHECKOUT (0x281b)

- Modern: 10267 (0x281b) · 1.12.1: — · Area: General
- Layout: `{ u32*2 u64 u8*2 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 6 · 7 · 1 · (2 unused)

### SMSG_UPDATE_BNET_SESSION_KEY (0x281c)

- Modern: 10268 (0x281c) · 1.12.1: — · Area: General
- Layout: `{ u8 loop[ u8 ] loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · (1 unused) · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### SMSG_INVENTORY_FULL_OVERFLOW (0x281d)

- Modern: 10269 (0x281d) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_WILL_BE_KICKED_FOR_ADDED_SUBSCRIPTION_TIME (0x281e)

- Modern: 10270 (0x281e) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_UPDATE_GAME_TIME_STATE (0x281f)

- Modern: 10271 (0x281f) · 1.12.1: — · Area: General
- Layout: `{ { u32*3 u8 } }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · 1 · 1 · (5 unused)

### SMSG_GAME_OBJECT_BASE (0x2822)

- Modern: 10274 (0x2822) · 1.12.1: — · Area: General
- Layout: `{ guid { u8 opt[ u8 opt[ u8 u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_LEGACY_LOOT_RULES (0x2823)

- Modern: 10275 (0x2823) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_BATCH_PRESENCE_SUBSCRIPTION (0x2853)

- Modern: 10323 (0x2853) · 1.12.1: — · Area: General
- Layout: `{ u8 u32 loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_MOVEMENT_ENFORCEMENT_ALERT (0x2854)

- Modern: 10324 (0x2854) · 1.12.1: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 12 · (4 unused)

### SMSG_PREPOPULATE_NAME_CACHE (0x2857)

- Modern: 10327 (0x2857) · 1.12.1: — · Area: General
- Layout: `{ u64 u32 loop[ { u8 loop[ opt[ u8 ] alt[ u8 ] ] loop[ bytes ] guid guid guid u64 u32 u8*5 bytes } ] }`

### — (0x2867)

- Modern: 10343 (0x2867) · 1.12.1: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x2868)

- Modern: 10344 (0x2868) · 1.12.1: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x2869)

- Modern: 10345 (0x2869) · 1.12.1: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_AUCTION_LIST_ITEMS_RESULT (0x2870)

- Modern: 10352 (0x2870) · 1.12.1: 604 (0x25c) · Area: General
- Layout: `{ u32*3 u8 loop[ { u8 u8 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 loop[ u32*3 u8 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] opt[ { { loop[ opt[ u8 ] alt[ u8 ] ] bits(4) } opt[ u8 ] opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] opt[ u16 ] opt[ u16 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)
- HermesProxy: `AuctionListItemsResult` — differs

### SMSG_ACCOUNT_CRITERIA_UPDATE (0x2875)

- Modern: 10357 (0x2875) · 1.12.1: — · Area: General
- Layout: `{ { u32 u64 guid { u32 } u64*2 u8 opt[ u64 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 34: 4 · 1 · (3 unused)

### SMSG_SYNC_WOW_ENTITLEMENTS (0x2878)

- Modern: 10360 (0x2878) · 1.12.1: — · Area: General
- Layout: `{ u32 u32 loop[ { u32 u64*2 u32 u8 } ] loop[ { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] }`

### SMSG_WOW_ENTITLEMENT_NOTIFICATION (0x2879)

- Modern: 10361 (0x2879) · 1.12.1: — · Area: General
- Layout: `{ { u8 u32 u64*2 u32 u8 } { u32 u8 u32*10 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*12 u32 u32*2 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 3 · (5 unused); byte 25: 1 · (7 unused); byte 71: 8 · 1 · 1 · 7 · 1 · (6 unused)

### — (0x287d)

- Modern: 10365 (0x287d) · 1.12.1: — · Area: General
- Layout: `{ u32*3 u32 loop[ { u32*6 u8 u32 u8*2 bytes } ] }`

### SMSG_PARTY_NOTIFY_LFG_LEADER_CHANGE (0x2885)

- Modern: 10373 (0x2885) · 1.12.1: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT (0x288a)

- Modern: 10378 (0x288a) · 1.12.1: 605 (0x25d) · Area: General
- Layout: `{ u32*3 loop[ { u8 u8 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 loop[ u32*3 u8 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] opt[ { { loop[ opt[ u8 ] alt[ u8 ] ] bits(4) } opt[ u8 ] opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] opt[ u16 ] opt[ u16 ] } ] } ] }`

### SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT (0x288b)

- Modern: 10379 (0x288b) · 1.12.1: 613 (0x265) · Area: General
- Layout: `{ u32*3 loop[ { u8 u8 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 loop[ u32*3 u8 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] opt[ { { loop[ opt[ u8 ] alt[ u8 ] ] bits(4) } opt[ u8 ] opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] opt[ u16 ] opt[ u16 ] } ] } ] }`

### SMSG_AREA_TRIGGER_MESSAGE (0x288e)

- Modern: 10382 (0x288e) · 1.12.1: 696 (0x2b8) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `AreaTriggerMessage` — matches

### SMSG_VOICE_CHANNEL_STT_TOKEN_RESPONSE (0x2890)

- Modern: 10384 (0x2890) · 1.12.1: — · Area: General
- Layout: `{ u32 u8*2 { opt[ bytes ] } { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 5 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2891)

- Modern: 10385 (0x2891) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_UPDATE_AADC_STATUS_RESPONSE (0x2892)

- Modern: 10386 (0x2892) · 1.12.1: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)
