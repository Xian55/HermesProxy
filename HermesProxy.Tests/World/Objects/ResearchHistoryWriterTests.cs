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
/// 4.4.2's ActivePlayerData.ResearchHistory (archaeology's completed projects) as the client reads it
/// (WowPacketParser's 4.4.2 reader, made from the client): after group 102's PetStable presence bit,
/// on a byte of its own. Written into the presence bit's byte, the client read a mask of 0 and showed
/// no completed project.
/// </summary>
public sealed class ResearchHistoryWriterTests
{
    private static readonly WowGuid128 Player = new(3, 0x0800040000000000);

    [Fact]
    public void Update_AfterThePresenceBit_OnAByteOfItsOwn()
    {
        var (builder, src) = Make([(201u, 1792033349L, 2u)]);
        using var data = new WorldPacket(1u);
        var blocks = new global::Framework.Util.StackBitMask(stackalloc uint[45]);
        blocks.SetBit(102);
        blocks.SetBit(122);

        builder.WriteUpdateActivePlayerPetStableBit(data, ref blocks, src);

        string expected =
            "00"                    // HasPetStable 0, then a flush
            + "C000000060"          // mask 0b11, 32-bit project count 1, one bit of its update mask
            + "F0"                  // the project's four changed bits
            + "C9000000" + "4542D06A00000000" + "02000000";
        Assert.Equal(expected, Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Update_WithoutBit122_WritesOnlyThePresenceBit()
    {
        var (builder, src) = Make([(201u, 1792033349L, 2u)]);
        using var data = new WorldPacket(1u);
        var blocks = new global::Framework.Util.StackBitMask(stackalloc uint[45]);
        blocks.SetBit(102);

        builder.WriteUpdateActivePlayerPetStableBit(data, ref blocks, src);

        Assert.Equal("00", Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Create_CountThenEachProject()
    {
        var (builder, src) = Make([(201u, 1792033349L, 2u), (305u, 0L, 1u)]);
        using var data = new WorldPacket(1u);

        builder.WriteCreateActivePlayerResearchHistory(data, src);

        string expected = "02000000"
            + "C9000000" + "4542D06A00000000" + "02000000"
            + "31010000" + "0000000000000000" + "01000000";
        Assert.Equal(expected, Convert.ToHexString(Bytes(data)));
    }

    private static (ObjectUpdateBuilder Builder, ActivePlayerData Src) Make(List<(uint, long, uint)> history)
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new Dictionary<WowGuid128, ObjectType>());
        session.CurrentPlayerGuid = Player;
        session.ResearchHistory = history;

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
