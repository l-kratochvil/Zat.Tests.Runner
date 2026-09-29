namespace Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// Application log writer with a fixed log source.
/// </summary>
/// <param name="loggerHub">Hub entries are appended to.</param>
/// <param name="source">Log source stamped on every entry.</param>
public sealed class AppLogger(IAppLoggerHub loggerHub, string source) : IAppLogger
{
    /// <inheritdoc/>
    public string Source { get; } = source;

    /// <inheritdoc/>
    public void Info(string message, string? detail = null)
    {
        this.Log(LogSeverity.Info, message, detail);
    }

    /// <inheritdoc/>
    public void Warning(string message, string? detail = null)
    {
        this.Log(LogSeverity.Warning, message, detail);
    }

    /// <inheritdoc/>
    public void Error(string message, string? detail = null)
    {
        this.Log(LogSeverity.Error, message, detail);
    }

    /// <inheritdoc/>
    public void Log(LogSeverity severity, string message, string? detail = null)
    {
        loggerHub.Append(new LogEntry(DateTimeOffset.Now, severity, this.Source, message, detail));
    }
}