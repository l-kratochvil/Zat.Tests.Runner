namespace Zat.Tests.Runner.WebApp.Application.Logging;

using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// Mirrors every application log entry into <see cref="ILogger"/>, so that the entries reach the
/// logging pipeline of the host and, through it, the log file.
/// </summary>
/// <remarks>
/// The log source becomes the logger category, so that one channel can be filtered on its own
/// through the standard <c>Logging:&lt;provider&gt;:LogLevel</c> configuration. This is the only
/// way the application log reaches the disk: nothing writes to a file directly.
/// </remarks>
/// <param name="loggerFactory">Factory the category loggers are created from.</param>
public sealed class DiagnosticsLoggerSink(ILoggerFactory loggerFactory) : IAppLoggerSink
{
    /// <summary>
    /// Prefix every logger category of the application log starts with.
    /// </summary>
    public const string CategoryPrefix = "Zat.Tests.Runner.TuiAppLog.";

    private readonly ConcurrentDictionary<string, ILogger> loggers = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    /// <remarks>
    /// Never raised: handing an entry to <see cref="ILogger"/> cannot fail. A failure of the
    /// destination behind the pipeline is reported by whoever owns that destination, which is why
    /// the accessors are empty rather than backed by a delegate nobody ever invokes.
    /// </remarks>
    event Action<string>? IAppLoggerSink.Failed
    {
        add { }

        remove { }
    }

    /// <summary>
    /// Builds the logger category entries of the given log source are written under.
    /// </summary>
    /// <param name="source">Source of the entries, see <see cref="LogSources"/>.</param>
    /// <returns>The logger category.</returns>
    public static string GetCategory(string source)
    {
        return CategoryPrefix + source;
    }

    /// <inheritdoc/>
    public void Write(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var logger = this.loggers.GetOrAdd(
            entry.Source,
            source => loggerFactory.CreateLogger(GetCategory(source)));

        var level = GetLevel(entry.Severity);

        if (string.IsNullOrEmpty(entry.Detail))
        {
            logger.Log(level, "{Message}", entry.Message);

            return;
        }

        // The detail stays a value of its own instead of being concatenated into the message, so
        // that the message template keeps its meaning for structured destinations. Destinations
        // rendering plain text see it as the lines following the message.
        logger.Log(level, "{Message}\n{Detail}", entry.Message, entry.Detail);
    }

    private static LogLevel GetLevel(LogSeverity severity)
        => severity switch
        {
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Error => LogLevel.Error,
            _ => LogLevel.Information,
        };
}