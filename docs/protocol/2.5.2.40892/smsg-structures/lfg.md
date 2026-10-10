# LFG — server packet layouts, 2.5.2.40892

### SMSG_LFG_JOIN_RESULT (0x2a1c)

- Modern: 10780 (0x2a1c) · 2.4.3: — · Area: LFG
- Layout: `{ { guid u32*2 u64 } u8*2 u32 loop[ { u8 u32 opt[ guid ] loop[ u32*5 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 8 · 8
- HermesProxy: `DFJoinResult` — differs

### SMSG_LFG_LIST_JOIN_RESULT (0x2a1d)

- Modern: 10781 (0x2a1d) · 2.4.3: — · Area: LFG
- Layout: `{ { guid u32*2 u64 } u8*2 }`
- Size: 18 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 8 · 8

### SMSG_LFG_LIST_SEARCH_RESULTS (0x2a1e)

- Modern: 10782 (0x2a1e) · 2.4.3: — · Area: LFG
- Layout: `{ u16 u32 alt[ ] loop[ { guid u32*2 u64 u32 guid guid guid guid guid u32*4 u32 u32 u32 u32 u64 u8 guid loop[ guid ] loop[ guid ] loop[ guid ] { loop[ u32 ] u32*2 u8*4 bytes bytes bytes opt[ u32 ] } loop[ guid u8*3 ] } ] }`

### SMSG_LFG_LIST_SEARCH_STATUS (0x2a1f)

- Modern: 10783 (0x2a1f) · 2.4.3: — · Area: LFG
- Layout: `{ { guid u32*2 u64 } u8*2 }`
- Size: 18 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 8 · 1 · (7 unused)

### SMSG_LFG_QUEUE_STATUS (0x2a20)

- Modern: 10784 (0x2a20) · 2.4.3: — · Area: LFG
- Layout: `{ { guid u32*2 u64 } u32*3 loop[ u32 u8 ] u32 }`
- HermesProxy: `DFQueueStatus` — differs

### SMSG_LFG_ROLE_CHECK_UPDATE (0x2a21)

- Modern: 10785 (0x2a21) · 2.4.3: — · Area: LFG
- Layout: `{ u8*2 u32 u64 u32*2 loop[ u32 ] u8 loop[ guid u32 u8*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8; byte 22: 1 · 1 · (6 unused)
- HermesProxy: `LFGRoleCheckUpdate` — differs

### SMSG_LFG_READY_CHECK_UPDATE (0x2a22)

- Modern: 10786 (0x2a22) · 2.4.3: — · Area: LFG
- Layout: `{ u8*2 u64 u32 u8 loop[ guid u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8; byte 14: 1 · (7 unused)

### SMSG_LFG_UPDATE_STATUS (0x2a24)

- Modern: 10788 (0x2a24) · 2.4.3: — · Area: LFG
- Layout: `{ { { guid u32*2 u64 } u8*2 u32*3 u32 loop[ u32 ] loop[ guid ] u8 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 8 · 8; byte 36: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `DFUpdateStatus` — differs

### SMSG_LFG_INSTANCE_SHUTDOWN_COUNTDOWN (0x2a25)

- Modern: 10789 (0x2a25) · 2.4.3: — · Area: LFG
- Layout: `{ { guid u32*2 u64 } u32 }`
- Size: 20 bytes (packed GUIDs not counted)

### SMSG_LFG_LIST_UPDATE_STATUS (0x2a26)

- Modern: 10790 (0x2a26) · 2.4.3: — · Area: LFG
- Layout: `{ { guid u32*2 u64 } u64 u8 { loop[ u32 ] u32*2 u8*4 bytes bytes bytes opt[ u32 ] } u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 47: 8 · 9 · 8 · 1 · 1 · 1 · 1 · (3 unused) · 1 · (7 unused)

### SMSG_LFG_LIST_UPDATE_EXPIRATION (0x2a27)

- Modern: 10791 (0x2a27) · 2.4.3: — · Area: LFG
- Layout: `{ { guid u32*2 u64 } u64 u8 }`
- Size: 25 bytes (packed GUIDs not counted)

### SMSG_LFG_LIST_UPDATE_BLACKLIST (0x2a2a)

- Modern: 10794 (0x2a2a) · 2.4.3: — · Area: LFG
- Layout: `{ { u32 loop[ u32*2 ] } }`
- HermesProxy: `LFGListUpdateBlacklist` — matches

### SMSG_LFG_LIST_SEARCH_RESULTS_UPDATE (0x2a2c)

- Modern: 10796 (0x2a2c) · 2.4.3: — · Area: LFG
- Layout: `{ u32 loop[ { guid u32*2 u64 u32*2 u8*2 { loop[ u32 ] u32*2 u8*4 bytes bytes bytes opt[ u32 ] } opt[ guid ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ guid ] opt[ guid ] opt[ guid ] opt[ guid ] loop[ guid u8*3 ] } ] }`

### SMSG_LFG_PROPOSAL_UPDATE (0x2a2d)

- Modern: 10797 (0x2a2d) · 2.4.3: — · Area: LFG
- Layout: `{ { { guid u32*2 u64 } u64 u32*2 u8 u32*3 u8*2 loop[ u32 u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 47: 8 · 1 · 1 · 1 · (5 unused)
- HermesProxy: `DFProposalUpdate` — differs

### SMSG_SET_DF_FAST_LAUNCH_RESULT (0x2a2e)

- Modern: 10798 (0x2a2e) · 2.4.3: — · Area: LFG
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_LFG_SLOT_INVALID (0x2a30)

- Modern: 10800 (0x2a30) · 2.4.3: — · Area: LFG
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_OPEN_LFG_DUNGEON_FINDER (0x2a31)

- Modern: 10801 (0x2a31) · 2.4.3: — · Area: LFG
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LFG_TELEPORT_DENIED (0x2a32)

- Modern: 10802 (0x2a32) · 2.4.3: — · Area: LFG
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)
- HermesProxy: `LFGTeleportDenied` — matches

### SMSG_LFG_DISABLED (0x2a33)

- Modern: 10803 (0x2a33) · 2.4.3: 920 (0x398) · Area: LFG
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `LFGDisabled` — matches

### SMSG_LFG_OFFER_CONTINUE (0x2a34)

- Modern: 10804 (0x2a34) · 2.4.3: — · Area: LFG
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `LFGOfferContinue` — matches

### SMSG_LFG_BOOT_PLAYER (0x2a35)

- Modern: 10805 (0x2a35) · 2.4.3: — · Area: LFG
- Layout: `{ { u8*2 guid u32*4 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 8 · (4 unused)

### SMSG_LFG_PARTY_INFO (0x2a36)

- Modern: 10806 (0x2a36) · 2.4.3: — · Area: LFG
- Layout: `{ u32 loop[ { u8 u32 opt[ guid ] loop[ u32*5 ] } ] }`
- HermesProxy: `LFGPartyInfo` — matches

### SMSG_LFG_PLAYER_INFO (0x2a37)

- Modern: 10807 (0x2a37) · 2.4.3: — · Area: LFG
- Layout: `{ u32 alt[ ] { u8 u32 opt[ guid ] loop[ u32*5 ] } loop[ { u32*16 u8 { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } loop[ { u32*4 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `LFGPlayerInfoPkt` — differs

### SMSG_LFG_PLAYER_REWARD (0x2a38)

- Modern: 10808 (0x2a38) · 2.4.3: — · Area: LFG
- Layout: `{ { u32*5 alt[ ] loop[ { u8 opt[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*2 opt[ u32 ] } ] } }`
- HermesProxy: `LFGPlayerReward` — differs

### SMSG_ROLE_CHOSEN (0x2a39)

- Modern: 10809 (0x2a39) · 2.4.3: — · Area: LFG
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `RoleChosen` — differs

### SMSG_LFG_READY_CHECK_RESULT (0x2a3a)

- Modern: 10810 (0x2a3a) · 2.4.3: — · Area: LFG
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)
