namespace Zat.Tests.Runner.WebApp.Shared.Model;

/// <summary>
/// One record in the application log.
/// </summary>
/// <param name="Timestamp">Moment the entry was created.</param>
/// <param name="LogLevel">Log level of the entry.</param>
/// <param name="Source">Log source of the entry.</param>
/// <param name="Message">Single-line message shown in the log.</param>
/// <param name="Detail">Optional multi-line detail, such as a stack trace.</param>
public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogLevel LogLevel,
    string Source,
    string Message,
    string? Detail = null);