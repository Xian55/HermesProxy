using Framework;
using HermesProxy.World.Enums;
using Framework.Logging;

namespace HermesProxy.World;

/// <summary>
/// 128-bit GUID used in modern WoW versions (7.0.3+)
/// </summary>
public readonly record struct WowGuid128(ulong Low, ulong High)
{
    private const ulong UNKNOWN_TMP_GUID_START = 10_000_000_000;
    private static ulong _nextUnknownTmpGuid = UNKNOWN_TMP_GUID_START;

    public static WowGuid128 Empty => default;

    #region Static Factory Methods

    public static WowGuid128 Create(WowGuid64 guid, GameSessionData gamestate) => guid.GetHighType() switch
    {
        HighGuidType.Player => Create(HighGuidType703.Player, guid.GetCounter()),
        HighGuidType.Item => Create(HighGuidType703.Item, guid.GetCounter()),
        HighGuidType.Transport or HighGuidType.MOTransport => TransportCreate(guid.GetCounter(), guid.GetEntry()),
        HighGuidType.RaidGroup => Create(HighGuidType703.RaidGroup, guid.GetCounter()),
        HighGuidType.GameObject => Create(HighGuidType703.GameObject, gamestate.GetObjectSpawnCounter(guid), guid.GetEntry(), guid.GetCounter()),
        HighGuidType.Creature => Create(HighGuidType703.Creature, gamestate.GetObjectSpawnCounter(guid), guid.GetEntry(), guid.GetCounter()),
        HighGuidType.Pet => Create(HighGuidType703.Pet, 0, guid.GetEntry(), guid.GetCounter()),
        HighGuidType.Vehicle => Create(HighGuidType703.Vehicle, 0, guid.GetEntry(), guid.GetCounter()),
        HighGuidType.DynamicObject => Create(HighGuidType703.DynamicObject, 0, guid.GetEntry(), guid.GetCounter()),
        HighGuidType.Corpse => Create(HighGuidType703.Corpse, 0, guid.GetEntry(), guid.GetCounter()),
        _ => WowGuid128.Empty,
    };
    public static WowGuid128 Create(HighGuidType703 type, ulong counter)
    {
        switch (type)
        {
            case HighGuidType703.Uniq:
            case HighGuidType703.Party:
            case HighGuidType703.WowAccount:
            case HighGuidType703.BNetAccount:
            case HighGuidType703.GMTask:
            case HighGuidType703.RaidGroup:
            case HighGuidType703.Spell:
            case HighGuidType703.Mail:
            case HighGuidType703.UserRouter:
            case HighGuidType703.PVPQueueGroup:
            case HighGuidType703.UserClient:
            case HighGuidType703.UniqUserClient:
            case HighGuidType703.BattlePet:
            case HighGuidType703.CommerceObj:
            case HighGuidType703.ClientSession:
            case HighGuidType703.ArenaTeam:
                return GlobalCreate(type, counter);
            case HighGuidType703.Player:
            case HighGuidType703.Item:   // This is not exactly correct, there are 2 more unknown parts in highguid: (high >> 10 & 0xFF), (high >> 18 & 0xFFFFFF)
            case HighGuidType703.Guild:
            case HighGuidType703.Transport:
                return RealmSpecificCreate(type, counter);
            default:
                Log.Print(LogType.Error, $"This guid type cannot be constructed using Create(HighGuid: {type} ulong counter).");
                break;
        }
        return WowGuid128.Empty;
    }

    public static WowGuid128 Create(HighGuidType703 type, uint mapId, uint entry, ulong counter)
    {
        return MapSpecificCreate(type, 0, (ushort)mapId, 0, entry, counter);
    }

    public static WowGuid128 Create(HighGuidType703 type, World.Enums.SpellCastSource subType, uint mapId, uint entry, ulong counter)
    {
        return MapSpecificCreate(type, (byte)subType, (ushort)mapId, 0, entry, counter);
    }

    public static WowGuid128 CreateLootGuid(HighGuidTypeLegacy type, uint entry, ulong counter)
    {
        return MapSpecificCreate(HighGuidType703.LootObject, 0, 0, (uint)type, entry, counter);
    }

    public static WowGuid128 CreateUnknownPlayerGuid()
    {
        return Create(HighGuidType703.Player, _nextUnknownTmpGuid++);
    }

    public static bool IsUnknownPlayerGuid(WowGuid128 playerGuid)
    {
        return playerGuid.IsPlayer() && playerGuid.GetCounter() >= UNKNOWN_TMP_GUID_START;
    }

    static WowGuid128 GlobalCreate(HighGuidType703 type, ulong counter)
    {
        return new WowGuid128(counter, (ulong)type << 58);
    }

    static WowGuid128 TransportCreate(ulong counter, uint entry)
    {
        return new WowGuid128(0, (ulong)HighGuidType703.Transport << 58 | (counter << 38) | entry);
    }

    static WowGuid128 RealmSpecificCreate(HighGuidType703 type, ulong counter)
    {
        if (type == HighGuidType703.Transport)
            return new WowGuid128(0, (ulong)type << 58 | (counter << 38));
        else
            return new WowGuid128(counter, (ulong)type << 58 | (ulong)1 /*realmId*/ << 42);
    }

    static WowGuid128 MapSpecificCreate(HighGuidType703 type, byte subType, ushort mapId, uint serverId, uint entry, ulong counter)
    {
        return new WowGuid128((((ulong)(serverId & 0xFFFFFF) << 40) | (counter & 0xFFFFFFFFFF)),
            (((ulong)type << 58) | ((ulong)(1 /*realmId*/ & 0x1FFF) << 42) | ((ulong)(mapId & 0x1FFF) << 29) | ((ulong)(entry & 0x7FFFFF) << 6) | ((ulong)subType & 0x3F)));
    }

    #endregion

    #region Conversion Methods

    public static WowGuid64 ConvertUniqGuid(WowGuid128 guid) => (UniqGuid)guid.Low switch
    {
        UniqGuid.SpellTargetTradeItem => new WowGuid64((ulong)TradeSlots.NonTraded),
        _ => WowGuid64.Empty,
    };

    #endregion
}
