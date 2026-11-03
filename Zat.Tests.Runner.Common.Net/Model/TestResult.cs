namespace Zat.Tests.Runner.Common.Net.Model;

using Zat.Tests.Runner.Common.Model;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// The test result of one sub-run, including its recorded timing and the shared selection of reportable entities.
/// </summary>
/// <param name="ProxyTestResult">The test result obtained from the proxy.</param>
/// <param name="TestType">The type of the executed tests.</param>
/// <param name="StartTime">When the sub-run started.</param>
/// <param name="EndTime">When the sub-run ended.</param>
/// <param name="TestedHwAssemblyType">The tested HW assembly type, required for runtime tests.</param>
public record TestResult(
    ProxyTestResult ProxyTestResult,
    TestType TestType,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    HwAssemblyType? TestedHwAssemblyType = null)
    : ProxyTestResult(ProxyTestResult)
{
    /// <summary>
    /// The non-passed statuses ordered from the one taking precedence in <see cref="OverallStatus"/>.
    /// </summary>
    private static readonly TestStatus[] OverallStatusPrecedence =
    [
        TestStatus.Failure,
        TestStatus.Error,
        TestStatus.Invalid,
        TestStatus.Inconclusive,
        TestStatus.Warning,
        TestStatus.Skipped,
        TestStatus.Ignored,
        TestStatus.Explicit,
        TestStatus.Unknown,
    ];

    private TestStatus? overallStatus;

    /// <summary>
    /// Gets a value indicating whether the test entity has failed.
    /// </summary>
    public bool Failed
        => this.FailedResults.Length > 0;

    /// <summary>
    /// Gets the overall status of the test result, taking the statuses of test suites and test fixtures into account
    /// as well as the statuses of test cases.
    /// </summary>
    public TestStatus OverallStatus
        => this.overallStatus ??= this.GetOverallStatus();

    /// <summary>
    /// Gets the duration of the sub-run.
    /// </summary>
    public TimeSpan Duration
        => this.EndTime - this.StartTime;

    /// <summary>
    /// Gets the test fixture results of all test suites.
    /// </summary>
    public TestFixtureResult[] TestFixtureResults
        => field ??= [.. this.TestSuiteResults.SelectMany(x => x.TestFixtureResults)];

    /// <summary>
    /// Gets the test case results of all test fixtures.
    /// </summary>
    public TestCaseResult[] TestCaseResults
        => field ??= [.. this.TestFixtureResults.SelectMany(x => x.TestCaseResults)];

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Passed"/>.
    /// </summary>
    public TestCaseResult[] PassedResults
        => field ??= this.GetTestCaseResults(TestStatus.Passed);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Failure"/>.
    /// </summary>
    public TestCaseResult[] FailureResults
        => field ??= this.GetTestCaseResults(TestStatus.Failure);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Error"/>.
    /// </summary>
    public TestCaseResult[] ErrorResults
        => field ??= this.GetTestCaseResults(TestStatus.Error);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Invalid"/>.
    /// </summary>
    public TestCaseResult[] InvalidResults
        => field ??= this.GetTestCaseResults(TestStatus.Invalid);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Warning"/>.
    /// </summary>
    public TestCaseResult[] WarningResults
        => field ??= this.GetTestCaseResults(TestStatus.Warning);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Inconclusive"/>.
    /// </summary>
    public TestCaseResult[] InconclusiveResults
        => field ??= this.GetTestCaseResults(TestStatus.Inconclusive);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Skipped"/>.
    /// </summary>
    public TestCaseResult[] SkippedResults
        => field ??= this.GetTestCaseResults(TestStatus.Skipped);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Ignored"/>.
    /// </summary>
    public TestCaseResult[] IgnoredResults
        => field ??= this.GetTestCaseResults(TestStatus.Ignored);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Explicit"/>.
    /// </summary>
    public TestCaseResult[] ExplicitResults
        => field ??= this.GetTestCaseResults(TestStatus.Explicit);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Unknown"/>.
    /// </summary>
    public TestCaseResult[] UnknownResults
        => field ??= this.GetTestCaseResults(TestStatus.Unknown);

    /// <summary>
    /// Gets the test case results with a status of <see cref="TestStatus.Failure"/>, <see cref="TestStatus.Error"/>,
    /// or <see cref="TestStatus.Invalid"/>.
    /// </summary>
    public TestCaseResult[] FailedResults
        => field ??= [.. this.TestCaseResults.Where(x => x.Failed)];

    /// <summary>
    /// Gets the test case results that were not run: with a status of <see cref="TestStatus.Skipped"/>,
    /// <see cref="TestStatus.Ignored"/>, <see cref="TestStatus.Explicit"/> or <see cref="TestStatus.Unknown"/>.
    /// </summary>
    public TestCaseResult[] NotRunResults
        => field ??= this.GetTestCaseResults(
            TestStatus.Skipped,
            TestStatus.Ignored,
            TestStatus.Explicit,
            TestStatus.Unknown);

    /// <summary>
    /// Gets the non-passed results to report for entities in the test tree, ordered by status.
    /// A non-passed suite or fixture is reported once instead of its children.
    /// </summary>
    public TestEntityResult[] ReportedNotRunResults
        => field ??= this.GetReportedResults(
            TestStatus.Ignored,
            TestStatus.Explicit,
            TestStatus.Skipped,
            TestStatus.Unknown);

    /// <summary>
    /// Gets the problem results to report for entities in the test tree, ordered by status.
    /// A non-passed suite or fixture is reported once instead of its children.
    /// </summary>
    public TestEntityResult[] ReportedProblemResults
        => field ??= this.GetReportedResults(
            TestStatus.Error,
            TestStatus.Invalid,
            TestStatus.Failure,
            TestStatus.Warning);

    /// <summary>
    /// Gets test case results skipped or unknown for a reason other than ignored or explicit.
    /// </summary>
    public TestCaseResult[] OtherNotRunResults
        => field ??= this.GetTestCaseResults(TestStatus.Skipped, TestStatus.Unknown);

    private TestCaseResult[] GetTestCaseResults(params TestStatus[] statuses)
        => [.. this.TestCaseResults.Where(x => statuses.Contains(x.Status))];

    private TestEntityResult[] GetReportedResults(params TestStatus[] statuses)
    {
        var results = new List<TestEntityResult>();

        foreach (var testSuite in this.TestSuiteResults)
        {
            if (testSuite.Status is not TestStatus.Passed)
            {
                results.Add(testSuite);
                continue;
            }

            foreach (var testFixture in testSuite.TestFixtureResults)
            {
                if (testFixture.Status is not TestStatus.Passed)
                {
                    results.Add(testFixture);
                    continue;
                }

                results.AddRange(testFixture.TestCaseResults.Where(x => x.Status is not TestStatus.Passed));
            }
        }

        return
        [
            .. statuses.SelectMany(status => results.Where(result => result.Status == status)),
        ];
    }

    private TestStatus GetOverallStatus()
    {
        TestStatus[] statuses =
        [
            .. this.TestSuiteResults.Select(x => x.Status),
            .. this.TestFixtureResults.Select(x => x.Status),
            .. this.TestCaseResults.Select(x => x.Status),
        ];

        return statuses.All(x => x is TestStatus.Passed)
            ? TestStatus.Passed
            : OverallStatusPrecedence
                .Where(statuses.Contains)
                .DefaultIfEmpty(TestStatus.Unknown)
                .First();
    }
}