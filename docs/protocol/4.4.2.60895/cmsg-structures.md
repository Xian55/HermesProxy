# Client packet layouts (CMSG), 4.4.2.60895

Notation: see [the README](README.md).

### CMSG_BATTLE_PET_CLEAR_FANFARE (0x2e0002)

- Modern: 3014658 (0x2e0002) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_MOUNT_CLEAR_FANFARE (0x2e0003)

- Modern: 3014659 (0x2e0003) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_TOY_CLEAR_FANFARE (0x2e0004)

- Modern: 3014660 (0x2e0004) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CLEAR_NEW_APPEARANCE (0x2e0005)

- Modern: 3014661 (0x2e0005) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_REQUEST_STORE_FRONT_INFO_UPDATE (0x2e001d)

- Modern: 3014685 (0x2e001d) · 4.3.4: —
- Layout (after the opcode): `u32*3 loop[ u32 ]`

### CMSG_CHAT_JOIN_CHANNEL (0x300000)

- Modern: 3145728 (0x300000) · 4.3.4: 342 (0x156)
- Layout (after the opcode): `u32 { u32 opt[ u8 ] bits(7) bits(7) flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · 1 · 7 · 7

### CMSG_CHAT_LEAVE_CHANNEL (0x300001)

- Modern: 3145729 (0x300001) · 4.3.4: 11606 (0x2d56)
- Layout (after the opcode): `u32*2 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 7 · (1 unused)

### CMSG_CHAT_REPORT_IGNORED (0x300003)

- Modern: 3145731 (0x300003) · 4.3.4: 3412 (0xd54)
- Layout (after the opcode): `u32 guid u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_CHAT_REPORT_FILTERED (0x300004)

- Modern: 3145732 (0x300004) · 4.3.4: 2374 (0x946)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CHAT_REGISTER_ADDON_PREFIXES (0x300005)

- Modern: 3145733 (0x300005) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ bits(5) flush opt[ bytes ] ]`

### CMSG_CHAT_UNREGISTER_ALL_ADDON_PREFIXES (0x300006)

- Modern: 3145734 (0x300006) · 4.3.4: 15700 (0x3d54)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CHAT_MESSAGE_CHANNEL (0x300007)

- Modern: 3145735 (0x300007) · 4.3.4: 7492 (0x1d44)
- Layout (after the opcode): `u32 { u32 guid u8*2 bits(3) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 8: 9 · 11 · 1 · (3 unused)

### CMSG_CHAT_MESSAGE_WHISPER (0x300008)

- Modern: 3145736 (0x300008) · 4.3.4: 3414 (0xd56)
- Layout (after the opcode): `u32*2 guid u32 u8*2 bits(3) flush opt[ bytes ] opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 12: (7 unused) · 1 · 1 · (9 unused) · 1 · 1 · (4 unused)

### CMSG_CHAT_MESSAGE_GUILD (0x300009)

- Modern: 3145737 (0x300009) · 4.3.4: 14678 (0x3956)
- Layout (after the opcode): `u32*2 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · (5 unused)

### CMSG_CHAT_MESSAGE_OFFICER (0x30000a)

- Modern: 3145738 (0x30000a) · 4.3.4: 6470 (0x1946)
- Layout (after the opcode): `u32*2 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · (5 unused)

### CMSG_CHAT_MESSAGE_AFK (0x30000b)

- Modern: 3145739 (0x30000b) · 4.3.4: 3396 (0xd44)
- Layout (after the opcode): `u32 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 11 · (5 unused)

### CMSG_CHAT_MESSAGE_DND (0x30000c)

- Modern: 3145740 (0x30000c) · 4.3.4: 10566 (0x2946)
- Layout (after the opcode): `u32 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 11 · (5 unused)

### CMSG_CHAT_CHANNEL_LIST (0x30000d)

- Modern: 3145741 (0x30000d) · 4.3.4: 5462 (0x1556)
- Layout (after the opcode): `u32 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### CMSG_CHAT_CHANNEL_DISPLAY_LIST (0x30000e)

- Modern: 3145742 (0x30000e) · 4.3.4: 8516 (0x2144)
- Layout (after the opcode): `u32 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### CMSG_CHAT_CHANNEL_PASSWORD (0x30000f)

- Modern: 3145743 (0x30000f) · 4.3.4: 9558 (0x2556)
- Layout (after the opcode): `u32 { bits(7) bits(7) flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · 7 · (2 unused)

### CMSG_CHAT_CHANNEL_SET_OWNER (0x300010)

- Modern: 3145744 (0x300010) · 4.3.4: 13654 (0x3556)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_OWNER (0x300011)

- Modern: 3145745 (0x300011) · 4.3.4: 15684 (0x3d44)
- Layout (after the opcode): `u32 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### — (0x300012)

- Modern: 3145746 (0x300012) · 4.3.4: —
- Layout (after the opcode): `u32 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### CMSG_CHAT_CHANNEL_MODERATOR (0x300013)

- Modern: 3145747 (0x300013) · 4.3.4: 326 (0x146)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_UNMODERATOR (0x300014)

- Modern: 3145748 (0x300014) · 4.3.4: 6484 (0x1954)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_INVITE (0x300017)

- Modern: 3145751 (0x300017) · 4.3.4: 324 (0x144)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_KICK (0x300018)

- Modern: 3145752 (0x300018) · 4.3.4: 12630 (0x3156)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_BAN (0x300019)

- Modern: 3145753 (0x300019) · 4.3.4: 15702 (0x3d56)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_UNBAN (0x30001a)

- Modern: 3145754 (0x30001a) · 4.3.4: 11590 (0x2d46)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_ANNOUNCEMENTS (0x30001b)

- Modern: 3145755 (0x30001b) · 4.3.4: 4422 (0x1146)
- Layout (after the opcode): `u32 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### CMSG_CHAT_CHANNEL_SILENCE_ALL (0x30001c)

- Modern: 3145756 (0x30001c) · 4.3.4: 8532 (0x2154)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_UNSILENCE_ALL (0x30001d)

- Modern: 3145757 (0x30001d) · 4.3.4: 9542 (0x2546)
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 5 · 9

### CMSG_CHAT_CHANNEL_DECLINE_INVITE (0x30001e)

- Modern: 3145758 (0x30001e) · 4.3.4: —
- Layout (after the opcode): `u32 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### CMSG_CHAT_MESSAGE_SAY (0x30001f)

- Modern: 3145759 (0x30001f) · 4.3.4: 4436 (0x1154)
- Layout (after the opcode): `u32*2 u8 bits(3) opt[ u8 ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · 1 · (4 unused)

### CMSG_CHAT_MESSAGE_EMOTE (0x300020)

- Modern: 3145760 (0x300020) · 4.3.4: 4438 (0x1156)
- Layout (after the opcode): `u32 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 11 · (5 unused)

### CMSG_CHAT_MESSAGE_YELL (0x300021)

- Modern: 3145761 (0x300021) · 4.3.4: 13636 (0x3544)
- Layout (after the opcode): `u32*2 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · (5 unused)

### CMSG_CHAT_MESSAGE_PARTY (0x300022)

- Modern: 3145762 (0x300022) · 4.3.4: 7494 (0x1d46)
- Layout (after the opcode): `u32*2 u8 bits(3) opt[ u8 ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · 1 · (4 unused)

### CMSG_CHAT_MESSAGE_RAID (0x300023)

- Modern: 3145763 (0x300023) · 4.3.4: 11588 (0x2d44)
- Layout (after the opcode): `u32*2 u8 bits(3) opt[ u8 ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · 1 · (4 unused)

### CMSG_CHAT_MESSAGE_INSTANCE_CHAT (0x300024)

- Modern: 3145764 (0x300024) · 4.3.4: —
- Layout (after the opcode): `u32*2 u8 bits(3) opt[ u8 ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · 1 · (4 unused)

### CMSG_CHAT_MESSAGE_RAID_WARNING (0x300025)

- Modern: 3145765 (0x300025) · 4.3.4: 2372 (0x944)
- Layout (after the opcode): `u32*2 u8 bits(3) opt[ u8 ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · 1 · (4 unused)

### CMSG_CHAT_ADDON_MESSAGE (0x300026)

- Modern: 3145766 (0x300026) · 4.3.4: —
- Layout (after the opcode): `u32 { bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush u32 opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 5 · 8 · 1 · (2 unused)

### CMSG_CHAT_ADDON_MESSAGE_TARGETED (0x300027)

- Modern: 3145767 (0x300027) · 4.3.4: —
- Layout (after the opcode): `u32 { bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush u32 opt[ bytes ] opt[ bytes ] } guid guid u32 u8*2 flush opt[ bytes ] opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: (1 unused) · 4 · (4 unused) · 4 · 1 · (2 unused); byte 16: (7 unused) · 1 · 1 · (6 unused) · 1 · 1 · (7 unused)

### CMSG_GUILD_PROMOTE_MEMBER (0x320000)

- Modern: 3276800 (0x320000) · 4.3.4: 4144 (0x1030)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_DEMOTE_MEMBER (0x320001)

- Modern: 3276801 (0x320001) · 4.3.4: 4128 (0x1020)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_ASSIGN_MEMBER_RANK (0x320002)

- Modern: 3276802 (0x320002) · 4.3.4: 12338 (0x3032)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GUILD_LEAVE (0x320003)

- Modern: 3276803 (0x320003) · 4.3.4: 4129 (0x1021)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_OFFICER_REMOVE_MEMBER (0x320004)

- Modern: 3276804 (0x320004) · 4.3.4: 4657 (0x1231)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_ADD_RANK (0x320005)

- Modern: 3276805 (0x320005) · 4.3.4: 12336 (0x3030)
- Layout (after the opcode): `u32 bits(7) flush u32 opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### CMSG_GUILD_DELETE_RANK (0x320006)

- Modern: 3276806 (0x320006) · 4.3.4: 12852 (0x3234)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GUILD_SHIFT_RANK (0x320007)

- Modern: 3276807 (0x320007) · 4.3.4: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_GUILD_SET_RANK_PERMISSIONS (0x320008)

- Modern: 3276808 (0x320008) · 4.3.4: 4132 (0x1024)
- Layout (after the opcode): `u32 { u8 u32*3 loop[ u32*2 ] bits(7) flush opt[ bytes ] } u32`
- Bit fields (message of zeros, widths in order written): byte 79: 7 · (1 unused)

### CMSG_GUILD_DELETE (0x320009)

- Modern: 3276809 (0x320009) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_QUERY_MEMBER_RECIPES (0x32000a)

- Modern: 3276810 (0x32000a) · 4.3.4: 4151 (0x1037)
- Layout (after the opcode): `u32 guid guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GUILD_QUERY_RECIPES (0x32000b)

- Modern: 3276811 (0x32000b) · 4.3.4: 12339 (0x3033)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_QUERY_NEWS (0x32000d)

- Modern: 3276813 (0x32000d) · 4.3.4: 12320 (0x3020)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_GET_RANKS (0x32000e)

- Modern: 3276814 (0x32000e) · 4.3.4: 4134 (0x1026)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_NEWS_UPDATE_STICKY (0x32000f)

- Modern: 3276815 (0x32000f) · 4.3.4: 12835 (0x3223)
- Layout (after the opcode): `u32 guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### CMSG_GUILD_SET_ACHIEVEMENT_TRACKING (0x320010)

- Modern: 3276816 (0x320010) · 4.3.4: 4135 (0x1027)
- Layout (after the opcode): `u32*2 loop[ u32 ]`

### CMSG_GUILD_SET_FOCUSED_ACHIEVEMENT (0x320011)

- Modern: 3276817 (0x320011) · 4.3.4: 12853 (0x3235)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GUILD_GET_ACHIEVEMENT_MEMBERS (0x320012)

- Modern: 3276818 (0x320012) · 4.3.4: —
- Layout (after the opcode): `u32 guid guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GUILD_SET_MEMBER_NOTE (0x320013)

- Modern: 3276819 (0x320013) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8 flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 1 · (7 unused)

### CMSG_GUILD_GET_ROSTER (0x320014)

- Modern: 3276820 (0x320014) · 4.3.4: 4646 (0x1226)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_UPDATE_MOTD_TEXT (0x320015)

- Modern: 3276821 (0x320015) · 4.3.4: —
- Layout (after the opcode): `u32 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 11 · (5 unused)

### CMSG_GUILD_UPDATE_INFO_TEXT (0x320016)

- Modern: 3276822 (0x320016) · 4.3.4: —
- Layout (after the opcode): `u32 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 11 · (5 unused)

### CMSG_GUILD_CHALLENGE_UPDATE_REQUEST (0x320017)

- Modern: 3276823 (0x320017) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_CHANGE_NAME_REQUEST (0x320018)

- Modern: 3276824 (0x320018) · 4.3.4: 4658 (0x1232)
- Layout (after the opcode): `u32 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### CMSG_GUILD_BANK_LOG_QUERY (0x320019)

- Modern: 3276825 (0x320019) · 4.3.4: 12836 (0x3224)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY (0x32001a)

- Modern: 3276826 (0x32001a) · 4.3.4: 4645 (0x1225)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_PERMISSIONS_QUERY (0x32001b)

- Modern: 3276827 (0x32001b) · 4.3.4: 12322 (0x3022)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_EVENT_LOG_QUERY (0x32001c)

- Modern: 3276828 (0x32001c) · 4.3.4: 4640 (0x1220)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_BANK_SET_TAB_TEXT (0x32001d)

- Modern: 3276829 (0x32001d) · 4.3.4: 12323 (0x3023)
- Layout (after the opcode): `u32*2 u8 bits(6) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: (1 unused) · 13 · (2 unused)

### CMSG_GUILD_BANK_TEXT_QUERY (0x32001e)

- Modern: 3276830 (0x32001e) · 4.3.4: 12832 (0x3220)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GUILD_REPLACE_GUILD_MASTER (0x32001f)

- Modern: 3276831 (0x32001f) · 4.3.4: 4148 (0x1034)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_ADD_BATTLENET_FRIEND (0x320020)

- Modern: 3276832 (0x320020) · 4.3.4: —
- Layout (after the opcode): `u32 u64 guid flush`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · (7 unused)

### CMSG_REFORGE_ITEM (0x340000)

- Modern: 3407872 (0x340000) · 4.3.4: 13082 (0x331a)
- Layout (after the opcode): `u32 guid u32*3`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_INITIATE_TRADE (0x340001)

- Modern: 3407873 (0x340001) · 4.3.4: 30998 (0x7916)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BEGIN_TRADE (0x340002)

- Modern: 3407874 (0x340002) · 4.3.4: 29214 (0x721e)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BUSY_TRADE (0x340003)

- Modern: 3407875 (0x340003) · 4.3.4: 13084 (0x331c)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_IGNORE_TRADE (0x340004)

- Modern: 3407876 (0x340004) · 4.3.4: 28946 (0x7112)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ACCEPT_TRADE (0x340005)

- Modern: 3407877 (0x340005) · 4.3.4: 28944 (0x7110)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_UNACCEPT_TRADE (0x340006)

- Modern: 3407878 (0x340006) · 4.3.4: 14618 (0x391a)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CANCEL_TRADE (0x340007)

- Modern: 3407879 (0x340007) · 4.3.4: 29470 (0x731e)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_TRADE_ITEM (0x340008)

- Modern: 3407880 (0x340008) · 4.3.4: 31500 (0x7b0c)
- Layout (after the opcode): `u32 u8*3`
- Size: 7 bytes (packed GUIDs not counted)

### CMSG_CLEAR_TRADE_ITEM (0x340009)

- Modern: 3407881 (0x340009) · 4.3.4: 28696 (0x7018)
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_SET_TRADE_GOLD (0x34000a)

- Modern: 3407882 (0x34000a) · 4.3.4: 12296 (0x3008)
- Layout (after the opcode): `u32 u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_STABLE_PET (0x340013)

- Modern: 3407891 (0x340013) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_PET_SLOT (0x340014)

- Modern: 3407892 (0x340014) · 4.3.4: 14852 (0x3a04)
- Layout (after the opcode): `u32*2 u8 guid`
- Size: 9 bytes (packed GUIDs not counted)

### CMSG_SET_CURRENCY_FLAGS (0x340015)

- Modern: 3407893 (0x340015) · 4.3.4: 29446 (0x7306)
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_BATTLEFIELD_LEAVE (0x34001e)

- Modern: 3407902 (0x34001e) · 4.3.4: 12312 (0x3018)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_QUEST_COMPLETION_NPCS (0x340020)

- Modern: 3407904 (0x340020) · 4.3.4: 29442 (0x7302)
- Layout (after the opcode): `u32*2 loop[ u32 ]`

### CMSG_QUERY_QUEST_ITEM_USABILITY (0x340021)

- Modern: 3407905 (0x340021) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32 loop[ guid ]`

### CMSG_REQUEST_CEMETERY_LIST (0x340022)

- Modern: 3407906 (0x340022) · 4.3.4: 29194 (0x720a)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_PREFERRED_CEMETERY (0x340023)

- Modern: 3407907 (0x340023) · 4.3.4: 12574 (0x311e)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_JOIN_RATED_BATTLEGROUND (0x340024)

- Modern: 3407908 (0x340024) · 4.3.4: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_REQUEST_HONOR_STATS (0x340027)

- Modern: 3407911 (0x340027) · 4.3.4: 31006 (0x791e)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_PVP_LOG_DATA (0x340028)

- Modern: 3407912 (0x340028) · 4.3.4: 29448 (0x7308)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLEFIELD_LIST (0x34002a)

- Modern: 3407914 (0x34002a) · 4.3.4: 14356 (0x3814)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CANCEL_QUEUED_SPELL (0x34002b)

- Modern: 3407915 (0x34002b) · 4.3.4: 31516 (0x7b1c)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_OBJECT_UPDATE_FAILED (0x34002c)

- Modern: 3407916 (0x34002c) · 4.3.4: 14344 (0x3808)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_OBJECT_UPDATE_RESCUED (0x34002d)

- Modern: 3407917 (0x34002d) · 4.3.4: 14598 (0x3906)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_VIOLENCE_LEVEL (0x340030)

- Modern: 3407920 (0x340030) · 4.3.4: 30742 (0x7816)
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_USED_FOLLOW (0x340032)

- Modern: 3407922 (0x340032) · 4.3.4: 30994 (0x7912)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_STAND_STATE_CHANGE (0x340035)

- Modern: 3407925 (0x340035) · 4.3.4: 1333 (0x535)
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_MISSILE_TRAJECTORY_COLLISION (0x340036)

- Modern: 3407926 (0x340036) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32 guid f32*3`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_SAVE_CUF_PROFILES (0x340037)

- Modern: 3407927 (0x340037) · 4.3.4: 29454 (0x730e)
- Layout (after the opcode): `u32*2 loop[ { bits(7) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush u16*2 u8*5 u16*3 opt[ bytes ] } ]`

### CMSG_REQUEST_PVP_REWARDS (0x34003f)

- Modern: 3407935 (0x34003f) · 4.3.4: 30732 (0x780c)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_SCHEDULED_PVP_INFO (0x340040)

- Modern: 3407936 (0x340040) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TRANSMOGRIFY_ITEMS (0x340041)

- Modern: 3407937 (0x340041) · 4.3.4: 15118 (0x3b0e)
- Layout (after the opcode): `u32*2 guid loop[ u32*4 ] flush`

### CMSG_UNLOCK_VOID_STORAGE (0x34004b)

- Modern: 3407947 (0x34004b) · 4.3.4: 31508 (0x7b14)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_VOID_STORAGE (0x34004c)

- Modern: 3407948 (0x34004c) · 4.3.4: 30986 (0x790a)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_VOID_STORAGE_TRANSFER (0x34004d)

- Modern: 3407949 (0x34004d) · 4.3.4: 14350 (0x380e)
- Layout (after the opcode): `u32 guid u32*2 loop[ guid ] loop[ guid ]`

### CMSG_SWAP_VOID_ITEM (0x34004e)

- Modern: 3407950 (0x34004e) · 4.3.4: 12804 (0x3204)
- Layout (after the opcode): `u32 guid guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CLEAR_RAID_MARKER (0x340050)

- Modern: 3407952 (0x340050) · 4.3.4: 29440 (0x7300)
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_REQUEST_GUILD_REWARDS_LIST (0x340051)

- Modern: 3407953 (0x340051) · 4.3.4: 12306 (0x3012)
- Layout (after the opcode): `u32 u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_REQUEST_GUILD_PARTY_STATE (0x340052)

- Modern: 3407954 (0x340052) · 4.3.4: 14592 (0x3900)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_COUNTDOWN_TIMER (0x340053)

- Modern: 3407955 (0x340053) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CANCEL_AURA (0x340058)

- Modern: 3407960 (0x340058) · 4.3.4: 3622 (0xe26)
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GAME_EVENT_DEBUG_DISABLE (0x34005a)

- Modern: 3407962 (0x34005a) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GAME_EVENT_DEBUG_ENABLE (0x34005b)

- Modern: 3407963 (0x34005b) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_GAME_EVENT_DEBUG_VIEW_STATE (0x340062)

- Modern: 3407970 (0x340062) · 4.3.4: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### — (0x340063)

- Modern: 3407971 (0x340063) · 4.3.4: —
- Layout (after the opcode): `u32*3 guid flush`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · (7 unused)

### CMSG_AREA_TRIGGER (0x340084)

- Modern: 3408004 (0x340084) · 4.3.4: 2359 (0x937)
- Layout (after the opcode): `u32*2 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · 1 · (6 unused)

### CMSG_BATTLE_PET_UPDATE_NOTIFY (0x34008d)

- Modern: 3408013 (0x34008d) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_UPDATE_DISPLAY_NOTIFY (0x34008e)

- Modern: 3408014 (0x34008e) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ASSIGN_EQUIPMENT_SET_SPEC (0x3400b5)

- Modern: 3408053 (0x3400b5) · 4.3.4: —
- Layout (after the opcode): `u32 u64 u32`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_CONFIRM_RESPEC_WIPE (0x3400bb)

- Modern: 3408059 (0x3400bb) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_CONFIRM_BARBERS_CHOICE (0x3400bc)

- Modern: 3408060 (0x3400bc) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LOOT_UNIT (0x3400bd)

- Modern: 3408061 (0x3400bd) · 4.3.4: 295 (0x127)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LOOT_MONEY (0x3400be)

- Modern: 3408062 (0x3400be) · 4.3.4: 25127 (0x6227)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_LOOT_ITEM (0x3400bf)

- Modern: 3408063 (0x3400bf) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ { guid u8 } ] flush`

### CMSG_MASTER_LOOT_ITEM (0x3400c0)

- Modern: 3408064 (0x3400c0) · 4.3.4: —
- Layout (after the opcode): `u32*2 guid loop[ { guid u8 } ]`

### CMSG_LOOT_RELEASE (0x3400c1)

- Modern: 3408065 (0x3400c1) · 4.3.4: 8199 (0x2007)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LOOT_ROLL (0x3400c2)

- Modern: 3408066 (0x3400c2) · 4.3.4: 26932 (0x6934)
- Layout (after the opcode): `u32 guid u8*2`
- Size: 6 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8

### CMSG_SCENE_PLAYBACK_COMPLETE (0x3400d0)

- Modern: 3408080 (0x3400d0) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_SCENE_PLAYBACK_CANCELED (0x3400d1)

- Modern: 3408081 (0x3400d1) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_SCENE_TRIGGER_EVENT (0x3400d2)

- Modern: 3408082 (0x3400d2) · 4.3.4: —
- Layout (after the opcode): `u32 bits(6) flush u32 opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 6 · (2 unused)

### CMSG_SET_DIFFICULTY_ID (0x3400d3)

- Modern: 3408083 (0x3400d3) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_KEYBOUND_OVERRIDE (0x3400d4)

- Modern: 3408084 (0x3400d4) · 4.3.4: —
- Layout (after the opcode): `u32 u16`
- Size: 6 bytes (packed GUIDs not counted)

### CMSG_MAIL_DELETE (0x3400d6)

- Modern: 3408086 (0x3400d6) · 4.3.4: 24836 (0x6104)
- Layout (after the opcode): `u32 u64 u32`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_MAKE_CONTITIONAL_APPEARANCE_PERMANENT (0x3400d9)

- Modern: 3408089 (0x3400d9) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_REQUEST_VEHICLE_EXIT (0x3400ed)

- Modern: 3408109 (0x3400ed) · 4.3.4: 11061 (0x2b35)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_VEHICLE_PREV_SEAT (0x3400ee)

- Modern: 3408110 (0x3400ee) · 4.3.4: 19460 (0x4c04)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_VEHICLE_NEXT_SEAT (0x3400ef)

- Modern: 3408111 (0x3400ef) · 4.3.4: 17460 (0x4434)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_VEHICLE_SWITCH_SEAT (0x3400f0)

- Modern: 3408112 (0x3400f0) · 4.3.4: 19476 (0x4c14)
- Layout (after the opcode): `u32 guid u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_RIDE_VEHICLE_INTERACT (0x3400f1)

- Modern: 3408113 (0x3400f1) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_EJECT_PASSENGER (0x3400f2)

- Modern: 3408114 (0x3400f2) · 4.3.4: 26919 (0x6927)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_USE_CRITTER_ITEM (0x3400f7)

- Modern: 3408119 (0x3400f7) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CHECK_IS_ADVENTURE_MAP_POI_VALID (0x3400fd)

- Modern: 3408125 (0x3400fd) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_ATTACK_SWING (0x34010b)

- Modern: 3408139 (0x34010b) · 4.3.4: 2342 (0x926)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ATTACK_STOP (0x34010c)

- Modern: 3408140 (0x34010c) · 4.3.4: 16646 (0x4106)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CANCEL_CHANNELLING (0x340121)

- Modern: 3408161 (0x340121) · 4.3.4: 27685 (0x6c25)
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_CANCEL_GROWTH_AURA (0x340126)

- Modern: 3408166 (0x340126) · 4.3.4: 567 (0x237)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_CREATURE (0x340127)

- Modern: 3408167 (0x340127) · 4.3.4: 9990 (0x2706)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUERY_GAME_OBJECT (0x340128)

- Modern: 3408168 (0x340128) · 4.3.4: 16407 (0x4017)
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUERY_NPC_TEXT (0x340129)

- Modern: 3408169 (0x340129) · 4.3.4: 20004 (0x4e24)
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUERY_QUEST_INFO (0x34012a)

- Modern: 3408170 (0x34012a) · 4.3.4: 3334 (0xd06)
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUERY_PAGE_TEXT (0x34012b)

- Modern: 3408171 (0x34012b) · 4.3.4: 26132 (0x6614)
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUERY_PET_NAME (0x34012c)

- Modern: 3408172 (0x34012c) · 4.3.4: 28452 (0x6f24)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_BATTLE_PET_NAME (0x34012d)

- Modern: 3408173 (0x34012d) · 4.3.4: —
- Layout (after the opcode): `u32 guid guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_PETITION (0x34012e)

- Modern: 3408174 (0x34012e) · 4.3.4: —
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_REQUEST_PLAYED_TIME (0x340131)

- Modern: 3408177 (0x340131) · 4.3.4: 2052 (0x804)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_SET_TITLE (0x340135)

- Modern: 3408181 (0x340135) · 4.3.4: 8471 (0x2117)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CANCEL_MOUNT_AURA (0x340136)

- Modern: 3408182 (0x340136) · 4.3.4: 1589 (0x635)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_MOUNT_SPECIAL_ANIM (0x340137)

- Modern: 3408183 (0x340137) · 4.3.4: 10247 (0x2807)
- Layout (after the opcode): `u32 { u32*2 loop[ u32 ] }`

### CMSG_SPAWN_TRACKING_UPDATE (0x340149)

- Modern: 3408201 (0x340149) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ u32*3 ]`

### CMSG_DESTROY_ITEM (0x34014c)

- Modern: 3408204 (0x34014c) · 4.3.4: 18983 (0x4a27)
- Layout (after the opcode): `u32*2 u8*2`
- Size: 10 bytes (packed GUIDs not counted)

### CMSG_GET_MIRROR_IMAGE_DATA (0x340150)

- Modern: 3408208 (0x340150) · 4.3.4: 3109 (0xc25)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_USE_ITEM (0x340151)

- Modern: 3408209 (0x340151) · 4.3.4: 11270 (0x2c06)
- Layout (after the opcode): `u32 u8*2 guid { guid loop[ u32 ] u32*2 f32*2 guid u32*3 u8 loop[ u32*2 ] bits(5) opt[ u8 ] bits(2) opt[ u8 ] flush { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ { guid f32*3 } ] opt[ { guid f32*3 } ] opt[ f32 ] opt[ u32 ] opt[ bytes ] } opt[ u64 ] loop[ u32*3 flush opt[ u8 ] ] loop[ u32*3 flush opt[ u8 ] ] opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8; byte 46: 8 · 5 · 1 · 2 · 1 · (7 unused) · 28; byte 52: 1 · 1 · 1 · 1 · 7 · (1 unused)

### CMSG_ADD_TOY (0x340152)

- Modern: 3408210 (0x340152) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_USE_TOY (0x340153)

- Modern: 3408211 (0x340153) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32*2 f32*2 guid u32*3 u8 loop[ u32*2 ] bits(5) opt[ u8 ] bits(2) opt[ u8 ] flush { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ { guid f32*3 } ] opt[ { guid f32*3 } ] opt[ f32 ] opt[ u32 ] opt[ bytes ] } opt[ u64 ] loop[ u32*3 flush opt[ u8 ] ] loop[ u32*3 flush opt[ u8 ] ] opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 42: 8 · 5 · 1 · 2 · 1 · (7 unused) · 28; byte 48: 1 · 1 · 1 · 1 · 7 · (1 unused)

### CMSG_PET_CAST_SPELL (0x340154)

- Modern: 3408212 (0x340154) · 4.3.4: 25399 (0x6337)
- Layout (after the opcode): `u32 guid { guid loop[ u32 ] u32*2 f32*2 guid u32*3 u8 loop[ u32*2 ] bits(5) opt[ u8 ] bits(2) opt[ u8 ] flush { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ { guid f32*3 } ] opt[ { guid f32*3 } ] opt[ f32 ] opt[ u32 ] opt[ bytes ] } opt[ u64 ] loop[ u32*3 flush opt[ u8 ] ] loop[ u32*3 flush opt[ u8 ] ] opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 44: 8 · 5 · 1 · 2 · 1 · (7 unused) · 28; byte 50: 1 · 1 · 1 · 1 · 7 · (1 unused)

### CMSG_CAST_SPELL (0x340155)

- Modern: 3408213 (0x340155) · 4.3.4: 19463 (0x4c07)
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32*2 f32*2 guid u32*3 u8 loop[ u32*2 ] bits(5) opt[ u8 ] bits(2) opt[ u8 ] flush { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ { guid f32*3 } ] opt[ { guid f32*3 } ] opt[ f32 ] opt[ u32 ] opt[ bytes ] } opt[ u64 ] loop[ u32*3 flush opt[ u8 ] ] loop[ u32*3 flush opt[ u8 ] ] opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 42: 8 · 5 · 1 · 2 · 1 · (7 unused) · 28; byte 48: 1 · 1 · 1 · 1 · 7 · (1 unused)

### CMSG_UPDATE_SPELL_VISUAL (0x340156)

- Modern: 3408214 (0x340156) · 4.3.4: —
- Layout (after the opcode): `u32*3 guid`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_UPDATE_AREA_TRIGGER_VISUAL (0x340157)

- Modern: 3408215 (0x340157) · 4.3.4: —
- Layout (after the opcode): `u32*3 guid`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_CANCEL_CAST (0x340158)

- Modern: 3408216 (0x340158) · 4.3.4: 277 (0x115)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CLOSE_QUEST_CHOICE (0x34015c)

- Modern: 3408220 (0x34015c) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_LFG_LIST_BLACKLIST (0x34015e)

- Modern: 3408222 (0x34015e) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SAVE_GUILD_EMBLEM (0x340162)

- Modern: 3408226 (0x340162) · 4.3.4: —
- Layout (after the opcode): `u32 guid { u32*5 }`
- Size: 24 bytes (packed GUIDs not counted)

### CMSG_TABARD_VENDOR_ACTIVATE (0x340163)

- Modern: 3408227 (0x340163) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_TOGGLE_PVP (0x340165)

- Modern: 3408229 (0x340165) · 4.3.4: 26645 (0x6815)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_PVP (0x340166)

- Modern: 3408230 (0x340166) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_BATTLEMASTER_HELLO (0x34016b)

- Modern: 3408235 (0x34016b) · 4.3.4: 564 (0x234)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_ADVANCED_COMBAT_LOGGING (0x34016e)

- Modern: 3408238 (0x34016e) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_ITEM_TEXT_QUERY (0x340180)

- Modern: 3408256 (0x340180) · 4.3.4: 9222 (0x2406)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_OPEN_ITEM (0x340181)

- Modern: 3408257 (0x340181) · 4.3.4: 27188 (0x6a34)
- Layout (after the opcode): `u32 u8*2`
- Size: 6 bytes (packed GUIDs not counted)

### CMSG_READ_ITEM (0x340182)

- Modern: 3408258 (0x340182) · 4.3.4: 12054 (0x2f16)
- Layout (after the opcode): `u32 u8*2`
- Size: 6 bytes (packed GUIDs not counted)

### CMSG_SET_INSERT_ITEMS_LEFT_TO_RIGHT (0x340185)

- Modern: 3408261 (0x340185) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_ADVENTURE_MAP_START_QUEST (0x340194)

- Modern: 3408276 (0x340194) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUERY_TREASURE_PICKER (0x340197)

- Modern: 3408279 (0x340197) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_REQUEST_WORLD_QUEST_UPDATE (0x340198)

- Modern: 3408280 (0x340198) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_AREA_POI_UPDATE (0x340199)

- Modern: 3408281 (0x340199) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REMOVE_NEW_ITEM (0x34019a)

- Modern: 3408282 (0x34019a) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_AZERITE_EMPOWERED_ITEM_VIEWED (0x34019b)

- Modern: 3408283 (0x34019b) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LFG_LIST_JOIN (0x3401ab)

- Modern: 3408299 (0x3401ab) · 4.3.4: —
- Layout (after the opcode): `u32 { bits(5) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush { f32*2 u32 loop[ u32 f32 u32*2 flush ] } u32 f32 loop[ u32 ] opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 5 · 10 · 11; byte 5: 8 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused)

### CMSG_LFG_LIST_UPDATE_REQUEST (0x3401ac)

- Modern: 3408300 (0x3401ac) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush } { bits(5) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush { f32*2 u32 loop[ u32 f32 u32*2 flush ] } u32 f32 loop[ u32 ] opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] }`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused) · 5 · 10 · 11; byte 24: 8 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused)

### CMSG_AZERITE_EMPOWERED_ITEM_SELECT_POWER (0x3401ad)

- Modern: 3408301 (0x3401ad) · 4.3.4: —
- Layout (after the opcode): `u32 u8*3 u32`
- Size: 11 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8

### CMSG_AZERITE_ESSENCE_UNLOCK_MILESTONE (0x3401ae)

- Modern: 3408302 (0x3401ae) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_AZERITE_ESSENCE_ACTIVATE_ESSENCE (0x3401af)

- Modern: 3408303 (0x3401af) · 4.3.4: —
- Layout (after the opcode): `u32*2 u8`
- Size: 9 bytes (packed GUIDs not counted)

### CMSG_REPORT_SERVER_LAG (0x3401b6)

- Modern: 3408310 (0x3401b6) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_OFFER_PETITION (0x3401b8)

- Modern: 3408312 (0x3401b8) · 4.3.4: 18455 (0x4817)
- Layout (after the opcode): `u32*2 guid guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_REMOVE_GLYPH (0x3401bb)

- Modern: 3408315 (0x3401bb) · 4.3.4: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_LFG_LIST_SET_ROLES (0x3401bd)

- Modern: 3408317 (0x3401bd) · 4.3.4: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_TRAITS_COMMIT_CONFIG (0x3401be)

- Modern: 3408318 (0x3401be) · 4.3.4: —
- Layout (after the opcode): `u32 { u32*4 opt[ u32 ] opt[ u32*3 ] opt[ u32 ] loop[ u32*4 ] u8 flush loop[ u32*2 loop[ u32*4 ] flush ] opt[ bytes ] } u32*2`
- Bit fields (message of zeros, widths in order written): byte 18: 9 · (7 unused)

### CMSG_CLOSE_TRAIT_SYSTEM_INTERACTION (0x3401c9)

- Modern: 3408329 (0x3401c9) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SEAMLESS_TRANSFER_COMPLETE (0x3401cd)

- Modern: 3408333 (0x3401cd) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3401dc)

- Modern: 3408348 (0x3401dc) · 4.3.4: —
- Layout (after the opcode): `u32 u8 u32*2 u64*2 u32 u8 u32*2 loop[ { u64*2 u32*2 } ] loop[ { u32*2 } ]`

### CMSG_SEND_TEXT_EMOTE (0x350012)

- Modern: 3473426 (0x350012) · 4.3.4: 11812 (0x2e24)
- Layout (after the opcode): `u32 guid u32*2 { u32*2 loop[ u32 ] }`

### CMSG_SET_SHEATHED (0x350013)

- Modern: 3473427 (0x350013) · 4.3.4: 17190 (0x4326)
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_PET_SET_ACTION (0x350014)

- Modern: 3473428 (0x350014) · 4.3.4: 26884 (0x6904)
- Layout (after the opcode): `u32 guid u32*2 flush opt[ u32*2 ]`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · (7 unused)

### CMSG_PET_ACTION (0x350015)

- Modern: 3473429 (0x350015) · 4.3.4: 550 (0x226)
- Layout (after the opcode): `u32 guid u32 guid f32*3`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_PET_STOP_ATTACK (0x350016)

- Modern: 3473430 (0x350016) · 4.3.4: 27668 (0x6c14)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_PET_ABANDON (0x350017)

- Modern: 3473431 (0x350017) · 4.3.4: 3108 (0xc24)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_PET_CANCEL_AURA (0x350018)

- Modern: 3473432 (0x350018) · 4.3.4: 19237 (0x4b25)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_PET_SPELL_AUTOCAST (0x350019)

- Modern: 3473433 (0x350019) · 4.3.4: 9492 (0x2514)
- Layout (after the opcode): `u32 guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### CMSG_REQUEST_PET_INFO (0x35001a)

- Modern: 3473434 (0x35001a) · 4.3.4: 18724 (0x4924)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_STABLED_PETS (0x35001b)

- Modern: 3473435 (0x35001b) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TALK_TO_GOSSIP (0x35001c)

- Modern: 3473436 (0x35001c) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CLOSE_INTERACTION (0x35001d)

- Modern: 3473437 (0x35001d) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GOSSIP_SELECT_OPTION (0x35001e)

- Modern: 3473438 (0x35001e) · 4.3.4: 534 (0x216)
- Layout (after the opcode): `u32 guid u32*2 u8 flush opt[ bytes ]`

### CMSG_SPELL_CLICK (0x35001f)

- Modern: 3473439 (0x35001f) · 4.3.4: 2053 (0x805)
- Layout (after the opcode): `u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_QUEST_GIVER_HELLO (0x350020)

- Modern: 3473440 (0x350020) · 4.3.4: 3351 (0xd17)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUEST_GIVER_QUERY_QUEST (0x350021)

- Modern: 3473441 (0x350021) · 4.3.4: 12052 (0x2f14)
- Layout (after the opcode): `u32 { guid u32 flush }`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### CMSG_QUEST_GIVER_ACCEPT_QUEST (0x350022)

- Modern: 3473442 (0x350022) · 4.3.4: 27447 (0x6b37)
- Layout (after the opcode): `u32 guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### CMSG_QUEST_GIVER_COMPLETE_QUEST (0x350023)

- Modern: 3473443 (0x350023) · 4.3.4: 276 (0x114)
- Layout (after the opcode): `u32 guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### CMSG_QUEST_GIVER_CHOOSE_REWARD (0x350024)

- Modern: 3473444 (0x350024) · 4.3.4: 8485 (0x2125)
- Layout (after the opcode): `u32 guid u32 { bits(2) flush { u32*3 flush { bits(6) flush loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 8: 2 · (6 unused); byte 21: 1 · (7 unused) · 6 · (2 unused)

### CMSG_QUEST_GIVER_REQUEST_REWARD (0x350025)

- Modern: 3473445 (0x350025) · 4.3.4: 9524 (0x2534)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUEST_GIVER_STATUS_QUERY (0x350026)

- Modern: 3473446 (0x350026) · 4.3.4: 17415 (0x4407)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY (0x350027)

- Modern: 3473447 (0x350027) · 4.3.4: 25349 (0x6305)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUEST_CONFIRM_ACCEPT (0x350028)

- Modern: 3473448 (0x350028) · 4.3.4: 3349 (0xd15)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_PUSH_QUEST_TO_PARTY (0x350029)

- Modern: 3473449 (0x350029) · 4.3.4: 19220 (0x4b14)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUEST_PUSH_RESULT (0x35002a)

- Modern: 3473450 (0x35002a) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32 u8`
- Size: 9 bytes (packed GUIDs not counted)

### CMSG_LIST_INVENTORY (0x35002b)

- Modern: 3473451 (0x35002b) · 4.3.4: 10246 (0x2806)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SELL_ITEM (0x35002c)

- Modern: 3473452 (0x35002c) · 4.3.4: 19989 (0x4e15)
- Layout (after the opcode): `u32 guid guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_BUY_ITEM (0x35002d)

- Modern: 3473453 (0x35002d) · 4.3.4: 1846 (0x736)
- Layout (after the opcode): `u32 guid guid u32*4 { u32*3 flush { bits(6) flush loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 34: 1 · (7 unused) · 6 · (2 unused)

### CMSG_BUY_BACK_ITEM (0x35002e)

- Modern: 3473454 (0x35002e) · 4.3.4: 27671 (0x6c17)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_TAXI_NODE_STATUS_QUERY (0x350032)

- Modern: 3473458 (0x350032) · 4.3.4: 12069 (0x2f25)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ENABLE_TAXI_NODE (0x350033)

- Modern: 3473459 (0x350033) · 4.3.4: 3094 (0xc16)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TAXI_QUERY_AVAILABLE_NODES (0x350034)

- Modern: 3473460 (0x350034) · 4.3.4: 27654 (0x6c06)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ACTIVATE_TAXI (0x350035)

- Modern: 3473461 (0x350035) · 4.3.4: 28166 (0x6e06)
- Layout (after the opcode): `u32 guid u32*3`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_TAXI_REQUEST_EARLY_LANDING (0x350036)

- Modern: 3473462 (0x350036) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TRAINER_LIST (0x350037)

- Modern: 3473463 (0x350037) · 4.3.4: 9014 (0x2336)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TRAINER_BUY_SPELL (0x350038)

- Modern: 3473464 (0x350038) · 4.3.4: 17429 (0x4415)
- Layout (after the opcode): `u32 guid u32*2`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_SPIRIT_HEALER_ACTIVATE (0x350039)

- Modern: 3473465 (0x350039) · 4.3.4: 11814 (0x2e26)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_AREA_SPIRIT_HEALER_QUERY (0x35003a)

- Modern: 3473466 (0x35003a) · 4.3.4: 18695 (0x4907)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_AREA_SPIRIT_HEALER_QUEUE (0x35003b)

- Modern: 3473467 (0x35003b) · 4.3.4: 18453 (0x4815)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BINDER_ACTIVATE (0x35003c)

- Modern: 3473468 (0x35003c) · 4.3.4: 16390 (0x4006)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BANKER_ACTIVATE (0x35003d)

- Modern: 3473469 (0x35003d) · 4.3.4: 5 (0x5)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_BUY_BANK_SLOT (0x35003e)

- Modern: 3473470 (0x35003e) · 4.3.4: 1061 (0x425)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_BANK_ACTIVATE (0x35003f)

- Modern: 3473471 (0x35003f) · 4.3.4: 11831 (0x2e37)
- Layout (after the opcode): `u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_AUTO_GUILD_BANK_ITEM (0x350040)

- Modern: 3473472 (0x350040) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8 · 1 · (7 unused)

### CMSG_STORE_GUILD_BANK_ITEM (0x350041)

- Modern: 3473473 (0x350041) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8 · 1 · (7 unused)

### CMSG_SWAP_ITEM_WITH_GUILD_BANK_ITEM (0x350042)

- Modern: 3473474 (0x350042) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8 · 1 · (7 unused)

### CMSG_SWAP_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM (0x350043)

- Modern: 3473475 (0x350043) · 4.3.4: —
- Layout (after the opcode): `u32 guid loop[ u8*2 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 16

### CMSG_MOVE_GUILD_BANK_ITEM (0x350044)

- Modern: 3473476 (0x350044) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*4`
- Size: 8 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8 · 8

### CMSG_MERGE_ITEM_WITH_GUILD_BANK_ITEM (0x350045)

- Modern: 3473477 (0x350045) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8; byte 11: 1 · (7 unused)

### CMSG_SPLIT_ITEM_TO_GUILD_BANK (0x350046)

- Modern: 3473478 (0x350046) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8; byte 11: 1 · (7 unused)

### CMSG_MERGE_GUILD_BANK_ITEM_WITH_ITEM (0x350047)

- Modern: 3473479 (0x350047) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8; byte 11: 1 · (7 unused)

### CMSG_SPLIT_GUILD_BANK_ITEM_TO_INVENTORY (0x350048)

- Modern: 3473480 (0x350048) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8; byte 11: 1 · (7 unused)

### CMSG_AUTO_STORE_GUILD_BANK_ITEM (0x350049)

- Modern: 3473481 (0x350049) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*2`
- Size: 6 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8

### CMSG_MERGE_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM (0x35004a)

- Modern: 3473482 (0x35004a) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*4 u32`
- Size: 12 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8 · 8

### CMSG_SPLIT_GUILD_BANK_ITEM (0x35004b)

- Modern: 3473483 (0x35004b) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*4 u32`
- Size: 12 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 8 · 8

### CMSG_GUILD_BANK_QUERY_TAB (0x35004c)

- Modern: 3473484 (0x35004c) · 4.3.4: 11829 (0x2e35)
- Layout (after the opcode): `u32 guid u8 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 1 · (7 unused)

### CMSG_GUILD_BANK_BUY_TAB (0x35004d)

- Modern: 3473485 (0x35004d) · 4.3.4: 3127 (0xc37)
- Layout (after the opcode): `u32 guid u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_GUILD_BANK_UPDATE_TAB (0x35004e)

- Modern: 3473486 (0x35004e) · 4.3.4: 262 (0x106)
- Layout (after the opcode): `u32 { guid u8 bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 7 · 9

### CMSG_GUILD_BANK_DEPOSIT_MONEY (0x35004f)

- Modern: 3473487 (0x35004f) · 4.3.4: 1799 (0x707)
- Layout (after the opcode): `u32 guid u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_GUILD_BANK_WITHDRAW_MONEY (0x350050)

- Modern: 3473488 (0x350050) · 4.3.4: 55 (0x37)
- Layout (after the opcode): `u32 guid u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_PETITION_SHOW_LIST (0x350051)

- Modern: 3473489 (0x350051) · 4.3.4: 17943 (0x4617)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_PETITION_BUY (0x350052)

- Modern: 3473490 (0x350052) · 4.3.4: 19973 (0x4e05)
- Layout (after the opcode): `u32 u8 flush guid u32 opt[ bytes ]`

### CMSG_PETITION_SHOW_SIGNATURES (0x350053)

- Modern: 3473491 (0x350053) · 4.3.4: 20245 (0x4f15)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_AUCTION_HELLO_REQUEST (0x350054)

- Modern: 3473492 (0x350054) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_AUCTION_SELL_ITEM (0x350055)

- Modern: 3473493 (0x350055) · 4.3.4: 18950 (0x4a06)
- Layout (after the opcode): `u32 guid u64*2 u32 bits(6) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ guid u32 ]`
- Bit fields (message of zeros, widths in order written): byte 24: 1 · 6 · (1 unused)

### CMSG_AUCTION_REMOVE_ITEM (0x350056)

- Modern: 3473494 (0x350056) · 4.3.4: 25638 (0x6426)
- Layout (after the opcode): `u32 guid u32*2 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ]`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · (7 unused)

### CMSG_AUCTION_LIST_ITEMS (0x350057)

- Modern: 3473495 (0x350057) · 4.3.4: 804 (0x324)
- Layout (after the opcode): `u32 { guid u32 u8*2 u32 u8 u32 u8 loop[ u8 ] u8 flush opt[ bytes ] bits(3) opt[ u8 ] opt[ u8 ] flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ { u32 bits(5) flush loop[ u64 u32 ] } ] u32 bytes }`
- Bit fields (message of zeros, widths in order written): byte 8: 8 · 8; byte 19: 8 · 1 · (15 unused) · 3 · 1 · 1 · (3 unused)

### CMSG_AUCTION_REPLICATE_ITEMS (0x350058)

- Modern: 3473496 (0x350058) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32*4 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ]`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused)

### CMSG_AUCTION_LIST_OWNER_ITEMS (0x350059)

- Modern: 3473497 (0x350059) · 4.3.4: 518 (0x206)
- Layout (after the opcode): `u32 guid u32 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ]`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### CMSG_AUCTION_LIST_BIDDER_ITEMS (0x35005a)

- Modern: 3473498 (0x35005a) · 4.3.4: 26935 (0x6937)
- Layout (after the opcode): `u32 guid u32 bits(7) opt[ u8 ] flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 8: 7 · 1

### CMSG_AUCTION_PLACE_BID (0x35005b)

- Modern: 3473499 (0x35005b) · 4.3.4: 8966 (0x2306)
- Layout (after the opcode): `u32 guid u32 u64 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ]`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · (7 unused)

### CMSG_AUCTION_BROWSE_QUERY (0x35005c)

- Modern: 3473500 (0x35005c) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32 u8*4 u32*2 u8 u32 loop[ u8 ] u8 bits(3) bits(2) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] opt[ bytes ] loop[ { u32 bits(5) flush loop[ u64 u32 ] } ] loop[ { u8 flush } ] }`
- Bit fields (message of zeros, widths in order written): byte 8: 8 · 8 · 8 · 8; byte 25: 1 · 8 · 3 · 2 · (2 unused)

### CMSG_AUCTION_LIST_ITEMS_BY_BUCKET_KEY (0x35005d)

- Modern: 3473501 (0x35005d) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32 u8 bits(2) flush { loop[ u8 ] bits(4) opt[ u8 ] opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] flush opt[ u16 ] opt[ u16 ] } opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ { u8 flush } ]`
- Bit fields (message of zeros, widths in order written): byte 8: 8 · 1 · 2 · (5 unused) · 20; byte 12: 1 · 11; byte 14: 1 · (7 unused)

### CMSG_AUCTION_LIST_ITEMS_BY_ITEM_ID (0x35005e)

- Modern: 3473502 (0x35005e) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32*3 bits(2) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ { u8 flush } ]`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · 2 · (5 unused)

### CMSG_AUCTION_LIST_OWNED_ITEMS (0x35005f)

- Modern: 3473503 (0x35005f) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32 bits(2) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ { u8 flush } ]`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · 2 · (5 unused)

### CMSG_AUCTION_LIST_BIDDED_ITEMS (0x350060)

- Modern: 3473504 (0x350060) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32 bits(7) bits(2) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ u32 ] loop[ { u8 flush } ]`

### CMSG_AUCTION_LIST_BUCKETS_BY_BUCKET_KEYS (0x350061)

- Modern: 3473505 (0x350061) · 4.3.4: —
- Layout (after the opcode): `u32 guid bits(7) bits(2) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ { loop[ u8 ] bits(4) opt[ u8 ] opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] flush opt[ u16 ] opt[ u16 ] } ] loop[ { u8 flush } ]`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 7 · 2 · (6 unused)

### CMSG_AUCTION_GET_COMMODITY_QUOTE (0x350062)

- Modern: 3473506 (0x350062) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32*2 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ]`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · (7 unused)

### CMSG_AUCTION_CONFIRM_COMMODITIES_PURCHASE (0x350063)

- Modern: 3473507 (0x350063) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32*2 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ]`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · (7 unused)

### CMSG_AUCTION_CANCEL_COMMODITIES_PURCHASE (0x350064)

- Modern: 3473508 (0x350064) · 4.3.4: —
- Layout (after the opcode): `u32 guid flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ]`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_AUCTION_SELL_COMMODITY (0x350065)

- Modern: 3473509 (0x350065) · 4.3.4: —
- Layout (after the opcode): `u32 guid u64 u32 bits(6) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] loop[ guid u32 ]`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · 6 · (1 unused)

### CMSG_AUCTION_LIST_PENDING_SALES (0x350066)

- Modern: 3473510 (0x350066) · 4.3.4: 11287 (0x2c17)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_TIME (0x350069)

- Modern: 3473513 (0x350069) · 4.3.4: 2614 (0xa36)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LOGOUT_REQUEST (0x35006a)

- Modern: 3473514 (0x35006a) · 4.3.4: 2597 (0xa25)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_LOGOUT_CANCEL (0x35006b)

- Modern: 3473515 (0x35006b) · 4.3.4: 8996 (0x2324)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LOGOUT_INSTANT (0x35006c)

- Modern: 3473516 (0x35006c) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_RECLAIM_CORPSE (0x35006e)

- Modern: 3473518 (0x35006e) · 4.3.4: 16438 (0x4036)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_COMPLETE_MOVIE (0x350070)

- Modern: 3473520 (0x350070) · 4.3.4: 16694 (0x4136)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_FACTION_AT_WAR (0x350071)

- Modern: 3473521 (0x350071) · 4.3.4: 1798 (0x706)
- Layout (after the opcode): `u32 u16`
- Size: 6 bytes (packed GUIDs not counted)

### CMSG_SET_FACTION_NOT_AT_WAR (0x350072)

- Modern: 3473522 (0x350072) · 4.3.4: —
- Layout (after the opcode): `u32 u16`
- Size: 6 bytes (packed GUIDs not counted)

### CMSG_SET_FACTION_INACTIVE (0x350073)

- Modern: 3473523 (0x350073) · 4.3.4: 3639 (0xe37)
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_SET_WATCHED_FACTION (0x350074)

- Modern: 3473524 (0x350074) · 4.3.4: 9268 (0x2434)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_DUEL_RESPONSE (0x350075)

- Modern: 3473525 (0x350075) · 4.3.4: —
- Layout (after the opcode): `u32 guid opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 1 · (6 unused)

### CMSG_UNLEARN_SKILL (0x350078)

- Modern: 3473528 (0x350078) · 4.3.4: 24838 (0x6106)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CANCEL_AUTO_REPEAT_SPELL (0x35007a)

- Modern: 3473530 (0x35007a) · 4.3.4: 27701 (0x6c35)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_FAR_SIGHT (0x35007b)

- Modern: 3473531 (0x35007b) · 4.3.4: 18485 (0x4835)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_SOCKET_GEMS (0x35007e)

- Modern: 3473534 (0x35007e) · 4.3.4: 12036 (0x2f04)
- Layout (after the opcode): `u32 guid loop[ guid ]`

### CMSG_REPAIR_ITEM (0x35007f)

- Modern: 3473535 (0x35007f) · 4.3.4: 10519 (0x2917)
- Layout (after the opcode): `u32 guid guid flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_GAME_OBJ_USE (0x350081)

- Modern: 3473537 (0x350081) · 4.3.4: 19991 (0x4e17)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GAME_OBJ_REPORT_USE (0x350082)

- Modern: 3473538 (0x350082) · 4.3.4: 18471 (0x4827)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CANCEL_TEMP_ENCHANTMENT (0x350085)

- Modern: 3473541 (0x350085) · 4.3.4: 27703 (0x6c37)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SET_TAXI_BENCHMARK_MODE (0x350086)

- Modern: 3473542 (0x350086) · 4.3.4: 17172 (0x4314)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_REPORT_PVP_PLAYER_AFK (0x350087)

- Modern: 3473543 (0x350087) · 4.3.4: 26420 (0x6734)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ALTER_APPEARANCE (0x350088)

- Modern: 3473544 (0x350088) · 4.3.4: 2324 (0x914)
- Layout (after the opcode): `u32*2 u8 u32*3 loop[ { u32*2 } ]`

### CMSG_OPT_OUT_OF_LOOT (0x350089)

- Modern: 3473545 (0x350089) · 4.3.4: 27414 (0x6b16)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_TOTEM_DESTROYED (0x35008b)

- Modern: 3473547 (0x35008b) · 4.3.4: 16903 (0x4207)
- Layout (after the opcode): `u32 u8 guid`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_DISMISS_CRITTER (0x35008c)

- Modern: 3473548 (0x35008c) · 4.3.4: 16935 (0x4227)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_INSPECT_ACHIEVEMENTS (0x350093)

- Modern: 3473555 (0x350093) · 4.3.4: 19751 (0x4d27)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_RAF_CLAIM_ACTIVITY_REWARD (0x350097)

- Modern: 3473559 (0x350097) · 4.3.4: —
- Layout (after the opcode): `u32 u64 u32`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_HEARTH_AND_RESURRECT (0x35009a)

- Modern: 3473562 (0x35009a) · 4.3.4: 19252 (0x4b34)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SAVE_EQUIPMENT_SET (0x35009d)

- Modern: 3473565 (0x35009d) · 4.3.4: 20263 (0x4f27)
- Layout (after the opcode): `u32 { u32 u64 u32*2 loop[ guid u32 ] loop[ u32 ] u32*4 u8*2 opt[ u8 ] flush opt[ u32 ] opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 160: 1 · 8 · 9 · (6 unused)

### CMSG_DELETE_EQUIPMENT_SET (0x35009e)

- Modern: 3473566 (0x35009e) · 4.3.4: —
- Layout (after the opcode): `u32 u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_INSTANCE_LOCK_RESPONSE (0x35009f)

- Modern: 3473567 (0x35009f) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_LOW_LEVEL_RAID1 (0x3500a6)

- Modern: 3473574 (0x3500a6) · 4.3.4: 17461 (0x4435)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_DECLINE_GUILD_INVITES (0x3500b1)

- Modern: 3473585 (0x3500b1) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_OVERRIDE_SCREEN_FLASH (0x3500b2)

- Modern: 3473586 (0x3500b2) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_BATTLEMASTER_JOIN (0x3500b4)

- Modern: 3473588 (0x3500b4) · 4.3.4: 30978 (0x7902)
- Layout (after the opcode): `u32 u64 u8 loop[ u32 ] guid u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 29: 1 · (7 unused)

### CMSG_BATTLEMASTER_JOIN_ARENA (0x3500b5)

- Modern: 3473589 (0x3500b5) · 4.3.4: 28700 (0x701c)
- Layout (after the opcode): `u32 guid u8*2`
- Size: 6 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8

### CMSG_BATTLEMASTER_JOIN_SKIRMISH (0x3500b6)

- Modern: 3473590 (0x3500b6) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8*2 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 8 · 1 · 1 · (6 unused)

### CMSG_BATTLEFIELD_PORT (0x3500bb)

- Modern: 3473595 (0x3500bb) · 4.3.4: 28954 (0x711a)
- Layout (after the opcode): `u32 { guid u32*2 u64 flush } flush`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused) · 1 · (7 unused)

### CMSG_REPOP_REQUEST (0x3500bc)

- Modern: 3473596 (0x3500bc) · 4.3.4: 25141 (0x6235)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_CLIENT_PORT_GRAVEYARD (0x3500bd)

- Modern: 3473597 (0x3500bd) · 4.3.4: 12318 (0x301e)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_SELECTION (0x3500be)

- Modern: 3473598 (0x3500be) · 4.3.4: 1286 (0x506)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_INSPECT (0x3500bf)

- Modern: 3473599 (0x3500bf) · 4.3.4: 2343 (0x927)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_CROWD_CONTROL_SPELL (0x3500c0)

- Modern: 3473600 (0x3500c0) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BLACK_MARKET_OPEN (0x3500c1)

- Modern: 3473601 (0x3500c1) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUEST_LOG_REMOVE_QUEST (0x3500c4)

- Modern: 3473604 (0x3500c4) · 4.3.4: 3350 (0xd16)
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_GET_ITEM_PURCHASE_DATA (0x3500c5)

- Modern: 3473605 (0x3500c5) · 4.3.4: 8710 (0x2206)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ITEM_PURCHASE_REFUND (0x3500c6)

- Modern: 3473606 (0x3500c6) · 4.3.4: 24884 (0x6134)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SELF_RES (0x3500c7)

- Modern: 3473607 (0x3500c7) · 4.3.4: 24853 (0x6115)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SET_ACTION_BAR_TOGGLES (0x3500c8)

- Modern: 3473608 (0x3500c8) · 4.3.4: 9478 (0x2506)
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_SIGN_PETITION (0x3500c9)

- Modern: 3473609 (0x3500c9) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_DECLINE_PETITION (0x3500ca)

- Modern: 3473610 (0x3500ca) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TURN_IN_PETITION (0x3500cb)

- Modern: 3473611 (0x3500cb) · 4.3.4: 2855 (0xb27)
- Layout (after the opcode): `u32 guid u32*5`
- Size: 24 bytes (packed GUIDs not counted)

### CMSG_MAIL_GET_LIST (0x3500cc)

- Modern: 3473612 (0x3500cc) · 4.3.4: 19767 (0x4d37)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_MAIL_TAKE_MONEY (0x3500cd)

- Modern: 3473613 (0x3500cd) · 4.3.4: 16436 (0x4034)
- Layout (after the opcode): `u32 guid u64*2`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_MAIL_TAKE_ITEM (0x3500ce)

- Modern: 3473614 (0x3500ce) · 4.3.4: 11014 (0x2b06)
- Layout (after the opcode): `u32 guid u64*2`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_QUERY_NEXT_MAIL_TIME (0x3500cf)

- Modern: 3473615 (0x3500cf) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_MAIL_MARK_AS_READ (0x3500d0)

- Modern: 3473616 (0x3500d0) · 4.3.4: 3079 (0xc07)
- Layout (after the opcode): `u32 guid u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_MAIL_CREATE_TEXT_ITEM (0x3500d1)

- Modern: 3473617 (0x3500d1) · 4.3.4: 2836 (0xb14)
- Layout (after the opcode): `u32 guid u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_EMOTE (0x3500d7)

- Modern: 3473623 (0x3500d7) · 4.3.4: 19494 (0x4c26)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_OPENING_CINEMATIC (0x3500d9)

- Modern: 3473625 (0x3500d9) · 4.3.4: 2582 (0xa16)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_NEXT_CINEMATIC_CAMERA (0x3500da)

- Modern: 3473626 (0x3500da) · 4.3.4: 8212 (0x2014)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_COMPLETE_CINEMATIC (0x3500db)

- Modern: 3473627 (0x3500db) · 4.3.4: 8470 (0x2116)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CONVERSATION_LINE_STARTED (0x3500dc)

- Modern: 3473628 (0x3500dc) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUEST_GIVER_CLOSE_QUEST (0x3500df)

- Modern: 3473631 (0x3500df) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_LEARN_TALENT (0x3500e8)

- Modern: 3473640 (0x3500e8) · 4.3.4: 774 (0x306)
- Layout (after the opcode): `u32*2 u16`
- Size: 10 bytes (packed GUIDs not counted)

### CMSG_LEARN_PREVIEW_TALENTS (0x3500e9)

- Modern: 3473641 (0x3500e9) · 4.3.4: 9237 (0x2415)
- Layout (after the opcode): `u32*3 loop[ { u32*2 } ]`

### CMSG_PET_LEARN_TALENT (0x3500ea)

- Modern: 3473642 (0x3500ea) · 4.3.4: 26405 (0x6725)
- Layout (after the opcode): `u32 guid u32 u16`
- Size: 10 bytes (packed GUIDs not counted)

### CMSG_LEARN_PREVIEW_TALENTS_PET (0x3500eb)

- Modern: 3473643 (0x3500eb) · 4.3.4: 28196 (0x6e24)
- Layout (after the opcode): `u32 guid u32 loop[ { u32*2 } ]`

### CMSG_SET_PRIMARY_TALENT_TREE (0x3500ed)

- Modern: 3473645 (0x3500ed) · 4.3.4: 17700 (0x4524)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CONTRIBUTION_LAST_UPDATE_REQUEST (0x3500f2)

- Modern: 3473650 (0x3500f2) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_SET_ACTION_BUTTON (0x3500f4)

- Modern: 3473652 (0x3500f4) · 4.3.4: 28422 (0x6f06)
- Layout (after the opcode): `u32*2 u8`
- Size: 9 bytes (packed GUIDs not counted)

### CMSG_SET_AMMO (0x3500f5)

- Modern: 3473653 (0x3500f5) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SHOWING_HELM (0x350103)

- Modern: 3473667 (0x350103) · 4.3.4: 1845 (0x735)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_SHOWING_CLOAK (0x350104)

- Modern: 3473668 (0x350104) · 4.3.4: 16693 (0x4135)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_QUEST_GIVER_STATUS_TRACKED_QUERY (0x350106)

- Modern: 3473670 (0x350106) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ guid ]`

### CMSG_WRAP_ITEM (0x360000)

- Modern: 3538944 (0x360000) · 4.3.4: 20230 (0x4f06)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused)

### CMSG_USE_EQUIPMENT_SET (0x360001)

- Modern: 3538945 (0x360001) · 4.3.4: —
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] loop[ guid u8*2 ] u64`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused); byte 5: 8 · 8; byte 9: 8 · 8; byte 13: 8 · 8; byte 17: 8 · 8; byte 21: 8 · 8; byte 25: 8 · 8; byte 29: 8 · 8; byte 33: 8 · 8; byte 37: 8 · 8; byte 41: 8 · 8; byte 45: 8 · 8; byte 49: 8 · 8; byte 53: 8 · 8; byte 57: 8 · 8; byte 61: 8 · 8; byte 65: 8 · 8; byte 69: 8 · 8; byte 73: 8 · 8; byte 77: 8 · 8

### CMSG_AUTOSTORE_BANK_ITEM (0x360002)

- Modern: 3538946 (0x360002) · 4.3.4: 1543 (0x607)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] u8*2`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused) · 8 · 8

### CMSG_AUTOBANK_ITEM (0x360003)

- Modern: 3538947 (0x360003) · 4.3.4: 9527 (0x2537)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] u8*3`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused) · 8 · 8 · 8

### CMSG_AUTO_EQUIP_ITEM (0x360004)

- Modern: 3538948 (0x360004) · 4.3.4: 17156 (0x4304)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] u8*2`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused) · 8 · 8

### CMSG_AUTO_STORE_BAG_ITEM (0x360005)

- Modern: 3538949 (0x360005) · 4.3.4: 566 (0x236)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] u8*3`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused) · 8 · 8 · 8

### CMSG_SWAP_ITEM (0x360006)

- Modern: 3538950 (0x360006) · 4.3.4: 25382 (0x6326)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] u8*4`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused) · 8 · 8 · 8 · 8

### CMSG_SWAP_INV_ITEM (0x360007)

- Modern: 3538951 (0x360007) · 4.3.4: 9748 (0x2614)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] u8*2`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused) · 8 · 8

### CMSG_SPLIT_ITEM (0x360008)

- Modern: 3538952 (0x360008) · 4.3.4: 3863 (0xf17)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] u8*4 u32`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused) · 8 · 8 · 8 · 8

### CMSG_AUTO_EQUIP_ITEM_SLOT (0x360009)

- Modern: 3538953 (0x360009) · 4.3.4: 18967 (0x4a17)
- Layout (after the opcode): `u32 bits(2) flush loop[ u8*2 ] guid u8`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused)

### CMSG_MOVE_START_FORWARD (0x370000)

- Modern: 3604480 (0x370000) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_BACKWARD (0x370001)

- Modern: 3604481 (0x370001) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_STOP (0x370002)

- Modern: 3604482 (0x370002) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_STRAFE_LEFT (0x370003)

- Modern: 3604483 (0x370003) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_STRAFE_RIGHT (0x370004)

- Modern: 3604484 (0x370004) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_STOP_STRAFE (0x370005)

- Modern: 3604485 (0x370005) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_JUMP (0x370006)

- Modern: 3604486 (0x370006) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_DOUBLE_JUMP (0x370007)

- Modern: 3604487 (0x370007) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_TURN_LEFT (0x370008)

- Modern: 3604488 (0x370008) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_TURN_RIGHT (0x370009)

- Modern: 3604489 (0x370009) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_STOP_TURN (0x37000a)

- Modern: 3604490 (0x37000a) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_PITCH_UP (0x37000b)

- Modern: 3604491 (0x37000b) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_PITCH_DOWN (0x37000c)

- Modern: 3604492 (0x37000c) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_STOP_PITCH (0x37000d)

- Modern: 3604493 (0x37000d) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_RUN_MODE (0x37000e)

- Modern: 3604494 (0x37000e) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_WALK_MODE (0x37000f)

- Modern: 3604495 (0x37000f) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_TELEPORT_ACK (0x370016)

- Modern: 3604502 (0x370016) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32*2`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_MOVE_FALL_LAND (0x370017)

- Modern: 3604503 (0x370017) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_SWIM (0x370018)

- Modern: 3604504 (0x370018) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_STOP_SWIM (0x370019)

- Modern: 3604505 (0x370019) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_TURN_RATE_CHEAT (0x370022)

- Modern: 3604514 (0x370022) · 4.3.4: —
- Layout (after the opcode): `u32 f32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_MOVE_SET_FACING (0x370025)

- Modern: 3604517 (0x370025) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_PITCH (0x370026)

- Modern: 3604518 (0x370026) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK (0x370027)

- Modern: 3604519 (0x370027) · 4.3.4: 30744 (0x7818)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK (0x370028)

- Modern: 3604520 (0x370028) · 4.3.4: 12822 (0x3216)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK (0x370029)

- Modern: 3604521 (0x370029) · 4.3.4: 31248 (0x7a10)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_ROOT_ACK (0x37002a)

- Modern: 3604522 (0x37002a) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_UNROOT_ACK (0x37002b)

- Modern: 3604523 (0x37002b) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_HEARTBEAT (0x37002c)

- Modern: 3604524 (0x37002c) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_KNOCK_BACK_ACK (0x37002e)

- Modern: 3604526 (0x37002e) · 4.3.4: 29212 (0x721c)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } flush opt[ f32*2 ]`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1; byte 57: 1 · (7 unused)

### CMSG_MOVE_HOVER_ACK (0x37002f)

- Modern: 3604527 (0x37002f) · 4.3.4: 13080 (0x3318)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_VEHICLE_REC_ID_ACK (0x370030)

- Modern: 3604528 (0x370030) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } u32`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_APPLY_MOVEMENT_FORCE_ACK (0x370031)

- Modern: 3604529 (0x370031) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } { guid f32*6 u32 f32 u32 bits(2) flush }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1; byte 95: 2 · (6 unused)

### CMSG_MOVE_REMOVE_MOVEMENT_FORCE_ACK (0x370032)

- Modern: 3604530 (0x370032) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } guid`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_REMOVE_MOVEMENT_FORCES (0x370033)

- Modern: 3604531 (0x370033) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SPLINE_DONE (0x370034)

- Modern: 3604532 (0x370034) · 4.3.4: 30990 (0x790e)
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FALL_RESET (0x370035)

- Modern: 3604533 (0x370035) · 4.3.4: 12554 (0x310a)
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_UPDATE_FALL_SPEED (0x370036)

- Modern: 3604534 (0x370036) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_TIME_SKIPPED (0x370037)

- Modern: 3604535 (0x370037) · 4.3.4: 31242 (0x7a0a)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_MOVE_FEATHER_FALL_ACK (0x370038)

- Modern: 3604536 (0x370038) · 4.3.4: 12560 (0x3110)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_WATER_WALK_ACK (0x370039)

- Modern: 3604537 (0x370039) · 4.3.4: 15104 (0x3b00)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_ENABLE_DOUBLE_JUMP_ACK (0x37003a)

- Modern: 3604538 (0x37003a) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK (0x37003d)

- Modern: 3604541 (0x37003d) · 4.3.4: 29200 (0x7210)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK (0x37003e)

- Modern: 3604542 (0x37003e) · 4.3.4: 31254 (0x7a16)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK (0x37003f)

- Modern: 3604543 (0x37003f) · 4.3.4: 29462 (0x7316)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_ENABLE_SWIM_TO_FLY_TRANS_ACK (0x370040)

- Modern: 3604544 (0x370040) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_CAN_TURN_WHILE_FALLING_ACK (0x370041)

- Modern: 3604545 (0x370041) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_IGNORE_MOVEMENT_FORCES_ACK (0x370042)

- Modern: 3604546 (0x370042) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_CAN_FLY_ACK (0x370043)

- Modern: 3604547 (0x370043) · 4.3.4: 30988 (0x790c)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x370044)

- Modern: 3604548 (0x370044) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_FLY (0x370045)

- Modern: 3604549 (0x370045) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_ASCEND (0x370046)

- Modern: 3604550 (0x370046) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_STOP_ASCEND (0x370047)

- Modern: 3604551 (0x370047) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK (0x37004a)

- Modern: 3604554 (0x37004a) · 4.3.4: 29460 (0x7314)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK (0x37004b)

- Modern: 3604555 (0x37004b) · 4.3.4: 12558 (0x310e)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_CHANGE_TRANSPORT (0x37004c)

- Modern: 3604556 (0x37004c) · 4.3.4: 12546 (0x3102)
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_START_DESCEND (0x37004d)

- Modern: 3604557 (0x37004d) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK (0x37004f)

- Modern: 3604559 (0x37004f) · 4.3.4: 12544 (0x3100)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_DISMISS_VEHICLE (0x370050)

- Modern: 3604560 (0x370050) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_CHANGE_VEHICLE_SEATS (0x370051)

- Modern: 3604561 (0x370051) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } guid u8`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_GRAVITY_DISABLE_ACK (0x370052)

- Modern: 3604562 (0x370052) · 4.3.4: 12568 (0x3118)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_GRAVITY_ENABLE_ACK (0x370053)

- Modern: 3604563 (0x370053) · 4.3.4: 28682 (0x700a)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_INERTIA_DISABLE_ACK (0x370054)

- Modern: 3604564 (0x370054) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_INERTIA_ENABLE_ACK (0x370055)

- Modern: 3604565 (0x370055) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_COLLISION_DISABLE_ACK (0x370056)

- Modern: 3604566 (0x370056) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_COLLISION_ENABLE_ACK (0x370057)

- Modern: 3604567 (0x370057) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_COLLISION_HEIGHT_ACK (0x370058)

- Modern: 3604568 (0x370058) · 4.3.4: 28948 (0x7114)
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } f32 u32 u8`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_SET_ACTIVE_MOVER (0x370059)

- Modern: 3604569 (0x370059) · 4.3.4: 13076 (0x3314)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TIME_SYNC_RESPONSE (0x37005a)

- Modern: 3604570 (0x37005a) · 4.3.4: 15116 (0x3b0c)
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_TIME_SYNC_RESPONSE_FAILED (0x37005b)

- Modern: 3604571 (0x37005b) · 4.3.4: 28938 (0x710a)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_TIME_SYNC_RESPONSE_DROPPED (0x37005c)

- Modern: 3604572 (0x37005c) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_TIME_ADJUSTMENT_RESPONSE (0x37005d)

- Modern: 3604573 (0x37005d) · 4.3.4: 14360 (0x3818)
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_DISCARDED_TIME_SYNC_ACKS (0x37005e)

- Modern: 3604574 (0x37005e) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_MOVE_SET_MOD_MOVEMENT_FORCE_MAGNITUDE_ACK (0x37005f)

- Modern: 3604575 (0x37005f) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_UPDATE_MISSILE_TRAJECTORY (0x370060)

- Modern: 3604576 (0x370060) · 4.3.4: 30750 (0x781e)
- Layout (after the opcode): `u32 guid guid u32*2 f32*8 flush opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ]`
- Bit fields (message of zeros, widths in order written): byte 46: 1 · (7 unused)

### CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE (0x370063)

- Modern: 3604579 (0x370063) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_MOVE_APPLY_INERTIA_ACK (0x37006b)

- Modern: 3604587 (0x37006b) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } u32*2`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_REMOVE_INERTIA_ACK (0x37006c)

- Modern: 3604588 (0x37006c) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } u32`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_ADD_IMPULSE_ACK (0x37006d)

- Modern: 3604589 (0x37006d) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } { guid f32*6 u32 f32 u32 bits(2) flush }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1; byte 95: 2 · (6 unused)

### CMSG_MOVE_SET_CAN_ADV_FLY_ACK (0x37006e)

- Modern: 3604590 (0x37006e) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLY (0x37006f)

- Modern: 3604591 (0x37006f) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_AIR_FRICTION_ACK (0x370070)

- Modern: 3604592 (0x370070) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_MAX_VEL_ACK (0x370071)

- Modern: 3604593 (0x370071) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_LIFT_COEFFICIENT_ACK (0x370072)

- Modern: 3604594 (0x370072) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_DOUBLE_JUMP_VEL_MOD_ACK (0x370073)

- Modern: 3604595 (0x370073) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_GLIDE_START_MIN_HEIGHT_ACK (0x370074)

- Modern: 3604596 (0x370074) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_ADD_IMPULSE_MAX_SPEED_ACK (0x370075)

- Modern: 3604597 (0x370075) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_BANKING_RATE_ACK (0x370076)

- Modern: 3604598 (0x370076) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32*2 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_PITCHING_RATE_DOWN_ACK (0x370077)

- Modern: 3604599 (0x370077) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32*2 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_PITCHING_RATE_UP_ACK (0x370078)

- Modern: 3604600 (0x370078) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32*2 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_TURN_VELOCITY_THRESHOLD_ACK (0x370079)

- Modern: 3604601 (0x370079) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32*2 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_SURFACE_FRICTION_ACK (0x37007a)

- Modern: 3604602 (0x37007a) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_OVER_MAX_DECELERATION_ACK (0x37007b)

- Modern: 3604603 (0x37007b) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_FACING_HEARTBEAT (0x37007c)

- Modern: 3604604 (0x37007c) · 4.3.4: —
- Layout (after the opcode): `u32 { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_ADV_FLYING_LAUNCH_SPEED_COEFFICIENT_ACK (0x37007d)

- Modern: 3604605 (0x37007d) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x370080)

- Modern: 3604608 (0x370080) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_ENABLE_FULL_SPEED_TURNING_ACK (0x370081)

- Modern: 3604609 (0x370081) · 4.3.4: —
- Layout (after the opcode): `u32 { { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 52: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_CONNECT_TO_FAILED (0x390000)

- Modern: 3735552 (0x390000) · 4.3.4: 9523 (0x2533)
- Layout (after the opcode): `u32 u8 u32`
- Size: 9 bytes (packed GUIDs not counted)

### CMSG_ADDON_LIST (0x390004)

- Modern: 3735556 (0x390004) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32 u16 u8 u32 loop[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ]`

### CMSG_SET_ROLE (0x390005)

- Modern: 3735557 (0x390005) · 4.3.4: —
- Layout (after the opcode): `u32 flush guid u8 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (6 unused) · 1

### CMSG_INITIATE_ROLE_POLL (0x390006)

- Modern: 3735558 (0x390006) · 4.3.4: —
- Layout (after the opcode): `u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_REQUEST_BATTLEFIELD_STATUS (0x390008)

- Modern: 3735560 (0x390008) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_START_WAR_GAME (0x39000a)

- Modern: 3735562 (0x39000a) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32 u16 } u64 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_START_SPECTATOR_WAR_GAME (0x39000b)

- Modern: 3735563 (0x39000b) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32 u16 } { guid u32 u16 } u64 flush`
- Bit fields (message of zeros, widths in order written): byte 26: 1 · (7 unused)

### CMSG_ACCEPT_WARGAME_INVITE (0x39000c)

- Modern: 3735564 (0x39000c) · 4.3.4: —
- Layout (after the opcode): `u32 guid u64 flush`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · (7 unused)

### CMSG_REQUEST_RATED_PVP_INFO (0x39000f)

- Modern: 3735567 (0x39000f) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_DB_QUERY_BULK (0x390010)

- Modern: 3735568 (0x390010) · 4.3.4: —
- Layout (after the opcode): `u32*2 u8 bits(5) flush loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 6: 13 · (3 unused)

### CMSG_HOTFIX_REQUEST (0x390011)

- Modern: 3735569 (0x390011) · 4.3.4: —
- Layout (after the opcode): `u32*4 loop[ u32 ]`

### CMSG_GENERATE_RANDOM_CHARACTER_NAME (0x390013)

- Modern: 3735571 (0x390013) · 4.3.4: 9235 (0x2413)
- Layout (after the opcode): `u32 u8*2`
- Size: 6 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8

### CMSG_ENUM_CHARACTERS (0x390014)

- Modern: 3735572 (0x390014) · 4.3.4: 1282 (0x502)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REORDER_CHARACTERS (0x390015)

- Modern: 3735573 (0x390015) · 4.3.4: 1427 (0x593)
- Layout (after the opcode): `u32 u8 flush loop[ guid u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · (7 unused)

### CMSG_PLAYER_LOGIN (0x390016)

- Modern: 3735574 (0x390016) · 4.3.4: 1457 (0x5b1)
- Layout (after the opcode): `u32 guid { f32 }`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_WARDEN3_DATA (0x390018)

- Modern: 3735576 (0x390018) · 4.3.4: —
- Layout (after the opcode): `u32*3 bytes`

### CMSG_GET_PVP_OPTIONS_ENABLED (0x39001a)

- Modern: 3735578 (0x39001a) · 4.3.4: 9377 (0x24a1)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_COMMENTATOR_START_WARGAME (0x39001b)

- Modern: 3735579 (0x39001b) · 4.3.4: 9632 (0x25a0)
- Layout (after the opcode): `u32 { opt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] flush u64 opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 6 · 6 · 1 · (3 unused)

### CMSG_COMMENTATOR_ENABLE (0x39001c)

- Modern: 3735580 (0x39001c) · 4.3.4: 2823 (0xb07)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_COMMENTATOR_GET_MAP_INFO (0x39001d)

- Modern: 3735581 (0x39001d) · 4.3.4: 38 (0x26)
- Layout (after the opcode): `u32 bits(6) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 6 · (2 unused)

### CMSG_COMMENTATOR_GET_PLAYER_INFO (0x39001e)

- Modern: 3735582 (0x39001e) · 4.3.4: 3348 (0xd14)
- Layout (after the opcode): `u32*3 u16 u8`
- Size: 15 bytes (packed GUIDs not counted)

### CMSG_COMMENTATOR_GET_PLAYER_COOLDOWNS (0x39001f)

- Modern: 3735583 (0x39001f) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32 loop[ { u32*2 } ]`

### CMSG_COMMENTATOR_ENTER_INSTANCE (0x390020)

- Modern: 3735584 (0x390020) · 4.3.4: 16645 (0x4105)
- Layout (after the opcode): `u32*3 u16 u8 u64 u32`
- Size: 27 bytes (packed GUIDs not counted)

### CMSG_COMMENTATOR_EXIT_INSTANCE (0x390021)

- Modern: 3735585 (0x390021) · 4.3.4: 24886 (0x6136)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_PARTY_JOIN_UPDATES (0x390023)

- Modern: 3735587 (0x390023) · 4.3.4: —
- Layout (after the opcode): `u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_LOADING_SCREEN_NOTIFY (0x390024)

- Modern: 3735588 (0x390024) · 4.3.4: 9250 (0x2422)
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_WORLD_PORT_RESPONSE (0x390025)

- Modern: 3735589 (0x390025) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SEND_MAIL (0x390026)

- Modern: 3735590 (0x390026) · 4.3.4: 1315 (0x523)
- Layout (after the opcode): `u32 { guid u32 u64*2 u8*3 bits(3) bits(5) flush opt[ bytes ] opt[ bytes ] opt[ bytes ] loop[ { u8 guid } ] }`
- Bit fields (message of zeros, widths in order written): byte 24: 9 · 9 · 11 · 5 · (6 unused)

### CMSG_ACCEPT_GUILD_INVITE (0x390029)

- Modern: 3735593 (0x390029) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_DECLINE_INVITATION (0x39002a)

- Modern: 3735594 (0x39002a) · 4.3.4: 12849 (0x3231)
- Layout (after the opcode): `u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_PARTY_INVITE (0x390030)

- Modern: 3735600 (0x390030) · 4.3.4: —
- Layout (after the opcode): `u32 flush { u8*2 flush u32 guid opt[ bytes ] opt[ bytes ] } opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused) · 9 · 9 · (6 unused)

### CMSG_PARTY_INVITE_RESPONSE (0x390032)

- Modern: 3735602 (0x390032) · 4.3.4: —
- Layout (after the opcode): `u32 opt[ u8 ] opt[ u8 ] flush opt[ u8 ] opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 1 · (5 unused)

### CMSG_GUILD_INVITE_BY_NAME (0x390034)

- Modern: 3735604 (0x390034) · 4.3.4: —
- Layout (after the opcode): `u32 u8 flush opt[ bytes ] opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · 1 · (6 unused)

### CMSG_DF_PROPOSAL_RESPONSE (0x390035)

- Modern: 3735605 (0x390035) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush } u64 u32 flush`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused); byte 33: 1 · (7 unused)

### CMSG_DF_JOIN (0x390037)

- Modern: 3735607 (0x390037) · 4.3.4: —
- Layout (after the opcode): `u32 opt[ u8 ] opt[ u8 ] flush u8 u32 opt[ u8 ] loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 1 · (5 unused) · 8

### CMSG_LFG_LIST_LEAVE (0x390038)

- Modern: 3735608 (0x390038) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush }`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused)

### CMSG_LFG_LIST_GET_STATUS (0x390039)

- Modern: 3735609 (0x390039) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LFG_LIST_SEARCH (0x39003a)

- Modern: 3735610 (0x39003a) · 4.3.4: —
- Layout (after the opcode): `u32 { bits(5) opt[ u8 ] flush loop[ { loop[ opt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] ] flush loop[ bytes ] } ] u32*9 u8 u32*2 loop[ u32 ] loop[ u32 ] loop[ u32 ] loop[ guid ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 5 · 1 · (2 unused)

### CMSG_LFG_LIST_APPLY_TO_GROUP (0x39003b)

- Modern: 3735611 (0x39003b) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush } u32 u8*2 flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused); byte 25: 8 · 8

### CMSG_LFG_LIST_CANCEL_APPLICATION (0x39003c)

- Modern: 3735612 (0x39003c) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush }`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused)

### CMSG_LFG_LIST_DECLINE_APPLICANT (0x39003d)

- Modern: 3735613 (0x39003d) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush } { guid u32*2 u64 flush }`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused); byte 39: 1 · (7 unused)

### CMSG_LFG_LIST_INVITE_APPLICANT (0x39003e)

- Modern: 3735614 (0x39003e) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush } u32 loop[ { guid u8 } ] { guid u32*2 u64 flush }`

### CMSG_LFG_LIST_INVITE_RESPONSE (0x39003f)

- Modern: 3735615 (0x39003f) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush } flush`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused) · 1 · (7 unused)

### CMSG_DF_LEAVE (0x390040)

- Modern: 3735616 (0x390040) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u32*2 u64 flush }`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused)

### CMSG_DF_GET_SYSTEM_INFO (0x390041)

- Modern: 3735617 (0x390041) · 4.3.4: 1042 (0x412)
- Layout (after the opcode): `u32 opt[ u8 ] flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)

### CMSG_DF_GET_JOIN_STATUS (0x390042)

- Modern: 3735618 (0x390042) · 4.3.4: 9601 (0x2581)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_DF_SET_ROLES (0x390043)

- Modern: 3735619 (0x390043) · 4.3.4: —
- Layout (after the opcode): `u32 flush u8 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused) · 8

### CMSG_DF_BOOT_PLAYER_VOTE (0x390044)

- Modern: 3735620 (0x390044) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_DF_TELEPORT (0x390045)

- Modern: 3735621 (0x390045) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_SET_EVERYONE_IS_ASSISTANT (0x390046)

- Modern: 3735622 (0x390046) · 4.3.4: 9520 (0x2530)
- Layout (after the opcode): `u32 opt[ u8 ] flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)

### CMSG_DF_READY_CHECK_RESPONSE (0x390048)

- Modern: 3735624 (0x390048) · 4.3.4: —
- Layout (after the opcode): `u32 opt[ u8 ] flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)

### CMSG_BATTLE_PET_REQUEST_JOURNAL_LOCK (0x39004d)

- Modern: 3735629 (0x39004d) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_REQUEST_JOURNAL (0x39004e)

- Modern: 3735630 (0x39004e) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_SUMMON (0x390053)

- Modern: 3735635 (0x390053) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_SET_BATTLE_SLOT (0x390057)

- Modern: 3735639 (0x390057) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_SET_FLAGS (0x39005a)

- Modern: 3735642 (0x39005a) · 4.3.4: —
- Layout (after the opcode): `u32 guid u16 bits(2) flush`
- Bit fields (message of zeros, widths in order written): byte 6: 2 · (6 unused)

### CMSG_MOUNT_SET_FAVORITE (0x39005c)

- Modern: 3735644 (0x39005c) · 4.3.4: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_COLLECTION_ITEM_SET_FAVORITE (0x39005d)

- Modern: 3735645 (0x39005d) · 4.3.4: —
- Layout (after the opcode): `u32*3 flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### CMSG_DO_READY_CHECK (0x39005e)

- Modern: 3735646 (0x39005e) · 4.3.4: —
- Layout (after the opcode): `u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_READY_CHECK_RESPONSE (0x39005f)

- Modern: 3735647 (0x39005f) · 4.3.4: —
- Layout (after the opcode): `u32 opt[ u8 ] flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)

### CMSG_CREATE_CHARACTER (0x39006e)

- Modern: 3735662 (0x39006e) · 4.3.4: 18998 (0x4a36)
- Layout (after the opcode): `u32 { bits(6) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush u8*3 u32*2 opt[ bytes ] opt[ u32 ] loop[ u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 6 · 1 · 1 · 1 · 1 · (6 unused) · 8 · 8 · 8

### CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT (0x390070)

- Modern: 3735664 (0x390070) · 4.3.4: 9473 (0x2501)
- Layout (after the opcode): `u32 { { u32 f32*4 u32 } guid u32*3 { u32 flush loop[ u64 u8 bits(4) flush opt[ bytes ] ] opt[ u32 ] } u8 bits(2) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ flush ] { u32 loop[ u64 guid opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 bits(4) flush opt[ u64 ] opt[ guid ] opt[ u32 u16 u8 ] opt[ u32 ] opt[ bytes ] ] } opt[ bytes ] opt[ { u64 u8 bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] } ] opt[ u64*2 u8 flush opt[ bytes ] ] opt[ guid u8 flush opt[ bytes ] ] opt[ bits(7) flush guid opt[ bytes ] ] opt[ { { guid u32*2 u64 flush } u32 u8 guid guid guid guid guid u8 bits(2) opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] alt[ u8 ] flush opt[ bytes ] opt[ bytes ] opt[ bytes ] } ] opt[ { guid u32*2 u64 flush } u8 flush opt[ bytes ] ] opt[ u64*2 guid u8 bits(4) flush opt[ bytes ] ] opt[ bits(7) flush guid opt[ bytes ] ] }`
- Bit fields (message of zeros, widths in order written): byte 44: 1 · (7 unused) · 10 · (14 unused)

### CMSG_SUPPORT_TICKET_SUBMIT_BUG (0x390071)

- Modern: 3735665 (0x390071) · 4.3.4: 9504 (0x2520)
- Layout (after the opcode): `u32 { u32 f32*4 u32 } u8 bits(2) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 26: 10 · (6 unused)

### CMSG_SUPPORT_TICKET_SUBMIT_SUGGESTION (0x390072)

- Modern: 3735666 (0x390072) · 4.3.4: —
- Layout (after the opcode): `u32 { u32 f32*4 u32 } u8 bits(2) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 26: 10 · (6 unused)

### CMSG_PARTY_UNINVITE (0x390073)

- Modern: 3735667 (0x390073) · 4.3.4: —
- Layout (after the opcode): `u32 { u8 opt[ u8 ] flush guid opt[ u8 ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 9 · (6 unused)

### CMSG_SET_LOOT_METHOD (0x390074)

- Modern: 3735668 (0x390074) · 4.3.4: 12068 (0x2f24)
- Layout (after the opcode): `u32 flush u8 guid u32 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused) · 8

### CMSG_LEAVE_GROUP (0x390075)

- Modern: 3735669 (0x390075) · 4.3.4: —
- Layout (after the opcode): `u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_SET_PARTY_LEADER (0x390076)

- Modern: 3735670 (0x390076) · 4.3.4: —
- Layout (after the opcode): `u32 flush guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (6 unused) · 1

### CMSG_MINIMAP_PING (0x390077)

- Modern: 3735671 (0x390077) · 4.3.4: —
- Layout (after the opcode): `u32 flush f32*2 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_CHANGE_SUB_GROUP (0x390078)

- Modern: 3735672 (0x390078) · 4.3.4: —
- Layout (after the opcode): `u32 guid u8 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 1 · (7 unused)

### CMSG_SWAP_SUB_GROUPS (0x390079)

- Modern: 3735673 (0x390079) · 4.3.4: —
- Layout (after the opcode): `u32 flush guid guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (6 unused) · 1

### CMSG_CONVERT_RAID (0x39007a)

- Modern: 3735674 (0x39007a) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_SET_ASSISTANT_LEADER (0x39007b)

- Modern: 3735675 (0x39007b) · 4.3.4: 24613 (0x6025)
- Layout (after the opcode): `u32 opt[ u8 ] flush guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)

### CMSG_UPDATE_RAID_TARGET (0x39007c)

- Modern: 3735676 (0x39007c) · 4.3.4: —
- Layout (after the opcode): `u32 flush guid u8 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (6 unused) · 1

### CMSG_SET_PARTY_ASSIGNMENT (0x39007d)

- Modern: 3735677 (0x39007d) · 4.3.4: —
- Layout (after the opcode): `u32 opt[ u8 ] flush u8 guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused) · 8

### CMSG_SILENCE_PARTY_TALKER (0x39007e)

- Modern: 3735678 (0x39007e) · 4.3.4: —
- Layout (after the opcode): `u32 opt[ u8 ] flush guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)

### CMSG_REQUEST_PARTY_MEMBER_STATS (0x39007f)

- Modern: 3735679 (0x39007f) · 4.3.4: 3076 (0xc04)
- Layout (after the opcode): `u32 flush guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (6 unused) · 1

### CMSG_RANDOM_ROLL (0x390080)

- Modern: 3735680 (0x390080) · 4.3.4: —
- Layout (after the opcode): `u32 flush u32*2 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_MAIL_RETURN_TO_SENDER (0x390081)

- Modern: 3735681 (0x390081) · 4.3.4: 2070 (0x816)
- Layout (after the opcode): `u32 u64 guid`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_QUERY_SCENARIO_POI (0x390082)

- Modern: 3735682 (0x390082) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ u32 ]`

### CMSG_TOGGLE_DIFFICULTY (0x390083)

- Modern: 3735683 (0x390083) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ADD_BATTLENET_FRIEND (0x390086)

- Modern: 3735686 (0x390086) · 4.3.4: —
- Layout (after the opcode): `u32 u64 u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · (7 unused)

### CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT (0x39008c)

- Modern: 3735692 (0x39008c) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_CORPSE_TRANSPORT (0x39008d)

- Modern: 3735693 (0x39008d) · 4.3.4: —
- Layout (after the opcode): `u32 guid guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CAN_DUEL (0x39008e)

- Modern: 3735694 (0x39008e) · 4.3.4: —
- Layout (after the opcode): `u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_UPDATE_CLIENT_SETTINGS (0x390090)

- Modern: 3735696 (0x390090) · 4.3.4: —
- Layout (after the opcode): `u32 { f32 }`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_RESET_INSTANCES (0x390094)

- Modern: 3735700 (0x390094) · 4.3.4: 28180 (0x6e14)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SUMMON_RESPONSE (0x390096)

- Modern: 3735702 (0x390096) · 4.3.4: 28455 (0x6f27)
- Layout (after the opcode): `u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_COMPLAINT (0x390098)

- Modern: 3735704 (0x390098) · 4.3.4: 1063 (0x427)
- Layout (after the opcode): `u32 u8 { guid u32*2 } opt[ u64 ] opt[ u64*2 ] opt[ { u32*2 u8 bits(4) flush opt[ bytes ] } ]`

### CMSG_CALENDAR_GET (0x39009b)

- Modern: 3735707 (0x39009b) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_GET_EVENT (0x39009c)

- Modern: 3735708 (0x39009c) · 4.3.4: 25622 (0x6416)
- Layout (after the opcode): `u32 u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_COMMUNITY_INVITE (0x39009d)

- Modern: 3735709 (0x39009d) · 4.3.4: —
- Layout (after the opcode): `u32 u64 u8*3`
- Size: 15 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_INVITE (0x39009e)

- Modern: 3735710 (0x39009e) · 4.3.4: —
- Layout (after the opcode): `u32 { u64*3 u8 opt[ u8 ] flush opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 26: 9 · 1 · 1 · (5 unused)

### CMSG_CALENDAR_REMOVE_INVITE (0x39009f)

- Modern: 3735711 (0x39009f) · 4.3.4: —
- Layout (after the opcode): `u32 guid u64*3`
- Size: 28 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_RSVP (0x3900a0)

- Modern: 3735712 (0x3900a0) · 4.3.4: —
- Layout (after the opcode): `u32 u64*2 u8`
- Size: 21 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_STATUS (0x3900a1)

- Modern: 3735713 (0x3900a1) · 4.3.4: —
- Layout (after the opcode): `u32 guid u64*3 u8`
- Size: 29 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_MODERATOR_STATUS (0x3900a2)

- Modern: 3735714 (0x3900a2) · 4.3.4: —
- Layout (after the opcode): `u32 guid u64*3 u8`
- Size: 29 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_REMOVE_EVENT (0x3900a3)

- Modern: 3735715 (0x3900a3) · 4.3.4: 26166 (0x6636)
- Layout (after the opcode): `u32 u64*3 u32`
- Size: 32 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_COPY_EVENT (0x3900a4)

- Modern: 3735716 (0x3900a4) · 4.3.4: 519 (0x207)
- Layout (after the opcode): `u32 u64*3 u32`
- Size: 32 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_COMPLAIN (0x3900a5)

- Modern: 3735717 (0x3900a5) · 4.3.4: 19510 (0x4c36)
- Layout (after the opcode): `u32 guid u64*2`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_GET_NUM_PENDING (0x3900a6)

- Modern: 3735718 (0x3900a6) · 4.3.4: 19717 (0x4d05)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_EVENT_SIGN_UP (0x3900a7)

- Modern: 3735719 (0x3900a7) · 4.3.4: 26118 (0x6606)
- Layout (after the opcode): `u32 u64*2 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_CALENDAR_ADD_EVENT (0x3900a9)

- Modern: 3735721 (0x3900a9) · 4.3.4: 1830 (0x726)
- Layout (after the opcode): `u32 { u64 u8 u32*4 u8*2 bits(3) flush loop[ guid u8*2 opt[ u8 ] opt[ u8 ] flush opt[ guid ] opt[ u64 ] opt[ u64 ] ] opt[ bytes ] opt[ bytes ] } u32`
- Bit fields (message of zeros, widths in order written): byte 27: 8 · 11 · (5 unused)

### CMSG_CALENDAR_UPDATE_EVENT (0x3900aa)

- Modern: 3735722 (0x3900aa) · 4.3.4: 8468 (0x2114)
- Layout (after the opcode): `u32 { u64*3 u8 u32*2 u16 u8*2 bits(3) flush opt[ bytes ] opt[ bytes ] } u32`
- Bit fields (message of zeros, widths in order written): byte 37: 8 · 11 · (5 unused)

### CMSG_KEEP_ALIVE (0x3900ab)

- Modern: 3735723 (0x3900ab) · 4.3.4: 21 (0x15)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_WHO_IS (0x3900ac)

- Modern: 3735724 (0x3900ac) · 4.3.4: 27397 (0x6b05)
- Layout (after the opcode): `u32 bits(6) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 6 · (2 unused)

### CMSG_WHO (0x3900ad)

- Modern: 3735725 (0x3900ad) · 4.3.4: 27669 (0x6c15)
- Layout (after the opcode): `u32 bits(4) opt[ u8 ] flush { u32*2 u64 u32 bits(6) opt[ u8 ] alt[ u8 ] opt[ u8 ] bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] bits(3) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush loop[ bits(7) flush opt[ bytes ] ] opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ u32*3 ] } u32 u8 loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 4 · 1 · (3 unused); byte 23: (3 unused) · 3 · (6 unused) · 3 · (4 unused) · 3 · (6 unused) · 3 · 3 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_SET_DUNGEON_DIFFICULTY (0x3900ae)

- Modern: 3735726 (0x3900ae) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_RESURRECT_RESPONSE (0x3900af)

- Modern: 3735727 (0x3900af) · 4.3.4: 26663 (0x6827)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_PET_RENAME (0x3900b0)

- Modern: 3735728 (0x3900b0) · 4.3.4: 25606 (0x6406)
- Layout (after the opcode): `u32 { guid u32 u8 loop[ opt[ u8 ] alt[ u8 ] ] flush opt[ opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ bytes ] ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 8: 8 · 1 · (7 unused)

### CMSG_BUG_REPORT (0x3900b1)

- Modern: 3735729 (0x3900b1) · 4.3.4: —
- Layout (after the opcode): `u32 { u8 bits(4) opt[ u8 ] alt[ u8 ] bits(2) flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 12 · 10 · (1 unused)

### CMSG_SET_SAVED_INSTANCE_EXTEND (0x3900b2)

- Modern: 3735730 (0x3900b2) · 4.3.4: 26374 (0x6706)
- Layout (after the opcode): `u32*3 flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### CMSG_SET_PLAYER_DECLINED_NAMES (0x3900b4)

- Modern: 3735732 (0x3900b4) · 4.3.4: 25366 (0x6316)
- Layout (after the opcode): `u32 { guid loop[ opt[ u8 ] alt[ u8 ] ] flush opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 4: 7 · 7 · 7 · 7 · 7 · (5 unused)

### CMSG_QUERY_REALM_NAME (0x3900b5)

- Modern: 3735733 (0x3900b5) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUERY_GUILD_INFO (0x3900b6)

- Modern: 3735734 (0x3900b6) · 4.3.4: 17446 (0x4426)
- Layout (after the opcode): `u32 guid guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CHAR_CUSTOMIZE (0x3900b7)

- Modern: 3735735 (0x3900b7) · 4.3.4: 11316 (0x2c34)
- Layout (after the opcode): `u32 guid u8 u32 loop[ { u32*2 } ] bits(6) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 9: 6 · (2 unused)

### CMSG_GM_TICKET_GET_SYSTEM_STATUS (0x3900b9)

- Modern: 3735737 (0x3900b9) · 4.3.4: 16901 (0x4205)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GM_TICKET_GET_CASE_STATUS (0x3900ba)

- Modern: 3735738 (0x3900ba) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GM_TICKET_ACKNOWLEDGE_SURVEY (0x3900bb)

- Modern: 3735739 (0x3900bb) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CHAR_RACE_OR_FACTION_CHANGE (0x3900bd)

- Modern: 3735741 (0x3900bd) · 4.3.4: —
- Layout (after the opcode): `u32 { bits(6) flush guid u8*3 u32 opt[ bytes ] loop[ { u32*2 } ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 6 · (1 unused); byte 5: 8 · 8 · 8

### CMSG_ACCOUNT_STORE_BEGIN_PURCHASE_OR_REFUND (0x3900bf)

- Modern: 3735743 (0x3900bf) · 4.3.4: —
- Layout (after the opcode): `u32*2 u8 u32`
- Size: 13 bytes (packed GUIDs not counted)

### CMSG_SUBMIT_USER_FEEDBACK (0x3900c0)

- Modern: 3735744 (0x3900c0) · 4.3.4: —
- Layout (after the opcode): `u32 { u32 f32*4 u32 } { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] } opt[ u8 ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 26: (22 unused) · 1 · 1 · 1 · (7 unused)

### CMSG_REQUEST_ACCOUNT_DATA (0x3900c1)

- Modern: 3735745 (0x3900c1) · 4.3.4: 25861 (0x6505)
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_UPDATE_ACCOUNT_DATA (0x3900c2)

- Modern: 3735746 (0x3900c2) · 4.3.4: 18230 (0x4736)
- Layout (after the opcode): `u32 u64 u32 guid u32*2 bytes`

### CMSG_SERVER_TIME_OFFSET_REQUEST (0x3900c9)

- Modern: 3735753 (0x3900c9) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CHAR_DELETE (0x3900ca)

- Modern: 3735754 (0x3900ca) · 4.3.4: 25637 (0x6425)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LOW_LEVEL_RAID2 (0x3900ce)

- Modern: 3735758 (0x3900ce) · 4.3.4: 1334 (0x536)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_INSPECT_PVP (0x3900d0)

- Modern: 3735760 (0x3900d0) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_ARENA_TEAM (0x3900d1)

- Modern: 3735761 (0x3900d1) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUEST_POI_QUERY (0x3900df)

- Modern: 3735775 (0x3900df) · 4.3.4: 16439 (0x4037)
- Layout (after the opcode): `u32*2 loop[ u32 ]`

### CMSG_ARENA_TEAM_ROSTER (0x3900e5)

- Modern: 3735781 (0x3900e5) · 4.3.4: 28471 (0x6f37)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_ARENA_TEAM_ACCEPT (0x3900e6)

- Modern: 3735782 (0x3900e6) · 4.3.4: 10789 (0x2a25)
- Layout (after the opcode): `u32 guid guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ARENA_TEAM_DECLINE (0x3900e7)

- Modern: 3735783 (0x3900e7) · 4.3.4: 26917 (0x6925)
- Layout (after the opcode): `u32 guid guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ARENA_TEAM_LEAVE (0x3900e8)

- Modern: 3735784 (0x3900e8) · 4.3.4: 3606 (0xe16)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_ARENA_TEAM_REMOVE (0x3900e9)

- Modern: 3735785 (0x3900e9) · 4.3.4: 12037 (0x2f05)
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_ARENA_TEAM_DISBAND (0x3900ea)

- Modern: 3735786 (0x3900ea) · 4.3.4: 25860 (0x6504)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_ARENA_TEAM_LEADER (0x3900eb)

- Modern: 3735787 (0x3900eb) · 4.3.4: 16900 (0x4204)
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GET_ACCOUNT_CHARACTER_LIST (0x3900ec)

- Modern: 3735788 (0x3900ec) · 4.3.4: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_LIVE_REGION_GET_ACCOUNT_CHARACTER_LIST (0x3900ed)

- Modern: 3735789 (0x3900ed) · 4.3.4: —
- Layout (after the opcode): `u32 { u32 u8*2 bits(6) flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 6: 8 · 9 · 6 · (1 unused)

### CMSG_LIVE_REGION_CHARACTER_COPY (0x3900ee)

- Modern: 3735790 (0x3900ee) · 4.3.4: —
- Layout (after the opcode): `u32 { u32 u8 u32 guid bits(6) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 13: 6 · 9 · (1 unused)

### CMSG_LIVE_REGION_ACCOUNT_RESTORE (0x3900ef)

- Modern: 3735791 (0x3900ef) · 4.3.4: —
- Layout (after the opcode): `u32 { u32 u8 guid u8 bits(6) flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 9: 9 · 6 · (1 unused)

### CMSG_BATTLE_PAY_GET_PRODUCT_LIST (0x3900f1)

- Modern: 3735793 (0x3900f1) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_GET_PURCHASE_LIST (0x3900f2)

- Modern: 3735794 (0x3900f2) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_RENAME_REQUEST (0x3900f6)

- Modern: 3735798 (0x3900f6) · 4.3.4: 8999 (0x2327)
- Layout (after the opcode): `u32 guid bits(6) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 4: 6 · (2 unused)

### CMSG_SHOW_TRADE_SKILL (0x3900f7)

- Modern: 3735799 (0x3900f7) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32*2`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_TO_TARGET (0x3900f8)

- Modern: 3735800 (0x3900f8) · 4.3.4: —
- Layout (after the opcode): `u32*2 u64 guid u32`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_UPGRADE_MANUAL_UNREVOKE_REQUEST (0x3900f9)

- Modern: 3735801 (0x3900f9) · 4.3.4: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_UPGRADE_START (0x3900fa)

- Modern: 3735802 (0x3900fa) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_CHECK_UPGRADE (0x3900fb)

- Modern: 3735803 (0x3900fb) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_SET_GUILD_MASTER (0x3900fd)

- Modern: 3735805 (0x3900fd) · 4.3.4: 12340 (0x3034)
- Layout (after the opcode): `u32 u8 flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · (7 unused)

### CMSG_PETITION_RENAME_GUILD (0x3900fe)

- Modern: 3735806 (0x3900fe) · 4.3.4: —
- Layout (after the opcode): `u32 guid bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 4: 7 · (1 unused)

### CMSG_REQUEST_RAID_INFO (0x3900ff)

- Modern: 3735807 (0x3900ff) · 4.3.4: 12070 (0x2f26)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_START_PURCHASE (0x390100)

- Modern: 3735808 (0x390100) · 4.3.4: —
- Layout (after the opcode): `u32 { u32*2 guid bits(6) opt[ u8 ] alt[ u8 ] bits(4) bits(7) flush opt[ bytes ] opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 12: 6 · 12; byte 14: 7 · (7 unused)

### CMSG_BATTLE_PAY_CONFIRM_PURCHASE_RESPONSE (0x390101)

- Modern: 3735809 (0x390101) · 4.3.4: —
- Layout (after the opcode): `u32 flush u32 u64`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_BATTLE_PAY_ACK_FAILED_RESPONSE (0x390102)

- Modern: 3735810 (0x390102) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SEND_CONTACT_LIST (0x390104)

- Modern: 3735812 (0x390104) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_ADD_FRIEND (0x390105)

- Modern: 3735813 (0x390105) · 4.3.4: 25895 (0x6527)
- Layout (after the opcode): `u32 { u8*2 bits(2) flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · 10 · (5 unused)

### CMSG_DEL_FRIEND (0x390106)

- Modern: 3735814 (0x390106) · 4.3.4: 27157 (0x6a15)
- Layout (after the opcode): `u32 { u32 guid }`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SET_CONTACT_NOTES (0x390107)

- Modern: 3735815 (0x390107) · 4.3.4: 24885 (0x6135)
- Layout (after the opcode): `u32 { u32 guid } u8 bits(2) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 8: 10 · (6 unused)

### CMSG_BATTLENET_CHALLENGE_RESPONSE (0x390108)

- Modern: 3735816 (0x390108) · 4.3.4: —
- Layout (after the opcode): `u32*2 bits(3) opt[ bits(6) ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 3 · (5 unused)

### CMSG_ADD_IGNORE (0x390109)

- Modern: 3735817 (0x390109) · 4.3.4: 18214 (0x4726)
- Layout (after the opcode): `u32 u8 flush guid opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · (7 unused)

### CMSG_DEL_IGNORE (0x39010a)

- Modern: 3735818 (0x39010a) · 4.3.4: 27942 (0x6d26)
- Layout (after the opcode): `u32 { u32 guid }`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SET_RAID_DIFFICULTY (0x390110)

- Modern: 3735824 (0x390110) · 4.3.4: —
- Layout (after the opcode): `u32*2 u8`
- Size: 9 bytes (packed GUIDs not counted)

### CMSG_TUTORIAL (0x390111)

- Modern: 3735825 (0x390111) · 4.3.4: —
- Layout (after the opcode): `u32 bits(2) flush opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused)

### CMSG_ENUM_CHARACTERS_DELETED_BY_CLIENT (0x390112)

- Modern: 3735826 (0x390112) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_UNDELETE_CHARACTER (0x390113)

- Modern: 3735827 (0x390113) · 4.3.4: —
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GET_UNDELETE_CHARACTER_COOLDOWN_STATUS (0x390114)

- Modern: 3735828 (0x390114) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ENGINE_SURVEY (0x390118)

- Modern: 3735832 (0x390118) · 4.3.4: —
- Layout (after the opcode): `u32 { u32*6 u8*2 u32 u8 u32*4 u64 u32 u8 u32*6 u8*4 u64*2 u32 bits(6) bits(6) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(6) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 26: 8 · 8; byte 86: 8 · 8 · 8 · 8; byte 110: 6 · 6 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 6 · 1 · 1 · 1 · 1 · 1 · (4 unused)

### CMSG_COMMERCE_TOKEN_GET_COUNT (0x390119)

- Modern: 3735833 (0x390119) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_COMMERCE_TOKEN_GET_MARKET_PRICE (0x39011a)

- Modern: 3735834 (0x39011a) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_AUCTIONABLE_TOKEN_SELL (0x39011b)

- Modern: 3735835 (0x39011b) · 4.3.4: —
- Layout (after the opcode): `u32 u64*2 u32`
- Size: 24 bytes (packed GUIDs not counted)

### CMSG_AUCTIONABLE_TOKEN_SELL_AT_MARKET_PRICE (0x39011c)

- Modern: 3735836 (0x39011c) · 4.3.4: —
- Layout (after the opcode): `u32 guid u32*2 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused)

### CMSG_CONSUMABLE_TOKEN_CAN_VETERAN_BUY (0x39011d)

- Modern: 3735837 (0x39011d) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CONSUMABLE_TOKEN_BUY (0x39011e)

- Modern: 3735838 (0x39011e) · 4.3.4: —
- Layout (after the opcode): `u32*2 guid u64`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_CONSUMABLE_TOKEN_BUY_AT_MARKET_PRICE (0x39011f)

- Modern: 3735839 (0x39011f) · 4.3.4: —
- Layout (after the opcode): `u32*3 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_GET_REMAINING_GAME_TIME (0x390120)

- Modern: 3735840 (0x390120) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CONSUMABLE_TOKEN_REDEEM (0x390121)

- Modern: 3735841 (0x390121) · 4.3.4: —
- Layout (after the opcode): `u32*2 u64 u32`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_CONSUMABLE_TOKEN_REDEEM_CONFIRMATION (0x390122)

- Modern: 3735842 (0x390122) · 4.3.4: —
- Layout (after the opcode): `u32*2 u64 guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused)

### CMSG_COMMERCE_TOKEN_GET_LOG (0x390123)

- Modern: 3735843 (0x390123) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GET_VAS_ACCOUNT_CHARACTER_LIST (0x390125)

- Modern: 3735845 (0x390125) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_GET_VAS_TRANSFER_TARGET_REALM_LIST (0x390126)

- Modern: 3735846 (0x390126) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_START_VAS_PURCHASE (0x390127)

- Modern: 3735847 (0x390127) · 4.3.4: —
- Layout (after the opcode): `u32 { u32*2 guid u32*2 guid guid guid bits(6) bits(7) bits(7) bits(6) opt[ u8 ] alt[ u8 ] bits(4) opt[ u8 ] flush opt[ opt[ bytes ] alt[ opt[ bytes ] alt[ bytes ] ] ] opt[ bytes ] opt[ bytes ] opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 26: 6 · 7 · 7 · 6 · 12 · 1 · (1 unused)

### CMSG_UPDATE_VAS_PURCHASE_STATES (0x390128)

- Modern: 3735848 (0x390128) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLENET_REQUEST (0x390129)

- Modern: 3735849 (0x390129) · 4.3.4: —
- Layout (after the opcode): `u32 { u64*2 u32 } u32 bytes`

### CMSG_CLUB_PRESENCE_SUBSCRIBE (0x39012b)

- Modern: 3735851 (0x39012b) · 4.3.4: —
- Layout (after the opcode): `u32 flush u32 loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_CHANGE_REALM_TICKET (0x39012d)

- Modern: 3735853 (0x39012d) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 6: (250 unused) · 1 · (5 unused)

### CMSG_REPORT_ENABLED_ADDONS (0x390132)

- Modern: 3735858 (0x390132) · 4.3.4: —
- Layout (after the opcode): `u32 { u32 loop[ bits(7) bits(6) opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] ] }`

### CMSG_REPORT_CLIENT_VARIABLES (0x390133)

- Modern: 3735859 (0x390133) · 4.3.4: —
- Layout (after the opcode): `u32 { u32 loop[ bits(6) opt[ u8 ] alt[ u8 ] bits(2) flush opt[ opt[ bytes ] alt[ opt[ bytes ] alt[ bytes ] ] ] opt[ bytes ] ] }`

### CMSG_REPORT_KEYBINDING_EXECUTION_COUNTS (0x390134)

- Modern: 3735860 (0x390134) · 4.3.4: —
- Layout (after the opcode): `u32 u8 bits(2) flush loop[ { bits(6) bits(6) flush u32 opt[ bytes ] opt[ bytes ] } ]`
- Bit fields (message of zeros, widths in order written): byte 2: 10 · (6 unused)

### CMSG_QUICK_JOIN_RESPOND_TO_INVITE (0x390137)

- Modern: 3735863 (0x390137) · 4.3.4: —
- Layout (after the opcode): `u32 guid guid flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_QUICK_JOIN_REQUEST_INVITE (0x390138)

- Modern: 3735864 (0x390138) · 4.3.4: —
- Layout (after the opcode): `u32 { u8*2 flush u32 guid u64 u8 opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · 9 · 1 · (5 unused)

### CMSG_QUICK_JOIN_AUTO_ACCEPT_REQUESTS (0x390139)

- Modern: 3735865 (0x390139) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_CAN_REDEEM_TOKEN_FOR_BALANCE (0x39013b)

- Modern: 3735867 (0x39013b) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_REQUEST_PRICE_INFO (0x39013c)

- Modern: 3735868 (0x39013c) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_VAS_GET_SERVICE_STATUS (0x39013d)

- Modern: 3735869 (0x39013d) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_VAS_GET_QUEUE_MINUTES (0x39013e)

- Modern: 3735870 (0x39013e) · 4.3.4: —
- Layout (after the opcode): `u32 u64 u32`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_VAS_CHECK_TRANSFER_OK (0x39013f)

- Modern: 3735871 (0x39013f) · 4.3.4: —
- Layout (after the opcode): `u32*2 u8 bits(3) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 6: 11 · (5 unused)

### CMSG_BATTLE_PAY_OPEN_CHECKOUT (0x390140)

- Modern: 3735872 (0x390140) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_VOICE_CHAT_LOGIN (0x390142)

- Modern: 3735874 (0x390142) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_VOICE_CHANNEL_STT_TOKEN_REQUEST (0x390143)

- Modern: 3735875 (0x390143) · 4.3.4: —
- Layout (after the opcode): `u32 bits(7) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)

### CMSG_VOICE_CHAT_JOIN_CHANNEL (0x390144)

- Modern: 3735876 (0x390144) · 4.3.4: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x390146)

- Modern: 3735878 (0x390146) · 4.3.4: —
- Layout (after the opcode): `u32 { bits(6) bits(7) opt[ u8 ] flush opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 6 · 7 · 1 · (2 unused)

### CMSG_BATTLE_PAY_CANCEL_OPEN_CHECKOUT (0x390147)

- Modern: 3735879 (0x390147) · 4.3.4: —
- Layout (after the opcode): `u32 bits(7) opt[ u8 ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · 1

### CMSG_DO_COUNTDOWN (0x39014c)

- Modern: 3735884 (0x39014c) · 4.3.4: —
- Layout (after the opcode): `u32 opt[ u8 ] flush u32 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)

### CMSG_CLUB_FINDER_POST (0x39014d)

- Modern: 3735885 (0x39014d) · 4.3.4: —
- Layout (after the opcode): `u32 { bits(7) opt[ u8 ] alt[ u8 ] bits(4) bits(3) opt[ u8 ] flush u64*2 u32*3 opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · 12; byte 4: 3 · 1 · (1 unused)

### CMSG_CLUB_FINDER_REQUEST_CLUBS_LIST (0x39014e)

- Modern: 3735886 (0x39014e) · 4.3.4: —
- Layout (after the opcode): `u32 { u8 bits(3) opt[ u8 ] flush u32*2 opt[ bytes ] loop[ { bits(3) flush bits(3) opt[ { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] } ] flush opt[ alt[ u32 u64 ] opt[ bytes ] ] } ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · 3 · 1 · (3 unused)

### CMSG_CLUB_FINDER_REQUEST_MEMBERSHIP_TO_CLUB (0x39014f)

- Modern: 3735887 (0x39014f) · 4.3.4: —
- Layout (after the opcode): `u32 guid u64 u8 bits(2) flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 12: 10 · (6 unused)

### CMSG_CLUB_FINDER_GET_APPLICANTS_LIST (0x390150)

- Modern: 3735888 (0x390150) · 4.3.4: —
- Layout (after the opcode): `u32 bits(3) flush`
- Bit fields (message of zeros, widths in order written): byte 2: 3 · (5 unused)

### CMSG_CLUB_FINDER_RESPOND_TO_APPLICANT (0x390151)

- Modern: 3735889 (0x390151) · 4.3.4: —
- Layout (after the opcode): `u32 guid guid bits(3) opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · 3 · 1 · (3 unused)

### CMSG_CLUB_FINDER_APPLICATION_RESPONSE (0x390152)

- Modern: 3735890 (0x390152) · 4.3.4: —
- Layout (after the opcode): `u32 guid bits(3) bits(3) flush`
- Bit fields (message of zeros, widths in order written): byte 4: 3 · 3 · (2 unused)

### CMSG_CLUB_FINDER_REQUEST_PENDING_CLUBS_LIST (0x390153)

- Modern: 3735891 (0x390153) · 4.3.4: —
- Layout (after the opcode): `u32 bits(3) flush`
- Bit fields (message of zeros, widths in order written): byte 2: 3 · (5 unused)

### CMSG_CLUB_FINDER_REQUEST_CLUBS_DATA (0x390154)

- Modern: 3735892 (0x390154) · 4.3.4: —
- Layout (after the opcode): `u32*3 loop[ u32 ] bits(3) opt[ u8 ] flush loop[ { bits(3) flush bits(3) opt[ { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] } ] flush opt[ alt[ u32 u64 ] opt[ bytes ] ] } ]`

### CMSG_CLUB_FINDER_REQUEST_SUBSCRIBED_CLUB_POSTING_IDS (0x390155)

- Modern: 3735893 (0x390155) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ u64 ]`

### CMSG_RAF_CLAIM_NEXT_REWARD (0x390157)

- Modern: 3735895 (0x390157) · 4.3.4: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x390158)

- Modern: 3735896 (0x390158) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_RAF_GENERATE_RECRUITMENT_LINK (0x390159)

- Modern: 3735897 (0x390159) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_REMOVE_RAF_RECRUIT (0x39015a)

- Modern: 3735898 (0x39015a) · 4.3.4: —
- Layout (after the opcode): `u32 u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_GET_ACCOUNT_NOTIFICATIONS (0x390168)

- Modern: 3735912 (0x390168) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ACCOUNT_NOTIFICATION_ACKNOWLEDGED (0x390169)

- Modern: 3735913 (0x390169) · 4.3.4: —
- Layout (after the opcode): `u32 u64 u32*2`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_AUCTION_SET_FAVORITE_ITEM (0x39016a)

- Modern: 3735914 (0x39016a) · 4.3.4: —
- Layout (after the opcode): `u32 flush { u32*5 }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_UPDATE_AADC_STATUS (0x39016c)

- Modern: 3735916 (0x39016c) · 4.3.4: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_VAS (0x39016d)

- Modern: 3735917 (0x39016d) · 4.3.4: —
- Layout (after the opcode): `u32 { u32 u64 guid u32*2 guid guid guid u8*3 u32 loop[ { u32*2 } ] bits(6) bits(7) bits(7) opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ bytes ] opt[ bytes ] opt[ bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 30: 8 · 8 · 8; byte 37: 6 · 7 · 7 · 1 · 1 · 1 · (1 unused)

### CMSG_CLUB_FINDER_WHISPER_APPLICANT_REQUEST (0x39016f)

- Modern: 3735919 (0x39016f) · 4.3.4: —
- Layout (after the opcode): `u32 guid guid`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x390177)

- Modern: 3735927 (0x390177) · 4.3.4: —
- Layout (after the opcode): `u32 { flush { u32 guid u8 u32 u8*3 u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] } { u32 guid u8 u32 u8*3 u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] } }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused); byte 14: 8 · 8 · 8; byte 36: 8 · 8 · 8

### — (0x390178)

- Modern: 3735928 (0x390178) · 4.3.4: —
- Layout (after the opcode): `u32*2 { u8*3 u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] } guid`
- Bit fields (message of zeros, widths in order written): byte 6: 8 · 8 · 8

### CMSG_SOCIAL_CONTRACT_REQUEST (0x39017b)

- Modern: 3735931 (0x39017b) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ACCEPT_SOCIAL_CONTRACT (0x39017c)

- Modern: 3735932 (0x39017c) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SAVE_ACCOUNT_DATA_EXPORT (0x390180)

- Modern: 3735936 (0x390180) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ guid ]`

### CMSG_CHAR_CREATE_FINALIZE_REINCARNATION (0x390195)

- Modern: 3735957 (0x390195) · 4.3.4: —
- Layout (after the opcode): `u32 guid { bits(6) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush u8*3 u32*2 opt[ bytes ] opt[ u32 ] loop[ u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 4: 6 · 1 · 1 · 1 · 1 · (6 unused) · 8 · 8 · 8

### CMSG_SUSPEND_COMMS_ACK (0x3a0000)

- Modern: 3801088 (0x3a0000) · 4.3.4: 17513 (0x4469)
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_AUTH_SESSION (0x3a0001)

- Modern: 3801089 (0x3a0001) · 4.3.4: 1097 (0x449)
- Layout (after the opcode): `u32 u64 u32*3 loop[ u8 ] loop[ u8 ] flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 22: 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 1 · (7 unused)

### CMSG_AUTH_CONTINUED_SESSION (0x3a0002)

- Modern: 3801090 (0x3a0002) · 4.3.4: 1101 (0x44d)
- Layout (after the opcode): `u32 u64*2 loop[ u8 ] loop[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 18: (154 unused) · 1 · (293 unused)

### CMSG_ENTER_ENCRYPTED_MODE_ACK (0x3a0003)

- Modern: 3801091 (0x3a0003) · 4.3.4: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_PING (0x3a0004)

- Modern: 3801092 (0x3a0004) · 4.3.4: 17485 (0x444d)
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_LOG_DISCONNECT (0x3a0005)

- Modern: 3801093 (0x3a0005) · 4.3.4: 17517 (0x446d)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SUSPEND_TOKEN_RESPONSE (0x3a0006)

- Modern: 3801094 (0x3a0006) · 4.3.4: 1133 (0x46d)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_ENABLE_NAGLE (0x3a0007)

- Modern: 3801095 (0x3a0007) · 4.3.4: 17481 (0x4449)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUEUED_MESSAGES_END (0x3a0008)

- Modern: 3801096 (0x3a0008) · 4.3.4: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_LOG_STREAMING_ERROR (0x3a0009)

- Modern: 3801097 (0x3a0009) · 4.3.4: —
- Layout (after the opcode): `u32 u8 flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · (7 unused)

### CMSG_QUERY_PLAYER_NAME_BY_COMMUNITY_ID (0x3a000b)

- Modern: 3801099 (0x3a000b) · 4.3.4: —
- Layout (after the opcode): `u32 { guid u64 }`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_QUERY_PLAYER_NAMES_FOR_COMMUNITY (0x3a000c)

- Modern: 3801100 (0x3a000c) · 4.3.4: —
- Layout (after the opcode): `u32 u64 u32 loop[ { guid u64 } ]`

### CMSG_LATENCY_REPORT (0x3a000d)

- Modern: 3801101 (0x3a000d) · 4.3.4: —
- Layout (after the opcode): `u32*3 loop[ { u32 u16 u8 u64 u32 } ]`

### CMSG_QUERY_PLAYER_NAMES (0x3a000e)

- Modern: 3801102 (0x3a000e) · 4.3.4: —
- Layout (after the opcode): `u32*2 loop[ guid ]`
