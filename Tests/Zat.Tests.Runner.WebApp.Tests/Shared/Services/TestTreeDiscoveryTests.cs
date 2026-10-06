namespace Zat.Tests.Runner.WebApp.Tests.Shared.Services;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Services;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestTreeDiscoveryTests
{
    private readonly Mock<ITestTreeStore> testTreeStoreMock = new();
    private readonly Mock<ILogger<LogSources.TestRun>> loggerMock = new();
    private readonly TestTreeDiscovery unit;

    public TestTreeDiscoveryTests()
        => this.unit = new TestTreeDiscovery(this.testTreeStoreMock.Object, this.loggerMock.Object);

    [Test]
    public async Task StartAsync__WhenApplicationStarts__ThenShouldDiscoverTestTree()
    {
        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        this.testTreeStoreMock.Verify(store => store.DiscoverAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void StartAsync__WhenTestAssemblyCannotBeRead__ThenShouldNotThrow()
    {
        // Given:
        // Discovery failing must not take the application down with it: the user would lose the
        // log that says why, which is the only thing that makes the failure actionable.
        this.GivenDiscoveryFails(new InvalidOperationException("no runner"));

        // When, Then:
        Assert.DoesNotThrowAsync(() => this.unit.StartAsync(CancellationToken.None));
    }

    [Test]
    public async Task StartAsync__WhenTestAssemblyCannotBeRead__ThenShouldWarnUser()
    {
        // Given:
        var givenException = new InvalidOperationException("no runner");
        this.GivenDiscoveryFails(givenException);

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        this.loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(givenException.Message)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void GivenDiscoveryFails(Exception exception)
        => this.testTreeStoreMock
            .Setup(store => store.DiscoverAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
}