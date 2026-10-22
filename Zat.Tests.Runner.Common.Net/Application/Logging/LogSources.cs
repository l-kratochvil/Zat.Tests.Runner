namespace Zat.Tests.Runner.Common.Net.Application.Logging;

/// <summary>
/// Log sources, used as logger categories.
/// </summary>
public static class LogSources
{
    /// <summary>
    /// The log provided to the user; it reaches only providers registered for the log.
    /// </summary>
    public record App;

    /// <summary>
    /// Default log source for general diagnostics.
    /// </summary>
    public record Default;

    /// <summary>
    /// Diagnostics of a test run..
    /// </summary>
    public record TestRun;

    /// <summary>
    /// Diagnostics of test result handling.
    /// </summary>
    public record TestResultHandler;
}