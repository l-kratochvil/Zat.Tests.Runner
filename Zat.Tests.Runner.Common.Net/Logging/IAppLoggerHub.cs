namespace Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// The application log buffer shared by every browser connected to this server.
/// </summary>
public interface IAppLoggerHub
{
    /// <summary>
    /// Raised on the caller's thread after a log entry is appended.
    /// </summary>
    event Action<LogEntry>? EntryAppended;

    /// <summary>
    /// Appends <paramref name="entry"/> to the buffer and to every sink.
    /// </summary>
    /// <param name="entry">Log entry to append.</param>
    void Append(LogEntry entry);

    /// <summary>
    /// Records that something feeding the log has failed.
    /// </summary>
    /// <remarks>
    /// The failure reaches the buffer only. Sending it to sinks would either fail again or, for a
    /// sink on the logging pipeline itself, loop straight back into that pipeline.
    /// </remarks>
    /// <param name="message">Description of the failure, shown to the user.</param>
    void ReportFailure(string message);

    /// <summary>
    /// Takes a snapshot of the buffered log entries, oldest first.
    /// </summary>
    /// <returns>The buffered log entries.</returns>
    IReadOnlyList<LogEntry> GetEntries();
}