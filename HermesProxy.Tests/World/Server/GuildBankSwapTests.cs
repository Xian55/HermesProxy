using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public sealed class GuildBankSwapTests
{
    /// <summary>
    /// Dropping a guild bank item on another one sends its own opcode. It was the one guild bank
    /// move with no handler, so the items stayed where they were and nothing was said.
    /// </summary>
    [Fact]
    public unsafe void SwapOpcode_HasAHandler()
        => Assert.True(GeneratedCmsgDispatch.Get(Opcode.CMSG_SWAP_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM) != null);

    /// <summary>
    /// A legacy server has one bank to bank packet and swaps when the target slot holds an item, so
    /// the swap goes out as a move does: the second slot first.
    /// </summary>
    [Fact]
    public void Swap_GoesOutAsABankToBankMove()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false, recordServerPackets: true);
        var ctx = new SessionContext(harness.Session, null, harness.Client);
        var vault = WowGuid128.Create(HighGuidType703.GameObject, 0, 187293, 42);

        GuildSystem.HandleMoveGuildBankItem(new MoveGuildBankItem(vault, BankTab1: 1, BankSlot1: 5, BankTab2: 2, BankSlot2: 7), in ctx);

        var sent = Assert.Single(harness.ServerWire.Sent);
        Assert.Equal(LegacyVersion.GetCurrentOpcode(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS), sent.Opcode);
        byte[] afterTheVault =
        [
            1,              // bank to bank
            2, 7,           // tab, slot
            0, 0, 0, 0,     // item id
            1, 5,           // tab, slot
            0, 0, 0, 0,     // item id
            0,              // no auto store
            0, 0, 0, 0,     // whole stack
        ];
        Assert.Equal(afterTheVault, sent.Bytes[^afterTheVault.Length..]);
    }
}
