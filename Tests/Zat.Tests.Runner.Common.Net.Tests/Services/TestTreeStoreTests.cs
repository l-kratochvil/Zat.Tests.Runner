namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using DevKit.Core.UtilityObjects;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Application.Paths;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Services;

/// <summary>
/// The store reads the test tree through a mocked proxy; the directory watcher and time are faked so a change of
/// the test assembly and the quiet period after it can be played out.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestTreeStoreTests
{
    private const string TestLibsPath = @"C:\TestLibs";
    private const string TestAssemblyDllPath = TestLibsPath + @"\Zat.Z2xxTests.dll";

    private readonly Mock<INUnitTestRunnerProxy> proxyMock = new();
    private readonly FakeDirectoryWatcher directoryWatcher = new();
    private readonly FakeTimeProvider timeProvider = new();
    private readonly List<TestTreeChange> reportedChanges = [];

    private TestTreeStore unit = null!;

    [SetUp]
    public void SetUp()
    {
        var connectionMock = new Mock<INUnitTestRunnerProxyConnection>();
        connectionMock.SetupGet(x => x.Proxy).Returns(this.proxyMock.Object);
        connectionMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var connectorMock = new Mock<INUnitTestRunnerProxyConnector>();
        connectorMock
            .Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionMock.Object);

        var appPathsProviderMock = new Mock<ISharedAppPathsProvider>();
        appPathsProviderMock
            .SetupGet(x => x.Files)
            .Returns(new SharedAppFilePaths(UserSettings: "user-settings.json", MainAssemblyDll: TestAssemblyDllPath));

        this.unit = new TestTreeStore(
            connectorMock.Object,
            this.directoryWatcher,
            this.timeProvider,
            appPathsProviderMock.Object,
            NullLogger<LogSources.TestRun>.Instance);
        this.unit.Changed += this.reportedChanges.Add;
    }

    [TearDown]
    public void TearDown()
        => this.unit.Dispose();

    [Test]
    public async Task DiscoverAsync__WhenTestAssemblyIsRead__ThenShouldHoldItsTestSuites()
    {
        // Given:
        var testSuites = this.GivenTestAssemblyHolds("A");

        // When:
        await this.unit.DiscoverAsync();

        // Then:
        Assert.That(this.unit.TestSuites, Is.EqualTo(testSuites));
    }

    [Test]
    public async Task OnTestAssemblyChanged__WhenQuietPeriodPasses__ThenShouldDiscoverTestTreeAgain()
    {
        // Given:
        this.GivenTestAssemblyHolds("A");
        await this.unit.DiscoverAsync();
        var changedTestSuites = this.GivenTestAssemblyHolds("A", "B");

        // When:
        this.directoryWatcher.Change();
        this.timeProvider.Advance(TestTreeStore.QuietPeriod);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.directoryWatcher.WatchedDirectoryPaths, Is.EqualTo(new[] { TestLibsPath }));
            Assert.That(this.unit.TestSuites, Is.EqualTo(changedTestSuites));
            Assert.That(
                this.reportedChanges.Last(),
                Is.EqualTo(new TestTreeChange(changedTestSuites, ExecutionPathsChanged: true)));
        }
    }

    [Test]
    public async Task OnTestAssemblyChanged__WhenItChangesAgainWithinQuietPeriod__ThenShouldWaitForQuietPeriodToPassAgain()
    {
        // Given:
        this.GivenTestAssemblyHolds("A");
        await this.unit.DiscoverAsync();

        // When:
        this.directoryWatcher.Change();
        this.timeProvider.Advance(TestTreeStore.QuietPeriod - TimeSpan.FromMilliseconds(1));
        this.directoryWatcher.Change();
        this.timeProvider.Advance(TestTreeStore.QuietPeriod - TimeSpan.FromMilliseconds(1));
        var discoveriesWithinQuietPeriod = this.reportedChanges.Count;
        this.timeProvider.Advance(TimeSpan.FromMilliseconds(1));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(discoveriesWithinQuietPeriod, Is.EqualTo(1));
            Assert.That(this.reportedChanges, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public async Task OnTestAssemblyChanged__WhenTestAssemblyHoldsSameTestEntities__ThenShouldSayExecutionPathsDidNotChange()
    {
        // Given:
        this.GivenTestAssemblyHolds("A");
        await this.unit.DiscoverAsync();
        var sameTestSuites = this.GivenTestAssemblyHolds("A");

        // When:
        this.directoryWatcher.Change();
        this.timeProvider.Advance(TestTreeStore.QuietPeriod);

        // Then:
        Assert.That(
            this.reportedChanges.Last(),
            Is.EqualTo(new TestTreeChange(sameTestSuites, ExecutionPathsChanged: false)));
    }

    [Test]
    public async Task OnTestAssemblyChanged__WhenTestAssemblyCannotBeRead__ThenShouldHoldNoTestSuites()
    {
        // Given:
        this.GivenTestAssemblyHolds("A");
        await this.unit.DiscoverAsync();
        this.proxyMock
            .Setup(x => x.LoadTestAssemblyAsync(TestAssemblyDllPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("The file is being written."));

        // When:
        this.directoryWatcher.Change();
        this.timeProvider.Advance(TestTreeStore.QuietPeriod);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.TestSuites, Is.Empty);
            Assert.That(this.reportedChanges.Last().TestSuites, Is.Empty);
            Assert.That(this.reportedChanges.Last().ExecutionPathsChanged, Is.True);
        }
    }

    private static TestSuiteEntity[] TestTree(params string[] testCaseNames)
        =>
        [
            new(
                [
                    new(
                        [.. testCaseNames.Select(name => new TestCaseEntity(
                            TestType.Application,
                            id: name,
                            name: name,
                            executionPath: $"Suite.Fixture.{name}"))],
                        TestType.Application,
                        name: "Fixture",
                        executionPath: "Suite.Fixture"),
                ],
                TestType.Application,
                name: "Suite",
                executionPath: "Suite"),
        ];

    private TestSuiteEntity[] GivenTestAssemblyHolds(params string[] testCaseNames)
    {
        var testSuites = TestTree(testCaseNames);
        this.proxyMock
            .Setup(x => x.LoadTestAssemblyAsync(TestAssemblyDllPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testSuites);

        return testSuites;
    }

    private sealed class FakeDirectoryWatcher : IDirectoryWatcher
    {
        private readonly List<(string DirectoryPath, Action OnChanged)> watches = [];

        public IReadOnlyList<string> WatchedDirectoryPaths
            => [.. this.watches.Select(watch => watch.DirectoryPath)];

        public IDisposable Watch(string directoryPath, Action onChanged)
        {
            var watch = (directoryPath, onChanged);
            this.watches.Add(watch);

            return new Disposer(() => this.watches.Remove(watch));
        }

        public void Change()
        {
            foreach (var watch in this.watches.ToArray())
            {
                watch.OnChanged();
            }
        }
    }
}
