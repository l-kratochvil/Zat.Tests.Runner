namespace Zat.Tests.Runner.Common.Net.Services;

using System.Text;

using Microsoft.Extensions.Logging;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Validation;

using static Zat.Tests.Runner.Common.Net.Services.IEmailSender;

/// <summary>
/// Emails the summary of the whole test run to the addresses of <see cref="IContext"/>.
/// </summary>
public class EmailResultHandler(
    IEmailSender emailSender,
    ISecretsProvider secretsProvider,
    EmailResultHandler.IContext context,
    ILogger<LogSources.TestResultHandler> logger)
    : ITestResultHandler
{
    private const string SmtpServer = "zat01ex.zat.lan:25";
    private const string SenderAddress = "tester-z2xx@zat.cz";

    /// <summary>
    /// Describes the context for the email result handler, providing email notification settings and recipient addresses.
    /// </summary>
    public interface IContext
    {
        bool IsEmailNotificationEnabled { get; }

        IReadOnlyList<string> EmailAddresses { get; }
    }

    /// <inheritdoc />
    public void HandleTestRunResult(TestRunResult testRunResult)
    {
        if (!context.IsEmailNotificationEnabled || context.EmailAddresses.Count == 0)
        {
            return;
        }

        var recipients = context
            .EmailAddresses
            .Where(this.IsValidRecipient)
            .ToArray();
        if (recipients.Length == 0)
        {
            logger.Log(LogLevel.Warning, "No email address passed validation, the test run result email is not sent");
            return;
        }

        var subject = MakeSubject(testRunResult);
        var message = new EmailMessage(
            SenderAddress, recipients, subject, MakeBody(subject, testRunResult));

        try
        {
            var smtpAuth = secretsProvider.GetSecrets().SmtpAuth;
            emailSender.Send(new SendConfig(SmtpServer, smtpAuth.UserName, smtpAuth.Password), message);
        }
        catch (Exception ex)
        {
            // A mail outage must never fail the test run nor keep the other test result handlers from their work.
            logger.Log(LogLevel.Error, ex, "Sending the test run result email failed");
            return;
        }

        logger.Log(
            LogLevel.Information,
            "Test run result email sent to {EmailAddresses}",
            string.Join(", ", recipients));
    }

    private static string MakeBody(string subject, TestRunResult testRunResult)
    {
        var body = new StringBuilder()
            .AppendLine(subject)
            .AppendLine($"{Resources.EmailResult_Summary_Duration}: {FormatDuration(testRunResult.ElapsedTime)}");

        if (testRunResult.SubRunResults.Count == 0)
        {
            body.AppendLine().AppendLine(Resources.EmailResult_NoSubRunResults);
        }

        foreach (var subRunResult in testRunResult.SubRunResults)
        {
            body.AppendLine().AppendLine($"== {MakeSubRunTitle(subRunResult)} ==");
            AppendSummary(body, subRunResult);
            AppendNotRunSection(body, subRunResult);
        }

        body.AppendLine().AppendLine(Resources.EmailResult_FullResultsInTestLink);

        return body.ToString();
    }

    private static void AppendSummary(StringBuilder body, TestResult result)
    {
        body.AppendLine($"{Resources.EmailResult_Summary_OverallResult}: {MapStatus(result.OverallStatus)}");
        body.AppendLine(
            $"{Resources.EmailResult_Summary_TestCount}: {result.TestCaseResults.Length}" +
            $", {Resources.EmailResult_Summary_Passed}: {result.PassedResults.Length}" +
            $", {Resources.EmailResult_Summary_Failed}: {result.FailedResults.Length}" +
            $", {Resources.EmailResult_Summary_Warnings}: {result.WarningResults.Length}" +
            $", {Resources.EmailResult_Summary_Inconclusive}: {result.InconclusiveResults.Length}" +
            $", {Resources.EmailResult_Summary_Skipped}: {result.NotRunResults.Length}");
        body.AppendLine(
            $"  {Resources.EmailResult_Summary_FailedTests} -" +
            $" {Resources.EmailResult_Summary_Failures}: {result.FailureResults.Length}" +
            $", {Resources.EmailResult_Summary_Errors}: {result.ErrorResults.Length}" +
            $", {Resources.EmailResult_Summary_Invalid}: {result.InvalidResults.Length}");
        body.AppendLine(
            $"  {Resources.EmailResult_Summary_SkippedTests} -" +
            $" {Resources.EmailResult_Summary_Ignored}: {result.IgnoredResults.Length}" +
            $", {Resources.EmailResult_Summary_Explicit}: {result.ExplicitResults.Length}" +
            $", {Resources.EmailResult_Summary_Other}: {result.SkippedResults.Length + result.UnknownResults.Length}");
    }

    private static void AppendNotRunSection(StringBuilder body, TestResult result)
        => AppendEntrySection(
            body,
            Resources.EmailResult_TestsNotRun_SectionHeader,
            GetReportedResults(result),
            [
                (TestStatus.Ignored, Resources.EmailResult_Label_Ignored),
                (TestStatus.Explicit, Resources.EmailResult_Label_Explicit),
                (TestStatus.Skipped, Resources.EmailResult_Label_Skipped),
                (TestStatus.Unknown, Resources.EmailResult_Status_Unknown),
            ]);

    private static void AppendEntrySection(
        StringBuilder body,
        string header,
        TestEntityResult[] reportedResults,
        (TestStatus Status, string Label)[] labelsByStatus)
    {
        var entries = labelsByStatus
            .SelectMany(x => reportedResults.Where(r => r.Status == x.Status).Select(r => (x.Label, Result: r)))
            .ToArray();
        if (entries.Length == 0)
        {
            return;
        }

        body.AppendLine().AppendLine($"{header}:");
        for (var i = 0; i < entries.Length; i++)
        {
            var (label, result) = entries[i];
            body.AppendLine($"{i + 1}) {label} : {MakeEntityTitle(result)}");

            var message = result.Detail?.Message;
            if (!string.IsNullOrEmpty(message))
            {
                body.AppendLine(Indent(message));
            }
        }
    }

    /// <summary>
    /// Walks the result tree of <paramref name="result"/>: a node that did not pass is reported once and its children
    /// are skipped, otherwise the children that did not pass are reported.
    /// </summary>
    private static TestEntityResult[] GetReportedResults(TestResult result)
    {
        var reportedResults = new List<TestEntityResult>();

        foreach (var testSuite in result.TestSuiteResults)
        {
            if (testSuite.Status is not TestStatus.Passed)
            {
                reportedResults.Add(testSuite);
                continue;
            }

            foreach (var testFixture in testSuite.TestFixtureResults)
            {
                if (testFixture.Status is not TestStatus.Passed)
                {
                    reportedResults.Add(testFixture);
                    continue;
                }

                reportedResults.AddRange(testFixture.TestCaseResults.Where(x => x.Status is not TestStatus.Passed));
            }
        }

        return [.. reportedResults];
    }

    private static string MakeEntityTitle(TestEntityResult result)
        => result is TestCaseResult testCase ? testCase.Id : result.EntityName;

    private static string Indent(string text)
        => string.Join(
            Environment.NewLine,
            text.Split(["\r\n", "\n"], StringSplitOptions.None).Select(x => $"   {x}"));

    private static string FormatDuration(TimeSpan duration)
        => $"{(int)duration.TotalHours:00}:{duration:mm\\:ss}";

    private static string MapStatus(TestStatus status)
        => status switch
        {
            TestStatus.Passed => Resources.EmailResult_Status_Passed,
            TestStatus.Failure or
                TestStatus.Error or
                TestStatus.Invalid => Resources.EmailResult_Status_Failed,
            TestStatus.Skipped or
                TestStatus.Ignored or
                TestStatus.Explicit => Resources.EmailResult_Status_Skipped,
            TestStatus.Inconclusive => Resources.EmailResult_Status_Inconclusive,
            TestStatus.Warning => Resources.EmailResult_Status_Warning,
            _ => Resources.EmailResult_Status_Unknown,
        };

    private static string MakeSubRunTitle(TestResult subRunResult)
        => subRunResult.TestType switch
        {
            TestType.Application => Resources.EmailResult_ApplicationTests,
            TestType.Runtime => $"{Resources.RuntimeTests} – {subRunResult.TestedHwAssemblyType}",
            _ => subRunResult.TestType.ToString(),
        };

    private static string MakeSubject(TestRunResult testRunResult)
    {
        var testCaseResults = testRunResult.SubRunResults.SelectMany(x => x.TestCaseResults).ToArray();
        var subject = string.Format(
            Resources.EmailResult_Subject_Format,
            testCaseResults.Count(x => x.Status is TestStatus.Passed),
            testCaseResults.Length);

        return testRunResult.IsStoppedByUser
            ? $"{subject} {Resources.EmailResult_StoppedByUser}"
            : subject;
    }

    private bool IsValidRecipient(string address)
    {
        if (!EmailAddressValidators.IsValidEmail(address))
        {
            logger.Log(LogLevel.Warning, "Email address '{EmailAddress}' is not a valid email address, skipped", address);
            return false;
        }

        // ReSharper disable once InvertIf
        if (!EmailAddressValidators.IsInZatDomain(address))
        {
            logger.Log(LogLevel.Warning, "Email address '{EmailAddress}' is not in the zat.cz domain, skipped", address);
            return false;
        }

        return true;
    }
}