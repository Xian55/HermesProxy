# Area trigger — server packet layouts, 3.4.3.54261

### — (0x28fd)

- Modern: 10493 (0x28fd) · 3.3.5a: — · Area: Area trigger
- Layout: `{ guid u8 opt[ { u32*2 u8*2 loop[ u32*3 ] } ] opt[ u32*4 ] opt[ { u8 u32*7 opt[ guid ] opt[ u32*3 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · 1 · (5 unused)

### — (0x2900)

- Modern: 10496 (0x2900) · 3.3.5a: — · Area: Area trigger
- Layout: `guid u32*4 { u32*2 u8*2 loop[ u32*3 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 26: 8 · 3 · 5

### — (0x2901)

- Modern: 10497 (0x2901) · 3.3.5a: — · Area: Area trigger
- Layout: `guid { u32*2 u8*2 loop[ u32*3 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 3 · 5

### — (0x2902)

- Modern: 10498 (0x2902) · 3.3.5a: — · Area: Area trigger
- Layout: `guid { u32 u32 u32*2 loop[ u32*2 ] loop[ u32*2 ] }`

### — (0x2903)

- Modern: 10499 (0x2903) · 3.3.5a: — · Area: Area trigger
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
