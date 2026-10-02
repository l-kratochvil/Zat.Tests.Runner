namespace Zat.Tests.Runner.Common.Net.Model;

/// <summary>
/// The state of the test run on this environment, shared by everyone who can start or stop it.
/// </summary>
public enum TestRunState
{
    /// <summary>
    /// No test run is in progress, so one can be started.
    /// </summary>
    Stopped,

    /// <summary>
    /// A test run is in progress.
    /// </summary>
    Running,

    /// <summary>
    /// A stop of the test run was requested and the run is winding down.
    /// </summary>
    Stopping,
}