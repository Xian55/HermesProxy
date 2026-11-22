using System.Collections.Generic;

using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Issue #187: the proxy no longer builds ItemEffect hotfix records from CSV. The ones
/// <c>CSV/Hotfix/ItemEffect{3,4}.csv</c> used to give it were reachable only through
/// <see cref="GameData.UpdateHotfix"/>, which rewrites a record's content before
/// <see cref="GameData.GenerateHotFixMessage"/> sends it. These tests pin that down: the client is
/// sent the same record whether or not a CSV record was under it, apart from the hotfix id.
/// </summary>
public sealed class ItemEffectHotfixRecordTests
{
    // Clear of the client DB2's ItemEffect ids and of the ids the rest of the suite uses.
    private const uint RecordId = 0x7E00_0187;
    private const uint ParentItemId = 0x7E00_0187;

    // Where LoadItemEffectHotfixes numbered a CSV row: HotfixItemEffectBegin + row.
    private const uint CsvHotfixId = GameData.HotfixItemEffectBegin + 99_999;

    private static GameData.ItemEffect Effect() => new()
    {
        Id = (int)RecordId,
        LegacySlotIndex = 0,
        TriggerType = 0,
        Charges = -1,
        CoolDownMSec = -1,
        CategoryCoolDownMSec = -1,
        SpellCategoryID = 0,
        SpellID = 42_336,
        ChrSpecializationID = 0,
        ParentItemID = (int)ParentItemId,
    };

    // The record LoadItemEffectHotfixes built for this id, with the client DB2's older spell.
    private static HotfixRecord CsvRecord()
    {
        var record = new HotfixRecord
        {
            Status = HotfixStatus.Valid,
            TableHash = DB2Hash.ItemEffect,
            HotfixId = CsvHotfixId,
            UniqueId = CsvHotfixId,
            RecordId = RecordId,
        };
        var stale = Effect();
        stale.SpellID = 1;
        GameData.WriteItemEffectHotfix(stale, record.HotfixContent);
        return record;
    }

    private static List<HotfixRecord> Sent(GameData.ItemEffect effect, bool remove)
    {
        GameData.UpdateHotfix(effect, remove);
        return GameData.GenerateHotFixMessage(effect, remove)!.Hotfixes;
    }

    private static void Forget(IEnumerable<HotfixRecord> records)
    {
        foreach (var record in records)
            GameData.Hotfixes.TryRemove(record.HotfixId, out _);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UpdateHotfix_ItemEffect_SendsTheSameRecordWithOrWithoutACsvRecordUnderIt(bool remove)
    {
        // Callers hold this lock; it also keeps other tests' item records out of the way.
        using var held = GameData.ItemRecordLock.EnterScope();

        List<HotfixRecord> alone = [];
        List<HotfixRecord> overCsv = [];
        try
        {
            alone = Sent(Effect(), remove);
            Forget(alone);

            GameData.Hotfixes[CsvHotfixId] = CsvRecord();
            overCsv = Sent(Effect(), remove);

            var made = Assert.Single(alone);
            var rewritten = Assert.Single(overCsv);

            Assert.Equal(CsvHotfixId, rewritten.HotfixId);
            Assert.NotEqual(CsvHotfixId, made.HotfixId);
            Assert.True(made.HotfixId >= GameData.HotfixItemBegin);

            Assert.Equal(rewritten.TableHash, made.TableHash);
            Assert.Equal(rewritten.RecordId, made.RecordId);
            Assert.Equal(remove ? HotfixStatus.RecordRemoved : HotfixStatus.Valid, made.Status);
            Assert.Equal(rewritten.Status, made.Status);
            Assert.Equal(rewritten.HotfixContent.GetData(), made.HotfixContent.GetData());
        }
        finally
        {
            Forget(alone);
            Forget(overCsv);
            GameData.Hotfixes.TryRemove(CsvHotfixId, out _);
        }
    }
}
