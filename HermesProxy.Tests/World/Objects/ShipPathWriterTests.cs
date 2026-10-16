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
/// A boat or zeppelin (GAMEOBJECT_TYPE_MAP_OBJ_TRANSPORT) as the 4.4.2 client needs it created: the
/// GameObject block with a ship path (TrinityCore cata_classic BuildMovementUpdate, WowPacketParser's
/// 4.4.2 reader, and a native Cataclysm Classic create of The Bravery all have it). Without the ship
/// path the client drew the boat where the create left it and it never moved, so it never docked.
/// </summary>
public sealed class ShipPathWriterTests
{
    [Fact]
    public void Ship_CreateCarriesTheGameObjectBlockWithAShipPath()
    {
        byte[] ship = Movement(typeId: 15);
        byte[] chest = Movement(typeId: 3);

        // the ship's block is the chest's plus WorldEffectID, three bits, then the ship path
        Assert.Equal(chest.Length + 4 + 1 + 4 + 4 + 1, ship.Length);
        Assert.Equal(
            "00000000"              // WorldEffectID
            + "40"                  // no int, a ship path, no transport state
            + "00000000"            // time offset: the ServerTime sent is the ship's own timer
            + "00000000"            // no next stop
            + "00",                 // not stopping, not stopped
            Convert.ToHexString(ship.AsSpan(ship.Length - 14)));
    }

    private static byte[] Movement(sbyte typeId)
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new Dictionary<WowGuid128, ObjectType>());
        var globalSession = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

        var guid = new WowGuid128(0, 0x1800040000000000);
        var update = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, globalSession);
        update.CreateData.ObjectType = ObjectType.GameObject;
        update.CreateData.MoveInfo = new MovementInfo();
        update.CreateData.TransportPathTimer = 435960925;
        update.GameObjectData ??= new GameObjectData();
        update.GameObjectData.TypeID = typeId;

        var builder = new ObjectUpdateBuilder(update, session);
        builder.SetCreateObjectBits();
        using var data = new WorldPacket(1u);
        builder.BuildMovementUpdate(data);
        data.FlushBits();
        return data.GetDataSpan().ToArray();
    }
}
