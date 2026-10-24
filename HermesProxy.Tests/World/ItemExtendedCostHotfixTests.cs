using System;
using System.IO;
using System.Linq;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// A 3.3.5a server's vendor prices, sent to the 3.4.3 and 4.4.2 clients as ItemExtendedCost
/// hotfixes where their own rows differ (row 386, the Alliance Battle Standard: 15300 honor on
/// 3.3.5a, 7650 on the 3.4.3 client). The row layout is TrinityCore wotlk_classic and
/// cata_classic ItemExtendedCostLoadInfo; the CSVs come from scripts/build-item-extended-cost-hotfix.py.
/// </summary>
public sealed class ItemExtendedCostHotfixTests
{
    [Fact]
    public void Hotfix_IsWrittenInTheClientsFieldOrder()
    {
        int[] row = [386, 0x0102, 3, 4, 5, 0x06070809, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30];

        var record = GameData.ItemExtendedCostHotfix(3_200_001, row);

        Assert.Equal(DB2Hash.ItemExtendedCost, record.TableHash);
        Assert.Equal(386u, record.RecordId);
        Assert.Equal(3_200_001u, record.HotfixId);
        Assert.Equal(
            "0201" + "03" + "04" + "05" + "09080706" + "0A"           // rating, bracket, flags, faction, reputation, achievement
            + "0B000000" + "0C000000" + "0D000000" + "0E000000" + "0F000000" // ItemID, i32
            + "1000" + "1100" + "1200" + "1300" + "1400"              // ItemCount, u16
            + "1500" + "1600" + "1700" + "1800" + "1900"              // CurrencyID, u16
            + "1A000000" + "1B000000" + "1C000000" + "1D000000" + "1E000000", // CurrencyCount, u32
            Convert.ToHexString(record.HotfixContent.GetData()));
    }

    [Theory]
    [InlineData("ItemExtendedCost3.csv", "386", 15300u)]
    [InlineData("ItemExtendedCost4.csv", "386", 1_530_000u)] // the 4.4.2 client counts honor in hundredths
    // 4.3.4's 175 honor at the arena vendors, a row the 4.4.2 client lacks; 4.3.4 counts in hundredths too
    [InlineData("ItemExtendedCostCata4.csv", "2285", 17_500u)]
    public void ShippedRows_CarryTheServersPrice(string file, string id, uint honor)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "CSV", "Hotfix", file);
        string[] header = File.ReadLines(path).First().Split(',');
        string[] row = File.ReadLines(path).Select(l => l.Split(',')).Single(c => c[0] == id);

        Assert.Equal(GameData.ItemExtendedCostColumns, header.Length);
        Assert.Equal(((uint)Currency.HonorPoints).ToString(), row[Array.IndexOf(header, "CurrencyID_0")]);
        Assert.Equal(honor.ToString(), row[Array.IndexOf(header, "CurrencyCount_0")]);
    }
}
