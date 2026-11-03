namespace Zat.Tests.Runner.Common.Net.Tests.Services;

using System.Reflection;

using DevKit.Core.Extensions.Types;
using DevKit.Core.UtilityObjects;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.Common.Services;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// The engine runs a real test assembly through a real proxy. The bridge to the test assembly is mocked
/// except in the test of the bridge itself, as its pipe name is machine-wide. A proxy that fails or refuses
/// to stop can't be had for real, so such a proxy is mocked too.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestRunnerEngineTests
{
    private const string TestAssemblyName = NUnitTestAssembly.Net481.TestAssembly.Name;
    private const string TestCaseFixturePath = TestAssemblyName + "." + nameof(NUnitTestAssembly.Net481.TestCaseFixture);
    private const string PassingTestCasePath =
        TestCaseFixturePath + "." + nameof(NUnitTestAssembly.Net481.TestCaseFixture.PassingTest);

    private const string BridgeTestCasePath =
        TestCaseFixturePath + "." + nameof(NUnitTestAssembly.Net481.TestCaseFixture.UseTestRunnerBridgeTest);

    private static readonly string TestAssemblyDllPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetAssemblyDirectoryPath(),
        "NUnitTestAssemblies",
        TestAssemblyName,
        $"{TestAssemblyName}.dll");

    private readonly List<TestRunState> reportedStates = [];

    private readonly FakeLogger<LogSources.TestRun> testRunLogger = new();
    private readonly FakeLogger<LogSources.App> appLogger = new();

    private Mock<ITestRunnerBridgeConnector> bridgeConnectorMock = null!;
    private Mock<ITestResultHandler> resultHandlerMock = null!;
    private TestRunnerEngine unit = null!;

    [SetUp]
    public void SetUp()
    {
        this.bridgeConnectorMock = new Mock<ITestRunnerBridgeConnector>();
        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        this.resultHandlerMock = new Mock<ITestResultHandler>();

        this.unit = new TestRunnerEngine(
            this.bridgeConnectorMock.Object,
            new NUnitTestRunnerProxyConnector(launchDebugger: false),
            this.testRunLogger,
            this.appLogger);
        this.unit.StateChanged += this.reportedStates.Add;
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldLogEachStateChangeToBothLogAndDiagnostics()
    {
        // When:
        await this.unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                this.appLogger.Collector.GetSnapshot().Select(x => x.Level),
                Is.EqualTo([LogLevel.Information, LogLevel.Information]));
            Assert.That(
                this.testRunLogger.Collector.GetSnapshot().Select(x => x.Level),
                Is.EqualTo([LogLevel.Information, LogLevel.Information]));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldReportRunningAndThenStopped()
    {
        // When:
        await this.unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.reportedStates, Is.EqualTo([TestRunState.Running, TestRunState.Stopped]));
            Assert.That(this.unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldHandItsResult()
    {
        // When:
        var results = await this.unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig());

        // Then:
        var handledResult = results.Value.Single();
        using (Assert.EnterMultipleScope())
        {
            this.resultHandlerMock.Verify(x => x.HandleSubRunResult(handledResult), Times.Once);
            Assert.That(
                handledResult.TestCaseResults.Select(x => (x.EntityName, x.Status)),
                Is.EqualTo([(PassingTestCasePath, TestStatus.Passed)]));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldHandTestRunResultOnceWithAllSubRunResults()
    {
        // When:
        var results = await this.unit.RunTestAsync(
            [CreateRuntimeTestEntity()],
            this.CreateConfig(HwAssemblyType.HW00, HwAssemblyType.HW01));

        // Then:
        Assert.That(results.Value, Has.Length.EqualTo(2));
        this.resultHandlerMock.Verify(
            x => x.HandleTestRunResult(It.Is<TestRunResult>(r =>
                r.SubRunResults.SequenceEqual(results.Value) && !r.IsStoppedByUser)),
            Times.Once);
        this.resultHandlerMock.Verify(x => x.HandleTestRunResult(It.IsAny<TestRunResult>()), Times.Once);
    }

    [Test]
    public async Task StopTestRun__WhenCalledBetweenSubRuns__ThenShouldHandTestRunResultAsStoppedByUser()
    {
        // Given:
        this.resultHandlerMock
            .Setup(x => x.HandleSubRunResult(It.IsAny<TestResult>()))
            .Callback(this.unit.StopTestRun);

        // When:
        var results = await this.unit.RunTestAsync(
            [CreateRuntimeTestEntity()],
            this.CreateConfig(HwAssemblyType.HW00, HwAssemblyType.HW01));

        // Then:
        this.resultHandlerMock.Verify(
            x => x.HandleTestRunResult(It.Is<TestRunResult>(r =>
                r.SubRunResults.SequenceEqual(results.Value) && r.IsStoppedByUser)),
            Times.Once);
        this.resultHandlerMock.Verify(x => x.HandleTestRunResult(It.IsAny<TestRunResult>()), Times.Once);
    }

    [Test]
    public async Task RunTestAsync__WhenCallerCancelsBeforeAnySubRunFinishes__ThenShouldStillHandTestRunResult()
    {
        // Given:
        using var callerCts = new CancellationTokenSource();
        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(callerCts.Cancel)
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        // When:
        await this.unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig(), callerCts.Token);

        // Then:
        this.resultHandlerMock.Verify(
            x => x.HandleTestRunResult(It.Is<TestRunResult>(r => r.SubRunResults.Count == 0 && r.IsStoppedByUser)),
            Times.Once);
    }

    [Test]
    public async Task RunTestAsync__WhenCallerCancelsDuringSubRun__ThenShouldStopWithoutHandlingAnyResult()
    {
        // Given:
        using var callerCts = new CancellationTokenSource();
        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(callerCts.Cancel)
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        // When:
        var results = await this.unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig(), callerCts.Token);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Value, Is.Empty);
            this.resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Never);
            Assert.That(
                this.reportedStates,
                Is.EqualTo([TestRunState.Running, TestRunState.Stopping, TestRunState.Stopped]));
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
                    secondStart = this.unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig());
                }
            })
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        // When:
        await this.unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig());

        // Then:
        Assert.ThrowsAsync<InvalidOperationException>(() => secondStart!);
    }

    [Test]
    public async Task RunTestAsync__WhenTestAssemblyLacksSelectedTestEntity__ThenShouldFailWithoutRunningAnything()
    {
        // Given:
        const string missingTestCasePath = TestCaseFixturePath + ".Missing";
        var missingTestCase = new TestEntity(TestType.Application, name: "Missing", executionPath: missingTestCasePath);

        // When:
        var results = await this.unit.RunTestAsync([CreateApplicationTestEntity(), missingTestCase], this.CreateConfig());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.IsFailure, Is.True);
            Assert.That(
                results.IsFailure ? results.Error : string.Empty,
                Does.Contain(missingTestCasePath).And.Not.Contain(PassingTestCasePath));
            this.bridgeConnectorMock.Verify(x => x.Connect(It.IsAny<TestConfig>()), Times.Never);
            this.resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Never);
            this.resultHandlerMock.Verify(x => x.HandleTestRunResult(It.IsAny<TestRunResult>()), Times.Never);
            Assert.That(this.reportedStates, Is.EqualTo([TestRunState.Running, TestRunState.Stopped]));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestCaseReadsTestConfigOverBridge__ThenShouldServeItFromConfig()
    {
        // Given:
        var unit = new TestRunnerEngine(
            new TestRunnerBridgeConnector(),
            new NUnitTestRunnerProxyConnector(launchDebugger: false),
            NullLogger<LogSources.TestRun>.Instance,
            NullLogger<LogSources.App>.Instance);
        var bridgeTestCase = new TestEntity(
            TestType.Application,
            name: nameof(NUnitTestAssembly.Net481.TestCaseFixture.UseTestRunnerBridgeTest),
            executionPath: BridgeTestCasePath);

        // When:
        var results = await unit.RunTestAsync(
            [bridgeTestCase],
            this.CreateConfig() with
            {
                TestedRuntimeVersion = NUnitTestAssembly.Net481.TestCaseFixture.ExpectedTestedRuntimeVersion,
            });

        // Then:
        var testCaseResult = results.Value.Single().TestCaseResults.Single();
        Assert.That(testCaseResult.Status, Is.EqualTo(TestStatus.Passed), testCaseResult.Detail?.Message);
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
            CreateTestRunnerProxyConnector(new Mock<INUnitTestRunnerProxyConnection>(), proxyMock),
            NullLogger<LogSources.TestRun>.Instance,
            NullLogger<LogSources.App>.Instance);
        unit.StateChanged += this.reportedStates.Add;

        // When:
        var testRun = unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig());

        // Then:
        Assert.CatchAsync(() => testRun);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.reportedStates, Is.EqualTo([TestRunState.Running, TestRunState.Stopped]));
            Assert.That(unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task StopTestRun__WhenCalledBetweenSubRuns__ThenShouldSkipTheRemainingOnes()
    {
        // Given:
        this.resultHandlerMock
            .Setup(x => x.HandleSubRunResult(It.IsAny<TestResult>()))
            .Callback(this.unit.StopTestRun);

        // When:
        var results = await this.unit.RunTestAsync(
            [CreateRuntimeTestEntity()],
            this.CreateConfig(HwAssemblyType.HW00, HwAssemblyType.HW01));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Value.Select(x => x.TestedHwAssemblyType), Is.EqualTo([HwAssemblyType.HW00]));
            this.resultHandlerMock.Verify(x => x.HandleSubRunResult(results.Value[0]), Times.Once);
            this.resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Once);
            Assert.That(
                this.reportedStates,
                Is.EqualTo([TestRunState.Running, TestRunState.Stopping, TestRunState.Stopped]));
        }
    }

    [Test]
    public async Task StopTestRun__WhenCalledDuringSubRun__ThenShouldAbortItWithoutHandlingItsResult()
    {
        // Given:
        this.bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(this.unit.StopTestRun)
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        // When:
        var results = await this.unit.RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            // The proxy was handed the stop, so it did not run the test case at all.
            Assert.That(results.Value, Is.Empty);
            this.resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Never);
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
            CreateTestRunnerProxyConnector(connectionMock, proxyMock),
            NullLogger<LogSources.TestRun>.Instance,
            NullLogger<LogSources.App>.Instance);

        proxyMock
            .Setup(x => x.RunTestAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<CancellationToken>()))
            .Callback(unit.StopTestRun)
            .Returns(new TaskCompletionSource<ProxyTestResult>().Task);

        // When:
        var results = await unit
            .RunTestAsync([CreateApplicationTestEntity()], this.CreateConfig())
            .WaitAsync(TestRunnerEngine.StopTimeout * 3);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Value, Is.Empty);
            this.resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Never);
            Assert.That(unit.State, Is.EqualTo(TestRunState.Stopped));
            connectionMock.Verify(x => x.DisposeAsync(), Times.Once);
        }
    }

    private static INUnitTestRunnerProxyConnector CreateTestRunnerProxyConnector(
        Mock<INUnitTestRunnerProxyConnection> connectionMock,
        Mock<INUnitTestRunnerProxy> proxyMock)
    {
        // The test assembly holds the test case the tests select, so the test run gets to the proxy.
        TestSuiteEntity[] testTree =
        [
            new(
                [
                    new(
                        [CreateApplicationTestCase(id: nameof(NUnitTestAssembly.Net481.TestCaseFixture.PassingTest))],
                        TestType.Application,
                        name: nameof(NUnitTestAssembly.Net481.TestCaseFixture),
                        executionPath: TestCaseFixturePath),
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

    private static TestEntity CreateApplicationTestEntity()
        => new(
            TestType.Application,
            name: nameof(NUnitTestAssembly.Net481.TestCaseFixture.PassingTest),
            executionPath: PassingTestCasePath);

    private static TestEntity CreateRuntimeTestEntity()
        => new(
            TestType.Runtime,
            name: nameof(NUnitTestAssembly.Net481.TestCaseFixture.PassingTest),
            executionPath: PassingTestCasePath);

    private static TestCaseEntity CreateApplicationTestCase(string id)
        => new(
            TestType.Application,
            id: id,
            name: nameof(NUnitTestAssembly.Net481.TestCaseFixture.PassingTest),
            executionPath: PassingTestCasePath);

    private ITestRunnerEngine.Config CreateConfig(params HwAssemblyType[] testedHwAssemblyTypes)
        => new(
            TestAssemblyDllPath: TestAssemblyDllPath,
            IsDebug: false,
            TestedRuntimeVersion: null,
            TestedHwAssemblyTypes: testedHwAssemblyTypes,
            TestResultHandlers: [this.resultHandlerMock.Object]);
}