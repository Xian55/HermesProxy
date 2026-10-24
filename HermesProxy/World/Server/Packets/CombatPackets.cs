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


using System;
using System.Collections.Generic;
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

/// <summary>Data only — parsing lives in <c>AttackSwingCodec</c>, behaviour in <c>CombatSystem</c>.</summary>
public readonly record struct AttackSwing(WowGuid128 Victim);

public class AttackSwingError : ServerPacket, ISpanWritable
{
    public AttackSwingError() : base(Opcode.SMSG_ATTACK_SWING_ERROR) { }

    public override void Write()
    {
        _worldPacket.WriteBits((uint)Reason, 3);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 1; // 3 bits packed into 1 byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)Reason, 3);
        writer.FlushBits();
        return writer.Position;
    }

    public AttackSwingErr Reason;
}

/// <summary>An empty payload; the opcode itself is the whole message.</summary>
public readonly record struct AttackStop;

public class SAttackStart : ServerPacket, ISpanWritable
{
    public SAttackStart() : base(Opcode.SMSG_ATTACK_START, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Attacker);
        _worldPacket.WritePackedGuid128(Victim);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2; // 2 packed GUIDs

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Attacker.Low, Attacker.High);
        writer.WritePackedGuid128(Victim.Low, Victim.High);
        return writer.Position;
    }

    public WowGuid128 Attacker;
    public WowGuid128 Victim;
}

public class SAttackStop : ServerPacket, ISpanWritable
{
    public SAttackStop() : base(Opcode.SMSG_ATTACK_STOP, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Attacker);
        _worldPacket.WritePackedGuid128(Victim);
        _worldPacket.WriteBit(NowDead);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 1; // 2 packed GUIDs + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Attacker.Low, Attacker.High);
        writer.WritePackedGuid128(Victim.Low, Victim.High);
        writer.WriteBit(NowDead);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 Attacker;
    public WowGuid128 Victim;
    public bool NowDead;
}

sealed class AttackerStateUpdate : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<AttackerStateUpdate>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new CombatLogTuningLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new ContentTuningParamsLayout()));

    private static readonly ServerPacketLayout<AttackerStateUpdate> Layout = Layouts.ForRunningClient();

    public AttackerStateUpdate() : base(Opcode.SMSG_ATTACKER_STATE_UPDATE, ConnectionType.Instance) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>Up to 3.4.3: the attack round ends with the combat log's 13-byte content tuning.</summary>
    internal sealed class CombatLogTuningLayout : ServerPacketLayout<AttackerStateUpdate>
    {
        public override void Write(AttackerStateUpdate packet, WorldPacket data)
        {
            WorldPacket attackRoundInfo = new();
            WriteRoundUpToTuning(packet, attackRoundInfo, damageRanges: 1);

            var tuning = packet.ContentTuning;
            attackRoundInfo.WriteUInt8((byte)tuning.TuningType);
            attackRoundInfo.WriteUInt8(tuning.TargetLevel);
            attackRoundInfo.WriteUInt8(tuning.Expansion);
            attackRoundInfo.WriteInt16(tuning.PlayerLevelDelta);
            attackRoundInfo.WriteFloat(tuning.PlayerItemLevel);
            attackRoundInfo.WriteFloat(tuning.TargetItemLevel);

            WriteLogDataAndRound(packet, data, attackRoundInfo);
        }
    }

    /// <summary>
    /// 4.4.1 and later (TrinityCore cata_classic AttackerStateUpdate, WowPacketParser's 4.4 reader):
    /// the attack round ends with ContentTuningParams, 14 bytes, and the debug block carries five
    /// damage ranges. Written in the older form, the round came one byte short and the client read
    /// past it.
    /// </summary>
    internal sealed class ContentTuningParamsLayout : ServerPacketLayout<AttackerStateUpdate>
    {
        public override void Write(AttackerStateUpdate packet, WorldPacket data)
        {
            WorldPacket attackRoundInfo = new();
            WriteRoundUpToTuning(packet, attackRoundInfo, damageRanges: 5);

            var tuning = packet.ContentTuning;
            attackRoundInfo.WriteUInt16((ushort)tuning.PlayerItemLevel);
            attackRoundInfo.WriteInt16(tuning.PlayerLevelDelta);
            attackRoundInfo.WriteUInt32((uint)tuning.TargetItemLevel);
            attackRoundInfo.WriteUInt8(tuning.TargetLevel);
            attackRoundInfo.WriteUInt8(tuning.Expansion);
            attackRoundInfo.WriteUInt8(tuning.TargetMinScalingLevel);
            attackRoundInfo.WriteUInt8(tuning.TargetMaxScalingLevel);
            attackRoundInfo.WriteInt8(tuning.TargetScalingLevelDelta);
            attackRoundInfo.WriteBits((uint)tuning.TuningType, 4);
            attackRoundInfo.WriteBit(false);    // ScalesWithItemLevel
            attackRoundInfo.FlushBits();

            WriteLogDataAndRound(packet, data, attackRoundInfo);
        }
    }

    // The attack round up to its content tuning. The debug block's damage range is the first of
    // damageRanges; any after it are zero.
    private static void WriteRoundUpToTuning(AttackerStateUpdate packet, WorldPacket attackRoundInfo, int damageRanges)
    {
        var hitInfo = packet.HitInfo;
        attackRoundInfo.WriteUInt32((uint)hitInfo);
        attackRoundInfo.WritePackedGuid128(packet.AttackerGUID);
        attackRoundInfo.WritePackedGuid128(packet.VictimGUID);
        attackRoundInfo.WriteInt32(packet.Damage);
        attackRoundInfo.WriteInt32(packet.OriginalDamage);
        attackRoundInfo.WriteInt32(packet.OverDamage);
        attackRoundInfo.WriteUInt8((byte)packet.SubDmg.Count);

        foreach (var subDmg in packet.SubDmg)
        {
            attackRoundInfo.WriteUInt32(subDmg.SchoolMask);
            attackRoundInfo.WriteFloat(subDmg.FloatDamage);
            attackRoundInfo.WriteInt32(subDmg.IntDamage);
            if (hitInfo.HasAnyFlag(HitInfo.FullAbsorb | HitInfo.PartialAbsorb))
                attackRoundInfo.WriteInt32(subDmg.Absorbed);
            if (hitInfo.HasAnyFlag(HitInfo.FullResist | HitInfo.PartialResist))
                attackRoundInfo.WriteInt32(subDmg.Resisted);
        }

        attackRoundInfo.WriteUInt8(packet.VictimState);
        attackRoundInfo.WriteInt32(packet.AttackerState);
        attackRoundInfo.WriteUInt32(packet.MeleeSpellID);

        if (hitInfo.HasAnyFlag(HitInfo.Block))
            attackRoundInfo.WriteInt32(packet.BlockAmount);

        if (hitInfo.HasAnyFlag(HitInfo.RageGain))
            attackRoundInfo.WriteInt32(packet.RageGained);

        if (hitInfo.HasAnyFlag(HitInfo.Unk0))
        {
            var unkState = packet.UnkState;
            attackRoundInfo.WriteUInt32(unkState.State1);
            attackRoundInfo.WriteFloat(unkState.State2);
            attackRoundInfo.WriteFloat(unkState.State3);
            attackRoundInfo.WriteFloat(unkState.State4);
            attackRoundInfo.WriteFloat(unkState.State5);
            attackRoundInfo.WriteFloat(unkState.State6);
            attackRoundInfo.WriteFloat(unkState.State7);
            attackRoundInfo.WriteFloat(unkState.State8);
            attackRoundInfo.WriteFloat(unkState.State9);
            attackRoundInfo.WriteFloat(unkState.State10);
            attackRoundInfo.WriteFloat(unkState.State11);
            for (int i = 1; i < damageRanges; i++)
            {
                attackRoundInfo.WriteFloat(0.0f);
                attackRoundInfo.WriteFloat(0.0f);
            }
            attackRoundInfo.WriteUInt32(unkState.State12);
        }

        if (hitInfo.HasAnyFlag(HitInfo.Block | HitInfo.Unk12))
            attackRoundInfo.WriteFloat(packet.Unk);
    }

    private static void WriteLogDataAndRound(AttackerStateUpdate packet, WorldPacket data, WorldPacket attackRoundInfo)
    {
        data.WriteBit(packet.LogData != null);
        if (packet.LogData != null)
            packet.LogData.Write(data);
        data.FlushBits();

        data.WriteUInt32(attackRoundInfo.GetSize());
        data.WriteBytes(attackRoundInfo);
    }

    public HitInfo HitInfo; // Flags
    public WowGuid128 AttackerGUID;
    public WowGuid128 VictimGUID;
    public int Damage;
    public int OriginalDamage;
    public int OverDamage = -1; // (damage - health) or -1 if unit is still alive
    public List<SubDamage> SubDmg = new();
    public byte VictimState;
    public int AttackerState = 0;
    public uint MeleeSpellID = 0;
    public int BlockAmount;
    public int RageGained = 0;
    public UnkAttackerState UnkState;
    public float Unk = 0.0f;
    public ContentTuningParams ContentTuning = new();
    public SpellCastLogData LogData = null!;
}

public class SubDamage
{
    public uint SchoolMask;
    public float FloatDamage; // Float damage (Most of the time equals to Damage)
    public int IntDamage;
    public int Absorbed;
    public int Resisted;
}

public struct UnkAttackerState
{
    public uint State1;
    public float State2;
    public float State3;
    public float State4;
    public float State5;
    public float State6;
    public float State7;
    public float State8;
    public float State9;
    public float State10;
    public float State11;
    public uint State12;
}

public class SpellCastLogData
{
    public void Write(WorldPacket data)
    {
        data.WriteInt64(Health);
        data.WriteInt32(AttackPower);
        data.WriteInt32(SpellPower);
        data.WriteUInt32(Armor);
        data.WriteBits(PowerData.Count, 9);
        data.FlushBits();

        foreach (SpellLogPowerData powerData in PowerData)
        {
            data.WriteInt32(powerData.PowerType);
            data.WriteInt32(powerData.Amount);
            data.WriteInt32(powerData.Cost);
        }
    }

    public long Health;
    public int AttackPower;
    public int SpellPower;
    public uint Armor;
    public List<SpellLogPowerData> PowerData = new();
}

public struct SpellLogPowerData
{
    public int PowerType;
    public int Amount;
    public int Cost;
}

public class CancelCombat : ServerPacket, ISpanWritable
{
    public CancelCombat() : base(Opcode.SMSG_CANCEL_COMBAT) { }

    public override void Write() { }

    public int MaxSize => 0; // Empty packet

    public int WriteToSpan(Span<byte> buffer) => 0;
}

/// <summary>
/// Data only. The class this replaced defaulted <c>Animate</c> to true, which a positional record
/// struct cannot express — harmless here because the codec assigns it on every path, but the
/// reason the frozen oracle keeps that initializer.
/// </summary>
public readonly record struct SetSheathed(int SheathState, bool Animate);

public class AIReaction : ServerPacket, ISpanWritable
{
    public AIReaction() : base(Opcode.SMSG_AI_REACTION, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(UnitGUID);
        _worldPacket.WriteUInt32(Reaction);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4; // Packed GUID + uint32

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(UnitGUID.Low, UnitGUID.High);
        writer.WriteUInt32(Reaction);
        return writer.Position;
    }

    public WowGuid128 UnitGUID;
    public uint Reaction;
}

class PartyKillLog : ServerPacket, ISpanWritable
{
    public PartyKillLog() : base(Opcode.SMSG_PARTY_KILL_LOG) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Player);
        _worldPacket.WritePackedGuid128(Victim);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2; // 2 packed GUIDs

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Player.Low, Player.High);
        writer.WritePackedGuid128(Victim.Low, Victim.High);
        return writer.Position;
    }

    public WowGuid128 Player;
    public WowGuid128 Victim;
}

public readonly struct ThreatInfo
{
    public readonly WowGuid128 UnitGUID;
    public readonly long Threat;

    public ThreatInfo(WowGuid128 unitGuid, long threat)
    {
        UnitGUID = unitGuid;
        Threat = threat;
    }
}

class ThreatUpdate : ServerPacket
{
    public ThreatUpdate() : base(Opcode.SMSG_THREAT_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(UnitGUID);
        _worldPacket.WriteInt32(ThreatList.Count);
        foreach (ThreatInfo info in ThreatList)
        {
            _worldPacket.WritePackedGuid128(info.UnitGUID);
            _worldPacket.WriteInt64(info.Threat);
        }
    }

    public WowGuid128 UnitGUID;
    public List<ThreatInfo> ThreatList = new();
}

class HighestThreatUpdate : ServerPacket
{
    public HighestThreatUpdate() : base(Opcode.SMSG_HIGHEST_THREAT_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(UnitGUID);
        _worldPacket.WritePackedGuid128(HighestThreatGUID);
        _worldPacket.WriteInt32(ThreatList.Count);
        foreach (ThreatInfo info in ThreatList)
        {
            _worldPacket.WritePackedGuid128(info.UnitGUID);
            _worldPacket.WriteInt64(info.Threat);
        }
    }

    public WowGuid128 UnitGUID;
    public WowGuid128 HighestThreatGUID;
    public List<ThreatInfo> ThreatList = new();
}

class ThreatRemove : ServerPacket, ISpanWritable
{
    public ThreatRemove() : base(Opcode.SMSG_THREAT_REMOVE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(UnitGUID);
        _worldPacket.WritePackedGuid128(AboutGUID);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(UnitGUID.Low, UnitGUID.High);
        writer.WritePackedGuid128(AboutGUID.Low, AboutGUID.High);
        return writer.Position;
    }

    public WowGuid128 UnitGUID;
    public WowGuid128 AboutGUID;
}

class ThreatClear : ServerPacket, ISpanWritable
{
    public ThreatClear() : base(Opcode.SMSG_THREAT_CLEAR, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(GUID);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(GUID.Low, GUID.High);
        return writer.Position;
    }

    public WowGuid128 GUID;
}

class Dismount : ServerPacket, ISpanWritable
{
    public Dismount() : base(Opcode.SMSG_DISMOUNT, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        return writer.Position;
    }

    public WowGuid128 Guid;
}

class BreakTarget : ServerPacket, ISpanWritable
{
    public BreakTarget() : base(Opcode.SMSG_BREAK_TARGET, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(UnitGUID);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(UnitGUID.Low, UnitGUID.High);
        return writer.Position;
    }

    public WowGuid128 UnitGUID;
}

class ClearTarget : ServerPacket, ISpanWritable
{
    public ClearTarget() : base(Opcode.SMSG_CLEAR_TARGET, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        return writer.Position;
    }

    public WowGuid128 Guid;
}
