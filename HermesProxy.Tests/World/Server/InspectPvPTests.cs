using System;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads SMSG_INSPECT_PVP as the player GUID, a u32 bracket count, a byte whose top two
/// bits count the arena teams, the brackets, then the teams. It sizes its bracket array from the u32
/// unchecked, so the old 3+2-bit count byte, read as the low byte of that u32 with three GUID bytes of the
/// first team above it, asked for 54190428960 bytes and the client exited (#363).
/// </summary>
[Collection("V343ValuesFilter")]
public class InspectPvPTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private const string EmptyGuid = "0000";
    private const string EmptyTeam = EmptyGuid + "00000000" + "00000000" + "00000000" + "00000000" + "00000000";

    private static InspectPvP Packet(ArenaTeamInspectData? first = null) => new()
    {
        ArenaTeams = [first ?? new ArenaTeamInspectData(), new ArenaTeamInspectData(), new ArenaTeamInspectData()],
    };

    private static string ViaWrite(InspectPvP packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    private static string ViaSpan(InspectPvP packet)
    {
        var buffer = new byte[packet.MaxSize];
        int written = packet.WriteToSpan(buffer);
        Assert.True(written >= 0);
        return Convert.ToHexString(buffer.AsSpan(0, written));
    }

    [Fact]
    public void Write_OnV343_SendsAU32BracketCountThenTheTeamCountInTheTopTwoBits()
    {
        InspectPvP.ForceV343ForTests = true;
        try
        {
            // 0xC0 is three teams; TrinityCore's InspectPvPResult notes 192 "from sniffs".
            Assert.Equal(EmptyGuid + "00000000" + "C0" + EmptyTeam + EmptyTeam + EmptyTeam, ViaWrite(Packet()));
        }
        finally
        {
            InspectPvP.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_OnV343_KeepsTheBracketCountZeroWhateverTheTeamGuid()
    {
        InspectPvP.ForceV343ForTests = true;
        try
        {
            var team = new ArenaTeamInspectData { TeamGuid = WowGuid128.Create(HighGuidType703.ArenaTeam, 42), TeamRating = 1935 };

            string data = ViaWrite(Packet(team));

            Assert.Equal("00000000", data.Substring(EmptyGuid.Length, 8));
            Assert.Equal("C0", data.Substring(EmptyGuid.Length + 8, 2));
        }
        finally
        {
            InspectPvP.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void WriteToSpan_OnV343_MatchesWrite()
    {
        InspectPvP.ForceV343ForTests = true;
        try
        {
            var team = new ArenaTeamInspectData { TeamGuid = WowGuid128.Create(HighGuidType703.ArenaTeam, 42), TeamRating = 1935 };
            Assert.Equal(ViaWrite(Packet(team)), ViaSpan(Packet(team)));
        }
        finally
        {
            InspectPvP.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_BeforeV343_KeepsTheThreeAndTwoBitCounts()
    {
        InspectPvP.ForceV343ForTests = false;
        try
        {
            // 000 11 000: no brackets, three teams.
            Assert.Equal(EmptyGuid + "18" + EmptyTeam + EmptyTeam + EmptyTeam, ViaWrite(Packet()));
        }
        finally
        {
            InspectPvP.ForceV343ForTests = null;
        }
    }
}
