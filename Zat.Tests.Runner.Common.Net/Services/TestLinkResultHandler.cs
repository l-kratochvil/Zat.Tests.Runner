namespace Zat.Tests.Runner.Common.Net.Services;

using System.Diagnostics;

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

        var buildName = $"IDE v{context.IdeVersion}, RT v{context.RuntimeVersion}";

        if (context.BetaVersion is not null)
        {
            buildName += $"(beta{context.BetaVersion})";
        }

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

        if (testLink.GetBuildsForTestPlan(testPlanId).All(x => x.Name != buildName))
        {
            var buildNotes = "# Datum vydání";

            if (!string.IsNullOrEmpty(context.IdeReleaseDate))
            {
                buildNotes += $"<p>- IDE: {context.IdeReleaseDate}</p>";
            }

            if (!string.IsNullOrEmpty(context.RuntimeReleaseDate))
            {
                buildNotes += $"<p>- RT: {context.RuntimeReleaseDate}</p>";
            }

            testLink.CreateBuild(testPlanId, buildName, buildNotes);
        }

        var testBuild = testLink
            .GetBuildsForTestPlan(testPlanId)
            .FirstOrDefault(x => x.Name == buildName)
            .CheckIsNotNull($"Build '{buildName}' for test plan with ID '{testPlanId}' not found");

        foreach (var testCaseResult in testResult.TestCaseResults)
            {
                var testCaseExternalId = $"Z200-{testCaseResult.Id}";
                var testCaseId = testLink.GetTestCaseByExternalId(testCaseExternalId).Id;
                var resultStatus = testCaseResult.Status switch
                {
                    // TODO: Check all TestStatuses are mapped correctly
                    TestStatus.Passed => "p",
                    TestStatus.Failure or
                        TestStatus.Error or
                        TestStatus.Invalid => "f",
                    TestStatus.Skipped or
                        TestStatus.Ignored or
                        TestStatus.Explicit or
                        TestStatus.Other => "b",
                    _ => string.Empty,
                };

                var res = testLink.ReportTestCaseResult(
                    testCaseId,
                    testPlanId,
                    resultStatus,
                    platformName: string.Empty,
                    overwrite: true,
                    notes: testCaseResult.Message, // TODO: Include stacktrace as legacy runner does
                    buildId: testBuild.Id);
            }
    }

    public interface IContext
    {
        bool IsTestLinkReportingEnabled { get; }

        string? IdeVersion { get; }

        string? IdeReleaseDate { get; }

        string? RuntimeVersion { get; }

        string? RuntimeReleaseDate { get; }

        string? BetaVersion { get; }
    }
}