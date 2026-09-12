using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's loot CMSGs.
/// </summary>
/// <remarks>
/// The interesting part is the V3_4_3 auto-loot pair. A modern auto-loot sends
/// <c>CMSG_LOOT_ITEM</c> and <c>CMSG_LOOT_MONEY</c> together, but TC 3.3.5 master auto-loots every
/// item and closes the loot session on the <i>first</i> <c>CMSG_AUTOSTORE_LOOT_ITEM</c>, so the
/// money half then lands on an empty view and credits nothing.
/// <see cref="HandleLootItem"/> therefore claims the coins first and sets a flag that
/// <see cref="HandleLootMoney"/> consumes to drop the client's now-redundant half.
/// </remarks>
public static class LootSystem
{
    [HandlesCmsg(Opcode.CMSG_LOOT_RELEASE)]
    public static void HandleLootRelease(in LootRelease loot, in SessionContext ctx)
    {
        ctx.GetSession().GameState.ExpectingLootReleaseResponse = true;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_RELEASE);
        packet.WriteGuid(loot.Owner.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_LOOT_ITEM)]
    public static void HandleLootItem(in LootItemPkt loot, in SessionContext ctx)
    {
        var state = ctx.GetSession().GameState;

        // TC 3.3.5 master auto-loots all items + closes the loot session on the FIRST
        // CMSG_AUTOSTORE_LOOT_ITEM. Any unclaimed coins are orphaned (subsequent
        // CMSG_LOOT_MONEY lands on an empty AELootView and returns money=0, never
        // crediting the player). Pre-claim the gold *before* the item forward so the
        // server processes money first; the client's matching CMSG_LOOT_MONEY half of
        // the auto-loot pair is suppressed below in HandleLootMoney.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            && state.RemainingLootCoins > 0)
        {
            WorldPacket moneyPacket = new WorldPacket(Opcode.CMSG_LOOT_MONEY);
            ctx.SendPacketToServer(moneyPacket);
            state.LootMoneyPreClaimed = true;
        }

        foreach (var item in loot.Loot)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTOSTORE_LOOT_ITEM);
            packet.WriteUInt8(item.LootListID);
            ctx.SendPacketToServer(packet);
        }
    }

    [HandlesCmsg(Opcode.CMSG_LOOT_UNIT)]
    public static void HandleLootUnit(in LootUnit loot, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_UNIT);
        packet.WriteGuid(loot.Unit.To64());
        ctx.SendPacketToServer(packet);
        ctx.GetSession().GameState.LastLootTargetGuid = loot.Unit.To64();
    }

    [HandlesCmsg(Opcode.CMSG_LOOT_MONEY)]
    public static void HandleLootMoney(in LootMoney loot, in SessionContext ctx)
    {
        var state = ctx.GetSession().GameState;
        // V3_4_3 auto-loot pair: HandleLootItem already pre-claimed the gold to dodge
        // TC 3.3.5 master's session-close-on-item race. The client's matching
        // CMSG_LOOT_MONEY would now land on a closed legacy loot and produce
        // "+0 copper" feedback — drop it.
        if (state.LootMoneyPreClaimed)
        {
            state.LootMoneyPreClaimed = false;
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_MONEY);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_LOOT_METHOD)]
    public static void HandleSetLootMethod(in SetLootMethod loot, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_LOOT_METHOD);
        packet.WriteUInt32((uint)loot.LootMethod);
        packet.WriteGuid(loot.LootMasterGUID.To64());
        packet.WriteUInt32(loot.LootThreshold);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_OPT_OUT_OF_LOOT)]
    public static void HandleOptOutOfLoot(in OptOutOfLoot loot, in SessionContext ctx)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_OPT_OUT_OF_LOOT);
            packet.WriteInt32(loot.PassOnLoot ? 1 : 0);
            ctx.SendPacketToServer(packet);
        }
        else
            ctx.GetSession().GameState.IsPassingOnLoot = loot.PassOnLoot;
    }

    [HandlesCmsg(Opcode.CMSG_LOOT_ROLL)]
    public static void HandleLootRoll(in LootRoll loot, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_ROLL);
        packet.WriteGuid(loot.LootObj.To64());
        packet.WriteUInt32(loot.LootListID);
        packet.WriteUInt8((byte)loot.RollType);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_LOOT_MASTER_GIVE)]
    public static void HandleLootMasterGive(in LootMasterGive loot, in SessionContext ctx)
    {
        foreach (var item in loot.Loot)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_MASTER_GIVE);
            packet.WriteGuid(item.LootObj.To64());
            packet.WriteUInt8(item.LootListID);
            packet.WriteGuid(loot.TargetGUID.To64());
            ctx.SendPacketToServer(packet);
        }
    }
}
