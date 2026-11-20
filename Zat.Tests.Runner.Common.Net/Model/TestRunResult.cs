namespace Zat.Tests.Runner.Common.Net.Model;

/// <summary>
/// The result of a whole test run: the test results of all its sub-runs that gathered any.
/// </summary>
/// <param name="SubRunResults">The test results of the sub-runs, in the order they ran.</param>
/// <param name="IsStoppedByUser">Whether the test run was stopped before it ran all its sub-runs.</param>
/// <param name="ElapsedTime">How long the test run took.</param>
public record TestRunResult(
    IReadOnlyList<TestResult> SubRunResults,
    bool IsStoppedByUser,
    TimeSpan ElapsedTime);