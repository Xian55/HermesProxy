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
/// 4.4.2's ActivePlayerData.Transmog (bit 12), the appearances the client counts as collected: a bit
/// per ItemModifiedAppearance in uint32 blocks. A 4.3.4 server keeps no collection, so every item's
/// tooltip said its appearance was not collected; the proxy sets the carried items' bits. An update
/// carries only the changed blocks, behind TrinityCore cata_classic's dynamic-field mask.
/// </summary>
public sealed class TransmogWriterTests
{
    private static readonly WowGuid128 Player = new(3, 0x0800040000000000);

    [Fact]
    public void Update_OneChangedBlock_MaskThenThatBlock()
    {
        var (builder, session) = Make();
        session.SetTransmogBit(32 + 4, true);       // block 1, bit 4
        session.SetTransmogBit(64 + 2, true);       // block 2, which grows the field to 3
        session.ClearTransmogChanges();
        session.SetTransmogBit(32 + 5, true);       // only block 1 changes now
        using var data = new WorldPacket(1u);

        builder.WriteUpdateActivePlayerData(data);

        Assert.Equal(
            "01000000"              // blocks: block 0
            + "000000004004"        // 14 bits of blocks 32-45, block 0 = bits 0 and 12, flushed
            + "00000003" + "40"     // Transmog: size 3, changed bits 0b010, flushed
            + "30000000",           // block 1: bits 4 and 5
            Convert.ToHexString(Bytes(data)));
        Assert.False(session.TransmogDirty);
        Assert.Empty(session.TransmogBlocksChanged);
    }

    [Fact]
    public void Update_MoreThan32Blocks_WholeMaskBlocksAsUInt32()
    {
        // After the 32-bit size the bits end on a byte, where TrinityCore appends each whole block
        // of the mask as a little-endian uint32 rather than as 32 bits.
        var (builder, session) = Make();
        session.SetTransmogBit(39 * 32, true);      // 40 blocks
        session.ClearTransmogChanges();
        session.SetTransmogBit(32 + 0, true);       // block 1
        session.SetTransmogBit(33 * 32 + 1, true);  // block 33
        using var data = new WorldPacket(1u);

        builder.WriteUpdateActivePlayerData(data);

        Assert.Equal(
            "01000000" + "000000004004"
            + "00000028"            // size 40
            + "02000000"            // blocks 0-31 of the mask: block 1
            + "02"                  // the last 8: block 33
            + "01000000" + "02000000",
            Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Update_NothingChanged_NoTransmog()
    {
        var (builder, session) = Make();
        session.SetTransmogBit(5, true);
        session.ClearTransmogChanges();
        using var data = new WorldPacket(1u);

        builder.WriteUpdateActivePlayerData(data);

        Assert.Equal("00000000" + "0000", Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void SetTransmogBit_ClearingABitIsAChange_ClearingAnUnsetOneIsNot()
    {
        var (_, session) = Make();
        Assert.True(session.SetTransmogBit(70, true));
        session.ClearTransmogChanges();

        Assert.False(session.SetTransmogBit(70, true));
        Assert.False(session.SetTransmogBit(500, false));
        Assert.Empty(session.TransmogBlocksChanged);

        Assert.True(session.SetTransmogBit(70, false));
        Assert.Equal([0u, 0u, 0u], session.TransmogBlocks);
        Assert.Equal([0b100u], session.TransmogBlocksChanged);
    }

    private static (ObjectUpdateBuilder Builder, GameSessionData Session) Make()
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new Dictionary<WowGuid128, ObjectType>());
        typeof(GameSessionData).GetField(nameof(GameSessionData.TransmogBlocks))!.SetValue(session, new List<uint>());
        typeof(GameSessionData).GetField(nameof(GameSessionData.TransmogBlocksChanged))!.SetValue(session, new List<uint>());
        session.CurrentPlayerGuid = Player;

        var globalSession = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        var update = new ObjectUpdate(Player, UpdateTypeModern.Values, globalSession);
        update.ActivePlayerData ??= new ActivePlayerData();
        return (new ObjectUpdateBuilder(update, session), session);
    }

    private static byte[] Bytes(WorldPacket data)
    {
        data.FlushBits();
        return data.GetDataSpan().ToArray();
    }
}
