using HermesProxy.World.Enums;
using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging methods for <see cref="Server.WorldSocket"/> hot paths.
/// <c>NetDir</c> and <c>SourceFile</c> are intentional overflow properties so the
/// Serilog output template can render them in their own columns.
/// </summary>
#pragma warning disable SYSLIB1015
internal static partial class WorldSocketLogMessages
{
    // EventId 100-199 range is reserved for WorldSocket packet dispatch.

    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Debug,
        Message = "Received opcode {Opcode} ({OpcodeId}).")]
    public static partial void PacketReceived(
        ILogger logger,
        string SourceFile,
        string NetDir,
        Opcode Opcode,
        uint OpcodeId);

    /// <summary>
    /// Verbose variant of <see cref="PacketReceived"/> for noisy opcodes.
    /// Gated by Log.Server.MinimumLevel=Verbose. See <see cref="NoisyOpcodes"/>.
    /// </summary>
    [LoggerMessage(
        EventId = 103,
        Level = LogLevel.Trace,
        Message = "Received opcode {Opcode} ({OpcodeId}).")]
    public static partial void PacketReceivedNoisy(
        ILogger logger,
        string SourceFile,
        string NetDir,
        Opcode Opcode,
        uint OpcodeId);

    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Debug,
        Message = "Sending opcode {Opcode} ({OpcodeId}).")]
    public static partial void PacketSent(
        ILogger logger,
        string SourceFile,
        string NetDir,
        Opcode Opcode,
        uint OpcodeId);

    /// <summary>Verbose variant of <see cref="PacketSent"/> for noisy opcodes.</summary>
    [LoggerMessage(
        EventId = 104,
        Level = LogLevel.Trace,
        Message = "Sending opcode {Opcode} ({OpcodeId}).")]
    public static partial void PacketSentNoisy(
        ILogger logger,
        string SourceFile,
        string NetDir,
        Opcode Opcode,
        uint OpcodeId);

    [LoggerMessage(
        EventId = 102,
        Level = LogLevel.Warning,
        Message = "No handler for opcode {Opcode} ({OpcodeId}) (Got unknown packet from ModernClient)")]
    public static partial void NoHandlerForOpcode(
        ILogger logger,
        string SourceFile,
        string NetDir,
        Opcode Opcode,
        uint OpcodeId);

    [LoggerMessage(
        EventId = 110,
        Level = LogLevel.Debug,
        Message = "Guild bank swap player->bank tab={Tab} slot={BankSlot} srcBag={SrcBag}->{LegacyBag} srcSlot={SrcSlot}->{LegacySlot}")]
    public static partial void GuildBankPlayerToBank(
        ILogger logger,
        string SourceFile,
        string NetDir,
        byte Tab,
        byte BankSlot,
        byte SrcBag,
        byte LegacyBag,
        byte SrcSlot,
        byte LegacySlot);

    [LoggerMessage(
        EventId = 111,
        Level = LogLevel.Debug,
        Message = "Guild bank query results tab={Tab} tabs={TabCount} items={ItemCount} fullUpdate={FullUpdate} money={Money}")]
    public static partial void GuildBankQueryResults(
        ILogger logger,
        string SourceFile,
        string NetDir,
        int Tab,
        int TabCount,
        int ItemCount,
        bool FullUpdate,
        ulong Money);

    [LoggerMessage(
        EventId = 112,
        Level = LogLevel.Debug,
        Message = "Quest close quest={QuestId} action={Action}")]
    public static partial void QuestClose(
        ILogger logger,
        string SourceFile,
        string NetDir,
        int QuestId,
        string Action);

    [LoggerMessage(
        EventId = 113,
        Level = LogLevel.Debug,
        Message = "Arena team invite team={TeamId} name={Name}")]
    public static partial void ArenaTeamPartyInvite(
        ILogger logger,
        string SourceFile,
        string NetDir,
        uint TeamId,
        string Name);

    [LoggerMessage(
        EventId = 114,
        Level = LogLevel.Debug,
        Message = "CMSG_BATTLEMASTER_JOIN_ARENA teamIndex={TeamIndex} teamId={TeamId}")]
    public static partial void BattlemasterJoinArena(
        ILogger logger,
        string SourceFile,
        string NetDir,
        uint TeamIndex,
        uint TeamId);

    [LoggerMessage(
        EventId = 115,
        Level = LogLevel.Debug,
        Message = "CMSG_BATTLEMASTER_JOIN_SKIRMISH teamSize={TeamSize} asGroup={AsGroup}")]
    public static partial void BattlemasterJoinSkirmish(
        ILogger logger,
        string SourceFile,
        string NetDir,
        byte TeamSize,
        bool AsGroup);

    /// <summary>
    /// The modern client is told BadServer and dropped here, so this line is the only place the
    /// legacy world server's own verdict reaches the log on the client-facing side. Carrying
    /// <paramref name="LegacyAuthResult"/> across saves a .pkt decode to answer "why" -- see
    /// <see cref="WorldClientLogMessages.AuthenticationFailed"/> for what the codes mean.
    /// </summary>
    [LoggerMessage(
        EventId = 116,
        Level = LogLevel.Error,
        Message = "The WorldClient failed to connect to the selected world server! (legacy SMSG_AUTH_RESPONSE: {LegacyAuthResult})")]
    public static partial void WorldClientConnectFailed(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string LegacyAuthResult);

    /// <summary>
    /// A barber change could not be expressed as legacy BarberShopStyle rows, so it was refused
    /// instead of being forwarded into a silent drop on the legacy server.
    /// </summary>
    [LoggerMessage(
        EventId = 117,
        Level = LogLevel.Warning,
        Message = "Barber shop change refused: no BarberShopStyle row for race {Race} sex {Sex} (hairStyle {HairStyle} -> {HairStyleId}, facialHair {FacialHair} -> {FacialHairId}).")]
    public static partial void BarberShopStyleUnresolved(
        ILogger logger,
        string SourceFile,
        string NetDir,
        byte Race,
        byte Sex,
        byte HairStyle,
        byte FacialHair,
        uint HairStyleId,
        uint FacialHairId);
}
