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
    public void HandleTestRunResult__WhenSubRunsHaveResults__ThenSubjectShouldShowPassedOfAllTestCases()
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
        this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], true, TimeSpan.Zero));

        // Then:
        Assert.That(this.sentEmails.Single().Message.Subject, Does.Contain(Resources.EmailResult_StoppedByUser));
    }

    [Test]
    public void HandleTestRunResult__WhenTestCaseFailed__ThenBodyShouldListItByIdWithItsMessage()
    {
        // When:
        const string passingTestCaseId = "TC-1";
        const string failingTestCaseId = "TC-2";
        const string failingEntityName = "Suite.Fixture.Failing";

        this.unit.HandleTestRunResult(new TestRunResult(
            [
                SubRun(
                    new TestCaseResult(
                        passingTestCaseId, "Suite.Fixture.Passing", TestStatus.Passed),
                    new TestCaseResult(
                        failingTestCaseId, failingEntityName, TestStatus.Failure, new Detail("Button not found", "at X"))),
            ],
            false,
            TimeSpan.Zero));

        // Then:
        var body = this.sentEmails.Single().Message.Body;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(body, Does.Contain($"{Resources.EmailResult_Label_Failed} : {failingTestCaseId}"));
            Assert.That(body, Does.Contain("Button not found"));
            Assert.That(body, Does.Not.Contain(failingEntityName).And.Not.Contain("at X"));
            Assert.That(body, Does.Not.Contain(passingTestCaseId));
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
            () => this.unit.HandleTestRunResult(new TestRunResult([SubRun(TestStatus.Passed)], false, TimeSpan.Zero)),
            Throws.Nothing);
    }

    private static TestResult SubRun(params TestStatus[] testCaseStatuses)
        => SubRun([..testCaseStatuses.Select((status, i) => new TestCaseResult($"TC-{i}", $"Case{i}", status))]);

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

    private void SetupHandlerContext(bool enabled, params string[] addresses)
    {
        this.contextMock.SetupGet(x => x.IsEmailNotificationEnabled).Returns(enabled);
        this.contextMock.SetupGet(x => x.EmailAddresses).Returns(addresses);
    }
}