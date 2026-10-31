using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// The lists of opcodes left alone on purpose only speak for a packet that found no handler. An
/// entry that has one says nothing, and it says the wrong thing in the file: that the opcode is
/// not handled. These keep the lists and the dispatch tables apart.
/// </summary>
public sealed unsafe class DeliberatelyUnhandledOpcodesTests
{
    [Fact]
    public void NothingLeftAloneFromTheClient_HasAHandler()
    {
        foreach (Opcode opcode in DeliberatelyUnhandledOpcodes.FromClient)
            Assert.True(GeneratedCmsgDispatch.Get(opcode) == null, $"{opcode} has a [HandlesCmsg] handler");
    }

    [Fact]
    public void NothingLeftAloneFromTheServer_HasAHandler()
    {
        foreach (Opcode opcode in DeliberatelyUnhandledOpcodes.FromServer)
            Assert.True(GeneratedSmsgDispatch.Get(opcode) == null, $"{opcode} has a [HandlesSmsg] handler");
    }

    [Fact]
    public void TheListsNameTheirOwnDirection()
    {
        foreach (Opcode opcode in DeliberatelyUnhandledOpcodes.FromClient)
            Assert.StartsWith("CMSG_", opcode.ToString());
        foreach (Opcode opcode in DeliberatelyUnhandledOpcodes.FromServer)
            Assert.Matches("^S?MSG_", opcode.ToString());
    }

    [Theory]
    [InlineData(Opcode.CMSG_QUERY_REALM_NAME)]          // the proxy knows its realm's name
    [InlineData(Opcode.CMSG_GET_ITEM_PURCHASE_DATA)]    // a legacy server has the refund data
    [InlineData(Opcode.CMSG_REQUEST_CATEGORY_COOLDOWNS)] // the proxy answers that there are none
    public void ARequestThatCouldBeAnswered_StaysAWarning(Opcode opcode)
    {
        Assert.False(DeliberatelyUnhandledOpcodes.IsFromClient(opcode));
    }

    [Theory]
    [InlineData(Opcode.SMSG_RAID_MARKERS_CHANGED)]      // the 4.4.2 client draws raid markers
    [InlineData(Opcode.SMSG_LFG_UPDATE_STATUS)]         // the dungeon finder, not carried for 4.3.4 yet
    public void AServerPacketTheClientHasAPlaceFor_StaysAWarning(Opcode opcode)
    {
        Assert.False(DeliberatelyUnhandledOpcodes.IsFromServer(opcode));
    }
}
