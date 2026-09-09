namespace HermesProxy.World.Enums;

/// <summary>
/// gameobject_template.type as the legacy (1.12 / 2.4.3 / 3.3.5a) server sends it in
/// SMSG_GAMEOBJECT_QUERY_RESPONSE and packs into GAMEOBJECT_BYTES_1 byte 1.
///
/// Only the members the proxy actually reasons about are listed — the ones that carry a
/// Lock.dbc id (see GameObjectStats.LegacyLockId). Modern builds kept these numbers, so
/// GameObjectTypeModern covers the handful that matter on the other side of the wire.
/// </summary>
public enum GameObjectTypeLegacy : uint
{
    Door = 0,
    Button = 1,
    QuestGiver = 2,
    Chest = 3,
    Trap = 6,
    Goober = 10,
    AreaDamage = 12,
    Camera = 13,
    FlagStand = 24,
    FishingHole = 25,
    FlagDrop = 26,
}
