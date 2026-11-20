namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using System.Net.Mime;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Properties;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestLinkResultHandlerTests
{
    private const int ExecutionId = 42;
    private const string TestCaseExternalId = "TC-1";
    private const string IdeVersion = "1.0";
    private const string RuntimeVersion = "1.0";
    private const string BuildName = $"IDE v{IdeVersion}, RT v{RuntimeVersion}";

    private string tempDirPath = null!;

    [SetUp]
    public void SetUp()
        => this.tempDirPath = Directory.CreateTempSubdirectory().FullName;

    [TearDown]
    public void TearDown()
        => Directory.Delete(this.tempDirPath, recursive: true);

    [Test]
    public void HandleSubRunResult__WhenReportingIsDisabled__ThenShouldNotVerifyConnection()
    {
        // Given:
        var testLinkMock = new Mock<ITestLink>();
        var contextMock = new Mock<TestLinkResultHandler.IContext>();
        contextMock.SetupGet(x => x.IsTestLinkReportingEnabled).Returns(false);
        var logger = new FakeLogger<LogSources.TestResultHandler>();
        var appLogger = new FakeLogger<LogSources.App>();
        var unit = CreateUnit(
            testLink: testLinkMock.Object,
            context: contextMock.Object,
            logger: logger,
            appLogger: appLogger);

        // When:
        unit.HandleSubRunResult(SubRun(TestCaseWith(TestStatus.Passed)));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            testLinkMock.Verify(x => x.SayHello(), Times.Never);
            Assert.That(logger.Collector.GetSnapshot(), Is.Empty);
            Assert.That(appLogger.Collector.GetSnapshot(), Is.Empty);
        }
    }

    [Test]
    public void HandleSubRunResult__WhenSayHelloThrows__ThenShouldLogWarningAndSkipUploadingResult()
    {
        // Given:
        var expectedException = new InvalidOperationException("CONNECTION FAILED");
        var testLinkMock = new Mock<ITestLink>();
        testLinkMock.Setup(x => x.SayHello()).Throws(expectedException);
        var logger = new FakeLogger<LogSources.TestResultHandler>();
        var appLogger = new FakeLogger<LogSources.App>();
        var unit = CreateUnit(
            testLink: testLinkMock.Object,
            context: CreateEnabledContext(),
            logger: logger,
            appLogger: appLogger);

        // When:
        unit.HandleSubRunResult(SubRun(TestCaseWith(TestStatus.Passed)));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            var diagnosticsEntry = logger.Collector.GetSnapshot().Single();
            Assert.That(diagnosticsEntry.Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(diagnosticsEntry.Exception, Is.SameAs(expectedException));

            var appEntry = appLogger.Collector.GetSnapshot().Single();
            Assert.That(appEntry.Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(appEntry.Message, Is.EqualTo(Resources.TestLinkResult_ConnectionFailed));

            testLinkMock.Verify(x => x.GetBuildsForTestPlan(It.IsAny<int>()), Times.Never);
            testLinkMock.Verify(
                x => x.ReportTestCaseResult(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void HandleSubRunResult__WhenSayHelloReturnsBlank__ThenShouldLogWarningAndSkipUploadingResult(
        string? givenResponse)
    {
        // Given:
        var testLinkMock = new Mock<ITestLink>();
        testLinkMock.Setup(x => x.SayHello()).Returns(givenResponse!);
        var logger = new FakeLogger<LogSources.TestResultHandler>();
        var appLogger = new FakeLogger<LogSources.App>();
        var unit = CreateUnit(
            testLink: testLinkMock.Object,
            context: CreateEnabledContext(),
            logger: logger,
            appLogger: appLogger);

        // When:
        unit.HandleSubRunResult(SubRun(TestCaseWith(TestStatus.Passed)));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            var diagnosticsEntry = logger.Collector.GetSnapshot().Single();
            Assert.That(diagnosticsEntry.Level, Is.EqualTo(LogLevel.Warning));

            var appEntry = appLogger.Collector.GetSnapshot().Single();
            Assert.That(appEntry.Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(appEntry.Message, Is.EqualTo(Resources.TestLinkResult_ConnectionFailed));

            testLinkMock.Verify(x => x.GetBuildsForTestPlan(It.IsAny<int>()), Times.Never);
        }
    }

    [TestCase(TestStatus.Passed)]
    [TestCase(TestStatus.Failure)]
    public void HandleSubRunResult__WhenTestCaseHasAttachments__ThenShouldUploadEachToItsExecution(TestStatus status)
    {
        // Given:
        const string ScreenshotFileName = "screen.png";
        const string ScreenshotDescription = "Failure Screenshot";
        const string LogFileName = "run.zatlog";
        const string LogDescription = "Log";

        var uploads = new List<Upload>();
        var testLinkMock = CreateConnectedTestLinkMock(uploads);
        var screenshot = new TestAttachment(
            description: ScreenshotDescription,
            filePath: this.CreateFile(ScreenshotFileName));
        var log = new TestAttachment(
            description: LogDescription,
            filePath: this.CreateFile(LogFileName));
        var unit = CreateUnit(
            testLink: testLinkMock.Object,
            context: CreateEnabledContext());

        // When:
        unit.HandleSubRunResult(SubRun(TestCaseWith(status, screenshot, log)));

        // Then:
        Assert.That(
            uploads,
            Is.EqualTo(
            [
                new Upload(
                    ExecutionId,
                    FileName: ScreenshotFileName,
                    FileType: MediaTypeNames.Image.Png,
                    Title: ScreenshotDescription),
                new Upload(
                    ExecutionId,
                    FileName: LogFileName,
                    FileType: MediaTypeNames.Application.Octet,
                    Title: LogDescription),
            ]));
    }

    [Test]
    public void HandleSubRunResult__WhenAttachmentFileIsMissing__ThenShouldSkipItAndUploadTheRest()
    {
        // Given:
        const string MissingFileName = "missing.png";
        const string MissingDescription = "Missing";
        const string ExistingFileName = "existing.png";
        const string ExistingDescription = "Existing";

        var uploads = new List<Upload>();
        var testLinkMock = CreateConnectedTestLinkMock(uploads);
        var missing = new TestAttachment(
            description: MissingDescription,
            filePath: Path.Combine(this.tempDirPath, MissingFileName));
        var existing = new TestAttachment(
            description: ExistingDescription,
            filePath: this.CreateFile(ExistingFileName));
        var unit = CreateUnit(
            testLink: testLinkMock.Object,
            context: CreateEnabledContext());

        // When:
        unit.HandleSubRunResult(SubRun(TestCaseWith(TestStatus.Failure, missing, existing)));

        // Then:
        Assert.That(uploads.Select(x => x.FileName), Is.EqualTo([ExistingFileName]));
    }

    private static TestLinkResultHandler CreateUnit(
        ITestLink? testLink = null,
        TestLinkResultHandler.IContext? context = null,
        ILogger<LogSources.TestResultHandler>? logger = null,
        ILogger<LogSources.App>? appLogger = null)
    {
        testLink ??= Mock.Of<ITestLink>();
        context ??= Mock.Of<TestLinkResultHandler.IContext>();
        logger ??= NullLogger<LogSources.TestResultHandler>.Instance;
        appLogger ??= NullLogger<LogSources.App>.Instance;

        return new TestLinkResultHandler(testLink, context, logger, appLogger);
    }

    private static TestLinkResultHandler.IContext CreateEnabledContext()
    {
        var contextMock = new Mock<TestLinkResultHandler.IContext>();
        contextMock.SetupGet(x => x.IsTestLinkReportingEnabled).Returns(true);
        contextMock.SetupGet(x => x.IdeVersion).Returns(IdeVersion);
        contextMock.SetupGet(x => x.RuntimeVersion).Returns(RuntimeVersion);
        return contextMock.Object;
    }

    private static Mock<ITestLink> CreateConnectedTestLinkMock(List<Upload> uploads)
    {
        var testLinkMock = new Mock<ITestLink>();
        testLinkMock.Setup(x => x.SayHello()).Returns("Hello!");
        testLinkMock
            .Setup(x => x.GetBuildsForTestPlan(It.IsAny<int>()))
            .Returns(
            [
                new Build(
                    Active: true,
                    Id: 1,
                    IsOpen: true,
                    Name: BuildName,
                    Notes: string.Empty,
                    TestplanId: 0),
            ]);
        testLinkMock
            .Setup(x => x.GetTestCasesForTestPlan(It.IsAny<int>()))
            .Returns([new TestCaseFromTestPlan(TestCaseId: 1, FullExternalId: TestCaseExternalId, Name: "TC")]);
        testLinkMock
            .Setup(x => x.GetTestCaseByExternalId(It.IsAny<string>()))
            .Returns(new TestCase(Id: 1, Name: "TC", ExternalId: 1, TestSuiteId: 1));
        testLinkMock
            .Setup(x => x.ReportTestCaseResult(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .Returns(new GeneralResult(
                AdditionalInfo: null,
                Id: ExecutionId,
                Message: string.Empty,
                Operation: string.Empty,
                Status: true));
        testLinkMock
            .Setup(x => x.UploadExecutionAttachment(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<int, string, string, byte[], string, string>(
                (executionId, fileName, fileType, _, title, _) =>
                    uploads.Add(new Upload(executionId, FileName: fileName, FileType: fileType, Title: title)));

        return testLinkMock;
    }

    private static TestCaseResult TestCaseWith(TestStatus status, params TestAttachment[] attachments)
        => new(
            Id: TestCaseExternalId,
            EntityName: "Suite.Fixture.Case",
            Status: status,
            TestAttachments: attachments);

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
            StartTime: DateTimeOffset.UtcNow,
            EndTime: DateTimeOffset.UtcNow);

    private string CreateFile(string fileName)
    {
        var filePath = Path.Combine(this.tempDirPath, fileName);
        File.WriteAllText(filePath, "CONTENT");
        return filePath;
    }

    private sealed record Upload(
        int ExecutionId,
        string FileName,
        string FileType,
        string Title);
}