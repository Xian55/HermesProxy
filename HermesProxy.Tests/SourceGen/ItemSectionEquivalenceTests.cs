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

// Byte-equivalence oracle for Phase 5b Item section. Production
// WriteCreateItemData / WriteUpdateItemData / HasAnyItemFieldSet are now
// generator-emitted from V3_4_3_54261.ItemField; the Enchantment[13] nested
// inner mask is delegated to hand-rolled WriteEnchantmentCreate/Update writers
// on the builder partial class.
//
// Exercises new generator features: OwnerOnly Create-path wrapping, CustomWriter
// per-element delegation, Cast prefix on UInt8/Int32 conversions, and the
// trailing-bits placeholder. Two guid-shapes drive IsOwner true (HighGuidType703.Item)
// vs false (HighGuidType703.Creature).
public class ItemSectionEquivalenceTests
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
        return new ObjectUpdateBuilder(update, session);
    }

    // ---------------------------------------------------------------------
    // Create-path byte-eq
    // ---------------------------------------------------------------------

    public static System.Collections.Generic.IEnumerable<object[]> CreateScenarios()
    {
        // ownerGuid flag flips IsOwner via the guid HighType (Item / Creature).
        yield return new object[] { "owner-empty",        true,  (System.Action<ItemData>)(_ => { }) };
        yield return new object[] { "non-owner-empty",    false, (System.Action<ItemData>)(_ => { }) };
        yield return new object[] { "owner-all-scalars",  true,  (System.Action<ItemData>)(item => {
            item.Owner = WowGuid128.Create(HighGuidType703.Player, 7);
            item.ContainedIn = WowGuid128.Create(HighGuidType703.Item, 8);
            item.Creator = WowGuid128.Create(HighGuidType703.Player, 9);
            item.GiftCreator = WowGuid128.Create(HighGuidType703.Player, 10);
            item.StackCount = 20u;
            item.Duration = 3600u;
            item.Flags = 0x40u;
            item.PropertySeed = 12345u;
            item.RandomProperty = 0xDEADu;
            item.Durability = 100u;
            item.MaxDurability = 100u;
            item.CreatePlayedTime = 999u;
        }) };
        yield return new object[] { "non-owner-scalars",  false, (System.Action<ItemData>)(item => {
            item.Owner = WowGuid128.Create(HighGuidType703.Player, 7);
            item.Flags = 0x40u;
            item.PropertySeed = 12345u;
        }) };
        yield return new object[] { "spell-charges-mixed", true,  (System.Action<ItemData>)(item => {
            item.SpellCharges[0] = 5;
            item.SpellCharges[3] = -1;
        }) };
        yield return new object[] { "enchantment-slot-0",  true, (System.Action<ItemData>)(item => {
            item.Enchantment[0] = new ItemEnchantment { ID = 1234, Duration = 60u, Charges = 3, Inactive = 0 };
        }) };
        yield return new object[] { "enchantment-mixed",   true, (System.Action<ItemData>)(item => {
            item.Enchantment[2] = new ItemEnchantment { ID = 1, Duration = 100u };
            item.Enchantment[5] = new ItemEnchantment { ID = 9999, Charges = 5 };
            item.Enchantment[12] = new ItemEnchantment { ID = 42 };
        }) };
    }

    [Theory]
    [MemberData(nameof(CreateScenarios))]
    public void WriteCreateItemData_GeneratedMatchesHandPort(string _label, bool ownerGuid, System.Action<ItemData> populate)
    {
        var session = CreateGameSession();
        var guid = ownerGuid
            ? WowGuid128.Create(HighGuidType703.Item, 1)
            : WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);
        // Ensure ItemData exists for Creature guids — constructor only allocates it for Item highTypes.
        if (update.ItemData == null) update.ItemData = new ItemData();
        populate(update.ItemData);

        var actual = new WorldPacket();
        builder.WriteCreateItemData(actual);

        var expected = new WorldPacket();
        WriteCreateItemData_HandPort(expected, update.ItemData, ownerGuid);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    [Fact]
    public void WriteCreateItemData_NullItem_MatchesEmptyHandPort()
    {
        // Hand-port branched into WriteEmptyItemCreate when ItemData was null; generator
        // emits the same byte sequence as WriteCreateItemData(new ItemData()).
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var builder = MakeBuilder(guid, session, out var update);
        update.ItemData = null!;

        var actual = new WorldPacket();
        builder.WriteCreateItemData(actual);

        var expected = new WorldPacket();
        WriteEmptyItemCreate_HandPort(expected, isOwner: true);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // Update-path byte-eq
    // ---------------------------------------------------------------------

    public static System.Collections.Generic.IEnumerable<object[]> UpdateScenarios()
    {
        yield return new object[] { "empty",              (System.Action<ItemData>)(_ => { }) };
        yield return new object[] { "owner-only",         (System.Action<ItemData>)(item => item.Owner = WowGuid128.Create(HighGuidType703.Player, 1)) };
        yield return new object[] { "stack-only",         (System.Action<ItemData>)(item => item.StackCount = 5u) };
        yield return new object[] { "spell-charges-1-3",  (System.Action<ItemData>)(item => {
            item.SpellCharges[1] = 9;
            item.SpellCharges[3] = -2;
        }) };
        yield return new object[] { "enchantment-only-id",(System.Action<ItemData>)(item => {
            item.Enchantment[0] = new ItemEnchantment { ID = 42 };
        }) };
        yield return new object[] { "enchantment-full",   (System.Action<ItemData>)(item => {
            item.Enchantment[3] = new ItemEnchantment { ID = 1, Duration = 60u, Charges = 7 };
        }) };
        yield return new object[] { "ench-block-boundary",(System.Action<ItemData>)(item => {
            // bit 31 = Enchantment[1] (end of block 0), bit 32 = Enchantment[2] (block 1).
            item.Enchantment[1] = new ItemEnchantment { ID = 31 };
            item.Enchantment[2] = new ItemEnchantment { ID = 32 };
        }) };
        yield return new object[] { "appearance-mod-cast", (System.Action<ItemData>)(item => {
            item.ItemAppearanceModID = 250u; // truncates to byte: 250
        }) };
        yield return new object[] { "all-fields",          (System.Action<ItemData>)(item => {
            item.Owner = WowGuid128.Create(HighGuidType703.Player, 1);
            item.ContainedIn = WowGuid128.Create(HighGuidType703.Item, 2);
            item.Creator = WowGuid128.Create(HighGuidType703.Player, 3);
            item.GiftCreator = WowGuid128.Create(HighGuidType703.Player, 4);
            item.StackCount = 10u;
            item.Duration = 7200u;
            item.Flags = 0x80u;
            item.PropertySeed = 123u;
            item.RandomProperty = 456u;
            item.Durability = 80u;
            item.MaxDurability = 100u;
            item.CreatePlayedTime = 1234u;
            item.Context = 7;
            item.ArtifactXP = 99999uL;
            item.ItemAppearanceModID = 5u;
            for (int i = 0; i < 5; i++) item.SpellCharges[i] = i * 10;
            for (int i = 0; i < 13; i++) item.Enchantment[i] = new ItemEnchantment { ID = i + 100, Duration = (uint)(i * 10) };
        }) };
    }

    [Theory]
    [MemberData(nameof(UpdateScenarios))]
    public void WriteUpdateItemData_GeneratedMatchesHandPort(string _label, System.Action<ItemData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.ItemData!);

        var actual = new WorldPacket();
        builder.WriteUpdateItemData(actual);

        var expected = new WorldPacket();
        WriteUpdateItemData_HandPort(expected, update.ItemData!);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    [Theory]
    [MemberData(nameof(UpdateScenarios))]
    public void HasAnyItemFieldSet_GeneratedMatchesHandPort(string _label, System.Action<ItemData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.ItemData!);

        Assert.Equal(HasAnyItemFieldSet_HandPort(update.ItemData!), builder.HasAnyItemFieldSet());
    }

    // ---------------------------------------------------------------------
    // Inlined pre-Phase-5b hand-ports — byte-for-byte oracles. Frozen here so
    // the production hand-port can be deleted in favour of generator output.
    // Identical to bodies removed from V3_4_3_54261/ObjectUpdateBuilder.cs
    // (WriteCreateItemData 355-417, WriteEmptyItemCreate 420-464,
    // WriteUpdateItemData 3029-3135).
    // ---------------------------------------------------------------------

    private static void WriteCreateItemData_HandPort(WorldPacket data, ItemData item, bool isOwner)
    {
        if (item == null)
        {
            WriteEmptyItemCreate_HandPort(data, isOwner);
            return;
        }
        data.WritePackedGuid128(item.Owner ?? WowGuid128.Empty);
        data.WritePackedGuid128(item.ContainedIn ?? WowGuid128.Empty);
        data.WritePackedGuid128(item.Creator ?? WowGuid128.Empty);
        data.WritePackedGuid128(item.GiftCreator ?? WowGuid128.Empty);
        if (isOwner)
        {
            data.WriteUInt32(item.StackCount.GetValueOrDefault());
            data.WriteUInt32(item.Duration.GetValueOrDefault());
            for (int i = 0; i < 5; i++)
                data.WriteInt32(item.SpellCharges[i].GetValueOrDefault());
        }
        data.WriteUInt32(item.Flags.GetValueOrDefault());
        for (int j = 0; j < 13; j++)
        {
            var ench = item.Enchantment[j];
            if (ench != null)
            {
                data.WriteInt32(ench.ID.GetValueOrDefault());
                data.WriteUInt32(ench.Duration.GetValueOrDefault());
                data.WriteInt16((short)ench.Charges.GetValueOrDefault());
                data.WriteUInt16(ench.Inactive.GetValueOrDefault());
            }
            else
            {
                data.WriteInt32(0);
                data.WriteUInt32(0u);
                data.WriteInt16(0);
                data.WriteUInt16(0);
            }
        }
        data.WriteInt32((int)item.PropertySeed.GetValueOrDefault());
        data.WriteInt32((int)item.RandomProperty.GetValueOrDefault());
        if (isOwner)
        {
            data.WriteUInt32(item.Durability.GetValueOrDefault());
            data.WriteUInt32(item.MaxDurability.GetValueOrDefault());
        }
        data.WriteUInt32(item.CreatePlayedTime.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteInt64(0L);
        if (isOwner)
        {
            data.WriteUInt64(0uL);
            data.WriteUInt8(0);
        }
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        if (isOwner)
            data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        if (isOwner)
            data.WriteUInt16(0);
        data.WriteBits(0u, 6);
        data.FlushBits();
    }

    private static void WriteEmptyItemCreate_HandPort(WorldPacket data, bool isOwner)
    {
        for (int i = 0; i < 4; i++)
            data.WritePackedGuid128(WowGuid128.Empty);
        if (isOwner)
        {
            data.WriteUInt32(0u);
            data.WriteUInt32(0u);
            for (int j = 0; j < 5; j++)
                data.WriteInt32(0);
        }
        data.WriteUInt32(0u);
        for (int k = 0; k < 13; k++)
        {
            data.WriteInt32(0);
            data.WriteUInt32(0u);
            data.WriteInt16(0);
            data.WriteUInt16(0);
        }
        data.WriteInt32(0);
        data.WriteInt32(0);
        if (isOwner)
        {
            data.WriteUInt32(0u);
            data.WriteUInt32(0u);
        }
        data.WriteUInt32(0u);
        data.WriteInt32(0);
        data.WriteInt64(0L);
        if (isOwner)
        {
            data.WriteUInt64(0uL);
            data.WriteUInt8(0);
        }
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        if (isOwner)
            data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        if (isOwner)
            data.WriteUInt16(0);
        data.WriteBits(0u, 6);
        data.FlushBits();
    }

    private static void WriteUpdateItemData_HandPort(WorldPacket data, ItemData item)
    {
        if (item == null)
        {
            data.WriteBits(0u, 2);
            data.FlushBits();
            return;
        }
        System.Span<uint> blocksBuf = stackalloc uint[2];
        var blocks = new global::Framework.Util.StackBitMask(blocksBuf);
        if (item.Owner != null) { blocks.SetBit(0); blocks.SetBit(3); }
        if (item.ContainedIn != null) { blocks.SetBit(0); blocks.SetBit(4); }
        if (item.Creator != null) { blocks.SetBit(0); blocks.SetBit(5); }
        if (item.GiftCreator != null) { blocks.SetBit(0); blocks.SetBit(6); }
        if (item.StackCount.HasValue) { blocks.SetBit(0); blocks.SetBit(7); }
        if (item.Duration.HasValue) { blocks.SetBit(0); blocks.SetBit(8); }
        if (item.Flags.HasValue) { blocks.SetBit(0); blocks.SetBit(9); }
        if (item.PropertySeed.HasValue) { blocks.SetBit(0); blocks.SetBit(10); }
        if (item.RandomProperty.HasValue) { blocks.SetBit(0); blocks.SetBit(11); }
        if (item.Durability.HasValue) { blocks.SetBit(0); blocks.SetBit(12); }
        if (item.MaxDurability.HasValue) { blocks.SetBit(0); blocks.SetBit(13); }
        if (item.CreatePlayedTime.HasValue) { blocks.SetBit(0); blocks.SetBit(14); }
        if (item.Context.HasValue) { blocks.SetBit(0); blocks.SetBit(15); }
        if (item.ArtifactXP.HasValue) { blocks.SetBit(0); blocks.SetBit(17); }
        if (item.ItemAppearanceModID.HasValue) { blocks.SetBit(0); blocks.SetBit(18); }
        for (int i = 0; i < 5; i++)
            if (item.SpellCharges[i].HasValue) { blocks.SetBit(23); blocks.SetBit(24 + i); }
        for (int i = 0; i < 13; i++)
            if (item.Enchantment[i] != null) { blocks.SetBit(29); blocks.SetBit(30 + i); }

        byte blocksMask = 0;
        if (blocks[0] != 0) blocksMask |= 1;
        if (blocks[1] != 0) blocksMask |= 2;
        data.WriteBits((uint)blocksMask, 2);
        for (int b = 0; b < 2; b++)
            if ((blocksMask & (1 << b)) != 0)
                data.WriteBits(blocks[b], 32);
        data.FlushBits();

        if ((blocks[0] & 1) != 0)
        {
            if (item.Owner != null) data.WritePackedGuid128(item.Owner.Value);
            if (item.ContainedIn != null) data.WritePackedGuid128(item.ContainedIn.Value);
            if (item.Creator != null) data.WritePackedGuid128(item.Creator.Value);
            if (item.GiftCreator != null) data.WritePackedGuid128(item.GiftCreator.Value);
            if (item.StackCount.HasValue) data.WriteUInt32(item.StackCount.Value);
            if (item.Duration.HasValue) data.WriteUInt32(item.Duration.Value);
            if (item.Flags.HasValue) data.WriteUInt32(item.Flags.Value);
            if (item.PropertySeed.HasValue) data.WriteInt32((int)item.PropertySeed.Value);
            if (item.RandomProperty.HasValue) data.WriteInt32((int)item.RandomProperty.Value);
            if (item.Durability.HasValue) data.WriteUInt32(item.Durability.Value);
            if (item.MaxDurability.HasValue) data.WriteUInt32(item.MaxDurability.Value);
            if (item.CreatePlayedTime.HasValue) data.WriteUInt32(item.CreatePlayedTime.Value);
            if (item.Context.HasValue) data.WriteInt32(item.Context.Value);
            if (item.ArtifactXP.HasValue) data.WriteUInt64(item.ArtifactXP.Value);
            if (item.ItemAppearanceModID.HasValue) data.WriteUInt8((byte)item.ItemAppearanceModID.Value);
        }
        if ((blocks[0] & (1u << 23)) != 0)
        {
            for (int i = 0; i < 5; i++)
                if (item.SpellCharges[i].HasValue)
                    data.WriteInt32(item.SpellCharges[i]!.Value);
        }
        if ((blocks[0] & (1u << 29)) != 0)
        {
            for (int i = 0; i < 13; i++)
            {
                if (item.Enchantment[i] != null)
                {
                    var ench = item.Enchantment[i]!;
                    uint enchMask = 0;
                    if (ench.ID.HasValue) enchMask |= 2;
                    if (ench.Duration.HasValue) enchMask |= 4;
                    if (ench.Charges.HasValue) enchMask |= 8;
                    if (enchMask != 0) enchMask |= 1;
                    data.WriteBits(enchMask, 4);
                    data.FlushBits();
                    if (ench.ID.HasValue) data.WriteInt32(ench.ID.Value);
                    if (ench.Duration.HasValue) data.WriteUInt32(ench.Duration.Value);
                    if (ench.Charges.HasValue) data.WriteUInt16(ench.Charges.Value);
                }
            }
        }
    }

    private static bool HasAnyItemFieldSet_HandPort(ItemData item)
    {
        if (item == null) return false;
        if (item.Owner != null || item.ContainedIn != null || item.Creator != null || item.GiftCreator != null) return true;
        if (item.StackCount.HasValue || item.Duration.HasValue || item.Flags.HasValue) return true;
        if (item.PropertySeed.HasValue || item.RandomProperty.HasValue) return true;
        if (item.Durability.HasValue || item.MaxDurability.HasValue || item.CreatePlayedTime.HasValue) return true;
        if (item.Context.HasValue || item.ArtifactXP.HasValue || item.ItemAppearanceModID.HasValue) return true;
        for (int i = 0; i < 5; i++) if (item.SpellCharges[i].HasValue) return true;
        for (int i = 0; i < 13; i++) if (item.Enchantment[i] != null) return true;
        return false;
    }
}
