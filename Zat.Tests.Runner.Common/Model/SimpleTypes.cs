namespace Zat.Tests.Runner.Common.Model;

using Zat.Tests.Runner.Common.Services;

/// <summary>
/// Represents a test result obtained from a <see cref="INUnitTestRunnerProxy"/>.
/// </summary>
/// <param name="TestCaseResults">Test case results.</param>
public record ProxyTestResult(TestCaseResult[] TestCaseResults)
{
    public TestCaseResult[] TestCaseResults { get; } = TestCaseResults;
}

public record TestCaseResult(
    int Id,
    string EntityName,
    string Message,
    string StackTrace,
    TestStatus Status)
{
    /// <summary>
    /// Gets the status of the test case.
    /// </summary>
    public TestStatus Status { get; } = Status;

    /// <summary>
    /// Gets the name of the entity associated with the test case.
    /// </summary>
    public string EntityName { get; } = EntityName;

    /// <summary>
    /// Gets the message associated with the test case result.
    /// </summary>
    public string Message { get; } = Message;

    /// <summary>
    /// Gets the stack trace associated with the test case result.
    /// </summary>
    public string StackTrace { get; } = StackTrace;

    /// <summary>
    /// Gets the identifier of the test case.
    /// </summary>
    public int Id { get; } = Id;
}