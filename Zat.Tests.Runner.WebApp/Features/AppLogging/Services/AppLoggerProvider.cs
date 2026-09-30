namespace Zat.Tests.Runner.WebApp.Features.AppLogging.Services;

using Fluxor;
using Zat.Tests.Runner.WebApp.Shared.Model;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

public sealed class AppLoggerProvider(
    IDispatcher dispatcher)
    : ILoggerProvider
{
    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
        => new Logger(categoryName, dispatcher);

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    private class Logger(
        string categoryName,
        IDispatcher dispatcher)
        : ILogger
    {
        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => dispatcher.Dispatch(
                new DataChangedAction(new ValueChange<LogEntry>(
                    new LogEntry(
                        Timestamp: DateTimeOffset.Now,
                        LogLevel: logLevel,
                        Source: categoryName,
                        Message: formatter(state, exception),
                        Detail: exception?.ToString()))));
    }
}