namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Properties;
using Zat.Tests.Runner.Common.Net.Services;

using static Zat.Tests.Runner.Common.Net.Services.IEmailSender;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class EmailResultHandlerTests
{
    private readonly Mock<IEmailSender> emailSenderMock = new();
    private readonly Mock<EmailResultHandler.IContext> contextMock = new();
    private readonly List<(SendConfig Config, EmailMessage Message)> sentEmails = [];

    private EmailResultHandler unit = null!;

    [SetUp]
    public void SetUp()
    {
        this.emailSenderMock
            .Setup(x => x.Send(It.IsAny<SendConfig>(), It.IsAny<EmailMessage>()))
            .Callback<SendConfig, EmailMessage>((config, message) => this.sentEmails.Add((config, message)));

        var secretsProviderMock = new Mock<ISecretsProvider>();
        secretsProviderMock
            .Setup(x => x.GetSecrets())
            .Returns(new Secrets(
                new SmtpAuth("smtp-user", "smtp-password"),
                new TestLinkApi(string.Empty)));

        this.SetupHandlerContext(enabled: true, "first@zat.cz", "second@zat.cz");

        this.unit = new EmailResultHandler(
            this.emailSenderMock.Object,
            secretsProviderMock.Object,
            this.contextMock.Object,
            NullLogger<LogSources.TestResultHandler>.Instance);
    }

    [Test]
    public void HandleTestRunResult__WhenNotificationIsEnabled__ThenShouldSendOneEmailToConfiguredAddresses()
    {
        // When:
        this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], false, TimeSpan.Zero));

        // Then:
        var (config, message) = this.sentEmails.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config, Is.EqualTo(new SendConfig("zat01ex.zat.lan:25", "smtp-user", "smtp-password")));
            Assert.That(message.From, Is.EqualTo("tester-z2xx@zat.cz"));
            Assert.That(message.To, Is.EqualTo(["first@zat.cz", "second@zat.cz"]));
        }
    }

    [Test]
    public void HandleTestRunResult__WhenNotificationIsDisabled__ThenShouldSendNothing()
    {
        // Given:
        this.SetupHandlerContext(enabled: false, "first@zat.cz");

        // When:
        this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], false, TimeSpan.Zero));

        // Then:
        Assert.That(this.sentEmails, Is.Empty);
    }

    [Test]
    public void HandleTestRunResult__WhenNoAddressIsConfigured__ThenShouldSendNothing()
    {
        // Given:
        this.SetupHandlerContext(enabled: true);

        // When:
        this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], false, TimeSpan.Zero));

        // Then:
        Assert.That(this.sentEmails, Is.Empty);
    }

    [Test]
    public void HandleTestRunResult__WhenSomeAddressesAreInvalidOrForeign__ThenShouldSendOnlyToValidZatAddresses()
    {
        // Given:
        this.SetupHandlerContext(enabled: true, "not-an-email", "first@zat.cz", "someone@example.com");

        // When:
        this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], false, TimeSpan.Zero));

        // Then:
        Assert.That(this.sentEmails.Single().Message.To, Is.EqualTo(["first@zat.cz"]));
    }

    [Test]
    public void HandleTestRunResult__WhenNoAddressPassesValidation__ThenShouldSendNothing()
    {
        // Given:
        this.SetupHandlerContext(enabled: true, "not-an-email", "someone@example.com");

        // When:
        this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], false, TimeSpan.Zero));

        // Then:
        Assert.That(this.sentEmails, Is.Empty);
    }

    [Test]
    public void HandleTestRunResult__WhenSubRunsHaveResults__ThenShouldSumThemUpInSubjectAndBody()
    {
        // When:
        this.unit.HandleTestRunResult(new TestRunResult(
            [
                SubRun(TestStatus.Passed, TestStatus.Failure),
                SubRun(TestStatus.Passed, TestStatus.Passed, TestStatus.Skipped),
            ],
            false,
            TimeSpan.Zero));

        // Then:
        var (_, message) = this.sentEmails.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Subject, Does.Contain("3/5"));
            Assert.That(message.Subject, Does.Not.Contain(Resources.EmailResult_StoppedByUser));
            Assert.That(message.Body, Does.Contain(SummaryCounts(testCount: 2, passed: 1, failed: 1)));
            Assert.That(message.Body, Does.Contain(SummaryCounts(testCount: 3, passed: 2, failed: 0)));
        }
    }

    [Test]
    public void HandleTestRunResult__WhenTestRunWasStoppedByUser__ThenSubjectShouldSaySo()
    {
        // When:
        this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], true, TimeSpan.Zero));

        // Then:
        Assert.That(this.sentEmails.Single().Message.Subject, Does.Contain(Resources.EmailResult_StoppedByUser));
    }

    [Test]
    public void HandleTestRunResult__WhenSenderFails__ThenShouldSwallowTheException()
    {
        // Given:
        this.emailSenderMock
            .Setup(x => x.Send(It.IsAny<SendConfig>(), It.IsAny<EmailMessage>()))
            .Throws(new InvalidOperationException("SMTP DOWN"));

        // Then:
        Assert.That(
            () => this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], false, TimeSpan.Zero)),
            Throws.Nothing);
    }

    private static string SummaryCounts(int testCount, int passed, int failed)
        => $"{Resources.EmailResult_Summary_TestCount}: {testCount}" +
           $", {Resources.EmailResult_Summary_Passed}: {passed}" +
           $", {Resources.EmailResult_Summary_Failed}: {failed}";

    private static TestResult SubRun(params TestStatus[] testCaseStatuses)
        => SubRun([.. testCaseStatuses.Select((status, i) => new TestCaseResult($"TC-{i}", $"Case{i}", status))]);

    private static TestResult SubRun(params TestCaseResult[] testCaseResults)
        => new(
            new ProxyTestResult(
            [
                new TestSuiteResult(
                    [new TestFixtureResult(testCaseResults, "Fixture", TestStatus.Passed)],
                    "Suite",
                    TestStatus.Passed),
            ]),
            TestType.Application,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

    private void SetupHandlerContext(bool enabled, params string[] addresses)
    {
        this.contextMock.SetupGet(x => x.IsEmailNotificationEnabled).Returns(enabled);
        this.contextMock.SetupGet(x => x.EmailAddresses).Returns(addresses);
    }
}