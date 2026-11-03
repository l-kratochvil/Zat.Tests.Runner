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

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldLogEachStateChangeToBothLogAndDiagnostics()
    {
        // Given:
        var testRunLogger = new FakeLogger<LogSources.TestRun>();
        var appLogger = new FakeLogger<LogSources.App>();
        var unit = CreateUnit(
            logger: testRunLogger,
            appLogger: appLogger);

        // When:
        await unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                appLogger.Collector.GetSnapshot().Select(x => x.Level),
                Is.EqualTo([LogLevel.Information, LogLevel.Information]));
            Assert.That(
                testRunLogger.Collector.GetSnapshot().Select(x => x.Level),
                Is.EqualTo([LogLevel.Information, LogLevel.Information]));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldReportRunningAndThenStopped()
    {
        // Given:
        var reportedStates = new List<TestRunState>();
        var unit = CreateUnit();
        unit.StateChanged += reportedStates.Add;

        // When:
        await unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(reportedStates, Is.EqualTo([TestRunState.Running, TestRunState.Stopped]));
            Assert.That(unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldHandItsResult()
    {
        // Given:
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var unit = CreateUnit();

        // When:
        var results = await unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig(resultHandlerMock.Object));

        // Then:
        var handledResult = results.Value.Single();
        using (Assert.EnterMultipleScope())
        {
            resultHandlerMock.Verify(x => x.HandleSubRunResult(handledResult), Times.Once);
            Assert.That(
                handledResult.TestCaseResults.Select(x => (x.EntityName, x.Status)),
                Is.EqualTo([(PassingTestCasePath, TestStatus.Passed)]));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestRunCompletes__ThenShouldHandTestRunResultOnceWithAllSubRunResults()
    {
        // Given:
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var unit = CreateUnit();

        // When:
        var results = await unit.RunTestAsync(
            [CreateRuntimeTestEntity()],
            CreateConfig(resultHandlerMock.Object, HwAssemblyType.HW00, HwAssemblyType.HW01));

        // Then:
        Assert.That(results.Value, Has.Length.EqualTo(2));
        resultHandlerMock.Verify(
            x => x.HandleTestRunResult(It.Is<TestRunResult>(r =>
                r.SubRunResults.SequenceEqual(results.Value) && !r.IsStoppedByUser)),
            Times.Once);
        resultHandlerMock.Verify(x => x.HandleTestRunResult(It.IsAny<TestRunResult>()), Times.Once);
    }

    [Test]
    public async Task StopTestRun__WhenCalledBetweenSubRuns__ThenShouldHandTestRunResultAsStoppedByUser()
    {
        // Given:
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var unit = CreateUnit();
        resultHandlerMock
            .Setup(x => x.HandleSubRunResult(It.IsAny<TestResult>()))
            .Callback(unit.StopTestRun);

        // When:
        var results = await unit.RunTestAsync(
            [CreateRuntimeTestEntity()],
            CreateConfig(resultHandlerMock.Object, HwAssemblyType.HW00, HwAssemblyType.HW01));

        // Then:
        resultHandlerMock.Verify(
            x => x.HandleTestRunResult(It.Is<TestRunResult>(r =>
                r.SubRunResults.SequenceEqual(results.Value) && r.IsStoppedByUser)),
            Times.Once);
        resultHandlerMock.Verify(x => x.HandleTestRunResult(It.IsAny<TestRunResult>()), Times.Once);
    }

    [Test]
    public async Task RunTestAsync__WhenCallerCancelsBeforeAnySubRunFinishes__ThenShouldStillHandTestRunResult()
    {
        // Given:
        using var callerCts = new CancellationTokenSource();
        var bridgeConnectorMock = CreateBridgeConnectorMock();
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var unit = CreateUnit(bridgeConnectorMock.Object);
        bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(callerCts.Cancel)
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        // When:
        await unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig(resultHandlerMock.Object), callerCts.Token);

        // Then:
        resultHandlerMock.Verify(
            x => x.HandleTestRunResult(It.Is<TestRunResult>(r => r.SubRunResults.Count == 0 && r.IsStoppedByUser)),
            Times.Once);
    }

    [Test]
    public async Task RunTestAsync__WhenCallerCancelsDuringSubRun__ThenShouldStopWithoutHandlingAnyResult()
    {
        // Given:
        using var callerCts = new CancellationTokenSource();
        var bridgeConnectorMock = CreateBridgeConnectorMock();
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var reportedStates = new List<TestRunState>();
        var unit = CreateUnit(bridgeConnectorMock.Object);
        unit.StateChanged += reportedStates.Add;
        bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(callerCts.Cancel)
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        // When:
        var results = await unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig(resultHandlerMock.Object), callerCts.Token);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Value, Is.Empty);
            resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Never);
            Assert.That(
                reportedStates,
                Is.EqualTo([TestRunState.Running, TestRunState.Stopping, TestRunState.Stopped]));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenAnotherTestRunIsInProgress__ThenShouldRefuseToStart()
    {
        // Given:
        Task? secondStart = null;
        var bridgeConnectorMock = CreateBridgeConnectorMock();
        var unit = CreateUnit(bridgeConnectorMock.Object);
        bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(() =>
            {
                if (secondStart is null)
                {
                    // Marked first, so a start that is not refused can't recurse back in here.
                    secondStart = Task.CompletedTask;
                    secondStart = unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig());
                }
            })
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        // When:
        await unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig());

        // Then:
        Assert.ThrowsAsync<InvalidOperationException>(() => secondStart!);
    }

    [Test]
    public async Task RunTestAsync__WhenTestAssemblyLacksSelectedTestEntity__ThenShouldFailWithoutRunningAnything()
    {
        // Given:
        const string missingTestCasePath = TestCaseFixturePath + ".Missing";
        var missingTestCase = new TestEntity(TestType.Application, name: "Missing", executionPath: missingTestCasePath);
        var bridgeConnectorMock = CreateBridgeConnectorMock();
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var reportedStates = new List<TestRunState>();
        var unit = CreateUnit(bridgeConnectorMock.Object);
        unit.StateChanged += reportedStates.Add;

        // When:
        var results = await unit.RunTestAsync(
            [CreateApplicationTestEntity(), missingTestCase],
            CreateConfig(resultHandlerMock.Object));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.IsFailure, Is.True);
            Assert.That(
                results.IsFailure ? results.Error : string.Empty,
                Does.Contain(missingTestCasePath).And.Not.Contain(PassingTestCasePath));
            bridgeConnectorMock.Verify(x => x.Connect(It.IsAny<TestConfig>()), Times.Never);
            resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Never);
            resultHandlerMock.Verify(x => x.HandleTestRunResult(It.IsAny<TestRunResult>()), Times.Never);
            Assert.That(reportedStates, Is.EqualTo([TestRunState.Running, TestRunState.Stopped]));
        }
    }

    [Test]
    public async Task RunTestAsync__WhenTestCaseReadsTestConfigOverBridge__ThenShouldServeItFromConfig()
    {
        // Given:
        var unit = CreateUnit(testRunnerBridgeConnector: new TestRunnerBridgeConnector());
        var bridgeTestCase = new TestEntity(
            TestType.Application,
            name: nameof(NUnitTestAssembly.Net481.TestCaseFixture.UseTestRunnerBridgeTest),
            executionPath: BridgeTestCasePath);

        // When:
        var results = await unit.RunTestAsync(
            [bridgeTestCase],
            CreateConfig() with
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

        var reportedStates = new List<TestRunState>();
        var unit = CreateUnit(
            nunitTestRunnerProxyConnector: CreateTestRunnerProxyConnector(
                new Mock<INUnitTestRunnerProxyConnection>(),
                proxyMock));
        unit.StateChanged += reportedStates.Add;

        // When:
        var testRun = unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig());

        // Then:
        Assert.CatchAsync(() => testRun);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(reportedStates, Is.EqualTo([TestRunState.Running, TestRunState.Stopped]));
            Assert.That(unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task StopTestRun__WhenCalledBetweenSubRuns__ThenShouldSkipTheRemainingOnes()
    {
        // Given:
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var reportedStates = new List<TestRunState>();
        var unit = CreateUnit();
        unit.StateChanged += reportedStates.Add;
        resultHandlerMock
            .Setup(x => x.HandleSubRunResult(It.IsAny<TestResult>()))
            .Callback(unit.StopTestRun);

        // When:
        var results = await unit.RunTestAsync(
            [CreateRuntimeTestEntity()],
            CreateConfig(resultHandlerMock.Object, HwAssemblyType.HW00, HwAssemblyType.HW01));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Value.Select(x => x.TestedHwAssemblyType), Is.EqualTo([HwAssemblyType.HW00]));
            resultHandlerMock.Verify(x => x.HandleSubRunResult(results.Value[0]), Times.Once);
            resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Once);
            Assert.That(
                reportedStates,
                Is.EqualTo([TestRunState.Running, TestRunState.Stopping, TestRunState.Stopped]));
        }
    }

    [Test]
    public async Task StopTestRun__WhenCalledDuringSubRun__ThenShouldAbortItWithoutHandlingItsResult()
    {
        // Given:
        var bridgeConnectorMock = CreateBridgeConnectorMock();
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var unit = CreateUnit(bridgeConnectorMock.Object);
        bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Callback(unit.StopTestRun)
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        // When:
        var results = await unit.RunTestAsync([CreateApplicationTestEntity()], CreateConfig(resultHandlerMock.Object));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            // The proxy was handed the stop, so it did not run the test case at all.
            Assert.That(results.Value, Is.Empty);
            resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Never);
            Assert.That(unit.State, Is.EqualTo(TestRunState.Stopped));
        }
    }

    [Test]
    public async Task StopTestRun__WhenProxyDoesNotGiveInDuringSubRun__ThenShouldEndTestRunAndItsConnection()
    {
        // Given:
        var proxyConnectionMock = new Mock<INUnitTestRunnerProxyConnection>();
        var proxyMock = new Mock<INUnitTestRunnerProxy>();
        var resultHandlerMock = new Mock<ITestResultHandler>();
        var unit = CreateUnit(
            nunitTestRunnerProxyConnector: CreateTestRunnerProxyConnector(proxyConnectionMock, proxyMock));

        proxyMock
            .Setup(x => x.RunTestAsync(It.IsAny<IEnumerable<TestEntity>>(), It.IsAny<CancellationToken>()))
            .Callback(unit.StopTestRun)
            .Returns(new TaskCompletionSource<ProxyTestResult>().Task);

        // When:
        var results = await unit
            .RunTestAsync([CreateApplicationTestEntity()], CreateConfig(resultHandlerMock.Object))
            .WaitAsync(TestRunnerEngine.StopTimeout * 3);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Value, Is.Empty);
            resultHandlerMock.Verify(x => x.HandleSubRunResult(It.IsAny<TestResult>()), Times.Never);
            Assert.That(unit.State, Is.EqualTo(TestRunState.Stopped));
            proxyConnectionMock.Verify(x => x.DisposeAsync(), Times.Once);
        }
    }

    private static Mock<ITestRunnerBridgeConnector> CreateBridgeConnectorMock()
    {
        var bridgeConnectorMock = new Mock<ITestRunnerBridgeConnector>();
        bridgeConnectorMock
            .Setup(x => x.Connect(It.IsAny<TestConfig>()))
            .Returns(() => new AsyncDisposer(() => Task.CompletedTask));

        return bridgeConnectorMock;
    }

    private static TestRunnerEngine CreateUnit(
        ITestRunnerBridgeConnector? testRunnerBridgeConnector = null,
        INUnitTestRunnerProxyConnector? nunitTestRunnerProxyConnector = null,
        ILogger<LogSources.TestRun>? logger = null,
        ILogger<LogSources.App>? appLogger = null,
        TimeProvider? timeProvider = null)
        => new(
            testRunnerBridgeConnector ?? CreateBridgeConnectorMock().Object,
            nunitTestRunnerProxyConnector ?? new NUnitTestRunnerProxyConnector(launchDebugger: false),
            logger ?? NullLogger<LogSources.TestRun>.Instance,
            appLogger ?? NullLogger<LogSources.App>.Instance,
            timeProvider ?? TimeProvider.System);

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

    private static ITestRunnerEngine.Config CreateConfig(
        ITestResultHandler? resultHandler = null,
        params HwAssemblyType[] testedHwAssemblyTypes)
        => new(
            TestAssemblyDllPath: TestAssemblyDllPath,
            IsDebug: false,
            TestedRuntimeVersion: null,
            TestedHwAssemblyTypes: testedHwAssemblyTypes,
            TestResultHandlers: [resultHandler ?? Mock.Of<ITestResultHandler>()]);
}