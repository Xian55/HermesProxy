#!/usr/bin/env python3
"""Generate the 4.3.4 movement element sequences from TrinityCore 4.3.4.

A 4.3.4 movement packet has no fixed layout: each opcode lists its fields, presence bits and
masked-GUID bytes in its own order (TrinityCore 4.3.4 MovementStructures.cpp, one array per
opcode). This turns those arrays into HermesProxy/World/Objects/LegacyMovementSequencesCata.cs,
keyed by HermesProxy's opcode names. TrinityCore's names often differ from ours, so each case
label is resolved through its opcode value in TrinityCore's Opcodes.h and our 4.3.4 table.

Usage
-----
    python scripts/gen-movement-sequences-434.py <TrinityCore 4.3.4 checkout>
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
OUT = REPO / 'HermesProxy' / 'World' / 'Objects' / 'LegacyMovementSequencesCata.cs'


# Movement packets TrinityCore 4.3.4 writes by hand (src/server/game/Server/Packets/MovementPackets.cpp)
# instead of listing them in MovementStructures.cpp: their order transcribed from that writer, with
# the same meaning per element as the listed ones (a presence bit the writer negates is the same
# element as in the arrays).
HAND_WRITTEN = {
    # MoveUpdateTeleport::Write: what the players around see of a unit that teleported
    'MoveUpdateTeleport': ('SMSG_MOVE_UPDATE_TELEPORT', [
        'PositionZ', 'PositionY', 'PositionX',
        'HasOrientation', 'HasSpline', 'HasMovementFlags', 'HasGuidByte2', 'HasGuidByte4', 'HasGuidByte6',
        'HasFallData', 'HasGuidByte0', 'HasTransportData', 'HasGuidByte5',
        'HasTransportGuidByte1', 'HasTransportGuidByte4', 'HasTransportGuidByte5', 'HasTransportGuidByte3',
        'HasTransportGuidByte0', 'HasTransportTime2', 'HasTransportGuidByte7', 'HasTransportGuidByte6',
        'HasVehicleId', 'HasTransportGuidByte2',
        'HasHeightChangeFailed', 'HasGuidByte7', 'HasGuidByte3',
        'HasPitch', 'HasMovementFlags2', 'HasTimestamp',
        'HasFallDirection', 'MovementFlags2', 'HasSplineElevation', 'MovementFlags', 'HasGuidByte1',
        'FlushBits',
        'GuidByte7',
        'TransportGuidByte3', 'TransportGuidByte4', 'TransportOrientation', 'TransportVehicleId',
        'TransportGuidByte1', 'TransportTime2', 'TransportPositionZ', 'TransportGuidByte7',
        'TransportGuidByte0', 'TransportGuidByte6', 'TransportGuidByte5', 'TransportGuidByte2',
        'TransportSeat', 'TransportTime', 'TransportPositionY', 'TransportPositionX',
        'GuidByte6', 'Pitch', 'SplineElevation', 'Orientation', 'GuidByte2', 'GuidByte3', 'GuidByte1',
        'FallTime', 'FallHorizontalSpeed', 'FallCosAngle', 'FallSinAngle', 'FallVerticalSpeed',
        'GuidByte5', 'GuidByte4', 'Timestamp', 'GuidByte0',
        'End',
    ]),
}


def enum_values(text: str) -> dict[str, int]:
    return {m.group(1): int(m.group(2), 0)
            for m in re.finditer(r'^\s*([A-Z][A-Z0-9_]+)\s*=\s*(0x[0-9A-Fa-f]+|\d+)', text, re.M)}


def main() -> None:
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    tc = Path(sys.argv[1])
    structures = (tc / 'src/server/game/Movement/MovementStructures.cpp').read_text(encoding='utf-8')
    header = (tc / 'src/server/game/Movement/MovementStructures.h').read_text(encoding='utf-8')
    tc_opcodes = enum_values((tc / 'src/server/game/Server/Protocol/Opcodes.h').read_text(encoding='utf-8'))
    ours = enum_values((REPO / 'HermesProxy/World/Enums/V4_3_4_15595/Opcode.cs').read_text(encoding='utf-8-sig'))
    ours_by_value: dict[int, list[str]] = {}
    for name, value in ours.items():
        ours_by_value.setdefault(value, []).append(name)

    # The enum's comments name elements too ("Allowed internal elements are: ..."), so drop
    # comments from both the enum and the arrays before collecting names.
    header = re.sub(r'//[^\n]*', '', header)
    structures = re.sub(r'//[^\n]*', '', structures)
    elements = re.search(r'enum MovementStatusElements\s*\{(.*?)\};', header, re.S).group(1)
    element_names = [re.sub(r'^MSE', '', e) for e in re.findall(r'\b(MSE\w+)', elements)]

    arrays = {m.group(1): re.findall(r'\bMSE(\w+)', m.group(2))
              for m in re.finditer(r'MovementStatusElements (?:const )?(\w+)\[\]\s*=\s*\{(.*?)\};', structures, re.S)}
    switch = structures[structures.index('GetMovementStatusElementsSequence(uint32 opcode)'):]
    cases = re.findall(r'case (\w+):\s*return (\w+);', switch)

    entries: list[tuple[str, str]] = []
    unresolved: list[str] = []
    for tc_name, array in cases:
        value = tc_opcodes.get(tc_name)
        names = ours_by_value.get(value, []) if value else []
        if not names:
            unresolved.append(tc_name)
            continue
        for name in names:
            entries.append((name, array))

    for array, (name, sequence) in HAND_WRITTEN.items():
        unknown = [e for e in sequence if e not in element_names]
        if unknown or name not in ours or array in arrays:
            sys.exit(f'hand-written {array}: unknown elements {unknown}, or no opcode {name}, or listed already')
        arrays[array] = sequence
        entries.append((name, array))

    used = sorted({a for _, a in entries})
    lines = [
        '// <auto-generated>',
        '// Generated by scripts/gen-movement-sequences-434.py from TrinityCore 4.3.4',
        '// (Cataclysm Preservation Project) src/server/game/Movement/MovementStructures.cpp.',
        '// and, for the packets it writes by hand, from those writers (HAND_WRITTEN in the script).',
        '// Do not edit by hand; re-run the script.',
        '// </auto-generated>',
        '',
        'using System.Collections.Frozen;',
        'using System.Collections.Generic;',
        'using HermesProxy.World.Enums;',
        '',
        'namespace HermesProxy.World.Objects;',
        '',
        '/// <summary>One step of a 4.3.4 movement packet: a presence bit, a field or a GUID byte.</summary>',
        'public enum LegacyMovementElement : byte',
        '{',
    ]
    lines += [f'    {e},' for e in element_names]
    lines += [
        '}',
        '',
        '/// <summary>',
        '/// The order a 4.3.4 server reads (and writes) each movement opcode in. Every opcode has',
        '/// its own order; see <see cref="LegacyMovementCata"/> for what each element means.',
        '/// </summary>',
        'public static class LegacyMovementSequencesCata',
        '{',
        '    // The arrays come first: static fields initialise in textual order, and a dictionary',
        '    // declared above them would capture every one of them as null.',
    ]
    for array in used:
        body = ', '.join(f'LegacyMovementElement.{e}' for e in arrays[array])
        lines.append(f'    private static readonly LegacyMovementElement[] {array} = [{body}];')
    lines += [
        '',
        '    public static readonly FrozenDictionary<Opcode, LegacyMovementElement[]> ByOpcode =',
        '        new Dictionary<Opcode, LegacyMovementElement[]>',
        '        {',
    ]
    lines += [f'            [Opcode.{name}] = {array},' for name, array in entries]
    lines += ['        }.ToFrozenDictionary();', '}', '']
    OUT.write_text('\n'.join(lines).replace('\n', '\r\n'), encoding='utf-8')
    print(f'wrote {OUT.relative_to(REPO)}: {len(entries)} opcodes, {len(used)} sequences')
    if unresolved:
        print('no HermesProxy name for:', ', '.join(unresolved))


if __name__ == '__main__':
    main()
