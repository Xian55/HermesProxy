using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Issue #535: the records the proxy makes for an item, such as a new appearance, are made once per
/// proxy, by whichever session's query first describes the item. Every other session's client
/// still has to be sent them, or it shows the item as its own data has it.
/// </summary>
public sealed class ItemRecordsPerSessionTests
{
    // Clear of the entries and display ids the rest of the suite uses.
    private const uint Entry = 0x7E00_0535;
    private const uint DisplayId = 0x7E00_0535;

    // A weapon, so the appearance generators make records for it with no CSV data loaded.
    private static byte[] ItemQueryResponse() => LegacyPacketBuilder.Build(Opcode.SMSG_ITEM_QUERY_SINGLE_RESPONSE, packet =>
    {
        packet.WriteUInt32(Entry);
        packet.WriteInt32(2);
        packet.WriteUInt32(7);
        packet.WriteInt32(-1);
        packet.WriteCString("Probe Blade");
        packet.WriteCString("");
        packet.WriteCString("");
        packet.WriteCString("");
        packet.WriteUInt32(DisplayId);
        // Every later field zero: no stats, spells or text.
        packet.WriteBytes(new byte[1024]);
    });

    private static byte[] ItemCreate() => LegacyCreateWire.Build(MovementScenarios.ItemCreate with { Entry = Entry });

    private static HotfixRecord[] RecordsSent(LegacyHandlerHarness harness) =>
        [.. harness.ClientWire.Sent.Select(p => p.Packet).OfType<HotFixMessage>().SelectMany(m => m.Hotfixes)];

    private static void Answer(LegacyHandlerHarness harness)
        => harness.Deliver(Opcode.SMSG_ITEM_QUERY_SINGLE_RESPONSE, ItemQueryResponse(), harness.Client.HandleItemQueryResponse);

    [Fact]
    public void ItemDescribedToOneSession_ReachesAnotherSessionsBags_ThatClientGetsTheSameRecordsOnce()
    {
        var first = new LegacyHandlerHarness(recordClientPackets: true);
        var second = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        try
        {
            Answer(first);
            HotfixRecord[] made = RecordsSent(first);
            Assert.NotEmpty(made);

            second.Deliver(Opcode.SMSG_UPDATE_OBJECT, ItemCreate(), second.Client.HandleUpdateObject);

            Assert.Equal(made.Select(r => r.HotfixId), RecordsSent(second).Select(r => r.HotfixId));
            uint query = LegacyVersion.GetCurrentOpcode(Opcode.CMSG_ITEM_QUERY_SINGLE);
            Assert.DoesNotContain(second.ServerWire.Sent, p => p.Opcode == query);

            second.Deliver(Opcode.SMSG_UPDATE_OBJECT, ItemCreate(), second.Client.HandleUpdateObject);
            Assert.Equal(made.Length, RecordsSent(second).Length);
        }
        finally
        {
            Forget(first);
        }
    }

    /// <summary>
    /// Both sessions asked before either answer came back. The second answer finds the stores up
    /// to date, so the generators make nothing for it.
    /// </summary>
    [Fact]
    public void ItemDescribedToTwoSessions_SecondAnswerFindsStoresUpToDate_SecondClientStillGetsTheRecords()
    {
        var first = new LegacyHandlerHarness(recordClientPackets: true);
        var second = new LegacyHandlerHarness(recordClientPackets: true);
        try
        {
            Answer(first);
            HotfixRecord[] made = RecordsSent(first);
            Assert.NotEmpty(made);

            Answer(second);

            Assert.Equal(made.Select(r => r.HotfixId), RecordsSent(second).Select(r => r.HotfixId));
            Assert.Equal(made.Length, RecordsSent(first).Length);
        }
        finally
        {
            Forget(first);
        }
    }

    // What the item left in the shared stores, so the next test makes its records afresh.
    private static void Forget(LegacyHandlerHarness first)
    {
        GameData.ItemTemplates.TryRemove(Entry, out _);
        GameData.ItemRecordDeliveries.TryRemove(Entry, out _);
        GameData.ItemModifiedAppearanceByItemStore.TryRemove(Entry, out _);
        foreach (var record in RecordsSent(first))
        {
            GameData.Hotfixes.TryRemove(record.HotfixId, out _);
            if (record.TableHash == DB2Hash.ItemModifiedAppearance)
                GameData.ItemModifiedAppearanceStore.TryRemove(record.RecordId, out _);
            if (record.TableHash == DB2Hash.ItemAppearance)
            {
                lock (GameData.ItemRecordLock)
                    GameData.ItemAppearanceStore.Remove(record.RecordId);
            }
        }
    }
}
