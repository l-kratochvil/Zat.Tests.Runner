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
            .Returns(new Secrets(new SmtpAuth("smtp-user", "smtp-password")));

        this.GivenNotification(enabled: true, "first@zat.cz", "second@zat.cz");

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
        this.unit.HandleTestRunResult(TestRun(isStoppedByUser: false, SubRun(TestStatus.Passed)));

        // Then:
        var (config, message) = this.sentEmails.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config, Is.EqualTo(new SendConfig("zat01ex.zat.lan:25", "smtp-user", "smtp-password")));
            Assert.That(message.From, Is.EqualTo("tester-z2xx@zat.cz"));
            Assert.That(message.To, Is.EqualTo(new[] { "first@zat.cz", "second@zat.cz" }));
        }
    }

    [Test]
    public void HandleTestRunResult__WhenNotificationIsDisabled__ThenShouldSendNothing()
    {
        // Given:
        this.GivenNotification(enabled: false, "first@zat.cz");

        // When:
        this.unit.HandleTestRunResult(TestRun(isStoppedByUser: false, SubRun(TestStatus.Passed)));

        // Then:
        Assert.That(this.sentEmails, Is.Empty);
    }

    [Test]
    public void HandleTestRunResult__WhenNoAddressIsConfigured__ThenShouldSendNothing()
    {
        // Given:
        this.GivenNotification(enabled: true);

        // When:
        this.unit.HandleTestRunResult(TestRun(isStoppedByUser: false, SubRun(TestStatus.Passed)));

        // Then:
        Assert.That(this.sentEmails, Is.Empty);
    }

    [Test]
    public void HandleTestRunResult__WhenSomeAddressesAreInvalidOrForeign__ThenShouldSendOnlyToValidZatAddresses()
    {
        // Given:
        this.GivenNotification(enabled: true, "not-an-email", "first@zat.cz", "someone@example.com");

        // When:
        this.unit.HandleTestRunResult(TestRun(isStoppedByUser: false, SubRun(TestStatus.Passed)));

        // Then:
        Assert.That(this.sentEmails.Single().Message.To, Is.EqualTo(new[] { "first@zat.cz" }));
    }

    [Test]
    public void HandleTestRunResult__WhenNoAddressPassesValidation__ThenShouldSendNothing()
    {
        // Given:
        this.GivenNotification(enabled: true, "not-an-email", "someone@example.com");

        // When:
        this.unit.HandleTestRunResult(TestRun(isStoppedByUser: false, SubRun(TestStatus.Passed)));

        // Then:
        Assert.That(this.sentEmails, Is.Empty);
    }

    [Test]
    public void HandleTestRunResult__WhenSubRunsHaveResults__ThenSubjectShouldShowPassedOfAllTestCases()
    {
        // When:
        this.unit.HandleTestRunResult(TestRun(
            isStoppedByUser: false,
            SubRun(TestStatus.Passed, TestStatus.Failure),
            SubRun(TestStatus.Passed, TestStatus.Passed, TestStatus.Skipped)));

        // Then:
        var subject = this.sentEmails.Single().Message.Subject;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(subject, Does.Contain("3/5"));
            Assert.That(subject, Does.Not.Contain(Resources.EmailResult_StoppedByUser));
        }
    }

    [Test]
    public void HandleTestRunResult__WhenTestRunWasStoppedByUser__ThenSubjectShouldSaySo()
    {
        // When:
        this.unit.HandleTestRunResult(TestRun(isStoppedByUser: true, SubRun(TestStatus.Passed)));

        // Then:
        Assert.That(this.sentEmails.Single().Message.Subject, Does.Contain(Resources.EmailResult_StoppedByUser));
    }

    [Test]
    public void HandleTestRunResult__WhenTestCaseFailed__ThenBodyShouldListItWithItsMessage()
    {
        // When:
        this.unit.HandleTestRunResult(TestRun(
            isStoppedByUser: false,
            SubRun(
                new TestCaseResult("TC-1", "Passing", TestStatus.Passed),
                new TestCaseResult("TC-2", "Failing", TestStatus.Failure, new Detail("Button not found", null)))));

        // Then:
        var body = this.sentEmails.Single().Message.Body;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(body, Does.Contain("TC-2").And.Contain("Button not found"));
            Assert.That(body, Does.Not.Contain("TC-1"));
        }
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
            () => this.unit.HandleTestRunResult(TestRun(isStoppedByUser: false, SubRun(TestStatus.Passed))),
            Throws.Nothing);
    }

    private static TestRunResult TestRun(bool isStoppedByUser, params TestResult[] subRunResults)
        => new(subRunResults, isStoppedByUser);

    private static TestResult SubRun(params TestStatus[] testCaseStatuses)
        => SubRun(testCaseStatuses.Select((status, i) => new TestCaseResult($"TC-{i}", $"Case{i}", status)).ToArray());

    private static TestResult SubRun(params TestCaseResult[] testCaseResults)
        => new(
            new ProxyTestResult(
            [
                new TestSuiteResult(
                    [new TestFixtureResult(testCaseResults, "Fixture", TestStatus.Passed)],
                    "Suite",
                    TestStatus.Passed),
            ]),
            TestType.Application);

    private void GivenNotification(bool enabled, params string[] addresses)
    {
        this.contextMock.SetupGet(x => x.IsEmailNotificationEnabled).Returns(enabled);
        this.contextMock.SetupGet(x => x.EmailAddresses).Returns(addresses);
    }
}