using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client
    [PacketHandler(Opcode.CMSG_LOOT_RELEASE)]
    void HandleLootRelease(LootRelease loot)
    {
        GetSession().GameState.ExpectingLootReleaseResponse = true;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_RELEASE);
        packet.WriteGuid(loot.Owner.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_LOOT_ITEM)]
    void HandleLootItem(LootItemPkt loot)
    {
        var state = GetSession().GameState;

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
            SendPacketToServer(moneyPacket);
            state.LootMoneyPreClaimed = true;
        }

        foreach (var item in loot.Loot)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTOSTORE_LOOT_ITEM);
            packet.WriteUInt8(item.LootListID);
            SendPacketToServer(packet);
        }
    }

    [PacketHandler(Opcode.CMSG_LOOT_UNIT)]
    void HandleLootUnit(LootUnit loot)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_UNIT);
        packet.WriteGuid(loot.Unit.To64());
        SendPacketToServer(packet);
        GetSession().GameState.LastLootTargetGuid = loot.Unit.To64();
    }

    [PacketHandler(Opcode.CMSG_LOOT_MONEY)]
    void HandleLootMoney(LootMoney loot)
    {
        var state = GetSession().GameState;
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
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SET_LOOT_METHOD)]
    void HandleSetLootMethod(SetLootMethod loot)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_LOOT_METHOD);
        packet.WriteUInt32((uint)loot.LootMethod);
        packet.WriteGuid(loot.LootMasterGUID.To64());
        packet.WriteUInt32(loot.LootThreshold);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_OPT_OUT_OF_LOOT)]
    void HandleOptOutOfLoot(OptOutOfLoot loot)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_OPT_OUT_OF_LOOT);
            packet.WriteInt32(loot.PassOnLoot ? 1 : 0);
            SendPacketToServer(packet);
        }
        else
            GetSession().GameState.IsPassingOnLoot = loot.PassOnLoot;
    }

    [PacketHandler(Opcode.CMSG_LOOT_ROLL)]
    void HandleLootRoll(LootRoll loot)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_ROLL);
        packet.WriteGuid(loot.LootObj.To64());
        packet.WriteUInt32(loot.LootListID);
        packet.WriteUInt8((byte)loot.RollType);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_LOOT_MASTER_GIVE)]
    void HandleLootMasterGive(LootMasterGive loot)
    {
        foreach (var item in loot.Loot)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_MASTER_GIVE);
            packet.WriteGuid(item.LootObj.To64());
            packet.WriteUInt8(item.LootListID);
            packet.WriteGuid(loot.TargetGUID.To64());
            SendPacketToServer(packet);
        }
    }
}
