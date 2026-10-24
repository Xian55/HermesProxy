using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_DUEL_REQUESTED goes to both players when a duel is asked for. From 3.4 on it ends with a
/// byte holding the ToTheDeath bit (TrinityCore wotlk_classic and cata_classic, the 3.4.3 and 4.4.2
/// clients' readers); 1.14 and 2.5 read the three GUIDs only. Written without the byte, every duel
/// request reached the 3.4.3 and 4.4.2 clients a byte short.
/// </summary>
public sealed class DuelRequestedLayoutTests
{
    // the flag (a game object), who asked and the account, all empty
    private const string Guids = "0000" + "0000" + "0000";

    [Fact]
    public void GuidsLayout_IsTheFormerWrite()
    {
        Assert.Equal(Guids, Write(new DuelRequested.GuidsLayout(), toTheDeath: false));
        Assert.Equal(Guids, WriteToSpan(new DuelRequested.GuidsLayout(), toTheDeath: false));
    }

    [Theory]
    [InlineData(false, "00")]
    [InlineData(true, "80")]
    public void ToTheDeathLayout_EndsWithTheBit(bool toTheDeath, string lastByte)
    {
        Assert.Equal(Guids + lastByte, Write(new DuelRequested.ToTheDeathLayout(), toTheDeath));
        Assert.Equal(Guids + lastByte, WriteToSpan(new DuelRequested.ToTheDeathLayout(), toTheDeath));
    }

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<DuelRequested.GuidsLayout>(DuelRequested.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<DuelRequested.GuidsLayout>(DuelRequested.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<DuelRequested.ToTheDeathLayout>(DuelRequested.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<DuelRequested.ToTheDeathLayout>(DuelRequested.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    private static DuelRequested Sample(bool toTheDeath) => new() { ToTheDeath = toTheDeath };

    private static string Write(ServerPacketLayout<DuelRequested> layout, bool toTheDeath)
    {
        using var data = new WorldPacket(1u);
        layout.Write(Sample(toTheDeath), data);
        return Convert.ToHexString(data.GetDataSpan());
    }

    private static string WriteToSpan(ServerPacketLayout<DuelRequested> layout, bool toTheDeath)
    {
        Span<byte> buffer = stackalloc byte[layout.MaxSize];
        int written = layout.WriteToSpan(Sample(toTheDeath), buffer);
        return Convert.ToHexString(buffer[..written]);
    }
}
