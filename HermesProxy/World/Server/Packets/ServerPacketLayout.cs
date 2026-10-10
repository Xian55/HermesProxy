using System;
using HermesProxy.Enums;

namespace HermesProxy.World.Server.Packets;

/// <summary>
/// One wire layout of a server packet whose layout differs between client builds.
/// </summary>
/// <remarks>
/// Each layout is a type of its own and holds no version check. The packet lists its layouts in a
/// <see cref="ServerPacketLayouts{TLayout}"/>, keeps the one for the running client in a static
/// readonly field, and forwards <c>Write</c>, <c>WriteToSpan</c> and <c>MaxSize</c> to it. Call
/// sites keep constructing the packet class itself.
/// </remarks>
public abstract class ServerPacketLayout<TPacket> where TPacket : ServerPacket
{
    /// <summary>
    /// See <see cref="Framework.IO.ISpanWritable.MaxSize"/>. Overridden by the layouts of packets
    /// that implement <see cref="Framework.IO.ISpanWritable"/>; nothing asks the others.
    /// </summary>
    public virtual int MaxSize => throw new NotSupportedException($"{typeof(TPacket).Name} is not span-writable.");

    public abstract void Write(TPacket packet, WorldPacket data);

    /// <summary>See <see cref="Framework.IO.ISpanWritable.WriteToSpan"/> and <see cref="MaxSize"/>.</summary>
    public virtual int WriteToSpan(TPacket packet, Span<byte> buffer)
        => throw new NotSupportedException($"{typeof(TPacket).Name} is not span-writable.");
}

/// <summary>
/// The layouts of one server packet, each with the client builds it is for.
/// </summary>
/// <remarks>
/// <para>
/// A range is [<c>AddedIn</c>, <c>RemovedIn</c>), compared by expansion, major and minor version,
/// the same rule as the ranges on <c>[PacketCodec]</c>: a layout added in 3.4.3 also serves 4.4.x
/// until a 4.4.x layout takes over with its own range. <see cref="ClientVersionBuild.Zero"/> leaves
/// a side open.
/// </para>
/// <para>
/// The running client's layout is picked once, by the packet's static initializer, so the packet
/// pays a field load and a call the JIT can devirtualise through the static readonly field.
/// <c>ServerPacketLayoutTests</c> checks that every list picks exactly one layout for each
/// supported build, because a gap or an overlap would otherwise surface as a type initialiser
/// exception on the first packet of that kind.
/// </para>
/// </remarks>
public sealed class ServerPacketLayouts<TLayout> where TLayout : class
{
    private readonly (ClientVersionBuild AddedIn, ClientVersionBuild RemovedIn, TLayout Layout)[] _layouts;

    public ServerPacketLayouts(params (ClientVersionBuild AddedIn, ClientVersionBuild RemovedIn, TLayout Layout)[] layouts)
    {
        _layouts = layouts;
    }

    /// <summary>The layout for the running client.</summary>
    public TLayout ForRunningClient()
        => For(ModernVersion.ExpansionVersion, ModernVersion.MajorVersion, ModernVersion.MinorVersion);

    /// <summary>The layout for <paramref name="build"/>.</summary>
    public TLayout For(ClientVersionBuild build)
        => For(VersionChecker.GetExpansionVersion(build), VersionChecker.GetMajorPatchVersion(build), VersionChecker.GetMinorPatchVersion(build));

    private TLayout For(byte expansion, byte major, byte minor)
    {
        TLayout? found = null;
        foreach (var (addedIn, removedIn, layout) in _layouts)
        {
            if (!Holds(addedIn, removedIn, expansion, major, minor))
                continue;

            if (found is not null)
                throw new InvalidOperationException(
                    $"{typeof(TLayout).Name}: {found.GetType().Name} and {layout.GetType().Name} both cover {expansion}.{major}.{minor}.");
            found = layout;
        }

        return found ?? throw new InvalidOperationException(
            $"{typeof(TLayout).Name}: no layout covers {expansion}.{major}.{minor}.");
    }

    private static bool Holds(ClientVersionBuild addedIn, ClientVersionBuild removedIn, byte expansion, byte major, byte minor)
    {
        var version = (expansion, major, minor);
        return (addedIn == ClientVersionBuild.Zero || Compare(version, addedIn) >= 0)
            && (removedIn == ClientVersionBuild.Zero || Compare(version, removedIn) < 0);
    }

    private static int Compare((byte Expansion, byte Major, byte Minor) version, ClientVersionBuild build)
        => (version.Expansion, version.Major, version.Minor).CompareTo(
            (VersionChecker.GetExpansionVersion(build), VersionChecker.GetMajorPatchVersion(build), VersionChecker.GetMinorPatchVersion(build)));
}
