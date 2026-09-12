using System;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the spell and pet codecs, plus the two <c>SpellCastRequest</c> readers.
/// </summary>
/// <remarks>
/// <para>
/// <c>SpellCastRequest</c> is the deepest nested read in the inbound set and is not a frozen-oracle
/// case: it was never a <c>ClientPacket</c>, so there is nothing to freeze. Instead the two
/// implementations — <c>Read(WorldPacket)</c> and the generated <c>Read(ref SpanPacketReader)</c> —
/// are pinned against each other, the same way <c>MovementReaderEquivalenceTests</c> pins the
/// movement pair. Two copies of one wire layout is exactly the hand-sync hazard
/// <c>docs/version-shape-dispatch.md</c> describes, and this is what keeps them in step until the
/// <c>WorldPacket</c> one can be deleted.
/// </para>
/// <para>
/// The cases that matter are the optional blocks: a cast with no movement update versus one with,
/// and a target with none of its four sub-blocks versus all of them. Those change how many bytes
/// precede everything after, which field assertions alone cannot see.
/// </para>
/// </remarks>
public class SpellCodecEquivalenceTests
{
    static SpellCodecEquivalenceTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static (WorldPacket Oracle, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    private static readonly WowGuid128 Guid = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);
    private static readonly WowGuid128 Guid2 = new(0x1122334455667788UL, 0x99AABBCCDDEEFF00UL);

    // ---- SpellCastRequest: the two readers against each other ----

    /// <summary>
    /// Writes a SpellCastRequest body matching what <c>Read</c> expects on the suite's build.
    /// </summary>
    private static void WriteCastRequest(WorldPacket w, bool withMoveUpdate, bool withTargetBlocks,
                                         int reagents = 0, int currencies = 0, int weights = 0)
    {
        w.WritePackedGuid128(Guid);          // CastID
        w.WriteUInt32(11);                   // Misc[0]
        w.WriteUInt32(22);                   // Misc[1]
        w.WriteUInt32(133);                  // SpellID
        w.WriteUInt32(0);                    // SpellXSpellVisualID
        w.WriteFloat(0.5f);                  // MissileTrajectory.Pitch
        w.WriteFloat(30f);                   // MissileTrajectory.Speed
        w.WritePackedGuid128(WowGuid128.Empty); // CraftingNPC
        w.WriteUInt32((uint)reagents);
        w.WriteUInt32((uint)currencies);

        for (int i = 0; i < reagents; i++)
        {
            w.WriteInt32(100 + i); w.WriteInt32(i); w.WriteInt32(1);
        }
        for (int i = 0; i < currencies; i++)
        {
            w.WriteInt32(200 + i); w.WriteInt32(i); w.WriteInt32(2);
        }

        w.WriteBits(5u, 5);                  // SendCastFlags
        w.WriteBit(withMoveUpdate);
        w.WriteBits((uint)weights, 2);

        // SpellTargetData
        w.WriteBits((uint)SpellCastTargetFlags.Unit, 26);
        w.WriteBit(withTargetBlocks);        // SrcLocation
        w.WriteBit(withTargetBlocks);        // DstLocation
        w.WriteBit(withTargetBlocks);        // Orientation
        w.WriteBit(withTargetBlocks);        // MapID
        w.WriteBits(0u, 7);                  // nameLength
        w.WritePackedGuid128(Guid2);         // Unit
        w.WritePackedGuid128(WowGuid128.Empty); // Item
        if (withTargetBlocks)
        {
            w.WritePackedGuid128(WowGuid128.Empty); w.WriteVector3(new Vector3(1, 2, 3));   // Src
            w.WritePackedGuid128(WowGuid128.Empty); w.WriteVector3(new Vector3(4, 5, 6));   // Dst
            w.WriteFloat(1.25f);                                                            // Orientation
            w.WriteInt32(571);                                                              // MapID
        }

        if (withMoveUpdate)
        {
            // WriteMovementInfoModern emits the mover GUID itself, which is the packed GUID the
            // reader takes as MoverGUID before the movement block proper.
            var info = new MovementInfo { MoveTime = 4242, Position = new Vector3(10, 20, 30), Orientation = 1.5f };
            info.WriteMovementInfoModern(w, Guid);
        }

        for (int i = 0; i < weights; i++)
        {
            w.WriteBits(1u, 2);
            w.WriteInt32(7 + i);
            w.WriteUInt32(3u);
        }
    }

    [Theory]
    [InlineData(false, false, 0, 0, 0)]
    [InlineData(false, true, 0, 0, 0)]
    [InlineData(true, false, 0, 0, 0)]
    [InlineData(true, true, 0, 0, 0)]
    [InlineData(false, false, 2, 1, 0)]
    [InlineData(false, false, 0, 0, 2)]
    public void SpellCastRequest_BothReadersAgree(bool withMoveUpdate, bool withTargetBlocks,
                                                  int reagents, int currencies, int weights)
    {
        var (o, f) = Build(w => WriteCastRequest(w, withMoveUpdate, withTargetBlocks, reagents, currencies, weights));

        var expected = new SpellCastRequest();
        expected.Read(o);

        var r = ReaderOver(f);
        var actual = new SpellCastRequest();
        actual.Read(ref r);

        Assert.Equal(expected.CastID, actual.CastID);
        Assert.Equal(expected.SpellID, actual.SpellID);
        Assert.Equal(133u, actual.SpellID);
        Assert.Equal(expected.SpellXSpellVisualID, actual.SpellXSpellVisualID);
        Assert.Equal(expected.SendCastFlags, actual.SendCastFlags);
        Assert.Equal(expected.Misc[0], actual.Misc[0]);
        Assert.Equal(expected.Misc[1], actual.Misc[1]);
        Assert.Equal(expected.MissileTrajectory.Pitch, actual.MissileTrajectory.Pitch);
        Assert.Equal(expected.MissileTrajectory.Speed, actual.MissileTrajectory.Speed);
        Assert.Equal(expected.CraftingNPC, actual.CraftingNPC);

        Assert.Equal(expected.Target.Flags, actual.Target.Flags);
        Assert.Equal(expected.Target.Unit, actual.Target.Unit);
        Assert.Equal(expected.Target.Item, actual.Target.Item);
        Assert.Equal(expected.Target.Name, actual.Target.Name);
        Assert.Equal(expected.Target.SrcLocation == null, actual.Target.SrcLocation == null);
        Assert.Equal(expected.Target.DstLocation == null, actual.Target.DstLocation == null);
        Assert.Equal(expected.Target.Orientation, actual.Target.Orientation);
        Assert.Equal(expected.Target.MapID, actual.Target.MapID);
        if (withTargetBlocks)
        {
            Assert.Equal(expected.Target.SrcLocation!.Location, actual.Target.SrcLocation!.Location);
            Assert.Equal(expected.Target.DstLocation!.Location, actual.Target.DstLocation!.Location);
        }

        Assert.Equal(expected.OptionalReagents.Count, actual.OptionalReagents.Count);
        Assert.Equal(reagents, actual.OptionalReagents.Count);
        Assert.Equal(expected.OptionalCurrencies.Count, actual.OptionalCurrencies.Count);
        Assert.Equal(currencies, actual.OptionalCurrencies.Count);
        Assert.Equal(expected.Weight.Count, actual.Weight.Count);
        Assert.Equal(weights, actual.Weight.Count);

        Assert.Equal(expected.MoveUpdate == null, actual.MoveUpdate == null);
        Assert.Equal(expected.MoverGUID, actual.MoverGUID);
        if (withMoveUpdate)
            Assert.Equal(expected.MoveUpdate!.MoveTime, actual.MoveUpdate!.MoveTime);

        // The assertion the optional blocks exist for.
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CastSpell_Matches(bool withMoveUpdate)
    {
        var (o, f) = Build(w => WriteCastRequest(w, withMoveUpdate, withTargetBlocks: false));
        var expected = new SpellCastRequest(); expected.Read(o);
        var r = ReaderOver(f); CastSpellCodec.Read(ref r, out var a);
        Assert.Equal(expected.SpellID, a.Cast.SpellID);
        Assert.Equal(expected.CastID, a.Cast.CastID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void PetCastSpell_ReadsGuidThenRequest()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid2); WriteCastRequest(w, false, false); });
        var r = ReaderOver(f); PetCastSpellCodec.Read(ref r, out var a);
        Assert.Equal(Guid2, a.PetGUID);
        Assert.Equal(133u, a.Cast.SpellID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void UseItem_ReadsSlotsAndItemBeforeRequest()
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt8(255);
            w.WriteUInt8(23);
            w.WritePackedGuid128(Guid2);
            WriteCastRequest(w, false, false);
        });

        var r = ReaderOver(f); UseItemCodec.Read(ref r, out var a);
        Assert.Equal((byte)255, a.PackSlot);
        Assert.Equal((byte)23, a.Slot);
        Assert.Equal(Guid2, a.CastItem);
        Assert.Equal(133u, a.Cast.SpellID);
        Assert.Equal(0, r.Remaining);
    }

    // ---- the flat spell packets ----

    [Fact]
    public void CancelCast_Matches()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(133); });
        var e = new Frozen.CancelCast(); e.Read(o);
        var r = ReaderOver(f); CancelCastCodec.Read(ref r, out var a);
        Assert.Equal(e.CastID, a.CastID);
        Assert.Equal(e.SpellID, a.SpellID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(133, 40)]
    [InlineData(0, 16)]
    public void CancelChannelling_Matches(int spellId, int reason)
    {
        var (o, f) = Build(w => { w.WriteInt32(spellId); w.WriteInt32(reason); });
        var e = new Frozen.CancelChannelling(); e.Read(o);
        var r = ReaderOver(f); CancelChannellingCodec.Read(ref r, out var a);
        Assert.Equal(e.SpellID, a.SpellID);
        Assert.Equal(e.Reason, a.Reason);
        Assert.Equal(reason, a.Reason);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// SpellID precedes the GUID here and follows it in CancelCast — transposing them would read
    /// the spell id out of the GUID's mask byte.
    [Fact]
    public void CancelAura_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(133); w.WritePackedGuid128(Guid); });
        var e = new Frozen.CancelAura(); e.Read(o);
        var r = ReaderOver(f); CancelAuraCodec.Read(ref r, out var a);
        Assert.Equal(e.SpellID, a.SpellID);
        Assert.Equal(e.CasterGUID, a.CasterGUID);
        Assert.Equal(133u, a.SpellID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void LearnTalent_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(1234); w.WriteUInt16(3); });
        var e = new Frozen.LearnTalent(); e.Read(o);
        var r = ReaderOver(f); LearnTalentCodec.Read(ref r, out var a);
        Assert.Equal(e.TalentID, a.TalentID);
        Assert.Equal(e.Rank, a.Rank);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    public void ResurrectResponse_Matches(uint response)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(response); });
        var e = new Frozen.ResurrectResponse(); e.Read(o);
        var r = ReaderOver(f); ResurrectResponseCodec.Read(ref r, out var a);
        Assert.Equal(e.CasterGUID, a.CasterGUID);
        Assert.Equal(e.Response, a.Response);
        Assert.Equal(response, a.Response);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SelfRes_Matches()
    {
        var (o, f) = Build(w => w.WriteUInt32(21169));
        var e = new Frozen.SelfRes(); e.Read(o);
        var r = ReaderOver(f); SelfResCodec.Read(ref r, out var a);
        Assert.Equal(e.SpellId, a.SpellId);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void TotemDestroyed_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt8(2); w.WritePackedGuid128(Guid); });
        var e = new Frozen.TotemDestroyed(); e.Read(o);
        var r = ReaderOver(f); TotemDestroyedCodec.Read(ref r, out var a);
        Assert.Equal(e.Slot, a.Slot);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- pet ----

    [Fact]
    public void PetAction_Matches()
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(0x0100_0007);
            w.WritePackedGuid128(Guid2);
            w.WriteVector3(new Vector3(1f, 2f, 3f));
        });

        var e = new Frozen.PetAction(); e.Read(o);
        var r = ReaderOver(f); PetActionCodec.Read(ref r, out var a);
        Assert.Equal(e.PetGUID, a.PetGUID);
        Assert.Equal(e.Action, a.Action);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(e.ActionPosition, a.ActionPosition);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void PetSetAction_Matches()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(3); w.WriteUInt32(0x81_0000); });
        var e = new Frozen.PetSetAction(); e.Read(o);
        var r = ReaderOver(f); PetSetActionCodec.Read(ref r, out var a);
        Assert.Equal(e.PetGUID, a.PetGUID);
        Assert.Equal(e.Index, a.Index);
        Assert.Equal(e.Action, a.Action);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void PetStopAttack_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.PetStopAttack(); e.Read(o);
        var r = ReaderOver(f); PetStopAttackCodec.Read(ref r, out var a);
        Assert.Equal(e.PetGUID, a.PetGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// The name length is read before the declined-names block and the name itself after it, so
    /// five 7-bit counts and five strings sit between a length and its string.
    /// </summary>
    [Theory]
    [InlineData("Fluffy", false)]
    [InlineData("Fluffy", true)]
    [InlineData("", false)]
    public void PetRename_Matches(string newName, bool declined)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteInt32(77);
            w.WriteBits((uint)newName.Length, 8);
            w.WriteBit(declined);
            if (declined)
            {
                for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                    w.WriteBits(3u, 7);
                for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                    w.WriteString("abc");
            }
            w.WriteString(newName);
        });

        var e = new Frozen.PetRename(); e.Read(o);
        var r = ReaderOver(f); PetRenameCodec.Read(ref r, out var a);
        Assert.Equal(e.RenameData.PetGUID, a.RenameData.PetGUID);
        Assert.Equal(e.RenameData.PetNumber, a.RenameData.PetNumber);
        Assert.Equal(e.RenameData.HasDeclinedNames, a.RenameData.HasDeclinedNames);
        Assert.Equal(e.RenameData.NewName, a.RenameData.NewName);
        Assert.Equal(newName, a.RenameData.NewName);
        if (declined)
            for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                Assert.Equal(e.RenameData.DeclinedNames.name[i], a.RenameData.DeclinedNames.name[i]);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// UnstablePet and StableSwapPet read the number before the GUID; the three stable-master
    /// packets read only a GUID. Easy to cross-wire.
    [Fact]
    public void UnstablePet_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(5); w.WritePackedGuid128(Guid); });
        var e = new Frozen.UnstablePet(); e.Read(o);
        var r = ReaderOver(f); UnstablePetCodec.Read(ref r, out var a);
        Assert.Equal(e.PetNumber, a.PetNumber);
        Assert.Equal(e.StableMaster, a.StableMaster);
        Assert.Equal(5u, a.PetNumber);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void StableSwapPet_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(9); w.WritePackedGuid128(Guid); });
        var e = new Frozen.StableSwapPet(); e.Read(o);
        var r = ReaderOver(f); StableSwapPetCodec.Read(ref r, out var a);
        Assert.Equal(e.PetNumber, a.PetNumber);
        Assert.Equal(e.StableMaster, a.StableMaster);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void StablePet_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.StablePet(); e.Read(o);
        var r = ReaderOver(f); StablePetCodec.Read(ref r, out var a);
        Assert.Equal(e.StableMaster, a.StableMaster);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void PetCancelAura_Matches()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteUInt32(133); });
        var e = new Frozen.PetCancelAura(); e.Read(o);
        var r = ReaderOver(f); PetCancelAuraCodec.Read(ref r, out var a);
        Assert.Equal(e.PetGUID, a.PetGUID);
        Assert.Equal(e.SpellID, a.SpellID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void PetAbandon_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.PetAbandon(); e.Read(o);
        var r = ReaderOver(f); PetAbandonCodec.Read(ref r, out var a);
        Assert.Equal(e.PetGUID, a.PetGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
