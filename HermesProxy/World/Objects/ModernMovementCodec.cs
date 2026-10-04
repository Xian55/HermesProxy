using System;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

/// <summary>
/// The movement block as a 1.14, 2.5 or 3.4.3 client reads and writes it.
/// </summary>
/// <remarks>
/// <para>
/// One reader and one writer, both over a span. The <see cref="WorldPacket"/> overloads run the
/// same code against the packet's bytes rather than restating the layout: the block starts and
/// ends on a byte boundary, so it can be read or written as a unit wherever it sits.
/// </para>
/// <para>
/// The overloads without a layout use the process's modern build and are the ones production
/// calls. The overloads that take one exist for tests; see <c>MovementLayouts.cs</c> for why the
/// layout is a type parameter underneath.
/// </para>
/// </remarks>
public static class ModernMovementCodec
{
    /// <summary>
    /// Room for the largest block <see cref="Write(Span{byte}, WowGuid128, in MovementInfo)"/>
    /// can produce: mover guid 18, flags 12, times and position 40, header bits 1, transport 48,
    /// fall 21. Typical blocks are 54 to 75 bytes.
    /// </summary>
    public const int MaxSize = 192;

    private const int MaxTransportSize = 64;

    // ---- read ---------------------------------------------------------------------------

    public static void Read(ref SpanPacketReader data, out MovementInfo info)
        => Read(ModernMovementLayouts.Current, ref data, out info);

    /// <summary>Reads the block at the packet's read position and advances past it.</summary>
    public static void Read(WorldPacket data, out MovementInfo info)
    {
        var reader = new SpanPacketReader(data.GetRemainingSpan());
        Read(ModernMovementLayouts.Current, ref reader, out info);
        data.Skip(reader.Position);
    }

    // Inlined so that the caller's layout, a static readonly in production, folds the switch away.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ModernMovementLayout layout, ref SpanPacketReader data, out MovementInfo info)
    {
        switch (layout)
        {
            case ModernMovementLayout.WotLKClassic:
                ReadCore<WotLKClassicMovementLayout>(ref data, out info);
                break;
            case ModernMovementLayout.WordFlags:
                ReadCore<WordFlagsMovementLayout>(ref data, out info);
                break;
            default:
                ReadCore<BitFlagsMovementLayout>(ref data, out info);
                break;
        }
    }

    private static void ReadCore<TLayout>(ref SpanPacketReader data, out MovementInfo info)
        where TLayout : struct, IModernMovementLayout
    {
        // Written field by field as it is read. The destination may be a reused variable, so it
        // is cleared first: every field a block does not carry has to come out as its default.
        info = default;
        if (TLayout.FlagsAreWords)
        {
            info.Flags = (MovementFlagModern)data.ReadUInt32();
            info.FlagsExtra = data.ReadUInt32();
            info.FlagsExtra2 = data.ReadUInt32();
        }

        info.MoveTime = data.ReadUInt32();
        info.Position = data.ReadVector3();
        info.Orientation = data.ReadFloat();
        info.SwimPitch = data.ReadFloat();
        info.SplineElevation = data.ReadFloat();

        uint removeMovementForcesCount = data.ReadUInt32();
        data.ReadUInt32(); // MoveIndex

        for (uint i = 0; i < removeMovementForcesCount; ++i)
            data.ReadPackedGuid128();

        if (!TLayout.FlagsAreWords)
        {
            info.Flags = (MovementFlagModern)data.ReadBits<uint>(30);
            info.FlagsExtra = data.ReadBits<uint>(18);
        }

        bool hasStandingOnGameObject = TLayout.HasStandingOnGameObject && data.HasBit();
        bool hasTransport = data.HasBit();
        bool hasFall = data.HasBit();
        data.ReadBit(); // HasSpline
        data.ReadBit(); // HeightChangeFailed
        data.ReadBit(); // RemoteTimeValid
        bool hasInertia = TLayout.HasInertia && data.HasBit();
        bool hasAdvFlying = TLayout.HasAdvFlying && data.HasBit();

        if (hasTransport)
        {
            WowGuid128 guid = data.ReadPackedGuid128();
            Vector3 offset = data.ReadVector3();
            float transportOrientation = data.ReadFloat();
            sbyte seat = data.ReadInt8();                   // VehicleSeatIndex
            uint time = data.ReadUInt32();                  // MoveTime

            bool hasPrevTime = data.HasBit();
            bool hasVehicleId = data.HasBit();
            uint prevTime = hasPrevTime ? data.ReadUInt32() : 0u;       // PrevMoveTime
            uint vehicleId = hasVehicleId ? data.ReadUInt32() : 0u;     // VehicleRecID

            info.Transport = new TransportInfo
            {
                Guid = guid,
                Offset = offset,
                Orientation = transportOrientation,
                Time = time,
                PrevTime = prevTime,
                Seat = seat,
                VehicleId = vehicleId,
            };
        }

        if (hasStandingOnGameObject)
            info.StandingOnGameObjectGuid = data.ReadPackedGuid128();

        if (hasInertia)
        {
            data.ReadPackedGuid128();
            data.ReadVector3(); // Force
            data.ReadUInt32();  // Lifetime
        }

        if (hasAdvFlying)
        {
            data.ReadFloat(); // forwardVelocity
            data.ReadFloat(); // upVelocity
        }

        if (hasFall)
        {
            info.FallTime = data.ReadUInt32();
            info.JumpVerticalSpeed = data.ReadFloat();
            if (data.HasBit())
            {
                info.JumpSinAngle = data.ReadFloat();
                info.JumpCosAngle = data.ReadFloat();
                info.JumpHorizontalSpeed = data.ReadFloat();
            }
        }
    }

    // ---- write --------------------------------------------------------------------------

    /// <summary>Writes the mover guid and the block. Returns the number of bytes written.</summary>
    public static int Write(Span<byte> buffer, WowGuid128 mover, in MovementInfo info)
        => Write(ModernMovementLayouts.Current, buffer, mover, in info);

    public static void Write(WorldPacket data, WowGuid128 mover, in MovementInfo info)
    {
        Span<byte> buffer = stackalloc byte[MaxSize];
        data.WriteBytes(buffer[..Write(ModernMovementLayouts.Current, buffer, mover, in info)]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(ModernMovementLayout layout, Span<byte> buffer, WowGuid128 mover, in MovementInfo info) => layout switch
    {
        ModernMovementLayout.WotLKClassic => WriteCore<WotLKClassicMovementLayout>(buffer, mover, in info),
        ModernMovementLayout.WordFlags => WriteCore<WordFlagsMovementLayout>(buffer, mover, in info),
        _ => WriteCore<BitFlagsMovementLayout>(buffer, mover, in info),
    };

    private static int WriteCore<TLayout>(Span<byte> buffer, WowGuid128 mover, in MovementInfo info)
        where TLayout : struct, IModernMovementLayout
    {
        bool hasFallDirection = info.Flags.HasAnyFlag(MovementFlagModern.Falling | MovementFlagModern.FallingFar);
        bool hasFall = hasFallDirection || info.FallTime != 0;
        uint flagsExtra = TLayout.WritesExtraFlagsAsZero ? 0u : info.FlagsExtra;
        bool hasTransport = info.TransportGuid != default;

        var writer = new SpanPacketWriter(buffer);

        writer.WritePackedGuid128(mover.Low, mover.High);                // MoverGUID

        if (TLayout.FlagsAreWords)
        {
            writer.WriteUInt32((uint)info.Flags);
            writer.WriteUInt32(flagsExtra);
            writer.WriteUInt32(info.FlagsExtra2);
        }

        writer.WriteUInt32(info.MoveTime);                               // MoveTime
        writer.WriteFloat(info.Position.X);
        writer.WriteFloat(info.Position.Y);
        writer.WriteFloat(info.Position.Z);
        writer.WriteFloat(info.Orientation);

        writer.WriteFloat(info.SwimPitch);                               // Pitch
        writer.WriteFloat(info.SplineElevation);                         // StepUpStartElevation

        writer.WriteUInt32(0);                                           // RemoveForcesIDs.size()
        writer.WriteUInt32(0);                                           // MoveIndex

        if (!TLayout.FlagsAreWords)
        {
            writer.WriteBits((uint)info.Flags, 30);
            writer.WriteBits(flagsExtra, 18);
        }

        if (TLayout.HasStandingOnGameObject)
            writer.WriteBit(false);                                      // HasStandingOnGameObjectGUID
        writer.WriteBit(hasTransport);                                   // HasTransport
        writer.WriteBit(hasFall);                                        // HasFall
        writer.WriteBit(info.HasSplineData);                             // HasSpline
        writer.WriteBit(false);                                          // HeightChangeFailed
        writer.WriteBit(false);                                          // RemoteTimeValid
        if (TLayout.HasInertia)
            writer.WriteBit(false);                                      // HasInertia
        if (TLayout.HasAdvFlying)
            writer.WriteBit(false);                                      // HasAdvFlying
        // 3.4.3 has no HasDriveStatus bit; see ModernMovementLayouts.For.
        writer.FlushBits();

        if (hasTransport)
            WriteTransport(ref writer, info.Transport.GetValueOrDefault());

        if (hasFall)
        {
            writer.WriteUInt32(info.FallTime);                           // Time
            writer.WriteFloat(info.JumpVerticalSpeed);                   // JumpVelocity
            writer.WriteBit(hasFallDirection);
            writer.FlushBits();

            if (hasFallDirection)
            {
                writer.WriteFloat(info.JumpSinAngle);                    // Direction
                writer.WriteFloat(info.JumpCosAngle);
                writer.WriteFloat(info.JumpHorizontalSpeed);             // Speed
            }
        }

        return writer.Position;
    }

    /// <summary>
    /// The transport part on its own, as a game object riding a transport carries it in its
    /// create. The layout is the same on every modern build.
    /// </summary>
    public static void WriteTransport(WorldPacket data, in TransportInfo transport)
    {
        Span<byte> buffer = stackalloc byte[MaxTransportSize];
        var writer = new SpanPacketWriter(buffer);
        WriteTransport(ref writer, in transport);
        data.WriteBytes(buffer[..writer.Position]);
    }

    private static void WriteTransport(ref SpanPacketWriter writer, in TransportInfo transport)
    {
        bool hasVehicleId = transport.VehicleId != 0;

        writer.WritePackedGuid128(transport.Guid.Low, transport.Guid.High);
        writer.WriteFloat(transport.Offset.X);
        writer.WriteFloat(transport.Offset.Y);
        writer.WriteFloat(transport.Offset.Z);
        writer.WriteFloat(transport.Orientation);
        writer.WriteInt8(transport.Seat);
        writer.WriteUInt32(transport.Time);

        // The legacy block's second time is not forwarded as PrevMoveTime.
        writer.WriteBit(false);                                          // HasPrevTime
        writer.WriteBit(hasVehicleId);
        writer.FlushBits();

        if (hasVehicleId)
            writer.WriteUInt32(transport.VehicleId);
    }
}
