using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Legacy 3.3.5a and V3_4_3 number lfg::LfgUpdateType differently, and V3_4_3 carries the value
/// in LFGUpdateStatus.Reason rather than SubType. Forwarding the raw legacy byte as SubType left
/// the Dungeon Finder panel stuck: Leave stayed greyed out after a queue was dropped, and the
/// in-dungeon LFG eye never appeared once a group had formed.
/// </summary>
public class LfgUpdateTypesTests
{
    [Theory]
    [InlineData(0, 0)]    // DEFAULT
    [InlineData(1, 1)]    // LEADER_UNK1
    [InlineData(4, 4)]    // ROLECHECK_ABORTED
    [InlineData(5, 6)]    // JOIN_QUEUE
    [InlineData(6, 7)]    // ROLECHECK_FAILED
    [InlineData(7, 8)]    // REMOVED_FROM_QUEUE
    [InlineData(8, 9)]    // PROPOSAL_FAILED
    [InlineData(9, 10)]   // PROPOSAL_DECLINED
    [InlineData(10, 11)]  // GROUP_FOUND
    [InlineData(12, 13)]  // ADDED_TO_QUEUE
    [InlineData(13, 15)]  // PROPOSAL_BEGIN
    [InlineData(14, 16)]  // UPDATE_STATUS
    [InlineData(15, 17)]  // GROUP_MEMBER_OFFLINE
    [InlineData(16, 18)]  // GROUP_DISBAND_UNK16
    public void ToModern_MapsKnownLegacyUpdateTypes(byte legacy, byte expected)
    {
        Assert.Equal(expected, LfgUpdateTypes.ToModern(legacy));
    }

    [Theory]
    [InlineData(2)]  // LFG_UPDATETYPE_LEAVE_RAIDBROWSER, legacy-only
    [InlineData(3)]  // LFG_UPDATETYPE_JOIN_RAIDBROWSER, legacy-only
    [InlineData(200)]
    public void ToModern_WithNoModernCounterpart_FallsBackToDefault(byte legacy)
    {
        Assert.Equal(LfgUpdateTypes.ModernDefault, LfgUpdateTypes.ToModern(legacy));
    }

    [Fact]
    public void ToModern_ShiftsTheRenumberedBlockUpByOne()
    {
        // The 5..10 block is exactly where legacy and modern diverge by one; a mistake here
        // silently turns "removed from queue" into "role check failed".
        for (byte legacy = 5; legacy <= 10; legacy++)
            Assert.Equal((byte)(legacy + 1), LfgUpdateTypes.ToModern(legacy));
    }

    [Fact]
    public void IsStillLfgJoined_IsFalseOnlyForRemovedFromQueue()
    {
        Assert.False(LfgUpdateTypes.IsStillLfgJoined(LfgUpdateTypes.ModernRemovedFromQueue));
        Assert.True(LfgUpdateTypes.IsStillLfgJoined(LfgUpdateTypes.ModernGroupFound));
        Assert.True(LfgUpdateTypes.IsStillLfgJoined(LfgUpdateTypes.ModernUpdateStatus));
        Assert.True(LfgUpdateTypes.IsStillLfgJoined(LfgUpdateTypes.ModernAddedToQueue));
    }

    [Fact]
    public void SubTypeIsAQueueTypeNotAnUpdateType()
    {
        // TC 3.4.3 writes lfg::LFG_QUEUE_DUNGEON into SubType and the update type into Reason.
        // LFG_QUEUE_DUNGEON is 1, not 0 — the 3.4.3.54261 reference sniff
        // (World_5_man_party_join_dungeon_finder_parsed.txt:596176) shows SubType 1, Reason 6.
        // Pinning this stops a future edit from putting the update type back in SubType.
        Assert.Equal(1, LfgUpdateTypes.ModernQueueDungeon);
        Assert.NotEqual(LfgUpdateTypes.ModernQueueDungeon, LfgUpdateTypes.ModernGroupFound);
    }
}
