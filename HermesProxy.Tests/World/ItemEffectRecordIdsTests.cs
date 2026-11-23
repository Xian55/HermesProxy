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
/// and the hotfix overwrote that row. The ids for the rest have to be the same on every proxy run,
/// since the client keeps the records it was sent in its hotfix cache.
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

    /// <summary>
    /// An id of the lookup that is a row of the running client's table is that row's item and
    /// slot, so a hotfix never replaces another row of the client's.
    /// </summary>
    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public void BuildItemEffectRecordIds_NoId_IsAnotherRowOfTheClientsTable(byte expansion)
    {
        var rowById = ClientRows(expansion).ToDictionary(r => r.Id);

        var ids = GameData.BuildItemEffectRecordIds(expansion);

        Assert.All(ids, kv =>
        {
            if (rowById.TryGetValue(kv.Value, out var row))
                Assert.Equal(kv.Key, (row.Item, row.Slot));
        });
        Assert.Equal(ids.Count, ids.Values.Distinct().Count());
    }

    /// <summary>
    /// The lookup depends on the client tables alone, not on what was looked up before, so a
    /// new proxy process gives every item and slot the id the client cached from the last one.
    /// </summary>
    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public void BuildItemEffectRecordIds_IsTheSameOnEveryCall(byte expansion)
    {
        var first = GameData.BuildItemEffectRecordIds(expansion);
        var second = GameData.BuildItemEffectRecordIds(expansion);

        Assert.Equal(first.OrderBy(kv => kv.Key), second.OrderBy(kv => kv.Key));
    }

    [Fact]
    public void BuildItemEffectRecordIds_On343_IsTheClientsOwnTable()
    {
        var own = GameData.ParseItemEffectRecordIds(3);

        Assert.Equal(own.OrderBy(kv => kv.Key), GameData.BuildItemEffectRecordIds(3).OrderBy(kv => kv.Key));
    }

    /// <summary>
    /// On 4.4.2, an item and slot only 3.4.3 has keeps 3.4.3's id, unless 4.4.2 has a row with
    /// that id: then the id moves up by <see cref="GameData.ItemEffectMovedRecordIdOffset"/>.
    /// </summary>
    [Fact]
    public void BuildItemEffectRecordIds_On442_Keeps343sIdUnlessThe442ClientHasARowWithIt()
    {
        var own = GameData.ParseItemEffectRecordIds(4);
        var wotlk = GameData.ParseItemEffectRecordIds(3);
        var ownRecordIds = own.Values.ToHashSet();

        var ids = GameData.BuildItemEffectRecordIds(4);

        Assert.All(own, kv => Assert.Equal(kv.Value, ids[kv.Key]));

        var onlyIn343 = wotlk.Where(kv => !own.ContainsKey(kv.Key)).ToList();
        Assert.Equal(own.Count + onlyIn343.Count, ids.Count);

        var moved = onlyIn343.Where(kv => ownRecordIds.Contains(kv.Value)).ToList();
        Assert.Equal(295, moved.Count);
        Assert.Equal(11_577, onlyIn343.Count - moved.Count);
        Assert.All(onlyIn343, kv => Assert.Equal(
            ownRecordIds.Contains(kv.Value) ? GameData.ItemEffectMovedRecordIdOffset + kv.Value : kv.Value,
            ids[kv.Key]));

        // Item 867, slot 0: 3.4.3's 97093 is 4.4.2's slot 2. Item 892, slot 0: 4.4.2 has no 97047.
        Assert.Equal(GameData.ItemEffectMovedRecordIdOffset + 97093, ids[(867, 0)]);
        Assert.Equal(97093u, ids[(867, 2)]);
        Assert.Equal(97047u, ids[(892, 0)]);
    }

    /// <summary>
    /// A moved id is above every row of every client table and below
    /// <see cref="GameData.HotfixItemBegin"/>, where the free ids for items neither table has start.
    /// </summary>
    [Fact]
    public void BuildItemEffectRecordIds_MovedIds_AreAboveEveryClientRowAndBelowTheFreeIds()
    {
        uint highestClientId = new byte[] { 1, 2, 3, 4 }.SelectMany(ClientRows).Max(r => r.Id);
        uint highest343Id = ClientRows(3).Max(r => r.Id);

        Assert.True(GameData.ItemEffectMovedRecordIdOffset > highestClientId);
        Assert.True(GameData.ItemEffectMovedRecordIdOffset + highest343Id < GameData.HotfixItemBegin);
    }
}
