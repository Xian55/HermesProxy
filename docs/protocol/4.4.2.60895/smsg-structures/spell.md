# Spell — server packet layouts, 4.4.2.60895

### SMSG_CHEAT_IGNORE_DIMISHING_RETURNS (0x510002)

- Modern: 5308418 (0x510002) · 4.3.4: — · Area: Spell
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_MIRROR_IMAGE_CREATURE_DATA (0x510003)

- Modern: 5308419 (0x510003) · 4.3.4: — · Area: Spell
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MIRROR_IMAGE_COMPONENTED_DATA (0x510004)

- Modern: 5308420 (0x510004) · 4.3.4: 9780 (0x2634) · Area: Spell
- Layout: `guid u32 u8*3 u32 guid u32 loop[ { u32*2 } ] loop[ u32 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 8 · 8 · 8

### SMSG_SPELL_COOLDOWN (0x510005)

- Modern: 5308421 (0x510005) · 4.3.4: 19222 (0x4b16) · Area: Spell
- Layout: `guid u8 u32 loop[ u32*3 ]`

### SMSG_SPELL_CATEGORY_COOLDOWN (0x510006)

- Modern: 5308422 (0x510006) · 4.3.4: — · Area: Spell
- Layout: `u32*3 u8`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_SPELL_DISPELL_LOG (0x510007)

- Modern: 5308423 (0x510007) · 4.3.4: 17686 (0x4516) · Area: Spell
- Layout: `{ u8 guid guid u32*2 loop[ u32 u8 opt[ u32 ] opt[ u32 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_SPELL_PERIODIC_AURA_LOG (0x510008)

- Modern: 5308424 (0x510008) · 4.3.4: 1046 (0x416) · Area: Spell
- Layout: `{ guid guid u32*2 u8 loop[ { u32*8 loop[ guid u32*3 ] u8 opt[ u16*2 u32 u8*6 ] opt[ u32*2 ] } ] opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_SPELL_ENERGIZE_LOG (0x510009)

- Modern: 5308425 (0x510009) · 4.3.4: 1044 (0x414) · Area: Spell
- Layout: `guid guid u32 u8 u32*2 u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 17: 1 · (7 unused)

### SMSG_SPELL_HEAL_LOG (0x51000a)

- Modern: 5308426 (0x51000a) · 4.3.4: 10262 (0x2816) · Area: Spell
- Layout: `guid guid u32*5 u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ] opt[ u32 ] opt[ u32 ] opt[ { u16*2 u32 u8*6 } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 1 · 1 · 1 · 1 · (3 unused)

### SMSG_SPELL_HEAL_ABSORB_LOG (0x51000b)

- Modern: 5308427 (0x51000b) · 4.3.4: — · Area: Spell
- Layout: `guid guid guid u32*4 u8 opt[ { u16*2 u32 u8*6 } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · (7 unused)

### SMSG_SPELL_ABSORB_LOG (0x51000c)

- Modern: 5308428 (0x51000c) · 4.3.4: — · Area: Spell
- Layout: `guid guid u32*2 guid u32*2 u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · 1 · (6 unused)

### SMSG_SPELL_INTERRUPT_LOG (0x51000d)

- Modern: 5308429 (0x51000d) · 4.3.4: 7591 (0x1da7) · Area: Spell
- Layout: `guid guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_ENVIRONMENTAL_DAMAGE_LOG (0x51000e)

- Modern: 5308430 (0x51000e) · 4.3.4: 27653 (0x6c05) · Area: Spell
- Layout: `guid u8 u32*3 u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 15: 1 · (7 unused)

### SMSG_AURA_UPDATE (0x510011)

- Modern: 5308433 (0x510011) · 4.3.4: 18183 (0x4707) · Area: Spell
- Layout: `u8 { opt[ u8 ] alt[ u8 ] opt[ u8 ] } loop[ u8*2 opt[ { guid u32*2 u16 u32 u16 u8 u32 u8*3 opt[ { u16*2 u32 u8*6 } ] opt[ guid ] opt[ u32 ] opt[ u32 ] opt[ u32 ] loop[ u32 ] loop[ u32 ] } ] ] guid`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 9 · (6 unused)

### SMSG_AURA_POINTS_DEPLETED (0x510012)

- Modern: 5308434 (0x510012) · 4.3.4: 31927 (0x7cb7) · Area: Spell
- Layout: `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8

### SMSG_PET_CLEAR_SPELLS (0x510013)

- Modern: 5308435 (0x510013) · 4.3.4: — · Area: Spell
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_PET_SPELLS_MESSAGE (0x510014)

- Modern: 5308436 (0x510014) · 4.3.4: 16660 (0x4114) · Area: Spell
- Layout: `{ guid u16*2 u32 u8*3 loop[ u32 ] u32*3 loop[ u32 ] loop[ u32*4 u16 ] loop[ u32*3 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 8 · 8

### SMSG_CLEAR_COOLDOWNS (0x510015)

- Modern: 5308437 (0x510015) · 4.3.4: 22964 (0x59b4) · Area: Spell
- Layout: `u32 loop[ u32 ] u8`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_CLEAR_ALL_SPELL_CHARGES (0x510016)

- Modern: 5308438 (0x510016) · 4.3.4: — · Area: Spell
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_CLEAR_SPELL_CHARGES (0x510017)

- Modern: 5308439 (0x510017) · 4.3.4: — · Area: Spell
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_SET_SPELL_CHARGES (0x510018)

- Modern: 5308440 (0x510018) · 4.3.4: — · Area: Spell
- Layout: `u32*2 u8 u32 u8`
- Size: 14 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 13: 1 · (7 unused)

### SMSG_SEND_KNOWN_SPELLS (0x510019)

- Modern: 5308441 (0x510019) · 4.3.4: 260 (0x104) · Area: Spell
- Layout: `u8 u32*2 loop[ u32 ] loop[ u32 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_SEND_SPELL_HISTORY (0x51001a)

- Modern: 5308442 (0x51001a) · 4.3.4: — · Area: Spell
- Layout: `u32 loop[ { u32*6 u8 opt[ u32 ] opt[ u32 ] } ]`

### SMSG_REFRESH_SPELL_HISTORY (0x51001b)

- Modern: 5308443 (0x51001b) · 4.3.4: — · Area: Spell
- Layout: `u32 loop[ { u32*6 u8 opt[ u32 ] opt[ u32 ] } ]`

### SMSG_SEND_SPELL_CHARGES (0x51001c)

- Modern: 5308444 (0x51001c) · 4.3.4: — · Area: Spell
- Layout: `u32 loop[ { u32*3 u8 } ]`

### SMSG_SEND_UNLEARN_SPELLS (0x51001d)

- Modern: 5308445 (0x51001d) · 4.3.4: 20005 (0x4e25) · Area: Spell
- Layout: `u32 loop[ u32 ]`

### SMSG_SPELL_OR_DAMAGE_IMMUNE (0x51001e)

- Modern: 5308446 (0x51001e) · 4.3.4: 17671 (0x4507) · Area: Spell
- Layout: `guid guid u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_DISPEL_FAILED (0x51001f)

- Modern: 5308447 (0x51001f) · 4.3.4: 775 (0x307) · Area: Spell
- Layout: `guid guid u32*2 loop[ u32 ]`

### SMSG_SPELL_DAMAGE_SHIELD (0x510020)

- Modern: 5308448 (0x510020) · 4.3.4: 10535 (0x2927) · Area: Spell
- Layout: `guid guid u32*6 u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 28: 1 · (7 unused)

### SMSG_SPELL_NON_MELEE_DAMAGE_LOG (0x510021)

- Modern: 5308449 (0x510021) · 4.3.4: 17173 (0x4315) · Area: Spell
- Layout: `{ guid guid guid u32*5 u8 u32*3 u8*2 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ] opt[ u32*10 ] opt[ { u16*2 u32 u8*6 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 39: 1 · 7 · 1 · 1 · 1 · (5 unused)

### SMSG_SPELL_INSTAKILL_LOG (0x510022)

- Modern: 5308450 (0x510022) · 4.3.4: 25110 (0x6216) · Area: Spell
- Layout: `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_SPELL_CHANNEL_START (0x510023)

- Modern: 5308451 (0x510023) · 4.3.4: — · Area: Spell
- Layout: `guid u32*3 u8 opt[ u32*2 ] opt[ guid { u32 u8 guid } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · 1 · (6 unused)

### SMSG_SPELL_CHANNEL_UPDATE (0x510024)

- Modern: 5308452 (0x510024) · 4.3.4: — · Area: Spell
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_SET_FLAT_SPELL_MODIFIER (0x510025)

- Modern: 5308453 (0x510025) · 4.3.4: 10292 (0x2834) · Area: Spell
- Layout: `u32 loop[ { u8 u32 loop[ u32 u8 ] } ]`

### SMSG_SET_PCT_SPELL_MODIFIER (0x510026)

- Modern: 5308454 (0x510026) · 4.3.4: 548 (0x224) · Area: Spell
- Layout: `u32 loop[ { u8 u32 loop[ u32 u8 ] } ]`

### SMSG_SPELL_PREPARE (0x510027)

- Modern: 5308455 (0x510027) · 4.3.4: — · Area: Spell
- Layout: `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_SPELL_GO (0x510028)

- Modern: 5308456 (0x510028) · 4.3.4: 28182 (0x6e16) · Area: Spell
- Layout: `{ guid guid guid guid u32*7 u8 u32*3 u8 guid u8*10 { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] alt[ u8 ] guid guid opt[ guid u32*3 ] opt[ guid u32*3 ] opt[ u32 ] opt[ u32 ] bytes } loop[ guid ] loop[ guid ] loop[ u8 opt[ u8 ] ] loop[ u8 u32 ] opt[ u8*2 u32 loop[ u8 ] ] loop[ guid u32*3 ] opt[ u32 ] opt[ u32 ] } u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 52: 16; byte 54: 16; byte 56: 16; byte 58: 9 · 1 · 16; byte 61: 1 · 1 · (4 unused) · 28; byte 65: 1 · 1 · 1 · 1 · 7 · (1 unused); byte 71: 1 · (7 unused)

### SMSG_SPELL_START (0x510029)

- Modern: 5308457 (0x510029) · 4.3.4: 25621 (0x6415) · Area: Spell
- Layout: `{ guid guid guid guid u32*7 u8 u32*3 u8 guid u8*10 { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] alt[ u8 ] guid guid opt[ guid u32*3 ] opt[ guid u32*3 ] opt[ u32 ] opt[ u32 ] bytes } loop[ guid ] loop[ guid ] loop[ u8 opt[ u8 ] ] loop[ u8 u32 ] opt[ u8*2 u32 loop[ u8 ] ] loop[ guid u32*3 ] opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 52: 16; byte 54: 16; byte 56: 16; byte 58: 9 · 1 · 16; byte 61: 1 · 1 · (4 unused) · 28; byte 65: 1 · 1 · 1 · 1 · 7 · (1 unused)

### SMSG_RESUME_CAST (0x51002a)

- Modern: 5308458 (0x51002a) · 4.3.4: — · Area: Spell
- Layout: `guid u32 guid guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_RESUME_CAST_BAR (0x51002d)

- Modern: 5308461 (0x51002d) · 4.3.4: — · Area: Spell
- Layout: `guid guid u32*4 u8 opt[ u32*2 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)

### SMSG_SPELL_DELAYED (0x51002e)

- Modern: 5308462 (0x51002e) · 4.3.4: 1813 (0x715) · Area: Spell
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_SPELL_EXECUTE_LOG (0x51002f)

- Modern: 5308463 (0x51002f) · 4.3.4: 1574 (0x626) · Area: Spell
- Layout: `{ guid u32*2 loop[ { u32*7 loop[ guid u32 u8 u32 ] loop[ guid u32 ] loop[ guid u32*2 ] loop[ guid ] loop[ u32 ] loop[ u32 ] } ] u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · (7 unused)

### SMSG_SPELL_MISS_LOG (0x510030)

- Modern: 5308464 (0x510030) · 4.3.4: 1573 (0x625) · Area: Spell
- Layout: `u32 guid u32 loop[ guid u8*2 opt[ u32*2 ] ]`

### SMSG_NOTIFY_DEST_LOC_SPELL_CAST (0x510032)

- Modern: 5308466 (0x510032) · 4.3.4: 25092 (0x6204) · Area: Spell
- Layout: `{ guid guid u32*11 u8 guid }`
- Size: 45 bytes (packed GUIDs not counted)

### SMSG_CANCEL_SPELL_VISUAL (0x510033)

- Modern: 5308467 (0x510033) · 4.3.4: — · Area: Spell
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_PLAY_SPELL_VISUAL (0x510034)

- Modern: 5308468 (0x510034) · 4.3.4: 4273 (0x10b1) · Area: Spell
- Layout: `guid guid guid { u32*3 } u32*2 u16*3 u32*2 u8`
- Size: 35 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 40: 1 · (7 unused)

### SMSG_CANCEL_ORPHAN_SPELL_VISUAL (0x510035)

- Modern: 5308469 (0x510035) · 4.3.4: — · Area: Spell
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_PLAY_ORPHAN_SPELL_VISUAL (0x510036)

- Modern: 5308470 (0x510036) · 4.3.4: — · Area: Spell
- Layout: `{ u32*9 guid guid u32*4 u8 }`
- Size: 53 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 56: 1 · (7 unused)

### SMSG_CANCEL_SPELL_VISUAL_KIT (0x510037)

- Modern: 5308471 (0x510037) · 4.3.4: — · Area: Spell
- Layout: `guid u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_PLAY_SPELL_VISUAL_KIT (0x510038)

- Modern: 5308472 (0x510038) · 4.3.4: 21925 (0x55a5) · Area: Spell
- Layout: `guid u32*3 u8`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### SMSG_GAME_OBJECT_PLAY_SPELL_VISUAL_KIT (0x510039)

- Modern: 5308473 (0x510039) · 4.3.4: — · Area: Spell
- Layout: `guid u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_GAME_OBJECT_PLAY_SPELL_VISUAL (0x51003a)

- Modern: 5308474 (0x51003a) · 4.3.4: — · Area: Spell
- Layout: `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_SUPERCEDED_SPELLS (0x51003b)

- Modern: 5308475 (0x51003b) · 4.3.4: 13744 (0x35b0) · Area: Spell
- Layout: `u32 loop[ { u32 u8 opt[ u32 ] opt[ u32 ] opt[ u32 ] } ]`

### SMSG_LEARNED_SPELLS (0x51003c)

- Modern: 5308476 (0x51003c) · 4.3.4: — · Area: Spell
- Layout: `u32*2 u8 loop[ { u32 u8 opt[ u32 ] opt[ u32 ] opt[ u32 ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_UNLEARNED_SPELLS (0x51003d)

- Modern: 5308477 (0x51003d) · 4.3.4: 18436 (0x4804) · Area: Spell
- Layout: `u32 loop[ u32 ] u8`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_PET_LEARNED_SPELLS (0x51003e)

- Modern: 5308478 (0x51003e) · 4.3.4: 1287 (0x507) · Area: Spell
- Layout: `u32 loop[ u32 ]`

### SMSG_PET_UNLEARNED_SPELLS (0x51003f)

- Modern: 5308479 (0x51003f) · 4.3.4: 27140 (0x6a04) · Area: Spell
- Layout: `u32 loop[ u32 ]`

### SMSG_PUSH_SPELL_TO_ACTION_BAR (0x510040)

- Modern: 5308480 (0x510040) · 4.3.4: — · Area: Spell
- Layout: `struct`

### SMSG_REMOVE_SPELL_FROM_ACTION_BAR (0x510041)

- Modern: 5308481 (0x510041) · 4.3.4: — · Area: Spell
- Layout: `struct`

### SMSG_SPELL_FAILURE (0x510042)

- Modern: 5308482 (0x510042) · 4.3.4: 17717 (0x4535) · Area: Spell
- Layout: `guid guid u32*2 u16`
- Size: 10 bytes (packed GUIDs not counted)

### SMSG_ACTIVE_GLYPHS (0x510043)

- Modern: 5308483 (0x510043) · 4.3.4: — · Area: Spell
- Layout: `u32 loop[ u32 u16 ] u8`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_SPELL_FAILED_OTHER (0x510044)

- Modern: 5308484 (0x510044) · 4.3.4: 3124 (0xc34) · Area: Spell
- Layout: `guid guid u32*2 u8`
- Size: 9 bytes (packed GUIDs not counted)

### SMSG_SCRIPT_CAST (0x510045)

- Modern: 5308485 (0x510045) · 4.3.4: — · Area: Spell
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_CAST_FAILED (0x510046)

- Modern: 5308486 (0x510046) · 4.3.4: 19734 (0x4d16) · Area: Spell
- Layout: `guid u32*5`
- Size: 20 bytes (packed GUIDs not counted)

### SMSG_PET_CAST_FAILED (0x510047)

- Modern: 5308487 (0x510047) · 4.3.4: 11029 (0x2b15) · Area: Spell
- Layout: `guid u32*4`
- Size: 16 bytes (packed GUIDs not counted)

### SMSG_INTERRUPT_POWER_REGEN (0x510048)

- Modern: 5308488 (0x510048) · 4.3.4: — · Area: Spell
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)

### SMSG_SPELL_FAILURE_MESSAGE (0x510049)

- Modern: 5308489 (0x510049) · 4.3.4: — · Area: Spell
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_RESYNC_RUNES (0x51004e)

- Modern: 5308494 (0x51004e) · 4.3.4: 25124 (0x6224) · Area: Spell
- Layout: `{ u8*2 u32 loop[ u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8

### SMSG_CONVERT_RUNE (0x51004f)

- Modern: 5308495 (0x51004f) · 4.3.4: 20244 (0x4f14) · Area: Spell
- Layout: `{ u8*2 u32 loop[ u8 ] } u32*2`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8

### SMSG_DAMAGE_CALC_LOG (0x510051)

- Modern: 5308497 (0x510051) · 4.3.4: 9270 (0x2436) · Area: Spell
- Layout: `guid guid u32*3 loop[ { u8 opt[ u8*2 bytes ] u32 bytes opt[ u32*4 loop[ { guid u32*3 } ] ] } ]`
