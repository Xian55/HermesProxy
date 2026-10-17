using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// What an arena join becomes on the legacy server (#423). A 3.3.5a server reads the
/// battlemaster's GUID, the slot, "as group" and "is rated". TrinityCore 4.3.4
/// <c>HandleBattlemasterJoinArena</c> reads the slot alone, and 4.3.4 has no skirmishes.
/// </summary>
/// <remarks>
/// The 4.3.4 side runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).
/// </remarks>
public class ArenaJoinTests
{
    private static readonly WowGuid64 Battlemaster = new(HighGuidTypeLegacy.Creature, 19859, 77);

    private static (LegacyHandlerHarness Harness, SessionContext Ctx) NewSession()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 1));
        return (harness, new SessionContext(harness.Session, socket: null, harness.Client));
    }

    [Fact]
    public void RatedJoin_SendsWhatTheServerReads()
    {
        var (harness, ctx) = NewSession();
        var join = new BattlemasterJoinArena(Battlemaster.To128(harness.Session.GameState), 1, 0);

        ArenaSystem.HandleBattlematerJoinArena(in join, in ctx);

        var sent = Assert.Single(harness.ServerWire.Sent);
        using var expected = new WorldPacket();
        if (WorldClient.IsCataLegacy)
        {
            expected.WriteUInt8(1);
        }
        else
        {
            expected.WriteGuid(Battlemaster);
            expected.WriteUInt8(1);
            expected.WriteBool(true);
            expected.WriteBool(true);
        }
        Assert.Equal(expected.GetDataSpan().ToArray(), sent.Bytes);
    }

    [Fact]
    public void Skirmish_IsNotSentToA434Server()
    {
        var (harness, ctx) = NewSession();
        var join = new BattlemasterJoinSkirmish(Battlemaster.To128(harness.Session.GameState), 0, 2, true, false);

        ArenaSystem.HandleBattlematerJoinSkirmish(in join, in ctx);

        if (WorldClient.IsCataLegacy)
        {
            Assert.Empty(harness.ServerWire.Sent);
            return;
        }
        var sent = Assert.Single(harness.ServerWire.Sent);
        Assert.Equal(11, sent.Bytes.Length);
        Assert.Equal(0, sent.Bytes[^1]);     // not rated
    }
}
