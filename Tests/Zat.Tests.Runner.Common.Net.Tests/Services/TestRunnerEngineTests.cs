namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using System.Reflection;

using DevKit.Core.Extensions.Types;
using DevKit.Core.UtilityObjects;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// The engine runs a real test assembly through a real proxy; only the bridge to the tested runtime is
/// mocked, as it needs the runtime itself.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestRunnerEngineTests
{
    private const string TestAssemblyName = "NUnitTestAssembly.Net481";
    private const string PassingTestCasePath = TestAssemblyName + ".SampleTestSuite.Pass";

    private static readonly string TestAssemblyDllPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetAssemblyDirectoryPath(),
        "NUnitTestAssemblies",
        TestAssemblyName,
        $"{TestAssemblyName}.dll");

    private readonly List<TestRunState> reportedStates = [];

    private NUnitTestRunnerProxyConnector connector = null!;
    private Mock<ITestRunnerBridgeConnector> bridgeConnectorMock = null!;
    private Mock<ITestResultHandler> resultHandlerMock = null!;
    private TestRunnerEngine unit = null!;

    [SetUp]
    public async Task SetUp()
    {
        this.connector = await NUnitTestRunnerProxyConnector.ConnectAsync(launchDebugger: false);

        this.bridgeConnectorMock = new Mock<ITestRunnerBridgeConnector>();
        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Returns(() => new Disposer(() => { }));

        this.resultHandlerMock = new Mock<ITestResultHandler>();

        this.unit = new TestRunnerEngine(
            this.bridgeConnectorMock.Object,
            this.connector.Proxy,
            NullLogger<LogSources.TestRun>.Instance);
        this.unit.StateChanged += this.reportedStates.Add;
    }

    [TearDown]
    public async Task TearDown()
        => await this.connector.DisposeAsync();

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldReportRunningAndThenStopped()
    {
        // Given:
        await this.GivenTestAssemblyLoadedAsync();

        // When:
        await this.unit.RunTestAsync([ApplicationTestCase()], this.Config());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.reportedStates, Is.EqualTo(new[] { TestRunState.Running, TestRunState.Stopped }));
            Assert.That(this.unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldHandItsResult()
    {
        // Given:
        await this.GivenTestAssemblyLoadedAsync();

        // When:
        var results = await this.unit.RunTestAsync([ApplicationTestCase()], this.Config());

        // Then:
        var handledResult = results.Single();
        using (Assert.EnterMultipleScope())
        {
            this.resultHandlerMock.Verify(x => x.Handle(handledResult), Times.Once);
            Assert.That(
                handledResult.TestCaseResults.Select(x => (x.EntityName, x.Status)),
                Is.EqualTo(new[] { (PassingTestCasePath, TestStatus.Passed) }));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenCallerCancelsDuringSubRun__ThenShouldStopWithoutHandlingAnyResult()
    {
        // Given:
        await this.GivenTestAssemblyLoadedAsync();

        using var callerCts = new CancellationTokenSource();
        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(callerCts.Cancel)
            .Returns(() => new Disposer(() => { }));

        // When:
        var results = await this.unit.RunTestAsync([ApplicationTestCase()], this.Config(), callerCts.Token);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Is.Empty);
            this.resultHandlerMock.Verify(x => x.Handle(It.IsAny<TestResult>()), Times.Never);
            Assert.That(
                this.reportedStates,
                Is.EqualTo(new[] { TestRunState.Running, TestRunState.Stopping, TestRunState.Stopped }));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenAnotherTestRunIsInProgress__ThenShouldRefuseToStart()
    {
        // Given:
        await this.GivenTestAssemblyLoadedAsync();

        Task? secondStart = null;
        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(() =>
            {
                if (secondStart is null)
                {
                    // Marked first, so a start that is not refused can't recurse back in here.
                    secondStart = Task.CompletedTask;
                    secondStart = this.unit.RunTestAsync([ApplicationTestCase()], this.Config());
                }
            })
            .Returns(() => new Disposer(() => { }));

        // When:
        await this.unit.RunTestAsync([ApplicationTestCase()], this.Config());

        // Then:
        Assert.ThrowsAsync<InvalidOperationException>(() => secondStart!);
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunFails__ThenShouldBeStoppedAgain()
    {
        // Given:
        // With no test assembly loaded the proxy refuses to run.

        // When:
        var testRun = this.unit.RunTestAsync([ApplicationTestCase()], this.Config());

        // Then:
        Assert.CatchAsync(() => testRun);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.reportedStates, Is.EqualTo(new[] { TestRunState.Running, TestRunState.Stopped }));
            Assert.That(this.unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task StopTestRun__WhenCalledBetweenSubRuns__ThenShouldSkipTheRemainingOnes()
    {
        // Given:
        await this.GivenTestAssemblyLoadedAsync();

        this.resultHandlerMock
            .Setup(x => x.Handle(It.IsAny<TestResult>()))
            .Callback(this.unit.StopTestRun);

        // When:
        var results = await this.unit.RunTestAsync(
            [RuntimeTestCase()],
            this.Config(HwAssemblyType.HW00, HwAssemblyType.HW01));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Select(x => x.TestedHwAssemblyType), Is.EqualTo(new[] { HwAssemblyType.HW00 }));
            this.resultHandlerMock.Verify(x => x.Handle(results[0]), Times.Once);
            this.resultHandlerMock.Verify(x => x.Handle(It.IsAny<TestResult>()), Times.Once);
            Assert.That(
                this.reportedStates,
                Is.EqualTo(new[] { TestRunState.Running, TestRunState.Stopping, TestRunState.Stopped }));
        }
    }

    [Test]
    public async Task StopTestRun__WhenCalledDuringSubRun__ThenShouldAbortItWithoutHandlingItsResult()
    {
        // Given:
        await this.GivenTestAssemblyLoadedAsync();

        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(this.unit.StopTestRun)
            .Returns(() => new Disposer(() => { }));

        // When:
        var results = await this.unit.RunTestAsync([ApplicationTestCase()], this.Config());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            // The proxy was handed the stop, so it did not run the test case at all.
            Assert.That(results, Is.Empty);
            this.resultHandlerMock.Verify(x => x.Handle(It.IsAny<TestResult>()), Times.Never);
            Assert.That(this.unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    private static TestEntity ApplicationTestCase()
        => new(TestType.Application, name: "Pass", executionPath: PassingTestCasePath);

    // The proxy runs by execution path alone, so a test case of the test assembly can stand in for a runtime one.
    private static TestEntity RuntimeTestCase()
        => new(TestType.Runtime, name: "Pass", executionPath: PassingTestCasePath);

    private ITestRunnerEngine.Config Config(params HwAssemblyType[] testedHwAssemblyTypes)
        => new(
            IsDebug: false,
            TestedRuntimeVersion: null,
            TestedHwAssemblyTypes: testedHwAssemblyTypes,
            TestResultHandlers: [this.resultHandlerMock.Object]);

    private Task GivenTestAssemblyLoadedAsync()
        => this.connector.Proxy.LoadTestAssemblyAsync(TestAssemblyDllPath);
}