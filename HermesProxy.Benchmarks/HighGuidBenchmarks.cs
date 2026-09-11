using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using HermesProxy.World;
using HermesProxy.World.Enums;

namespace HermesProxy.Benchmarks;

// High-guid type lookup, which sits under GetCounter/GetEntry/HasEntry/GetObjectType/IsItem and
// therefore runs several times per guid on the update path.
//
// The Legacy* types below are verbatim copies of the abstract-class design this replaced (minus
// its unknown-high logging, which no benchmarked value reaches).
//
// Every benchmark walks an array rather than converting one guid held in a field. A single-guid
// version reported 0.0000 ns for the static lookup: it is pure and loop-invariant, so the JIT
// hoists it straight out of BenchmarkDotNet's unrolled loop, while the legacy side's dictionary
// lookup is an opaque call that has to run every iteration. That measures hoisting, not the
// mapping. Walking varied input keeps both sides honest, so the numbers are per Count lookups.
[MemoryDiagnoser]
[ShortRunJob]
public class HighGuidBenchmarks
{
    private const int Count = 256;

    private WowGuid64[] _guids64 = null!;
    private WowGuid128[] _guids128 = null!;
    private byte[] _highs703 = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Spread over the highs that actually arrive from a legacy backend, so neither the
        // switch nor the dictionary gets an unrealistically predictable input.
        HighGuidTypeLegacy[] legacyHighs =
        [
            HighGuidTypeLegacy.Creature,
            HighGuidTypeLegacy.Player,
            HighGuidTypeLegacy.Item,
            HighGuidTypeLegacy.GameObject,
            HighGuidTypeLegacy.Pet,
            HighGuidTypeLegacy.DynamicObject,
            HighGuidTypeLegacy.Corpse,
            HighGuidTypeLegacy.ItemContainer,
        ];

        HighGuidType703[] highs703 =
        [
            HighGuidType703.Creature,
            HighGuidType703.Player,
            HighGuidType703.Item,
            HighGuidType703.GameObject,
            HighGuidType703.Pet,
            HighGuidType703.DynamicObject,
            HighGuidType703.Corpse,
            HighGuidType703.Transport,
        ];

        _guids64 = new WowGuid64[Count];
        _guids128 = new WowGuid128[Count];
        _highs703 = new byte[Count];

        for (int i = 0; i < Count; i++)
        {
            var legacyHigh = legacyHighs[i % legacyHighs.Length];
            _guids64[i] = new WowGuid64(legacyHigh, 25000u + (uint)i, (uint)(i + 1));
            _guids128[i] = WowGuid128.Create(HighGuidType703.Creature, 0, 25000u + (uint)i, (ulong)(i + 1));
            _highs703[i] = (byte)highs703[i % highs703.Length];
        }
    }

    [Benchmark(Baseline = true)]
    public int Legacy_Lookup_64()
    {
        int acc = 0;
        foreach (var guid in _guids64)
            acc += (int)new LegacyHighGuidLegacy(guid.GetHighGuidTypeLegacy()).GetHighGuidType();
        return acc;
    }

    [Benchmark]
    public int Static_Lookup_64()
    {
        int acc = 0;
        foreach (var guid in _guids64)
            acc += (int)HighGuid.FromLegacy(guid.GetHighGuidTypeLegacy());
        return acc;
    }

    [Benchmark]
    public int Legacy_Lookup_703()
    {
        int acc = 0;
        foreach (var high in _highs703)
            acc += (int)new LegacyHighGuid703(high).GetHighGuidType();
        return acc;
    }

    [Benchmark]
    public int Static_Lookup_703()
    {
        int acc = 0;
        foreach (var high in _highs703)
            acc += (int)HighGuid.From703(high);
        return acc;
    }

    // End-to-end, current implementation only — there is no paired legacy number because the
    // conversion code itself is unchanged. GetHighType() runs once for the Create switch and
    // again under GetEntry() and GetCounter().
    [Benchmark]
    public ulong Convert128To64()
    {
        ulong acc = 0;
        foreach (var guid in _guids128)
            acc += WowGuid64.Create(guid).Low;
        return acc;
    }

    #region Verbatim copies of the pre-refactor design

    private abstract class LegacyHighGuid
    {
        protected HighGuidType highGuidType;

        public HighGuidType GetHighGuidType() => highGuidType;
    }

    private sealed class LegacyHighGuidLegacy : LegacyHighGuid
    {
        HighGuidTypeLegacy high;
        static readonly Dictionary<HighGuidTypeLegacy, HighGuidType> HighLegacyToHighType
            = new Dictionary<HighGuidTypeLegacy, HighGuidType>
        {
            { HighGuidTypeLegacy.None, HighGuidType.Null },
            { HighGuidTypeLegacy.Player, HighGuidType.Player },
            { HighGuidTypeLegacy.Group, HighGuidType.RaidGroup },
            { HighGuidTypeLegacy.Group2, HighGuidType.RaidGroup },
            { HighGuidTypeLegacy.MOTransport, HighGuidType.MOTransport },
            { HighGuidTypeLegacy.Item, HighGuidType.Item },
            { HighGuidTypeLegacy.ItemContainer, HighGuidType.Item },
            { HighGuidTypeLegacy.DynamicObject, HighGuidType.DynamicObject },
            { HighGuidTypeLegacy.GameObject, HighGuidType.GameObject },
            { HighGuidTypeLegacy.Transport, HighGuidType.Transport },
            { HighGuidTypeLegacy.Creature, HighGuidType.Creature },
            { HighGuidTypeLegacy.Pet, HighGuidType.Pet },
            { HighGuidTypeLegacy.Vehicle, HighGuidType.Vehicle },
            { HighGuidTypeLegacy.Corpse, HighGuidType.Corpse },
        };

        public LegacyHighGuidLegacy(HighGuidTypeLegacy high)
        {
            this.high = high;
            if (!HighLegacyToHighType.TryGetValue(high, out highGuidType))
                highGuidType = HighGuidType.Null;
        }
    }

    private sealed class LegacyHighGuid703 : LegacyHighGuid
    {
        byte high;
        static readonly Dictionary<HighGuidType703, HighGuidType> High703ToHighType
            = new Dictionary<HighGuidType703, HighGuidType>
        {
            { HighGuidType703.Null,              HighGuidType.Null },
            { HighGuidType703.Uniq,              HighGuidType.Uniq },
            { HighGuidType703.Player,            HighGuidType.Player },
            { HighGuidType703.Item,              HighGuidType.Item },
            { HighGuidType703.WorldTransaction,  HighGuidType.WorldTransaction },
            { HighGuidType703.StaticDoor,        HighGuidType.StaticDoor },
            { HighGuidType703.Transport,         HighGuidType.Transport },
            { HighGuidType703.Conversation,      HighGuidType.Conversation },
            { HighGuidType703.Creature,          HighGuidType.Creature },
            { HighGuidType703.Vehicle,           HighGuidType.Vehicle },
            { HighGuidType703.Pet,               HighGuidType.Pet },
            { HighGuidType703.GameObject,        HighGuidType.GameObject },
            { HighGuidType703.DynamicObject,     HighGuidType.DynamicObject },
            { HighGuidType703.AreaTrigger,       HighGuidType.AreaTrigger },
            { HighGuidType703.Corpse,            HighGuidType.Corpse },
            { HighGuidType703.LootObject,        HighGuidType.LootObject },
            { HighGuidType703.SceneObject,       HighGuidType.SceneObject },
            { HighGuidType703.Scenario,          HighGuidType.Scenario },
            { HighGuidType703.AIGroup,           HighGuidType.AIGroup },
            { HighGuidType703.DynamicDoor,       HighGuidType.DynamicDoor },
            { HighGuidType703.ClientActor,       HighGuidType.ClientActor },
            { HighGuidType703.Vignette,          HighGuidType.Vignette },
            { HighGuidType703.CallForHelp,       HighGuidType.CallForHelp },
            { HighGuidType703.AIResource,        HighGuidType.AIResource },
            { HighGuidType703.AILock,            HighGuidType.AILock },
            { HighGuidType703.AILockTicket,      HighGuidType.AILockTicket },
            { HighGuidType703.ChatChannel,       HighGuidType.ChatChannel },
            { HighGuidType703.Party,             HighGuidType.Party },
            { HighGuidType703.Guild,             HighGuidType.Guild },
            { HighGuidType703.WowAccount,        HighGuidType.WowAccount },
            { HighGuidType703.BNetAccount,       HighGuidType.BNetAccount },
            { HighGuidType703.GMTask,            HighGuidType.GMTask },
            { HighGuidType703.MobileSession,     HighGuidType.MobileSession },
            { HighGuidType703.RaidGroup,         HighGuidType.RaidGroup },
            { HighGuidType703.Spell,             HighGuidType.Spell },
            { HighGuidType703.Mail,              HighGuidType.Mail },
            { HighGuidType703.WebObj,            HighGuidType.WebObj },
            { HighGuidType703.LFGObject,         HighGuidType.LFGObject },
            { HighGuidType703.LFGList,           HighGuidType.LFGList },
            { HighGuidType703.UserRouter,        HighGuidType.UserRouter },
            { HighGuidType703.PVPQueueGroup,     HighGuidType.PVPQueueGroup },
            { HighGuidType703.UserClient,        HighGuidType.UserClient },
            { HighGuidType703.PetBattle,         HighGuidType.PetBattle },
            { HighGuidType703.UniqUserClient,    HighGuidType.UniqUserClient },
            { HighGuidType703.BattlePet,         HighGuidType.BattlePet },
            { HighGuidType703.CommerceObj,       HighGuidType.CommerceObj },
            { HighGuidType703.ClientSession,     HighGuidType.ClientSession },
            { HighGuidType703.Cast,              HighGuidType.Cast },
            { HighGuidType703.ClientConnection,  HighGuidType.ClientConnection },
            { HighGuidType703.ClubFinder,        HighGuidType.ClubFinder },
            { HighGuidType703.ToolsClient,       HighGuidType.ToolsClient },
            { HighGuidType703.WorldLayer,        HighGuidType.WorldLayer },
            { HighGuidType703.ArenaTeam,         HighGuidType.ArenaTeam },
            { HighGuidType703.Invalid,           HighGuidType.Invalid }
        };

        public LegacyHighGuid703(byte high)
        {
            this.high = high;
            if (!High703ToHighType.TryGetValue((HighGuidType703)high, out highGuidType))
                highGuidType = HighGuidType.Null;
        }
    }

    #endregion
}
