using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_SPELL_START and SMSG_SPELL_GO moved from one <see cref="SpellCastData"/> writer with a 3.4.3
/// branch to one writer per shape, picked by the packet's layout. The 1.14/2.5 and 3.4.3 writers must
/// give the bytes the old branches did; 4.4.2 differs from 3.4.3 only in the order of each
/// remaining-power entry (TrinityCore cata_classic, the client's reader, and a native SMSG_SPELL_GO
/// in refs/native-captures/tc_cata_442_20261010-141101.pkt, packet 254: Type 1, Cost 1000).
/// </summary>
public sealed class SpellCastLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<SpellGo.ClassicEraLayout>(SpellGo.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<SpellGo.WotLKClassicLayout>(SpellGo.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<SpellGo.CataClassicLayout>(SpellGo.Layouts.For(ClientVersionBuild.V4_4_2_60895));
        Assert.IsType<SpellStart.ClassicEraLayout>(SpellStart.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<SpellStart.WotLKClassicLayout>(SpellStart.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<SpellStart.CataClassicLayout>(SpellStart.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void ClassicEraLayout_MatchesTheOldLegacyBranch()
    {
        var cast = Sample();
        Assert.Equal(OldWrite(cast, wotlk: false), Write(new SpellStart.ClassicEraLayout(), new SpellStart { Cast = cast }));
        Assert.Equal([.. OldWrite(cast, wotlk: false), 0], Write(new SpellGo.ClassicEraLayout(), new SpellGo { Cast = cast }));
    }

    [Fact]
    public void WotLKClassicLayout_MatchesTheOld3_4_3Branch()
    {
        var cast = Sample();
        Assert.Equal(OldWrite(cast, wotlk: true), Write(new SpellStart.WotLKClassicLayout(), new SpellStart { Cast = cast }));
        Assert.Equal([.. OldWrite(cast, wotlk: true), 0], Write(new SpellGo.WotLKClassicLayout(), new SpellGo { Cast = cast }));
    }

    [Fact]
    public void CataClassicLayout_PutsThePowerTypeFirst()
    {
        var cast = Sample();
        cast.RemainingPower.Clear();
        cast.RemainingPower.Add(new SpellPowerData { Type = (PowerType)1, Cost = 1000 });

        byte[] wotlk = Write(new SpellGo.WotLKClassicLayout(), new SpellGo { Cast = cast });
        byte[] cata = Write(new SpellGo.CataClassicLayout(), new SpellGo { Cast = cast });

        // The power entry is the last thing before the log-data bit, the sample having no runes,
        // target points or ammo.
        Assert.Equal("E80300000100", Convert.ToHexString(wotlk.AsSpan(wotlk.Length - 6)));
        Assert.Equal("01E803000000", Convert.ToHexString(cata.AsSpan(cata.Length - 6)));
        Assert.Equal(wotlk.AsSpan(0, wotlk.Length - 6).ToArray(), cata.AsSpan(0, cata.Length - 6).ToArray());
    }

    private static byte[] Write<T>(ServerPacketLayout<T> layout, T packet) where T : ServerPacket
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        return data.GetDataSpan().ToArray();
    }

    private static SpellCastData Sample()
    {
        var player = new WowGuid128(1, 0x0800040000000000);
        var cast = new SpellCastData
        {
            CasterGUID = player,
            CasterUnit = player,
            CastID = new WowGuid128(167, 0xBC00040000026643),
            SpellID = 2457,
            SpellXSpellVisualID = 238278,
            CastFlags = 2305,
            CastFlagsEx = 512,
            CastTime = 697964,
        };
        cast.Target.Flags = (SpellCastTargetFlags)2;
        cast.Target.Unit = new WowGuid128(230, 0x200004000030B3C0);
        cast.HitTargets.Add(player);
        cast.MissTargets.Add(new WowGuid128(231, 0x200004000030B3C0));
        cast.MissStatus.Add(new SpellMissStatus(SpellMissInfo.Dodge, SpellMissInfo.None));
        cast.RemainingPower.Add(new SpellPowerData { Type = (PowerType)1, Cost = 1000 });
        cast.RemainingPower.Add(new SpellPowerData { Type = 0, Cost = 25 });
        return cast;
    }

    // SpellCastData.Write as it stood before the layouts, frozen; wotlk was
    // ModernVersion.IsWotLKClassicOrLater.
    private static byte[] OldWrite(SpellCastData c, bool wotlk)
    {
        using var data = new WorldPacket(1u);
        data.WritePackedGuid128(c.CasterGUID);
        data.WritePackedGuid128(c.CasterUnit);
        data.WritePackedGuid128(c.CastID);
        data.WritePackedGuid128(c.OriginalCastID);
        data.WriteInt32(c.SpellID);
        data.WriteUInt32(c.SpellXSpellVisualID);
        data.WriteUInt32(c.CastFlags);
        data.WriteUInt32(c.CastFlagsEx);
        data.WriteUInt32(c.CastTime);

        c.MissileTrajectory.Write(data);

        data.WriteUInt8(c.DestLocSpellCastIndex);

        c.Immunities.Write(data);
        c.Predict.Write(data);

        data.WriteBits(c.HitTargets.Count, 16);
        data.WriteBits(c.MissTargets.Count, 16);
        data.WriteBits(c.MissStatus.Count, 16);
        data.WriteBits(c.RemainingPower.Count, 9);
        data.WriteBit(c.RemainingRunes != null);
        data.WriteBits(c.TargetPoints.Count, 16);
        data.WriteBit(c.AmmoDisplayId != null);
        data.WriteBit(c.AmmoInventoryType != null);
        data.FlushBits();

        if (wotlk)
        {
            c.Target.Write(data);

            foreach (WowGuid128 hitTarget in c.HitTargets)
                data.WritePackedGuid128(hitTarget);

            foreach (WowGuid128 missTarget in c.MissTargets)
                data.WritePackedGuid128(missTarget);

            foreach (SpellMissStatus missStatus in c.MissStatus)
                missStatus.Write(data);

            foreach (SpellPowerData power in c.RemainingPower)
                power.Write(data);

            if (c.RemainingRunes != null)
                c.RemainingRunes.Write(data);
        }
        else
        {
            foreach (SpellMissStatus missStatus in c.MissStatus)
                missStatus.Write(data);

            c.Target.Write(data);

            foreach (WowGuid128 hitTarget in c.HitTargets)
                data.WritePackedGuid128(hitTarget);

            foreach (WowGuid128 missTarget in c.MissTargets)
                data.WritePackedGuid128(missTarget);

            foreach (SpellPowerData power in c.RemainingPower)
                power.Write(data);

            if (c.RemainingRunes != null)
                c.RemainingRunes.Write(data);
        }

        foreach (TargetLocation targetLoc in c.TargetPoints)
            targetLoc.Write(data);

        if (c.AmmoDisplayId != null)
            data.WriteInt32((int)c.AmmoDisplayId);

        if (c.AmmoInventoryType != null)
            data.WriteInt32((int)c.AmmoInventoryType);

        return data.GetDataSpan().ToArray();
    }
}
