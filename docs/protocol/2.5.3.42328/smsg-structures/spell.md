# Spell — server packet layouts, 2.5.3.42328

### SMSG_CHEAT_IGNORE_DIMISHING_RETURNS (0x2c12)

- Modern: 11282 (0x2c12) · 2.4.3: — · Area: Spell
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_MIRROR_IMAGE_CREATURE_DATA (0x2c13)

- Modern: 11283 (0x2c13) · 2.4.3: — · Area: Spell
- Layout: `{ guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MIRROR_IMAGE_COMPONENTED_DATA (0x2c14)

- Modern: 11284 (0x2c14) · 2.4.3: 1025 (0x401) · Area: Spell
- Layout: `{ guid u32 u8*3 u32 guid u32 loop[ { u32*2 } ] loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 8 · 8 · 8

### SMSG_SPELL_COOLDOWN (0x2c15)

- Modern: 11285 (0x2c15) · 2.4.3: 308 (0x134) · Area: Spell
- Layout: `{ guid u8 u32 loop[ u32*3 ] }`
- HermesProxy: `SpellCooldownPkt` — matches

### SMSG_CATEGORY_COOLDOWN (0x2c16)

- Modern: 11286 (0x2c16) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ u32*2 ] }`

### SMSG_SPELL_CATEGORY_COOLDOWN (0x2c17)

- Modern: 11287 (0x2c17) · 2.4.3: — · Area: Spell
- Layout: `{ u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)

### SMSG_WEEKLY_SPELL_USAGE (0x2c18)

- Modern: 11288 (0x2c18) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ u32 u8 ] }`

### SMSG_UPDATE_WEEKLY_SPELL_USAGE (0x2c19)

- Modern: 11289 (0x2c19) · 2.4.3: — · Area: Spell
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)

### SMSG_SPELL_DISPELL_LOG (0x2c1a)

- Modern: 11290 (0x2c1a) · 2.4.3: 635 (0x27b) · Area: Spell
- Layout: `{ { u8 guid guid u32*2 loop[ u32 u8 opt[ u32 ] opt[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)
- HermesProxy: `SpellDispellLog` — matches

### SMSG_SPELL_PERIODIC_AURA_LOG (0x2c1b)

- Modern: 11291 (0x2c1b) · 2.4.3: 590 (0x24e) · Area: Spell
- Layout: `{ guid guid u32*2 u8 loop[ { u32*7 u8 opt[ u16*3 u8*6 ] opt[ u32*2 ] } ] opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)
- HermesProxy: `SpellPeriodicAuraLog` — differs

### SMSG_SPELL_ENERGIZE_LOG (0x2c1c)

- Modern: 11292 (0x2c1c) · 2.4.3: 337 (0x151) · Area: Spell
- Layout: `{ guid guid u32*4 u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)
- HermesProxy: `SpellEnergizeLog` — matches

### SMSG_SPELL_HEAL_LOG (0x2c1d)

- Modern: 11293 (0x2c1d) · 2.4.3: 336 (0x150) · Area: Spell
- Layout: `{ { guid guid u32*5 u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] opt[ u32 ] opt[ u32 ] opt[ { u16*3 u8*6 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 1 · 1 · 1 · 1 · (3 unused)
- HermesProxy: `SpellHealLog` — differs

### SMSG_SPELL_HEAL_ABSORB_LOG (0x2c1e)

- Modern: 11294 (0x2c1e) · 2.4.3: — · Area: Spell
- Layout: `{ guid guid guid u32*4 u8 opt[ { u16*3 u8*6 } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · (7 unused)

### SMSG_SPELL_ABSORB_LOG (0x2c1f)

- Modern: 11295 (0x2c1f) · 2.4.3: — · Area: Spell
- Layout: `{ guid guid u32*2 guid u32*2 u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · 1 · (6 unused)

### SMSG_SPELL_INTERRUPT_LOG (0x2c20)

- Modern: 11296 (0x2c20) · 2.4.3: — · Area: Spell
- Layout: `{ guid guid u32*2 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_ENVIRONMENTAL_DAMAGE_LOG (0x2c21)

- Modern: 11297 (0x2c21) · 2.4.3: 508 (0x1fc) · Area: Spell
- Layout: `{ guid u8 u32*3 u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 15: 1 · (7 unused)
- HermesProxy: `EnvironmentalDamageLog` — matches

### SMSG_AURA_UPDATE (0x2c22)

- Modern: 11298 (0x2c22) · 2.4.3: — · Area: Spell
- Layout: `{ { u8*2 alt[ ] loop[ u8*2 opt[ { guid u32*2 u16 u32 u16 u8 u32 u8*3 opt[ { u16*3 u8*6 } ] opt[ guid ] opt[ u32 ] opt[ u32 ] opt[ u32 ] loop[ u32 ] loop[ u32 ] } ] ] guid } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 9 · (6 unused)
- HermesProxy: `AuraUpdate` — differs

### SMSG_AURA_POINTS_DEPLETED (0x2c23)

- Modern: 11299 (0x2c23) · 2.4.3: — · Area: Spell
- Layout: `{ guid u8*2 }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8

### SMSG_PET_CLEAR_SPELLS (0x2c24)

- Modern: 11300 (0x2c24) · 2.4.3: — · Area: Spell
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `PetClearSpells` — matches

### SMSG_PET_SPELLS_MESSAGE (0x2c25)

- Modern: 11301 (0x2c25) · 2.4.3: 377 (0x179) · Area: Spell
- Layout: `{ { guid u16*2 u32 u16 u8 loop[ u32 ] u32*2 u32 loop[ u32 ] loop[ u32*4 u16 ] loop[ u32*3 u8 ] } }`
- HermesProxy: `PetSpells` — matches

### SMSG_CLEAR_COOLDOWNS (0x2c26)

- Modern: 11302 (0x2c26) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ u32 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_CLEAR_ALL_SPELL_CHARGES (0x2c27)

- Modern: 11303 (0x2c27) · 2.4.3: — · Area: Spell
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_CLEAR_SPELL_CHARGES (0x2c28)

- Modern: 11304 (0x2c28) · 2.4.3: — · Area: Spell
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_SET_SPELL_CHARGES (0x2c29)

- Modern: 11305 (0x2c29) · 2.4.3: — · Area: Spell
- Layout: `{ u32*2 u8 u32 u8 }`
- Size: 14 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 13: 1 · (7 unused)

### SMSG_SEND_KNOWN_SPELLS (0x2c2a)

- Modern: 11306 (0x2c2a) · 2.4.3: 298 (0x12a) · Area: Spell
- Layout: `{ u8 u32*2 loop[ u32 ] loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `SendKnownSpells` — matches

### SMSG_SEND_SPELL_HISTORY (0x2c2b)

- Modern: 11307 (0x2c2b) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ { u32*6 u8 opt[ u32 ] opt[ u32 ] } ] }`
- HermesProxy: `SendSpellHistory`; `EmptySpellHistory` — matches

### SMSG_REFRESH_SPELL_HISTORY (0x2c2c)

- Modern: 11308 (0x2c2c) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ { u32*6 u8 opt[ u32 ] opt[ u32 ] } ] }`

### SMSG_SEND_SPELL_CHARGES (0x2c2d)

- Modern: 11309 (0x2c2d) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ { u32*3 u8 } ] }`
- HermesProxy: `SendSpellCharges`; `EmptySpellCharges` — matches

### SMSG_SEND_UNLEARN_SPELLS (0x2c2e)

- Modern: 11310 (0x2c2e) · 2.4.3: 1053 (0x41d) · Area: Spell
- Layout: `{ u32 loop[ u32 ] }`
- HermesProxy: `SendUnlearnSpells` — matches

### SMSG_SPELL_OR_DAMAGE_IMMUNE (0x2c2f)

- Modern: 11311 (0x2c2f) · 2.4.3: 611 (0x263) · Area: Spell
- Layout: `{ guid guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · (7 unused)

### SMSG_DISPEL_FAILED (0x2c30)

- Modern: 11312 (0x2c30) · 2.4.3: 610 (0x262) · Area: Spell
- Layout: `{ guid guid u32*2 loop[ u32 ] }`
- HermesProxy: `DispelFailed` — matches

### SMSG_SPELL_DAMAGE_SHIELD (0x2c31)

- Modern: 11313 (0x2c31) · 2.4.3: 591 (0x24f) · Area: Spell
- Layout: `{ guid guid u32*6 u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 28: 1 · (7 unused)
- HermesProxy: `SpellDamageShield` — matches

### SMSG_SPELL_NON_MELEE_DAMAGE_LOG (0x2c32)

- Modern: 11314 (0x2c32) · 2.4.3: 592 (0x250) · Area: Spell
- Layout: `{ { guid guid guid u32*5 u8 u32*3 u8*2 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] opt[ u32*10 ] opt[ { u16*3 u8*6 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 39: 1 · 7 · 1 · 1 · 1 · (5 unused)
- HermesProxy: `SpellNonMeleeDamageLog` — differs

### SMSG_SPELL_INSTAKILL_LOG (0x2c33)

- Modern: 11315 (0x2c33) · 2.4.3: 815 (0x32f) · Area: Spell
- Layout: `{ guid guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SpellInstakillLog` — matches

### SMSG_SPELL_CHANNEL_START (0x2c34)

- Modern: 11316 (0x2c34) · 2.4.3: — · Area: Spell
- Layout: `{ guid u32*3 u8 opt[ u32*2 ] opt[ guid { u32 u8 guid } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · 1 · (6 unused)
- HermesProxy: `SpellChannelStart` — matches

### SMSG_SPELL_CHANNEL_UPDATE (0x2c35)

- Modern: 11317 (0x2c35) · 2.4.3: — · Area: Spell
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SpellChannelUpdate` — matches

### SMSG_SET_FLAT_SPELL_MODIFIER (0x2c36)

- Modern: 11318 (0x2c36) · 2.4.3: 614 (0x266) · Area: Spell
- Layout: `{ u32 loop[ { u8 u32 loop[ u32 u8 ] } ] }`

### SMSG_SET_PCT_SPELL_MODIFIER (0x2c37)

- Modern: 11319 (0x2c37) · 2.4.3: 615 (0x267) · Area: Spell
- Layout: `{ u32 loop[ { u8 u32 loop[ u32 u8 ] } ] }`

### SMSG_SPELL_PREPARE (0x2c38)

- Modern: 11320 (0x2c38) · 2.4.3: — · Area: Spell
- Layout: `{ guid guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SpellPrepare` — matches

### SMSG_SPELL_GO (0x2c39)

- Modern: 11321 (0x2c39) · 2.4.3: 306 (0x132) · Area: Spell
- Layout: `{ { guid guid guid guid u32*7 u8 u32*3 u8 guid u8*2 u8*2 u8*2 u8*2 u8*2 loop[ u8 ] { { loop[ opt[ u8 ] alt[ u8 ] ] bits(2) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] guid guid opt[ { guid u32*3 } ] opt[ { guid u32*3 } ] opt[ u32 ] opt[ u32 ] bytes } loop[ guid ] loop[ guid ] loop[ u32 u8 ] opt[ u8*2 u32 loop[ u8 ] ] loop[ guid u32*3 ] opt[ u32 ] opt[ u32 ] } u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 52: 16; byte 54: 16; byte 56: 16; byte 58: 9 · 1 · 16; byte 61: 1 · 1 · (4 unused) · 26; byte 65: 1 · 1 · 1 · 1 · 7 · (3 unused); byte 71: 1 · (7 unused)
- HermesProxy: `SpellGo` — matches

### SMSG_SPELL_START (0x2c3a)

- Modern: 11322 (0x2c3a) · 2.4.3: 305 (0x131) · Area: Spell
- Layout: `{ { guid guid guid guid u32*7 u8 u32*3 u8 guid u8*2 u8*2 u8*2 u8*2 u8*2 loop[ u8 ] { { loop[ opt[ u8 ] alt[ u8 ] ] bits(2) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] guid guid opt[ { guid u32*3 } ] opt[ { guid u32*3 } ] opt[ u32 ] opt[ u32 ] bytes } loop[ guid ] loop[ guid ] loop[ u32 u8 ] opt[ u8*2 u32 loop[ u8 ] ] loop[ guid u32*3 ] opt[ u32 ] opt[ u32 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 52: 16; byte 54: 16; byte 56: 16; byte 58: 9 · 1 · 16; byte 61: 1 · 1 · (4 unused) · 26; byte 65: 1 · 1 · 1 · 1 · 7 · (3 unused)
- HermesProxy: `SpellStart` — matches

### SMSG_RESUME_CAST (0x2c3b)

- Modern: 11323 (0x2c3b) · 2.4.3: — · Area: Spell
- Layout: `{ guid u32 guid guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_RESUME_CAST_BAR (0x2c3e)

- Modern: 11326 (0x2c3e) · 2.4.3: — · Area: Spell
- Layout: `{ guid guid u32*4 u8 opt[ u32*2 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 1 · (7 unused)

### SMSG_SPELL_DELAYED (0x2c3f)

- Modern: 11327 (0x2c3f) · 2.4.3: 482 (0x1e2) · Area: Spell
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SpellDelayed` — matches

### SMSG_SPELL_EXECUTE_LOG (0x2c40)

- Modern: 11328 (0x2c40) · 2.4.3: 588 (0x24c) · Area: Spell
- Layout: `{ { guid u32*2 alt[ ] loop[ { u32*2 u32 u32 u32 u32 u32 loop[ guid u32*3 ] loop[ guid u32 ] loop[ guid u32*2 ] loop[ guid ] loop[ u32 ] loop[ u32 ] } ] u8 opt[ { u64 u32*3 u8*2 loop[ u32*3 ] } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · (7 unused)
- HermesProxy: `SpellExecuteLog` — matches

### SMSG_SPELL_MISS_LOG (0x2c41)

- Modern: 11329 (0x2c41) · 2.4.3: 587 (0x24b) · Area: Spell
- Layout: `{ u32 guid u32 loop[ guid u8*2 opt[ u32*2 ] ] }`
- HermesProxy: `SpellMissLog` — matches

### SMSG_NOTIFY_DEST_LOC_SPELL_CAST (0x2c43)

- Modern: 11331 (0x2c43) · 2.4.3: — · Area: Spell
- Layout: `{ { guid guid u32*11 u8 guid } }`
- Size: 45 bytes (packed GUIDs not counted)

### SMSG_CANCEL_SPELL_VISUAL (0x2c44)

- Modern: 11332 (0x2c44) · 2.4.3: — · Area: Spell
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_PLAY_SPELL_VISUAL (0x2c45)

- Modern: 11333 (0x2c45) · 2.4.3: 499 (0x1f3) · Area: Spell
- Layout: `{ guid guid guid u32*5 u16*3 u32*2 u8 }`
- Size: 35 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 40: 1 · (7 unused)

### SMSG_CANCEL_ORPHAN_SPELL_VISUAL (0x2c46)

- Modern: 11334 (0x2c46) · 2.4.3: — · Area: Spell
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_PLAY_ORPHAN_SPELL_VISUAL (0x2c47)

- Modern: 11335 (0x2c47) · 2.4.3: — · Area: Spell
- Layout: `{ u32*9 guid u32*4 u8 }`
- Size: 53 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 54: 1 · (7 unused)

### SMSG_CANCEL_SPELL_VISUAL_KIT (0x2c48)

- Modern: 11336 (0x2c48) · 2.4.3: — · Area: Spell
- Layout: `{ guid u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_PLAY_SPELL_VISUAL_KIT (0x2c49)

- Modern: 11337 (0x2c49) · 2.4.3: — · Area: Spell
- Layout: `{ guid u32*3 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)
- HermesProxy: `PlaySpellVisualKit` — matches

### SMSG_GAME_OBJECT_PLAY_SPELL_VISUAL_KIT (0x2c4a)

- Modern: 11338 (0x2c4a) · 2.4.3: — · Area: Spell
- Layout: `{ guid u32*3 }`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_GAME_OBJECT_PLAY_SPELL_VISUAL (0x2c4b)

- Modern: 11339 (0x2c4b) · 2.4.3: — · Area: Spell
- Layout: `{ guid guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_SUPERCEDED_SPELLS (0x2c4c)

- Modern: 11340 (0x2c4c) · 2.4.3: 300 (0x12c) · Area: Spell
- Layout: `{ u32*3 loop[ u32 ] loop[ u32 ] loop[ u32 ] }`
- HermesProxy: `SupercededSpells` — matches

### SMSG_LEARNED_SPELLS (0x2c4d)

- Modern: 11341 (0x2c4d) · 2.4.3: — · Area: Spell
- Layout: `{ u32*3 loop[ u32 ] loop[ u32 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · (7 unused)
- HermesProxy: `LearnedSpells` — matches

### SMSG_UNLEARNED_SPELLS (0x2c4e)

- Modern: 11342 (0x2c4e) · 2.4.3: 515 (0x203) · Area: Spell
- Layout: `{ u32 loop[ u32 ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `UnlearnedSpells` — matches

### SMSG_PET_LEARNED_SPELLS (0x2c4f)

- Modern: 11343 (0x2c4f) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ u32 ] }`
- HermesProxy: `PetLearnedSpells` — matches

### SMSG_PET_UNLEARNED_SPELLS (0x2c50)

- Modern: 11344 (0x2c50) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ u32 ] }`
- HermesProxy: `PetUnlearnedSpells` — matches

### SMSG_PUSH_SPELL_TO_ACTION_BAR (0x2c51)

- Modern: 11345 (0x2c51) · 2.4.3: — · Area: Spell
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_SPELL_FAILURE (0x2c53)

- Modern: 11347 (0x2c53) · 2.4.3: 307 (0x133) · Area: Spell
- Layout: `{ guid guid u32*2 u16 }`
- Size: 10 bytes (packed GUIDs not counted)
- HermesProxy: `SpellFailure` — matches

### SMSG_ACTIVE_GLYPHS (0x2c54)

- Modern: 11348 (0x2c54) · 2.4.3: — · Area: Spell
- Layout: `{ u32 loop[ { u32 u16 } ] u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `ActiveGlyphs`; `EmptyActiveGlyphs` — matches

### SMSG_SPELL_FAILED_OTHER (0x2c55)

- Modern: 11349 (0x2c55) · 2.4.3: 678 (0x2a6) · Area: Spell
- Layout: `{ guid guid u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- HermesProxy: `SpellFailedOther` — matches

### SMSG_SCRIPT_CAST (0x2c56)

- Modern: 11350 (0x2c56) · 2.4.3: — · Area: Spell
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_CAST_FAILED (0x2c57)

- Modern: 11351 (0x2c57) · 2.4.3: 304 (0x130) · Area: Spell
- Layout: `{ guid u32*5 }`
- Size: 20 bytes (packed GUIDs not counted)
- HermesProxy: `CastFailed` — matches

### SMSG_PET_CAST_FAILED (0x2c58)

- Modern: 11352 (0x2c58) · 2.4.3: 312 (0x138) · Area: Spell
- Layout: `{ guid u32*4 }`
- Size: 16 bytes (packed GUIDs not counted)
- HermesProxy: `PetCastFailed` — matches

### SMSG_INTERRUPT_POWER_REGEN (0x2c59)

- Modern: 11353 (0x2c59) · 2.4.3: — · Area: Spell
- Layout: `{ struct }`
- Struct fields the client uses (1 bytes): +0: u8

### SMSG_SPELL_FAILURE_MESSAGE (0x2c5a)

- Modern: 11354 (0x2c5a) · 2.4.3: — · Area: Spell
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_RESYNC_RUNES (0x2c5f)

- Modern: 11359 (0x2c5f) · 2.4.3: — · Area: Spell
- Layout: `{ { u8*2 u32 loop[ u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8
