using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public class SetRolePktTests
{
    private static readonly WowGuid128 Target = WowGuid128.Create(HighGuidType703.Player, 42);
    private static readonly WowGuid128 From = WowGuid128.Create(HighGuidType703.Player, 7);

    private static WorldPacket FrameClientBody(WorldPacket payload)
    {
        byte[] body = payload.GetData();
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);
        return new WorldPacket(framed);
    }

    /// Through the production accessor, as the dispatch site builds it.
    private static SpanPacketReader ReaderOver(WorldPacket payload)
        => new(FrameClientBody(payload).GetRemainingSpan());

    private static byte[] ExpectedInformBody(byte partyIndex, byte oldRole, byte newRole)
    {
        var expected = new WorldPacket(1u);
        expected.WriteUInt8(partyIndex);
        expected.WritePackedGuid128(From);
        expected.WritePackedGuid128(Target);
        expected.WriteUInt8(oldRole);
        expected.WriteUInt8(newRole);
        return expected.GetData();
    }

    [Theory]
    [InlineData((byte)2)]
    [InlineData((byte)4)]
    [InlineData((byte)8)]
    public void Read_114_ParsesInt8GuidInt32(byte role)
    {
        var payload = new WorldPacket(1u);
        payload.WriteInt8(0);
        payload.WritePackedGuid128(Target);
        payload.WriteInt32(role);

        var reader = ReaderOver(payload);
        SetRoleCodecPreWotLKClassic.Read(ref reader, out var packet);

        Assert.Equal(0, packet.PartyIndex);
        Assert.Equal(Target, packet.ChangedUnit);
        Assert.Equal(role, packet.Role);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void V343Wire_OptionalPartyIndexThenUint8Role()
    {
        var payload = new WorldPacket(1u);
        payload.WriteBit(false);
        payload.FlushBits();
        payload.WritePackedGuid128(Target);
        payload.WriteUInt8(8);

        var reader = ReaderOver(payload);
        SetRoleCodecWotLKClassic.Read(ref reader, out var packet);

        Assert.Equal(0, packet.PartyIndex);          // the optional byte was absent
        Assert.Equal(Target, packet.ChangedUnit);
        Assert.Equal(8, packet.Role);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void WriteToSpan_EmitsTrinityRoleChangedInformLayout()
    {
        var inform = new RoleChangedInform
        {
            PartyIndex = 0,
            From = From,
            ChangedUnit = Target,
            OldRole = 0,
            NewRole = 2
        };

        byte[] expected = ExpectedInformBody(0, 0, 2);
        Span<byte> buffer = stackalloc byte[inform.MaxSize];
        int written = inform.WriteToSpan(buffer);

        Assert.Equal(expected.Length, written);
        Assert.True(expected.AsSpan().SequenceEqual(buffer[..written]));
    }

    [Fact]
    public void WritePacketData_MatchesWriteToSpan()
    {
        var inform = new RoleChangedInform
        {
            PartyIndex = 1,
            From = From,
            ChangedUnit = Target,
            OldRole = 2,
            NewRole = 8
        };

        inform.WritePacketData();
        Assert.Equal(ExpectedInformBody(1, 2, 8), inform.GetData());
    }

    [Fact]
    public void CloneUnwritten_CopiesRolesAssigned()
    {
        var src = new PartyUpdate
        {
            PartyIndex = 0,
            PlayerList =
            {
                new PartyPlayerInfo { GUID = From, Name = "A", VoiceStateID = "", RolesAssigned = 2 },
                new PartyPlayerInfo { GUID = Target, Name = "B", VoiceStateID = "", RolesAssigned = 8 },
            }
        };

        var copy = src.CloneUnwritten();
        Assert.Equal(2, copy.PlayerList[0].RolesAssigned);
        Assert.Equal(8, copy.PlayerList[1].RolesAssigned);
        Assert.NotSame(src.PlayerList, copy.PlayerList);
    }
}
