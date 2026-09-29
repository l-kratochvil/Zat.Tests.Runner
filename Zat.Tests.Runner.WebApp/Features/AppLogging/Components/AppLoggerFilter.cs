namespace Zat.Tests.Runner.WebApp.Features.AppLogging.Components;

using Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// Which severities and log sources are shown in the log panel.
/// </summary>
/// <remarks>
/// Severities are tracked as shown values because they are known up front. Log sources are tracked
/// as hidden values so a source outside <see cref="LogSources.All"/> stays visible by default.
/// </remarks>
public sealed class AppLoggerFilter
{
    private readonly HashSet<LogSeverity> selectedSeverities;
    private readonly HashSet<string> hiddenSources = new(StringComparer.OrdinalIgnoreCase);

    private AppLoggerFilter(IEnumerable<LogSeverity> selectedSeverities)
    {
        this.selectedSeverities = [.. selectedSeverities];
    }

    /// <summary>
    /// Creates the filter used when the panel first opens: every severity and log source is shown.
    /// </summary>
    /// <returns>The default filter.</returns>
    public static AppLoggerFilter CreateDefault()
    {
        return new AppLoggerFilter(Enum.GetValues<LogSeverity>());
    }

    /// <summary>
    /// Determines whether log entries of <paramref name="severity"/> are shown.
    /// </summary>
    /// <param name="severity">Severity to test.</param>
    /// <returns><see langword="true"/> when <paramref name="severity"/> is shown.</returns>
    public bool IsSelected(LogSeverity severity)
    {
        return this.selectedSeverities.Contains(severity);
    }

    /// <summary>
    /// Determines whether log entries of <paramref name="source"/> are shown.
    /// </summary>
    /// <param name="source">Log source to test.</param>
    /// <returns><see langword="true"/> when <paramref name="source"/> is shown.</returns>
    public bool IsSelected(string source)
    {
        return !this.hiddenSources.Contains(source);
    }

    /// <summary>
    /// Shows or hides log entries of <paramref name="severity"/>.
    /// </summary>
    /// <param name="severity">Severity to change.</param>
    /// <param name="selected">Whether <paramref name="severity"/> should be shown.</param>
    public void SetSelected(LogSeverity severity, bool selected)
    {
        if (selected)
        {
            this.selectedSeverities.Add(severity);
        }
        else
        {
            this.selectedSeverities.Remove(severity);
        }
    }

    /// <summary>
    /// Shows or hides log entries of <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Log source to change.</param>
    /// <param name="selected">Whether <paramref name="source"/> should be shown.</param>
    public void SetSelected(string source, bool selected)
    {
        if (selected)
        {
            this.hiddenSources.Remove(source);
        }
        else
        {
            this.hiddenSources.Add(source);
        }
    }

    /// <summary>
    /// Determines whether <paramref name="entry"/> passes the filter.
    /// </summary>
    /// <param name="entry">Log entry to test.</param>
    /// <returns><see langword="true"/> when <paramref name="entry"/> should be shown.</returns>
    public bool Matches(LogEntry entry)
    {
        return this.IsSelected(entry.Severity) && this.IsSelected(entry.Source);
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