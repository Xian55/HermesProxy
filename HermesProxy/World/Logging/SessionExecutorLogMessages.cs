using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for the session executor: backpressure, overflow and events that threw.
///
/// Nothing here fires per event — that would cost more than the work it describes. Backpressure is
/// reported once per episode (on and off again), an event that throws is an Error because the
/// executor swallows it to keep the session alive, and overflow is the last word before the session
/// is disconnected. EventId 1520-1539 is reserved for this file.
/// </summary>
internal static partial class SessionExecutorLogMessages
{
    [LoggerMessage(
        EventId = 1520,
        Level = LogLevel.Error,
        Message = "[Executor] {Name}: an event threw; it is dropped and the session continues")]
    public static partial void EventFailed(ILogger logger, System.Exception exception, string name);

    [LoggerMessage(
        EventId = 1521,
        Level = LogLevel.Warning,
        Message = "[Executor] {Name}: {Depth} events queued, pausing reads")]
    public static partial void BackpressureOn(ILogger logger, string name, int depth);

    [LoggerMessage(
        EventId = 1522,
        Level = LogLevel.Information,
        Message = "[Executor] {Name}: queue drained to {Depth}, resuming reads")]
    public static partial void BackpressureOff(ILogger logger, string name, int depth);

    [LoggerMessage(
        EventId = 1523,
        Level = LogLevel.Error,
        Message = "[Executor] {Name}: {Depth} events queued, past the {Cap} cap; the session is dropped")]
    public static partial void Overflow(ILogger logger, string name, int depth, int cap);
}
