using System.Text;
using Framework.Constants;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Wire cover for the Russian declined-name submission, which has no frozen <c>Read()</c> body to
/// be equivalent to — it is a path the proxy never had.
/// </summary>
/// <remarks>
/// <para>
/// The interesting part is that the five string lengths are byte counts in a 7-bit field, packed
/// as a block ahead of the strings themselves. Every name that reaches this packet is Cyrillic, so
/// a length taken from <c>string.Length</c> instead of the UTF-8 byte count would misread every
/// case after the first — and would still pass against ASCII test data.
/// </para>
/// <para>
/// Readers are built through <c>GetRemainingSpan()</c>, the accessor dispatch uses, so the two
/// header bytes are skipped the same way in the test as in production.
/// </para>
/// </remarks>
public class DeclinedNamesCodecTests
{
    static DeclinedNamesCodecTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static readonly WowGuid128 Guid = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);

    // The five oblique cases of "Гроза" as a ruRU client would submit them.
    private static readonly string[] Cases =
    [
        "Грозы", "Грозе", "Грозу", "Грозой", "Грозе"
    ];

    private static byte[] Frame(WowGuid128 guid, string[] names)
    {
        using var w = new WorldPacket(1u);
        w.WritePackedGuid128(guid);
        foreach (string name in names)
            w.WriteBits((uint)Encoding.UTF8.GetByteCount(name), 7);
        foreach (string name in names)
            w.WriteString(name);

        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return framed;
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    [Fact]
    public void Read_CyrillicCases_RecoversEveryCaseAndConsumesThePacket()
    {
        var r = ReaderOver(Frame(Guid, Cases));

        SetPlayerDeclinedNamesCodec.Read(ref r, out var packet);

        Assert.Equal(Guid, packet.Player);
        Assert.Equal(Cases, packet.Names);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>
    /// Latin names never reach a real backend — both emulators reject a name whose first letter is
    /// not Cyrillic — but they are the case where a char-count length happens to be correct, so
    /// they pin the reader against a regression that only Cyrillic data would otherwise catch.
    /// </summary>
    [Fact]
    public void Read_AsciiCases_RecoversEveryCase()
    {
        string[] ascii = ["Aaa", "Bbbb", "Ccccc", "Dddddd", "Eeeeeee"];

        var r = ReaderOver(Frame(Guid, ascii));

        SetPlayerDeclinedNamesCodec.Read(ref r, out var packet);

        Assert.Equal(ascii, packet.Names);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>
    /// A client that submits fewer cases than it has fields still sends all five lengths, so empty
    /// entries are normal rather than malformed.
    /// </summary>
    [Fact]
    public void Read_EmptyCases_YieldsEmptyStringsNotNull()
    {
        string[] empty = ["", "", "", "", ""];

        var r = ReaderOver(Frame(Guid, empty));

        SetPlayerDeclinedNamesCodec.Read(ref r, out var packet);

        Assert.Equal(GameLimits.MaxDeclinedNameCases, packet.Names.Length);
        Assert.All(packet.Names, name => Assert.Equal("", name));
    }

    /// <summary>
    /// Lengths are client-controlled, and a truncated packet must be dropped rather than throw out
    /// of a socket callback.
    /// </summary>
    [Fact]
    public void Read_TruncatedPayload_DoesNotThrow()
    {
        byte[] framed = Frame(Guid, Cases);
        byte[] truncated = framed[..(framed.Length - 6)];

        var r = ReaderOver(truncated);

        SetPlayerDeclinedNamesCodec.Read(ref r, out var packet);

        Assert.Equal(Guid, packet.Player);
        Assert.Equal(GameLimits.MaxDeclinedNameCases, packet.Names.Length);
    }
}

/// <summary>
/// The opcode mappings the declined-name path depends on, including the one that is deliberately
/// absent.
/// </summary>
public class DeclinedNamesOpcodeMappingTests
{
    [Theory]
    [InlineData(ClientVersionBuild.V3_4_3_54261, 13961u)]
    [InlineData(ClientVersionBuild.V1_14_1_40688, 0x3689u)]
    [InlineData(ClientVersionBuild.V2_5_3_41750, 0x3689u)]
    [InlineData(ClientVersionBuild.V3_3_5a_12340, 0x419u)]
    [InlineData(ClientVersionBuild.V2_4_3_8606, 0x418u)]
    public void SetPlayerDeclinedNames_IsMapped(ClientVersionBuild build, uint expected)
    {
        Assert.Equal(expected, Opcodes.GetOpcodeValueForVersion(Opcode.CMSG_SET_PLAYER_DECLINED_NAMES, build));
    }

    /// <summary>
    /// Vanilla predates the feature. This is what the handler's version guard exists for: an
    /// unmapped name resolves to 0, and forwarding that would put a nonsense opcode on the wire.
    /// </summary>
    [Fact]
    public void SetPlayerDeclinedNames_IsUnmappedOnVanilla()
    {
        Assert.Equal(0u, Opcodes.GetOpcodeValueForVersion(Opcode.CMSG_SET_PLAYER_DECLINED_NAMES, ClientVersionBuild.V1_12_1_5875));
    }

    [Theory]
    [InlineData(ClientVersionBuild.V3_4_3_54261, 12291u)]
    [InlineData(ClientVersionBuild.V3_3_5a_12340, 0x41Au)]
    [InlineData(ClientVersionBuild.V2_4_3_8606, 0x419u)]
    public void SetPlayerDeclinedNamesResult_IsMapped(ClientVersionBuild build, uint expected)
    {
        Assert.Equal(expected, Opcodes.GetOpcodeValueForVersion(Opcode.SMSG_SET_PLAYER_DECLINED_NAMES_RESULT, build));
    }
}
