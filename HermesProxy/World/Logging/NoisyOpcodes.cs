using System.Collections.Frozen;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Logging;

/// <summary>
/// High-volume opcodes that should be demoted from Debug to Verbose for the
/// "Sending opcode ..." / "Received opcode ..." trace lines, so the default
/// debug log isn't drowned by movement spam.
///
/// Membership is checked once per packet via <see cref="FrozenSet{T}"/>;
/// O(1) lookup, zero allocation in the hot path. Add / remove entries here
/// whenever a new opcode turns out to be too chatty for default-level logs.
/// The packets themselves are still logged — just at LogType.Trace, which is
/// gated by Log.Server.MinimumLevel=Verbose and off by default.
/// </summary>
internal static class NoisyOpcodes
{
    private static readonly FrozenSet<Opcode> s_noisy = new HashSet<Opcode>
    {
        // Per-tick monster movement updates — flood the log when many NPCs are
        // visible. Reasonable to silence at Debug; Verbose still shows them.
        Opcode.SMSG_ON_MONSTER_MOVE,
        Opcode.SMSG_MONSTER_MOVE_TRANSPORT,
        // TC fires SMSG_RESYNC_RUNES on every rune-regen tick (~25/sec while any
        // DK rune is recharging). The V3_4_3 client manages its own rune-cooldown
        // visual timer locally and we don't forward these, so the Debug log line
        // is pure noise during DK combat.
        Opcode.SMSG_RESYNC_RUNES,
    }.ToFrozenSet();

    public static bool IsNoisy(Opcode opcode) => s_noisy.Contains(opcode);
}
