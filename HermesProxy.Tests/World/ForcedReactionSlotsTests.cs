using System;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Forced reactions for a client that keeps them in its player object (4.4.2), fed from a legacy
/// server that sends the whole list each time.
///
/// What a slot does when a faction comes and goes is TrinityCore cata_classic's
/// <c>Player::SetVisibleForcedReaction</c> / <c>RemoveVisibleForcedReaction</c>; the bytes are its
/// <c>ZonePlayerForcedReaction::WriteCreate</c> / <c>WriteUpdate</c>.
/// </summary>
public sealed class ForcedReactionSlotsTests
{
    private const int Stormwind = 72;
    private const int Ironforge = 47;
    private const int Darnassus = 69;

    private const int Hostile = 1;
    private const int Unfriendly = 2;
    private const int Friendly = 4;

    private static ForcedReaction Forced(int faction, int reaction) => new() { Faction = faction, Reaction = reaction };

    private static uint Apply(ForcedReaction[] slots, params ForcedReaction[] wanted) => ForcedReactionSlots.Apply(slots, wanted);

    [Fact]
    public void TheFirstOne_TakesTheFirstSlot()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];

        uint changed = Apply(slots, Forced(Stormwind, Unfriendly));

        Assert.Equal(0b1u, changed);
        Assert.Equal(Stormwind, slots[0].Faction);
        Assert.Equal(Unfriendly, slots[0].Reaction);
        Assert.Equal(0, slots[1].Faction);
    }

    [Fact]
    public void TheSameListAgain_ChangesNothing()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        Apply(slots, Forced(Stormwind, Unfriendly), Forced(Ironforge, Hostile));

        Assert.Equal(0u, Apply(slots, Forced(Stormwind, Unfriendly), Forced(Ironforge, Hostile)));
        // the order of the server's list is not the order of the slots
        Assert.Equal(0u, Apply(slots, Forced(Ironforge, Hostile), Forced(Stormwind, Unfriendly)));
    }

    [Fact]
    public void AnotherFaction_TakesTheNextSlotAndLeavesTheFirstAlone()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        Apply(slots, Forced(Stormwind, Unfriendly));

        uint changed = Apply(slots, Forced(Stormwind, Unfriendly), Forced(Ironforge, Hostile));

        Assert.Equal(0b10u, changed);
        Assert.Equal(Stormwind, slots[0].Faction);
        Assert.Equal(Ironforge, slots[1].Faction);
        Assert.Equal(Hostile, slots[1].Reaction);
    }

    [Fact]
    public void OneThatEnds_LeavesItsSlotEmptyAndMovesNoOther()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        Apply(slots, Forced(Stormwind, Unfriendly), Forced(Ironforge, Hostile));

        uint changed = Apply(slots, Forced(Ironforge, Hostile));

        Assert.Equal(0b01u, changed);
        Assert.Equal(0, slots[0].Faction);
        Assert.Equal(0, slots[0].Reaction);
        Assert.Equal(Ironforge, slots[1].Faction);
    }

    [Fact]
    public void ANewOne_TakesTheFirstEmptySlot()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        Apply(slots, Forced(Stormwind, Unfriendly), Forced(Ironforge, Hostile));
        Apply(slots, Forced(Ironforge, Hostile));

        uint changed = Apply(slots, Forced(Ironforge, Hostile), Forced(Darnassus, Friendly));

        Assert.Equal(0b01u, changed);
        Assert.Equal(Darnassus, slots[0].Faction);
        Assert.Equal(Friendly, slots[0].Reaction);
    }

    [Fact]
    public void ANewStandingForAFaction_StaysInItsSlot()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        Apply(slots, Forced(Stormwind, Unfriendly), Forced(Ironforge, Hostile));

        uint changed = Apply(slots, Forced(Stormwind, Unfriendly), Forced(Ironforge, Friendly));

        Assert.Equal(0b10u, changed);
        Assert.Equal(Ironforge, slots[1].Faction);
        Assert.Equal(Friendly, slots[1].Reaction);
    }

    [Fact]
    public void OneSwappedForAnother_ReusesTheSlotInOneChange()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        Apply(slots, Forced(Stormwind, Unfriendly));

        uint changed = Apply(slots, Forced(Ironforge, Hostile));

        Assert.Equal(0b1u, changed);
        Assert.Equal(Ironforge, slots[0].Faction);
        Assert.Equal(Hostile, slots[0].Reaction);
    }

    [Fact]
    public void AnEmptyList_EmptiesEverySlotThatWasTaken()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        Apply(slots, Forced(Stormwind, Unfriendly), Forced(Ironforge, Hostile));

        Assert.Equal(0b11u, Apply(slots));
        Assert.All(slots, slot => Assert.Equal(0, slot.Faction));
        Assert.Equal(0u, Apply(slots));
    }

    [Fact]
    public void MoreThanTheClientHolds_KeepsWhatFits()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        var wanted = new ForcedReaction[ForcedReactionSlots.Count + 3];
        for (int i = 0; i < wanted.Length; i++)
            wanted[i] = Forced(100 + i, Friendly);

        uint changed = Apply(slots, wanted);

        Assert.Equal(uint.MaxValue, changed);
        for (int i = 0; i < ForcedReactionSlots.Count; i++)
            Assert.Equal(100 + i, slots[i].Faction);
        Assert.Equal(0u, Apply(slots, wanted));
    }

    [Fact]
    public void FactionZero_IsNotAForcedReaction()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];

        Assert.Equal(0u, Apply(slots, Forced(0, Friendly)));
        Assert.Equal(0, slots[0].Reaction);
    }

    [Fact]
    public void WriteCreate_IsEverySlotAsFactionThenReaction()
    {
        var slots = new ForcedReaction[ForcedReactionSlots.Count];
        slots[0] = Forced(Stormwind, Unfriendly);
        slots[31] = Forced(Ironforge, Hostile);
        using var packet = new WorldPacket();

        ForcedReactionSlots.WriteCreate(packet, slots);

        byte[] bytes = packet.GetData();
        Assert.Equal(ForcedReactionSlots.Count * 8, bytes.Length);
        Assert.Equal(Stormwind, BitConverter.ToInt32(bytes, 0));
        Assert.Equal(Unfriendly, BitConverter.ToInt32(bytes, 4));
        Assert.Equal(0, BitConverter.ToInt32(bytes, 8));
        Assert.Equal(Ironforge, BitConverter.ToInt32(bytes, 31 * 8));
        Assert.Equal(Hostile, BitConverter.ToInt32(bytes, 31 * 8 + 4));
    }

    [Fact]
    public void WriteCreate_WithNoSlots_IsAllZero()
    {
        using var packet = new WorldPacket();

        ForcedReactionSlots.WriteCreate(packet, null);

        byte[] bytes = packet.GetData();
        Assert.Equal(ForcedReactionSlots.Count * 8, bytes.Length);
        Assert.All(bytes, b => Assert.Equal(0, b));
    }

    [Fact]
    public void WriteUpdate_IsAThreeBitMaskThenBothFields()
    {
        using var packet = new WorldPacket();

        ForcedReactionSlots.WriteUpdate(packet, Forced(Stormwind, Unfriendly));

        // changesMask: the entry, FactionID, Reaction; flushed to a byte
        Assert.Equal("E0" + "48000000" + "02000000", Convert.ToHexString(packet.GetData()));
    }

    [Fact]
    public void WriteUpdate_OfAnEmptiedSlot_StillWritesBothFields()
    {
        using var packet = new WorldPacket();

        ForcedReactionSlots.WriteUpdate(packet, default);

        Assert.Equal("E0" + "00000000" + "00000000", Convert.ToHexString(packet.GetData()));
    }
}
