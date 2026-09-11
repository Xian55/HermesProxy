using System.Runtime.CompilerServices;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World;

/// <summary>
/// Maps a raw high-guid value — the legacy 64-bit form's top 16 bits, or the 7.0.3+ form's
/// 6-bit type field — onto the version-independent <see cref="HighGuidType"/>.
///
/// <para>
/// This was an abstract class with a HighGuidLegacy / HighGuid703 subclass, one instance
/// constructed per lookup. <c>GetHighType()</c> sits under <c>GetCounter()</c>,
/// <c>GetEntry()</c>, <c>HasEntry()</c>, <c>GetObjectType()</c> and <c>IsItem()</c>, so
/// converting a single creature guid between the two widths allocated three or more of them,
/// on the per-packet update path. The mapping holds no state beyond the answer, so there is
/// nothing left to allocate: both directions are switches over the raw value now.
/// </para>
/// </summary>
public static class HighGuid
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer =
        Log.CreateMelLogger(Log.CategoryServer);

    /// <summary>
    /// Legacy (pre-7.0.3) high-guid. Sparse 16-bit values, so this compiles to a small
    /// binary search rather than a jump table.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HighGuidType FromLegacy(HighGuidTypeLegacy high) => high switch
    {
        HighGuidTypeLegacy.None => HighGuidType.Null,
        HighGuidTypeLegacy.Player => HighGuidType.Player,
        HighGuidTypeLegacy.Group => HighGuidType.RaidGroup,
        HighGuidTypeLegacy.Group2 => HighGuidType.RaidGroup,
        HighGuidTypeLegacy.MOTransport => HighGuidType.MOTransport, // ?? unused in wpp
        HighGuidTypeLegacy.Item => HighGuidType.Item,
        HighGuidTypeLegacy.ItemContainer => HighGuidType.Item, // cmangos 0x4700 → treat as Item
        HighGuidTypeLegacy.DynamicObject => HighGuidType.DynamicObject,
        HighGuidTypeLegacy.GameObject => HighGuidType.GameObject,
        HighGuidTypeLegacy.Transport => HighGuidType.Transport,
        HighGuidTypeLegacy.Creature => HighGuidType.Creature,
        HighGuidTypeLegacy.Pet => HighGuidType.Pet,
        HighGuidTypeLegacy.Vehicle => HighGuidType.Vehicle,
        HighGuidTypeLegacy.Corpse => HighGuidType.Corpse,
        _ => UnknownLegacy(high),
    };

    /// <summary>
    /// 7.0.3+ high-guid: the 6-bit type field, so callers pass <c>(high >> 58) &amp; 0x3F</c>
    /// and every input lands in 0-63.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HighGuidType From703(byte high) => (HighGuidType703)high switch
    {
        HighGuidType703.Null => HighGuidType.Null,
        HighGuidType703.Uniq => HighGuidType.Uniq,
        HighGuidType703.Player => HighGuidType.Player,
        HighGuidType703.Item => HighGuidType.Item,
        HighGuidType703.WorldTransaction => HighGuidType.WorldTransaction,
        HighGuidType703.StaticDoor => HighGuidType.StaticDoor,
        HighGuidType703.Transport => HighGuidType.Transport,
        HighGuidType703.Conversation => HighGuidType.Conversation,
        HighGuidType703.Creature => HighGuidType.Creature,
        HighGuidType703.Vehicle => HighGuidType.Vehicle,
        HighGuidType703.Pet => HighGuidType.Pet,
        HighGuidType703.GameObject => HighGuidType.GameObject,
        HighGuidType703.DynamicObject => HighGuidType.DynamicObject,
        HighGuidType703.AreaTrigger => HighGuidType.AreaTrigger,
        HighGuidType703.Corpse => HighGuidType.Corpse,
        HighGuidType703.LootObject => HighGuidType.LootObject,
        HighGuidType703.SceneObject => HighGuidType.SceneObject,
        HighGuidType703.Scenario => HighGuidType.Scenario,
        HighGuidType703.AIGroup => HighGuidType.AIGroup,
        HighGuidType703.DynamicDoor => HighGuidType.DynamicDoor,
        HighGuidType703.ClientActor => HighGuidType.ClientActor,
        HighGuidType703.Vignette => HighGuidType.Vignette,
        HighGuidType703.CallForHelp => HighGuidType.CallForHelp,
        HighGuidType703.AIResource => HighGuidType.AIResource,
        HighGuidType703.AILock => HighGuidType.AILock,
        HighGuidType703.AILockTicket => HighGuidType.AILockTicket,
        HighGuidType703.ChatChannel => HighGuidType.ChatChannel,
        HighGuidType703.Party => HighGuidType.Party,
        HighGuidType703.Guild => HighGuidType.Guild,
        HighGuidType703.WowAccount => HighGuidType.WowAccount,
        HighGuidType703.BNetAccount => HighGuidType.BNetAccount,
        HighGuidType703.GMTask => HighGuidType.GMTask,
        HighGuidType703.MobileSession => HighGuidType.MobileSession,
        HighGuidType703.RaidGroup => HighGuidType.RaidGroup,
        HighGuidType703.Spell => HighGuidType.Spell,
        HighGuidType703.Mail => HighGuidType.Mail,
        HighGuidType703.WebObj => HighGuidType.WebObj,
        HighGuidType703.LFGObject => HighGuidType.LFGObject,
        HighGuidType703.LFGList => HighGuidType.LFGList,
        HighGuidType703.UserRouter => HighGuidType.UserRouter,
        HighGuidType703.PVPQueueGroup => HighGuidType.PVPQueueGroup,
        HighGuidType703.UserClient => HighGuidType.UserClient,
        HighGuidType703.PetBattle => HighGuidType.PetBattle,
        HighGuidType703.UniqUserClient => HighGuidType.UniqUserClient,
        HighGuidType703.BattlePet => HighGuidType.BattlePet,
        HighGuidType703.CommerceObj => HighGuidType.CommerceObj,
        HighGuidType703.ClientSession => HighGuidType.ClientSession,
        HighGuidType703.Cast => HighGuidType.Cast,
        HighGuidType703.ClientConnection => HighGuidType.ClientConnection,
        HighGuidType703.ClubFinder => HighGuidType.ClubFinder,
        HighGuidType703.ToolsClient => HighGuidType.ToolsClient,
        HighGuidType703.WorldLayer => HighGuidType.WorldLayer,
        HighGuidType703.ArenaTeam => HighGuidType.ArenaTeam,
        HighGuidType703.Invalid => HighGuidType.Invalid,
        _ => Unknown703(high),
    };

    // FIXME(phase5a-7c): an unknown legacy high-guid is mapped to Null and the object is
    // dropped on the modern side. Originally added for cmangos's non-standard 0x4700
    // ItemContainer; that case now has its own enum entry and proper mapping. Any remaining
    // unknown high here likely indicates a missing mapping (or a corrupt packet) —
    // investigate the warning log before adding cases above or removing this fallback.
    //
    // Kept off the inlined switch so the hot path carries none of the logging call's
    // register pressure.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static HighGuidType UnknownLegacy(HighGuidTypeLegacy high)
    {
        GuidLogMessages.UnknownLegacyHighGuid(_melServer, (uint)high);
        return HighGuidType.Null;
    }

    // Defense-in-depth: an unknown 703 high-guid type must NOT throw — that exception
    // propagates out of HandleUpdateObject into the WorldClient receive loop and tears down
    // the whole legacy connection (one bad guid disconnects the client, issue #101). Mirror
    // the legacy side: log it and treat as Null so the single object is skipped on the modern
    // side instead of killing the session.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static HighGuidType Unknown703(byte high)
    {
        GuidLogMessages.Unknown703HighGuid(_melServer, high);
        return HighGuidType.Null;
    }
}
