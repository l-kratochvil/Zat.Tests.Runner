namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestLinkResultHandlerTests
{
    private const int ExecutionId = 42;

    private readonly Mock<ITestLink> testLinkMock = new();
    private readonly Mock<TestLinkResultHandler.IContext> contextMock = new();
    private readonly List<Upload> uploads = [];

    private string tempDirPath = null!;
    private TestLinkResultHandler unit = null!;

    [SetUp]
    public void SetUp()
    {
        this.tempDirPath = Directory.CreateTempSubdirectory().FullName;

        this.contextMock.SetupGet(x => x.IsTestLinkReportingEnabled).Returns(true);
        this.contextMock.SetupGet(x => x.IdeVersion).Returns("1.0");
        this.contextMock.SetupGet(x => x.RuntimeVersion).Returns("1.0");

        this.testLinkMock
            .Setup(x => x.GetBuildsForTestPlan(It.IsAny<int>()))
            .Returns([new Build(true, 1, true, "IDE v1.0, RT v1.0", string.Empty, 0)]);
        this.testLinkMock
            .Setup(x => x.GetTestCaseByExternalId(It.IsAny<string>()))
            .Returns(new TestCase(1, "TC", 1, 1));
        this.testLinkMock
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
            .Returns(new GeneralResult(null, ExecutionId, string.Empty, string.Empty, true));
        this.testLinkMock
            .Setup(x => x.UploadExecutionAttachment(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<int, string, string, byte[], string, string>(
                (executionId, fileName, fileType, _, title, _) =>
                    this.uploads.Add(new Upload(executionId, fileName, fileType, title)));

        this.unit = new TestLinkResultHandler(
            this.testLinkMock.Object,
            this.contextMock.Object,
            NullLogger<LogSources.TestResultHandler>.Instance);
    }

    [TearDown]
    public void TearDown()
        => Directory.Delete(this.tempDirPath, recursive: true);

    [TestCase(TestStatus.Passed)]
    [TestCase(TestStatus.Failure)]
    public void HandleSubRunResult__WhenTestCaseHasAttachments__ThenShouldUploadEachToItsExecution(TestStatus status)
    {
        // Given:
        var screenshot = new TestAttachment("Failure Screenshot", this.CreateFile("screen.png"));
        var log = new TestAttachment("Log", this.CreateFile("run.zatlog"));

        // When:
        this.unit.HandleSubRunResult(SubRun(TestCaseWith(status, screenshot, log)));

        // Then:
        Assert.That(
            this.uploads,
            Is.EqualTo(
            [
                new Upload(ExecutionId, "screen.png", "image/png", "Failure Screenshot"),
                new Upload(ExecutionId, "run.zatlog", "application/octet-stream", "Log"),
            ]));
    }

    [Test]
    public void HandleSubRunResult__WhenAttachmentFileIsMissing__ThenShouldSkipItAndUploadTheRest()
    {
        // Given:
        var missing = new TestAttachment("Missing", Path.Combine(this.tempDirPath, "missing.png"));
        var existing = new TestAttachment("Existing", this.CreateFile("existing.png"));

        // When:
        this.unit.HandleSubRunResult(SubRun(TestCaseWith(TestStatus.Failure, missing, existing)));

        // Then:
        Assert.That(this.uploads.Select(x => x.FileName), Is.EqualTo(["existing.png"]));
    }

    private static TestCaseResult TestCaseWith(TestStatus status, params TestAttachment[] attachments)
        => new("TC-1", "Suite.Fixture.Case", status, TestAttachments: attachments);

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

    private string CreateFile(string fileName)
    {
        var filePath = Path.Combine(this.tempDirPath, fileName);
        File.WriteAllText(filePath, "CONTENT");
        return filePath;
    }

    private sealed record Upload(int ExecutionId, string FileName, string FileType, string Title);
}