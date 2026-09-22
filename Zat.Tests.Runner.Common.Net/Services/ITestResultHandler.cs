namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// Describes a service that handles the test run result.
/// </summary>
public interface ITestResultHandler
{
    /// <summary>
    /// Handles the test run result.
    /// </summary>
    /// <param name="testRunResult">The handled <see cref="TestRunResult"/>.</param>
    void Handle(TestRunResult testRunResult);
}