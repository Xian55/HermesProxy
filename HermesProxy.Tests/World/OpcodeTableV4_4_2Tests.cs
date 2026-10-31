using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The 4.4.2.60895 opcode table: the 3.4.3.54261 names with the 4.4.2 client's values, then the rest
/// of the client's own list.
/// </summary>
public class OpcodeTableV4_4_2Tests
{
    [Fact]
    public void FourByteOpcodesSplitIntoGroups()
    {
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V4_4_2_60895, out Opcode[][] groups, out _, out int opcodeSize));

        Assert.Equal(sizeof(uint), opcodeSize);
        Assert.True(groups.Length > 1);
    }

    // TrinityCore cata_classic Opcodes.h; the in-world ones also in docs/protocol/4.4.2.60895.
    [Theory]
    [InlineData(Opcode.CMSG_AUTH_SESSION, 0x3A0001u)]
    [InlineData(Opcode.CMSG_PING, 0x3A0004u)]
    [InlineData(Opcode.CMSG_ENUM_CHARACTERS, 0x390014u)]
    [InlineData(Opcode.SMSG_AUTH_CHALLENGE, 0x420000u)]
    [InlineData(Opcode.SMSG_ENTER_ENCRYPTED_MODE, 0x420001u)]
    [InlineData(Opcode.SMSG_COMPRESSED_PACKET, 0x42000Au)]
    [InlineData(Opcode.SMSG_AUTH_RESPONSE, 0x3B0001u)]
    [InlineData(Opcode.SMSG_UPDATE_OBJECT, 0x4B0000u)]
    // 4.4.2 has the 3.4.3 request (there OWNED, here OWNER at 0x350059) and a retail-style one of
    // the same name. Its auction house sends only the retail-style one.
    [InlineData(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS, 0x35005Fu)]
    [InlineData(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS, 0x350060u)]
    [InlineData(Opcode.CMSG_AUCTION_BROWSE_QUERY, 0x35005Cu)]
    [InlineData(Opcode.SMSG_AUCTION_LIST_BUCKETS_RESULT, 0x3B02E5u)]
    public void MapsBothWays(Opcode universal, uint current)
    {
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V4_4_2_60895, out Opcode[][] groups, out uint[] universalToCurrent, out _));

        Assert.Equal(current, universalToCurrent[(int)universal]);
        Assert.Equal(universal, groups[current >> 16][current & 0xFFFF]);
    }

    /// <summary>
    /// The table begins as the 3.4.3 one revalued, so that a packet both builds have takes the same
    /// handler: every 3.4.3 name is here, with 0 where the 4.4.2 client has no such opcode.
    /// </summary>
    [Fact]
    public void HasEvery3_4_3Name()
    {
        var names = Enum.GetNames<global::HermesProxy.World.Enums.V4_4_2_60895.Opcode>().ToHashSet();

        Assert.DoesNotContain(Enum.GetNames<global::HermesProxy.World.Enums.V3_4_3_54261.Opcode>(), name => !names.Contains(name));
    }

    /// <summary>
    /// And it ends with the rest of the client's own list. An opcode the table lacks is logged as
    /// MSG_NULL_ACTION with a number and dropped, whatever it was: the ghost's "Return to Graveyard"
    /// button was one. scripts/gen-opcode-table-442.py appends what is missing.
    /// </summary>
    [Fact]
    public void HasEveryNamedOpcodeOfTheClientsList()
    {
        var values = Enum.GetValues<global::HermesProxy.World.Enums.V4_4_2_60895.Opcode>().Select(o => (uint)o).ToHashSet();

        var missing = ClientList().Where(row => !values.Contains(row.Value)).Select(row => row.Name).ToList();

        Assert.True(missing.Count == 0,
            $"run scripts/gen-opcode-table-442.py: {missing.Count} missing, first {string.Join(", ", missing.Take(8))}");
    }

    /// <summary>
    /// What the table has and the list has not: the connection opcodes, which the list leaves out,
    /// and the talent data, which it has without a name.
    /// </summary>
    [Fact]
    public void HasNothingBesideTheListButTheConnectionOpcodes()
    {
        var listed = ClientList().Select(row => row.Value).ToHashSet();

        var others = Enum.GetValues<global::HermesProxy.World.Enums.V4_4_2_60895.Opcode>()
            .Distinct()
            .Where(o => (uint)o != 0 && !listed.Contains((uint)o))
            .Select(o => o.ToString())
            .Order();

        Assert.Equal(
            [
                "SMSG_AUTH_CHALLENGE",
                "SMSG_COMPRESSED_PACKET",
                "SMSG_CONNECT_TO",
                "SMSG_ENTER_ENCRYPTED_MODE",
                "SMSG_PONG",
                "SMSG_RESUME_COMMS",
                "SMSG_SUSPEND_COMMS",
                "SMSG_UPDATE_TALENT_DATA",
            ],
            others);
    }

    // docs/protocol/4.4.2.60895: "| NAME | 0xhex | decimal | ..." for each opcode the client has a name for
    private static List<(string Name, uint Value)> ClientList()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "HermesProxy.sln")))
            dir = dir.Parent;
        Assert.NotNull(dir);

        var rows = new List<(string, uint)>();
        foreach (string file in new[] { "cmsg-opcodes.md", "smsg-opcodes.md" })
        {
            foreach (string line in File.ReadLines(Path.Combine(dir.FullName, "docs", "protocol", "4.4.2.60895", file)))
            {
                Match row = Regex.Match(line, @"^\| (\w+) \| 0x[0-9a-f]+ \| (\d+) \|");
                if (row.Success)
                    rows.Add((row.Groups[1].Value, uint.Parse(row.Groups[2].Value)));
            }
        }
        return rows;
    }

    /// <summary>
    /// Every opcode both builds have reaches the same universal opcode, so a 4.4.2 packet takes the
    /// handler its 3.4.3 counterpart takes. Where several names share one opcode, the same one wins.
    /// </summary>
    [Fact]
    public void ResolvesEverySharedOpcodeLike3_4_3()
    {
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V3_4_3_54261, out Opcode[][] groups343, out uint[] toCurrent343, out _));
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V4_4_2_60895, out Opcode[][] groups442, out uint[] toCurrent442, out _));

        int shared = 0;
        for (int universal = 0; universal < toCurrent343.Length && universal < toCurrent442.Length; universal++)
        {
            uint current343 = toCurrent343[universal];
            uint current442 = toCurrent442[universal];
            if (current343 == 0 || current442 == 0)
                continue;

            Assert.Equal(groups343[0][current343], groups442[current442 >> 16][current442 & 0xFFFF]);
            shared++;
        }

        Assert.True(shared > 900, $"only {shared} opcodes shared");
    }
}
