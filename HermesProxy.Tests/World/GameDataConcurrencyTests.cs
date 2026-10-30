using System.Threading;
using System.Threading.Tasks;
using HermesProxy.World;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Issue #355: what the legacy server says about creatures, quests and items is kept once per
/// proxy, and every session's legacy thread writes and reads it. A session's executor runs one of
/// that session's handlers at a time; it does nothing between two sessions. Two clients loading the
/// same area corrupted the creature store, and <c>CMSG_QUERY_CREATURE</c> threw.
/// </summary>
public sealed class GameDataConcurrencyTests
{
    private const int Threads = 8;
    private const int PerThread = 4_000;

    // Clear of any entry the rest of the suite stores, so this runs beside it.
    private const uint FirstEntry = 0x7F00_0000;

    private static void Concurrently(System.Action<int> body)
    {
        using var start = new Barrier(Threads);
        var workers = new Task[Threads];
        for (int t = 0; t < Threads; t++)
        {
            int thread = t;
            workers[t] = Task.Factory.StartNew(() =>
            {
                start.SignalAndWait();
                body(thread);
            }, TaskCreationOptions.LongRunning);
        }
        Task.WaitAll(workers);
    }

    [Fact]
    public void CreatureTemplates_StoredAndReadFromManySessionsAtOnce_AreAllThere()
    {
        Concurrently(thread =>
        {
            for (int i = 0; i < PerThread; i++)
            {
                uint entry = FirstEntry + (uint)(thread * PerThread + i);
                GameData.StoreCreatureTemplate(entry, new CreatureTemplate());
                // Another session asking about a creature this one has not stored yet.
                GameData.GetCreatureTemplate(entry + PerThread);
            }
        });

        try
        {
            for (uint i = 0; i < Threads * PerThread; i++)
                Assert.NotNull(GameData.GetCreatureTemplate(FirstEntry + i));
        }
        finally
        {
            for (uint i = 0; i < Threads * PerThread; i++)
                GameData.CreatureTemplates.TryRemove(FirstEntry + i, out _);
        }
    }

    /// <summary>
    /// The second hazard in the issue: a generator checks the store, takes the first free record
    /// id and adds the record as three steps. Two sessions in between each other's steps were handed
    /// the same id for different appearances, and the second add threw.
    /// </summary>
    [Fact]
    public void ItemAppearances_CreatedByManySessionsAtOnce_EachGetTheirOwnRecord()
    {
        const int Items = 100;
        var replies = new HermesProxy.World.Server.Packets.HotFixMessage?[Threads * Items];

        Concurrently(thread =>
        {
            for (int i = 0; i < Items; i++)
            {
                int n = thread * Items + i;
                var item = new ItemTemplate { Entry = FirstEntry + (uint)n, DisplayID = FirstEntry + (uint)n };
                replies[n] = GameData.GenerateItemAppearanceUpdateIfNeeded(item);
            }
        });

        var ids = new System.Collections.Generic.List<uint>();
        try
        {
            for (int n = 0; n < replies.Length; n++)
            {
                var reply = replies[n];
                Assert.NotNull(reply);
                uint id = reply.Hotfixes[0].RecordId;
                ids.Add(id);
                lock (GameData.ItemRecordLock)
                    Assert.Equal((int)(FirstEntry + (uint)n), GameData.ItemAppearanceStore[id].ItemDisplayInfoID);
            }
            Assert.Equal(ids.Count, new System.Collections.Generic.HashSet<uint>(ids).Count);
        }
        finally
        {
            lock (GameData.ItemRecordLock)
            {
                foreach (uint id in ids)
                    GameData.ItemAppearanceStore.Remove(id);
            }
            foreach (var reply in replies)
                if (reply != null)
                    foreach (var hotfix in reply.Hotfixes)
                        GameData.Hotfixes.TryRemove(hotfix.HotfixId, out _);
        }
    }

    [Fact]
    public void QuestAndItemTemplates_StoredAndReadFromManySessionsAtOnce_AreAllThere()
    {
        Concurrently(thread =>
        {
            for (int i = 0; i < PerThread; i++)
            {
                uint entry = FirstEntry + (uint)(thread * PerThread + i);
                GameData.StoreQuestTemplate(entry, new QuestTemplate());
                GameData.StoreItemTemplate(entry, new ItemTemplate());
                GameData.StoreItemName(entry, "x");
                GameData.GetQuestTemplate(entry + PerThread);
                GameData.GetItemTemplate(entry + PerThread);
            }
        });

        try
        {
            for (uint i = 0; i < Threads * PerThread; i++)
            {
                Assert.NotNull(GameData.GetQuestTemplate(FirstEntry + i));
                Assert.NotNull(GameData.GetItemTemplate(FirstEntry + i));
                Assert.Equal("x", GameData.GetItemName(FirstEntry + i));
            }
        }
        finally
        {
            for (uint i = 0; i < Threads * PerThread; i++)
            {
                GameData.QuestTemplates.TryRemove(FirstEntry + i, out _);
                GameData.ItemTemplates.TryRemove(FirstEntry + i, out _);
                GameData.ItemNames.TryRemove(FirstEntry + i, out _);
            }
        }
    }
}
