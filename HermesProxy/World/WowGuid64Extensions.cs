using HermesProxy.World.Enums;

namespace HermesProxy.World;

/// <summary>
/// Extension methods for WowGuid64
/// </summary>
public static class WowGuid64Extensions
{
    extension(WowGuid64 guid)
    {
        public bool IsEmpty() => guid == default;

        public ulong GetLowValue() => guid.Low;
        public ulong GetHighValue() => 0;

        public HighGuid GetHighGuid() => new HighGuidLegacy(guid.GetHighGuidTypeLegacy());

        public HighGuidType GetHighType() => guid.GetHighGuid().GetHighGuidType();

        public ulong GetCounter() => guid.HasEntry()
                ? (uint)(guid.Low & 0x0000000000FFFFFFul)
                : (uint)(guid.Low & 0x00000000FFFFFFFFul);

        public uint GetEntry()
        {
            if (!guid.HasEntry())
                return 0;

            return (uint)((guid.Low >> 24) & 0x0000000000FFFFFFul);
        }

        public bool HasEntry() => guid.GetHighType() switch
        {
            HighGuidType.Item or HighGuidType.Player or HighGuidType.DynamicObject or HighGuidType.Corpse or HighGuidType.MOTransport or HighGuidType.RaidGroup => false,
            _ => true,
        };

        public ObjectType GetObjectType() => guid.GetHighType() switch
        {
            HighGuidType.Player => ObjectType.Player,
            HighGuidType.DynamicObject => ObjectType.DynamicObject,
            HighGuidType.Corpse => ObjectType.Corpse,
            HighGuidType.Item => ObjectType.Item,
            HighGuidType.GameObject or HighGuidType.Transport or HighGuidType.MOTransport => ObjectType.GameObject,
            HighGuidType.Vehicle or HighGuidType.Creature or HighGuidType.Pet => ObjectType.Unit,
            HighGuidType.AreaTrigger => ObjectType.AreaTrigger,
            _ => ObjectType.Object,
        };

        public bool IsWorldObject() => guid.GetHighType() switch
        {
            HighGuidType.Player or HighGuidType.Transport or HighGuidType.MOTransport or HighGuidType.Creature or HighGuidType.Vehicle or HighGuidType.Pet or HighGuidType.GameObject or HighGuidType.DynamicObject or HighGuidType.Corpse => true,
            _ => false,
        };

        public bool IsTransport() => guid.GetHighType() switch
        {
            HighGuidType.Transport or HighGuidType.MOTransport => true,
            _ => false,
        };

        public bool IsPlayer() => guid.GetObjectType() switch
        {
            ObjectType.Player or ObjectType.ActivePlayer => true,
            _ => false,
        };

        public bool IsCreature() => guid.GetObjectType() == ObjectType.Unit;

        public bool IsItem() => guid.GetObjectType() switch
        {
            ObjectType.Item or ObjectType.Container => true,
            _ => false,
        };

        public WowGuid64 To64() => guid;

        public WowGuid128 To128(GameSessionData gameState) => WowGuid128.Create(guid, gameState);

        public WowGuid128 ToLootGuid() => WowGuid128.CreateLootGuid(guid.GetHighGuidTypeLegacy(), guid.GetEntry(), guid.GetCounter());

        public string ToString()
        {
            if (guid.Low == 0)
                return "0x0";

            if (guid.HasEntry())
            {
                return "Full: 0x" + guid.Low.ToString("X8") + " Type: " + guid.GetHighType()
                    + " Entry: " + guid.GetEntry() + " Low: " + guid.GetCounter();
            }

            return "Full: 0x" + guid.Low.ToString("X8") + " Type: " + guid.GetHighType()
                + " Low: " + guid.GetCounter();
        }
    }
}
