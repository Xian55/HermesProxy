using HermesProxy.World.Enums;

namespace HermesProxy.World;

/// <summary>
/// Extension methods for WowGuid128
/// </summary>
public static class WowGuid128Extensions
{
    extension(WowGuid128 guid)
    {
        public bool IsEmpty() => guid == default;

        public ulong GetLowValue() => guid.Low;
        public ulong GetHighValue() => guid.High;

        public HighGuid GetHighGuid() => new HighGuid703((byte)((guid.High >> 58) & 0x3F));

        public HighGuidType GetHighType() => guid.GetHighGuid().GetHighGuidType();

        public byte GetSubType() => (byte)(guid.High & 0x3F);

        public ushort GetRealmId() => (ushort)((guid.High >> 42) & 0x1FFF);

        public uint GetServerId() => (uint)((guid.Low >> 40) & 0xFFFFFF);

        public ushort GetMapId() => (ushort)((guid.High >> 29) & 0x1FFF);

        public uint GetEntry()
        {
            if (guid.GetHighType() == HighGuidType.Transport)
                return (uint)(guid.High & 0xFFFFFFFF);
            else
                return (uint)((guid.High >> 6) & 0x7FFFFF);
        }

        public ulong GetCounter()
        {
            if (guid.GetHighType() == HighGuidType.Transport)
                return (guid.High >> 38) & 0xFFFFF;
            else
                return guid.Low & 0xFFFFFFFFFF;
        }

        public bool HasEntry() => guid.GetHighType() switch
        {
            HighGuidType.Creature or HighGuidType.GameObject or HighGuidType.Pet or HighGuidType.Vehicle or HighGuidType.AreaTrigger => true,
            _ => false,
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

        public WowGuid64 To64() => WowGuid64.Create(guid);

        public WowGuid128 To128(GameSessionData gameState) => guid;

        public string ToString()
        {
            if (guid.Low == 0 && guid.High == 0)
                return "Full: 0x0";

            if (guid.HasEntry())
            {
                return $"Full: 0x{guid.High:X16}{guid.Low:X16} {guid.GetHighType()}/{guid.GetSubType()} R{guid.GetRealmId()}/S{guid.GetServerId()} Map: {guid.GetMapId()} Entry: {guid.GetEntry()} Low: {guid.GetCounter()}";
            }

            return $"Full: 0x{guid.High:X16}{guid.Low:X16} {guid.GetHighType()}/{guid.GetSubType()} R{guid.GetRealmId()}/S{guid.GetServerId()} Map: {guid.GetMapId()} Low: {guid.GetCounter()}";
        }
    }
}
