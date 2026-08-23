using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// A [PacketHandler] attribute binds to whatever declaration follows it, so inserting a
/// method between the attribute and its handler silently moves the registration onto the
/// new method and leaves the opcode unhandled. That happened to SMSG_UPDATE_OBJECT and
/// only showed up as a startup log line, so these assert the wiring directly.
/// </summary>
public class PacketHandlerRegistrationTests
{
    static PacketHandlerRegistrationTests()
    {
        // Reflecting over the handler hosts touches LegacyVersion's static initializers,
        // which refuse to run until a legacy build is chosen.
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static IEnumerable<MethodInfo> DecoratedMethods(Type hostType) =>
        hostType.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                            BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes<PacketHandlerAttribute>().Any());

    // WorldClient handlers take a raw WorldPacket; WorldSocket handlers take a deserialized
    // ClientPacket subclass. Each registrar rejects anything else at startup with a log line.
    [Fact]
    public void EveryWorldClientHandlerTakesAWorldPacket()
    {
        var broken = DecoratedMethods(typeof(WorldClient))
            .Where(m =>
            {
                var p = m.GetParameters();
                return p.Length == 0 || p[0].ParameterType != typeof(WorldPacket);
            })
            .Select(m => m.Name)
            .ToList();

        Assert.True(broken.Count == 0,
            $"[PacketHandler] on WorldClient methods that cannot be registered: {string.Join(", ", broken)}. " +
            "The attribute most likely slipped onto the wrong declaration.");
    }

    [Fact]
    public void EveryWorldSocketHandlerTakesAClientPacket()
    {
        var broken = DecoratedMethods(typeof(WorldSocket))
            .Where(m =>
            {
                var p = m.GetParameters();
                return p.Length == 0 || p[0].ParameterType.BaseType != typeof(ClientPacket);
            })
            .Select(m => m.Name)
            .ToList();

        Assert.True(broken.Count == 0,
            $"[PacketHandler] on WorldSocket methods that cannot be registered: {string.Join(", ", broken)}. " +
            "The attribute most likely slipped onto the wrong declaration.");
    }

    [Fact]
    public void UpdateObjectOpcodesAreHandled()
    {
        var handled = DecoratedMethods(typeof(WorldClient))
            .SelectMany(m => m.GetCustomAttributes<PacketHandlerAttribute>())
            .Select(a => a.Opcode)
            .ToHashSet();

        Assert.Contains(Opcode.SMSG_UPDATE_OBJECT, handled);
        Assert.Contains(Opcode.SMSG_COMPRESSED_UPDATE_OBJECT, handled);
    }
}
