namespace Zat.Tests.Runner.WebApp.Tests.NUnitTestRunner;

using Microsoft.Extensions.Logging;
using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Services;
using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Shared.Stores.NUnitTestRunner;

[TestFixture]
public class NUnitTestRunnerStoreTests
{
    private Mock<INUnitTestRunnerProxy> proxyMock;
    private Mock<ILogger<LogSources.TestRun>> loggerMock;
    private NUnitTestRunnerStore unit;

    [SetUp]
    public void SetUp()
    {
        this.proxyMock = new Mock<INUnitTestRunnerProxy>();
        this.loggerMock = new Mock<ILogger<LogSources.TestRun>>();
        var appPathsProviderMock = new Mock<IAppPathsProvider>();

        this.unit = new NUnitTestRunnerStore(
            this.proxyMock.Object,
            this.loggerMock.Object,
            appPathsProviderMock.Object);
    }

    [Test]
    public void LoadedTestSuites__WhenNothingHasStartedYet__ThenShouldBeEmpty()
    {
        // Then:
        Assert.That(this.unit.LoadedTestSuites, Is.Empty);
    }

    [Test]
    public async Task StartAsync__WhenTestAssemblyIsRead__ThenShouldHoldWhatWasDiscovered()
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
    public void StartAsync__WhenTestAssemblyCannotBeRead__ThenShouldNotThrow()
    {
        // Given:
        // Discovery failing must not take the application down with it: the user would lose the
        // log that says why, which is the only thing that makes the failure actionable.
        this.SetUpFailingDiscovery(new InvalidOperationException("no runner"));

        // When, Then:
        Assert.DoesNotThrowAsync(() => this.unit.StartAsync(CancellationToken.None));
    }

    [Test]
    public async Task StartAsync__WhenTestAssemblyCannotBeRead__ThenShouldLeaveNoTestsToChooseFrom()
    {
        // Given:
        this.SetUpFailingDiscovery(new InvalidOperationException("no runner"));

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        Assert.That(this.unit.LoadedTestSuites, Is.Empty);
    }

    [Test]
    public async Task StartAsync__WhenTestAssemblyCannotBeRead__ThenShouldWarnUser()
    {
        // Given:
        var givenException = new InvalidOperationException("no runner");
        this.SetUpFailingDiscovery(givenException);

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        this.loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<string>(),
                It.Is<string>(detail => detail != null && detail.Contains(givenException.Message))),
            Times.Once);
    }

    [Test]
    public async Task StartAsync__WhenTestAssemblyIsRead__ThenShouldReportWhatWasDiscovered()
    {
        // Given:
        this.SetUpDiscovery([CreateTestSuite()]);

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            this.loggerMock.Verify(
                logger => logger.Log(
                LogLevel.Information,
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
            this.loggerMock.Verify(
                logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }
    }

    private static TestSuiteEntity CreateTestSuite()
        => new([], TestType.Application, name: "Suite", executionPath: "Suite");

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