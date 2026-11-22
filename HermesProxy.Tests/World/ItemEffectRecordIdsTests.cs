using System.Collections.Generic;
using System.IO;
using System.Linq;

using HermesProxy.World;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// <see cref="GameData.AddItemEffectRecord"/> reuses the client's own record id for an item and
/// slot, so its hotfix updates the row the client already indexes. The 4.4.2 client was given
/// ids from the 3.4.3 table: 295 of them name another slot of the same item in 4.4.2's own table,
/// and the hotfix overwrote that row.
/// </summary>
public sealed class ItemEffectRecordIdsTests
{
    // (id, slot, item) of every row of the client table the store loads for an expansion.
    private static List<(uint Id, byte Slot, uint Item)> ClientRows(byte expansion)
        => [.. File.ReadLines(Path.Combine("CSV", $"ItemEffect{expansion}.csv")).Skip(1)
            .Select(line => line.Split(','))
            .Select(r => (uint.Parse(r[0]), byte.Parse(r[1]), uint.Parse(r[9])))];

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public void ParseItemEffectRecordIds_EveryId_IsThatClientsRowForTheItemAndSlot(byte expansion)
    {
        var rows = ClientRows(expansion);
        var rowById = rows.ToDictionary(r => r.Id);

        var ids = GameData.ParseItemEffectRecordIds(expansion);

        Assert.Equal(rows.Count, ids.Count);
        Assert.All(ids, kv =>
        {
            Assert.True(rowById.TryGetValue(kv.Value, out var row));
            Assert.Equal(kv.Key, (row.Item, row.Slot));
        });
    }

    /// <summary>
    /// Item 867: 3.4.3 has its effect in slot 0 as record 97093. 4.4.2 has no slot 0 row for it
    /// and uses 97093 for slot 2.
    /// </summary>
    [Fact]
    public void ParseItemEffectRecordIds_On442_DoesNotTake343sIdForASlotThe442ClientLacks()
    {
        Assert.Equal(97093u, GameData.ParseItemEffectRecordIds(3)[(867, 0)]);

        var ids = GameData.ParseItemEffectRecordIds(4);

        Assert.False(ids.ContainsKey((867, 0)));
        Assert.Equal(97093u, ids[(867, 2)]);
    }
}
