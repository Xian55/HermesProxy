using System;
using HermesProxy.Enums;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Dispatch;

/// <summary>
/// Marks a static system method as the handler for an inbound CMSG from the modern client.
/// </summary>
/// <remarks>
/// <para>
/// The range is <b>literal attribute data</b>, never a predicate. The older
/// <see cref="PacketHandlerAttribute"/> calls <c>LegacyVersion.InVersion(...)</c> inside its own
/// constructor, which a source generator cannot see through — it would have to execute the
/// attribute to learn the opcode. Storing <see cref="AddedIn"/> / <see cref="RemovedIn"/> as
/// constants lets the generator read the range off the symbol and emit the selection as code,
/// resolved once at table build instead of per <c>WorldSocket</c> construction.
/// </para>
/// <para>
/// There are no ranged CMSG sites today. The properties exist because adding a client is
/// currently unbounded on this axis: the modern side has no way at all to say "this handler is
/// for builds ≥ X", so every per-build difference has to hide inside a handler body. See
/// <c>docs/version-shape-dispatch.md</c> and issue #202.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class HandlesCmsgAttribute : Attribute
{
    public HandlesCmsgAttribute(Opcode opcode) => Opcode = opcode;

    public Opcode Opcode { get; }

    /// <summary>First build that sends this shape. <see cref="ClientVersionBuild.Zero"/> = unbounded.</summary>
    public ClientVersionBuild AddedIn { get; init; } = ClientVersionBuild.Zero;

    /// <summary>First build that no longer sends it, exclusive. <see cref="ClientVersionBuild.Zero"/> = unbounded.</summary>
    public ClientVersionBuild RemovedIn { get; init; } = ClientVersionBuild.Zero;
}

/// <summary>
/// Marks a static system method as the handler for an inbound SMSG from the legacy emulator.
/// </summary>
/// <remarks>
/// Same contract as <see cref="HandlesCmsgAttribute"/>, on the legacy axis. This is where the
/// ranges are actually used: 28 sites today, all of them opcodes whose shape changed between
/// vanilla, TBC and WotLK.
/// <para>
/// Direction is carried by the attribute's <i>identity</i> rather than a shared enum member.
/// That is deliberate — the generator targets netstandard2.0 and cannot reference this assembly,
/// so any enum it must interpret has to be mirrored inside it and matched by ordinal, which the
/// SourceGen handbook calls out as the sharpest footgun in the project. Two attribute types
/// means the generator matches a fully-qualified name and mirrors nothing.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class HandlesSmsgAttribute : Attribute
{
    public HandlesSmsgAttribute(Opcode opcode) => Opcode = opcode;

    public Opcode Opcode { get; }

    /// <inheritdoc cref="HandlesCmsgAttribute.AddedIn"/>
    public ClientVersionBuild AddedIn { get; init; } = ClientVersionBuild.Zero;

    /// <inheritdoc cref="HandlesCmsgAttribute.RemovedIn"/>
    public ClientVersionBuild RemovedIn { get; init; } = ClientVersionBuild.Zero;
}
