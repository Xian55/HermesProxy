using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.V3_4_3_54261;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

// Byte-equivalence oracle for Phase 5b Container section. Production
// WriteCreateContainerData / WriteUpdateContainerData / HasAnyContainerFieldSet are
// now generator-emitted from [DescriptorCreateField] + [DescriptorUpdateField] attrs
// on V3_4_3_54261.ContainerField. These tests inline the pre-Phase-5b hand-port logic
// and assert the generated methods produce byte-identical output across representative
// payloads: empty, NumSlots-only, single-slot-set, all-slots-set, mixed.
//
// Exercises three generator features new in this step: multi-block changesMask
// (2 blocks × 32 bits + 2-bit blocks-prefix), per-field ParentBit gating, and
// ArrayMode.PerElement (per-element bit + per-element conditional write).
public class ContainerSectionEquivalenceTests
{
    private static GlobalSessionData CreateGlobalSession()
        => (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

    private static GameSessionData CreateGameSession()
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new System.Collections.Generic.Dictionary<WowGuid128, ObjectType>());
        return session;
    }

    private static ObjectUpdateBuilder MakeBuilder(WowGuid128 guid, GameSessionData session, out ObjectUpdate update)
    {
        var globalSession = CreateGlobalSession();
        update = new ObjectUpdate(guid, UpdateTypeModern.Values, globalSession);
        return new ObjectUpdateBuilder(update, session);
    }

    // ---------------------------------------------------------------------
    // WriteCreate path — byte-eq vs frozen hand-port
    // ---------------------------------------------------------------------

    public static System.Collections.Generic.IEnumerable<object[]> CreateScenarios()
    {
        yield return new object[] { "empty",               (System.Action<ContainerData>)(_ => { }) };
        yield return new object[] { "numslots-only",       (System.Action<ContainerData>)(c => c.NumSlots = 16u) };
        yield return new object[] { "single-slot-zero",    (System.Action<ContainerData>)(c => c.Slots[0] = WowGuid128.Create(HighGuidType703.Item, 101)) };
        yield return new object[] { "single-slot-mid",     (System.Action<ContainerData>)(c => c.Slots[17] = WowGuid128.Create(HighGuidType703.Item, 202)) };
        yield return new object[] { "single-slot-last",    (System.Action<ContainerData>)(c => c.Slots[35] = WowGuid128.Create(HighGuidType703.Item, 303)) };
        yield return new object[] { "all-slots-set",       (System.Action<ContainerData>)(c => {
            for (int i = 0; i < 36; i++) c.Slots[i] = WowGuid128.Create(HighGuidType703.Item, (ulong)(1000 + i));
        }) };
        yield return new object[] { "mixed-numslots-slots", (System.Action<ContainerData>)(c => {
            c.NumSlots = 20u;
            c.Slots[0] = WowGuid128.Create(HighGuidType703.Item, 1);
            c.Slots[1] = WowGuid128.Create(HighGuidType703.Item, 2);
            c.Slots[31] = WowGuid128.Create(HighGuidType703.Item, 99);
            c.Slots[32] = WowGuid128.Create(HighGuidType703.Item, 100);
        }) };
    }

    [Theory]
    [MemberData(nameof(CreateScenarios))]
    public void WriteCreateContainerData_GeneratedMatchesHandPort(string _label, System.Action<ContainerData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.ContainerData!);

        var actual = new WorldPacket();
        builder.WriteCreateContainerData(actual);

        var expected = new WorldPacket();
        WriteCreateContainerData_HandPort(expected, update.ContainerData!);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // WriteUpdate path — byte-eq vs frozen hand-port
    // ---------------------------------------------------------------------

    public static System.Collections.Generic.IEnumerable<object[]> UpdateScenarios()
    {
        yield return new object[] { "empty",            (System.Action<ContainerData>)(_ => { }) };
        yield return new object[] { "numslots-only",    (System.Action<ContainerData>)(c => c.NumSlots = 24u) };
        yield return new object[] { "slot-0-only",      (System.Action<ContainerData>)(c => c.Slots[0] = WowGuid128.Create(HighGuidType703.Item, 7)) };
        yield return new object[] { "slot-31-only",     (System.Action<ContainerData>)(c => c.Slots[31] = WowGuid128.Create(HighGuidType703.Item, 31)) };
        yield return new object[] { "slot-32-only",     (System.Action<ContainerData>)(c => c.Slots[32] = WowGuid128.Create(HighGuidType703.Item, 32)) };
        yield return new object[] { "slot-35-only",     (System.Action<ContainerData>)(c => c.Slots[35] = WowGuid128.Create(HighGuidType703.Item, 35)) };
        yield return new object[] { "block-boundary",   (System.Action<ContainerData>)(c => {
            c.Slots[28] = WowGuid128.Create(HighGuidType703.Item, 28); // bit 31, end of block 0
            c.Slots[29] = WowGuid128.Create(HighGuidType703.Item, 29); // bit 32, start of block 1
        }) };
        yield return new object[] { "all-fields",       (System.Action<ContainerData>)(c => {
            c.NumSlots = 36u;
            for (int i = 0; i < 36; i++) c.Slots[i] = WowGuid128.Create(HighGuidType703.Item, (ulong)(50 + i));
        }) };
    }

    [Theory]
    [MemberData(nameof(UpdateScenarios))]
    public void WriteUpdateContainerData_GeneratedMatchesHandPort(string _label, System.Action<ContainerData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.ContainerData!);

        var actual = new WorldPacket();
        builder.WriteUpdateContainerData(actual);

        var expected = new WorldPacket();
        WriteUpdateContainerData_HandPort(expected, update.ContainerData!);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // HasAny — semantic eq
    // ---------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(UpdateScenarios))]
    public void HasAnyContainerFieldSet_GeneratedMatchesHandPort(string _label, System.Action<ContainerData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.ContainerData!);

        Assert.Equal(HasAnyContainerFieldSet_HandPort(update.ContainerData!), builder.HasAnyContainerFieldSet());
    }

    // ---------------------------------------------------------------------
    // Inlined pre-Phase-5b hand-port — byte-for-byte oracle. Frozen here so the
    // production hand-port can be deleted in favour of the source-generated
    // equivalent. Identical to bodies removed from
    // V3_4_3_54261/ObjectUpdateBuilder.cs (lines 466-472 Create / 1330-1338 HasAny /
    // 3137-3175 Update before the deletion).
    // ---------------------------------------------------------------------

    private static void WriteCreateContainerData_HandPort(WorldPacket data, ContainerData container)
    {
        for (int i = 0; i < 36; i++)
            data.WritePackedGuid128(container?.Slots[i] ?? WowGuid128.Empty);
        data.WriteUInt32((container?.NumSlots).GetValueOrDefault());
    }

    private static void WriteUpdateContainerData_HandPort(WorldPacket data, ContainerData container)
    {
        if (container == null)
        {
            data.WriteBits(0u, 2);
            data.FlushBits();
            return;
        }

        // 39 bits in 2 blocks of 32: bit 0 = NumSlots parent, bit 1 = NumSlots,
        // bit 2 = Slots parent, bits 3..38 = Slots[0..35].
        System.Span<uint> blocksBuf = stackalloc uint[2];
        var blocks = new global::Framework.Util.StackBitMask(blocksBuf);
        if (container.NumSlots.HasValue) { blocks.SetBit(0); blocks.SetBit(1); }
        for (int i = 0; i < 36; i++)
            if (container.Slots[i].HasValue) { blocks.SetBit(2); blocks.SetBit(3 + i); }

        byte blocksMask = 0;
        if (blocks[0] != 0) blocksMask |= 1;
        if (blocks[1] != 0) blocksMask |= 2;
        data.WriteBits((uint)blocksMask, 2);
        for (int b = 0; b < 2; b++)
            if ((blocksMask & (1 << b)) != 0)
                data.WriteBits(blocks[b], 32);
        data.FlushBits();

        if ((blocks[0] & (1u << 1)) != 0)
            data.WriteUInt32(container.NumSlots!.Value);
        if ((blocks[0] & (1u << 2)) != 0)
        {
            for (int i = 0; i < 36; i++)
                if (container.Slots[i].HasValue)
                    data.WritePackedGuid128(container.Slots[i]!.Value);
        }
    }

    private static bool HasAnyContainerFieldSet_HandPort(ContainerData c)
    {
        if (c == null) return false;
        if (c.NumSlots.HasValue) return true;
        for (int i = 0; i < 36; i++)
            if (c.Slots[i].HasValue) return true;
        return false;
    }
}
