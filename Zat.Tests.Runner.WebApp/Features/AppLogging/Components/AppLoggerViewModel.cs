namespace Zat.Tests.Runner.WebApp.Features.AppLogging.Components;

using CommunityToolkit.Mvvm.ComponentModel;

using Fluxor;

using Zat.Tests.Runner.WebApp.Shared.Model;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// The log as the log panel shows it: the entries that pass the filter, and whether the panel is open at all.
/// </summary>
public sealed partial class AppLoggerViewModel : ViewModelBase, IDisposable
{
    private readonly IState<AppLoggingState> state;
    private readonly AppLoggerFilter filter = AppLoggerFilter.CreateDefault();

    public AppLoggerViewModel(IState<AppLoggingState> state)
    {
        this.state = state;

        this.state.StateChanged += this.OnStateChanged;
    }

    /// <summary>
    /// Gets a value indicating whether the panel shows only how many errors there are.
    /// </summary>
    [ObservableProperty]
    public partial bool IsCollapsed { get; private set; }

    /// <summary>
    /// Gets the entries that pass the filter, oldest first.
    /// </summary>
    public IReadOnlyList<LogEntry> Entries
        => [..this.filter.Apply(this.state.Value.Entries)];

    /// <summary>
    /// Gets how many errors the log holds, whatever the filter hides.
    /// </summary>
    public int ErrorCount
        => this.state.Value.Entries.Count(entry => entry.LogLevel == LogLevel.Error);

    /// <summary>
    /// Determines whether entries of <paramref name="logLevel"/> are shown.
    /// </summary>
    /// <param name="logLevel">Log level to test.</param>
    /// <returns><see langword="true"/> when <paramref name="logLevel"/> is shown.</returns>
    public bool IsSelected(LogLevel logLevel)
        => this.filter.IsSelected(logLevel);

    /// <summary>
    /// Shows or hides entries of <paramref name="logLevel"/>.
    /// </summary>
    /// <param name="logLevel">Log level to change.</param>
    /// <param name="selected">Whether <paramref name="logLevel"/> should be shown.</param>
    public void SetSelected(LogLevel logLevel, bool selected)
    {
        this.filter.SetSelected(logLevel, selected);
        this.OnPropertyChanged(nameof(this.Entries));
    }

    /// <summary>
    /// Opens a collapsed panel, or collapses an open one.
    /// </summary>
    public void ToggleCollapsed()
        => this.IsCollapsed = !this.IsCollapsed;

    /// <inheritdoc/>
    public void Dispose()
        => this.state.StateChanged -= this.OnStateChanged;

    private void OnStateChanged(object? sender, EventArgs e)
    {
        this.OnPropertyChanged(nameof(this.Entries));
        this.OnPropertyChanged(nameof(this.ErrorCount));
    }
}