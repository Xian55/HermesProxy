using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

[CollectionDefinition("NpcBroadcastTexts", DisableParallelization = true)]
public class NpcBroadcastTextsCollection;

[Collection("NpcBroadcastTexts")]
public class NpcTextHotfixTests
{
    [Fact]
    public void MatchingLegacyRow_IsNeverReferencedByItsConflictingClassicId()
    {
        const uint legacyId = 123456;
        GameData.BroadcastTextStore.TryGetValue(legacyId, out var original);
        try
        {
            GameData.BroadcastTextStore[legacyId] = new GameData.BroadcastText
            {
                Entry = legacyId, MaleText = "Legacy collision", FemaleText = "Legacy collision female",
                Language = 0
            };
            uint id = GameData.GetBroadcastTextId("Legacy collision", "Legacy collision female", 0,
                [0, 0, 0], [0, 0, 0]);
            Assert.NotEqual(legacyId, id);
            Assert.InRange(id, 0x40000000u, (uint)int.MaxValue - 1);
            Assert.Equal("Legacy collision", GameData.GetBroadcastText(id)!.MaleText);
        }
        finally
        {
            if (original != null) GameData.BroadcastTextStore[legacyId] = original;
            else GameData.BroadcastTextStore.Remove(legacyId);
        }
    }

    [Fact]
    public void LegacyNpcText_IsSentBeforeItsReferences_WithoutUsingBakedIds()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        byte[] legacy = BuildResponse("Trainer greeting", "Trainer greeting (female)");
        harness.Deliver(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE, legacy, harness.Client.HandleQueryNpcTextResponse);

        Assert.Equal(2, harness.ClientWire.Sent.Count);
        var hotfix = Assert.IsType<DBReply>(harness.ClientWire.Sent[0].Packet);
        var response = Assert.IsType<QueryNPCTextResponse>(harness.ClientWire.Sent[1].Packet);
        Assert.True(response.Allow);
        Assert.Equal(123u, response.TextID);
        Assert.Equal(1f, response.Probabilities[0]);
        Assert.Equal(hotfix.RecordID, response.BroadcastTextID[0]);
        Assert.InRange(hotfix.RecordID, 0x40000000u, (uint)int.MaxValue - 1);
        Assert.All(response.BroadcastTextID.Skip(1), id => Assert.Equal(0u, id));
        Assert.Equal(DB2Hash.BroadcastText, hotfix.TableHash);
        Assert.Equal(HotfixStatus.Valid, hotfix.Status);
        using var body = new WorldPacket(1, hotfix.Data.GetDataSpan().ToArray());
        Assert.Equal("Trainer greeting", body.ReadCString());
        Assert.Equal("Trainer greeting (female)", body.ReadCString());
        Assert.Equal(hotfix.RecordID, body.ReadUInt32());
        Assert.Equal(7u, body.ReadUInt32());
    }

    [Fact]
    public void SameText_IsRefreshedAgain_ForAnAlreadyCachedClient()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        foreach (int unused in Enumerable.Range(0, 2))
            harness.Deliver(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE, BuildResponse("Cached greeting", ""),
                harness.Client.HandleQueryNpcTextResponse);
        Assert.Equal(4, harness.ClientWire.Sent.Count);
        Assert.IsType<DBReply>(harness.ClientWire.Sent[2].Packet);
        Assert.Equal(Assert.IsType<DBReply>(harness.ClientWire.Sent[0].Packet).RecordID,
            Assert.IsType<DBReply>(harness.ClientWire.Sent[2].Packet).RecordID);
    }

    [Fact]
    public void BroadcastIdentity_IncludesBothGendersLanguageAndEmotes()
    {
        ushort[] delays = [1, 2, 3], emotes = [4, 5, 6];
        uint id = GameData.GetBroadcastTextId("Male identity", "Female identity", 7, delays, emotes);
        Assert.Equal(id, GameData.GetBroadcastTextId("Male identity", "Female identity", 7, delays, emotes));
        Assert.NotEqual(id, GameData.GetBroadcastTextId("Male identity", "Different female", 7, delays, emotes));
        Assert.NotEqual(id, GameData.GetBroadcastTextId("Different male", "Female identity", 7, delays, emotes));
        Assert.NotEqual(id, GameData.GetBroadcastTextId("Male identity", "Female identity", 8, delays, emotes));
        Assert.NotEqual(id, GameData.GetBroadcastTextId("Male identity", "Female identity", 7, [2, 2, 3], emotes));
        Assert.NotEqual(id, GameData.GetBroadcastTextId("Male identity", "Female identity", 7, delays, [5, 5, 6]));
        delays[0] = 99;
        emotes[0] = 99;
        Assert.Equal((ushort)1, GameData.GetBroadcastText(id)!.EmoteDelays[0]);
        Assert.Equal((ushort)4, GameData.GetBroadcastText(id)!.Emotes[0]);
    }

    [Fact]
    public void MaskedNpcText_RemainsRejected_WithoutPublishingText()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        byte[] legacy = LegacyPacketBuilder.Build(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE,
            packet => packet.WriteUInt32(0x8000007B));
        harness.Deliver(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE, legacy, harness.Client.HandleQueryNpcTextResponse);
        var response = Assert.IsType<QueryNPCTextResponse>(Assert.Single(harness.ClientWire.Sent).Packet);
        Assert.False(response.Allow);
    }

    private static byte[] BuildResponse(string male, string female)
        => LegacyPacketBuilder.Build(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE, packet =>
        {
            packet.WriteUInt32(123);
            for (int i = 0; i < 8; i++)
            {
                packet.WriteFloat(i == 0 ? 1f : 0f);
                packet.WriteCString(i == 0 ? male : "");
                packet.WriteCString(i == 0 ? female : "");
                packet.WriteUInt32(7);
                for (int emote = 0; emote < 3; emote++)
                {
                    packet.WriteUInt32(0);
                    packet.WriteUInt32(0);
                }
            }
        });
}
