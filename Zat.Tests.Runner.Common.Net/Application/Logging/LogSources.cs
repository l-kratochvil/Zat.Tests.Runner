namespace Zat.Tests.Runner.Common.Net.Application.Logging;

/// <summary>
/// Known log sources, kept as plain strings so outside entries can be routed without a mapping.
/// </summary>
public static class LogSources
{
    /// <summary>
    /// Gets the list of all known sources.
    /// </summary>
    public static IReadOnlyList<string> All { get; } =
    [
        nameof(App), nameof(TestRun), nameof(TestLink)
    ];

    /// <summary>
    /// Source representing the application itself: lifecycle, navigation, and settings.
    /// </summary>
    public record App;

    /// <summary>
    /// Source representing a test run and the output of the test runner.
    /// </summary>
    public record TestRun;

    /// <summary>
    /// Source representing communication with TestLink.
    /// </summary>
    public record TestLink;
}