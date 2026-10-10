# Chat — server packet layouts, 4.4.2.60895

### SMSG_CHAT_IGNORED_ACCOUNT_MUTED (0x400000)

- Modern: 4194304 (0x400000) · 4.3.4: 5540 (0x15a4) · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CHAT (0x400001)

- Modern: 4194305 (0x400001) · 4.3.4: 8230 (0x2026) · Area: Chat
- Layout: `{ { u8 u32 guid guid guid guid u32*3 u16 u32*2 u8 bits(3) u8 alt[ u8 ] alt[ u8 ] alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bytes bytes bytes bytes bytes opt[ u32 ] opt[ guid ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 35: 11 · 11 · 5 · 7 · 12 · 1 · 1 · 1 · 1 · (6 unused)

### SMSG_WHO (0x400002)

- Modern: 4194306 (0x400002) · 4.3.4: 26887 (0x6907) · Area: Chat
- Layout: `{ u32 { u8 loop[ { u8*6 bytes bytes bytes bytes bytes guid guid guid u64 u32 u8*5 u32 bytes } guid u32*2 u8 bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 6 · (2 unused)

### SMSG_CHAT_PLAYER_AMBIGUOUS (0x400004)

- Modern: 4194308 (0x400004) · 4.3.4: 12084 (0x2f34) · Area: Chat
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### SMSG_EXPECTED_SPAM_RECORDS (0x400005)

- Modern: 4194309 (0x400005) · 4.3.4: 19766 (0x4d36) · Area: Chat
- Layout: `{ u32 loop[ u8*2 bytes ] }`

### SMSG_CHAT_NOT_IN_PARTY (0x400006)

- Modern: 4194310 (0x400006) · 4.3.4: 27156 (0x6a14) · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_CHAT_RESTRICTED (0x400007)

- Modern: 4194311 (0x400007) · 4.3.4: 25910 (0x6536) · Area: Chat
- Layout: `{ u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_RAID_INSTANCE_MESSAGE (0x400008)

- Modern: 4194312 (0x400008) · 4.3.4: 28181 (0x6e15) · Area: Chat
- Layout: `{ u32*4 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 8 · 1 · 1 · (6 unused)

### SMSG_ZONE_UNDER_ATTACK (0x400009)

- Modern: 4194313 (0x400009) · 4.3.4: 2566 (0xa06) · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_DEFENSE_MESSAGE (0x40000a)

- Modern: 4194314 (0x40000a) · 4.3.4: 788 (0x314) · Area: Chat
- Layout: `{ u32 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 12 · (4 unused)

### SMSG_CHAT_PLAYER_NOTFOUND (0x40000b)

- Modern: 4194315 (0x40000b) · 4.3.4: 9510 (0x2526) · Area: Chat
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### SMSG_CHAT_AUTO_RESPONDED (0x40000c)

- Modern: 4194316 (0x40000c) · 4.3.4: — · Area: Chat
- Layout: `{ u8*2 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 11 · (4 unused)

### SMSG_USERLIST_ADD (0x40000d)

- Modern: 4194317 (0x40000d) · 4.3.4: 3895 (0xf37) · Area: Chat
- Layout: `{ guid u8 u32*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 7 · (1 unused)

### SMSG_USERLIST_REMOVE (0x40000e)

- Modern: 4194318 (0x40000e) · 4.3.4: 8198 (0x2006) · Area: Chat
- Layout: `{ guid u32*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 7 · (1 unused)

### SMSG_USERLIST_UPDATE (0x40000f)

- Modern: 4194319 (0x40000f) · 4.3.4: 309 (0x135) · Area: Chat
- Layout: `{ guid u8 u32*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 7 · (1 unused)

### SMSG_BROADCAST_ACHIEVEMENT (0x400010)

- Modern: 4194320 (0x400010) · 4.3.4: — · Area: Chat
- Layout: `{ u8 guid u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 1

### SMSG_BROADCAST_LEVELUP (0x400011)

- Modern: 4194321 (0x400011) · 4.3.4: — · Area: Chat
- Layout: `{ u8 u16 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · (2 unused)

### SMSG_CHAT_DOWN (0x400012)

- Modern: 4194322 (0x400012) · 4.3.4: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CHAT_IS_DOWN (0x400013)

- Modern: 4194323 (0x400013) · 4.3.4: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CHAT_RECONNECT (0x400014)

- Modern: 4194324 (0x400014) · 4.3.4: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CHANNEL_NOTIFY (0x400015)

- Modern: 4194325 (0x400015) · 4.3.4: 2085 (0x825) · Area: Chat
- Layout: `{ u8*3 guid guid u32 guid u32*2 opt[ u8*2 ] bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 7 · 6 · (5 unused)

### SMSG_CHANNEL_NOTIFY_NPE_JOINED_BATCH (0x400016)

- Modern: 4194326 (0x400016) · 4.3.4: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### SMSG_CHANNEL_NOTIFY_JOINED (0x400017)

- Modern: 4194327 (0x400017) · 4.3.4: — · Area: Chat
- Layout: `{ u8*3 u32 u8 u32 u64 guid bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 11

### SMSG_CHANNEL_NOTIFY_LEFT (0x400018)

- Modern: 4194328 (0x400018) · 4.3.4: — · Area: Chat
- Layout: `{ u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 1

### SMSG_CHANNEL_LIST (0x400019)

- Modern: 4194329 (0x400019) · 4.3.4: 8724 (0x2214) · Area: Chat
- Layout: `{ u8 u32*2 bytes loop[ guid u32 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 7

### SMSG_CHAT_SERVER_MESSAGE (0x40001a)

- Modern: 4194330 (0x40001a) · 4.3.4: 27652 (0x6c04) · Area: Chat
- Layout: `{ u32 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 11 · (5 unused)

### SMSG_CHAT_NOT_IN_GUILD (0x400021)

- Modern: 4194337 (0x400021) · 4.3.4: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
