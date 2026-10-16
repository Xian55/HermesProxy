using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;
using ObjectUpdateBuilder = HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder;

namespace HermesProxy.Tests.World.Objects;

/// <summary>
/// 4.4.2's ActivePlayerData.SelfResSpells (bit 14), what the death popup offers instead of
/// releasing: a 4.3.4 server's PLAYER_SELF_RES_SPELL. TrinityCore cata_classic writes the masks of
/// bits 6-21 in a bit section of their own, after the research masks and a flush; written straight
/// after the blocks mask they came a byte early and the client read the spell a byte late.
/// </summary>
public sealed class SelfResSpellsWriterTests
{
    private static readonly WowGuid128 Player = new(3, 0x0800040000000000);

    [Fact]
    public void Update_Soulstone_MasksInTheirOwnSectionThenTheSpell()
    {
        var (builder, src) = Make();
        src.SelfResSpells = [3026];
        using var data = new WorldPacket(1u);

        builder.WriteUpdateActivePlayerData(data);

        Assert.Equal(
            "01000000"              // blocks: block 0
            + "000000010004"        // 14 bits of blocks 32-45, block 0 = bits 0 and 14, flushed
            + "0000000180"          // SelfResSpells: size 1 and its changed bit, flushed
            + "D20B0000",           // Use Soulstone (3026)
            Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Update_Resurrected_AnEmptyList()
    {
        // The server clears the field to 0 on resurrection; the offer must go away with it.
        var (builder, src) = Make();
        src.SelfResSpells = [0];
        using var data = new WorldPacket(1u);

        builder.WriteUpdateActivePlayerData(data);

        Assert.Equal("01000000" + "000000010004" + "00000000", Convert.ToHexString(Bytes(data)));
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
