using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using nietras.SeparatedValues;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The honor chat lines of the 4.4.2 client in front of a 3.3.5a server, sent as GlobalStrings
/// hotfixes (issue #483). Below Cataclysm's expansion level the client passes an honor gain to
/// COMBATLOG_HONORAWARD and COMBATLOG_HONORGAIN as a whole number (rva 1e15220, 1e15760), and its
/// own strings read it with a "%.2f": "You have been awarded 0.00 honor points." The row layout is
/// WoWDBDefs GlobalStrings; the CSV comes from scripts/build-global-strings-hotfix.py.
/// </summary>
public sealed class GlobalStringsHotfixTests
{
    [Fact]
    public void Hotfix_IsWrittenInTheClientsFieldOrder()
    {
        var record = GameData.GlobalStringsHotfix(3_400_002, 16021, "TAG", "%d x", 1);

        Assert.Equal(DB2Hash.GlobalStrings, record.TableHash);
        Assert.Equal(16021u, record.RecordId);
        Assert.Equal(3_400_002u, record.HotfixId);
        Assert.Equal(
            "54414700" + "25642078" + "00" + "01",   // BaseTag, TagText, Flags
            Convert.ToHexString(record.HotfixContent.GetData()));
    }

    [Fact]
    public void ShippedRows_TakeAWholeNumberInEveryLocale()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "CSV", "Hotfix", "GlobalStrings4.csv");
        using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromFile(path);
        var tags = new Dictionary<string, string>();
        foreach (var row in reader)
        {
            tags[row["BaseTag"].ToString()] = row["ID"].ToString();
            for (int i = 3; i < reader.Header.ColNames.Count; i++)
            {
                string text = row[i].ToString();
                Assert.DoesNotContain("%.2f", text);
                Assert.Equal(2, text.Split("%d").Length);
            }
        }

        Assert.Equal("16021", tags["COMBATLOG_HONORAWARD"]);
        Assert.Equal("15404", tags["COMBATLOG_HONORGAIN"]);
        Assert.Equal("19581", tags["COMBATLOG_HONORGAIN_NO_RANK"]);
        Assert.Contains("enUS", reader.Header.ColNames);
    }

    [Fact]
    public void Client_GetsTheTextOfItsLocale()
    {
        const uint hotfixId = 3_499_999;
        var enUS = GameData.GlobalStringsHotfix(hotfixId, 16021, "TAG", "You have been awarded %d honor points.", 1);
        var deDE = GameData.GlobalStringsHotfix(hotfixId, 16021, "TAG", "Ihr werdet mit %d Ehrenpunkten belohnt.", 1);
        GameData.LocalizedHotfixes[hotfixId] = new Dictionary<string, HotfixRecord> { ["enUS"] = enUS, ["deDE"] = deDE }.ToFrozenDictionary();
        try
        {
            Assert.Same(deDE, GameData.HotfixForLocale(enUS, "deDE"));
            Assert.Same(enUS, GameData.HotfixForLocale(enUS, "enUS"));
            // A locale the CSV has no column for, or none known yet, keeps the enUS text.
            Assert.Same(enUS, GameData.HotfixForLocale(enUS, "xxXX"));
            Assert.Same(enUS, GameData.HotfixForLocale(enUS, null));
        }
        finally
        {
            GameData.LocalizedHotfixes.TryRemove(hotfixId, out _);
        }
    }
}
