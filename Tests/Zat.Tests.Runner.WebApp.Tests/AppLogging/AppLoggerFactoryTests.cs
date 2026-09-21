namespace Zat.Tests.Runner.WebApp.Tests.AppLogging;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Features.AppLogging.Models;
using Zat.Tests.Runner.WebApp.Features.AppLogging.Services;
using Zat.Tests.Runner.WebApp.Shared.Logging;

[TestFixture]
public class AppLoggerFactoryTests
{
    private Mock<IAppLoggerHub> loggerHubMock;
    private AppLoggerFactory unit;

    [SetUp]
    public void SetUp()
    {
        this.loggerHubMock = new Mock<IAppLoggerHub>();
        this.unit = new AppLoggerFactory(this.loggerHubMock.Object);
    }

    [TestCase(LogSources.App, ExpectedResult = LogSources.App)]
    [TestCase(LogSources.TestRun, ExpectedResult = LogSources.TestRun)]
    [TestCase(LogSources.TestLink, ExpectedResult = LogSources.TestLink)]
    [TestCase("SomeFutureSource", ExpectedResult = "SomeFutureSource")]
    public string CreateLogger__WhenCalledWithASource__ThenShouldReturnALoggerBoundToThatSource(
        string givenSource)
    {
        // When:
        var result = this.unit.CreateLogger(givenSource);

        // Then:
        return result.Source;
    }

    [Test]
    public void CreateLogger__WhenTheCreatedLoggerLogs__ThenShouldAppendIntoTheSharedHub()
    {
        // Given:
        var givenLogger = this.unit.CreateLogger(LogSources.TestLink);

        // When:
        givenLogger.Error("unreachable");

        // Then:
        this.loggerHubMock.Verify(
            loggerHub => loggerHub.Append(It.Is<LogEntry>(entry =>
                entry.Source == LogSources.TestLink && entry.Severity == LogSeverity.Error)),
            Times.Once);
    }
}