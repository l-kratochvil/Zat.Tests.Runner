namespace Zat.Tests.Runner.Common;

using Zat.Tests.Runner.Common.Model;

public record ProxyTestResult(TestCaseResult[] TestCaseResults)
{
    public TestCaseResult[] TestCaseResults { get; } = TestCaseResults;

    public TestStatus OverallStatus
    {
        get
        {
            if (this.FailedResults.Length > 0)
            {
                return TestStatus.Failure;
            }

            // TODO: Review the order of precedence for test statuses (it doesn't seem correct).
            if (this.WarningResults.Length > 0)
            {
                return TestStatus.Warning;
            }

            if (this.InconclusiveResults.Length > 0)
            {
                return TestStatus.Inconclusive;
            }

            if (this.PassedResults.Length > 0)
            {
                return TestStatus.Passed;
            }

            if (this.SkippedResults.Length > 0)
            {
                return TestStatus.Skipped;
            }

            return TestStatus.Unknown;
        }
    }

    public TestCaseResult[] PassedResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Passed)];

    public TestCaseResult[] FailureResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Failure)];

    public TestCaseResult[] ErrorResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Error)];

    public TestCaseResult[] InvalidResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Invalid)];

    public TestCaseResult[] WarningResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Warning)];

    public TestCaseResult[] InconclusiveResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Inconclusive)];

    public TestCaseResult[] SkippedResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Skipped)];

    public TestCaseResult[] IgnoredResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Ignored)];

    public TestCaseResult[] ExplicitResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Explicit)];

    public TestCaseResult[] OtherResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Other)];

    public TestCaseResult[] FailedResults
        => field ??= [..this.TestCaseResults.Where(
                x => x.Status is
                    TestStatus.Failure or
                    TestStatus.Error or
                    TestStatus.Invalid)];
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