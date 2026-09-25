using System.Reflection;
using System.Runtime.CompilerServices;
using HermesProxy.Configuration.Options;
using HermesProxy.World;
using HermesProxy.World.Client;
using Xunit;

namespace HermesProxy.Tests.World;

public class LegacySniffOptionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisabledPacketLog_DoesNotOpenLegacyCapture(bool fromClient)
    {
        var session = new GlobalSessionData(new ClientOptions(), new LegacyServerOptions(),
            new ProxyNetworkOptions(), new DiagnosticsOptions { PacketsLog = false }, new ThrottlingOptions());
        try
        {
            var client = (WorldClient)RuntimeHelpers.GetUninitializedObject(typeof(WorldClient));
            typeof(WorldClient).GetField("_globalSession", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(client, session);
            using var packet = new WorldPacket(1u);
            typeof(WorldClient).GetMethod("WriteLegacySniff", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(client, [packet, fromClient]);
            Assert.Null(session.LegacySniff);
        }
        finally
        {
            session.LegacySniff?.CloseFile();
            session.Executor.Dispose();
        }
    }
}
