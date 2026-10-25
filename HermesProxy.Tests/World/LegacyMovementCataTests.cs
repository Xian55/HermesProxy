using System;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The 4.3.4 movement writer against a reader ported from TrinityCore 4.3.4's
/// Player::ReadMovementInfo, which is what the server runs on every movement packet.
/// </summary>
public sealed class LegacyMovementCataTests
{
    [Fact]
    public void EverySequenceHasElements()
    {
        // A dictionary built before the arrays it points at holds nulls, and each movement
        // packet then went out as a bare opcode.
        foreach (var (opcode, sequence) in LegacyMovementSequencesCata.ByOpcode)
        {
            Assert.NotNull(sequence);
            Assert.True(sequence.Length > 0, opcode.ToString());
        }
    }

    [Theory]
    [InlineData(Opcode.MSG_MOVE_START_FORWARD)]
    [InlineData(Opcode.MSG_MOVE_HEARTBEAT)]
    [InlineData(Opcode.MSG_MOVE_JUMP)]
    [InlineData(Opcode.MSG_MOVE_FALL_LAND)]
    [InlineData(Opcode.MSG_MOVE_SET_FACING)]
    [InlineData(Opcode.CMSG_MOVE_KNOCK_BACK_ACK)]
    [InlineData(Opcode.CMSG_MOVE_SET_CAN_FLY)]
    [InlineData(Opcode.CMSG_MOVE_CHANGE_TRANSPORT)]
    public void ServerReadsBackWhatWasWritten(Opcode opcode)
    {
        var info = new MovementInfo
        {
            Flags = MovementFlagModern.Forward | MovementFlagModern.Falling,
            MoveTime = 123456,
            Position = new Vector3(-8423.81f, 1361.3f, 104.671f),
            Orientation = 1.5f,
            FallTime = 250,
            JumpVerticalSpeed = -7.5f,
            JumpSinAngle = 0.25f,
            JumpCosAngle = 0.75f,
            JumpHorizontalSpeed = 7f,
        };
        var mover = new WowGuid64(0x0000000000A1B2C3);
        var sequence = LegacyMovementSequencesCata.ByOpcode[opcode];

        using var packet = new WorldPacket(Opcode.MSG_MOVE_HEARTBEAT);
        LegacyMovementCata.Write(packet, sequence, mover, in info, movementCounter: 7);
        packet.FlushBits();
        // A received WorldPacket starts with its 2-byte opcode.
        byte[] body = packet.GetDataSpan().ToArray();
        byte[] framed = new byte[sizeof(ushort) + body.Length];
        body.CopyTo(framed, sizeof(ushort));
        using var read = new WorldPacket(framed);

        var back = ReadLikeTrinityCore434(read, sequence);

        Assert.Equal(mover.Low, back.Guid);
        Assert.Equal((uint)info.Flags, back.Flags);
        Assert.Equal(info.MoveTime, back.Time);
        Assert.Equal(info.Position, back.Position);
        Assert.Equal(info.Orientation, back.Orientation);
        Assert.Equal(info.FallTime, back.FallTime);
        Assert.Equal(info.JumpVerticalSpeed, back.VerticalSpeed);
        Assert.Equal(info.JumpHorizontalSpeed, back.HorizontalSpeed);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Theory]
    [InlineData(0x110u, 0x410u)] // can pass from swimming to flying, always allow pitching
    [InlineData(0x400u, 0x000u)] // the client's turning while falling, which 4.3.4 has no flag for
    public void ExtraFlags_ServerReadsSwimToFlyAsItsOwnBit(uint client, uint server)
    {
        // TrinityCore 4.3.4 MOVEMENTFLAG2_CAN_SWIM_TO_FLY_TRANS is 0x400; the 4.4.2 client's is 0x100.
        var info = new MovementInfo { Position = new Vector3(-9723.4f, 55.9f, 42f), FlagsExtra = client };
        var sequence = LegacyMovementSequencesCata.ByOpcode[Opcode.MSG_MOVE_HEARTBEAT];

        using var packet = new WorldPacket(Opcode.MSG_MOVE_HEARTBEAT);
        LegacyMovementCata.Write(packet, sequence, new WowGuid64(0x0000000000A1B2C3), in info);
        packet.FlushBits();
        using var read = Framed(packet.GetDataSpan().ToArray());

        Assert.Equal(server, ReadLikeTrinityCore434(read, sequence).Flags2);
    }

    [Fact]
    public void ExtraFlags_SwimToFlyComesBackAsTheClientsBit()
        => Assert.Equal(0x110u, LegacyMovementCata.FlagsExtraFromLegacy(0x410));

    [Fact]
    public void SetFly_IsReportedAsSetCanFly()
    {
        // The client's CMSG_MOVE_SET_FLY has no MSG_ opcode; TrinityCore 4.3.4 reads a takeoff
        // or a landing as CMSG_MOVE_SET_CAN_FLY, with a sequence of its own.
        Assert.Equal(Opcode.CMSG_MOVE_SET_CAN_FLY, LegacyMovementCata.ReportWithoutMsg(Opcode.CMSG_MOVE_SET_FLY));
        Assert.True(LegacyMovementSequencesCata.ByOpcode.ContainsKey(Opcode.CMSG_MOVE_SET_CAN_FLY));
        Assert.Equal(Opcode.MSG_NULL_ACTION, LegacyMovementCata.ReportWithoutMsg(Opcode.CMSG_MOVE_SET_FACING_HEARTBEAT));
    }

    [Fact]
    public void ChangeTransport_KeepsItsOwnOpcode()
    {
        // TrinityCore 4.3.4 reads a change of transport as CMSG_MOVE_CHNG_TRANSPORT (0x3102), with a
        // sequence of its own; there is no MSG_ opcode for it.
        Assert.Equal(Opcode.CMSG_MOVE_CHANGE_TRANSPORT, LegacyMovementCata.ReportWithoutMsg(Opcode.CMSG_MOVE_CHANGE_TRANSPORT));
        Assert.True(LegacyMovementSequencesCata.ByOpcode.ContainsKey(Opcode.CMSG_MOVE_CHANGE_TRANSPORT));
    }

    [Theory]
    [InlineData(Opcode.SMSG_MOVE_SET_RUN_SPEED)]
    [InlineData(Opcode.SMSG_MOVE_SPLINE_SET_RUN_SPEED)]
    [InlineData(Opcode.SMSG_MOVE_ROOT)]
    [InlineData(Opcode.SMSG_MOVE_SPLINE_UNROOT)]
    [InlineData(Opcode.SMSG_MOVE_UPDATE_FLIGHT_BACK_SPEED)]
    public void ReaderUndoesTheWriter(Opcode opcode)
    {
        var info = new MovementInfo { Position = new Vector3(1f, 2f, 3f), Orientation = 0.5f, MoveTime = 99 };
        var mover = new WowGuid64(0x0000000000C0FFEE);
        using var packet = new WorldPacket(Opcode.MSG_MOVE_HEARTBEAT);
        LegacyMovementCata.Write(packet, LegacyMovementSequencesCata.ByOpcode[opcode], mover, in info,
            movementCounter: 42, extraFloat: 7.5f);
        packet.FlushBits();
        byte[] body = packet.GetDataSpan().ToArray();
        byte[] framed = new byte[sizeof(ushort) + body.Length];
        body.CopyTo(framed, sizeof(ushort));
        using var read = new WorldPacket(framed);

        Assert.True(LegacyMovementCata.TryRead(read, opcode, null!, out ulong guid, out _, out uint counter, out float extra));

        Assert.Equal(mover.Low, guid);
        var sequence = LegacyMovementSequencesCata.ByOpcode[opcode];
        if (Array.IndexOf(sequence, LegacyMovementElement.Counter) >= 0)
            Assert.Equal(42u, counter);
        if (Array.IndexOf(sequence, LegacyMovementElement.ExtraElement) >= 0)
            Assert.Equal(7.5f, extra);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    /// <summary>
    /// A packet that does not end where its order does was written in another order, and its
    /// mover would be made of other bytes (#405): it is dropped, not passed on.
    /// </summary>
    [Theory]
    [InlineData(Opcode.SMSG_MOVE_UPDATE, 1)]
    [InlineData(Opcode.SMSG_MOVE_UPDATE, -1)]
    [InlineData(Opcode.SMSG_MOVE_SET_RUN_SPEED, 1)]
    [InlineData(Opcode.SMSG_MOVE_SET_RUN_SPEED, -1)]
    public void APacketThatDoesNotEndWithItsOrder_IsDropped(Opcode opcode, int extraBytes)
    {
        var info = new MovementInfo { Position = new Vector3(1f, 2f, 3f), Orientation = 0.5f, MoveTime = 99 };
        using var packet = new WorldPacket(Opcode.MSG_MOVE_HEARTBEAT);
        LegacyMovementCata.Write(packet, LegacyMovementSequencesCata.ByOpcode[opcode], new WowGuid64(0x0000000000C0FFEE), in info,
            movementCounter: 42, extraFloat: 7.5f);
        packet.FlushBits();
        byte[] body = packet.GetDataSpan().ToArray();
        byte[] framed = new byte[sizeof(ushort) + body.Length + extraBytes];
        body.AsSpan(0, Math.Min(body.Length, framed.Length - sizeof(ushort))).CopyTo(framed.AsSpan(sizeof(ushort)));
        using var read = new WorldPacket(framed);

        Assert.False(LegacyMovementCata.TryRead(read, opcode, null!, out ulong mover, out _, out _, out _));
        Assert.Equal(0ul, mover);
    }

    [Fact]
    public void MoveUpdateTeleport_AsATrinityCore434ServerSentIt()
    {
        // What the players around saw of player 3 sent home to (-9420, 60, 58.91) by GM command.
        // TrinityCore 4.3.4 writes this one by hand (MoveUpdateTeleport::Write), not from its
        // sequence table: the position first, then the bits, no orientation, a time, GUID byte 0.
        byte[] body = Convert.FromHexString("55A66B42" + "00007042" + "003013C6" + "A1042008" + "A8FEA587" + "02");
        byte[] framed = new byte[sizeof(ushort) + body.Length];
        body.CopyTo(framed, sizeof(ushort));
        using var read = new WorldPacket(framed);

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_UPDATE_TELEPORT, null!, out ulong mover, out MovementInfo info, out _, out _));

        Assert.Equal(3ul, mover);
        Assert.Equal(-9420f, info.Position.X);
        Assert.Equal(60f, info.Position.Y);
        Assert.Equal(58.9124f, info.Position.Z, 3);
        Assert.Equal(0x87A5FEA8u, info.MoveTime);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveUpdateRunBackSpeed_AsATrinityCore434ServerSentIt()
    {
        // What another player saw of player 2 standing at (-8833.9, 622.1, 93.84) when a GM command
        // made its run-back speed 13.5. The sequence has no bits for the fall data or the timestamp;
        // the server sent both, the fall time (219) left over from the last jump.
        byte[] body = Convert.FromHexString("0540" + "DB000000" + "00000000" + "4F5F328A" + "03" + "9A070AC6"
            + "67861B44" + "00005841" + "D9ACBB42");
        using var read = Framed(body);

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_UPDATE_RUN_BACK_SPEED, null!, out ulong mover, out MovementInfo info, out _, out float speed));

        Assert.Equal(2ul, mover);
        Assert.Equal(13.5f, speed);
        Assert.Equal(-8833.9f, info.Position.X, 1);
        Assert.Equal(622.1f, info.Position.Y, 1);
        Assert.Equal(93.84f, info.Position.Z, 2);
        Assert.Equal(219u, info.FallTime);
        Assert.Equal(0x8A325F4Fu, info.MoveTime);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Theory]
    [InlineData("E11807F00000C84141", 25f)]
    [InlineData("E11807F00000204041", 2.5f)]
    public void SplineMoveSetWalkSpeed_AsATrinityCore434ServerSentIt(string hex, float expected)
    {
        // A hunter's pet (pet number 6) given a walk speed while it followed its owner. TrinityCore
        // 4.3.4 sends it as SMSG_SPLINE_MOVE_SET_WALK_SPEED (0x34A5); the opcode table had it under
        // WowPacketParser's name for it, a walk-back speed, which no handler takes.
        using var read = Framed(Convert.FromHexString(hex));

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_SPLINE_SET_WALK_SPEED, null!, out ulong mover, out _, out _, out float speed));

        Assert.Equal(0xF140000600000019ul, mover);
        Assert.Equal(expected, speed);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveUpdateWalkSpeed_AsATrinityCore434ServerSentIt()
    {
        // The same player made to walk at 5: here the fall data's bit for a direction is written
        // (no direction) between GUID bits, which only a reader that knows there is fall data takes.
        byte[] body = Convert.FromHexString("8180" + "00000000" + "DB000000" + "4F5F328A" + "67861B44" + "03"
            + "D9ACBB42" + "9A070AC6" + "0000A040");
        using var read = Framed(body);

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_UPDATE_WALK_SPEED, null!, out ulong mover, out MovementInfo info, out _, out float speed));

        Assert.Equal(2ul, mover);
        Assert.Equal(5f, speed);
        Assert.Equal(-8833.9f, info.Position.X, 1);
        Assert.Equal(622.1f, info.Position.Y, 1);
        Assert.Equal(219u, info.FallTime);
        Assert.Equal(0x8A325F4Fu, info.MoveTime);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveUpdateKnockBack_AsATrinityCore434ServerSentIt()
    {
        // What player 2 saw of player 3 knocked away from (-8833.9, 622.1, 93.84): falling, so the
        // server sent the fall data and its direction bit with no bit to say the fall data is there.
        byte[] body = Convert.FromHexString("080000280180" + "14070D40" + "4164173F" + "00004841" + "16704EBF"
            + "00000000" + "3333A7C1" + "D9ACBB42" + "9AEC3B8A" + "9A070AC6" + "67861B44" + "02");
        using var read = Framed(body);

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_UPDATE_KNOCK_BACK, null!, out ulong mover, out MovementInfo info, out _, out _));

        Assert.Equal(3ul, mover);
        Assert.Equal(-8833.9f, info.Position.X, 1);
        Assert.Equal(622.1f, info.Position.Y, 1);
        Assert.Equal(93.84f, info.Position.Z, 2);
        Assert.Equal(2.2f, info.Orientation, 2);
        Assert.Equal(12.5f, info.JumpHorizontalSpeed);
        Assert.Equal(-20.9f, info.JumpVerticalSpeed, 2);
        Assert.Equal(0.5914f, info.JumpSinAngle, 3);
        Assert.Equal(-0.8064f, info.JumpCosAngle, 3);
        Assert.Equal(0x8A3BEC9Au, info.MoveTime);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveSetCollisionHeight_AsATrinityCore434ServerSentIt()
    {
        // Player 2 taking the taxi mount: the reason in two bits ahead of the GUID mask (0, a
        // mount on 4.3.4), the move counter 5, the new height 2.58 last.
        byte[] body = Convert.FromHexString("0080" + "03" + "05000000" + "22542540");
        using var read = Framed(body);

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_SET_COLLISION_HEIGHT, null!, out ulong mover, out _, out uint counter, out float height, out byte reason));

        Assert.Equal(2ul, mover);
        Assert.Equal(5u, counter);
        Assert.Equal(2.583f, height, 3);
        Assert.Equal(0, reason);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveSetCollisionHeightAck_CarriesTheReasonAsTwoBits()
    {
        var info = new MovementInfo { Position = new Vector3(1f, 2f, 3f), Orientation = 0.5f, MoveTime = 99 };
        var mover = new WowGuid64(0x0000000000C0FFEE);
        using var packet = new WorldPacket(Opcode.MSG_MOVE_HEARTBEAT);
        Assert.True(LegacyMovementCata.TryWrite(packet, Opcode.CMSG_MOVE_SET_COLLISION_HEIGHT_ACK, mover, in info,
            movementCounter: 6, extraFloat: 2.0313f, extraBits: 1));
        packet.FlushBits();
        using var read = Framed(packet.GetDataSpan().ToArray());

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.CMSG_MOVE_SET_COLLISION_HEIGHT_ACK, null!, out ulong guid, out _, out uint counter, out float height, out byte reason));

        Assert.Equal(mover.Low, guid);
        Assert.Equal(6u, counter);
        Assert.Equal(2.0313f, height);
        Assert.Equal(1, reason);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void ChangeSeatsOnControlledVehicle_ServerReadsTheSeatAndItsVehicle()
    {
        // TrinityCore 4.3.4 HandleChangeSeatsOnControlledVehicle: the seat and the vehicle it is on
        // come through the sequence's extra elements, in an order of their own (accessoryGuid).
        LegacyMovementElement[] accessoryGuid =
        [
            LegacyMovementElement.ExtraInt8,
            LegacyMovementElement.HasGuidByte2, LegacyMovementElement.HasGuidByte4, LegacyMovementElement.HasGuidByte7,
            LegacyMovementElement.HasGuidByte6, LegacyMovementElement.HasGuidByte5, LegacyMovementElement.HasGuidByte0,
            LegacyMovementElement.HasGuidByte1, LegacyMovementElement.HasGuidByte3,
            LegacyMovementElement.GuidByte6, LegacyMovementElement.GuidByte1, LegacyMovementElement.GuidByte2,
            LegacyMovementElement.GuidByte5, LegacyMovementElement.GuidByte3, LegacyMovementElement.GuidByte0,
            LegacyMovementElement.GuidByte4, LegacyMovementElement.GuidByte7,
        ];
        var info = new MovementInfo { Position = new Vector3(5600.5f, 2010.25f, 798f), Orientation = 1.25f, MoveTime = 4242 };
        var vehicle = new WowGuid64(0xF150006DBE001234);
        var seatVehicle = new WowGuid64(0xF15000700A005678);
        using var packet = new WorldPacket(Opcode.MSG_MOVE_HEARTBEAT);
        LegacyMovementCata.WriteChangeSeats(packet, vehicle, in info, seatVehicle, 2);
        packet.FlushBits();
        using var read = Framed(packet.GetDataSpan().ToArray());

        var back = ReadLikeTrinityCore434(read, LegacyMovementSequencesCata.ByOpcode[Opcode.CMSG_CHANGE_SEATS_ON_CONTROLLED_VEHICLE], accessoryGuid);

        Assert.Equal(vehicle.Low, back.Guid);
        Assert.Equal(info.Position, back.Position);
        Assert.Equal(info.Orientation, back.Orientation);
        Assert.Equal(info.MoveTime, back.Time);
        Assert.Equal(seatVehicle.Low, back.ExtraGuid);
        Assert.Equal(2, back.ExtraInt8);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveUpdateRunBackSpeed_WithPresenceBits_IsRead()
    {
        // Another 4.3.4 core, in Warsong Gulch (2026-10-13): player 0x221C running forward under a
        // slow, run-back speed 1.8. It writes every presence bit, in WowPacketParser's order, which
        // none of the reference table's guesses fits.
        byte[] body = Convert.FromHexString("A09A00000004" + "33561200" + "23" + "90FD6F40" + "1D"
            + "25829344" + "294BB844" + "6666E63F" + "440C9843");
        using var read = Framed(body);

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_UPDATE_RUN_BACK_SPEED, null!, out ulong mover, out MovementInfo info, out _, out float speed));

        Assert.Equal(0x221Cul, mover);
        Assert.Equal(1.8f, speed);
        Assert.Equal(1180.07f, info.Position.X, 2);
        Assert.Equal(1474.35f, info.Position.Y, 2);
        Assert.Equal(304.10f, info.Position.Z, 2);
        Assert.Equal(3.7499f, info.Orientation, 4);
        Assert.Equal(1201715u, info.MoveTime);
        Assert.Equal(1u, (uint)info.Flags);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveUpdateWalkSpeed_WithPresenceBits_IsRead()
    {
        // The same core (2026-10-20): player 0x2224's walk speed back to 2.5 by GM command, with a
        // fall left over from a jump.
        byte[] body = Convert.FromHexString("8872" + "1000" + "D893FEC0" + "39030000" + "23" + "195BDB00"
            + "0C865040" + "3E3E8642" + "25" + "BC986042" + "04" + "9413C600" + "002040");
        using var read = Framed(body);

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_UPDATE_WALK_SPEED, null!, out ulong mover, out MovementInfo info, out _, out float speed));

        Assert.Equal(0x2224ul, mover);
        Assert.Equal(2.5f, speed);
        Assert.Equal(-9445.004f, info.Position.X, 2);
        Assert.Equal(67.1216f, info.Position.Y, 3);
        Assert.Equal(56.1492f, info.Position.Z, 3);
        Assert.Equal(3.2582f, info.Orientation, 4);
        Assert.Equal(14375705u, info.MoveTime);
        Assert.Equal(825u, info.FallTime);
        Assert.Equal(-7.9555f, info.JumpVerticalSpeed, 4);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveUpdateFlightBackSpeed_InTheClientsOrder_IsRead()
    {
        // The same core (2026-10-20): player 0x2225's flight-back speed back to 4.5 when a slow
        // ends. The position and the speed come before the bits, and the first bit is one the
        // reference table does not have.
        byte[] body = Convert.FromHexString("918B8542" + "00009040" + "EF9F13C6" + "EDCA6042" + "14A24000"
            + "2364ED30" + "3FE7B5E3" + "0024");
        using var read = Framed(body);

        Assert.True(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_UPDATE_FLIGHT_BACK_SPEED, null!, out ulong mover, out MovementInfo info, out _, out float speed));

        Assert.Equal(0x2225ul, mover);
        Assert.Equal(4.5f, speed);
        Assert.Equal(-9447.983f, info.Position.X, 2);
        Assert.Equal(66.7726f, info.Position.Y, 3);
        Assert.Equal(56.1982f, info.Position.Z, 3);
        Assert.Equal(0.6911f, info.Orientation, 4);
        Assert.Equal(14923239u, info.MoveTime);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Fact]
    public void MoveUpdateRunBackSpeed_NoGuessFits_IsNotRead()
    {
        // one byte more than the packet above: no choice of fall data and transport ends there
        byte[] body = Convert.FromHexString("0540" + "DB000000" + "00000000" + "4F5F328A" + "03" + "9A070AC6"
            + "67861B44" + "00005841" + "D9ACBB42" + "00");
        using var read = Framed(body);

        Assert.False(LegacyMovementCata.TryRead(read, Opcode.SMSG_MOVE_UPDATE_RUN_BACK_SPEED, null!, out _, out _, out _, out _));
    }

    private static WorldPacket Framed(byte[] body)
    {
        byte[] framed = new byte[sizeof(ushort) + body.Length];
        body.CopyTo(framed, sizeof(ushort));
        return new WorldPacket(framed);
    }

    private sealed record Read(ulong Guid, uint Flags, uint Time, Vector3 Position, float Orientation,
        uint FallTime, float VerticalSpeed, float HorizontalSpeed, ulong ExtraGuid, sbyte ExtraInt8, uint Flags2);

    // TrinityCore 4.3.4 Player::ReadMovementInfo, reduced to the fields asserted above; an extra
    // element takes the next one of `extra`, as its ExtraMovementStatusElement does.
    private static Read ReadLikeTrinityCore434(WorldPacket data, LegacyMovementElement[] sequence, LegacyMovementElement[]? extra = null)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> guid = stackalloc byte[8];
        Span<bool> extraMask = stackalloc bool[8];
        Span<byte> extraGuid = stackalloc byte[8];
        int extraIndex = 0;
        sbyte extraInt8 = 0;
        bool hasFlags = false, hasFlags2 = false, hasTime = false, hasOrientation = false, hasTransport = false;
        bool hasPitch = false, hasFall = false, hasFallDirection = false, hasSplineElevation = false;
        uint flags = 0, flags2 = 0, time = 0, fallTime = 0;
        float x = 0, y = 0, z = 0, o = 0, vertical = 0, horizontal = 0;

        foreach (var element in sequence)
        {
            switch (element)
            {
                case >= LegacyMovementElement.HasGuidByte0 and <= LegacyMovementElement.HasGuidByte7:
                    mask[element - LegacyMovementElement.HasGuidByte0] = data.HasBit();
                    break;
                case >= LegacyMovementElement.GuidByte0 and <= LegacyMovementElement.GuidByte7:
                    MaskedGuid.ReadByte(data, mask, guid, element - LegacyMovementElement.GuidByte0);
                    break;
                case LegacyMovementElement.HasMovementFlags: hasFlags = !data.HasBit(); break;
                case LegacyMovementElement.HasMovementFlags2: hasFlags2 = !data.HasBit(); break;
                case LegacyMovementElement.HasTimestamp: hasTime = !data.HasBit(); break;
                case LegacyMovementElement.HasOrientation: hasOrientation = !data.HasBit(); break;
                case LegacyMovementElement.HasTransportData: hasTransport = data.HasBit(); break;
                case LegacyMovementElement.HasPitch: hasPitch = !data.HasBit(); break;
                case LegacyMovementElement.HasFallData: hasFall = data.HasBit(); break;
                case LegacyMovementElement.HasFallDirection: if (hasFall) hasFallDirection = data.HasBit(); break;
                case LegacyMovementElement.HasSplineElevation: hasSplineElevation = !data.HasBit(); break;
                case LegacyMovementElement.HasSpline:
                case LegacyMovementElement.HasHeightChangeFailed:
                case LegacyMovementElement.ZeroBit:
                case LegacyMovementElement.OneBit:
                    data.HasBit();
                    break;
                case LegacyMovementElement.MovementFlags: if (hasFlags) flags = data.ReadBits<uint>(30); break;
                case LegacyMovementElement.MovementFlags2: if (hasFlags2) flags2 = data.ReadBits<uint>(12); break;
                case LegacyMovementElement.Timestamp: if (hasTime) time = data.ReadUInt32(); break;
                case LegacyMovementElement.PositionX: x = data.ReadFloat(); break;
                case LegacyMovementElement.PositionY: y = data.ReadFloat(); break;
                case LegacyMovementElement.PositionZ: z = data.ReadFloat(); break;
                case LegacyMovementElement.Orientation: if (hasOrientation) o = data.ReadFloat(); break;
                case LegacyMovementElement.Pitch: if (hasPitch) data.ReadFloat(); break;
                case LegacyMovementElement.FallTime: if (hasFall) fallTime = data.ReadUInt32(); break;
                case LegacyMovementElement.FallVerticalSpeed: if (hasFall) vertical = data.ReadFloat(); break;
                case LegacyMovementElement.FallCosAngle:
                case LegacyMovementElement.FallSinAngle:
                    if (hasFall && hasFallDirection) data.ReadFloat();
                    break;
                case LegacyMovementElement.FallHorizontalSpeed: if (hasFall && hasFallDirection) horizontal = data.ReadFloat(); break;
                case LegacyMovementElement.SplineElevation: if (hasSplineElevation) data.ReadFloat(); break;
                case LegacyMovementElement.Counter: data.ReadUInt32(); break;
                case LegacyMovementElement.FlushBits: data.ResetBitPos(); break;
                case LegacyMovementElement.ExtraElement:
                    LegacyMovementElement e = extra![extraIndex++];
                    if (e is >= LegacyMovementElement.HasGuidByte0 and <= LegacyMovementElement.HasGuidByte7)
                        extraMask[e - LegacyMovementElement.HasGuidByte0] = data.HasBit();
                    else if (e is >= LegacyMovementElement.GuidByte0 and <= LegacyMovementElement.GuidByte7)
                        MaskedGuid.ReadByte(data, extraMask, extraGuid, e - LegacyMovementElement.GuidByte0);
                    else
                        extraInt8 = (sbyte)data.ReadUInt8();
                    break;
                default:
                    Assert.False(hasTransport, $"transport element {element} with transport data");
                    break;
            }
        }

        Assert.Equal(extra?.Length ?? 0, extraIndex);
        return new Read(MaskedGuid.ToUInt64(guid), flags, time, new Vector3(x, y, z), o, fallTime, vertical, horizontal,
            MaskedGuid.ToUInt64(extraGuid), extraInt8, flags2);
    }
}
