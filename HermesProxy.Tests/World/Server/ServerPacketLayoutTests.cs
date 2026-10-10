using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HermesProxy.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// <see cref="ServerPacketLayouts{TLayout}"/>: the range rule, and that every packet's list picks
/// exactly one layout for each build it can run under.
/// </summary>
public class ServerPacketLayoutTests
{
    private sealed record Layout(string Name);

    /// <summary>Every supported modern build, plus 4.4.2, which is not supported yet but whose
    /// layouts the lists already have to cover.</summary>
    public static TheoryData<ClientVersionBuild> ModernBuilds()
    {
        var builds = new TheoryData<ClientVersionBuild>();
        foreach (var build in Enum.GetValues<ClientVersionBuild>()
                     .Where(b => VersionChecker.IsSupportedModernVersion(b) || b == ClientVersionBuild.V4_4_2_60895)
                     .Distinct())
            builds.Add(build);
        return builds;
    }

    [Theory]
    [MemberData(nameof(ModernBuilds))]
    public void EveryPacketLayoutList_PicksExactlyOneLayout(ClientVersionBuild build)
    {
        var lists = PacketLayoutLists().ToList();
        Assert.NotEmpty(lists);

        foreach (var (owner, list) in lists)
        {
            var forBuild = list.GetType().GetMethod(nameof(ServerPacketLayouts<>.For), [typeof(ClientVersionBuild)])!;
            try
            {
                Assert.NotNull(forBuild.Invoke(list, [build]));
            }
            catch (TargetInvocationException e)
            {
                Assert.Fail($"{owner.Name} for {build}: {e.InnerException!.Message}");
            }
        }
    }

    [Fact]
    public void Range_IncludesAddedIn_ExcludesRemovedIn()
    {
        var layouts = new ServerPacketLayouts<Layout>(
            (ClientVersionBuild.Zero, ClientVersionBuild.V2_5_3_41750, new Layout("before 2.5.3")),
            (ClientVersionBuild.V2_5_3_41750, ClientVersionBuild.V3_4_3_54261, new Layout("2.5.3 up to 3.4.3")),
            (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.Zero, new Layout("3.4.3 on")));

        Assert.Equal("before 2.5.3", layouts.For(ClientVersionBuild.V1_14_2_42597).Name);
        Assert.Equal("before 2.5.3", layouts.For(ClientVersionBuild.V2_5_2_39570).Name);
        Assert.Equal("2.5.3 up to 3.4.3", layouts.For(ClientVersionBuild.V2_5_3_41750).Name);
        Assert.Equal("3.4.3 on", layouts.For(ClientVersionBuild.V3_4_3_54261).Name);
    }

    [Fact]
    public void OpenUpperBound_CarriesALayoutForwardToLaterBuilds()
    {
        var layouts = new ServerPacketLayouts<Layout>(
            (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new Layout("retail")),
            (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.Zero, new Layout("3.4.3 on")));

        Assert.Equal("3.4.3 on", layouts.For(ClientVersionBuild.V4_4_2_60895).Name);
    }

    [Fact]
    public void Gap_Throws()
    {
        var layouts = new ServerPacketLayouts<Layout>(
            (ClientVersionBuild.Zero, ClientVersionBuild.V2_5_3_41750, new Layout("old")),
            (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.Zero, new Layout("new")));

        var e = Assert.Throws<InvalidOperationException>(() => layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.Contains("no layout covers 2.5.3", e.Message);
    }

    [Fact]
    public void Overlap_Throws()
    {
        var layouts = new ServerPacketLayouts<Layout>(
            (ClientVersionBuild.Zero, ClientVersionBuild.Zero, new Layout("all")),
            (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.Zero, new Layout("3.4.3 on")));

        var e = Assert.Throws<InvalidOperationException>(() => layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.Contains("both cover 3.4.3", e.Message);
    }

    /// <summary>Every static <see cref="ServerPacketLayouts{TLayout}"/> field in HermesProxy.</summary>
    private static IEnumerable<(Type Owner, object List)> PacketLayoutLists()
    {
        foreach (var type in typeof(ServerPacketLayouts<>).Assembly.GetTypes())
        {
            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(ServerPacketLayouts<>))
                    yield return (type, field.GetValue(null)!);
            }
        }
    }
}
