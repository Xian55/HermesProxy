using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Cover for the world-auth failure log lines.
///
/// The legacy server's verdict used to be dropped on the floor — the line read only
/// "Authentication failed!", so telling <c>AUTH_FAILED</c> (13, a digest or session-key mismatch)
/// apart from <c>AUTH_REJECT</c> (14, credentials accepted and a policy check refused the session)
/// meant decoding the <c>.pkt</c> by hand. That distinction is what identified the cause of
/// <see href="https://github.com/Xian55/HermesProxy/issues/248">#248</see>.
///
/// These assert the rendered text because the code is only useful to a human reading a log, and
/// because the failure path needs a backend that refuses the session, which no test backend does.
/// </summary>
public class AuthFailureLoggingTests
{
    private sealed class CapturingLogger : ILogger
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    [Theory]
    [InlineData(AuthResult.AUTH_REJECT, 14)]
    [InlineData(AuthResult.AUTH_FAILED, 13)]
    [InlineData(AuthResult.AUTH_BANNED, 28)]
    public void AuthenticationFailed_NamesTheCodeAndItsRawValue(AuthResult result, byte expectedId)
    {
        var logger = new CapturingLogger();

        WorldClientLogMessages.AuthenticationFailed(logger, "WorldClient", "", result, (byte)result);

        string message = Assert.Single(logger.Messages);
        Assert.Contains(result.ToString(), message);
        Assert.Contains($"({expectedId})", message);
        Assert.Equal(expectedId, (byte)result);
    }

    /// <summary>
    /// The client-facing half. <c>WorldSocket</c> tells the modern client <c>BadServer</c> and
    /// drops the session, so this is the only place the backend's verdict reaches the log on that
    /// side — including when no response arrived at all, which is what a local TrinityCore does:
    /// it sends <c>AUTH_REJECT</c> and closes so promptly that the read hits EOF first.
    /// </summary>
    [Fact]
    public void WorldClientConnectFailed_CarriesTheLegacyVerdict()
    {
        var logger = new CapturingLogger();

        WorldSocketLogMessages.WorldClientConnectFailed(logger, "WorldSocket", "", "AUTH_REJECT (14)");

        Assert.Contains("AUTH_REJECT (14)", Assert.Single(logger.Messages));
    }

    [Fact]
    public void WorldClientConnectFailed_NoResponse_SaysSo()
    {
        var logger = new CapturingLogger();

        WorldSocketLogMessages.WorldClientConnectFailed(logger, "WorldSocket", "", "none received");

        Assert.Contains("none received", Assert.Single(logger.Messages));
    }
}
