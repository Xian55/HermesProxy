using HermesProxy.World.Enums;

namespace HermesProxy.World;

/// <summary>
/// 64-bit GUID used in legacy WoW versions (pre-7.0.3).
///
/// <para>
/// Up to 3.3.5a the high type is the top 16 bits, the entry bits 24-47 and the counter the low 24
/// bits (32 without an entry). From 4.0 the counter is always the low 32 bits and the entry moves
/// up to bits 32-51, under a high type cut to the top 12 bits; corpses and area triggers keep 16
/// (TrinityCore 4.3.4 ObjectGuid). A 12-bit high shifted by 52 lands where its 16-bit
/// <see cref="HighGuidTypeLegacy"/> value shifted by 48 does, so the high part is built the same
/// either way.
/// </para>
/// </summary>
public readonly record struct WowGuid64(ulong Low)
{
    public static WowGuid64 Empty => default;

    /// <summary>The 4.x layout: 32-bit counter, entry at bit 32, 12-bit high type.</summary>
    internal static bool IsCataLayout => LegacyVersion.ExpansionVersion >= 4;

    public WowGuid64(HighGuidTypeLegacy hi, uint counter) : this(
        counter != 0 ? (ulong)counter | ((ulong)hi << 48) : 0)
    {
    }

    public WowGuid64(HighGuidTypeLegacy hi, uint entry, uint counter) : this(
        counter != 0 ? (ulong)counter | ((ulong)entry << (IsCataLayout ? 32 : 24)) | ((ulong)hi << 48) : 0)
    {
    }

    #region Static Factory Methods

    public static WowGuid64 Create(WowGuid128 guid) => guid.GetHighType() switch
    {
        HighGuidType.Uniq => WowGuid128.ConvertUniqGuid(guid),
        HighGuidType.Player => new WowGuid64(HighGuidTypeLegacy.Player, (uint)guid.GetCounter()),
        // Not hard-coded to 0x4000: cMaNGOS WotLK uses 0x4700 for items, and an item guid sent
        // back under the wrong high finds nothing on that backend (#278). See LegacyItemGuidHigh.
        HighGuidType.Item => new WowGuid64(LegacyItemGuidHigh.Current, (uint)guid.GetCounter()),
        HighGuidType.Transport => (guid.GetCounter() & WowGuid128.MoTransportCounterFlag) != 0
                            ? new WowGuid64(HighGuidTypeLegacy.MOTransport, (uint)(guid.GetCounter() & ~WowGuid128.MoTransportCounterFlag))
                            : new WowGuid64(HighGuidTypeLegacy.Transport, guid.GetEntry(), (uint)guid.GetCounter()),
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

        uint high = (uint)(Low >> 48) & 0x0000FFFF;
        if (IsCataLayout && high != (uint)HighGuidTypeLegacy.Corpse && high != AreaTriggerHighCata)
            high &= 0xFFF0;                 // the low nibble is the entry's top bits

        return (HighGuidTypeLegacy)high;
    }

    private const uint AreaTriggerHighCata = 0xF102;
}
