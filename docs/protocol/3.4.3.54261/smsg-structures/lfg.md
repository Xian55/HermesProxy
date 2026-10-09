# LFG — server packet layouts, 3.4.3.54261

### SMSG_LFG_JOIN_RESULT (0x2a1c)

- Modern: 10780 (0x2a1c) · 3.3.5a: 868 (0x364) · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u8*2 u32*2 loop[ { u8 u32 opt[ guid ] loop[ u32*5 ] } ] loop[ u8 ] loop[ { opt[ bytes ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused) · 8 · 8
- HermesProxy: `DFJoinResult` — matches

### — (0x2a1d)

- Modern: 10781 (0x2a1d) · 3.3.5a: — · Area: LFG
- Layout: `guid u32*2 u64 u8*3`
- Size: 19 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused) · 8 · 8

### — (0x2a1e)

- Modern: 10782 (0x2a1e) · 3.3.5a: — · Area: LFG
- Layout: `u16 u32 loop[ { { guid u32*2 u64 u8 } u32 u8 guid guid guid guid guid u32*4 u32 u32 u32 u32 u64 u8 guid { loop[ u32 u8 ] } u32 loop[ guid ] loop[ guid ] loop[ guid ] { u32*3 u8 bits(2) opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] opt[ u8 ] } u8 { u32*3 loop[ u32*4 u8 ] } loop[ { guid u8*3 u32 u8*2 } ] } ]`

### — (0x2a1f)

- Modern: 10783 (0x2a1f) · 3.3.5a: — · Area: LFG
- Layout: `guid u32*2 u64 u8*3`
- Size: 19 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused) · 8 · 1 · (7 unused)

### SMSG_LFG_QUEUE_STATUS (0x2a20)

- Modern: 10784 (0x2a20) · 3.3.5a: 869 (0x365) · Area: LFG
- Layout: `{ guid u32*2 u64 u8 } u32*3 loop[ u32 u8 ] u32`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)
- HermesProxy: `DFQueueStatus` — matches

### SMSG_LFG_ROLE_CHECK_UPDATE (0x2a21)

- Modern: 10785 (0x2a21) · 3.3.5a: 867 (0x363) · Area: LFG
- Layout: `{ u8*2 u32*4 loop[ u32 ] loop[ u64 ] u8 loop[ guid u8*3 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8; byte 18: 1 · 1 · (6 unused)
- HermesProxy: `LFGRoleCheckUpdate` — matches

### — (0x2a22)

- Modern: 10786 (0x2a22) · 3.3.5a: — · Area: LFG
- Layout: `u8*2 u32*2 loop[ u64 ] u8 loop[ guid u8 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8; byte 10: 1 · (7 unused)

### SMSG_LFG_UPDATE_STATUS (0x2a24)

- Modern: 10788 (0x2a24) · 3.3.5a: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u8*2 u32 u8 u32*2 loop[ u32 ] loop[ guid ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused) · 8 · 8; byte 34: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `DFUpdateStatus` — matches

### — (0x2a25)

- Modern: 10789 (0x2a25) · 3.3.5a: — · Area: LFG
- Layout: `guid u32*2 u64 u8 u32`
- Size: 21 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### — (0x2a26)

- Modern: 10790 (0x2a26) · 3.3.5a: — · Area: LFG
- Layout: `guid u32*2 u64 u8 u64 u8 { u32*3 u8 bits(2) opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] opt[ u8 ] } u8`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 40: 10 · 11 · 8 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (1 unused) · 1 · (7 unused)

### — (0x2a27)

- Modern: 10791 (0x2a27) · 3.3.5a: — · Area: LFG
- Layout: `guid u32*2 u64 u8 u64 u8`
- Size: 26 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### — (0x2a28)

- Modern: 10792 (0x2a28) · 3.3.5a: — · Area: LFG
- Layout: `guid u32*2 u64 u8 u64 u8*2 { guid u32*2 u64 u8 } u8`
- Size: 45 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 27: 8 · 8; byte 47: 1 · (7 unused) · 4 · (4 unused)

### — (0x2a29)

- Modern: 10793 (0x2a29) · 3.3.5a: — · Area: LFG
- Layout: `{ guid u32*2 u64 u8 } u64 u8*2 { guid u32*2 u64 u8 } u8 { { guid u32*2 u64 u8 } u32 u8 guid guid guid guid guid u32*4 u32 u32 u32 u32 u64 u8 guid { loop[ u32 u8 ] } u32 loop[ guid ] loop[ guid ] loop[ guid ] { u32*3 u8 bits(2) opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] opt[ u8 ] } u8 { u32*3 loop[ u32*4 u8 ] } loop[ { guid u8*3 u32 u8*2 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 27: 8 · 8; byte 47: 1 · (7 unused) · 4 · (4 unused); byte 67: 1 · (7 unused); byte 177: 10 · 11 · 8 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (1 unused) · 1 · (7 unused)

### SMSG_LFG_LIST_UPDATE_BLACKLIST (0x2a2a)

- Modern: 10794 (0x2a2a) · 3.3.5a: — · Area: LFG
- Layout: `{ u32 loop[ u32*2 ] }`
- HermesProxy: `LFGListUpdateBlacklist` — matches

### — (0x2a2b)

- Modern: 10795 (0x2a2b) · 3.3.5a: — · Area: LFG
- Layout: `{ guid u32*2 u64 u8 } u32 u32 loop[ { guid u32*2 u64 u8 guid u32 u8*2 loop[ guid u32*3 u8*2 u32*3 { loop[ u32 u8 ] } u32*2 loop[ u32*2 ] { u32*3 loop[ u32*4 u8 ] } ] bytes } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### — (0x2a2c)

- Modern: 10796 (0x2a2c) · 3.3.5a: — · Area: LFG
- Layout: `{ u32 loop[ { { guid u32*2 u64 u8 } u32*2 u8*3 { u32*3 u8 bits(2) opt[ u8 ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] opt[ u8 ] } opt[ guid ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ guid ] opt[ guid ] opt[ guid ] opt[ guid ] loop[ { guid u8*3 u32 u8*2 } ] } ] }`

### SMSG_LFG_PROPOSAL_UPDATE (0x2a2d)

- Modern: 10797 (0x2a2d) · 3.3.5a: 865 (0x361) · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u64 u32*2 u8 u32*3 u8*2 loop[ u8*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 48: 8 · 1 · 1 · 1 · (5 unused)
- HermesProxy: `DFProposalUpdate` — matches

### — (0x2a2e)

- Modern: 10798 (0x2a2e) · 3.3.5a: — · Area: LFG
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2a30)

- Modern: 10800 (0x2a30) · 3.3.5a: — · Area: LFG
- Layout: `struct`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### — (0x2a31)

- Modern: 10801 (0x2a31) · 3.3.5a: — · Area: LFG
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_LFG_TELEPORT_DENIED (0x2a32)

- Modern: 10802 (0x2a32) · 3.3.5a: 512 (0x200) · Area: LFG
- Layout: `bits(4)`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)
- HermesProxy: `LFGTeleportDenied` — matches

### SMSG_LFG_DISABLED (0x2a33)

- Modern: 10803 (0x2a33) · 3.3.5a: 920 (0x398) · Area: LFG
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `LFGDisabled` — matches

### SMSG_LFG_OFFER_CONTINUE (0x2a34)

- Modern: 10804 (0x2a34) · 3.3.5a: 659 (0x293) · Area: LFG
- Layout: `struct`
- Struct fields the client uses (3 bytes): +0: 3 bytes
- HermesProxy: `LFGOfferContinue` — matches

### — (0x2a35)

- Modern: 10805 (0x2a35) · 3.3.5a: — · Area: LFG
- Layout: `u8*2 guid u32*4 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 8 · (4 unused)

### SMSG_LFG_PARTY_INFO (0x2a36)

- Modern: 10806 (0x2a36) · 3.3.5a: 882 (0x372) · Area: LFG
- Layout: `u32 loop[ { u8 u32 opt[ guid ] loop[ u32*5 ] } ]`
- HermesProxy: `LFGPartyInfo` — matches

### SMSG_LFG_PLAYER_INFO (0x2a37)

- Modern: 10807 (0x2a37) · 3.3.5a: 879 (0x36f) · Area: LFG
- Layout: `{ u32 alt[ ] { u8 u32 opt[ guid ] loop[ u32*5 ] } loop[ { u32*16 u8 { u8 u32*3 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } loop[ { u8 u32*3 u32 u32 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `LFGPlayerInfoPkt` — matches

### SMSG_LFG_PLAYER_REWARD (0x2a38)

- Modern: 10808 (0x2a38) · 3.3.5a: 511 (0x1ff) · Area: LFG
- Layout: `{ u32*5 loop[ u8 opt[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] u32*2 opt[ u32 ] ] }`
- HermesProxy: `LFGPlayerReward` — differs (#364)

### SMSG_ROLE_CHOSEN (0x2a39)

- Modern: 10809 (0x2a39) · 3.3.5a: — · Area: LFG
- Layout: `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 1 · (7 unused)
- HermesProxy: `RoleChosen` — matches

### — (0x2a3a)

- Modern: 10810 (0x2a3a) · 3.3.5a: — · Area: LFG
- Layout: `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### — (0x2a3b)

- Modern: 10811 (0x2a3b) · 3.3.5a: — · Area: LFG
- Layout: `guid u32*2 u64 u8`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)
