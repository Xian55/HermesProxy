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
    }.ToFrozenSet();

    public static bool IsNoisy(Opcode opcode) => s_noisy.Contains(opcode);
}
