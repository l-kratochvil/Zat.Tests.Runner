namespace Zat.Tests.Runner.Common.Net.Model;

using Zat.Tests.Runner.Common.Model;
using Zat.Z2xxTests.Common.Model;

public record TestResult(
    ProxyTestResult ProxyTestResult,
    TestType TestType,
    HwAssemblyType? TestedHwAssemblyType = null)
    : ProxyTestResult(ProxyTestResult)
{
    private TestStatus? overallStatus;

    /// <summary>
    /// Gets the overall status of the test result.
    /// </summary>
    public TestStatus OverallStatus
        => this.overallStatus ??= this.GetOverallStatus();

    /// <summary>
    /// Gets the overall status of the test result.
    /// </summary>
    public TestCaseResult[] PassedResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Passed)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Failure"/>.
    /// </summary>
    public TestCaseResult[] FailureResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Failure)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Error"/>.
    /// </summary>
    public TestCaseResult[] ErrorResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Error)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Invalid"/>.
    /// </summary>
    public TestCaseResult[] InvalidResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Invalid)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Warning"/>.
    /// </summary>
    public TestCaseResult[] WarningResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Warning)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Inconclusive"/>.
    /// </summary>
    public TestCaseResult[] InconclusiveResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Inconclusive)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Skipped"/>.
    /// </summary>
    public TestCaseResult[] SkippedResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Skipped)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Ignored"/>.
    /// </summary>
    public TestCaseResult[] IgnoredResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Ignored)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Explicit"/>.
    /// </summary>
    public TestCaseResult[] ExplicitResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Explicit)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Other"/>.
    /// </summary>
    public TestCaseResult[] OtherResults
        => field ??= [..this.TestCaseResults.Where(x => x.Status is TestStatus.Other)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Failure"/>, <see cref="TestStatus.Error"/>, or <see cref="TestStatus.Invalid"/>.
    /// </summary>
    public TestCaseResult[] FailedResults
        => field ??= [..this.TestCaseResults.Where(
                x => x.Status is
                    TestStatus.Failure or
                    TestStatus.Error or
                    TestStatus.Invalid)];

    private TestStatus GetOverallStatus()
    {
        // Passed
        if (this.TestCaseResults.All(x => x.Status is TestStatus.Passed))
        {
            return TestStatus.Passed;
        }

        // Failed
        if (this.FailureResults.Length > 0)
        {
            return TestStatus.Failure;
        }

        if (this.ErrorResults.Length > 0)
        {
            return TestStatus.Error;
        }

        if (this.InvalidResults.Length > 0)
        {
            return TestStatus.Invalid;
        }

        // Blocked
        if (this.InconclusiveResults.Length > 0)
        {
            return TestStatus.Inconclusive;
        }

        if (this.WarningResults.Length > 0)
        {
            return TestStatus.Warning;
        }

        if (this.SkippedResults.Length > 0)
        {
            return TestStatus.Skipped;
        }

        if (this.IgnoredResults.Length > 0)
        {
            return TestStatus.Ignored;
        }

        if (this.ExplicitResults.Length > 0)
        {
            return TestStatus.Explicit;
        }

        if (this.OtherResults.Length > 0)
        {
            return TestStatus.Other;
        }

        return TestStatus.Unknown;
    }
}