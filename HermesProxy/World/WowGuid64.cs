using HermesProxy.World.Enums;

namespace HermesProxy.World;

/// <summary>
/// 64-bit GUID used in legacy WoW versions (pre-7.0.3)
/// </summary>
public readonly record struct WowGuid64(ulong Low)
{
    public static WowGuid64 Empty => default;

    public WowGuid64(HighGuidTypeLegacy hi, uint counter) : this(
        counter != 0 ? (ulong)counter | ((ulong)hi << 48) : 0)
    {
    }

    public WowGuid64(HighGuidTypeLegacy hi, uint entry, uint counter) : this(
        counter != 0 ? (ulong)counter | ((ulong)entry << 24) | ((ulong)hi << 48) : 0)
    {
    }

    #region Static Factory Methods

    public static WowGuid64 Create(WowGuid128 guid) => guid.GetHighType() switch
    {
        HighGuidType.Uniq => WowGuid128.ConvertUniqGuid(guid),
        HighGuidType.Player => new WowGuid64(HighGuidTypeLegacy.Player, (uint)guid.GetCounter()),
        HighGuidType.Item => new WowGuid64(HighGuidTypeLegacy.Item, (uint)guid.GetCounter()),
        HighGuidType.Transport => guid.GetEntry() != 0
                            ? new WowGuid64(HighGuidTypeLegacy.Transport, guid.GetEntry(), (uint)guid.GetCounter())
                            : new WowGuid64(HighGuidTypeLegacy.MOTransport, (uint)guid.GetCounter()),
        HighGuidType.RaidGroup => new WowGuid64(HighGuidTypeLegacy.Group, (uint)guid.GetCounter()),
        HighGuidType.GameObject => new WowGuid64(HighGuidTypeLegacy.GameObject, guid.GetEntry(), (uint)guid.GetCounter()),
        HighGuidType.Creature => new WowGuid64(HighGuidTypeLegacy.Creature, guid.GetEntry(), (uint)guid.GetCounter()),
        HighGuidType.Pet => new WowGuid64(HighGuidTypeLegacy.Pet, guid.GetEntry(), (uint)guid.GetCounter()),
        HighGuidType.Vehicle => new WowGuid64(HighGuidTypeLegacy.Vehicle, guid.GetEntry(), (uint)guid.GetCounter()),
        HighGuidType.DynamicObject => new WowGuid64(HighGuidTypeLegacy.DynamicObject, guid.GetEntry(), (uint)guid.GetCounter()),
        HighGuidType.Corpse => new WowGuid64(HighGuidTypeLegacy.Corpse, guid.GetEntry(), (uint)guid.GetCounter()),
        HighGuidType.LootObject => new WowGuid64((HighGuidTypeLegacy)guid.GetServerId(), guid.GetEntry(), (uint)guid.GetCounter()),
        _ => WowGuid64.Empty,
    };

    #endregion

    public HighGuidTypeLegacy GetHighGuidTypeLegacy()
    {
        if (Low == 0)
            return HighGuidTypeLegacy.None;

        return (HighGuidTypeLegacy)((Low >> 48) & 0x0000FFFF);
    }
}
