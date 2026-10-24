/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HermesProxy.World.Server.Packets;

public readonly record struct CanDuel(WowGuid128 TargetGUID);

public class CanDuelResult : ServerPacket, ISpanWritable
{
    public CanDuelResult() : base(Opcode.SMSG_CAN_DUEL_RESULT) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(TargetGUID);
        _worldPacket.WriteBit(Result);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 1; // GUID + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(TargetGUID.Low, TargetGUID.High);
        writer.WriteBit(Result);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 TargetGUID;
    public bool Result;
}

public sealed class DuelRequested : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<DuelRequested>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new GuidsLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.Zero, new ToTheDeathLayout()));

    private static readonly ServerPacketLayout<DuelRequested> Layout = Layouts.ForRunningClient();

    public DuelRequested() : base(Opcode.SMSG_DUEL_REQUESTED, ConnectionType.Instance) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    public int MaxSize => Layout.MaxSize;

    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    /// <summary>1.14 and 2.5: the flag, who asked and that player's account.</summary>
    internal sealed class GuidsLayout : ServerPacketLayout<DuelRequested>
    {
        public override int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 3; // 3 GUIDs

        public override void Write(DuelRequested packet, WorldPacket data) => WriteGuids(packet, data);

        public override int WriteToSpan(DuelRequested packet, Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            WriteGuids(packet, ref writer);
            return writer.Position;
        }
    }

    /// <summary>
    /// 3.4.3 and 4.4.2 (TrinityCore wotlk_classic and cata_classic DuelRequested::Write, the clients'
    /// own readers): the GUIDs, then a byte holding the ToTheDeath bit.
    /// </summary>
    internal sealed class ToTheDeathLayout : ServerPacketLayout<DuelRequested>
    {
        public override int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 3 + 1; // 3 GUIDs + 1 byte for bit

        public override void Write(DuelRequested packet, WorldPacket data)
        {
            WriteGuids(packet, data);
            data.WriteBit(packet.ToTheDeath);
            data.FlushBits();
        }

        public override int WriteToSpan(DuelRequested packet, Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            WriteGuids(packet, ref writer);
            writer.WriteBit(packet.ToTheDeath);
            writer.FlushBits();
            return writer.Position;
        }
    }

    private static void WriteGuids(DuelRequested packet, WorldPacket data)
    {
        data.WritePackedGuid128(packet.ArbiterGUID);
        data.WritePackedGuid128(packet.RequestedByGUID);
        data.WritePackedGuid128(packet.RequestedByWowAccount);
    }

    private static void WriteGuids(DuelRequested packet, ref SpanPacketWriter writer)
    {
        writer.WritePackedGuid128(packet.ArbiterGUID.Low, packet.ArbiterGUID.High);
        writer.WritePackedGuid128(packet.RequestedByGUID.Low, packet.RequestedByGUID.High);
        writer.WritePackedGuid128(packet.RequestedByWowAccount.Low, packet.RequestedByWowAccount.High);
    }

    public WowGuid128 ArbiterGUID;
    public WowGuid128 RequestedByGUID;
    public WowGuid128 RequestedByWowAccount;
    public bool ToTheDeath; // a Wrath Classic duel to the death; no legacy server has one
}

public readonly record struct DuelResponse(WowGuid128 ArbiterGUID, bool Accepted, bool Forfeited);

public class DuelCountdown : ServerPacket, ISpanWritable
{
    public DuelCountdown() : base(Opcode.SMSG_DUEL_COUNTDOWN) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(Countdown);
    }

    public int MaxSize => 4; // uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(Countdown);
        return writer.Position;
    }

    public uint Countdown;
}

public class DuelComplete : ServerPacket, ISpanWritable
{
    public DuelComplete() : base(Opcode.SMSG_DUEL_COMPLETE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBit(Started);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 1; // 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBit(Started);
        writer.FlushBits();
        return writer.Position;
    }

    public bool Started;
}

public class DuelWinner : ServerPacket, ISpanWritable
{
    public DuelWinner() : base(Opcode.SMSG_DUEL_WINNER, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBits(BeatenName.GetByteCount(), 6);
        _worldPacket.WriteBits(WinnerName.GetByteCount(), 6);
        _worldPacket.WriteBit(Fled);
        _worldPacket.WriteUInt32(BeatenVirtualRealmAddress);
        _worldPacket.WriteUInt32(WinnerVirtualRealmAddress);
        _worldPacket.WriteString(BeatenName);
        _worldPacket.WriteString(WinnerName);
    }

    // MaxSize: bits (6+6+1=13 -> 2 bytes) + 2 uints (8) + 2 names (48) = 58
    public int MaxSize => 2 + 8 + GameLimits.MaxPlayerNameBytes * 2;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(BeatenName), 6);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(WinnerName), 6);
        writer.WriteBit(Fled);
        writer.WriteUInt32(BeatenVirtualRealmAddress);
        writer.WriteUInt32(WinnerVirtualRealmAddress);
        writer.WriteString(BeatenName);
        writer.WriteString(WinnerName);
        return writer.Position;
    }

    public string BeatenName = string.Empty;
    public string WinnerName = string.Empty;
    public uint BeatenVirtualRealmAddress;
    public uint WinnerVirtualRealmAddress;
    public bool Fled;
}

public class DuelInBounds : ServerPacket, ISpanWritable
{
    public DuelInBounds() : base(Opcode.SMSG_DUEL_IN_BOUNDS, ConnectionType.Instance) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

public class DuelOutOfBounds : ServerPacket, ISpanWritable
{
    public DuelOutOfBounds() : base(Opcode.SMSG_DUEL_OUT_OF_BOUNDS, ConnectionType.Instance) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}
