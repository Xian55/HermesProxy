using System;
using System.Collections.Generic;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the chat-addon, account-data, battle-pet and equipment-set codecs against the
/// frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// The risk in this batch is bit-length ordering, not field order. Three of these packets read a
/// string's length several fields before the string itself — the targeted addon message reads a
/// 9-bit target length, then a whole nested params block, then a GUID, and only then the target —
/// so a codec that reads each length next to its own string produces a well-formed packet that is
/// entirely wrong from the first string onward. Every string here is therefore given a distinct
/// non-empty value, and the empty case is exercised separately because a zero length is the one
/// value that survives a transposition unnoticed.
/// </remarks>
public class ConfigChatBatchCodecEquivalenceTests
{
    static ConfigChatBatchCodecEquivalenceTests()
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

    private static void WriteParams(WorldPacket w, string prefix, string text, ChatMessageTypeModern type, bool logged)
    {
        w.WriteBits((uint)prefix.GetByteCount(), 5);
        w.WriteBits((uint)text.GetByteCount(), 8);
        w.WriteBit(logged);
        w.WriteInt32((int)type);
        w.WriteString(prefix);
        w.WriteString(text);
    }

    // ---- addon chat ----

    [Theory]
    [InlineData("PFX", "hello addon", true)]
    [InlineData("", "", false)]
    [InlineData("X", "y", false)]
    public void ChatAddonMessage_Matches(string prefix, string text, bool logged)
    {
        var (o, f) = Build(w => WriteParams(w, prefix, text, ChatMessageTypeModern.Party, logged));

        var e = new Frozen.ChatAddonMessage(); e.Read(o);
        var r = ReaderOver(f); ChatAddonMessageCodec.Read(ref r, out var a);

        Assert.Equal(e.Params.Prefix, a.Params.Prefix);
        Assert.Equal(e.Params.Text, a.Params.Text);
        Assert.Equal(e.Params.Type, a.Params.Type);
        Assert.Equal(e.Params.IsLogged, a.Params.IsLogged);
        Assert.Equal(prefix, a.Params.Prefix);
        Assert.Equal(text, a.Params.Text);
    }

    /// <summary>
    /// The target length is read first and its string last. Prefix, text and target are distinct
    /// and of different lengths, so any transposition of the three lengths fails.
    /// </summary>
    [Theory]
    [InlineData("PFX", "hello", "Targetname")]
    [InlineData("", "", "")]
    [InlineData("a", "bb", "ccc")]
    public void ChatAddonMessageTargeted_KeepsTheThreeLengthsApart(string prefix, string text, string target)
    {
        var (o, f) = Build(w =>
        {
            w.WriteBits((uint)target.GetByteCount(), 9);
            // The params block opens with ResetBitPos, which drops whatever is left of the byte
            // the 9-bit length landed in - so the length is byte-aligned on the wire, not packed
            // continuously with the bits that follow it.
            w.FlushBits();
            WriteParams(w, prefix, text, ChatMessageTypeModern.Whisper, true);
            w.WritePackedGuid128(Guid);
            w.WriteString(target);
        });

        var e = new Frozen.ChatAddonMessageTargeted(); e.Read(o);
        var r = ReaderOver(f); ChatAddonMessageTargetedCodec.Read(ref r, out var a);

        Assert.Equal(e.Params.Prefix, a.Params.Prefix);
        Assert.Equal(e.Params.Text, a.Params.Text);
        Assert.Equal(e.ChannelGuid, a.ChannelGuid);
        Assert.Equal(e.Target, a.Target);
        Assert.Equal(prefix, a.Params.Prefix);
        Assert.Equal(text, a.Params.Text);
        Assert.Equal(target, a.Target);
        Assert.Equal(Guid, a.ChannelGuid);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(17, 42)]
    [InlineData(int.MaxValue, int.MinValue)]
    public void CTextEmote_Matches(int emoteId, int soundIndex)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteInt32(emoteId);
            w.WriteInt32(soundIndex);
            if (ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
            {
                w.WriteUInt32(2);
                if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                    w.WriteInt32(7);
                w.WriteUInt32(111);
                w.WriteUInt32(222);
            }
        });

        var e = new Frozen.CTextEmote(); e.Read(o);
        var r = ReaderOver(f); CTextEmoteCodec.Read(ref r, out var a);

        Assert.Equal(e.Target, a.Target);
        Assert.Equal(e.EmoteID, a.EmoteID);
        Assert.Equal(e.SoundIndex, a.SoundIndex);
        Assert.Equal(e.SequenceVariation, a.SequenceVariation);
        Assert.Equal(e.SpellVisualKitIDs, a.SpellVisualKitIDs);
        Assert.Equal(emoteId, a.EmoteID);
        Assert.Equal(soundIndex, a.SoundIndex);
    }

    /// <summary>Each prefix carries its own inline 5-bit length; the count leads.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void ChatRegisterAddonPrefixes_Matches(int count)
    {
        var names = new List<string>();
        for (int i = 0; i < count; i++)
            names.Add(new string((char)('A' + i), i + 1));

        var (o, f) = Build(w =>
        {
            w.WriteInt32(count);
            foreach (var n in names)
            {
                w.WriteBits((uint)n.GetByteCount(), 5);
                w.WriteString(n);
            }
        });

        var e = new Frozen.ChatRegisterAddonPrefixes(); e.Read(o);
        var r = ReaderOver(f); ChatRegisterAddonPrefixesCodec.Read(ref r, out var a);

        Assert.Equal(e.Prefixes, a.Prefixes);
        Assert.Equal(names, a.Prefixes);
    }

    /// <summary>
    /// A wire count far larger than the payload must not be believed. The original body capped at
    /// 64 regardless of what the client claimed; the codec keeps that bound.
    /// </summary>
    [Fact]
    public void ChatRegisterAddonPrefixes_DoesNotTrustAnAbsurdCount()
    {
        var (_, f) = Build(w => w.WriteInt32(int.MaxValue));

        Assert.ThrowsAny<Exception>(() =>
        {
            var rr = ReaderOver(f);
            ChatRegisterAddonPrefixesCodec.Read(ref rr, out _);
        });
    }

    // ---- account data ----

    [Theory]
    [InlineData(0u, 0L, 0u)]
    [InlineData(3u, 1700000000L, 4096u)]
    public void UserClientUpdateAccountData_Matches(uint dataType, long time, uint size)
    {
        byte[] blob = [1, 2, 3, 4, 5, 6, 7];
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteInt64(time);
            w.WriteUInt32(size);
            w.WriteBits(dataType, ModernVersion.GetAccountDataCount() <= 8 ? 3 : 4);
            w.WriteUInt32((uint)blob.Length);
            w.WriteBytes(blob);
        });

        var e = new Frozen.UserClientUpdateAccountData(); e.Read(o);
        var r = ReaderOver(f); UserClientUpdateAccountDataCodec.Read(ref r, out var a);

        Assert.Equal(e.PlayerGuid, a.PlayerGuid);
        Assert.Equal(e.Time, a.Time);
        Assert.Equal(e.Size, a.Size);
        Assert.Equal(e.DataType, a.DataType);
        Assert.Equal(e.CompressedData, a.CompressedData);
        Assert.Equal(blob, a.CompressedData);
    }

    /// <summary>A zero compressed size skips the blob entirely — the skip path, not the happy one.</summary>
    [Fact]
    public void UserClientUpdateAccountData_EmptyBlob_Matches()
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteInt64(5);
            w.WriteUInt32(0);
            w.WriteBits(1u, ModernVersion.GetAccountDataCount() <= 8 ? 3 : 4);
            w.WriteUInt32(0);
        });

        var e = new Frozen.UserClientUpdateAccountData(); e.Read(o);
        var r = ReaderOver(f); UserClientUpdateAccountDataCodec.Read(ref r, out var a);

        Assert.Equal(e.CompressedData, a.CompressedData);
        Assert.Empty(a.CompressedData);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(7u)]
    public void RequestAccountData_Matches(uint dataType)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteBits(dataType, ModernVersion.GetAccountDataCount() <= 8 ? 3 : 4);
        });

        var e = new Frozen.RequestAccountData(); e.Read(o);
        var r = ReaderOver(f); RequestAccountDataCodec.Read(ref r, out var a);

        Assert.Equal(e.PlayerGuid, a.PlayerGuid);
        Assert.Equal(e.DataType, a.DataType);
        Assert.Equal(dataType, a.DataType);
    }

    /// <summary>
    /// ReadToEnd hands back a view over a pooled rental; the codec must copy, or the stored CUF
    /// blob is whatever the next packet happens to put there.
    /// </summary>
    [Fact]
    public void SaveCUFProfiles_CopiesTheTail()
    {
        byte[] blob = [9, 8, 7, 6];
        var (o, f) = Build(w => w.WriteBytes(blob));

        var e = new Frozen.SaveCUFProfiles(); e.Read(o);
        var r = ReaderOver(f); SaveCUFProfilesCodec.Read(ref r, out var a);

        Assert.Equal(e.Data, a.Data);
        Assert.Equal(blob, a.Data);

        // Overwrite the source buffer; a view would follow it, a copy would not.
        Array.Clear(f);
        Assert.Equal(blob, a.Data);
    }

    // ---- battle pet / mount ----

    [Fact]
    public void BattlePetSummon_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));

        var e = new Frozen.BattlePetSummon(); e.Read(o);
        var r = ReaderOver(f); BattlePetSummonCodec.Read(ref r, out var a);

        Assert.Equal(e.PetGuid, a.PetGuid);
        Assert.Equal(Guid, a.PetGuid);
    }

    [Theory]
    [InlineData((ushort)0, (byte)0)]
    [InlineData((ushort)1, (byte)2)]
    [InlineData(ushort.MaxValue, (byte)3)]
    public void BattlePetSetFlags_Matches(ushort flags, byte control)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt16(flags);
            w.WriteBits(control, 2);
        });

        var e = new Frozen.BattlePetSetFlags(); e.Read(o);
        var r = ReaderOver(f); BattlePetSetFlagsCodec.Read(ref r, out var a);

        Assert.Equal(e.PetGuid, a.PetGuid);
        Assert.Equal(e.Flags, a.Flags);
        Assert.Equal(e.ControlType, a.ControlType);
        Assert.Equal(flags, a.Flags);
        Assert.Equal(control, a.ControlType);
    }

    [Fact]
    public void DismissCritter_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));

        var e = new Frozen.DismissCritter(); e.Read(o);
        var r = ReaderOver(f); DismissCritterCodec.Read(ref r, out var a);

        Assert.Equal(e.CritterGUID, a.CritterGUID);
    }

    [Theory]
    [InlineData(0u, false)]
    [InlineData(33554u, true)]
    public void MountSetFavorite_Matches(uint spellId, bool favorite)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32(spellId);
            w.WriteBit(favorite);
        });

        var e = new Frozen.MountSetFavorite(); e.Read(o);
        var r = ReaderOver(f); MountSetFavoriteCodec.Read(ref r, out var a);

        Assert.Equal(e.MountSpellID, a.MountSpellID);
        Assert.Equal(e.IsFavorite, a.IsFavorite);
        Assert.Equal(spellId, a.MountSpellID);
        Assert.Equal(favorite, a.IsFavorite);
    }

    // ---- equipment sets ----

    /// <summary>
    /// The name and icon lengths are read before the optional spec index and both strings after
    /// it, so a codec that reads either string early misaligns the rest. The two strings differ in
    /// length, and the spec-index branch is exercised both ways.
    /// </summary>
    [Theory]
    [InlineData(true, "MySet", "Interface\\Icons\\INV_Misc_QuestionMark")]
    [InlineData(false, "S", "ii")]
    [InlineData(false, "", "")]
    public void SaveEquipmentSet_Matches(bool hasSpec, string name, string icon)
    {
        var (o, f) = Build(w =>
        {
            w.WriteInt32(3);
            w.WriteUInt64(0x1122334455667788UL);
            w.WriteUInt32(9);
            w.WriteUInt32(0b101);
            for (int i = 0; i < LoadEquipmentSet.SlotCount; i++)
            {
                w.WritePackedGuid128(i == 4 ? Guid : WowGuid128.Empty);
                w.WriteInt32(1000 + i);
            }
            w.WriteInt32(11);
            w.WriteInt32(22);
            w.WriteInt32(33);
            w.WriteInt32(44);
            w.WriteInt32(55);
            w.WriteInt32(66);
            w.WriteBit(hasSpec);
            w.WriteBits((uint)name.GetByteCount(), 8);
            w.WriteBits((uint)icon.GetByteCount(), 9);
            w.FlushBits();
            if (hasSpec)
                w.WriteInt32(2);
            w.WriteString(name);
            w.WriteString(icon);
        });

        var e = new Frozen.SaveEquipmentSet(); e.Read(o);
        var r = ReaderOver(f); SaveEquipmentSetCodec.Read(ref r, out var a);

        Assert.Equal(e.Set.Type, a.Set.Type);
        Assert.Equal(e.Set.Guid, a.Set.Guid);
        Assert.Equal(e.Set.SetID, a.Set.SetID);
        Assert.Equal(e.Set.IgnoreMask, a.Set.IgnoreMask);
        Assert.Equal(e.Set.AssignedSpecIndex, a.Set.AssignedSpecIndex);
        Assert.Equal(e.Set.SetName, a.Set.SetName);
        Assert.Equal(e.Set.SetIcon, a.Set.SetIcon);
        Assert.Equal(e.Set.Pieces, a.Set.Pieces);
        Assert.Equal(e.Set.Appearances, a.Set.Appearances);
        Assert.Equal(e.Set.Enchants, a.Set.Enchants);
        Assert.Equal(e.Set.SecondaryWeaponSlot, a.Set.SecondaryWeaponSlot);
        Assert.Equal(name, a.Set.SetName);
        Assert.Equal(icon, a.Set.SetIcon);
        Assert.Equal(hasSpec ? 2 : -1, a.Set.AssignedSpecIndex);
    }

    [Theory]
    [InlineData(0ul)]
    [InlineData(ulong.MaxValue)]
    public void DeleteEquipmentSet_Matches(ulong id)
    {
        var (o, f) = Build(w => w.WriteUInt64(id));

        var e = new Frozen.DeleteEquipmentSet(); e.Read(o);
        var r = ReaderOver(f); DeleteEquipmentSetCodec.Read(ref r, out var a);

        Assert.Equal(e.ID, a.ID);
        Assert.Equal(id, a.ID);
    }

    /// <summary>
    /// The leading 2-bit count describes a discarded block; skipping it wrong shifts every slot.
    /// Exercised at zero and at the maximum the two bits can express.
    /// </summary>
    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(3u)]
    public void UseEquipmentSet_SkipsTheLeadingInventoryBlock(uint invCount)
    {
        var (o, f) = Build(w =>
        {
            w.WriteBits(invCount, 2);
            w.FlushBits();
            for (uint i = 0; i < invCount; i++)
            {
                w.WriteUInt8(200);
                w.WriteUInt8(201);
            }
            for (int i = 0; i < LoadEquipmentSet.SlotCount; i++)
            {
                w.WritePackedGuid128(i == 2 ? EquipmentSetModern.IgnoredSlot : WowGuid128.Empty);
                w.WriteUInt8(255);
                w.WriteUInt8((byte)i);
            }
            w.WriteUInt64(55);
        });

        var e = new Frozen.UseEquipmentSet(); e.Read(o);
        var r = ReaderOver(f); UseEquipmentSetCodec.Read(ref r, out var a);

        var actual = a.Items;
        Assert.Equal(e.GUID, a.GUID);
        Assert.Equal(55ul, a.GUID);
        for (int i = 0; i < LoadEquipmentSet.SlotCount; i++)
        {
            Assert.Equal(e.Items[i].Item, actual[i].Item);
            Assert.Equal(e.Items[i].ContainerSlot, actual[i].ContainerSlot);
            Assert.Equal(e.Items[i].Slot, actual[i].Slot);
        }
        Assert.Equal(EquipmentSetModern.IgnoredSlot, actual[2].Item);
    }
}
