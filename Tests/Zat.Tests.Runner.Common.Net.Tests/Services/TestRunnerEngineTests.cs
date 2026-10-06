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
using Zat.Tests.Runner.Common.Services;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// The engine runs a real test assembly through a real proxy; only the bridge to the tested runtime is
/// mocked, as it needs the runtime itself. A proxy that fails or refuses to stop can't be had for real, so
/// such a proxy is mocked too.
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

    private Mock<ITestRunnerBridgeConnector> bridgeConnectorMock = null!;
    private Mock<ITestResultHandler> resultHandlerMock = null!;
    private TestRunnerEngine unit = null!;

    [SetUp]
    public void SetUp()
    {
        this.bridgeConnectorMock = new Mock<ITestRunnerBridgeConnector>();
        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Returns(() => new Disposer(() => { }));

        this.resultHandlerMock = new Mock<ITestResultHandler>();

        this.unit = new TestRunnerEngine(
            this.bridgeConnectorMock.Object,
            new NUnitTestRunnerProxyConnector(launchDebugger: false),
            NullLogger<LogSources.TestRun>.Instance);
        this.unit.StateChanged += this.reportedStates.Add;
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldReportRunningAndThenStopped()
    {
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
        // When:
        var results = await this.unit.RunTestAsync([ApplicationTestCase()], this.Config());

        // Then:
        var handledResult = results.Value.Single();
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
            Assert.That(results.Value, Is.Empty);
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
    public async Task RunTestAsync__WhenTestAssemblyLacksSelectedTestEntity__ThenShouldFailWithoutRunningAnything()
    {
        // Given:
        const string missingTestCasePath = TestAssemblyName + ".SampleTestSuite.Missing";
        var missingTestCase = new TestEntity(TestType.Application, name: "Missing", executionPath: missingTestCasePath);

        // When:
        var results = await this.unit.RunTestAsync([ApplicationTestCase(), missingTestCase], this.Config());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.IsFailure, Is.True);
            Assert.That(
                results.IsFailure ? results.Error : string.Empty,
                Does.Contain(missingTestCasePath).And.Not.Contain(PassingTestCasePath));
            this.bridgeConnectorMock.Verify(x => x.Connect(It.IsAny<TestConfig>()), Times.Never);
            this.resultHandlerMock.Verify(x => x.Handle(It.IsAny<TestResult>()), Times.Never);
            Assert.That(this.reportedStates, Is.EqualTo(new[] { TestRunState.Running, TestRunState.Stopped }));
        }
    }

    [Test]
    public void RunTestAsync__WhenTestRunFails__ThenShouldBeStoppedAgain()
    {
        // Given:
        var proxyMock = new Mock<INUnitTestRunnerProxy>();
        proxyMock
            .Setup(x => x.RunTestAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("RUN FAILURE"));

        var unit = new TestRunnerEngine(
            this.bridgeConnectorMock.Object,
            GivenConnector(new Mock<INUnitTestRunnerProxyConnection>(), proxyMock),
            NullLogger<LogSources.TestRun>.Instance);
        unit.StateChanged += this.reportedStates.Add;

        // When:
        var testRun = unit.RunTestAsync([ApplicationTestCase()], this.Config());

        // Then:
        Assert.CatchAsync(() => testRun);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.reportedStates, Is.EqualTo(new[] { TestRunState.Running, TestRunState.Stopped }));
            Assert.That(unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task StopTestRun__WhenCalledBetweenSubRuns__ThenShouldSkipTheRemainingOnes()
    {
        // Given:
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
            Assert.That(results.Value.Select(x => x.TestedHwAssemblyType), Is.EqualTo(new[] { HwAssemblyType.HW00 }));
            this.resultHandlerMock.Verify(x => x.Handle(results.Value[0]), Times.Once);
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
            Assert.That(results.Value, Is.Empty);
            this.resultHandlerMock.Verify(x => x.Handle(It.IsAny<TestResult>()), Times.Never);
            Assert.That(this.unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task StopTestRun__WhenProxyDoesNotGiveInDuringSubRun__ThenShouldEndTestRunAndItsConnection()
    {
        // Given:
        var connectionMock = new Mock<INUnitTestRunnerProxyConnection>();
        var proxyMock = new Mock<INUnitTestRunnerProxy>();
        var unit = new TestRunnerEngine(
            this.bridgeConnectorMock.Object,
            GivenConnector(connectionMock, proxyMock),
            NullLogger<LogSources.TestRun>.Instance);

        proxyMock
            .Setup(x => x.RunTestAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<CancellationToken>()))
            .Callback(unit.StopTestRun)
            .Returns(new TaskCompletionSource<ProxyTestResult>().Task);

        // When:
        var results = await unit
            .RunTestAsync([ApplicationTestCase()], this.Config())
            .WaitAsync(TestRunnerEngine.StopTimeout * 3);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Value, Is.Empty);
            this.resultHandlerMock.Verify(x => x.Handle(It.IsAny<TestResult>()), Times.Never);
            Assert.That(unit.State, Is.EqualTo(TestRunState.Stopped));
            connectionMock.Verify(x => x.DisposeAsync(), Times.Once);
        }
    }

    private static INUnitTestRunnerProxyConnector GivenConnector(
        Mock<INUnitTestRunnerProxyConnection> connectionMock,
        Mock<INUnitTestRunnerProxy> proxyMock)
    {
        // The test assembly holds the test case the tests select, so the test run gets to the proxy.
        TestSuiteEntity[] testTree =
        [
            new(
                [
                    new(
                        [ApplicationTestCase(id: "Pass")],
                        TestType.Application,
                        name: "SampleTestSuite",
                        executionPath: TestAssemblyName + ".SampleTestSuite"),
                ],
                TestType.Application,
                name: TestAssemblyName,
                executionPath: TestAssemblyName),
        ];

        proxyMock
            .Setup(x => x.LoadTestAssemblyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testTree);

        connectionMock.SetupGet(x => x.Proxy).Returns(proxyMock.Object);
        connectionMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var connectorMock = new Mock<INUnitTestRunnerProxyConnector>();
        connectorMock
            .Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionMock.Object);

        return connectorMock.Object;
    }

    private static TestEntity ApplicationTestCase()
        => new(TestType.Application, name: "Pass", executionPath: PassingTestCasePath);

    private static TestCaseEntity ApplicationTestCase(string id)
        => new(TestType.Application, id: id, name: "Pass", executionPath: PassingTestCasePath);

    // The proxy runs by execution path alone, so a test case of the test assembly can stand in for a runtime one.
    private static TestEntity RuntimeTestCase()
        => new(TestType.Runtime, name: "Pass", executionPath: PassingTestCasePath);

    private ITestRunnerEngine.Config Config(params HwAssemblyType[] testedHwAssemblyTypes)
        => new(
            TestAssemblyDllPath: TestAssemblyDllPath,
            IsDebug: false,
            TestedRuntimeVersion: null,
            TestedHwAssemblyTypes: testedHwAssemblyTypes,
            TestResultHandlers: [this.resultHandlerMock.Object]);
}