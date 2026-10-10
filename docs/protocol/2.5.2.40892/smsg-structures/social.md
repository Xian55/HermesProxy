# Social — server packet layouts, 2.5.2.40892

### — (0x303e)

- Modern: 12350 (0x303e) · 2.4.3: — · Area: Social
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### — (0x303f)

- Modern: 12351 (0x303f) · 2.4.3: — · Area: Social
- Layout: `{ struct }`
- Struct fields the client uses (6 bytes): +0: u32, +4: u16

### — (0x3040)

- Modern: 12352 (0x3040) · 2.4.3: — · Area: Social
- Layout: `{ u8*2 { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3041)

- Modern: 12353 (0x3041) · 2.4.3: — · Area: Social
- Layout: `{ u8*4 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 7 · 1 · 1 · 7 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3042)

- Modern: 12354 (0x3042) · 2.4.3: — · Area: Social
- Layout: `{ u8*4 u16 u32 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 7 · 1 · 1 · 7 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_TWITTER_STATUS (0x3043)

- Modern: 12355 (0x3043) · 2.4.3: — · Area: Social
- Layout: `{ u8*4 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 7 · 1 · 1 · 7 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
