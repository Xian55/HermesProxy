using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// A legacy guild id of 0 means "not in a guild" and must translate to an empty GUID.
/// </summary>
/// <remarks>
/// <c>WowGuid128.Create(HighGuidType703.Guild, 0)</c> does <b>not</b> produce an empty GUID: it
/// routes through <c>RealmSpecificCreate</c>, which packs the guild type and the realm id into the
/// high half, so only the counter ends up zero. A modern client reading that as
/// <c>UnitData.GuildGUID</c> still believes it is in a guild — which is exactly what happened after
/// <c>/gquit</c>: the server removed the player, sent <c>PLAYER_GUILDID = 0</c>, and the client kept
/// the guild UI up and carried on sending <c>CMSG_QUERY_GUILD_INFO</c> and guild-bank queries.
/// </remarks>
public class GuildGuidClearTests
{
    /// <summary>The trap itself. If this ever starts returning empty, the helper is redundant.</summary>
    [Fact]
    public void Create_WithZeroGuildId_IsNotEmpty()
    {
        var guid = WowGuid128.Create(HighGuidType703.Guild, 0);

        Assert.False(guid.IsEmpty());
        Assert.Equal(0UL, guid.Low);
        Assert.NotEqual(0UL, guid.High);
    }

    [Fact]
    public void CreateGuildOrEmpty_WithZeroGuildId_IsEmpty()
    {
        var guid = WowGuid128.CreateGuildOrEmpty(0);

        Assert.True(guid.IsEmpty());
        Assert.Equal(default, guid);
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(42u)]
    [InlineData(uint.MaxValue)]
    public void CreateGuildOrEmpty_WithRealGuildId_MatchesCreate(uint guildId)
    {
        Assert.Equal(WowGuid128.Create(HighGuidType703.Guild, guildId),
                     WowGuid128.CreateGuildOrEmpty(guildId));
    }

    [Fact]
    public void CreateGuildOrEmpty_WithRealGuildId_IsNotEmpty()
    {
        Assert.False(WowGuid128.CreateGuildOrEmpty(1).IsEmpty());
    }

    /// <summary>Distinct ids stay distinct — the clamp must not collapse anything but zero.</summary>
    [Fact]
    public void CreateGuildOrEmpty_DistinctIdsStayDistinct()
    {
        Assert.NotEqual(WowGuid128.CreateGuildOrEmpty(1), WowGuid128.CreateGuildOrEmpty(2));
        Assert.NotEqual(WowGuid128.CreateGuildOrEmpty(1), WowGuid128.CreateGuildOrEmpty(0));
    }
}
