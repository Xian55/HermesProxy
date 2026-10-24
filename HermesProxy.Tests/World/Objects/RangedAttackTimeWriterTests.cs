using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using ObjectUpdateBuilder343 = HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder;
using ObjectUpdateBuilder442 = HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder;

namespace HermesProxy.Tests.World.Objects;

/// <summary>
/// UnitData.RangedAttackRoundBaseTime, the ranged attack speed the client shows: update bit 45 on
/// 3.4.3 and 46 on 4.4.2, between AuraState and BoundingRadius (4.4.2 has Flags4 before it). The
/// proxy wrote it into the create only, so a hasted or slowed ranged attack time never reached the
/// client and UnitRangedDamage() kept the speed from the login.
/// </summary>
public sealed class RangedAttackTimeWriterTests
{
    private static readonly WowGuid128 Player = new(3, 0x0800040000000000);

    [Fact]
    public void Update343_RangedAttackTime_IsBit45()
    {
        var (session, update) = Make();
        update.UnitData.RangedAttackRoundBaseTime = 1465;
        using var data = new WorldPacket(1u);

        new ObjectUpdateBuilder343(update, session).WriteUpdateUnitData(data);

        Assert.Equal(Expected(bit: 45), Convert.ToHexString(Bytes(data)));
    }

    [Fact]
    public void Update442_RangedAttackTime_IsBit46()
    {
        var (session, update) = Make();
        update.UnitData.RangedAttackRoundBaseTime = 1465;
        using var data = new WorldPacket(1u);

        new ObjectUpdateBuilder442(update, session).WriteUpdateUnitData(data);

        Assert.Equal(Expected(bit: 46), Convert.ToHexString(Bytes(data)));
    }

    // Block 1 only, its word most significant bit first: the group bit (32) and the field's, then
    // 1465 ms.
    private static string Expected(int bit)
        => "02" + (1u | 1u << (bit - 32)).ToString("X8") + "B9050000";

    private static (GameSessionData Session, ObjectUpdate Update) Make()
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new Dictionary<WowGuid128, ObjectType>());
        session.CurrentPlayerGuid = Player;

        var globalSession = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        return (session, new ObjectUpdate(Player, UpdateTypeModern.Values, globalSession));
    }

    private static byte[] Bytes(WorldPacket data)
    {
        data.FlushBits();
        return data.GetDataSpan().ToArray();
    }
}
