namespace Zat.Tests.Runner.Common.Net.Services;

using System.Diagnostics;
using System.Text;

using DevKit.Core.Extensions;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net;

public class TestLinkResultHandler(
    ITestLink testLink,
    TestLinkResultHandler.IContext context)
    : ITestResultHandler
{
    private const int RuntimeTestsTestPlanId = 9560;
    private const int ApplicationTestsTestPlanId = 10130;

    /// <inheritdoc />
    public void Handle(TestResult testResult)
    {
        Debug.Fail("TODO");

        if (!context.IsTestLinkReportingEnabled)
        {
            return;
        }

        var testPlanId = testResult.TestType switch
        {
            TestType.Runtime => RuntimeTestsTestPlanId,
            TestType.Application => ApplicationTestsTestPlanId,
            TestType.Unknown => throw new InvalidOperationException("Unknown test type"),
            _ => throw new NotSupportedException(testResult.TestType.ToString()),
        };

        // TODO: Add "beta" suffix with its version (e.g. beta1, beta2, ...)
        var buildName = $"IDE v{context.IdeVersion}, RT v{context.RuntimeVersion}";

        if (testResult.TestType == TestType.Runtime)
        {
            if (testResult.TestedHwAssemblyType is null)
            {
                throw new InvalidOperationException(
                    $"Result has no {nameof(testResult.TestedHwAssemblyType)} " +
                    $"but it's required for runtime tests");
            }

            buildName += $" : {testResult.TestedHwAssemblyType}";
        }

        if (testLink.GetBuildsForTestPlan(testPlanId).All(x => x.name != buildName))
        {
            var buildNotesBuilder = new StringBuilder().AppendLine("# Datum vydání");

            if (!string.IsNullOrEmpty(context.IdeReleaseDate))
            {
                buildNotesBuilder.AppendLine($"- IDE: {context.IdeReleaseDate}");
            }

            if (!string.IsNullOrEmpty(context.RuntimeReleaseDate))
            {
                buildNotesBuilder.AppendLine($"- RT: {context.RuntimeReleaseDate}");
            }

            const string newLine = "\n";
            var buildNotesHtml = string.Join(
                string.Empty,
                buildNotesBuilder
                    .Replace("\r\n", newLine)
                    .ToString()
                    .Split(newLine)
                    .Select(r => $"<p>{r}</p>"));
            testLink.CreateBuild(testPlanId, buildName, buildNotesHtml);
        }

        var testBuild = testLink
            .GetBuildsForTestPlan(testPlanId)
            .FirstOrDefault(x => x.name == buildName)
            .CheckIsNotNull($"Build '{buildName}' for test plan with ID '{testPlanId}' not found");

        // TODO: Get test suite from TestResult (TestSuiteEntity) and their TestLink IDs using TestLink API?
        var executedTestSuiteIds = Array.Empty<int>();
        foreach (var executedTestSuiteId in executedTestSuiteIds)
        {
            // NOTE: testcase/testsuite ID se získá: Specifikace testů >> pravé tl. myši na test. příp. ve stromu
            var testsuiteTestcases = testLink.GetTestCasesForTestSuite(executedTestSuiteId, true);

            var executedTestCasesIds = Array.Empty<TestCaseEntity>();
            foreach (var executedTestCase in executedTestCasesIds)
            {
                var testcase = testsuiteTestcases
                    .FirstOrDefault(x => x.external_id == executedTestCase.Id)
                    .CheckIsNotNull($"Test case with ID {executedTestCase.Id} not found");
                var testLinkTestCaseId = testcase.id;
                var execitedTestCaseStatus = TestStatus.Passed; // TODO: Get actual status from executedTestCase
                var resultStatus = execitedTestCaseStatus switch
                {
                    TestStatus.Passed => "p",
                    TestStatus.Failed => "f",
                    TestStatus.Skipped => "b",
                    _ => string.Empty,
                };

                var res = testLink.UploadTestCaseExecutionResult(
                    testLinkTestCaseId,
                    testPlanId,
                    resultStatus,
                    platformName: string.Empty,
                    overwrite: false,
                    notes: string.Empty, // TODO: notes: executedTestCase.Notes,
                    buildId: testBuild.id);
            }
        }
    }

    public interface IContext
    {
        bool IsTestLinkReportingEnabled { get; }

        string? IdeVersion { get; }

        string? IdeReleaseDate { get; }

        string? RuntimeVersion { get; }

        string? RuntimeReleaseDate { get; }
    }
}