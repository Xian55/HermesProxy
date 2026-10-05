using System.Collections.Frozen;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;
using ClassicBuilder = HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder;

namespace HermesProxy.Tests.World;

[CollectionDefinition("HeirloomCatalog", DisableParallelization = true)]
public class HeirloomCatalogCollection;

[Collection("HeirloomCatalog")]
public class HeirloomCollectionTests
{
    [Fact]
    public void FullAccountRefresh_DoesNotGrantTheCompatibilityCatalog()
    {
        var original = GameData.Heirlooms;
        try
        {
            GameData.Heirlooms = new[] { 42943, 42985 }.ToFrozenSet();
            var packet = new AccountHeirloomUpdate();
            packet.WritePacketData();
            using var reader = new WorldPacket(1, packet.GetDataSpan().ToArray());
            Assert.True(reader.ReadBit()); // full replacement clears a previously populated collection
            reader.ResetBitPos();
            Assert.Equal(0, reader.ReadInt32());
            Assert.Equal(0u, reader.ReadUInt32()); // owned IDs
            Assert.Equal(0u, reader.ReadUInt32()); // corresponding flags
            Assert.False(reader.CanRead());
            packet.ReleaseData();
            Assert.Equal(2, GameData.Heirlooms.Count); // item compatibility still has its catalog
        }
        finally { GameData.Heirlooms = original; }
    }

    [Fact]
    public void PlayerCreate_MatchesTheEmptyAccountCollection_WithoutCatalogPayload()
    {
        var original = GameData.Heirlooms;
        try
        {
            GameData.Heirlooms = new[] { 42943, 42985 }.ToFrozenSet();
            var harness = new LegacyHandlerHarness(recordClientPackets: false);
            var guid = harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 77));
            var update = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, harness.Session);
            var builder = new ClassicBuilder(update, harness.Session.GameState);
            update.ActivePlayerData = new ActivePlayerData();
            using var counts = new WorldPacket();
            builder.WriteCreateActivePlayerHeirloomCounts(counts, update.ActivePlayerData);
            using var reader = new WorldPacket(1, counts.GetDataSpan().ToArray());
            Assert.Equal(0u, reader.ReadUInt32());
            Assert.Equal(0u, reader.ReadUInt32());
            Assert.False(reader.CanRead());
            using var payload = new WorldPacket();
            builder.WriteCreateActivePlayerDynamicPayloads(payload, update.ActivePlayerData);
            Assert.Equal(0u, payload.GetSize());
        }
        finally { GameData.Heirlooms = original; }
    }
}
