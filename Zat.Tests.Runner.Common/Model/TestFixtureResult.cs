namespace Zat.Tests.Runner.Common.Model;

/// <summary>
/// The outcome of a test fixture and of the test cases beneath it.
/// </summary>
/// <param name="TestCaseResults">Test case results.</param>
/// <param name="EntityName">The execution path of the test fixture.</param>
/// <param name="Status">The status of the test fixture.</param>
/// <param name="Detail">The detail of the outcome.</param>
public record TestFixtureResult(
    TestCaseResult[] TestCaseResults,
    string EntityName,
    TestStatus Status,
    Detail? Detail = null)
    : TestEntityResult(EntityName, Status, Detail)
{
    /// <summary>
    /// Gets the test case results.
    /// </summary>
    public TestCaseResult[] TestCaseResults { get; } = TestCaseResults;
}