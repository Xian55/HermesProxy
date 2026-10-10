using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// <see cref="FeatureSystemStatusGlueScreen"/> went from one writer with an expansion branch to one
/// layout per shape. The 1.14/2.5 and 3.4.3 layouts must give the bytes the old branches did; the
/// 4.4.2 layout must give the bytes of a native TrinityCore cata_classic packet.
/// </summary>
public sealed class FeatureSystemStatusGlueScreenLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<FeatureSystemStatusGlueScreen.ClassicEraLayout>(FeatureSystemStatusGlueScreen.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<FeatureSystemStatusGlueScreen.ClassicEraLayout>(FeatureSystemStatusGlueScreen.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<FeatureSystemStatusGlueScreen.WotLKClassicLayout>(FeatureSystemStatusGlueScreen.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<FeatureSystemStatusGlueScreen.CataClassicLayout>(FeatureSystemStatusGlueScreen.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClassicEraLayout_MatchesTheOldLegacyBranch(bool withEuropa)
    {
        var packet = Sample(withEuropa);
        Assert.Equal(OldWrite(packet, wotlk: false), Write(new FeatureSystemStatusGlueScreen.ClassicEraLayout(), packet));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WotLKClassicLayout_MatchesTheOld3_4_3Branch(bool withEuropa)
    {
        var packet = Sample(withEuropa);
        Assert.Equal(OldWrite(packet, wotlk: true), Write(new FeatureSystemStatusGlueScreen.WotLKClassicLayout(), packet));
    }

    [Fact]
    public void CataClassicLayout_MatchesANativePacket()
    {
        var europa = new EuropaTicketConfig();
        europa.ThrottleState.MaxTries = 10;
        europa.ThrottleState.PerMilliseconds = 60000;
        europa.ThrottleState.TryCount = 1;
        europa.ThrottleState.LastResetTimeBeforeNow = 111111;

        var packet = new FeatureSystemStatusGlueScreen
        {
            EuropaTicketSystemStatus = europa,
            MaxCharactersPerRealm = 60,
            MinimumExpansionLevel = 0,
            MaximumExpansionLevel = 3,
            MaxPlayerNameQueriesPerPacket = 50,
            PlayerNameQueryTelemetryInterval = 600,
            PlayerNameQueryInterval = 10,
            BNSendWhisperUseV2Services = true,
            BNSendGameDataUseV2Services = true,
        };
        packet.GameRuleValues.Add(new GameRuleValuePair { Rule = 98, Value = 1 });
        packet.GameRuleValues.Add(new GameRuleValuePair { Rule = 93, Value = 1 });

        Assert.Equal(NativePacket, Convert.ToHexString(Write(new FeatureSystemStatusGlueScreen.CataClassicLayout(), packet)));
    }

    // SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN from TrinityCore cata_classic to a 4.4.2.60895 client
    // (refs/native-captures/tc_cata_442_20261010-150803.pkt, packet 6).
    private const string NativePacket =
        "000008000038000A00000060EA00000100000007B20100000000000000000000000000000000003C000000000000000000000000000000000000000000000003" +
        "00000000000000020000000000000000000000320058020A0000000000000000000000000000006200000001000000000000005D0000000100000000000000";

    private static byte[] Write(ServerPacketLayout<FeatureSystemStatusGlueScreen> layout, FeatureSystemStatusGlueScreen packet)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        return data.GetDataSpan().ToArray();
    }

    private static FeatureSystemStatusGlueScreen Sample(bool withEuropa)
    {
        EuropaTicketConfig? europa = null;
        if (withEuropa)
        {
            europa = new EuropaTicketConfig { TicketsEnabled = true, ComplaintsEnabled = true };
            europa.ThrottleState.MaxTries = 10;
            europa.ThrottleState.PerMilliseconds = 60000;
            europa.ThrottleState.TryCount = 1;
            europa.ThrottleState.LastResetTimeBeforeNow = 111111;
        }

        var packet = new FeatureSystemStatusGlueScreen
        {
            BpayStoreEnabled = true,
            CharUndeleteEnabled = true,
            Unk14 = true,
            KioskModeEnabled = true,
            TrialBoostEnabled = true,
            LiveRegionCharacterListEnabled = true,
            LiveRegionKeyBindingsCopyEnabled = true,
            Unknown901CheckoutRelated = true,
            EuropaTicketSystemStatus = europa!,
            TokenPollTimeSeconds = 300,
            KioskSessionMinutes = 30,
            TokenBalanceAmount = -5,
            MaxCharactersPerRealm = 10,
            BpayStoreProductDeliveryDelay = 180,
            ActiveCharacterUpgradeBoostType = 2,
            ActiveClassTrialBoostType = 3,
            MinimumExpansionLevel = 5,
            MaximumExpansionLevel = 8,
            ActiveSeason = 4,
            MaxPlayerNameQueriesPerPacket = 50,
            PlayerNameQueryTelemetryInterval = 600,
        };
        packet.LiveRegionCharacterCopySourceRegions.AddRange([1, 3]);
        packet.GameRuleValues.Add(new GameRuleValuePair { Rule = 98, Value = 1 });
        return packet;
    }

    // FeatureSystemStatusGlueScreen.Write as it stood before the layouts, frozen.
    private static byte[] OldWrite(FeatureSystemStatusGlueScreen p, bool wotlk)
    {
        using var w = new WorldPacket(1u);
        if (wotlk)
        {
            w.WriteBit(p.BpayStoreEnabled);
            w.WriteBit(p.BpayStoreAvailable);
            w.WriteBit(p.BpayStoreDisabledByParentalControls);
            w.WriteBit(p.CharUndeleteEnabled);
            w.WriteBit(p.CommerceSystemEnabled);
            w.WriteBit(p.Unk14);
            w.WriteBit(p.WillKickFromWorld);
            w.WriteBit(p.IsExpansionPreorderInStore);

            w.WriteBit(p.KioskModeEnabled);
            w.WriteBit(p.CompetitiveModeEnabled);
            w.WriteBit(false);
            w.WriteBit(p.TrialBoostEnabled);
            w.WriteBit(p.TokenBalanceEnabled);
            w.WriteBit(p.LiveRegionCharacterListEnabled);
            w.WriteBit(p.LiveRegionCharacterCopyEnabled);
            w.WriteBit(p.LiveRegionAccountCopyEnabled);

            w.WriteBit(p.LiveRegionKeyBindingsCopyEnabled);
            w.WriteBit(p.Unknown901CheckoutRelated);
            w.WriteBit(false);
            w.WriteBit(p.EuropaTicketSystemStatus != null);
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(false);

            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(false);
            w.WriteBit(false);

            w.FlushBits();

            if (p.EuropaTicketSystemStatus != null)
                p.EuropaTicketSystemStatus.Write(w);

            w.WriteUInt32(p.TokenPollTimeSeconds);
            w.WriteUInt32(p.KioskSessionMinutes);
            w.WriteInt64(p.TokenBalanceAmount);
            w.WriteInt32(p.MaxCharactersPerRealm);
            w.WriteInt32(p.LiveRegionCharacterCopySourceRegions.Count);
            w.WriteUInt32(p.BpayStoreProductDeliveryDelay);
            w.WriteInt32(p.ActiveCharacterUpgradeBoostType);
            w.WriteInt32(p.ActiveClassTrialBoostType);
            w.WriteInt32(p.MinimumExpansionLevel);
            w.WriteInt32(p.MaximumExpansionLevel);
            w.WriteInt32(p.ActiveSeason);
            w.WriteInt32(p.GameRuleValues.Count);
            w.WriteInt16(p.MaxPlayerNameQueriesPerPacket);
            w.WriteInt16(p.PlayerNameQueryTelemetryInterval);
            w.WriteInt32(0);
            w.WriteInt32(0);
            w.WriteInt32(0);

            foreach (var sourceRegion in p.LiveRegionCharacterCopySourceRegions)
                w.WriteInt32(sourceRegion);

            foreach (var rulePair in p.GameRuleValues)
                rulePair.Write(w);

            return w.GetDataSpan().ToArray();
        }

        w.WriteBit(p.BpayStoreEnabled);
        w.WriteBit(p.BpayStoreAvailable);
        w.WriteBit(p.BpayStoreDisabledByParentalControls);
        w.WriteBit(p.CharUndeleteEnabled);
        w.WriteBit(p.CommerceSystemEnabled);
        w.WriteBit(p.Unk14);
        w.WriteBit(p.WillKickFromWorld);
        w.WriteBit(p.IsExpansionPreorderInStore);
        w.WriteBit(p.KioskModeEnabled);
        w.WriteBit(p.CompetitiveModeEnabled);
        w.WriteBit(p.TrialBoostEnabled);
        w.WriteBit(p.TokenBalanceEnabled);
        w.WriteBit(p.LiveRegionCharacterListEnabled);
        w.WriteBit(p.LiveRegionCharacterCopyEnabled);
        w.WriteBit(p.LiveRegionAccountCopyEnabled);
        w.WriteBit(p.LiveRegionKeyBindingsCopyEnabled);
        w.WriteBit(p.Unknown901CheckoutRelated);
        w.WriteBit(p.EuropaTicketSystemStatus != null);
        w.FlushBits();

        if (p.EuropaTicketSystemStatus != null)
            p.EuropaTicketSystemStatus.Write(w);

        w.WriteUInt32(p.TokenPollTimeSeconds);
        w.WriteUInt32(p.KioskSessionMinutes);
        w.WriteInt64(p.TokenBalanceAmount);
        w.WriteInt32(p.MaxCharactersPerRealm);
        w.WriteInt32(p.LiveRegionCharacterCopySourceRegions.Count);
        w.WriteUInt32(p.BpayStoreProductDeliveryDelay);
        w.WriteInt32(p.ActiveCharacterUpgradeBoostType);
        w.WriteInt32(p.ActiveClassTrialBoostType);
        w.WriteInt32(p.MinimumExpansionLevel);
        w.WriteInt32(p.MaximumExpansionLevel);

        if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
        {
            w.WriteInt32(p.ActiveSeason);
            w.WriteInt32(p.GameRuleValues.Count);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                w.WriteInt16(p.MaxPlayerNameQueriesPerPacket);

            if (ModernVersion.AddedInVersion(9, 2, 7, 1, 14, 4, 3, 4, 0))
                w.WriteInt16(p.PlayerNameQueryTelemetryInterval);
        }

        foreach (var sourceRegion in p.LiveRegionCharacterCopySourceRegions)
            w.WriteInt32(sourceRegion);

        if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
        {
            foreach (var rulePair in p.GameRuleValues)
                rulePair.Write(w);
        }

        return w.GetDataSpan().ToArray();
    }
}
