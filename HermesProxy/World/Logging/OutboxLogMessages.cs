using System;
using HermesProxy.World.Outbox;
using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for <see cref="PacketOutbox{TPacket}"/>: what was held, released, timed out,
/// cancelled or thrown away.
///
/// Held and Released fire once per hold, so they are Debug. A timeout gets a Warning at most once
/// per ten seconds per outbox and Debug otherwise: a server that stops answering times out many
/// holds at once, and a warning apiece would cost more than the holds did. Failures inside a
/// release are Errors, because the outbox swallows them to keep the session alive and the log is
/// then the only trace. EventId 1500-1519 is reserved for this file.
/// </summary>
internal static partial class OutboxLogMessages
{
    [LoggerMessage(
        EventId = 1500,
        Level = LogLevel.Debug,
        Message = "[Outbox] {Direction} held kind={Kind} event={EventKind}:{EventValue} pending={Pending}")]
    public static partial void Held(
        ILogger logger, string direction, OutboxHoldKind kind, OutboxEventKind eventKind, ulong eventValue, int pending);

    [LoggerMessage(
        EventId = 1501,
        Level = LogLevel.Debug,
        Message = "[Outbox] {Direction} released kind={Kind} after {HeldMs:F1} ms")]
    public static partial void Released(
        ILogger logger, string direction, OutboxHoldKind kind, double heldMs);

    [LoggerMessage(
        EventId = 1502,
        Level = LogLevel.Warning,
        Message = "[Outbox] {Direction} hold timed out after {HeldMs:F0} ms, {Action} (kind={Kind} event={EventKind}:{EventValue}; further timeouts in the next 10 s log at Debug)")]
    public static partial void TimedOutWarning(
        ILogger logger, string direction, OutboxHoldKind kind, OutboxEventKind eventKind, ulong eventValue, double heldMs, OutboxTimeoutAction action);

    [LoggerMessage(
        EventId = 1503,
        Level = LogLevel.Debug,
        Message = "[Outbox] {Direction} hold timed out after {HeldMs:F0} ms, {Action} (kind={Kind} event={EventKind}:{EventValue})")]
    public static partial void TimedOut(
        ILogger logger, string direction, OutboxHoldKind kind, OutboxEventKind eventKind, ulong eventValue, double heldMs, OutboxTimeoutAction action);

    [LoggerMessage(
        EventId = 1504,
        Level = LogLevel.Debug,
        Message = "[Outbox] {Direction} discarded {Count} holds for scope {Scope}")]
    public static partial void Discarded(
        ILogger logger, string direction, int count, OutboxScope scope);

    [LoggerMessage(
        EventId = 1505,
        Level = LogLevel.Debug,
        Message = "[Outbox] {Direction} cancelled {Count} holds for key {KeyKind}:{KeyValue}")]
    public static partial void Cancelled(
        ILogger logger, string direction, int count, HoldKeyKind keyKind, ulong keyValue);

    [LoggerMessage(
        EventId = 1506,
        Level = LogLevel.Error,
        Message = "[Outbox] {Direction} {Stage} threw; the hold is dropped and the session continues")]
    public static partial void CalloutFailed(
        ILogger logger, Exception exception, string direction, string stage);

    [LoggerMessage(
        EventId = 1507,
        Level = LogLevel.Error,
        Message = "[Outbox] {Direction} refused a hold: {Max} holds already pending")]
    public static partial void Overflow(
        ILogger logger, string direction, int max);

    [LoggerMessage(
        EventId = 1508,
        Level = LogLevel.Debug,
        Message = "[Outbox] {Direction} refused a hold after the outbox was disposed")]
    public static partial void RefusedAfterDispose(
        ILogger logger, string direction);

    [LoggerMessage(
        EventId = 1509,
        Level = LogLevel.Error,
        Message = "[Outbox] {Direction} stopped a release chain after {Limit} releases; dropped {Dropped} more")]
    public static partial void ReleaseRunLimit(
        ILogger logger, string direction, int limit, int dropped);

    [LoggerMessage(
        EventId = 1510,
        Level = LogLevel.Warning,
        Message = "[Outbox] {Direction} dropped {Opcode} ({Connection}): no connection to write it to")]
    public static partial void WireUnavailable(
        ILogger logger, string direction, Enums.Opcode opcode, string connection);
}
