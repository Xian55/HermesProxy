using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.V3_4_3_54261;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

// Byte-equivalence oracle for Phase 5b Player section. WriteCreatePlayerData /
// WriteUpdatePlayerData / HasAnyPlayerFieldSet are now generator-emitted from
// V3_4_3_54261.PlayerField. Custom writers on builder partial handle: Customizations
// count + variable-length data, QuestLog (Create owner-gated + Update PerElement),
// VisibleItems (Create always-write fallback + Update PerElement inner 4-bit mask),
// BnetAccount (owner-only GlobalSession lookup), LogoutTime (owner-only Time.UnixTime).
//
// Update-path new generator feature: DescriptorUpdateBitsPreamble emits the
// IsQuestLogChangesMaskSkipped = true literal bit unconditionally between blocks-mask
// + FlushBits.
//
// Tests use a non-owner Player GUID (IsOwner=false) so the LogoutTime/BnetAccount
// owner-only paths are skipped — those exercise GlobalSession + Time, which would
// require additional stubbing. The byte-eq oracle still validates the Create + Update
// emit order, scalar widths, Customizations variable-length, QuestLog Update
// PerElement, VisibleItems Update inner mask, and the IsQuestLogChangesMaskSkipped
// preamble.
public class PlayerSectionEquivalenceTests
{
    private static GlobalSessionData CreateGlobalSession()
        => (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

    private static GameSessionData CreateGameSession()
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new System.Collections.Generic.Dictionary<WowGuid128, ObjectType>());
        return session;
    }

    private static ObjectUpdateBuilder MakeBuilder(WowGuid128 guid, GameSessionData session, out ObjectUpdate update)
    {
        var globalSession = CreateGlobalSession();
        update = new ObjectUpdate(guid, UpdateTypeModern.Values, globalSession);
        if (update.PlayerData == null) update.PlayerData = new PlayerData();
        return new ObjectUpdateBuilder(update, session);
    }

    // ---------------------------------------------------------------------
    // WriteCreate — byte-eq vs frozen hand-port
    // ---------------------------------------------------------------------

    public static System.Collections.Generic.IEnumerable<object[]> CreateScenarios()
    {
        yield return new object[] { "empty", (System.Action<PlayerData>)(_ => { }) };
        yield return new object[] { "scalars", (System.Action<PlayerData>)(p =>
        {
            p.DuelArbiter = WowGuid128.Create(HighGuidType703.Player, 12);
            p.WowAccount = WowGuid128.Create(HighGuidType703.WowAccount, 7);
            p.LootTargetGUID = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 99);
            p.PlayerFlags = 0x100u;
            p.PlayerFlagsEx = 0x4u;
            p.GuildRankID = 3u;
            p.GuildDeleteDate = 0u;
            p.GuildLevel = 25;
            p.PartyType = 0x12;
            p.NumBankSlots = 6;
            p.NativeSex = 1;
            p.Inebriation = 0;
            p.PvpTitle = 0;
            p.ArenaFaction = 0;
            p.PvPRank = 0;
            p.DuelTeam = 0u;
            p.GuildTimeStamp = 1700000000;
            p.ChosenTitle = 42;
            p.VirtualPlayerRealm = 1u;
            p.CurrentSpecID = 264u;
            p.HonorLevel = 5;
        }) };
        yield return new object[] { "visible-items-partial", (System.Action<PlayerData>)(p =>
        {
            p.VisibleItems[0] = new VisibleItem { ItemID = 12345, ItemAppearanceModID = 1, ItemVisual = 0 };
            p.VisibleItems[5] = new VisibleItem { ItemID = 67890, ItemAppearanceModID = 2, ItemVisual = 100 };
            p.VisibleItems[18] = new VisibleItem { ItemID = 99999, ItemAppearanceModID = 0, ItemVisual = 0 };
        }) };
        yield return new object[] { "customizations-some", (System.Action<PlayerData>)(p =>
        {
            p.Customizations[0] = new ChrCustomizationChoice(1u, 11u);
            p.Customizations[3] = new ChrCustomizationChoice(4u, 44u);
            p.Customizations[35] = new ChrCustomizationChoice(36u, 360u);
        }) };
        yield return new object[] { "customizations-all", (System.Action<PlayerData>)(p =>
        {
            for (int i = 0; i < 36; i++)
                p.Customizations[i] = new ChrCustomizationChoice((uint)(i + 1), (uint)((i + 1) * 10));
        }) };
    }

    [Theory]
    [MemberData(nameof(CreateScenarios))]
    public void WriteCreatePlayerData_GeneratedMatchesHandPort(string _label, System.Action<PlayerData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.PlayerData!);

        var actual = new WorldPacket();
        builder.WriteCreatePlayerData(actual);

        var expected = new WorldPacket();
        WriteCreatePlayerData_HandPort(expected, update.PlayerData!);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // WriteUpdate — byte-eq vs frozen hand-port
    // ---------------------------------------------------------------------

    public static System.Collections.Generic.IEnumerable<object[]> UpdateScenarios()
    {
        yield return new object[] { "empty", (System.Action<PlayerData>)(_ => { }) };
        yield return new object[] { "scalars-block0",  (System.Action<PlayerData>)(p =>
        {
            p.PlayerFlags = 0x10u;
            p.GuildLevel = 12;
            p.HonorLevel = 4;
            p.ChosenTitle = 99;
        }) };
        yield return new object[] { "guid-scalars",   (System.Action<PlayerData>)(p =>
        {
            p.DuelArbiter = WowGuid128.Create(HighGuidType703.Player, 5);
            p.LootTargetGUID = WowGuid128.Create(HighGuidType703.Creature, 0, 1, 1);
        }) };
        yield return new object[] { "byte-scalars",  (System.Action<PlayerData>)(p =>
        {
            p.NumBankSlots = 4;
            p.NativeSex = 0;
            p.PvPRank = 3;
        }) };
        yield return new object[] { "questlog-single", (System.Action<PlayerData>)(p =>
        {
            p.QuestLog[0] = new QuestLog { QuestID = 1234, EndTime = 1700000000u, StateFlags = 0u };
            p.QuestLog[0].ObjectiveProgress[0] = 5;
        }) };
        yield return new object[] { "questlog-multi", (System.Action<PlayerData>)(p =>
        {
            for (int i = 0; i < 5; i++)
            {
                p.QuestLog[i] = new QuestLog { QuestID = 1000 + i, EndTime = (uint)i, StateFlags = 1u };
                for (int o = 0; o < 24; o++) p.QuestLog[i].ObjectiveProgress[o] = (short)(o + i);
            }
        }) };
        yield return new object[] { "visible-items-update", (System.Action<PlayerData>)(p =>
        {
            p.VisibleItems[0] = new VisibleItem { ItemID = 100, ItemAppearanceModID = 1, ItemVisual = 0 };
            p.VisibleItems[16] = new VisibleItem { ItemID = 200, ItemAppearanceModID = 2, ItemVisual = 50 };
            p.VisibleItems[18] = new VisibleItem { ItemID = 300, ItemAppearanceModID = 0, ItemVisual = 0 };
        }) };
        yield return new object[] { "all-block0-plus-quest-plus-items", (System.Action<PlayerData>)(p =>
        {
            p.DuelArbiter = WowGuid128.Create(HighGuidType703.Player, 1);
            p.PlayerFlags = 1u;
            p.PlayerFlagsEx = 2u;
            p.GuildRankID = 3u;
            p.GuildLevel = 4;
            p.NumBankSlots = 5;
            p.NativeSex = 0;
            p.HonorLevel = 6;
            p.ChosenTitle = 7;
            p.FakeInebriation = 8;
            p.VirtualPlayerRealm = 9u;
            p.CurrentSpecID = 10u;
            p.GuildTimeStamp = 11;
            p.DuelTeam = 12u;
            p.QuestLog[2] = new QuestLog { QuestID = 200, EndTime = 0u, StateFlags = 0u };
            p.VisibleItems[7] = new VisibleItem { ItemID = 77, ItemAppearanceModID = 0, ItemVisual = 0 };
        }) };
    }

    [Theory]
    [MemberData(nameof(UpdateScenarios))]
    public void WriteUpdatePlayerData_GeneratedMatchesHandPort(string _label, System.Action<PlayerData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.PlayerData!);

        var actual = new WorldPacket();
        builder.WriteUpdatePlayerData(actual);

        var expected = new WorldPacket();
        WriteUpdatePlayerData_HandPort(expected, update.PlayerData!);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // HasAny — semantic eq
    // ---------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(UpdateScenarios))]
    public void HasAnyPlayerFieldSet_GeneratedMatchesHandPort(string _label, System.Action<PlayerData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.PlayerData!);

        Assert.Equal(HasAnyPlayerFieldSet_HandPort(update.PlayerData!), builder.HasAnyPlayerFieldSet());
    }

    // ---------------------------------------------------------------------
    // Inlined pre-Phase-5b hand-port — frozen oracle. Identical to bodies
    // removed from V3_4_3_54261/ObjectUpdateBuilder.cs (lines 749-856 Create /
    // 1446-1469 HasAny / 2360-2498 Update before deletion). IsOwner=false for
    // all tests so owner-gated QuestLog Create, LogoutTime, and BnetAccount
    // paths drop out symmetrically in both implementations.
    // ---------------------------------------------------------------------

    private static void WriteCreatePlayerData_HandPort(WorldPacket data, PlayerData player)
    {
        data.WritePackedGuid128(player.DuelArbiter ?? WowGuid128.Empty);
        data.WritePackedGuid128(player.WowAccount ?? WowGuid128.Empty);
        data.WritePackedGuid128(player.LootTargetGUID ?? WowGuid128.Empty);
        data.WriteUInt32(player.PlayerFlags.GetValueOrDefault());
        data.WriteUInt32(player.PlayerFlagsEx.GetValueOrDefault());
        data.WriteUInt32(player.GuildRankID.GetValueOrDefault());
        data.WriteUInt32(player.GuildDeleteDate.GetValueOrDefault());
        data.WriteInt32(player.GuildLevel.GetValueOrDefault());

        int customizationCount = 0;
        for (int i = 0; i < player.Customizations.Length; i++)
            if (player.Customizations[i] != null) customizationCount++;
        data.WriteUInt32((uint)customizationCount);

        data.WriteUInt8(player.PartyType.GetValueOrDefault());
        data.WriteUInt8(0);
        data.WriteUInt8(player.NumBankSlots.GetValueOrDefault());
        data.WriteUInt8(player.NativeSex.GetValueOrDefault());
        data.WriteUInt8(player.Inebriation.GetValueOrDefault());
        data.WriteUInt8(player.PvpTitle.GetValueOrDefault());
        data.WriteUInt8(player.ArenaFaction.GetValueOrDefault());
        data.WriteUInt8(player.PvPRank.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteUInt32(player.DuelTeam.GetValueOrDefault());
        data.WriteInt32(player.GuildTimeStamp.GetValueOrDefault());

        // IsOwner=false → QuestLog Create skipped

        for (int j = 0; j < 19; j++)
        {
            if (player.VisibleItems != null && j < player.VisibleItems.Length
                && player.VisibleItems[j] is VisibleItem pv)
            {
                data.WriteInt32(pv.ItemID);
                data.WriteUInt16(pv.ItemAppearanceModID);
                data.WriteUInt16(pv.ItemVisual);
            }
            else
            {
                data.WriteInt32(0);
                data.WriteUInt16(0);
                data.WriteUInt16(0);
            }
        }

        data.WriteInt32(player.ChosenTitle.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteUInt32(player.VirtualPlayerRealm.GetValueOrDefault());
        data.WriteUInt32(player.CurrentSpecID.GetValueOrDefault());
        data.WriteInt32(0);
        for (int k = 0; k < 6; k++) data.WriteFloat(0f);
        data.WriteUInt8(0);
        data.WriteInt32(player.HonorLevel.GetValueOrDefault());
        // LogoutTime: IsOwner=false → 0L
        data.WriteInt64(0L);
        data.WriteUInt32(0u);
        data.WriteInt32(0);
        // BnetAccount: IsOwner=false → Empty
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteUInt32(0u);
        for (int l = 0; l < 19; l++) data.WriteUInt32(0u);
        for (int m = 0; m < player.Customizations.Length; m++)
        {
            var choice = player.Customizations[m];
            if (choice != null)
            {
                data.WriteUInt32(choice.ChrCustomizationOptionID);
                data.WriteUInt32(choice.ChrCustomizationChoiceID);
            }
        }
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteUInt32(0u);
    }

    private static void WriteUpdatePlayerData_HandPort(WorldPacket data, PlayerData p)
    {
        System.Span<uint> blocksBuf = stackalloc uint[4];
        var blocks = new global::Framework.Util.StackBitMask(blocksBuf);
        if (p.DuelArbiter != null) { blocks.SetBit(0); blocks.SetBit(4); }
        if (p.WowAccount != null) { blocks.SetBit(0); blocks.SetBit(5); }
        if (p.LootTargetGUID != null) { blocks.SetBit(0); blocks.SetBit(6); }
        if (p.PlayerFlags.HasValue) { blocks.SetBit(0); blocks.SetBit(7); }
        if (p.PlayerFlagsEx.HasValue) { blocks.SetBit(0); blocks.SetBit(8); }
        if (p.GuildRankID.HasValue) { blocks.SetBit(0); blocks.SetBit(9); }
        if (p.GuildDeleteDate.HasValue) { blocks.SetBit(0); blocks.SetBit(10); }
        if (p.GuildLevel.HasValue) { blocks.SetBit(0); blocks.SetBit(11); }
        if (p.NumBankSlots.HasValue) { blocks.SetBit(0); blocks.SetBit(12); }
        if (p.NativeSex.HasValue) { blocks.SetBit(0); blocks.SetBit(13); }
        if (p.Inebriation.HasValue) { blocks.SetBit(0); blocks.SetBit(14); }
        if (p.PvpTitle.HasValue) { blocks.SetBit(0); blocks.SetBit(15); }
        if (p.ArenaFaction.HasValue) { blocks.SetBit(0); blocks.SetBit(16); }
        if (p.PvPRank.HasValue) { blocks.SetBit(0); blocks.SetBit(17); }
        if (p.DuelTeam.HasValue) { blocks.SetBit(0); blocks.SetBit(19); }
        if (p.GuildTimeStamp.HasValue) { blocks.SetBit(0); blocks.SetBit(20); }
        if (p.ChosenTitle.HasValue) { blocks.SetBit(0); blocks.SetBit(21); }
        if (p.FakeInebriation.HasValue) { blocks.SetBit(0); blocks.SetBit(22); }
        if (p.VirtualPlayerRealm.HasValue) { blocks.SetBit(0); blocks.SetBit(23); }
        if (p.CurrentSpecID.HasValue) { blocks.SetBit(0); blocks.SetBit(24); }
        if (p.HonorLevel.HasValue) { blocks.SetBit(0); blocks.SetBit(27); }

        bool hasAnyQuestLog = false;
        for (int i = 0; i < QuestConst.MaxQuestLogSize; i++)
        {
            if (p.QuestLog[i] != null && p.QuestLog[i].QuestID.HasValue)
            {
                blocks.SetBit(35); blocks.SetBit(36 + i); hasAnyQuestLog = true;
            }
        }
        bool hasAnyVisibleItem = false;
        for (int i = 0; i < 19; i++)
        {
            if (p.VisibleItems != null && i < p.VisibleItems.Length && p.VisibleItems[i] != null)
            {
                blocks.SetBit(61); blocks.SetBit(62 + i); hasAnyVisibleItem = true;
            }
        }

        byte blocksMask = 0;
        for (int i = 0; i < 4; i++) if (blocks[i] != 0) blocksMask |= (byte)(1 << i);
        data.WriteBits(blocksMask, 4);
        for (int i = 0; i < 4; i++) if ((blocksMask & (1 << i)) != 0) data.WriteBits(blocks[i], 32);
        data.WriteBit(true);
        data.FlushBits();

        if (blocks.IsBitSet(0))
        {
            if (blocks.IsBitSet(4)) data.WritePackedGuid128(p.DuelArbiter!.Value);
            if (blocks.IsBitSet(5)) data.WritePackedGuid128(p.WowAccount!.Value);
            if (blocks.IsBitSet(6)) data.WritePackedGuid128(p.LootTargetGUID!.Value);
            if (blocks.IsBitSet(7)) data.WriteUInt32(p.PlayerFlags!.Value);
            if (blocks.IsBitSet(8)) data.WriteUInt32(p.PlayerFlagsEx!.Value);
            if (blocks.IsBitSet(9)) data.WriteUInt32(p.GuildRankID!.Value);
            if (blocks.IsBitSet(10)) data.WriteUInt32(p.GuildDeleteDate!.Value);
            if (blocks.IsBitSet(11)) data.WriteInt32(p.GuildLevel!.Value);
            if (blocks.IsBitSet(12)) data.WriteUInt8(p.NumBankSlots!.Value);
            if (blocks.IsBitSet(13)) data.WriteUInt8(p.NativeSex!.Value);
            if (blocks.IsBitSet(14)) data.WriteUInt8(p.Inebriation!.Value);
            if (blocks.IsBitSet(15)) data.WriteUInt8(p.PvpTitle!.Value);
            if (blocks.IsBitSet(16)) data.WriteUInt8(p.ArenaFaction!.Value);
            if (blocks.IsBitSet(17)) data.WriteUInt8(p.PvPRank!.Value);
            if (blocks.IsBitSet(19)) data.WriteUInt32(p.DuelTeam!.Value);
            if (blocks.IsBitSet(20)) data.WriteInt32(p.GuildTimeStamp!.Value);
            if (blocks.IsBitSet(21)) data.WriteInt32(p.ChosenTitle!.Value);
            if (blocks.IsBitSet(22)) data.WriteInt32(p.FakeInebriation!.Value);
            if (blocks.IsBitSet(23)) data.WriteUInt32(p.VirtualPlayerRealm!.Value);
            if (blocks.IsBitSet(24)) data.WriteUInt32(p.CurrentSpecID!.Value);
            if (blocks.IsBitSet(27)) data.WriteInt32(p.HonorLevel!.Value);
        }

        if (hasAnyQuestLog)
        {
            for (int i = 0; i < QuestConst.MaxQuestLogSize; i++)
            {
                if (blocks.IsBitSet(36 + i))
                {
                    QuestLog quest = p.QuestLog[i];
                    data.WriteInt64(quest?.EndTime ?? 0);
                    data.WriteInt32(quest?.QuestID ?? 0);
                    data.WriteUInt32(quest?.StateFlags ?? 0);
                    for (int obj = 0; obj < 24; obj++)
                        data.WriteUInt16((ushort)(quest?.ObjectiveProgress[obj] ?? 0));
                }
            }
        }

        if (hasAnyVisibleItem)
        {
            for (int i = 0; i < 19; i++)
            {
                if (blocks.IsBitSet(62 + i))
                {
                    VisibleItem item = p.VisibleItems![i]!.Value;
                    data.WriteBits(0x0Fu, 4);
                    data.FlushBits();
                    data.WriteInt32(item.ItemID);
                    data.WriteUInt16(item.ItemAppearanceModID);
                    data.WriteUInt16(item.ItemVisual);
                }
            }
        }
    }

    private static bool HasAnyPlayerFieldSet_HandPort(PlayerData p)
    {
        if (p.DuelArbiter != null || p.WowAccount != null || p.LootTargetGUID != null) return true;
        if (p.PlayerFlags.HasValue || p.PlayerFlagsEx.HasValue) return true;
        if (p.GuildRankID.HasValue || p.GuildDeleteDate.HasValue || p.GuildLevel.HasValue) return true;
        if (p.NumBankSlots.HasValue || p.NativeSex.HasValue || p.Inebriation.HasValue) return true;
        if (p.PvpTitle.HasValue || p.ArenaFaction.HasValue || p.PvPRank.HasValue) return true;
        if (p.DuelTeam.HasValue || p.GuildTimeStamp.HasValue || p.ChosenTitle.HasValue) return true;
        if (p.FakeInebriation.HasValue || p.VirtualPlayerRealm.HasValue || p.CurrentSpecID.HasValue) return true;
        if (p.HonorLevel.HasValue) return true;
        if (p.QuestLog != null)
            for (int i = 0; i < p.QuestLog.Length; i++)
                if (p.QuestLog[i] != null && p.QuestLog[i].QuestID.HasValue) return true;
        if (p.VisibleItems != null)
            for (int i = 0; i < p.VisibleItems.Length; i++)
                if (p.VisibleItems[i] != null) return true;
        return false;
    }
}
