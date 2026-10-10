using System;
using System.Numerics;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// <see cref="EnumCharactersResult"/> went from one writer with an expansion branch to one layout
/// per shape. The 1.14/2.5 and 3.4.3 layouts must give the bytes the old branches did; the 4.4.2
/// layout must give the bytes of a native TrinityCore cata_classic list.
/// </summary>
public sealed class EnumCharactersResultLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<EnumCharactersResult.ClassicEraLayout>(EnumCharactersResult.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<EnumCharactersResult.ClassicEraLayout>(EnumCharactersResult.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<EnumCharactersResult.WotLKClassicLayout>(EnumCharactersResult.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<EnumCharactersResult.CataClassicLayout>(EnumCharactersResult.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClassicEraLayout_MatchesTheOldLegacyBranch(bool withMask)
    {
        var packet = Sample(withMask);
        Assert.Equal(OldWrite(packet, wotlk: false), Write(new EnumCharactersResult.ClassicEraLayout(), packet));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WotLKClassicLayout_MatchesTheOld3_4_3Branch(bool withMask)
    {
        var packet = Sample(withMask);
        Assert.Equal(OldWrite(packet, wotlk: true), Write(new EnumCharactersResult.WotLKClassicLayout(), packet));
    }

    [Fact]
    public void CataClassicLayout_MatchesANativeList()
    {
        var character = new EnumCharactersResult.CharacterInfo
        {
            Guid = new WowGuid128(1, 0x0800040000000000),
            VirtualRealmAddress = 0x01010001,
            Name = "Xii",
            RaceId = Race.Human,
            ClassId = Class.Warrior,
            SexId = Gender.Female,
            ExperienceLevel = 1,
            ZoneId = 12,
            PreloadPos = new Vector3(-8916.80859375f, -128.05789184570312f, 81.11783599853516f),
            GuildClubMemberID = 281474976710657,
            LastPlayedTime = 1791642197,
            LastLoginVersion = 40402,
        };
        character.Customizations.AddRange([Choice(14, 17217), Choice(15, 17236), Choice(16, 17257), Choice(17, 17263), Choice(18, 17275)]);
        character.VisualItems[4] = new() { DisplayId = 5729, InvType = 5, Subclass = 1 };
        character.VisualItems[6] = new() { DisplayId = 6050, InvType = 7, Subclass = 1 };
        character.VisualItems[7] = new() { DisplayId = 6051, InvType = 8, Subclass = 1 };
        character.VisualItems[15] = new() { DisplayId = 1627, InvType = 17, Subclass = 8 };

        var packet = new EnumCharactersResult { Success = true, DisabledClassesMask = 0 };
        packet.Characters.Add(character);
        foreach (int race in (int[])[11, 10, 22, 9, 8, 7, 6, 5, 4, 3, 2, 1])
            packet.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(race, true, false, false));

        Assert.Equal(Convert.ToHexString(Convert.FromHexString(NativeList)),
            Convert.ToHexString(Write(new EnumCharactersResult.CataClassicLayout(), packet)));
    }

    // SMSG_ENUM_CHARACTERS_RESULT from TrinityCore cata_classic to a 4.4.2.60895 client
    // (refs/native-captures/tc_cata_442_20261010-150803.pkt, packet 13): one character, 12 races.
    private const string NativeList =
        "81000100000000000000010000000C0000000000000000000000000000000000000001A0010408010001010001010100000500000001000000000C0000003C530BC6D20E00C3553C" +
        "A24201000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000061160000050000000001000000000000000000" +
        "00000000000000000000000000000000000000000000000000A2170000070000000001000000000000000000000000A3170000080000000001000000000000000000000000000000" +
        "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "000000000000005B06000011000000000800000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "000000000000000000000000000000000000000000000000000000554ACA6A00000000D29D0000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000" +
        "0000000E000000414300000F000000544300001000000069430000110000006F430000120000007B4300000C586969000000000000000000000000000B000000800A000000801600" +
        "000080090000008008000000800700000080060000008005000000800400000080030000008002000000800100000080";

    private static ChrCustomizationChoice Choice(uint option, uint choice)
        => new(option, choice);

    private static byte[] Write(ServerPacketLayout<EnumCharactersResult> layout, EnumCharactersResult packet)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        return data.GetDataSpan().ToArray();
    }

    private static EnumCharactersResult Sample(bool withMask)
    {
        var first = new EnumCharactersResult.CharacterInfo
        {
            Guid = new WowGuid128(42, 0x0800040000000000),
            VirtualRealmAddress = 0x01010001,
            Name = "Stancee",
            ListPosition = 0,
            RaceId = Race.Human,
            ClassId = Class.Warrior,
            SexId = Gender.Female,
            ExperienceLevel = 23,
            ZoneId = 12,
            MapId = 0,
            PreloadPos = new Vector3(-8913.5f, -137.25f, 80.5f),
            GuildGuid = new WowGuid128(7, 0x1000040000000000),
            Flags = (CharacterFlags)0x2000,
            Flags2 = 1,
            Flags3 = 2,
            Flags4 = 3,
            FirstLogin = true,
            unkWod61x = 5,
            ExpansionChosen = true,
            LastPlayedTime = 1791642197,
            SpecID = 71,
            Unknown703 = 9,
            LastLoginVersion = 12340,
            OverrideSelectScreenFileDataID = 11,
            PetCreatureDisplayId = 903,
            PetExperienceLevel = 20,
            PetCreatureFamilyId = 1,
            BoostInProgress = true,
            ProfessionIds = [164, 186],
        };
        first.Customizations.AddRange([Choice(14, 17217), Choice(15, 17236)]);
        first.VisualItems[4] = new() { DisplayId = 5729, DisplayEnchantId = 3, SecondaryItemModifiedAppearanceID = 4, InvType = 5, Subclass = 1 };
        first.VisualItems[22] = new() { DisplayId = 6050, InvType = 18, Subclass = 0 };
        first.MailSenders.AddRange(["Thrall", ""]);
        first.MailSenderTypes.AddRange([0u, 2u]);

        var second = new EnumCharactersResult.CharacterInfo
        {
            Guid = new WowGuid128(43, 0x0800040000000000),
            Name = "Ткач",
            ListPosition = 1,
            RaceId = Race.Orc,
            ClassId = Class.Hunter,
            SexId = Gender.Male,
            ExperienceLevel = 1,
        };

        var packet = new EnumCharactersResult
        {
            Success = true,
            IsNewPlayer = true,
            IsTrialAccountRestricted = true,
            IsAlliedRacesCreationAllowed = true,
            MaxCharacterLevel = 80,
            DisabledClassesMask = withMask ? 0x40u : null,
        };
        packet.Characters.AddRange([first, second]);
        packet.UnlockedConditionalAppearances.Add(new EnumCharactersResult.UnlockedConditionalAppearance { AchievementID = 3, Unused = 0 });
        packet.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(1, true, false, false));
        packet.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(10, true, true, false));
        return packet;
    }

    // EnumCharactersResult.Write as it stood before the layouts, frozen. The per-character bodies did
    // not move; only the choice between them did, which is the `wotlk` flag here.
    private static byte[] OldWrite(EnumCharactersResult p, bool wotlk)
    {
        using var w = new WorldPacket(1u);
        w.WriteBit(p.Success);
        w.WriteBit(p.IsDeletedCharacters);
        w.WriteBit(p.IsNewPlayerRestrictionSkipped);
        w.WriteBit(p.IsNewPlayerRestricted);
        w.WriteBit(p.IsNewPlayer);

        if (wotlk)
        {
            w.WriteBit(p.IsTrialAccountRestricted);
            w.WriteBit(p.DisabledClassesMask.HasValue);
            w.WriteUInt32((uint)p.Characters.Count);
            w.WriteInt32(p.MaxCharacterLevel);
            w.WriteUInt32((uint)p.RaceUnlockData.Count);
            w.WriteUInt32((uint)p.UnlockedConditionalAppearances.Count);
            w.WriteUInt32((uint)p.RaceLimitDisablesCount);

            if (p.DisabledClassesMask.HasValue)
                w.WriteUInt32(p.DisabledClassesMask.Value);

            foreach (var unlockedConditionalAppearance in p.UnlockedConditionalAppearances)
                unlockedConditionalAppearance.Write(w);

            foreach (var charInfo in p.Characters)
                charInfo.WriteWotLKClassic(w);

            foreach (var raceUnlock in p.RaceUnlockData)
                raceUnlock.Write(w);

            return w.GetDataSpan().ToArray();
        }

        w.WriteBit(p.DisabledClassesMask.HasValue);
        w.WriteBit(p.IsAlliedRacesCreationAllowed);
        w.WriteInt32(p.Characters.Count);
        w.WriteInt32(p.MaxCharacterLevel);
        w.WriteInt32(p.RaceUnlockData.Count);
        w.WriteInt32(p.UnlockedConditionalAppearances.Count);

        if (p.DisabledClassesMask.HasValue)
            w.WriteUInt32(p.DisabledClassesMask.Value);

        foreach (var unlockedConditionalAppearance in p.UnlockedConditionalAppearances)
            unlockedConditionalAppearance.Write(w);

        foreach (var charInfo in p.Characters)
            charInfo.WriteClassicEra(w);

        foreach (var raceUnlock in p.RaceUnlockData)
            raceUnlock.Write(w);

        return w.GetDataSpan().ToArray();
    }
}
