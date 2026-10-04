using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

/// <summary>
/// What a legacy movement block carried that <see cref="MovementInfo"/> has no place for.
/// </summary>
/// <param name="Flags">
/// The flags as sent, normalised to the 3.3.5a vocabulary. The modern flags have no bit for
/// OnTransport or SplineEnabled, so this is where they survive.
/// </param>
/// <param name="FixedZ">
/// Vanilla's MOVEFLAG_FIXED_Z, which no later vocabulary names. A create turns it into the hover
/// animation bit.
/// </param>
public readonly record struct LegacyMovementExtras(MovementFlagWotLK Flags, bool FixedZ);

/// <summary>
/// The movement block as a 1.12, 2.4.3 or 3.3.5a server reads and writes it.
/// </summary>
/// <remarks>
/// The overloads without a layout use the process's legacy build and are the ones production
/// calls. The overloads that take one exist for tests; see <c>MovementLayouts.cs</c> for why the
/// layout is a type parameter underneath.
/// </remarks>
public static class LegacyMovementCodec
{
    /// <summary>
    /// Reads a movement block. The flags come back in the modern vocabulary but otherwise as the
    /// server sent them: run the result through <see cref="MovementSanitizer"/> before a modern
    /// client sees it, or use <see cref="ReadForClient"/>.
    /// </summary>
    /// <param name="legacy">What the block said that <paramref name="info"/> has no place for.</param>
    public static void Read(WorldPacket packet, GameSessionData gameState, out MovementInfo info, out LegacyMovementExtras legacy)
        => Read(LegacyMovementLayouts.Current, packet, gameState, out info, out legacy);

    /// <summary>
    /// <see cref="Read(WorldPacket, GameSessionData, out MovementInfo, out LegacyMovementExtras)"/>
    /// for the handlers that forward every block they read: decoded and repaired in one step.
    /// </summary>
    public static void ReadForClient(WorldPacket packet, GameSessionData gameState, out MovementInfo info)
    {
        Read(LegacyMovementLayouts.Current, packet, gameState, out info, out _);
        MovementSanitizer.Sanitize(ref info);
    }

    /// <summary>
    /// The flags of the block at the read position, normalised to the 3.3.5a vocabulary, without
    /// reading it. Lets a handler decide to drop a packet before it pays to decode one.
    /// </summary>
    public static MovementFlagWotLK PeekFlags(WorldPacket packet)
        => PeekFlags(LegacyMovementLayouts.Current, packet);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MovementFlagWotLK PeekFlags(LegacyMovementLayout layout, WorldPacket packet)
    {
        // Every layout leads with the 32-bit flags.
        uint wire = BinaryPrimitives.ReadUInt32LittleEndian(packet.GetRemainingSpan());
        return layout switch
        {
            LegacyMovementLayout.WotLK => (MovementFlagWotLK)wire,
            LegacyMovementLayout.Tbc => ((MovementFlagTBC)wire).CastFlags<MovementFlagTBC, MovementFlagWotLK>(),
            _ => ((MovementFlagVanilla)wire).CastFlags<MovementFlagVanilla, MovementFlagWotLK>(),
        };
    }

    // Inlined so that the caller's layout, a static readonly in production, folds the switch away.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(LegacyMovementLayout layout, WorldPacket packet, GameSessionData gameState, out MovementInfo info, out LegacyMovementExtras legacy)
    {
        switch (layout)
        {
            case LegacyMovementLayout.WotLK:
                ReadCore<WotLKMovementLayout>(packet, gameState, out info, out legacy);
                break;
            case LegacyMovementLayout.Tbc:
                ReadCore<TbcMovementLayout>(packet, gameState, out info, out legacy);
                break;
            default:
                ReadCore<VanillaMovementLayout>(packet, gameState, out info, out legacy);
                break;
        }
    }

    private static void ReadCore<TLayout>(WorldPacket packet, GameSessionData gameState, out MovementInfo info, out LegacyMovementExtras legacy)
        where TLayout : struct, ILegacyMovementLayout
    {
        // Every decision below is made on the flags normalised to the 3.3.5a vocabulary, except
        // the pitch: which flags put it on the wire differs by era, and TBC's Flying2 has no
        // 3.3.5a name to survive the normalisation under.
        MovementFlagWotLK flags;
        uint flagsExtra = 0;
        bool fixedZ = false;
        bool hasPitch;
        if (TLayout.Era == LegacyMovementLayout.WotLK)
        {
            flags = (MovementFlagWotLK)packet.ReadUInt32();
            flagsExtra = packet.ReadUInt16();
            hasPitch = flags.HasAnyFlag(MovementFlagWotLK.Swimming | MovementFlagWotLK.Flying)
                       || flagsExtra.HasAnyFlag((uint)MovementFlagExtra.AlwaysAllowPitching);
        }
        else if (TLayout.Era == LegacyMovementLayout.Tbc)
        {
            MovementFlagTBC wire = (MovementFlagTBC)packet.ReadUInt32();
            flags = wire.CastFlags<MovementFlagTBC, MovementFlagWotLK>();
            flagsExtra = packet.ReadUInt8();
            hasPitch = wire.HasAnyFlag(MovementFlagTBC.Swimming | MovementFlagTBC.Flying2);
        }
        else
        {
            MovementFlagVanilla wire = (MovementFlagVanilla)packet.ReadUInt32();
            flags = wire.CastFlags<MovementFlagVanilla, MovementFlagWotLK>();
            hasPitch = wire.HasAnyFlag(MovementFlagVanilla.Swimming);
            fixedZ = wire.HasAnyFlag(MovementFlagVanilla.FixedZ);
        }

        // Written field by field as it is read. The destination may be a reused variable, so it
        // is cleared first: every field a block does not carry has to come out as its default.
        info = default;
        info.FlagsExtra = flagsExtra;
        info.MoveTime = packet.ReadUInt32();
        info.Position = packet.ReadVector3();
        info.Orientation = packet.ReadFloat();

        if (flags.HasAnyFlag(MovementFlagWotLK.OnTransport))
        {
            WowGuid128 guid = (TLayout.PackedTransportGuid ? packet.ReadPackedGuid() : packet.ReadGuid()).To128(gameState);
            Vector3 offset = packet.ReadVector3();
            float transportOrientation = packet.ReadFloat();
            uint time = TLayout.HasTransportTime ? packet.ReadUInt32() : 0u;
            sbyte seat = TLayout.HasTransportSeat ? packet.ReadInt8() : (sbyte)-1;
            // Not gated on the era: only 3.x has extra flags wide enough to hold the bit.
            uint prevTime = flagsExtra.HasAnyFlag((uint)MovementFlagExtra.InterpolateMove) ? packet.ReadUInt32() : 0u;

            info.Transport = new TransportInfo
            {
                Guid = guid,
                Offset = offset,
                Orientation = transportOrientation,
                Time = time,
                PrevTime = prevTime,
                Seat = seat,
            };
        }

        if (hasPitch)
            info.SwimPitch = packet.ReadFloat();

        info.FallTime = packet.ReadUInt32();
        if (flags.HasAnyFlag(MovementFlagWotLK.Falling))
        {
            info.JumpVerticalSpeed = packet.ReadFloat();
            info.JumpSinAngle = packet.ReadFloat();
            info.JumpCosAngle = packet.ReadFloat();
            info.JumpHorizontalSpeed = packet.ReadFloat();
        }

        if (flags.HasAnyFlag(MovementFlagWotLK.SplineElevation))
            info.SplineElevation = packet.ReadFloat();

        info.Flags = flags.CastFlags<MovementFlagWotLK, MovementFlagModern>();
        legacy = new LegacyMovementExtras(flags, fixedZ);
    }

    /// <summary>Writes a movement block the modern client reported, for the legacy server.</summary>
    public static void Write(WorldPacket data, in MovementInfo info)
        => Write(LegacyMovementLayouts.Current, data, in info);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(LegacyMovementLayout layout, WorldPacket data, in MovementInfo info)
    {
        switch (layout)
        {
            case LegacyMovementLayout.WotLK:
                WriteCore<WotLKMovementLayout>(data, in info);
                break;
            case LegacyMovementLayout.Tbc:
                WriteCore<TbcMovementLayout>(data, in info);
                break;
            default:
                WriteCore<VanillaMovementLayout>(data, in info);
                break;
        }
    }

    private static void WriteCore<TLayout>(WorldPacket data, in MovementInfo info)
        where TLayout : struct, ILegacyMovementLayout
    {
        // The same flags sit on different bits in each era, so everything the layout hangs off a
        // flag is resolved to that era's bit here, once.
        uint flags;
        uint onTransport;
        uint pitching;
        uint falling;
        uint splineElevation;
        if (TLayout.Era == LegacyMovementLayout.WotLK)
        {
            flags = (uint)info.Flags.CastFlags<MovementFlagModern, MovementFlagWotLK>();
            onTransport = (uint)MovementFlagWotLK.OnTransport;
            pitching = (uint)(MovementFlagWotLK.Swimming | MovementFlagWotLK.Flying);
            falling = (uint)MovementFlagWotLK.Falling;
            splineElevation = (uint)MovementFlagWotLK.SplineElevation;
        }
        else if (TLayout.Era == LegacyMovementLayout.Tbc)
        {
            flags = (uint)info.Flags.CastFlags<MovementFlagModern, MovementFlagTBC>();
            onTransport = (uint)MovementFlagTBC.OnTransport;
            pitching = (uint)(MovementFlagTBC.Swimming | MovementFlagTBC.Flying2);
            falling = (uint)MovementFlagTBC.Falling;
            splineElevation = (uint)MovementFlagTBC.SplineElevation;
        }
        else
        {
            flags = (uint)info.Flags.CastFlags<MovementFlagModern, MovementFlagVanilla>();
            onTransport = (uint)MovementFlagVanilla.OnTransport;
            pitching = (uint)MovementFlagVanilla.Swimming;
            falling = (uint)MovementFlagVanilla.Falling;
            splineElevation = (uint)MovementFlagVanilla.SplineElevation;
        }

        // The modern flags have no OnTransport bit; riding something is what says so.
        if (info.TransportGuid != default)
            flags |= onTransport;

        data.WriteUInt32(flags);
        if (TLayout.Era == LegacyMovementLayout.WotLK)
            data.WriteUInt16((ushort)info.FlagsExtra);
        else if (TLayout.Era == LegacyMovementLayout.Tbc)
            data.WriteUInt8((byte)info.FlagsExtra);

        data.WriteUInt32(info.MoveTime);
        data.WriteVector3(info.Position);
        data.WriteFloat(info.Orientation);

        if ((flags & onTransport) != 0)
        {
            TransportInfo transport = info.Transport.GetValueOrDefault();
            if (TLayout.PackedTransportGuid)
                data.WritePackedGuid(transport.Guid.To64());
            else
                data.WriteGuid(transport.Guid.To64());

            data.WriteVector3(transport.Offset);
            data.WriteFloat(transport.Orientation);

            if (TLayout.HasTransportTime)
                data.WriteUInt32(transport.Time);

            if (TLayout.HasTransportSeat)
            {
                data.WriteInt8(transport.Seat);
                if (info.FlagsExtra.HasAnyFlag((uint)MovementFlagExtra.InterpolateMove))
                    data.WriteUInt32(transport.PrevTime);
            }
        }

        bool hasPitch = (flags & pitching) != 0
                        || (TLayout.Era == LegacyMovementLayout.WotLK && info.FlagsExtra.HasAnyFlag((uint)MovementFlagExtra.AlwaysAllowPitching));
        if (hasPitch)
            data.WriteFloat(info.SwimPitch);

        data.WriteUInt32(info.FallTime);

        if ((flags & falling) != 0)
        {
            data.WriteFloat(info.JumpVerticalSpeed);
            data.WriteFloat(info.JumpSinAngle);
            data.WriteFloat(info.JumpCosAngle);
            data.WriteFloat(info.JumpHorizontalSpeed);
        }

        if ((flags & splineElevation) != 0)
            data.WriteFloat(info.SplineElevation);
    }
}
