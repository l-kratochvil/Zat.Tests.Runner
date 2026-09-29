namespace Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// One record in the application log.
/// </summary>
/// <param name="Timestamp">Moment the entry was created.</param>
/// <param name="Severity">Severity of the entry.</param>
/// <param name="Source">Log source of the entry, see <see cref="LogSources"/>.</param>
/// <param name="Message">Single-line message shown in the log.</param>
/// <param name="Detail">Optional multi-line detail, such as a stack trace.</param>
public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogSeverity Severity,
    string Source,
    string Message,
    string? Detail = null);