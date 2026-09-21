namespace Zat.Tests.Runner.WebApp.Tests.NUnitTestRunner;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.WebApp.Shared.Logging;
using Zat.Tests.Runner.WebApp.Shared.Stores.NUnitTestRunner;

[TestFixture]
public class NUnitTestRunnerStoreTests
{
    private Mock<INUnitTestRunnerProxy> proxyMock;
    private Mock<IAppLogger> loggerMock;
    private NUnitTestRunnerStore unit;

    [SetUp]
    public void SetUp()
    {
        this.proxyMock = new Mock<INUnitTestRunnerProxy>();
        this.loggerMock = new Mock<IAppLogger>();

        var loggerFactoryMock = new Mock<IAppLoggerFactory>();
        loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(this.loggerMock.Object);

        this.unit = new NUnitTestRunnerStore(this.proxyMock.Object, loggerFactoryMock.Object);
    }

    [Test]
    public void Logger__WhenTheStoreIsBuilt__ThenShouldWriteUnderTheTestRunSource()
    {
        // Given:
        // Discovery is the test runner talking, so it belongs to the channel the tester watches for
        // the runner rather than to the one about the application itself.
        var givenLoggerFactoryMock = new Mock<IAppLoggerFactory>();
        givenLoggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(this.loggerMock.Object);

        // When:
        _ = new NUnitTestRunnerStore(this.proxyMock.Object, givenLoggerFactoryMock.Object);

        // Then:
        givenLoggerFactoryMock.Verify(
            factory => factory.CreateLogger(LogSources.TestRun), Times.Once);
    }

    [Test]
    public void LoadedTestSuites__WhenNothingHasStartedYet__ThenShouldBeEmpty()
    {
        // Then:
        Assert.That(this.unit.LoadedTestSuites, Is.Empty);
    }

    [Test]
    public async Task StartAsync__WhenTheTestAssemblyIsRead__ThenShouldHoldWhatWasDiscovered()
    {
        // Given:
        TestSuiteEntity[] givenTestSuites = [CreateTestSuite()];
        this.SetUpDiscovery(givenTestSuites);

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        Assert.That(this.unit.LoadedTestSuites, Is.EqualTo(givenTestSuites));
    }

    [Test]
    public void StartAsync__WhenTheTestAssemblyCannotBeRead__ThenShouldNotThrow()
    {
        // Given:
        // Discovery failing must not take the application down with it: the user would lose the
        // log that says why, which is the only thing that makes the failure actionable.
        this.SetUpFailingDiscovery(new InvalidOperationException("no runner"));

        // When, Then:
        Assert.DoesNotThrowAsync(() => this.unit.StartAsync(CancellationToken.None));
    }

    [Test]
    public async Task StartAsync__WhenTheTestAssemblyCannotBeRead__ThenShouldLeaveNoTestsToChooseFrom()
    {
        // Given:
        this.SetUpFailingDiscovery(new InvalidOperationException("no runner"));

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        Assert.That(this.unit.LoadedTestSuites, Is.Empty);
    }

    [Test]
    public async Task StartAsync__WhenTheTestAssemblyCannotBeRead__ThenShouldWarnTheUser()
    {
        // Given:
        var givenException = new InvalidOperationException("no runner");
        this.SetUpFailingDiscovery(givenException);

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        this.loggerMock.Verify(
            logger => logger.Warning(
                It.IsAny<string>(),
                It.Is<string>(detail => detail != null && detail.Contains(givenException.Message))),
            Times.Once);
    }

    [Test]
    public async Task StartAsync__WhenTheTestAssemblyIsRead__ThenShouldReportWhatWasDiscovered()
    {
        // Given:
        this.SetUpDiscovery([CreateTestSuite()]);

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            this.loggerMock.Verify(
                logger => logger.Info(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            this.loggerMock.Verify(
                logger => logger.Warning(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }

    private static TestSuiteEntity CreateTestSuite()
        => new([], TestType.ApplicationTest, name: "Suite", executionPath: "Suite");

    private void SetUpDiscovery(TestSuiteEntity[] testSuites)
        => this.proxyMock
            .Setup(proxy => proxy.LoadTestAssemblyAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testSuites);

    private void SetUpFailingDiscovery(Exception exception)
        => this.proxyMock
            .Setup(proxy => proxy.LoadTestAssemblyAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
}