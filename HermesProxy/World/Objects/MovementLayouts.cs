using HermesProxy.Enums;

namespace HermesProxy.World.Objects;

// A movement block's layout differs by build on both sides of the proxy. Each layout is described
// once, as a struct of constants, and the codecs are generic over it: the JIT compiles one copy
// of a reader or writer per layout with the constants folded in, so the copy the process runs is
// straight-line code with every other layout's branches gone.
//
// That folding is the reason the layouts are types and not values. A layout passed as an
// argument keeps every branch alive, which also costs the inlining of what is inside them: the
// modern write measured 34.6 ns that way against 24.8 ns folded (Apple M4).
// A static readonly struct passed by `in` does not fold either, even with the callee inlined.
//
// The enums are how a layout is named at run time: resolved once per process for the hot path,
// and passed explicitly by tests, which is what lets one test process exercise every layout.

/// <summary>Which movement block layout a legacy build uses.</summary>
public enum LegacyMovementLayout : byte
{
    /// <summary>1.12: 32-bit flags, no extra flags, a bare transport part.</summary>
    Vanilla,

    /// <summary>2.4.3: one byte of extra flags, and a time in the transport part.</summary>
    Tbc,

    /// <summary>3.3.5a: two bytes of extra flags, a packed transport guid, and a seat.</summary>
    WotLK,
}

/// <summary>Which movement block layout a modern build uses.</summary>
public enum ModernMovementLayout : byte
{
    /// <summary>1.14.0, 2.5.2: flags as 30 + 18 bits ahead of the header bits.</summary>
    BitFlags,

    /// <summary>1.14.1 and 2.5.3 on: flags as three leading 32-bit words, and a HasInertia bit.</summary>
    WordFlags,

    /// <summary>3.4.3: <see cref="WordFlags"/> plus two header bits, and extra flags that are not forwarded.</summary>
    WotLKClassic,
}

public static class LegacyMovementLayouts
{
    /// <summary>The layout of the legacy build this process talks to.</summary>
    public static readonly LegacyMovementLayout Current = For(LegacyVersion.Build);

    public static LegacyMovementLayout For(ClientVersionBuild build)
    {
        // Raw build numbers order correctly here: every legacy build is on the original release
        // line, where they only ever went up.
        if (build >= ClientVersionBuild.V3_0_2_9056)
            return LegacyMovementLayout.WotLK;
        if (build >= ClientVersionBuild.V2_0_1_6180)
            return LegacyMovementLayout.Tbc;
        return LegacyMovementLayout.Vanilla;
    }
}

public static class ModernMovementLayouts
{
    /// <summary>The layout of the modern build this process serves.</summary>
    public static readonly ModernMovementLayout Current = For(ModernVersion.Build);

    /// <summary>
    /// The one place a modern build is mapped to a movement layout, so a new client is one new
    /// arm here and one new layout struct below.
    /// </summary>
    public static ModernMovementLayout For(ClientVersionBuild build)
    {
        // An exact build, deliberately. 3.4.4 (59817) adds a ninth header bit, HasDriveStatus, so
        // a range here would hand it the 3.4.3 layout and misalign every packet after the bits.
        if (build == ClientVersionBuild.V3_4_3_54261)
            return ModernMovementLayout.WotLKClassic;

        byte expansion = VersionChecker.GetExpansionVersion(build);
        byte major = VersionChecker.GetMajorPatchVersion(build);
        byte minor = VersionChecker.GetMinorPatchVersion(build);

        bool flagsAreWords = VersionChecker.GetBranch(expansion, major) switch
        {
            ClientBranch.ClassicEra => AtLeast(expansion, major, minor, 1, 14, 1),
            ClientBranch.Classic => AtLeast(expansion, major, minor, 2, 5, 3),
            _ => AtLeast(expansion, major, minor, 9, 2, 0),
        };

        return flagsAreWords ? ModernMovementLayout.WordFlags : ModernMovementLayout.BitFlags;
    }

    private static bool AtLeast(byte expansion, byte major, byte minor, byte minExpansion, byte minMajor, byte minMinor)
    {
        if (expansion != minExpansion)
            return expansion > minExpansion;
        if (major != minMajor)
            return major > minMajor;
        return minor >= minMinor;
    }
}

internal interface ILegacyMovementLayout
{
    /// <summary>Selects the flag vocabulary and the width of the extra flags.</summary>
    static abstract LegacyMovementLayout Era { get; }

    /// <summary>3.1.0 on: the transport guid is packed rather than 8 bytes.</summary>
    static abstract bool PackedTransportGuid { get; }

    /// <summary>2.0.1 on.</summary>
    static abstract bool HasTransportTime { get; }

    /// <summary>
    /// 3.0.2 on: the transport part carries a seat, and a second time behind the InterpolateMove
    /// extra flag.
    /// </summary>
    static abstract bool HasTransportSeat { get; }
}

internal readonly struct VanillaMovementLayout : ILegacyMovementLayout
{
    public static LegacyMovementLayout Era => LegacyMovementLayout.Vanilla;
    public static bool PackedTransportGuid => false;
    public static bool HasTransportTime => false;
    public static bool HasTransportSeat => false;
}

internal readonly struct TbcMovementLayout : ILegacyMovementLayout
{
    public static LegacyMovementLayout Era => LegacyMovementLayout.Tbc;
    public static bool PackedTransportGuid => false;
    public static bool HasTransportTime => true;
    public static bool HasTransportSeat => false;
}

internal readonly struct WotLKMovementLayout : ILegacyMovementLayout
{
    public static LegacyMovementLayout Era => LegacyMovementLayout.WotLK;
    public static bool PackedTransportGuid => true;
    public static bool HasTransportTime => true;
    public static bool HasTransportSeat => true;
}

internal interface IModernMovementLayout
{
    /// <summary>
    /// The flags lead the block as three 32-bit words. Otherwise they are 30 + 18 bits ahead of
    /// the header bits, and there is no third word.
    /// </summary>
    static abstract bool FlagsAreWords { get; }

    static abstract bool HasInertia { get; }

    /// <summary>A header bit ahead of HasTransport, and the guid after the transport part.</summary>
    static abstract bool HasStandingOnGameObject { get; }

    /// <summary>The last header bit, and two velocities ahead of the fall part.</summary>
    static abstract bool HasAdvFlying { get; }

    /// <summary>
    /// A 3.3.5a server's extra flags share their numeric space with a different 3.4.3 enum: legacy
    /// 0x200 (AlwaysAllowPitching) is 3.4.3's VehiclePassengerIsTransitionAllowed, and forwarding
    /// it tells the client the player is mid vehicle transition, which stalls world entry. None of
    /// the 3.3.5a bits has a known 3.4.3 equivalent, so that client is sent zero.
    /// </summary>
    static abstract bool WritesExtraFlagsAsZero { get; }
}

internal readonly struct BitFlagsMovementLayout : IModernMovementLayout
{
    public static bool FlagsAreWords => false;
    public static bool HasInertia => false;
    public static bool HasStandingOnGameObject => false;
    public static bool HasAdvFlying => false;
    public static bool WritesExtraFlagsAsZero => false;
}

internal readonly struct WordFlagsMovementLayout : IModernMovementLayout
{
    public static bool FlagsAreWords => true;
    public static bool HasInertia => true;
    public static bool HasStandingOnGameObject => false;
    public static bool HasAdvFlying => false;
    public static bool WritesExtraFlagsAsZero => false;
}

internal readonly struct WotLKClassicMovementLayout : IModernMovementLayout
{
    public static bool FlagsAreWords => true;
    public static bool HasInertia => true;
    public static bool HasStandingOnGameObject => true;
    public static bool HasAdvFlying => true;
    public static bool WritesExtraFlagsAsZero => true;
}
