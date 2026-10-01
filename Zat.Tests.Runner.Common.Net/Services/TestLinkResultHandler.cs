namespace Zat.Tests.Runner.Common.Net.Services;

using DevKit.Core.Extensions;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Z2xxTests.Common;

public class TestLinkResultHandler(
    ITestLink testLink,
    TestLinkResultHandler.IContext context,
    ILogger<LogSources.TestLink> logger)
    : ITestResultHandler
{
    private const int RuntimeTestsTestPlanId = 9560;
    private const int ApplicationTestsTestPlanId = 10130;

    private static readonly FileExtensionContentTypeProvider FileExtensionContentTypeProvider = new();

    /// <inheritdoc />
    public void Handle(TestResult testResult)
    {
        if (!context.IsTestLinkReportingEnabled)
        {
            return;
        }

        var testPlanId = testResult.TestType switch
        {
            TestType.Runtime => RuntimeTestsTestPlanId,
            TestType.Application => ApplicationTestsTestPlanId,
            TestType.Unknown => throw new InvalidOperationException(
                $"Unable to determine test plan ID: Test type is '{testResult.TestType}'"),
            _ => throw new NotSupportedException(testResult.TestType.ToString()),
        };

        var buildName = MakeBuildName(context, testResult);

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
            if (testCaseResult.Status is TestStatus.Unknown)
            {
                // TODO: What to do next when we don't know the result?
                logger.Log(
                    LogLevel.Warning,
                    "Test case with ID '{TestCaseResultId}' has unknown status",
                    testCaseResult.Id);
            }

            var testCaseExternalId = testCaseResult.Id;
            var testCaseId = testLink.GetTestCaseByExternalId(testCaseExternalId).Id;
            var reportTestCaseResult = testLink.ReportTestCaseResult(
                testCaseId: testCaseId,
                testPlanId: testPlanId,
                status: testCaseResult.Status switch
                {
                    TestStatus.Passed => "p",
                    TestStatus.Failure or
                        TestStatus.Error or
                        TestStatus.Invalid => "f",
                    TestStatus.Skipped or
                        TestStatus.Ignored or
                        TestStatus.Explicit or
                        TestStatus.Inconclusive or
                        TestStatus.Warning or
                        TestStatus.Unknown => "b",
                    _ => throw new NotSupportedException(testCaseResult.Status.ToString()),
                },
                platformName: string.Empty,
                overwrite: true,
                notes: $"Message: {testCaseResult.Detail?.Message}\nStackTrace: {testCaseResult.Detail?.StackTrace}",
                buildId: testBuild.Id);

            // Attach failure screenshot only for failed test cases
            if (!testCaseResult.Failed)
            {
                continue;
            }

            var failureScreenshotName = Paths.FileNames.MakeFailureScreenshotName(
                testCaseId: testCaseExternalId,
                hwAssemblyType: testResult.TestedHwAssemblyType);
            var failureScreenshotPath = Path.Combine(Paths.Directories.Current, failureScreenshotName);
            if (!File.Exists(failureScreenshotPath))
            {
                logger.Log(
                    LogLevel.Error,
                    "Failure screenshot not found at path: {FailureScreenshotPath}",
                    failureScreenshotPath);
                continue;
            }

            var failureScreenshotFileBytes = File.ReadAllBytes(failureScreenshotPath);
            var fileType = FileExtensionContentTypeProvider.TryGetContentType(
                failureScreenshotPath, out var contentType)
                ? contentType
                : "application/octet-stream";

            testLink.UploadExecutionAttachment(
                executionId: reportTestCaseResult.Id,
                filename: failureScreenshotName,
                fileType: fileType,
                content: failureScreenshotFileBytes,
                title: "Screenshot", // TODO: Better text?
                description: "Attached screenshot for the test case result"); // TODO: Better text?
        }

        static string MakeBuildName(IContext context, TestResult testResult)
        {
            var buildName = $"IDE v{context.IdeVersion}, RT v{context.RuntimeVersion}";

            if (context.BetaVersion is not null)
            {
                buildName += $"(beta{context.BetaVersion})";
            }

            if (testResult.TestType is TestType.Runtime)
            {
                if (testResult.TestedHwAssemblyType is null)
                {
                    throw new InvalidOperationException(
                        $"Result has no {nameof(testResult.TestedHwAssemblyType)} " +
                        $"but it's required for runtime tests");
                }

                buildName += $" : {testResult.TestedHwAssemblyType}";
            }

            if (testResult.TestType is TestType.Runtime)
            {
                if (testResult.TestedHwAssemblyType is null)
                {
                    throw new InvalidOperationException(
                        $"Result has no {nameof(testResult.TestedHwAssemblyType)} " +
                        $"but it's required for runtime tests");
                }

                buildName += $" : {testResult.TestedHwAssemblyType}";
            }

            if (context.IsDebuggingEnabled)
            {
                buildName += " (Debug)";
            }

            return buildName;
        }
    }

    public interface IContext
    {
        bool IsTestLinkReportingEnabled { get; }

        bool IsDebuggingEnabled { get; }

        string? IdeVersion { get; }

        string? IdeReleaseDate { get; }

        string? RuntimeVersion { get; }

        string? RuntimeReleaseDate { get; }

        string? BetaVersion { get; }
    }
}