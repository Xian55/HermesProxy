# General — server packet layouts, 4.4.2.60895

### SMSG_AUTH_FAILED (0x3b0000)

- Modern: 3866624 (0x3b0000) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_AUTH_RESPONSE (0x3b0001)

- Modern: 3866625 (0x3b0001) · 4.3.4: 23990 (0x5db6) · Area: General
- Layout: `{ u32 u8 opt[ { u32*3 u8*2 u32*4 u64 loop[ { u8 u32 loop[ u8*4 ] } ] u8 { u32*3 u8 } opt[ u16 ] opt[ u16 ] opt[ u64 ] opt[ { loop[ u8*2 ] } ] loop[ { u32 u8*3 bytes bytes } ] loop[ u32*2 loop[ u8*2 ] u8*3 bytes bytes ] } ] opt[ u32*3 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_WAIT_QUEUE_UPDATE (0x3b0002)

- Modern: 3866626 (0x3b0002) · 4.3.4: 22689 (0x58a1) · Area: General
- Layout: `{ u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · 1 · (6 unused)

### SMSG_WAIT_QUEUE_FINISH (0x3b0003)

- Modern: 3866627 (0x3b0003) · 4.3.4: 30135 (0x75b7) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_ALL_ACHIEVEMENT_DATA (0x3b0004)

- Modern: 3866628 (0x3b0004) · 4.3.4: 22705 (0x58b1) · Area: General
- Layout: `{ u32*2 loop[ { u32 { u32 } guid u32*2 } ] loop[ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } ] }`

### SMSG_ALL_ACCOUNT_CRITERIA (0x3b0005)

- Modern: 3866629 (0x3b0005) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } ] }`

### SMSG_RESPOND_INSPECT_ACHIEVEMENTS (0x3b0006)

- Modern: 3866630 (0x3b0006) · 4.3.4: 5552 (0x15b0) · Area: General
- Layout: `{ guid { u32*2 loop[ { u32 { u32 } guid u32*2 } ] loop[ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } ] } }`

### SMSG_SETUP_CURRENCY (0x3b0007)

- Modern: 3866631 (0x3b0007) · 4.3.4: 5541 (0x15a5) · Area: General
- Layout: `{ u32 loop[ { u32*2 u8*2 opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u64 ] } ] }`

### SMSG_SET_CURRENCY (0x3b0008)

- Modern: 3866632 (0x3b0008) · 4.3.4: — · Area: General
- Layout: `{ { u32*4 loop[ { u32*2 } ] u8*2 opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u64 ] opt[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (4 unused)

### SMSG_RESET_WEEKLY_CURRENCY (0x3b0009)

- Modern: 3866633 (0x3b0009) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_MESSAGE_BOX (0x3b000a)

- Modern: 3866634 (0x3b000a) · 4.3.4: 12449 (0x30a1) · Area: General
- Layout: `{ u32 bytes }`

### SMSG_WARDEN3_DATA (0x3b000b)

- Modern: 3866635 (0x3b000b) · 4.3.4: — · Area: General
- Layout: `{ u32 bytes }`

### SMSG_PHASE_SHIFT_CHANGE (0x3b000c)

- Modern: 3866636 (0x3b000c) · 4.3.4: 28832 (0x70a0) · Area: General
- Layout: `{ { guid { u32*2 guid loop[ u32 u16 ] } u32 bytes u32 bytes u32 bytes } }`

### SMSG_PRELOAD_CHILD_MAP (0x3b000d)

- Modern: 3866637 (0x3b000d) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_UNLOAD_CHILD_MAP (0x3b000e)

- Modern: 3866638 (0x3b000e) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_MOUNT_RESULT (0x3b000f)

- Modern: 3866639 (0x3b000f) · 4.3.4: 8741 (0x2225) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_DISMOUNT_RESULT (0x3b0010)

- Modern: 3866640 (0x3b0010) · 4.3.4: 3365 (0xd25) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BIND_POINT_UPDATE (0x3b0011)

- Modern: 3866641 (0x3b0011) · 4.3.4: 1319 (0x527) · Area: General
- Layout: `{ u32*5 }`
- Size: 20 bytes (packed GUIDs not counted)

### SMSG_RESURRECT_REQUEST (0x3b0012)

- Modern: 3866642 (0x3b0012) · 4.3.4: 10501 (0x2905) · Area: General
- Layout: `{ guid u32*3 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · 1 · 1 · (3 unused)

### SMSG_INITIAL_SETUP (0x3b0014)

- Modern: 3866644 (0x3b0014) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (2 bytes): +0: u8, +1: u8

### SMSG_REFORGE_RESULT (0x3b0015)

- Modern: 3866645 (0x3b0015) · 4.3.4: 22692 (0x58a4) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_TRADE_UPDATED (0x3b0016)

- Modern: 3866646 (0x3b0016) · 4.3.4: — · Area: General
- Layout: `{ { u8 u32*3 u64 u32*4 loop[ u8 u32 guid { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u8 opt[ u32*2 guid u32*3 u8 loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] ] ] } }`

### SMSG_TRADE_STATUS (0x3b0017)

- Modern: 3866647 (0x3b0017) · 4.3.4: 23715 (0x5ca3) · Area: General
- Layout: `{ u8 opt[ u32*2 ] opt[ u32 ] opt[ guid guid ] opt[ u8 ] opt[ u32*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 5 · (2 unused)

### SMSG_ENUM_CHARACTERS_RESULT (0x3b0018)

- Modern: 3866648 (0x3b0018) · 4.3.4: 4272 (0x10b0) · Area: General
- Layout: `{ { u8*2 u32*7 opt[ u32 ] loop[ { u32*2 } ] loop[ u32*2 ] loop[ { u64 u8 u32*2 loop[ u32*2 opt[ guid ] ] } ] loop[ { guid u32 u8*4 u16 u32 u8 u32*5 u64 guid u32*3 u8 u32*3 loop[ u32 u8 u32 u8 u32*3 ] u32 u64 u32 { u32*5 } u32*4 loop[ { u32*2 } ] u8 bytes } { u8 u32*3 loop[ u32 ] loop[ u8 ] loop[ { bytes } ] } ] loop[ { guid u32 u8*4 u16 u32 u8 u32*5 u64 guid u32*3 u8 u32*3 loop[ u32 u8 u32 u8 u32*3 ] u32 u64 u32 { u32*5 } u32*4 loop[ { u32*2 } ] u8 bytes } u64 u32*2 u8 u16 ] loop[ u32 u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (7 unused)

### SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT (0x3b001c)

- Modern: 3866652 (0x3b001c) · 4.3.4: 14513 (0x38b1) · Area: General
- Layout: `{ u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)

### SMSG_ARCHAEOLOGY_SURVERY_CAST (0x3b001d)

- Modern: 3866653 (0x3b001d) · 4.3.4: — · Area: General
- Layout: `{ u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_PET_NEWLY_TAMED (0x3b001e)

- Modern: 3866654 (0x3b001e) · 4.3.4: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_PET_MODE (0x3b001f)

- Modern: 3866655 (0x3b001f) · 4.3.4: 8757 (0x2235) · Area: General
- Layout: `{ guid u8*3 }`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8 · 8

### SMSG_DIFFERENT_INSTANCE_FROM_PARTY (0x3b0020)

- Modern: 3866656 (0x3b0020) · 4.3.4: 5553 (0x15b1) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_ROLE_CHANGED_INFORM (0x3b0021)

- Modern: 3866657 (0x3b0021) · 4.3.4: — · Area: General
- Layout: `{ u8 guid guid u8*2 }`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 5: 8 · 8

### SMSG_ROLE_POLL_INFORM (0x3b0022)

- Modern: 3866658 (0x3b0022) · 4.3.4: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_TALENT_GROUP_ROLE_CHANGED (0x3b0023)

- Modern: 3866659 (0x3b0023) · 4.3.4: — · Area: General
- Layout: `{ u8*2 }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8

### SMSG_SUMMON_RAID_MEMBER_VALIDATE_FAILED (0x3b0024)

- Modern: 3866660 (0x3b0024) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ guid u32 ] }`

### SMSG_GROUP_ACTION_THROTTLED (0x3b0025)

- Modern: 3866661 (0x3b0025) · 4.3.4: 25892 (0x6524) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_REQUEST_CEMETERY_LIST_RESPONSE (0x3b0026)

- Modern: 3866662 (0x3b0026) · 4.3.4: 12455 (0x30a7) · Area: General
- Layout: `{ u8 u32 loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SET_FORGE_MASTER (0x3b0027)

- Modern: 3866663 (0x3b0027) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CHECK_WARGAME_ENTRY (0x3b0028)

- Modern: 3866664 (0x3b0028) · 4.3.4: — · Area: General
- Layout: `{ guid u64*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_PET_STABLE_RESULT (0x3b002b)

- Modern: 3866667 (0x3b002b) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8

### SMSG_NEW_WORLD (0x3b002c)

- Modern: 3866668 (0x3b002c) · 4.3.4: 31153 (0x79b1) · Area: General
- Layout: `{ u32*10 }`
- Size: 40 bytes (packed GUIDs not counted)

### SMSG_PRELOAD_WORLD (0x3b002d)

- Modern: 3866669 (0x3b002d) · 4.3.4: — · Area: General
- Layout: `{ u32 { u32*6 } u32*4 u8 }`
- Size: 45 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 44: 1 · (7 unused)

### SMSG_CANCEL_PRELOAD_WORLD (0x3b002f)

- Modern: 3866671 (0x3b002f) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_LOGIN_VERIFY_WORLD (0x3b0030)

- Modern: 3866672 (0x3b0030) · 4.3.4: 8197 (0x2005) · Area: General
- Layout: `{ u32*6 }`
- Size: 24 bytes (packed GUIDs not counted)

### SMSG_ABORT_NEW_WORLD (0x3b0031)

- Modern: 3866673 (0x3b0031) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_NOTIFY_MONEY (0x3b0032)

- Modern: 3866674 (0x3b0032) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes

### SMSG_ITEM_PURCHASE_REFUND_RESULT (0x3b0033)

- Modern: 3866675 (0x3b0033) · 4.3.4: 23985 (0x5db1) · Area: General
- Layout: `{ guid u8*2 opt[ { u64 u32*20 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 1 · (7 unused)

### SMSG_SET_ITEM_PURCHASE_DATA (0x3b0034)

- Modern: 3866676 (0x3b0034) · 4.3.4: — · Area: General
- Layout: `{ guid { u64 u32*20 } u32*2 }`
- Size: 96 bytes (packed GUIDs not counted)

### SMSG_ITEM_EXPIRE_PURCHASE_REFUND (0x3b0035)

- Modern: 3866677 (0x3b0035) · 4.3.4: 7328 (0x1ca0) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DISPLAY_GAME_ERROR (0x3b0036)

- Modern: 3866678 (0x3b0036) · 4.3.4: 12710 (0x31a6) · Area: General
- Layout: `{ u32 u8 opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_SET_MAX_WEEKLY_QUANTITY (0x3b0037)

- Modern: 3866679 (0x3b0037) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_PETITION_ALREADY_SIGNED (0x3b0038)

- Modern: 3866680 (0x3b0038) · 4.3.4: 23971 (0x5da3) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_RAID_MARKERS_CHANGED (0x3b0039)

- Modern: 3866681 (0x3b0039) · 4.3.4: 4257 (0x10a1) · Area: General
- Layout: `{ u8 u32 u8 loop[ { guid u32*4 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 5: 4 · (4 unused)

### SMSG_STREAMING_MOVIES (0x3b003d)

- Modern: 3866685 (0x3b003d) · 4.3.4: 5559 (0x15b7) · Area: General
- Layout: `{ u32 loop[ u16 ] }`

### SMSG_START_TIMER (0x3b003e)

- Modern: 3866686 (0x3b003e) · 4.3.4: 22949 (0x59a5) · Area: General
- Layout: `{ u64 u32 u64 u8 opt[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)

### SMSG_DISENCHANT_CREDIT (0x3b0040)

- Modern: 3866688 (0x3b0040) · 4.3.4: 21922 (0x55a2) · Area: General
- Layout: `{ guid { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused) · 6 · (2 unused)

### SMSG_SUSPEND_TOKEN (0x3b0041)

- Modern: 3866689 (0x3b0041) · 4.3.4: 5297 (0x14b1) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)

### SMSG_RESUME_TOKEN (0x3b0042)

- Modern: 3866690 (0x3b0042) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)

### SMSG_ADD_ITEM_PASSIVE (0x3b0043)

- Modern: 3866691 (0x3b0043) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_REMOVE_ITEM_PASSIVE (0x3b0044)

- Modern: 3866692 (0x3b0044) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SEND_ITEM_PASSIVES (0x3b0045)

- Modern: 3866693 (0x3b0045) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ u32 ] }`

### SMSG_WORLD_SERVER_INFO (0x3b0046)

- Modern: 3866694 (0x3b0046) · 4.3.4: 12706 (0x31a2) · Area: General
- Layout: `{ u32 u8 opt[ u32 ] opt[ u64 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · 1 · 1 · 1 · (3 unused)

### SMSG_ACCOUNT_MOUNT_UPDATE (0x3b0047)

- Modern: 3866695 (0x3b0047) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 loop[ u32 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_ACCOUNT_MOUNT_REMOVED (0x3b0048)

- Modern: 3866696 (0x3b0048) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_ACCOUNT_TOY_UPDATE (0x3b0049)

- Modern: 3866697 (0x3b0049) · 4.3.4: — · Area: General
- Layout: `{ u8 u32*3 loop[ u32 ] loop[ u8 ] loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_FORCE_RANDOM_TRANSMOG_TOAST (0x3b004b)

- Modern: 3866699 (0x3b004b) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_ACCOUNT_TRANSMOG_UPDATE (0x3b004c)

- Modern: 3866700 (0x3b004c) · 4.3.4: — · Area: General
- Layout: `{ u8 u32*2 loop[ u32 ] loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_ACCOUNT_TRANSMOG_SET_FAVORITES_UPDATE (0x3b004d)

- Modern: 3866701 (0x3b004d) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_RUNE_REGEN_DEBUG (0x3b004f)

- Modern: 3866703 (0x3b004f) · 4.3.4: 12723 (0x31b3) · Area: General
- Layout: `{ u32*5 loop[ u32 ] loop[ u32 ] }`

### SMSG_VENDOR_INVENTORY (0x3b0051)

- Modern: 3866705 (0x3b0051) · 4.3.4: 31920 (0x7cb0) · Area: General
- Layout: `{ guid u32*2 loop[ { u64 u32*6 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] }`

### SMSG_SET_PLAY_HOVER_ANIM (0x3b0053)

- Modern: 3866707 (0x3b0053) · 4.3.4: 12454 (0x30a6) · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_CLEAR_BOSS_EMOTES (0x3b0054)

- Modern: 3866708 (0x3b0054) · 4.3.4: 6563 (0x19a3) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOAD_CUF_PROFILES (0x3b0055)

- Modern: 3866709 (0x3b0055) · 4.3.4: 20657 (0x50b1) · Area: General
- Layout: `{ u32 loop[ { u8*4 u16*2 u8*5 u16*3 bytes } ] }`

### SMSG_PARTY_INVITE (0x3b0056)

- Modern: 3866710 (0x3b0056) · 4.3.4: — · Area: General
- Layout: `{ { u8*2 { u32 u8*3 bytes bytes } guid guid u16 u8 u32*2 bytes loop[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 6 · 1 · (3 unused); byte 6: 1 · 1 · 8 · 8 · (6 unused)

### SMSG_FEATURE_SYSTEM_STATUS (0x3b0058)

- Modern: 3866712 (0x3b0058) · 4.3.4: 15799 (0x3db7) · Area: General
- Layout: `{ { u8 u32*9 u64 u32*7 u16*2 u32*5 loop[ { u32*3 } ] u8*8 { u8 u32*22 } opt[ u32*3 ] opt[ u32 loop[ u8 ] ] bytes { u8 guid guid } opt[ { u8 u32*4 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 97: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 7 · 1 · 1 · 1 · 1 · 1 · 1 · (3 unused) · 1 · (7 unused); byte 194: 1 · (7 unused)

### SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN (0x3b0059)

- Modern: 3866713 (0x3b0059) · 4.3.4: — · Area: General
- Layout: `{ { u8*6 opt[ { u8 u32*4 } ] u32*2 u64 u32*11 u16*2 u32*4 opt[ u32 ] { bytes } loop[ u32 ] loop[ { u32*3 } ] loop[ { u32 u8 bytes } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 8 · 1 · (1 unused) · 1 · 1 · 1 · 1 · (1 unused)

### SMSG_PVP_SEASON (0x3b005a)

- Modern: 3866714 (0x3b005a) · 4.3.4: — · Area: General
- Layout: `{ { u32*7 u8 } }`
- Size: 29 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 28: 1 · 1 · 1 · (5 unused)

### SMSG_GAME_OBJECT_ACTIVATE_ANIM_KIT (0x3b005c)

- Modern: 3866716 (0x3b005c) · 4.3.4: 5283 (0x14a3) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_GAME_OBJECT_CUSTOM_ANIM (0x3b005d)

- Modern: 3866717 (0x3b005d) · 4.3.4: 18742 (0x4936) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_GAME_OBJECT_DESPAWN (0x3b005e)

- Modern: 3866718 (0x3b005e) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MAP_OBJ_EVENTS (0x3b005f)

- Modern: 3866719 (0x3b005f) · 4.3.4: 21682 (0x54b2) · Area: General
- Layout: `{ u32*2 bytes }`

### SMSG_MISSILE_CANCEL (0x3b0060)

- Modern: 3866720 (0x3b0060) · 4.3.4: 15796 (0x3db4) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_XP_GAIN_ABORTED (0x3b0062)

- Modern: 3866722 (0x3b0062) · 4.3.4: 20660 (0x50b4) · Area: General
- Layout: `{ guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_PRINT_NOTIFICATION (0x3b0063)

- Modern: 3866723 (0x3b0063) · 4.3.4: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 12 · (4 unused)

### SMSG_CUSTOM_LOAD_SCREEN (0x3b0064)

- Modern: 3866724 (0x3b0064) · 4.3.4: 7606 (0x1db6) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_SPELL_VISUAL_LOAD_SCREEN (0x3b0065)

- Modern: 3866725 (0x3b0065) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_TRANSFER_PENDING (0x3b0066)

- Modern: 3866726 (0x3b0066) · 4.3.4: 6310 (0x18a6) · Area: General
- Layout: `{ u32*4 u8 opt[ u32*2 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · 1 · (6 unused)

### SMSG_ADJUST_SPLINE_DURATION (0x3b0069)

- Modern: 3866729 (0x3b0069) · 4.3.4: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_TRAIT_CONFIG_COMMIT_FAILED (0x3b006a)

- Modern: 3866730 (0x3b006a) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 4 · (4 unused)

### SMSG_LEARN_TALENT_FAILED (0x3b006c)

- Modern: 3866732 (0x3b006c) · 4.3.4: — · Area: General
- Layout: `{ u8 u32*2 loop[ u16 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_LEARN_PVP_TALENT_FAILED (0x3b006d)

- Modern: 3866733 (0x3b006d) · 4.3.4: — · Area: General
- Layout: `{ u8 u32*2 loop[ { u16 u8 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### — (0x3b006f)

- Modern: 3866735 (0x3b006f) · 4.3.4: — · Area: General
- Layout: `{ u8 u32*2 loop[ u32*3 loop[ u16 ] loop[ { u16 u8 } ] ] }`

### — (0x3b0070)

- Modern: 3866736 (0x3b0070) · 4.3.4: — · Area: General
- Layout: `{ { u32 u8 u32 loop[ u8 u32 u8 u32 u8 u32 loop[ u32*2 ] loop[ u16 ] ] u8 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 9: 1 · (7 unused)

### SMSG_UPDATE_PRIMARY_SPEC (0x3b0071)

- Modern: 3866737 (0x3b0071) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (2 bytes): +0: u16

### SMSG_SHOW_NEUTRAL_PLAYER_FACTION_SELECT_UI (0x3b0074)

- Modern: 3866740 (0x3b0074) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_NEUTRAL_PLAYER_FACTION_SELECT_RESULT (0x3b0075)

- Modern: 3866741 (0x3b0075) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SOR_START_EXPERIENCE_INCOMPLETE (0x3b0076)

- Modern: 3866742 (0x3b0076) · 4.3.4: 31911 (0x7ca7) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_SET_CHR_UPGRADE_TIER (0x3b0078)

- Modern: 3866744 (0x3b0078) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_UPDATE_ACTION_BUTTONS (0x3b0079)

- Modern: 3866745 (0x3b0079) · 4.3.4: 14517 (0x38b5) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1441 bytes): +1440: u8

### SMSG_DONT_AUTO_PUSH_SPELLS_TO_ACTION_BAR (0x3b007a)

- Modern: 3866746 (0x3b007a) · 4.3.4: 14498 (0x38a2) · Area: General
- Layout: `{ struct }`

### SMSG_SCENE_OBJECT_EVENT (0x3b007b)

- Modern: 3866747 (0x3b007b) · 4.3.4: — · Area: General
- Layout: `{ guid { u8 bytes } }`

### SMSG_SCENE_OBJECT_PET_BATTLE_INITIAL_UPDATE (0x3b007c)

- Modern: 3866748 (0x3b007c) · 4.3.4: — · Area: General
- Layout: `{ guid { loop[ guid u32*2 u16 u8*3 loop[ { guid u32*3 u16*2 u32*5 u8 u16 u8 u32*3 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] bits(7) bytes } ] ] loop[ u32*2 loop[ { u32*4 u8 } ] loop[ u32*2 ] opt[ u16*2 u32*3 u8*2 guid u8 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 8 · 8 · 2 · (6 unused); byte 29: 8 · 8 · 2 · (6 unused); byte 72: 8 · 8; byte 76: 1 · 1 · (6 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_FIRST_ROUND (0x3b007d)

- Modern: 3866749 (0x3b007d) · 4.3.4: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u8 u16 u8 u32*3 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] bits(7) bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_ROUND_RESULT (0x3b007e)

- Modern: 3866750 (0x3b007e) · 4.3.4: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u8 u16 u8 u32*3 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] bits(7) bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_REPLACEMENTS_MADE (0x3b007f)

- Modern: 3866751 (0x3b007f) · 4.3.4: — · Area: General
- Layout: `{ guid { u32 u8 u32 loop[ u8*2 u16 ] u32 loop[ { u32 u16*2 u8*2 } ] u8 loop[ u32 u16*3 u8*3 u32 loop[ { u8 opt[ { guid u32*3 u16*2 u32*5 u8 u16 u8 u32*3 loop[ { u32 u16*2 u8*2 } ] loop[ { u32*4 u8 } ] loop[ u32*2 ] bits(7) bytes } ] u8 opt[ u32*4 ] opt[ u32*2 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32*3 ] opt[ u32 ] opt[ u32*2 ] } ] ] loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 8 · 8; byte 15: 8 · 8; byte 23: 3 · (5 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_FINAL_ROUND (0x3b0080)

- Modern: 3866752 (0x3b0080) · 4.3.4: — · Area: General
- Layout: `{ guid { u8 loop[ u8 ] loop[ u32 ] u32 loop[ guid u16*2 u32*2 u16 u8*2 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · 1 · 1 · (4 unused)

### SMSG_SCENE_OBJECT_PET_BATTLE_FINISHED (0x3b0081)

- Modern: 3866753 (0x3b0081) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PET_UPDATES (0x3b0083)

- Modern: 3866755 (0x3b0083) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 loop[ { guid u32*3 u16*4 u32*4 u8*3 bytes opt[ guid u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_BATTLE_PET_TRAP_LEVEL (0x3b0084)

- Modern: 3866756 (0x3b0084) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PET_BATTLE_SLOT_UPDATES (0x3b0085)

- Modern: 3866757 (0x3b0085) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 loop[ guid u32 u8*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_BATTLE_PET_JOURNAL_LOCK_ACQUIRED (0x3b0086)

- Modern: 3866758 (0x3b0086) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PET_JOURNAL_LOCK_DENIED (0x3b0087)

- Modern: 3866759 (0x3b0087) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PET_JOURNAL (0x3b0088)

- Modern: 3866760 (0x3b0088) · 4.3.4: — · Area: General
- Layout: `{ u16 u32*2 u8 loop[ guid u32 u8*2 ] loop[ { guid u32*3 u16*4 u32*4 u8*3 bytes opt[ guid u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · (7 unused)

### SMSG_BATTLE_PET_DELETED (0x3b0089)

- Modern: 3866761 (0x3b0089) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PET_REVOKED (0x3b008a)

- Modern: 3866762 (0x3b008a) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PET_RESTORED (0x3b008b)

- Modern: 3866763 (0x3b008b) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PETS_HEALED (0x3b008c)

- Modern: 3866764 (0x3b008c) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PARTY_UPDATE (0x3b008d)

- Modern: 3866765 (0x3b008d) · 4.3.4: — · Area: General
- Layout: `{ { u16 u8*2 u32 guid u32 guid u8 u32*2 u8 loop[ u8*2 guid u8*5 bytes { bytes } ] opt[ { u8 guid u8 } ] opt[ u32 loop[ u32 ] ] opt[ { u8 u32*2 u8 u32 u8*4 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8; byte 25: 1 · 1 · 1 · (5 unused)

### SMSG_READY_CHECK_STARTED (0x3b008f)

- Modern: 3866767 (0x3b008f) · 4.3.4: — · Area: General
- Layout: `{ u8 guid guid u64 }`
- Size: 9 bytes (packed GUIDs not counted)

### SMSG_READY_CHECK_RESPONSE (0x3b0090)

- Modern: 3866768 (0x3b0090) · 4.3.4: — · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_READY_CHECK_COMPLETED (0x3b0091)

- Modern: 3866769 (0x3b0091) · 4.3.4: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_START_ELAPSED_TIMER (0x3b009d)

- Modern: 3866781 (0x3b009d) · 4.3.4: — · Area: General
- Layout: `{ { u64 u32 } }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_STOP_ELAPSED_TIMER (0x3b009e)

- Modern: 3866782 (0x3b009e) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_START_ELAPSED_TIMERS (0x3b009f)

- Modern: 3866783 (0x3b009f) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ { u64 u32 } ] }`

### SMSG_RESPEC_WIPE_CONFIRM (0x3b00ab)

- Modern: 3866795 (0x3b00ab) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 guid }`
- Size: 5 bytes (packed GUIDs not counted)

### SMSG_LOOT_RESPONSE (0x3b00ad)

- Modern: 3866797 (0x3b00ad) · 4.3.4: 19478 (0x4c16) · Area: General
- Layout: `{ { guid guid u8*4 u32*3 u8 loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } ] loop[ u32*2 u8*2 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 8 · 8 · 8 · 8; byte 20: 1 · 1 · 1 · (5 unused)

### SMSG_LOOT_REMOVED (0x3b00ae)

- Modern: 3866798 (0x3b00ae) · 4.3.4: 26647 (0x6817) · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_COIN_REMOVED (0x3b00b0)

- Modern: 3866800 (0x3b00b0) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_AE_LOOT_TARGETS (0x3b00b1)

- Modern: 3866801 (0x3b00b1) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_AE_LOOT_TARGET_ACK (0x3b00b2)

- Modern: 3866802 (0x3b00b2) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOOT_RELEASE_ALL (0x3b00b3)

- Modern: 3866803 (0x3b00b3) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOOT_RELEASE (0x3b00b4)

- Modern: 3866804 (0x3b00b4) · 4.3.4: 27941 (0x6d25) · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_LOOT_MONEY_NOTIFY (0x3b00b5)

- Modern: 3866805 (0x3b00b5) · 4.3.4: 10294 (0x2836) · Area: General
- Layout: `{ u64*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)

### SMSG_START_LOOT_ROLL (0x3b00b6)

- Modern: 3866806 (0x3b00b6) · 4.3.4: — · Area: General
- Layout: `{ guid u32*2 u8 loop[ u32 ] u8 u32 { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 28: 2 · 3 · 1 · (2 unused); byte 41: 1 · (7 unused) · 6 · (2 unused); byte 47: 8 · 8

### SMSG_LOOT_ROLL (0x3b00b7)

- Modern: 3866807 (0x3b00b7) · 4.3.4: 25863 (0x6507) · Area: General
- Layout: `{ guid guid u32 u8 u32 { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 13: 2 · 3 · 1 · (2 unused); byte 26: 1 · (7 unused) · 6 · (2 unused); byte 32: 8 · 8 · 1 · 1 · (6 unused)

### SMSG_MASTER_LOOT_CANDIDATE_LIST (0x3b00b8)

- Modern: 3866808 (0x3b00b8) · 4.3.4: — · Area: General
- Layout: `{ guid u32 loop[ guid ] }`

### SMSG_LOOT_ROLLS_COMPLETE (0x3b00b9)

- Modern: 3866809 (0x3b00b9) · 4.3.4: — · Area: General
- Layout: `{ guid u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)

### SMSG_LOOT_ALL_PASSED (0x3b00ba)

- Modern: 3866810 (0x3b00ba) · 4.3.4: 25143 (0x6237) · Area: General
- Layout: `{ guid u32 { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 2 · 3 · 1 · (2 unused); byte 19: 1 · (7 unused) · 6 · (2 unused); byte 25: 8 · 8

### SMSG_LOOT_ROLL_WON (0x3b00bb)

- Modern: 3866811 (0x3b00bb) · 4.3.4: 26135 (0x6617) · Area: General
- Layout: `{ guid guid u32 u8 u32 { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8*2 } u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 13: 2 · 3 · 1 · (2 unused); byte 26: 1 · (7 unused) · 6 · (2 unused); byte 32: 8 · 8 · 1 · (7 unused)

### SMSG_ITEM_PUSH_RESULT (0x3b00bc)

- Modern: 3866812 (0x3b00bc) · 4.3.4: 3605 (0xe15) · Area: General
- Layout: `{ guid u8 u32*7 u8 u32 guid u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 38: 1 · 1 · 1 · 3 · 1 · 1; byte 51: 1 · (7 unused) · 6 · (2 unused)

### SMSG_DISPLAY_TOAST (0x3b00bd)

- Modern: 3866813 (0x3b00bd) · 4.3.4: — · Area: General
- Layout: `{ u64 u32*2 u8 opt[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · 2 · 1 · 1 · (3 unused); byte 29: 1 · (7 unused) · 6 · (2 unused)

### SMSG_SET_PET_SPECIALIZATION (0x3b00be)

- Modern: 3866814 (0x3b00be) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (2 bytes): +0: u16

### SMSG_BLACK_MARKET_REQUEST_ITEMS_RESULT (0x3b00c0)

- Modern: 3866816 (0x3b00c0) · 4.3.4: — · Area: General
- Layout: `{ u64 u32 loop[ u32*3 u64*3 u32*2 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u8 ] }`

### SMSG_BLACK_MARKET_BID_ON_ITEM_RESULT (0x3b00c1)

- Modern: 3866817 (0x3b00c1) · 4.3.4: — · Area: General
- Layout: `{ u32*2 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### SMSG_BLACK_MARKET_OUTBID (0x3b00c2)

- Modern: 3866818 (0x3b00c2) · 4.3.4: — · Area: General
- Layout: `{ u32*2 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### SMSG_BLACK_MARKET_WON (0x3b00c3)

- Modern: 3866819 (0x3b00c3) · 4.3.4: — · Area: General
- Layout: `{ u32*2 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused) · 6 · (2 unused)

### SMSG_SCENARIO_STATE (0x3b00c4)

- Modern: 3866820 (0x3b00c4) · 4.3.4: — · Area: General
- Layout: `{ { guid u32*10 guid loop[ { u32 } ] u8 loop[ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } ] loop[ { u32 u8 } ] loop[ u32 u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 44: 1 · (7 unused)

### SMSG_SCENARIO_PROGRESS_UPDATE (0x3b00c5)

- Modern: 3866821 (0x3b00c5) · 4.3.4: — · Area: General
- Layout: `{ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 42: 1 · (7 unused)

### SMSG_GROUP_NEW_LEADER (0x3b00c6)

- Modern: 3866822 (0x3b00c6) · 4.3.4: — · Area: General
- Layout: `{ u8*3 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 9 · (7 unused)

### SMSG_SEND_RAID_TARGET_UPDATE_ALL (0x3b00c7)

- Modern: 3866823 (0x3b00c7) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 loop[ guid u8 ] }`

### SMSG_SEND_RAID_TARGET_UPDATE_SINGLE (0x3b00c8)

- Modern: 3866824 (0x3b00c8) · 4.3.4: — · Area: General
- Layout: `{ u8*2 guid guid }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8

### SMSG_RANDOM_ROLL (0x3b00c9)

- Modern: 3866825 (0x3b00c9) · 4.3.4: — · Area: General
- Layout: `{ guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_INSPECT_RESULT (0x3b00ca)

- Modern: 3866826 (0x3b00ca) · 4.3.4: — · Area: General
- Layout: `{ { { guid u32*2 u8*4 u32 bytes loop[ { u32*2 } ] loop[ { guid u8 u32*2 loop[ u32 ] { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u32*3 u8 ] loop[ u32 u8 ] loop[ { u8 { u32*3 u8 opt[ u8 u32 loop[ u32 ] ] } } ] } ] } u32*2 u8 u16*2 u32*2 loop[ u16 ] { u32 u8 u32 loop[ u8 u32 u8 u32 u8 u32 loop[ u32*2 ] loop[ u16 ] ] u8 } u8 loop[ { u8 u32*17 u8 } ] opt[ { guid u32*2 } ] opt[ u32 ] { u32*2 { u32*4 opt[ u32 ] opt[ u32*3 ] opt[ u32 ] loop[ u32*4 ] u8*2 loop[ { u32*2 loop[ u32*4 ] u8 } ] bytes } } } }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 6 · (2 unused) · 8 · 8 · 8; byte 48: 1 · (7 unused) · 1 · 1 · (6 unused) · 8; byte 119: 1 · (7 unused) · 8; byte 189: 1 · (7 unused) · 8; byte 259: 1 · (7 unused) · 8; byte 329: 1 · (7 unused) · 8; byte 399: 1 · (7 unused) · 8; byte 469: 1 · (7 unused) · 8; byte 539: 1 · (7 unused) · 8; byte 609: 1 · (7 unused) · 8; byte 679: 1 · (7 unused); byte 704: 9 · (7 unused)

### SMSG_ARENA_CROWD_CONTROL_SPELL_RESULT (0x3b00cb)

- Modern: 3866827 (0x3b00cb) · 4.3.4: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_SCENARIO_POIS (0x3b00cc)

- Modern: 3866828 (0x3b00cc) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ { u32*2 loop[ u32*9 loop[ u32*3 ] ] } ] }`

### SMSG_INSTANCE_INFO (0x3b00cd)

- Modern: 3866829 (0x3b00cd) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ u32*2 u64 u32*2 u8 ] }`

### SMSG_CONSOLE_WRITE (0x3b00ce)

- Modern: 3866830 (0x3b00ce) · 4.3.4: — · Area: General
- Layout: `{ u8*2 u32 { bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 11 · 1 · (3 unused)

### SMSG_PLAY_SCENE (0x3b00cf)

- Modern: 3866831 (0x3b00cf) · 4.3.4: — · Area: General
- Layout: `{ { u32*4 guid u32*5 } u8 }`
- Size: 37 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 38: 1 · (7 unused)

### SMSG_CANCEL_SCENE (0x3b00d0)

- Modern: 3866832 (0x3b00d0) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BATTLE_PET_ERROR (0x3b00d1)

- Modern: 3866833 (0x3b00d1) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_MAIL_COMMAND_RESULT (0x3b00d4)

- Modern: 3866836 (0x3b00d4) · 4.3.4: 18727 (0x4927) · Area: General
- Layout: `{ u64 u32*3 u64 u32 }`
- Size: 32 bytes (packed GUIDs not counted)

### SMSG_NOTIFY_RECEIVED_MAIL (0x3b00d5)

- Modern: 3866837 (0x3b00d5) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_ADD_BATTLENET_FRIEND_RESPONSE (0x3b00d6)

- Modern: 3866838 (0x3b00d6) · 4.3.4: — · Area: General
- Layout: `{ u64 u8 opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 4 · 1 · (3 unused)

### SMSG_ADDON_LIST_REQUEST (0x3b00db)

- Modern: 3866843 (0x3b00db) · 4.3.4: — · Area: General
- Layout: `{ guid { u32 u16 u8 } }`
- Size: 7 bytes (packed GUIDs not counted)

### SMSG_ACHIEVEMENT_EARNED (0x3b00dc)

- Modern: 3866844 (0x3b00dc) · 4.3.4: 17413 (0x4405) · Area: General
- Layout: `{ guid guid u32 { u32 } u32*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)

### SMSG_BONUS_ROLL_EMPTY (0x3b00de)

- Modern: 3866846 (0x3b00de) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_UPDATE_EXPANSION_LEVEL (0x3b00df)

- Modern: 3866847 (0x3b00df) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 opt[ u8 ] opt[ u8 ] loop[ { u8 u32 loop[ u8*4 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · (5 unused)

### SMSG_CONTROL_UPDATE (0x3b00e0)

- Modern: 3866848 (0x3b00e0) · 4.3.4: 10295 (0x2837) · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_ARENA_PREP_OPPONENT_SPECIALIZATIONS (0x3b00e1)

- Modern: 3866849 (0x3b00e1) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ u32 u8 guid ] }`

### SMSG_FORCE_OBJECT_RELINK (0x3b00e4)

- Modern: 3866852 (0x3b00e4) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DISPLAY_PROMOTION (0x3b00e5)

- Modern: 3866853 (0x3b00e5) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SERVER_FIRST_ACHIEVEMENTS (0x3b00e7)

- Modern: 3866855 (0x3b00e7) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ { guid u32 } ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_CORPSE_LOCATION (0x3b00e8)

- Modern: 3866856 (0x3b00e8) · 4.3.4: — · Area: General
- Layout: `{ u8 guid u32*2 guid u32*3 }`
- Size: 21 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_REFRESH_COMPONENT (0x3b00ea)

- Modern: 3866858 (0x3b00ea) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_DEBUG_MENU_MANAGER_FULL_UPDATE (0x3b00f0)

- Modern: 3866864 (0x3b00f0) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOSS_OF_CONTROL_AURA_UPDATE (0x3b010b)

- Modern: 3866891 (0x3b010b) · 4.3.4: — · Area: General
- Layout: `{ guid u32 loop[ u32 u8*4 ] }`

### SMSG_ADD_LOSS_OF_CONTROL (0x3b010c)

- Modern: 3866892 (0x3b010c) · 4.3.4: — · Area: General
- Layout: `{ guid u32 guid u32*3 u8*2 }`
- Size: 18 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 20: 8 · 8

### SMSG_SET_TIME_ZONE_INFORMATION (0x3b0113)

- Modern: 3866899 (0x3b0113) · 4.3.4: — · Area: General
- Layout: `{ u8*3 bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 7 · 7 · (3 unused)

### SMSG_BATTLE_PET_CAGE_DATE_ERROR (0x3b0114)

- Modern: 3866900 (0x3b0114) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_TEXT_EMOTE (0x3b0116)

- Modern: 3866902 (0x3b0116) · 4.3.4: 2821 (0xb05) · Area: General
- Layout: `{ guid guid u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_PET_GOD_MODE (0x3b0117)

- Modern: 3866903 (0x3b0117) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_TAXI_NODE_STATUS (0x3b0118)

- Modern: 3866904 (0x3b0118) · 4.3.4: 10550 (0x2936) · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 2 · (6 unused)

### SMSG_ACTIVATE_TAXI_REPLY (0x3b0119)

- Modern: 3866905 (0x3b0119) · 4.3.4: 27191 (0x6a37) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_NEW_TAXI_PATH (0x3b011a)

- Modern: 3866906 (0x3b011a) · 4.3.4: 19253 (0x4b35) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_GAME_SPEED_SET (0x3b011d)

- Modern: 3866909 (0x3b011d) · 4.3.4: 20020 (0x4e34) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: f32

### SMSG_SERVER_TIME (0x3b011e)

- Modern: 3866910 (0x3b011e) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_LOGOUT_RESPONSE (0x3b011f)

- Modern: 3866911 (0x3b011f) · 4.3.4: 1316 (0x524) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_LOGOUT_COMPLETE (0x3b0120)

- Modern: 3866912 (0x3b0120) · 4.3.4: 8503 (0x2137) · Area: General
- Layout: `{ u8 opt[ { u8 { u32 guid u8 u32 u8*3 u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] } { u32 guid u8 u32 u8*3 u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] } } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_LOGOUT_CANCEL_ACK (0x3b0121)

- Modern: 3866913 (0x3b0121) · 4.3.4: 25876 (0x6514) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_INSTANCE_RESET (0x3b0122)

- Modern: 3866914 (0x3b0122) · 4.3.4: 28421 (0x6f05) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_INSTANCE_RESET_FAILED (0x3b0123)

- Modern: 3866915 (0x3b0123) · 4.3.4: 18213 (0x4725) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 2 · (6 unused)

### SMSG_UPDATE_LAST_INSTANCE (0x3b0124)

- Modern: 3866916 (0x3b0124) · 4.3.4: 1079 (0x437) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_KICK_REASON (0x3b0125)

- Modern: 3866917 (0x3b0125) · 4.3.4: 16423 (0x4027) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CALENDAR_SEND_CALENDAR (0x3b0127)

- Modern: 3866919 (0x3b0127) · 4.3.4: 26629 (0x6805) · Area: General
- Layout: `{ { u32*5 loop[ u64 u32*3 ] loop[ u32*5 ] loop[ u64*2 u8*3 guid u8 ] loop[ u64 u8 u32 u16 u32 u64 guid u8 bytes ] } }`

### SMSG_CALENDAR_SEND_EVENT (0x3b0128)

- Modern: 3866920 (0x3b0128) · 4.3.4: 3125 (0xc35) · Area: General
- Layout: `{ { u8 guid u64 u8 u32 u16 u32*2 u64 u32 u8*3 loop[ guid u64 u8*4 u32 u8 bytes ] bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 38: 8 · 11 · (5 unused)

### SMSG_CALENDAR_COMMUNITY_INVITE (0x3b0129)

- Modern: 3866921 (0x3b0129) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ guid u8 ] }`

### SMSG_CALENDAR_INVITE_ADDED (0x3b012a)

- Modern: 3866922 (0x3b012a) · 4.3.4: — · Area: General
- Layout: `{ guid u64*2 u8*3 u32 u8 }`
- Size: 24 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 8 · 8 · 8; byte 25: 1 · (7 unused)

### SMSG_CALENDAR_INVITE_REMOVED (0x3b012b)

- Modern: 3866923 (0x3b012b) · 4.3.4: — · Area: General
- Layout: `{ guid u64 u32 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### SMSG_CALENDAR_INVITE_STATUS (0x3b012c)

- Modern: 3866924 (0x3b012c) · 4.3.4: — · Area: General
- Layout: `{ guid u64 u32*2 u8 u32 u8 }`
- Size: 22 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 23: 1 · (7 unused)

### SMSG_CALENDAR_MODERATOR_STATUS (0x3b012d)

- Modern: 3866925 (0x3b012d) · 4.3.4: — · Area: General
- Layout: `{ guid u64 u8*2 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 1 · (7 unused)

### SMSG_CALENDAR_INVITE_ALERT (0x3b012e)

- Modern: 3866926 (0x3b012e) · 4.3.4: — · Area: General
- Layout: `{ { u64 u32 u16 u8 u32 u64*2 u8*2 guid guid u8*2 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 35: 8 · 8; byte 41: 8 · 1 · (7 unused)

### SMSG_CALENDAR_INVITE_STATUS_ALERT (0x3b012f)

- Modern: 3866927 (0x3b012f) · 4.3.4: — · Area: General
- Layout: `{ u64 u32*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)

### SMSG_CALENDAR_INVITE_REMOVED_ALERT (0x3b0130)

- Modern: 3866928 (0x3b0130) · 4.3.4: — · Area: General
- Layout: `{ u64 u32*2 u8 }`
- Size: 17 bytes (packed GUIDs not counted)

### SMSG_CALENDAR_EVENT_REMOVED_ALERT (0x3b0131)

- Modern: 3866929 (0x3b0131) · 4.3.4: 27957 (0x6d35) · Area: General
- Layout: `{ u64 u32 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_CALENDAR_EVENT_UPDATED_ALERT (0x3b0132)

- Modern: 3866930 (0x3b0132) · 4.3.4: 2311 (0x907) · Area: General
- Layout: `{ u64*2 u32*3 u16 u32 u8*4 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 34: 8 · 8 · 11 · 1 · (4 unused)

### SMSG_CALENDAR_INVITE_NOTES (0x3b0133)

- Modern: 3866931 (0x3b0133) · 4.3.4: — · Area: General
- Layout: `{ guid u64 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · 8 · (7 unused)

### SMSG_CALENDAR_INVITE_NOTES_ALERT (0x3b0134)

- Modern: 3866932 (0x3b0134) · 4.3.4: — · Area: General
- Layout: `{ u64 u8 bytes }`

### SMSG_CALENDAR_RAID_LOCKOUT_ADDED (0x3b0135)

- Modern: 3866933 (0x3b0135) · 4.3.4: 8965 (0x2305) · Area: General
- Layout: `{ struct }`

### SMSG_CALENDAR_RAID_LOCKOUT_REMOVED (0x3b0136)

- Modern: 3866934 (0x3b0136) · 4.3.4: 11813 (0x2e25) · Area: General
- Layout: `{ struct }`

### SMSG_CALENDAR_RAID_LOCKOUT_UPDATED (0x3b0137)

- Modern: 3866935 (0x3b0137) · 4.3.4: 17974 (0x4636) · Area: General
- Layout: `{ struct }`

### SMSG_CALENDAR_SEND_NUM_PENDING (0x3b0138)

- Modern: 3866936 (0x3b0138) · 4.3.4: 3095 (0xc17) · Area: General
- Layout: `{ struct }`

### SMSG_CALENDAR_CLEAR_PENDING_ACTION (0x3b0139)

- Modern: 3866937 (0x3b0139) · 4.3.4: 8454 (0x2106) · Area: General
- Layout: `{ struct }`

### SMSG_CALENDAR_COMMAND_RESULT (0x3b013a)

- Modern: 3866938 (0x3b013a) · 4.3.4: 28470 (0x6f36) · Area: General
- Layout: `{ u8*4 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8 · 9 · (7 unused)

### SMSG_SPECIAL_MOUNT_ANIM (0x3b013b)

- Modern: 3866939 (0x3b013b) · 4.3.4: — · Area: General
- Layout: `{ guid { u32*2 loop[ u32 ] } }`

### SMSG_PET_ACTION_SOUND (0x3b013c)

- Modern: 3866940 (0x3b013c) · 4.3.4: 17188 (0x4324) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_PET_DISMISS_SOUND (0x3b013d)

- Modern: 3866941 (0x3b013d) · 4.3.4: 11013 (0x2b05) · Area: General
- Layout: `{ guid u32*4 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_GM_TICKET_SYSTEM_STATUS (0x3b013e)

- Modern: 3866942 (0x3b013e) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GM_TICKET_CASE_STATUS (0x3b013f)

- Modern: 3866943 (0x3b013f) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ { u32 u64 u32 u16 u64 u32 u8*3 { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] } { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] } bytes bytes { bytes } { bytes } } ] }`

### SMSG_SET_DUNGEON_DIFFICULTY (0x3b0140)

- Modern: 3866944 (0x3b0140) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_WHO_IS (0x3b0141)

- Modern: 3866945 (0x3b0141) · 4.3.4: 26903 (0x6917) · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 11 · (5 unused)

### SMSG_WEATHER (0x3b0142)

- Modern: 3866946 (0x3b0142) · 4.3.4: 10500 (0x2904) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_START_LIGHTNING_STORM (0x3b0143)

- Modern: 3866947 (0x3b0143) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_END_LIGHTNING_STORM (0x3b0144)

- Modern: 3866948 (0x3b0144) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_UPDATE_INSTANCE_OWNERSHIP (0x3b0145)

- Modern: 3866949 (0x3b0145) · 4.3.4: 18709 (0x4915) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_NOTIFY_MISSILE_TRAJECTORY_COLLISION (0x3b0146)

- Modern: 3866950 (0x3b0146) · 4.3.4: — · Area: General
- Layout: `{ guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_COMPLAINT_RESULT (0x3b0147)

- Modern: 3866951 (0x3b0147) · 4.3.4: 27940 (0x6d24) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_SUMMON_CANCEL (0x3b014c)

- Modern: 3866956 (0x3b014c) · 4.3.4: 2868 (0xb34) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_DISMOUNT (0x3b014d)

- Modern: 3866957 (0x3b014d) · 4.3.4: 8501 (0x2135) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_EQUIPMENT_SET_ID (0x3b014e)

- Modern: 3866958 (0x3b014e) · 4.3.4: 8726 (0x2216) · Area: General
- Layout: `{ u32*2 u64 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_PET_TAME_FAILURE (0x3b014f)

- Modern: 3866959 (0x3b014f) · 4.3.4: 27428 (0x6b24) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8

### SMSG_AI_REACTION (0x3b0151)

- Modern: 3866961 (0x3b0151) · 4.3.4: 1591 (0x637) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_OFFER_PETITION_ERROR (0x3b0152)

- Modern: 3866962 (0x3b0152) · 4.3.4: 10006 (0x2716) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_RESET_FAILED_NOTIFY (0x3b0153)

- Modern: 3866963 (0x3b0153) · 4.3.4: 17942 (0x4616) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_ADD_RUNE_POWER (0x3b0154)

- Modern: 3866964 (0x3b0154) · 4.3.4: 26901 (0x6915) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_COOLDOWN_EVENT (0x3b0155)

- Modern: 3866965 (0x3b0155) · 4.3.4: 20262 (0x4f26) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_CLEAR_COOLDOWN (0x3b0156)

- Modern: 3866966 (0x3b0156) · 4.3.4: 1575 (0x627) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_OVERRIDE_LIGHT (0x3b0157)

- Modern: 3866967 (0x3b0157) · 4.3.4: 16933 (0x4225) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### SMSG_ENABLE_BARBER_SHOP (0x3b0158)

- Modern: 3866968 (0x3b0158) · 4.3.4: 11542 (0x2d16) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8

### SMSG_CONFIRM_BARBERS_CHOICE (0x3b0159)

- Modern: 3866969 (0x3b0159) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BARBER_SHOP_RESULT (0x3b015a)

- Modern: 3866970 (0x3b015a) · 4.3.4: 24869 (0x6125) · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_PETITION_SHOW_LIST (0x3b015b)

- Modern: 3866971 (0x3b015b) · 4.3.4: 25605 (0x6405) · Area: General
- Layout: `{ guid u32 loop[ u32*5 ] }`

### SMSG_PETITION_SHOW_SIGNATURES (0x3b015c)

- Modern: 3866972 (0x3b015c) · 4.3.4: 1814 (0x716) · Area: General
- Layout: `{ guid guid guid u32*2 loop[ guid u32 ] }`

### SMSG_RECRUIT_A_FRIEND_FAILURE (0x3b015d)

- Modern: 3866973 (0x3b015d) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 6 · (2 unused)

### SMSG_CROSSED_INEBRIATION_THRESHOLD (0x3b015e)

- Modern: 3866974 (0x3b015e) · 4.3.4: 8246 (0x2036) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_PET_NAME_INVALID (0x3b0160)

- Modern: 3866976 (0x3b0160) · 4.3.4: 24583 (0x6007) · Area: General
- Layout: `{ u8 { guid u32 u8*2 opt[ u8*4 bytes bytes bytes bytes bytes ] bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 7: 8 · 1 · (7 unused)

### SMSG_SELL_RESPONSE (0x3b0161)

- Modern: 3866977 (0x3b0161) · 4.3.4: — · Area: General
- Layout: `{ guid u32*2 loop[ guid ] }`

### SMSG_BUY_SUCCEEDED (0x3b0162)

- Modern: 3866978 (0x3b0162) · 4.3.4: 3878 (0xf26) · Area: General
- Layout: `{ guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_BUY_FAILED (0x3b0163)

- Modern: 3866979 (0x3b0163) · 4.3.4: 25653 (0x6435) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_TOTEM_CREATED (0x3b0164)

- Modern: 3866980 (0x3b0164) · 4.3.4: 9236 (0x2414) · Area: General
- Layout: `{ u8 guid u32*3 u8 }`
- Size: 14 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 15: 1 · (7 unused)

### SMSG_TOTEM_MOVED (0x3b0166)

- Modern: 3866982 (0x3b0166) · 4.3.4: — · Area: General
- Layout: `{ u8*2 guid }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8

### SMSG_TRIGGER_MOVIE (0x3b0167)

- Modern: 3866983 (0x3b0167) · 4.3.4: 17957 (0x4625) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SHOW_TAXI_NODES (0x3b0169)

- Modern: 3866985 (0x3b0169) · 4.3.4: 10806 (0x2a36) · Area: General
- Layout: `{ u8 u32*2 opt[ guid u32 ] loop[ u64 ] loop[ u64 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_MINIMAP_PING (0x3b016a)

- Modern: 3866986 (0x3b016a) · 4.3.4: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_FISH_NOT_HOOKED (0x3b016b)

- Modern: 3866987 (0x3b016b) · 4.3.4: 2583 (0xa17) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_FISH_ESCAPED (0x3b016c)

- Modern: 3866988 (0x3b016c) · 4.3.4: 8709 (0x2205) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_HEALTH_UPDATE (0x3b016d)

- Modern: 3866989 (0x3b016d) · 4.3.4: 18228 (0x4734) · Area: General
- Layout: `{ guid u64 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_POWER_UPDATE (0x3b016e)

- Modern: 3866990 (0x3b016e) · 4.3.4: 18951 (0x4a07) · Area: General
- Layout: `{ guid u32 loop[ u8 u32 ] }`

### SMSG_DEATH_RELEASE_LOC (0x3b016f)

- Modern: 3866991 (0x3b016f) · 4.3.4: 12039 (0x2f07) · Area: General
- Layout: `{ u32*4 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_FORCED_DEATH_UPDATE (0x3b0170)

- Modern: 3866992 (0x3b0170) · 4.3.4: 9734 (0x2606) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYED_TIME (0x3b0171)

- Modern: 3866993 (0x3b0171) · 4.3.4: 24631 (0x6037) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_TITLE_EARNED (0x3b0173)

- Modern: 3866995 (0x3b0173) · 4.3.4: 9254 (0x2426) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_TITLE_LOST (0x3b0174)

- Modern: 3866996 (0x3b0174) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_HIGHEST_THREAT_UPDATE (0x3b0175)

- Modern: 3866997 (0x3b0175) · 4.3.4: 16644 (0x4104) · Area: General
- Layout: `{ guid guid u32 loop[ guid u64 ] }`

### SMSG_THREAT_UPDATE (0x3b0176)

- Modern: 3866998 (0x3b0176) · 4.3.4: 18229 (0x4735) · Area: General
- Layout: `{ guid u32 loop[ guid u64 ] }`

### SMSG_THREAT_REMOVE (0x3b0177)

- Modern: 3866999 (0x3b0177) · 4.3.4: 11781 (0x2e05) · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_THREAT_CLEAR (0x3b0178)

- Modern: 3867000 (0x3b0178) · 4.3.4: 25655 (0x6437) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_PROPOSE_LEVEL_GRANT (0x3b0179)

- Modern: 3867001 (0x3b0179) · 4.3.4: 24852 (0x6114) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CANCEL_AUTO_REPEAT (0x3b017a)

- Modern: 3867002 (0x3b017a) · 4.3.4: 25654 (0x6436) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_TRAINER_LIST (0x3b017b)

- Modern: 3867003 (0x3b017b) · 4.3.4: 17428 (0x4414) · Area: General
- Layout: `{ { guid u32*3 loop[ u32*4 loop[ u32 ] u32 u8*2 ] u8*2 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · (5 unused)

### SMSG_TRAINER_BUY_FAILED (0x3b017c)

- Modern: 3867004 (0x3b017c) · 4.3.4: 4 (0x4) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_CRITERIA_UPDATE (0x3b017d)

- Modern: 3867005 (0x3b017d) · 4.3.4: 28215 (0x6e37) · Area: General
- Layout: `{ u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 42: 1 · (7 unused)

### SMSG_CHAR_CUSTOMIZE_FAILURE (0x3b017e)

- Modern: 3867006 (0x3b017e) · 4.3.4: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_CHAR_CUSTOMIZE_SUCCESS (0x3b017f)

- Modern: 3867007 (0x3b017f) · 4.3.4: — · Area: General
- Layout: `{ guid u8 u32 loop[ { u32*2 } ] u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 7: 6 · (2 unused)

### SMSG_QUERY_TIME_RESPONSE (0x3b0180)

- Modern: 3867008 (0x3b0180) · 4.3.4: 8484 (0x2124) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes

### SMSG_LOG_XP_GAIN (0x3b0181)

- Modern: 3867009 (0x3b0181) · 4.3.4: 17684 (0x4514) · Area: General
- Layout: `{ guid u32 u8 u32*2 }`
- Size: 13 bytes (packed GUIDs not counted)

### SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA (0x3b0182)

- Modern: 3867010 (0x3b0182) · 4.3.4: 19764 (0x4d34) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CRITERIA_DELETED (0x3b0183)

- Modern: 3867011 (0x3b0183) · 4.3.4: 10517 (0x2915) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_ACHIEVEMENT_DELETED (0x3b0184)

- Modern: 3867012 (0x3b0184) · 4.3.4: 27158 (0x6a16) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_LEVEL_UP_INFO (0x3b0185)

- Modern: 3867013 (0x3b0185) · 4.3.4: 1077 (0x435) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (76 bytes): +0: u32, +4: u32, +8: u32, +48: u32, +52: u32, +56: u32, +60: u32, +64: u32, +68: u32, +72: u32

### SMSG_ITEM_CHANGED (0x3b0187)

- Modern: 3867015 (0x3b0187) · 4.3.4: — · Area: General
- Layout: `{ guid { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused) · 6 · (2 unused); byte 28: 1 · (7 unused) · 6 · (2 unused)

### SMSG_AUCTION_HELLO_RESPONSE (0x3b018a)

- Modern: 3867018 (0x3b018a) · 4.3.4: — · Area: General
- Layout: `{ guid u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### SMSG_AUCTION_REPLICATE_RESPONSE (0x3b018b)

- Modern: 3867019 (0x3b018b) · 4.3.4: — · Area: General
- Layout: `{ u32*6 loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 u32 loop[ { u32*3 u8 } ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] }`

### SMSG_AUCTION_COMMAND_RESULT (0x3b018c)

- Modern: 3867020 (0x3b018c) · 4.3.4: 19493 (0x4c25) · Area: General
- Layout: `{ u32*4 guid u64*2 u32 }`
- Size: 36 bytes (packed GUIDs not counted)

### SMSG_AUCTION_WON_NOTIFICATION (0x3b018d)

- Modern: 3867021 (0x3b018d) · 4.3.4: — · Area: General
- Layout: `{ u32*2 guid { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · (7 unused) · 6 · (2 unused)

### SMSG_AUCTION_OUTBID_NOTIFICATION (0x3b018e)

- Modern: 3867022 (0x3b018e) · 4.3.4: — · Area: General
- Layout: `{ u32*2 guid { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u64*2 }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · (7 unused) · 6 · (2 unused)

### SMSG_AUCTION_CLOSED_NOTIFICATION (0x3b018f)

- Modern: 3867023 (0x3b018f) · 4.3.4: — · Area: General
- Layout: `{ u32 u64 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused) · 6 · (2 unused); byte 30: 1 · (7 unused)

### SMSG_AUCTION_OWNER_BID_NOTIFICATION (0x3b0190)

- Modern: 3867024 (0x3b0190) · 4.3.4: — · Area: General
- Layout: `{ u32 u64 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u64 guid }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused) · 6 · (2 unused)

### SMSG_SET_VEHICLE_REC_ID (0x3b0193)

- Modern: 3867027 (0x3b0193) · 4.3.4: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_PENDING_RAID_LOCK (0x3b0194)

- Modern: 3867028 (0x3b0194) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)

### SMSG_DESTRUCTIBLE_BUILDING_DAMAGE (0x3b0195)

- Modern: 3867029 (0x3b0195) · 4.3.4: 18469 (0x4825) · Area: General
- Layout: `{ guid guid guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_INSTANCE_GROUP_SIZE_CHANGED (0x3b0196)

- Modern: 3867030 (0x3b0196) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GOD_MODE (0x3b0198)

- Modern: 3867032 (0x3b0198) · 4.3.4: 1029 (0x405) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SET_FACTION_AT_WAR (0x3b019c)

- Modern: 3867036 (0x3b019c) · 4.3.4: 16918 (0x4216) · Area: General
- Layout: `{ u32 u16 }`
- Size: 6 bytes (packed GUIDs not counted)

### SMSG_CREATE_CHAR (0x3b019d)

- Modern: 3867037 (0x3b019d) · 4.3.4: 11525 (0x2d05) · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_DELETE_CHAR (0x3b019e)

- Modern: 3867038 (0x3b019e) · 4.3.4: 772 (0x304) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8

### SMSG_TRANSFER_ABORTED (0x3b019f)

- Modern: 3867039 (0x3b019f) · 4.3.4: 1335 (0x537) · Area: General
- Layout: `{ u32 u8 u32 u8 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 9: 6 · (2 unused)

### SMSG_PET_GUIDS (0x3b01a0)

- Modern: 3867040 (0x3b01a0) · 4.3.4: 11558 (0x2d26) · Area: General
- Layout: `{ u32 loop[ guid ] }`

### SMSG_CHARACTER_LOGIN_FAILED (0x3b01a1)

- Modern: 3867041 (0x3b01a1) · 4.3.4: 17431 (0x4417) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_COMMENTATOR_STATE_CHANGED (0x3b01a2)

- Modern: 3867042 (0x3b01a2) · 4.3.4: 1847 (0x737) · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_COMMENTATOR_MAP_INFO (0x3b01a3)

- Modern: 3867043 (0x3b01a3) · 4.3.4: 807 (0x327) · Area: General
- Layout: `{ u64 u32 loop[ { u32*5 loop[ u32 { u32 u16 u8 } u64 u32 loop[ guid u32 loop[ guid { u32 u16 u8 } ] ] ] } ] }`

### SMSG_COMMENTATOR_PLAYER_INFO (0x3b01a4)

- Modern: 3867044 (0x3b01a4) · 4.3.4: 12086 (0x2f36) · Area: General
- Layout: `{ u32 { u32 u16 u8 } u64 u32 u8 loop[ { guid u8 u32*2 u16*2 u32*4 u8*2 u32*3 loop[ u32*3 u8 ] loop[ u32*2 ] loop[ u32*6 u8 opt[ u32 ] opt[ u32 ] ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 23: 1 · (7 unused)

### SMSG_UPDATE_ACCOUNT_DATA (0x3b01a5)

- Modern: 3867045 (0x3b01a5) · 4.3.4: 26679 (0x6837) · Area: General
- Layout: `{ u64 u32 guid u32*2 bytes }`

### SMSG_ACCOUNT_DATA_TIMES (0x3b01a6)

- Modern: 3867046 (0x3b01a6) · 4.3.4: 19205 (0x4b05) · Area: General
- Layout: `{ guid u64 loop[ u64 ] }`

### SMSG_GAME_TIME_UPDATE (0x3b01a7)

- Modern: 3867047 (0x3b01a7) · 4.3.4: 16679 (0x4127) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_GAME_TIME_SET (0x3b01a8)

- Modern: 3867048 (0x3b01a8) · 4.3.4: 20 (0x14) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_LOGIN_SET_TIME_SPEED (0x3b01a9)

- Modern: 3867049 (0x3b01a9) · 4.3.4: 19733 (0x4d15) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32, +8: f32

### SMSG_LOAD_EQUIPMENT_SET (0x3b01aa)

- Modern: 3867050 (0x3b01aa) · 4.3.4: 11780 (0x2e04) · Area: General
- Layout: `{ u32 loop[ { u32 u64 u32*2 loop[ guid u32 ] u32*6 u8*3 opt[ u32 ] bytes bytes } ] }`

### SMSG_START_MIRROR_TIMER (0x3b01ab)

- Modern: 3867051 (0x3b01ab) · 4.3.4: 26660 (0x6824) · Area: General
- Layout: `{ u8 u32*4 u8 }`
- Size: 18 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 17: 1 · (7 unused)

### SMSG_PAUSE_MIRROR_TIMER (0x3b01ac)

- Modern: 3867052 (0x3b01ac) · 4.3.4: 16405 (0x4015) · Area: General
- Layout: `{ u8*2 }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 1 · (7 unused)

### SMSG_STOP_MIRROR_TIMER (0x3b01ad)

- Modern: 3867053 (0x3b01ad) · 4.3.4: 2822 (0xb06) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_CORPSE_TRANSPORT_QUERY (0x3b01ae)

- Modern: 3867054 (0x3b01ae) · 4.3.4: — · Area: General
- Layout: `{ guid u32*4 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_ENCHANTMENT_LOG (0x3b01af)

- Modern: 3867055 (0x3b01af) · 4.3.4: 24629 (0x6035) · Area: General
- Layout: `{ guid guid guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_SERVER_TIME_OFFSET (0x3b01b0)

- Modern: 3867056 (0x3b01b0) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes

### SMSG_AREA_TRIGGER_NO_CORPSE (0x3b01b2)

- Modern: 3867058 (0x3b01b2) · 4.3.4: 10772 (0x2a14) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_TALENTS_INVOLUNTARILY_RESET (0x3b01b3)

- Modern: 3867059 (0x3b01b3) · 4.3.4: 11303 (0x2c27) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SPEC_INVOLUNTARILY_CHANGED (0x3b01b4)

- Modern: 3867060 (0x3b01b4) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_PAGE_TEXT (0x3b01b5)

- Modern: 3867061 (0x3b01b5) · 4.3.4: 10533 (0x2925) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_GAME_OBJECT_UI_LINK (0x3b01b6)

- Modern: 3867062 (0x3b01b6) · 4.3.4: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_STAND_STATE_UPDATE (0x3b01b8)

- Modern: 3867064 (0x3b01b8) · 4.3.4: 28420 (0x6f04) · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)

### SMSG_GAME_OBJECT_RESET_STATE (0x3b01b9)

- Modern: 3867065 (0x3b01b9) · 4.3.4: 10774 (0x2a16) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_SUMMON_REQUEST (0x3b01bc)

- Modern: 3867068 (0x3b01bc) · 4.3.4: 10759 (0x2a07) · Area: General
- Layout: `{ guid u32*2 u8*2 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 1 · (7 unused)

### SMSG_INSPECT_PVP (0x3b01bd)

- Modern: 3867069 (0x3b01bd) · 4.3.4: — · Area: General
- Layout: `{ guid u32 u8 loop[ { u8 u32*17 u8 } ] loop[ { guid u32*5 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 2 · (6 unused)

### SMSG_REFER_A_FRIEND_EXPIRED (0x3b01be)

- Modern: 3867070 (0x3b01be) · 4.3.4: 18740 (0x4934) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_INITIALIZE_FACTIONS (0x3b01bf)

- Modern: 3867071 (0x3b01bf) · 4.3.4: 17972 (0x4634) · Area: General
- Layout: `{ u32*2 loop[ u32 u16 u32 ] loop[ u32 u8 ] }`

### SMSG_FACTION_BONUS_INFO (0x3b01c0)

- Modern: 3867072 (0x3b01c0) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ u32 u8 ] }`

### SMSG_CAMERA_EFFECT (0x3b01c1)

- Modern: 3867073 (0x3b01c1) · 4.3.4: — · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_SOCKET_GEMS_SUCCESS (0x3b01c2)

- Modern: 3867074 (0x3b01c2) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_SOCKET_GEMS_FAILURE (0x3b01c3)

- Modern: 3867075 (0x3b01c3) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_SET_FACTION_VISIBLE (0x3b01c5)

- Modern: 3867077 (0x3b01c5) · 4.3.4: 9509 (0x2525) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SET_FACTION_NOT_VISIBLE (0x3b01c6)

- Modern: 3867078 (0x3b01c6) · 4.3.4: 26423 (0x6737) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SET_FACTION_STANDING (0x3b01c7)

- Modern: 3867079 (0x3b01c7) · 4.3.4: 294 (0x126) · Area: General
- Layout: `{ u32*2 loop[ u32*3 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_SET_AI_ANIM_KIT (0x3b01cb)

- Modern: 3867083 (0x3b01cb) · 4.3.4: 17958 (0x4626) · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### SMSG_PLAY_ONE_SHOT_ANIM_KIT (0x3b01cc)

- Modern: 3867084 (0x3b01cc) · 4.3.4: 18997 (0x4a35) · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### SMSG_SET_MOVEMENT_ANIM_KIT (0x3b01cd)

- Modern: 3867085 (0x3b01cd) · 4.3.4: 3860 (0xf14) · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### SMSG_SET_MELEE_ANIM_KIT (0x3b01ce)

- Modern: 3867086 (0x3b01ce) · 4.3.4: 26389 (0x6715) · Area: General
- Layout: `{ guid u16 }`
- Size: 2 bytes (packed GUIDs not counted)

### SMSG_SET_ANIM_TIER (0x3b01cf)

- Modern: 3867087 (0x3b01cf) · 4.3.4: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 3 · (5 unused)

### SMSG_SET_PROFICIENCY (0x3b01d0)

- Modern: 3867088 (0x3b01d0) · 4.3.4: 25095 (0x6207) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_COOLDOWN_CHEAT (0x3b01d4)

- Modern: 3867092 (0x3b01d4) · 4.3.4: 17719 (0x4537) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_AREA_SPIRIT_HEALER_TIME (0x3b01db)

- Modern: 3867099 (0x3b01db) · 4.3.4: 1844 (0x734) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_LOOT_LIST (0x3b01dc)

- Modern: 3867100 (0x3b01dc) · 4.3.4: 26631 (0x6807) · Area: General
- Layout: `{ guid guid u8 opt[ guid ] opt[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · (6 unused)

### SMSG_DESTROY_ARENA_UNIT (0x3b01dd)

- Modern: 3867101 (0x3b01dd) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_FEIGN_DEATH_RESISTED (0x3b01df)

- Modern: 3867103 (0x3b01df) · 4.3.4: 3333 (0xd05) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_DURABILITY_DAMAGE_DEATH (0x3b01e0)

- Modern: 3867104 (0x3b01e0) · 4.3.4: 19495 (0x4c27) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_INIT_WORLD_STATES (0x3b01e1)

- Modern: 3867105 (0x3b01e1) · 4.3.4: 19477 (0x4c15) · Area: General
- Layout: `{ u32*4 loop[ u32*2 ] }`

### SMSG_UPDATE_WORLD_STATE (0x3b01e3)

- Modern: 3867107 (0x3b01e3) · 4.3.4: 18454 (0x4816) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_PET_ACTION_FEEDBACK (0x3b01e4)

- Modern: 3867108 (0x3b01e4) · 4.3.4: 2055 (0x807) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_CORPSE_RECLAIM_DELAY (0x3b01e5)

- Modern: 3867109 (0x3b01e5) · 4.3.4: 3380 (0xd34) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_REATTACH_RESURRECT (0x3b01e6)

- Modern: 3867110 (0x3b01e6) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_PETITION_SIGN_RESULTS (0x3b01e7)

- Modern: 3867111 (0x3b01e7) · 4.3.4: 25111 (0x6217) · Area: General
- Layout: `{ guid guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 4 · (4 unused)

### SMSG_TURN_IN_PETITION_RESULT (0x3b01e9)

- Modern: 3867113 (0x3b01e9) · 4.3.4: 3847 (0xf07) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_USE_EQUIPMENT_SET_RESULT (0x3b01ea)

- Modern: 3867114 (0x3b01ea) · 4.3.4: 9252 (0x2424) · Area: General
- Layout: `{ u32 u64 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_FORCE_ANIM (0x3b01ec)

- Modern: 3867116 (0x3b01ec) · 4.3.4: 19461 (0x4c05) · Area: General
- Layout: `{ guid u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 9 · (7 unused)

### SMSG_INVALID_PROMOTION_CODE (0x3b01ee)

- Modern: 3867118 (0x3b01ee) · 4.3.4: 28453 (0x6f25) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_ITEM_TIME_UPDATE (0x3b01ef)

- Modern: 3867119 (0x3b01ef) · 4.3.4: 9223 (0x2407) · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_ITEM_ENCHANT_TIME_UPDATE (0x3b01f0)

- Modern: 3867120 (0x3b01f0) · 4.3.4: 3879 (0xf27) · Area: General
- Layout: `{ guid u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MAIL_LIST_RESULT (0x3b01f1)

- Modern: 3867121 (0x3b01f1) · 4.3.4: 16919 (0x4217) · Area: General
- Layout: `{ u32*2 loop[ { u64 u32 u64 u32 u64 u32*4 opt[ alt[ guid ] alt[ u32 ] ] u8*3 loop[ u8 u64 u32*4 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] loop[ { u32*3 u8 } ] ] bytes bytes } ] }`

### SMSG_MAIL_QUERY_NEXT_TIME_RESULT (0x3b01f2)

- Modern: 3867122 (0x3b01f2) · 4.3.4: — · Area: General
- Layout: `{ u32*2 loop[ { guid u32*2 u8 u32 } ] }`

### SMSG_PARTY_MEMBER_PARTIAL_STATE (0x3b01f3)

- Modern: 3867123 (0x3b01f3) · 4.3.4: — · Area: General
- Layout: `{ { u8*3 opt[ { u8 opt[ u8 bytes ] opt[ guid ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ { u32 loop[ { u32 u16 u32*2 loop[ u32 ] } ] } ] } ] guid loop[ u8 ] opt[ u16 ] opt[ u8 ] opt[ u16 ] opt[ u32 ] opt[ u32 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u16 ] opt[ u32 ] opt[ { u16*3 } ] opt[ u32 ] opt[ { u32 loop[ { u32 u16 u32*2 loop[ u32 ] } ] } ] opt[ { u32*2 guid loop[ u32 u16 ] } ] opt[ { u32*3 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### SMSG_PARTY_MEMBER_FULL_STATE (0x3b01f4)

- Modern: 3867124 (0x3b01f4) · 4.3.4: — · Area: General
- Layout: `{ u8 { loop[ u8 ] u16 u8 u16 u32*2 u16*6 u32 u16*3 u32*4 guid loop[ u32 u16 ] u32*3 loop[ { u32 u16 u32*2 loop[ u32 ] } ] u8 opt[ guid u32*4 loop[ { u32 u16 u32*2 loop[ u32 ] } ] u8 bytes ] } guid }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused) · 8 · 8; byte 68: 1 · (7 unused)

### SMSG_PARTY_KILL_LOG (0x3b01f5)

- Modern: 3867125 (0x3b01f5) · 4.3.4: 18743 (0x4937) · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_PROC_RESIST (0x3b01f6)

- Modern: 3867126 (0x3b01f6) · 4.3.4: 1062 (0x426) · Area: General
- Layout: `{ guid guid u32 u8 opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)

### SMSG_ISLAND_AZERITE_GAIN (0x3b01f7)

- Modern: 3867127 (0x3b01f7) · 4.3.4: — · Area: General
- Layout: `{ u32 guid guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_ISLAND_COMPLETE (0x3b01f8)

- Modern: 3867128 (0x3b01f8) · 4.3.4: — · Area: General
- Layout: `{ u32*3 loop[ { guid u32*2 u8*4 u32 bytes loop[ { u32*2 } ] loop[ { guid u8 u32*2 loop[ u32 ] { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u32*3 u8 ] loop[ u32 u8 ] loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] } ] } ] }`

### SMSG_WARFRONT_COMPLETE (0x3b01f9)

- Modern: 3867129 (0x3b01f9) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_EXPLORATION_EXPERIENCE (0x3b01fa)

- Modern: 3867130 (0x3b01fa) · 4.3.4: 26390 (0x6716) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_ARENA_TEAM_ROSTER (0x3b01fb)

- Modern: 3867131 (0x3b01fb) · 4.3.4: 10007 (0x2717) · Area: General
- Layout: `{ u32*9 u8 loop[ { guid u8 u32 u8*2 u32*5 u8 bytes opt[ u32 ] opt[ u32 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 36: 1 · (7 unused)

### SMSG_ARENA_TEAM_INVITE (0x3b01fc)

- Modern: 3867132 (0x3b01fc) · 4.3.4: 3894 (0xf36) · Area: General
- Layout: `{ guid u32 guid u8*2 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 6 · 7 · (3 unused)

### SMSG_ARENA_TEAM_EVENT (0x3b01fd)

- Modern: 3867133 (0x3b01fd) · 4.3.4: 1559 (0x617) · Area: General
- Layout: `{ u8 loop[ u8 opt[ u8 ] ] bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 9 · 9 · 9 · (5 unused)

### SMSG_ARENA_TEAM_COMMAND_RESULT (0x3b01fe)

- Modern: 3867134 (0x3b01fe) · 4.3.4: 14771 (0x39b3) · Area: General
- Layout: `{ u8*4 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8 · 7 · 6 · (3 unused)

### SMSG_ARENA_TEAM_STATS (0x3b01ff)

- Modern: 3867135 (0x3b01ff) · 4.3.4: 17445 (0x4425) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (28 bytes): +0: u32, +4: u32, +8: u32, +12: u32, +16: u32, +20: u32, +24: u32

### SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT (0x3b0200)

- Modern: 3867136 (0x3b0200) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u8 loop[ { guid guid u32 u8*4 u64 u32 u8*2 bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_LIVE_REGION_GET_ACCOUNT_CHARACTER_LIST_RESULT (0x3b0201)

- Modern: 3867137 (0x3b0201) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u8 loop[ { guid guid u32 u8*4 u64 u32 u8*2 bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_CHARACTER_RENAME_RESULT (0x3b0202)

- Modern: 3867138 (0x3b0202) · 4.3.4: 8228 (0x2024) · Area: General
- Layout: `{ u8*2 opt[ guid ] bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 1 · 6 · (1 unused)

### SMSG_MODIFY_COOLDOWN (0x3b0203)

- Modern: 3867139 (0x3b0203) · 4.3.4: 24598 (0x6016) · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 1 · (6 unused)

### SMSG_UPDATE_COOLDOWN (0x3b0204)

- Modern: 3867140 (0x3b0204) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### SMSG_UPDATE_CHARGE_CATEGORY_COOLDOWN (0x3b0205)

- Modern: 3867141 (0x3b0205) · 4.3.4: — · Area: General
- Layout: `{ u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_PRE_RESSURECT (0x3b0206)

- Modern: 3867142 (0x3b0206) · 4.3.4: 27702 (0x6c36) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_PLAY_SOUND (0x3b0207)

- Modern: 3867143 (0x3b0207) · 4.3.4: 8500 (0x2134) · Area: General
- Layout: `{ u32 guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_PLAY_MUSIC (0x3b0208)

- Modern: 3867144 (0x3b0208) · 4.3.4: 19206 (0x4b06) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_UI_ACTION (0x3b0209)

- Modern: 3867145 (0x3b0209) · 4.3.4: — · Area: General
- Layout: `{ u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_PLAY_OBJECT_SOUND (0x3b020a)

- Modern: 3867146 (0x3b020a) · 4.3.4: 9781 (0x2635) · Area: General
- Layout: `{ u32 guid guid u32*4 }`
- Size: 20 bytes (packed GUIDs not counted)

### SMSG_PLAY_SPEAKERBOT_SOUND (0x3b020b)

- Modern: 3867147 (0x3b020b) · 4.3.4: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_STOP_SPEAKERBOT_SOUND (0x3b020c)

- Modern: 3867148 (0x3b020c) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_LIVE_REGION_CHARACTER_COPY_RESULT (0x3b020d)

- Modern: 3867149 (0x3b020d) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_LIVE_REGION_ACCOUNT_RESTORE_RESULT (0x3b020e)

- Modern: 3867150 (0x3b020e) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SHOW_TRADE_SKILL_RESPONSE (0x3b0210)

- Modern: 3867152 (0x3b0210) · 4.3.4: — · Area: General
- Layout: `{ guid { u32*8 loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ u16 ] loop[ u16 ] } }`

### SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE (0x3b0211)

- Modern: 3867153 (0x3b0211) · 4.3.4: — · Area: General
- Layout: `{ { u32*6 loop[ { u32 u64*2 u32*6 loop[ u32 ] loop[ u32 ] u8 opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] loop[ { u32*12 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] loop[ { u32*2 u8 u32*3 u8 { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] } bytes { bytes } } ] loop[ { u32*5 u8*2 opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] } }`

### SMSG_BATTLE_PAY_GET_PURCHASE_LIST_RESPONSE (0x3b0212)

- Modern: 3867154 (0x3b0212) · 4.3.4: — · Area: General
- Layout: `{ u32*2 loop[ { u64 u32*3 u64*3 u8 bytes } ] }`

### SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE (0x3b0213)

- Modern: 3867155 (0x3b0213) · 4.3.4: — · Area: General
- Layout: `{ { u32 u8*2 loop[ { u64 u32*2 guid guid u32*2 u64 u32 u8 opt[ { u32*12 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 11 · (5 unused)

### SMSG_BATTLE_PAY_DISTRIBUTION_UNREVOKED (0x3b0214)

- Modern: 3867156 (0x3b0214) · 4.3.4: — · Area: General
- Layout: `{ u32 guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PAY_DISTRIBUTION_UPDATE (0x3b0215)

- Modern: 3867157 (0x3b0215) · 4.3.4: — · Area: General
- Layout: `{ { u64 u32*2 guid guid u32*2 u64 u32 u8 opt[ { u32*12 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 40: 1 · 1 · (6 unused)

### SMSG_BATTLE_PAY_DELIVERY_STARTED (0x3b0216)

- Modern: 3867158 (0x3b0216) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PAY_DELIVERY_ENDED (0x3b0217)

- Modern: 3867159 (0x3b0217) · 4.3.4: — · Area: General
- Layout: `{ u64 u32 loop[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] }`

### SMSG_BATTLE_PAY_MOUNT_DELIVERED (0x3b0218)

- Modern: 3867160 (0x3b0218) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BATTLE_PAY_BATTLE_PET_DELIVERED (0x3b0219)

- Modern: 3867161 (0x3b0219) · 4.3.4: — · Area: General
- Layout: `{ u32 guid }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PAY_COLLECTION_ITEM_DELIVERED (0x3b021a)

- Modern: 3867162 (0x3b021a) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_INSTANCE_SAVE_CREATED (0x3b021c)

- Modern: 3867164 (0x3b021c) · 4.3.4: 292 (0x124) · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_ENCOUNTER_START (0x3b021d)

- Modern: 3867165 (0x3b021d) · 4.3.4: — · Area: General
- Layout: `{ u32*4 loop[ { guid u8 u32*10 loop[ u32 ] loop[ u32 ] u32*2 loop[ u32 ] loop[ u32 ] loop[ guid u32*2 ] loop[ u32 ] loop[ { u32*5 loop[ u32 ] loop[ u32 ] } ] u8 opt[ u32*2 loop[ u32 u16 u8 ] loop[ u32 u8*2 ] ] opt[ { u32*5 loop[ u32 ] loop[ u32*2 ] loop[ u32*3 ] } ] } ] }`

### SMSG_ENCOUNTER_END (0x3b021e)

- Modern: 3867166 (0x3b021e) · 4.3.4: — · Area: General
- Layout: `{ u32*4 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)

### SMSG_BATTLE_PAY_START_PURCHASE_RESPONSE (0x3b021f)

- Modern: 3867167 (0x3b021f) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u64 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_BATTLE_PAY_START_DISTRIBUTION_ASSIGN_TO_TARGET_RESPONSE (0x3b0220)

- Modern: 3867168 (0x3b0220) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +8: u32

### SMSG_BATTLE_PAY_PURCHASE_UPDATE (0x3b0222)

- Modern: 3867170 (0x3b0222) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ { u64 u32*3 u64*3 u8 bytes } ] }`

### SMSG_BATTLE_PAY_CONFIRM_PURCHASE (0x3b0223)

- Modern: 3867171 (0x3b0223) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: 8 bytes, +8: u32

### SMSG_BATTLE_PAY_ACK_FAILED (0x3b0224)

- Modern: 3867172 (0x3b0224) · 4.3.4: — · Area: General
- Layout: `{ u64 u32*3 }`
- Size: 20 bytes (packed GUIDs not counted)

### SMSG_CONTACT_LIST (0x3b0227)

- Modern: 3867175 (0x3b0227) · 4.3.4: 24599 (0x6017) · Area: General
- Layout: `{ u32 u8 loop[ guid guid u32*3 u8 u32*3 u8*2 bytes ] }`

### SMSG_FRIEND_STATUS (0x3b0228)

- Modern: 3867176 (0x3b0228) · 4.3.4: 1815 (0x717) · Area: General
- Layout: `{ u8 guid guid u32 u8 u32*3 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 10 · (6 unused)

### SMSG_CHARACTER_OBJECT_TEST_RESPONSE (0x3b0229)

- Modern: 3867177 (0x3b0229) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_BATTLENET_CHALLENGE_START (0x3b022a)

- Modern: 3867178 (0x3b022a) · 4.3.4: — · Area: General
- Layout: `{ u32 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 9 · (7 unused)

### SMSG_BATTLENET_CHALLENGE_ABORT (0x3b022b)

- Modern: 3867179 (0x3b022b) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_GROUP_DECLINE (0x3b022c)

- Modern: 3867180 (0x3b022c) · 4.3.4: 26677 (0x6835) · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### SMSG_GROUP_REQUEST_DECLINE (0x3b022d)

- Modern: 3867181 (0x3b022d) · 4.3.4: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### SMSG_GROUP_UNINVITE (0x3b022e)

- Modern: 3867182 (0x3b022e) · 4.3.4: 2567 (0xa07) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_GROUP_DESTROYED (0x3b022f)

- Modern: 3867183 (0x3b022f) · 4.3.4: 8711 (0x2207) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_GROUP_AUTO_KICK (0x3b0230)

- Modern: 3867184 (0x3b0230) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PARTY_COMMAND_RESULT (0x3b0231)

- Modern: 3867185 (0x3b0231) · 4.3.4: 28167 (0x6e07) · Area: General
- Layout: `{ u8*3 u32 guid bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · 4 · 6 · (5 unused)

### SMSG_SUGGEST_INVITE_INFORM (0x3b0232)

- Modern: 3867186 (0x3b0232) · 4.3.4: — · Area: General
- Layout: `{ u8*3 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · 9 · (6 unused)

### SMSG_GOSSIP_POI (0x3b0233)

- Modern: 3867187 (0x3b0233) · 4.3.4: 17174 (0x4316) · Area: General
- Layout: `{ u32*8 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 32: 6 · (2 unused)

### SMSG_READ_ITEM_RESULT_OK (0x3b023c)

- Modern: 3867196 (0x3b023c) · 4.3.4: 9733 (0x2605) · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_READ_ITEM_RESULT_FAILED (0x3b0244)

- Modern: 3867204 (0x3b0244) · 4.3.4: 3862 (0xf16) · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 2 · (6 unused)

### SMSG_SCENARIO_VACATE (0x3b0245)

- Modern: 3867205 (0x3b0245) · 4.3.4: — · Area: General
- Layout: `{ guid u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 2 · (6 unused)

### SMSG_CHAR_FACTION_CHANGE_RESULT (0x3b0247)

- Modern: 3867207 (0x3b0247) · 4.3.4: 19462 (0x4c06) · Area: General
- Layout: `{ u8 guid u8 opt[ { u8*3 u32 bytes loop[ { u32*2 } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 3: 1 · (7 unused)

### SMSG_RAID_DIFFICULTY_SET (0x3b0248)

- Modern: 3867208 (0x3b0248) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +0: u32, +4: u8

### SMSG_XP_GAIN_ENABLED (0x3b0249)

- Modern: 3867209 (0x3b0249) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_RAID_GROUP_ONLY (0x3b024a)

- Modern: 3867210 (0x3b024a) · 4.3.4: 2103 (0x837) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_INSTANCE_ENCOUNTER_ENGAGE_UNIT (0x3b024b)

- Modern: 3867211 (0x3b024b) · 4.3.4: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_INSTANCE_ENCOUNTER_DISENGAGE_UNIT (0x3b024c)

- Modern: 3867212 (0x3b024c) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_INSTANCE_ENCOUNTER_CHANGE_PRIORITY (0x3b024d)

- Modern: 3867213 (0x3b024d) · 4.3.4: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_INSTANCE_ENCOUNTER_TIMER_START (0x3b024e)

- Modern: 3867214 (0x3b024e) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_START (0x3b024f)

- Modern: 3867215 (0x3b024f) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_COMPLETE (0x3b0250)

- Modern: 3867216 (0x3b0250) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_INSTANCE_ENCOUNTER_START (0x3b0251)

- Modern: 3867217 (0x3b0251) · 4.3.4: — · Area: General
- Layout: `{ u32*4 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · (7 unused)

### SMSG_INSTANCE_ENCOUNTER_UPDATE_SUPPRESS_RELEASE (0x3b0252)

- Modern: 3867218 (0x3b0252) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_INSTANCE_ENCOUNTER_UPDATE_ALLOW_RELEASE_IN_PROGRESS (0x3b0253)

- Modern: 3867219 (0x3b0253) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_UPDATE (0x3b0254)

- Modern: 3867220 (0x3b0254) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_INSTANCE_ENCOUNTER_END (0x3b0255)

- Modern: 3867221 (0x3b0255) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_INSTANCE_ENCOUNTER_IN_COMBAT_RESURRECTION (0x3b0256)

- Modern: 3867222 (0x3b0256) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_INSTANCE_ENCOUNTER_GAIN_COMBAT_RESURRECTION_CHARGE (0x3b0257)

- Modern: 3867223 (0x3b0257) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_INSTANCE_ENCOUNTER_PHASE_SHIFT_CHANGED (0x3b0258)

- Modern: 3867224 (0x3b0258) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_TUTORIAL_FLAGS (0x3b0259)

- Modern: 3867225 (0x3b0259) · 4.3.4: 2869 (0xb35) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (32 bytes): +0: u32, +0: 8 bytes, +4: u32, +8: u32, +8: 8 bytes, +12: u32, +16: u32, +16: 8 bytes, +20: u32, +24: u32, +24: 8 bytes, +28: u32

### SMSG_CHARACTER_UPGRADE_STARTED (0x3b025a)

- Modern: 3867226 (0x3b025a) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CHARACTER_UPGRADE_COMPLETE (0x3b025b)

- Modern: 3867227 (0x3b025b) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CHARACTER_UPGRADE_ABORTED (0x3b025c)

- Modern: 3867228 (0x3b025c) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_CHARACTER_CHECK_UPGRADE_RESULT (0x3b025d)

- Modern: 3867229 (0x3b025d) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_CHARACTER_UPGRADE_MANUAL_UNREVOKE_RESULT (0x3b025e)

- Modern: 3867230 (0x3b025e) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_UPDATE_CHARACTER_FLAGS (0x3b025f)

- Modern: 3867231 (0x3b025f) · 4.3.4: — · Area: General
- Layout: `{ guid u8 opt[ u32 ] opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · 1 · (5 unused)

### SMSG_ITEM_COOLDOWN (0x3b0263)

- Modern: 3867235 (0x3b0263) · 4.3.4: 19732 (0x4d14) · Area: General
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_EMOTE (0x3b0264)

- Modern: 3867236 (0x3b0264) · 4.3.4: 2612 (0xa34) · Area: General
- Layout: `{ guid u32 { u32*2 loop[ u32 ] } }`

### SMSG_TRIGGER_CINEMATIC (0x3b0265)

- Modern: 3867237 (0x3b0265) · 4.3.4: 27687 (0x6c27) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_UNDELETE_CHARACTER_RESPONSE (0x3b0266)

- Modern: 3867238 (0x3b0266) · 4.3.4: — · Area: General
- Layout: `{ u32*2 guid }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_UNDELETE_COOLDOWN_STATUS_RESPONSE (0x3b0267)

- Modern: 3867239 (0x3b0267) · 4.3.4: — · Area: General
- Layout: `{ u8 u32*2 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SET_LOOT_METHOD_FAILED (0x3b026b)

- Modern: 3867243 (0x3b026b) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_COMMERCE_TOKEN_GET_COUNT_RESPONSE (0x3b026c)

- Modern: 3867244 (0x3b026c) · 4.3.4: — · Area: General
- Layout: `{ u32*4 loop[ u64 ] loop[ u64 ] }`

### SMSG_COMMERCE_TOKEN_UPDATE (0x3b026d)

- Modern: 3867245 (0x3b026d) · 4.3.4: — · Area: General
- Layout: `{ u32*2 loop[ u64 ] loop[ u64 ] }`

### SMSG_COMMERCE_TOKEN_GET_MARKET_PRICE_RESPONSE (0x3b026e)

- Modern: 3867246 (0x3b026e) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +12: u32, +16: u32

### SMSG_AUCTIONABLE_TOKEN_SELL_CONFIRM_REQUIRED (0x3b026f)

- Modern: 3867247 (0x3b026f) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (20 bytes): +0: 8 bytes, +12: u32, +16: u32

### SMSG_AUCTIONABLE_TOKEN_SELL_AT_MARKET_PRICE_RESPONSE (0x3b0270)

- Modern: 3867248 (0x3b0270) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +4: u32

### SMSG_AUCTIONABLE_TOKEN_AUCTION_SOLD (0x3b0271)

- Modern: 3867249 (0x3b0271) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CONSUMABLE_TOKEN_CAN_VETERAN_BUY_RESPONSE (0x3b0272)

- Modern: 3867250 (0x3b0272) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +12: u32

### SMSG_CONSUMABLE_TOKEN_BUY_CHOICE_REQUIRED (0x3b0273)

- Modern: 3867251 (0x3b0273) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CONSUMABLE_TOKEN_BUY_AT_MARKET_PRICE_RESPONSE (0x3b0274)

- Modern: 3867252 (0x3b0274) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +4: u32

### SMSG_GET_REMAINING_GAME_TIME_RESPONSE (0x3b0275)

- Modern: 3867253 (0x3b0275) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_CONSUMABLE_TOKEN_REDEEM_CONFIRM_REQUIRED (0x3b0276)

- Modern: 3867254 (0x3b0276) · 4.3.4: — · Area: General
- Layout: `{ u32*3 u64*2 u32 u8 }`
- Size: 33 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 32: 1 · 1 · (6 unused)

### SMSG_CONSUMABLE_TOKEN_REDEEM_RESPONSE (0x3b0277)

- Modern: 3867255 (0x3b0277) · 4.3.4: — · Area: General
- Layout: `{ u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_COMMERCE_TOKEN_GET_LOG_RESPONSE (0x3b0278)

- Modern: 3867256 (0x3b0278) · 4.3.4: — · Area: General
- Layout: `{ u32*3 loop[ { u64*3 u32*2 } ] }`

### SMSG_SCENARIO_COMPLETED (0x3b0287)

- Modern: 3867271 (0x3b0287) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GET_VAS_ACCOUNT_CHARACTER_LIST_RESULT (0x3b028a)

- Modern: 3867274 (0x3b028a) · 4.3.4: — · Area: General
- Layout: `{ u32*4 loop[ { guid guid u32 u8*4 u64 u32 u8*2 bytes bytes } ] }`

### SMSG_GET_VAS_TRANSFER_TARGET_REALM_LIST_RESULT (0x3b028b)

- Modern: 3867275 (0x3b028b) · 4.3.4: — · Area: General
- Layout: `{ u32*4 loop[ { u32*6 u8*2 u32 u8*2 bytes } ] }`

### SMSG_VAS_PURCHASE_STATE_UPDATE (0x3b028c)

- Modern: 3867276 (0x3b028c) · 4.3.4: — · Area: General
- Layout: `{ u32 { guid u32*2 u64 u8 loop[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 2 · (6 unused)

### SMSG_VAS_PURCHASE_COMPLETE (0x3b028d)

- Modern: 3867277 (0x3b028d) · 4.3.4: — · Area: General
- Layout: `{ u32*2 guid guid u32 guid u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 6 · (2 unused)

### SMSG_ENUM_VAS_PURCHASE_STATES_RESPONSE (0x3b028e)

- Modern: 3867278 (0x3b028e) · 4.3.4: — · Area: General
- Layout: `{ u8 loop[ { guid u32*2 u64 u8 loop[ u32 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · (2 unused)

### SMSG_ALLIED_RACE_DETAILS (0x3b0295)

- Modern: 3867285 (0x3b0295) · 4.3.4: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_COVENANT_PREVIEW_OPEN_NPC (0x3b0298)

- Modern: 3867288 (0x3b0298) · 4.3.4: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_SCENARIO_UI_UPDATE (0x3b029c)

- Modern: 3867292 (0x3b029c) · 4.3.4: — · Area: General
- Layout: `{ u32*2 loop[ u32 u8 ] }`

### SMSG_SCENARIO_SHOW_CRITERIA (0x3b029d)

- Modern: 3867293 (0x3b029d) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GAME_OBJECT_SET_STATE_LOCAL (0x3b029f)

- Modern: 3867295 (0x3b029f) · 4.3.4: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_BATTLENET_RESPONSE (0x3b02a0)

- Modern: 3867296 (0x3b02a0) · 4.3.4: — · Area: General
- Layout: `{ u32 { u64*2 u32 } u32 bytes }`

### SMSG_BATTLENET_NOTIFICATION (0x3b02a1)

- Modern: 3867297 (0x3b02a1) · 4.3.4: — · Area: General
- Layout: `{ { u64*2 u32 } u32 bytes }`

### SMSG_BATTLE_NET_CONNECTION_STATUS (0x3b02a2)

- Modern: 3867298 (0x3b02a2) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 2 · 1 · (5 unused)

### SMSG_CHANGE_REALM_TICKET_RESPONSE (0x3b02a3)

- Modern: 3867299 (0x3b02a3) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_FAILED_QUEST_TURN_IN (0x3b02ac)

- Modern: 3867308 (0x3b02ac) · 4.3.4: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### SMSG_QUEUE_SUMMARY_UPDATE (0x3b02ad)

- Modern: 3867309 (0x3b02ad) · 4.3.4: — · Area: General
- Layout: `{ u64*2 guid { guid guid u32*2 u8 loop[ { u8 opt[ opt[ opt[ u32*3 u8 u32 u8 ] alt[ { { guid u32*2 u64 u8 } u32 u8 guid guid guid guid guid u32*8 u64 u8 guid { loop[ u32 u8 ] } u32 loop[ guid ] loop[ guid ] loop[ guid ] { u8*6 { u32*3 loop[ u32*4 u8 ] } u32*2 loop[ u32 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] } u8 { u32*3 loop[ u32*4 u8 ] } loop[ { guid u8*3 u32 u8*2 } ] } ] alt[ { u32*3 loop[ u32 ] loop[ u32 ] loop[ u8 ] u8 } ] ] ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 30: 1 · 1 · (6 unused)

### SMSG_INVENTORY_FIXUP_COMPLETE (0x3b02ae)

- Modern: 3867310 (0x3b02ae) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_CONFIRM_PARTY_INVITE (0x3b02af)

- Modern: 3867311 (0x3b02af) · 4.3.4: — · Area: General
- Layout: `{ { guid guid { u32*3 loop[ u32 ] loop[ u32 ] loop[ u64 ] } u64 u8 u32*3 u8 { opt[ u8 ] alt[ u8 ] opt[ u8 ] } opt[ u8 ] opt[ u8 ] opt[ u8 ] { u8*2 guid u32 u64 u32 u8 bytes } bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 37: 8 · 9 · 1 · 1 · 1 · (4 unused) · 9 · 1 · 1 · (5 unused)

### SMSG_CAN_REDEEM_TOKEN_FOR_BALANCE_RESPONSE (0x3b02b0)

- Modern: 3867312 (0x3b02b0) · 4.3.4: — · Area: General
- Layout: `{ u32 u64*2 u32 u8 }`
- Size: 25 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)

### SMSG_BATTLE_PAY_VALIDATE_PURCHASE_RESPONSE (0x3b02b1)

- Modern: 3867313 (0x3b02b1) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u64*2 u8 }`
- Size: 25 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)

### SMSG_VAS_GET_SERVICE_STATUS_RESPONSE (0x3b02b2)

- Modern: 3867314 (0x3b02b2) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 4

### SMSG_VAS_GET_QUEUE_MINUTES_RESPONSE (0x3b02b3)

- Modern: 3867315 (0x3b02b3) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: 8 bytes, +8: u32

### SMSG_VAS_CHECK_TRANSFER_OK_RESPONSE (0x3b02b5)

- Modern: 3867317 (0x3b02b5) · 4.3.4: — · Area: General
- Layout: `{ u32*2 guid u32 loop[ { guid u8*2 bytes } ] }`

### SMSG_CONTRIBUTION_LAST_UPDATE_RESPONSE (0x3b02b6)

- Modern: 3867318 (0x3b02b6) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: 8 bytes, +8: u32, +12: u32

### SMSG_GENERATE_SSO_TOKEN_RESPONSE (0x3b02b7)

- Modern: 3867319 (0x3b02b7) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u64*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 7 · (1 unused)

### SMSG_VOICE_LOGIN_RESPONSE (0x3b02b8)

- Modern: 3867320 (0x3b02b8) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 u8*3 { bytes } { bytes } { bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 5: 1 · 4 · 1 · (1 unused) · 1 · 3 · 1 · (1 unused) · 1 · 6 · 1 · (3 unused)

### SMSG_VOICE_CHANNEL_INFO_RESPONSE (0x3b02b9)

- Modern: 3867321 (0x3b02b9) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 u8 u64*2 guid u8*3 { bytes } { bytes } { bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 4 · 1 · (1 unused) · 1 · 6 · 1 · (1 unused) · 1 · 4 · 1 · (2 unused)

### SMSG_UPDATE_CELESTIAL_BODY (0x3b02ba)

- Modern: 3867322 (0x3b02ba) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_WARDEN3_ENABLED (0x3b02bb)

- Modern: 3867323 (0x3b02bb) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_WARDEN3_DISABLED (0x3b02bc)

- Modern: 3867324 (0x3b02bc) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLE_PAY_START_CHECKOUT (0x3b02bd)

- Modern: 3867325 (0x3b02bd) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u64 u8*2 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 6 · 7 · 1 · (2 unused)

### SMSG_UPDATE_BNET_SESSION_KEY (0x3b02be)

- Modern: 3867326 (0x3b02be) · 4.3.4: — · Area: General
- Layout: `{ u8 loop[ u8 ] loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · (1 unused) · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### SMSG_INVENTORY_FULL_OVERFLOW (0x3b02bf)

- Modern: 3867327 (0x3b02bf) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_WILL_BE_KICKED_FOR_ADDED_SUBSCRIPTION_TIME (0x3b02c0)

- Modern: 3867328 (0x3b02c0) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_UPDATE_GAME_TIME_STATE (0x3b02c1)

- Modern: 3867329 (0x3b02c1) · 4.3.4: — · Area: General
- Layout: `{ { u32*3 u8 } }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · 1 · 1 · (5 unused)

### SMSG_GAME_OBJECT_BASE (0x3b02c3)

- Modern: 3867331 (0x3b02c3) · 4.3.4: — · Area: General
- Layout: `{ guid { u8 opt[ u8 opt[ u8 u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_LEGACY_LOOT_RULES (0x3b02c4)

- Modern: 3867332 (0x3b02c4) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_BATCH_PRESENCE_SUBSCRIPTION (0x3b02c9)

- Modern: 3867337 (0x3b02c9) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_MOVEMENT_ENFORCEMENT_ALERT (0x3b02ca)

- Modern: 3867338 (0x3b02ca) · 4.3.4: — · Area: General
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 12 · (4 unused)

### SMSG_PREPOPULATE_NAME_CACHE (0x3b02cd)

- Modern: 3867341 (0x3b02cd) · 4.3.4: — · Area: General
- Layout: `{ u64 u32 loop[ { u8*6 bytes bytes bytes bytes bytes guid guid guid u64 u32 u8*5 u32 bytes } ] }`

### SMSG_RETURN_RECRUITING_CLUBS (0x3b02d0)

- Modern: 3867344 (0x3b02d0) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ u32 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 3 · (5 unused)

### SMSG_RETURN_APPLICANT_LIST (0x3b02d1)

- Modern: 3867345 (0x3b02d1) · 4.3.4: — · Area: General
- Layout: `{ guid u32 u8 loop[ { guid guid u32*5 u64*2 u32 u8*3 bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 3 · (5 unused)

### SMSG_CLUB_FINDER_RESPONSE_CHARACTER_APPLICATION_LIST (0x3b02d2)

- Modern: 3867346 (0x3b02d2) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 loop[ { guid guid u32 u64 u8 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 3 · (5 unused)

### SMSG_CLUB_FINDER_UPDATE_APPLICATIONS (0x3b02d3)

- Modern: 3867347 (0x3b02d3) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 loop[ { guid guid u32 u64 u8 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 3 · (5 unused)

### SMSG_CLUB_FINDER_ERROR_MESSAGE (0x3b02d4)

- Modern: 3867348 (0x3b02d4) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 3 · 4 · (1 unused)

### SMSG_CLUB_FINDER_LOOKUP_CLUB_POSTINGS_LIST (0x3b02d5)

- Modern: 3867349 (0x3b02d5) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 loop[ { u8*4 guid u32 u64 u32*3 guid u64*2 bytes bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 3 · 1 · (4 unused)

### SMSG_CLUB_FINDER_RESPONSE_POST_RECRUITMENT_MESSAGE (0x3b02d6)

- Modern: 3867350 (0x3b02d6) · 4.3.4: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 3 · 3 · (2 unused)

### SMSG_CLUB_FINDER_GET_CLUB_POSTING_IDS_RESPONSE (0x3b02d7)

- Modern: 3867351 (0x3b02d7) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ { u64 u32*2 } ] }`

### SMSG_QUEST_SESSION_RESULT (0x3b02dd)

- Modern: 3867357 (0x3b02dd) · 4.3.4: — · Area: General
- Layout: `{ u8 guid }`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_QUEST_SESSION_READY_CHECK (0x3b02de)

- Modern: 3867358 (0x3b02de) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_QUEST_SESSION_READY_CHECK_RESPONSE (0x3b02df)

- Modern: 3867359 (0x3b02df) · 4.3.4: — · Area: General
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_AUCTION_LIST_BUCKETS_RESULT (0x3b02e5)

- Modern: 3867365 (0x3b02e5) · 4.3.4: — · Area: General
- Layout: `{ u32*4 u8 loop[ { { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } u32*2 u64 u32 loop[ u32 ] u8 opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u32 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 1 · 1 · (6 unused)

### SMSG_AUCTION_LIST_ITEMS_RESULT (0x3b02e6)

- Modern: 3867366 (0x3b02e6) · 4.3.4: — · Area: General
- Layout: `{ { u32*3 loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 u32 loop[ { u32*3 u8 } ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] } { u8 { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } u32 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 2 · 1 · (5 unused) · 20; byte 15: 1 · 11; byte 17: 1 · (7 unused)

### SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT (0x3b02e8)

- Modern: 3867368 (0x3b02e8) · 4.3.4: — · Area: General
- Layout: `{ u32*3 u8 loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 u32 loop[ { u32*3 u8 } ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 u32 loop[ { u32*3 u8 } ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT (0x3b02e9)

- Modern: 3867369 (0x3b02e9) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u8 loop[ { u8*2 opt[ u8 ] opt[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*4 guid u32 u8 u32 loop[ { u32*3 u8 } ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ u64 ] opt[ guid guid u32 ] opt[ guid ] opt[ opt[ guid ] opt[ u64 ] ] loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] opt[ { loop[ u8 ] u8*3 opt[ u16 ] opt[ u16 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_AUCTION_GET_COMMODITY_QUOTE_RESULT (0x3b02ea)

- Modern: 3867370 (0x3b02ea) · 4.3.4: — · Area: General
- Layout: `{ u8 u32*2 opt[ u64 ] opt[ u32 ] opt[ u64 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · (5 unused)

### SMSG_ACCOUNT_CRITERIA_UPDATE (0x3b02eb)

- Modern: 3867371 (0x3b02eb) · 4.3.4: — · Area: General
- Layout: `{ { u32 u64 guid u32*2 { u32 } u64*2 u8 opt[ u64 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 42: 1 · (7 unused)

### SMSG_SYNC_WOW_ENTITLEMENTS (0x3b02ee)

- Modern: 3867374 (0x3b02ee) · 4.3.4: — · Area: General
- Layout: `{ u32*2 loop[ { u32 u64*2 u32 u8 } ] loop[ { u32*12 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } ] }`

### SMSG_WOW_ENTITLEMENT_NOTIFICATION (0x3b02ef)

- Modern: 3867375 (0x3b02ef) · 4.3.4: — · Area: General
- Layout: `{ { u8 u32 u64*2 u32 u8 } { u32*12 u8*3 loop[ u32 u8 u32*4 u8 opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] ] bytes opt[ { u8*4 bits(5) u8 alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 alt[ u8 ] u8 alt[ u8 ] u32*4 opt[ u32 ] opt[ u32 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] bytes bytes loop[ u8*2 u32*3 bytes ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 3 · (5 unused); byte 25: 1 · (7 unused); byte 74: 8 · 1 · 1 · 7 · 1 · (6 unused)

### SMSG_AUCTION_FAVORITE_LIST (0x3b02f2)

- Modern: 3867378 (0x3b02f2) · 4.3.4: — · Area: General
- Layout: `{ u32 u8 loop[ { u32*5 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 7 · (1 unused)

### SMSG_PARTY_NOTIFY_LFG_LEADER_CHANGE (0x3b02fa)

- Modern: 3867386 (0x3b02fa) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_AREA_TRIGGER_MESSAGE (0x3b0303)

- Modern: 3867395 (0x3b0303) · 4.3.4: 17669 (0x4505) · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_VOICE_CHANNEL_STT_TOKEN_RESPONSE (0x3b0304)

- Modern: 3867396 (0x3b0304) · 4.3.4: — · Area: General
- Layout: `{ u32 u8*2 { bytes } { bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 4 · 1 · (1 unused) · 1 · 6 · 1 · (1 unused)

### SMSG_ACCOUNT_NOTIFICATIONS_RESPONSE (0x3b0305)

- Modern: 3867397 (0x3b0305) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ u64*2 ] }`

### SMSG_LATENCY_REPORT_PING (0x3b0306)

- Modern: 3867398 (0x3b0306) · 4.3.4: — · Area: General
- Layout: `{ u32*2 loop[ { { u32 u16 u8 } u64 u32 } ] }`

### SMSG_UPDATE_AADC_STATUS_RESPONSE (0x3b0308)

- Modern: 3867400 (0x3b0308) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_VAS_RESPONSE (0x3b030a)

- Modern: 3867402 (0x3b030a) · 4.3.4: — · Area: General
- Layout: `{ u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_NPC_INTERACTION_OPEN_RESULT (0x3b030b)

- Modern: 3867403 (0x3b030b) · 4.3.4: — · Area: General
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_GAME_OBJECT_INTERACTION (0x3b030c)

- Modern: 3867404 (0x3b030c) · 4.3.4: — · Area: General
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_GAME_OBJECT_CLOSE_INTERACTION (0x3b030d)

- Modern: 3867405 (0x3b030d) · 4.3.4: — · Area: General
- Layout: `{ struct }`

### SMSG_CLUB_FINDER_WHISPER_APPLICANT_RESPONSE (0x3b030e)

- Modern: 3867406 (0x3b030e) · 4.3.4: — · Area: General
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_LOBBY_MATCHMAKER_LOBBY_ACQUIRED_SERVER (0x3b030f)

- Modern: 3867407 (0x3b030f) · 4.3.4: — · Area: General
- Layout: `{ { u32 u16 u8 } u32 }`
- Size: 11 bytes (packed GUIDs not counted)

### SMSG_LOBBY_MATCHMAKER_PARTY_INFO (0x3b0310)

- Modern: 3867408 (0x3b0310) · 4.3.4: — · Area: General
- Layout: `{ { guid u32*3 guid guid u8 loop[ { u8 guid guid u64 u8*2 u32 loop[ u32 ] u32 bytes loop[ { u32*2 } ] loop[ { u32*2 } ] } ] loop[ { u8 guid guid u64 u8*2 u32 loop[ u32 ] u32 bytes loop[ { u32*2 } ] loop[ { u32*2 } ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 2 · 1 · 1 · 1 · (3 unused)

### SMSG_LOBBY_MATCHMAKER_PARTY_INVITE_REJECTED (0x3b0311)

- Modern: 3867409 (0x3b0311) · 4.3.4: — · Area: General
- Layout: `{ u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · (2 unused)

### SMSG_LOBBY_MATCHMAKER_RECEIVE_INVITE (0x3b0312)

- Modern: 3867410 (0x3b0312) · 4.3.4: — · Area: General
- Layout: `{ guid u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 6 · (2 unused)

### SMSG_LOBBY_MATCHMAKER_QUEUE_PROPOSED (0x3b0313)

- Modern: 3867411 (0x3b0313) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LOBBY_MATCHMAKER_QUEUE_RESULT (0x3b0314)

- Modern: 3867412 (0x3b0314) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 3 · (5 unused)

### SMSG_SOCIAL_CONTRACT_REQUEST_RESPONSE (0x3b0316)

- Modern: 3867414 (0x3b0316) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_WOW_LABS_NOTIFY_PLAYERS_MATCH_END (0x3b0317)

- Modern: 3867415 (0x3b0317) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_WOW_LABS_NOTIFY_PLAYERS_MATCH_STATE_CHANGED (0x3b0318)

- Modern: 3867416 (0x3b0318) · 4.3.4: — · Area: General
- Layout: `{ u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_WOW_LABS_SET_WOW_LABS_AREA_ID_RESPONSE (0x3b0319)

- Modern: 3867417 (0x3b0319) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_QUERY_SELECTED_WOW_LABS_AREA_RESPONSE (0x3b031a)

- Modern: 3867418 (0x3b031a) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_QUERY_WOW_LABS_AREA_INFO_RESPONSE (0x3b031b)

- Modern: 3867419 (0x3b031b) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ u32*5 ] }`

### — (0x3b031c)

- Modern: 3867420 (0x3b031c) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_WOW_LABS_SET_PREDICTION_CIRCLE (0x3b031d)

- Modern: 3867421 (0x3b031d) · 4.3.4: — · Area: General
- Layout: `{ u32*4 }`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_AUCTION_DISABLE_NEW_POSTINGS (0x3b0322)

- Modern: 3867426 (0x3b0322) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_WOW_LABS_PARTY_ERROR (0x3b0324)

- Modern: 3867428 (0x3b0324) · 4.3.4: — · Area: General
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_ACCOUNT_EXPORT_RESPONSE (0x3b0327)

- Modern: 3867431 (0x3b0327) · 4.3.4: — · Area: General
- Layout: `{ u32*2 u8 u32 bytes }`

### SMSG_SPECTATE_PLAYER (0x3b0329)

- Modern: 3867433 (0x3b0329) · 4.3.4: — · Area: General
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_SPECTATE_END (0x3b032a)

- Modern: 3867434 (0x3b032a) · 4.3.4: — · Area: General
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_NEW_DATA_BUILD (0x3b032b)

- Modern: 3867435 (0x3b032b) · 4.3.4: — · Area: General
- Layout: `{ { loop[ u8*2 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### SMSG_GET_REALM_HIDDEN_RESULT (0x3b032c)

- Modern: 3867436 (0x3b032c) · 4.3.4: — · Area: General
- Layout: `{ u8*2 { bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 8 · 1 · (6 unused)

### SMSG_HARDCORE_DEATH_ALERT (0x3b0334)

- Modern: 3867444 (0x3b0334) · 4.3.4: — · Area: General
- Layout: `{ u8 u32 u8 bytes opt[ { guid u32*2 guid } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 1 · (1 unused)

### SMSG_ACCOUNT_CHARACTER_CURRENCY_LISTS (0x3b0336)

- Modern: 3867446 (0x3b0336) · 4.3.4: — · Area: General
- Layout: `{ u32 loop[ { guid u32 loop[ u32*2 ] } ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
