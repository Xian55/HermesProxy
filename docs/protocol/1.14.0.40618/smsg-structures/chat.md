# Chat — server packet layouts, 1.14.0.40618

### SMSG_CHAT_IGNORED_ACCOUNT_MUTED (0x2bac)

- Modern: 11180 (0x2bac) · 1.12.1: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CHAT (0x2bad)

- Modern: 11181 (0x2bad) · 1.12.1: 150 (0x96) · Area: Chat
- Layout: `{ { u8 u32 guid guid guid guid u32*2 guid u32*2 u8*8 bytes bytes bytes bytes bytes opt[ u32 ] opt[ guid ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 31: 11 · 11 · 5 · 7 · 12 · 14; byte 38: 1 · 1 · 1 · 1
- HermesProxy: `ChatPkt` — matches

### SMSG_WHO (0x2bae)

- Modern: 11182 (0x2bae) · 1.12.1: 99 (0x63) · Area: Chat
- Layout: `{ u32 { u8 loop[ { u8 loop[ opt[ u8 ] alt[ u8 ] ] loop[ bytes ] guid guid guid u64 u32 u8*5 bytes } guid u32*2 u8 bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 6 · (2 unused)
- HermesProxy: `WhoResponsePkt` — matches

### SMSG_MOTD (0x2baf)

- Modern: 11183 (0x2baf) · 1.12.1: 829 (0x33d) · Area: Chat
- Layout: `{ u8 loop[ u8 bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)
- HermesProxy: `MOTD` — matches

### SMSG_CHAT_PLAYER_AMBIGUOUS (0x2bb0)

- Modern: 11184 (0x2bb0) · 1.12.1: 813 (0x32d) · Area: Chat
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)

### SMSG_EXPECTED_SPAM_RECORDS (0x2bb1)

- Modern: 11185 (0x2bb1) · 1.12.1: 818 (0x332) · Area: Chat
- Layout: `{ u32 loop[ u8*2 bytes ] }`

### SMSG_CHAT_NOT_IN_PARTY (0x2bb2)

- Modern: 11186 (0x2bb2) · 1.12.1: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_CHAT_RESTRICTED (0x2bb3)

- Modern: 11187 (0x2bb3) · 1.12.1: 765 (0x2fd) · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8

### SMSG_RAID_INSTANCE_MESSAGE (0x2bb4)

- Modern: 11188 (0x2bb4) · 1.12.1: 762 (0x2fa) · Area: Chat
- Layout: `{ u8 u32*2 u8 }`
- Size: 10 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 9: 1 · 1 · (6 unused)
- HermesProxy: `RaidInstanceMessage` — matches

### — (0x2bb5)

- Modern: 11189 (0x2bb5) · 1.12.1: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_DEFENSE_MESSAGE (0x2bb6)

- Modern: 11190 (0x2bb6) · 1.12.1: 827 (0x33b) · Area: Chat
- Layout: `{ u32 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 12 · (4 unused)
- HermesProxy: `DefenseMessage` — matches

### SMSG_CHAT_PLAYER_NOTFOUND (0x2bb7)

- Modern: 11191 (0x2bb7) · 1.12.1: 681 (0x2a9) · Area: Chat
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 9 · (7 unused)
- HermesProxy: `ChatPlayerNotfound` — matches

### SMSG_CHAT_AUTO_RESPONDED (0x2bb8)

- Modern: 11192 (0x2bb8) · 1.12.1: — · Area: Chat
- Layout: `{ u8*2 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 9 · (6 unused)

### SMSG_USERLIST_ADD (0x2bb9)

- Modern: 11193 (0x2bb9) · 1.12.1: — · Area: Chat
- Layout: `{ guid u8 u32*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 7 · (1 unused)

### SMSG_USERLIST_REMOVE (0x2bba)

- Modern: 11194 (0x2bba) · 1.12.1: — · Area: Chat
- Layout: `{ guid u32*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 7 · (1 unused)

### SMSG_USERLIST_UPDATE (0x2bbb)

- Modern: 11195 (0x2bbb) · 1.12.1: — · Area: Chat
- Layout: `{ guid u8 u32*2 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 7 · (1 unused)

### SMSG_BROADCAST_ACHIEVEMENT (0x2bbc)

- Modern: 11196 (0x2bbc) · 1.12.1: — · Area: Chat
- Layout: `{ u8 guid u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 1

### SMSG_CHAT_DOWN (0x2bbd)

- Modern: 11197 (0x2bbd) · 1.12.1: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CHAT_IS_DOWN (0x2bbe)

- Modern: 11198 (0x2bbe) · 1.12.1: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CHAT_RECONNECT (0x2bbf)

- Modern: 11199 (0x2bbf) · 1.12.1: — · Area: Chat
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_CHANNEL_NOTIFY (0x2bc0)

- Modern: 11200 (0x2bc0) · 1.12.1: 153 (0x99) · Area: Chat
- Layout: `{ u8*3 guid guid u32 guid u32*2 opt[ u8*2 ] bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 7 · 6 · (5 unused)
- HermesProxy: `ChannelNotify` — matches

### SMSG_CHANNEL_NOTIFY_JOINED (0x2bc1)

- Modern: 11201 (0x2bc1) · 1.12.1: — · Area: Chat
- Layout: `{ u8*3 u32*2 u64 guid bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 11
- HermesProxy: `ChannelNotifyJoined` — matches

### SMSG_CHANNEL_NOTIFY_LEFT (0x2bc2)

- Modern: 11202 (0x2bc2) · 1.12.1: — · Area: Chat
- Layout: `{ u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 7 · 1
- HermesProxy: `ChannelNotifyLeft` — matches

### SMSG_CHANNEL_LIST (0x2bc3)

- Modern: 11203 (0x2bc3) · 1.12.1: 155 (0x9b) · Area: Chat
- Layout: `{ u8 u32*2 bytes loop[ guid u32 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 7
- HermesProxy: `ChannelListResponse` — matches

### SMSG_CHAT_SERVER_MESSAGE (0x2bc4)

- Modern: 11204 (0x2bc4) · 1.12.1: 657 (0x291) · Area: Chat
- Layout: `{ u32 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 11 · (5 unused)
- HermesProxy: `ChatServerMessage` — matches
