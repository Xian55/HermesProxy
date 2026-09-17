using System;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Issue #235: which V3_4_3 Values deltas the filter keeps, and which it drops.
/// </summary>
/// <remarks>
/// The filter exists because cMangos emits bookkeeping updates that carry no field at all, and the
/// V3_4_3 client answers the resulting 13-byte body with CMSG_OBJECT_UPDATE_FAILED. Deciding which
/// was which used to mean a hand-written list of fields in <c>IsEmptyValuesDelta</c>, covering a
/// fraction of the descriptor tree. Every field added to a descriptor afterwards was absent from
/// that list, so a delta carrying only the new field was built, classified empty and deleted — and
/// because nothing failed, the loss showed up months later as a feature that silently did nothing:
/// <code>
/// ObjectData.DynamicFlags          corpse stayed sparkly after loot release
/// GameObjectData.Level/State       moving transports, doors and chests froze
/// ActivePlayerData.NumStableSlots  #224 - bought stable slots stayed locked
/// ActivePlayerData.MultiActionBars action bars 2-5 never became visible
/// ActivePlayerData.PackSlots       looted items invisible until relog
/// ItemData.Durability              "repair did nothing"
/// ActivePlayerData.QuestCompleted  quest completion never reached the client
/// PlayerData.QuestLog              an accepted quest appeared only after a relog
/// </code>
/// The filter now asks the writer instead — <c>ObjectUpdateBuilder.HasAnyValuesDelta</c> runs the
/// generated <c>HasAny*FieldSet</c> predicates over the descriptor tree the writer serializes from,
/// so a field reaches the wire and the filter in the same commit. These tests are the list above,
/// turned into the regression it never had.
/// <para>
/// <c>ModernVersion.Build</c> is fixed for the test process, so the V3_4_3 arm is reached through
/// <c>UpdateObject.ForceV343ForTests</c>.
/// </para>
/// </remarks>
[Collection("V343ValuesFilter")]
public class ValuesDeltaFilterTests
{
    private static readonly WowGuid128 Player = WowGuid128.Create(HighGuidType703.Player, 4020);
    private static readonly WowGuid128 Creature = WowGuid128.Create(HighGuidType703.Creature, 1, 299, 7);
    private static readonly WowGuid128 GameObject = WowGuid128.Create(HighGuidType703.GameObject, 1, 176080, 11);
    private static readonly WowGuid128 Item = WowGuid128.Create(HighGuidType703.Item, 91);
    private static readonly WowGuid128 Corpse = WowGuid128.Create(HighGuidType703.Corpse, 1, 0, 3);
    private static readonly WowGuid128 DynObject = WowGuid128.Create(HighGuidType703.DynamicObject, 1, 0, 5);

    private static GameSessionData SessionWithPlayer()
    {
        var state = GameSessionData.CreateNewGameSessionData(null!);
        state.CurrentPlayerGuid = Player;
        state.CurrentMapId = 0;
        return state;
    }

    /// <summary>
    /// Runs one Values delta through the filter and reports whether it survived. The guid is
    /// registered as client-known first: the unknown-guid arm is a separate rule (the client
    /// cannot apply a delta for an object it was never given) and would otherwise mask the
    /// empty-delta answer this asks about.
    /// </summary>
    private static bool Survives(WowGuid128 guid, Action<ObjectUpdate> populate, ObjectType? knownType = null)
    {
        UpdateObject.ForceV343ForTests = true;
        try
        {
            var state = SessionWithPlayer();
            state.ClientKnownGuids.Add(guid);
            if (knownType != null)
                state.OriginalObjectTypes[guid] = knownType.Value;

            var update = new ObjectUpdate(guid, UpdateTypeModern.Values, null!);
            populate(update);

            var batch = new UpdateObject(state);
            batch.ObjectUpdates.Add(update);
            UpdateObject.FilterV3_4_3Values(batch, state);

            return batch.ObjectUpdates.Contains(update);
        }
        finally
        {
            UpdateObject.ForceV343ForTests = null;
        }
    }

    // ===========================================================================
    // The tally in issue #235 — each of these was a live bug, found by a user
    // noticing that something had silently not happened.
    // ===========================================================================

    /// <summary>A looted corpse clears its Lootable bit and says nothing else.</summary>
    [Fact]
    public void ObjectDynamicFlagsAlone_Survives()
    {
        Assert.True(Survives(Creature, u => u.ObjectData.DynamicFlags = 0));
    }

    /// <summary>A moving transport's path period, once GAMEOBJECT_DYNAMIC has stopped changing.</summary>
    [Fact]
    public void GameObjectLevelAlone_Survives()
    {
        Assert.True(Survives(GameObject, u => u.GameObjectData!.Level = 8000));
    }

    /// <summary>A door or chest whose only change is GAMEOBJECT_BYTES_1.</summary>
    [Fact]
    public void GameObjectStateAlone_Survives()
    {
        Assert.True(Survives(GameObject, u => u.GameObjectData!.State = 1));
    }

    /// <summary>Issue #224: a bought stable slot stayed locked.</summary>
    [Fact]
    public void NumStableSlotsAlone_Survives()
    {
        Assert.True(Survives(Player, u => u.EnsureActivePlayerData().NumStableSlots = 3));
    }

    /// <summary>CMSG_SET_ACTION_BAR_TOGGLES lights only this byte.</summary>
    [Fact]
    public void MultiActionBarsAlone_Survives()
    {
        Assert.True(Survives(Player, u => u.EnsureActivePlayerData().MultiActionBars = 0x0F));
    }

    /// <summary>A looted item entering the backpack sets one pack slot and nothing else.</summary>
    [Fact]
    public void PackSlotAlone_Survives()
    {
        Assert.True(Survives(Player, u => u.EnsureActivePlayerData().EnsurePackSlots()[0] = Item));
    }

    /// <summary>A repair pushes an item delta carrying only Durability.</summary>
    [Fact]
    public void ItemDurabilityAlone_Survives()
    {
        Assert.True(Survives(Item, u => u.ItemData.Durability = 55));
    }

    /// <summary>
    /// CompletedQuestTracker.SendSingleUpdateToClient sets one QuestCompleted word. Dropping it
    /// left IsQuestFlaggedCompleted false for every quest, forever.
    /// </summary>
    [Fact]
    public void QuestCompletedWordAlone_Survives()
    {
        Assert.True(Survives(Player, u => u.EnsureActivePlayerData().EnsureQuestCompleted()[0] = 1uL));
    }

    /// <summary>
    /// The one the hand-written list never had at all: accepting a quest lights only the quest-log
    /// words, so the accepted quest reached the client's log only after a relog.
    /// </summary>
    [Fact]
    public void QuestLogEntryAlone_Survives()
    {
        Assert.True(Survives(Player, u => u.PlayerData.EnsureQuestLog()[0] = new QuestLog { QuestID = 5261 }));
    }

    /// <summary>Also absent from the hand-written list: a gear swap seen on another player.</summary>
    [Fact]
    public void VisibleItemAlone_Survives()
    {
        Assert.True(Survives(Player, u => u.PlayerData.EnsureVisibleItems()[0] = new VisibleItem(12640, 0, 0)));
    }

    /// <summary>A bag slot emptying — without this the source slot kept a ghost item until relog.</summary>
    [Fact]
    public void ContainerSlotAlone_Survives()
    {
        Assert.True(Survives(Item, u => u.EnsureContainerData().Slots[0] = WowGuid128.Empty,
            knownType: ObjectType.Container));
    }

    /// <summary>Battleground bones gaining the lootable-insignia bit.</summary>
    [Fact]
    public void CorpseDynamicFlagsAlone_Survives()
    {
        Assert.True(Survives(Corpse, u => u.CorpseData!.DynamicFlags = 1));
    }

    /// <summary>Radius changes on persistent-AoE spells.</summary>
    [Fact]
    public void DynamicObjectRadiusAlone_Survives()
    {
        Assert.True(Survives(DynObject, u => u.DynamicObjectData!.Radius = 8f));
    }

    /// <summary>A stance change, the field issue #300 was traced to.</summary>
    [Fact]
    public void ShapeshiftFormAlone_Survives()
    {
        Assert.True(Survives(Creature, u => u.UnitData.ShapeshiftForm = 17));
    }

    // ===========================================================================
    // What the filter is for. Letting these through is the regression in the
    // other direction: the client answers each one with CMSG_OBJECT_UPDATE_FAILED.
    // ===========================================================================

    /// <summary>A delta whose data blocks exist but hold no field at all.</summary>
    [Fact]
    public void ADeltaWithNoFieldSet_IsDropped()
    {
        Assert.False(Survives(Creature, _ => { }));
    }

    /// <summary>
    /// The flood the filter was written for: cMangos clears an NpcFlags slot it never set, and the
    /// writer treats a zero slot as nothing to say. The filter has to agree, or every one of them
    /// ships as a body the client rejects.
    /// </summary>
    [Fact]
    public void NpcFlagsClearedToZero_IsDropped()
    {
        Assert.False(Survives(Creature, u => u.UnitData.EnsureNpcFlags()[0] = 0));
    }

    /// <summary>The same slot carrying a real flag is a real delta.</summary>
    [Fact]
    public void NpcFlagsSetToAValue_Survives()
    {
        Assert.True(Survives(Creature, u => u.UnitData.EnsureNpcFlags()[0] = 1));
    }

    /// <summary>
    /// The one thing the filter now drops that the hand-written list kept. UnitData carries
    /// fields the 3.4.3 descriptor tree has no slot for — Legion-era scaling, the vanilla pet
    /// loyalty index — and a delta holding only one of those is exactly the packet the filter
    /// exists to stop: the writer has nowhere to put the field, so it emits a bare zero
    /// changedMask and the client answers CMSG_OBJECT_UPDATE_FAILED. Keeping it was never
    /// useful, it just wasn't visible.
    /// </summary>
    [Fact]
    public void AUnitFieldWithNoDescriptorSlot_IsDropped()
    {
        Assert.False(Survives(Creature, u => u.UnitData.ScalingLevelMin = 60));
    }

    // ===========================================================================
    // The invariant behind all of the above.
    // ===========================================================================

    /// <summary>
    /// The filter and the writer answer the same question, so they must give the same answer: a
    /// delta the filter keeps is one <c>WriteValuesUpdate</c> writes a non-zero changedMask for,
    /// and a delta it drops is one the writer would reduce to a bare zero mask. This is what the
    /// hand-written list could not promise, and the reason the two drifted apart seven times.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FilterAndWriter_AgreeOnWhatIsEmpty(bool populated)
    {
        var state = SessionWithPlayer();
        var update = new ObjectUpdate(Creature, UpdateTypeModern.Values, null!);
        if (populated)
            update.UnitData.ShapeshiftForm = 17;

        bool filterKeeps = HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder
            .HasAnyValuesDelta(update, state);

        var builder = new HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder(update, state);
        var packet = new WorldPacket();
        builder.WriteToPacket(packet);

        // The Values blob is length-prefixed and sits at the tail of the entry. A delta with
        // nothing to say reduces to a 4-byte blob holding a zero changedMask, which is the
        // 13-byte body the client rejects once the enclosing packet is counted.
        ReadOnlySpan<byte> emptyBlob = [4, 0, 0, 0, 0, 0, 0, 0];
        var data = packet.GetDataSpan();
        bool writerWroteFields = !(data.Length >= emptyBlob.Length &&
                                   data[^emptyBlob.Length..].SequenceEqual(emptyBlob));

        Assert.Equal(populated, filterKeeps);
        Assert.Equal(filterKeeps, writerWroteFields);
    }
}
