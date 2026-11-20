#pragma warning disable CA2254
namespace Zat.Tests.Runner.Common.Net.Services;

using DevKit.Core.Extensions;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Properties;

public class TestLinkResultHandler(
    ITestLink testLink,
    TestLinkResultHandler.IContext context,
    ILogger<LogSources.TestResultHandler> logger,
    ILogger<LogSources.App> appLogger)
    : ITestResultHandler
{
    private const int ApplicationTestsTestPlanId = 9560;
    private const int RuntimeTestsTestPlanId = 10130;

    private static readonly FileExtensionContentTypeProvider FileExtensionContentTypeProvider = new();

    /// <inheritdoc />
    public void HandleSubRunResult(TestResult testResult)
    {
        try
        {
            if (!context.IsTestLinkReportingEnabled || !VerifyConnection())
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

            var testCaseExternalIdsInPlan = testLink
                .GetTestCasesForTestPlan(testPlanId)
                .Select(x => x.FullExternalId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var testCaseResult in testResult.TestCaseResults)
            {
                try
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
                    if (!testCaseExternalIdsInPlan.Contains(testCaseExternalId))
                    {
                        logger.Log(
                            LogLevel.Warning,
                            "Test case '{TestCaseExternalId}' is not associated with test plan '{TestPlanId}', skipping",
                            testCaseExternalId,
                            testPlanId);
                        continue;
                    }

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
                        notes: MakeTestCaseResultNotes(testCaseResult.Detail),
                        buildId: testBuild.Id);

                    foreach (var testAttachment in testCaseResult.TestAttachments)
                    {
                        UploadAttachment(reportTestCaseResult.Id, testAttachment);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex, "Failed to report test case result to TestLink");
                }
            }

            logger.Log(LogLevel.Information, "Test run result uploaded to TestLink");
        }
        catch (Exception ex)
        {
            // A mail outage must never fail the test run nor keep the other test result handlers from their work.
            logger.Log(LogLevel.Error, ex, "Sending the test run result to testlink failed");
        }

        return;

        static string MakeBuildName(IContext context, TestResult testResult)
        {
            var buildName = $"IDE v{context.IdeVersion}, RT v{context.RuntimeVersion}";

            if (context.BetaVersion is not null)
            {
                buildName += $" (Beta{context.BetaVersion})";
            }

            if (testResult.TestType is TestType.Runtime)
            {
                if (testResult.TestedHwAssemblyType is null)
                {
                    throw new InvalidOperationException(
                        $"Result has no {nameof(testResult.TestedHwAssemblyType)} " +
                        $"but it's required for runtime tests");
                }

                var hwAssemblyType = testResult.TestedHwAssemblyType?
                    .ToString()
                    .Replace("_", " - ");
                buildName += $" : {hwAssemblyType}";
            }

            if (context.IsDebuggingEnabled)
            {
                buildName += " (Debug)";
            }

            return buildName;
        }

        static string MakeTestCaseResultNotes(Detail? resultDetail)
        {
            var notes = string.Empty;

            if (resultDetail is null)
            {
                return notes;
            }

            if (!string.IsNullOrEmpty(resultDetail.Message))
            {
                notes += $"Message: {resultDetail.Message}\n";
            }

            if (!string.IsNullOrEmpty(resultDetail.StackTrace))
            {
                notes += $"StackTrace: {resultDetail.StackTrace}";
            }

            return notes;
        }

        bool VerifyConnection()
        {
            Exception? connectionException = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(testLink.SayHello()))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                connectionException = ex;
            }

            logger.Log(
                LogLevel.Warning,
                connectionException,
                "Unable to verify connection to TestLink, skipping test result upload");
            appLogger.Log(LogLevel.Warning, Resources.TestLinkResult_ConnectionFailed);

            return false;
        }

        void UploadAttachment(int executionId, TestAttachment testAttachment)
        {
            var filePath = testAttachment.FilePath;
            if (!File.Exists(filePath))
            {
                logger.Log(LogLevel.Error, "Test attachment not found at path: {FilePath}", filePath);
                return;
            }

            var fileName = Path.GetFileName(filePath);
            var fileType = FileExtensionContentTypeProvider.TryGetContentType(filePath, out var contentType)
                ? contentType
                : "application/octet-stream";

            testLink.UploadExecutionAttachment(
                executionId: executionId,
                filename: fileName,
                fileType: fileType,
                content: File.ReadAllBytes(filePath),
                title: testAttachment.Description ?? fileName);
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