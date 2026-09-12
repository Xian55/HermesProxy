using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using V343 = HermesProxy.World.Enums.V3_4_3_54261;

namespace HermesProxy.Tests.World.Server;

public class RaidDifficultyTests
{
    [Fact]
    public void V343_RaidDifficultyOpcodes_MatchWpp51666AndLineagedr()
    {
        // Live 3.4.3.54261 click is wire 14051. Native lineagedr / WPP V3_4_3_51666
        // both list CMSG_SET_RAID_DIFFICULTY = 0x36E3, SMSG_RAID_DIFFICULTY_SET = 0x27AD.
        Assert.Equal(14051u, (uint)V343.Opcode.CMSG_SET_RAID_DIFFICULTY);
        Assert.Equal(10157u, (uint)V343.Opcode.SMSG_RAID_DIFFICULTY_SET);
        Assert.Equal((uint)V343.Opcode.CMSG_SET_DUNGEON_DIFFICULTY, 13956u);
    }

    [Theory]
    [InlineData(0, DifficultyModern.Raid10N, DifficultyModern.RaidClassic10N)]
    [InlineData(1, DifficultyModern.Raid25N, DifficultyModern.RaidClassic25N)]
    [InlineData(2, DifficultyModern.Raid10HC, DifficultyModern.RaidClassic10HC)]
    [InlineData(3, DifficultyModern.Raid25HC, DifficultyModern.RaidClassic25HC)]
    public void Maps335aRaidModesToBoth343Slots(byte ac, DifficultyModern legacyId, DifficultyModern classicId)
    {
        Assert.Equal(legacyId, RaidDifficulties.ToLegacyId(ac));
        Assert.Equal(classicId, RaidDifficulties.ToClassicId(ac));
        Assert.Equal(ac, RaidDifficulties.ToLegacy((int)legacyId));
        Assert.Equal(ac, RaidDifficulties.ToLegacy((int)classicId));
    }

    [Fact]
    public void ToLegacy_UnknownModern_DefaultsTo10Normal()
    {
        Assert.Equal(0u, RaidDifficulties.ToLegacy((int)DifficultyModern.Normal));
        Assert.Equal(DifficultyModern.Raid10N, RaidDifficulties.ToLegacyId(99));
    }

    [Fact]
    public void SetRaidDifficulty_Read_Int32ThenLegacyByte()
    {
        var payload = new WorldPacket(1u);
        payload.WriteInt32((int)DifficultyModern.Raid10N);
        payload.WriteUInt8(0);

        byte[] body = payload.GetData();
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);

        // Reads through the same accessor the dispatch site uses, not a hand-written offset.
        var reader = new global::Framework.IO.SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        SetRaidDifficultyCodec.Read(ref reader, out var packet);

        Assert.Equal((int)DifficultyModern.Raid10N, packet.DifficultyID);
        Assert.Equal(0, packet.Legacy);
    }

    [Fact]
    public void SetRaidDifficulty_Read_OmitsLegacyByteWhenAbsent()
    {
        // The trailing Legacy byte is optional — older clients stop after DifficultyID. This is
        // the conditional-read path, which is where a conversion to a positional record struct
        // goes wrong: the struct defaults to 0 where the class defaulted to 0, so the two agree
        // only as long as the codec actually honours the CanRead guard.
        var payload = new WorldPacket(1u);
        payload.WriteInt32((int)DifficultyModern.Raid25N);

        byte[] body = payload.GetData();
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);

        var reader = new global::Framework.IO.SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        SetRaidDifficultyCodec.Read(ref reader, out var packet);

        Assert.Equal((int)DifficultyModern.Raid25N, packet.DifficultyID);
        Assert.Equal(0, packet.Legacy);
    }

    [Fact]
    public void RaidDifficultySet_Write_Int32ThenLegacyByte()
    {
        var packet = new RaidDifficultySet
        {
            DifficultyID = (int)DifficultyModern.Raid10N,
            Legacy = 0,
        };

        packet.WritePacketData();
        byte[] data = packet.GetData()!;

        Assert.Equal(5, data.Length);
        Assert.Equal(3, data[0]);
        Assert.Equal(0, data[1]);
        Assert.Equal(0, data[2]);
        Assert.Equal(0, data[3]);
        Assert.Equal(0, data[4]);
    }
}
