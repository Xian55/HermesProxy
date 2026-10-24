using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_REQUEST_PVP_REWARDS_RESPONSE. The 3.4.3 client reads four rewards, the random
/// battleground's win and loss and then the holiday's, and shows a tenth of the honor (its handler,
/// rva 19b45d0, and the getter behind C_PvP.GetRandomBGRewards). The 4.4.2 client reads eight, and
/// its window shows a hundredth of what its getter (rva 1e840c0) gives, which is the honor as sent
/// under Cataclysm's expansion level and a tenth of it below that. The packet holds whole points.
/// </summary>
public sealed class RequestPvpRewardsResponseLayoutTests
{
    private const int ArenaPoints = (int)Currency.ArenaPoints;

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<RequestPvpRewardsResponse.WotLKClassicLayout>(RequestPvpRewardsResponse.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<RequestPvpRewardsResponse.CataClassicLayout>(RequestPvpRewardsResponse.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void WotLKClassic_RandomWinAndLoss_ThenTheHoliday_InTenths()
    {
        using var expected = new WorldPacket(1u);
        WriteReward(expected, 9300, (ArenaPoints, 25));
        WriteReward(expected, 1550);
        WriteReward(expected, 9310, (ArenaPoints, 26));
        WriteReward(expected, 1560);

        using var data = new WorldPacket(1u);
        new RequestPvpRewardsResponse.WotLKClassicLayout().Write(EverySlot(), data);

        Assert.Equal(expected.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    [Theory]
    [InlineData(3, 100)]   // a 4.3.4 server: Cataclysm
    [InlineData(2, 1000)]  // a 3.3.5a server: Wrath of the Lich King
    public void CataClassic_EightRewards_ScaledForTheExpansionLevel(byte expansionLevel, int scale)
    {
        using var expected = new WorldPacket(1u);
        WriteReward(expected, 930 * scale, (ArenaPoints, 25));
        WriteReward(expected, 155 * scale);
        WriteReward(expected, 465 * scale);
        WriteReward(expected, 77 * scale);
        WriteReward(expected, 1 * scale);
        WriteReward(expected, 2 * scale);
        WriteReward(expected, 3 * scale);
        WriteReward(expected, 5 * scale);

        using var data = new WorldPacket(1u);
        new RequestPvpRewardsResponse.CataClassicLayout(expansionLevel).Write(EverySlot(), data);

        Assert.Equal(expected.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    [Fact]
    public void ARewardWithoutHonor_HasNoHonorBit()
    {
        using var expected = new WorldPacket(1u);
        for (int i = 0; i < 4; i++)
            WriteReward(expected, null);

        using var data = new WorldPacket(1u);
        new RequestPvpRewardsResponse.WotLKClassicLayout().Write(new RequestPvpRewardsResponse(), data);

        Assert.Equal(expected.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    // A distinct amount in every slot, so a slot written in the wrong place shows.
    private static RequestPvpRewardsResponse EverySlot() => new()
    {
        FirstRandomBGWinReward = Reward(930, 25),
        FirstRandomBGLossReward = Reward(155),
        NthRandomBGWinReward = Reward(465),
        NthRandomBGLossReward = Reward(77),
        RatedBGRewards = Reward(1),
        Arena2v2Rewards = Reward(2),
        Arena3v3Rewards = Reward(3),
        Arena5v5Rewards = Reward(5),
        HolidayBGWinReward = Reward(931, 26),
        HolidayBGLossReward = Reward(156),
    };

    private static LFGPlayerQuestReward Reward(int honor, int arena = 0)
    {
        var reward = new LFGPlayerQuestReward { Honor = honor };
        if (arena > 0)
            reward.Currency = [new LFGPlayerQuestRewardCurrency { CurrencyID = ArenaPoints, Quantity = arena }];
        return reward;
    }

    // One reward as the client reads it: mask, money, experience, three counts, the lists, then
    // the bits of RewardSpellID, two unused fields and Honor, in that order, and the honor.
    private static void WriteReward(WorldPacket packet, int? honor, params (int Id, int Quantity)[] currencies)
    {
        packet.WriteUInt8(0);
        packet.WriteInt32(0);
        packet.WriteInt32(0);
        packet.WriteUInt32(0);
        packet.WriteUInt32((uint)currencies.Length);
        packet.WriteUInt32(0);
        foreach (var (id, quantity) in currencies)
        {
            packet.WriteInt32(id);
            packet.WriteInt32(quantity);
        }
        packet.WriteUInt8(honor.HasValue ? (byte)0x10 : (byte)0);
        if (honor.HasValue)
            packet.WriteInt32(honor.Value);
    }
}
