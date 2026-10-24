using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The PvP window's CMSG_REQUEST_PVP_REWARDS with a 3.3.5a server, which has no such request and
/// says what a battleground pays in every SMSG_BATTLEFIELD_LIST instead (issue #483). The proxy
/// keeps the last list's amounts, answers the request with them and sends new ones ahead of the list.
/// </summary>
/// <remarks>A 4.3.4 server answers the request itself; these tests run with a 3.3.5a one.</remarks>
public sealed class PvpRewardsWotLKTests
{
    private const uint RandomBattleground = 32;
    private const uint WarsongGulch = 2;

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);

    [Fact]
    public void AList_SendsItsRewardsAheadOfItself()
    {
        if (WorldClient.IsCataLegacy)
            return;

        DeliverList(WarsongGulch, winHonor: 930, winArena: 25, lossHonor: 155);

        Assert.Equal(2, _harness.ClientWire.Sent.Count);
        AssertRewards(_harness.ClientWire.Sent[0].Packet, winHonor: 930, winArena: 25, lossHonor: 155);
        Assert.IsType<BattlefieldList>(_harness.ClientWire.Sent[1].Packet);
    }

    [Fact]
    public void TheSameAmountsAgain_SendOnlyTheList()
    {
        if (WorldClient.IsCataLegacy)
            return;

        DeliverList(WarsongGulch, winHonor: 930, winArena: 25, lossHonor: 155);
        DeliverList(RandomBattleground, winHonor: 930, winArena: 25, lossHonor: 155);

        Assert.Equal(3, _harness.ClientWire.Sent.Count);
        Assert.IsType<BattlefieldList>(_harness.ClientWire.Sent[2].Packet);
    }

    [Fact]
    public void TheRandomBattlegroundsOwnAmounts_AreTheOnesKept()
    {
        if (WorldClient.IsCataLegacy)
            return;

        // After the first win of the day: 15 kills' honor and no arena points.
        DeliverList(RandomBattleground, winHonor: 930, winArena: 25, lossHonor: 155, random: (465, 0, 155));

        AssertRewards(_harness.ClientWire.Sent[0].Packet, winHonor: 465, winArena: 0, lossHonor: 155);
        Assert.Equal((465, 0, 155), _harness.Session.GameState.WotLKBattlegroundRewards);
    }

    [Fact]
    public void TheRequest_IsAnsweredWithTheLastList()
    {
        if (WorldClient.IsCataLegacy)
            return;

        var ctx = new SessionContext(_harness.Session, socket: null, _harness.Client);
        BattlegroundSystem.HandleRequestPvpRewards(new EmptyClientPacket(), in ctx);
        Assert.Empty(_harness.ClientWire.Sent);

        DeliverList(WarsongGulch, winHonor: 930, winArena: 25, lossHonor: 155);
        BattlegroundSystem.HandleRequestPvpRewards(new EmptyClientPacket(), in ctx);

        Assert.Equal(3, _harness.ClientWire.Sent.Count);
        AssertRewards(_harness.ClientWire.Sent[2].Packet, winHonor: 930, winArena: 25, lossHonor: 155);
        Assert.Empty(_harness.ServerWire.Sent);
    }

    private static void AssertRewards(ServerPacket sent, int winHonor, int winArena, int lossHonor)
    {
        var rewards = Assert.IsType<RequestPvpRewardsResponse>(sent);
        foreach (var win in new[] { rewards.FirstRandomBGWinReward, rewards.NthRandomBGWinReward, rewards.HolidayBGWinReward })
        {
            Assert.Equal(winHonor, win.Honor);
            if (winArena > 0)
                Assert.Equal((int)Currency.ArenaPoints, Assert.Single(win.Currency!).CurrencyID);
            else
                Assert.Null(win.Currency);
        }
        foreach (var loss in new[] { rewards.FirstRandomBGLossReward, rewards.NthRandomBGLossReward, rewards.HolidayBGLossReward })
        {
            Assert.Equal(lossHonor, loss.Honor);
            Assert.Null(loss.Currency);
        }
    }

    // SMSG_BATTLEFIELD_LIST as TrinityCore, AzerothCore and cMaNGOS 3.3.5 write it: the holiday's
    // rewards in every list, the random battleground's own only in its list.
    private void DeliverList(uint battleground, int winHonor, int winArena, int lossHonor, (int WinHonor, int WinArena, int LossHonor)? random = null)
    {
        bool isRandom = battleground == RandomBattleground;
        var own = random ?? (winHonor, winArena, lossHonor);
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_BATTLEFIELD_LIST, p =>
        {
            p.WriteUInt64(0);           // battlemaster
            p.WriteUInt8(1);            // from the PvP window
            p.WriteUInt32(battleground);
            p.WriteUInt8(0);
            p.WriteUInt8(0);
            p.WriteUInt8(0);            // has won a random battleground today
            p.WriteInt32(winHonor);
            p.WriteInt32(winArena);
            p.WriteInt32(lossHonor);
            p.WriteUInt8(isRandom ? (byte)1 : (byte)0);
            if (isRandom)
            {
                p.WriteUInt8(random.HasValue ? (byte)1 : (byte)0);
                p.WriteInt32(own.WinHonor);
                p.WriteInt32(own.WinArena);
                p.WriteInt32(own.LossHonor);
            }
            p.WriteUInt32(0);           // instances
        });
        _harness.Deliver(Opcode.SMSG_BATTLEFIELD_LIST, wire, _harness.Client.HandleBattlefieldListWotLK);
    }
}
