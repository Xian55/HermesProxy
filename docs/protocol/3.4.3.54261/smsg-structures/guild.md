# Guild — server packet layouts, 3.4.3.54261

### — (0x29b8)

- Modern: 10680 (0x29b8) · 3.3.5a: — · Area: Guild
- Layout: `u32 loop[ u32 { u32 } guid u32*2 ]`

### SMSG_GUILD_SEND_RANK_CHANGE (0x29b9)

- Modern: 10681 (0x29b9) · 3.3.5a: — · Area: Guild
- Layout: `guid guid u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `GuildSendRankChange` — matches

### SMSG_GUILD_COMMAND_RESULT (0x29ba)

- Modern: 10682 (0x29ba) · 3.3.5a: 147 (0x93) · Area: Guild
- Layout: `u32*2 u8 bytes`
- HermesProxy: `GuildCommandResult` — matches

### SMSG_GUILD_ROSTER (0x29bb)

- Modern: 10683 (0x29bb) · 3.3.5a: 138 (0x8a) · Area: Guild
- Layout: `{ u32 { u32 } u32*2 u8*3 loop[ { guid u32*5 loop[ u32*3 ] u32 u8*4 u64 u8*4 bytes bytes bytes } ] bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 11 · 11 · (2 unused)
- HermesProxy: `GuildRoster` — matches

### — (0x29bc)

- Modern: 10684 (0x29bc) · 3.3.5a: — · Area: Guild
- Layout: `u32 loop[ { guid u32*5 loop[ u32*3 ] u32 u8*4 u64 u8*4 bytes bytes bytes } ]`

### — (0x29bd)

- Modern: 10685 (0x29bd) · 3.3.5a: — · Area: Guild
- Layout: `guid u32*3 u8 opt[ guid ] opt[ u8 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · 1 · 1 · (5 unused)

### — (0x29be)

- Modern: 10686 (0x29be) · 3.3.5a: — · Area: Guild
- Layout: `guid u32*3 loop[ u8 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### — (0x29bf)

- Modern: 10687 (0x29bf) · 3.3.5a: — · Area: Guild
- Layout: `u32 loop[ u32 loop[ u8 ] ]`

### — (0x29c0)

- Modern: 10688 (0x29c0) · 3.3.5a: — · Area: Guild
- Layout: `u32*3 loop[ guid ]`

### — (0x29c1)

- Modern: 10689 (0x29c1) · 3.3.5a: — · Area: Guild
- Layout: `{ u64 u32 alt[ ] loop[ u32*3 u64 u32*2 u64 loop[ u32 ] ] }`

### — (0x29c2)

- Modern: 10690 (0x29c2) · 3.3.5a: — · Area: Guild
- Layout: `{ u32 alt[ ] loop[ u32 { u32 } u32*2 loop[ u32 ] guid u32 loop[ guid ] u8 opt[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] ] }`

### — (0x29c3)

- Modern: 10691 (0x29c3) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x29c4)

- Modern: 10692 (0x29c4) · 3.3.5a: — · Area: Guild
- Layout: `u32 loop[ u32 u64*4 guid u32*2 ]`

### — (0x29c5)

- Modern: 10693 (0x29c5) · 3.3.5a: — · Area: Guild
- Layout: `guid u32 { u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x29c6)

- Modern: 10694 (0x29c6) · 3.3.5a: — · Area: Guild
- Layout: `guid u32 { u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x29c7)

- Modern: 10695 (0x29c7) · 3.3.5a: — · Area: Guild
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x29c8)

- Modern: 10696 (0x29c8) · 3.3.5a: — · Area: Guild
- Layout: `guid u32*2 loop[ guid ]`

### SMSG_GUILD_RANKS (0x29c9)

- Modern: 10697 (0x29c9) · 3.3.5a: — · Area: Guild
- Layout: `u32 loop[ u8 u32*3 loop[ u32*2 ] u8 bytes ]`
- HermesProxy: `GuildRanks` — matches

### — (0x29ca)

- Modern: 10698 (0x29ca) · 3.3.5a: — · Area: Guild
- Layout: `guid u8*2 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 1 · (7 unused)

### SMSG_GUILD_INVITE (0x29cb)

- Modern: 10699 (0x29cb) · 3.3.5a: 131 (0x83) · Area: Guild
- Layout: `{ u8*3 u32*2 guid u32 guid u32*6 bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 7 · 7 · (4 unused)
- HermesProxy: `GuildInvite` — matches

### — (0x29cc)

- Modern: 10700 (0x29cc) · 3.3.5a: — · Area: Guild
- Layout: `u8 u32*3`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x29cd)

- Modern: 10701 (0x29cd) · 3.3.5a: — · Area: Guild
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x29ce)

- Modern: 10702 (0x29ce) · 3.3.5a: — · Area: Guild
- Layout: `u8 opt[ u8*2 u32*4 u64 bytes ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x29cf)

- Modern: 10703 (0x29cf) · 3.3.5a: — · Area: Guild
- Layout: `u32 loop[ { u8*3 guid u32*12 u8*2 bytes bytes } ]`

### — (0x29d0)

- Modern: 10704 (0x29d0) · 3.3.5a: — · Area: Guild
- Layout: `{ u32 u64 loop[ guid u32*9 u8*2 bytes bytes ] }`

### — (0x29d1)

- Modern: 10705 (0x29d1) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (5 bytes): +4: u8

### — (0x29d2)

- Modern: 10706 (0x29d2) · 3.3.5a: — · Area: Guild
- Layout: `{ u32*2 loop[ guid u32*6 u8*3 bytes bytes ] }`

### — (0x29d3)

- Modern: 10707 (0x29d3) · 3.3.5a: — · Area: Guild
- Layout: `{ opt[ struct ] u32*24 }`

### — (0x29d4)

- Modern: 10708 (0x29d4) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (16 bytes): +0: u32, +4: u32, +12: u32

### — (0x29d5)

- Modern: 10709 (0x29d5) · 3.3.5a: — · Area: Guild
- Layout: `guid u32 bits(6) { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused); byte 19: 1 · (7 unused) · 6 · (2 unused)

### — (0x29d6)

- Modern: 10710 (0x29d6) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x29d7)

- Modern: 10711 (0x29d7) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x29d9)

- Modern: 10713 (0x29d9) · 3.3.5a: — · Area: Guild
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x29da)

- Modern: 10714 (0x29da) · 3.3.5a: — · Area: Guild
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x29db)

- Modern: 10715 (0x29db) · 3.3.5a: — · Area: Guild
- Layout: `guid u8 bytes`

### — (0x29dc)

- Modern: 10716 (0x29dc) · 3.3.5a: — · Area: Guild
- Layout: `guid bits(7) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 7 · (1 unused)

### — (0x29dd)

- Modern: 10717 (0x29dd) · 3.3.5a: — · Area: Guild
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x29de)

- Modern: 10718 (0x29de) · 3.3.5a: — · Area: Guild
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GUILD_BANK_QUERY_RESULTS (0x29df)

- Modern: 10719 (0x29df) · 3.3.5a: 1000 (0x3e8) · Area: Guild
- Layout: `{ u64 u32*3 u32 alt[ ] u8 loop[ u32 u8*2 bytes bytes ] loop[ u32*6 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u8 loop[ u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)
- HermesProxy: `GuildBankQueryResults` — matches

### SMSG_GUILD_BANK_LOG_QUERY_RESULTS (0x29e0)

- Modern: 10720 (0x29e0) · 3.3.5a: — · Area: Guild
- Layout: `{ u32*2 u8 loop[ guid u32 u8*2 opt[ u64 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] ] opt[ u64 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `GuildBankLogQueryResults` — matches

### SMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY (0x29e1)

- Modern: 10721 (0x29e1) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: 8 bytes
- HermesProxy: `GuildBankRemainingWithdrawMoney` — matches

### SMSG_GUILD_PERMISSIONS_QUERY_RESULTS (0x29e2)

- Modern: 10722 (0x29e2) · 3.3.5a: — · Area: Guild
- Layout: `u32*5 loop[ u32*2 ]`
- HermesProxy: `GuildPermissionsQueryResults` — matches

### SMSG_GUILD_EVENT_LOG_QUERY_RESULTS (0x29e3)

- Modern: 10723 (0x29e3) · 3.3.5a: — · Area: Guild
- Layout: `{ u32 alt[ ] loop[ guid guid u8*2 u32 ] }`

### SMSG_GUILD_BANK_TEXT_QUERY_RESULT (0x29e4)

- Modern: 10724 (0x29e4) · 3.3.5a: — · Area: Guild
- Layout: `u32 u8 bits(6) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 13 · (2 unused)
- HermesProxy: `GuildBankTextQueryResult` — differs

### — (0x29e5)

- Modern: 10725 (0x29e5) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_QUERY_GUILD_INFO_RESPONSE (0x29e6)

- Modern: 10726 (0x29e6) · 3.3.5a: 85 (0x55) · Area: Guild
- Layout: `{ guid u8 opt[ guid u32*2 u32*5 u8 loop[ u32*2 u8 bytes ] bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)
- HermesProxy: `QueryGuildInfoResponse` — matches

### SMSG_GUILD_INVITE_DECLINED (0x29e9)

- Modern: 10729 (0x29e9) · 3.3.5a: 134 (0x86) · Area: Guild
- Layout: `bits(6) opt[ u8 ] u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 1 · (1 unused)
- HermesProxy: `GuildInviteDeclined` — matches

### — (0x29ea)

- Modern: 10730 (0x29ea) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_PLAYER_JOINED (0x29eb)

- Modern: 10731 (0x29eb) · 3.3.5a: — · Area: Guild
- Layout: `guid u32 u8 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 6 · (2 unused)
- HermesProxy: `GuildEventPlayerJoined` — matches

### SMSG_GUILD_EVENT_PLAYER_LEFT (0x29ec)

- Modern: 10732 (0x29ec) · 3.3.5a: — · Area: Guild
- Layout: `u8 bits(6) opt[ bits(6) guid u32 bytes ] guid u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)
- HermesProxy: `GuildEventPlayerLeft` — matches

### SMSG_GUILD_EVENT_NEW_LEADER (0x29ed)

- Modern: 10733 (0x29ed) · 3.3.5a: — · Area: Guild
- Layout: `u8 bits(6) bits(6) guid u32 guid u32 bytes bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · 6 · (3 unused)
- HermesProxy: `GuildEventNewLeader` — matches

### SMSG_GUILD_EVENT_DISBANDED (0x29ee)

- Modern: 10734 (0x29ee) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GuildEventDisbanded` — matches

### SMSG_GUILD_EVENT_MOTD (0x29ef)

- Modern: 10735 (0x29ef) · 3.3.5a: — · Area: Guild
- Layout: `bits(11) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 11 · (5 unused)
- HermesProxy: `GuildEventMotd` — matches

### SMSG_GUILD_EVENT_PRESENCE_CHANGE (0x29f0)

- Modern: 10736 (0x29f0) · 3.3.5a: — · Area: Guild
- Layout: `guid u32 bits(6) opt[ u8 ] opt[ u8 ] bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 6 · 1 · 1
- HermesProxy: `GuildEventPresenceChange` — matches

### SMSG_GUILD_EVENT_RANKS_UPDATED (0x29f1)

- Modern: 10737 (0x29f1) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GuildEventRanksUpdated` — matches

### — (0x29f2)

- Modern: 10738 (0x29f2) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GUILD_EVENT_TAB_ADDED (0x29f3)

- Modern: 10739 (0x29f3) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GuildEventTabAdded` — matches

### — (0x29f4)

- Modern: 10740 (0x29f4) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_TAB_MODIFIED (0x29f5)

- Modern: 10741 (0x29f5) · 3.3.5a: — · Area: Guild
- Layout: `u32 u8*2 bytes bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 7 · 9
- HermesProxy: `GuildEventTabModified` — matches

### SMSG_GUILD_EVENT_TAB_TEXT_CHANGED (0x29f6)

- Modern: 10742 (0x29f6) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `GuildEventTabTextChanged` — matches

### SMSG_GUILD_EVENT_BANK_MONEY_CHANGED (0x29f7)

- Modern: 10743 (0x29f7) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: 8 bytes
- HermesProxy: `GuildEventBankMoneyChanged` — matches

### — (0x29f8)

- Modern: 10744 (0x29f8) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_SAVE_GUILD_EMBLEM (0x29f9)

- Modern: 10745 (0x29f9) · 3.3.5a: — · Area: Guild
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `PlayerSaveGuildEmblem` — matches

### SMSG_PETITION_RENAME_GUILD_RESPONSE (0x29fa)

- Modern: 10746 (0x29fa) · 3.3.5a: — · Area: Guild
- Layout: `guid bits(7) bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 7 · (1 unused)
- HermesProxy: `PetitionRenameGuildResponse` — matches
