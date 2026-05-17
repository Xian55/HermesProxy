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

// Byte-equivalence oracle for Phase 5b Object section. Production WriteUpdateObjectData
// + HasAnyObjectFieldSet are now generator-emitted from [DescriptorUpdateField] attrs on
// V3_4_3_54261.ObjectField. These tests inline the pre-Phase-5b hand-port logic and
// assert the generated methods produce byte-identical output for representative payloads.
public class ObjectSectionEquivalenceTests
{
    private static GlobalSessionData CreateGlobalSession()
        => (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

    // GameSessionData has a private ctor + public factory that allocates CurrentPlayerStorage;
    // for an isolated wire-format test we sidestep all that with GetUninitializedObject and
    // only populate the field the builder dereferences (OriginalObjectTypes).
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

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public void WriteUpdateObjectData_GeneratedMatchesHandPort(bool setEntry, bool setDynFlags, bool setScale)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        var builder = MakeBuilder(guid, session, out var update);
        if (setEntry) update.ObjectData.EntryID = 5678;
        if (setDynFlags) update.ObjectData.DynamicFlags = 0x00000010u;
        if (setScale) update.ObjectData.Scale = 1.5f;

        var actual = new WorldPacket();
        builder.WriteUpdateObjectData(actual);

        var expected = new WorldPacket();
        WriteUpdateObjectData_HandPort(expected, update.ObjectData);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    [Theory]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(false, false, false, false)]
    public void HasAnyObjectFieldSet_GeneratedMatchesHandPort(bool setEntry, bool setDynFlags, bool setScale, bool expected)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        var builder = MakeBuilder(guid, session, out var update);
        if (setEntry) update.ObjectData.EntryID = 7;
        if (setDynFlags) update.ObjectData.DynamicFlags = 1u;
        if (setScale) update.ObjectData.Scale = 1f;

        Assert.Equal(expected, builder.HasAnyObjectFieldSet());
    }

    // ---------------------------------------------------------------------
    // Inlined pre-Phase-5b hand-port — byte-for-byte oracle. Frozen here so the
    // production hand-port can be deleted in favour of the source-generated
    // equivalent. Identical to the body removed from
    // V3_4_3_54261/ObjectUpdateBuilder.cs (line 1635-1678 before the deletion)
    // minus the trace log (which only emitted to Framework.Logging and produced
    // no wire bytes).
    // ---------------------------------------------------------------------
    private static void WriteUpdateObjectData_HandPort(WorldPacket data, ObjectData obj)
    {
        uint mask = 0u;
        if (obj.EntryID.HasValue) mask |= 2;
        if (obj.DynamicFlags.HasValue) mask |= 4;
        if (obj.Scale.HasValue) mask |= 8;
        if (mask != 0) mask |= 1;
        data.WriteBits(mask, 4);
        data.FlushBits();
        if ((mask & 1) != 0)
        {
            if (obj.EntryID.HasValue) data.WriteInt32(obj.EntryID.Value);
            if (obj.DynamicFlags.HasValue) data.WriteUInt32(obj.DynamicFlags.Value);
            if (obj.Scale.HasValue) data.WriteFloat(obj.Scale.Value);
        }
    }
}
