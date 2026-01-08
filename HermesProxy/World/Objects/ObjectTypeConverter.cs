using HermesProxy.World.Enums;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace HermesProxy.World.Objects
{
    public static class ObjectTypeConverter
    {
        // Forward lookups: Legacy/Versioned -> Universal
        private static readonly FrozenDictionary<ObjectTypeLegacy, ObjectType> ConvDictLegacy = new Dictionary<ObjectTypeLegacy, ObjectType>
        {
            { ObjectTypeLegacy.Object,                 ObjectType.Object },
            { ObjectTypeLegacy.Item,                   ObjectType.Item },
            { ObjectTypeLegacy.Container,              ObjectType.Container },
            { ObjectTypeLegacy.Unit,                   ObjectType.Unit },
            { ObjectTypeLegacy.Player,                 ObjectType.Player },
            { ObjectTypeLegacy.GameObject,             ObjectType.GameObject },
            { ObjectTypeLegacy.DynamicObject,          ObjectType.DynamicObject },
            { ObjectTypeLegacy.Corpse,                 ObjectType.Corpse },
            { ObjectTypeLegacy.AreaTrigger,            ObjectType.AreaTrigger },
            { ObjectTypeLegacy.SceneObject,            ObjectType.SceneObject },
            { ObjectTypeLegacy.Conversation,           ObjectType.Conversation }
        }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ObjectType801, ObjectType> ConvDict801 = new Dictionary<ObjectType801, ObjectType>
        {
            { ObjectType801.Object,                 ObjectType.Object },
            { ObjectType801.Item,                   ObjectType.Item },
            { ObjectType801.Container,              ObjectType.Container },
            { ObjectType801.AzeriteEmpoweredItem,   ObjectType.AzeriteEmpoweredItem },
            { ObjectType801.AzeriteItem,            ObjectType.AzeriteItem },
            { ObjectType801.Unit,                   ObjectType.Unit },
            { ObjectType801.Player,                 ObjectType.Player },
            { ObjectType801.ActivePlayer,           ObjectType.ActivePlayer },
            { ObjectType801.GameObject,             ObjectType.GameObject },
            { ObjectType801.DynamicObject,          ObjectType.DynamicObject },
            { ObjectType801.Corpse,                 ObjectType.Corpse },
            { ObjectType801.AreaTrigger,            ObjectType.AreaTrigger },
            { ObjectType801.SceneObject,            ObjectType.SceneObject },
            { ObjectType801.Conversation,           ObjectType.Conversation }
        }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ObjectTypeBCC, ObjectType> ConvDictBCC = new Dictionary<ObjectTypeBCC, ObjectType>
        {
            { ObjectTypeBCC.Object,                 ObjectType.Object },
            { ObjectTypeBCC.Item,                   ObjectType.Item },
            { ObjectTypeBCC.Container,              ObjectType.Container },
            { ObjectTypeBCC.Unit,                   ObjectType.Unit },
            { ObjectTypeBCC.Player,                 ObjectType.Player },
            { ObjectTypeBCC.ActivePlayer,           ObjectType.ActivePlayer },
            { ObjectTypeBCC.GameObject,             ObjectType.GameObject },
            { ObjectTypeBCC.DynamicObject,          ObjectType.DynamicObject },
            { ObjectTypeBCC.Corpse,                 ObjectType.Corpse },
            { ObjectTypeBCC.AreaTrigger,            ObjectType.AreaTrigger },
            { ObjectTypeBCC.SceneObject,            ObjectType.SceneObject },
            { ObjectTypeBCC.Conversation,           ObjectType.Conversation }
        }.ToFrozenDictionary();

        // Reverse lookups: Universal -> Legacy/Versioned (O(1) instead of O(n))
        private static readonly FrozenDictionary<ObjectType, ObjectTypeLegacy> ReverseDictLegacy =
            ConvDictLegacy.ToFrozenDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private static readonly FrozenDictionary<ObjectType, ObjectType801> ReverseDict801 =
            ConvDict801.ToFrozenDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private static readonly FrozenDictionary<ObjectType, ObjectTypeBCC> ReverseDictBCC =
            ConvDictBCC.ToFrozenDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static ObjectType Convert(ObjectTypeLegacy type)
        {
            if (!ConvDictLegacy.TryGetValue(type, out var result))
                throw new ArgumentOutOfRangeException(nameof(type), $"0x{(int)type:X}");
            return result;
        }

        public static ObjectTypeLegacy ConvertToLegacy(ObjectType type)
        {
            if (!ReverseDictLegacy.TryGetValue(type, out var result))
                throw new ArgumentOutOfRangeException(nameof(type), $"0x{(int)type:X}");
            return result;
        }

        public static ObjectType Convert(ObjectType801 type)
        {
            if (!ConvDict801.TryGetValue(type, out var result))
                throw new ArgumentOutOfRangeException(nameof(type), $"0x{(int)type:X}");
            return result;
        }

        public static ObjectType801 ConvertTo801(ObjectType type)
        {
            if (!ReverseDict801.TryGetValue(type, out var result))
                throw new ArgumentOutOfRangeException(nameof(type), $"0x{(int)type:X}");
            return result;
        }

        public static ObjectType Convert(ObjectTypeBCC type)
        {
            if (!ConvDictBCC.TryGetValue(type, out var result))
                throw new ArgumentOutOfRangeException(nameof(type), $"0x{(int)type:X}");
            return result;
        }

        public static ObjectTypeBCC ConvertToBCC(ObjectType type)
        {
            if (!ReverseDictBCC.TryGetValue(type, out var result))
                throw new ArgumentOutOfRangeException(nameof(type), $"0x{(int)type:X}");
            return result;
        }
    }
}
