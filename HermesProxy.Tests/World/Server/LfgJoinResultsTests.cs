using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Legacy 3.3.5a and V3_4_3 number lfg::LfgJoinResult differently. Forwarding the raw legacy
/// byte made every Dungeon Finder rejection invisible in the modern client.
/// </summary>
public class LfgJoinResultsTests
{
    [Theory]
    [InlineData(0, 0x00)]   // OK
    [InlineData(2, 0x1F)]   // group full
    [InlineData(5, 0x22)]   // does not meet requirements
    [InlineData(11, 0x27)]  // one or more dungeons was not valid
    [InlineData(12, 0x28)]  // deserter debuff
    [InlineData(14, 0x2A)]  // random dungeon cooldown
    [InlineData(17, 0x2D)]  // using the battleground system
    public void ToModern_MapsKnownLegacyCodes(byte legacy, byte expected)
    {
        Assert.Equal(expected, LfgJoinResults.ToModern(legacy));
    }

    [Fact]
    public void ToModern_KeepsPartyRequirementsCodeUnchanged()
    {
        // The only value both enums happen to share.
        Assert.Equal(6, LfgJoinResults.ToModern(6));
    }

    [Fact]
    public void ToModern_WithUnknownCode_FallsBackToAnInternalError()
    {
        // Showing "Internal LFG Error" beats showing nothing at all.
        Assert.Equal(LfgJoinResults.ModernNoLfgObject, LfgJoinResults.ToModern(200));
    }

    [Fact]
    public void ToModern_NeverReturnsOkForAFailure()
    {
        for (byte legacy = 1; legacy < 60; legacy++)
            Assert.NotEqual(LfgJoinResults.ModernOk, LfgJoinResults.ToModern(legacy));
    }
}
