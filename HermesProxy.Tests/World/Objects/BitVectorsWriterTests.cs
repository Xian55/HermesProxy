using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;
using ObjectUpdateBuilder = HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder;

namespace HermesProxy.Tests.World.Objects;

/// <summary>
/// 4.4.2's ActivePlayerData.BitVectors as TrinityCore cata_classic writes them and WowPacketParser's
/// 4.4.2 reader reads them: the explored zones are the second vector, the completed quests the
/// tenth. The client reads the world map's fog and IsQuestFlaggedCompleted from them, so while they
/// went empty the map stayed unexplored and every quest undone.
/// </summary>
public sealed class BitVectorsWriterTests
{
    private static readonly WowGuid128 Player = new(3, 0x0800040000000000);

    [Fact]
    public void Create_ThirteenVectors_ExploredZonesSecond_QuestsTenth()
    {
        var (builder, src) = Make();
        src.EnsureExploredZones()[0] = 0x11ul;
        src.EnsureQuestCompleted()[4] = 1ul << 49;
        using var data = new WorldPacket(1u);

        builder.WriteCreateActivePlayerBitVectors(data, src);

        byte[] bytes = Bytes(data);
        int offset = 4;                                                         // vector 0: empty
        Assert.Equal(0u, BitConverter.ToUInt32(bytes, 0));
        Assert.Equal((uint)ActivePlayerData.ExploredZoneWords, BitConverter.ToUInt32(bytes, offset));
        Assert.Equal(0x11ul, BitConverter.ToUInt64(bytes, offset + 4));
        offset += 4 + ActivePlayerData.ExploredZoneWords * 8;
        for (int vector = 2; vector < 9; vector++, offset += 4)
            Assert.Equal(0u, BitConverter.ToUInt32(bytes, offset));
        Assert.Equal((uint)ActivePlayerData.QuestCompletedWords, BitConverter.ToUInt32(bytes, offset));
        Assert.Equal(1ul << 49, BitConverter.ToUInt64(bytes, offset + 4 + 4 * 8));
        offset += 4 + ActivePlayerData.QuestCompletedWords * 8;
        // vectors 10-12, then the two element lists
        Assert.Equal(5 * 4, bytes.Length - offset);
        Assert.All(bytes[offset..], b => Assert.Equal(0, b));
    }

    [Fact]
    public void Create_NothingKnown_ThirteenEmptyVectors()
    {
        var (builder, src) = Make();
        using var data = new WorldPacket(1u);

        builder.WriteCreateActivePlayerBitVectors(data, src);

        Assert.Equal(new byte[15 * 4], Bytes(data));
    }

    [Fact]
    public void Update_OneQuestWord_TheVectorsMaskThenTheWord()
    {
        var (builder, src) = Make();
        src.EnsureQuestCompleted()[4] = 1ul << 49;
        using var data = new WorldPacket(1u);
        var blocks = new global::Framework.Util.StackBitMask(stackalloc uint[45]);
        builder.ApplyActivePlayerBitVectorsMaskMutator(ref blocks, src);
        Assert.True(blocks.IsBitSet(38));
        Assert.True(blocks.IsBitSet(63));

        builder.WriteUpdateActivePlayerBitVectors(data, ref blocks, src);

        string expected =
            "8000020080"            // one block of changes: the group and vector 9 (bits 0 and 10)
            + "C00000DAC0"          // vector mask 0b11, size 875 in 32 bits, the word mask begins
            + "00000400"            // word 4's bit
            + new string('0', 210)  // the rest of the 875 bits, flushed
            + "0000000000000200";   // word 4
        Assert.Equal(expected, Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Update_NothingInTheBitVectors_NoBits()
    {
        var (builder, src) = Make();
        var blocks = new global::Framework.Util.StackBitMask(stackalloc uint[45]);

        builder.ApplyActivePlayerBitVectorsMaskMutator(ref blocks, src);

        Assert.False(blocks.IsBitSet(63));
        Assert.False(ObjectUpdateBuilder.HasAnyBitVectorWord(src));
    }

    private static (ObjectUpdateBuilder Builder, ActivePlayerData Src) Make()
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new Dictionary<WowGuid128, ObjectType>());
        session.CurrentPlayerGuid = Player;

        var globalSession = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        var update = new ObjectUpdate(Player, UpdateTypeModern.Values, globalSession);
        update.ActivePlayerData ??= new ActivePlayerData();
        return (new ObjectUpdateBuilder(update, session), update.ActivePlayerData);
    }

    private static byte[] Bytes(WorldPacket data)
    {
        data.FlushBits();
        return data.GetDataSpan().ToArray();
    }
}
