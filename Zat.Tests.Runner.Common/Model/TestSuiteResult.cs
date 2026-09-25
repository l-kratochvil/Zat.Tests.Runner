namespace Zat.Tests.Runner.Common.Model;

/// <summary>
/// The outcome of a test suite and of the test fixtures beneath it.
/// </summary>
/// <param name="TestFixtureResults">Test fixture results.</param>
/// <param name="EntityName">The execution path of the test suite.</param>
/// <param name="Status">The status of the test suite.</param>
/// <param name="Detail">The detail of the outcome.</param>
public record TestSuiteResult(
    TestFixtureResult[] TestFixtureResults,
    string EntityName,
    TestStatus Status,
    Detail? Detail = null)
    : TestEntityResult(EntityName, Status, Detail)
{
    /// <summary>
    /// Gets the test fixture results.
    /// </summary>
    public TestFixtureResult[] TestFixtureResults { get; } = TestFixtureResults;
}