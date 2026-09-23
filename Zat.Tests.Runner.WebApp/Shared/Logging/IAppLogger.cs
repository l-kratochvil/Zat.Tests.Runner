namespace Zat.Tests.Runner.WebApp.Shared.Logging;

/// <summary>
/// Writes log entries under one log source.
/// </summary>
/// <remarks>
/// This is the tester-facing log. Diagnostics still go through
/// <see cref="ILogger{TCategoryName}"/>.
/// </remarks>
public interface IAppLogger
{
    /// <summary>
    /// Gets the log source of every entry.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Writes a log entry for normal progress, including successful outcomes.
    /// </summary>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Info(string message, string? detail = null);

    /// <summary>
    /// Writes a log entry for something unexpected that does not stop the application.
    /// </summary>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Warning(string message, string? detail = null);

    /// <summary>
    /// Writes a log entry for a failed operation.
    /// </summary>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Error(string message, string? detail = null);

    /// <summary>
    /// Writes a log entry with the given <paramref name="severity"/>.
    /// </summary>
    /// <param name="severity">Severity of the log entry.</param>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Log(LogSeverity severity, string message, string? detail = null);
}