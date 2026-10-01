namespace Zat.Tests.Runner.WebApp.Features.AppLogging.Services;

using System.Collections.Concurrent;

using Microsoft.Extensions.Logging.Abstractions;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Model;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

public sealed class AppLoggerProvider(
    GlobalWeakReferenceMessanger globalMessanger)
    : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, Logger> loggers = new();

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
        => this.loggers.GetOrAdd(
            categoryName, _ => new Logger(categoryName, globalMessanger));

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    private class Logger(
        string categoryName,
        GlobalWeakReferenceMessanger globalMessanger)
        : ILogger
    {
        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => NullLogger.Instance.BeginScope(state);

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
            => logLevel != LogLevel.None;

        /// <inheritdoc/>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => globalMessanger.Send(
                new DataChangedAction(new ValueChange<LogEntry>(
                    new LogEntry(
                        Timestamp: DateTimeOffset.Now,
                        LogLevel: logLevel,
                        Source: categoryName,
                        Message: formatter(state, exception),
                        Detail: exception?.ToString()))));
    }
}