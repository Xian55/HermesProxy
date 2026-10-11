using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The in-world <see cref="FeatureSystemStatus"/> got a 4.4.2 layout. It must give, byte for
/// byte, the packet a native TrinityCore cata_classic server sent a 4.4.2.60895 client; the
/// earlier builds keep the writers they had.
/// </summary>
public sealed class FeatureSystemStatusLayoutTests
{
    // SMSG_FEATURE_SYSTEM_STATUS from a stock cata_classic realm, payload only (240 bytes).
    private const string NativeCataClassic =
        "02020000000000000000000000000000000000000000000000000000002c0100000000000000000000000000" +
        "00000000000000000060ea000000000000020000000000000000000000320058020a000000000000000a0000" +
        "0001000000010000006200000001000000000000005d0000000100000000000000400419200aa20180000000" +
        "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "000000000a00000060ea00000100000007b20100";

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<FeatureSystemStatus.PreCataClassicLayout>(FeatureSystemStatus.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<FeatureSystemStatus.PreCataClassicLayout>(FeatureSystemStatus.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<FeatureSystemStatus.CataClassicLayout>(FeatureSystemStatus.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void CataClassicLayout_MatchesTheNativeServerPacket()
    {
        var packet = new FeatureSystemStatus
        {
            ComplaintStatus = 2,
            CfgRealmID = 2,
            TokenPollTimeSeconds = 300,
            HiddenUIClubsPresenceUpdateTimer = 60000,
            MaxPlayerNameQueriesPerPacket = 50,
            PlayerNameQueryTelemetryInterval = 600,
            TutorialsEnabled = true,
            WarModeFeatureEnabled = true,
            ClubsEnabled = true,
            ClubsPresenceUpdateEnabled = true,
            QuestSessionEnabled = true,
            EuropaTicketSystemStatus = new EuropaTicketConfig
            {
                ThrottleState = { MaxTries = 10, PerMilliseconds = 60000, TryCount = 1, LastResetTimeBeforeNow = 111111 },
            },
        };
        packet.GameRuleValues.Add(new GameRuleValuePair { Rule = 98, Value = 1 });
        packet.GameRuleValues.Add(new GameRuleValuePair { Rule = 93, Value = 1 });

        using var data = new WorldPacket(1u);
        new FeatureSystemStatus.CataClassicLayout().Write(packet, data);

        Assert.Equal(NativeCataClassic, Convert.ToHexString(data.GetDataSpan()).ToLowerInvariant());
    }
}
