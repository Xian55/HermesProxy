using System;
using System.Collections.Frozen;
using System.IO;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads SMSG_WORLD_SERVER_INFO as a u32 difficulty, then five bits: IsTournamentRealm,
/// XRealmPvpAlert and the presence of the three optionals, as TrinityCore's 3.4.3 writer sends it. Sent with
/// IsTournamentRealm as a byte, the client took that byte as the flags and never saw InstanceGroupSize (#361).
/// </summary>
[Collection("V343ValuesFilter")]
public class WorldServerInfoTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static FrozenDictionary<uint, (uint DifficultyId, uint MaxPlayers)> ShippedDifficulties
        => GameData.ParseMapDifficulties(Path.Combine("CSV", "MapDifficulty3.csv"));

    private static WorldServerInfo Packet(byte tournament = 0) => new()
    {
        DifficultyID = 1,
        IsTournamentRealm = tournament,
        InstanceGroupSize = 5,
    };

    private static string ViaWrite(WorldServerInfo packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    private static string ViaSpan(WorldServerInfo packet)
    {
        var buffer = new byte[packet.MaxSize];
        int written = packet.WriteToSpan(buffer);
        Assert.True(written >= 0);
        return Convert.ToHexString(buffer.AsSpan(0, written));
    }

    [Theory]
    // Bits tournament, xrealm, level, money, size: 0000 1 + 3 padding bits.
    [InlineData(0, "01000000" + "08" + "05000000")]
    [InlineData(1, "01000000" + "88" + "05000000")]
    public void Write_OnV343_SendsTheTournamentAsTheFirstBit(byte tournament, string expected)
    {
        WorldServerInfo.ForceV343ForTests = true;
        try
        {
            Assert.Equal(expected, ViaWrite(Packet(tournament)));
        }
        finally
        {
            WorldServerInfo.ForceV343ForTests = null;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void WriteToSpan_OnV343_MatchesWrite(byte tournament)
    {
        WorldServerInfo.ForceV343ForTests = true;
        try
        {
            Assert.Equal(ViaWrite(Packet(tournament)), ViaSpan(Packet(tournament)));
        }
        finally
        {
            WorldServerInfo.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_BeforeV343_KeepsTheTournamentByte()
    {
        WorldServerInfo.ForceV343ForTests = false;
        try
        {
            // Bits xrealm, level, money, size: 0001 + 4 padding bits.
            Assert.Equal("01000000" + "00" + "10" + "05000000", ViaWrite(Packet()));
        }
        finally
        {
            WorldServerInfo.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void ParseMapDifficulties_KeepsEachInstancesFirstDifficulty()
    {
        var difficulties = ShippedDifficulties;

        Assert.Equal((1u, 5u), difficulties[36]);     // Deadmines
        Assert.Equal((9u, 40u), difficulties[409]);   // Molten Core
        Assert.Equal((148u, 20u), difficulties[309]); // Zul'Gurub
        Assert.Equal((3u, 10u), difficulties[249]);   // Onyxia's Lair: 10, 25 and the old 40
        Assert.Equal((3u, 10u), difficulties[568]);   // Zul'Aman, without the stray difficulty 0 row
        Assert.Equal((3u, 10u), difficulties[649]);   // Trial of the Crusader
        Assert.False(difficulties.ContainsKey(571));  // Northrend
        Assert.False(difficulties.ContainsKey(607));  // Strand of the Ancients
    }

    [Theory]
    // What a native server sends on each: Wrathion captures for 530, 571, 607, 409 and 649.
    [InlineData(0u, 0u, 0u)]
    [InlineData(530u, 0u, 0u)]
    [InlineData(571u, 0u, 0u)]
    [InlineData(607u, 0u, 0u)]
    [InlineData(36u, 1u, 5u)]
    [InlineData(409u, 9u, 40u)]
    [InlineData(649u, 3u, 10u)]
    public void ForMap_OnV343_SendsTheMapsDifficultyAndAlwaysASize(uint mapId, uint difficulty, uint groupSize)
    {
        var shipped = GameData.DefaultInstanceDifficulty;
        GameData.DefaultInstanceDifficulty = ShippedDifficulties;
        WorldServerInfo.ForceV343ForTests = true;
        try
        {
            var info = WorldServerInfo.ForMap(mapId);

            Assert.Equal(difficulty, info.DifficultyID);
            Assert.Equal(groupSize, info.InstanceGroupSize);
        }
        finally
        {
            WorldServerInfo.ForceV343ForTests = null;
            GameData.DefaultInstanceDifficulty = shipped;
        }
    }

    [Theory]
    [InlineData(0u, 0u, null)]
    [InlineData(1u, 0u, null)]
    [InlineData(571u, 1u, 5u)]
    [InlineData(409u, 1u, 5u)]
    public void ForMap_BeforeV343_KeepsTheMapIdGuess(uint mapId, uint difficulty, uint? groupSize)
    {
        WorldServerInfo.ForceV343ForTests = false;
        try
        {
            var info = WorldServerInfo.ForMap(mapId);

            Assert.Equal(difficulty, info.DifficultyID);
            Assert.Equal(groupSize, info.InstanceGroupSize);
        }
        finally
        {
            WorldServerInfo.ForceV343ForTests = null;
        }
    }
}
