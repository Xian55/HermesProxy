using System;
using System.Linq;
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
/// Equivalence for the character codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// Every case asserts field values <b>and</b> the reader's final position, over readers built
/// through <c>GetRemainingSpan()</c>. The count-driven ones get the most attention —
/// <c>ReorderCharacters</c>, <c>QueryPlayerNames</c>, <c>CreateCharacter</c> and
/// <c>AlterAppearance</c> all size a loop from the wire, and an off-by-one there is invisible to a
/// field-only assertion.
/// </remarks>
public class CharacterCodecEquivalenceTests
{
    static CharacterCodecEquivalenceTests()
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

    // ---- character list ----

    /// <summary>
    /// A 9-bit count, so the list can be long. Zero matters most: the handler returns early on an
    /// empty array, and a codec that left it null would throw instead.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public void ReorderCharacters_Matches(int count)
    {
        var (o, f) = Build(w =>
        {
            w.WriteBits((uint)count, 9);
            for (int i = 0; i < count; i++)
            {
                w.WritePackedGuid128(new WowGuid128((ulong)(100 + i), 0x42UL));
                w.WriteUInt8((byte)i);
            }
        });

        var e = new Frozen.ReorderCharacters(); e.Read(o);
        var r = ReaderOver(f); ReorderCharactersCodec.Read(ref r, out var a);

        Assert.NotNull(a.Entries);
        Assert.Equal(e.Entries.Length, a.Entries.Length);
        Assert.Equal(count, a.Entries.Length);
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(e.Entries[i].PlayerGuid, a.Entries[i].PlayerGuid);
            Assert.Equal(e.Entries[i].NewPosition, a.Entries[i].NewPosition);
        }
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(9999u)]
    public void GetAccountCharacterListRequest_Matches(uint token)
    {
        var (o, f) = Build(w => w.WriteUInt32(token));
        var e = new Frozen.GetAccountCharacterListRequest(); e.Read(o);
        var r = ReaderOver(f); GetAccountCharacterListRequestCodec.Read(ref r, out var a);
        Assert.Equal(e.Token, a.Token);
        Assert.Equal(token, a.Token);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void GenerateRandomCharacterNameRequest_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt8((byte)Race.Draenei); w.WriteUInt8((byte)Gender.Female); });
        var e = new Frozen.GenerateRandomCharacterNameRequest(); e.Read(o);
        var r = ReaderOver(f); GenerateRandomCharacterNameRequestCodec.Read(ref r, out var a);
        Assert.Equal(e.Race, a.Race);
        Assert.Equal(e.Sex, a.Sex);
        Assert.Equal(Race.Draenei, a.Race);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// Three bits, then fixed fields, then a name whose length came from a 6-bit prefix read before
    /// any of them, then an optional uint32, then a customization list. Plenty of room to drift.
    /// </summary>
    [Theory]
    [InlineData("Pally", false, 0)]
    [InlineData("Pally", true, 3)]
    [InlineData("A", false, 5)]
    public void CreateCharacter_Matches(string name, bool hasTemplateSet, int customizations)
    {
        var (o, f) = Build(w =>
        {
            w.WriteBits((uint)name.Length, 6);
            w.WriteBit(hasTemplateSet);
            w.WriteBit(false);                  // IsTrialBoost
            w.WriteBit(true);                   // UseNPE
            w.WriteUInt8((byte)Race.Draenei);
            w.WriteUInt8((byte)Class.Paladin);
            w.WriteUInt8((byte)Gender.Female);
            w.WriteUInt32((uint)customizations);
            w.WriteString(name);
            if (hasTemplateSet)
                w.WriteUInt32(77);
            for (int i = 0; i < customizations; i++)
            {
                w.WriteUInt32((uint)(1000 + i));
                w.WriteUInt32((uint)(2000 + i));
            }
        });

        var e = new Frozen.CreateCharacter(); e.Read(o);
        var r = ReaderOver(f); CreateCharacterCodec.Read(ref r, out var a);

        Assert.Equal(e.CreateInfo.Name, a.CreateInfo.Name);
        Assert.Equal(name, a.CreateInfo.Name);
        Assert.Equal(e.CreateInfo.RaceId, a.CreateInfo.RaceId);
        Assert.Equal(e.CreateInfo.ClassId, a.CreateInfo.ClassId);
        Assert.Equal(e.CreateInfo.Sex, a.CreateInfo.Sex);
        Assert.Equal(e.CreateInfo.IsTrialBoost, a.CreateInfo.IsTrialBoost);
        Assert.Equal(e.CreateInfo.UseNPE, a.CreateInfo.UseNPE);
        Assert.Equal(e.CreateInfo.TemplateSet, a.CreateInfo.TemplateSet);
        Assert.Equal(e.CreateInfo.Customizations.Count, a.CreateInfo.Customizations.Count);
        Assert.Equal(customizations, a.CreateInfo.Customizations.Count);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void CharDelete_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.CharDelete(); e.Read(o);
        var r = ReaderOver(f); CharDeleteCodec.Read(ref r, out var a);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u, false)]
    [InlineData(571u, true)]
    [InlineData(0xFFFFFFFFu, true)]
    public void LoadingScreenNotify_Matches(uint mapId, bool showing)
    {
        var (o, f) = Build(w => { w.WriteUInt32(mapId); w.WriteBit(showing); });
        var e = new Frozen.LoadingScreenNotify(); e.Read(o);
        var r = ReaderOver(f); LoadingScreenNotifyCodec.Read(ref r, out var a);
        Assert.Equal(e.MapID, a.MapID);
        Assert.Equal(e.Showing, a.Showing);
        Assert.Equal(mapId, a.MapID);
        Assert.Equal(showing, a.Showing);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- name queries ----

    [Fact]
    public void QueryPlayerName_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.QueryPlayerName(); e.Read(o);
        var r = ReaderOver(f); QueryPlayerNameCodec.Read(ref r, out var a);
        Assert.Equal(e.Player, a.Player);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    public void QueryPlayerNames_Matches(int count)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32((uint)count);
            for (int i = 0; i < count; i++)
                w.WritePackedGuid128(new WowGuid128((ulong)(500 + i), 0x7UL));
        });

        var e = new Frozen.QueryPlayerNames(); e.Read(o);
        var r = ReaderOver(f); QueryPlayerNamesCodec.Read(ref r, out var a);
        Assert.NotNull(a.Players);
        Assert.Equal(e.Players, a.Players);
        Assert.Equal(count, a.Players.Count);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- session lifecycle ----

    /// <summary>
    /// The trailing bit exists on Vanilla and TBC and not on WotLK Classic. The suite runs as V1_14,
    /// so the oracle and the codec both take the read-it branch here and agree; the assertion that
    /// earns its keep is the final position, which is what a wrongly-included or wrongly-skipped bit
    /// moves.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PlayerLogin_Matches(bool unkBit)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteFloat(120.5f); w.WriteBit(unkBit); });
        var e = new Frozen.PlayerLogin(); e.Read(o);
        var r = ReaderOver(f); PlayerLoginCodec.Read(ref r, out var a);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(e.FarClip, a.FarClip);
        Assert.Equal(e.UnkBit, a.UnkBit);
        Assert.Equal(120.5f, a.FarClip);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void PlayerLogin_ReadsTheBitOnThisBuild()
        => Assert.True(ModernVersion.ExpansionVersion < 3);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LogoutRequest_Matches(bool idle)
    {
        var (o, f) = Build(w => w.WriteBit(idle));
        var e = new Frozen.LogoutRequest(); e.Read(o);
        var r = ReaderOver(f); LogoutRequestCodec.Read(ref r, out var a);
        Assert.Equal(e.IdleLogout, a.IdleLogout);
        Assert.Equal(idle, a.IdleLogout);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RequestPlayedTime_Matches(bool trigger)
    {
        var (o, f) = Build(w => w.WriteBit(trigger));
        var e = new Frozen.RequestPlayedTime(); e.Read(o);
        var r = ReaderOver(f); RequestPlayedTimeCodec.Read(ref r, out var a);
        Assert.Equal(e.TriggerScriptEvent, a.TriggerScriptEvent);
        Assert.Equal(trigger, a.TriggerScriptEvent);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- appearance, titles, pvp ----

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void SetTitle_Matches(int titleId)
    {
        var (o, f) = Build(w => w.WriteInt32(titleId));
        var e = new Frozen.SetTitle(); e.Read(o);
        var r = ReaderOver(f); SetTitleCodec.Read(ref r, out var a);
        Assert.Equal(e.TitleID, a.TitleID);
        Assert.Equal(titleId, a.TitleID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// The count comes first, before the three fixed fields — read them in the wrong order and the
    /// loop runs the wrong number of times as well as producing wrong values.
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    public void AlterAppearance_Matches(int customizations)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32((uint)customizations);
            w.WriteUInt8((byte)Gender.Female);
            w.WriteUInt32((uint)Race.NightElf);
            w.WriteUInt32(42);
            for (int i = 0; i < customizations; i++)
            {
                w.WriteUInt32((uint)(10 + i));
                w.WriteUInt32((uint)(20 + i));
            }
        });

        var e = new Frozen.AlterAppearance(); e.Read(o);
        var r = ReaderOver(f); AlterAppearanceCodec.Read(ref r, out var a);
        Assert.Equal(e.NewSexId, a.NewSexId);
        Assert.Equal(e.CustomizedRace, a.CustomizedRace);
        Assert.Equal(e.CustomizedChrModelId, a.CustomizedChrModelId);
        Assert.Equal(e.Customizations.Count, a.Customizations.Count);
        Assert.Equal(customizations, a.Customizations.Count);
        for (int i = 0; i < customizations; i++)
        {
            Assert.Equal(e.Customizations[i].ChrCustomizationOptionID, a.Customizations[i].ChrCustomizationOptionID);
            Assert.Equal(e.Customizations[i].ChrCustomizationChoiceID, a.Customizations[i].ChrCustomizationChoiceID);
        }
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetPvP_Matches(bool enable)
    {
        var (o, f) = Build(w => w.WriteBit(enable));
        var e = new Frozen.SetPvP(); e.Read(o);
        var r = ReaderOver(f); SetPvPCodec.Read(ref r, out var a);
        Assert.Equal(e.Enable, a.Enable);
        Assert.Equal(enable, a.Enable);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PlayerShowingHelmOrCloak_Matches(bool showing)
    {
        var (o, f) = Build(w => w.WriteBit(showing));
        var e = new Frozen.PlayerShowingHelmOrCloak(); e.Read(o);
        var r = ReaderOver(f); PlayerShowingHelmOrCloakCodec.Read(ref r, out var a);
        Assert.Equal(e.Showing, a.Showing);
        Assert.Equal(showing, a.Showing);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- action bar ----

    /// <summary>
    /// Two uint16s and a byte on the wire; what the two halves <em>mean</em> differs by build and is
    /// the handler's problem, not the codec's. Picking them apart wrongly here silently miscoded
    /// macros and mounts as spells with truncated ids, which crashed the V3_4_3 client when the
    /// bogus slot was rendered — so the codec stays faithful to the wire and the recombination
    /// stays in one place.
    /// </summary>
    [Theory]
    [InlineData((ushort)0x1234, (ushort)0x5678, (byte)11)]
    [InlineData((ushort)0, (ushort)0, (byte)0)]
    [InlineData(ushort.MaxValue, ushort.MaxValue, byte.MaxValue)]
    public void SetActionButton_Matches(ushort action, ushort type, byte index)
    {
        var (o, f) = Build(w => { w.WriteUInt16(action); w.WriteUInt16(type); w.WriteUInt8(index); });
        var e = new Frozen.SetActionButton(); e.Read(o);
        var r = ReaderOver(f); SetActionButtonCodec.Read(ref r, out var a);
        Assert.Equal(e.Action, a.Action);
        Assert.Equal(e.Type, a.Type);
        Assert.Equal(e.Index, a.Index);
        Assert.Equal(action, a.Action);
        Assert.Equal(type, a.Type);
        Assert.Equal(index, a.Index);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)0x3F)]
    public void SetActionBarToggles_Matches(byte mask)
    {
        var (o, f) = Build(w => w.WriteUInt8(mask));
        var e = new Frozen.SetActionBarToggles(); e.Read(o);
        var r = ReaderOver(f); SetActionBarTogglesCodec.Read(ref r, out var a);
        Assert.Equal(e.Mask, a.Mask);
        Assert.Equal(mask, a.Mask);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void UnlearnSkill_Matches()
    {
        var (o, f) = Build(w => w.WriteUInt32(186));
        var e = new Frozen.UnlearnSkill(); e.Read(o);
        var r = ReaderOver(f); UnlearnSkillCodec.Read(ref r, out var a);
        Assert.Equal(e.SkillLine, a.SkillLine);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- inspection ----

    [Fact]
    public void Inspect_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid2));
        var e = new Frozen.Inspect(); e.Read(o);
        var r = ReaderOver(f); InspectCodec.Read(ref r, out var a);
        Assert.Equal(e.Target, a.Target);
        Assert.Equal(Guid2, a.Target);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// The 6-bit name length is read after the GUID, unlike CreateCharacter where it comes first.
    [Theory]
    [InlineData("Newname")]
    [InlineData("")]
    public void CharacterRenameRequest_Matches(string newName)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteBits((uint)newName.Length, 6);
            w.WriteString(newName);
        });

        var e = new Frozen.CharacterRenameRequest(); e.Read(o);
        var r = ReaderOver(f); CharacterRenameRequestCodec.Read(ref r, out var a);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(e.NewName, a.NewName);
        Assert.Equal(newName, a.NewName);
        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
