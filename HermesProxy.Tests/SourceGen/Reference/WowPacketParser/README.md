# WowPacketParser reference

`UpdateFieldsHandler343.cs.txt` is a byte-for-byte copy of
`WowPacketParserModule.V3_4_0_45166/Parsers/UpdateFieldsHandler343.cs` from
https://github.com/TrinityCore/WowPacketParser, which is GPL-3.0 like HermesProxy.

- Copied at WPP commit ea494ff6ca2a6478d71cdc38fb2dc0fa650ea7b8 (2026-08-30)
- Last upstream change to the file: c1b9ab6275bae29efb80a82a4dcfd0596d8ab1cc (2024-12-18)

It is WPP's update-field reader for the 3.4.3.54261 client. `UpdateFieldWppConformanceTests`
parses it with Roslyn (syntax only, never compiled). It then checks every `[DescriptorUpdateField]`,
`[DescriptorCustomField]` and `[DescriptorMaskPreamble]` under `HermesProxy/World/Enums/V3_4_3_54261`
against it: bit, parent bit, array size and value width.

TrinityCore's `wotlk_classic` `UpdateFields.h` would be easier to parse, but it can't be used. It
tracks a later 3.4.x client: it puts `NpcFlags` at bits 8/9, where 3.4.3.54261 has them at 113 and
114+i.

To refresh, copy the file again, keep the `.txt` extension, update the two commits above, and run
the tests.
