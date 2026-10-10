using System;
using System.Buffers;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_QUEST_GIVER_STATUS and its _MULTIPLE twin went from one writer with a 3.4.3 branch to one
/// layout per build. 1.14/2.5 and 3.4.3 must keep the old bytes; 4.4.2 renumbered the status enum,
/// and its available / in-progress values must be the ones a native TrinityCore cata_classic server
/// sends (refs/native-captures/tc_cata_442_*.pkt: 0x400000 and 0x2000).
/// </summary>
public sealed class QuestGiverStatusLayoutTests
{
    private static readonly WowGuid128 Giver = new(5457, 0x2000040000014E40);

    public static TheoryData<QuestGiverStatusModern> Statuses() =>
    [
        QuestGiverStatusModern.None, QuestGiverStatusModern.Unavailable, QuestGiverStatusModern.LowLevelAvailable,
        QuestGiverStatusModern.Incomplete, QuestGiverStatusModern.RewardRep, QuestGiverStatusModern.AvailableRep,
        QuestGiverStatusModern.Available, QuestGiverStatusModern.Reward2, QuestGiverStatusModern.Reward,
    ];

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.NotSame(QuestGiverStatusPkt.Layouts.For(ClientVersionBuild.V2_5_3_41750), QuestGiverStatusPkt.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.NotSame(QuestGiverStatusPkt.Layouts.For(ClientVersionBuild.V3_4_3_54261), QuestGiverStatusPkt.Layouts.For(ClientVersionBuild.V4_4_2_60895));
        Assert.NotSame(QuestGiverStatusMultiple.Layouts.For(ClientVersionBuild.V3_4_3_54261), QuestGiverStatusMultiple.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Theory]
    [MemberData(nameof(Statuses))]
    public void OlderLayouts_MatchTheOldBranches(QuestGiverStatusModern status)
    {
        var packet = new QuestGiverStatusPkt { QuestGiver = { Guid = Giver, Status = status } };

        using var retail = new WorldPacket(1u);
        retail.WritePackedGuid128(Giver);
        retail.WriteUInt32((uint)status);
        AssertBothSerialisers(QuestGiverStatusPkt.Layouts.For(ClientVersionBuild.V2_5_3_41750), packet, retail.GetDataSpan().ToArray());

        using var wotlk = new WorldPacket(1u);
        wotlk.WritePackedGuid128(Giver);
        wotlk.WriteUInt64(QuestGiverStatusV343Converter.FromModern(status));
        AssertBothSerialisers(QuestGiverStatusPkt.Layouts.For(ClientVersionBuild.V3_4_3_54261), packet, wotlk.GetDataSpan().ToArray());
    }

    [Theory]
    [InlineData(QuestGiverStatusModern.Available, 0x400000UL)]   // yellow !, as TrinityCore cata_classic sends it
    [InlineData(QuestGiverStatusModern.Incomplete, 0x2000UL)]    // quest taken, not done
    [InlineData(QuestGiverStatusModern.Reward, 0x400000000UL)]   // yellow ?
    [InlineData(QuestGiverStatusModern.None, 0UL)]
    public void CataLayout_WritesTheRenumberedStatus(QuestGiverStatusModern status, ulong wire)
    {
        var packet = new QuestGiverStatusPkt { QuestGiver = { Guid = Giver, Status = status } };

        using var expected = new WorldPacket(1u);
        expected.WritePackedGuid128(Giver);
        expected.WriteUInt64(wire);
        AssertBothSerialisers(QuestGiverStatusPkt.Layouts.For(ClientVersionBuild.V4_4_2_60895), packet, expected.GetDataSpan().ToArray());

        var multiple = new QuestGiverStatusMultiple();
        multiple.QuestGivers.Add(new QuestGiverInfo(Giver, status));
        multiple.QuestGivers.Add(new QuestGiverInfo(Giver, status));

        using var expectedMultiple = new WorldPacket(1u);
        expectedMultiple.WriteInt32(2);
        for (int i = 0; i < 2; i++)
        {
            expectedMultiple.WritePackedGuid128(Giver);
            expectedMultiple.WriteUInt64(wire);
        }
        AssertBothSerialisers(QuestGiverStatusMultiple.Layouts.For(ClientVersionBuild.V4_4_2_60895), multiple, expectedMultiple.GetDataSpan().ToArray());
    }

    private static void AssertBothSerialisers<T>(ServerPacketLayout<T> layout, T packet, byte[] expected) where T : ServerPacket, ISpanWritable
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        byte[] buffer = ArrayPool<byte>.Shared.Rent(packet.MaxSize);
        try
        {
            int written = layout.WriteToSpan(packet, buffer);
            Assert.Equal(expected, buffer.AsSpan(0, written).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
