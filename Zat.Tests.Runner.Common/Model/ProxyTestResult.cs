namespace Zat.Tests.Runner.Common.Model;

using Zat.Tests.Runner.Common.Services;

/// <summary>
/// Represents a test result obtained from a <see cref="INUnitTestRunnerProxy"/>: a tree mirroring the test tree.
/// </summary>
/// <param name="TestSuiteResults">Test suite results.</param>
public record ProxyTestResult(TestSuiteResult[] TestSuiteResults)
{
    /// <summary>
    /// Gets the test suite results.
    /// </summary>
    public TestSuiteResult[] TestSuiteResults { get; } = TestSuiteResults;
}