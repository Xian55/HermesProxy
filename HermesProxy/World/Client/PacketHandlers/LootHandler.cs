using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_LOOT_RESPONSE)]
    internal void HandleLootResponse(WorldPacket packet)
    {
        LootResponse loot = new();
        var state = GetSession().GameState;
        // Reset before anything else so a failed loot doesn't leak stale counters.
        state.RemainingLootSlots.Clear();
        state.RemainingLootCurrencySlots.Clear();
        state.RemainingLootCoins = 0;
        state.ExpectingLootReleaseResponse = false;
        state.LootMoneyPreClaimed = false;

        state.LastLootTargetGuid = packet.ReadGuid();
        loot.Owner = state.LastLootTargetGuid.To128(state);
        loot.LootObj = state.LastLootTargetGuid.ToLootGuid();
        loot.AcquireReason = (LootType)packet.ReadUInt8();
        if (loot.AcquireReason == LootType.None)
        {
            loot.FailureReason = (LootError)packet.ReadUInt8();
            // The 3.4.3 and 4.4.2 clients say why from a response they did not acquire, as
            // TrinityCore wotlk_classic and cata_classic send one. Dropped, a second Pick Pocket on
            // the same target ("already pickpocketed") or a corpse someone else may loot answered
            // nothing at all.
            if (ModernVersion.IsWotLKClassicOrLater)
            {
                loot.Acquired = false;
                SendPacketToClient(loot);
            }
            return;
        }
        loot.LootMethod = state.GetCurrentLootMethod();

        loot.Coins = packet.ReadUInt32();

        var itemsCount = packet.ReadUInt8();
        // 4.3.4 counts the currencies here too and lists them after the items (TrinityCore 4.3.4
        // LootResponse::Write). Read as 3.3.5a, that count became the first item's slot and the
        // loot window never opened.
        byte currencyCount = IsCataLegacy ? packet.ReadUInt8() : (byte)0;
        for (var i = 0; i < itemsCount; ++i)
        {
            LootItemData lootItem = new();
            lootItem.LootListID = packet.ReadUInt8();
            lootItem.Loot.ItemID = packet.ReadUInt32();
            lootItem.Quantity = packet.ReadUInt32();
            packet.ReadUInt32(); // DisplayID
            lootItem.Loot.RandomPropertiesSeed = packet.ReadUInt32();
            lootItem.Loot.RandomPropertiesID = packet.ReadUInt32();
            var uiType = (LootSlotTypeLegacy)packet.ReadUInt8();
            lootItem.UIType = uiType.CastEnum<LootSlotTypeModern>();
            loot.Items.Add(lootItem);
            state.RemainingLootSlots.Add(lootItem.LootListID);
        }
        // A currency slot counts from the same list as the items. Its amount is as the server
        // stores it, in hundredths for the currencies the client keeps in hundredths.
        for (var i = 0; i < currencyCount; ++i)
        {
            LootCurrency currency = new();
            currency.LootListID = packet.ReadUInt8();
            currency.CurrencyID = ModernCurrencyId(packet.ReadUInt32());
            currency.Quantity = packet.ReadUInt32();
            currency.UIType = (byte)LootSlotTypeModern.AllowLoot;
            loot.Currencies.Add(currency);
            state.RemainingLootCurrencySlots.Add(currency.LootListID);
        }
        state.RemainingLootCoins = loot.Coins;
        SendMasterLootListIfApplicable();
        SendPacketToClient(loot);
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_RELEASE)]
    internal void HandleLootRelease(WorldPacket packet)
    {
        LootReleaseResponse loot = new();
        WowGuid64 owner = packet.ReadGuid();
        var state = GetSession().GameState;
        loot.LootObj = owner.ToLootGuid();
        packet.ReadBool(); // unk

        // Legacy 3.3.5a sends an unsolicited SMSG_LOOT_RELEASE after each auto-looted item,
        // which the V3_4_3 client treats as "close the session." That makes subsequent
        // SMSG_LOOT_REMOVED packets for the remaining slots no-ops on the UI side.
        // Only forward when (a) gate is inactive, (b) client itself requested release, or
        // (c) the loot is genuinely drained.
        bool gateActive = ModernVersion.IsWotLKClassicOrLater;
        bool clientAsked = state.ExpectingLootReleaseResponse;
        bool drained = state.LootDrained;

        // V3_4_3 client expects Owner = the looting player's own GUID (see TC's
        // Player::SendLootRelease in HermesProxy-WOTLK/tmp_certs/tc_player.cpp:8700-8705).
        // Legacy 3.3.5a sent the corpse GUID; previous proxies forwarded that verbatim,
        // which is why the modern UI silently ignored every close on this version.
        loot.Owner = gateActive ? state.CurrentPlayerGuid : owner.To128(state);

        if (!gateActive || clientAsked || drained)
        {
            SendPacketToClient(loot);
            state.LastMasterLootSentTarget = default;
            state.ExpectingLootReleaseResponse = false;
            state.RemainingLootSlots.Clear();
            state.RemainingLootCurrencySlots.Clear();
            state.RemainingLootCoins = 0;
            state.LootMoneyPreClaimed = false;
        }
        // else: suppress; session is logically still open. Leave LastMasterLootSentTarget.
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_REMOVED)]
    internal void HandleLootRemoved(WorldPacket packet)
    {
        LootRemoved loot = new();
        var state = GetSession().GameState;
        loot.Owner = state.LastLootTargetGuid.To128(state);
        loot.LootObj = state.LastLootTargetGuid.ToLootGuid();
        byte legacyByte = packet.ReadUInt8();

        // TC 3.3.5 master's auto-loot drains the whole corpse from one
        // CMSG_AUTOSTORE_LOOT_ITEM and echoes the *clicked* slot byte in every
        // SMSG_LOOT_REMOVED, not the slot of the item actually being removed. Resolve
        // the legacy byte against the remaining-slots list: take it if it's still in
        // the set, otherwise pop the first remaining slot (legacy order from Response).
        // cMaNGOS / older legacy backends always hit the first branch (real slot
        // numbers), so this is a no-op for them.
        byte resolvedSlot = legacyByte;
        int slotIndex = state.RemainingLootSlots.IndexOf(legacyByte);
        if (slotIndex >= 0)
        {
            state.RemainingLootSlots.RemoveAt(slotIndex);
        }
        else if (state.RemainingLootSlots.Count > 0)
        {
            resolvedSlot = state.RemainingLootSlots[0];
            state.RemainingLootSlots.RemoveAt(0);
        }
        loot.LootListID = resolvedSlot;
        SendPacketToClient(loot);

        if (ModernVersion.IsWotLKClassicOrLater
            && state.LootDrained)
        {
            // Last item drained: synthesize the close that we will have suppressed (or
            // that the legacy server may still send) so the V3_4_3 UI auto-closes.
            LootReleaseResponse synth = new();
            synth.Owner = state.CurrentPlayerGuid;
            synth.LootObj = state.LastLootTargetGuid.ToLootGuid();
            SendPacketToClient(synth);
            state.LastMasterLootSentTarget = default;
        }
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>SendNotifyCurrencyLootRemoved</c>: the slot of a currency taken out of
    /// the loot. The 4.4.2 client has no packet of its own for that and clears a currency slot as
    /// it clears an item's.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_CURRENCY_LOOT_REMOVED)]
    internal void HandleCurrencyLootRemoved(WorldPacket packet)
    {
        var state = GetSession().GameState;
        LootRemoved loot = new();
        loot.Owner = state.LastLootTargetGuid.To128(state);
        loot.LootObj = state.LastLootTargetGuid.ToLootGuid();
        loot.LootListID = packet.ReadUInt8();
        state.RemainingLootCurrencySlots.Remove(loot.LootListID);
        SendPacketToClient(loot);

        // The last of it: the close the client waits for, as for the last item.
        if (state.LootDrained)
        {
            LootReleaseResponse synth = new();
            synth.Owner = state.CurrentPlayerGuid;
            synth.LootObj = state.LastLootTargetGuid.ToLootGuid();
            SendPacketToClient(synth);
            state.LastMasterLootSentTarget = default;
        }
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_MONEY_NOTIFY)]
    internal void HandleLootMoneyNotify(WorldPacket packet)
    {
        LootMoneyNotify loot = new();
        loot.Money = packet.ReadUInt32();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            loot.SoleLooter = packet.ReadBool();
        SendPacketToClient(loot);

        // V3_4_3 close synthesis for the coin path lives here, not in HandleLootCelarMoney:
        // the legacy server sends SMSG_LOOT_CLEAR_MONEY first and SMSG_LOOT_MONEY_NOTIFY
        // second, and the V3_4_3 client refuses to auto-close if the release arrives
        // *before* the notify. Fire here once the session is fully drained.
        var state = GetSession().GameState;
        if (ModernVersion.IsWotLKClassicOrLater
            && state.LootDrained)
        {
            LootReleaseResponse synth = new();
            synth.Owner = state.CurrentPlayerGuid;
            synth.LootObj = state.LastLootTargetGuid.ToLootGuid();
            SendPacketToClient(synth);
            state.LastMasterLootSentTarget = default;
        }
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_CLEAR_MONEY)]
    internal void HandleLootCelarMoney(WorldPacket packet)
    {
        CoinRemoved loot = new();
        var state = GetSession().GameState;
        loot.LootObj = state.LastLootTargetGuid.ToLootGuid();
        SendPacketToClient(loot);
        state.RemainingLootCoins = 0;

        // The closing synth is deferred to HandleLootMoneyNotify — the V3_4_3 client
        // expects SMSG_COIN_REMOVED → SMSG_LOOT_MONEY_NOTIFY → SMSG_LOOT_RELEASE in
        // that order; synthesizing here would put the release before the notify on
        // the wire and the UI refuses to auto-close.
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_START_ROLL)]
    internal void HandleLootStartRoll(WorldPacket packet)
    {
        StartLootRoll loot = new StartLootRoll();
        WowGuid64 owner = packet.ReadGuid();
        loot.LootObj = owner.ToLootGuid();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            loot.MapID = packet.ReadUInt32();
        else
            loot.MapID = (uint)GetSession().GameState.CurrentMapId!;
        loot.Item.LootListID = (byte)packet.ReadUInt32();
        loot.Item.Loot.ItemID = packet.ReadUInt32();
        loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
        loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            loot.Item.Quantity = packet.ReadUInt32();
        else
            loot.Item.Quantity = 1;
        loot.RollTime = packet.ReadUInt32();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            loot.ValidRolls = (RollMask)packet.ReadUInt8();
        else
            loot.ValidRolls = RollMask.AllNoDisenchant;

        // Only this packet and SMSG_LOOT_ALL_PASSED carry the loot object on 3.3.5a; the
        // result packets send an empty guid. Remember it so they can be given the same one.
        var rolls = GetSession().GameState.LootRolls;
        long now = Environment.TickCount64;
        rolls.RemoveAll(roll => roll.ExpiresAt <= now);
        rolls.Add(new(loot.Item.LootListID, loot.Item.Loot.ItemID, loot.LootObj, now + 2L * loot.RollTime));

        SendPacketToClient(loot);

        if (GetSession().GameState.IsPassingOnLoot)
        {
            WorldPacket packet2 = new WorldPacket(Opcode.CMSG_LOOT_ROLL);
            packet2.WriteGuid(owner);
            packet2.WriteUInt32(loot.Item.LootListID);
            packet2.WriteUInt8((byte)RollType.Pass);
            SendPacketToServer(packet2);
        }
    }

    /// <summary>
    /// 3.3.5a sends <c>ObjectGuid::Empty</c> as the loot object on SMSG_LOOT_ROLL and
    /// SMSG_LOOT_ROLL_WON (AzerothCore Group.cpp SendLootRoll / SendLootRollWon), so the
    /// value on the wire cannot be forwarded — the modern client needs the same LootObj it
    /// was given in SMSG_LOOT_START_ROLL or it cannot match the roll to the open dialog.
    /// Falls back to whatever the packet carried when the slot was never announced, which
    /// keeps behaviour unchanged on a backend that does populate it.
    /// </summary>
    private WowGuid128 ResolveLootRollObject(byte lootListId, uint itemId, WowGuid64 legacyOwner, WowGuid128 converted)
    {
        if (!legacyOwner.IsEmpty())
            return converted;

        foreach (LootRollEntry roll in GetSession().GameState.LootRolls)
        {
            if (roll.LootListID == lootListId && roll.ItemID == itemId)
                return roll.LootObj;
        }

        return converted;
    }

    private void ForgetLootRoll(byte lootListId, uint itemId, WowGuid128 lootObj)
    {
        var rolls = GetSession().GameState.LootRolls;
        int index = rolls.FindIndex(roll => roll.LootListID == lootListId && roll.ItemID == itemId && roll.LootObj == lootObj);
        if (index >= 0)
            rolls.RemoveAt(index);
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_ROLL)]
    internal void HandleLootRoll(WorldPacket packet)
    {
        LootRollBroadcast loot = new LootRollBroadcast();
        WowGuid64 owner = packet.ReadGuid();
        loot.LootObj = owner.ToLootGuid();
        loot.Item.LootListID = (byte)packet.ReadUInt32();
        loot.Player = packet.ReadGuid().To128(GetSession().GameState);
        loot.Item.Loot.ItemID = packet.ReadUInt32();
        loot.LootObj = ResolveLootRollObject(loot.Item.LootListID, loot.Item.Loot.ItemID, owner, loot.LootObj);
        loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
        loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
        loot.Item.Quantity = 1;
        if (IsCataLegacy)
        {
            // TrinityCore 4.3.4 Group::SendLootRoll: the roll is an int32, and a choice just made
            // is a negative one (-1 pass, -7 greed, -8 disenchant; need is 0 with type 0) where
            // 3.3.5a sent 128. Read as a byte, the type came out of the middle of the number.
            int rollNumber = packet.ReadInt32();
            byte cataRollType = packet.ReadUInt8();
            if (rollNumber == -1)
                loot.RollType = RollType.Pass;
            else if (rollNumber == 0 && cataRollType == 0)
                loot.RollType = RollType.Need;
            else
                loot.RollType = (RollType)cataRollType;
            loot.Roll = rollNumber > 0 ? rollNumber : 0;
            loot.Autopassed = packet.ReadBool();
            SendPacketToClient(loot);
            return;
        }

        loot.Roll = packet.ReadUInt8();

        byte rollType = packet.ReadUInt8();
        if (loot.Roll == 128 && rollType == 128)
            loot.RollType = RollType.Pass;
        else if (loot.Roll == 0 && rollType == 0)
            loot.RollType = RollType.Need;
        else
            loot.RollType = (RollType) rollType;

        if (loot.Roll == 128)
            loot.Roll = 0;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            loot.Autopassed = packet.ReadBool();

        SendPacketToClient(loot);
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_ROLL_WON)]
    internal void HandleLootRollWon(WorldPacket packet)
    {
        LootRollWon loot = new LootRollWon();
        WowGuid64 wonOwner = packet.ReadGuid();
        loot.LootObj = wonOwner.ToLootGuid();
        loot.Item.LootListID = (byte)packet.ReadUInt32();
        loot.Item.Loot.ItemID = packet.ReadUInt32();
        loot.LootObj = ResolveLootRollObject(loot.Item.LootListID, loot.Item.Loot.ItemID, wonOwner, loot.LootObj);
        loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
        loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
        loot.Item.Quantity = 1;
        loot.Winner = packet.ReadGuid().To128(GetSession().GameState);
        // 4.3.4 Group::SendLootRollWon: the roll is an int32 here too.
        loot.Roll = IsCataLegacy ? packet.ReadInt32() : packet.ReadUInt8();
        loot.RollType = (RollType)packet.ReadUInt8();
        if (loot.RollType == RollType.Need)
            loot.MainSpec = 128;
        SendPacketToClient(loot);

        LootRollsComplete complete = new LootRollsComplete();
        complete.LootObj = loot.LootObj;
        complete.LootListID = loot.Item.LootListID;
        SendPacketToClient(complete);

        ForgetLootRoll(loot.Item.LootListID, loot.Item.Loot.ItemID, loot.LootObj);
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_ALL_PASSED)]
    internal void HandleLootAllPassed(WorldPacket packet)
    {
        LootAllPassed loot = new LootAllPassed();
        loot.LootObj = packet.ReadGuid().ToLootGuid();
        loot.Item.LootListID = (byte)packet.ReadUInt32();
        loot.Item.Loot.ItemID = packet.ReadUInt32();
        loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
        loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
        loot.Item.Quantity = 1;
        SendPacketToClient(loot);

        LootRollsComplete complete = new LootRollsComplete();
        complete.LootObj = loot.LootObj;
        complete.LootListID = loot.Item.LootListID;
        SendPacketToClient(complete);

        ForgetLootRoll(loot.Item.LootListID, loot.Item.Loot.ItemID, loot.LootObj);
    }

    /// <summary>
    /// Who may loot a creature that died to a group: its master looter and its round robin
    /// winner, sent to the group at the kill (3.3.5a and 4.3.4: the creature's full GUID, then
    /// both as packed GUIDs, empty when there is none). The modern packet also names the loot
    /// object, the one <see cref="HandleLootResponse"/> gives the client for that creature.
    /// </summary>
    /// <remarks>
    /// Passed on to a 3.4.3 or later client, whose layout and opcode were read from the client
    /// (3.4.3 and 4.4.2). The 1.14 and 2.5 tables have not been checked for this opcode.
    /// </remarks>
    [HandlesSmsg(Opcode.SMSG_LOOT_LIST)]
    internal void HandleLootList(WorldPacket packet)
    {
        if (!ModernVersion.IsWotLKClassicOrLater)
            return;

        var gameState = GetSession().GameState;
        WowGuid64 owner = packet.ReadGuid();
        WowGuid64 master = packet.ReadPackedGuid();
        WowGuid64 roundRobinWinner = packet.ReadPackedGuid();

        LootList list = new LootList();
        list.Owner = owner.To128(gameState);
        list.LootObj = owner.ToLootGuid();
        if (!master.IsEmpty())
            list.Master = master.To128(gameState);
        if (!roundRobinWinner.IsEmpty())
            list.RoundRobinWinner = roundRobinWinner.To128(gameState);
        SendPacketToClient(list);
    }

    [HandlesSmsg(Opcode.SMSG_LOOT_MASTER_LIST)]
    internal void HandleLootMasterList(WorldPacket packet)
    {
        // Cache the candidate list -- do NOT send packets here.
        // The legacy server sends this only once per corpse (to whoever loots first),
        // so we defer sending until HandleLootResponse where we can check
        // whether the current player is actually the master looter.
        byte count = packet.ReadUInt8();
        var candidates = new List<WowGuid128>(count);
        for (byte i = 0; i < count; i++)
            candidates.Add(packet.ReadGuid().To128(GetSession().GameState));
        GetSession().GameState.MasterLootCandidates = candidates;
    }

    void SendMasterLootListIfApplicable()
    {
        var gameState = GetSession().GameState;

        // Only send once per corpse open. The server re-sends SMSG_LOOT_RESPONSE
        // after each master loot distribution; re-sending LootList would cause the
        // client to auto-open the master loot popup for the wrong (stale) item.
        if (gameState.LastMasterLootSentTarget == gameState.LastLootTargetGuid)
            return;

        var group = gameState.GetCurrentGroup();
        if (group?.LootSettings is not { Method: LootMethod.MasterLoot })
            return;

        if (group.LootSettings.LootMaster != gameState.CurrentPlayerGuid)
            return;

        var candidates = gameState.MasterLootCandidates;

        LootList list = new LootList();
        list.Owner = gameState.LastLootTargetGuid.To128(gameState);
        list.LootObj = gameState.LastLootTargetGuid.ToLootGuid();
        list.Master = gameState.CurrentPlayerGuid;
        SendPacketToClient(list);

        MasterLootCandidateList candidateList = new MasterLootCandidateList();
        candidateList.LootObj = gameState.LastLootTargetGuid.ToLootGuid();
        if (candidates != null)
            candidateList.Players.AddRange(candidates);
        else
            candidateList.Players.AddRange(group.PlayerList.Select(p => p.GUID));
        SendPacketToClient(candidateList);

        gameState.LastMasterLootSentTarget = gameState.LastLootTargetGuid;
    }
}
