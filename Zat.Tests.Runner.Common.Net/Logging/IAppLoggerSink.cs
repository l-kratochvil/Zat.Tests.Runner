namespace Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// A destination the application log is mirrored to, in addition to the in-memory buffer.
/// </summary>
/// <remarks>
/// A sink is best effort: <see cref="Write"/> must never throw or block the caller, because it runs
/// on the hot path of logging.
/// </remarks>
public interface IAppLoggerSink
{
    /// <summary>
    /// Raised when the sink fails, so the failure can be surfaced in the log panel.
    /// </summary>
    event Action<string>? Failed;

    /// <summary>
    /// Hands <paramref name="entry"/> to the sink.
    /// </summary>
    /// <param name="entry">Log entry to write.</param>
    void Write(LogEntry entry);
}