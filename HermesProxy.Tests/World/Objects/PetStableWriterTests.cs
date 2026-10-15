using System;
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
/// 4.4.2's ActivePlayerData.PetStable as the client reads it (WowPacketParser's 4.4.2 reader, made
/// from the client): StableInfo starts on a byte of its own after the presence bit, and each pet
/// carries a byte after its flags. The expected bytes are written out from that reader's order.
/// </summary>
public sealed class PetStableWriterTests
{
    private static readonly WowGuid128 Player = new(4, 0x0800040000000000);

    [Fact]
    public void Update_PresenceBitThenTheStableOnAByteOfItsOwn()
    {
        var (builder, src, _) = Make(Fangi());
        using var data = new WorldPacket(1u);
        var blocks = new global::Framework.Util.StackBitMask(stackalloc uint[45]);

        builder.WriteUpdateActivePlayerPetStableBit(data, ref blocks, src);
        builder.WriteUpdateActivePlayerPetStable(data, src);

        string expected =
            "80"                    // HasPetStable, then a flush
            + "E000000030"          // mask 0b111, 32-bit pet count 1, one bit of its update mask
            + "FF80"                // the pet's nine changed bits
            + "00000000" + "06000000" + "DDA60000" + "87030000" + "0A000000"
            + "01" + "00"           // PetFlags (active), the byte after them
            + "05" + "46616E6769"   // "Fangi"
            + "0000";               // no stable master
        Assert.Equal(expected, Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Update_WithoutAStable_SaysSoInTheGroupsBit()
    {
        var (builder, src, _) = Make(null);
        using var data = new WorldPacket(1u);
        var blocks = new global::Framework.Util.StackBitMask(stackalloc uint[45]);

        builder.WriteUpdateActivePlayerPetStableBit(data, ref blocks, src);

        Assert.Equal("00", Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Create_TailBitsThenTheStable()
    {
        var (builder, src, _) = Make(Fangi());
        using var data = new WorldPacket(1u);

        builder.WriteCreateActivePlayerTailBits(data, src);
        builder.WriteCreateActivePlayerPetStable(data, src);

        string expected =
            "20"                    // SortBags, InsertItems off; PetStable there; no bank tabs
            + "01000000" + "0000"   // one pet, no stable master
            + "00000000" + "06000000" + "DDA60000" + "87030000" + "0A000000"
            + "01" + "00" + "05" + "46616E6769";
        Assert.Equal(expected, Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Create_WithoutAStable_WritesTheOldTail()
    {
        var (builder, src, _) = Make(null);
        using var data = new WorldPacket(1u);

        builder.WriteCreateActivePlayerTailBits(data, src);
        builder.WriteCreateActivePlayerPetStable(data, src);

        Assert.Equal("00", Convert.ToHexString(Bytes(data)));
    }

    private static PetStableData Fangi()
    {
        var stable = new PetStableData();
        stable.Pets.Add(new PetStableInfo
        {
            PetSlot = 0,
            PetNumber = 6,
            CreatureID = 42717,
            DisplayID = 903,
            ExperienceLevel = 10,
            PetFlags = 1,
            PetName = "Fangi",
        });
        return stable;
    }

    private static (ObjectUpdateBuilder Builder, ActivePlayerData Src, GameSessionData Session) Make(PetStableData? stable)
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new System.Collections.Generic.Dictionary<WowGuid128, ObjectType>());
        session.CurrentPlayerGuid = Player;
        session.PetStable = stable;

        var globalSession = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        var update = new ObjectUpdate(Player, UpdateTypeModern.Values, globalSession);
        update.ActivePlayerData ??= new ActivePlayerData();
        update.ActivePlayerData.PetStable = stable;
        return (new ObjectUpdateBuilder(update, session), update.ActivePlayerData, session);
    }

    private static byte[] Bytes(WorldPacket data)
    {
        data.FlushBits();
        return data.GetDataSpan().ToArray();
    }
}
