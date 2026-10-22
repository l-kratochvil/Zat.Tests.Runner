namespace Zat.Tests.Runner.WebApp.Features.AppLogging.Components;

using Zat.Tests.Runner.WebApp.Shared.Model;

/// <summary>
/// Filters which severities are shown in the log panel.
/// </summary>
public sealed class AppLoggerFilter
{
    private readonly HashSet<LogLevel> selectedLogLevels;

    private AppLoggerFilter(IEnumerable<LogLevel> selectedLogLevels)
    {
        this.selectedLogLevels = [.. selectedLogLevels];
    }

    /// <summary>
    /// Creates the filter used when the panel first opens: every logLevel is shown.
    /// </summary>
    /// <returns>The default filter.</returns>
    public static AppLoggerFilter CreateDefault()
    {
        return new AppLoggerFilter(Enum.GetValues<LogLevel>());
    }

    /// <summary>
    /// Determines whether log entries of <paramref name="logLevel"/> are shown.
    /// </summary>
    /// <param name="logLevel">LogLevel to test.</param>
    /// <returns><see langword="true"/> when <paramref name="logLevel"/> is shown.</returns>
    public bool IsSelected(LogLevel logLevel)
    {
        return this.selectedLogLevels.Contains(logLevel);
    }

    /// <summary>
    /// Shows or hides log entries of <paramref name="logLevel"/>.
    /// </summary>
    /// <param name="logLevel">LogLevel to change.</param>
    /// <param name="selected">Whether <paramref name="logLevel"/> should be shown.</param>
    public void SetSelected(LogLevel logLevel, bool selected)
    {
        if (selected)
        {
            this.selectedLogLevels.Add(logLevel);
        }
        else
        {
            this.selectedLogLevels.Remove(logLevel);
        }
    }

    /// <summary>
    /// Determines whether <paramref name="entry"/> passes the filter.
    /// </summary>
    /// <param name="entry">Log entry to test.</param>
    /// <returns><see langword="true"/> when <paramref name="entry"/> should be shown.</returns>
    public bool Matches(LogEntry entry)
    {
        return this.IsSelected(entry.LogLevel);
    }

    /// <summary>
    /// Applies the filter to log entries.
    /// </summary>
    /// <param name="entries">Log entries to filter.</param>
    /// <returns>Log entries that pass the filter, in the original order.</returns>
    public IEnumerable<LogEntry> Apply(IEnumerable<LogEntry> entries)
    {
        return entries.Where(this.Matches);
    }
}