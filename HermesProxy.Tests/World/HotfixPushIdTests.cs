using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

using nietras.SeparatedValues;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The client keeps the hotfixes it was sent under their push ids, and asks again only for a push
/// id it lacks or whose unique id has changed. ItemExtendedCost, CurrencyTypes and GlobalStrings
/// numbered their push ids by CSV row, and the unique id was the push id. So one push id named a
/// different record after the shipped rows changed, or when a 4.4.2 client moved between a 3.3.5a
/// and a 4.3.4 server, and the client kept its cached copy without asking.
/// </summary>
public sealed class HotfixPushIdTests
{
    private static string HotfixCsv(string file) => Path.Combine(AppContext.BaseDirectory, "CSV", "Hotfix", file);

    private static List<HotfixRecord> ItemExtendedCostRecords(string file)
    {
        var records = new List<HotfixRecord>();
        int[] values = new int[GameData.ItemExtendedCostColumns];
        foreach (string line in File.ReadLines(HotfixCsv(file)).Skip(1))
        {
            string[] cells = line.Split(',');
            for (int i = 0; i < values.Length; i++)
                values[i] = int.Parse(cells[i]);
            records.Add(GameData.ItemExtendedCostHotfix(values));
        }
        return records;
    }

    private static List<HotfixRecord> CurrencyTypesRecords(string file)
    {
        var records = new List<HotfixRecord>();
        using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromFile(HotfixCsv(file));
        var columns = new string[GameData.CurrencyTypesColumns];
        foreach (var row in reader)
        {
            for (int i = 0; i < columns.Length; i++)
                columns[i] = row[i].ToString();
            records.Add(GameData.CurrencyTypesHotfix(columns));
        }
        return records;
    }

    private static List<HotfixRecord> GlobalStringsRecords(string file)
    {
        var records = new List<HotfixRecord>();
        using var reader = Sep.Reader(o => o with { HasHeader = true, Unescape = true }).FromFile(HotfixCsv(file));
        int enUS = reader.Header.IndexOf("enUS");
        foreach (var row in reader)
            records.Add(GameData.GlobalStringsHotfix(row[0].Parse<uint>(), row[1].ToString(), row[enUS].ToString(), row[2].Parse<byte>()));
        return records;
    }

    [Theory]
    [InlineData("ItemExtendedCost3.csv", GameData.HotfixItemExtendedCostBegin)]
    [InlineData("ItemExtendedCost4.csv", GameData.HotfixItemExtendedCostBegin)]
    [InlineData("ItemExtendedCostCata4.csv", GameData.HotfixItemExtendedCostBegin)]
    [InlineData("CurrencyTypesCata4.csv", GameData.HotfixCurrencyTypesBegin)]
    [InlineData("GlobalStrings4.csv", GameData.HotfixGlobalStringsBegin)]
    public void ShippedRows_GetThePushIdOfTheirRecordId(string file, uint begin)
    {
        var records = file.StartsWith("ItemExtendedCost") ? ItemExtendedCostRecords(file)
            : file.StartsWith("CurrencyTypes") ? CurrencyTypesRecords(file)
            : GlobalStringsRecords(file);

        Assert.NotEmpty(records);
        Assert.All(records, r => Assert.Equal(begin + r.RecordId, r.HotfixId));
        Assert.Equal(records.Count, records.Select(r => r.HotfixId).Distinct().Count());
    }

    /// <summary>
    /// A 4.4.2 client on a 3.3.5a server is sent ItemExtendedCost4.csv, on a 4.3.4 server
    /// ItemExtendedCostCata4.csv, and keeps one cache for both. A push id with the same unique id
    /// must therefore mean the same record and content in both.
    /// </summary>
    [Fact]
    public void ItemExtendedCost_ThePushAndUniqueIdOfOneServer_NeverNameTheOtherServersContent()
    {
        var wotlk = ItemExtendedCostRecords("ItemExtendedCost4.csv").ToDictionary(r => r.HotfixId);
        var cata = ItemExtendedCostRecords("ItemExtendedCostCata4.csv").ToDictionary(r => r.HotfixId);

        int shared = 0;
        foreach (var (pushId, cataRecord) in cata)
        {
            if (!wotlk.TryGetValue(pushId, out var wotlkRecord))
                continue;
            shared++;
            Assert.Equal(wotlkRecord.RecordId, cataRecord.RecordId);
            bool sameContent = wotlkRecord.HotfixContent.GetData().AsSpan().SequenceEqual(cataRecord.HotfixContent.GetData());
            Assert.Equal(sameContent, GameData.ContentUniqueId(wotlkRecord) == GameData.ContentUniqueId(cataRecord));
        }
        Assert.NotEqual(0, shared);
    }

    [Fact]
    public void HotfixIdForRecord_ARecordIdPastTheTablesRange_Throws()
    {
        Assert.Equal(GameData.HotfixGlobalStringsBegin + 99_999,
            GameData.HotfixIdForRecord(GameData.HotfixGlobalStringsBegin, 99_999, DB2Hash.GlobalStrings));
        Assert.Throws<InvalidDataException>(() =>
            GameData.HotfixIdForRecord(GameData.HotfixItemExtendedCostBegin, GameData.HotfixIdRangeSize, DB2Hash.ItemExtendedCost));
    }

    private static HotfixRecord Record(uint recordId, byte content, DB2Hash table = DB2Hash.ItemExtendedCost,
        HotfixStatus status = HotfixStatus.Valid)
    {
        var record = new HotfixRecord { TableHash = table, RecordId = recordId, Status = status, HotfixId = 1, UniqueId = 1 };
        record.HotfixContent.WriteUInt8(content);
        return record;
    }

    [Fact]
    public void ContentUniqueId_FollowsTheContentAndNothingElse()
    {
        uint id = GameData.ContentUniqueId(Record(386, 7));

        Assert.Equal(id, GameData.ContentUniqueId(Record(386, 7)));
        var otherPushId = Record(386, 7);
        otherPushId.HotfixId = 2;
        Assert.Equal(id, GameData.ContentUniqueId(otherPushId));

        Assert.NotEqual(id, GameData.ContentUniqueId(Record(386, 8)));
        Assert.NotEqual(id, GameData.ContentUniqueId(Record(387, 7)));
        Assert.NotEqual(id, GameData.ContentUniqueId(Record(386, 7, DB2Hash.CurrencyTypes)));
        Assert.NotEqual(id, GameData.ContentUniqueId(Record(386, 7, status: HotfixStatus.RecordRemoved)));
        Assert.NotEqual(0u, id);
    }

    /// <summary>The value FNV-1a gives the bytes, so it cannot drift between processes or releases.</summary>
    [Fact]
    public void ContentUniqueId_IsTheSameInEveryProcess()
    {
        // Table 0, record 0, status 0 and no content: thirteen zero bytes, the content length included.
        var record = new HotfixRecord { TableHash = 0, RecordId = 0, Status = 0 };
        Assert.Equal(Fnv1a(new byte[13]), GameData.ContentUniqueId(record));
    }

    private static uint Fnv1a(byte[] bytes)
    {
        uint hash = 2_166_136_261;
        foreach (byte b in bytes)
            hash = (hash ^ b) * 16_777_619;
        return hash;
    }

    [Fact]
    public void AssignContentUniqueIds_GivesEveryLocaleOneValue_ThatAnyLocalesTextChanges()
    {
        var hotfixes = new ConcurrentDictionary<uint, HotfixRecord>();
        var localized = new ConcurrentDictionary<uint, FrozenDictionary<string, HotfixRecord>>();

        uint Assign(string german)
        {
            var enUS = GameData.GlobalStringsHotfix(16021, "TAG", "%d honor", 1);
            var deDE = GameData.GlobalStringsHotfix(16021, "TAG", german, 1);
            var plain = Record(386, 7);
            plain.HotfixId = GameData.HotfixItemExtendedCostBegin + 386;
            hotfixes[enUS.HotfixId] = enUS;
            hotfixes[plain.HotfixId] = plain;
            localized[enUS.HotfixId] = new Dictionary<string, HotfixRecord> { ["enUS"] = enUS, ["deDE"] = deDE }.ToFrozenDictionary();

            GameData.AssignContentUniqueIds(hotfixes, localized);

            Assert.Equal(enUS.UniqueId, deDE.UniqueId);
            Assert.Equal(GameData.ContentUniqueId(plain), plain.UniqueId);
            Assert.NotEqual(plain.HotfixId, plain.UniqueId);
            return enUS.UniqueId;
        }

        uint before = Assign("%d Ehre");
        Assert.Equal(before, Assign("%d Ehre"));
        Assert.NotEqual(before, Assign("%d Ehrenpunkte"));
    }
}
