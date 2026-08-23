using System.Linq;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Many per-build opcode enums leave entries at 0 (no mapping for that client build).
/// Building a packet for one of those used to hit a Trace.Assert, which aborts the process
/// from a socket completion callback and bypasses every handler-level guard. It must throw
/// a catchable exception instead so the packet is dropped and logged.
/// </summary>
public class UnmappedOpcodeTests
{
    private sealed class TestServerPacket : ServerPacket
    {
        public TestServerPacket(Opcode universalOpcode) : base(universalOpcode) { }
        public override void Write() { }
    }

    private static Opcode FindUnmappedModernOpcode()
    {
        return System.Enum.GetValues<Opcode>()
            .First(op => op != Opcode.MSG_NULL_ACTION && ModernVersion.GetCurrentOpcode(op) == 0);
    }

    [Fact]
    public void ServerPacket_WithUnmappedOpcode_ThrowsInsteadOfAborting()
    {
        Opcode unmapped = FindUnmappedModernOpcode();

        var ex = Assert.Throws<UnmappedOpcodeException>(() => new TestServerPacket(unmapped));

        Assert.Equal(unmapped, ex.UniversalOpcode);
        Assert.True(ex.IsModern);
    }

    [Fact]
    public void ServerPacket_WithMappedOpcode_Succeeds()
    {
        var packet = new TestServerPacket(Opcode.SMSG_AUTH_RESPONSE);

        Assert.NotEqual(0u, packet.GetOpcode());
    }
}
