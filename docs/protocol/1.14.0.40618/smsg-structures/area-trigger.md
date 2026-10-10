# Area trigger — server packet layouts, 1.14.0.40618

### SMSG_AREA_TRIGGER_RE_PATH (0x28fd)

- Modern: 10493 (0x28fd) · 1.12.1: — · Area: Area trigger
- Layout: `{ guid u8 opt[ { u32*2 u8*2 loop[ u32*3 ] } ] opt[ { u32*4 } ] opt[ { u8 u32*7 opt[ guid ] opt[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · 1 · (5 unused)

### SMSG_AREA_TRIGGER_PLAY_SPELL_VISUAL (0x28fe)

- Modern: 10494 (0x28fe) · 1.12.1: — · Area: Area trigger
- Layout: `{ guid u32*2 u8 }`
- Size: 9 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 10: 1 · (7 unused)

### SMSG_AREA_TRIGGER_FORCE_SET_POSITION_AND_FACING (0x2900)

- Modern: 10496 (0x2900) · 1.12.1: — · Area: Area trigger
- Layout: `{ guid u32*4 { u32*2 u8*2 loop[ u32*3 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 26: 8 · 3 · 5

### SMSG_AREA_TRIGGER_UNATTACH (0x2901)

- Modern: 10497 (0x2901) · 1.12.1: — · Area: Area trigger
- Layout: `{ guid { u32*2 u8*2 loop[ u32*3 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 3 · 5

### SMSG_AREA_TRIGGER_RE_SHAPE (0x2902)

- Modern: 10498 (0x2902) · 1.12.1: — · Area: Area trigger
- Layout: `{ guid { u32 u32 u32*2 loop[ u32*2 ] loop[ u32*2 ] } }`

### SMSG_AREA_TRIGGER_DENIED (0x2903)

- Modern: 10499 (0x2903) · 1.12.1: — · Area: Area trigger
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
