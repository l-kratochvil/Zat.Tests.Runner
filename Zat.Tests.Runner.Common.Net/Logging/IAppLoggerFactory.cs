namespace Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// Creates loggers bound to a log source.
/// </summary>
public interface IAppLoggerFactory
{
    /// <summary>
    /// Creates a logger for <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source of the entries, see <see cref="LogSources"/>.</param>
    /// <returns>Logger bound to <paramref name="source"/>.</returns>
    IAppLogger CreateLogger(string source);
}