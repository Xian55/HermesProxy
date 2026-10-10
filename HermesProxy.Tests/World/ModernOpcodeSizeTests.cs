using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The opcode's size on the modern wire: 2 bytes before 4.4.0, 4 from it, where the upper 16 bits
/// carry an opcode group.
/// </summary>
public class ModernOpcodeSizeTests
{
    /// <summary>Every build with a table today. None of them has an opcode group.</summary>
    public static TheoryData<ClientVersionBuild> TableBuilds() =>
    [
        ClientVersionBuild.V1_12_1_5875,
        ClientVersionBuild.V2_4_3_8606,
        ClientVersionBuild.V3_3_5a_12340,
        ClientVersionBuild.V2_5_2_39570,
        ClientVersionBuild.V2_5_3_41750,
        ClientVersionBuild.V1_14_1_40688,
        ClientVersionBuild.V3_4_3_54261,
    ];

    [Theory]
    [MemberData(nameof(TableBuilds))]
    public void ExistingBuilds_HaveOneGroupAndTwoByteOpcodes(ClientVersionBuild build)
    {
        Assert.True(GeneratedOpcodeTables.TryGet(build, out Opcode[][] groups, out _, out int opcodeSize));

        Assert.Equal(sizeof(ushort), opcodeSize);
        Assert.Single(groups);
    }

    [Fact]
    public void ModernVersion_TestBuildUsesTwoByteOpcodes()
    {
        Assert.Equal(sizeof(ushort), ModernVersion.OpcodeSize);
    }

    [Fact]
    public void ModernVersion_LooksUpGroupZeroAndRejectsOtherGroups()
    {
        uint ping = ModernVersion.GetCurrentOpcode(Opcode.CMSG_PING);
        Assert.NotEqual(0u, ping);

        Assert.Equal(Opcode.CMSG_PING, ModernVersion.GetUniversalOpcode(ping));
        Assert.Equal(Opcode.MSG_NULL_ACTION, ModernVersion.GetUniversalOpcode(0x3B0000 | ping));
        Assert.Equal(Opcode.MSG_NULL_ACTION, ModernVersion.GetUniversalOpcode(uint.MaxValue));
    }

    // CMSG_PING: 0x3A0004 in 4.4.2, 0x3768 in 3.4.3.
    [Fact]
    public void WorldPacket_FourByteOpcode_ReadsTheWholeOpcodeAndStartsThePayloadAfterIt()
    {
        byte[] wire = [0x04, 0x00, 0x3A, 0x00, 0xAA, 0xBB, 0xCC];

        using var packet = new WorldPacket(wire, sizeof(uint));

        byte[] payload = [0xAA, 0xBB, 0xCC];
        Assert.Equal(0x3A0004u, packet.GetOpcode());
        Assert.Equal(payload, packet.GetRemainingSpan().ToArray());
    }

    [Fact]
    public void WorldPacket_TwoByteOpcode_IsTheDefault()
    {
        byte[] wire = [0x68, 0x37, 0xAA, 0xBB];

        using var sized = new WorldPacket(wire, sizeof(ushort));
        using var unsized = new WorldPacket(wire);

        byte[] payload = [0xAA, 0xBB];
        Assert.Equal(0x3768u, sized.GetOpcode());
        Assert.Equal(0x3768u, unsized.GetOpcode());
        Assert.Equal(payload, sized.GetRemainingSpan().ToArray());
        Assert.Equal(payload, unsized.GetRemainingSpan().ToArray());
    }

    [Fact]
    public void WorldPacket_FourByteOpcode_BodyShorterThanOpcodeThrows()
    {
        byte[] wire = [0x04, 0x00];

        Assert.ThrowsAny<Exception>(() => new WorldPacket(wire, sizeof(uint)));
    }
}
