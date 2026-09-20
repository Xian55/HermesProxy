using System.Collections.Generic;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The legacy field cache holds whatever a block carried for a guid, so an item the proxy only ever
/// saw a Values update for has no <c>OBJECT_FIELD_ENTRY</c>. Reading it through the dictionary
/// indexer threw <see cref="KeyNotFoundException"/>, and one caller runs inside the update
/// translator, where a throw costs the whole packet.
/// </summary>
public class CachedItemFieldsTests
{
    private static GameSessionData StateWithCachedFields(WowGuid128 guid, Dictionary<int, UpdateField> fields)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false);
        lock (harness.Session.GameState.ObjectCacheLock)
            harness.Session.GameState.ObjectCacheLegacy[guid] = fields;
        return harness.Session.GameState;
    }

    [Fact]
    public void GetItemId_WhenTheCachedFieldsHaveNoEntry_ReturnsZero()
    {
        var guid = WowGuid128.Create(HighGuidType703.Item, 12);
        var state = StateWithCachedFields(guid, []);

        Assert.Equal(0u, state.GetItemId(guid));
    }

    [Fact]
    public void GetItemSpellSlot_WhenTheCachedFieldsHaveNoEntry_ReturnsZero()
    {
        var guid = WowGuid128.Create(HighGuidType703.Item, 13);
        var state = StateWithCachedFields(guid, []);

        Assert.Equal(0, state.GetItemSpellSlot(guid, spellId: 1234));
    }

    [Fact]
    public void GetItemId_WhenTheEntryWasCached_ReturnsIt()
    {
        var guid = WowGuid128.Create(HighGuidType703.Item, 14);
        int entryField = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
        var state = StateWithCachedFields(guid, new Dictionary<int, UpdateField> { [entryField] = new(6948) });

        Assert.Equal(6948u, state.GetItemId(guid));
    }
}
