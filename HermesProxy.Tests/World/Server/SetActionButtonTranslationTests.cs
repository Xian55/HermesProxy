using System;
using Framework.IO;
using HermesProxy.Tests.Support;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Pins what <c>CMSG_SET_ACTION_BUTTON</c> becomes on its way to the legacy server. The client's
/// first four bytes are already the legacy packed value (low 24 bits action, high 8 bits type), so
/// the type is the high byte of the codec's second <c>ushort</c>. Taking it from the low byte
/// turned every item and macro into a spell, which the server refuses to store: the button was
/// gone at the next login, while spells, whose type is zero either way, survived (issue 358).
/// </summary>
public class SetActionButtonTranslationTests
{
    [Theory]
    [InlineData("882100802B", "2B88210080")] // item 8584 on slot 43, as a 1.14.2 client sent it
    [InlineData("0400004018", "1804000040")] // macro 4
    [InlineData("CB19000000", "00CB190000")] // spell 6603
    [InlineData("000000002B", "2B00000000")] // slot cleared
    public unsafe void SetActionButton_KeepsTheButtonTypeOnTheLegacyWire(string fromClient, string toServer)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false, recordServerPackets: true);
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
        var reader = new SpanPacketReader(Convert.FromHexString(fromClient));

        var handler = GeneratedCmsgDispatch.Get(Opcode.CMSG_SET_ACTION_BUTTON);
        Assert.True(handler != null);
        handler(ref reader, in ctx);

        Assert.Equal(0, reader.Remaining);
        var sent = Assert.Single(harness.ServerWire.Sent);
        Assert.Equal(Opcode.CMSG_SET_ACTION_BUTTON, LegacyVersion.GetUniversalOpcode(sent.Opcode));
        Assert.Equal(toServer, Convert.ToHexString(sent.Bytes));
    }
}
