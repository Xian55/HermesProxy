using HermesProxy.Tests.Support;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A modern client asks at login whom its quests can be handed in to. Only a 4.x server knows the
/// request, and on 3.3.5a the opcode of that name is another one in the cores, so nothing goes to
/// the server and the proxy answers for each quest that nobody is known.
/// </summary>
public sealed class QuestCompletionNpcsTests
{
    [Fact]
    public void HandleQueryQuestCompletionNPCs_OnAnOlderServer_AnswersEachQuestWithNobody()
    {
        Assert.SkipWhen(HermesProxy.World.Client.WorldClient.IsCataLegacy, "A 4.3.4 server answers this itself.");
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);

        var query = new QueryQuestCompletionNPCs([12, 345]);
        QuestSystem.HandleQueryQuestCompletionNPCs(in query, in ctx);

        var answer = Assert.Single(harness.ClientWire.Sent);
        var response = Assert.IsType<QuestCompletionNPCResponse>(answer.Packet);
        Assert.Equal([12, 345], response.Quests.ConvertAll(q => q.QuestID));
        Assert.All(response.Quests, q => Assert.Empty(q.NPCs));
        Assert.Empty(harness.ServerWire.Sent);
    }
}
