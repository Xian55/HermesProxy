using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World;

/// <summary>
/// Thrown when a universal opcode has no numeric mapping for the configured build.
/// Roughly 50 V3_4_3 SMSG opcodes are still unmapped (= 0u in the per-build enum), and the
/// Trace.Assert that used to guard this aborted the whole process from a socket callback,
/// bypassing every handler guard. Throwing instead lets the packet be dropped and logged.
/// </summary>
public sealed class UnmappedOpcodeException : Exception
{
    public Opcode UniversalOpcode { get; }

    /// <summary>True when the missing mapping is on the modern-client side, false for the legacy server.</summary>
    public bool IsModern { get; }

    public UnmappedOpcodeException(Opcode universalOpcode, bool isModern)
        : base($"No {(isModern ? ModernVersion.Build : LegacyVersion.Build)} opcode mapping for {universalOpcode}, packet dropped.")
    {
        UniversalOpcode = universalOpcode;
        IsModern = isModern;
    }
}
