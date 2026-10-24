using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HermesProxy.World;
using HermesProxy.World.Enums;
using nietras.SeparatedValues;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// A 4.3.4 server's weekly caps for Conquest and Valor Points, sent to the 4.4.2 client as
/// CurrencyTypes hotfixes (issue #483). Cataclysm Classic's own rows give both a season's total and
/// no weekly cap, and the client takes a weekly cap from its row alone: from the server's
/// SMSG_SETUP_CURRENCY only when the row has ComputedWeeklyMaximum (its tooltip, rva 224b370, and
/// the weekly maximum behind it, rva 205ea50). The row layout is TrinityCore cata_classic
/// CurrencyTypesLoadInfo; the CSV comes from scripts/build-currency-types-hotfix.py.
/// </summary>
public sealed class CurrencyTypesHotfixTests
{
    private const int ComputedWeeklyMaximum = 0x4;
    private const int UseTotalEarnedForEarned = 0x1;

    [Fact]
    public void Hotfix_IsWrittenInTheClientsFieldOrder()
    {
        string[] row = ["396", "Valor", "Points", "22", "463447", "3", "4", "5", "100000", "-1", "6", "7", "8", "1.5", "456", "1"];

        var record = GameData.CurrencyTypesHotfix(3_300_001, row);

        Assert.Equal(DB2Hash.CurrencyTypes, record.TableHash);
        Assert.Equal(396u, record.RecordId);
        Assert.Equal(3_300_001u, record.HotfixId);
        Assert.Equal(
            "56616C6F7200" + "506F696E747300"          // Name, Description
            + "16" + "57120700" + "03000000" + "04"     // CategoryID, InventoryIconFileID, SpellWeight, SpellCategory
            + "05000000" + "A0860100"                   // MaxQty, MaxEarnablePerWeek
            + "FF" + "06000000" + "07000000" + "08000000" // Quality, FactionID, AwardConditionID, MaxQtyWorldStateID
            + "0000C03F" + "C8010000" + "01000000",     // Unknown1154, Flags, FlagsB
            Convert.ToHexString(record.HotfixContent.GetData()));
    }

    [Fact]
    public void ShippedRows_HaveTheServersWeeklyCaps()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "CSV", "Hotfix", "CurrencyTypesCata4.csv");
        using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromFile(path);
        Assert.Equal(GameData.CurrencyTypesColumns, reader.Header.ColNames.Count);
        var rows = new Dictionary<string, Dictionary<string, string>>();
        foreach (var row in reader)
        {
            var columns = new Dictionary<string, string>();
            foreach (string name in reader.Header.ColNames)
                columns[name] = row[name].ToString();
            rows[columns["ID"]] = columns;
        }

        Assert.Equal(["390", "396", "483", "484"], rows.Keys.Order());
        // Conquest Points: as many a week as the server works out from the rating, and the season's
        // total the server tracks under "Season Total Earned".
        Assert.NotEqual(0, int.Parse(rows["390"]["Flags_0"]) & ComputedWeeklyMaximum);
        Assert.Equal(UseTotalEarnedForEarned, int.Parse(rows["390"]["Flags_1"]));
        // Valor Points: 1000 a week, in hundredths, and no season's total.
        Assert.Equal("100000", rows["396"]["MaxEarnablePerWeek"]);
        Assert.Equal(0, int.Parse(rows["396"]["Flags_0"]) & ComputedWeeklyMaximum);
        Assert.Equal("0", rows["396"]["Flags_1"]);
        // The rest is the client's own row.
        Assert.Equal("Conquest Points", rows["390"]["Name_lang"]);
        Assert.StartsWith("Earned as a reward for Arena or Rated Battlegrounds", rows["390"]["Description_lang"]);
    }
}
