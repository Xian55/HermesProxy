using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_ATTACKER_STATE_UPDATE ends its attack round with the content tuning. Up to 3.4.3 that is
/// the combat log's 13-byte form; from 4.4.1 on it is ContentTuningParams, 14 bytes (TrinityCore
/// cata_classic, WowPacketParser's 4.4 reader). Written in the older form, every swing reached the
/// 4.4.2 client one byte short.
/// </summary>
public sealed class AttackerStateUpdateLayoutTests
{
    // HitInfo AffectsVictim, two empty GUIDs, 321 damage (not a kill), one physical sub-damage,
    // VictimState normal, AttackerState 0, no melee spell.
    private const string RoundUpToTuning =
        "02000000" + "0000" + "0000" + "41010000" + "41010000" + "FFFFFFFF"
        + "01" + "01000000" + "0080A043" + "41010000"
        + "01" + "00000000" + "00000000";

    [Fact]
    public void CombatLogTuningLayout_IsTheFormerWrite()
    {
        string expected = "00"                      // no log data, flushed
            + "37000000"                            // 55 bytes of round
            + RoundUpToTuning
            + "02" + "14" + "03" + "DAFF"           // type, target level, expansion, level delta
            + "00004843" + "00007041";              // item levels 200 and 15 as floats

        Assert.Equal(expected, Write(new AttackerStateUpdate.CombatLogTuningLayout()));
    }

    [Fact]
    public void ContentTuningParamsLayout_IsTrinityCoreCataClassics()
    {
        string expected = "00"
            + "38000000"                            // 56 bytes of round
            + RoundUpToTuning
            + "C800" + "DAFF" + "0F000000"          // item level 200 (u16), level delta, target item level 15 (u32)
            + "14" + "03" + "01" + "55" + "FE"      // target level, expansion, scaling 1-85, scaling delta -2
            + "20";                                 // type 2 in four bits, ScalesWithItemLevel 0, flushed

        Assert.Equal(expected, Write(new AttackerStateUpdate.ContentTuningParamsLayout()));
    }

    [Fact]
    public void ContentTuningParamsLayout_DebugBlockHasFiveDamageRanges()
    {
        var packet = Sample();
        packet.HitInfo |= HitInfo.Unk0;
        packet.UnkState.State10 = 10f;
        packet.UnkState.State11 = 20f;

        using var older = new WorldPacket(1u);
        new AttackerStateUpdate.CombatLogTuningLayout().Write(packet, older);
        using var cata = new WorldPacket(1u);
        new AttackerStateUpdate.ContentTuningParamsLayout().Write(packet, cata);

        // four more (min, max) pairs of zero, and the tuning one byte longer
        Assert.Equal(older.GetDataSpan().Length + 4 * 8 + 1, cata.GetDataSpan().Length);
    }

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<AttackerStateUpdate.CombatLogTuningLayout>(AttackerStateUpdate.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<AttackerStateUpdate.CombatLogTuningLayout>(AttackerStateUpdate.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<AttackerStateUpdate.CombatLogTuningLayout>(AttackerStateUpdate.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<AttackerStateUpdate.ContentTuningParamsLayout>(AttackerStateUpdate.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    private static AttackerStateUpdate Sample()
    {
        var packet = new AttackerStateUpdate
        {
            HitInfo = HitInfo.AffectsVictim,
            Damage = 321,
            OriginalDamage = 321,
            VictimState = 1,
        };
        packet.SubDmg.Add(new SubDamage { SchoolMask = 1, FloatDamage = 321f, IntDamage = 321 });
        packet.ContentTuning.TuningType = ContentTuningParams.ContentTuningType.PlayerToCreatureDamage;
        packet.ContentTuning.TargetLevel = 20;
        packet.ContentTuning.Expansion = 3;
        packet.ContentTuning.PlayerLevelDelta = -38;
        packet.ContentTuning.PlayerItemLevel = 200f;
        packet.ContentTuning.TargetItemLevel = 15f;
        packet.ContentTuning.TargetMinScalingLevel = 1;
        packet.ContentTuning.TargetMaxScalingLevel = 85;
        packet.ContentTuning.TargetScalingLevelDelta = -2;
        return packet;
    }

    private static string Write(ServerPacketLayout<AttackerStateUpdate> layout)
    {
        using var data = new WorldPacket(1u);
        layout.Write(Sample(), data);
        data.FlushBits();
        return Convert.ToHexString(data.GetDataSpan());
    }
}
