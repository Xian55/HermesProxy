# Area trigger — server packet layouts, 4.4.2.60895

### SMSG_AREA_TRIGGER_RE_PATH (0x3c0003)

- Modern: 3932163 (0x3c0003) · 4.3.4: — · Area: Area trigger
- Layout: `{ guid guid u8 opt[ { u32*2 u8*2 loop[ u32*3 ] } ] opt[ { u32*4 } ] opt[ { u8 u32*7 opt[ guid ] opt[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · 1 · 1 · (5 unused)

### SMSG_AREA_TRIGGER_FORCE_SET_POSITION_AND_FACING (0x3c0006)

- Modern: 3932166 (0x3c0006) · 4.3.4: — · Area: Area trigger
- Layout: `guid guid u32*4 { u32*2 u8*2 loop[ u32*3 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 28: 16

### SMSG_AREA_TRIGGER_UNATTACH (0x3c0007)

- Modern: 3932167 (0x3c0007) · 4.3.4: — · Area: Area trigger
- Layout: `guid { u32*2 u8*2 loop[ u32*3 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 16

### SMSG_AREA_TRIGGER_RE_SHAPE (0x3c0008)

- Modern: 3932168 (0x3c0008) · 4.3.4: — · Area: Area trigger
- Layout: `guid { u32*4 loop[ u32*2 ] loop[ u32*2 ] }`

### SMSG_AREA_TRIGGER_DENIED (0x3c0009)

- Modern: 3932169 (0x3c0009) · 4.3.4: — · Area: Area trigger
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
