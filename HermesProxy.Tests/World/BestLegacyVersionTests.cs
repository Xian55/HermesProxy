using System;
using System.Linq;
using HermesProxy.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// <c>LegacyServerOptions:Build = auto</c> resolves through
/// <see cref="VersionChecker.GetBestLegacyVersion"/>. A supported client whose expansion it did
/// not map resolved to <c>Zero</c>, and the proxy refused to start; 4.4.2 did exactly that.
/// </summary>
public sealed class BestLegacyVersionTests
{
    public static TheoryData<ClientVersionBuild> SupportedModernBuilds()
    {
        var builds = new TheoryData<ClientVersionBuild>();
        foreach (var build in Enum.GetValues<ClientVersionBuild>().Where(VersionChecker.IsSupportedModernVersion))
            builds.Add(build);
        return builds;
    }

    [Theory]
    [MemberData(nameof(SupportedModernBuilds))]
    public void GetBestLegacyVersion_EverySupportedClient_ResolvesToASupportedServer(ClientVersionBuild build)
        => Assert.True(VersionChecker.IsSupportedLegacyVersion(VersionChecker.GetBestLegacyVersion(build)));

    [Fact]
    public void GetBestLegacyVersion_CataclysmClassic_IsCataclysm434()
        => Assert.Equal(ClientVersionBuild.V4_3_4_15595, VersionChecker.GetBestLegacyVersion(ClientVersionBuild.V4_4_2_60895));
}
