using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.IO;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Raid frame profiles between the 4.4.2 client and a 4.3.4 server.
///
/// The client's side is pinned with blobs the client itself saved (it was made to save a profile
/// with one thing set at a time). The server's side is pinned against TrinityCore 4.3.4, written
/// out here a second time under its own names: <see cref="ServerWrites"/> is
/// <c>LoadCUFProfiles::Write</c> and <see cref="ServerReads"/> is <c>HandleSaveCUFProfiles</c>,
/// so the order tables in the code are checked against the source they came from, not against
/// themselves.
/// </summary>
public sealed class CufProfilesTests
{
    // --- the 4.4.2 client's blob

    /// <summary>What a new character's client saved: one profile, "Primary", as the client makes it.</summary>
    private const string ClientDefault = "01000000" + "0E49E000" + "2400" + "4800" + "0000000000" + "000000000000" + "5072696D617279";

    private static List<CufProfile> ReadModern(string hex)
    {
        List<CufProfile> profiles = [];
        Assert.True(CufProfiles.TryReadModern(Convert.FromHexString(hex), profiles));
        return profiles;
    }

    private static CufOption[] OptionsOf(CufProfile profile)
        => Enum.GetValues<CufOption>().Where(profile.Has).ToArray();

    [Fact]
    public void Modern_TheClientsOwnDefault_IsWhatItsOptionsWindowSays()
    {
        CufProfile profile = Assert.Single(ReadModern(ClientDefault));

        Assert.Equal("Primary", profile.Name);
        // the client's own list for this profile: border, main tank and assist, non-boss debuffs,
        // locked and shown are on, and its position is dynamic
        Assert.Equal([CufOption.DisplayMainTankAndAssist, CufOption.DisplayBorder, CufOption.DisplayNonBossDebuffs,
            CufOption.DynamicPosition, CufOption.Locked, CufOption.Shown], OptionsOf(profile));
        Assert.Equal(36, profile.FrameHeight);
        Assert.Equal(72, profile.FrameWidth);
    }

    [Theory]
    // name length 7, then the one option: the four bytes the client saved for each
    [InlineData("0F000000", CufOption.KeepGroupsTogether)]
    [InlineData("0E800000", CufOption.DisplayPets)]
    [InlineData("0E400000", CufOption.DisplayMainTankAndAssist)]
    [InlineData("0E200000", CufOption.DisplayOnlyDispellableDebuffs)]
    [InlineData("0E100000", CufOption.DisplayPowerBar)]
    [InlineData("0E080000", CufOption.DisplayBorder)]
    [InlineData("0E040000", CufOption.UseClassColors)]
    [InlineData("0E020000", CufOption.HorizontalGroups)]
    [InlineData("0E010000", CufOption.DisplayNonBossDebuffs)]
    [InlineData("0E008000", CufOption.DynamicPosition)]
    [InlineData("0E004000", CufOption.Locked)]
    [InlineData("0E002000", CufOption.Shown)]
    [InlineData("0E001000", CufOption.AutoActivate2Players)]
    [InlineData("0E000800", CufOption.AutoActivate3Players)]
    [InlineData("0E000400", CufOption.AutoActivate5Players)]
    [InlineData("0E000200", CufOption.AutoActivate10Players)]
    [InlineData("0E000100", CufOption.AutoActivate15Players)]
    [InlineData("0E000080", CufOption.AutoActivate20Players)]
    [InlineData("0E000040", CufOption.AutoActivate40Players)]
    [InlineData("0E000020", CufOption.AutoActivatePvP)]
    [InlineData("0E000010", CufOption.AutoActivatePvE)]
    public void Modern_OneOptionSet_IsThatOption(string bits, CufOption expected)
    {
        CufProfile profile = Assert.Single(ReadModern("01000000" + bits + "24004800" + "0000000000" + "000000000000" + "5072696D617279"));

        Assert.Equal([expected], OptionsOf(profile));
    }

    [Fact]
    public void Modern_TheNumbers_AreInTheOrderTheClientSavedThem()
    {
        // saved by the client with height 41 and width 97, sorted alphabetically, health as a
        // percentage, the top anchored to BOTTOM at 111, the bottom to BOTTOM at 222, the left to LEFT at 333
        CufProfile profile = Assert.Single(ReadModern("01000000" + "0E000000" + "2900" + "6100" + "02" + "03" + "01" + "01" + "01" + "6F00" + "DE00" + "4D01" + "5072696D617279"));

        Assert.Equal((41, 97), (profile.FrameHeight, profile.FrameWidth));
        Assert.Equal((2, 3), (profile.SortBy, profile.HealthText));
        Assert.Equal((1, 1, 1), (profile.TopPoint, profile.BottomPoint, profile.LeftPoint));
        Assert.Equal((111, 222, 333), (profile.TopOffset, profile.BottomOffset, profile.LeftOffset));
    }

    [Fact]
    public void Modern_WrittenBack_IsTheBlobTheClientSaved()
    {
        Assert.Equal(ClientDefault, Convert.ToHexString(CufProfiles.WriteModern(ReadModern(ClientDefault))));

        string two = "02000000" + "0E49E000" + "24004800" + "0000000000" + "000000000000" + "5072696D617279"
                   + "08801010" + "2900610002030101016F00DE004D01" + "52616964";
        Assert.Equal(two, Convert.ToHexString(CufProfiles.WriteModern(ReadModern(two))));
    }

    [Theory]
    [InlineData("")]                                                    // no count
    [InlineData("01000000" + "0E49E000" + "2400")]                      // ends inside the numbers
    [InlineData("01000000" + "0E49E000" + "24004800" + "0000000000" + "000000000000" + "5072")]    // ends inside the name
    [InlineData("FF000000")]                                            // a count no client has
    [InlineData("00000000" + "00")]                                     // something after the last profile
    public void Modern_ABlobThatDoesNotAddUp_IsRefused(string hex)
    {
        Assert.False(CufProfiles.TryReadModern(Convert.FromHexString(hex), []));
    }

    // --- the 4.3.4 server, after TrinityCore, under TrinityCore's names

    private sealed class ServerProfile
    {
        public string Name = "";
        public HashSet<string> Options = [];
        public ushort FrameHeight, FrameWidth, TopOffset, BottomOffset, LeftOffset;
        public byte SortBy, HealthText, TopPoint, BottomPoint, LeftPoint;
    }

    /// <summary>TrinityCore 4.3.4 <c>WorldPackets::Misc::LoadCUFProfiles::Write</c>.</summary>
    private static byte[] ServerWrites(params ServerProfile[] profiles)
    {
        using var packet = new WorldPacket();
        packet.WriteUInt16(0);      // where a received packet has its opcode
        packet.WriteBits((uint)profiles.Length, 20);
        foreach (ServerProfile p in profiles)
        {
            packet.WriteBit(p.Options.Contains("CUF_UNK_157"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_10_PLAYERS"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_5_PLAYERS"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_25_PLAYERS"));
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_HEAL_PREDICTION"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_PVE"));
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_HORIZONTAL_GROUPS"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_40_PLAYERS"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_3_PLAYERS"));
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_AGGRO_HIGHLIGHT"));
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_BORDER"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_2_PLAYERS"));
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_NON_BOSS_DEBUFFS"));
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_MAIN_TANK_AND_ASSIST"));
            packet.WriteBit(p.Options.Contains("CUF_UNK_156"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_SPEC_2"));
            packet.WriteBit(p.Options.Contains("CUF_USE_CLASS_COLORS"));
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_POWER_BAR"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_SPEC_1"));
            packet.WriteBits((uint)Encoding.UTF8.GetByteCount(p.Name), 8);
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_ONLY_DISPELLABLE_DEBUFFS"));
            packet.WriteBit(p.Options.Contains("CUF_KEEP_GROUPS_TOGETHER"));
            packet.WriteBit(p.Options.Contains("CUF_UNK_145"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_15_PLAYERS"));
            packet.WriteBit(p.Options.Contains("CUF_DISPLAY_PETS"));
            packet.WriteBit(p.Options.Contains("CUF_AUTO_ACTIVATE_PVP"));
        }
        packet.FlushBits();
        foreach (ServerProfile p in profiles)
        {
            packet.WriteUInt16(p.LeftOffset);
            packet.WriteUInt16(p.FrameHeight);
            packet.WriteUInt16(p.BottomOffset);
            packet.WriteUInt8(p.BottomPoint);
            packet.WriteUInt16(p.TopOffset);
            packet.WriteUInt8(p.TopPoint);
            packet.WriteUInt8(p.HealthText);
            packet.WriteUInt8(p.SortBy);
            packet.WriteUInt16(p.FrameWidth);
            packet.WriteUInt8(p.LeftPoint);
            packet.WriteString(p.Name);
        }
        return packet.GetDataSpan().ToArray();
    }

    /// <summary>TrinityCore 4.3.4 <c>WorldSession::HandleSaveCUFProfiles</c>.</summary>
    private static List<ServerProfile> ServerReads(ReadOnlySpan<byte> payload)
    {
        var reader = new SpanPacketReader(payload);
        int count = (int)reader.ReadBits<uint>(20);
        List<ServerProfile> profiles = [];
        int[] nameLengths = new int[count];
        for (int i = 0; i < count; i++)
        {
            ServerProfile p = new();
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_SPEC_2");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_10_PLAYERS");
            Bit(ref reader, p, "CUF_UNK_157");
            Bit(ref reader, p, "CUF_DISPLAY_HEAL_PREDICTION");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_SPEC_1");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_PVP");
            Bit(ref reader, p, "CUF_DISPLAY_POWER_BAR");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_15_PLAYERS");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_40_PLAYERS");
            Bit(ref reader, p, "CUF_DISPLAY_PETS");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_5_PLAYERS");
            Bit(ref reader, p, "CUF_DISPLAY_ONLY_DISPELLABLE_DEBUFFS");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_2_PLAYERS");
            Bit(ref reader, p, "CUF_UNK_156");
            Bit(ref reader, p, "CUF_DISPLAY_NON_BOSS_DEBUFFS");
            Bit(ref reader, p, "CUF_DISPLAY_MAIN_TANK_AND_ASSIST");
            Bit(ref reader, p, "CUF_DISPLAY_AGGRO_HIGHLIGHT");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_3_PLAYERS");
            Bit(ref reader, p, "CUF_DISPLAY_BORDER");
            Bit(ref reader, p, "CUF_USE_CLASS_COLORS");
            Bit(ref reader, p, "CUF_UNK_145");
            nameLengths[i] = (int)reader.ReadBits<uint>(8);
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_PVE");
            Bit(ref reader, p, "CUF_DISPLAY_HORIZONTAL_GROUPS");
            Bit(ref reader, p, "CUF_AUTO_ACTIVATE_25_PLAYERS");
            Bit(ref reader, p, "CUF_KEEP_GROUPS_TOGETHER");
            profiles.Add(p);
        }
        reader.ResetBitReader();
        for (int i = 0; i < count; i++)
        {
            ServerProfile p = profiles[i];
            p.TopPoint = reader.ReadUInt8();
            p.Name = reader.ReadString(nameLengths[i]);
            p.BottomOffset = reader.ReadUInt16();
            p.FrameHeight = reader.ReadUInt16();
            p.FrameWidth = reader.ReadUInt16();
            p.TopOffset = reader.ReadUInt16();
            p.HealthText = reader.ReadUInt8();
            p.BottomPoint = reader.ReadUInt8();
            p.SortBy = reader.ReadUInt8();
            p.LeftOffset = reader.ReadUInt16();
            p.LeftPoint = reader.ReadUInt8();
        }
        Assert.Equal(0, reader.Remaining);
        return profiles;
    }

    private static void Bit(ref SpanPacketReader reader, ServerProfile profile, string name)
    {
        if (reader.ReadBit())
            profile.Options.Add(name);
    }

    /// <summary>Which of the server's options is which of the client's.</summary>
    public static TheoryData<string, CufOption> SharedOptions => new()
    {
        { "CUF_KEEP_GROUPS_TOGETHER", CufOption.KeepGroupsTogether },
        { "CUF_DISPLAY_PETS", CufOption.DisplayPets },
        { "CUF_DISPLAY_MAIN_TANK_AND_ASSIST", CufOption.DisplayMainTankAndAssist },
        { "CUF_DISPLAY_ONLY_DISPELLABLE_DEBUFFS", CufOption.DisplayOnlyDispellableDebuffs },
        { "CUF_DISPLAY_POWER_BAR", CufOption.DisplayPowerBar },
        { "CUF_DISPLAY_BORDER", CufOption.DisplayBorder },
        { "CUF_USE_CLASS_COLORS", CufOption.UseClassColors },
        { "CUF_DISPLAY_HORIZONTAL_GROUPS", CufOption.HorizontalGroups },
        { "CUF_DISPLAY_NON_BOSS_DEBUFFS", CufOption.DisplayNonBossDebuffs },
        { "CUF_UNK_145", CufOption.DynamicPosition },
        { "CUF_UNK_156", CufOption.Locked },
        { "CUF_UNK_157", CufOption.Shown },
        { "CUF_AUTO_ACTIVATE_2_PLAYERS", CufOption.AutoActivate2Players },
        { "CUF_AUTO_ACTIVATE_3_PLAYERS", CufOption.AutoActivate3Players },
        { "CUF_AUTO_ACTIVATE_5_PLAYERS", CufOption.AutoActivate5Players },
        { "CUF_AUTO_ACTIVATE_10_PLAYERS", CufOption.AutoActivate10Players },
        { "CUF_AUTO_ACTIVATE_15_PLAYERS", CufOption.AutoActivate15Players },
        { "CUF_AUTO_ACTIVATE_25_PLAYERS", CufOption.AutoActivate20Players },
        { "CUF_AUTO_ACTIVATE_40_PLAYERS", CufOption.AutoActivate40Players },
        { "CUF_AUTO_ACTIVATE_PVP", CufOption.AutoActivatePvP },
        { "CUF_AUTO_ACTIVATE_PVE", CufOption.AutoActivatePvE },
    };

    private static List<CufProfile> ReadLegacy(byte[] wire)
    {
        using WorldPacket packet = LegacyPacketBuilder.Receive(wire);
        List<CufProfile> profiles = [];
        Assert.True(CufProfiles.TryReadLegacy(packet, profiles));
        return profiles;
    }

    private static List<ServerProfile> WriteLegacy(params CufProfile[] profiles)
    {
        using var packet = new WorldPacket();
        CufProfiles.WriteLegacy(packet, [.. profiles]);
        return ServerReads(packet.GetDataSpan());
    }

    [Theory]
    [MemberData(nameof(SharedOptions))]
    public void Legacy_AnOptionTheServerSends_IsTheSameOptionOnTheClient(string serverOption, CufOption option)
    {
        CufProfile profile = Assert.Single(ReadLegacy(ServerWrites(new ServerProfile { Name = "Raid", Options = [serverOption] })));

        Assert.Equal([option], OptionsOf(profile));
        Assert.Equal(CufLegacyOnlyOptions.None, profile.LegacyOnly);
        Assert.Equal("Raid", profile.Name);
    }

    [Theory]
    [MemberData(nameof(SharedOptions))]
    public void Legacy_AnOptionTheClientSaves_IsTheSameOptionOnTheServer(string serverOption, CufOption option)
    {
        ServerProfile saved = Assert.Single(WriteLegacy(new CufProfile { Name = "Raid", Options = 1u << (int)option }));

        Assert.Equal([serverOption], saved.Options);
        Assert.Equal("Raid", saved.Name);
    }

    [Theory]
    [InlineData("CUF_DISPLAY_HEAL_PREDICTION", CufLegacyOnlyOptions.DisplayHealPrediction)]
    [InlineData("CUF_DISPLAY_AGGRO_HIGHLIGHT", CufLegacyOnlyOptions.DisplayAggroHighlight)]
    [InlineData("CUF_AUTO_ACTIVATE_SPEC_1", CufLegacyOnlyOptions.AutoActivateSpec1)]
    [InlineData("CUF_AUTO_ACTIVATE_SPEC_2", CufLegacyOnlyOptions.AutoActivateSpec2)]
    public void Legacy_AnOptionOnlyTheServerHas_IsKeptAsideAndGoesBack(string serverOption, CufLegacyOnlyOptions kept)
    {
        CufProfile profile = Assert.Single(ReadLegacy(ServerWrites(new ServerProfile { Name = "Raid", Options = [serverOption] })));

        Assert.Equal(kept, profile.LegacyOnly);
        Assert.Empty(OptionsOf(profile));
        // the client is told nothing of it: its four spare bits stay clear
        Assert.Equal("01000000" + "08000000", Convert.ToHexString(CufProfiles.WriteModern([profile]))[..16]);
        Assert.Equal([serverOption], Assert.Single(WriteLegacy(profile)).Options);
    }

    [Fact]
    public void Legacy_TheNumbers_KeepTheirNamesBothWays()
    {
        var sent = new ServerProfile
        {
            Name = "Raid", FrameHeight = 41, FrameWidth = 97, SortBy = 2, HealthText = 3,
            TopPoint = 1, BottomPoint = 2, LeftPoint = 3, TopOffset = 111, BottomOffset = 222, LeftOffset = 333,
        };

        CufProfile profile = Assert.Single(ReadLegacy(ServerWrites(sent)));

        Assert.Equal((41, 97, 2, 3), (profile.FrameHeight, profile.FrameWidth, profile.SortBy, profile.HealthText));
        Assert.Equal((1, 2, 3), (profile.TopPoint, profile.BottomPoint, profile.LeftPoint));
        Assert.Equal((111, 222, 333), (profile.TopOffset, profile.BottomOffset, profile.LeftOffset));

        ServerProfile saved = Assert.Single(WriteLegacy(profile));
        Assert.Equal((41, 97, 2, 3), (saved.FrameHeight, saved.FrameWidth, saved.SortBy, saved.HealthText));
        Assert.Equal((1, 2, 3), (saved.TopPoint, saved.BottomPoint, saved.LeftPoint));
        Assert.Equal((111, 222, 333), (saved.TopOffset, saved.BottomOffset, saved.LeftOffset));
    }

    [Fact]
    public void Legacy_TwoProfiles_HaveAllTheirBitsBeforeAnyOfTheirNumbers()
    {
        var first = new ServerProfile { Name = "Primary", Options = ["CUF_DISPLAY_PETS"], FrameHeight = 36 };
        var second = new ServerProfile { Name = "Healing", Options = ["CUF_AUTO_ACTIVATE_PVP", "CUF_UNK_157"], FrameHeight = 50 };

        List<CufProfile> profiles = ReadLegacy(ServerWrites(first, second));

        Assert.Equal(["Primary", "Healing"], profiles.Select(p => p.Name));
        Assert.Equal([CufOption.DisplayPets], OptionsOf(profiles[0]));
        Assert.Equal([CufOption.Shown, CufOption.AutoActivatePvP], OptionsOf(profiles[1]));
        Assert.Equal((36, 50), (profiles[0].FrameHeight, profiles[1].FrameHeight));

        List<ServerProfile> saved = WriteLegacy([.. profiles]);
        Assert.Equal(["Primary", "Healing"], saved.Select(p => p.Name));
        Assert.Equal(["CUF_DISPLAY_PETS"], saved[0].Options);
        Assert.Equal(["CUF_AUTO_ACTIVATE_PVP", "CUF_UNK_157"], saved[1].Options.Order());
    }

    [Fact]
    public void Legacy_NoProfiles_IsThreeBytesAndAnEmptyBlob()
    {
        Assert.Empty(ReadLegacy(ServerWrites()));
        Assert.Equal("00000000", Convert.ToHexString(CufProfiles.WriteModern([])));
    }

    [Fact]
    public void Legacy_APacketThatEndsEarly_IsRefused()
    {
        byte[] whole = ServerWrites(new ServerProfile { Name = "Primary" });
        using WorldPacket packet = LegacyPacketBuilder.Receive(whole[..^4]);

        Assert.False(CufProfiles.TryReadLegacy(packet, []));
    }

    // --- the handler

    [Fact]
    public void HandleLoadCUFProfiles_GivesTheClientItsBlob_AndRemembersWhatItCannotCarry()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        byte[] wire = ServerWrites(new ServerProfile
        {
            Name = "Primary", FrameHeight = 36, FrameWidth = 72,
            Options = ["CUF_DISPLAY_MAIN_TANK_AND_ASSIST", "CUF_DISPLAY_BORDER", "CUF_DISPLAY_NON_BOSS_DEBUFFS",
                       "CUF_UNK_145", "CUF_UNK_156", "CUF_UNK_157", "CUF_DISPLAY_HEAL_PREDICTION"],
        });

        harness.Deliver(Opcode.SMSG_LOAD_CUF_PROFILES, wire, harness.Client.HandleLoadCUFProfiles);

        var load = Assert.IsType<LoadCUFProfiles>(harness.ClientWire.Sent[^1].Packet);
        Assert.Equal(ClientDefault, Convert.ToHexString(load.Data));
        Assert.Equal(CufLegacyOnlyOptions.DisplayHealPrediction, harness.Session.GameState.CufOptionsKeptForServer["Primary"]);
    }
}
