namespace Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// Known log sources, kept as plain strings so outside entries can be routed without a mapping.
/// </summary>
public static class LogSources
{
    /// <summary>
    /// Gets the source representing the application itself: lifecycle, navigation, and settings.
    /// </summary>
    public const string App = "App";

    /// <summary>
    /// Gets the source representing a test run and the output of the test runner.
    /// </summary>
    public const string TestRun = "TestRun";

    /// <summary>
    /// Gets the source representing communication with TestLink.
    /// </summary>
    public const string TestLink = "TestLink";

    /// <summary>
    /// Gets the list of all known sources.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = [App, TestRun, TestLink];
}