namespace Zat.Tests.Runner.Common.Net.Services;

using Zat.Tests.Runner.Common.Net.Model;

/// <summary>
/// Describes a service that handles the test results of a test run, both per sub-run and of the whole test run.
/// </summary>
public interface ITestResultHandler
{
    /// <summary>
    /// Handles the test result of one sub-run, once the sub-run completes.
    /// </summary>
    /// <param name="testResult">The handled <see cref="TestResult"/>.</param>
    void HandleSubRunResult(TestResult testResult);

    /// <summary>
    /// Handles the result of the whole test run, once it ends, stopped or not.
    /// </summary>
    /// <param name="testRunResult">The handled <see cref="TestRunResult"/>.</param>
    void HandleTestRunResult(TestRunResult testRunResult)
    {
    }
}