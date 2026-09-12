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

/// <summary>
/// Marks a codec as the reader for <paramref name="packetType"/> over a build range.
/// </summary>
/// <remarks>
/// <para>
/// Without this a packet has exactly one codec, found by convention (<c>Foo</c> ⇒ <c>FooCodec</c>),
/// and any per-build difference in its layout has to live as an <c>if (ModernVersion.Build == …)</c>
/// inside the read. That is the shape <c>docs/version-shape-dispatch.md</c> argues against: the
/// branch is re-evaluated on every packet, and — because it is exact equality — a build the code
/// has never seen silently takes the <c>else</c>, which is the V1_14/V2_5 layout. That is the
/// cliff issue #202 walks off.
/// </para>
/// <para>
/// Declaring the range as data instead lets the generator emit one thunk per candidate and choose
/// between them once, when the table is built. The per-packet branch disappears, and a new client
/// becomes a new codec plus a range rather than an edit to every reader.
/// </para>
/// <para>
/// A packet with no <c>[PacketCodec]</c> anywhere keeps the convention lookup, so this is opt-in
/// per packet.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PacketCodecAttribute : Attribute
{
    public PacketCodecAttribute(Type packetType) => PacketType = packetType;

    public Type PacketType { get; }

    /// <inheritdoc cref="HandlesCmsgAttribute.AddedIn"/>
    public ClientVersionBuild AddedIn { get; init; } = ClientVersionBuild.Zero;

    /// <inheritdoc cref="HandlesCmsgAttribute.RemovedIn"/>
    public ClientVersionBuild RemovedIn { get; init; } = ClientVersionBuild.Zero;

    /// <summary>
    /// Which side's build the range is measured against. Packet *shape* on the modern side is
    /// chosen by the client build; on the legacy side by the server build.
    /// </summary>
    public bool AgainstLegacyVersion { get; init; }
}
