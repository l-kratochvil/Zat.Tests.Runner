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

    public interface IContext
    {
        bool IsEmailNotificationEnabled { get; }

        IReadOnlyList<string> EmailAddresses { get; }
    }

    /// <inheritdoc />
    public void HandleSubRunResult(TestResult testResult)
    {
    }

    /// <inheritdoc />
    public void HandleTestRunResult(TestRunResult testRunResult)
    {
        if (!context.IsEmailNotificationEnabled || context.EmailAddresses.Count == 0)
        {
            return;
        }

        var recipients = context.EmailAddresses.Where(this.IsValidRecipient).ToArray();
        if (recipients.Length == 0)
        {
            logger.Log(LogLevel.Warning, "No email address passed validation, the test run result email is not sent");
            return;
        }

        var subject = MakeSubject(testRunResult);
        var message = new EmailMessage(SenderAddress, recipients, subject, MakeBody(subject, testRunResult));

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
        var body = new StringBuilder().AppendLine(subject);

        if (testRunResult.SubRunResults.Count == 0)
        {
            body.AppendLine().AppendLine(Resources.EmailResult_NoSubRunResults);
        }

        foreach (var subRunResult in testRunResult.SubRunResults)
        {
            body.AppendLine().AppendLine($"== {MakeSubRunTitle(subRunResult)} ==");

            var countsByStatus = subRunResult.TestCaseResults
                .GroupBy(x => x.Status)
                .OrderBy(x => x.Key is not TestStatus.Passed)
                .ThenBy(x => x.Key);
            foreach (var statusCount in countsByStatus)
            {
                body.AppendLine($"{statusCount.Key}: {statusCount.Count()}");
            }

            if (subRunResult.FailedResults.Length == 0)
            {
                continue;
            }

            body.AppendLine().AppendLine(Resources.EmailResult_FailedTestCases);
            foreach (var failedResult in subRunResult.FailedResults)
            {
                body.AppendLine($"- {failedResult.Id} ({failedResult.EntityName}): {failedResult.Detail?.Message}");
            }
        }

        return body.ToString();
    }

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

        if (!EmailAddressValidators.IsInZatDomain(address))
        {
            logger.Log(LogLevel.Warning, "Email address '{EmailAddress}' is not in the zat.cz domain, skipped", address);
            return false;
        }

        return true;
    }
}