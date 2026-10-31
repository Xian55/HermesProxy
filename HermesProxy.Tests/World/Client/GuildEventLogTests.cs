using HermesProxy.Enums;
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
/// The guild log from a 3.3.5a server, as AzerothCore writes it. Only a 4.3.4 server used to be
/// asked, so this log was always empty for a modern client.
/// </summary>
public sealed class GuildEventLogTests
{
    private const byte Invited = 1, Joined = 2, Promoted = 3, Left = 6;

    private static bool LegacyAsksThisWay => LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) && !WorldClient.IsCataLegacy;

    [Fact]
    public unsafe void Answer_HasALegacyHandler()
        => Assert.True(GeneratedSmsgDispatch.Get(Opcode.MSG_GUILD_EVENT_LOG_QUERY) != null);

    [Fact]
    public void Query_AsksTheServerUnderTheOneOpcodeItHas()
    {
        if (!LegacyAsksThisWay)
            return;

        var harness = new LegacyHandlerHarness(recordClientPackets: false, recordServerPackets: true);
        var ctx = new SessionContext(harness.Session, null, harness.Client);

        GuildSystem.HandleGuildEventLogQuery(default, in ctx);

        var sent = Assert.Single(harness.ServerWire.Sent);
        Assert.Equal(LegacyVersion.GetCurrentOpcode(Opcode.MSG_GUILD_EVENT_LOG_QUERY), sent.Opcode);
    }

    /// <summary>
    /// An entry's length depends on its kind, so one kind read wrongly loses every entry after it.
    /// </summary>
    [Fact]
    public void Answer_ReadsEveryShapeOfEntry()
    {
        if (!LegacyAsksThisWay)
            return;

        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var masterGuid = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        var memberGuid = new WowGuid64(HighGuidTypeLegacy.Player, 78);
        WowGuid128 master = harness.SetActivePlayer(masterGuid);
        WowGuid128 member = harness.AddKnownObject(memberGuid, ObjectType.Player);

        byte[] wire = LegacyPacketBuilder.Build(Opcode.MSG_GUILD_EVENT_LOG_QUERY, packet =>
        {
            packet.WriteUInt8(4);

            packet.WriteUInt8(Invited);
            packet.WriteGuid(masterGuid);
            packet.WriteGuid(memberGuid);
            packet.WriteUInt32(40);

            packet.WriteUInt8(Joined);
            packet.WriteGuid(memberGuid);
            packet.WriteUInt32(30);

            packet.WriteUInt8(Promoted);
            packet.WriteGuid(masterGuid);
            packet.WriteGuid(memberGuid);
            packet.WriteUInt8(3);
            packet.WriteUInt32(20);

            packet.WriteUInt8(Left);
            packet.WriteGuid(memberGuid);
            packet.WriteUInt32(10);
        });
        harness.Deliver(Opcode.MSG_GUILD_EVENT_LOG_QUERY, wire, harness.Client.HandleGuildEventLogQuery);

        var log = Assert.IsType<GuildEventLogQueryResults>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.Collection(log.Entries,
            e => Check(e, Invited, master, member, rank: 0, age: 40),
            e => Check(e, Joined, member, WowGuid128.Empty, rank: 0, age: 30),
            e => Check(e, Promoted, master, member, rank: 3, age: 20),
            e => Check(e, Left, member, WowGuid128.Empty, rank: 0, age: 10));
    }

    private static void Check(GuildEventEntry entry, byte kind, WowGuid128 player, WowGuid128 other, byte rank, uint age)
    {
        Assert.Equal(kind, entry.TransactionType);
        Assert.Equal(player, entry.PlayerGUID);
        Assert.Equal(other, entry.OtherGUID);
        Assert.Equal(rank, entry.RankID);
        Assert.Equal(age, entry.TransactionDate);
    }
}
