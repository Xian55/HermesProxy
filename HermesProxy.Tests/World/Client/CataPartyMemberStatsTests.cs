using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Party member stats of a 4.3.4 server. The change mask is not the 3.3.5a one with more bits:
/// a WMO group id at 0x100 moves the position, the auras and the pet up a bit, the position has a
/// height, an aura block has a byte and a count around its mask and 16 bits of flags an aura.
/// Read the 3.3.5a way, a member's position was taken for auras and the packet ran short.
/// The two full states are packets a 4.3.4 server sent in a battleground, as logged when they
/// were misread; the rest is written out after TrinityCore 4.3.4 <c>PartyMemberState::Write</c>.
/// </summary>
public sealed class CataPartyMemberStatsTests
{
    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly WowGuid128 _member;

    public CataPartyMemberStatsTests()
    {
        _harness.SetActivePlayer(new WowGuid64(2));
        _member = _harness.AddKnownObject(new WowGuid64(0x21), ObjectType.Player);
    }

    // a received packet starts with its opcode, which the handlers never look at
    private static byte[] Wire(string hex) => [0, 0, .. Convert.FromHexString(hex.Replace("-", "").Replace(" ", ""))];

    private PartyMemberFullState FullState(string hex)
    {
        _harness.Deliver(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, Wire(hex), _harness.Client.HandlePartyMemberFullStateCata);
        return Assert.IsType<PartyMemberFullState>(_harness.ClientWire.Sent[^1].Packet);
    }

    private PartyMemberPartialState PartialState(string hexFromTheMask)
    {
        _harness.Deliver(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, Wire(hexFromTheMask),
            packet => _harness.Client.HandlePartyMemberPartialStateCata(packet, _member));
        return Assert.IsType<PartyMemberPartialState>(_harness.ClientWire.Sent[^1].Packet);
    }

    private const string LoggedFullState =
        "00-03-8F-20-F7-07-E8-7F-03-00-9E-1F-00-00-9E-1F-00-00-6A-22-6A-22-44-00-CD-0C-00-00-F1-05-C7-05-60-01-00-7F-00-00-00-00-00-00-00-07-00-00-00-E9-AD-00-00-1F-00-AB-22-01-00-19-00-7E-1C-00-00-53-00-35-00-00-00-00-00-00-00-00-00-00-00-16-F8-00-00-16-00-2B-F8-00-00-13-00-F5-C9-00-00-13-00-D7-34-01-00-53-00-05-00-00-00-22-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-08-00-00-00-00-00-00-00";

    private const string LoggedFullStateWithPowerType =
        "00-03-29-21-FF-07-E8-7F-03-00-8A-20-00-00-8A-20-00-00-03-64-00-64-00-44-00-CD-0C-00-00-F7-05-CB-05-5F-01-00-FF-00-00-00-00-00-00-00-08-00-00-00-F5-C9-00-00-1F-00-F8-06-00-00-1F-00-E9-AD-00-00-1F-00-AB-22-01-00-19-00-7E-1C-00-00-53-00-35-00-00-00-00-00-00-00-00-00-00-00-16-F8-00-00-16-00-2B-F8-00-00-13-00-D7-34-01-00-53-00-05-00-00-00-22-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-08-00-00-00-00-00-00-00";

    [Fact]
    public void FullState_AsLogged_ReadsToItsEnd()
    {
        PartyMemberFullState state = FullState(LoggedFullState);

        Assert.Equal(new WowGuid64(0x208F).To128(_harness.Session.GameState), state.MemberGuid);
        Assert.Equal((GroupMemberOnlineStatus)3, state.StatusFlags);
        Assert.Equal(8094, state.CurrentHealth);
        Assert.Equal(8094, state.MaxHealth);
        Assert.Equal(8810, state.CurrentPower);
        Assert.Equal(8810, state.MaxPower);
        Assert.Equal(68, state.Level);
        Assert.Equal(3277, state.ZoneID);                                   // Warsong Gulch
        Assert.Equal((1521, 1479, 352), (state.PositionX, state.PositionY, state.PositionZ));
        Assert.Equal([44521u, 74411u, 7294u, 63510u, 63531u, 51701u, 79063u], state.Auras.Select(a => a.SpellId));
        Assert.Null(state.Pet);                                             // the pet's auras are in the mask, empty
    }

    [Fact]
    public void FullState_AScalableAura_KeepsTheValuesOfTheEffectsItHas()
    {
        PartyMemberFullState state = FullState(LoggedFullState);

        // flags 0x53: effects 0 and 1, positive, scalable; three values on the wire
        PartyMemberAuraStates scalable = state.Auras[2];
        Assert.Equal([53f, 0f], scalable.Points);
        Assert.Equal(3u, scalable.ActiveFlags);
        Assert.Equal((ushort)(AuraFlagsModern.Positive | AuraFlagsModern.Scalable), scalable.AuraFlags);
        Assert.Equal([5f, 34f], state.Auras[6].Points);

        // flags 0x1F: three effects, no caster, positive
        Assert.Empty(state.Auras[0].Points);
        Assert.Equal(7u, state.Auras[0].ActiveFlags);
        Assert.Equal((ushort)(AuraFlagsModern.Positive | AuraFlagsModern.NoCaster), state.Auras[0].AuraFlags);
    }

    [Fact]
    public void FullState_WithAPowerType_ReadsItBetweenHealthAndPower()
    {
        PartyMemberFullState state = FullState(LoggedFullStateWithPowerType);

        Assert.Equal(8330, state.CurrentHealth);
        Assert.Equal(3, state.PowerType);                                   // energy
        Assert.Equal(100, state.CurrentPower);
        Assert.Equal(100, state.MaxPower);
        Assert.Equal((1527, 1483, 351), (state.PositionX, state.PositionY, state.PositionZ));
        Assert.Equal(8, state.Auras.Count);
    }

    [Fact]
    public void PartialState_HealthAndPosition_SkipsTheWmoGroup()
    {
        // mask 0x302: health, WMO group, position
        PartyMemberPartialState state = PartialState("02 03 00 00" + "D2 04 00 00" + "07 00" + "F1 05 C7 05 60 01");

        Assert.Equal(_member, state.AffectedGUID);
        Assert.Equal(1234u, state.CurrentHealth);
        Assert.Equal((1521, 1479, 352), (state.Position!.Value.X, state.Position.Value.Y, state.Position.Value.Z));
        Assert.Null(state.WmoGroupID);
        Assert.Null(state.Auras);
        Assert.Null(state.StatusFlags);
    }

    [Fact]
    public void PartialState_APet_ItsAurasAndTheSeatAfterIt()
    {
        // mask 0x3FF800: every pet field, the vehicle seat, the phases
        PartyMemberPartialState state = PartialState("00 F8 3F 00"
            + "44 33 22 11 00 00 40 F1"                 // pet GUID
            + "52 65 78 00"                             // "Rex"
            + "A1 02"                                   // model
            + "E8 03 00 00" + "D0 07 00 00"             // health 1000 of 2000
            + "02" + "64 00" + "64 00"                  // focus, 100 of 100
            + "00" + "01 00 00 00 00 00 00 00" + "40 00 00 00" + "88 13 00 00" + "11 00"   // one aura: 5000, effect 0, positive
            + "2A 00 00 00"                             // seat
            + "08 00 00 00" + "02 00 00 00" + "A9 00 AA 00");   // two phases

        Assert.NotNull(state.Pet);
        Assert.Equal("Rex", state.Pet.NewPetName);
        Assert.Equal(673u, state.Pet.DisplayID);
        Assert.Equal(1000u, state.Pet.Health);
        Assert.Equal(2000u, state.Pet.MaxHealth);
        Assert.Equal(5000u, Assert.Single(state.Pet.Auras).SpellId);
        Assert.Equal(42u, state.VehicleSeatRecID);
        Assert.Null(state.Phase);
    }

    [Fact]
    public void PartialState_PetAurasWithoutAPet_MakeNoPet()
    {
        // mask 0x80001: status, and an empty pet aura block
        PartyMemberPartialState state = PartialState("01 00 08 00" + "01 00" + "00" + "00 00 00 00 00 00 00 00" + "00 00 00 00");

        Assert.Equal((ushort)1, state.StatusFlags);
        Assert.Null(state.Pet);
    }

    [Theory]
    [InlineData(GroupUpdateFlagCata.CurrentHealth | GroupUpdateFlagCata.Position, GroupUpdateFlagTBC.CurrentHealth | GroupUpdateFlagTBC.Position)]
    [InlineData(GroupUpdateFlagCata.MaxPower, GroupUpdateFlagTBC.MaxPower)]
    public void ThrottleFlags_OfHealthPowerAndPosition_AreTheOnesTheThrottleHoldsBack(GroupUpdateFlagCata cata, GroupUpdateFlagTBC expected)
    {
        Assert.Equal(expected, HermesProxy.World.Client.WorldClient.ThrottleFlagsOf(cata));
    }

    [Theory]
    [InlineData(GroupUpdateFlagCata.Auras)]
    [InlineData(GroupUpdateFlagCata.WmoGroupId)]
    [InlineData(GroupUpdateFlagCata.PetGuid)]
    [InlineData(GroupUpdateFlagCata.VehicleSeat)]
    [InlineData(GroupUpdateFlagCata.Phase)]
    public void ThrottleFlags_OfAnythingElse_CarryABitThatIsNeverHeldBack(GroupUpdateFlagCata cata)
    {
        const GroupUpdateFlagTBC heldBack = GroupUpdateFlagTBC.CurrentHealth | GroupUpdateFlagTBC.MaxHealth
            | GroupUpdateFlagTBC.PowerType | GroupUpdateFlagTBC.CurrentPower | GroupUpdateFlagTBC.MaxPower | GroupUpdateFlagTBC.Position;

        GroupUpdateFlagTBC flags = HermesProxy.World.Client.WorldClient.ThrottleFlagsOf(cata | GroupUpdateFlagCata.CurrentHealth);

        Assert.NotEqual((GroupUpdateFlagTBC)0, flags & ~heldBack);
    }
}
