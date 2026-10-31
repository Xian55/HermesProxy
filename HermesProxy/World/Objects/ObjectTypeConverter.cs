using HermesProxy.World.Enums;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace HermesProxy.World.Objects;

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
    private static readonly FrozenDictionary<ObjectType, ObjectTypeBCC> ReverseDictBCC =
        ConvDictBCC.ToFrozenDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static ObjectType Convert(ObjectTypeLegacy type)
    {
        if (!ConvDictLegacy.TryGetValue(type, out var result))
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
