using HermesProxy.Enums;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Shape-B systems forward the opcode they were dispatched with, so the universal opcode reaching
/// the handler must still translate to the legacy value that handler's own body would have chosen.
/// </summary>
/// <remarks>
/// A shape-B handler serves several opcodes from one body — <c>SocialSystem.HandleDelFriend</c>
/// covers remove-friend and remove-ignore — and writes <c>new WorldPacket(opcode)</c> rather than a
/// literal. That makes the universal-to-legacy table the only thing deciding what goes on the wire,
/// and a wrong entry there is invisible: the packet is well-formed, the size is right, and the
/// server simply performs a different action. These pin the social block, whose six opcodes sit in
/// one contiguous run where an off-by-one would still land on a real opcode.
/// </remarks>
public class ShapeBOpcodeForwardingTests
{
    [Theory]
    [InlineData(Opcode.CMSG_CONTACT_LIST, 0x066u)]
    [InlineData(Opcode.CMSG_ADD_FRIEND, 0x069u)]
    [InlineData(Opcode.CMSG_DEL_FRIEND, 0x06Au)]
    [InlineData(Opcode.CMSG_SET_CONTACT_NOTES, 0x06Bu)]
    [InlineData(Opcode.CMSG_ADD_IGNORE, 0x06Cu)]
    [InlineData(Opcode.CMSG_DEL_IGNORE, 0x06Du)]
    public void SocialOpcodes_TranslateToTheirLegacyValue(Opcode universal, uint expectedLegacy)
    {
        Assert.Equal(expectedLegacy,
            Opcodes.GetOpcodeValueForVersion(universal, ClientVersionBuild.V3_3_5a_12340));
    }

    /// <summary>
    /// The two opcodes that share <c>HandleDelFriend</c> must not collapse onto one legacy value —
    /// that is the specific way a shape-B body can silently do the wrong thing.
    /// </summary>
    [Fact]
    public void DelFriendAndDelIgnore_DoNotShareALegacyOpcode()
    {
        uint delFriend = Opcodes.GetOpcodeValueForVersion(Opcode.CMSG_DEL_FRIEND, ClientVersionBuild.V3_3_5a_12340);
        uint delIgnore = Opcodes.GetOpcodeValueForVersion(Opcode.CMSG_DEL_IGNORE, ClientVersionBuild.V3_3_5a_12340);

        Assert.NotEqual(delFriend, delIgnore);
        Assert.NotEqual(0u, delFriend);
        Assert.NotEqual(0u, delIgnore);
    }
}
