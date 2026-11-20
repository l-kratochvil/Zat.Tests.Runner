namespace Zat.Tests.Runner.Common.Net.Services;

using System.Globalization;
using System.Xml;

using Microsoft.Extensions.Logging;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Z2xxTests.Common;
using Zat.Z2xxTests.Common.Utils;

/// <summary>
/// Saves the test result of each sub-run as the XML that TestLink imports: one file for the application tests and one
/// for the runtime tests of each tested HW assembly type.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="resultDirectoryPath">The directory to save the files to; <see cref="Paths.Directories.AutomizedTests"/>
/// when omitted.</param>
public class TestLinkImportXmlResultHandler(
    ILogger<LogSources.TestResultHandler> logger,
    string? resultDirectoryPath = null)
    : ITestResultHandler
{
    private readonly string resultDirectoryPath = resultDirectoryPath ?? Paths.Directories.AutomizedTests;

    private (string Date, string Id)? testRun;

    /// <inheritdoc />
    public void HandleSubRunResult(TestResult testResult)
    {
        try
        {
            var (date, testRunId) = this.testRun ??= (
                testResult.StartTime.LocalDateTime.ToString("ddMMyyyy", CultureInfo.InvariantCulture),
                Guid.NewGuid().ToString("N")[..5]);

            var filePath = FindAvailablePath(Path.Combine(
                this.resultDirectoryPath,
                Paths.FileNames.MakeTestLinkImportResultName(
                    isRuntimeTest: testResult.TestType is TestType.Runtime,
                    hwAssemblyTypeName: testResult.TestedHwAssemblyType?.ToString(),
                    date: date,
                    testRunId: testRunId)));

            Directory.CreateDirectory(this.resultDirectoryPath);
            TestLinkUtils
                .ConvertFromXml(MakeNUnitResult(testResult.TestCaseResults))
                .Save(filePath);

            logger.Log(LogLevel.Information, "TestLink import result saved to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            // A failed save must never fail the test run nor keep the other test result handlers from their work.
            logger.Log(LogLevel.Error, ex, "Saving the TestLink import result failed");
        }
    }

    /// <inheritdoc />
    public void HandleTestRunResult(TestRunResult testRunResult)
        => this.testRun = null;

    /// <summary>
    /// Makes the NUnit result XML that NUnit itself would write for <paramref name="testCaseResults"/>, so that
    /// <see cref="TestLinkUtils.ConvertFromXml"/> converts them the same way as the results saved by the test
    /// assembly. It reverses the mapping of the NUnit result states done by the proxy.
    /// </summary>
    private static XmlDocument MakeNUnitResult(IEnumerable<TestCaseResult> testCaseResults)
    {
        var document = new XmlDocument();
        var testRunElement = document.CreateElement("test-run");
        document.AppendChild(testRunElement);

        // A test case NUnit didn't get to has no result in the NUnit result XML either.
        foreach (var testCaseResult in testCaseResults.Where(x => x.Status is not TestStatus.Unknown))
        {
            var (result, label) = testCaseResult.Status switch
            {
                TestStatus.Passed => ("Passed", null),
                TestStatus.Failure => ("Failed", null),
                TestStatus.Error => ("Failed", "Error"),
                TestStatus.Invalid => ("Failed", "Invalid"),
                TestStatus.Skipped => ("Skipped", null),
                TestStatus.Ignored => ("Skipped", "Ignored"),
                TestStatus.Explicit => ("Skipped", "Explicit"),
                TestStatus.Inconclusive => ("Inconclusive", null),
                TestStatus.Warning => ("Warning", null),
                _ => throw new NotSupportedException(testCaseResult.Status.ToString()),
            };

            var testCaseElement = document.CreateElement("test-case");
            testCaseElement.SetAttribute("name", testCaseResult.Id);
            testCaseElement.SetAttribute("fullname", testCaseResult.EntityName);
            testCaseElement.SetAttribute("result", result);

            if (label is not null)
            {
                testCaseElement.SetAttribute("label", label);
            }

            if (result is "Failed")
            {
                var failureElement = AppendElement(testCaseElement, "failure");
                AppendElement(failureElement, "message").InnerText = testCaseResult.Detail?.Message ?? string.Empty;

                if (!string.IsNullOrEmpty(testCaseResult.Detail?.StackTrace))
                {
                    AppendElement(failureElement, "stack-trace").InnerText = testCaseResult.Detail.StackTrace;
                }
            }
            else if (!string.IsNullOrWhiteSpace(testCaseResult.Detail?.Message))
            {
                AppendElement(AppendElement(testCaseElement, "reason"), "message").InnerText =
                    testCaseResult.Detail.Message;
            }

            testRunElement.AppendChild(testCaseElement);
        }

        return document;

        static XmlElement AppendElement(XmlElement parent, string name)
            => (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(name))!;
    }

    private static string FindAvailablePath(string filePath)
    {
        var availablePath = filePath;

        for (var suffixNumber = 1; File.Exists(availablePath); suffixNumber++)
        {
            availablePath = Path.Combine(
                Path.GetDirectoryName(filePath)!,
                $"{Path.GetFileNameWithoutExtension(filePath)}_{suffixNumber}{Path.GetExtension(filePath)}");
        }

        return availablePath;
    }
}