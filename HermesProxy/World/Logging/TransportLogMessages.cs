using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for the transports the proxy sails itself -- the Strand of the
/// Ancients gunships on a backend that never relocates them -- and for the client's own
/// account of boarding and leaving one.
///
/// The rider message is the diagnostic that decided how much of issue #197 had to be built:
/// a 3.3.5a client attaches itself to a transport WMO it lands on and reports the transport
/// guid in its own movement packets, and the V3_4_3 client turned out to do the same, so the
/// passenger link needs no synthesis. It fires on attach and detach only, not on every
/// movement packet, and logs the standing-on guid next to the transport guid because the two
/// answer different questions: standing-on without transport means the client sees the
/// object under the player but is not treating it as a transport.
///
/// Guids are logged as their raw low half, matching the other files in this directory; the
/// record struct's generated ToString allocates. All Trace level. EventId 1110-1119 is
/// reserved for this file.
/// </summary>
internal static partial class TransportLogMessages
{
    [LoggerMessage(
        EventId = 1110,
        Level = LogLevel.Trace,
        Message = "[TransportSail] guidLow={GuidLow} entry={Entry} state={State} stopFrame={StopFrame} " +
                  "now={Now} level={Level}")]
    public static partial void SailScheduled(
        ILogger logger, ulong guidLow, uint entry, int state, uint stopFrame, uint now, int level);

    [LoggerMessage(
        EventId = 1111,
        Level = LogLevel.Trace,
        Message = "[TransportSail] guidLow={GuidLow} entry={Entry} state={State} parked: no stop frame known, " +
                  "level left expired")]
    public static partial void SailUnknownStopFrame(ILogger logger, ulong guidLow, uint entry, int state);

    [LoggerMessage(
        EventId = 1112,
        Level = LogLevel.Trace,
        Message = "[TransportRider] {Opcode} transport {PreviousLow}/{PreviousHigh} -> {TransportLow}/{TransportHigh} " +
                  "standingOnLow={StandingOnLow} offset=({X},{Y},{Z}) o={O} seat={Seat} world=({WorldX},{WorldY},{WorldZ})")]
    public static partial void ClientTransportChanged(
        ILogger logger, string opcode,
        ulong previousLow, ulong previousHigh, ulong transportLow, ulong transportHigh, ulong standingOnLow,
        float x, float y, float z, float o, sbyte seat,
        float worldX, float worldY, float worldZ);

    [LoggerMessage(
        EventId = 1114,
        Level = LogLevel.Trace,
        Message = "[TransportSail] guidLow={GuidLow} entry={Entry} OUT_OF_RANGE from the backend suppressed; " +
                  "transports are never range-destroyed")]
    public static partial void OutOfRangeSuppressed(ILogger logger, ulong guidLow, uint entry);

    [LoggerMessage(
        EventId = 1113,
        Level = LogLevel.Trace,
        Message = "[TransportSail] guidLow={GuidLow} entry={Entry} parentRotation forwarded on Values: " +
                  "({X},{Y},{Z},{W})")]
    public static partial void ParentRotationForwarded(
        ILogger logger, ulong guidLow, uint entry, float x, float y, float z, float w);

    // clientKnowsTransport is a ClientKnownGuids set probe at the call site, so this one has to
    // stay inside an IsEnabled block -- the lookup runs per create otherwise.
    [LoggerMessage(
        EventId = 1115,
        Level = LogLevel.Trace,
        Message = "[TransportRider] passenger create guidLow={GuidLow} " +
                  "transport={TransportLow}/{TransportHigh} clientKnowsTransport={ClientKnowsTransport} " +
                  "offset=({X},{Y},{Z}) seat={Seat}")]
    public static partial void PassengerCreate(
        ILogger logger, ulong guidLow, ulong transportLow, ulong transportHigh,
        bool clientKnowsTransport, float x, float y, float z, sbyte seat);
}
