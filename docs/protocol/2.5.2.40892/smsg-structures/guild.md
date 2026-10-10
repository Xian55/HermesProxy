# Guild — server packet layouts, 2.5.2.40892

### SMSG_ALL_GUILD_ACHIEVEMENTS (0x29b8)

- Modern: 10680 (0x29b8) · 2.4.3: — · Area: Guild
- Layout: `{ u32 loop[ { u32 { u32 } guid u32*2 } ] }`

### SMSG_GUILD_SEND_RANK_CHANGE (0x29b9)

- Modern: 10681 (0x29b9) · 2.4.3: — · Area: Guild
- Layout: `{ guid guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `GuildSendRankChange` — matches

### SMSG_GUILD_COMMAND_RESULT (0x29ba)

- Modern: 10682 (0x29ba) · 2.4.3: 147 (0x93) · Area: Guild
- Layout: `{ u32*2 u8 bytes }`
- HermesProxy: `GuildCommandResult` — matches

### SMSG_GUILD_ROSTER (0x29bb)

- Modern: 10683 (0x29bb) · 2.4.3: 138 (0x8a) · Area: Guild
- Layout: `{ u32 { u32 } u32*2 u8*3 loop[ { guid u32*5 loop[ u32*3 ] u32 u8*7 bytes bytes bytes } ] bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 16: 11 · 11 · (2 unused)
- HermesProxy: `GuildRoster` — matches

### SMSG_GUILD_ROSTER_UPDATE (0x29bc)

- Modern: 10684 (0x29bc) · 2.4.3: — · Area: Guild
- Layout: `{ u32 loop[ { guid u32*5 loop[ u32*3 ] u32 u8*7 bytes bytes bytes } ] }`

### SMSG_GUILD_MEMBER_RECIPES (0x29bd)

- Modern: 10685 (0x29bd) · 2.4.3: — · Area: Guild
- Layout: `{ guid u32*3 loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### SMSG_GUILD_KNOWN_RECIPES (0x29be)

- Modern: 10686 (0x29be) · 2.4.3: — · Area: Guild
- Layout: `{ u32 loop[ u32 loop[ u8 ] ] }`

### SMSG_GUILD_MEMBERS_WITH_RECIPE (0x29bf)

- Modern: 10687 (0x29bf) · 2.4.3: — · Area: Guild
- Layout: `{ u32*3 loop[ guid ] }`

### SMSG_GUILD_REWARD_LIST (0x29c0)

- Modern: 10688 (0x29c0) · 2.4.3: — · Area: Guild
- Layout: `{ { u64 u32 alt[ ] loop[ u32*3 u64 u32*2 u64 loop[ u32 ] ] } }`

### SMSG_GUILD_NEWS (0x29c1)

- Modern: 10689 (0x29c1) · 2.4.3: — · Area: Guild
- Layout: `{ { u32 alt[ ] loop[ u32 { u32 } u32*2 loop[ u32 ] guid u32 loop[ guid ] u8 opt[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] ] } }`

### SMSG_GUILD_NEWS_DELETED (0x29c2)

- Modern: 10690 (0x29c2) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GUILD_CRITERIA_UPDATE (0x29c3)

- Modern: 10691 (0x29c3) · 2.4.3: — · Area: Guild
- Layout: `{ u32 loop[ u32 u64*4 guid u32 ] }`

### SMSG_GUILD_ACHIEVEMENT_EARNED (0x29c4)

- Modern: 10692 (0x29c4) · 2.4.3: — · Area: Guild
- Layout: `{ guid u32 { u32 } }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_GUILD_ACHIEVEMENT_DELETED (0x29c5)

- Modern: 10693 (0x29c5) · 2.4.3: — · Area: Guild
- Layout: `{ guid u32 { u32 } }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_GUILD_CRITERIA_DELETED (0x29c6)

- Modern: 10694 (0x29c6) · 2.4.3: — · Area: Guild
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_GUILD_ACHIEVEMENT_MEMBERS (0x29c7)

- Modern: 10695 (0x29c7) · 2.4.3: — · Area: Guild
- Layout: `{ guid u32*2 loop[ guid ] }`

### SMSG_GUILD_RANKS (0x29c8)

- Modern: 10696 (0x29c8) · 2.4.3: — · Area: Guild
- Layout: `{ u32 loop[ { u8 u32*3 loop[ u32*2 ] u8 bytes } ] }`
- HermesProxy: `GuildRanks` — matches

### SMSG_GUILD_MEMBER_UPDATE_NOTE (0x29c9)

- Modern: 10697 (0x29c9) · 2.4.3: — · Area: Guild
- Layout: `{ guid u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 1 · (7 unused)

### SMSG_GUILD_INVITE (0x29ca)

- Modern: 10698 (0x29ca) · 2.4.3: 131 (0x83) · Area: Guild
- Layout: `{ { u8*3 u32*2 guid u32 guid u32*6 bytes bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 7 · 7 · (4 unused)
- HermesProxy: `GuildInvite` — matches

### SMSG_GUILD_PARTY_STATE (0x29cb)

- Modern: 10699 (0x29cb) · 2.4.3: — · Area: Guild
- Layout: `{ u8 u32*3 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GUILD_REPUTATION_REACTION_CHANGED (0x29cc)

- Modern: 10700 (0x29cc) · 2.4.3: — · Area: Guild
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x29cd)

- Modern: 10701 (0x29cd) · 2.4.3: — · Area: Guild
- Layout: `{ u8 opt[ u8*2 u32*4 u64 bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x29ce)

- Modern: 10702 (0x29ce) · 2.4.3: — · Area: Guild
- Layout: `{ u32 loop[ { u8*3 guid u32*12 u8*2 bytes bytes } ] }`

### — (0x29cf)

- Modern: 10703 (0x29cf) · 2.4.3: — · Area: Guild
- Layout: `{ { u32 u64 loop[ guid u32*9 u8*2 bytes bytes ] } }`

### — (0x29d0)

- Modern: 10704 (0x29d0) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (5 bytes): +4: u8

### — (0x29d1)

- Modern: 10705 (0x29d1) · 2.4.3: — · Area: Guild
- Layout: `{ { u32*2 loop[ guid u32*6 u8*3 bytes bytes ] } }`

### SMSG_GUILD_CHALLENGE_UPDATE (0x29d2)

- Modern: 10706 (0x29d2) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (96 bytes): +0: u32, +4: u32, +8: u32, +12: u32, +16: u32, +20: u32, +24: u32, +28: u32, +32: u32, +36: u32, +40: u32, +44: u32, +48: u32, +52: u32, +56: u32, +60: u32, +64: u32, +68: u32, +72: u32, +76: u32, +80: u32, +84: u32, +88: u32, +92: u32

### SMSG_GUILD_CHALLENGE_COMPLETED (0x29d3)

- Modern: 10707 (0x29d3) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (16 bytes): +0: u32, +4: u32, +12: u32

### SMSG_GUILD_ITEM_LOOTED_NOTIFY (0x29d4)

- Modern: 10708 (0x29d4) · 2.4.3: — · Area: Guild
- Layout: `{ guid u32 u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 6 · (2 unused); byte 19: 1 · (7 unused) · 6 · (2 unused)

### — (0x29d5)

- Modern: 10709 (0x29d5) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x29d6)

- Modern: 10710 (0x29d6) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_RESET (0x29d8)

- Modern: 10712 (0x29d8) · 2.4.3: — · Area: Guild
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_GUILD_MOVE_STARTING (0x29d9)

- Modern: 10713 (0x29d9) · 2.4.3: — · Area: Guild
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_GUILD_MOVED (0x29da)

- Modern: 10714 (0x29da) · 2.4.3: — · Area: Guild
- Layout: `{ guid u8 bytes }`

### SMSG_GUILD_NAME_CHANGED (0x29db)

- Modern: 10715 (0x29db) · 2.4.3: — · Area: Guild
- Layout: `{ guid u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 7 · (1 unused)

### SMSG_GUILD_FLAGGED_FOR_RENAME (0x29dc)

- Modern: 10716 (0x29dc) · 2.4.3: — · Area: Guild
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GUILD_CHANGE_NAME_RESULT (0x29dd)

- Modern: 10717 (0x29dd) · 2.4.3: — · Area: Guild
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GUILD_BANK_QUERY_RESULTS (0x29de)

- Modern: 10718 (0x29de) · 2.4.3: 999 (0x3e7) · Area: Guild
- Layout: `{ { u64 u32*3 u32 alt[ ] u8 loop[ u32 u8*2 bytes bytes ] loop[ u32*6 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u8 loop[ { u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } } ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · (7 unused)
- HermesProxy: `GuildBankQueryResults` — matches

### SMSG_GUILD_BANK_LOG_QUERY_RESULTS (0x29df)

- Modern: 10719 (0x29df) · 2.4.3: — · Area: Guild
- Layout: `{ { u32*2 u8 loop[ guid u32 u8*2 opt[ u64 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] ] opt[ u64 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)
- HermesProxy: `GuildBankLogQueryResults` — matches

### SMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY (0x29e0)

- Modern: 10720 (0x29e0) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes
- HermesProxy: `GuildBankRemainingWithdrawMoney` — matches

### SMSG_GUILD_PERMISSIONS_QUERY_RESULTS (0x29e1)

- Modern: 10721 (0x29e1) · 2.4.3: — · Area: Guild
- Layout: `{ u32*5 loop[ u32*2 ] }`
- HermesProxy: `GuildPermissionsQueryResults` — matches

### SMSG_GUILD_EVENT_LOG_QUERY_RESULTS (0x29e2)

- Modern: 10722 (0x29e2) · 2.4.3: — · Area: Guild
- Layout: `{ u32 loop[ guid guid u8*2 u32 ] }`

### SMSG_GUILD_BANK_TEXT_QUERY_RESULT (0x29e3)

- Modern: 10723 (0x29e3) · 2.4.3: — · Area: Guild
- Layout: `{ u32 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 14 · (2 unused)
- HermesProxy: `GuildBankTextQueryResult` — matches

### SMSG_GUILD_MEMBER_DAILY_RESET (0x29e4)

- Modern: 10724 (0x29e4) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_QUERY_GUILD_INFO_RESPONSE (0x29e5)

- Modern: 10725 (0x29e5) · 2.4.3: 85 (0x55) · Area: Guild
- Layout: `{ { guid guid u8 opt[ guid u32*2 u32*5 u8 loop[ u32*2 u8 bytes ] bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryGuildInfoResponse` — matches

### SMSG_GUILD_INVITE_DECLINED (0x29e8)

- Modern: 10728 (0x29e8) · 2.4.3: 134 (0x86) · Area: Guild
- Layout: `{ u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 1 · (1 unused)
- HermesProxy: `GuildInviteDeclined` — matches

### SMSG_GUILD_INVITE_EXPIRED (0x29e9)

- Modern: 10729 (0x29e9) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_PLAYER_JOINED (0x29ea)

- Modern: 10730 (0x29ea) · 2.4.3: — · Area: Guild
- Layout: `{ guid u32 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 6 · (2 unused)
- HermesProxy: `GuildEventPlayerJoined` — matches

### SMSG_GUILD_EVENT_PLAYER_LEFT (0x29eb)

- Modern: 10731 (0x29eb) · 2.4.3: — · Area: Guild
- Layout: `{ u8 opt[ u8 ] opt[ guid u32 bytes ] guid u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)
- HermesProxy: `GuildEventPlayerLeft` — matches

### SMSG_GUILD_EVENT_NEW_LEADER (0x29ec)

- Modern: 10732 (0x29ec) · 2.4.3: — · Area: Guild
- Layout: `{ u8*2 guid u32 guid u32 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · 6 · (3 unused)
- HermesProxy: `GuildEventNewLeader` — matches

### SMSG_GUILD_EVENT_DISBANDED (0x29ed)

- Modern: 10733 (0x29ed) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GuildEventDisbanded` — matches

### SMSG_GUILD_EVENT_MOTD (0x29ee)

- Modern: 10734 (0x29ee) · 2.4.3: — · Area: Guild
- Layout: `{ u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 11 · (5 unused)
- HermesProxy: `GuildEventMotd` — matches

### SMSG_GUILD_EVENT_PRESENCE_CHANGE (0x29ef)

- Modern: 10735 (0x29ef) · 2.4.3: — · Area: Guild
- Layout: `{ guid u32 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 6 · 1 · 1
- HermesProxy: `GuildEventPresenceChange` — matches

### SMSG_GUILD_EVENT_RANKS_UPDATED (0x29f1)

- Modern: 10737 (0x29f1) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GuildEventRanksUpdated` — matches

### SMSG_GUILD_EVENT_RANK_CHANGED (0x29f2)

- Modern: 10738 (0x29f2) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GUILD_EVENT_TAB_ADDED (0x29f3)

- Modern: 10739 (0x29f3) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GuildEventTabAdded` — matches

### SMSG_GUILD_EVENT_TAB_DELETED (0x29f4)

- Modern: 10740 (0x29f4) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_GUILD_EVENT_TAB_MODIFIED (0x29f5)

- Modern: 10741 (0x29f5) · 2.4.3: — · Area: Guild
- Layout: `{ u32 u8*2 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 7 · 9
- HermesProxy: `GuildEventTabModified` — matches

### SMSG_GUILD_EVENT_TAB_TEXT_CHANGED (0x29f6)

- Modern: 10742 (0x29f6) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `GuildEventTabTextChanged` — matches

### SMSG_GUILD_EVENT_BANK_MONEY_CHANGED (0x29f7)

- Modern: 10743 (0x29f7) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: 8 bytes
- HermesProxy: `GuildEventBankMoneyChanged` — matches

### SMSG_GUILD_EVENT_BANK_CONTENTS_CHANGED (0x29f8)

- Modern: 10744 (0x29f8) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_SAVE_GUILD_EMBLEM (0x29f9)

- Modern: 10745 (0x29f9) · 2.4.3: — · Area: Guild
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `PlayerSaveGuildEmblem` — matches

### SMSG_PETITION_RENAME_GUILD_RESPONSE (0x29fa)

- Modern: 10746 (0x29fa) · 2.4.3: — · Area: Guild
- Layout: `{ guid u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 7 · (1 unused)
- HermesProxy: `PetitionRenameGuildResponse` — matches
