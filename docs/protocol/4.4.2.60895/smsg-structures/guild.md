# Guild — server packet layouts, 4.4.2.60895

### SMSG_ALL_GUILD_ACHIEVEMENTS (0x470000)

- Modern: 4653056 (0x470000) · 4.3.4: 21687 (0x54b7) · Area: Guild
- Layout: `u32 loop[ { u32 { u32 } guid u32*2 } ]`

### SMSG_GUILD_SEND_RANK_CHANGE (0x470001)

- Modern: 4653057 (0x470001) · 4.3.4: 23968 (0x5da0) · Area: Guild
- Layout: `guid guid u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_GUILD_COMMAND_RESULT (0x470002)

- Modern: 4653058 (0x470002) · 4.3.4: 32179 (0x7db3) · Area: Guild
- Layout: `u32*2 u8 bytes`

### SMSG_GUILD_ROSTER (0x470003)

- Modern: 4653059 (0x470003) · 4.3.4: 15779 (0x3da3) · Area: Guild
- Layout: `{ u32 { u32 } u32*2 u8*3 loop[ { guid u32*5 loop[ u32*3 ] u32 u8*4 u64 u8 bits(6) u8*2 opt[ u8 ] { u32*3 loop[ u32*4 u8 ] } bytes bytes bytes } ] bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 11 · 11 · (2 unused)

### SMSG_GUILD_HARDCORE_MEMBER_DEATH (0x470004)

- Modern: 4653060 (0x470004) · 4.3.4: — · Area: Guild
- Layout: `guid u32*3 u8 opt[ guid ] opt[ u8 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · 1 · 1 · (5 unused)

### SMSG_GUILD_MEMBER_RECIPES (0x470005)

- Modern: 4653061 (0x470005) · 4.3.4: 7344 (0x1cb0) · Area: Guild
- Layout: `guid u32*3 loop[ u8 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### SMSG_GUILD_KNOWN_RECIPES (0x470006)

- Modern: 4653062 (0x470006) · 4.3.4: 4275 (0x10b3) · Area: Guild
- Layout: `u32 loop[ u32 loop[ u8 ] ]`

### SMSG_GUILD_MEMBERS_WITH_RECIPE (0x470007)

- Modern: 4653063 (0x470007) · 4.3.4: — · Area: Guild
- Layout: `u32*3 loop[ guid ]`

### SMSG_GUILD_REWARD_LIST (0x470008)

- Modern: 4653064 (0x470008) · 4.3.4: 7600 (0x1db0) · Area: Guild
- Layout: `u64 u32 loop[ u32*3 u64 u32*2 u64 loop[ u32 ] ]`

### SMSG_GUILD_NEWS (0x470009)

- Modern: 4653065 (0x470009) · 4.3.4: — · Area: Guild
- Layout: `{ u32 loop[ u32 { u32 } u32*2 loop[ u32 ] guid u32 loop[ guid ] u8 opt[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] ] }`

### SMSG_GUILD_NEWS_DELETED (0x47000a)

- Modern: 4653066 (0x47000a) · 4.3.4: 29863 (0x74a7) · Area: Guild
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GUILD_CRITERIA_UPDATE (0x47000b)

- Modern: 4653067 (0x47000b) · 4.3.4: — · Area: Guild
- Layout: `u32 loop[ u32 u64*4 guid u32*2 ]`

### SMSG_GUILD_ACHIEVEMENT_EARNED (0x47000c)

- Modern: 4653068 (0x47000c) · 4.3.4: 20661 (0x50b5) · Area: Guild
- Layout: `guid u32 { u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_GUILD_ACHIEVEMENT_DELETED (0x47000d)

- Modern: 4653069 (0x47000d) · 4.3.4: 13728 (0x35a0) · Area: Guild
- Layout: `guid u32 { u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_GUILD_CRITERIA_DELETED (0x47000e)

- Modern: 4653070 (0x47000e) · 4.3.4: 21937 (0x55b1) · Area: Guild
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_GUILD_ACHIEVEMENT_MEMBERS (0x47000f)

- Modern: 4653071 (0x47000f) · 4.3.4: 14501 (0x38a5) · Area: Guild
- Layout: `guid u32*2 loop[ guid ]`

### SMSG_GUILD_RANKS (0x470010)

- Modern: 4653072 (0x470010) · 4.3.4: 12468 (0x30b4) · Area: Guild
- Layout: `u32 loop[ { u8 u32*3 loop[ u32*2 ] u8 bytes } ]`

### SMSG_GUILD_MEMBER_UPDATE_NOTE (0x470011)

- Modern: 4653073 (0x470011) · 4.3.4: 31904 (0x7ca0) · Area: Guild
- Layout: `guid u8*2 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 1 · (7 unused)

### SMSG_GUILD_INVITE (0x470012)

- Modern: 4653074 (0x470012) · 4.3.4: 5282 (0x14a2) · Area: Guild
- Layout: `{ u8*3 u32*2 guid u32 guid u32*6 bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 7 · 7 · (4 unused)

### SMSG_GUILD_PARTY_STATE (0x470013)

- Modern: 4653075 (0x470013) · 4.3.4: 20646 (0x50a6) · Area: Guild
- Layout: `u8 u32*3`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GUILD_REPUTATION_REACTION_CHANGED (0x470014)

- Modern: 4653076 (0x470014) · 4.3.4: 29872 (0x74b0) · Area: Guild
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_GUILD_CHALLENGE_UPDATE (0x47001a)

- Modern: 4653082 (0x47001a) · 4.3.4: 6321 (0x18b1) · Area: Guild
- Layout: `{ opt[ struct ] u32*24 }`

### SMSG_GUILD_CHALLENGE_COMPLETED (0x47001b)

- Modern: 4653083 (0x47001b) · 4.3.4: 14755 (0x39a3) · Area: Guild
- Layout: `struct`
- Struct fields the client uses (16 bytes): +0: u32, +4: u32, +8: u32, +12: u32

### SMSG_GUILD_ITEM_LOOTED_NOTIFY (0x47001c)

- Modern: 4653084 (0x47001c) · 4.3.4: — · Area: Guild
- Layout: `guid u32 bits(6) { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused); byte 19: 1 · (7 unused) · 6 · (2 unused)

### SMSG_GUILD_RESET (0x470020)

- Modern: 4653088 (0x470020) · 4.3.4: 7349 (0x1cb5) · Area: Guild
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_GUILD_MOVE_STARTING (0x470021)

- Modern: 4653089 (0x470021) · 4.3.4: 28836 (0x70a4) · Area: Guild
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_GUILD_MOVED (0x470022)

- Modern: 4653090 (0x470022) · 4.3.4: — · Area: Guild
- Layout: `guid u8 bytes`

### SMSG_GUILD_NAME_CHANGED (0x470023)

- Modern: 4653091 (0x470023) · 4.3.4: — · Area: Guild
- Layout: `guid bits(7) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 7 · (1 unused)

### SMSG_GUILD_FLAGGED_FOR_RENAME (0x470024)

- Modern: 4653092 (0x470024) · 4.3.4: 12470 (0x30b6) · Area: Guild
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GUILD_CHANGE_NAME_RESULT (0x470025)

- Modern: 4653093 (0x470025) · 4.3.4: 15537 (0x3cb1) · Area: Guild
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GUILD_BANK_QUERY_RESULTS (0x470026)

- Modern: 4653094 (0x470026) · 4.3.4: 30885 (0x78a5) · Area: Guild
- Layout: `{ u64 u32*4 u8 loop[ u32 u8*2 bytes bytes ] loop[ u32*6 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ { u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } } ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)

### SMSG_GUILD_BANK_LOG_QUERY_RESULTS (0x470027)

- Modern: 4653095 (0x470027) · 4.3.4: 12466 (0x30b2) · Area: Guild
- Layout: `u32*2 u8 loop[ { guid u32 u8*2 opt[ u64 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] } ] opt[ u64 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY (0x470028)

- Modern: 4653096 (0x470028) · 4.3.4: 23988 (0x5db4) · Area: Guild
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: 8 bytes

### SMSG_GUILD_PERMISSIONS_QUERY_RESULTS (0x470029)

- Modern: 4653097 (0x470029) · 4.3.4: 13475 (0x34a3) · Area: Guild
- Layout: `u32*5 loop[ u32*2 ]`

### SMSG_GUILD_EVENT_LOG_QUERY_RESULTS (0x47002a)

- Modern: 4653098 (0x47002a) · 4.3.4: 4274 (0x10b2) · Area: Guild
- Layout: `u32 loop[ guid guid u8*2 u32 ]`

### SMSG_GUILD_BANK_TEXT_QUERY_RESULT (0x47002b)

- Modern: 4653099 (0x47002b) · 4.3.4: 30115 (0x75a3) · Area: Guild
- Layout: `u32 u8 bits(6) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 13 · (2 unused)

### SMSG_GUILD_MEMBER_DAILY_RESET (0x47002c)

- Modern: 4653100 (0x47002c) · 4.3.4: 4261 (0x10a5) · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_QUERY_GUILD_INFO_RESPONSE (0x47002d)

- Modern: 4653101 (0x47002d) · 4.3.4: 3590 (0xe06) · Area: Guild
- Layout: `{ guid u8 opt[ guid u32*7 u8 loop[ u32*2 u8 bytes ] bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_GUILD_INVITE_DECLINED (0x470030)

- Modern: 4653104 (0x470030) · 4.3.4: — · Area: Guild
- Layout: `bits(6) opt[ u8 ] u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 1 · (1 unused)

### SMSG_GUILD_INVITE_EXPIRED (0x470031)

- Modern: 4653105 (0x470031) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_PLAYER_JOINED (0x470032)

- Modern: 4653106 (0x470032) · 4.3.4: — · Area: Guild
- Layout: `guid u32 bits(6) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 6 · (2 unused)

### SMSG_GUILD_EVENT_PLAYER_LEFT (0x470033)

- Modern: 4653107 (0x470033) · 4.3.4: — · Area: Guild
- Layout: `u8 bits(6) opt[ bits(6) guid u32 bytes ] guid u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)

### SMSG_GUILD_EVENT_NEW_LEADER (0x470034)

- Modern: 4653108 (0x470034) · 4.3.4: — · Area: Guild
- Layout: `u8 bits(6) bits(6) guid u32 guid u32 bytes bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · 6 · (3 unused)

### SMSG_GUILD_EVENT_DISBANDED (0x470035)

- Modern: 4653109 (0x470035) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_MOTD (0x470036)

- Modern: 4653110 (0x470036) · 4.3.4: — · Area: Guild
- Layout: `{ opt[ u8 ] alt[ u8 ] bits(3) } bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 11 · (5 unused)

### SMSG_GUILD_EVENT_PRESENCE_CHANGE (0x470037)

- Modern: 4653111 (0x470037) · 4.3.4: — · Area: Guild
- Layout: `guid u32 bits(6) opt[ u8 ] bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 6 · 1 · (1 unused)

### SMSG_GUILD_EVENT_STATUS_CHANGE (0x470038)

- Modern: 4653112 (0x470038) · 4.3.4: — · Area: Guild
- Layout: `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · (6 unused)

### SMSG_GUILD_EVENT_RANKS_UPDATED (0x470039)

- Modern: 4653113 (0x470039) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_RANK_CHANGED (0x47003a)

- Modern: 4653114 (0x47003a) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GUILD_EVENT_TAB_ADDED (0x47003b)

- Modern: 4653115 (0x47003b) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_TAB_DELETED (0x47003c)

- Modern: 4653116 (0x47003c) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_TAB_MODIFIED (0x47003d)

- Modern: 4653117 (0x47003d) · 4.3.4: — · Area: Guild
- Layout: `u32 u8*2 bytes bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 7 · 9

### SMSG_GUILD_EVENT_TAB_TEXT_CHANGED (0x47003e)

- Modern: 4653118 (0x47003e) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GUILD_EVENT_BANK_MONEY_CHANGED (0x47003f)

- Modern: 4653119 (0x47003f) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: 8 bytes

### SMSG_GUILD_EVENT_BANK_CONTENTS_CHANGED (0x470040)

- Modern: 4653120 (0x470040) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_SAVE_GUILD_EMBLEM (0x470041)

- Modern: 4653121 (0x470041) · 4.3.4: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_PETITION_RENAME_GUILD_RESPONSE (0x470042)

- Modern: 4653122 (0x470042) · 4.3.4: — · Area: Guild
- Layout: `guid bits(7) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 7 · (1 unused)
