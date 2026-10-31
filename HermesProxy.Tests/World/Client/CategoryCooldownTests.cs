using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The 1.14 and 2.5 clients ask for the spell categories with a changed cooldown at login. No
/// legacy server has that request; the proxy answers with an empty list.
/// </summary>
public sealed class CategoryCooldownTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Fact]
    public unsafe void Request_HasAHandler()
        => Assert.True(GeneratedCmsgDispatch.Get(Opcode.CMSG_REQUEST_CATEGORY_COOLDOWNS) != null);

    [Fact]
    public void Answer_IsACountOfNone()
    {
        var packet = new CategoryCooldown();

        packet.Write();

        Assert.Equal([0x00, 0x00, 0x00, 0x00], ((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }
}
